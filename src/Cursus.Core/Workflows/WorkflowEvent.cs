namespace Cursus.Core.Workflows;

/// <summary>
/// Ce qu'un run raconte de lui-même pendant qu'il se déroule. Les variantes
/// sont imbriquées, comme celles de <see cref="Guard"/> : leurs noms sont trop
/// courants pour occuper le namespace à eux seuls.
/// </summary>
public abstract record WorkflowEvent
{
    /// <summary>
    /// Le run commence — rien n'a encore été exécuté. Il emporte la définition
    /// telle qu'elle était : relire un run six mois plus tard doit dire ce qui
    /// a tourné, pas ce que le fichier est devenu depuis.
    /// </summary>
    public sealed record RunStarted(
        WorkflowDefinition Definition,
        string WorkspaceRoot,
        RunTrigger Trigger) : WorkflowEvent;

    /// <summary>
    /// Une visite d'étape commence. Une étape en boucle en engendre autant que
    /// de tours, d'où l'itération : elle seule les distingue.
    /// </summary>
    public sealed record StepStarted(string StepId, int Iteration) : WorkflowEvent;

    /// <summary>
    /// Une visite d'étape s'achève. Elle emporte ce que le process a fait
    /// (<see cref="ScriptResult"/>) et où sa sortie a été rangée
    /// (<see cref="StepOutput"/>) — le contenu, lui, est déjà sur disque.
    /// </summary>
    public sealed record StepFinished(
        string StepId, int Iteration, ScriptResult Result, StepOutput Output) : WorkflowEvent;

    /// <summary>
    /// Le routage a retenu une arête. Séparé de la fin d'étape parce que c'est
    /// la seule <b>décision</b> du moteur : tout le reste est de l'observation.
    /// </summary>
    public sealed record EdgeChosen(string FromStepId, string ToStepId) : WorkflowEvent;

    /// <summary>
    /// Le run s'achève, quelle qu'en soit l'issue. Il est émis même quand une
    /// exception va remonter : un run sans clôture resterait « en cours » pour
    /// toujours dans le journal.
    /// </summary>
    public sealed record RunFinished(RunState State, AbortReason? AbortReason = null) : WorkflowEvent;
}
