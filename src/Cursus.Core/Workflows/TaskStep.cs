namespace Cursus.Core.Workflows;

/// <summary>
/// L'étape qui agit sur le tableau de tâches — le 3e kind. Elle confie un
/// <see cref="TaskOperation"/> (lire, déplacer, étiqueter) au tracker via son
/// exécuteur, et rend un <see cref="ScriptResult"/> routable par les gardes tout
/// comme une étape-script ou -agent. La tâche visée n'est pas dans la définition
/// (qui reste portable, §7.3) : elle vient du <see cref="RunTrigger"/> du run, et
/// descend jusqu'à l'exécuteur par le contexte d'exécution.
/// </summary>
public sealed record TaskStep(
    string Id,
    string Name,
    TaskOperation Operation,
    int MaxVisits,
    IReadOnlyList<Edge> OutEdges,
    string? WorkingSubdirectory = null,
    string? Description = null)
    : StepDefinition(Id, Name, MaxVisits, OutEdges, WorkingSubdirectory, Description);
