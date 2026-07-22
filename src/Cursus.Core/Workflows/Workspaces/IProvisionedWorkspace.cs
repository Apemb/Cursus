using Cursus.Core.Workflows.Execution;

namespace Cursus.Core.Workflows.Workspaces;

/// <summary>
/// Le répertoire de travail isolé d'<b>un</b> run, le temps de son exécution.
/// Le refermer démonte l'isolation (pour un worktree git, <c>git worktree
/// remove</c>) — le travail qui devait survivre a déjà été commité sur sa
/// branche, qui reste dans le dépôt.
/// </summary>
public interface IProvisionedWorkspace : IDisposable
{
    /// <summary>Le contexte à passer au moteur : sa racine est le worktree, pas le dépôt.</summary>
    RunContext Context { get; }
}
