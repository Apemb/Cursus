using System.Collections.Generic;

using CommunityToolkit.Mvvm.ComponentModel;

using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Editing;

namespace Cursus.App.ViewModels;

/// <summary>
/// La ligne d'une étape-script : elle ne porte que sa <b>commande</b> — binaire suivi
/// de ses arguments, en une ligne (1er token = binaire, via <see cref="CommandLine"/>).
/// Champ <b>local</b> édité librement, poussé au brouillon à la perte de focus, pour
/// qu'une frappe ne déclenche pas une re-projection qui recréerait la ligne en pleine
/// saisie. Non testé, comme toute la vue (§7.12).
/// </summary>
public sealed partial class ScriptStepRow : StepEditorRow
{
    public ScriptStepRow(WorkflowEditorViewModel editor, ScriptStep step, IReadOnlyList<string> stepIds)
        : base(editor, step, stepIds)
    {
        _command = CommandLine.Format(step.Script.FileName, step.Script.Arguments);
    }

    /// <summary>La commande : binaire suivi de ses arguments, en une ligne. Descend dans le brouillon à la perte de focus.</summary>
    [ObservableProperty]
    private string _command;

    // Dès que la commande change (perte de focus du TextBox), elle descend dans le
    // brouillon — pas de re-projection, la ligne garde son focus.
    partial void OnCommandChanged(string value) => Editor.UpdateScript(Id, Command);

    public override void Flush() => Editor.UpdateScript(Id, Command);
}
