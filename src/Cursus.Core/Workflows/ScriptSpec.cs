namespace Cursus.Core.Workflows;

/// <summary>
/// Description immuable de ce qu'un <see cref="IProcessRunner"/> doit lancer.
/// Placeholder minimal au jalon 1 (le moteur ne l'interprète jamais) ; enrichi
/// au jalon 2 (environnement, working directory, timeout).
/// </summary>
public sealed record ScriptSpec(string FileName, IReadOnlyList<string> Arguments);
