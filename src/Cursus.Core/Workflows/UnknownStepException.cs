namespace Cursus.Core.Workflows;

/// <summary>
/// Levée quand le graphe référence une étape (point d'entrée ou cible d'arête)
/// qui n'existe pas. Au jalon 1 c'est un invariant que le moteur protège ; la
/// validation exhaustive du graphe remontera dans le loader au jalon 3.
/// </summary>
public sealed class UnknownStepException(string stepId)
    : Exception($"Étape inconnue dans le workflow : {stepId}")
{
    public string StepId { get; } = stepId;
}
