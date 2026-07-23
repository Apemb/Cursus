using System.Collections.Generic;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Projection;

namespace Cursus.App.ViewModels;

/// <summary>
/// Le module graphe de l'écran de run : adaptateur mince sur <see cref="GraphProjection"/>
/// (§7.12), vue sœur de la trajectoire. L'écran lui fanne le <b>même</b> flux d'événements
/// qu'à la liste ; il en tient une projection à part et la reflète en lignes bindables.
/// C'est une brique adossée à sa propre projection, ignorant quel écran l'héberge
/// (<c>D-016</c>) — non testé, comme toute la vue.
/// </summary>
public sealed class RunGraphViewModel : ObservableObject
{
    private readonly GraphProjection _projection = new();
    private readonly Dictionary<string, GraphNodeRow> _rows = new();

    /// <summary>Les nœuds du graphe, dans l'ordre de la définition — peuplés au premier <c>RunStarted</c>.</summary>
    public ObservableCollection<GraphNodeRow> Nodes { get; } = new();

    /// <summary>Absorbe un événement : plie la projection, puis reflète les nœuds.</summary>
    public void Apply(WorkflowEvent @event)
    {
        _projection.Apply(@event);

        // La structure naît du RunStarted (il emporte la définition) : c'est là, et
        // là seulement, qu'on crée une ligne par nœud.
        if (@event is WorkflowEvent.RunStarted)
        {
            Nodes.Clear();
            _rows.Clear();
            foreach (var node in _projection.Nodes)
            {
                var row = new GraphNodeRow(node);
                _rows[node.StepId] = row;
                Nodes.Add(row);
            }

            return;
        }

        // Sinon, on recale les lignes existantes sur l'overlay fraîchement plié — le
        // statut d'un nœud et l'état de ses arêtes ont pu bouger.
        foreach (var node in _projection.Nodes)
            if (_rows.TryGetValue(node.StepId, out var row))
                row.SyncWith(node);
    }
}
