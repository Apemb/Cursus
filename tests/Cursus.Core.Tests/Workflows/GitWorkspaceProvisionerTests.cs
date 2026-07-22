using System.Diagnostics;

using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Execution;
using Cursus.Core.Workflows.Workspaces;

using static Cursus.Core.Tests.Workflows.WorkflowFixtures;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// Le provisionnement d'un répertoire de travail isolé par run — un worktree
/// git. Adossé à un vrai dépôt de test : ces cas ne se jouent pas sur un double,
/// c'est git lui-même qui doit se comporter comme on l'attend.
/// </summary>
public sealed class GitWorkspaceProvisionerTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("cursus-provision-").FullName;

    private string RepositoryRoot => Path.Combine(_root, "repo");
    private string WorktreesRoot => Path.Combine(_root, "worktrees");

    public GitWorkspaceProvisionerTests()
    {
        // un dépôt minimal avec un commit initial : de quoi accrocher un worktree
        Directory.CreateDirectory(RepositoryRoot);
        Git("init");
        Git("config", "user.email", "test@cursus.dev");
        Git("config", "user.name", "Cursus Test");
        File.WriteAllText(Path.Combine(RepositoryRoot, "README.md"), "depot de test\n");
        Git("add", "-A");
        Git("commit", "-m", "commit initial");
    }

    [Fact(DisplayName = "étant donné un dépôt git, quand on provisionne un workspace de nouveau travail sur une base, alors un worktree isolé existe à l'emplacement du run, sur un HEAD détaché à cette base")]
    public void Provisioning_new_work_creates_a_detached_worktree_at_the_run_location()
    {
        // arrange
        var provisioner = NewProvisioner();

        // act
        using var workspace = provisioner.Provision("run-1", new WorkspaceRequest.NewWork("HEAD"));

        // assert
        var expected = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(Path.Combine(WorktreesRoot, "run-1")));
        Assert.Equal(expected, workspace.Context.WorkspaceRoot);
        // le checkout a bien eu lieu — le fichier du commit de base est là
        Assert.True(File.Exists(Path.Combine(workspace.Context.WorkspaceRoot, "README.md")));
        // HEAD détaché : abbrev-ref rend « HEAD » quand aucune branche n'est active
        Assert.Equal("HEAD", GitAt(workspace.Context.WorkspaceRoot, "rev-parse", "--abbrev-ref", "HEAD"));
    }

    [Fact(DisplayName = "étant donné un dépôt git avec une branche existante, quand on provisionne un workspace de review sur cette branche, alors le worktree la checkout")]
    public void Provisioning_a_review_checks_out_the_existing_branch()
    {
        // arrange — une branche à relire, dérivée du commit initial
        Git("branch", "feature-x");
        var provisioner = NewProvisioner();

        // act
        using var workspace = provisioner.Provision("run-1", new WorkspaceRequest.Review("feature-x"));

        // assert
        Assert.Equal("feature-x", GitAt(workspace.Context.WorkspaceRoot, "rev-parse", "--abbrev-ref", "HEAD"));
    }

    [Fact(DisplayName = "étant donné un workspace provisionné, quand on le referme, alors le worktree est retiré et son répertoire disparaît")]
    public void Closing_a_workspace_removes_the_worktree()
    {
        // arrange — un worktree où le run a laissé un fichier non commité
        var provisioner = NewProvisioner();
        string path;
        using (var workspace = provisioner.Provision("run-1", new WorkspaceRequest.NewWork("HEAD")))
        {
            path = workspace.Context.WorkspaceRoot;
            File.WriteAllText(Path.Combine(path, "brouillon.txt"), "travail en cours");
        }

        // assert — le répertoire a disparu, et git ne connaît plus ce worktree
        Assert.False(Directory.Exists(path));
        Assert.DoesNotContain("run-1", Git("worktree", "list"));
    }

    [Fact(DisplayName = "étant donné que git est absent du PATH, quand on provisionne, alors l'échec est explicite plutôt qu'une erreur de process brute")]
    public void Provisioning_without_git_fails_explicitly()
    {
        // arrange — un runner qui rend un lancement impossible, comme quand git manque
        var provisioner = new GitWorkspaceProvisioner(
            new StubProcessRunner(new ScriptResult(127, ScriptOutcome.LaunchFailed)),
            RepositoryRoot,
            WorktreesRoot);

        // act / assert
        Assert.Throws<GitNotAvailableException>(
            () => provisioner.Provision("run-1", new WorkspaceRequest.NewWork("HEAD")));
    }

    [Fact(DisplayName = "étant donné deux workspaces provisionnés sur un même dépôt, quand chacun écrit dans le sien, alors leurs répertoires sont distincts et leurs fichiers ne collisionnent pas")]
    public void Two_workspaces_are_isolated_from_each_other()
    {
        // arrange
        var provisioner = NewProvisioner();
        using var a = provisioner.Provision("run-a", new WorkspaceRequest.NewWork("HEAD"));
        using var b = provisioner.Provision("run-b", new WorkspaceRequest.NewWork("HEAD"));

        // act — chacun écrit sous le même nom, dans son propre worktree
        File.WriteAllText(Path.Combine(a.Context.WorkspaceRoot, "travail.txt"), "A");
        File.WriteAllText(Path.Combine(b.Context.WorkspaceRoot, "travail.txt"), "B");

        // assert
        Assert.NotEqual(a.Context.WorkspaceRoot, b.Context.WorkspaceRoot);
        Assert.Equal("A", File.ReadAllText(Path.Combine(a.Context.WorkspaceRoot, "travail.txt")));
        Assert.Equal("B", File.ReadAllText(Path.Combine(b.Context.WorkspaceRoot, "travail.txt")));
    }

    [Fact(DisplayName = "étant donné deux runs de nouveau travail, quand chacun crée sa branche nommée dans son worktree, alors les deux branches coexistent")]
    public void Two_new_work_runs_create_coexisting_branches()
    {
        // arrange
        var provisioner = NewProvisioner();
        using var a = provisioner.Provision("run-a", new WorkspaceRequest.NewWork("HEAD"));
        using var b = provisioner.Provision("run-b", new WorkspaceRequest.NewWork("HEAD"));

        // act — chaque run baptise sa branche une fois son nom connu ; le HEAD
        // détaché le permet sans le refus « branch already checked out »
        GitAt(a.Context.WorkspaceRoot, "checkout", "-b", "task/ENG-1");
        GitAt(b.Context.WorkspaceRoot, "checkout", "-b", "task/ENG-2");

        // assert
        var branches = Git("branch", "--format=%(refname:short)");
        Assert.Contains("task/ENG-1", branches);
        Assert.Contains("task/ENG-2", branches);
    }

    // --- helpers ---

    private GitWorkspaceProvisioner NewProvisioner() =>
        new(new ProcessRunner(), RepositoryRoot, WorktreesRoot);

    private string Git(params string[] arguments) => Run(RepositoryRoot, arguments);

    private static string GitAt(string workingDirectory, params string[] arguments) =>
        Run(workingDirectory, arguments);

    /// <summary>git piloté en direct pour le montage du décor — hors production, l'invariant 3 ne s'y applique pas.</summary>
    private static string Run(string workingDirectory, string[] arguments)
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
