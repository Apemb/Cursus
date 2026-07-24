namespace Cursus.Core.Workflows.Editing;

/// <summary>
/// Traduit une <b>ligne de commande</b> humaine en <c>(binaire, arguments)</c> et
/// retour. Pure, jumelle d'<see cref="ArgumentLine"/> — dont elle réutilise le
/// tokeniseur et le requoteur — mais avec une règle de plus : le <b>premier</b>
/// token est le binaire, le reste ses arguments. De quoi confier à l'éditeur un
/// seul champ « Commande » au lieu d'un binaire et d'une ligne d'arguments séparés.
/// </summary>
public static class CommandLine
{
    /// <summary>
    /// Découpe la ligne : le 1er token est le <c>FileName</c>, les suivants les
    /// <c>Arguments</c>. Une ligne vide (ou blancs seuls) rend un binaire vide et
    /// aucun argument — un brouillon sans commande encore saisie.
    /// </summary>
    public static (string FileName, IReadOnlyList<string> Arguments) Parse(string line)
    {
        var tokens = ArgumentLine.Parse(line);
        if (tokens.Count == 0)
            return ("", []);

        return (tokens[0], tokens.Skip(1).ToList());
    }

    /// <summary>
    /// Recompose la ligne affichable : le binaire en tête, puis les arguments, chacun
    /// requoté au besoin par <see cref="ArgumentLine.Format"/>. Un binaire vide sans
    /// argument rend la chaîne vide.
    /// </summary>
    public static string Format(string fileName, IReadOnlyList<string> arguments)
    {
        // Un brouillon sans commande n'affiche rien — pas des guillemets vides, ce
        // que rendrait ArgumentLine en requotant le seul token vide du binaire.
        if (fileName.Length == 0 && arguments.Count == 0)
            return "";

        return ArgumentLine.Format([fileName, .. arguments]);
    }
}
