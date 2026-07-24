using System;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Core.Projects;

namespace Cursus.App.ViewModels;

/// <summary>
/// La coquille : le rail des projets connus à gauche, une surface à droite (le
/// projet ouvert). Adaptateur mince sur <see cref="ProjectRegistry"/> — toute la
/// logique de registre vit en Core ; ici on ne fait que binder et traduire un
/// refus en message.
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
    private ProjectWorkspace? _currentWorkspace;

    public ShellViewModel(ProjectRegistry registry, Func<Project, ProjectWorkspace> openWorkspace)
    {
        _registry = registry;
        _openWorkspace = openWorkspace;
        Projects = new ObservableCollection<Project>(registry.Projects);
    }

    /// <summary>Les projets du rail, reflet observable de la liste du registre.</summary>
    public ObservableCollection<Project> Projects { get; }

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
    private Project? _selectedProject;

    /// <summary>
    /// La jonction rail → surface : choisir un projet ouvre son host, lit le
    /// dernier passage de ses workflows et en fait la surface ; le désélectionner
    /// la vide. Le host du projet précédent est disposé — une connexion SQLite par
    /// projet, jamais deux — et la surface reconstruite, l'objet ouvert est jetable.
    /// </summary>
    partial void OnSelectedProjectChanged(Project? value)
    {
        _currentWorkspace?.Dispose();

        if (value is null)
        {
            _currentWorkspace = null;
            CurrentSurface = null;
            return;
        }

        var workspace = _currentWorkspace = _openWorkspace(value);

        // Les fabriques de run capturent le host et le magasin d'artefacts du
        // projet : la surface obtient des RunViewModel déjà câblés, sans jamais
        // toucher au host elle-même (règle de sens unique).
        CurrentSurface = new OpenProjectViewModel(
            value.Name,
            workspace.Catalog,
            workspace.Host.LastRunPerWorkflow,
            workflowId => RunViewModel.StartLive(workflowId, workspace.Host, workspace.Artifacts),
            row => RunViewModel.Replay(row.LastRun!, workspace.Host, workspace.Artifacts));
    }

    /// <summary>Ferme le workspace encore ouvert : sa connexion SQLite ne doit pas fuir à la fermeture.</summary>
    public void Dispose() => _currentWorkspace?.Dispose();

    /// <summary>Le dernier refus d'ajout à afficher ; <c>null</c> si le dernier ajout a réussi.</summary>
    [ObservableProperty]
    private string? _addError;

    /// <summary>
    /// Inscrit un projet depuis un chemin choisi par l'utilisateur (le sélecteur
    /// de dossier vit dans la vue). Le refus « ce n'est pas un projet Cursus »
    /// remonte du registre : on le traduit en message plutôt que de laisser
    /// l'exception filer.
    /// </summary>
    public void AddProject(string projectRoot)
    {
        try
        {
            _registry.Add(projectRoot);
            SyncProjects();
            AddError = null;
        }
        catch (ProjectNotFoundException)
        {
            AddError = "Ce dossier n'est pas un projet Cursus (aucun .cursus/ trouvé).";
        }
    }

    [RelayCommand]
    private void RemoveProject(Project? project)
    {
        if (project is null)
            return;

        _registry.Remove(project.Root);
        SyncProjects();
    }

    private void SyncProjects()
    {
        Projects.Clear();
        foreach (var project in _registry.Projects)
            Projects.Add(project);
    }
}
