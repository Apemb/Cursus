namespace Cursus.Core.Workflows.Execution;

/// <summary>
/// Abstraction du lancement d'un process. Le moteur délègue ici et ne fait
/// jamais de <c>Process.Start</c> lui-même — ce qui le rend entièrement
/// testable sur un double renvoyant des résultats programmés.
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// Lance le process décrit et attend sa fin, en <b>ruisselant</b> ses deux
    /// sorties (octets bruts) vers <paramref name="stdout"/> et
    /// <paramref name="stderr"/> à mesure qu'elles arrivent. Une annulation tue
    /// le process et lève <see cref="OperationCanceledException"/> : elle n'est
    /// pas une issue d'exécution mais une interruption du run.
    /// </summary>
    Task<ScriptResult> RunAsync(
        ScriptSpec spec, Stream stdout, Stream stderr, CancellationToken cancellationToken = default);
}
