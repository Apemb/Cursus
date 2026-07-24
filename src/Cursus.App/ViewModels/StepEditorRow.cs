using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Core.Workflows;

namespace Cursus.App.ViewModels;

/// <summary>
/// Une étape telle que l'éditeur la montre : ce qui est <b>commun à tout kind</b> —
/// son titre, son id, ses arêtes sortantes, et les gestes de tracé/suppression. Ce
/// qui <b>diffère par kind</b> vit dans les sous-types : le champ commande pour une
/// étape-script (<see cref="ScriptStepRow"/>), le modèle et le prompt pour une
/// étape-agent (<see cref="AgentStepRow"/>). Une ligne « script <b>ou</b> agent » est
/// une variante de <b>type</b> — pas un objet unique à champs nullables : c'est la
/// convention de modélisation du dépôt appliquée jusque dans la vue. Les gestes
/// délèguent au <see cref="WorkflowEditorViewModel"/> parent, seul à tenir le
/// brouillon. Non testé, comme toute la vue (§7.12).
/// </summary>
public abstract partial class StepEditorRow : ObservableObject
{
    /// <summary>Le parent qui tient le brouillon ; les sous-types y poussent leurs champs propres.</summary>
    protected readonly WorkflowEditorViewModel Editor;

    protected StepEditorRow(WorkflowEditorViewModel editor, StepDefinition step, IReadOnlyList<string> stepIds)
    {
        Editor = editor;
        Id = step.Id;
        Name = step.Name;
        TargetChoices = stepIds;
        _newEdgeTarget = stepIds.Count > 0 ? stepIds[0] : "";
        Edges = new ObservableCollection<EdgeEditorRow>(
            step.OutEdges.Select((edge, index) => new EdgeEditorRow(
                editor, step.Id, index, WorkflowEditorViewModel.GuardLabel(edge.Guard), edge.Target)));
    }

    /// <summary>
    /// Fabrique la ligne du bon kind — le dispatch par type appartient à la famille de
    /// lignes, pas à un <c>switch</c> dans le VM. Ajouter un kind, c'est allonger ce
    /// switch, comme au moteur on allonge la liste d'exécuteurs.
    /// </summary>
    public static StepEditorRow For(
        WorkflowEditorViewModel editor, StepDefinition step, IReadOnlyList<string> stepIds) => step switch
    {
        ScriptStep script => new ScriptStepRow(editor, script, stepIds),
        AgentStep agent => new AgentStepRow(editor, agent, stepIds),
        _ => throw new ArgumentOutOfRangeException(nameof(step), step.GetType(), "Kind d'étape inconnu."),
    };

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

    /// <summary>La garde choisie pour la prochaine arête à tracer.</summary>
    [ObservableProperty]
    private string _newEdgeGuard = "Succès";

    /// <summary>La cible choisie pour la prochaine arête à tracer.</summary>
    [ObservableProperty]
    private string _newEdgeTarget;

    /// <summary>
    /// Rapatrie les champs <b>locaux</b> de la ligne dans le brouillon — filet
    /// d'enregistrement contre le champ dont le binding n'aurait pas encore validé
    /// (les champs descendent d'ordinaire à la perte de focus). Chaque kind pousse les
    /// siens : la commande d'un côté, le modèle et le prompt de l'autre.
    /// </summary>
    public abstract void Flush();

    /// <summary>Supprime cette étape du graphe.</summary>
    [RelayCommand]
    private void Remove() => Editor.RemoveStep(Id);

    /// <summary>Trace une arête de cette étape vers la cible choisie sous la garde choisie.</summary>
    [RelayCommand]
    private void AddEdge() => Editor.AddEdge(Id, NewEdgeGuard, NewEdgeTarget);
}
