using System.Text;
using Cursus.Core.Workflows;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// Test d'assemblage : le moteur de traversée sur le vrai <see cref="ProcessRunner"/>.
/// Le noyau déterministe exécute ici de vrais process, sans aucun double. La
/// sortie ne transite plus par le résultat : elle ruisselle dans un puits, ici
/// en mémoire, d'où on la relit.
/// </summary>
public class WorkflowExecutionTests
{
    [Fact(DisplayName = "étant donné un graphe de vrais scripts dont l'un échoue, quand on exécute le workflow, alors il emprunte l'arête d'échec, va jusqu'au bout et sa sortie se relit depuis le puits")]
    public async Task A_real_graph_of_scripts_runs_end_to_end_through_its_failure_edge()
    {
        // arrange — quatre scripts partageant un répertoire de travail
        var workspace = new RunContext(Directory.CreateTempSubdirectory("cursus-workflow-").FullName);
        var output = new InMemoryRunOutputStore();
        var definition = new WorkflowDefinition("preparer", new[]
        {
            Step("preparer", "echo bonjour > artefact.txt", new Edge(Guard.OnSuccess, "verifier")),
            Step("verifier", "grep -q bonjour artefact.txt", new Edge(Guard.OnSuccess, "tester")),
            Step("tester", "echo 2 tests en echec >&2; exit 1", new Edge(Guard.OnFailure, "rapporter")),
            Step("rapporter", "echo rapport ecrit"),
        });

        // act
        var run = await new WorkflowEngine(new ProcessRunner(), new InMemoryRunJournal(), output)
            .ExecuteAsync(definition, workspace, Guid.NewGuid().ToString());

        // assert
        Assert.Equal(new[] { "preparer", "verifier", "tester", "rapporter" }, run.History.Select(s => s.StepId));
        Assert.Equal(RunState.Completed, run.State);
        Assert.Contains("2 tests en echec", Captured(output, run.RunId, "tester", "stderr"));
        Assert.Contains("rapport ecrit", Captured(output, run.RunId, "rapporter", "stdout"));
        Assert.True(File.Exists(Path.Combine(workspace.WorkspaceRoot, "artefact.txt")));

        // le StepRun porte les tailles que le puits a rangées
        var reported = run.History[3].Output.Artifacts.Single(a => a.Name == "stdout").Size;
        Assert.Equal(output.Captured(run.RunId, "rapporter", 1, "stdout").Length, reported);

        // --- helpers locaux ---
        static StepDefinition Step(string id, string script, params Edge[] edges) =>
            new(id, id, new ScriptSpec("/bin/sh", ["-c", script]), MaxVisits: 1, edges);
    }

    [Fact(DisplayName = "étant donné un workflow décrit en JSON et un workspace, quand on le charge puis qu'on l'exécute, alors la trajectoire est parcourue et les fichiers atterrissent dans les bons sous-répertoires")]
    public async Task A_workflow_declared_in_a_document_runs_end_to_end()
    {
        // arrange — plus une seule ligne de C# ne déclare le graphe.
        var workspace = new RunContext(Directory.CreateTempSubdirectory("cursus-json-").FullName);
        var output = new InMemoryRunOutputStore();
        Directory.CreateDirectory(Path.Combine(workspace.WorkspaceRoot, "backend"));

        const string document = """
            {
              "entryStep": "preparer",
              "steps": [
                { "id": "preparer", "name": "Préparer", "maxVisits": 1,
                  "workingSubdirectory": "backend",
                  "script": { "fileName": "/bin/sh", "arguments": ["-c", "echo bonjour > artefact.txt"] },
                  "edges": [ { "guard": "success", "target": "tester" } ] },

                { "id": "tester", "name": "Tester", "maxVisits": 1,
                  "workingSubdirectory": "backend",
                  "script": { "fileName": "/bin/sh", "arguments": ["-c", "echo 2 tests en echec >&2; exit 3"] },
                  "edges": [ { "guard": "exit:3", "target": "rapporter" } ] },

                { "id": "rapporter", "name": "Rapporter", "maxVisits": 1,
                  "script": { "fileName": "/bin/sh", "arguments": ["-c", "echo rapport ecrit > rapport.txt"] },
                  "edges": [] }
              ]
            }
            """;

        // act
        var loaded = WorkflowSerializer.Read(document);
        var run = await new WorkflowEngine(new ProcessRunner(), new InMemoryRunJournal(), output)
            .ExecuteAsync(loaded.Definition!, workspace, Guid.NewGuid().ToString());

        // assert
        Assert.True(loaded.Report.IsValid);
        Assert.Equal(new[] { "preparer", "tester", "rapporter" }, run.History.Select(s => s.StepId));
        Assert.Equal(RunState.Completed, run.State);
        Assert.Contains("2 tests en echec", Captured(output, run.RunId, "tester", "stderr"));

        // le sous-chemin déclaré est bien celui où le script a travaillé
        Assert.True(File.Exists(Path.Combine(workspace.WorkspaceRoot, "backend", "artefact.txt")));
        Assert.True(File.Exists(Path.Combine(workspace.WorkspaceRoot, "rapport.txt")));
    }

    private static string Captured(InMemoryRunOutputStore store, string runId, string stepId, string name) =>
        Encoding.UTF8.GetString(store.Captured(runId, stepId, 1, name));
}
