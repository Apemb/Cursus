using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Cursus.Core.Projects;

/// <summary>
/// Traduit entre le disque et <see cref="Project"/>, dans les deux sens. Le
/// seul type du noyau qui écrive la disposition d'un projet.
/// </summary>
public static class ProjectStore
{
    internal const string DirectoryName = ".cursus";
    internal const string FileName = "project.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,

        // Un nom de projet accentué doit rester lisible dans un fichier destiné
        // à être relu dans une PR — même raison qu'au sérialiseur de workflows.
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    public static Project Create(string root, string name)
    {
        var project = new Project(Guid.NewGuid().ToString(), name, Path.GetFullPath(root));

        // Ni fusion ni écrasement : réécrire le fichier changerait l'identité du
        // projet, et le registre machine ne saurait plus qu'il s'agit du même.
        if (File.Exists(project.ProjectFilePath))
            throw new InvalidOperationException(
                $"Ce répertoire porte déjà un projet Cursus : {project.CursusDirectory}");

        Directory.CreateDirectory(project.WorkflowsDirectory);
        File.WriteAllText(
            project.ProjectFilePath,
            JsonSerializer.Serialize(new ProjectDocument(project.Id, project.Name), Options));

        // La coupe entre l'intention (versionnée) et l'observation (locale) n'est
        // un principe que si quelque chose la rend effective : sans ce fichier,
        // la base part au premier « git add . ». L'astérisque couvre les
        // compagnons du mode WAL, « -wal » et « -shm ».
        File.WriteAllText(
            Path.Combine(project.CursusDirectory, ".gitignore"),
            """
            cursus.db*
            runs/
            worktrees/

            """);

        return project;
    }

    /// <summary>
    /// Remonte l'arborescence depuis un point de départ jusqu'au premier projet
    /// rencontré, façon git. Distinct de <see cref="Open"/>, qui exige la racine
    /// exacte : ouvrir ce qu'on désigne et retrouver ce dans quoi on se trouve
    /// sont deux besoins différents.
    /// </summary>
    public static Project Discover(string startPath)
    {
        var directory = new DirectoryInfo(Path.GetFullPath(startPath));

        for (var candidate = directory; candidate is not null; candidate = candidate.Parent)
        {
            if (File.Exists(ProjectFileIn(candidate.FullName)))
                return Open(candidate.FullName);
        }

        throw new ProjectNotFoundException(directory.FullName);
    }

    public static Project Open(string root)
    {
        var full = Path.GetFullPath(root);
        var file = ProjectFileIn(full);

        if (!File.Exists(file))
            throw new ProjectNotFoundException(full);

        ProjectDocument? document;
        try
        {
            document = JsonSerializer.Deserialize<ProjectDocument>(File.ReadAllText(file), Options);
        }
        catch (JsonException failure)
        {
            throw new InvalidProjectException(file, failure.Message);
        }

        // L'identité est la seule donnée sans laquelle un projet ne peut pas
        // exister : le nom n'est qu'un libellé, son absence n'empêche rien.
        if (string.IsNullOrWhiteSpace(document?.Id))
            throw new InvalidProjectException(file, "le document ne porte pas d'identifiant de projet.");

        return new Project(document.Id, document.Name ?? "", full);
    }

    /// <summary>Le seul endroit qui sache reconnaître un projet sans en avoir déjà un.</summary>
    private static string ProjectFileIn(string root) => Path.Combine(root, DirectoryName, FileName);
}
