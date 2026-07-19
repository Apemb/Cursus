using Cursus.Core.Sessions;
using Xunit;

namespace Cursus.Core.Tests;

public class SessionWorkspaceTests
{
    [Fact(DisplayName = "étant donné un workspace vide, quand on ajoute une session shell, alors il en contient une et elle est sélectionnée")]
    public void Adding_a_session_to_an_empty_workspace_selects_it()
    {
        // arrange
        var workspace = new SessionWorkspace();

        // act
        workspace.AddShellSession();

        // assert
        Assert.Single(workspace.Sessions);
        Assert.Same(workspace.Sessions[0], workspace.SelectedSession);
    }

    [Fact(DisplayName = "étant donné une session existante, quand on en ajoute une autre, alors la nouvelle est sélectionnée")]
    public void Adding_a_second_session_selects_the_new_one()
    {
        // arrange
        var workspace = new SessionWorkspace();
        workspace.AddShellSession();

        // act
        var second = workspace.AddShellSession();

        // assert
        Assert.Equal(2, workspace.Sessions.Count);
        Assert.Same(second, workspace.SelectedSession);
    }

    [Fact(DisplayName = "étant donné deux ajouts, quand on lit les titres, alors ils s'incrémentent (Session 1, Session 2)")]
    public void Adding_sessions_increments_their_title_number()
    {
        // arrange
        var workspace = new SessionWorkspace();

        // act
        var first = workspace.AddShellSession();
        var second = workspace.AddShellSession();

        // assert
        Assert.Equal("Session 1", first.Title);
        Assert.Equal("Session 2", second.Title);
    }

    [Fact(DisplayName = "étant donné la session sélectionnée, quand on la ferme, alors elle est retirée et la sélection passe à la voisine")]
    public void Closing_the_selected_session_selects_a_neighbour()
    {
        // arrange
        var workspace = new SessionWorkspace();
        var first = workspace.AddShellSession();
        var second = workspace.AddShellSession();
        workspace.SelectedSession = first;

        // act
        workspace.CloseSession(first);

        // assert
        Assert.DoesNotContain(first, workspace.Sessions);
        Assert.Same(second, workspace.SelectedSession);
    }

    [Fact(DisplayName = "étant donné une session non sélectionnée, quand on la ferme, alors la sélection est inchangée")]
    public void Closing_an_unselected_session_keeps_the_selection()
    {
        // arrange
        var workspace = new SessionWorkspace();
        var first = workspace.AddShellSession();
        var second = workspace.AddShellSession(); // sélectionnée

        // act
        workspace.CloseSession(first);

        // assert
        Assert.Same(second, workspace.SelectedSession);
        Assert.DoesNotContain(first, workspace.Sessions);
    }

    [Fact(DisplayName = "étant donné l'unique session, quand on la ferme, alors la sélection devient nulle")]
    public void Closing_the_last_session_clears_the_selection()
    {
        // arrange
        var workspace = new SessionWorkspace();
        var only = workspace.AddShellSession();

        // act
        workspace.CloseSession(only);

        // assert
        Assert.Empty(workspace.Sessions);
        Assert.Null(workspace.SelectedSession);
    }

    [Fact(DisplayName = "étant donné null ou une session absente, quand on ferme, alors rien ne change")]
    public void Closing_null_or_unknown_session_changes_nothing()
    {
        // arrange
        var workspace = new SessionWorkspace();
        var only = workspace.AddShellSession();
        var stranger = new TerminalSession("Étranger", "/bin/zsh", "/tmp");

        // act
        workspace.CloseSession(null);
        workspace.CloseSession(stranger);

        // assert
        Assert.Single(workspace.Sessions);
        Assert.Same(only, workspace.SelectedSession);
    }
}
