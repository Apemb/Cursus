namespace Cursus.Core.Workflows;

/// <summary>État terminal d'une exécution de workflow.</summary>
public enum RunState
{
    /// <summary>Un nœud terminal a été atteint sur un chemin réussi.</summary>
    Completed,

    /// <summary>Une étape terminale a échoué sans arête de secours.</summary>
    Failed,

    /// <summary>Le run a été interrompu par un garde-fou (ex. boucle non convergente).</summary>
    Aborted,
}

/// <summary>Raison d'un run <see cref="RunState.Aborted"/>.</summary>
public enum AbortReason
{
    /// <summary>Une étape a dépassé son <see cref="StepDefinition.MaxVisits"/>.</summary>
    LoopNotConverging,

    /// <summary>L'appelant a annulé le run en cours de route.</summary>
    Canceled,
}

/// <summary>
/// Résultat d'une exécution : l'état terminal et la trajectoire complète
/// (la séquence des StepRun visités, dans l'ordre).
/// </summary>
public sealed record WorkflowRun(
    RunState State,
    IReadOnlyList<StepRun> History,
    AbortReason? AbortReason = null);
