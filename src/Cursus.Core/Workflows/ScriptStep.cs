namespace Cursus.Core.Workflows;

/// <summary>
/// L'étape qui lance un <see cref="ScriptSpec"/> déterministe : le kind historique du
/// noyau, routable tel quel par son code de sortie. Son exécution — appliquer le
/// répertoire résolu puis lancer le process — vit dans <c>ScriptStepExecutor</c>, pas
/// ici : la définition reste donnée pure (séparation définition/exécution, §3).
/// </summary>
public sealed record ScriptStep(
    string Id,
    string Name,
    ScriptSpec Script,
    int MaxVisits,
    IReadOnlyList<Edge> OutEdges,
    string? WorkingSubdirectory = null,
    string? Description = null)
    : StepDefinition(Id, Name, MaxVisits, OutEdges, WorkingSubdirectory, Description);
