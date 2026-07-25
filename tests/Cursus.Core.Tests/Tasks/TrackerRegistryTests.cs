using Cursus.Core.Tasks;

namespace Cursus.Core.Tests.Tasks;

/// <summary>
/// Le registre des connexions tracker : quels jetons cette machine connaît, et ce que
/// chacun dessert. Jumeau du registre des projets — même dossier de configuration,
/// même forme —, à une différence près qui commande tout : <b>le secret n'est pas
/// ici</b>. Le registre ne porte que ce qui peut s'écrire en clair ; le jeton vit au
/// trousseau, sous une clé dérivée de l'identifiant de connexion.
///
/// <para>
/// L'identifiant est attribué <b>par le registre</b> : c'est lui qui répond de
/// l'unicité, et l'appelant ne fait que dire quel genre de connexion construire avec.
/// </para>
/// </summary>
public sealed class TrackerRegistryTests : IDisposable
{
    private readonly string _configDir = Directory.CreateTempSubdirectory("cursus-trackers-").FullName;

    private static readonly TrackerWorkspace Cursus = new("ws-cursus", "cursus-app", "Cursus");

    [Fact(DisplayName = "étant donné un registre vide, quand on ajoute une connexion, alors elle figure dans la liste avec un identifiant attribué")]
    public void Adding_a_connection_lists_it_with_an_identifier()
    {
        // arrange
        var registry = new TrackerRegistry(_configDir);

        // act
        var connection = registry.Add(id => new LinearConnection(id, "Mon compte Linear", Cursus));

        // assert — l'identifiant est attribué par le registre, pas fourni : c'est lui
        // qui devra désigner le jeton au trousseau
        Assert.NotEmpty(connection.Id);
        Assert.Equal("Mon compte Linear", Assert.Single(registry.Connections).Label);
    }

    [Fact(DisplayName = "étant donné deux connexions de même libellé, quand on les inscrit, alors leurs identifiants diffèrent")]
    public void Two_connections_never_share_an_identifier()
    {
        // arrange — deux clés du même espace : rien ne l'interdit, et le lien
        // connexion ↔ projet n'a aucune raison d'être un pour un chez tous les trackers
        var registry = new TrackerRegistry(_configDir);

        // act
        var first = registry.Add(id => new LinearConnection(id, "Linear", Cursus));
        var second = registry.Add(id => new LinearConnection(id, "Linear", Cursus));

        // assert — l'identifiant désigne le jeton au trousseau : le partager ferait
        // s'écraser un secret par l'autre, en silence
        Assert.NotEqual(first.Id, second.Id);
        Assert.NotEqual(first.SecretKey, second.SecretKey);
        Assert.Equal(2, registry.Connections.Count);
    }

    [Fact(DisplayName = "étant donné une connexion Linear inscrite, quand un registre neuf la relit, alors elle reste une connexion Linear et garde son espace")]
    public void A_linear_connection_survives_as_a_linear_connection()
    {
        // arrange
        new TrackerRegistry(_configDir).Add(id => new LinearConnection(id, "Mon compte Linear", Cursus));

        // act — l'app redémarre : un autre registre, le même dossier
        var reopened = new TrackerRegistry(_configDir);

        // assert — relire une connexion sans son genre la rendrait inutilisable : c'est
        // le type concret qui dit à quel tracker parler et ce qu'il faut afficher
        var connection = Assert.IsType<LinearConnection>(Assert.Single(reopened.Connections));
        Assert.Equal("Mon compte Linear", connection.Label);
        Assert.Equal(Cursus, connection.Workspace);
    }

    [Fact(DisplayName = "étant donné deux connexions inscrites, quand on en retire une, alors elle quitte la liste et le fichier")]
    public void Removing_a_connection_forgets_it_for_good()
    {
        // arrange
        var registry = new TrackerRegistry(_configDir);
        var doomed = registry.Add(id => new LinearConnection(id, "Clé jetable", Cursus));
        registry.Add(id => new LinearConnection(id, "Clé gardée", Cursus));

        // act
        registry.Remove(doomed.Id);

        // assert — l'oubli doit survivre au redémarrage, sinon la connexion revient
        Assert.Equal("Clé gardée", Assert.Single(registry.Connections).Label);
        Assert.Equal("Clé gardée", Assert.Single(new TrackerRegistry(_configDir).Connections).Label);
    }

    public void Dispose() => Directory.Delete(_configDir, recursive: true);
}
