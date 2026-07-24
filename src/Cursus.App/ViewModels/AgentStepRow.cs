using System.Collections.Generic;

using CommunityToolkit.Mvvm.ComponentModel;

using Cursus.Core.Workflows;

namespace Cursus.App.ViewModels;

/// <summary>
/// La ligne d'une étape-agent : elle porte le <b>modèle</b> choisi (parmi ceux du seul
/// harness connu, <see cref="AgenticHarness.ClaudeCode"/>, en dur ici) et son
/// <b>prompt</b>. Le modèle descend au brouillon dès la sélection ; le prompt à la
/// perte de focus. Pas de champ commande — c'est l'agent qui décide quoi faire du
/// prompt. Non testé, comme toute la vue (§7.12).
/// </summary>
public sealed partial class AgentStepRow : StepEditorRow
{
    public AgentStepRow(WorkflowEditorViewModel editor, AgentStep step, IReadOnlyList<string> stepIds)
        : base(editor, step, stepIds)
    {
        _model = step.ModelId;
        _prompt = step.Prompt;
    }

    /// <summary>
    /// Les modèles offerts par le harness Claude Code — en dur, une seule instance connue.
    /// Le dropdown les affiche par leur libellé et retient leur identifiant.
    /// </summary>
    public IReadOnlyList<AgentModel> Models { get; } = AgenticHarness.ClaudeCode.Models;

    /// <summary>Le modèle choisi (identifiant). Descend dans le brouillon dès la sélection.</summary>
    [ObservableProperty]
    private string _model;

    /// <summary>Le prompt confié à l'agent. Descend dans le brouillon à la perte de focus.</summary>
    [ObservableProperty]
    private string _prompt;

    partial void OnModelChanged(string value) => Editor.UpdateModel(Id, Model);

    partial void OnPromptChanged(string value) => Editor.UpdatePrompt(Id, Prompt);

    public override void Flush()
    {
        Editor.UpdateModel(Id, Model);
        Editor.UpdatePrompt(Id, Prompt);
    }
}
