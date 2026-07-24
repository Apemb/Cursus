namespace Cursus.Core.Workflows;

/// <summary>
/// Un modèle offert par un <see cref="AgenticHarness"/> : son identifiant stable
/// (« opus » — ce qui est tapé dans le document et passé au binaire) et son libellé
/// d'affichage (« Opus »), ce que l'UI met dans son menu déroulant.
/// </summary>
public sealed record AgentModel(string Id, string Label);
