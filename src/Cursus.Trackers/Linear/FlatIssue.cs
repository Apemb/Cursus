namespace Cursus.Trackers.Linear;

/// <summary>
/// Une issue telle que l'API la rend : <b>sans ses enfants</b>, mais sachant de qui elle
/// pend et à quel projet elle appartient. La forme intermédiaire de la lecture — le
/// domaine, lui, ne connaît que l'arbre reconstruit
/// (<see cref="Cursus.Core.Tasks.TaskSummary"/>).
/// </summary>
/// <param name="ParentKey">
/// La mère, ou <c>null</c> quand l'issue est de premier rang.
/// </param>
/// <param name="ProjectId">
/// Le projet auquel raccrocher la carte, ou <c>null</c> quand elle n'appartient à
/// <b>aucun</b> projet — Linear l'autorise. Invisible tant qu'on partait des projets,
/// ce cas remonte dès qu'on part des issues racine.
///
/// <para>
/// Deux nullables ici, et aucun ne contredit la convention du dépôt : ni l'un ni l'autre
/// ne distingue des <em>types</em> d'issue, tous deux disent qu'une <em>valeur</em> peut
/// manquer.
/// </para>
/// </param>
public sealed record FlatIssue(
    string Key,
    string Title,
    string Column,
    string? ParentKey,
    IReadOnlyList<string> Labels,
    string? ProjectId);
