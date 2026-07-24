namespace Cursus.Core.Workflows.Execution;

/// <summary>
/// Ce qu'une visite reçoit du moteur pour s'exécuter : le répertoire de travail
/// déjà absolutisé (§4.3), et — pour un run déclenché par une tâche — la clé de
/// cette tâche. Un seul type là où l'exécuteur ne recevait qu'un chemin nu : c'est
/// le véhicule qui accueillera aussi, le moment venu, les références à la sortie
/// des étapes précédentes (<c>${ref.output}</c>, §4.9) sans nouvelle rupture de la
/// signature d'<see cref="IStepExecutor"/>.
/// </summary>
/// <param name="TaskKey">
/// La clé de la tâche du run (« ENG-1234 »), ou <c>null</c> pour un run manuel. Seul
/// le <see cref="TaskStepExecutor"/> la lit ; son absence n'est pas une erreur de
/// graphe mais un fait de run (une étape-tâche sans clé échoue, elle ne jette pas).
/// </param>
public sealed record StepExecutionContext(string WorkingDirectory, string? TaskKey = null);
