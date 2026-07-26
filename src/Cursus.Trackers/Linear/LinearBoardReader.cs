using System.Text.Json;

using Cursus.Core.Tasks;

namespace Cursus.Trackers.Linear;

/// <summary>
/// Traduit une réponse GraphQL de Linear en modèle de domaine — la seule part
/// <b>testée</b> du client, parce que c'est la seule qui décide quelque chose.
///
/// <para>
/// ⚠️ <b>L'arbre n'est pas donné par l'API, il se reconstruit.</b> Linear rend
/// <c>project.issues</c> <b>à plat</b>, parents et enfants confondus et dans un ordre
/// quelconque (l'enfant précède souvent sa mère). Le lien se lit sur <c>parent</c>,
/// jamais sur la position dans la liste — d'où deux passes : indexer, puis suspendre.
/// </para>
/// </summary>
public static class LinearBoardReader
{
    /// <summary>
    /// L'espace auquel le jeton donne accès. Bien moins coûteux que la liste des
    /// projets, et suffisant pour éprouver une clé : si l'organisation répond, la clé
    /// vaut, et elle dit du même coup ce qu'elle dessert.
    /// </summary>
    public static TrackerWorkspace ReadWorkspace(string json)
    {
        using var document = JsonDocument.Parse(json);
        var organization = document.RootElement.GetProperty("data").GetProperty("organization");

        return new TrackerWorkspace(
            organization.GetProperty("id").GetString() ?? "",
            organization.GetProperty("urlKey").GetString() ?? "",
            organization.GetProperty("name").GetString() ?? "");
    }

    /// <summary>
    /// Une page de projets <b>nus</b>. Requête bon marché, et la seule qui garde les
    /// projets vides visibles.
    /// </summary>
    public static LinearPage<BareProject> ReadProjects(string json)
    {
        using var document = JsonDocument.Parse(json);
        var connection = document.RootElement.GetProperty("data").GetProperty("projects");

        return new LinearPage<BareProject>(
            [.. connection.GetProperty("nodes").EnumerateArray().Select(project => new BareProject(
                project.GetProperty("id").GetString() ?? "",
                project.GetProperty("name").GetString() ?? ""))],
            NextCursorOf(connection));
    }

    /// <summary>
    /// Où reprendre, ou <c>null</c> si la page est la dernière.
    ///
    /// <para>
    /// ⚠️ <b>C'est <c>hasNextPage</c> qui décide, jamais la présence du curseur</b> :
    /// Linear rend un <c>endCursor</c> plein <b>même sur la dernière page</b> (mesuré —
    /// <c>linear-api.md</c> §7). Le lire sans cette garde offrirait une page suivante
    /// là où il n'y en a aucune, et la boucle redemanderait éternellement la même.
    /// </para>
    /// </summary>
    private static string? NextCursorOf(JsonElement connection) =>
        connection.TryGetProperty("pageInfo", out var page)
        && page.TryGetProperty("hasNextPage", out var more)
        && more.GetBoolean()
            ? page.GetProperty("endCursor").GetString()
            : null;

    public static IReadOnlyList<TaskProject> Read(string json)
    {
        using var document = JsonDocument.Parse(json);
        var nodes = document.RootElement.GetProperty("data").GetProperty("projects").GetProperty("nodes");

        return [.. nodes.EnumerateArray().Select(ReadProject)];
    }

    private static TaskProject ReadProject(JsonElement project)
    {
        var connection = project.GetProperty("issues");
        var issues = connection.GetProperty("nodes").EnumerateArray()
            .Select(ReadFlat)
            .ToList();

        var byParent = issues.Where(issue => issue.ParentKey is not null).ToLookup(issue => issue.ParentKey!);
        var present = issues.Select(issue => issue.Key).ToHashSet();

        // Est racine ce qui n'a pas de mère — mais aussi ce dont la mère manque à
        // l'appel (page tronquée, ou mère rattachée ailleurs). Sans cette seconde
        // clause, l'orpheline ne serait suspendue à rien et disparaîtrait de l'écran :
        // une tâche au mauvais rang se remarque, une tâche absente ne se remarque pas.
        var roots = issues.Where(issue => issue.ParentKey is null || !present.Contains(issue.ParentKey));

        return new TaskProject(
            project.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "",
            project.GetProperty("name").GetString() ?? "",
            [.. roots.Select(root => Suspend(root, byParent))],
            connection.TryGetProperty("pageInfo", out var page)
                && page.TryGetProperty("hasNextPage", out var more)
                && more.GetBoolean());
    }

    /// <summary>Suspend récursivement les enfants d'une issue sous elle.</summary>
    private static TaskSummary Suspend(FlatIssue issue, ILookup<string, FlatIssue> byParent) => new(
        issue.Key,
        issue.Title,
        issue.Column,
        issue.Labels,
        [.. byParent[issue.Key].Select(child => Suspend(child, byParent))]);

    private static FlatIssue ReadFlat(JsonElement issue) => new(
        issue.GetProperty("identifier").GetString() ?? "",
        issue.GetProperty("title").GetString() ?? "",
        issue.GetProperty("state").GetProperty("name").GetString() ?? "",
        issue.TryGetProperty("parent", out var parent) && parent.ValueKind is not JsonValueKind.Null
            ? parent.GetProperty("identifier").GetString()
            : null,
        ReadLabels(issue));

    /// <summary>
    /// Les noms des étiquettes, ou aucune. Tolère le champ <b>absent</b> autant que la
    /// liste vide : les deux disent « cette carte ne porte rien », et exiger le champ
    /// ferait dépendre la lecture d'une requête particulière.
    /// </summary>
    private static IReadOnlyList<string> ReadLabels(JsonElement issue) =>
        issue.TryGetProperty("labels", out var labels)
        && labels.ValueKind is not JsonValueKind.Null
        && labels.TryGetProperty("nodes", out var nodes)
            ? [.. nodes.EnumerateArray().Select(label => label.GetProperty("name").GetString() ?? "")]
            : [];

    /// <summary>
    /// Une issue telle que l'API la rend : sans ses enfants, mais sachant de qui elle
    /// pend. La forme intermédiaire de la première passe — le domaine, lui, ne connaît
    /// que l'arbre reconstruit.
    /// </summary>
    private sealed record FlatIssue(
        string Key,
        string Title,
        string Column,
        string? ParentKey,
        IReadOnlyList<string> Labels);
}
