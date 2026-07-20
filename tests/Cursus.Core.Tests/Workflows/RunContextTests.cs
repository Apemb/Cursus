using Cursus.Core.Workflows;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// La racine du workspace d'un run : obligatoire, existante, et infranchissable.
/// </summary>
public class RunContextTests
{
    [Fact(DisplayName = "étant donné une racine vide, quand on construit le contexte, alors la construction est refusée")]
    public void An_empty_workspace_root_is_rejected()
    {
        // act / assert
        Assert.Throws<ArgumentException>(() => new RunContext(""));
    }

    [Fact(DisplayName = "étant donné une racine relative, quand on construit le contexte, alors la construction est refusée")]
    public void A_relative_workspace_root_is_rejected()
    {
        // act / assert — une racine relative dépendrait du cwd du process hôte,
        // c'est-à-dire précisément ce que le contexte existe pour éliminer.
        Assert.Throws<ArgumentException>(() => new RunContext("projets/cursus"));
    }

    [Fact(DisplayName = "étant donné une racine pointant vers un répertoire inexistant, quand on construit le contexte, alors la construction est refusée")]
    public void A_workspace_root_that_does_not_exist_is_rejected()
    {
        // act / assert — sans ce refus, chaque script du run échouerait plus
        // tard, un par un, sans jamais nommer la vraie cause.
        Assert.Throws<DirectoryNotFoundException>(() => new RunContext("/chemin/qui/nexiste/pas"));
    }

    [Fact(DisplayName = "étant donné une racine existante exprimée avec un détour, quand on construit le contexte, alors la racine retenue est sa forme normalisée")]
    public void The_workspace_root_is_kept_in_normalised_form()
    {
        // arrange — un aller-retour par un sous-répertoire désigne la racine
        // elle-même, mais textuellement autrement.
        var root = Directory.CreateTempSubdirectory("cursus-ctx-").FullName;
        Directory.CreateDirectory(Path.Combine(root, "detour"));

        // act
        var context = new RunContext(Path.Combine(root, "detour", ".."));

        // assert
        Assert.Equal(root, context.WorkspaceRoot);
    }

    [Fact(DisplayName = "étant donné aucun sous-chemin, quand on résout, alors on obtient la racine elle-même")]
    public void Resolving_no_subdirectory_yields_the_workspace_root()
    {
        // arrange
        var context = NewContext(out var root);

        // act / assert
        Assert.Equal(root, context.Resolve(null));
    }

    [Fact(DisplayName = "étant donné un sous-chemin simple, quand on résout, alors on obtient le chemin absolu correspondant sous la racine")]
    public void Resolving_a_subdirectory_yields_its_absolute_path()
    {
        // arrange
        var context = NewContext(out var root);

        // act / assert
        Assert.Equal(Path.Combine(root, "backend"), context.Resolve("backend"));
    }

    [Fact(DisplayName = "étant donné un sous-chemin imbriqué, quand on résout, alors on obtient le chemin absolu correspondant")]
    public void Resolving_a_nested_subdirectory_yields_its_absolute_path()
    {
        // arrange
        var context = NewContext(out var root);

        // act / assert
        Assert.Equal(Path.Combine(root, "backend", "tests"), context.Resolve("backend/tests"));
    }

    [Fact(DisplayName = "étant donné un sous-chemin qui remonte hors de la racine, quand on résout, alors l'évasion est refusée")]
    public void A_subdirectory_climbing_out_of_the_workspace_is_rejected()
    {
        // arrange
        var context = NewContext(out _);

        // act / assert
        Assert.Throws<PathEscapesWorkspaceException>(() => context.Resolve("../../etc"));
    }

    [Fact(DisplayName = "étant donné un sous-chemin menant à un voisin dont le nom commence par celui de la racine, quand on résout, alors l'évasion est refusée")]
    public void A_sibling_sharing_the_workspace_name_prefix_is_rejected()
    {
        // arrange — « <racine>-autre » commence par la racine sans être dedans :
        // une comparaison de préfixe naïve le laisserait passer.
        var context = NewContext(out var root);
        var sibling = $"../{Path.GetFileName(root)}-autre";

        // act / assert
        Assert.Throws<PathEscapesWorkspaceException>(() => context.Resolve(sibling));
    }

    [Fact(DisplayName = "étant donné un sous-chemin absolu, quand on résout, alors il est refusé")]
    public void An_absolute_subdirectory_is_rejected()
    {
        // arrange — seul le relatif est déclarable : un absolu ferait sortir
        // du workspace sans même avoir l'air de remonter.
        var context = NewContext(out _);

        // act / assert
        Assert.Throws<PathEscapesWorkspaceException>(() => context.Resolve("/etc"));
    }

    // --- helpers ---

    /// <summary>Un contexte sur un workspace temporaire neuf.</summary>
    private static RunContext NewContext(out string root)
    {
        root = Directory.CreateTempSubdirectory("cursus-ctx-").FullName;
        return new RunContext(root);
    }
}
