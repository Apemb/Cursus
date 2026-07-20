namespace Cursus.Core.Workflows;

/// <summary>
/// Un nœud du graphe : un script déterministe, ses arêtes gardées sortantes,
/// et le garde-fou <see cref="MaxVisits"/> qui borne les boucles.
/// </summary>
/// <param name="WorkingSubdirectory">
/// Sous-chemin <b>relatif</b> à la racine du run où lancer le script ; à défaut,
/// la racine elle-même. Relatif pour que la définition reste portable d'un
/// workspace à l'autre — c'est le <see cref="RunContext"/> qui l'absolutise.
/// </param>
public sealed record StepDefinition(
    string Id,
    string Name,
    ScriptSpec Script,
    int MaxVisits,
    IReadOnlyList<Edge> OutEdges,
    string? WorkingSubdirectory = null);
