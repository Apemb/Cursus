using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Projection;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// Le calcul de disposition du graphe : il pose chaque étape sur une grille par couches
/// (profondeur en plus-long-chemin depuis l'entrée), ordonne les nœuds d'une même couche,
/// et classe les arêtes (avant vs retour) pour que l'App dessine les boucles à part.
/// Fonction <b>pure de la structure</b> — sœur statique de <see cref="GraphProjection"/>
/// (dynamique) — donc du calcul testable, sans une ligne d'Avalonia (§7.12, D-017).
/// </summary>
public sealed class GraphLayoutTests
{
    [Fact(DisplayName = "étant donné une définition d'une seule étape sans arête, quand on calcule le layout, alors elle est en colonne 0, ligne 0")]
    public void A_lone_step_sits_at_the_origin()
    {
        // arrange — une seule étape, l'entrée, sans arête sortante
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        var layout = GraphLayout.Of(definition);

        // assert — l'entrée s'ancre à l'origine de la grille
        var a = PlacementOf(layout, "A");
        Assert.Equal(0, a.Column);
        Assert.Equal(0, a.Row);
    }

    [Fact(DisplayName = "étant donné A→B, quand on calcule le layout, alors A est en colonne 0 et B en colonne 1")]
    public void An_edge_pushes_its_target_one_column_deeper()
    {
        // arrange — A route vers B en cas de succès
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B"),
        });

        // act
        var layout = GraphLayout.Of(definition);

        // assert — B suit A d'une couche
        Assert.Equal(0, PlacementOf(layout, "A").Column);
        Assert.Equal(1, PlacementOf(layout, "B").Column);
    }

    [Fact(DisplayName = "étant donné A→B→C, quand on calcule le layout, alors les colonnes valent 0, 1, 2")]
    public void A_chain_lays_out_in_successive_columns()
    {
        // arrange
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B", new Edge(Guard.OnSuccess, "C")),
            Step("C"),
        });

        // act
        var layout = GraphLayout.Of(definition);

        // assert
        Assert.Equal(0, PlacementOf(layout, "A").Column);
        Assert.Equal(1, PlacementOf(layout, "B").Column);
        Assert.Equal(2, PlacementOf(layout, "C").Column);
    }

    [Fact(DisplayName = "étant donné un diamant A→B, A→C, B→D, C→D, quand on calcule le layout, alors D est en colonne 2 et B, C en colonne 1")]
    public void A_diamond_places_the_join_past_both_branches()
    {
        // arrange — deux branches qui divergent puis reconvergent sur D
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B"), new Edge(Guard.OnFailure, "C")),
            Step("B", new Edge(Guard.OnSuccess, "D")),
            Step("C", new Edge(Guard.OnSuccess, "D")),
            Step("D"),
        });

        // act
        var layout = GraphLayout.Of(definition);

        // assert — D se pose après ses deux prédécesseurs, pas sous une arête qui l'enjambe
        Assert.Equal(1, PlacementOf(layout, "B").Column);
        Assert.Equal(1, PlacementOf(layout, "C").Column);
        Assert.Equal(2, PlacementOf(layout, "D").Column);
    }

    [Fact(DisplayName = "étant donné A→C et A→B→C, quand on calcule le layout, alors C est en colonne 2 et non 1 (le plus long chemin gagne)")]
    public void The_longest_path_decides_the_column()
    {
        // arrange — C est atteignable directement (chemin court) et via B (chemin long)
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B"), new Edge(Guard.OnFailure, "C")),
            Step("B", new Edge(Guard.OnSuccess, "C")),
            Step("C"),
        });

        // act
        var layout = GraphLayout.Of(definition);

        // assert — le plus long chemin (via B) fixe la profondeur de C
        Assert.Equal(2, PlacementOf(layout, "C").Column);
    }

    [Fact(DisplayName = "étant donné B et C tombant dans la même colonne, quand on calcule le layout, alors ils ont des lignes distinctes 0 et 1, dans l'ordre de la définition")]
    public void Nodes_sharing_a_column_stack_in_definition_order()
    {
        // arrange — A ouvre deux branches B puis C : toutes deux en colonne 1
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B"), new Edge(Guard.OnFailure, "C")),
            Step("B"),
            Step("C"),
        });

        // act
        var layout = GraphLayout.Of(definition);

        // assert — empilées sans se chevaucher, dans l'ordre où la définition les liste
        Assert.Equal(0, PlacementOf(layout, "B").Row);
        Assert.Equal(1, PlacementOf(layout, "C").Row);
    }

    [Fact(DisplayName = "étant donné une arête avant simple A→B, quand on lit l'arête disposée, alors elle n'est pas une arête-retour")]
    public void A_plain_forward_edge_is_not_a_back_edge()
    {
        // arrange
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B"),
        });

        // act
        var layout = GraphLayout.Of(definition);

        // assert — une arête qui avance ne referme aucun cycle
        Assert.False(EdgeOf(layout, "A", "B").IsBackEdge);
    }

    [Fact(DisplayName = "étant donné A→B et B→A, quand on calcule le layout, alors B→A est classée arête-retour, A reste en colonne 0 et B en colonne 1")]
    public void A_two_node_loop_marks_its_return_edge()
    {
        // arrange — B renvoie vers A : un cycle direct
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B", new Edge(Guard.OnFailure, "A")),
        });

        // act
        var layout = GraphLayout.Of(definition);

        // assert — l'arête de retour est repérée, et le cycle n'a pas fait diverger la profondeur
        Assert.True(EdgeOf(layout, "B", "A").IsBackEdge);
        Assert.False(EdgeOf(layout, "A", "B").IsBackEdge);
        Assert.Equal(0, PlacementOf(layout, "A").Column);
        Assert.Equal(1, PlacementOf(layout, "B").Column);
    }

    [Fact(DisplayName = "étant donné A→B, B→C, C→B, quand on calcule le layout, alors C→B est classée arête-retour et le calcul termine")]
    public void A_loop_deeper_in_the_graph_marks_its_return_edge()
    {
        // arrange — la boucle Tester⇄Corriger : C renvoie vers B
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B", new Edge(Guard.OnSuccess, "C")),
            Step("C", new Edge(Guard.OnFailure, "B")),
        });

        // act — ne doit pas diverger
        var layout = GraphLayout.Of(definition);

        // assert — l'arête qui remonte est l'arête-retour ; les colonnes restent finies
        Assert.True(EdgeOf(layout, "C", "B").IsBackEdge);
        Assert.False(EdgeOf(layout, "B", "C").IsBackEdge);
        Assert.Equal(1, PlacementOf(layout, "B").Column);
        Assert.Equal(2, PlacementOf(layout, "C").Column);
    }

    [Fact(DisplayName = "étant donné une étape sans arête entrante depuis l'entrée (un îlot), quand on calcule le layout, alors elle reçoit tout de même un placement")]
    public void An_unreachable_island_still_gets_a_placement()
    {
        // arrange — D n'est relié à rien : c'est précisément ce que la vue graphe existe pour montrer
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B"),
            Step("D"),
        });

        // act
        var layout = GraphLayout.Of(definition);

        // assert — l'îlot est disposé, pas oublié
        Assert.Contains(layout.Placements, placement => placement.StepId == "D");
    }

    [Fact(DisplayName = "étant donné A→B→C, quand on lit ColumnCount, alors il vaut 3")]
    public void The_column_count_spans_the_deepest_path()
    {
        // arrange
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B", new Edge(Guard.OnSuccess, "C")),
            Step("C"),
        });

        // act
        var layout = GraphLayout.Of(definition);

        // assert — trois colonnes occupées : 0, 1, 2
        Assert.Equal(3, layout.ColumnCount);
    }

    [Fact(DisplayName = "étant donné deux nœuds dans la même colonne, quand on lit RowCount, alors il vaut au moins 2")]
    public void The_row_count_spans_the_widest_column()
    {
        // arrange — B et C partagent la colonne 1
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B"), new Edge(Guard.OnFailure, "C")),
            Step("B"),
            Step("C"),
        });

        // act
        var layout = GraphLayout.Of(definition);

        // assert — la colonne la plus large impose la hauteur de la grille
        Assert.True(layout.RowCount >= 2);
    }

    /// <summary>Une étape déterministe, son id valant aussi son nom — le décor commun des tests.</summary>
    private static StepDefinition Step(string id, params Edge[] outEdges) =>
        new ScriptStep(id, id, new ScriptSpec("/usr/bin/true", []), MaxVisits: 1, outEdges);

    private static NodePlacement PlacementOf(GraphLayout layout, string stepId) =>
        layout.Placements.First(placement => placement.StepId == stepId);

    private static LaidOutEdge EdgeOf(GraphLayout layout, string from, string to) =>
        layout.Edges.First(edge => edge.From == from && edge.To == to);
}
