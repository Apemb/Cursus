using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

using Avalonia.Media;

using CommunityToolkit.Mvvm.ComponentModel;

using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Projection;

namespace Cursus.App.ViewModels;

/// <summary>
/// Le module graphe de l'écran de run : adaptateur sur <see cref="GraphProjection"/> (le
/// statut, plié du flux) et <see cref="GraphLayout"/> (la disposition, calculée une fois
/// de la structure). Vue sœur de la trajectoire, brique adossée à sa propre projection,
/// ignorant quel écran l'héberge (<c>D-016</c>). Il traduit la grille <b>abstraite</b> de
/// Core en <b>pixels</b> — c'est ici, et pas en Core, que vivent les constantes de pas, la
/// mesure des libellés et le tracé des connecteurs (§7.12, <c>D-017</c>). Non testé.
/// </summary>
public sealed partial class RunGraphViewModel : ObservableObject
{
    // Géométrie de vue : réglage à l'œil, pas un invariant — c'est ce que Core ignore.
    private const double NodeHeight = 44;
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

    private readonly GraphProjection _projection = new();
    private readonly Dictionary<string, GraphNodeRow> _rows = new();
    private readonly Dictionary<(string From, string To), GraphConnectorRow> _connectors = new();

    /// <summary>Les nœuds du graphe, positionnés sur le canevas — peuplés au premier <c>RunStarted</c>.</summary>
    public ObservableCollection<GraphNodeRow> Nodes { get; } = new();

    /// <summary>Les connecteurs tracés entre les nœuds — arêtes avant et arêtes-retour.</summary>
    public ObservableCollection<GraphConnectorRow> Connectors { get; } = new();

    /// <summary>La largeur du canevas — dimensionnée sur la somme des largeurs de colonnes.</summary>
    [ObservableProperty]
    private double _canvasWidth;

    /// <summary>La hauteur du canevas — dimensionnée sur la colonne la plus large (arcs de boucle compris).</summary>
    [ObservableProperty]
    private double _canvasHeight;

    /// <summary>Absorbe un événement : plie la projection, puis recale nœuds et connecteurs.</summary>
    public void Apply(WorkflowEvent @event)
    {
        _projection.Apply(@event);

        // La structure — et donc la disposition — naît du RunStarted (il emporte la
        // définition). C'est là, et là seulement, qu'on calcule le layout et qu'on pose
        // les nœuds et les connecteurs.
        if (@event is WorkflowEvent.RunStarted started)
        {
            Rebuild(started.Definition);
            return;
        }

        // Sinon, on recale : le statut d'un nœud et l'emprunt d'une arête ont pu bouger ;
        // les positions, elles, ne bougent pas (le graphe est statique).
        foreach (var node in _projection.Nodes)
        {
            if (_rows.TryGetValue(node.StepId, out var row))
                row.SyncWith(node);

            foreach (var edge in node.OutEdges)
                if (_connectors.TryGetValue((node.StepId, edge.Target), out var connector))
                    connector.SyncTraversed(edge.Traversed);
        }
    }

    /// <summary>Calcule la disposition et bâtit nœuds + connecteurs pour la définition qui démarre.</summary>
    private void Rebuild(WorkflowDefinition definition)
    {
        Nodes.Clear();
        Connectors.Clear();
        _rows.Clear();
        _connectors.Clear();

        var layout = GraphLayout.Of(definition);
        var placements = layout.Placements.ToDictionary(placement => placement.StepId);
        if (layout.ColumnCount == 0)
        {
            CanvasWidth = CanvasHeight = 0;
            return;
        }

        // Largeur de chaque colonne = le plus large de ses libellés mesurés. Toutes les
        // boîtes d'une colonne partagent cette largeur pour s'aligner ; les connecteurs
        // s'accrochent alors à un bord de colonne net.
        var columnWidth = new double[layout.ColumnCount];
        foreach (var node in _projection.Nodes)
        {
            var column = placements[node.StepId].Column;
            columnWidth[column] = Math.Max(columnWidth[column], NodeBoxWidth(node.Name));
        }

        // Abscisse de départ de chaque colonne : la somme des largeurs précédentes.
        var columnX = new double[layout.ColumnCount];
        var cursor = Margin;
        for (var column = 0; column < layout.ColumnCount; column++)
        {
            columnX[column] = cursor;
            cursor += columnWidth[column] + ColumnGap;
        }

        foreach (var node in _projection.Nodes)
        {
            var placement = placements[node.StepId];
            var row = new GraphNodeRow(node, columnX[placement.Column], NodeY(placement.Row), columnWidth[placement.Column]);
            _rows[node.StepId] = row;
            Nodes.Add(row);
        }

        foreach (var edge in layout.Edges)
        {
            var path = PathFor(placements[edge.From], placements[edge.To], edge.IsBackEdge, columnX, columnWidth);
            var connector = new GraphConnectorRow(edge.From, edge.To, Geometry.Parse(path), edge.IsBackEdge);
            _connectors[(edge.From, edge.To)] = connector;
            Connectors.Add(connector);
        }

        var loopDip = layout.Edges.Any(edge => edge.IsBackEdge) ? RowStride * 0.6 : 0;
        CanvasWidth = columnX[^1] + columnWidth[^1] + Margin;
        CanvasHeight = 2 * Margin + Math.Max(0, layout.RowCount - 1) * RowStride + NodeHeight + loopDip;
    }

    /// <summary>La largeur d'une boîte : le chrome fixe plus le libellé mesuré, borné pour ne pas s'étirer sans fin.</summary>
    private static double NodeBoxWidth(string label) =>
        Math.Clamp(NodeChrome + MeasureLabel(label), MinNodeWidth, MaxNodeWidth);

    /// <summary>Mesure la largeur rendue d'un libellé — le ViewModel ne dessine pas, il mesure pour disposer.</summary>
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
