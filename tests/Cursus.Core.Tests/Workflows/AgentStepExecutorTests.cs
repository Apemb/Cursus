using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Execution;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// L'exécuteur de l'étape-agent : il ne route rien, il <b>lance</b> — traduire le prompt
/// et le modèle en une invocation headless de « claude », et rendre son code de sortie.
/// </summary>
public class AgentStepExecutorTests
{
    [Fact(DisplayName = "étant donné une étape-agent, quand l'exécuteur la lance, alors il invoque claude en headless avec le modèle et le prompt, dans le répertoire résolu")]
    public async Task It_invokes_claude_headless_with_model_and_prompt()
    {
        // arrange
        var runner = new StubProcessRunner(new ScriptResult(0, ScriptOutcome.Completed));
        var executor = new AgentStepExecutor(runner);
        var step = new AgentStep(
            "corriger", "Corriger", "Claude Code", "opus", "Corrige les tests rouges", MaxVisits: 1, []);
        using var stdout = new MemoryStream();
        using var stderr = new MemoryStream();

        // act
        var result = await executor.ExecuteAsync(
            step, new StepExecutionContext("/tmp/run"), stdout, stderr, CancellationToken.None);

        // assert
        var spec = Assert.Single(runner.Executed);
        Assert.Equal("claude", spec.FileName);
        Assert.Equal(["--model", "opus", "-p", "Corrige les tests rouges"], spec.Arguments);
        Assert.Equal("/tmp/run", spec.WorkingDirectory);
        Assert.True(result.IsSuccess);
    }

    [Fact(DisplayName = "étant donné un exécuteur d'agent, quand on lui présente une étape-script, alors il ne la prend pas — mais il prend une étape-agent")]
    public void It_claims_agent_steps_only()
    {
        // arrange
        var executor = new AgentStepExecutor(new StubProcessRunner());

        // act / assert
        Assert.False(executor.CanExecute(
            new ScriptStep("a", "a", new ScriptSpec("/usr/bin/true", []), MaxVisits: 1, [])));
        Assert.True(executor.CanExecute(
            new AgentStep("a", "a", "Claude Code", "opus", "x", MaxVisits: 1, [])));
    }
}
