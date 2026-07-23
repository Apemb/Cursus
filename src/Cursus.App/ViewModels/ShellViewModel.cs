using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Core.Projects;

namespace Cursus.App.ViewModels;

/// <summary>
/// La coquille : le rail des projets connus à gauche, une surface à droite (pour
/// l'instant les sessions terminal). Adaptateur mince sur <see cref="ProjectRegistry"/> —
/// toute la logique de registre vit en Core ; ici on ne fait que binder et
/// traduire un refus en message.
/// </summary>
public partial class ShellViewModel : ObservableObject
{
    private readonly ProjectRegistry _registry;

    public ShellViewModel(ProjectRegistry registry)
    {
        _registry = registry;
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
    /// La jonction rail → surface : choisir un projet l'ouvre sur son mode run,
    /// le désélectionner rend la surface vide. Une sélection neuve reconstruit la
    /// surface — pas de recyclage, l'objet ouvert est jetable.
    /// </summary>
    partial void OnSelectedProjectChanged(Project? value) =>
        CurrentSurface = value is null ? null : new OpenProjectViewModel(value);

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
