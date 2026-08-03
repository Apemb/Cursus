using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Application;
using Cursus.Core.Projects;

namespace Cursus.App.ViewModels;

/// <summary>
/// La coquille : le rail des projets connus à gauche, une surface à droite (le
/// projet ouvert). Adaptateur mince sur <see cref="ProjectRegistry"/> — toute la
/// logique de registre vit en Core ; ici on ne fait que binder et traduire un
/// refus en message. Le rail présente des <see cref="ProjectRowViewModel"/> (et
/// non des <see cref="Project"/> nus) pour loger l'état de renommage inline.
///
/// <para>
/// Elle tient aussi, en attendant sa réification (§7.13), le rôle de racine
/// au-dessus des hosts : elle <b>construit</b> le <see cref="ProjectHost"/> du
/// projet sélectionné et le <b>dispose</b> quand on en change. Elle le fait par
/// une fabrique reçue — jamais elle n'apprend que c'est du SQLite —, et la règle
/// de sens unique tient : la surface reçoit la projection du host, pas le host.
/// </para>
/// </summary>
public partial class ShellViewModel : ObservableObject, IDisposable
{
    private readonly ProjectRegistry _registry;
    private readonly Func<Project, ProjectWorkspace> _openWorkspace;
    private readonly Func<TrackerSettingsViewModel> _openSettings;
    private readonly Func<Project, Action, TaskBoardViewModel> _openTaskBoard;
    private ProjectWorkspace? _currentWorkspace;

    public ShellViewModel(
        ProjectRegistry registry,
        Func<Project, ProjectWorkspace> openWorkspace,
        Func<TrackerSettingsViewModel> openSettings,
        Func<Project, Action, TaskBoardViewModel> openTaskBoard)
    {
        _registry = registry;
        _openWorkspace = openWorkspace;
        _openSettings = openSettings;
        _openTaskBoard = openTaskBoard;
        Projects = new ObservableCollection<ProjectRowViewModel>(
            registry.Projects.Select(project => new ProjectRowViewModel(project)));
    }

    /// <summary>Les projets du rail, reflet observable de la liste du registre — en lignes bindables.</summary>
    public ObservableCollection<ProjectRowViewModel> Projects { get; }

    /// <summary>
    /// Les sessions terminal. Elles ne sont plus la surface câblée depuis que
    /// sélectionner un projet ouvre son mode run ; on les garde en attendant leur
    /// réintégration <em>par projet</em>, via le futur sélecteur run/sessions.
    /// </summary>
    public MainViewModel Sessions { get; } = new();

    /// <summary>
    /// La surface de droite : le projet ouvert, ou <c>null</c> quand aucun n'est
    /// sélectionné. Un seul objet à la fois — la coquille montre l'un ou l'autre,
    /// sans routeur.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOpenProject))]
    private OpenProjectViewModel? _currentSurface;

    /// <summary>Vrai quand un projet occupe la surface ; pilote l'affichage du repère « choisissez un projet ».</summary>
    public bool HasOpenProject => CurrentSurface is not null;

    [ObservableProperty]
    private ProjectRowViewModel? _selectedProject;

    /// <summary>
    /// La jonction rail → surface : choisir un projet ouvre son host, lit le
    /// dernier passage de ses workflows et en fait la surface ; le désélectionner
    /// la vide. Le host du projet précédent est disposé — une connexion SQLite par
    /// projet, jamais deux — et la surface reconstruite, l'objet ouvert est jetable.
    /// </summary>
    partial void OnSelectedProjectChanged(ProjectRowViewModel? value)
    {
        _currentWorkspace?.Dispose();

        if (value is null)
        {
            _currentWorkspace = null;
            CurrentSurface = null;
            return;
        }

        var project = value.Project;
        var workspace = _currentWorkspace = _openWorkspace(project);

        // Les fabriques de run capturent le host et le magasin d'artefacts du
        // projet : la surface obtient des RunViewModel déjà câblés, sans jamais
        // toucher au host elle-même (règle de sens unique).
        CurrentSurface = new OpenProjectViewModel(
            project.Name,
            workspace.Catalog,
            workspace.Host.LastRunPerWorkflow,
            workflowId => RunViewModel.StartLive(workflowId, workspace.Host, workspace.Artifacts),
            summary => RunViewModel.Replay(summary, workspace.Host, workspace.Artifacts),
            workspace.Host.RunsOf,

            // L'écran des tâches n'appartient pas au workspace du projet : le registre
            // des connexions est global, et le tableau ne se monte qu'une fois la
            // connexion arrêtée. La coquille lui remet aussi de quoi ouvrir les
            // réglages, sa seule issue quand aucun jeton ne dessert la cible déclarée.
            () => _openTaskBoard(project, OpenSettings));
    }

    /// <summary>Ferme le workspace encore ouvert : sa connexion SQLite ne doit pas fuir à la fermeture.</summary>
    public void Dispose() => _currentWorkspace?.Dispose();

    // --- les réglages : un panneau de coquille, pas un module de surface ---

    /// <summary>
    /// Le panneau des connexions tracker quand il est ouvert. Il se superpose à la
    /// surface au lieu d'y prendre place : un jeton dessert des projets du tracker, pas
    /// un projet Cursus, et il reste joignable même sans projet ouvert.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSettings))]
    private TrackerSettingsViewModel? _settings;

