using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Cursus.Core.Workflows.Execution;

/// <summary>
/// Frontière I/O du noyau déterministe : lance un process décrit par un
/// <see cref="ScriptSpec"/>, sorties redirigées (pas de PTY), et en rapporte le
/// résultat. Rien d'autre dans ce namespace n'appelle <c>Process.Start</c>.
/// </summary>
public sealed class ProcessRunner : IProcessRunner
{
    /// <summary>Code de sortie rapporté quand l'exécutable n'a pas pu être lancé (convention shell).</summary>
    private const int CommandNotFound = 127;

    private readonly PathStrategy _pathStrategy;

    /// <param name="pathStrategy">
    /// De quoi résoudre un binaire malgré un <c>PATH</c> graphique tronqué (§9.2-15).
    /// À défaut, les racines connues de la machine (<see cref="PathStrategy.Default"/>) —
    /// inoffensif là où le <c>PATH</c> est déjà complet (dev, <c>dotnet test</c>).
    /// </param>
    public ProcessRunner(PathStrategy? pathStrategy = null) =>
        _pathStrategy = pathStrategy ?? PathStrategy.Default;

    public async Task<ScriptResult> RunAsync(
        ScriptSpec spec, Stream stdout, Stream stderr, CancellationToken cancellationToken = default)
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
            // Le pourquoi de l'échec devient le contenu stderr de la visite.
            await stderr.WriteAsync(Encoding.UTF8.GetBytes(failure.Message), CancellationToken.None);
            return new ScriptResult(CommandNotFound, ScriptOutcome.LaunchFailed);
        }

        // Copie brute des deux tubes, en parallèle : lire l'un jusqu'au bout avant
        // l'autre bloquerait le process dès que le tube non lu est plein (64 Kio).
        // On copie les octets tels quels (BaseStream), sans décision d'encodage.
        // Aucun jeton sur la copie : à la mort du process les tubes se ferment,
        // elle s'achève d'elle-même et rend la sortie partielle.
        var pumpOut = process.StandardOutput.BaseStream.CopyToAsync(stdout, CancellationToken.None);
        var pumpErr = process.StandardError.BaseStream.CopyToAsync(stderr, CancellationToken.None);

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

            // Vider les tubes avant de rendre la main : la destination peut être
            // refermée par l'appelant sitôt l'annulation remontée.
            await Task.WhenAll(pumpOut, pumpErr);

            // Une annulation demandée par l'appelant interrompt le run : elle
            // remonte. Un dépassement de délai, lui, est une issue d'exécution
            // ordinaire, que la garde OnFailure routera.
            cancellationToken.ThrowIfCancellationRequested();
            outcome = ScriptOutcome.TimedOut;
        }

        await Task.WhenAll(pumpOut, pumpErr);
        return new ScriptResult(process.ExitCode, outcome, chrono.Elapsed);
    }

    /// <summary>
    /// Traduit un <see cref="ScriptSpec"/> en process prêt à démarrer : argv passé
    /// token par token (aucun quoting), sorties redirigées, environnement hôte
    /// surchargé clé par clé, puis <c>PATH</c> enrichi des racines connues — en
    /// dernier, pour couvrir aussi bien le <c>PATH</c> hérité qu'un éventuel
    /// surchargé par l'étape.
    /// </summary>
    private Process Describe(ScriptSpec spec)
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

        // Le PATH effectif est l'hérité, éventuellement surchargé par l'étape. On
        // résout la commande directe en absolu (car .NET ne cherche pas dans le PATH
        // de StartInfo), et on enrichit le PATH transmis pour les process descendants.
        process.StartInfo.Environment.TryGetValue("PATH", out var effectivePath);
        process.StartInfo.FileName = _pathStrategy.Resolve(spec.FileName, effectivePath);
        process.StartInfo.Environment["PATH"] = _pathStrategy.Enrich(effectivePath);

        return process;
    }
}
