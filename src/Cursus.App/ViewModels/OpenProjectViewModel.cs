using System.Collections.Generic;

using Cursus.Core.Projects;

namespace Cursus.App.ViewModels;

/// <summary>
/// Un projet ouvert, tel qu'il occupe la surface de droite. Pour l'instant son
/// seul mode run : le nom du projet et la liste de ses workflows. Il accueillera
/// plus tard le sélecteur run/sessions et l'engrenage de configuration — d'où ce
/// nom, qui désigne le conteneur d'un projet ouvert et non le seul mode run.
/// Adaptateur mince : lister les workflows vit dans <see cref="WorkflowCatalog"/>
/// (Core), déjà testé ; ici on ne fait que l'exposer au binding.
/// </summary>
public sealed class OpenProjectViewModel
{
    public OpenProjectViewModel(Project project)
    {
        Name = project.Name;
        Workflows = new WorkflowCatalog(project).List();
    }

    /// <summary>Le libellé du projet, affiché en tête de la surface.</summary>
    public string Name { get; }

    /// <summary>Les workflows du projet ; leur <c>Id</c> (nom de fichier) est le libellé affiché.</summary>
    public IReadOnlyList<WorkflowEntry> Workflows { get; }
}
