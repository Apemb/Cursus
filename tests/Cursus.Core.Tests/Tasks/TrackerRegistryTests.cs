using Cursus.Core.Tasks;

namespace Cursus.Core.Tests.Tasks;

/// <summary>
/// Le registre des connexions tracker : quels jetons cette machine connaît, et ce que
/// chacun dessert. Jumeau du registre des projets — même dossier de configuration,
/// même forme —, à une différence près qui commande tout : <b>le secret n'est pas
/// ici</b>. Le registre ne porte que ce qui peut s'écrire en clair ; le jeton vit au
/// trousseau, sous une clé dérivée de l'identifiant de connexion.
/// </summary>
public sealed class TrackerRegistryTests : IDisposable
{
    private readonly string _configDir = Directory.CreateTempSubdirectory("cursus-trackers-").FullName;

    [Fact(DisplayName = "étant donné un registre vide, quand on ajoute une connexion, alors elle figure dans la liste avec un identifiant attribué")]
    public void Adding_a_connection_lists_it_with_an_identifier()
    {
        // arrange
        var registry = new TrackerRegistry(_configDir);

        // act
        var connection = registry.Add("Mon compte Linear", new TrackerScope.WholeWorkspace());

        // assert — l'identifiant est attribué par le registre, pas fourni : c'est lui
        // qui devra désigner le jeton au trousseau
        Assert.NotEmpty(connection.Id);
        Assert.Equal("Mon compte Linear", Assert.Single(registry.Connections).Label);
    }

    [Fact(DisplayName = "étant donné deux connexions de même libellé, quand on les inscrit, alors leurs identifiants diffèrent")]
    public void Two_connections_never_share_an_identifier()
    {
        // arrange — le cas réel : deux clés du même espace, une de compte, une de projet
        var registry = new TrackerRegistry(_configDir);

        // act
        var first = registry.Add("Linear", new TrackerScope.WholeWorkspace());
        var second = registry.Add("Linear", new TrackerScope.WholeWorkspace());

        // assert — l'identifiant désigne le jeton au trousseau : le partager ferait
        // s'écraser un secret par l'autre, en silence
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(2, registry.Connections.Count);
    }

    [Fact(DisplayName = "étant donné une connexion inscrite, quand un registre neuf relit le dossier, alors elle a survécu")]
    public void A_connection_survives_a_fresh_registry()
    {
        // arrange
        new TrackerRegistry(_configDir).Add("Mon compte Linear", new TrackerScope.WholeWorkspace());

        // act — l'app redémarre : un autre registre, le même dossier
        var reopened = new TrackerRegistry(_configDir);

        // assert
        Assert.Equal("Mon compte Linear", Assert.Single(reopened.Connections).Label);
    }

    [Fact(DisplayName = "étant donné une connexion restreinte à des projets, quand un registre neuf la relit, alors sa portée reste une sélection et garde ses projets")]
    public void A_narrowed_scope_survives_as_a_selection()
    {
        // arrange
        new TrackerRegistry(_configDir).Add(
            "Clé du projet Robustesse",
            new TrackerScope.SelectedProjects(["proj-robustesse", "proj-e2e"]));

        // act
        var reopened = new TrackerRegistry(_configDir);

        // assert — relire « tout l'espace » là où l'utilisateur avait restreint
        // élargirait sa portée à son insu
        var scope = Assert.IsType<TrackerScope.SelectedProjects>(Assert.Single(reopened.Connections).Scope);
        Assert.Equal(["proj-robustesse", "proj-e2e"], scope.ProjectIds);
    }

    [Fact(DisplayName = "étant donné deux connexions inscrites, quand on en retire une, alors elle quitte la liste et le fichier")]
    public void Removing_a_connection_forgets_it_for_good()
    {
        // arrange
        var registry = new TrackerRegistry(_configDir);
        var doomed = registry.Add("Clé jetable", new TrackerScope.WholeWorkspace());
        registry.Add("Clé gardée", new TrackerScope.WholeWorkspace());

        // act
        registry.Remove(doomed.Id);

        // assert — l'oubli doit survivre au redémarrage, sinon la connexion revient
        Assert.Equal("Clé gardée", Assert.Single(registry.Connections).Label);
        Assert.Equal("Clé gardée", Assert.Single(new TrackerRegistry(_configDir).Connections).Label);
    }

    public void Dispose() => Directory.Delete(_configDir, recursive: true);
}
