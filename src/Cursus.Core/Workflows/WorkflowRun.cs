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

    /// <summary>
    /// Un invariant a été violé et l'exception remonte à l'appelant. Cette
    /// raison n'apparaît que dans le journal : elle sert à clore un run que
    /// personne ne clôturerait, jamais à convertir l'exception en résultat.
    /// </summary>
    Faulted,
}

/// <summary>
/// Résultat d'une exécution : son identité, l'état terminal et la trajectoire
/// complète (la séquence des StepRun visités, dans l'ordre).
/// </summary>
/// <param name="RunId">
/// L'identifiant sous lequel le run a été journalisé — le seul moyen, pour
/// l'appelant, de retrouver plus tard ce que le journal en a gardé.
/// </param>
public sealed record WorkflowRun(
    string RunId,
    RunState State,
    IReadOnlyList<StepRun> History,
    AbortReason? AbortReason = null);
