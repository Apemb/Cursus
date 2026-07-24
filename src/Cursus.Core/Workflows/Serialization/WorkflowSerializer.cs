using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Cursus.Core.Workflows.Validation;

namespace Cursus.Core.Workflows.Serialization;

/// <summary>Ce qu'a produit la lecture d'un document : une définition, ou les raisons de son refus.</summary>
/// <param name="Definition">Non nulle si et seulement si le rapport est valide.</param>
public sealed record LoadResult(WorkflowDefinition? Definition, ValidationReport Report);

/// <summary>
/// Le jumeau de <see cref="LoadResult"/> pour l'édition — même forme, invariant
/// opposé. Ici <see cref="Definition"/> est non nulle <b>dès que le document a
/// parsé</b>, valide ou non : sa validité se lit dans <see cref="Report"/>, pas
/// dans sa nullité. Un brouillon cassé se rouvre donc pour être corrigé, là où
/// <see cref="LoadResult"/> l'aurait annulé.
/// </summary>
/// <param name="Definition">Non nulle si et seulement si le <i>parsing</i> a abouti.</param>
public sealed record ParsedWorkflow(WorkflowDefinition? Definition, ValidationReport Report);

/// <summary>
/// Traduit entre le document JSON et le modèle, dans les deux sens. Ne rend
/// jamais une définition non validée, et ne touche jamais au disque : lire et
/// écrire le fichier appartient à l'appelant.
/// </summary>
public static class WorkflowSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,

        // Un nom d'étape accentué doit rester lisible dans le fichier : par
        // défaut, « Préparer » s'écrirait avec un « e » échappé en séquence
        // Unicode. L'échappement des caractères sensibles au HTML est conservé.
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    // --- du document vers le modèle ---

    /// <summary>
    /// Lit un document <b>pour l'exécuter</b> : ne rend une définition que si elle
    /// est valide, sinon null + le rapport de son refus. C'est la projection
    /// validité-couplée de <see cref="ReadEditable"/>, dont dépend le chemin de
    /// lancement — d'où le maintien strict de l'invariant « non-null ⟺ valide ».
    /// </summary>
    public static LoadResult Read(string json)
    {
        var parsed = ReadEditable(json);
        return new LoadResult(parsed.Report.IsValid ? parsed.Definition : null, parsed.Report);
    }

    /// <summary>
    /// Lit un document <b>pour l'éditer</b> : rend la définition parsée même
    /// invalide, la validité se lisant dans le rapport. Ne rend rien que si le
    /// <i>parsing</i> lui-même échoue (JSON malformé, garde inconnue) — là il n'y
    /// a pas de graphe à corriger, seulement du texte.
    /// </summary>
    public static ParsedWorkflow ReadEditable(string json)
    {
        WorkflowDefinition definition;
        try
        {
            var document = JsonSerializer.Deserialize<WorkflowDocument>(json, Options)
                           ?? throw new JsonException("Le document est vide.");

            definition = new WorkflowDefinition(
                document.EntryStep ?? "",
                (document.Steps ?? []).Select(ToStep).ToList());
        }
        catch (JsonException failure)
        {
            return new ParsedWorkflow(null, new ValidationReport(
                [new ValidationIssue(ValidationIssueKind.MalformedDocument, failure.Message)]));
        }
        catch (UnknownGuardException failure)
        {
            return new ParsedWorkflow(null, new ValidationReport(
                [new ValidationIssue(ValidationIssueKind.UnknownGuard, failure.Message)]));
        }

        return new ParsedWorkflow(definition, WorkflowValidator.Validate(definition));
    }

    // L'adaptateur : le discriminant `kind` du document choisit le sous-type à
    // construire. Absent (ou inconnu) retombe sur l'étape-script — c'est ce qui garde
    // valides les fichiers écrits avant l'arrivée des kinds.
    private static StepDefinition ToStep(StepDocument step) => step.Kind switch
    {
        "agent" => new AgentStep(
            step.Id ?? "",
            step.Name ?? step.Id ?? "",
            step.Agent?.Harness ?? "",
            step.Agent?.Model ?? "",
            step.Agent?.Prompt ?? "",
            step.MaxVisits,
            (step.Edges ?? []).Select(ToEdge).ToList(),
            step.WorkingSubdirectory,
            step.Description),
        "task" => new TaskStep(
            step.Id ?? "",
            step.Name ?? step.Id ?? "",
            ToOperation(step.Task),
            step.MaxVisits,
            (step.Edges ?? []).Select(ToEdge).ToList(),
            step.WorkingSubdirectory,
            step.Description),
        _ => new ScriptStep(
            step.Id ?? "",
            step.Name ?? step.Id ?? "",
            new ScriptSpec(
                step.Script?.FileName ?? "",
                step.Script?.Arguments ?? [],
                Environment: step.Script?.Environment,
                Timeout: step.Script?.TimeoutSeconds is { } seconds
                    ? TimeSpan.FromSeconds(seconds)
                    : null),
            step.MaxVisits,
            (step.Edges ?? []).Select(ToEdge).ToList(),
            step.WorkingSubdirectory,
            step.Description),
    };

    private static Edge ToEdge(EdgeDocument edge) =>
        new(ToGuard(edge.Guard), edge.Target ?? "");

    // Le discriminant `operation` choisit la variante ; absent (ou inconnu) retombe
    // sur la lecture — le geste le plus inoffensif, qui ne mute pas le tableau.
    private static TaskOperation ToOperation(TaskDocument? task) => task?.Operation switch
    {
        "move" => new TaskOperation.MoveCard(task.Column ?? ""),
        "label" => new TaskOperation.ApplyLabel(task.Label ?? ""),
        _ => new TaskOperation.ReadTask(),
    };

    /// <summary>
    /// Une garde s'écrit en chaîne : « success », « failure », « default », ou
    /// « exit:&lt;code&gt; ». Le préfixe laisse la place à d'autres familles
    /// (« stdout:… ») sans changer la forme du document.
    /// </summary>
    private static Guard ToGuard(string? guard) => guard switch
    {
        "success" => Guard.OnSuccess,
        "failure" => Guard.OnFailure,
        "default" => Guard.Default,
        _ when guard?.StartsWith("exit:", StringComparison.Ordinal) is true
               && int.TryParse(guard.AsSpan(5), out var code) => Guard.OnExitCode(code),
        _ => throw new UnknownGuardException(guard),
    };

    // --- du modèle vers le document ---

    /// <summary>
    /// Réécrit une définition sous sa forme de document. L'éditeur graphique
    /// passera par là à chaque sauvegarde : ce qui a été lu doit ressortir
    /// intact.
    /// </summary>
    public static string Write(WorkflowDefinition definition) =>
        JsonSerializer.Serialize(
            new WorkflowDocument(definition.EntryStep, definition.Steps.Select(ToDocument).ToList()),
            Options);

    // L'adaptateur dans l'autre sens : chaque sous-type d'étape connaît sa forme de
    // document. Un seul kind aujourd'hui ; un kind de plus ajoute un bras à ce switch.
    private static StepDocument ToDocument(StepDefinition step) => step switch
    {
        ScriptStep s => new StepDocument(
            s.Id,
            s.Name,
            s.Description,
            "script",
            s.MaxVisits,
            new ScriptDocument(
                s.Script.FileName,
                s.Script.Arguments,
                s.Script.Environment,
                s.Script.Timeout?.TotalSeconds),
            Agent: null,
            s.OutEdges.Select(ToDocument).ToList(),
            s.WorkingSubdirectory),
        AgentStep a => new StepDocument(
            a.Id,
            a.Name,
            a.Description,
            "agent",
            a.MaxVisits,
            Script: null,
            new AgentDocument(a.HarnessName, a.ModelId, a.Prompt),
            a.OutEdges.Select(ToDocument).ToList(),
            a.WorkingSubdirectory),
        TaskStep t => new StepDocument(
            t.Id,
            t.Name,
            t.Description,
            "task",
            t.MaxVisits,
            Script: null,
            Agent: null,
            t.OutEdges.Select(ToDocument).ToList(),
            t.WorkingSubdirectory,
            ToDocument(t.Operation)),
        _ => throw new ArgumentOutOfRangeException(
            nameof(step), step.GetType().Name, "Type d'étape non sérialisable."),
    };

    // Le geste d'une étape-tâche s'écrit par son discriminant ; column/label ne
    // portent que pour leur opération. Miroir exact de ToOperation, dont dépend l'aller-retour.
    private static TaskDocument ToDocument(TaskOperation operation) => operation switch
    {
        TaskOperation.ReadTask => new TaskDocument("read", null, null),
        TaskOperation.MoveCard move => new TaskDocument("move", move.Column, null),
        TaskOperation.ApplyLabel label => new TaskDocument("label", null, label.Label),
        _ => throw new ArgumentOutOfRangeException(
            nameof(operation), operation.GetType().Name, "Opération de tâche non sérialisable."),
    };

    private static EdgeDocument ToDocument(Edge edge) => new(WriteGuard(edge.Guard), edge.Target);

    /// <summary>Le miroir exact de <see cref="ToGuard"/>, dont dépend l'aller-retour.</summary>
    private static string WriteGuard(Guard guard) => guard switch
    {
        Guard.SuccessGuard => "success",
        Guard.FailureGuard => "failure",
        Guard.AlwaysGuard => "default",
        Guard.ExitCodeGuard exit => $"exit:{exit.Code}",
        _ => throw new UnknownGuardException(guard.GetType().Name),
    };
}
