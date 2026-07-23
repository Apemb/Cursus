namespace Cursus.Core.Workflows.Serialization;

/// <summary>
/// La forme du fichier de workflow, délibérément distincte du modèle interne :
/// le format survit aux refactors du noyau, et un document structurellement
/// lisible mais sémantiquement faux devient un rapport de validation plutôt
/// qu'une exception de désérialisation.
/// </summary>
internal sealed record WorkflowDocument(string? EntryStep, IReadOnlyList<StepDocument>? Steps);

internal sealed record StepDocument(
    string? Id,
    string? Name,
    string? Description,
    int MaxVisits,
    ScriptDocument? Script,
    IReadOnlyList<EdgeDocument>? Edges,
    string? WorkingSubdirectory);

internal sealed record ScriptDocument(
    string? FileName,
    IReadOnlyList<string>? Arguments,
    IReadOnlyDictionary<string, string>? Environment,
    double? TimeoutSeconds);

/// <summary>
/// Une arête. La garde s'écrit en chaîne (« success », « exit:2 ») : compact,
/// tapable, et extensible sans changer la forme du document.
/// </summary>
internal sealed record EdgeDocument(string? Guard, string? Target);
