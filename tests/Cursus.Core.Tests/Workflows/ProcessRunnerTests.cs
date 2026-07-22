using System.Text;
using Cursus.Core.Workflows;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// Tests d'I/O réels : ils lancent de vrais process. Adossés aux binaires POSIX
/// du système (macOS/Linux), la cible de Cursus. La sortie ne revient plus dans
/// le résultat : elle ruisselle vers deux flux fournis, qu'on capture ici.
/// </summary>
public class ProcessRunnerTests
{
    [Fact(DisplayName = "étant donné un binaire qui se termine avec le code 0, quand on l'exécute, alors le résultat est terminé avec le code 0")]
    public async Task A_process_exiting_zero_completes_with_exit_code_zero()
    {
        // arrange
        var spec = Sh("exit 0");

        // act
        var capture = await Run(spec);

        // assert
        Assert.Equal(ScriptOutcome.Completed, capture.Result.Outcome);
        Assert.Equal(0, capture.Result.ExitCode);
    }

    [Fact(DisplayName = "étant donné un binaire qui se termine avec le code 3, quand on l'exécute, alors le résultat est terminé avec le code 3")]
    public async Task A_process_exiting_non_zero_reports_its_exit_code()
    {
        // arrange
        var spec = Sh("exit 3");

        // act
        var capture = await Run(spec);

        // assert
        Assert.Equal(ScriptOutcome.Completed, capture.Result.Outcome);
        Assert.Equal(3, capture.Result.ExitCode);
    }

    [Fact(DisplayName = "étant donné un script qui écrit sur la sortie standard, quand on l'exécute, alors le flux standard reçoit ces octets")]
    public async Task Standard_output_is_streamed()
    {
        // arrange
        var spec = Sh("echo bonjour");

        // act
        var capture = await Run(spec);

        // assert
        Assert.Contains("bonjour", capture.StdoutText);
    }

    [Fact(DisplayName = "étant donné un script qui écrit sur la sortie d'erreur, quand on l'exécute, alors le flux d'erreur la reçoit et le flux standard reste vide")]
    public async Task Standard_error_is_streamed_separately_from_standard_output()
    {
        // arrange
        var spec = Sh("echo panique >&2");

        // act
        var capture = await Run(spec);

        // assert
        Assert.Contains("panique", capture.StderrText);
        Assert.Empty(capture.Stdout);
    }

    [Fact(DisplayName = "étant donné un script qui écrit sur les deux sorties plus que le tampon d'un tube, quand on l'exécute, alors tout est reçu sans blocage et les tailles sont exactes à l'octet", Timeout = 15000)]
    public async Task Both_streams_are_drained_concurrently_so_a_full_pipe_never_blocks()
    {
        // arrange — 300 Kio sur chaque sortie, très au-delà des 64 Kio d'un tube
        var spec = Sh("""
            dd if=/dev/zero bs=1024 count=300 2>/dev/null | tr '\0' 'b' >&2
            dd if=/dev/zero bs=1024 count=300 2>/dev/null | tr '\0' 'a'
            """);

        // act
        var capture = await Run(spec);

        // assert
        Assert.Equal(300 * 1024, capture.Stdout.Length);
        Assert.Equal(300 * 1024, capture.Stderr.Length);
    }

    [Fact(DisplayName = "étant donné des arguments contenant espaces et guillemets, quand le script les réémet, alors ils sont transmis tels quels et restent séparés")]
    public async Task Arguments_are_passed_verbatim_as_argv_tokens()
    {
        // arrange
        var spec = new ScriptSpec("/bin/sh", ["-c", "printf '[%s]' \"$@\"", "sh", "a b", "c\"d"]);

        // act
        var capture = await Run(spec);

        // assert
        Assert.Equal("[a b][c\"d]", capture.StdoutText);
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
        var capture = await Run(spec);

        // assert
        Assert.Equal("temoin.txt", capture.StdoutText.Trim());
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
        var capture = await Run(spec);

        // assert
        Assert.Equal("build", capture.StdoutText);
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
        var capture = await Run(spec);

        // assert
        Assert.Equal("herite", capture.StdoutText);
    }

    [Fact(DisplayName = "étant donné un exécutable introuvable, quand on l'exécute, alors le résultat est un échec de lancement et le flux d'erreur reçoit le message")]
    public async Task A_missing_executable_yields_a_launch_failure_rather_than_an_exception()
    {
        // arrange
        var spec = new ScriptSpec("/chemin/qui/nexiste/pas", []);

        // act
        var capture = await Run(spec);

        // assert
        Assert.Equal(ScriptOutcome.LaunchFailed, capture.Result.Outcome);
        Assert.False(capture.Result.IsSuccess);
        Assert.NotEmpty(capture.Stderr);
    }

