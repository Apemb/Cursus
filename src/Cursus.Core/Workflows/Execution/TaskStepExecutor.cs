using System.Text;

namespace Cursus.Core.Workflows.Execution;

/// <summary>
/// L'exécuteur de l'<see cref="TaskStep"/> : traduit son <see cref="TaskOperation"/>
/// en un geste sur le tableau via l'<see cref="ITaskTracker"/>, et rend un
/// <see cref="ScriptResult"/> routable par les gardes comme n'importe quelle étape.
/// La clé de la tâche visée lui vient du <see cref="StepExecutionContext"/> — le
/// moteur l'a tirée du <see cref="RunTrigger"/> du run.
/// </summary>
public sealed class TaskStepExecutor : IStepExecutor
{
    private static readonly ScriptResult Success = new(0, ScriptOutcome.Completed);
    private static readonly ScriptResult Failure = new(1, ScriptOutcome.Completed);

    private readonly ITaskTracker _tracker;

    public TaskStepExecutor(ITaskTracker tracker) => _tracker = tracker;

    public bool CanExecute(StepDefinition step) => step is TaskStep;

    public async Task<ScriptResult> ExecuteAsync(
        StepDefinition step,
        StepExecutionContext context,
        Stream stdout,
        Stream stderr,
        CancellationToken cancellationToken)
    {
        // La tâche visée vient du run, pas de la définition : sans clé (run manuel),
        // il n'y a rien à toucher. C'est un fait de run, pas de graphe — donc un échec
        // routable (arête de secours, journal, écran), jamais une exception.
        if (context.TaskKey is not { } key)
            return await FailAsync(stderr, "Aucune tâche associée à ce run : le geste de tableau est sans objet.")
                .ConfigureAwait(false);

        var operation = ((TaskStep)step).Operation;
        try
        {
            // Le geste dépend du type de l'opération — c'est le seul aiguillage propre
            // à ce kind ; chaque bras appelle le tracker et rend la ligne à journaliser.
            var line = operation switch
            {
                TaskOperation.ReadTask => await ReadAsync(key, context.WorkingDirectory, cancellationToken)
                    .ConfigureAwait(false),
                TaskOperation.MoveCard move => await MoveAsync(key, move.Column, cancellationToken)
                    .ConfigureAwait(false),
                TaskOperation.ApplyLabel label => await LabelAsync(key, label.Label, cancellationToken)
                    .ConfigureAwait(false),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(step), operation.GetType().Name, "Opération de tâche inconnue."),
            };
            await WriteLineAsync(stdout, line).ConfigureAwait(false);
            return Success;
        }
        catch (Exception failure)
        {
            // Un tracker injoignable ou refusant l'écriture est un échec de run comme
            // un autre : routable, pas une exception qui casserait la traversée.
            return await FailAsync(stderr, $"Le geste de tableau a échoué : {failure.Message}")
                .ConfigureAwait(false);
        }
    }

    private async Task<string> ReadAsync(string key, string workingDirectory, CancellationToken cancellationToken)
    {
        var card = await _tracker.ReadAsync(key, cancellationToken).ConfigureAwait(false);

        // Le corps de la tâche descend dans le worktree — la mémoire partagée du run
        // (§4.9) : une étape-agent en aval lit « TASK.md » sans qu'aucune référence
        // n'ait à circuler d'une étape à l'autre.
        var body = $"# {card.Title}\n\n{card.Description}\n";
        await File.WriteAllTextAsync(Path.Combine(workingDirectory, "TASK.md"), body, cancellationToken)
            .ConfigureAwait(false);
        return $"Tâche {key} lue dans TASK.md.";
    }

    private async Task<string> MoveAsync(string key, string column, CancellationToken cancellationToken)
    {
        await _tracker.MoveAsync(key, column, cancellationToken).ConfigureAwait(false);
        return $"Carte {key} déplacée vers {column}.";
    }

    private async Task<string> LabelAsync(string key, string label, CancellationToken cancellationToken)
    {
        await _tracker.ApplyLabelAsync(key, label, cancellationToken).ConfigureAwait(false);
        return $"Étiquette « {label} » posée sur {key}.";
    }

    private static async Task<ScriptResult> FailAsync(Stream stderr, string reason)
    {
        await WriteLineAsync(stderr, reason).ConfigureAwait(false);
        return Failure;
    }

    private static Task WriteLineAsync(Stream stream, string line)
    {
        var bytes = Encoding.UTF8.GetBytes(line + '\n');
        return stream.WriteAsync(bytes).AsTask();
    }
}
