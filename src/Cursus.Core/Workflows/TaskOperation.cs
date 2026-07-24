namespace Cursus.Core.Workflows;

/// <summary>
/// Le geste qu'une <see cref="TaskStep"/> confie au tableau : lire une tâche,
/// déplacer sa carte, poser une étiquette. La variante est un <b>type</b>, chaque
/// sous-type ne portant que sa propre donnée (toutes non-nulles) — jamais des champs
/// nullables mutuellement exclusifs. L'exécuteur route sur ce type. Les variantes
/// sont ajoutées par triangulation, au fil des tests.
/// </summary>
public abstract record TaskOperation
{
    /// <summary>Lire la tâche du run et en déposer le corps dans le worktree (TASK.md).</summary>
    public sealed record ReadTask : TaskOperation;

    /// <summary>Déplacer la carte de la tâche vers une colonne du tableau.</summary>
    public sealed record MoveCard(string Column) : TaskOperation;

    /// <summary>Apposer une étiquette sur la tâche.</summary>
    public sealed record ApplyLabel(string Label) : TaskOperation;
}
