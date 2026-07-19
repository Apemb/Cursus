namespace Cursus.Core.Workflows;

/// <summary>
/// Issue de l'exécution d'un script par un <see cref="IProcessRunner"/>,
/// indépendamment du code de sortie.
/// </summary>
public enum ScriptOutcome
{
    /// <summary>Le process a démarré et s'est terminé de lui-même.</summary>
    Completed,

    /// <summary>Le process a été tué parce qu'il a dépassé son délai.</summary>
    TimedOut,

    /// <summary>Le process n'a pas pu être lancé (binaire introuvable, etc.).</summary>
    LaunchFailed,
}
