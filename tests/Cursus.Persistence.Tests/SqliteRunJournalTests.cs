using Cursus.Core.Workflows;

namespace Cursus.Persistence.Tests;

/// <summary>
/// Ce que le journal SQLite retient d'un run. La table des événements est la
/// source ; celle des runs n'en est qu'une projection, entretenue à l'écriture
/// pour qu'une liste ne coûte pas un rejeu complet.
/// </summary>
public class SqliteRunJournalTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("cursus-journal-").FullName;

    [Fact(DisplayName = "étant donné une base inexistante, quand on construit le journal, alors le schéma est créé et le journal est utilisable")]
    public void A_journal_creates_its_schema_on_a_database_that_does_not_exist_yet()
    {
        // arrange / act
        using var journal = NewJournal();
        journal.Append("run-1", AnyStart);

        // assert
        Assert.Single(journal.ReadEvents("run-1"));
    }

    [Fact(DisplayName = "étant donné un démarrage journalisé puis le journal refermé et rouvert, quand on le relit, alors il porte la définition figée, la racine et le déclenchement")]
    public void A_start_survives_a_reopen_with_its_definition_its_root_and_its_trigger()
    {
        // arrange
        using (var journal = NewJournal())
        {
            journal.Append("run-1", new WorkflowEvent.RunStarted(
                AnyDefinition, "/un/workspace", RunTrigger.ForTask("ENG-1234")));
        }

        // act
        using var reopened = NewJournal();
        var started = Assert.IsType<WorkflowEvent.RunStarted>(reopened.ReadEvents("run-1").Single().Event);

        // assert
        Assert.Equal("A", started.Definition.EntryStep);
        Assert.Equal("/un/workspace", started.WorkspaceRoot);
        Assert.Equal(RunTriggerKind.Task, started.Trigger.Kind);
        Assert.Equal("ENG-1234", started.Trigger.TaskKey);
    }

    [Fact(DisplayName = "étant donné un run entier journalisé puis le journal refermé et rouvert, quand on le relit, alors les événements reviennent dans l'ordre de leur séquence")]
    public void A_whole_run_survives_a_reopen_in_sequence_order()
    {
        // arrange
        using (var journal = NewJournal())
        {
            journal.Append("run-1", AnyStart);
            journal.Append("run-1", new WorkflowEvent.StepStarted("A", 1));
            journal.Append("run-1", new WorkflowEvent.StepFinished("A", 1, new ScriptResult(0, ScriptOutcome.Completed), NoOutput));
            journal.Append("run-1", new WorkflowEvent.EdgeChosen("A", "B"));
            journal.Append("run-1", new WorkflowEvent.RunFinished(RunState.Completed));
        }

        // act
        using var reopened = NewJournal();
        var entries = reopened.ReadEvents("run-1");

        // assert
        Assert.Equal(new long[] { 1, 2, 3, 4, 5 }, entries.Select(entry => entry.Seq));
        Assert.Collection(
            entries.Select(entry => entry.Event),
            e => Assert.IsType<WorkflowEvent.RunStarted>(e),
            e => Assert.IsType<WorkflowEvent.StepStarted>(e),
            e => Assert.IsType<WorkflowEvent.StepFinished>(e),
            e => Assert.Equal("B", Assert.IsType<WorkflowEvent.EdgeChosen>(e).ToStepId),
            e => Assert.Equal(RunState.Completed, Assert.IsType<WorkflowEvent.RunFinished>(e).State));
    }

    [Fact(DisplayName = "étant donné une visite terminée, quand on la relit, alors elle porte son code de sortie et son issue")]
    public void A_finished_visit_reads_back_with_its_exit_code_and_outcome()
    {
        // arrange
        using var journal = NewJournal();
        journal.Append("run-1", AnyStart);

        // act
        journal.Append("run-1", new WorkflowEvent.StepFinished(
            "A", 2, new ScriptResult(3, ScriptOutcome.TimedOut, Duration: TimeSpan.FromSeconds(1.5)), NoOutput));

        // assert
        var visit = Assert.IsType<WorkflowEvent.StepFinished>(journal.ReadEvents("run-1")[^1].Event);
        Assert.Equal("A", visit.StepId);
        Assert.Equal(2, visit.Iteration);
        Assert.Equal(3, visit.Result.ExitCode);
        Assert.Equal(ScriptOutcome.TimedOut, visit.Result.Outcome);
        Assert.Equal(TimeSpan.FromSeconds(1.5), visit.Result.Duration);
    }

    [Fact(DisplayName = "étant donné une visite portant des artefacts de sortie, quand on la relit, alors ses artefacts sont restitués et le journal n'a gardé aucun octet de contenu")]
    public void A_visit_reads_back_with_its_artifacts_and_no_output_bytes()
    {
        // arrange
        using var journal = NewJournal();
        journal.Append("run-1", AnyStart);
        var output = new StepOutput([
            new OutputArtifact("stdout", "/runs/run-1/A.1.stdout", 13),
            new OutputArtifact("stderr", null, 0),
        ]);

        // act — le journal ne garde des sorties que les artefacts (nom, chemin,
        // taille), jamais les octets : ceux-ci vivent dans le magasin.
        journal.Append("run-1", new WorkflowEvent.StepFinished(
            "A", 1, new ScriptResult(0, ScriptOutcome.Completed), output));

        // assert
        var visit = Assert.IsType<WorkflowEvent.StepFinished>(journal.ReadEvents("run-1")[^1].Event);
        var stdout = visit.Output.Artifacts.Single(a => a.Name == "stdout");
        Assert.Equal("/runs/run-1/A.1.stdout", stdout.Path);
        Assert.Equal(13, stdout.Size);
        Assert.Null(visit.Output.Artifacts.Single(a => a.Name == "stderr").Path);
    }

    [Fact(DisplayName = "étant donné deux runs journalisés, quand on relit les événements de l'un, alors ceux de l'autre n'y figurent pas")]
    public void Reading_one_run_does_not_return_the_events_of_another()
    {
        // arrange
        using var journal = NewJournal();

        // act
        journal.Append("run-1", AnyStart);
        journal.Append("run-2", AnyStart);
        journal.Append("run-2", new WorkflowEvent.StepStarted("A", 1));

        // assert
        Assert.Single(journal.ReadEvents("run-1"));
        Assert.Equal(2, journal.ReadEvents("run-2").Count);
    }

    [Fact(DisplayName = "étant donné un run encore en cours et un run achevé, quand on liste les runs, alors seul le second porte un état terminal")]
    public void A_run_still_going_is_listed_without_a_terminal_state()
    {
        // arrange
        using var journal = NewJournal();
        journal.Append("en-cours", AnyStart);
        journal.Append("acheve", AnyStart);

        // act
        journal.Append("acheve", new WorkflowEvent.RunFinished(RunState.Aborted, AbortReason.LoopNotConverging));

        // assert
        var runs = journal.ListRuns().ToDictionary(run => run.RunId);
        Assert.Null(runs["en-cours"].State);
        Assert.Equal(RunState.Aborted, runs["acheve"].State);
        Assert.Equal(AbortReason.LoopNotConverging, runs["acheve"].AbortReason);
    }

    [Fact(DisplayName = "étant donné plusieurs runs journalisés en concurrence sur une même base, quand tous se terminent, alors chacun se relit intégralement, avec sa séquence propre continue, sans perte ni doublon")]
    public async Task Concurrent_appends_never_corrupt_the_journal()
    {
        // arrange — une seule base, donc une seule connexion partagée par tous les runs
        using var journal = NewJournal();
        const int runs = 24;
        const int eventsPerRun = 6;

        // act — les runs écrivent de front : sans sérialisation, deux transactions
        // sur la même connexion se heurtent, et la contention le rend quasi certain
        await Task.WhenAll(Enumerable.Range(0, runs).Select(r => Task.Run(() =>
        {
            var runId = $"run-{r}";
            journal.Append(runId, AnyStart);
            for (var i = 1; i <= eventsPerRun - 2; i++)
                journal.Append(runId, new WorkflowEvent.StepStarted("A", i));
            journal.Append(runId, new WorkflowEvent.RunFinished(RunState.Completed));
        })));

        // assert — chaque run a ses événements numérotés de 1 à N, sans trou ni doublon
        for (var r = 0; r < runs; r++)
            Assert.Equal(
                Enumerable.Range(1, eventsPerRun).Select(i => (long)i),
                journal.ReadEvents($"run-{r}").Select(entry => entry.Seq));
    }

    // --- helpers ---

    private SqliteRunJournal NewJournal() => new(Path.Combine(_root, "cursus.db"));

    private static StepOutput NoOutput => new([]);

    /// <summary>
    /// Un graphe minimal mais <b>valide</b> : la définition figée dans la table
    /// des runs repasse par le validateur à la relecture, et un graphe bancal
    /// n'y survivrait pas.
    /// </summary>
    private static WorkflowDefinition AnyDefinition => new("A", new[]
    {
        new StepDefinition("A", "A", new ScriptSpec("/usr/bin/true", []), MaxVisits: 1, []),
    });

    private static WorkflowEvent AnyStart =>
        new WorkflowEvent.RunStarted(AnyDefinition, "/tmp", RunTrigger.Manual);

    public void Dispose() => Directory.Delete(_root, recursive: true);
}
