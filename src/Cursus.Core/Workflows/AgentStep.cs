namespace Cursus.Core.Workflows;

/// <summary>
/// L'étape qui confie le travail à un agent — le harness « Claude Code » aujourd'hui —
/// piloté en <b>headless</b> : un prompt entre, un code de sortie sort, routable par les
/// gardes existantes tout comme une étape-script. Elle ne référence son harness et son
/// modèle que par <b>identifiants</b> (<see cref="HarnessName"/>, <see cref="ModelId"/>) :
/// le catalogue en est la source, et l'invocation réelle (« claude --model … -p … ») vit
/// dans son exécuteur, pas ici — la définition reste donnée pure (§3).
/// </summary>
public sealed record AgentStep(
    string Id,
    string Name,
    string HarnessName,
    string ModelId,
    string Prompt,
    int MaxVisits,
    IReadOnlyList<Edge> OutEdges,
    string? WorkingSubdirectory = null,
    string? Description = null)
    : StepDefinition(Id, Name, MaxVisits, OutEdges, WorkingSubdirectory, Description);
