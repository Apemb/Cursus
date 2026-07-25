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
/// <param name="IsTruncated">
/// Vrai quand le tableau a plus de tâches que la réponse n'en portait. Un écran qui
/// montrerait une page en la faisant passer pour la liste entière mentirait sans le
/// dire ; c'est au modèle de porter l'aveu, pas à l'interface de le deviner.
/// </param>
public sealed record TaskProject(
    string Id, string Name, IReadOnlyList<TaskSummary> Tasks, bool IsTruncated = false);
