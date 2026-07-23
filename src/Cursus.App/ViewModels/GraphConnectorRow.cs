using Avalonia.Collections;
using Avalonia.Media;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Cursus.App.ViewModels;

/// <summary>
/// Une arête tracée sur le canevas du graphe : sa géométrie en pixels (calculée par le
/// <see cref="RunGraphViewModel"/> depuis la grille de <c>GraphLayout</c>), et le fait
/// qu'un <c>EdgeChosen</c> l'ait empruntée. Une arête-retour (une boucle) se dessine à
/// part — tiretée, arquée sous les nœuds — pour distinguer le chemin qui reboucle des
/// arêtes qui avancent. Non testé (§7.12, D-017) ; la logique — quelle arête est un
/// retour — est décidée en Core.
/// </summary>
public partial class GraphConnectorRow : ObservableObject
{
    public GraphConnectorRow(string from, string to, Geometry geometry, bool isBackEdge)
    {
        From = from;
        To = to;
        Geometry = geometry;
        IsBackEdge = isBackEdge;
    }

    public string From { get; }

    public string To { get; }

    /// <summary>Le tracé du connecteur : segment droit pour une arête avant, arc sous les nœuds pour un retour.</summary>
    public Geometry Geometry { get; }

    /// <summary>Vrai pour une arête-retour — dessinée tiretée, distincte des arêtes qui avancent.</summary>
    public bool IsBackEdge { get; }

    /// <summary>Vrai quand un <c>EdgeChosen</c> a routé par cette arête — pilote couleur et graisse.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Stroke))]
    [NotifyPropertyChangedFor(nameof(Thickness))]
    private bool _traversed;

    /// <summary>Le vert « emprunté » quand le routage l'a prise (comme les nœuds §9.5), le gris neutre du chemin resté mort sinon.</summary>
    public IBrush Stroke => Traversed ? TakenBrush : MutedBrush;

    /// <summary>Une arête prise est appuyée ; une arête morte reste fine.</summary>
    public double Thickness => Traversed ? 2.4 : 1.4;

    /// <summary>Tiretée pour une arête-retour, pleine sinon — la boucle se lit d'un coup d'œil.</summary>
    public AvaloniaList<double>? Dashes => IsBackEdge ? BackEdgeDashes : null;

    /// <summary>Recale l'état d'emprunt sur l'overlay fraîchement plié.</summary>
    public void SyncTraversed(bool traversed) => Traversed = traversed;

    private static readonly IBrush TakenBrush = new SolidColorBrush(Color.Parse("#2F9E44"));
    private static readonly IBrush MutedBrush = new SolidColorBrush(Color.Parse("#8A8A8E"));
    private static readonly AvaloniaList<double> BackEdgeDashes = new() { 4, 3 };
}
