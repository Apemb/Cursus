using Cursus.Core.Workflows.Journaling;
using Cursus.Core.Workflows.Output;

namespace Cursus.Core.Workflows.Execution;

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
    private readonly IRunOutputStore _output;

    public WorkflowEngine(IProcessRunner runner, IRunJournal journal, IRunOutputStore output)
    {
        _runner = runner;
        _journal = journal;
        _output = output;
    }

    public async Task<WorkflowRun> ExecuteAsync(
        WorkflowDefinition definition,
        RunContext context,
        string runId,
        RunTrigger? trigger = null,
        CancellationToken cancellationToken = default)
    {
        // L'identité n'est plus forgée ici : l'appelant la fournit, parce que
        // c'est lui — le fabricant du RunContext — qui provisionne le workspace
        // isolé à ce nom, avant même que le run démarre.
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

            // Le puits s'ouvre avant l'étape : c'est ce qui permet à la sortie de
            // ruisseler pendant le run plutôt que d'être écrite à la fin.
            ScriptResult result;
            StepOutput output;
            using (var sink = _output.Open(runId, cursor, iteration))
            {
                try
                {
                    // La définition déclare un sous-chemin relatif, le runner attend
                    // un répertoire absolu : le moteur est le seul à connaître le
                    // contexte, donc le seul à pouvoir traduire.
                    var script = step.Script with { WorkingDirectory = context.Resolve(step.WorkingSubdirectory) };
                    result = await _runner.RunAsync(script, sink.Stdout, sink.Stderr, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // L'annulation interrompt le run mais ne l'efface pas : la
                    // trajectoire déjà parcourue reste observable, et la sortie
                    // partielle est chassée sur disque en refermant le puits.
                    _journal.Append(runId, new WorkflowEvent.RunFinished(
                        RunState.Aborted, AbortReason.Canceled));
                    return new WorkflowRun(runId, RunState.Aborted, history, AbortReason.Canceled);
                }

                output = sink.Complete();
            }

            history.Add(new StepRun(cursor, iteration, result, output));
            _journal.Append(runId, new WorkflowEvent.StepFinished(cursor, iteration, result, output));

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
