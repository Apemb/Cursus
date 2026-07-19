namespace Cursus.Core.Workflows;

/// <summary>
/// Résultat de l'exécution d'un script. Source unique de vérité du succès :
/// un script réussit s'il s'est terminé de lui-même avec le code 0.
/// </summary>
public sealed record ScriptResult(int ExitCode, ScriptOutcome Outcome)
{
    public bool IsSuccess => Outcome == ScriptOutcome.Completed && ExitCode == 0;
}
