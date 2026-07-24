using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Core.Projects;
using Cursus.Core.Workflows;

namespace Cursus.App.ViewModels;

/// <summary>L'onglet ouvert d'une page de workflow.</summary>
public enum WorkflowPageTab
{
    History,
    Steps,
}

/// <summary>
/// La page d'un workflow : un <em>lieu</em> plutôt qu'une ligne. Elle ne possède
/// rien, elle <b>compose</b> sous une barre d'onglets des modules qui s'ignorent
/// (honneur concret de <c>D-016</c>) — l'<b>historique</b> de ses runs et son
/// <b>éditeur</b> d'étapes (inchangé, simplement hébergé ici). Le graphe (statique)
/// et les déclencheurs viendront comme onglets de plus, sans toucher les autres.
///
/// <para>
/// Adaptateur mince, non testé (§7.12). Lancer un run et rouvrir un passé passent
/// par des fabriques reçues (la coquille les câble sur le host + le magasin
/// d'artefacts du projet) ; la page ne fait que <b>remonter</b> le
/// <see cref="RunViewModel"/> à la surface — c'est elle, propriétaire de l'espace,
/// qui décide de l'afficher. La page ne connaît donc ni host ni SQLite.
/// </para>
/// </summary>
public partial class WorkflowPageViewModel : ObservableObject
{
    private readonly Func<string, IReadOnlyList<RunSummary>> _runsOf;
    private readonly Func<string, RunViewModel> _startLive;
    private readonly Func<RunSummary, RunViewModel> _replay;
    private readonly Action<RunViewModel> _showRun;
    private readonly Action _close;

    public WorkflowPageViewModel(
        string workflowId,
        WorkflowCatalog catalog,
        Func<string, IReadOnlyList<RunSummary>> runsOf,
        Func<string, RunViewModel> startLive,
        Func<RunSummary, RunViewModel> replay,
        Action<RunViewModel> showRun,
        Action onSaved,
        Action close)
    {
        WorkflowId = workflowId;
        _runsOf = runsOf;
        _startLive = startLive;
        _replay = replay;
        _showRun = showRun;
        _close = close;

        // L'éditeur existant, tel quel : la page l'héberge sous l'onglet « Étapes »
        // au lieu que la surface le porte à plat. Sa sauvegarde rafraîchit la liste.
        Editor = WorkflowEditorViewModel.Open(workflowId, catalog, onSaved);
        History = new ObservableCollection<RunRowViewModel>();
        RefreshHistory();
    }

    /// <summary>L'identifiant du workflow, en tête de la page.</summary>
    public string WorkflowId { get; }

    /// <summary>L'éditeur d'étapes du workflow, hébergé sous l'onglet « Étapes ».</summary>
    public WorkflowEditorViewModel Editor { get; }

    /// <summary>Les passages du workflow, du plus récent au plus ancien ; chacun rouvrable en relecture.</summary>
    public ObservableCollection<RunRowViewModel> History { get; }

    /// <summary>Vrai quand le workflow n'a jamais tourné — pilote le repère « aucun passage ».</summary>
    public bool IsHistoryEmpty => History.Count == 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHistory))]
    [NotifyPropertyChangedFor(nameof(IsSteps))]
    private WorkflowPageTab _selectedTab = WorkflowPageTab.History;

    /// <summary>Vrai quand l'onglet historique occupe la page.</summary>
    public bool IsHistory => SelectedTab == WorkflowPageTab.History;

    /// <summary>Vrai quand l'onglet étapes (l'éditeur) occupe la page.</summary>
    public bool IsSteps => SelectedTab == WorkflowPageTab.Steps;

    [RelayCommand]
    private void SelectHistory() => SelectedTab = WorkflowPageTab.History;

    [RelayCommand]
    private void SelectSteps() => SelectedTab = WorkflowPageTab.Steps;

    /// <summary>Lance le workflow et confie son run vif à la surface pour affichage.</summary>
    [RelayCommand]
    private void Launch() => _showRun(_startLive(WorkflowId));

    /// <summary>Referme la page et revient à la liste.</summary>
    [RelayCommand]
    private void Close() => _close();

    /// <summary>
    /// Recharge l'historique depuis le host — à l'ouverture, et au retour d'un run
    /// pour que le passage qui vient de finir y figure. Reconstruit depuis la source
    /// de vérité (le journal, via <c>RunsOf</c>), jamais deviné.
    /// </summary>
    public void RefreshHistory()
    {
        History.Clear();
        foreach (var run in _runsOf(WorkflowId))
            History.Add(new RunRowViewModel(run, () => _showRun(_replay(run))));
        OnPropertyChanged(nameof(IsHistoryEmpty));
    }
}
