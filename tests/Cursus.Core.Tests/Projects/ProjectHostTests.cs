using Cursus.Core.Projects;
using Cursus.Core.Tests.Workflows;
using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Execution;
using Cursus.Core.Workflows.Journaling;
using Cursus.Core.Workflows.Output;

namespace Cursus.Core.Tests.Projects;

/// <summary>
/// La racine de composition d'un projet ouvert : elle possède le journal et
/// répond à la seule question de la marche 3a — le dernier passage de chaque
/// workflow. Le journal est ici un double en mémoire, seedé à la main ; le vrai
/// préréglage SQLite est exercé par l'end-to-end de <c>Cursus.Persistence.Tests</c>.
/// </summary>
public class ProjectHostTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("cursus-host-").FullName;

    [Fact(DisplayName = "étant donné un projet dont aucun workflow n'a jamais tourné, quand on demande le dernier passage de chacun, alors chaque workflow connu est là, sans run")]
    public void Workflows_that_never_ran_are_reported_without_a_last_run()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "build");
        Deposit(project, "verifier");
        using var host = new ProjectHost(project, () => new InMemoryRunJournal(), AnyLauncher());

        // act
        var passages = host.LastRunPerWorkflow();

        // assert
        Assert.Equal(["build", "verifier"], passages.Select(passage => passage.Workflow.Id));
        Assert.All(passages, passage => Assert.Null(passage.LastRun));
    }

    [Fact(DisplayName = "étant donné un workflow qui a tourné à côté d'un qui n'a jamais tourné, quand on demande le dernier passage, alors seul le premier porte son run et son état")]
    public void A_workflow_that_ran_carries_its_run_while_the_others_stay_empty()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "build");
        Deposit(project, "verifier");
        var journal = new InMemoryRunJournal();
        journal.Append("r1", Started("verifier"));
        journal.Append("r1", new WorkflowEvent.RunFinished(RunState.Failed));
        using var host = new ProjectHost(project, () => journal, AnyLauncher());

        // act
        var passages = host.LastRunPerWorkflow().ToDictionary(passage => passage.Workflow.Id);

        // assert
        Assert.Null(passages["build"].LastRun);
        Assert.Equal("r1", passages["verifier"].LastRun!.RunId);
        Assert.Equal(RunState.Failed, passages["verifier"].LastRun!.State);
    }

    [Fact(DisplayName = "étant donné deux runs d'un même workflow à des instants différents, quand on demande le dernier passage, alors c'est le plus récent qui est rendu")]
    public void The_most_recent_run_of_a_workflow_wins()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "verifier");
        var clock = new TestClock(new DateTimeOffset(2026, 7, 20, 9, 0, 0, TimeSpan.Zero));
        var journal = new InMemoryRunJournal(clock);
        journal.Append("ancien", Started("verifier"));
        journal.Append("ancien", new WorkflowEvent.RunFinished(RunState.Failed));
        clock.UtcNow = clock.UtcNow.AddHours(2);
        journal.Append("recent", Started("verifier"));
        journal.Append("recent", new WorkflowEvent.RunFinished(RunState.Completed));
        using var host = new ProjectHost(project, () => journal, AnyLauncher());

        // act
        var passage = host.LastRunPerWorkflow().Single(passage => passage.Workflow.Id == "verifier");

        // assert
        Assert.Equal("recent", passage.LastRun!.RunId);
    }

    [Fact(DisplayName = "étant donné plusieurs runs d'un même workflow et un run d'un autre, quand on demande les runs de ce workflow, alors on les obtient tous, du plus récent au plus ancien, et aucun d'un autre workflow")]
    public void All_runs_of_a_workflow_come_back_most_recent_first_without_the_others()
    {
        // arrange — trois runs intercalés dans le temps : deux de « verifier »
        // encadrant un de « build », pour éprouver à la fois l'ordre et l'isolement.
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "build");
        Deposit(project, "verifier");
        var clock = new TestClock(new DateTimeOffset(2026, 7, 20, 9, 0, 0, TimeSpan.Zero));
        var journal = new InMemoryRunJournal(clock);
        journal.Append("v-ancien", Started("verifier"));
        journal.Append("v-ancien", new WorkflowEvent.RunFinished(RunState.Failed));
        clock.UtcNow = clock.UtcNow.AddHours(1);
        journal.Append("b1", Started("build"));
        journal.Append("b1", new WorkflowEvent.RunFinished(RunState.Completed));
        clock.UtcNow = clock.UtcNow.AddHours(1);
        journal.Append("v-recent", Started("verifier"));
        journal.Append("v-recent", new WorkflowEvent.RunFinished(RunState.Completed));
        using var host = new ProjectHost(project, () => journal, AnyLauncher());

        // act
        var runs = host.RunsOf("verifier");

        // assert
        Assert.Equal(["v-recent", "v-ancien"], runs.Select(run => run.RunId));
    }

    [Fact(DisplayName = "étant donné un host et l'id d'un workflow du projet, quand on le lance puis relit le dernier passage, alors ce workflow n'est plus « jamais lancé »")]
    public async Task Launching_a_workflow_fills_its_last_run()
    {
        // arrange — le lanceur écrit dans le même journal que le host relit : c'est
        // ce partage qui ferme la boucle 3a (lire) ↔ 3b (lancer).
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "verifier");
        var journal = new InMemoryRunJournal();
        var launcher = new WorkflowLauncher(
            new StubProcessRunner(new ScriptResult(0, ScriptOutcome.Completed)),
            journal, new InMemoryRunOutputStore(), new FakeProvisioner());
        using var host = new ProjectHost(project, () => journal, launcher);

        // act
        await host.LaunchAsync("verifier");

        // assert
        var passage = host.LastRunPerWorkflow().Single(passage => passage.Workflow.Id == "verifier");
        Assert.NotNull(passage.LastRun);
        Assert.Equal(RunState.Completed, passage.LastRun!.State);
    }

    [Fact(DisplayName = "étant donné un host ouvert sur un journal disposable, quand on dispose le host, alors le journal sous-jacent est disposé")]
    public void Disposing_the_host_disposes_its_journal()
    {
        // arrange — un projet minimal suffit, seule la vie du journal est en jeu
        var project = ProjectStore.Create(_root, "Démo");
        var journal = new DisposableJournalSpy();
        var host = new ProjectHost(project, () => journal, AnyLauncher());

        // act
        host.Dispose();

        // assert
        Assert.True(journal.Disposed);
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    // --- helpers ---

    private static void Deposit(Project project, string id) =>
        File.WriteAllText(Path.Combine(project.WorkflowsDirectory, $"{id}.json"), AnyDocument);

    /// <summary>
    /// Un lanceur dont ces tests-ci n'exercent pas le lancement : ils portent sur
    /// la lecture. Sur son propre journal jetable, sans rapport avec le lecteur.
    /// </summary>
    private static WorkflowLauncher AnyLauncher() =>
        new(new StubProcessRunner(new ScriptResult(0, ScriptOutcome.Completed)),
            new InMemoryRunJournal(), new InMemoryRunOutputStore(), new FakeProvisioner());

    /// <summary>Un démarrage rattaché à un workflow. Le double InMemory ne revalide
    /// pas la définition, un graphe nu suffit.</summary>
    private static WorkflowEvent Started(string workflowId) =>
        new WorkflowEvent.RunStarted(new WorkflowDefinition("A", []), "/tmp", RunTrigger.Manual, workflowId);

    /// <summary>
    /// Un journal disposable dont on n'observe que la disposition : le vrai
    /// journal SQLite détient une connexion à fermer, et c'est au host de le faire.
    /// </summary>
    private sealed class DisposableJournalSpy : IRunJournalReader, IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;

        public IReadOnlyList<RunSummary> ListRuns() => [];

        public IReadOnlyList<JournalEntry> ReadEvents(string runId) => [];
    }

    /// <summary>Un graphe valide dont le détail n'importe pas au host.</summary>
    private const string AnyDocument = """
        {
          "entryStep": "seule",
          "steps": [
            { "id": "seule", "name": "Seule", "maxVisits": 1,
              "script": { "fileName": "/bin/true", "arguments": [] }, "edges": [] }
          ]
        }
        """;
}
