namespace Cursus.Core.Workflows.Projection;

/// <summary>
/// La projection graphe : elle plie une séquence de <see cref="WorkflowEvent"/> en
/// <b>overlay de graphe</b> — la structure apprise du <c>RunStarted</c>, le statut de
/// chaque nœud, et le fait qu'une arête ait routé. Sœur de <see cref="RunProjection"/> :
/// même flux, deux alimentations (live ou relecture, parcours §1.4), mais là où l'une
/// déroule la trajectoire parcourue, l'autre montre le graphe entier — donc ce qui n'a
/// <b>pas</b> été parcouru. Elle reste, comme sa sœur, sans une ligne d'Avalonia (§7.12).
/// </summary>
public sealed class GraphProjection
{
    private List<GraphNode> _nodes = [];

    /// <summary>Les nœuds du graphe, dans l'ordre de la définition — vides tant que rien n'a appris la structure.</summary>
    public IReadOnlyList<GraphNode> Nodes => _nodes;

    /// <summary>Absorbe un événement et met à jour l'overlay projeté.</summary>
    public void Apply(WorkflowEvent @event)
    {
        switch (@event)
        {
            case WorkflowEvent.RunStarted started:
                _nodes = started.Definition.Steps
                    .Select(step => new GraphNode(
                        step.Id,
                        step.Name,
                        GraphNodeStatus.NotVisited,
                        step.OutEdges.Select(edge => new GraphEdge(edge.Target, Traversed: false)).ToList()))
                    .ToList();
                break;

            case WorkflowEvent.StepStarted started:
                UpdateNode(started.StepId, node => node with
                {
                    Status = GraphNodeStatus.Running,
                    VisitCount = node.VisitCount + 1,
                });
                break;

            case WorkflowEvent.StepFinished finished:
                UpdateNode(finished.StepId, node => node with
                {
                    Status = finished.Result.IsSuccess ? GraphNodeStatus.Succeeded : GraphNodeStatus.Failed,
                });
                break;

            case WorkflowEvent.EdgeChosen chosen:
                UpdateNode(chosen.FromStepId, node => node with
                {
                    OutEdges = node.OutEdges
                        .Select(edge => edge.Target == chosen.ToStepId ? edge with { Traversed = true } : edge)
                        .ToList(),
                });
                break;
        }
    }

    /// <summary>Remplace le nœud d'une étape par sa version transformée — le record est immuable.</summary>
    private void UpdateNode(string stepId, Func<GraphNode, GraphNode> transform)
    {
        var index = _nodes.FindIndex(node => node.StepId == stepId);
        if (index >= 0)
            _nodes[index] = transform(_nodes[index]);
    }
}
