namespace Cursus.Core.Workflows;

/// <summary>
/// Ce qu'une visite a laissé derrière elle sur un flux : un nom (<c>stdout</c>,
/// <c>stderr</c>…), l'endroit où c'est rangé, et sa taille en octets. Le chemin
/// est absent quand le flux n'a rien reçu — rien n'a alors été créé.
/// </summary>
public sealed record OutputArtifact(string Name, string? Path, long Size);
