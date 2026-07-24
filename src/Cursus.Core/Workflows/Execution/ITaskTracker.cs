namespace Cursus.Core.Workflows.Execution;

/// <summary>
/// Le port du tableau de tâches — jumeau d'<see cref="IProcessRunner"/> : le
/// <see cref="TaskStepExecutor"/> délègue ici et ne parle jamais HTTP lui-même, ce
/// qui le rend testable sur un double. L'implémentation réelle (Linear) vit hors du
/// noyau déterministe, dans son propre projet, pour que la dépendance réseau ne
/// franchisse pas la frontière de <c>Workflows/</c> (miroir de <c>Cursus.Persistence</c>).
/// </summary>
public interface ITaskTracker
{
    /// <summary>Lit l'instantané d'une tâche par sa clé (« ENG-1234 »).</summary>
    Task<TaskCard> ReadAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Déplace la carte d'une tâche vers une colonne du tableau. Doit être idempotent (§7.10.3).</summary>
    Task MoveAsync(string key, string column, CancellationToken cancellationToken = default);

    /// <summary>Appose une étiquette sur une tâche. Doit être idempotent (§7.10.3).</summary>
    Task ApplyLabelAsync(string key, string label, CancellationToken cancellationToken = default);
}
