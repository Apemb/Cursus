using Cursus.Core.Projects;
using Cursus.Core.Workflows;

namespace Cursus.Persistence.Tests;

/// <summary>
/// L'assemblage du jalon 5 : un projet sur disque, un workflow lu depuis son
/// dossier, un run journalisé aux emplacements que le projet désigne. Rien n'y
/// est composé à la main — c'est la différence exacte avec
/// <c>JournalledExecutionTests</c>, qui reste le témoin du jalon 4.
/// </summary>
public class ProjectRunTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("cursus-projet-").FullName;

    [Fact(DisplayName = "étant donné un projet créé sur disque et un workflow déposé dans son dossier, quand on le charge et qu'on l'exécute avec un journal placé aux emplacements du projet, alors le run se relit depuis le journal du projet et ses sorties sont sous la racine d'artefacts du projet")]
    public async Task A_project_carries_everything_a_run_needs()
    {
        // arrange — le projet, puis un workflow déposé comme le ferait un humain
        var project = ProjectStore.Create(_root, "Démo");
        File.WriteAllText(
            Path.Combine(project.WorkflowsDirectory, "recenser.json"),
            """
            {
              "entryStep": "recenser",
              "steps": [
                { "id": "recenser", "name": "Recenser le workspace", "maxVisits": 1,
                  "script": { "fileName": "/bin/sh", "arguments": ["-c", "ls -a > inventaire.txt; echo recensement termine"] },
                  "edges": [ { "guard": "success", "target": "confirmer" } ] },

                { "id": "confirmer", "name": "Confirmer", "maxVisits": 1,
                  "script": { "fileName": "/bin/sh", "arguments": ["-c", "test -f inventaire.txt"] },
                  "edges": [] }
              ]
            }
            """);

        var loaded = new WorkflowCatalog(project).Load("recenser");

        // act — le run se déroule, puis tout ce qui le portait est refermé
        string runId;
        using (var journal = JournalOf(project))
        {
            var run = await new WorkflowEngine(new ProcessRunner(), journal)
                .ExecuteAsync(loaded.Definition!, project.CreateRunContext());

            runId = run.RunId;
            Assert.Equal(RunState.Completed, run.State);
        }

        // assert — le workspace du run était bien la racine du projet
        Assert.True(File.Exists(Path.Combine(project.Root, "inventaire.txt")));

        // le journal du projet se relit dans une instance neuve
        using var reopened = JournalOf(project);

        var summary = Assert.Single(reopened.ListRuns());
        Assert.Equal(runId, summary.RunId);
        Assert.Equal(RunState.Completed, summary.State);

        var chosen = Assert.Single(reopened.ReadEvents(runId).Select(e => e.Event).OfType<WorkflowEvent.EdgeChosen>());
        Assert.Equal("confirmer", chosen.ToStepId);

        // et les sorties sont sous la racine d'artefacts que le projet désigne
        Assert.Contains(
            "recensement termine",
            new RunArtifactStore(project.ArtifactsRoot)
                .Read(runId, "recenser", 1, ArtifactStream.StandardOutput));
    }

    /// <summary>Le journal d'un projet : ses deux emplacements viennent du projet, jamais du test.</summary>
    private static SqliteRunJournal JournalOf(Project project) =>
        new(project.DatabasePath, new RunArtifactStore(project.ArtifactsRoot));

    public void Dispose() => Directory.Delete(_root, recursive: true);
}
