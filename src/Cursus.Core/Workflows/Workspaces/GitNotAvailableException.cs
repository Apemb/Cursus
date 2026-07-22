namespace Cursus.Core.Workflows.Workspaces;

/// <summary>
/// Levée quand le provisionnement échoue faute de pouvoir lancer <c>git</c> :
/// git n'est pas installé, ou pas sur le <c>PATH</c>. Une erreur explicite
/// plutôt qu'un échec de process brut — c'est un prérequis de Cursus lui-même,
/// pas une défaillance du run.
/// </summary>
public sealed class GitNotAvailableException()
    : Exception("git est introuvable : la commande n'a pas pu être lancée. " +
        "Vérifiez que git est installé et présent sur le PATH.");
