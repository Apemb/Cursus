namespace Cursus.Core.Workflows;

/// <summary>
/// Parcourt un graphe de <see cref="StepDefinition"/> depuis son point d'entrée,
/// exécute chaque visite via un <see cref="IProcessRunner"/>, route sur le code
/// de sortie via les arêtes gardées, borne les boucles, et synthétise un
/// <see cref="WorkflowRun"/>. Ne connaît rien d'un agent.
/// </summary>
public sealed class WorkflowEngine
{
    private readonly IProcessRunner _runner;
    private readonly IRunJournal _journal;

    public WorkflowEngine(IProcessRunner runner, IRunJournal journal)
    {
        _runner = runner;
        _journal = journal;
    }

    public async Task<WorkflowRun> ExecuteAsync(
        WorkflowDefinition definition,
        RunContext context,
        RunTrigger? trigger = null,
        CancellationToken cancellationToken = default)
    {
        var runId = Guid.NewGuid().ToString();
        _journal.Append(runId, new WorkflowEvent.RunStarted(
            definition, context.WorkspaceRoot, trigger ?? RunTrigger.Manual));

        try
        {
            return await TraverseAsync(definition, context, runId, cancellationToken);
        }
        catch
        {
            // Les invariants (étape inconnue, évasion de chemin) remontent tels
            // quels — mais le run doit être clos avant, sinon il resterait « en
            // cours » à jamais dans le journal, indiscernable d'un crash machine.
            _journal.Append(runId, new WorkflowEvent.RunFinished(RunState.Aborted, AbortReason.Faulted));
            throw;
        }
    }

    private async Task<WorkflowRun> TraverseAsync(
        WorkflowDefinition definition,
        RunContext context,
        string runId,
        CancellationToken cancellationToken)
    {
        var history = new List<StepRun>();
        var visits = new Dictionary<string, int>();
        var cursor = definition.EntryStep;

        while (true)
        {
            var step = definition.GetStep(cursor);

            var iteration = visits[cursor] = visits.GetValueOrDefault(cursor) + 1;
            if (iteration > step.MaxVisits)
            {
                _journal.Append(runId, new WorkflowEvent.RunFinished(
                    RunState.Aborted, AbortReason.LoopNotConverging));
                return new WorkflowRun(runId, RunState.Aborted, history, AbortReason.LoopNotConverging);
            }

            _journal.Append(runId, new WorkflowEvent.StepStarted(cursor, iteration));

            ScriptResult result;
            try
            {
                // La définition déclare un sous-chemin relatif, le runner attend
                // un répertoire absolu : le moteur est le seul à connaître le
                // contexte, donc le seul à pouvoir traduire.
                var script = step.Script with { WorkingDirectory = context.Resolve(step.WorkingSubdirectory) };
                result = await _runner.RunAsync(script, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // L'annulation interrompt le run mais ne l'efface pas : la
                // trajectoire déjà parcourue reste observable.
                _journal.Append(runId, new WorkflowEvent.RunFinished(
                    RunState.Aborted, AbortReason.Canceled));
                return new WorkflowRun(runId, RunState.Aborted, history, AbortReason.Canceled);
            }

            history.Add(new StepRun(cursor, iteration, result));
            _journal.Append(runId, new WorkflowEvent.StepFinished(cursor, iteration, result));

            var edge = step.OutEdges.FirstOrDefault(e => e.Guard.Matches(result));
            if (edge is null)
            {
                var state = result.IsSuccess ? RunState.Completed : RunState.Failed;
                _journal.Append(runId, new WorkflowEvent.RunFinished(state));
                return new WorkflowRun(runId, state, history);
            }

            _journal.Append(runId, new WorkflowEvent.EdgeChosen(cursor, edge.Target));
            cursor = edge.Target;
        }
    }
}
