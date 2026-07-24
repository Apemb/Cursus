namespace Cursus.Core.Projects;

/// <summary>
/// Levée quand un identifiant de workflow ne peut pas devenir un nom de fichier
/// sûr — vide, ou porteur d'un séparateur de chemin qui le ferait atterrir hors
/// du dossier des workflows. Le catalogue rejette ; il ne <i>slugifie</i> pas
/// (transformer un libellé humain en identifiant est l'affaire de l'éditeur).
/// </summary>
public sealed class InvalidWorkflowIdException(string id)
    : Exception($"« {id} » n'est pas un identifiant de workflow valide.")
{
    public string Id { get; } = id;
}
