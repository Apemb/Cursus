using System.Linq;

using Cursus.Core.Tasks;

namespace Cursus.App.ViewModels;

/// <summary>
/// Une connexion enregistrée, telle qu'elle se lit dans la liste : son nom, et ce
/// qu'elle dessert. ⚠️ Jamais son jeton — une connexion configurée ne réaffiche pas
/// son secret, sous aucun prétexte.
/// </summary>
public sealed class TrackerConnectionRow(TrackerConnection connection)
{
    public TrackerConnection Connection { get; } = connection;

    public string Label => Connection.Label;

    /// <summary>La portée en clair, pour que l'utilisateur voie ce qu'il a coché.</summary>
    public string ScopeLabel => Connection.Scope switch
    {
        TrackerScope.SelectedProjects selection => selection.ProjectIds.Count == 1
            ? "1 projet"
            : $"{selection.ProjectIds.Count} projets",
        _ => "tout l'espace",
    };
}
