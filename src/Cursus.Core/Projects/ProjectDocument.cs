namespace Cursus.Core.Projects;

/// <summary>
/// La forme du fichier <c>project.json</c>, distincte du modèle pour la même
/// raison que <c>WorkflowDocument</c> : le format est versionné dans un dépôt,
/// il doit survivre aux refactors du noyau.
/// </summary>
/// <remarks>
/// La racine du workspace n'y figure pas : ce fichier se partage par git, or un
/// chemin absolu n'a de sens que sur la machine qui l'a écrit. La racine est
/// déduite — c'est le dossier qui contient le <c>.cursus/</c>.
/// </remarks>
internal sealed record ProjectDocument(string? Id, string? Name, TrackerDocument? Tracker = null);

/// <summary>
/// Le tableau de tâches que le dépôt déclare viser. Le discriminant <c>kind</c> vit
/// <b>ici seulement</b> : il choisit le sous-type de <c>TrackerBinding</c> à construire
/// et ne remonte jamais en propriété du modèle.
/// </summary>
/// <param name="WorkspaceKey">
/// Propre à Linear. Un autre tracker en apportera d'autres champs, tous optionnels au
/// niveau du document — c'est le <paramref name="Kind"/> qui dit lesquels comptent, et
/// la construction du sous-type qui les rend non-nuls.
/// </param>
internal sealed record TrackerDocument(string? Kind, string? WorkspaceKey);
