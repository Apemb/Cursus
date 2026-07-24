using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Projection;

namespace Cursus.App.ViewModels;

/// <summary>
/// Le graphe <b>statique</b> d'une <see cref="WorkflowDefinition"/> : la silhouette qu'on
/// montre en header de l'onglet Étapes, seconde vue de ce que l'éditeur manipule en texte.
/// Sœur sans-projection de <see cref="RunGraphViewModel"/> — pas d'événements, pas de statut,
/// juste la forme, recalculée à chaque édition par <see cref="Show"/>. La disposition vient
/// du <see cref="GraphLayout"/> de Core (qui place déjà toute étape, orpheline comprise), la
/// géométrie du foyer partagé <see cref="GraphGeometry"/> ; les connecteurs sont les mêmes
/// <see cref="GraphConnectorRow"/> que le run, jamais empruntés (tracé gris statique). Non
/// testé (§7.12).
/// </summary>
public sealed partial class DefinitionGraphViewModel : ObservableObject
{
    /// <summary>Les nœuds du graphe, posés sur le canevas — recalculés à chaque <see cref="Show"/>.</summary>
    public ObservableCollection<DefinitionNodeRow> Nodes { get; } = new();

    /// <summary>Les connecteurs entre nœuds — arêtes avant et arêtes-retour, jamais marqués « empruntés ».</summary>
    public ObservableCollection<GraphConnectorRow> Connectors { get; } = new();

    /// <summary>La largeur du canevas — dimensionnée sur la somme des largeurs de colonnes.</summary>
    [ObservableProperty]
    private double _canvasWidth;

    /// <summary>La hauteur du canevas — dimensionnée sur la colonne la plus large (arcs de boucle compris).</summary>
    [ObservableProperty]
    private double _canvasHeight;

    /// <summary>Vrai dès qu'il y a au moins un nœud — pilote la visibilité du header (replié si vide).</summary>
    public bool HasNodes => Nodes.Count > 0;

    /// <summary>Recalcule la silhouette pour la définition courante — appelé par l'éditeur à chaque mutation.</summary>
    public void Show(WorkflowDefinition definition)
    {
        Nodes.Clear();
        Connectors.Clear();

        var names = definition.Steps.ToDictionary(step => step.Id, step => step.Name);
        var geometry = GraphGeometry.Of(GraphLayout.Of(definition), id => names.GetValueOrDefault(id, id));

        foreach (var step in definition.Steps)
        {
            var box = geometry.Boxes[step.Id];
            Nodes.Add(new DefinitionNodeRow(step.Id, step.Name, box.X, box.Y, box.Width));
        }

        foreach (var edge in geometry.Edges)
            Connectors.Add(new GraphConnectorRow(edge.From, edge.To, edge.Geometry, edge.Arrow, edge.IsBackEdge));

        CanvasWidth = geometry.CanvasWidth;
        CanvasHeight = geometry.CanvasHeight;
        OnPropertyChanged(nameof(HasNodes));
    }
}
