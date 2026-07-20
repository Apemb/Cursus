namespace Cursus.Core.Workflows;

/// <summary>Ce qui a mis un run en route.</summary>
public enum RunTriggerKind
{
    /// <summary>Un humain l'a lancé.</summary>
    Manual,

    /// <summary>Une tâche du tableau le rendait disponible.</summary>
    Task,
}

/// <summary>
/// La cause d'un run. Le noyau n'en fait rien — il la transmet au journal, et
/// c'est tout ce qu'il saura jamais du tableau de tâches. La consigner dès
/// maintenant coûte deux champs ; l'ajouter après coup rendrait tous les runs
/// antérieurs orphelins de leur cause, sans moyen de la reconstituer.
/// </summary>
public sealed record RunTrigger(RunTriggerKind Kind, string? TaskKey = null)
{
    /// <summary>Lancé à la main — le seul cas tant qu'aucun tracker n'est branché.</summary>
    public static RunTrigger Manual { get; } = new(RunTriggerKind.Manual);

    /// <summary>Lancé depuis une tâche du tableau, identifiée par sa clé (« ENG-1234 »).</summary>
    public static RunTrigger ForTask(string taskKey) => new(RunTriggerKind.Task, taskKey);
}
