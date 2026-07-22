namespace Cursus.Core.Projects;

/// <summary>
/// La racine machine, au-dessus des projets : la liste des projets connus de
/// cette installation de Cursus, indépendante de tout projet particulier. C'est
/// la première pierre du registre machine que <see cref="Project.Id"/> anticipe.
/// </summary>
public sealed class ProjectRegistry
{
    private readonly List<Project> _projects = [];

    public ProjectRegistry(string configDirectory)
    {
        // Le dossier de configuration portera la liste persistée ; aucun
        // comportement ne l'exige encore (il arrive au comportement « la
        // machine se souvient »).
    }

    public IReadOnlyList<Project> Projects => _projects;

    /// <summary>
    /// Inscrit un projet désigné par la racine de son workspace. La validité
    /// « c'est un projet Cursus » est l'invariant de <see cref="ProjectStore"/> :
    /// on l'ouvre, et on laisse remonter son refus.
    /// </summary>
    public void Add(string projectRoot)
    {
        var project = ProjectStore.Open(projectRoot);

        // La racine que rend ProjectStore.Open est déjà absolue et normalisée :
        // deux formes du même chemin s'y ramènent, donc la comparaison suffit à
        // ne pas inscrire deux fois le même projet.
        if (_projects.Exists(inscribed => inscribed.Root == project.Root))
            return;

        _projects.Add(project);
    }

    /// <summary>
    /// Retire un projet de la liste. Ne touche jamais au dépôt qu'il désigne :
    /// oublier un projet et le supprimer sont deux gestes distincts. On normalise
    /// le chemin nous-mêmes plutôt que de passer par <see cref="ProjectStore"/> —
    /// un projet devenu illisible doit rester retirable.
    /// </summary>
    public void Remove(string projectRoot)
    {
        var root = Path.GetFullPath(projectRoot);
        _projects.RemoveAll(inscribed => inscribed.Root == root);
    }
}
