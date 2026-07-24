using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Cursus.Core.Workflows.Editing;

/// <summary>
/// Transforme un libellé humain en identifiant sûr. Sert deux fois : l'id d'une
/// étape ajoutée depuis son titre, et l'id de fichier d'un workflow créé depuis
/// son nom (où le rejet des séparateurs le rend légal pour le catalogue).
/// </summary>
public static class Slug
{
    public static string From(string label)
    {
        var folded = StripDiacritics(label.ToLowerInvariant());
        var hyphenated = Regex.Replace(folded, @"\s+", "-");
        var legal = Regex.Replace(hyphenated, "[^a-z0-9-]", "");
        return Regex.Replace(legal, "-+", "-").Trim('-');
    }

    /// <summary>
    /// Déplie les lettres accentées en leur base (« é » → « e ») : on décompose en
    /// forme canonique, puis on rejette les marques non-espaçantes (les accents,
    /// désormais des caractères à part). Sans quoi un « Générer » français
    /// produirait un id que le système de fichiers et la relecture supportent mal.
    /// </summary>
    private static string StripDiacritics(string text)
    {
        var builder = new StringBuilder(text.Length);

        foreach (var rune in text.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(rune) != UnicodeCategory.NonSpacingMark)
                builder.Append(rune);

        return builder.ToString();
    }
}