    /// <summary>Vrai quand le panneau des réglages couvre la fenêtre.</summary>
    public bool HasSettings => Settings is not null;

    [RelayCommand]
    private void OpenSettings() => Settings = _openSettings();

    [RelayCommand]
    private void CloseSettings() => Settings = null;

    /// <summary>Le dernier refus d'ajout ou de création à afficher ; <c>null</c> si le dernier geste a réussi.</summary>
    [ObservableProperty]
    private string? _addError;

    /// <summary>
    /// Un seul geste depuis un dossier choisi par l'utilisateur (le sélecteur vit
    /// dans la vue) : si le dossier porte déjà un <c>.cursus/</c>, on l'inscrit et
    /// on l'ouvre ; sinon on propose de <b>créer</b> le projet ici plutôt que de
    /// refuser. Le refus « ce n'est pas un projet » du registre devient donc une
    /// bifurcation vers la création, pas un message d'erreur — plus ergonomique
    /// qu'un bouton par cas.
    /// </summary>
    public void OpenOrCreateProject(string projectRoot)
    {
        try
        {
            _registry.Add(projectRoot);
            SyncProjects();
            AddError = null;

            // On ouvre le projet inscrit — même geste réactif que la création.
            var full = Path.GetFullPath(projectRoot);
            SelectedProject = Projects.FirstOrDefault(row => row.Project.Root == full);
        }
        catch (ProjectNotFoundException)
        {
            BeginCreateProject(projectRoot);
        }
    }

    // --- créer un projet : flux en deux temps (sélecteur de dossier → champ nom) ---

    private string? _pendingCreateRoot;

    /// <summary>Vrai quand le rail montre le champ de nom d'un projet en cours de création.</summary>
    [ObservableProperty]
    private bool _isCreatingProject;

    /// <summary>Le nom saisi pour le projet à créer, pré-rempli du nom du dossier choisi.</summary>
    [ObservableProperty]
    private string _createNameDraft = "";

    /// <summary>
    /// Le sélecteur de dossier (dans la vue) a rendu un chemin : on ouvre l'état de
    /// création en attente, en pré-remplissant le champ nom du nom feuille du
    /// dossier. L'utilisateur ajuste, puis <see cref="ConfirmCreateProject"/> pose
    /// le <c>.cursus/</c>. Le sélecteur ne rend qu'un chemin — la frontière vue/VM.
    /// </summary>
    public void BeginCreateProject(string projectRoot)
    {
        _pendingCreateRoot = Path.GetFullPath(projectRoot);
        // Le sélecteur rend souvent un chemin à séparateur final (« …/Projet/ ») :
        // sans le couper, GetFileName renvoie une chaîne vide et le champ reste nu.
        CreateNameDraft = Path.GetFileName(Path.TrimEndingDirectorySeparator(_pendingCreateRoot));
        IsCreatingProject = true;
        AddError = null;
    }

    /// <summary>
    /// Confirme la création : pose un <c>.cursus/</c> neuf, inscrit le projet et
    /// l'ouvre. Un dossier qui porte déjà un projet fait lever
    /// <see cref="InvalidOperationException"/> par le noyau — on la traduit en
    /// message plutôt que de confler créer et ajouter.
    /// </summary>
    [RelayCommand]
    private void ConfirmCreateProject()
    {
        if (_pendingCreateRoot is null)
            return;

        var root = _pendingCreateRoot;
        var name = string.IsNullOrWhiteSpace(CreateNameDraft)
            ? Path.GetFileName(Path.TrimEndingDirectorySeparator(root))
            : CreateNameDraft;

        try
        {
            ProjectStore.Create(root, name);
            _registry.Add(root);
            SyncProjects();
            IsCreatingProject = false;
            _pendingCreateRoot = null;
            AddError = null;
            SelectedProject = Projects.FirstOrDefault(row => row.Project.Root == root);
        }
        catch (InvalidOperationException)
        {
            AddError = "Ce dossier porte déjà un projet Cursus — utilisez « Ajouter un projet ».";
            IsCreatingProject = false;
            _pendingCreateRoot = null;
        }
    }

    /// <summary>Abandonne la création en attente sans rien poser sur le disque.</summary>
    [RelayCommand]
    private void CancelCreateProject()
    {
        IsCreatingProject = false;
        _pendingCreateRoot = null;
    }

    /// <summary>
    /// Renomme un projet inscrit : réécrit son <c>project.json</c> et rafraîchit
    /// l'instantané du registre (via <see cref="ProjectRegistry.Rename"/>), puis
    /// adopte le projet frais sur la ligne — sans reconstruire le rail, la
    /// sélection courante survit. Un renommage à blanc est ignoré (le nom garde sa
    /// valeur). La règle « réécrire le nom sans toucher à l'identité » vit en Core.
    /// </summary>
    [RelayCommand]
    private void RenameProject(ProjectRowViewModel? row)
    {
        if (row is null || string.IsNullOrWhiteSpace(row.DraftName))
            return;

        var renamed = _registry.Rename(row.Project.Root, row.DraftName);
        row.Applied(renamed);
    }

    [RelayCommand]
    private void RemoveProject(ProjectRowViewModel? row)
    {
        if (row is null)
            return;

        _registry.Remove(row.Project.Root);
        SyncProjects();
    }

    private void SyncProjects()
    {
        Projects.Clear();
        foreach (var project in _registry.Projects)
            Projects.Add(new ProjectRowViewModel(project));
    }
}
