namespace Cursus.Core.Projects;

/// <summary>
/// La forme sur disque du registre machine : la liste des racines de projets
/// connus. Distincte du modèle pour la même raison que <see cref="ProjectDocument"/>.
/// </summary>
/// <remarks>
/// Ici, à l'inverse de <c>project.json</c>, les chemins <b>sont</b> absolus : ce
/// fichier ne se partage jamais par git, il décrit une installation de Cursus
/// sur une machine donnée.
/// </remarks>
internal sealed record RegistryDocument(string[]? Projects);
