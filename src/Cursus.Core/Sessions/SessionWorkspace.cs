using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cursus.Core.Sessions;

/// <summary>
/// Détient la collection de sessions et la sélection courante, et applique
/// la politique d'ajout / fermeture. UI-agnostique (aucune dépendance
/// Avalonia) : c'est ici que vivra la future gestion des sessions d'agents.
/// </summary>
public partial class SessionWorkspace : ObservableObject
{
    [ObservableProperty]
    private TerminalSession? _selectedSession;

    private int _counter;

    public ObservableCollection<TerminalSession> Sessions { get; } = new();

    public TerminalSession AddShellSession()
    {
        var session = TerminalSession.CreateShell($"Session {++_counter}");
        Sessions.Add(session);
        SelectedSession = session;
        return session;
    }

    public void CloseSession(TerminalSession? session)
    {
        if (session is null)
            return;

        var index = Sessions.IndexOf(session);
        if (index < 0)
            return;

        Sessions.Remove(session);

        if (SelectedSession == session)
            SelectedSession = Sessions.Count > 0
                ? Sessions[Math.Min(index, Sessions.Count - 1)]
                : null;
    }
}
