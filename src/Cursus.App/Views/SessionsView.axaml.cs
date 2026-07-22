using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

using Avalonia.Controls;

using Cursus.App.Terminals;
using Cursus.App.ViewModels;
using Cursus.Core.Sessions;

using RoyalTerminal.Avalonia.Controls;

namespace Cursus.App.Views;

/// <summary>
/// La surface des sessions terminal, extraite telle quelle de l'ancienne
/// fenêtre : son <c>DataContext</c> est un <see cref="MainViewModel"/>, donc
/// toute la plomberie PTY et les bindings sont inchangés. Le contrôle terminal
/// <b>est</b> son état (process, moteur VT, scrollback) ; il vit dans l'arbre
/// visuel et survit à son invisibilité (façon TMUX), jamais recréé.
/// </summary>
public partial class SessionsView : UserControl
{
    // Un contrôle terminal vivant par session, gardé en vie même masqué.
    private readonly Dictionary<Guid, TerminalControl> _terminals = new();
    private SessionWorkspace? _workspace;

    public SessionsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_workspace is not null)
        {
            _workspace.Sessions.CollectionChanged -= OnSessionsChanged;
            _workspace.PropertyChanged -= OnWorkspacePropertyChanged;
        }

        _workspace = (DataContext as MainViewModel)?.Workspace;

        if (_workspace is not null)
        {
            _workspace.Sessions.CollectionChanged += OnSessionsChanged;
            _workspace.PropertyChanged += OnWorkspacePropertyChanged;
            UpdateVisibleTerminal();
        }
    }

    private void OnSessionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
            foreach (TerminalSession session in e.NewItems)
                EnsureTerminal(session);

        if (e.OldItems is not null)
            foreach (TerminalSession session in e.OldItems)
                RemoveTerminal(session);

        UpdateVisibleTerminal();
    }

    private void OnWorkspacePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SessionWorkspace.SelectedSession))
            UpdateVisibleTerminal();
    }

    private void EnsureTerminal(TerminalSession session)
    {
        if (_terminals.ContainsKey(session.Id))
            return;

        var terminal = NativeTerminalFactory.Create();
        terminal.FontFamilyName = "Menlo";
        terminal.TerminalFontSize = 13;
        terminal.IsVisible = false;

        // Le PTY démarre au premier affichage réel (bounds connues), une seule fois.
        var started = false;
        terminal.Loaded += (_, _) =>
        {
            if (started)
                return;
            started = true;
            terminal.StartPty(session.ShellPath, session.WorkingDirectory, new[] { "-l" });
        };

        _terminals[session.Id] = terminal;
        TerminalHost.Children.Add(terminal);
    }

    private void RemoveTerminal(TerminalSession session)
    {
        if (!_terminals.TryGetValue(session.Id, out var terminal))
            return;

        try
        {
            terminal.StopPty();
        }
        catch
        {
            // Session déjà terminée : rien à faire.
        }

        TerminalHost.Children.Remove(terminal);
        _terminals.Remove(session.Id);
    }

    private void UpdateVisibleTerminal()
    {
        var selected = _workspace?.SelectedSession;

        foreach (var (id, terminal) in _terminals)
            terminal.IsVisible = selected is not null && id == selected.Id;

        EmptyPlaceholder.IsVisible = selected is null;

        if (selected is not null && _terminals.TryGetValue(selected.Id, out var active))
            active.Focus();
    }
}
