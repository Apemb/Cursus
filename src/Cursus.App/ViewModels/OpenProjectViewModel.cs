using System.Collections.Generic;
using System.Linq;

using Cursus.Core.Projects;

namespace Cursus.App.ViewModels;

/// <summary>
/// Un projet ouvert, tel qu'il occupe la surface de droite. Pour l'instant son
/// seul mode run : le nom du projet et ses workflows, chacun avec la trace de son
/// dernier passage. Il accueillera plus tard le sélecteur run/sessions et
/// l'engrenage de configuration — d'où ce nom, qui désigne le conteneur d'un
/// projet ouvert et non le seul mode run.
///
/// <para>
/// Adaptateur mince : la jointure workflows × runs vit dans <see cref="ProjectHost"/>
/// (Core), déjà testée ; ici on ne fait que traduire chaque
/// <see cref="WorkflowLastRun"/> en une ligne bindable — dont le libellé de
/// verdict, seul arbitrage qui appartient à la présentation.
/// </para>
/// </summary>
public sealed class OpenProjectViewModel
{
    public OpenProjectViewModel(string name, IReadOnlyList<WorkflowLastRun> workflows)
    {
        Name = name;
        Workflows = workflows.Select(workflow => new WorkflowRowViewModel(workflow)).ToList();
    }

    /// <summary>Le libellé du projet, affiché en tête de la surface.</summary>
    public string Name { get; }

    /// <summary>Les workflows du projet, chacun avec son dernier passage.</summary>
    public IReadOnlyList<WorkflowRowViewModel> Workflows { get; }
}
