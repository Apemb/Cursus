using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cursus.Core.Sessions;

namespace Cursus.App.ViewModels;

/// <summary>
/// Adaptateur mince entre la vue et le <see cref="SessionWorkspace"/> (Core) :
/// expose l'état bindable et enrobe les actions en commandes pour XAML.
/// Toute la logique de sessions est testée dans Core.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    public SessionWorkspace Workspace { get; } = new();

    [RelayCommand]
    private void AddSession() => Workspace.AddShellSession();

    [RelayCommand]
    private void CloseSession(TerminalSession? session) => Workspace.CloseSession(session);
}
