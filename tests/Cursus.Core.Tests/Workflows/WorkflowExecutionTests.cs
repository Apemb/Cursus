using Cursus.Core.Workflows;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// Test d'assemblage : le moteur de traversée sur le vrai <see cref="ProcessRunner"/>.
/// Le noyau déterministe exécute ici de vrais process, sans aucun double.
/// </summary>
public class WorkflowExecutionTests
{
    [Fact(DisplayName = "étant donné un graphe de vrais scripts dont l'un échoue, quand on exécute le workflow, alors il emprunte l'arête d'échec, va jusqu'au bout et rapporte les sorties")]
    public async Task A_real_graph_of_scripts_runs_end_to_end_through_its_failure_edge()
    {
        // arrange — quatre scripts partageant un répertoire de travail
        var workspace = Directory.CreateTempSubdirectory("cursus-workflow-").FullName;
        var definition = new WorkflowDefinition("preparer", new[]
        {
            Step("preparer", "echo bonjour > artefact.txt", new Edge(Guard.OnSuccess, "verifier")),
            Step("verifier", "grep -q bonjour artefact.txt", new Edge(Guard.OnSuccess, "tester")),
            Step("tester", "echo 2 tests en echec >&2; exit 1", new Edge(Guard.OnFailure, "rapporter")),
            Step("rapporter", "echo rapport ecrit"),
        });

        // act
        var run = await new WorkflowEngine(new ProcessRunner()).ExecuteAsync(definition);

        // assert
        Assert.Equal(new[] { "preparer", "verifier", "tester", "rapporter" }, run.History.Select(s => s.StepId));
        Assert.Equal(RunState.Completed, run.State);
        Assert.Contains("2 tests en echec", run.History[2].Result.Stderr);
        Assert.Contains("rapport ecrit", run.History[3].Result.Stdout);
        Assert.True(File.Exists(Path.Combine(workspace, "artefact.txt")));

        // --- helpers locaux ---
        StepDefinition Step(string id, string script, params Edge[] edges) =>
            new(id, id, new ScriptSpec("/bin/sh", ["-c", script], workspace), MaxVisits: 1, edges);
    }
}
