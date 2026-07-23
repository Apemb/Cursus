using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
/// Core en <b>pixels</b> — c'est ici, et pas en Core, que vivent les constantes de pas et
/// le tracé des connecteurs (§7.12, <c>D-017</c>). Non testé, comme toute la vue.
/// </summary>
public sealed partial class RunGraphViewModel : ObservableObject
{
    // Géométrie de vue : le pas d'une colonne et d'une ligne, la taille d'un nœud, la
    // marge. Réglage à l'œil, pas un invariant — c'est justement ce que Core ignore.
    // NOTE : NodeWidth/NodeHeight sont dupliqués dans RunView.axaml (la boîte du nœud) —
    // les connecteurs s'accrochent à ces bords, les deux valeurs doivent rester égales.
    private const double NodeWidth = 150;
    private const double NodeHeight = 44;
    private const double ColumnStride = NodeWidth + 48;
    private const double RowStride = NodeHeight + 24;
    private const double Margin = 16;

    private readonly GraphProjection _projection = new();
    private readonly Dictionary<string, GraphNodeRow> _rows = new();
    private readonly Dictionary<(string From, string To), GraphConnectorRow> _connectors = new();

    /// <summary>Les nœuds du graphe, positionnés sur le canevas — peuplés au premier <c>RunStarted</c>.</summary>
    public ObservableCollection<GraphNodeRow> Nodes { get; } = new();

    /// <summary>Les connecteurs tracés entre les nœuds — arêtes avant et arêtes-retour.</summary>
    public ObservableCollection<GraphConnectorRow> Connectors { get; } = new();

    /// <summary>La largeur du canevas — dimensionnée sur la profondeur du graphe.</summary>
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
        var placements = new Dictionary<string, NodePlacement>();
        foreach (var placement in layout.Placements)
            placements[placement.StepId] = placement;

        foreach (var node in _projection.Nodes)
        {
            var placement = placements[node.StepId];
            var row = new GraphNodeRow(node, NodeX(placement.Column), NodeY(placement.Row));
            _rows[node.StepId] = row;
            Nodes.Add(row);
        }

        foreach (var edge in layout.Edges)
        {
            var geometry = Geometry.Parse(PathFor(placements[edge.From], placements[edge.To], edge.IsBackEdge));
            var connector = new GraphConnectorRow(edge.From, edge.To, geometry, edge.IsBackEdge);
            _connectors[(edge.From, edge.To)] = connector;
            Connectors.Add(connector);
        }

        // Une arête-retour s'arque sous les nœuds : réserver la place de l'arc en bas.
        var loopDip = layout.Edges.Any(edge => edge.IsBackEdge) ? RowStride * 0.6 : 0;
        CanvasWidth = 2 * Margin + Math.Max(0, layout.ColumnCount - 1) * ColumnStride + NodeWidth;
        CanvasHeight = 2 * Margin + Math.Max(0, layout.RowCount - 1) * RowStride + NodeHeight + loopDip;
    }

    private static double NodeX(int column) => Margin + column * ColumnStride;

    private static double NodeY(int row) => Margin + row * RowStride;

    /// <summary>
    /// Le tracé d'une arête. Une arête avant relie le bord droit de la source au bord
    /// gauche de la cible (segment droit). Une arête-retour, elle, part du bas de la
    /// source et s'arque sous les nœuds jusqu'au bas de la cible — la boucle se voit.
    /// Formaté en culture invariante : le séparateur décimal doit rester le point, sinon
    /// une virgule casserait la grammaire du chemin.
    /// </summary>
    private static string PathFor(NodePlacement from, NodePlacement to, bool isBackEdge)
    {
        if (isBackEdge)
        {
            var bx1 = NodeX(from.Column) + NodeWidth / 2;
            var by1 = NodeY(from.Row) + NodeHeight;
            var bx2 = NodeX(to.Column) + NodeWidth / 2;
            var by2 = NodeY(to.Row) + NodeHeight;
            var dip = Math.Max(by1, by2) + RowStride * 0.55;
            return FormattableString.Invariant($"M {bx1},{by1} C {bx1},{dip} {bx2},{dip} {bx2},{by2}");
        }

        var x1 = NodeX(from.Column) + NodeWidth;
        var y1 = NodeY(from.Row) + NodeHeight / 2;
        var x2 = NodeX(to.Column);
        var y2 = NodeY(to.Row) + NodeHeight / 2;
        return FormattableString.Invariant($"M {x1},{y1} L {x2},{y2}");
    }
}
