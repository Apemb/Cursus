using Cursus.Core.Sessions;
using Xunit;

namespace Cursus.Core.Tests;

public class ShellResolverTests
{
    [Fact(DisplayName = "étant donné SHELL défini sur un chemin existant, quand on résout, alors on obtient ce chemin")]
    public void Resolves_to_shell_env_when_it_points_to_an_existing_path()
    {
        // arrange
        static bool FileExists(string path) => path == "/usr/bin/fish";

        // act
        var shell = ShellResolver.Resolve("/usr/bin/fish", FileExists);

        // assert
        Assert.Equal("/usr/bin/fish", shell);
    }

    [Fact(DisplayName = "étant donné SHELL vide, quand on résout, alors on obtient /bin/zsh s'il existe")]
    public void Falls_back_to_zsh_when_shell_env_is_blank()
    {
        // arrange
        static bool FileExists(string path) => path == "/bin/zsh";

        // act
        var shell = ShellResolver.Resolve("", FileExists);

        // assert
        Assert.Equal("/bin/zsh", shell);
    }

    [Fact(DisplayName = "étant donné SHELL défini sur un chemin inexistant, quand on résout, alors on retombe sur /bin/zsh existant")]
    public void Falls_back_to_zsh_when_shell_env_path_does_not_exist()
    {
        // arrange
        static bool FileExists(string path) => path == "/bin/zsh";

        // act
        var shell = ShellResolver.Resolve("/does/not/exist", FileExists);

        // assert
        Assert.Equal("/bin/zsh", shell);
    }

    [Fact(DisplayName = "étant donné SHELL vide et /bin/zsh absent, quand on résout, alors on obtient /bin/bash")]
    public void Falls_back_to_bash_when_no_zsh_available()
    {
        // arrange
        static bool FileExists(string path) => false;

        // act
        var shell = ShellResolver.Resolve(null, FileExists);

        // assert
        Assert.Equal("/bin/bash", shell);
    }
}
