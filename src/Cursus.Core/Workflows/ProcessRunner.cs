using System.ComponentModel;
using System.Diagnostics;

namespace Cursus.Core.Workflows;

/// <summary>
/// Frontière I/O du noyau déterministe : lance un process décrit par un
/// <see cref="ScriptSpec"/>, sorties redirigées (pas de PTY), et en rapporte le
/// résultat. Rien d'autre dans ce namespace n'appelle <c>Process.Start</c>.
/// </summary>
public sealed class ProcessRunner : IProcessRunner
{
    /// <summary>Code de sortie rapporté quand l'exécutable n'a pas pu être lancé (convention shell).</summary>
    private const int CommandNotFound = 127;

    public async Task<ScriptResult> RunAsync(ScriptSpec spec, CancellationToken cancellationToken = default)
    {
        using var process = Describe(spec);

        var chrono = Stopwatch.StartNew();
        try
        {
            process.Start();
        }
        catch (Win32Exception failure)
        {
            // Un binaire introuvable est un résultat d'étape ordinaire — la garde
            // OnFailure le routera — pas une exception que le moteur devrait gérer.
            return new ScriptResult(CommandNotFound, ScriptOutcome.LaunchFailed, Stderr: failure.Message);
        }

        // Les deux tubes sont vidés en parallèle : lire l'un jusqu'au bout avant
        // l'autre bloquerait le process dès que le tube non lu est plein (64 Kio).
        // Aucun jeton ici : à la mort du process les tubes se ferment, les
        // lectures s'achèvent d'elles-mêmes et rendent la sortie partielle.
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();

        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (spec.Timeout is { } timeout)
            deadline.CancelAfter(timeout);

        var outcome = ScriptOutcome.Completed;
        try
        {
            await process.WaitForExitAsync(deadline.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync(CancellationToken.None);

            // Une annulation demandée par l'appelant interrompt le run : elle
            // remonte. Un dépassement de délai, lui, est une issue d'exécution
            // ordinaire, que la garde OnFailure routera.
            cancellationToken.ThrowIfCancellationRequested();
            outcome = ScriptOutcome.TimedOut;
        }

        return new ScriptResult(process.ExitCode, outcome, await stdout, await stderr, chrono.Elapsed);
    }

    /// <summary>
    /// Traduit un <see cref="ScriptSpec"/> en process prêt à démarrer : argv passé
    /// token par token (aucun quoting), sorties redirigées, environnement hôte
    /// surchargé clé par clé.
    /// </summary>
    private static Process Describe(ScriptSpec spec)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo(spec.FileName)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = spec.WorkingDirectory ?? "",
            },
        };

        foreach (var argument in spec.Arguments)
            process.StartInfo.ArgumentList.Add(argument);

        foreach (var (key, value) in spec.Environment ?? new Dictionary<string, string>())
            process.StartInfo.Environment[key] = value;

        return process;
    }
}
