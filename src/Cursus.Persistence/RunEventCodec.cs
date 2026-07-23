using System.Text.Json;
using System.Text.Json.Serialization;

using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Serialization;

namespace Cursus.Persistence;

/// <summary>
/// Traduit un <see cref="WorkflowEvent"/> vers le payload JSON de la colonne
/// <c>payload</c>, et retour.
/// </summary>
/// <remarks>
/// Des DTO plutôt qu'une sérialisation directe du modèle, pour la même raison
/// qu'au jalon 3 : le format stocké ne doit pas suivre chaque renommage du
/// domaine. Deux contraintes le rendent d'ailleurs obligatoire ici — les gardes
/// d'une <see cref="WorkflowDefinition"/> sont des types abstraits que
/// <c>System.Text.Json</c> ne sait pas reconstruire (c'est
/// <see cref="WorkflowSerializer"/> qui le fait, depuis la colonne
/// <c>definition_json</c>), et les sorties d'un script n'ont pas à entrer en base.
/// </remarks>
internal static class RunEventCodec
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Le nom de kind sous lequel un événement est rangé en base.</summary>
    internal static string KindOf(WorkflowEvent @event) => @event.GetType().Name;

    internal static string Encode(WorkflowEvent @event) => @event switch
    {
        WorkflowEvent.RunStarted e => Write(new RunStartedPayload(
            e.WorkspaceRoot, e.Trigger.Kind.ToString(), e.Trigger.TaskKey, e.WorkflowId)),

        WorkflowEvent.StepStarted e => Write(new StepStartedPayload(e.StepId, e.Iteration)),

        WorkflowEvent.StepFinished e => Write(new StepFinishedPayload(
            e.StepId,
            e.Iteration,
            e.Result.ExitCode,
            e.Result.Outcome.ToString(),
            e.Result.Duration.TotalSeconds,
            e.Output.Artifacts.Select(a => new ArtifactPayload(a.Name, a.Path, a.Size)).ToList())),

        WorkflowEvent.EdgeChosen e => Write(new EdgeChosenPayload(e.FromStepId, e.ToStepId)),

        WorkflowEvent.RunFinished e => Write(new RunFinishedPayload(
            e.State.ToString(), e.AbortReason?.ToString())),

        _ => throw new NotSupportedException($"Événement non journalisable : {@event.GetType().Name}"),
    };

    /// <summary>
    /// Reconstruit un événement. La définition n'est pas dans le payload : elle
    /// vient de la colonne <c>definition_json</c> de la table des runs, d'où
    /// <paramref name="definition"/>.
    /// </summary>
    internal static WorkflowEvent Decode(string kind, string payload, Func<WorkflowDefinition> definition)
    {
        switch (kind)
        {
            case nameof(WorkflowEvent.RunStarted):
            {
                var p = Read<RunStartedPayload>(payload);
                return new WorkflowEvent.RunStarted(
                    definition(),
                    p.WorkspaceRoot,
                    new RunTrigger(Enum.Parse<RunTriggerKind>(p.TriggerKind), p.TaskKey),
                    p.WorkflowId);
            }

            case nameof(WorkflowEvent.StepStarted):
            {
                var p = Read<StepStartedPayload>(payload);
                return new WorkflowEvent.StepStarted(p.StepId, p.Iteration);
            }

            case nameof(WorkflowEvent.StepFinished):
            {
                // Le contenu n'est pas là : le payload ne garde que les artefacts
                // (nom, chemin, taille), et c'est le magasin qui détient les octets.
                var p = Read<StepFinishedPayload>(payload);
                var result = new ScriptResult(
                    p.ExitCode,
                    Enum.Parse<ScriptOutcome>(p.Outcome),
                    Duration: TimeSpan.FromSeconds(p.DurationSeconds));
                var output = new StepOutput(
                    p.Artifacts.Select(a => new OutputArtifact(a.Name, a.Path, a.Size)).ToList());
                return new WorkflowEvent.StepFinished(p.StepId, p.Iteration, result, output);
            }

            case nameof(WorkflowEvent.EdgeChosen):
            {
                var p = Read<EdgeChosenPayload>(payload);
                return new WorkflowEvent.EdgeChosen(p.FromStepId, p.ToStepId);
            }

            case nameof(WorkflowEvent.RunFinished):
            {
                var p = Read<RunFinishedPayload>(payload);
                return new WorkflowEvent.RunFinished(
                    Enum.Parse<RunState>(p.State),
                    p.AbortReason is null ? null : Enum.Parse<AbortReason>(p.AbortReason));
            }

            default:
                throw new NotSupportedException($"Kind d'événement inconnu dans le journal : {kind}");
        }
    }

    private static string Write<T>(T payload) => JsonSerializer.Serialize(payload, Options);

    private static T Read<T>(string payload) => JsonSerializer.Deserialize<T>(payload, Options)!;

    // --- DTO de payload, un par kind ---

    private sealed record RunStartedPayload(
        string WorkspaceRoot, string TriggerKind, string? TaskKey, string? WorkflowId);

    private sealed record StepStartedPayload(string StepId, int Iteration);

    private sealed record StepFinishedPayload(
        string StepId,
        int Iteration,
        int ExitCode,
        string Outcome,
        double DurationSeconds,
        IReadOnlyList<ArtifactPayload> Artifacts);

    private sealed record ArtifactPayload(string Name, string? Path, long Size);

    private sealed record EdgeChosenPayload(string FromStepId, string ToStepId);

    private sealed record RunFinishedPayload(string State, string? AbortReason);
}
