using Avalonia.Media;

using CommunityToolkit.Mvvm.ComponentModel;

using Cursus.Core.Workflows.Projection;

namespace Cursus.App.ViewModels;

/// <summary>
/// Un nœud du graphe dans la vue sœur de la trajectoire : une étape, l'état où le run
/// l'a laissée, et sa position sur le canevas. Là où la liste déroule les visites, le
/// graphe montre le graphe entier — donc les nœuds jamais atteints. Sa place <see cref="X"/>/
/// <see cref="Y"/> est posée par le <see cref="RunGraphViewModel"/> à partir de la grille
/// abstraite de <see cref="GraphLayout"/> (§7.12, D-017) ; toute la logique testable vit
/// en Core. Non testé, comme toute la vue.
/// </summary>
public partial class GraphNodeRow : ObservableObject
{
    public GraphNodeRow(GraphNode node, double x, double y, double width)
    {
        StepId = node.StepId;
        Name = node.Name;
        X = x;
        Y = y;
        Width = width;
        SyncWith(node);
    }

    public string StepId { get; }

    public string Name { get; }

    /// <summary>Abscisse du nœud sur le canevas — début de sa colonne, posée par le ViewModel.</summary>
    public double X { get; }

    /// <summary>Ordonnée du nœud sur le canevas — ligne × pas, posée par le ViewModel.</summary>
    public double Y { get; }

    /// <summary>Largeur de la boîte — ajustée à la colonne (au plus large de ses libellés). Les connecteurs s'accrochent à ce bord.</summary>
    public double Width { get; }

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

    /// <summary>Recale le nœud sur l'overlay fraîchement plié par la projection. La place ne bouge pas (le graphe est statique).</summary>
    public void SyncWith(GraphNode node)
    {
        Status = node.Status;
        VisitCount = node.VisitCount;
    }

    // Mêmes couleurs sémantiques que la trajectoire (§9.5), plus le gris du non-parcouru
    // — c'est ce que le graphe apporte que la liste ne peut pas montrer.
    private static readonly IBrush RunningBrush = new SolidColorBrush(Color.Parse("#0A84FF"));
    private static readonly IBrush SucceededBrush = new SolidColorBrush(Color.Parse("#2F9E44"));
    private static readonly IBrush FailedBrush = new SolidColorBrush(Color.Parse("#C0392B"));
    private static readonly IBrush NotVisitedBrush = new SolidColorBrush(Color.Parse("#B0B0B8"));
}
