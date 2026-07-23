namespace Cursus.Core.Workflows;

/// <summary>
/// Un nœud du graphe : un script déterministe, ses arêtes gardées sortantes,
/// et le garde-fou <see cref="MaxVisits"/> qui borne les boucles.
/// </summary>
/// <param name="Name">
/// Le <b>titre court</b> de l'étape (« Compiler », « Tester ») : ce que le graphe
/// affiche dans une boîte. La phrase longue va dans <see cref="Description"/>.
/// </param>
/// <param name="WorkingSubdirectory">
/// Sous-chemin <b>relatif</b> à la racine du run où lancer le script ; à défaut,
/// la racine elle-même. Relatif pour que la définition reste portable d'un
/// workspace à l'autre — c'est le <see cref="RunContext"/> qui l'absolutise.
/// </param>
/// <param name="Description">
/// Le texte long, optionnel, qui explicite l'étape — ce que <see cref="Name"/>
/// portait avant qu'on l'ait ramené à un titre court. En fin de record parce
/// qu'optionnel : les constructions positionnelles existantes n'en pâtissent pas.
/// </param>
public sealed record StepDefinition(
    string Id,
    string Name,
    ScriptSpec Script,
    int MaxVisits,
    IReadOnlyList<Edge> OutEdges,
    string? WorkingSubdirectory = null,
    string? Description = null);
