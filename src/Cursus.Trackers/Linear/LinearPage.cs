namespace Cursus.Trackers.Linear;

/// <summary>
/// Une page telle qu'une connexion Linear la rend. Toutes les connexions de cette API
/// ont la même forme — <c>nodes</c> et <c>pageInfo</c> — d'où un seul type, générique,
/// pour les projets comme pour les issues.
/// </summary>
/// <param name="Items">Ce que la page portait, dans l'ordre où l'API l'a rendu.</param>
/// <param name="NextCursor">
/// Où reprendre, ou <c>null</c> quand la page est la dernière. Un nullable est ici
/// légitime au regard de la convention du dépôt : il n'y a qu'un <em>type</em> en jeu —
/// un curseur — qui peut <b>manquer</b>. C'est la nuance « valeur optionnelle », façon
/// <c>Description?</c>, pas une variante de type qu'une hiérarchie devrait porter.
/// </param>
public sealed record LinearPage<T>(IReadOnlyList<T> Items, string? NextCursor);
