namespace Cursus.Core.Tasks;

/// <summary>
/// Une tâche telle qu'un <b>écran</b> la montre — l'instantané de requête, à ne pas
/// confondre avec la <see cref="Workflows.Execution.TaskCard"/> que le geste
/// d'exécution manipule. Récursif à dessein : une sous-tâche a exactement la forme
/// d'une tâche, seule sa position dans l'arbre diffère.
/// </summary>
/// <param name="Key">La clé humaine (« CUR-12 ») — celle-là même que porte le déclencheur d'un run.</param>
/// <param name="Column">Le nom de la colonne où siège la carte, tel que le tableau l'affiche.</param>
public sealed record TaskSummary(
    string Key,
    string Title,
    string Column,
    IReadOnlyList<TaskSummary> Children);
