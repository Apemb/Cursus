using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Projection;

namespace Cursus.App.ViewModels;

/// <summary>
/// Le module graphe de l'écran de run : adaptateur sur <see cref="GraphProjection"/> (le
/// statut, plié du flux) et <see cref="GraphLayout"/> (la disposition, calculée une fois
/// de la structure). Vue sœur de la trajectoire, brique adossée à sa propre projection,
/// ignorant quel écran l'héberge (<c>D-016</c>). La traduction de la grille <b>abstraite</b>
/// de Core en <b>pixels</b> est déléguée à <see cref="GraphGeometry"/>, foyer partagé avec
/// le header de définition (§7.12, <c>D-017</c>) ; ce VM ne garde que le statut. Non testé.
/// </summary>
public sealed partial class RunGraphViewModel : ObservableObject
{
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

        // Les libellés viennent de la projection (le nom porté par le run) ; la géométrie,
        // elle, est posée par le foyer partagé — ce VM n'y ajoute que le statut.
        var names = _projection.Nodes.ToDictionary(node => node.StepId, node => node.Name);
        var geometry = GraphGeometry.Of(GraphLayout.Of(definition), id => names.GetValueOrDefault(id, id));

        foreach (var node in _projection.Nodes)
        {
            var box = geometry.Boxes[node.StepId];
            var row = new GraphNodeRow(node, box.X, box.Y, box.Width);
            _rows[node.StepId] = row;
            Nodes.Add(row);
        }

        foreach (var edge in geometry.Edges)
        {
            var connector = new GraphConnectorRow(edge.From, edge.To, edge.Geometry, edge.Arrow, edge.IsBackEdge);
            _connectors[(edge.From, edge.To)] = connector;
            Connectors.Add(connector);
        }

        CanvasWidth = geometry.CanvasWidth;
        CanvasHeight = geometry.CanvasHeight;
    }
}
