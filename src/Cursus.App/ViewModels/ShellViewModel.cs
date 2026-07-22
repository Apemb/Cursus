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
    /// La surface courante. Sélectionner un projet ne la change pas encore — la
    /// jonction rail → surface est la marche suivante ; ici la sélection est
    /// seulement mémorisée.
    /// </summary>
    public MainViewModel Sessions { get; } = new();

    [ObservableProperty]
    private Project? _selectedProject;

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
