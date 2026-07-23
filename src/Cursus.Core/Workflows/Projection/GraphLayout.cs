namespace Cursus.Core.Workflows.Projection;

/// <summary>La place d'un nœud dans la grille de disposition : sa colonne (profondeur) et sa ligne.</summary>
public sealed record NodePlacement(string StepId, int Column, int Row);

/// <summary>
/// Une arête disposée. <see cref="IsBackEdge"/> est vrai pour une arête de boucle — celle
/// qu'il a fallu retirer pour rendre le graphe acyclique et le disposer sans diverger ;
/// c'est elle que l'App dessine à part (le connecteur qui revient en arrière).
/// </summary>
public sealed record LaidOutEdge(string From, string To, bool IsBackEdge);

/// <summary>
/// Le calcul de disposition du graphe : il pose un <see cref="WorkflowDefinition"/> sur
/// une grille par couches. Fonction <b>pure de la structure</b>, sœur <em>statique</em> de
/// <see cref="GraphProjection"/> (dynamique). Sans une ligne d'Avalonia, sans pixel : il
/// rend une grille abstraite <c>(colonne, ligne)</c> que l'App traduit en coordonnées.
/// </summary>
public sealed class GraphLayout
{
    private GraphLayout(IReadOnlyList<NodePlacement> placements, IReadOnlyList<LaidOutEdge> edges)
    {
        Placements = placements;
        Edges = edges;
        ColumnCount = placements.Count == 0 ? 0 : placements.Max(placement => placement.Column) + 1;
        RowCount = placements.Count == 0 ? 0 : placements.Max(placement => placement.Row) + 1;
    }

    /// <summary>La place de chaque nœud dans la grille.</summary>
    public IReadOnlyList<NodePlacement> Placements { get; }

    /// <summary>Les arêtes disposées, chacune sachant si elle referme une boucle.</summary>
    public IReadOnlyList<LaidOutEdge> Edges { get; }

    /// <summary>Le nombre de colonnes occupées — la profondeur du plus long chemin, plus un.</summary>
    public int ColumnCount { get; }

    /// <summary>Le nombre de lignes de la colonne la plus large — la hauteur de la grille.</summary>
    public int RowCount { get; }

    /// <summary>Dispose la définition sur la grille.</summary>
    public static GraphLayout Of(WorkflowDefinition definition)
    {
        var backEdges = FindBackEdges(definition);

        // Prédécesseurs de chaque nœud, arêtes-retour exclues : sur le DAG restant, la
        // colonne se lit sur eux (plus-long-chemin) sans que le cycle fasse diverger.
        var predecessors = definition.Steps.ToDictionary(step => step.Id, _ => new List<string>());
        foreach (var step in definition.Steps)
            foreach (var edge in step.OutEdges)
                if (!backEdges.Contains((step.Id, edge.Target)) && predecessors.TryGetValue(edge.Target, out var into))
                    into.Add(step.Id);

        var columns = new Dictionary<string, int>();

        // La colonne d'un nœud : 0 s'il n'a aucun prédécesseur, sinon un cran au-delà du
        // plus profond d'entre eux — la profondeur suit la plus longue dépendance.
        int Column(string id) =>
            columns.TryGetValue(id, out var known)
                ? known
                : columns[id] = predecessors[id].Count == 0
                    ? 0
                    : predecessors[id].Max(pred => Column(pred) + 1);

        // Ligne : les nœuds d'une même colonne s'empilent dans l'ordre de définition,
        // chacun sur la première ligne encore libre de sa colonne.
        var nextRow = new Dictionary<int, int>();
        var placements = new List<NodePlacement>();
        foreach (var step in definition.Steps)
        {
            var column = Column(step.Id);
            var row = nextRow.TryGetValue(column, out var taken) ? taken : 0;
            nextRow[column] = row + 1;
            placements.Add(new NodePlacement(step.Id, column, row));
        }

        var edges = definition.Steps
            .SelectMany(step => step.OutEdges.Select(edge =>
                new LaidOutEdge(step.Id, edge.Target, backEdges.Contains((step.Id, edge.Target)))))
            .ToList();

        return new GraphLayout(placements, edges);
    }

    /// <summary>
    /// Repère les arêtes-retour par un parcours en profondeur : une arête vers un nœud
    /// encore sur la pile referme un cycle. On les retire pour disposer sur un DAG ; c'est
    /// aussi ce qui permet de dessiner les boucles à part. On enracine le parcours à
    /// l'entrée d'abord, puis on couvre les nœuds restants (les îlots) en ordre de
    /// définition — ainsi le résultat est déterministe.
    /// </summary>
    private static HashSet<(string From, string To)> FindBackEdges(WorkflowDefinition definition)
    {
        var successors = definition.Steps.ToDictionary(step => step.Id, step => step.OutEdges);
        var backEdges = new HashSet<(string, string)>();

        // 0 = non visité, 1 = sur la pile (gris), 2 = clos (noir).
        var state = new Dictionary<string, int>();

        void Visit(string id)
        {
            state[id] = 1;
            foreach (var edge in successors[id])
            {
                var color = state.GetValueOrDefault(edge.Target);
                if (color == 1)
                    backEdges.Add((id, edge.Target));
                else if (color == 0 && successors.ContainsKey(edge.Target))
                    Visit(edge.Target);
            }

            state[id] = 2;
        }

        if (successors.ContainsKey(definition.EntryStep))
            Visit(definition.EntryStep);
        foreach (var step in definition.Steps)
            if (state.GetValueOrDefault(step.Id) == 0)
                Visit(step.Id);

        return backEdges;
    }
}
