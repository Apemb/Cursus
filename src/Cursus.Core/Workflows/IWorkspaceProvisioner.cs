namespace Cursus.Core.Workflows;

/// <summary>
/// Donne à un run un répertoire de travail <b>isolé</b>, pour que plusieurs runs
/// coexistent sur un même dépôt sans que leurs écritures se marchent dessus.
/// Collaborateur de l'<b>appelant</b> (le futur host), jamais du moteur : il
/// provisionne avant <c>ExecuteAsync</c> et démonte après.
/// </summary>
public interface IWorkspaceProvisioner
{
    /// <summary>
    /// Provisionne l'espace du run <paramref name="runId"/> selon ce qu'il
    /// demande. L'emplacement dérive du <paramref name="runId"/> : c'est ce qui
    /// permet de retrouver le worktree d'un run depuis son journal.
    /// </summary>
    IProvisionedWorkspace Provision(string runId, WorkspaceRequest request);
}
