using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Execution;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// L'exécuteur de l'étape-tâche : il ne route rien, il <b>agit sur le tableau</b> —
/// déplacer une carte, poser une étiquette, lire une tâche — via le port
/// <see cref="ITaskTracker"/>, et rend un <see cref="ScriptResult"/> routable comme
/// n'importe quelle étape. La clé de la tâche visée lui vient du contexte d'exécution,
/// que le moteur bâtit depuis le <see cref="RunTrigger"/>.
/// </summary>
public class TaskStepExecutorTests
{
    [Fact(DisplayName = "étant donné une étape-tâche « déplacer vers En review » et un contexte portant la clé « ENG-1 », quand on l'exécute, alors le tracker déplace « ENG-1 » vers « En review » et l'issue est un succès")]
    public async Task It_moves_the_card_of_the_context_task()
    {
        // arrange
        var tracker = new StubTaskTracker();
        var executor = new TaskStepExecutor(tracker);
        var step = new TaskStep(
            "entrer", "Entrer en review", new TaskOperation.MoveCard("En review"), MaxVisits: 1, []);
        var context = new StepExecutionContext("/tmp/run", TaskKey: "ENG-1");
        using var stdout = new MemoryStream();
        using var stderr = new MemoryStream();

        // act
        var result = await executor.ExecuteAsync(step, context, stdout, stderr, CancellationToken.None);

        // assert
        Assert.Equal(("ENG-1", "En review"), Assert.Single(tracker.Moves));
        Assert.True(result.IsSuccess);
    }

    [Fact(DisplayName = "étant donné une étape-tâche « déplacer » et un contexte sans clé de tâche, quand on l'exécute, alors aucune carte n'est déplacée et l'issue est un échec")]
    public async Task It_fails_without_touching_the_board_when_no_task_key()
    {
        // arrange
        var tracker = new StubTaskTracker();
        var executor = new TaskStepExecutor(tracker);
        var step = new TaskStep(
            "entrer", "Entrer en review", new TaskOperation.MoveCard("En review"), MaxVisits: 1, []);
        var context = new StepExecutionContext("/tmp/run", TaskKey: null);
        using var stdout = new MemoryStream();
        using var stderr = new MemoryStream();

        // act
        var result = await executor.ExecuteAsync(step, context, stdout, stderr, CancellationToken.None);

        // assert
        Assert.Empty(tracker.Moves);
        Assert.False(result.IsSuccess);
    }

    [Fact(DisplayName = "étant donné un tracker qui lève à la mutation, quand on exécute le déplacement, alors l'issue est un échec et l'exception ne remonte pas")]
    public async Task It_turns_a_tracker_fault_into_a_routable_failure()
    {
        // arrange
        var tracker = new StubTaskTracker { Fault = new InvalidOperationException("tableau injoignable") };
        var executor = new TaskStepExecutor(tracker);
        var step = new TaskStep(
            "entrer", "Entrer en review", new TaskOperation.MoveCard("En review"), MaxVisits: 1, []);
        var context = new StepExecutionContext("/tmp/run", TaskKey: "ENG-1");
        using var stdout = new MemoryStream();
        using var stderr = new MemoryStream();

        // act
        var result = await executor.ExecuteAsync(step, context, stdout, stderr, CancellationToken.None);

        // assert
        Assert.False(result.IsSuccess);
    }

    [Fact(DisplayName = "étant donné une étape-tâche « poser Done » et un contexte portant « ENG-1 », quand on l'exécute, alors le tracker pose « Done » sur « ENG-1 » et l'issue est un succès")]
    public async Task It_applies_the_label_of_the_context_task()
    {
        // arrange
        var tracker = new StubTaskTracker();
        var executor = new TaskStepExecutor(tracker);
        var step = new TaskStep(
            "sortir", "Marquer terminé", new TaskOperation.ApplyLabel("Done"), MaxVisits: 1, []);
        var context = new StepExecutionContext("/tmp/run", TaskKey: "ENG-1");
        using var stdout = new MemoryStream();
        using var stderr = new MemoryStream();

        // act
        var result = await executor.ExecuteAsync(step, context, stdout, stderr, CancellationToken.None);

        // assert
        Assert.Equal(("ENG-1", "Done"), Assert.Single(tracker.Labels));
        Assert.True(result.IsSuccess);
    }

    [Fact(DisplayName = "étant donné une étape-tâche « lire » et un contexte (clé « ENG-1 » + répertoire), quand on l'exécute, alors le corps de la carte est écrit dans « TASK.md » du répertoire et l'issue est un succès")]
    public async Task It_writes_the_read_task_into_the_worktree()
    {
        // arrange
        var directory = Directory.CreateTempSubdirectory("cursus-taskstep-").FullName;
        var tracker = new StubTaskTracker
        {
            Card = new TaskCard("ENG-1", "Corriger le tri", "Le tri est instable sur les doublons.", "En cours", []),
        };
        var executor = new TaskStepExecutor(tracker);
        var step = new TaskStep("lire", "Lire la tâche", new TaskOperation.ReadTask(), MaxVisits: 1, []);
        var context = new StepExecutionContext(directory, TaskKey: "ENG-1");
        using var stdout = new MemoryStream();
        using var stderr = new MemoryStream();

        // act
        var result = await executor.ExecuteAsync(step, context, stdout, stderr, CancellationToken.None);

        // assert
        Assert.Equal("ENG-1", Assert.Single(tracker.Reads));
        var written = await File.ReadAllTextAsync(Path.Combine(directory, "TASK.md"));
        Assert.Contains("Corriger le tri", written);
        Assert.Contains("Le tri est instable sur les doublons.", written);
        Assert.True(result.IsSuccess);
    }
}
