namespace Cursus.Core.Workflows;

/// <summary>
/// Signale, à l'intérieur du sérialiseur, une garde que le document déclare et
/// que le modèle ne connaît pas. Interne au chargement : l'appelant, lui, ne
/// voit qu'un <see cref="ValidationIssue"/>.
/// </summary>
internal sealed class UnknownGuardException(string? guard)
    : Exception($"Garde inconnue : « {guard} »");
