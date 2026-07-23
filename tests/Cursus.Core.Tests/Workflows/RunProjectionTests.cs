using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Projection;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// La projection d'un run : elle plie une séquence de <see cref="WorkflowEvent"/>
/// en trajectoire de visites + statut, sans savoir d'où vient la séquence — le
/// flux live d'un run en cours ou la relecture d'un run passé l'alimentent à
/// l'identique (« un seul objet, deux alimentations », parcours §1.4). Cœur
/// testable de l'écran de run, sans une ligne d'Avalonia (§7.12).
/// </summary>
public sealed class RunProjectionTests
{
    [Fact(DisplayName = "étant donné un RunStarted appliqué, quand on lit la projection, alors le run est en cours et la trajectoire est vide")]
    public void A_started_run_is_running_with_an_empty_trajectory()
    {
        // arrange
        var projection = new RunProjection();

        // act
        projection.Apply(new WorkflowEvent.RunStarted(AnyDefinition, "/tmp", RunTrigger.Manual, "verifier"));

        // assert
        Assert.True(projection.IsRunning);
        Assert.Empty(projection.Trajectory);
    }

    [Fact(DisplayName = "étant donné une visite d'étape démarrée, quand on l'applique, alors la trajectoire porte cette visite, en cours")]
    public void A_started_step_appears_as_a_running_visit()
    {
        // arrange
        var projection = new RunProjection();
        projection.Apply(new WorkflowEvent.RunStarted(AnyDefinition, "/tmp", RunTrigger.Manual, "verifier"));

        // act
        projection.Apply(new WorkflowEvent.StepStarted("A", Iteration: 1));

        // assert
        var visit = Assert.Single(projection.Trajectory);
        Assert.Equal("A", visit.StepId);
        Assert.Equal(1, visit.Iteration);
        Assert.True(visit.IsRunning);
    }

    [Fact(DisplayName = "étant donné une visite d'étape achevée, quand on l'applique, alors cette visite porte son issue et ne tourne plus")]
    public void A_finished_step_closes_its_visit_with_the_outcome()
    {
        // arrange
        var projection = new RunProjection();
        projection.Apply(new WorkflowEvent.RunStarted(AnyDefinition, "/tmp", RunTrigger.Manual, "verifier"));
        projection.Apply(new WorkflowEvent.StepStarted("A", Iteration: 1));

        // act
        projection.Apply(new WorkflowEvent.StepFinished(
            "A", Iteration: 1, new ScriptResult(0, ScriptOutcome.Completed), NoOutput));

        // assert
        var visit = Assert.Single(projection.Trajectory);
        Assert.False(visit.IsRunning);
        Assert.Equal(0, visit.Result!.ExitCode);
    }

    [Fact(DisplayName = "étant donné une étape rejouée en boucle, quand on applique ses deux visites, alors la trajectoire les distingue par leur itération")]
    public void A_looping_step_yields_one_visit_per_iteration()
    {
        // arrange
        var projection = new RunProjection();
        projection.Apply(new WorkflowEvent.RunStarted(AnyDefinition, "/tmp", RunTrigger.Manual, "verifier"));

        // act — Tester échoue (sortie 1), la boucle repart, Tester est revisité
        projection.Apply(new WorkflowEvent.StepStarted("Tester", Iteration: 1));
        projection.Apply(new WorkflowEvent.StepFinished(
            "Tester", 1, new ScriptResult(1, ScriptOutcome.Completed), NoOutput));
        projection.Apply(new WorkflowEvent.StepStarted("Tester", Iteration: 2));

        // assert — deux visites distinctes : la première close en échec, la seconde en cours
        Assert.Collection(projection.Trajectory,
            first => { Assert.Equal(1, first.Iteration); Assert.Equal(1, first.Result!.ExitCode); },
            second => { Assert.Equal(2, second.Iteration); Assert.True(second.IsRunning); });
    }

    [Fact(DisplayName = "étant donné un RunFinished, quand on l'applique, alors le run n'est plus en cours et porte son état terminal")]
    public void A_finished_run_stops_running_and_carries_its_terminal_state()
    {
        // arrange
        var projection = new RunProjection();
        projection.Apply(new WorkflowEvent.RunStarted(AnyDefinition, "/tmp", RunTrigger.Manual, "verifier"));

        // act
        projection.Apply(new WorkflowEvent.RunFinished(RunState.Completed));

        // assert
        Assert.False(projection.IsRunning);
        Assert.Equal(RunState.Completed, projection.State);
    }

