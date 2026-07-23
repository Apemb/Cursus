using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Media;

using CommunityToolkit.Mvvm.ComponentModel;

using Cursus.Core.Workflows.Projection;

namespace Cursus.App.ViewModels;

/// <summary>
/// Un nœud du graphe dans la vue sœur de la trajectoire : une étape, l'état où le run
/// l'a laissée, et ses arêtes sortantes. Là où la liste déroule les visites, le graphe
/// montre le graphe entier — donc les nœuds jamais atteints. Non testé, comme toute la
/// vue (§7.12) ; toute la logique vit dans <see cref="GraphProjection"/>.
/// </summary>
public partial class GraphNodeRow : ObservableObject
{
    public GraphNodeRow(GraphNode node)
    {
        StepId = node.StepId;
        Name = node.Name;
        OutEdges = new ObservableCollection<GraphEdgeRow>(node.OutEdges.Select(edge => new GraphEdgeRow(edge)));
        SyncWith(node);
    }

    public string StepId { get; }

    public string Name { get; }

    /// <summary>Les arêtes sortantes, dans l'ordre de la définition — estompées tant que le routage ne les a pas prises.</summary>
    public ObservableCollection<GraphEdgeRow> OutEdges { get; }

    /// <summary>L'état du nœud — pilote le glyphe et sa couleur. <c>NotVisited</c> tant que le run ne l'a pas atteint.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Glyph))]
    [NotifyPropertyChangedFor(nameof(StatusBrush))]
    private GraphNodeStatus _status;

    /// <summary>Le nombre de passages — au-delà de un, la boucle a rebouclé sur ce nœud.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VisitBadge))]
    [NotifyPropertyChangedFor(nameof(HasRevisits))]
    private int _visitCount;

    /// <summary>Le libellé du nœud.</summary>
    public string Label => Name;

    /// <summary>Le badge « ×n » d'un nœud rebouclé — vide au premier (ou zéro) passage.</summary>
    public string VisitBadge => VisitCount > 1 ? $"×{VisitCount}" : "";

    /// <summary>Vrai quand le nœud a été visité plus d'une fois — montre alors son badge.</summary>
    public bool HasRevisits => VisitCount > 1;

    /// <summary>Le repère d'un coup d'œil : jamais atteint, en cours, réussi, échoué.</summary>
    public string Glyph => Status switch
    {
        GraphNodeStatus.Succeeded => "✓",
        GraphNodeStatus.Failed => "✗",
        GraphNodeStatus.Running => "▸",
        _ => "○",
    };

    /// <summary>La couleur du glyphe — sémantique, comme la trajectoire ; le gris dit « non parcouru ».</summary>
    public IBrush StatusBrush => Status switch
    {
        GraphNodeStatus.Succeeded => SucceededBrush,
        GraphNodeStatus.Failed => FailedBrush,
        GraphNodeStatus.Running => RunningBrush,
        _ => NotVisitedBrush,
    };

    /// <summary>Recale le nœud et ses arêtes sur l'overlay fraîchement plié par la projection.</summary>
    public void SyncWith(GraphNode node)
    {
        Status = node.Status;
        VisitCount = node.VisitCount;
        for (var i = 0; i < OutEdges.Count && i < node.OutEdges.Count; i++)
            OutEdges[i].SyncWith(node.OutEdges[i]);
    }

    // Mêmes couleurs sémantiques que la trajectoire (§9.5), plus le gris du non-parcouru
    // — c'est ce que le graphe apporte que la liste ne peut pas montrer.
    private static readonly IBrush RunningBrush = new SolidColorBrush(Color.Parse("#0A84FF"));
    private static readonly IBrush SucceededBrush = new SolidColorBrush(Color.Parse("#2F9E44"));
    private static readonly IBrush FailedBrush = new SolidColorBrush(Color.Parse("#C0392B"));
    private static readonly IBrush NotVisitedBrush = new SolidColorBrush(Color.Parse("#B0B0B8"));
}

/// <summary>
/// Une arête sortante dans la vue graphe : sa cible, et le fait que le routage l'ait
/// empruntée — l'estompe distingue les chemins pris des chemins morts. Non testé (§7.12).
/// </summary>
public partial class GraphEdgeRow : ObservableObject
{
    public GraphEdgeRow(GraphEdge edge)
    {
        Target = edge.Target;
        SyncWith(edge);
    }

    public string Target { get; }

    /// <summary>Le libellé de l'arête : « → cible ».</summary>
    public string Label => $"→ {Target}";

    /// <summary>Vrai quand un <c>EdgeChosen</c> a routé par cette arête.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Opacity))]
    [NotifyPropertyChangedFor(nameof(EdgeBrush))]
    [NotifyPropertyChangedFor(nameof(EdgeWeight))]
    private bool _traversed;

    /// <summary>
    /// Empruntée ou non se lit d'abord à la <b>couleur</b> et à la <b>graisse</b> — pas
    /// à la seule opacité. Un chemin mort reste franchement lisible (0.55), juste plus
    /// discret ; une opacité plus basse le rendait absent, pas discret.
    /// </summary>
    public double Opacity => Traversed ? 1.0 : 0.55;

    /// <summary>Le vert « emprunté » quand le routage l'a prise, le gris neutre sinon.</summary>
    public IBrush EdgeBrush => Traversed ? TakenBrush : MutedBrush;

    /// <summary>Un chemin pris est appuyé (semi-gras) ; un chemin mort reste en graisse normale.</summary>
    public FontWeight EdgeWeight => Traversed ? FontWeight.SemiBold : FontWeight.Normal;

    /// <summary>Recale l'arête sur l'overlay fraîchement plié.</summary>
    public void SyncWith(GraphEdge edge) => Traversed = edge.Traversed;

    // Le vert « emprunté » reprend le vert d'issue des nœuds (§9.5) — l'arête prise
    // fait partie du chemin qui a coulé ; le gris neutre dit le chemin resté mort.
    private static readonly IBrush TakenBrush = new SolidColorBrush(Color.Parse("#2F9E44"));
    private static readonly IBrush MutedBrush = new SolidColorBrush(Color.Parse("#8A8A8E"));
}
