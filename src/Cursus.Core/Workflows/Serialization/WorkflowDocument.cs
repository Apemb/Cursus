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
    string? WorkingSubdirectory,
    TaskDocument? Task = null);

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
/// La charge d'une étape-tâche : quel geste sur le tableau. Le discriminant
/// <c>operation</c> (« read » · « move » · « label ») dit laquelle des variantes de
/// <see cref="Cursus.Core.Workflows.TaskOperation"/> construire ; <c>column</c> et
/// <c>label</c> ne portent que pour leur opération respective. Ces champs optionnels
/// vivent <b>dans le document seulement</b> — le modèle, lui, n'admet pas d'état
/// illégal, chaque variante ne portant que sa donnée.
/// </summary>
internal sealed record TaskDocument(
    string? Operation,
    string? Column,
    string? Label);

/// <summary>
/// Une arête. La garde s'écrit en chaîne (« success », « exit:2 ») : compact,
/// tapable, et extensible sans changer la forme du document.
/// </summary>
internal sealed record EdgeDocument(string? Guard, string? Target);
