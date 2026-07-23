using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Projection;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// La projection graphe : elle plie le <b>même</b> flux de <see cref="WorkflowEvent"/>
/// que <see cref="RunProjection"/>, mais en un <b>overlay de graphe</b> — la structure
/// apprise du <c>RunStarted</c>, le statut de chaque nœud, et le fait qu'une arête ait
/// routé. Vue sœur de la trajectoire : là où la liste dit ce qui a été parcouru, le
/// graphe montre ce qui ne l'a pas été. Cœur testable, sans une ligne d'Avalonia
/// (§7.12), premier honneur concret de <c>D-016</c>.
/// </summary>
public sealed class GraphProjectionTests
{
    [Fact(DisplayName = "étant donné une projection neuve, quand on lit ses nœuds, alors ils sont vides")]
    public void A_fresh_projection_has_no_nodes()
    {
        // arrange
        var projection = new GraphProjection();

        // act / assert — rien n'a appris la structure : pas de graphe à montrer
        Assert.Empty(projection.Nodes);
    }

    [Fact(DisplayName = "étant donné un RunStarted appliqué, quand on lit les nœuds, alors il y a un nœud par étape, tous non visités")]
    public void Applying_RunStarted_yields_one_unvisited_node_per_step()
    {
        // arrange — le RunStarted emporte la définition : c'est d'elle que naît le graphe
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B"),
        });
        var projection = new GraphProjection();

        // act
        projection.Apply(new WorkflowEvent.RunStarted(definition, "/tmp", RunTrigger.Manual));

        // assert — un nœud par étape, dans l'ordre de la définition, aucun encore parcouru
        Assert.Collection(projection.Nodes,
            a => { Assert.Equal("A", a.StepId); Assert.Equal(GraphNodeStatus.NotVisited, a.Status); },
            b => { Assert.Equal("B", b.StepId); Assert.Equal(GraphNodeStatus.NotVisited, b.Status); });
    }

    [Fact(DisplayName = "étant donné un RunStarted appliqué, quand on lit les arêtes d'un nœud, alors elles reflètent ses OutEdges, aucune traversée")]
    public void A_node_carries_its_out_edges_none_traversed()
    {
        // arrange — A route vers B en cas de succès, vers C en cas d'échec
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B"), new Edge(Guard.OnFailure, "C")),
            Step("B"),
            Step("C"),
        });
        var projection = new GraphProjection();

        // act
        projection.Apply(new WorkflowEvent.RunStarted(definition, "/tmp", RunTrigger.Manual));

        // assert — les deux arêtes sortantes de A, vers B et C, aucune encore routée
        var a = projection.Nodes[0];
        Assert.Collection(a.OutEdges,
            toB => { Assert.Equal("B", toB.Target); Assert.False(toB.Traversed); },
            toC => { Assert.Equal("C", toC.Target); Assert.False(toC.Traversed); });
    }

    [Fact(DisplayName = "étant donné une étape démarrée, quand on lit son nœud, alors il est en cours")]
    public void A_started_step_marks_its_node_running()
    {
        // arrange
        var projection = Started();

        // act
        projection.Apply(new WorkflowEvent.StepStarted("A", Iteration: 1));

        // assert — le nœud du graphe suit ce qui tourne
        Assert.Equal(GraphNodeStatus.Running, NodeOf(projection, "A").Status);
    }

    [Fact(DisplayName = "étant donné une étape achevée avec le code 0, quand on lit son nœud, alors il est réussi")]
    public void A_successful_finish_marks_its_node_succeeded()
    {
        // arrange
        var projection = Started();
        projection.Apply(new WorkflowEvent.StepStarted("A", 1));

        // act
        projection.Apply(new WorkflowEvent.StepFinished(
            "A", 1, new ScriptResult(0, ScriptOutcome.Completed), NoOutput));

        // assert
        Assert.Equal(GraphNodeStatus.Succeeded, NodeOf(projection, "A").Status);
    }

    [Fact(DisplayName = "étant donné une étape achevée en échec, quand on lit son nœud, alors il est échoué")]
    public void A_failed_finish_marks_its_node_failed()
    {
        // arrange
        var projection = Started();
        projection.Apply(new WorkflowEvent.StepStarted("A", 1));

        // act — sortie non nulle : un échec
        projection.Apply(new WorkflowEvent.StepFinished(
            "A", 1, new ScriptResult(1, ScriptOutcome.Completed), NoOutput));

        // assert
        Assert.Equal(GraphNodeStatus.Failed, NodeOf(projection, "A").Status);
    }

    [Fact(DisplayName = "étant donné une étape jamais démarrée, quand on lit son nœud, alors il reste non visité")]
    public void An_untouched_step_stays_not_visited()
    {
        // arrange
        var projection = Started();

        // act — seule A tourne puis réussit ; B n'est jamais atteinte
        projection.Apply(new WorkflowEvent.StepStarted("A", 1));
        projection.Apply(new WorkflowEvent.StepFinished(
            "A", 1, new ScriptResult(0, ScriptOutcome.Completed), NoOutput));

        // assert — le graphe montre justement ce qui n'a pas été parcouru
        Assert.Equal(GraphNodeStatus.NotVisited, NodeOf(projection, "B").Status);
    }

    [Fact(DisplayName = "étant donné une étape visitée deux fois en boucle, quand on lit son nœud, alors il porte la dernière issue et un compte de visites de 2")]
    public void A_looping_node_carries_its_last_outcome_and_visit_count()
    {
        // arrange — A échoue au tour 1, la boucle repart, A réussit au tour 2
        var projection = Started();

        // act
        projection.Apply(new WorkflowEvent.StepStarted("A", 1));
        projection.Apply(new WorkflowEvent.StepFinished(
            "A", 1, new ScriptResult(1, ScriptOutcome.Completed), NoOutput));
        projection.Apply(new WorkflowEvent.StepStarted("A", 2));
        projection.Apply(new WorkflowEvent.StepFinished(
            "A", 2, new ScriptResult(0, ScriptOutcome.Completed), NoOutput));

        // assert — la dernière issue gagne, et le nœud sait qu'on y est passé deux fois
        var a = NodeOf(projection, "A");
        Assert.Equal(GraphNodeStatus.Succeeded, a.Status);
        Assert.Equal(2, a.VisitCount);
    }

    [Fact(DisplayName = "étant donné un EdgeChosen appliqué, quand on lit les arêtes, alors celle empruntée est traversée et l'autre ne l'est pas")]
    public void An_edge_chosen_marks_only_that_edge_traversed()
    {
        // arrange — A route vers B en cas de succès, vers C en cas d'échec
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B"), new Edge(Guard.OnFailure, "C")),
            Step("B"),
            Step("C"),
        });
        var projection = new GraphProjection();
        projection.Apply(new WorkflowEvent.RunStarted(definition, "/tmp", RunTrigger.Manual));

        // act — le routage a retenu A→B
        projection.Apply(new WorkflowEvent.EdgeChosen("A", "B"));

        // assert — A→B empruntée, A→C restée un chemin mort
        var a = NodeOf(projection, "A");
        Assert.True(a.OutEdges.First(edge => edge.Target == "B").Traversed);
        Assert.False(a.OutEdges.First(edge => edge.Target == "C").Traversed);
    }

    /// <summary>Une étape déterministe, son id valant aussi son nom — le décor commun des tests.</summary>
    private static StepDefinition Step(string id, params Edge[] outEdges) =>
        new(id, id, new ScriptSpec("/usr/bin/true", []), MaxVisits: 1, outEdges);

    /// <summary>Un graphe A→B dont le run vient de démarrer — le décor commun des tests de statut.</summary>
    private static GraphProjection Started()
    {
        var projection = new GraphProjection();
        projection.Apply(new WorkflowEvent.RunStarted(TwoSteps, "/tmp", RunTrigger.Manual));
        return projection;
    }

    private static GraphNode NodeOf(GraphProjection projection, string stepId) =>
        projection.Nodes.First(node => node.StepId == stepId);

    private static StepOutput NoOutput => new([]);

    private static readonly WorkflowDefinition TwoSteps = new("A", new[]
    {
        Step("A", new Edge(Guard.OnSuccess, "B")),
        Step("B"),
    });
}
