using Cursus.Core.Workflows;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// Tests d'I/O réels : ils lancent de vrais process. Adossés aux binaires POSIX
/// du système (macOS/Linux), la cible de Cursus.
/// </summary>
public class ProcessRunnerTests
{
    [Fact(DisplayName = "étant donné un binaire qui se termine avec le code 0, quand on l'exécute, alors le résultat est terminé avec le code 0")]
    public async Task A_process_exiting_zero_completes_with_exit_code_zero()
    {
        // arrange
        var spec = Sh("exit 0");

        // act
        var result = await new ProcessRunner().RunAsync(spec);

        // assert
        Assert.Equal(ScriptOutcome.Completed, result.Outcome);
        Assert.Equal(0, result.ExitCode);
    }

    [Fact(DisplayName = "étant donné un binaire qui se termine avec le code 3, quand on l'exécute, alors le résultat est terminé avec le code 3")]
    public async Task A_process_exiting_non_zero_reports_its_exit_code()
    {
        // arrange
        var spec = Sh("exit 3");

        // act
        var result = await new ProcessRunner().RunAsync(spec);

        // assert
        Assert.Equal(ScriptOutcome.Completed, result.Outcome);
        Assert.Equal(3, result.ExitCode);
    }

    [Fact(DisplayName = "étant donné un script qui écrit sur la sortie standard, quand on l'exécute, alors cette sortie est capturée")]
    public async Task Standard_output_is_captured()
    {
        // arrange
        var spec = Sh("echo bonjour");

        // act
        var result = await new ProcessRunner().RunAsync(spec);

        // assert
        Assert.Contains("bonjour", result.Stdout);
    }

    [Fact(DisplayName = "étant donné un script qui écrit sur la sortie d'erreur, quand on l'exécute, alors elle est capturée à part et la sortie standard reste vide")]
    public async Task Standard_error_is_captured_separately_from_standard_output()
    {
        // arrange
        var spec = Sh("echo panique >&2");

        // act
        var result = await new ProcessRunner().RunAsync(spec);

        // assert
        Assert.Contains("panique", result.Stderr);
        Assert.Equal("", result.Stdout);
    }

    [Fact(DisplayName = "étant donné un script qui écrit sur les deux sorties plus que le tampon d'un tube, quand on l'exécute, alors tout est capturé sans blocage", Timeout = 15000)]
    public async Task Both_streams_are_drained_concurrently_so_a_full_pipe_never_blocks()
    {
        // arrange — 300 Kio sur chaque sortie, très au-delà des 64 Kio d'un tube
        var spec = Sh("""
            dd if=/dev/zero bs=1024 count=300 2>/dev/null | tr '\0' 'b' >&2
            dd if=/dev/zero bs=1024 count=300 2>/dev/null | tr '\0' 'a'
            """);

        // act
        var result = await new ProcessRunner().RunAsync(spec);

        // assert
        Assert.Equal(300 * 1024, result.Stdout.Length);
        Assert.Equal(300 * 1024, result.Stderr.Length);
    }

    [Fact(DisplayName = "étant donné des arguments contenant espaces et guillemets, quand le script les réémet, alors ils sont transmis tels quels et restent séparés")]
    public async Task Arguments_are_passed_verbatim_as_argv_tokens()
    {
        // arrange
        var spec = new ScriptSpec("/bin/sh", ["-c", "printf '[%s]' \"$@\"", "sh", "a b", "c\"d"]);

        // act
        var result = await new ProcessRunner().RunAsync(spec);

        // assert
        Assert.Equal("[a b][c\"d]", result.Stdout);
    }

