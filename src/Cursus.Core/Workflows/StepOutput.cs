namespace Cursus.Core.Workflows;

/// <summary>
/// La sortie d'une visite, décrite de façon neutre pour le code qui l'entoure :
/// une liste d'artefacts nommés. Une liste, et non une paire figée, pour qu'un
/// futur type d'étape en produise d'autres (transcript, activité) sans reshaper
/// ce type — l'ajout serait une donnée, pas un changement de forme. Ni le moteur
/// ni le journal ne regardent dedans.
/// </summary>
public sealed record StepOutput(IReadOnlyList<OutputArtifact> Artifacts);
