using Cursus.Core.Workflows;

namespace Cursus.Core.Tests.Workflows;

public class WorkflowEngineTests
{
    [Fact(DisplayName = "étant donné un graphe A→B→C relié en succès et un runner qui réussit, quand on exécute le workflow, alors l'historique est A, B, C et le run est terminé avec succès")]
    public async Task Sequential_success_path_visits_all_steps_in_order()
    {
        // arrange
        var runner = new StubProcessRunner(Exit(0));
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B", new Edge(Guard.OnSuccess, "C")),
            Step("C"),
        });
        var engine = new WorkflowEngine(runner);

        // act
        var run = await engine.ExecuteAsync(definition, Workspace);

        // assert
        Assert.Equal(new[] { "A", "B", "C" }, run.History.Select(s => s.StepId));
        Assert.Equal(RunState.Completed, run.State);
    }

    [Fact(DisplayName = "étant donné les arêtes succès→B puis échec→C et un runner qui réussit, quand on route l'étape, alors la cible retenue est B")]
    public async Task Routing_takes_the_success_edge_when_the_step_succeeds()
    {
        // arrange
        var runner = new StubProcessRunner(Exit(0));
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B"), new Edge(Guard.OnFailure, "C")),
            Step("B"),
            Step("C"),
        });

        // act
        var run = await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace);

        // assert
        Assert.Equal(new[] { "A", "B" }, run.History.Select(s => s.StepId));
    }

    [Fact(DisplayName = "étant donné les arêtes succès→B puis échec→C et un runner qui échoue, quand on route l'étape, alors la cible retenue est C")]
    public async Task Routing_takes_the_failure_edge_when_the_step_fails()
    {
        // arrange
        var runner = new StubProcessRunner(Exit(1));
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B"), new Edge(Guard.OnFailure, "C")),
            Step("B"),
            Step("C"),
        });

        // act
        var run = await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace);

        // assert
        Assert.Equal(new[] { "A", "C" }, run.History.Select(s => s.StepId));
    }

    [Fact(DisplayName = "étant donné les arêtes code 2→B puis défaut→C et un runner qui sort en code 2, quand on route l'étape, alors la cible retenue est B")]
    public async Task Routing_takes_the_matching_exit_code_edge()
    {
        // arrange
        var runner = new StubProcessRunner(Exit(2));
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnExitCode(2), "B"), new Edge(Guard.Default, "C")),
            Step("B"),
            Step("C"),
        });

        // act
        var run = await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace);

        // assert
        Assert.Equal(new[] { "A", "B" }, run.History.Select(s => s.StepId));
    }

    [Fact(DisplayName = "étant donné les arêtes code 2→B puis défaut→C et un runner qui sort en code 5, quand on route l'étape, alors la cible de repli est C")]
    public async Task Routing_falls_back_to_the_default_edge_when_no_exit_code_matches()
    {
        // arrange
        var runner = new StubProcessRunner(Exit(5));
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnExitCode(2), "B"), new Edge(Guard.Default, "C")),
            Step("B"),
            Step("C"),
        });

        // act
        var run = await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace);

        // assert
        Assert.Equal(new[] { "A", "C" }, run.History.Select(s => s.StepId));
    }

    [Fact(DisplayName = "étant donné une étape terminale atteinte en succès, quand on exécute, alors le run est terminé avec succès sur cette étape")]
    public async Task A_terminal_step_reached_on_success_completes_the_run()
    {
        // arrange
        var runner = new StubProcessRunner(Exit(0));
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        var run = await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace);

        // assert
        Assert.Equal(new[] { "A" }, run.History.Select(s => s.StepId));
        Assert.Equal(RunState.Completed, run.State);
    }

    [Fact(DisplayName = "étant donné une étape qui échoue sans arête applicable, quand on exécute, alors le run est en échec sur cette étape")]
    public async Task A_failing_step_without_a_matching_edge_fails_the_run()
    {
        // arrange
        var runner = new StubProcessRunner(Exit(1));
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        var run = await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace);

        // assert
        Assert.Equal(RunState.Failed, run.State);
    }

    [Fact(DisplayName = "étant donné deux arêtes dont les gardes matchent toutes les deux, quand on route, alors la première déclarée l'emporte")]
    public async Task Routing_prefers_the_first_declared_matching_edge()
    {
        // arrange
        var runner = new StubProcessRunner(Exit(0));
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B"), new Edge(Guard.Default, "C")),
            Step("B"),
            Step("C"),
        });

        // act
        var run = await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace);

        // assert
        Assert.Equal(new[] { "A", "B" }, run.History.Select(s => s.StepId));
    }

    [Fact(DisplayName = "étant donné une boucle échec→soi avec maxVisits 3 et un runner qui échoue toujours, quand on exécute, alors 3 visites ont lieu puis le run est interrompu pour boucle non convergente")]
    public async Task A_never_converging_loop_is_aborted_at_max_visits()
    {
        // arrange
        var runner = new StubProcessRunner(Exit(1));
        var definition = new WorkflowDefinition("D", new[]
        {
            Step("D", maxVisits: 3, new Edge(Guard.OnFailure, "D")),
        });

        // act
        var run = await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace);

        // assert
        Assert.Equal(new[] { 1, 2, 3 }, run.History.Select(s => s.Iteration));
        Assert.Equal(RunState.Aborted, run.State);
        Assert.Equal(AbortReason.LoopNotConverging, run.AbortReason);
    }

    [Fact(DisplayName = "étant donné une boucle échec→soi et un runner qui échoue puis réussit, quand on exécute, alors 2 visites ont lieu et le run est terminé avec succès")]
    public async Task A_loop_that_converges_stops_and_completes()
    {
        // arrange
        var runner = new StubProcessRunner(Exit(1), Exit(0));
        var definition = new WorkflowDefinition("D", new[]
        {
            Step("D", maxVisits: 3, new Edge(Guard.OnFailure, "D")),
        });

        // act
        var run = await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace);

        // assert
        Assert.Equal(new[] { 1, 2 }, run.History.Select(s => s.Iteration));
        Assert.Equal(RunState.Completed, run.State);
    }

    [Fact(DisplayName = "étant donné une boucle échec→soi et un runner qui réussit d'emblée, quand on exécute, alors une seule visite a lieu et le run n'est pas interrompu")]
    public async Task A_loop_that_converges_on_the_first_try_runs_once()
    {
        // arrange
        var runner = new StubProcessRunner(Exit(0));
        var definition = new WorkflowDefinition("D", new[]
        {
            Step("D", maxVisits: 3, new Edge(Guard.OnFailure, "D")),
        });

        // act
        var run = await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace);

        // assert
        Assert.Single(run.History);
        Assert.Equal(RunState.Completed, run.State);
    }

    [Fact(DisplayName = "étant donné une étape avec arêtes succès/échec et un runner qui dépasse le délai, quand on route, alors l'arête d'échec est retenue")]
    public async Task A_timed_out_step_routes_through_the_failure_edge()
    {
        // arrange
        var runner = new StubProcessRunner(new ScriptResult(-1, ScriptOutcome.TimedOut));
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B"), new Edge(Guard.OnFailure, "C")),
            Step("B"),
            Step("C"),
        });

        // act
        var run = await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace);

        // assert
        Assert.Equal(new[] { "A", "C" }, run.History.Select(s => s.StepId));
    }

    [Fact(DisplayName = "étant donné une étape dont le lancement échoue et sans arête applicable, quand on exécute, alors le run est en échec")]
    public async Task A_launch_failure_fails_the_run()
    {
        // arrange
        var runner = new StubProcessRunner(new ScriptResult(127, ScriptOutcome.LaunchFailed));
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        var run = await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace);

        // assert
        Assert.Equal(RunState.Failed, run.State);
    }

    [Fact(DisplayName = "étant donné une arête qui pointe une étape absente du graphe, quand on exécute, alors une erreur d'étape inconnue est levée")]
    public async Task Routing_to_an_unknown_step_raises_a_named_error()
    {
        // arrange
        var runner = new StubProcessRunner(Exit(0));
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "Z")),
        });

        // act / assert
        await Assert.ThrowsAsync<UnknownStepException>(
            async () => await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace));
    }

    [Fact(DisplayName = "étant donné un jeton annulé après la première étape, quand on exécute le workflow, alors le run est interrompu pour annulation")]
    public async Task A_cancelled_run_is_aborted()
    {
        // arrange
        using var cancellation = new CancellationTokenSource();
        var runner = new StubProcessRunner(Exit(0)) { CancelAfterRun = cancellation };
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B"),
        });

        // act
        var run = await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace, cancellation.Token);

        // assert
        Assert.Equal(RunState.Aborted, run.State);
        Assert.Equal(AbortReason.Canceled, run.AbortReason);
    }

    [Fact(DisplayName = "étant donné un jeton annulé après la première étape, quand on exécute le workflow, alors l'historique conserve les étapes déjà exécutées")]
    public async Task A_cancelled_run_keeps_the_steps_already_executed()
    {
        // arrange
        using var cancellation = new CancellationTokenSource();
        var runner = new StubProcessRunner(Exit(0)) { CancelAfterRun = cancellation };
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B"),
        });

        // act
        var run = await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace, cancellation.Token);

        // assert
        Assert.Equal(new[] { "A" }, run.History.Select(s => s.StepId));
    }

    [Fact(DisplayName = "étant donné une étape sans sous-chemin, quand on l'exécute, alors le script reçoit la racine du workspace comme répertoire de travail")]
    public async Task A_step_without_a_subdirectory_runs_at_the_workspace_root()
    {
        // arrange
        var runner = new StubProcessRunner(Exit(0));
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace);

        // assert
        Assert.Equal(Workspace.WorkspaceRoot, runner.Executed.Single().WorkingDirectory);
    }

    [Fact(DisplayName = "étant donné deux étapes déclarant des sous-chemins différents, quand on les exécute, alors chacune reçoit le sien résolu sous la racine")]
    public async Task Each_step_runs_in_its_own_resolved_subdirectory()
    {
        // arrange
        var runner = new StubProcessRunner(Exit(0));
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")) with { WorkingSubdirectory = "backend" },
            Step("B") with { WorkingSubdirectory = "frontend/web" },
        });

        // act
        await new WorkflowEngine(runner).ExecuteAsync(definition, Workspace);

        // assert
        Assert.Equal(
            new[]
            {
                Path.Combine(Workspace.WorkspaceRoot, "backend"),
                Path.Combine(Workspace.WorkspaceRoot, "frontend", "web"),
            },
            runner.Executed.Select(s => s.WorkingDirectory));
    }

    // --- helpers ---

    /// <summary>Un workspace neuf, partagé : aucun test de cette classe n'écrit dedans.</summary>
    private static readonly RunContext Workspace =
        new(Directory.CreateTempSubdirectory("cursus-engine-").FullName);

    private static readonly ScriptSpec AnyScript = new("/usr/bin/true", []);

    private static StepDefinition Step(string id, params Edge[] edges) =>
        new(id, id, AnyScript, MaxVisits: 1, edges);

    private static StepDefinition Step(string id, int maxVisits, params Edge[] edges) =>
        new(id, id, AnyScript, maxVisits, edges);

    private static ScriptResult Exit(int code) => new(code, ScriptOutcome.Completed);
}

/// <summary>
/// Double de test : renvoie des résultats programmés, dans l'ordre ; répète le
/// dernier une fois la liste épuisée (pratique pour « le runner réussit toujours »).
/// </summary>
file sealed class StubProcessRunner : IProcessRunner
{
    private readonly IReadOnlyList<ScriptResult> _results;
    private int _index;

    public StubProcessRunner(params ScriptResult[] results) => _results = results;

    /// <summary>Si fourni, s'annule une fois l'exécution rendue — comme une annulation survenant pendant le run.</summary>
    public CancellationTokenSource? CancelAfterRun { get; init; }

    /// <summary>Les specs effectivement reçues, dans l'ordre — le moteur les compose avant de les transmettre.</summary>
    public List<ScriptSpec> Executed { get; } = [];

    public Task<ScriptResult> RunAsync(ScriptSpec spec, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Executed.Add(spec);

        var result = _results[Math.Min(_index, _results.Count - 1)];
        _index++;
        CancelAfterRun?.Cancel();
        return Task.FromResult(result);
    }
}
