using Cursus.Core.Workflows.Editing;

namespace Cursus.Core.Tests.Workflows.Editing;

/// <summary>
/// La transformation d'un libellé humain en identifiant : ce qui rend « Générer
/// le binaire » utilisable comme id d'étape ou nom de fichier de workflow. Les
/// règles émergent une à une, par triangulation.
/// </summary>
public class SlugTests
{
    [Fact(DisplayName = "étant donné un libellé à majuscules, quand on le slugifie, alors il passe en minuscules")]
    public void An_uppercase_label_is_lowercased()
    {
        // act / assert
        Assert.Equal("compiler", Slug.From("COMPILER"));
    }

    [Fact(DisplayName = "étant donné un libellé accentué, quand on le slugifie, alors les diacritiques sont dépliés")]
    public void Diacritics_are_stripped()
    {
        // act / assert
        Assert.Equal("generer", Slug.From("Générer"));
    }

    [Fact(DisplayName = "étant donné un libellé à espaces, quand on le slugifie, alors chaque suite de blancs devient un tiret unique")]
    public void Runs_of_whitespace_become_a_single_hyphen()
    {
        // act / assert
        Assert.Equal("generer-le-binaire", Slug.From("Générer   le binaire"));
    }

    [Fact(DisplayName = "étant donné un libellé à caractères illégaux, quand on le slugifie, alors ils sont retirés")]
    public void Illegal_characters_are_dropped()
    {
        // act / assert
        Assert.Equal("etape2prod", Slug.From("Étape#2/prod."));
    }

    [Fact(DisplayName = "étant donné un libellé produisant des tirets en tête, en queue ou consécutifs, quand on le slugifie, alors ils sont fusionnés et rognés")]
    public void Hyphens_are_collapsed_and_trimmed()
    {
        // act / assert
        Assert.Equal("compiler-et-tester", Slug.From("  -Compiler -- et tester-  "));
    }

    [Fact(DisplayName = "étant donné un libellé sans aucun caractère retenu, quand on le slugifie, alors on obtient une chaîne vide")]
    public void A_label_with_nothing_retainable_yields_an_empty_string()
    {
        // act / assert
        Assert.Equal("", Slug.From("### ///"));
    }
}
