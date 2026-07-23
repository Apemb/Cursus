using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Core.Projects;
using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Projection;
using Cursus.Persistence;

namespace Cursus.App.ViewModels;

/// <summary>
/// L'écran d'un run : adaptateur mince sur <see cref="RunProjection"/> (le cœur
/// testable). Il n'ajoute que ce qui relève de la vue et se refuse au test (§7.12) :
/// refléter la trajectoire en lignes bindables, suivre le log de la visite
/// sélectionnée, et commander l'arrêt. La <b>même</b> classe sert un run vif et un
/// run passé — seule l'alimentation change (« un écran, deux sources », parcours §1.4) :
/// le flux live de <see cref="ProjectHost.LaunchAsync"/> pour l'un, la relecture
/// de <see cref="ProjectHost.ReadEvents"/> pour l'autre.
/// </summary>
public partial class RunViewModel : ObservableObject, IDisposable
{
    private readonly RunProjection _projection = new();
    private readonly RunArtifactStore _artifacts;
    private readonly Dictionary<(string StepId, int Iteration), RunVisitRow> _rows = new();
    private readonly CancellationTokenSource? _cts;
    private readonly DispatcherTimer? _logTimer;

    private ArtifactTail? _stdoutTail;
    private ArtifactTail? _stderrTail;

    private RunViewModel(string workflowName, RunArtifactStore artifacts, bool live)
    {
        WorkflowName = workflowName;
        _artifacts = artifacts;

        if (live)
        {
            _cts = new CancellationTokenSource();

            // Le log grossit *entre* les événements ; il faut donc le tirer au fil
            // du temps, là où la trajectoire, elle, se rafraîchit sur événement.
            _logTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            _logTimer.Tick += (_, _) => PullLog();
            _logTimer.Start();
        }
    }

    /// <summary>Le nom du workflow, en tête de l'écran.</summary>
    public string WorkflowName { get; }

    /// <summary>La trajectoire déroulée : une ligne par visite, dans l'ordre de démarrage.</summary>
    public ObservableCollection<RunVisitRow> Trajectory { get; } = new();

    /// <summary>
    /// La visite dont le détail (log) est montré. Suit ce qui tourne par défaut —
    /// chaque nouvelle visite prend le focus — jusqu'à ce qu'on en fige une à la main.
    /// </summary>
    [ObservableProperty]
    private RunVisitRow? _selectedVisit;

    /// <summary>Le log de la visite sélectionnée — vif s'il tourne, figé s'il est passé.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOutput))]
    private string _log = "";

    /// <summary>Vrai dès que la visite sélectionnée a émis quelque chose — masque le repère « en attente ».</summary>
    public bool HasOutput => Log.Length > 0;

    /// <summary>Le statut du run en toutes lettres — le verdict qu'arbitre la présentation (parcours §4).</summary>
    [ObservableProperty]
    private string _status = "En cours";

    /// <summary>Vrai tant que le contrôle d'arrêt a lieu d'être (run en cours ou tout juste arrêté).</summary>
    [ObservableProperty]
    private bool _isControlVisible = true;

    /// <summary>Le libellé du contrôle à trois positions : « Arrêter » / « Arrêt en cours… » / « Arrêté ».</summary>
    [ObservableProperty]
    private string _stopLabel = "Arrêter";

    /// <summary>Vrai seulement en position « en cours » : c'est le seul moment où arrêter a un sens.</summary>
    [ObservableProperty]
    private bool _canStop = true;

    // --- alimentations ---

    /// <summary>Démarre un run vif et rend son écran : le flux live plie la projection au fil de l'eau.</summary>
    public static RunViewModel StartLive(string workflowId, ProjectHost host, RunArtifactStore artifacts)
    {
        var view = new RunViewModel(workflowId, artifacts, live: true);

        // Progress capture le contexte de synchronisation du thread UI (on est ici
        // sur ce thread) : chaque Report replie l'événement sur le thread UI, où
        // toucher les collections observables est sûr.
        var observer = new Progress<WorkflowEvent>(view.Ingest);
        _ = view.RunAsync(host, workflowId, observer);
        return view;
    }

