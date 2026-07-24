using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Avalonia.Media;

using Cursus.Core.Workflows.Projection;

namespace Cursus.App.ViewModels;

/// <summary>La boîte d'un nœud posée en pixels : son coin haut-gauche et sa largeur (la hauteur est fixe).</summary>
public sealed record NodeBox(double X, double Y, double Width);

/// <summary>Une arête posée en pixels : son tracé absolu et le fait qu'elle referme une boucle.</summary>
public sealed record EdgePath(string From, string To, Geometry Geometry, bool IsBackEdge);

/// <summary>
/// La traduction de la grille abstraite de <see cref="GraphLayout"/> (colonnes, lignes) en
/// <b>pixels</b> — le foyer unique de la « géométrie » que <c>D-017</c> situe en App, hors de Core.
/// C'est ici que vivent les constantes de pas, la mesure des libellés et le tracé des connecteurs ;
/// les deux graphes — l'overlay de run (<see cref="RunGraphViewModel"/>) et le header de définition
/// (<see cref="DefinitionGraphViewModel"/>) — passent par elle, donc rendent identiquement. Sans
/// état, non testé (§7.12).
/// </summary>
public sealed class GraphGeometry
{
    // Géométrie de vue : réglage à l'œil, pas un invariant — c'est ce que Core ignore.
    public const double NodeHeight = 44;
    private const double RowStride = NodeHeight + 24;
    private const double ColumnGap = 40;
    private const double Margin = 16;
    private const double MinNodeWidth = 96;
    private const double MaxNodeWidth = 260;

    // Chrome horizontal d'une boîte (marges internes + glyphe + espacement + réserve de
    // badge « ×n ») qui s'ajoute à la largeur mesurée du libellé. Le badge est réservé
    // partout — un nœud peut reboucler en cours de run sans qu'on veuille le réélargir.
    private const double LabelFontSize = 13;
    private const double NodeChrome = 10 + 16 + 7 + 20 + 10;

    private GraphGeometry(IReadOnlyDictionary<string, NodeBox> boxes, IReadOnlyList<EdgePath> edges, double width, double height)
    {
        Boxes = boxes;
        Edges = edges;
        CanvasWidth = width;
        CanvasHeight = height;
    }

    /// <summary>La boîte de chaque nœud, indexée par son id d'étape.</summary>
    public IReadOnlyDictionary<string, NodeBox> Boxes { get; }

    /// <summary>Les arêtes tracées — arêtes pendantes exclues (une extrémité sans place ne se dessine pas).</summary>
    public IReadOnlyList<EdgePath> Edges { get; }

    /// <summary>La largeur du canevas — somme des largeurs de colonnes.</summary>
    public double CanvasWidth { get; }

    /// <summary>La hauteur du canevas — colonne la plus large, arcs de boucle compris.</summary>
    public double CanvasHeight { get; }

    /// <summary>
    /// Pose la grille en pixels. <paramref name="labelOf"/> donne le libellé d'un nœud (mesuré
    /// pour dimensionner sa colonne) : le run le tire de sa projection, la définition de l'étape.
    /// </summary>
    public static GraphGeometry Of(GraphLayout layout, Func<string, string> labelOf)
    {
        var placements = layout.Placements.ToDictionary(placement => placement.StepId);
        if (layout.ColumnCount == 0)
            return new GraphGeometry(new Dictionary<string, NodeBox>(), [], 0, 0);

        // Largeur de chaque colonne = le plus large de ses libellés mesurés. Toutes les
        // boîtes d'une colonne partagent cette largeur pour s'aligner ; les connecteurs
        // s'accrochent alors à un bord de colonne net.
        var columnWidth = new double[layout.ColumnCount];
        foreach (var placement in layout.Placements)
            columnWidth[placement.Column] = Math.Max(columnWidth[placement.Column], NodeBoxWidth(labelOf(placement.StepId)));

        // Abscisse de départ de chaque colonne : la somme des largeurs précédentes.
        var columnX = new double[layout.ColumnCount];
        var cursor = Margin;
        for (var column = 0; column < layout.ColumnCount; column++)
        {
            columnX[column] = cursor;
            cursor += columnWidth[column] + ColumnGap;
        }

        var boxes = new Dictionary<string, NodeBox>();
        foreach (var placement in layout.Placements)
            boxes[placement.StepId] = new NodeBox(columnX[placement.Column], NodeY(placement.Row), columnWidth[placement.Column]);

        // Garde contre les arêtes pendantes : l'éditeur tolère une cible inexistante (D-021),
        // GraphLayout émet alors une arête sans placement d'arrivée. Sans place aux deux bouts,
        // pas de tracé — le validateur signale déjà la référence pendante en texte.
        var edges = layout.Edges
            .Where(edge => placements.ContainsKey(edge.From) && placements.ContainsKey(edge.To))
            .Select(edge => new EdgePath(
                edge.From,
                edge.To,
                Geometry.Parse(PathFor(placements[edge.From], placements[edge.To], edge.IsBackEdge, columnX, columnWidth)),
                edge.IsBackEdge))
            .ToList();

        var loopDip = layout.Edges.Any(edge => edge.IsBackEdge) ? RowStride * 0.6 : 0;
        var width = columnX[^1] + columnWidth[^1] + Margin;
        var height = 2 * Margin + Math.Max(0, layout.RowCount - 1) * RowStride + NodeHeight + loopDip;
        return new GraphGeometry(boxes, edges, width, height);
    }

    /// <summary>La largeur d'une boîte : le chrome fixe plus le libellé mesuré, borné pour ne pas s'étirer sans fin.</summary>
    private static double NodeBoxWidth(string label) =>
        Math.Clamp(NodeChrome + MeasureLabel(label), MinNodeWidth, MaxNodeWidth);

    /// <summary>Mesure la largeur rendue d'un libellé — on ne dessine pas ici, on mesure pour disposer.</summary>
    private static double MeasureLabel(string label) =>
        new FormattedText(label, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, LabelFontSize, null).Width;

    private static double NodeY(int row) => Margin + row * RowStride;

    /// <summary>
    /// Le tracé d'une arête. Une arête avant relie le bord droit de la colonne source au
    /// bord gauche de la colonne cible (segment droit). Une arête-retour part du bas de la
    /// source et s'arque sous les nœuds jusqu'au bas de la cible — la boucle se voit.
    /// Formaté en culture invariante : le séparateur décimal doit rester le point, sinon
    /// une virgule casserait la grammaire du chemin.
    /// </summary>
    private static string PathFor(NodePlacement from, NodePlacement to, bool isBackEdge, double[] columnX, double[] columnWidth)
    {
        if (isBackEdge)
        {
            var bx1 = columnX[from.Column] + columnWidth[from.Column] / 2;
            var by1 = NodeY(from.Row) + NodeHeight;
            var bx2 = columnX[to.Column] + columnWidth[to.Column] / 2;
            var by2 = NodeY(to.Row) + NodeHeight;
            var dip = Math.Max(by1, by2) + RowStride * 0.55;
            return FormattableString.Invariant($"M {bx1},{by1} C {bx1},{dip} {bx2},{dip} {bx2},{by2}");
        }

        var x1 = columnX[from.Column] + columnWidth[from.Column];
        var y1 = NodeY(from.Row) + NodeHeight / 2;
        var x2 = columnX[to.Column];
        var y2 = NodeY(to.Row) + NodeHeight / 2;
        return FormattableString.Invariant($"M {x1},{y1} L {x2},{y2}");
    }
}
