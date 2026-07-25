namespace Cursus.Core.Secrets;

/// <summary>
/// Le port du trousseau : range et rend un secret sous une clé. Un token ne
/// s'écrit jamais sur disque en clair, même hors dépôt (§7.10.1) — d'où un port
/// dédié plutôt qu'un champ de configuration de plus.
///
/// <para>
/// La clé est <b>opaque</b> pour ce port ; sa convention
/// (<c>&lt;provider&gt;:&lt;connexion&gt;</c>) appartient à l'appelant. Elle ne porte
/// pas le projet, parce que <b>le token appartient au compte, pas au projet</b> : cinq
/// dépôts pilotés depuis le même Linear partagent une seule saisie.
/// </para>
/// </summary>
public interface ISecretStore
{
    /// <summary>
    /// Le secret rangé sous cette clé, ou <c>null</c> s'il n'y en a pas. L'absence
    /// est un cas nominal — « aucun token configuré » est un état ordinaire de
    /// l'application, pas une erreur.
    /// </summary>
    Task<string?> ReadAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Range un secret sous cette clé, en remplaçant celui qui s'y trouverait.</summary>
    Task WriteAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Efface le secret rangé sous cette clé. <b>Idempotent</b> : effacer ce qui n'est
    /// pas là n'est pas un échec. Retirer une connexion doit emporter son jeton —
    /// sinon le trousseau accumule des secrets que plus rien ne désigne.
    /// </summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
