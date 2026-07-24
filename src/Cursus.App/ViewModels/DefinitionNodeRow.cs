namespace Cursus.App.ViewModels;

/// <summary>
/// Un nœud du graphe de <b>définition</b> : une étape et sa place sur le canevas, sans
/// aucune sémantique de run. Là où <see cref="GraphNodeRow"/> porte un statut, un glyphe
/// et un badge de reboucle — les couleurs d'un <em>run</em> —, celui-ci n'est qu'une boîte
/// nommée : c'est la <em>forme</em> qu'on montre en éditant, pas un état d'exécution. Sa
/// place <see cref="X"/>/<see cref="Y"/> et sa <see cref="Width"/> sont posées par le
/// <see cref="DefinitionGraphViewModel"/> depuis le foyer <see cref="GraphGeometry"/>
/// (§7.12, <c>D-017</c>). Immuable, non testé.
/// </summary>
public sealed class DefinitionNodeRow
{
    public DefinitionNodeRow(string stepId, string label, double x, double y, double width)
    {
        StepId = stepId;
        Label = label;
        X = x;
        Y = y;
        Width = width;
    }

    public string StepId { get; }

    /// <summary>Le libellé affiché — le nom de l'étape.</summary>
    public string Label { get; }

    /// <summary>Abscisse de la boîte sur le canevas — début de sa colonne.</summary>
    public double X { get; }

    /// <summary>Ordonnée de la boîte sur le canevas — ligne × pas.</summary>
    public double Y { get; }

    /// <summary>Largeur de la boîte — ajustée à la colonne ; les connecteurs s'accrochent à ce bord.</summary>
    public double Width { get; }
}
