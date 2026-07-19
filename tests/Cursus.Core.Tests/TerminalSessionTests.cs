using Cursus.Core.Sessions;
using Xunit;

namespace Cursus.Core.Tests;

public class TerminalSessionTests
{
    [Fact(DisplayName = "étant donné un titre/shell/dossier explicites, quand on construit, alors les propriétés correspondent et Kind vaut Shell")]
    public void Constructs_with_the_provided_values_and_defaults_to_shell_kind()
    {
        // act
        var session = new TerminalSession("Build", "/bin/zsh", "/Users/moi");

        // assert
        Assert.Equal("Build", session.Title);
        Assert.Equal("/bin/zsh", session.ShellPath);
        Assert.Equal("/Users/moi", session.WorkingDirectory);
        Assert.Equal(SessionKind.Shell, session.Kind);
    }

    [Fact(DisplayName = "étant donné deux constructions, quand on compare les Id, alors ils diffèrent")]
    public void Assigns_a_unique_id_to_each_session()
    {
        // act
        var first = new TerminalSession("A", "/bin/zsh", "/tmp");
        var second = new TerminalSession("B", "/bin/zsh", "/tmp");

        // assert
        Assert.NotEqual(first.Id, second.Id);
    }
}
