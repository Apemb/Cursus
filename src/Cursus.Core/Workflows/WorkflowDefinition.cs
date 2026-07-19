namespace Cursus.Core.Workflows;

/// <summary>
/// Le graphe déterministe : un point d'entrée et l'ensemble des étapes.
/// Reviewable en Git ; construit en code dans les tests au jalon 1
/// (le loader déclaratif viendra au jalon 3).
/// </summary>
public sealed record WorkflowDefinition(string EntryStep, IReadOnlyList<StepDefinition> Steps)
{
    public StepDefinition GetStep(string id) =>
        Steps.FirstOrDefault(s => s.Id == id)
        ?? throw new UnknownStepException(id);
}
