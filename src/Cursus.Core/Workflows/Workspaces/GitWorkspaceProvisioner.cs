using Cursus.Core.Workflows.Execution;

namespace Cursus.Core.Workflows.Workspaces;

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

    public async Task<IProvisionedWorkspace> ProvisionAsync(
        string runId, WorkspaceRequest request, CancellationToken cancellationToken = default)
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

        var result = await RunGitAsync(arguments, cancellationToken).ConfigureAwait(false);
        if (result.Outcome == ScriptOutcome.LaunchFailed)
            throw new GitNotAvailableException();

        return new GitProvisionedWorkspace(this, path);
    }

    private async Task<ScriptResult> RunGitAsync(string[] arguments, CancellationToken cancellationToken)
    {
        var spec = new ScriptSpec("git", arguments, _repositoryRoot);
        using var stdout = new MemoryStream();
        using var stderr = new MemoryStream();

        // git worktree est bref, mais c'est de l'I/O : on l'attend sans détenir le
        // thread appelant (aucun sync-over-async, D-015). ConfigureAwait(false) : le
        // montage n'a aucune raison de revenir sur le contexte de l'appelant (l'UI).
        return await _runner.RunAsync(spec, stdout, stderr, cancellationToken).ConfigureAwait(false);
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

        public async ValueTask DisposeAsync()
        {
            if (_removed)
                return;

            // --force : le run laisse des fichiers non suivis ou modifiés, et git
            // refuserait sinon de retirer un worktree « sale ». Ce qui devait
            // survivre est déjà commité sur sa branche, qui reste dans le dépôt.
            // Le démontage aussi s'attend, il ne bloque pas (D-015).
            _removed = true;
            await _provisioner.RunGitAsync(["worktree", "remove", "--force", _path], CancellationToken.None)
                              .ConfigureAwait(false);
        }
    }
}
