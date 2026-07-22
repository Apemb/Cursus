namespace Cursus.Core.Workflows;

/// <summary>
/// Provisionne l'espace isolé d'un run par un <b>worktree git</b> : partage
/// l'object store du dépôt (bon marché), donne à chaque run son checkout, et
/// rend le travail lisible depuis le dépôt principal. Vit dans le noyau à côté
/// de <see cref="ProcessRunner"/> — il lance git <b>via</b> <see cref="IProcessRunner"/>,
/// jamais un <c>Process.Start</c> direct (invariant 3).
/// </summary>
public sealed class GitWorkspaceProvisioner : IWorkspaceProvisioner
{
    private readonly IProcessRunner _runner;
    private readonly string _repositoryRoot;
    private readonly string _worktreesRoot;

    public GitWorkspaceProvisioner(IProcessRunner runner, string repositoryRoot, string worktreesRoot)
    {
        _runner = runner;
        _repositoryRoot = repositoryRoot;
        _worktreesRoot = worktreesRoot;
    }

    public IProvisionedWorkspace Provision(string runId, WorkspaceRequest request)
    {
        var path = Path.Combine(_worktreesRoot, runId);
        Directory.CreateDirectory(_worktreesRoot);

        // Nouveau travail : HEAD détaché sur la base, la branche viendra après.
        // Review : checkout de la ref telle quelle. Le nom n'est jamais forgé ici.
        string[] arguments = request switch
        {
            WorkspaceRequest.NewWork newWork => ["worktree", "add", "--detach", path, newWork.BaseRef],
            WorkspaceRequest.Review review => ["worktree", "add", path, review.Reference],
            _ => throw new ArgumentOutOfRangeException(nameof(request)),
        };

        var result = RunGit(arguments);
        if (result.Outcome == ScriptOutcome.LaunchFailed)
            throw new GitNotAvailableException();

        return new GitProvisionedWorkspace(this, path);
    }

    private ScriptResult RunGit(params string[] arguments)
    {
        var spec = new ScriptSpec("git", arguments, _repositoryRoot);
        using var stdout = new MemoryStream();
        using var stderr = new MemoryStream();

        // Provisionnement synchrone : git worktree est bref, et c'est du montage,
        // pas un chemin chaud. Le démontage suit la même voie.
        return _runner.RunAsync(spec, stdout, stderr).GetAwaiter().GetResult();
    }

    /// <summary>Le worktree d'un run : porte son <see cref="RunContext"/>, se démonte à la fermeture.</summary>
    private sealed class GitProvisionedWorkspace : IProvisionedWorkspace
    {
        private readonly GitWorkspaceProvisioner _provisioner;
        private readonly string _path;
        private bool _removed;

        public GitProvisionedWorkspace(GitWorkspaceProvisioner provisioner, string path)
        {
            _provisioner = provisioner;
            _path = path;
            Context = new RunContext(path);
        }

        public RunContext Context { get; }

        public void Dispose()
        {
            if (_removed)
                return;

            // --force : le run laisse des fichiers non suivis ou modifiés, et git
            // refuserait sinon de retirer un worktree « sale ». Ce qui devait
            // survivre est déjà commité sur sa branche, qui reste dans le dépôt.
            _provisioner.RunGit("worktree", "remove", "--force", _path);
            _removed = true;
        }
    }
}
