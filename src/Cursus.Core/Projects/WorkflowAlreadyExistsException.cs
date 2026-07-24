namespace Cursus.Core.Projects;

/// <summary>
/// Levée quand une écriture refuserait d'écraser une identité déjà prise — créer
/// un workflow sous un identifiant existant, ou renommer vers une cible occupée.
/// Écraser changerait silencieusement le contenu d'un autre workflow ; le
/// catalogue préfère refuser et laisser l'appelant trancher.
/// </summary>
public sealed class WorkflowAlreadyExistsException(string id)
    : Exception($"Un workflow porte déjà l'identifiant « {id} ».")
{
    public string Id { get; } = id;
}
