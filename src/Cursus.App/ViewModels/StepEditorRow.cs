using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Editing;

namespace Cursus.App.ViewModels;

/// <summary>
/// Une étape telle que l'éditeur la montre : son titre, son id, sa <b>commande</b>,
/// et ses arêtes sortantes. La commande est un champ <b>local</b> — édité librement,
/// poussé au brouillon à la perte de focus — pour qu'une frappe ne déclenche pas une
/// re-projection qui recréerait la ligne en pleine saisie. Une seule ligne porte le
/// binaire et ses arguments (1er token = binaire, via <see cref="CommandLine"/>) au
/// lieu de deux champs séparés. Les gestes délèguent au
/// <see cref="WorkflowEditorViewModel"/> parent, seul à tenir le brouillon. Non testé,
/// comme toute la vue (§7.12).
/// </summary>
public partial class StepEditorRow : ObservableObject
{
    private readonly WorkflowEditorViewModel _editor;

    public StepEditorRow(WorkflowEditorViewModel editor, StepDefinition step, IReadOnlyList<string> stepIds)
    {
        _editor = editor;
        Id = step.Id;
        Name = step.Name;
        _command = CommandLine.Format(step.Script.FileName, step.Script.Arguments);
        TargetChoices = stepIds;
        _newEdgeTarget = stepIds.Count > 0 ? stepIds[0] : "";
        Edges = new ObservableCollection<EdgeEditorRow>(
            step.OutEdges.Select((edge, index) => new EdgeEditorRow(
                editor, step.Id, index, WorkflowEditorViewModel.GuardLabel(edge.Guard), edge.Target)));
    }

    /// <summary>L'identifiant (slug), stable une fois posé — affiché sous le titre.</summary>
    public string Id { get; }

    /// <summary>Le titre court de l'étape.</summary>
    public string Name { get; }

    /// <summary>Les arêtes sortantes de l'étape.</summary>
    public ObservableCollection<EdgeEditorRow> Edges { get; }

    /// <summary>Les gardes proposées pour une nouvelle arête (Code = n reste hors de l'éditeur minimal).</summary>
    public IReadOnlyList<string> GuardChoices { get; } = ["Succès", "Échec", "Toujours"];

    /// <summary>Les cibles proposées : les identifiants d'étapes du graphe au moment de la projection.</summary>
    public IReadOnlyList<string> TargetChoices { get; }

    /// <summary>La commande : binaire suivi de ses arguments, en une ligne. Descend dans le brouillon dès la perte de focus.</summary>
    [ObservableProperty]
    private string _command;

    /// <summary>La garde choisie pour la prochaine arête à tracer.</summary>
    [ObservableProperty]
    private string _newEdgeGuard = "Succès";

    /// <summary>La cible choisie pour la prochaine arête à tracer.</summary>
    [ObservableProperty]
    private string _newEdgeTarget;

    // Dès que la commande change (perte de focus du TextBox), elle descend dans le
    // brouillon — plus de bouton « Appliquer » à ne pas oublier. Pas de re-projection
    // côté éditeur, donc la ligne garde son focus.
    partial void OnCommandChanged(string value) => _editor.UpdateScript(Id, Command);

    /// <summary>Supprime cette étape du graphe.</summary>
    [RelayCommand]
    private void Remove() => _editor.RemoveStep(Id);

    /// <summary>Trace une arête de cette étape vers la cible choisie sous la garde choisie.</summary>
    [RelayCommand]
    private void AddEdge() => _editor.AddEdge(Id, NewEdgeGuard, NewEdgeTarget);
}