    [Fact(DisplayName = "étant donné un script plus long que son délai maximum, quand on l'exécute, alors il est tué et le résultat est un dépassement de délai", Timeout = 15000)]
    public async Task A_process_outliving_its_timeout_is_killed_and_reported_as_timed_out()
    {
        // arrange
        var spec = Sh("sleep 30") with { Timeout = TimeSpan.FromMilliseconds(200) };

        // act
        var capture = await Run(spec);

        // assert
        Assert.Equal(ScriptOutcome.TimedOut, capture.Result.Outcome);
        Assert.False(capture.Result.IsSuccess);
    }

    [Fact(DisplayName = "étant donné un script en cours et un jeton annulé, quand on l'exécute, alors l'appel lève une annulation et le process est tué", Timeout = 15000)]
    public async Task Cancelling_kills_the_process_and_surfaces_as_a_cancellation()
    {
        // arrange
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        var spec = Sh("sleep 30");

        // act / assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await Run(spec, cancellation.Token));
    }

    [Fact(DisplayName = "étant donné un script qui dure un temps mesurable, quand on l'exécute, alors la durée rapportée couvre au moins ce temps")]
    public async Task The_reported_duration_covers_the_time_the_process_ran()
    {
        // arrange
        var spec = Sh("sleep 0.3");

        // act
        var capture = await Run(spec);

        // assert
        Assert.True(
            capture.Result.Duration >= TimeSpan.FromMilliseconds(300),
            $"durée rapportée : {capture.Result.Duration}");
    }

    [Fact(DisplayName = "étant donné un script qui écrit une marque puis attend un signal, quand on l'exécute, alors la marque est lisible dans sa sortie avant que le process se termine", Timeout = 15000)]
    public async Task Output_is_readable_while_the_process_is_still_running()
    {
        // arrange — le script émet une marque, puis bloque jusqu'à l'apparition
        // d'un fichier sentinelle : c'est le test qui libère, pas une course au sleep.
        var directory = Directory.CreateTempSubdirectory("cursus-stream-").FullName;
        var release = Path.Combine(directory, "release");
        var spec = new ScriptSpec("/bin/sh", ["-c", $"echo MARQUE; while [ ! -f '{release}' ]; do sleep 0.02; done"]);
        var stdout = new MarkerStream("MARQUE");

        // act — on lance sans attendre, on observe la marque, puis on libère
        var run = new ProcessRunner().RunAsync(spec, stdout, Stream.Null);
        await stdout.Found;
        var seenWhileRunning = !run.IsCompleted;
        File.WriteAllText(release, "");
        var result = await run;

        // assert
        Assert.True(seenWhileRunning, "la marque doit être lisible alors que le process tourne encore");
        Assert.Equal(ScriptOutcome.Completed, result.Outcome);
    }

    // --- helpers ---

    private static ScriptSpec Sh(string script) => new("/bin/sh", ["-c", script]);

    private static async Task<Capture> Run(ScriptSpec spec, CancellationToken cancellationToken = default)
    {
        using var stdout = new MemoryStream();
        using var stderr = new MemoryStream();
        var result = await new ProcessRunner().RunAsync(spec, stdout, stderr, cancellationToken);
        return new Capture(result, stdout.ToArray(), stderr.ToArray());
    }

    private sealed record Capture(ScriptResult Result, byte[] Stdout, byte[] Stderr)
    {
        public string StdoutText => Encoding.UTF8.GetString(Stdout);

        public string StderrText => Encoding.UTF8.GetString(Stderr);
    }

    /// <summary>
    /// Flux en écriture qui signale dès qu'une marque a été reçue — de quoi
    /// observer la sortie pendant que le process tourne encore.
    /// </summary>
    private sealed class MarkerStream : Stream
    {
        private readonly List<byte> _seen = [];
        private readonly byte[] _marker;
        private readonly TaskCompletionSource _found = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public MarkerStream(string marker) => _marker = Encoding.UTF8.GetBytes(marker);

        public Task Found => _found.Task;

        public override void Write(byte[] buffer, int offset, int count) =>
            Absorb(buffer.AsSpan(offset, count));

        public override void Write(ReadOnlySpan<byte> buffer) => Absorb(buffer);

        private void Absorb(ReadOnlySpan<byte> buffer)
        {
            lock (_seen)
            {
                foreach (var b in buffer)
                    _seen.Add(b);

                if (Contains(_seen, _marker))
                    _found.TrySetResult();
            }
        }

        private static bool Contains(List<byte> haystack, byte[] needle)
        {
            for (var start = 0; start + needle.Length <= haystack.Count; start++)
            {
                var match = true;
                for (var i = 0; i < needle.Length; i++)
                {
                    if (haystack[start + i] != needle[i])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return true;
            }

            return false;
        }

        public override void Flush()
        {
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
