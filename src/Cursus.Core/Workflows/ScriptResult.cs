namespace Cursus.Core.Workflows;

/// <summary>
/// Ce que le process a fait — et rien de sa sortie, qui a ruisselé ailleurs
/// (voir <see cref="StepOutput"/>). Source unique de vérité du succès : un
/// script réussit s'il s'est terminé de lui-même avec le code 0.
/// </summary>
public sealed record ScriptResult(
    int ExitCode,
    ScriptOutcome Outcome,
    TimeSpan Duration = default)
{
    public bool IsSuccess => Outcome == ScriptOutcome.Completed && ExitCode == 0;
}
