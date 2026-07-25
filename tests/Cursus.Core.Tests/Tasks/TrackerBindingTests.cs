using Cursus.Core.Tasks;

namespace Cursus.Core.Tests.Tasks;

/// <summary>
/// La déclaration de tracker d'un projet : <b>ce que le dépôt dit viser</b>, écrit dans
/// son <c>project.json</c> versionné, face à ce que cette machine sait joindre.
///
/// <para>
/// C'est la moitié partageable du lien projet ↔ tracker. L'autre — le jeton — vit au
/// trousseau, désignée par une connexion du registre machine. Les apparier est le seul
/// comportement de cette marche qui mérite une garantie : c'est lui qui décide quel
/// tableau un projet interroge, et un appariement muet enverrait un run déplacer une
/// carte dans le mauvais espace.
/// </para>
///
/// <para>
/// L'appariement se fait sur la <b>clé lisible</b> de l'espace et non sur son
/// identifiant opaque : un fichier versionné dont le contenu ne se relit pas en revue
/// perd la raison d'être qui l'a mis là. Contrepartie assumée — un espace renommé
/// devient une divergence signalée, pas un lien qui suit en silence.
/// </para>
/// </summary>
public sealed class TrackerBindingTests
{
    private static readonly TrackerWorkspace Cursus = new("ws-cursus", "cursus-app", "Cursus");

    [Fact(DisplayName = "étant donné une déclaration Linear et une connexion vers le même espace, quand on les apparie, alors la connexion correspond")]
    public void A_connection_to_the_declared_workspace_matches()
    {
        // arrange
        var declaration = new LinearBinding("cursus-app");
        var connection = new LinearConnection("c1", "Mon compte Linear", Cursus);

        // act
        var matches = declaration.Matches(connection);

        // assert
        Assert.True(matches);
    }

    [Fact(DisplayName = "étant donné une déclaration Linear et une connexion vers un autre espace, quand on les apparie, alors elle ne correspond pas")]
    public void A_connection_to_another_workspace_does_not_match()
    {
        // arrange — un poste qui n'a que le jeton d'un autre compte : le cas exact que la
        // déclaration versionnée existe pour rendre visible
        var declaration = new LinearBinding("cursus-app");
        var connection = new LinearConnection("c1", "Linear du client", new TrackerWorkspace("ws-autre", "autre-boite", "Autre Boîte"));

        // act
        var matches = declaration.Matches(connection);

        // assert
        Assert.False(matches);
    }

    [Fact(DisplayName = "étant donné une connexion Linear, quand on en tire sa déclaration, alors celle-ci porte la clé de son espace")]
    public void A_connection_yields_the_declaration_that_designates_it()
    {
        // arrange
        var connection = new LinearConnection("c1", "Mon compte Linear", Cursus);

        // act — on ne fait jamais saisir l'espace : la déclaration s'écrit comme
        // conséquence du choix d'une connexion, jamais comme un formulaire à remplir
        var declaration = connection.ToBinding();

        // assert
        Assert.Equal(new LinearBinding("cursus-app"), declaration);
    }

    [Fact(DisplayName = "étant donné une connexion, quand on apparie la déclaration qu'elle a produite, alors elle se reconnaît elle-même")]
    public void The_declaration_a_connection_produces_matches_it_back()
    {
        // arrange
        var connection = new LinearConnection("c1", "Mon compte Linear", Cursus);

        // act
        var matches = connection.ToBinding().Matches(connection);

        // assert — les deux sens sont écrits séparément, chacun sur son type : rien
        // n'empêcherait l'un d'évoluer sans l'autre, et un projet déclarerait alors une
        // connexion que plus rien ne reconnaît
        Assert.True(matches);
    }
}
