using Cursus.Core.Workflows.Editing;

namespace Cursus.Core.Tests.Workflows.Editing;

/// <summary>
/// La traduction d'une ligne d'arguments humaine en <c>argv</c> et retour. Pure,
/// jumelle de <see cref="Slug"/> : l'éditeur y confie un champ texte, le runner en
/// reçoit des tokens. Le nerf est le <b>quoting</b> — un argument peut contenir des
/// espaces s'il est entre guillemets (le cas d'un <c>zsh -c "commande"</c>).
/// </summary>
public class ArgumentLineTests
{
    [Fact(DisplayName = "étant donné une ligne vide, quand on la découpe, alors on n'obtient aucun token")]
    public void An_empty_line_yields_no_token()
    {
        // act
        var tokens = ArgumentLine.Parse("");

        // assert
        Assert.Empty(tokens);
    }

    [Fact(DisplayName = "étant donné des mots séparés par des espaces, quand on les découpe, alors chacun est un token")]
    public void Words_separated_by_spaces_become_tokens()
    {
        // act
        var tokens = ArgumentLine.Parse("build test run");

        // assert
        Assert.Equal(["build", "test", "run"], tokens);
    }

    [Fact(DisplayName = "étant donné des espaces multiples entre les mots, quand on les découpe, alors les vides ne font pas de token")]
    public void Runs_of_spaces_do_not_yield_empty_tokens()
    {
        // act
        var tokens = ArgumentLine.Parse("  build   test  ");

        // assert
        Assert.Equal(["build", "test"], tokens);
    }

    [Fact(DisplayName = "étant donné une région entre guillemets doubles, quand on découpe, alors ses espaces sont préservés en un seul token")]
    public void A_double_quoted_region_keeps_its_spaces_as_one_token()
    {
        // act — le cas d'un zsh -c "commande avec des espaces"
        var tokens = ArgumentLine.Parse("-c \"dotnet build -warnaserror\"");

        // assert
        Assert.Equal(["-c", "dotnet build -warnaserror"], tokens);
    }

    [Fact(DisplayName = "étant donné une région entre guillemets simples, quand on découpe, alors ses espaces sont préservés en un seul token")]
    public void A_single_quoted_region_keeps_its_spaces_as_one_token()
    {
        // act
        var tokens = ArgumentLine.Parse("echo 'ls -al ./src'");

        // assert
        Assert.Equal(["echo", "ls -al ./src"], tokens);
    }

    [Fact(DisplayName = "étant donné une apostrophe dans une région à guillemets doubles, quand on découpe, alors elle est littérale")]
    public void A_single_quote_inside_double_quotes_is_literal()
    {
        // act
        var tokens = ArgumentLine.Parse("\"it's here\"");

        // assert
        Assert.Equal(["it's here"], tokens);
    }

    [Fact(DisplayName = "étant donné un guillemet double dans une région à guillemets simples, quand on découpe, alors il est littéral")]
    public void A_double_quote_inside_single_quotes_is_literal()
    {
        // act
        var tokens = ArgumentLine.Parse("'say \"hi\"'");

        // assert
        Assert.Equal(["say \"hi\""], tokens);
    }

    [Fact(DisplayName = "étant donné un guillemet ouvert non refermé, quand on découpe, alors il se clôt en fin de ligne")]
    public void An_unterminated_quote_closes_at_the_end()
    {
        // act — indulgent : l'utilisateur tape au fil de l'eau
        var tokens = ArgumentLine.Parse("-c \"dotnet build");

        // assert
        Assert.Equal(["-c", "dotnet build"], tokens);
    }

    [Fact(DisplayName = "étant donné aucun token, quand on formate, alors on obtient une ligne vide")]
    public void No_token_formats_to_an_empty_line()
    {
        // act
        var line = ArgumentLine.Format([]);

        // assert
        Assert.Equal("", line);
    }

    [Fact(DisplayName = "étant donné des tokens simples, quand on formate, alors ils sont joints par des espaces sans guillemets")]
    public void Plain_tokens_are_joined_by_spaces()
    {
        // act
        var line = ArgumentLine.Format(["build", "test"]);

        // assert
        Assert.Equal("build test", line);
    }

    [Fact(DisplayName = "étant donné un token contenant une espace, quand on formate, alors il est entouré de guillemets doubles")]
    public void A_token_with_a_space_is_double_quoted()
    {
        // act
        var line = ArgumentLine.Format(["-c", "dotnet build"]);

        // assert
        Assert.Equal("-c \"dotnet build\"", line);
    }

    [Fact(DisplayName = "étant donné un token contenant un guillemet double, quand on formate, alors il est entouré de guillemets simples")]
    public void A_token_with_a_double_quote_is_single_quoted()
    {
        // act — sans quoi le guillemet interne rouvrirait une région à la relecture
        var line = ArgumentLine.Format(["say \"hi\""]);

        // assert
        Assert.Equal("'say \"hi\"'", line);
    }

    [Theory(DisplayName = "étant donné des tokens quelconques, quand on formate puis redécoupe, alors on retrouve les tokens d'origine")]
    [InlineData("dotnet", "build", "-warnaserror")]
    [InlineData("-c", "dotnet build -warnaserror")]
    [InlineData("echo", "it's a test")]
    [InlineData("echo", "say \"hi\"")]
    [InlineData("")]
    public void Formatting_then_parsing_round_trips(params string[] tokens)
    {
        // act
        var round = ArgumentLine.Parse(ArgumentLine.Format(tokens));

        // assert
        Assert.Equal(tokens, round);
    }
}
