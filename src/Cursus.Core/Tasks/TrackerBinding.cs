namespace Cursus.Core.Tasks;

/// <summary>
/// Ce qu'un projet Cursus <b>déclare viser</b> : le tableau où vivent ses tickets, écrit
/// dans son <c>project.json</c> versionné.
///
/// <para>
/// C'est la moitié <b>partageable</b> du lien projet ↔ tracker, et c'est pour cela
/// qu'elle vit dans le dépôt : elle voisine les prédicats de disponibilité, qui nomment
/// des colonnes et seraient à moitié partagés si l'espace où trouver ces colonnes était
/// un réglage machine invisible. L'autre moitié — le jeton — est une
/// <see cref="TrackerConnection"/> du registre machine.
/// </para>
///
/// <para>
/// ⚠️ Séparer les deux n'est pas une complication : c'est ce qui rend une <b>divergence
/// observable</b>. Un appariement rangé au seul registre machine <em>est</em> la vérité,
/// donc il ne peut jamais être faux ; une déclaration versionnée, elle, crée un écart
/// visible entre ce que le dépôt dit viser et ce que ce poste sait joindre — la forme
/// d'erreur qui coûterait cher autrement, un run déplaçant une carte dans le mauvais
/// espace sans qu'un mot l'ait annoncé.
/// </para>
/// </summary>
public abstract record TrackerBinding
{
    /// <summary>
    /// Vrai quand cette connexion dessert ce que la déclaration vise.
    ///
    /// <para>
    /// Abstrait à dessein : chaque tracker reconnaît les siennes à sa façon, et cette
    /// connaissance doit rester chez lui. La faire tenir par un service qui
    /// discriminerait les genres concentrerait au même endroit ce que la découpe par le
    /// type disperse justement — et l'écran, lui, n'a jamais à nommer Linear.
    /// </para>
    /// </summary>
    public abstract bool Matches(TrackerConnection connection);
}

/// <summary>
/// Un projet qui suit ses tickets sur un espace Linear.
/// </summary>
/// <param name="WorkspaceKey">
/// La clé <b>lisible</b> de l'espace (« cursus-app »), et non son identifiant opaque :
/// ce document est relu en revue, un identifiant y serait muet. Contrepartie assumée —
/// renommer l'espace chez Linear rompt l'appariement, ce qui se signale comme une
/// divergence au lieu de suivre en silence.
/// </param>
public sealed record LinearBinding(string WorkspaceKey) : TrackerBinding
{
    public override bool Matches(TrackerConnection connection) =>
        connection is LinearConnection linear && linear.Workspace.Key == WorkspaceKey;
}
