using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Journaling;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// L'enveloppe que le journal pose autour de ce qu'on lui donne : le numéro de
/// séquence et l'instant. L'émetteur ne fournit ni l'un ni l'autre.
/// </summary>
public class InMemoryRunJournalTests
{
    [Fact(DisplayName = "étant donné un journal vide, quand on y ajoute un événement, alors il porte le premier numéro de séquence")]
    public void The_first_event_of_a_journal_is_numbered_one()
    {
        // arrange
        var journal = new InMemoryRunJournal();

        // act
        journal.Append("run-1", AnyEvent);

        // assert
        Assert.Equal(1, journal.Entries.Single().Seq);
    }

    [Fact(DisplayName = "étant donné plusieurs événements d'un même run, quand on les ajoute, alors leurs numéros de séquence se suivent")]
    public void The_events_of_a_run_are_numbered_consecutively()
    {
        // arrange
        var journal = new InMemoryRunJournal();

        // act
        journal.Append("run-1", AnyEvent);
        journal.Append("run-1", AnyEvent);
        journal.Append("run-1", AnyEvent);

        // assert
        Assert.Equal(new long[] { 1, 2, 3 }, journal.Entries.Select(entry => entry.Seq));
    }

    [Fact(DisplayName = "étant donné deux runs journalisés en alternance, quand on les relit, alors chacun a sa propre séquence")]
    public void Each_run_gets_its_own_sequence()
    {
        // arrange
        var journal = new InMemoryRunJournal();

        // act
        journal.Append("run-1", AnyEvent);
        journal.Append("run-2", AnyEvent);
        journal.Append("run-1", AnyEvent);
        journal.Append("run-2", AnyEvent);

        // assert
        Assert.Equal(new long[] { 1, 2 }, journal.ReadEvents("run-1").Select(entry => entry.Seq));
        Assert.Equal(new long[] { 1, 2 }, journal.ReadEvents("run-2").Select(entry => entry.Seq));
    }

    [Fact(DisplayName = "étant donné une horloge fixée, quand on ajoute un événement, alors il porte l'instant qu'elle donne")]
    public void An_entry_is_stamped_by_the_clock_it_was_given()
    {
        // arrange
        var instant = new DateTimeOffset(2026, 7, 20, 14, 30, 0, TimeSpan.Zero);
        var journal = new InMemoryRunJournal(new TestClock(instant));

        // act
        journal.Append("run-1", AnyEvent);

        // assert
        Assert.Equal(instant, journal.Entries.Single().At);
    }

    [Fact(DisplayName = "étant donné plusieurs runs journalisés, quand on liste les runs, alors le plus récemment démarré vient en premier")]
    public void Runs_are_listed_most_recently_started_first()
    {
        // arrange
        var clock = new TestClock(new DateTimeOffset(2026, 7, 20, 10, 0, 0, TimeSpan.Zero));
        var journal = new InMemoryRunJournal(clock);

        // act
        journal.Append("run-1", AnyStart);
        clock.UtcNow = clock.UtcNow.AddHours(1);
        journal.Append("run-2", AnyStart);

        // assert
        Assert.Equal(new[] { "run-2", "run-1" }, journal.ListRuns().Select(run => run.RunId));
    }

    [Fact(DisplayName = "étant donné un run encore en cours et un run achevé, quand on liste les runs, alors seul le second porte un état terminal")]
    public void A_run_still_going_is_listed_without_a_terminal_state()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        journal.Append("en-cours", AnyStart);
        journal.Append("acheve", AnyStart);

        // act
        journal.Append("acheve", new WorkflowEvent.RunFinished(RunState.Aborted, AbortReason.Canceled));

        // assert
        var runs = journal.ListRuns().ToDictionary(run => run.RunId);
        Assert.Null(runs["en-cours"].State);
        Assert.Equal(RunState.Aborted, runs["acheve"].State);
        Assert.Equal(AbortReason.Canceled, runs["acheve"].AbortReason);
    }

    [Fact(DisplayName = "étant donné plusieurs runs journalisés en concurrence, quand tous se terminent, alors chacun garde sa séquence propre, sans perte ni doublon")]
    public async Task Concurrent_appends_keep_each_run_sequence_intact()
    {
        // arrange — un seul journal, partagé par tous les runs
        var journal = new InMemoryRunJournal();
        const int runs = 24;
        const int eventsPerRun = 6;

        // act — les runs écrivent de front sur la même liste, non thread-safe sans garde
        await Task.WhenAll(Enumerable.Range(0, runs).Select(r => Task.Run(() =>
        {
            var runId = $"run-{r}";
            for (var i = 0; i < eventsPerRun; i++)
                journal.Append(runId, AnyEvent);
        })));

        // assert — chaque run porte ses événements numérotés de 1 à N, sans trou ni doublon
        for (var r = 0; r < runs; r++)
            Assert.Equal(
                Enumerable.Range(1, eventsPerRun).Select(i => (long)i),
                journal.ReadEvents($"run-{r}").Select(entry => entry.Seq));
    }

    [Fact(DisplayName = "étant donné un run clos démarré sous un id de workflow, quand on liste les runs, alors le résumé porte l'instant de clôture et le workflow, comme le journal durable")]
    public void A_finished_run_is_summarized_with_its_end_instant_and_workflow()
    {
        // arrange
        var clock = new TestClock(new DateTimeOffset(2026, 7, 20, 18, 0, 0, TimeSpan.Zero));
        var journal = new InMemoryRunJournal(clock);
        journal.Append("run-1", new WorkflowEvent.RunStarted(
            new WorkflowDefinition("A", []), "/tmp", RunTrigger.Manual, WorkflowId: "verifier"));

        // act
        clock.UtcNow = clock.UtcNow.AddMinutes(4);
        journal.Append("run-1", new WorkflowEvent.RunFinished(RunState.Failed));

        // assert
        var run = journal.ListRuns().Single();
        Assert.Equal("verifier", run.WorkflowId);
        Assert.Equal(clock.UtcNow, run.EndedAt);
    }

    private static WorkflowEvent AnyEvent => new WorkflowEvent.RunFinished(RunState.Completed);

    private static WorkflowEvent AnyStart =>
        new WorkflowEvent.RunStarted(new WorkflowDefinition("A", []), "/tmp", RunTrigger.Manual);
}
