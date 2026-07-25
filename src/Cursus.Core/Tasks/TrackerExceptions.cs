namespace Cursus.Core.Tasks;

/// <summary>
/// Aucun jeton n'est rangé pour ce tableau : Cursus n'est pas relié au tracker. État
/// <b>ordinaire</b> d'une installation neuve, pas une panne — la surface doit
/// l'attraper pour inviter à configurer, jamais l'afficher comme une erreur.
/// </summary>
public sealed class TrackerNotConfiguredException()
    : Exception("Aucun jeton n'est configuré pour cette connexion.");

/// <summary>
/// Un jeton est bien rangé, mais le tracker le refuse — révoqué, expiré, ou collé
/// incomplet. À distinguer de <see cref="TrackerUnreachableException"/> : ici le
/// tableau <b>a</b> répondu, et le remède est de refaire une clé, pas de vérifier sa
/// connexion. Confondre les deux envoie l'utilisateur chercher au mauvais endroit.
/// </summary>
public sealed class TrackerRejectedException()
    : Exception("Le jeton a été refusé par le tracker — il est peut-être révoqué, expiré, ou collé incomplet.");

/// <summary>
/// Le tableau n'a pas répondu, ou a répondu une erreur. Exception <b>de domaine</b>
/// (elle vit dans le noyau, pas dans l'adaptateur) pour que la surface l'attrape sans
/// rien connaître de HTTP ni de GraphQL : le jour où un second tracker arrive, elle
/// n'a pas une seconde famille d'exceptions à apprendre.
/// </summary>
public sealed class TrackerUnreachableException(string reason)
    : Exception($"Le tableau de tâches n'a pas pu être interrogé : {reason}");
