using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Execution;
using Cursus.Core.Workflows.Serialization;

namespace Cursus.Persistence.Tests;

/// <summary>
/// L'assemblage du jalon, sans le moindre double : un workflow décrit en JSON,
/// de vrais process, un journal sur fichier — puis tout est refermé, et on
/// redemande à une instance neuve ce qui s'est passé.
/// </summary>
public class JournalledExecutionTests : IDisposable
{
    private readonly string _cursusDirectory = Directory.CreateTempSubdirectory("cursus-dot-").FullName;

    [Fact(DisplayName = "étant donné un workflow JSON exécuté avec un journal sur fichier, quand on rouvre le journal dans une instance neuve, alors la trajectoire relue est celle qui a été parcourue et les sorties des scripts se retrouvent sur disque")]
    public async Task A_run_can_be_replayed_from_a_journal_reopened_after_the_fact()
    {
        // arrange
        var workspace = new RunContext(Directory.CreateTempSubdirectory("cursus-workspace-").FullName);

        const string document = """
            {
              "entryStep": "compiler",
              "steps": [
                { "id": "compiler", "name": "Compiler", "maxVisits": 1,
                  "script": { "fileName": "/bin/sh", "arguments": ["-c", "echo compilation terminee"] },
                  "edges": [ { "guard": "success", "target": "tester" } ] },

                { "id": "tester", "name": "Tester", "maxVisits": 1,
                  "script": { "fileName": "/bin/sh", "arguments": ["-c", "echo 2 tests en echec >&2; exit 3"] },
                  "edges": [] }
              ]
            }
            """;

        var loaded = WorkflowSerializer.Read(document);

        // act — le run se déroule, puis le journal est refermé pour de bon.
        string runId;
        using (var journal = NewJournal())
        {
            var run = await new WorkflowEngine(new ProcessRunner(), journal, Artifacts())
                .ExecuteAsync(loaded.Definition!, workspace, Guid.NewGuid().ToString(), RunTrigger.ForTask("ENG-1234"));

            runId = run.RunId;
            Assert.Equal(RunState.Failed, run.State);
        }

        // assert — plus rien de ce run n'est en mémoire.
        using var reopened = NewJournal();

        var summary = Assert.Single(reopened.ListRuns());
        Assert.Equal(runId, summary.RunId);
        Assert.Equal(RunState.Failed, summary.State);

        var events = reopened.ReadEvents(runId);
        Assert.Equal(
            new[] { "RunStarted", "StepStarted", "StepFinished", "EdgeChosen", "StepStarted", "StepFinished", "RunFinished" },
            events.Select(entry => entry.Event.GetType().Name));

        // la définition figée au démarrage est relue telle qu'elle a tourné
        var started = Assert.IsType<WorkflowEvent.RunStarted>(events[0].Event);
        Assert.Equal("compiler", started.Definition.EntryStep);
        Assert.Equal(workspace.WorkspaceRoot, started.WorkspaceRoot);
        Assert.Equal("ENG-1234", started.Trigger.TaskKey);

        // le routage retenu est celui que le code de sortie commandait
        var chosen = Assert.Single(events.Select(e => e.Event).OfType<WorkflowEvent.EdgeChosen>());
        Assert.Equal("tester", chosen.ToStepId);

        // les sorties ne sont pas en base, mais elles sont bien quelque part
        var artifacts = Artifacts();
        Assert.Contains("compilation terminee", artifacts.Read(runId, "compiler", 1, ArtifactStream.StandardOutput));
        Assert.Contains("2 tests en echec", artifacts.Read(runId, "tester", 1, ArtifactStream.StandardError));
    }

    // --- helpers ---

    private SqliteRunJournal NewJournal() =>
        new(Path.Combine(_cursusDirectory, "cursus.db"));

    private RunArtifactStore Artifacts() => new(Path.Combine(_cursusDirectory, "runs"));

    public void Dispose() => Directory.Delete(_cursusDirectory, recursive: true);
}
