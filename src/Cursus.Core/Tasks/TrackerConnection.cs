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
public sealed record TrackerConnection(string Id, string Label, TrackerScope Scope);
