using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Serialization;

namespace Cursus.Core.Projects;

/// <summary>
/// Les workflows que porte un projet. Ne traduit rien lui-même : il apporte le
/// disque et l'identité, et délègue la lecture du document au sérialiseur.
/// </summary>
public sealed class WorkflowCatalog(Project project)
{
    /// <summary>
    /// Énumère sans rien ouvrir : un document cassé se découvre au chargement,
    /// il ne doit pas rendre le projet entier inutilisable.
    /// </summary>
    public IReadOnlyList<WorkflowEntry> List() =>
        Directory.EnumerateFiles(project.WorkflowsDirectory, "*.json")
                 .Select(path => new WorkflowEntry(Path.GetFileNameWithoutExtension(path), path))
                 // Tri explicite : l'ordre d'énumération du système de fichiers
                 // n'est garanti nulle part, et une liste qui se réordonne toute
                 // seule d'un affichage à l'autre est incompréhensible.
                 .OrderBy(entry => entry.Id, StringComparer.Ordinal)
                 .ToList();

    /// <summary>
    /// Lit le document et le confie au sérialiseur. Un identifiant qu'aucun
    /// fichier ne porte lève le <see cref="FileNotFoundException"/> du
    /// framework : l'invariant violé est celui du système de fichiers, pas celui
    /// du catalogue.
    /// </summary>
    public LoadResult Load(string id) =>
        WorkflowSerializer.Read(File.ReadAllText(PathOf(id)));

    private string PathOf(string id) => Path.Combine(project.WorkflowsDirectory, $"{id}.json");
}

/// <summary>Un workflow présent dans le projet, désigné par son fichier.</summary>
public sealed record WorkflowEntry(string Id, string Path);
