using System.Diagnostics;

using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Execution;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// La stratégie de résolution du <c>PATH</c> : depuis l'app installée, le <c>PATH</c>
/// hérité de l'interface graphique est tronqué (§9.2-15), et <see cref="ProcessRunner"/>
/// ne lance aucun shell de login pour le ré-enrichir. Une étape déclarant un binaire
/// d'<c>asdf</c>/Homebrew échouerait alors en <c>LaunchFailed</c>. La stratégie enrichit
/// le <c>PATH</c> de racines connues — en ajoutant, jamais en retirant.
/// </summary>
public sealed class PathStrategyTests
{
    [Fact(DisplayName = "étant donné un PATH privé d'une racine connue, quand la stratégie l'enrichit, alors la racine y figure sans perdre l'existant")]
    public void Enrich_appends_a_missing_known_root()
    {
        // arrange
        var strategy = new PathStrategy(["/opt/tools"]);

        // act
        var enriched = strategy.Enrich("/usr/bin").Split(Path.PathSeparator);

        // assert
        Assert.Contains("/opt/tools", enriched);
        Assert.Contains("/usr/bin", enriched);
    }

    [Fact(DisplayName = "étant donné une racine déjà sur le PATH, quand on enrichit, alors elle n'y est pas dupliquée")]
    public void Enrich_does_not_duplicate_a_present_root()
    {
        // arrange
        var strategy = new PathStrategy(["/usr/bin"]);

        // act
        var enriched = strategy.Enrich("/usr/bin:/bin").Split(Path.PathSeparator);

        // assert
        Assert.Single(enriched, entry => entry == "/usr/bin");
    }

    [Fact(DisplayName = "étant donné un ProcessRunner doté d'une racine connue et un binaire n'y étant accessible que par elle, quand une étape le déclare par son nom nu, alors elle s'exécute au lieu d'échouer au lancement")]
    public async Task A_runner_resolves_a_bare_command_through_a_known_root()
    {
        // arrange — un exécutable rangé dans une racine hors du PATH fourni à l'étape
        var root = Directory.CreateTempSubdirectory("cursus-path-").FullName;
        var tool = Path.Combine(root, "cursus-probe");
        File.WriteAllText(tool, "#!/bin/sh\nexit 7\n");
        MakeExecutable(tool);

        var runner = new ProcessRunner(new PathStrategy([root]));
        var spec = new ScriptSpec(
            "cursus-probe", [],
            Environment: new Dictionary<string, string> { ["PATH"] = "/cursus-nowhere" });

        // act — seule la racine connue, ajoutée par la stratégie, peut résoudre « cursus-probe »
        var result = await runner.RunAsync(spec, Stream.Null, Stream.Null);

        // assert — le binaire a bien tourné (son code 7 le prouve), pas un LaunchFailed
        Assert.Equal(ScriptOutcome.Completed, result.Outcome);
        Assert.Equal(7, result.ExitCode);
    }

    [Fact(DisplayName = "étant donné une commande déjà donnée en chemin, quand on la résout, alors elle est rendue inchangée")]
    public void Resolve_leaves_an_explicit_path_untouched()
    {
        // arrange
        var strategy = new PathStrategy(["/opt/tools"]);

        // act / assert — l'auteur a donné un chemin, on ne le réinterprète pas
        Assert.Equal("/usr/bin/true", strategy.Resolve("/usr/bin/true", "/cursus-nowhere"));
    }

    [Fact(DisplayName = "étant donné une commande introuvable partout, quand on la résout, alors elle est rendue telle quelle (échec net au lancement)")]
    public void Resolve_returns_an_unresolved_command_verbatim()
    {
        // arrange
        var strategy = new PathStrategy(["/cursus-nowhere-either"]);

        // act / assert — ne rien inventer : ProcessRunner en fera un LaunchFailed clair
        Assert.Equal("cursus-absent", strategy.Resolve("cursus-absent", "/cursus-nowhere"));
    }

    private static void MakeExecutable(string path)
    {
        using var chmod = Process.Start("chmod", $"+x \"{path}\"")!;
        chmod.WaitForExit();
    }
}
