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
    string? Kind,
    int MaxVisits,
    ScriptDocument? Script,
    AgentDocument? Agent,
    IReadOnlyList<EdgeDocument>? Edges,
    string? WorkingSubdirectory);

internal sealed record ScriptDocument(
    string? FileName,
    IReadOnlyList<string>? Arguments,
    IReadOnlyDictionary<string, string>? Environment,
    double? TimeoutSeconds);

/// <summary>
/// La charge d'une étape-agent : quel harness, quel modèle, et le prompt. Symétrique
/// de <see cref="ScriptDocument"/> — le discriminant <c>kind</c> dit lequel des deux
/// porte le sens, l'adaptateur construit le sous-type correspondant.
/// </summary>
internal sealed record AgentDocument(
    string? Harness,
    string? Model,
    string? Prompt);

/// <summary>
/// Une arête. La garde s'écrit en chaîne (« success », « exit:2 ») : compact,
/// tapable, et extensible sans changer la forme du document.
/// </summary>
internal sealed record EdgeDocument(string? Guard, string? Target);
