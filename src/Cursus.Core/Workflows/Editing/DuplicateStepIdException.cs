namespace Cursus.Core.Workflows.Editing;

/// <summary>
/// Levée quand un renommage viserait un identifiant déjà porté par une autre
/// étape. L'unicité d'id est un invariant du brouillon — ses opérations
/// travaillent par id — et un renommage est un <b>choix d'id délibéré</b> :
/// l'ajuster en douce (comme le fait <see cref="WorkflowDraft.AddStep"/> pour un
/// id dérivé) trahirait l'intention. On refuse plutôt.
/// </summary>
public sealed class DuplicateStepIdException(string stepId)
    : Exception($"L'identifiant d'étape « {stepId} » est déjà pris.")
{
    public string StepId { get; } = stepId;
}
