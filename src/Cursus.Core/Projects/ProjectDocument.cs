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
internal sealed record ProjectDocument(string? Id, string? Name);
