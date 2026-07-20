namespace Cursus.Core.Workflows;

/// <summary>
/// Description immuable de ce qu'un <see cref="IProcessRunner"/> doit lancer.
/// Le moteur ne l'interprète jamais : il la transmet telle quelle au runner.
/// </summary>
/// <param name="FileName">Chemin de l'exécutable, sans expansion de <c>~</c> ni de variable.</param>
/// <param name="Arguments">Tokens d'argv, transmis verbatim (aucun quoting à gérer).</param>
/// <param name="WorkingDirectory">Répertoire de travail ; à défaut, celui du process hôte.</param>
/// <param name="Environment">
/// Variables surchargées par-dessus l'environnement hôte, clé par clé. Un script
/// de workflow est du code commité, de confiance : il hérite de l'environnement
/// (l'allowlist stricte est réservée au monde agent).
/// </param>
/// <param name="Timeout">Délai au-delà duquel le process est tué ; à défaut, aucune limite.</param>
public sealed record ScriptSpec(
    string FileName,
    IReadOnlyList<string> Arguments,
    string? WorkingDirectory = null,
    IReadOnlyDictionary<string, string>? Environment = null,
    TimeSpan? Timeout = null);
