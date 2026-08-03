namespace Cursus.Application.Tests;

/// <summary>
/// Le même invariant que celui du noyau (`docs/design/architecture.md` §7.12), tenu
/// ici pour le socle : toute
/// la logique doit rester atteignable sans Avalonia. L'enjeu est plus dur pour
/// le socle que pour le noyau — il est partagé par les deux portes, et une
/// dépendance UI qui s'y glisserait rendrait la seconde porte inconstructible.
///
/// <para>
/// Chaque assembly du périmètre testé porte cette garantie dans son propre
/// projet de tests, plutôt qu'une liste centralisée : centraliser ferait
/// référencer le socle par les tests du noyau, c'est-à-dire remonter une couche.
/// </para>
/// </summary>
public sealed class ArchitectureTests
{
    [Fact(DisplayName = "étant donné l'assembly du socle applicatif, quand on inspecte ses références, alors aucune ne pointe vers Avalonia")]
    public void The_application_layer_depends_on_no_avalonia_assembly()
    {
        // arrange — un type quelconque du socle désigne son assembly
        var application = typeof(ProjectWorkspace).Assembly;

        // act
        var referenced = application.GetReferencedAssemblies();

        // assert
        Assert.DoesNotContain(
            referenced,
            assembly => assembly.Name!.StartsWith("Avalonia", StringComparison.OrdinalIgnoreCase));
    }
}
