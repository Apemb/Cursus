namespace Cursus.Core.Tasks;

/// <summary>
/// Une connexion à un tracker : un jeton nommé, et ce qu'il dessert.
///
/// <para>
/// La variante est un <b>type</b>, chaque sous-type ne portant que ce qui identifie
/// une connexion <em>chez son tracker</em> — un espace pour Linear, un site et un
/// projet pour Jira le jour venu, tout non-nul. Le générique ne porte donc que ce qui
/// est vrai partout : un identifiant, un nom. Loger le <c>Workspace</c> ici en ferait
/// un champ vide pour tout tracker qui n'en a pas.
/// </para>
///
/// <para>
/// ⚠️ <b>Le jeton n'est pas ici.</b> Cet objet s'écrit en clair dans le registre
/// machine ; le secret, lui, vit au trousseau sous <see cref="SecretKey"/>.
/// </para>
/// </summary>
public abstract record TrackerConnection(string Id, string Label)
{
    /// <summary>
    /// La clé sous laquelle le trousseau garde le jeton de cette connexion. Elle vit
    /// ici plutôt que chez l'adaptateur : laisser chaque appelant la composer, c'est
    /// laisser deux d'entre eux la composer différemment — et un jeton rangé sous une
    /// clé, relu sous une autre, se manifeste par un « aucun jeton configuré » que rien
    /// n'explique.
    /// </summary>
    public string SecretKey => $"tracker:{Id}";
}

/// <summary>
/// Une connexion Linear. Elle est identifiée par son <b>espace</b> : une clé Linear
/// est attachée à exactement un workspace, constaté à la saisie et jamais choisi.
///
/// <para>
/// Le type vit en Core bien que l'adaptateur HTTP vive ailleurs — c'est de la
/// <em>donnée</em>, au même titre que <c>AgenticHarness.ClaudeCode</c> nomme un
/// harnais concret sans l'implémenter.
/// </para>
/// </summary>
public sealed record LinearConnection(string Id, string Label, TrackerWorkspace Workspace)
    : TrackerConnection(Id, Label);
