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
    /// Le registre de la machine, à son emplacement par défaut : <c>~/.config/cursus/</c>
    /// (ou <c>$XDG_CONFIG_HOME/cursus</c>). C'est la fabrique de composition pour les
    /// drivers (App aujourd'hui, une CLI un jour) ; les tests, eux, injectent un
    /// dossier temporaire par le constructeur.
    /// </summary>
    /// <remarks>
    /// On ne passe **pas** par <see cref="Environment.SpecialFolder.ApplicationData"/> :
    /// sur macOS il rend <c>~/Library/Application Support</c>, la convention native — qui
    /// n'est pas ce qu'on veut pour un outil de dev. On vise <c>.config</c> explicitement,
    /// en parité avec <c>build/reset-data.sh</c>.
    /// </remarks>
    public static ProjectRegistry ForCurrentUser()
        => new(ResolveConfigDirectory(
            Environment.GetEnvironmentVariable("XDG_CONFIG_HOME"),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));

    /// <summary>
    /// Le dossier de configuration machine de Cursus à partir de l'environnement :
    /// <c>$XDG_CONFIG_HOME/cursus</c> s'il est posé, sinon <c>&lt;home&gt;/.config/cursus</c>.
    /// Une valeur vide compte comme non définie — comme le fait le shell
    /// (<c>${XDG_CONFIG_HOME:-$HOME/.config}</c>), sans quoi l'app et le script de reset
    /// viseraient deux dossiers.
    /// </summary>
    public static string ResolveConfigDirectory(string? xdgConfigHome, string home)
    {
        var configHome = string.IsNullOrEmpty(xdgConfigHome)
            ? Path.Combine(home, ".config")
            : xdgConfigHome;
        return Path.Combine(configHome, "cursus");
    }

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
    /// Renomme un projet connu. Le nom vit sur disque (dans son <c>project.json</c>),
    /// pas dans le fichier du registre — qui ne liste que des racines. On réécrit
    /// donc le disque via <see cref="ProjectStore.Rename"/>, <b>et</b> on remplace
    /// l'instantané que le registre garde en mémoire : sans ce remplacement, la
    /// liste ressusciterait l'ancien nom à la prochaine relecture. Aucune écriture
    /// du registre lui-même — les racines, elles, ne bougent pas.
    /// </summary>
    public Project Rename(string projectRoot, string newName)
    {
        var root = Path.GetFullPath(projectRoot);
        var index = _projects.FindIndex(inscribed => inscribed.Root == root);

        var renamed = ProjectStore.Rename(_projects[index], newName);
        _projects[index] = renamed;
        return renamed;
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
