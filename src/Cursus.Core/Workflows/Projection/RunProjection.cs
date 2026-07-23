namespace Cursus.Core.Workflows.Projection;

/// <summary>
/// La projection d'un run : elle plie une séquence de <see cref="WorkflowEvent"/>
/// en <b>trajectoire de visites + statut</b>, sans savoir d'où vient la séquence.
/// Le flux live d'un run en cours (l'<see cref="IProgress{T}"/> du lanceur) et la
/// relecture d'un run passé (<c>ReadEvents</c>) l'alimentent à l'identique — « un
/// seul objet, deux alimentations » (parcours §1.4). C'est le cœur testable de
/// l'écran de run, sans une ligne d'Avalonia.
/// </summary>
public sealed class RunProjection
{
    /// <summary>Vrai tant que le run n'a pas été clos par un <c>RunFinished</c>.</summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// L'état terminal du run — absent tant qu'il tourne, posé au <c>RunFinished</c>.
    /// C'est l'état <b>brut</b> ; le traduire en « Réussi »/« Échoué » appartient à
    /// la présentation, qui arbitre le verdict (parcours §4).
    /// </summary>
    public RunState? State { get; private set; }

    /// <summary>La raison d'un arrêt, quand le run s'est clos sur <c>Aborted</c> — sépare « Arrêté » de « Planté ».</summary>
    public AbortReason? AbortReason { get; private set; }

    private readonly List<RunVisit> _trajectory = [];
    private (string StepId, int Iteration)? _explicitSelection;
    private bool _stopRequested;

    /// <summary>Les visites, dans l'ordre où elles ont commencé.</summary>
    public IReadOnlyList<RunVisit> Trajectory => _trajectory;

    /// <summary>
    /// La visite dont le détail (log) est montré. À défaut de choix explicite,
    /// c'est la visite <b>en cours</b> — le détail suit ce qui tourne — ou, run
    /// terminé, la dernière visite. Un choix explicite fige sur un passé.
    /// </summary>
    public RunVisit? Selected =>
        _explicitSelection is { } key
            ? _trajectory.Find(visit => visit.StepId == key.StepId && visit.Iteration == key.Iteration)
            : _trajectory.LastOrDefault(visit => visit.IsRunning) ?? _trajectory.LastOrDefault();

    /// <summary>
    /// Fige la sélection sur une visite précise — par sa clé, pour qu'elle survive
    /// à la clôture d'une visite qui tournait quand on l'a choisie.
    /// </summary>
    public void Select(RunVisit visit) => _explicitSelection = (visit.StepId, visit.Iteration);

    /// <summary>
    /// La position du contrôle d'arrêt — <c>null</c> quand elle ne s'applique pas
    /// (run abouti normalement : l'écran montre alors le verdict, pas le contrôle).
    /// </summary>
    public RunControl? Control => IsRunning
        ? _stopRequested ? RunControl.Stopping : RunControl.Running
        : State == RunState.Aborted && AbortReason == Workflows.AbortReason.Canceled
            ? RunControl.Stopped
            : null;

    /// <summary>Demande l'arrêt : l'étape courante finira, aucune autre ne démarrera.</summary>
    public void RequestStop() => _stopRequested = true;

    /// <summary>Annule la demande d'arrêt — on repasse par le milieu, on n'y reste pas.</summary>
    public void RevokeStop() => _stopRequested = false;

    /// <summary>Absorbe un événement et met à jour l'état projeté.</summary>
    public void Apply(WorkflowEvent @event)
    {
        switch (@event)
        {
            case WorkflowEvent.RunStarted:
                IsRunning = true;
                break;

            case WorkflowEvent.StepStarted started:
                _trajectory.Add(new RunVisit(started.StepId, started.Iteration));
                break;

            case WorkflowEvent.StepFinished finished:
                Close(finished);
                break;

            case WorkflowEvent.RunFinished finished:
                IsRunning = false;
                State = finished.State;
                AbortReason = finished.AbortReason;
                break;
        }
    }

    /// <summary>
    /// Scelle la visite en cours que la fin d'étape désigne — par son étape et
    /// son itération, seule paire qui distingue les tours d'une boucle.
    /// </summary>
    private void Close(WorkflowEvent.StepFinished finished)
    {
        var index = _trajectory.FindLastIndex(
            visit => visit.StepId == finished.StepId && visit.Iteration == finished.Iteration);

        if (index >= 0)
            _trajectory[index] = _trajectory[index] with { Result = finished.Result };
    }
}
