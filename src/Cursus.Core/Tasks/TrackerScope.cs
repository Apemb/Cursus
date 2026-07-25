namespace Cursus.Core.Tasks;

/// <summary>
/// Ce qu'une connexion dessert : tout ce que son jeton voit, ou des projets nommés.
/// La variante est un <b>type</b> — jamais une liste vide qui vaudrait « tout », ce
/// qui laisserait représentable un état ambigu (une sélection vide est-elle « rien »
/// ou « tout » ?).
///
/// <para>
/// ⚠️ La portée d'un jeton n'est pas <b>déclarable</b>, elle est <b>constatable</b> :
/// une clé Linear couvre soit le compte, soit un projet, et seule l'interrogation le
/// révèle. Ce type dit donc ce que l'utilisateur a <em>choisi</em> de regarder parmi
/// ce que le jeton lui montrait — pas ce à quoi le jeton donne droit.
/// </para>
/// </summary>
public abstract record TrackerScope
{
    /// <summary>
    /// Ne retient du tableau que ce que cette portée couvre. La règle vit sur le type
    /// plutôt que dans l'écran : elle sert autant à l'affichage qu'au choix d'une tâche
    /// à lancer, et une règle recopiée à deux endroits finit par diverger.
    /// </summary>
    public abstract IReadOnlyList<TaskProject> Filter(IReadOnlyList<TaskProject> projects);

    /// <summary>Tout ce que le jeton voit, sans filtre.</summary>
    public sealed record WholeWorkspace : TrackerScope
    {
        public override IReadOnlyList<TaskProject> Filter(IReadOnlyList<TaskProject> projects) => projects;
    }

    /// <summary>Les seuls projets désignés.</summary>
    public sealed record SelectedProjects(IReadOnlyList<string> ProjectIds) : TrackerScope
    {
        public override IReadOnlyList<TaskProject> Filter(IReadOnlyList<TaskProject> projects) =>
            [.. projects.Where(project => ProjectIds.Contains(project.Id))];
    }
}
