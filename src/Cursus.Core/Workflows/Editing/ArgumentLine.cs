using System.Linq;
using System.Text;

namespace Cursus.Core.Workflows.Editing;

/// <summary>
/// Traduit une ligne d'arguments humaine en <c>argv</c> et retour. Pure, jumelle de
/// <see cref="Slug"/> : l'éditeur y confie un champ texte, le runner en reçoit des
/// tokens token par token. Honore les guillemets — un argument peut contenir des
/// espaces s'il est entre <c>"…"</c> ou <c>'…'</c> — pour rendre exprimable le cas
/// courant d'un <c>zsh -c "commande avec des espaces"</c>, que le seul découpage aux
/// espaces cassait. Pas d'échappement backslash (assumé minimal) : un guillemet
/// d'une sorte est littéral à l'intérieur de l'autre.
/// </summary>
public static class ArgumentLine
{
    /// <summary>
    /// Découpe la ligne en tokens : les blancs séparent, sauf à l'intérieur d'une
    /// région entre guillemets, dont les guillemets sont retirés. Un guillemet non
    /// refermé se clôt en fin de ligne (indulgent : la saisie se fait au fil de
    /// l'eau).
    /// </summary>
    public static IReadOnlyList<string> Parse(string line)
    {
        var tokens = new List<string>();
        var token = new StringBuilder();
        var started = false;
        var quote = '\0';

        foreach (var c in line)
        {
            if (quote != '\0')
            {
                if (c == quote)
                    quote = '\0';
                else
                    token.Append(c);
            }
            else if (c is '"' or '\'')
            {
                // Ouvrir un guillemet démarre un token même s'il reste vide (« "" »
                // est un argument vide explicite).
                quote = c;
                started = true;
            }
            else if (char.IsWhiteSpace(c))
            {
                if (started)
                {
                    tokens.Add(token.ToString());
                    token.Clear();
                    started = false;
                }
            }
            else
            {
                token.Append(c);
                started = true;
            }
        }

        if (started)
            tokens.Add(token.ToString());

        return tokens;
    }

    /// <summary>
    /// Recompose la ligne affichable depuis les tokens : chacun tel quel, sauf ceux
    /// qui contiennent une espace, un guillemet, ou sont vides — entourés de
    /// guillemets pour que <see cref="Parse"/> les retrouve à l'identique. Un token
    /// qui porte un guillemet double se protège par des simples (et réciproquement) ;
    /// un token qui contient les <b>deux</b> sortes ne round-trip pas (cas
    /// pathologique assumé — pas d'échappement).
    /// </summary>
    public static string Format(IReadOnlyList<string> tokens) =>
        string.Join(' ', tokens.Select(FormatToken));

    private static string FormatToken(string token)
    {
        var needsQuoting = token.Length == 0
            || token.Any(char.IsWhiteSpace)
            || token.Contains('"')
            || token.Contains('\'');

        if (!needsQuoting)
            return token;

        return token.Contains('"') ? $"'{token}'" : $"\"{token}\"";
    }
}
