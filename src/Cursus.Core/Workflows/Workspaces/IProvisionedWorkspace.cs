using Cursus.Core.Workflows.Execution;

namespace Cursus.Core.Workflows.Workspaces;

/// <summary>
/// Le répertoire de travail isolé d'<b>un</b> run, le temps de son exécution.
/// Le refermer démonte l'isolation (pour un worktree git, <c>git worktree
/// remove</c>) — le travail qui devait survivre a déjà été commité sur sa
/// branche, qui reste dans le dépôt.
///
/// <para>
/// <b><see cref="IAsyncDisposable"/></b> et non <see cref="IDisposable"/> : le
/// démontage attend un sous-process git, donc de l'I/O — il l'<c>await</c>, il ne
/// bloque pas un thread dessus (aucun <c>sync-over-async</c> ; voir <c>D-015</c>).
/// </para>
/// </summary>
public interface IProvisionedWorkspace : IAsyncDisposable
{
    /// <summary>Le contexte à passer au moteur : sa racine est le worktree, pas le dépôt.</summary>
    RunContext Context { get; }
}
