namespace Cursus.Core.Workflows;

/// <summary>
/// Un nœud du graphe : un script déterministe, ses arêtes gardées sortantes,
/// et le garde-fou <see cref="MaxVisits"/> qui borne les boucles.
/// </summary>
public sealed record StepDefinition(
    string Id,
    string Name,
    ScriptSpec Script,
    int MaxVisits,
    IReadOnlyList<Edge> OutEdges);
