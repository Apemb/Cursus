namespace Cursus.Core.Tasks;

/// <summary>
/// Une connexion à un tracker : un jeton nommé, et ce qu'il dessert.
///
/// <para>
/// ⚠️ <b>Le jeton n'est pas ici.</b> Cet objet s'écrit en clair dans le registre
/// machine ; le secret, lui, vit au trousseau sous une clé dérivée de
/// <see cref="Id"/>. C'est aussi pourquoi l'identifiant existe : deux connexions
/// peuvent viser le même espace (une clé de compte et une clé de projet), et une
/// clé de trousseau indexée par espace les ferait s'écraser l'une l'autre.
/// </para>
/// </summary>
public sealed record TrackerConnection(string Id, string Label, TrackerScope Scope)
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
