namespace Cursus.Core.Projects;

/// <summary>
/// Levée quand aucun projet Cursus ne se trouve à l'emplacement demandé — ni le
/// dossier désigné, ni, pour une découverte, aucun de ses ancêtres.
/// </summary>
public sealed class ProjectNotFoundException(string searchedFrom)
    : Exception($"Aucun projet Cursus trouvé à partir de : {searchedFrom}")
{
    public string SearchedFrom { get; } = searchedFrom;
}
