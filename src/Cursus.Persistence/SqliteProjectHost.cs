using Cursus.Core.Projects;
using Cursus.Core.Workflows.Execution;
using Cursus.Core.Workflows.Workspaces;

namespace Cursus.Persistence;

/// <summary>
/// Le préréglage SQLite d'un <see cref="ProjectHost"/> : le seul endroit qui
/// connaît les deux mondes. Le host vit dans <c>Cursus.Core</c> et ne reçoit
/// qu'une fabrique de journal, sans jamais apprendre que c'est du SQLite ; c'est
/// ici que cette fabrique se lie au <see cref="SqliteRunJournal"/> du projet,
/// pour que le câblage concret n'existe qu'en un exemplaire (architecture.md §7.12).
/// </summary>
public static class SqliteProjectHost
{
    /// <summary>
    /// Ouvre le host d'un projet sur sa vraie base. À l'appelant de le disposer :
    /// un projet = un host, et disposer le host ferme la connexion SQLite.
    /// </summary>
    /// <remarks>
    /// Le <b>même</b> <see cref="SqliteRunJournal"/> sert de lecteur au host et
    /// d'écrivain au lanceur : une seule connexion, donc ce qui est lancé se relit
    /// sans divergence et se ferme d'une seule disposition. ⚠️ Séquentiel en 3b
    /// (lancer <em>puis</em> lire) ; une lecture concurrente d'un lancement en cours
    /// (runs de front, 6c·3c / §7.13) exigera de revoir le partage de connexion.
    /// </remarks>
    public static ProjectHost Open(Project project)
    {
        var journal = new SqliteRunJournal(project.DatabasePath);
        var launcher = new WorkflowLauncher(
            new ProcessRunner(),
            journal,
            new RunArtifactStore(project.ArtifactsRoot),
            new GitWorkspaceProvisioner(new ProcessRunner(), project.Root, project.WorktreesRoot));
        return new ProjectHost(project, () => journal, launcher);
    }
}
