using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

using Cursus.Core.Tasks;

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

        // Même motif : un projet qui ne vise aucun tableau ne doit pas porter un
        // « tracker: null » dans un document qu'on relit en revue. Ce qui manque se
        // dit par l'absence de clé, pas par une clé vide.
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
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
    /// Réécrit le nom d'un projet sans toucher à son identité : le <c>project.json</c>
    /// repart avec le nouveau libellé mais le même <see cref="Project.Id"/>, sans quoi
    /// le registre machine ne reconnaîtrait plus le même projet. Rend le
    /// <see cref="Project"/> frais — l'ancien, immuable, garde son ancien nom.
    /// </summary>
    public static Project Rename(Project project, string newName)
        => Rewrite(project, fresh => new Project(fresh.Id, newName, fresh.Root, fresh.Tracker));

    /// <summary>
    /// Déclare le tableau de tâches que ce dépôt vise. Rend le <see cref="Project"/>
    /// frais — l'ancien, immuable, garde son état d'avant.
    /// </summary>
    public static Project DeclareTracker(Project project, TrackerBinding tracker)
        => Rewrite(project, fresh => new Project(fresh.Id, fresh.Name, fresh.Root, tracker));

    /// <summary>
    /// Le seul chemin par lequel un <c>project.json</c> déjà posé se réécrit.
    ///
    /// <para>
    /// ⚠️ Il <b>relit le disque</b> avant d'appliquer le changement, et n'accorde à
    /// l'appelant que le champ qu'il vient modifier. La raison est un piège vécu : le
    /// registre machine garde un instantané des projets pris au démarrage, et renommer
    /// depuis cet instantané réécrivait le document entier — effaçant en silence toute
    /// donnée posée entre-temps. L'invariant est donc local, et non une précaution que
    /// chaque appelant devrait se rappeler : <b>un écrivain partiel relit avant
    /// d'écrire</b>.
    /// </para>
    ///
    /// <para>
    /// Il est aussi l'unique endroit qui sérialise un projet existant : un second
    /// pourrait oublier un champ, et le manque ne se verrait qu'après coup.
    /// </para>
    /// </summary>
    private static Project Rewrite(Project project, Func<Project, Project> change)
    {
        var updated = change(Open(project.Root));

        File.WriteAllText(
            updated.ProjectFilePath,
            JsonSerializer.Serialize(
                new ProjectDocument(
                    updated.Id,
                    updated.Name,
                    updated.Tracker is { } tracker ? ToDocument(tracker) : null),
                Options));

        return updated;
    }

    // L'adaptateur, dans les deux sens : le discriminant `kind` choisit le sous-type et
    // ne remonte jamais en propriété du modèle — même partage qu'au registre des
    // connexions.
    private static TrackerDocument ToDocument(TrackerBinding tracker) => tracker switch
    {
        LinearBinding linear => new TrackerDocument(LinearKind, linear.WorkspaceKey),
        _ => throw new NotSupportedException(
            $"Aucune forme de document pour {tracker.GetType().Name} — une déclaration "
            + "s'écrit et se relit, sinon elle disparaît au prochain enregistrement."),
    };

    private static TrackerBinding? ToBinding(TrackerDocument? tracker) => tracker?.Kind switch
    {
        LinearKind => new LinearBinding(tracker!.WorkspaceKey ?? ""),

        // Un genre inconnu — fichier écrit par une version plus récente, ou tracker
        // qu'on ne sait pas encore joindre — est **ignoré**, jamais dégradé : viser un
        // tableau approximatif serait pire que n'en viser aucun.
        _ => null,
    };

    private const string LinearKind = "linear";

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

        return new Project(document.Id, document.Name ?? "", full, ToBinding(document.Tracker));
    }

    /// <summary>Le seul endroit qui sache reconnaître un projet sans en avoir déjà un.</summary>
    private static string ProjectFileIn(string root) => Path.Combine(root, DirectoryName, FileName);
}
