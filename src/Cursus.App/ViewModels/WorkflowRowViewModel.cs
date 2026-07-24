using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Core.Projects;
using Cursus.Core.Workflows;

namespace Cursus.App.ViewModels;

/// <summary>
/// Une ligne de la liste des workflows : son nom, et la trace de son dernier
/// passage. Le libellé de ce passage vient de <see cref="RunRowViewModel"/> — seul
/// endroit qui traduit une issue en mots, partagé avec l'historique de la page ;
/// l'écran arbitre le résultat, il ne recopie pas <see cref="RunState"/> (parcours §4).
/// Non testé, comme toute la vue (§7.12).
///
/// <para>
/// Elle porte aussi l'état <b>transitoire</b> de son renommage inline
/// (<see cref="IsEditing"/>/<see cref="DraftTitle"/>) : purement d'UI, le geste
/// réel (slug + déplacement de fichier) reste au parent, qui seul tient le
/// catalogue. La ligne ne fait qu'ouvrir et fermer le champ d'édition.
/// </para>
/// </summary>
public partial class WorkflowRowViewModel : ObservableObject
{
    public WorkflowRowViewModel(WorkflowLastRun workflow)
    {
        Name = workflow.Workflow.Id;
        LastRun = workflow.LastRun;
        _draftTitle = Name;
    }

    /// <summary>Le nom du workflow (son fichier), en tête de ligne.</summary>
    public string Name { get; }

    /// <summary>Vrai quand la ligne montre son champ de renommage plutôt que son libellé.</summary>
    [ObservableProperty]
    private bool _isEditing;

    /// <summary>Le titre saisi pendant un renommage ; le parent le slugifie en nouvel identifiant.</summary>
    [ObservableProperty]
    private string _draftTitle;

    /// <summary>Ouvre le champ de renommage, pré-rempli du nom courant.</summary>
    [RelayCommand]
    private void BeginRename()
    {
        DraftTitle = Name;
        IsEditing = true;
    }

    /// <summary>Referme le champ sans renommer.</summary>
    [RelayCommand]
    private void CancelRename() => IsEditing = false;

    /// <summary>Le dernier passage du workflow — <c>null</c> s'il n'a jamais tourné.</summary>
    public RunSummary? LastRun { get; }

    /// <summary>
    /// Le dernier passage en une phrase — « Échoué le 22/07 à 18:04 », ou « Jamais
    /// lancé » quand rien n'a encore tourné. Le libellé vient de
    /// <see cref="RunRowViewModel"/>, seul endroit qui traduit une issue en mots.
    /// </summary>
    public string LastPassage => LastRun is null
        ? "Jamais lancé"
        : $"{RunRowViewModel.FormatVerdict(LastRun)} {RunRowViewModel.FormatWhen(LastRun)}";
}