    /// <summary>Rouvre un run passé et rend son écran : la relecture plie la même projection, figée.</summary>
    public static RunViewModel Replay(RunSummary summary, ProjectHost host, RunArtifactStore artifacts)
    {
        var view = new RunViewModel(summary.WorkflowId ?? summary.RunId, artifacts, live: false);
        foreach (var entry in host.ReadEvents(summary.RunId))
            view.Ingest(entry.Event);
        return view;
    }

    private async Task RunAsync(ProjectHost host, string workflowId, IProgress<WorkflowEvent> observer)
    {
        try
        {
            await host.LaunchAsync(workflowId, observer: observer, cancellationToken: _cts!.Token);
        }
        catch (OperationCanceledException)
        {
            // L'annulation est déjà racontée par un RunFinished(Aborted, Canceled)
            // émis avant de rendre la main ; rien à faire de plus ici.
        }
        finally
        {
            _logTimer?.Stop();
            PullLog(); // dernier tirage : chasser la fin du log sur l'écran
        }
    }

    /// <summary>Absorbe un événement : plie la projection, puis reflète trajectoire, statut et contrôle.</summary>
    private void Ingest(WorkflowEvent @event)
    {
        _projection.Apply(@event);

        if (@event is WorkflowEvent.StepStarted started)
        {
            var row = new RunVisitRow(started.StepId, started.Iteration);
            _rows[(started.StepId, started.Iteration)] = row;
            Trajectory.Add(row);
            SelectedVisit = row; // suivre la visite qui vient de démarrer
        }
        else if (@event is WorkflowEvent.StepFinished finished
                 && _rows.TryGetValue((finished.StepId, finished.Iteration), out var closed))
        {
            closed.Status = finished.Result.IsSuccess ? RunVisitStatus.Succeeded : RunVisitStatus.Failed;
        }

        RefreshStatus();
        RefreshControl();
    }

    // --- log de la visite sélectionnée ---

    partial void OnSelectedVisitChanged(RunVisitRow? value)
    {
        // Le log ne dépend que de la visite sélectionnée : on repart de zéro sur
        // ses propres tubes. Un run passé est figé (une lecture suffit) ; un run
        // vif continuera d'être tiré par le minuteur.
        _stdoutTail = null;
        _stderrTail = null;
        Log = "";

        if (value is null || _projection.RunId is not { } runId)
            return;

        _stdoutTail = _artifacts.Follow(runId, value.StepId, value.Iteration, ArtifactStream.StandardOutput);
        _stderrTail = _artifacts.Follow(runId, value.StepId, value.Iteration, ArtifactStream.StandardError);
        PullLog();
    }

    private void PullLog()
    {
        // Deux tubes fusionnés au grain du tirage : la sortie standard puis l'erreur.
        // L'entrelacement fin des deux n'est pas reconstituable (pas d'horodatage à
        // l'octet) ; à l'échelle d'un tirage, c'est un log lisible qui montre aussi
        // les erreurs — le raffinement est renvoyé à plus tard.
        var fresh = (_stdoutTail?.ReadMore() ?? "") + (_stderrTail?.ReadMore() ?? "");
        if (fresh.Length > 0)
            Log += fresh;
    }

    // --- contrôle à trois positions ---

    /// <summary>Demande l'arrêt : l'étape en cours finira, aucune autre ne démarrera. Le contrôle passe « Arrêt en cours… ».</summary>
    [RelayCommand]
    private void Stop()
    {
        _projection.RequestStop(); // la position bascule tout de suite, avant même la clôture
        _cts?.Cancel();
        RefreshControl();
    }

    private void RefreshStatus() => Status = _projection.State switch
    {
        RunState.Completed => "Réussi",
        RunState.Failed => "Échoué",
        RunState.Aborted when _projection.AbortReason == AbortReason.Canceled => "Arrêté",
        RunState.Aborted when _projection.AbortReason == AbortReason.Faulted => "Planté",
        RunState.Aborted => "Échoué", // boucle non convergente : un échec, pas un arrêt voulu
        _ => "En cours",
    };

    private void RefreshControl()
    {
        IsControlVisible = _projection.Control is not null;
        CanStop = _projection.Control == RunControl.Running;
        StopLabel = _projection.Control switch
        {
            RunControl.Stopping => "Arrêt en cours…",
            RunControl.Stopped => "Arrêté",
            _ => "Arrêter",
        };
    }

    public void Dispose() => _logTimer?.Stop();
}
