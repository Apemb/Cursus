using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Cursus.Core.Projects;

/// <summary>
/// La racine machine, au-dessus des projets : la liste des projets connus de
/// cette installation de Cursus, indépendante de tout projet particulier. C'est
/// la première pierre du registre machine que <see cref="Project.Id"/> anticipe.
/// </summary>
public sealed class ProjectRegistry
{
    internal const string FileName = "projects.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,

        // Un chemin accentué doit rester lisible dans le fichier — même raison
        // qu'au sérialiseur de projets.
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    private readonly string _filePath;
    private readonly List<Project> _projects = [];

    public ProjectRegistry(string configDirectory)
    {
        _filePath = Path.Combine(configDirectory, FileName);
        Load();
    }

    /// <summary>
    /// Le registre de la machine, à son emplacement par défaut :
    /// <c>~/.config/cursus/</c> (résolu comme <see cref="Environment.SpecialFolder.ApplicationData"/>,
    /// soit le même chemin sur macOS et Linux). C'est la fabrique de composition
    /// pour les drivers (App aujourd'hui, une CLI un jour) ; les tests, eux,
    /// injectent un dossier temporaire par le constructeur.
    /// </summary>
    public static ProjectRegistry ForCurrentUser()
        => new(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "cursus"));

    public IReadOnlyList<Project> Projects => _projects;

    /// <summary>
    /// Inscrit un projet désigné par la racine de son workspace. La validité
    /// « c'est un projet Cursus » est l'invariant de <see cref="ProjectStore"/> :
    /// on l'ouvre, et on laisse remonter son refus.
    /// </summary>
    public void Add(string projectRoot)
    {
        var project = ProjectStore.Open(projectRoot);

        // La racine que rend ProjectStore.Open est déjà absolue et normalisée :
        // deux formes du même chemin s'y ramènent, donc la comparaison suffit à
        // ne pas inscrire deux fois le même projet.
        if (_projects.Exists(inscribed => inscribed.Root == project.Root))
            return;

        _projects.Add(project);
        Save();
    }

    /// <summary>
    /// Retire un projet de la liste. Ne touche jamais au dépôt qu'il désigne :
    /// oublier un projet et le supprimer sont deux gestes distincts. On normalise
    /// le chemin nous-mêmes plutôt que de passer par <see cref="ProjectStore"/> —
    /// un projet devenu illisible doit rester retirable.
    /// </summary>
    public void Remove(string projectRoot)
    {
        var root = Path.GetFullPath(projectRoot);
        if (_projects.RemoveAll(inscribed => inscribed.Root == root) > 0)
            Save();
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
            return;

        var document = JsonSerializer.Deserialize<RegistryDocument>(File.ReadAllText(_filePath), Options);
        foreach (var root in document?.Projects ?? [])
        {
            try
            {
                _projects.Add(ProjectStore.Open(root));
            }
            catch (ProjectNotFoundException)
            {
                // Un chemin qui ne résout plus (dossier supprimé, volume démonté)
                // est ignoré de l'affichage — mais on n'écrit rien ici, donc
                // l'entrée survit dans le fichier. Distinguer « déplacé » de
                // « supprimé » est le problème du registre machine complet ; une
                // simple relecture, elle, ne doit jamais faire perdre un projet.
            }
        }
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        var document = new RegistryDocument(_projects.ConvertAll(project => project.Root).ToArray());
        File.WriteAllText(_filePath, JsonSerializer.Serialize(document, Options));
    }
}
