namespace Cursus.Core.Workflows;

/// <summary>
/// Un nœud du graphe : le <b>commun</b> à tout type d'étape — son identité, son titre
/// court, ses arêtes gardées sortantes, et le garde-fou <see cref="MaxVisits"/> qui
/// borne les boucles. Ce que l'étape <b>fait</b> (lancer un script, piloter un agent)
/// vit dans les sous-types (<see cref="ScriptStep"/>, …) : le moteur route sur le
/// <i>type</i> de l'étape via un exécuteur dédié, sans jamais connaître les kinds — le
/// pari central du pivot (voir <c>docs/design/architecture.md</c> §5).
/// </summary>
/// <param name="Name">
/// Le <b>titre court</b> de l'étape (« Compiler », « Tester ») : ce que le graphe
/// affiche dans une boîte. La phrase longue va dans <see cref="Description"/>.
/// </param>
/// <param name="WorkingSubdirectory">
/// Sous-chemin <b>relatif</b> à la racine du run où lancer l'étape ; à défaut, la
/// racine elle-même. Relatif pour que la définition reste portable d'un workspace à
/// l'autre — c'est le <see cref="RunContext"/> qui l'absolutise.
/// </param>
/// <param name="Description">
/// Le texte long, optionnel, qui explicite l'étape — ce que <see cref="Name"/>
/// portait avant qu'on l'ait ramené à un titre court. En fin de record parce
/// qu'optionnel : les constructions positionnelles existantes n'en pâtissent pas.
/// </param>
public abstract record StepDefinition(
    string Id,
    string Name,
    int MaxVisits,
    IReadOnlyList<Edge> OutEdges,
    string? WorkingSubdirectory = null,
    string? Description = null);
