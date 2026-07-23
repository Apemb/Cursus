namespace Cursus.Core.Workflows.Projection;

/// <summary>
/// Le contrôle d'un run n'est pas un bouton mais un <b>état à trois positions</b>
/// (parcours §1.4). « Arrêter » est instantané, or arrêter proprement veut dire
/// laisser l'étape courante finir et n'en démarrer aucune autre : il existe donc
/// un moment où l'arrêt est <b>demandé mais pas obtenu</b>. Et « Arrêté » n'est
/// pas « Échoué » — le noyau les sépare déjà (<c>Aborted / Canceled</c>).
/// </summary>
public enum RunControl
{
    /// <summary>Le run avance ; aucune demande d'arrêt.</summary>
    Running,

    /// <summary>L'arrêt est demandé, l'étape courante finit — la position du milieu, où l'on passe sans la choisir.</summary>
    Stopping,

    /// <summary>Le run s'est arrêté sur demande (<c>Aborted / Canceled</c>).</summary>
    Stopped,
}
