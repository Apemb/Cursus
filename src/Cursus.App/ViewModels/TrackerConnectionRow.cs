using System;

using Cursus.Core.Tasks;

namespace Cursus.App.ViewModels;

/// <summary>
/// Une connexion enregistrée, telle qu'elle se lit dans la liste. La ligne est
/// <b>polymorphe</b> — même patron que <c>StepEditorRow</c> (<c>D-031</c>) : ce qui
/// identifie une connexion dépend de son tracker, et un « espace » n'a de sens que
/// pour Linear. Chaque sous-type affiche donc ce qu'il possède, sans champ vide.
///
/// <para>
/// ⚠️ Jamais le jeton — une connexion configurée ne réaffiche pas son secret, sous
/// aucun prétexte.
/// </para>
/// </summary>
public abstract class TrackerConnectionRow(TrackerConnection connection)
{
    public TrackerConnection Connection { get; } = connection;

    public string Label => Connection.Label;

    /// <summary>Ce à quoi cette connexion donne accès, en clair — pour distinguer deux jetons l'un de l'autre.</summary>
    public abstract string ScopeLabel { get; }

    /// <summary>
    /// La ligne qui convient à cette connexion. La fabrique vit ici plutôt qu'à
    /// l'appelant : ajouter un tracker doit se voir à un seul endroit.
    /// </summary>
    public static TrackerConnectionRow For(TrackerConnection connection) => connection switch
    {
        LinearConnection linear => new LinearConnectionRow(linear),
        _ => throw new NotSupportedException(
            $"Aucune ligne d'affichage pour {connection.GetType().Name}."),
    };
}

/// <summary>Une connexion Linear se reconnaît à son espace : « Cursus · cursus-app ».</summary>
public sealed class LinearConnectionRow(LinearConnection connection) : TrackerConnectionRow(connection)
{
    public override string ScopeLabel =>
        $"Linear · {connection.Workspace.Name} ({connection.Workspace.Key})";
}
