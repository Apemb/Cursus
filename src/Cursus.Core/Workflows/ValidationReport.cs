namespace Cursus.Core.Workflows;

/// <summary>Nature d'un problème relevé dans un graphe.</summary>
public enum ValidationIssueKind
{
    /// <summary>Le point d'entrée n'est pas renseigné.</summary>
    MissingEntryStep,

    /// <summary>Le point d'entrée désigne une étape absente du graphe.</summary>
    UnknownEntryStep,

    /// <summary>Une arête pointe vers une étape absente du graphe.</summary>
    UnknownEdgeTarget,

    /// <summary>Deux étapes ou plus partagent le même identifiant.</summary>
    DuplicateStepId,

    /// <summary>Une étape n'a pas d'identifiant, donc aucune arête ne peut la viser.</summary>
    EmptyStepId,

    /// <summary>Une étape déclare un nombre de visites maximum qui interdit sa propre exécution.</summary>
    NonPositiveMaxVisits,

    /// <summary>Aucun chemin depuis le point d'entrée ne mène à cette étape.</summary>
    UnreachableStep,
}

/// <summary>Un problème relevé, désigné par sa nature et, quand c'est pertinent, l'étape concernée.</summary>
public sealed record ValidationIssue(ValidationIssueKind Kind, string Message, string? StepId = null);

/// <summary>
/// L'inventaire complet de ce qui empêche un graphe d'être exécuté. Vide quand
/// le graphe est sain. Agrégé — et non interrompu au premier problème — parce
/// qu'un éditeur doit pouvoir tout afficher d'un coup.
/// </summary>
public sealed record ValidationReport(IReadOnlyList<ValidationIssue> Issues)
{
    public bool IsValid => Issues.Count == 0;
}
