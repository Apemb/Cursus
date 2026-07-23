using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Cursus.App.Views;

/// <summary>
/// L'écran d'un run : trajectoire déroulée en haut, log de la visite sélectionnée
/// en bas. Vue passive (§7.12) — toute la matière vit dans <c>RunViewModel</c>,
/// dont elle hérite le DataContext.
/// </summary>
public partial class RunView : UserControl
{
    public RunView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
