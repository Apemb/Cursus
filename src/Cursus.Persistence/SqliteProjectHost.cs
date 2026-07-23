using Cursus.Core.Projects;

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
    public static ProjectHost Open(Project project) =>
        new(project, () => new SqliteRunJournal(project.DatabasePath));
}
