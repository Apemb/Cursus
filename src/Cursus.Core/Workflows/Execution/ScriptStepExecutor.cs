namespace Cursus.Core.Workflows.Execution;

/// <summary>
/// L'exécuteur du <see cref="ScriptStep"/> : applique le répertoire résolu au
/// <see cref="ScriptSpec"/> déclaré, puis le lance via l'<see cref="IProcessRunner"/>.
/// C'est ici que vit ce que le moteur faisait en dur avant l'abstraction des kinds.
/// </summary>
public sealed class ScriptStepExecutor : IStepExecutor
{
    private readonly IProcessRunner _runner;

    public ScriptStepExecutor(IProcessRunner runner) => _runner = runner;

    public bool CanExecute(StepDefinition step) => step is ScriptStep;

    public Task<ScriptResult> ExecuteAsync(
        StepDefinition step,
        string workingDirectory,
        Stream stdout,
        Stream stderr,
        CancellationToken cancellationToken)
    {
        // Le moteur a déjà absolutisé le cwd (§4.3) ; on le pose sur le ScriptSpec
        // juste avant le lancement — le with non destructif garde la définition intacte.
        var script = ((ScriptStep)step).Script with { WorkingDirectory = workingDirectory };
        return _runner.RunAsync(script, stdout, stderr, cancellationToken);
    }
}