    [Fact(DisplayName = "étant donné un répertoire de travail fourni, quand le script liste son répertoire courant, alors il y voit le contenu de celui fourni")]
    public async Task The_process_runs_in_the_requested_working_directory()
    {
        // arrange — un témoin identifie le répertoire sans dépendre de la forme
        // du chemin (sur macOS, /var est un lien vers /private/var).
        var directory = Directory.CreateTempSubdirectory("cursus-run-").FullName;
        File.WriteAllText(Path.Combine(directory, "temoin.txt"), "");
        var spec = Sh("ls") with { WorkingDirectory = directory };

        // act
        var result = await new ProcessRunner().RunAsync(spec);

        // assert
        Assert.Equal("temoin.txt", result.Stdout.Trim());
    }

    [Fact(DisplayName = "étant donné une variable d'environnement fournie par le spec, quand le script la lit, alors elle vaut la valeur fournie")]
    public async Task Environment_entries_from_the_spec_reach_the_process()
    {
        // arrange
        var spec = Sh("printf '%s' \"$CURSUS_STEP\"") with
        {
            Environment = new Dictionary<string, string> { ["CURSUS_STEP"] = "build" },
        };

        // act
        var result = await new ProcessRunner().RunAsync(spec);

        // assert
        Assert.Equal("build", result.Stdout);
    }

    [Fact(DisplayName = "étant donné une variable présente sur l'hôte et non surchargée, quand le script la lit, alors elle est héritée")]
    public async Task Host_environment_is_inherited_when_not_overridden()
    {
        // arrange
        Environment.SetEnvironmentVariable("CURSUS_HOTE", "herite");
        var spec = Sh("printf '%s' \"$CURSUS_HOTE\"") with
        {
            Environment = new Dictionary<string, string> { ["AUTRE"] = "x" },
        };

        // act
        var result = await new ProcessRunner().RunAsync(spec);

        // assert
        Assert.Equal("herite", result.Stdout);
    }

    [Fact(DisplayName = "étant donné un exécutable introuvable, quand on l'exécute, alors le résultat est un échec de lancement et aucune exception ne remonte")]
    public async Task A_missing_executable_yields_a_launch_failure_rather_than_an_exception()
    {
        // arrange
        var spec = new ScriptSpec("/chemin/qui/nexiste/pas", []);

        // act
        var result = await new ProcessRunner().RunAsync(spec);

        // assert
        Assert.Equal(ScriptOutcome.LaunchFailed, result.Outcome);
        Assert.False(result.IsSuccess);
        Assert.NotEqual("", result.Stderr);
    }

    [Fact(DisplayName = "étant donné un script plus long que son délai maximum, quand on l'exécute, alors il est tué et le résultat est un dépassement de délai", Timeout = 15000)]
    public async Task A_process_outliving_its_timeout_is_killed_and_reported_as_timed_out()
    {
        // arrange
        var spec = Sh("sleep 30") with { Timeout = TimeSpan.FromMilliseconds(200) };

        // act
        var result = await new ProcessRunner().RunAsync(spec);

        // assert
        Assert.Equal(ScriptOutcome.TimedOut, result.Outcome);
        Assert.False(result.IsSuccess);
    }

    [Fact(DisplayName = "étant donné un script en cours et un jeton annulé, quand on l'exécute, alors l'appel lève une annulation et le process est tué", Timeout = 15000)]
    public async Task Cancelling_kills_the_process_and_surfaces_as_a_cancellation()
    {
        // arrange
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        var spec = Sh("sleep 30");

        // act / assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await new ProcessRunner().RunAsync(spec, cancellation.Token));
    }

    [Fact(DisplayName = "étant donné un script qui dure un temps mesurable, quand on l'exécute, alors la durée rapportée couvre au moins ce temps")]
    public async Task The_reported_duration_covers_the_time_the_process_ran()
    {
        // arrange
        var spec = Sh("sleep 0.3");

        // act
        var result = await new ProcessRunner().RunAsync(spec);

        // assert
        Assert.True(
            result.Duration >= TimeSpan.FromMilliseconds(300),
            $"durée rapportée : {result.Duration}");
    }

    // --- helpers ---

    private static ScriptSpec Sh(string script) => new("/bin/sh", ["-c", script]);
}
