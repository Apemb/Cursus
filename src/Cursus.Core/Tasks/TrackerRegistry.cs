using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

using Cursus.Core.Projects;

namespace Cursus.Core.Tasks;

/// <summary>
/// Les connexions tracker connues de cette installation — jumeau de
/// <see cref="ProjectRegistry"/>, même dossier de configuration machine.
/// Un jeton dessert des projets du tracker, pas un projet Cursus : il n'y a donc
/// aucune raison de le ranger par projet.
///
/// <para>
/// ⚠️ <b>Aucun secret n'est écrit ici.</b> Ce fichier est en clair ; le jeton vit au
/// trousseau, désigné par l'identifiant de la connexion.
/// </para>
/// </summary>
public sealed class TrackerRegistry
{
    internal const string FileName = "trackers.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,

        // Un libellé accentué doit rester lisible dans le fichier — même raison qu'au
        // registre des projets.
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    private readonly string _filePath;
    private readonly List<TrackerConnection> _connections = [];

    public TrackerRegistry(string configDirectory)
    {
        _filePath = Path.Combine(configDirectory, FileName);
        Load();
    }

    /// <summary>
    /// Le registre de la machine, à son emplacement par défaut — le même que celui des
    /// projets, résolu par la même règle.
    /// </summary>
    public static TrackerRegistry ForCurrentUser()
        => new(ProjectRegistry.ResolveConfigDirectory(
            Environment.GetEnvironmentVariable("XDG_CONFIG_HOME"),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));

    public IReadOnlyList<TrackerConnection> Connections => _connections;

    /// <summary>
    /// Inscrit une connexion et lui attribue son identifiant. C'est le registre qui
    /// l'attribue, jamais l'appelant : cet identifiant désigne le jeton au trousseau,
    /// et deux connexions ne doivent jamais pouvoir se le disputer.
    /// </summary>
    public TrackerConnection Add(string label, TrackerScope scope)
    {
        var connection = new TrackerConnection(Guid.NewGuid().ToString("n"), label, scope);
        _connections.Add(connection);
        Save();
        return connection;
    }

    /// <summary>
    /// Oublie une connexion. ⚠️ Le jeton qu'elle désigne, lui, vit au trousseau :
    /// l'appelant doit l'effacer aussi, sans quoi le secret reste orphelin.
    /// </summary>
    public void Remove(string connectionId)
    {
        _connections.RemoveAll(connection => connection.Id == connectionId);
        Save();
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
            return;

        var document = JsonSerializer.Deserialize<RegistryDocument>(File.ReadAllText(_filePath), Options);
        foreach (var connection in document?.Connections ?? [])
            _connections.Add(new TrackerConnection(
                connection.Id ?? "",
                connection.Label ?? "",
                ToScope(connection)));
    }

    // L'adaptateur : le discriminant `kind` du document choisit le sous-type. Il ne
    // remonte jamais en propriété du modèle — la portée est un type, pas un champ.
    // Un kind inconnu (fichier d'une version plus récente) retombe sur « tout
    // l'espace » : montrer trop de projets se remarque et se corrige, une connexion
    // muette laisse l'utilisateur sans explication.
    private static TrackerScope ToScope(ConnectionDocument connection) => connection.Kind switch
    {
        SelectionKind => new TrackerScope.SelectedProjects(connection.Projects ?? []),
        _ => new TrackerScope.WholeWorkspace(),
    };

    // L'adaptateur dans l'autre sens : chaque portée connaît sa forme de document.
    private static ConnectionDocument ToDocument(TrackerConnection connection) => connection.Scope switch
    {
        TrackerScope.SelectedProjects selection =>
            new ConnectionDocument(connection.Id, connection.Label, SelectionKind, selection.ProjectIds),
        _ => new ConnectionDocument(connection.Id, connection.Label, WorkspaceKind, Projects: null),
    };

    private const string WorkspaceKind = "workspace";
    private const string SelectionKind = "projects";

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        File.WriteAllText(
            _filePath,
            JsonSerializer.Serialize(
                new RegistryDocument([.. _connections.Select(ToDocument)]),
                Options));
    }

    private sealed record RegistryDocument(IReadOnlyList<ConnectionDocument> Connections);

    private sealed record ConnectionDocument(
        string? Id, string? Label, string? Kind, IReadOnlyList<string>? Projects);
}
