using CommunityToolkit.Mvvm.Input;

namespace Cursus.App.ViewModels;

/// <summary>
/// Une arête sortante telle que l'éditeur la montre : sa garde et sa cible en une
/// phrase, et le geste pour la retirer. La suppression délègue au
/// <see cref="WorkflowEditorViewModel"/> parent par la position de l'arête — le
/// seul discriminant non ambigu quand deux arêtes partagent une cible sous des
/// gardes distinctes. Non testé, comme toute la vue (§7.12).
/// </summary>
public partial class EdgeEditorRow
{
    private readonly WorkflowEditorViewModel _editor;
    private readonly string _fromId;
    private readonly int _index;

    public EdgeEditorRow(WorkflowEditorViewModel editor, string fromId, int index, string guardLabel, string target)
    {
        _editor = editor;
        _fromId = fromId;
        _index = index;
        Describe = $"{guardLabel} → {target}";
    }

    /// <summary>L'arête en une phrase : « Succès → deployer ».</summary>
    public string Describe { get; }

    /// <summary>Retire cette arête de son étape source, par sa position.</summary>
    [RelayCommand]
    private void Remove() => _editor.RemoveEdge(_fromId, _index);
}
