using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Core.Projects;
using Cursus.Core.Workflows.Editing;

namespace Cursus.App.ViewModels;

/// <summary>
/// Un projet ouvert, tel qu'il occupe la surface de droite. Il tient deux
/// contenus d'une même surface — <b>sans routeur</b> : la liste de ses workflows,
/// ou l'écran du run courant quand on en lance (ou rouvre) un. Il accueillera plus
/// tard le sélecteur run/sessions et l'engrenage de configuration — d'où ce nom,
/// qui désigne le conteneur d'un projet ouvert et non le seul mode run.
///
/// <para>
/// Adaptateur mince : la jointure workflows × runs vit dans <see cref="ProjectHost"/>
/// (Core), déjà testée ; ici on ne fait que binder, et déléguer le montage d'un
/// <see cref="RunViewModel"/> à des fabriques reçues (la coquille les câble sur le
/// host et le magasin d'artefacts du projet). Non testé, comme toute la vue (§7.12).
/// </para>
/// </summary>
public partial class OpenProjectViewModel : ObservableObject
{
    private readonly WorkflowCatalog _catalog;
    private readonly Func<IReadOnlyList<WorkflowLastRun>> _loadWorkflows;
    private readonly Func<string, RunViewModel> _startRun;
    private readonly Func<WorkflowRowViewModel, RunViewModel> _openPastRun;

    public OpenProjectViewModel(
        string name,
        WorkflowCatalog catalog,
        Func<IReadOnlyList<WorkflowLastRun>> loadWorkflows,
        Func<string, RunViewModel> startRun,
        Func<WorkflowRowViewModel, RunViewModel> openPastRun)
    {
        Name = name;
        _catalog = catalog;
        _loadWorkflows = loadWorkflows;
        _startRun = startRun;
        _openPastRun = openPastRun;
        Workflows = new ObservableCollection<WorkflowRowViewModel>();
        RefreshWorkflows();
    }

    /// <summary>Le libellé du projet, affiché en tête de la surface.</summary>
    public string Name { get; }

    /// <summary>
    /// Les workflows du projet, chacun avec son dernier passage. Observable parce
    /// que le volet catalogue la remanie en direct : créer, renommer ou supprimer
    /// un workflow rejoue <see cref="RefreshWorkflows"/>, qui la reconstruit depuis
    /// le disque (via <see cref="_loadWorkflows"/>) — la liste reste le reflet fidèle
    /// du dossier, sans qu'on devine l'effet d'une mutation.
    /// </summary>
    public ObservableCollection<WorkflowRowViewModel> Workflows { get; }

    /// <summary>Le titre saisi pour un nouveau workflow ; vidé après une création réussie.</summary>
    [ObservableProperty]
    private string _newWorkflowTitle = "";

    /// <summary>Le dernier refus du catalogue à afficher, ou <c>null</c> si la dernière opération a réussi.</summary>
    [ObservableProperty]
    private string? _catalogError;

    /// <summary>
    /// Crée un workflow depuis le titre saisi : <see cref="WorkflowCatalog.CreateFromTitle"/>
    /// en slugifie l'identifiant de fichier (<c>D-022</c>). Traduit ses deux refus
    /// — nom déjà pris, titre qui ne donne aucun identifiant — en message plutôt
    /// que de laisser l'exception filer.
    /// </summary>
    [RelayCommand]
    private void NewWorkflow()
    {
        var title = NewWorkflowTitle.Trim();
        if (title.Length == 0)
            return;

        try
        {
            _catalog.CreateFromTitle(title);
            NewWorkflowTitle = "";
            CatalogError = null;
            RefreshWorkflows();
        }
        catch (WorkflowAlreadyExistsException)
        {
            CatalogError = $"Un workflow « {title} » existe déjà.";
        }
        catch (InvalidWorkflowIdException)
        {
            CatalogError = "Ce titre ne donne aucun identifiant de fichier valide.";
        }
    }

    /// <summary>
    /// Renomme le workflow d'une ligne vers le titre qu'elle a saisi : le nouvel
    /// identifiant en est le slug (même règle que la création, <c>D-022</c>), le
    /// fichier est déplacé. Referme le champ d'édition au succès ; les mêmes refus
    /// deviennent un message.
    /// </summary>
    [RelayCommand]
    private void RenameWorkflow(WorkflowRowViewModel? row)
    {
        var title = row?.DraftTitle.Trim() ?? "";
        if (row is null || title.Length == 0)
            return;

        var newId = Slug.From(title);
        try
        {
            _catalog.Rename(row.Name, newId);
            row.IsEditing = false;
            CatalogError = null;
            RefreshWorkflows();
        }
        catch (WorkflowAlreadyExistsException)
        {
            CatalogError = $"Un workflow « {newId} » existe déjà.";
        }
        catch (InvalidWorkflowIdException)
        {
            CatalogError = "Ce titre ne donne aucun identifiant de fichier valide.";
        }
    }

    /// <summary>Supprime le workflow d'une ligne, puis rafraîchit la liste.</summary>
    [RelayCommand]
    private void DeleteWorkflow(WorkflowRowViewModel? row)
    {
        if (row is null)
            return;

        _catalog.Delete(row.Name);
        CatalogError = null;
        RefreshWorkflows();
    }

    /// <summary>Reconstruit la liste depuis le disque — le reflet fidèle du dossier après une mutation.</summary>
    private void RefreshWorkflows()
    {
        Workflows.Clear();
        foreach (var workflow in _loadWorkflows())
            Workflows.Add(new WorkflowRowViewModel(workflow));
    }

    /// <summary>
    /// L'écran du run occupant la surface, ou <c>null</c> quand la surface montre
    /// la liste. Un seul à la fois — la liste ou le run, pas les deux.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsShowingRun))]
    private RunViewModel? _currentRun;

    /// <summary>Vrai quand le run occupe la surface ; pilote le basculement liste ⇄ run.</summary>
    public bool IsShowingRun => CurrentRun is not null;

    /// <summary>Lance le workflow d'une ligne et ouvre son run vif sur la surface.</summary>
    [RelayCommand]
    private void StartRun(WorkflowRowViewModel? row)
    {
        if (row is null)
            return;

        SwapRun(_startRun(row.Name));
    }

    /// <summary>Rouvre le dernier passage d'une ligne en relecture — même écran, figé.</summary>
    [RelayCommand]
    private void OpenPastRun(WorkflowRowViewModel? row)
    {
        if (row is null || !row.HasLastRun)
            return;

        SwapRun(_openPastRun(row));
    }

    /// <summary>Referme le run et revient à la liste.</summary>
    [RelayCommand]
    private void CloseRun() => SwapRun(null);

    private void SwapRun(RunViewModel? next)
    {
        CurrentRun?.Dispose();
        CurrentRun = next;
    }
}
