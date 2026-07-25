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
    /// Inscrit une connexion. Le registre <b>attribue l'identifiant</b> et le remet au
    /// constructeur reçu : c'est lui qui répond de l'unicité — cet identifiant désigne
    /// le jeton au trousseau, et deux connexions ne doivent jamais pouvoir se le
    /// disputer — tandis que l'appelant seul sait quel genre de connexion bâtir.
    /// </summary>
    public TrackerConnection Add(Func<string, TrackerConnection> build)
    {
        var connection = build(Guid.NewGuid().ToString("n"));
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
            if (ToConnection(connection) is { } read)
                _connections.Add(read);
    }

    // L'adaptateur : le discriminant `kind` du document choisit le sous-type, et ne
    // remonte jamais en propriété du modèle. Un kind inconnu — fichier écrit par une
    // version plus récente, ou tracker qu'on ne sait pas encore joindre — est
    // **ignoré** plutôt que dégradé : une connexion dont on ne sait pas à quoi elle
    // parle n'est pas une connexion, et en fabriquer une approximative ferait échouer
    // chaque usage sans dire pourquoi.
    private static TrackerConnection? ToConnection(ConnectionDocument connection) => connection.Kind switch
    {
        LinearKind => new LinearConnection(
            connection.Id ?? "",
            connection.Label ?? "",
            new TrackerWorkspace(
                connection.Workspace?.Id ?? "",
                connection.Workspace?.Key ?? "",
                connection.Workspace?.Name ?? "")),
        _ => null,
    };

    // L'adaptateur dans l'autre sens : chaque genre de connexion connaît sa forme de
    // document.
    private static ConnectionDocument ToDocument(TrackerConnection connection) => connection switch
    {
        LinearConnection linear => new ConnectionDocument(
            linear.Id,
            linear.Label,
            LinearKind,
            new WorkspaceDocument(linear.Workspace.Id, linear.Workspace.Key, linear.Workspace.Name)),
        _ => throw new NotSupportedException(
            $"Aucune forme de document pour {connection.GetType().Name} — un genre de connexion "
            + "s'écrit et se relit, sinon il disparaît au redémarrage."),
    };

    private const string LinearKind = "linear";

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
        string? Id, string? Label, string? Kind, WorkspaceDocument? Workspace);

    private sealed record WorkspaceDocument(string? Id, string? Key, string? Name);
}
