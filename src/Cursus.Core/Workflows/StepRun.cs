namespace Cursus.Core.Workflows;

/// <summary>
/// Une visite d'une étape dans un run. Un même step engendre N StepRun s'il est
/// dans une boucle (<see cref="Iteration"/> = 1, 2, 3…).
/// </summary>
public sealed record StepRun(string StepId, int Iteration, ScriptResult Result);
