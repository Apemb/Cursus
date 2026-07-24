using Cursus.Core.Workflows.Editing;

namespace Cursus.Core.Tests.Workflows.Editing;

/// <summary>
/// La traduction d'une ligne de commande en <c>(binaire, arguments)</c> et retour.
/// Pure, jumelle d'<see cref="ArgumentLine"/> dont elle réutilise le tokeniseur ; le
/// nerf propre est que le <b>premier</b> token a un rôle spécial — le binaire.
/// </summary>
public class CommandLineTests
{
    [Fact(DisplayName = "étant donné une ligne binaire puis arguments, quand on la parse, alors le 1er token est le binaire et le reste les arguments")]
    public void A_line_splits_into_a_binary_head_and_an_argument_tail()
    {
        // act
        var command = CommandLine.Parse("dotnet build -warnaserror");

        // assert
        Assert.Equal("dotnet", command.FileName);
        Assert.Equal(["build", "-warnaserror"], command.Arguments);
    }

    [Fact(DisplayName = "étant donné une ligne à un seul token, quand on la parse, alors le binaire est ce token et il n'y a aucun argument")]
    public void A_single_token_line_is_all_binary_and_no_arguments()
    {
        // act
        var command = CommandLine.Parse("dotnet");

        // assert
        Assert.Equal("dotnet", command.FileName);
        Assert.Empty(command.Arguments);
    }

    [Fact(DisplayName = "étant donné une ligne vide ou faite de blancs, quand on la parse, alors le binaire est vide et il n'y a aucun argument")]
    public void A_blank_line_yields_an_empty_binary_and_no_arguments()
    {
        // act — un brouillon sans commande encore saisie
        var command = CommandLine.Parse("   ");

        // assert
        Assert.Equal("", command.FileName);
        Assert.Empty(command.Arguments);
    }

    [Fact(DisplayName = "étant donné une ligne dont un argument entre guillemets contient des espaces, quand on la parse, alors cet argument reste un seul token")]
    public void A_quoted_argument_with_spaces_stays_one_token()
    {
        // act — le cas d'un /bin/sh -c "commande avec des espaces"
        var command = CommandLine.Parse("/bin/sh -c \"dotnet build -warnaserror\"");

        // assert
        Assert.Equal("/bin/sh", command.FileName);
        Assert.Equal(["-c", "dotnet build -warnaserror"], command.Arguments);
    }

    [Fact(DisplayName = "étant donné une ligne dont le binaire entre guillemets contient une espace, quand on la parse, alors le binaire est ce chemin sans les guillemets")]
    public void A_quoted_binary_with_a_space_is_unquoted()
    {
        // act
        var command = CommandLine.Parse("\"/chemin avec espace/outil\" arg");

        // assert
        Assert.Equal("/chemin avec espace/outil", command.FileName);
        Assert.Equal(["arg"], command.Arguments);
    }

    [Fact(DisplayName = "étant donné un binaire et des arguments, quand on les formate, alors le binaire est en tête, suivi des arguments")]
    public void Formatting_puts_the_binary_first_then_the_arguments()
    {
        // act
        var line = CommandLine.Format("dotnet", ["build", "-warnaserror"]);

        // assert
        Assert.Equal("dotnet build -warnaserror", line);
    }

    [Fact(DisplayName = "étant donné un binaire vide et aucun argument, quand on les formate, alors on obtient la chaîne vide")]
    public void Formatting_an_empty_binary_with_no_arguments_yields_the_empty_string()
    {
        // act — le pendant du brouillon sans commande : rien à afficher, pas des guillemets vides
        var line = CommandLine.Format("", []);

        // assert
        Assert.Equal("", line);
    }

    [Fact(DisplayName = "étant donné un binaire contenant une espace, quand on le formate, alors il est entouré de guillemets")]
    public void Formatting_a_binary_with_a_space_quotes_it()
    {
        // act
        var line = CommandLine.Format("/chemin avec espace/outil", ["arg"]);

        // assert
        Assert.Equal("\"/chemin avec espace/outil\" arg", line);
    }

    [Theory(DisplayName = "étant donné une commande représentable, quand on la formate puis la parse, alors on retrouve le binaire et les arguments à l'identique")]
    [InlineData("dotnet", new[] { "build", "-warnaserror" })]
    [InlineData("/bin/sh", new[] { "-c", "dotnet build -warnaserror" })]
    [InlineData("/chemin avec espace/outil", new[] { "arg" })]
    [InlineData("", new string[0])]
    public void Format_then_parse_round_trips(string fileName, string[] arguments)
    {
        // act
        var (roundtrippedFileName, roundtrippedArguments) = CommandLine.Parse(CommandLine.Format(fileName, arguments));

        // assert
        Assert.Equal(fileName, roundtrippedFileName);
        Assert.Equal(arguments, roundtrippedArguments);
    }
}
