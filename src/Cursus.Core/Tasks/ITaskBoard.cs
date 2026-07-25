namespace Cursus.Core.Tasks;

/// <summary>
/// Le port de <b>requête</b> du tableau de tâches — la <b>porte sœur</b> d'
/// <see cref="Workflows.Execution.ITaskTracker"/>, qui est celui du <b>geste</b>
/// (<c>D-033</c>).
///
/// <para>
/// Pourquoi deux ports et non un : leurs consommateurs n'ont rien en commun.
/// <c>ITaskTracker</c> est le collaborateur du <c>TaskStepExecutor</c>, au cœur de
/// l'exécution ; celui-ci sert une <b>surface</b> qui montre le tableau et ne pose
/// aucun geste. Les réunir obligerait tout double de test d'exécution à implémenter
/// une requête dont il n'a que faire, et ferait connaître les gestes à un écran qui
/// n'en pose pas. Un seul adaptateur (Linear) implémente les deux ; rien d'autre ne
/// les voit ensemble.
/// </para>
/// </summary>
public interface ITaskBoard
{
    /// <summary>
    /// L'arbre du tableau : les projets, leurs tâches de premier rang, et les
    /// sous-tâches suspendues sous leur mère.
    /// </summary>
    Task<IReadOnlyList<TaskProject>> ListProjectsAsync(CancellationToken cancellationToken = default);
}
