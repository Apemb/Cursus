namespace Cursus.Core.Tasks;

/// <summary>
/// Un regroupement de tâches sur le tableau — le « projet » de Linear, la *feature*
/// dans la convention du dépôt (projet = feature · issue = US · sous-tâche = commit).
/// Ne porte que ses tâches de <b>premier rang</b> : les sous-tâches pendent de leur
/// mère, jamais d'ici.
/// </summary>
/// <param name="Id">
/// L'identifiant du projet chez le tracker : ce qu'un futur prédicat de disponibilité
/// retiendra pour désigner une <em>feature</em> sans ambiguïté — un nom se répète et se
/// renomme, un identifiant désigne.
/// </param>
public sealed record TaskProject(
    string Id, string Name, IReadOnlyList<TaskSummary> Tasks);
