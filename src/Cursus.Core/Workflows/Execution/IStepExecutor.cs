namespace Cursus.Core.Workflows.Execution;

/// <summary>
/// Exécute le travail propre à un <b>type</b> d'étape et en rend le
/// <see cref="ScriptResult"/>, routable par les gardes existantes. Le moteur choisit
/// l'exécuteur par le type de l'étape (<see cref="CanExecute"/>), sans jamais connaître
/// les kinds : ajouter un kind, c'est ajouter un exécuteur avec <b>ses propres</b>
/// collaborateurs, pas toucher la traversée — le pari central du pivot (§5).
/// </summary>
public interface IStepExecutor
{
    /// <summary>Vrai si cet exécuteur sait exécuter cette étape — jugé sur son type.</summary>
    bool CanExecute(StepDefinition step);

    /// <summary>
    /// Exécute l'étape dans le répertoire déjà résolu par le moteur (§4.3), en
    /// ruisselant les sorties vers les deux flux fournis (le puits ouvert avant la
    /// visite). Rend l'issue du travail, que le moteur routera.
    /// </summary>
    Task<ScriptResult> ExecuteAsync(
        StepDefinition step,
        string workingDirectory,
        Stream stdout,
        Stream stderr,
        CancellationToken cancellationToken);
}
