using Cursus.Core.Projects;
using Cursus.Core.Workflows;

namespace Cursus.Persistence.Tests;

/// <summary>
/// Le premier des deux tests que §7.12 exige pour rendre <c>ProjectHost</c>
/// exécutable : un end-to-end <b>headless</b>, sur une <b>vraie</b> base SQLite,
/// sans instancier Avalonia. Il force le préréglage de <c>Cursus.Persistence</c>
/// à suffire — ouvrir le journal d'un projet et lire son passé. Cadré ici sur la
/// lecture ; 3b y ajoutera lancer/observer.
/// </summary>
public sealed class ProjectHostEndToEndTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("cursus-host-e2e-").FullName;

    [Fact(DisplayName = "étant donné un run journalisé dans la vraie base d'un projet, quand on ouvre un ProjectHost via le préréglage et lit le dernier passage, alors il rend ce run et son état, sans Avalonia")]
    public void A_project_host_reads_the_last_passage_over_a_real_database()
    {
        // arrange — un vrai projet sur disque, un run écrit dans sa vraie base
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "verifier");
        using (var journal = new SqliteRunJournal(project.DatabasePath))
        {
            journal.Append("r1", new WorkflowEvent.RunStarted(
                AnyDefinition, "/tmp", RunTrigger.Manual, WorkflowId: "verifier"));
            journal.Append("r1", new WorkflowEvent.RunFinished(RunState.Failed));
        }

        // act — le host rouvre la même base par le seul préréglage
        using var host = SqliteProjectHost.Open(project);
        var passage = host.LastRunPerWorkflow().Single(passage => passage.Workflow.Id == "verifier");

        // assert
        Assert.Equal("r1", passage.LastRun!.RunId);
        Assert.Equal(RunState.Failed, passage.LastRun!.State);
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    // --- helpers ---

    private static void Deposit(Project project, string id) =>
        File.WriteAllText(Path.Combine(project.WorkflowsDirectory, $"{id}.json"), AnyDocument);

    /// <summary>Un graphe valide : la définition figée repasse par le validateur à la relecture SQLite.</summary>
    private static WorkflowDefinition AnyDefinition => new("A", new[]
    {
        new StepDefinition("A", "A", new ScriptSpec("/usr/bin/true", []), MaxVisits: 1, []),
    });

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
