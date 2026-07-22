using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Cursus.Core.Workflows.Validation;

namespace Cursus.Core.Workflows.Serialization;

/// <summary>Ce qu'a produit la lecture d'un document : une définition, ou les raisons de son refus.</summary>
/// <param name="Definition">Non nulle si et seulement si le rapport est valide.</param>
public sealed record LoadResult(WorkflowDefinition? Definition, ValidationReport Report);

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

    public static LoadResult Read(string json)
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
            return new LoadResult(null, new ValidationReport(
                [new ValidationIssue(ValidationIssueKind.MalformedDocument, failure.Message)]));
        }
        catch (UnknownGuardException failure)
        {
            // Un vocabulaire du document que le modèle ne sait pas traduire est
            // un problème du même ordre qu'une arête cassée : il se rapporte.
            return new LoadResult(null, new ValidationReport(
                [new ValidationIssue(ValidationIssueKind.UnknownGuard, failure.Message)]));
        }

        var report = WorkflowValidator.Validate(definition);
        return new LoadResult(report.IsValid ? definition : null, report);
    }

    private static StepDefinition ToStep(StepDocument step) =>
        new(
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
            step.WorkingSubdirectory);

    private static Edge ToEdge(EdgeDocument edge) =>
        new(ToGuard(edge.Guard), edge.Target ?? "");

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

    private static StepDocument ToDocument(StepDefinition step) =>
        new(
            step.Id,
            step.Name,
            step.MaxVisits,
            new ScriptDocument(
                step.Script.FileName,
                step.Script.Arguments,
                step.Script.Environment,
                step.Script.Timeout?.TotalSeconds),
            step.OutEdges.Select(ToDocument).ToList(),
            step.WorkingSubdirectory);

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
