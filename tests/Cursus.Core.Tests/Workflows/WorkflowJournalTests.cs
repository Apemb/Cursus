using Cursus.Core.Workflows;

using static Cursus.Core.Tests.Workflows.WorkflowFixtures;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// Ce que le moteur raconte pendant qu'il traverse — la seule source dont
/// disposera un observateur extérieur au run.
/// </summary>
public class WorkflowJournalTests
{
    [Fact(DisplayName = "étant donné un graphe à une étape réussie, quand on l'exécute, alors le journal porte le démarrage, l'entrée dans l'étape, sa fin et la fin du run, dans cet ordre")]
    public async Task A_successful_run_is_narrated_from_start_to_finish()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        await Engine(new StubProcessRunner(Exit(0)), journal)
            .ExecuteAsync(definition, Workspace, NewRunId());

        // assert
        Assert.Collection(
            journal.Entries.Select(entry => entry.Event),
            e => Assert.IsType<WorkflowEvent.RunStarted>(e),
            e => Assert.IsType<WorkflowEvent.StepStarted>(e),
            e => Assert.IsType<WorkflowEvent.StepFinished>(e),
            e => Assert.IsType<WorkflowEvent.RunFinished>(e));
    }

    [Fact(DisplayName = "étant donné un run, quand on l'exécute, alors tous ses événements portent le même identifiant de run")]
    public async Task Every_event_of_a_run_carries_the_same_run_identifier()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B"),
        });

        // act
        await Engine(new StubProcessRunner(Exit(0)), journal)
            .ExecuteAsync(definition, Workspace, NewRunId());

        // assert
        var runId = Assert.Single(journal.Entries.Select(entry => entry.RunId).Distinct());
        Assert.NotEmpty(runId);
    }

    [Fact(DisplayName = "étant donné deux exécutions successives du même moteur, quand on les journalise, alors leurs identifiants de run diffèrent")]
    public async Task Two_executions_of_the_same_engine_get_distinct_run_identifiers()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("A", new[] { Step("A") });
        var engine = Engine(new StubProcessRunner(Exit(0)), journal);

        // act
        await engine.ExecuteAsync(definition, Workspace, NewRunId());
        await engine.ExecuteAsync(definition, Workspace, NewRunId());

        // assert
        Assert.Equal(2, journal.Entries.Select(entry => entry.RunId).Distinct().Count());
    }

    [Fact(DisplayName = "étant donné un run terminé, quand on lit son résultat, alors il porte l'identifiant sous lequel il a été journalisé")]
    public async Task The_result_carries_the_identifier_the_run_was_journalled_under()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        var run = await Engine(new StubProcessRunner(Exit(0)), journal)
            .ExecuteAsync(definition, Workspace, NewRunId());

        // assert
        Assert.Equal(journal.Entries[0].RunId, run.RunId);
    }

    [Fact(DisplayName = "étant donné un identifiant de run fourni par l'appelant, quand le run s'exécute, alors le résultat et le journal portent cet identifiant, non un forgé par le moteur")]
    public async Task The_caller_supplies_the_run_identifier()
    {
        // arrange — l'identité n'est plus l'affaire du moteur : le futur host la
        // connaît avant le run, pour provisionner un worktree à son nom.
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        var run = await Engine(new StubProcessRunner(Exit(0)), journal)
            .ExecuteAsync(definition, Workspace, "run-choisi-dehors");

        // assert
        Assert.Equal("run-choisi-dehors", run.RunId);
        Assert.Equal("run-choisi-dehors", journal.Entries[0].RunId);
    }

    [Fact(DisplayName = "étant donné un démarrage journalisé, quand on le lit, alors il porte la définition exécutée et la racine du workspace")]
    public async Task The_start_event_carries_the_definition_and_the_workspace_root()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        await Engine(new StubProcessRunner(Exit(0)), journal)
            .ExecuteAsync(definition, Workspace, NewRunId());

        // assert
        var started = Assert.IsType<WorkflowEvent.RunStarted>(journal.Entries[0].Event);
        Assert.Same(definition, started.Definition);
        Assert.Equal(Workspace.WorkspaceRoot, started.WorkspaceRoot);
    }

    [Fact(DisplayName = "étant donné une étape qui route vers une autre, quand on l'exécute, alors le journal porte le choix d'arête et sa cible")]
    public async Task Routing_to_another_step_is_journalled_with_its_target()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B"),
        });

        // act
        await Engine(new StubProcessRunner(Exit(0)), journal)
            .ExecuteAsync(definition, Workspace, NewRunId());

        // assert
        var chosen = Assert.Single(
            journal.Entries.Select(entry => entry.Event).OfType<WorkflowEvent.EdgeChosen>());
        Assert.Equal("A", chosen.FromStepId);
        Assert.Equal("B", chosen.ToStepId);
    }

    [Fact(DisplayName = "étant donné une étape terminale sans arête applicable, quand on l'exécute, alors aucun choix d'arête n'est journalisé")]
    public async Task A_terminal_step_journals_no_edge_choice()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        await Engine(new StubProcessRunner(Exit(0)), journal)
            .ExecuteAsync(definition, Workspace, NewRunId());

        // assert
        Assert.Empty(journal.Entries.Select(entry => entry.Event).OfType<WorkflowEvent.EdgeChosen>());
    }

    [Fact(DisplayName = "étant donné une étape en boucle visitée trois fois, quand on l'exécute, alors chaque visite est journalisée avec son numéro d'itération")]
    public async Task Each_visit_of_a_looping_step_is_journalled_with_its_iteration()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("D", new[]
        {
            Step("D", maxVisits: 3, new Edge(Guard.OnFailure, "D")),
        });

        // act
        await Engine(new StubProcessRunner(Exit(1)), journal)
            .ExecuteAsync(definition, Workspace, NewRunId());

        // assert
        var visits = journal.Entries.Select(entry => entry.Event).OfType<WorkflowEvent.StepStarted>();
        Assert.Equal(new[] { 1, 2, 3 }, visits.Select(v => v.Iteration));
        Assert.All(visits, v => Assert.Equal("D", v.StepId));
    }

    [Fact(DisplayName = "étant donné une boucle qui dépasse son plafond, quand on l'exécute, alors le run se clôt sans qu'une visite supplémentaire soit journalisée")]
    public async Task Exceeding_the_visit_ceiling_closes_the_run_without_a_further_visit()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("D", new[]
        {
            Step("D", maxVisits: 2, new Edge(Guard.OnFailure, "D")),
        });

        // act
        await Engine(new StubProcessRunner(Exit(1)), journal)
            .ExecuteAsync(definition, Workspace, NewRunId());

        // assert
        Assert.Equal(2, journal.Entries.Count(entry => entry.Event is WorkflowEvent.StepStarted));
        Assert.IsType<WorkflowEvent.RunFinished>(journal.Entries[^1].Event);
    }

    [Fact(DisplayName = "étant donné un run annulé en cours de route, quand on l'exécute, alors le journal se clôt sur une interruption pour annulation")]
    public async Task A_cancelled_run_closes_its_journal_on_a_cancellation()
    {
        // arrange
        using var cancellation = new CancellationTokenSource();
        var journal = new InMemoryRunJournal();
        var runner = new StubProcessRunner(Exit(0)) { CancelAfterRun = cancellation };
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B"),
        });

        // act
        await Engine(runner, journal)
            .ExecuteAsync(definition, Workspace, NewRunId(), cancellationToken: cancellation.Token);

        // assert
        var finished = Assert.IsType<WorkflowEvent.RunFinished>(journal.Entries[^1].Event);
        Assert.Equal(RunState.Aborted, finished.State);
        Assert.Equal(AbortReason.Canceled, finished.AbortReason);
    }

    [Fact(DisplayName = "étant donné un point d'entrée inconnu, quand on exécute, alors le journal se clôt sur une interruption pour défaillance et l'exception remonte")]
    public async Task An_unknown_entry_step_closes_the_journal_before_the_exception_escapes()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("Z", new[] { Step("A") });

        // act
        await Assert.ThrowsAsync<UnknownStepException>(async () =>
            await Engine(new StubProcessRunner(Exit(0)), journal)
                .ExecuteAsync(definition, Workspace, NewRunId()));

        // assert
        var finished = Assert.IsType<WorkflowEvent.RunFinished>(journal.Entries[^1].Event);
        Assert.Equal(RunState.Aborted, finished.State);
        Assert.Equal(AbortReason.Faulted, finished.AbortReason);
    }

    [Fact(DisplayName = "étant donné un sous-chemin qui s'évade du workspace, quand on exécute, alors le journal se clôt sur une interruption pour défaillance et l'exception remonte")]
    public async Task An_escaping_subdirectory_closes_the_journal_before_the_exception_escapes()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A") with { WorkingSubdirectory = "../ailleurs" },
        });

        // act
        await Assert.ThrowsAsync<PathEscapesWorkspaceException>(async () =>
            await Engine(new StubProcessRunner(Exit(0)), journal)
                .ExecuteAsync(definition, Workspace, NewRunId()));

        // assert
        var finished = Assert.IsType<WorkflowEvent.RunFinished>(journal.Entries[^1].Event);
        Assert.Equal(AbortReason.Faulted, finished.AbortReason);
    }

    [Fact(DisplayName = "étant donné une visite terminée, quand on la journalise, alors elle porte l'étape, son itération et le résultat du script")]
    public async Task A_finished_visit_carries_its_step_its_iteration_and_its_result()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        await Engine(new StubProcessRunner(Exit(3)), journal)
            .ExecuteAsync(definition, Workspace, NewRunId());

        // assert
        var visit = Assert.Single(
            journal.Entries.Select(entry => entry.Event).OfType<WorkflowEvent.StepFinished>());
        Assert.Equal("A", visit.StepId);
        Assert.Equal(1, visit.Iteration);
        Assert.Equal(3, visit.Result.ExitCode);
    }

    [Fact(DisplayName = "étant donné un run sans déclencheur explicite, quand on l'exécute, alors le démarrage journalisé le dit déclenché à la main")]
    public async Task A_run_without_an_explicit_trigger_is_journalled_as_manual()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        await Engine(new StubProcessRunner(Exit(0)), journal)
            .ExecuteAsync(definition, Workspace, NewRunId());

        // assert
        var started = Assert.IsType<WorkflowEvent.RunStarted>(journal.Entries[0].Event);
        Assert.Equal(RunTriggerKind.Manual, started.Trigger.Kind);
        Assert.Null(started.Trigger.TaskKey);
    }

    [Fact(DisplayName = "étant donné un déclenchement par une tâche, quand on exécute, alors le démarrage journalisé porte la clé de cette tâche")]
    public async Task A_task_triggered_run_journals_the_task_key()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        await Engine(new StubProcessRunner(Exit(0)), journal)
            .ExecuteAsync(definition, Workspace, NewRunId(), RunTrigger.ForTask("ENG-1234"));

        // assert
        var started = Assert.IsType<WorkflowEvent.RunStarted>(journal.Entries[0].Event);
        Assert.Equal(RunTriggerKind.Task, started.Trigger.Kind);
        Assert.Equal("ENG-1234", started.Trigger.TaskKey);
    }
}
