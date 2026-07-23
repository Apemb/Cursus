namespace Cursus.Core.Workflows.Projection;

/// <summary>
/// Un nœud du graphe projeté : une étape de la définition, l'état où le run l'a laissé,
/// et ses arêtes sortantes — chacune sachant si le routage l'a empruntée.
/// </summary>
/// <param name="VisitCount">
/// Le nombre de fois que le run est passé par cette étape. Le nœud reste unique quand
/// une boucle le revisite (le dépliage vit dans la trajectoire, parcours §1.4) ; ce
/// compte est ce qui, côté graphe, dit « passé ici <em>n</em> fois ».
/// </param>
public sealed record GraphNode(
    string StepId,
    string Name,
    GraphNodeStatus Status,
    IReadOnlyList<GraphEdge> OutEdges,
    int VisitCount = 0);

/// <summary>
/// Une arête sortante projetée : sa cible, et le fait qu'un <c>EdgeChosen</c> l'ait
/// routée. C'est ce drapeau qui distingue les chemins pris des chemins morts.
/// </summary>
public sealed record GraphEdge(string Target, bool Traversed);

/// <summary>L'état d'un nœud tel que l'œil le lit : jamais atteint, en cours, réussi, échoué.</summary>
public enum GraphNodeStatus
{
    NotVisited,
    Running,
    Succeeded,
    Failed,
}
