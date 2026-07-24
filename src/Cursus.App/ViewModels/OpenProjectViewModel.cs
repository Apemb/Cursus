using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Core.Projects;
using Cursus.Core.Workflows;
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
    private readonly Func<string, RunViewModel> _startLive;
    private readonly Func<RunSummary, RunViewModel> _replay;
    private readonly Func<string, IReadOnlyList<RunSummary>> _runsOf;

    public OpenProjectViewModel(
        string name,
        WorkflowCatalog catalog,
        Func<IReadOnlyList<WorkflowLastRun>> loadWorkflows,
        Func<string, RunViewModel> startLive,
        Func<RunSummary, RunViewModel> replay,
        Func<string, IReadOnlyList<RunSummary>> runsOf)
    {
        Name = name;
        _catalog = catalog;
        _loadWorkflows = loadWorkflows;
        _startLive = startLive;
        _replay = replay;
        _runsOf = runsOf;
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
    /// autre chose. Un seul module à la fois — liste, run <b>ou</b> page de workflow
    /// (D-016, pas de routeur).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsShowingRun))]
    [NotifyPropertyChangedFor(nameof(IsShowingList))]
    private RunViewModel? _currentRun;

    /// <summary>
    /// La page du workflow ouvert (historique + éditeur en onglets), ou <c>null</c>.
    /// Sœur de <see cref="CurrentRun"/> : troisième contenu d'un même espace,
    /// mutuellement exclusif avec le run.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsShowingPage))]
    [NotifyPropertyChangedFor(nameof(IsShowingList))]
    private WorkflowPageViewModel? _currentWorkflowPage;

    // La page à ré-afficher quand on ferme un run lancé depuis elle : le workflow
    // reste le contexte courant, on ne retombe pas sur la liste.
    private WorkflowPageViewModel? _pageBehindRun;

    /// <summary>Vrai quand le run occupe la surface.</summary>
    public bool IsShowingRun => CurrentRun is not null;

    /// <summary>Vrai quand la page d'un workflow occupe la surface.</summary>
    public bool IsShowingPage => CurrentWorkflowPage is not null;

    /// <summary>Vrai quand ni run ni page n'occupe la surface : c'est la liste qui s'affiche.</summary>
    public bool IsShowingList => !IsShowingRun && !IsShowingPage;

    /// <summary>
    /// Ouvre la page du workflow d'une ligne — le clic sur son corps. La page compose
    /// son historique (via <see cref="_runsOf"/>) et son éditeur ; lancer ou rouvrir un
    /// run depuis elle repasse par <see cref="ShowRun"/>. Ferme un run éventuel.
    /// </summary>
    [RelayCommand]
    private void OpenWorkflowPage(WorkflowRowViewModel? row)
    {
        if (row is null)
            return;

        CurrentRun?.Dispose();
        CurrentRun = null;
        _pageBehindRun = null;
        CurrentWorkflowPage = new WorkflowPageViewModel(
            row.Name, _catalog, _runsOf, _startLive, _replay, ShowRun, RefreshWorkflows, CloseWorkflowPage);
    }

    /// <summary>Referme la page et revient à la liste.</summary>
    private void CloseWorkflowPage()
    {
        CurrentWorkflowPage = null;
        _pageBehindRun = null;
    }

    /// <summary>Lance le workflow d'une ligne et ouvre son run vif — le raccourci « Lancer » de la liste.</summary>
    [RelayCommand]
    private void StartRun(WorkflowRowViewModel? row)
    {
        if (row is null)
            return;

        ShowRun(_startLive(row.Name));
    }

    /// <summary>Referme le run ; revient à la page si le run en venait, sinon à la liste.</summary>
    [RelayCommand]
    private void CloseRun()
    {
        CurrentRun?.Dispose();
        CurrentRun = null;

        if (_pageBehindRun is not null)
        {
            _pageBehindRun.RefreshHistory(); // le passage qui vient de finir doit y apparaître
            CurrentWorkflowPage = _pageBehindRun;
            _pageBehindRun = null;
        }
    }

    /// <summary>
    /// Confie le run (vif ou en relecture) à la surface. Appelé par le raccourci de la
    /// liste comme par la page ; dans ce dernier cas la page est mémorisée pour qu'on y
    /// revienne à la fermeture — le workflow reste le contexte courant.
    /// </summary>
    private void ShowRun(RunViewModel run)
    {
        _pageBehindRun = CurrentWorkflowPage;
        CurrentWorkflowPage = null;
        CurrentRun?.Dispose();
        CurrentRun = run;
    }
}