    [Fact(DisplayName = "étant donné une visite en cours et aucune sélection explicite, quand on lit la sélection, alors c'est la visite en cours")]
    public void The_running_visit_is_selected_by_default()
    {
        // arrange
        var projection = new RunProjection();
        projection.Apply(new WorkflowEvent.RunStarted(AnyDefinition, "/tmp", RunTrigger.Manual, "verifier"));

        // act
        projection.Apply(new WorkflowEvent.StepStarted("A", Iteration: 1));

        // assert — le détail du bas suit par défaut ce qui tourne
        Assert.Equal("A", projection.Selected!.StepId);
        Assert.Equal(1, projection.Selected!.Iteration);
    }

    [Fact(DisplayName = "étant donné une trajectoire à plusieurs visites, quand on sélectionne une visite passée, alors c'est elle la sélection")]
    public void Selecting_a_past_visit_overrides_the_default()
    {
        // arrange — Tester ↺1 close, Tester ↺2 en cours
        var projection = new RunProjection();
        projection.Apply(new WorkflowEvent.RunStarted(AnyDefinition, "/tmp", RunTrigger.Manual, "verifier"));
        projection.Apply(new WorkflowEvent.StepStarted("Tester", 1));
        projection.Apply(new WorkflowEvent.StepFinished("Tester", 1, new ScriptResult(1, ScriptOutcome.Completed), NoOutput));
        projection.Apply(new WorkflowEvent.StepStarted("Tester", 2));
        var past = projection.Trajectory[0];

        // act
        projection.Select(past);

        // assert — la sélection ne suit plus la visite en cours, mais le passé choisi
        Assert.Equal(1, projection.Selected!.Iteration);
        Assert.False(projection.Selected!.IsRunning);
    }

    [Fact(DisplayName = "étant donné un run en cours sans demande d'arrêt, quand on lit le contrôle, alors il est « en cours »")]
    public void A_running_run_without_a_stop_request_is_controlled_as_running()
    {
        // arrange / act
        var projection = Started();

        // assert
        Assert.Equal(RunControl.Running, projection.Control);
    }

    [Fact(DisplayName = "étant donné un run en cours, quand on demande l'arrêt, alors le contrôle passe « arrêt en cours »")]
    public void Requesting_a_stop_moves_the_control_to_stopping()
    {
        // arrange
        var projection = Started();

        // act
        projection.RequestStop();

        // assert — l'arrêt est demandé mais pas encore obtenu (l'étape courante finit)
        Assert.Equal(RunControl.Stopping, projection.Control);
    }

    [Fact(DisplayName = "étant donné une demande d'arrêt, quand on la révoque, alors le contrôle revient « en cours »")]
    public void Revoking_a_stop_returns_the_control_to_running()
    {
        // arrange
        var projection = Started();
        projection.RequestStop();

        // act
        projection.RevokeStop();

        // assert — on repasse par le milieu, on n'y reste pas
        Assert.Equal(RunControl.Running, projection.Control);
    }

    [Fact(DisplayName = "étant donné une demande d'arrêt, quand RunFinished(Aborted, Canceled) arrive, alors le contrôle est « arrêté »")]
    public void A_canceled_finish_lands_the_control_on_stopped()
    {
        // arrange
        var projection = Started();
        projection.RequestStop();

        // act
        projection.Apply(new WorkflowEvent.RunFinished(RunState.Aborted, AbortReason.Canceled));

        // assert
        Assert.Equal(RunControl.Stopped, projection.Control);
    }

    [Fact(DisplayName = "étant donné un run qui se termine normalement, quand RunFinished(Completed) arrive, alors le contrôle n'est ni « en cours » ni « arrêté »")]
    public void A_normal_finish_leaves_no_control_position()
    {
        // arrange
        var projection = Started();

        // act
        projection.Apply(new WorkflowEvent.RunFinished(RunState.Completed));

        // assert — « Arrêté » n'est pas « Réussi » : un run abouti n'a pas de contrôle, il a un verdict
        Assert.Null(projection.Control);
    }

    /// <summary>Une projection sur un run qui vient de démarrer — le décor commun des tests de contrôle.</summary>
    private static RunProjection Started()
    {
        var projection = new RunProjection();
        projection.Apply(new WorkflowEvent.RunStarted(AnyDefinition, "/tmp", RunTrigger.Manual, "verifier"));
        return projection;
    }

    private static StepOutput NoOutput => new([]);

    private static WorkflowDefinition AnyDefinition => new("A", new[]
    {
        new StepDefinition("A", "A", new ScriptSpec("/usr/bin/true", []), MaxVisits: 1, []),
    });
}
