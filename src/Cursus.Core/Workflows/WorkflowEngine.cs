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

    public WorkflowEngine(IProcessRunner runner) => _runner = runner;

    public async Task<WorkflowRun> ExecuteAsync(
        WorkflowDefinition definition,
        RunContext context,
        CancellationToken cancellationToken = default)
    {
        var history = new List<StepRun>();
        var visits = new Dictionary<string, int>();
        var cursor = definition.EntryStep;

        while (true)
        {
            var step = definition.GetStep(cursor);

            var iteration = visits[cursor] = visits.GetValueOrDefault(cursor) + 1;
            if (iteration > step.MaxVisits)
                return new WorkflowRun(RunState.Aborted, history, AbortReason.LoopNotConverging);

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
                return new WorkflowRun(RunState.Aborted, history, AbortReason.Canceled);
            }

            history.Add(new StepRun(cursor, iteration, result));

            var edge = step.OutEdges.FirstOrDefault(e => e.Guard.Matches(result));
            if (edge is null)
                return new WorkflowRun(result.IsSuccess ? RunState.Completed : RunState.Failed, history);

            cursor = edge.Target;
        }
    }
}
