namespace Cursus.Trackers.Linear;

/// <summary>
/// Un projet <b>sans ses issues</b> — ce que rend la requête bon marché de la lecture
/// paginée. C'est elle qui garde visibles les projets <b>vides</b> : partir des issues
/// racine les ferait disparaître, puisqu'un projet sans issue n'apparaît dans aucune.
///
/// <para>
/// Forme d'adaptateur, pas de domaine : elle décrit ce que Linear rend, et se change en
/// <see cref="Cursus.Core.Tasks.TaskProject"/> une fois les issues raccrochées.
/// </para>
/// </summary>
public sealed record BareProject(string Id, string Name);
