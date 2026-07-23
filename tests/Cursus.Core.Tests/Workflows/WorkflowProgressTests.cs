using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Execution;
using Cursus.Core.Workflows.Journaling;

using static Cursus.Core.Tests.Workflows.WorkflowFixtures;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// Ce que le moteur pousse en direct à un observateur pendant qu'il traverse —
/// le flux éphémère sur lequel l'écran de run se branchera (6c·3c). Invariant
/// central : ce flux part des mêmes points que le journal, donc ne diverge
/// jamais de lui.
/// </summary>
public class WorkflowProgressTests
{
    [Fact(DisplayName = "étant donné un observateur passé à l'exécution, quand le run traverse deux étapes reliées, alors il reçoit démarrage, entrée, fin, choix d'arête, entrée, fin, fin de run dans cet ordre")]
    public async Task The_observer_receives_every_milestone_in_order()
    {
        // arrange
        var observer = new RecordingObserver();
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B"),
        });

        // act
        await Engine(new StubProcessRunner(Exit(0)))
            .ExecuteAsync(definition, Workspace, NewRunId(), observer: observer);

        // assert
        Assert.Collection(
            observer.Events,
            e => Assert.IsType<WorkflowEvent.RunStarted>(e),
            e => Assert.IsType<WorkflowEvent.StepStarted>(e),
            e => Assert.IsType<WorkflowEvent.StepFinished>(e),
            e => Assert.IsType<WorkflowEvent.EdgeChosen>(e),
            e => Assert.IsType<WorkflowEvent.StepStarted>(e),
            e => Assert.IsType<WorkflowEvent.StepFinished>(e),
            e => Assert.IsType<WorkflowEvent.RunFinished>(e));
    }

    [Fact(DisplayName = "étant donné un observateur, quand le run se termine, alors la séquence qu'il a reçue est identique à celle enregistrée par le journal")]
    public async Task The_observed_sequence_is_identical_to_the_journalled_one()
    {
        // arrange — l'invariant : émission live et écriture durable partent du même point
        var journal = new InMemoryRunJournal();
        var observer = new RecordingObserver();
        var definition = new WorkflowDefinition("D", new[]
        {
            Step("D", maxVisits: 2, new Edge(Guard.OnFailure, "D")),
        });

        // act
        await Engine(new StubProcessRunner(Exit(1)), journal)
            .ExecuteAsync(definition, Workspace, NewRunId(), observer: observer);

        // assert
        Assert.Equal(journal.Entries.Select(entry => entry.Event), observer.Events);
    }

    [Fact(DisplayName = "étant donné aucun observateur, quand le run s'exécute, alors il se déroule et journalise normalement")]
    public async Task Without_an_observer_the_run_still_journals_normally()
    {
        // arrange — l'observateur est optionnel : un run headless n'en fournit pas
        var journal = new InMemoryRunJournal();
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        var run = await Engine(new StubProcessRunner(Exit(0)), journal)
            .ExecuteAsync(definition, Workspace, NewRunId());

        // assert
        Assert.Equal(RunState.Completed, run.State);
        Assert.IsType<WorkflowEvent.RunStarted>(journal.Entries[0].Event);
        Assert.IsType<WorkflowEvent.RunFinished>(journal.Entries[^1].Event);
    }

    [Fact(DisplayName = "étant donné un run annulé en cours de route, quand l'annulation le clôt, alors l'observateur reçoit la même clôture pour annulation que le journal")]
    public async Task A_cancelled_run_pushes_the_same_closing_event_as_the_journal()
    {
        // arrange
        using var cancellation = new CancellationTokenSource();
        var journal = new InMemoryRunJournal();
        var observer = new RecordingObserver();
        var runner = new StubProcessRunner(Exit(0)) { CancelAfterRun = cancellation };
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B"),
        });

        // act
        await Engine(runner, journal)
            .ExecuteAsync(definition, Workspace, NewRunId(), observer: observer, cancellationToken: cancellation.Token);

        // assert
        var journalled = Assert.IsType<WorkflowEvent.RunFinished>(journal.Entries[^1].Event);
        var observed = Assert.IsType<WorkflowEvent.RunFinished>(observer.Events[^1]);
        Assert.Equal(AbortReason.Canceled, journalled.AbortReason);
        Assert.Equal(journalled, observed);
    }
}
