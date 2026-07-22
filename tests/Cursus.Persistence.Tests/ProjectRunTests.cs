using System.Diagnostics;

using Cursus.Core.Projects;
using Cursus.Core.Workflows;

namespace Cursus.Persistence.Tests;

/// <summary>
/// L'assemblage du jalon 6b, sans le moindre double : un projet qui est un dépôt
/// git, deux tâches, deux runs lancés <b>de front</b>, chacun provisionné dans
/// son propre worktree. C'est la preuve que la cible — plusieurs workflows en
/// même temps sur un même projet — tient : ni le journal partagé ni les fichiers
/// de travail ne se corrompent.
/// </summary>
public sealed class ProjectRunTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("cursus-projet-").FullName;

    [Fact(DisplayName = "étant donné deux tâches sur un même projet, quand on lance leurs deux runs en concurrence chacun dans son worktree, alors les deux se relisent dans le journal du projet, leurs branches coexistent, et aucun fichier de travail n'a collisionné")]
    public async Task Two_runs_execute_concurrently_each_in_its_own_worktree()
    {
        // arrange — un projet, un workflow déposé, puis le tout érigé en dépôt git
        var project = ProjectStore.Create(_root, "Concurrent");
        File.WriteAllText(
            Path.Combine(project.WorkflowsDirectory, "travailler.json"),
            """
            {
              "entryStep": "travailler",
              "steps": [
                { "id": "travailler", "name": "Travailler", "maxVisits": 1,
                  "script": { "fileName": "/bin/sh", "arguments": ["-c", "echo done > resultat.txt"] },
                  "edges": [] }
              ]
            }
            """);
        InitRepository();

        var loaded = new WorkflowCatalog(project).Load("travailler");
        var provisioner = new GitWorkspaceProvisioner(new ProcessRunner(), project.Root, project.WorktreesRoot);

        // le provisionnement est du montage (séquentiel) ; l'appelant, qui connaît
        // la tâche, baptise chaque branche — le worktree détaché le permet.
        using var workspaceA = provisioner.Provision("run-a", new WorkspaceRequest.NewWork("HEAD"));
        using var workspaceB = provisioner.Provision("run-b", new WorkspaceRequest.NewWork("HEAD"));
        Git(workspaceA.Context.WorkspaceRoot, "checkout", "-b", "task/ENG-1");
        Git(workspaceB.Context.WorkspaceRoot, "checkout", "-b", "task/ENG-2");

        // act — les deux runs tournent de front, journalisant dans la même base
        using (var journal = JournalOf(project))
        {
            var artifacts = new RunArtifactStore(project.ArtifactsRoot);
            var engine = new WorkflowEngine(new ProcessRunner(), journal, artifacts);

            await Task.WhenAll(
                engine.ExecuteAsync(loaded.Definition!, workspaceA.Context, "run-a", RunTrigger.ForTask("ENG-1")),
                engine.ExecuteAsync(loaded.Definition!, workspaceB.Context, "run-b", RunTrigger.ForTask("ENG-2")));
        }

        // assert — les deux runs se relisent dans le journal du projet, tous deux terminés
        using var reopened = JournalOf(project);
        var runs = reopened.ListRuns().ToDictionary(run => run.RunId);
        Assert.Equal(RunState.Completed, runs["run-a"].State);
        Assert.Equal(RunState.Completed, runs["run-b"].State);
        Assert.Equal(4, reopened.ReadEvents("run-a").Count); // start, step start, step finish, run finish
        Assert.Equal(4, reopened.ReadEvents("run-b").Count);

        // les branches créées par chaque run coexistent dans le dépôt
        var branches = Git(project.Root, "branch", "--format=%(refname:short)");
        Assert.Contains("task/ENG-1", branches);
        Assert.Contains("task/ENG-2", branches);

        // aucune collision : chaque run a son fichier, dans son worktree, et le
        // dépôt principal n'a rien vu passer
        Assert.True(File.Exists(Path.Combine(workspaceA.Context.WorkspaceRoot, "resultat.txt")));
        Assert.True(File.Exists(Path.Combine(workspaceB.Context.WorkspaceRoot, "resultat.txt")));
        Assert.False(File.Exists(Path.Combine(project.Root, "resultat.txt")));
    }

    // --- helpers ---

    /// <summary>Le journal d'un projet : ses emplacements viennent du projet, jamais du test.</summary>
    private static SqliteRunJournal JournalOf(Project project) => new(project.DatabasePath);

    /// <summary>Érige la racine du projet en dépôt git avec un commit initial — la base des worktrees.</summary>
    private void InitRepository()
    {
        Git(_root, "init");
        Git(_root, "config", "user.email", "test@cursus.dev");
        Git(_root, "config", "user.name", "Cursus Test");
        Git(_root, "add", "-A");
        Git(_root, "commit", "-m", "commit initial");
    }

    /// <summary>git piloté en direct pour le décor — hors production, l'invariant 3 ne s'y applique pas.</summary>
    private static string Git(string workingDirectory, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo)!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"git {string.Join(' ', arguments)} a échoué ({process.ExitCode}) : {stderr}");

        return stdout.Trim();
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);
}
