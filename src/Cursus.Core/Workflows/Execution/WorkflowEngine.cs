using Cursus.Core.Workflows.Journaling;
using Cursus.Core.Workflows.Output;

namespace Cursus.Core.Workflows.Execution;

/// <summary>
/// Parcourt un graphe de <see cref="StepDefinition"/> depuis son point d'entrée,
/// exécute chaque visite via l'<see cref="IStepExecutor"/> qui sait traiter son type,
/// route sur le code de sortie via les arêtes gardées, borne les boucles, et synthétise
/// un <see cref="WorkflowRun"/>. Ne connaît rien d'un agent ni d'aucun kind : router par
/// le type de l'étape est ce qui laisse greffer un kind sans toucher la traversée (§5).
/// </summary>
public sealed class WorkflowEngine
{
    private readonly IReadOnlyList<IStepExecutor> _executors;
    private readonly IRunJournal _journal;
    private readonly IRunOutputStore _output;

    public WorkflowEngine(
        IProcessRunner runner, IRunJournal journal, IRunOutputStore output, ITaskTracker? tracker = null)
    {
        // Les kinds du noyau, chacun avec son exécuteur — script et agent adossés au même
        // runner de process (l'agent est headless), tâche adossée au tracker. Ajouter un
        // kind, c'est allonger cette liste, jamais toucher la boucle de traversée : le
        // premier exécuteur qui reconnaît l'étape la prend, l'ordre départageant un
        // éventuel recouvrement (aucun aujourd'hui, les kinds étant disjoints par type).
        //
        // Le tracker est optionnel : tant que le client réel (Linear, jambe 2·2b) n'est
        // pas branché, l'appelant n'en fournit pas, et une étape-tâche échoue de façon
        // routable au lieu de casser le run (le null-object ci-dessous jette, la garde de
        // l'exécuteur le traduit en échec).
        _executors =
        [
            new ScriptStepExecutor(runner),
            new AgentStepExecutor(runner),
            new TaskStepExecutor(tracker ?? UnconfiguredTaskTracker.Instance),
        ];
        _journal = journal;
        _output = output;
    }

    public async Task<WorkflowRun> ExecuteAsync(
        WorkflowDefinition definition,
        RunContext context,
        string runId,
        RunTrigger? trigger = null,
        string? workflowId = null,
        IProgress<WorkflowEvent>? observer = null,
        CancellationToken cancellationToken = default)
    {
        // L'identité n'est plus forgée ici : l'appelant la fournit, parce que
        // c'est lui — le fabricant du RunContext — qui provisionne le workspace
        // isolé à ce nom, avant même que le run démarre. Le workflowId, lui, est
        // la provenance : de quel workflow du catalogue ce run est issu.
        var resolvedTrigger = trigger ?? RunTrigger.Manual;
        Emit(runId, new WorkflowEvent.RunStarted(
            definition, context.WorkspaceRoot, resolvedTrigger, workflowId, runId), observer);

        try
        {
            return await TraverseAsync(
                    definition, context, resolvedTrigger.TaskKey, runId, observer, cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            // Les invariants (étape inconnue, évasion de chemin) remontent tels
            // quels — mais le run doit être clos avant, sinon il resterait « en
            // cours » à jamais dans le journal, indiscernable d'un crash machine.
            Emit(runId, new WorkflowEvent.RunFinished(RunState.Aborted, AbortReason.Faulted), observer);
            throw;
        }
    }

    /// <summary>
    /// Le seul point d'émission : rendre l'événement durable (journal) et le
    /// pousser à l'observateur live (s'il y en a un), dans le même geste. Router
    /// tout par ici est ce qui garantit que le flux et le journal ne divergent
    /// jamais — même séquence, même ordre, par construction.
    /// </summary>
    private void Emit(string runId, WorkflowEvent @event, IProgress<WorkflowEvent>? observer)
    {
        _journal.Append(runId, @event);
        observer?.Report(@event);
    }

    private async Task<WorkflowRun> TraverseAsync(
        WorkflowDefinition definition,
        RunContext context,
        string? taskKey,
        string runId,
        IProgress<WorkflowEvent>? observer,
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
                Emit(runId, new WorkflowEvent.RunFinished(
                    RunState.Aborted, AbortReason.LoopNotConverging), observer);
                return new WorkflowRun(runId, RunState.Aborted, history, AbortReason.LoopNotConverging);
            }

            Emit(runId, new WorkflowEvent.StepStarted(cursor, iteration), observer);

            // Le puits s'ouvre avant l'étape : c'est ce qui permet à la sortie de
            // ruisseler pendant le run plutôt que d'être écrite à la fin.
            ScriptResult result;
            StepOutput output;
            using (var sink = _output.Open(runId, cursor, iteration))
            {
                try
                {
                    // La définition déclare un sous-chemin relatif, l'exécuteur attend
                    // un répertoire absolu : le moteur est le seul à connaître le
                    // contexte, donc le seul à pouvoir traduire. Le choix de l'exécuteur
                    // se fait sur le type de l'étape — le moteur, lui, reste kind-aveugle.
                    var stepContext = new StepExecutionContext(
                        context.Resolve(step.WorkingSubdirectory), taskKey);
                    var executor = _executors.First(e => e.CanExecute(step));
                    result = await executor
                        .ExecuteAsync(step, stepContext, sink.Stdout, sink.Stderr, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // L'annulation interrompt le run mais ne l'efface pas : la
                    // trajectoire déjà parcourue reste observable, et la sortie
                    // partielle est chassée sur disque en refermant le puits.
                    Emit(runId, new WorkflowEvent.RunFinished(
                        RunState.Aborted, AbortReason.Canceled), observer);
                    return new WorkflowRun(runId, RunState.Aborted, history, AbortReason.Canceled);
                }

                output = sink.Complete();
            }

            history.Add(new StepRun(cursor, iteration, result, output));
            Emit(runId, new WorkflowEvent.StepFinished(cursor, iteration, result, output), observer);

            var edge = step.OutEdges.FirstOrDefault(e => e.Guard.Matches(result));
            if (edge is null)
            {
                var state = result.IsSuccess ? RunState.Completed : RunState.Failed;
                Emit(runId, new WorkflowEvent.RunFinished(state), observer);
                return new WorkflowRun(runId, state, history);
            }

            Emit(runId, new WorkflowEvent.EdgeChosen(cursor, edge.Target), observer);
            cursor = edge.Target;
        }
    }
}

/// <summary>
/// Le tracker par défaut du moteur tant qu'aucun client réel n'est branché : il
/// refuse tout geste. L'exécuteur d'étape-tâche traduit ce refus en échec routable,
/// de sorte qu'une définition contenant une étape-tâche reste lançable — elle échoue
/// proprement au lieu de casser le run — avant même que Linear (jambe 2·2b) existe.
/// </summary>
internal sealed class UnconfiguredTaskTracker : ITaskTracker
{
    internal static readonly UnconfiguredTaskTracker Instance = new();

    private static readonly InvalidOperationException NotConfigured =
        new("Aucun tracker de tâches n'est configuré : cette étape-tâche ne peut pas agir sur le tableau.");

    public Task<TaskCard> ReadAsync(string key, CancellationToken cancellationToken = default) => throw NotConfigured;

    public Task MoveAsync(string key, string column, CancellationToken cancellationToken = default) => throw NotConfigured;

    public Task ApplyLabelAsync(string key, string label, CancellationToken cancellationToken = default) => throw NotConfigured;
}
