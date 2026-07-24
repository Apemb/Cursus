namespace Cursus.Core.Workflows.Execution;

/// <summary>
/// L'exécuteur de l'<see cref="AgentStep"/> : traduit le modèle et le prompt en une
/// invocation <b>headless</b> de « claude » (<c>claude --model … -p …</c>, tubes
/// redirigés, code de sortie), la lance via l'<see cref="IProcessRunner"/>, et en rend le
/// <see cref="ScriptResult"/> — routable par les gardes comme n'importe quelle étape.
///
/// <para>
/// Claude Code est le seul harness du jour, donc l'invocation y est câblée ici ; un
/// second harness amènerait sa propre traduction (par harness ou par stratégie). Le
/// binaire est passé <b>nu</b> : c'est le <see cref="ProcessRunner"/> qui applique la
/// <see cref="PathStrategy"/> au lancement, de sorte qu'un <c>PATH</c> GUI tronqué ne
/// l'empêche pas de le résoudre.
/// </para>
/// </summary>
public sealed class AgentStepExecutor : IStepExecutor
{
    private const string ClaudeBinary = "claude";

    private readonly IProcessRunner _runner;

    public AgentStepExecutor(IProcessRunner runner) => _runner = runner;

    public bool CanExecute(StepDefinition step) => step is AgentStep;

    public Task<ScriptResult> ExecuteAsync(
        StepDefinition step,
        StepExecutionContext context,
        Stream stdout,
        Stream stderr,
        CancellationToken cancellationToken)
    {
        var agent = (AgentStep)step;
        var spec = new ScriptSpec(
            ClaudeBinary,
            ["--model", agent.ModelId, "-p", agent.Prompt],
            WorkingDirectory: context.WorkingDirectory);
        return _runner.RunAsync(spec, stdout, stderr, cancellationToken);
    }
}
