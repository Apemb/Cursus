using Cursus.Core.Workflows;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// La preuve du pari central sur le cas agent : le moteur greffe le nouveau kind
/// sans que sa traversée change — il le confie à l'exécuteur qui sait le prendre, et
/// le route par les gardes existantes exactement comme une étape-script.
/// </summary>
public class AgentStepEngineTests
{
    [Fact(DisplayName = "étant donné un workflow d'une étape-agent, quand le moteur l'exécute, alors il la confie à l'exécuteur d'agent (claude est invoqué)")]
    public async Task The_engine_dispatches_an_agent_step_to_the_agent_executor()
    {
        // arrange
        var runner = new StubProcessRunner(new ScriptResult(0, ScriptOutcome.Completed));
        var engine = WorkflowFixtures.Engine(runner);
        var definition = new WorkflowDefinition("corriger", new StepDefinition[]
        {
            new AgentStep("corriger", "Corriger", "Claude Code", "opus", "Corrige", MaxVisits: 1, []),
        });

        // act
        var run = await engine.ExecuteAsync(
            definition, WorkflowFixtures.Workspace, WorkflowFixtures.NewRunId());

        // assert — le runner a vu l'invocation claude : l'agent a bien été routé sur son exécuteur
        Assert.Equal("claude", Assert.Single(runner.Executed).FileName);
        Assert.Equal(RunState.Completed, run.State);
    }

    [Fact(DisplayName = "étant donné une étape-agent réussie suivie d'une arête de succès, quand le moteur l'exécute, alors il route vers la cible comme pour toute étape")]
    public async Task An_agent_step_is_routed_by_the_existing_guards()
    {
        // arrange — agent (exit 0) --success--> rapport (script)
        var runner = new StubProcessRunner(new ScriptResult(0, ScriptOutcome.Completed));
        var engine = WorkflowFixtures.Engine(runner);
        var definition = new WorkflowDefinition("corriger", new StepDefinition[]
        {
            new AgentStep("corriger", "Corriger", "Claude Code", "opus", "Corrige",
                MaxVisits: 1, [new Edge(Guard.OnSuccess, "rapport")]),
            new ScriptStep("rapport", "Rapport", new ScriptSpec("/usr/bin/true", []), MaxVisits: 1, []),
        });

        // act
        var run = await engine.ExecuteAsync(
            definition, WorkflowFixtures.Workspace, WorkflowFixtures.NewRunId());

        // assert — les deux étapes ont été visitées dans l'ordre : l'arête de succès a porté
        Assert.Equal(RunState.Completed, run.State);
        Assert.Equal(["corriger", "rapport"], run.History.Select(h => h.StepId));
    }
}
