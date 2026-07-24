using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Core.Projects;

namespace Cursus.App.ViewModels;

/// <summary>
/// Une ligne du rail des projets : son nom, sa racine, et l'état transitoire de
/// son renommage inline. Enveloppe mince sur un <see cref="Project"/> (Core,
/// immuable) — le rail ne peut pas loger d'état d'édition sur le modèle nu.
/// Non testée, comme toute la vue (§7.12).
///
/// <para>
/// Le geste réel de renommage (réécrire le disque, rafraîchir l'instantané) reste
/// au parent (<see cref="ShellViewModel"/>), seul à tenir le registre ; la ligne
/// n'ouvre et ne ferme que son champ. Une fois renommé, le parent pousse ici le
/// <see cref="Project"/> frais via <see cref="Applied"/> — la ligne survit, la
/// sélection courante aussi.
/// </para>
/// </summary>
public partial class ProjectRowViewModel : ObservableObject
{
    public ProjectRowViewModel(Project project)
    {
        _project = project;
        _draftName = project.Name;
    }

    /// <summary>Le projet sous-jacent, remplacé au renommage — d'où sa notification de <see cref="Name"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Name))]
    private Project _project;

    /// <summary>Le libellé du projet, en tête de ligne ; suit le <see cref="Project"/> quand il est remplacé.</summary>
    public string Name => Project.Name;

    /// <summary>La racine du workspace, en sous-titre.</summary>
    public string Root => Project.Root;

    /// <summary>Vrai quand la ligne montre son champ de renommage plutôt que son libellé.</summary>
    [ObservableProperty]
    private bool _isEditing;

    /// <summary>Le nom saisi pendant un renommage ; le parent le réécrit sur le disque.</summary>
    [ObservableProperty]
    private string _draftName;

    /// <summary>Ouvre le champ de renommage, pré-rempli du nom courant.</summary>
    [RelayCommand]
    private void BeginRename()
    {
        DraftName = Name;
        IsEditing = true;
    }

    /// <summary>Referme le champ sans renommer.</summary>
    [RelayCommand]
    private void CancelRename() => IsEditing = false;

    /// <summary>Le parent a renommé : on adopte le projet frais et on referme le champ.</summary>
    public void Applied(Project renamed)
    {
        Project = renamed;
        IsEditing = false;
    }
}
