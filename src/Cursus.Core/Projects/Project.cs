using Cursus.Core.Workflows;

namespace Cursus.Core.Projects;

/// <summary>
/// Un projet Cursus : une racine de workspace et la disposition
/// <c>.cursus/</c> qu'elle porte. Seul endroit du dépôt qui sait où atterrissent
/// les fichiers d'un projet — les collaborateurs demandent un chemin, ils n'en
/// composent jamais un.
/// </summary>
public sealed class Project
{
    internal Project(string id, string name, string root)
    {
        Id = id;
        Name = name;
        Root = root;
    }

    /// <summary>
    /// Identité stable, indépendante de l'emplacement : c'est elle qui permettra
    /// au registre machine de distinguer un projet déplacé d'un projet supprimé.
    /// </summary>
    public string Id { get; }

    /// <summary>Libellé destiné à l'humain, sans contrainte d'unicité.</summary>
    public string Name { get; }

    /// <summary>Racine absolue et normalisée du workspace : le dossier qui contient le <c>.cursus/</c>.</summary>
    public string Root { get; }

    public string CursusDirectory => Path.Combine(Root, ProjectStore.DirectoryName);

    public string ProjectFilePath => Path.Combine(CursusDirectory, ProjectStore.FileName);

    public string WorkflowsDirectory => Path.Combine(CursusDirectory, "workflows");

    /// <summary>
    /// Le journal du projet. Une base par projet : l'identité d'un projet est
    /// l'emplacement de son <c>.cursus/</c>, aucune requête ne peut donc en
    /// mélanger deux.
    /// </summary>
    public string DatabasePath => Path.Combine(CursusDirectory, "cursus.db");

    /// <summary>
    /// Les sorties des scripts, voisines du journal qui les référence : elles se
    /// sauvegardent et se détruisent ensemble.
    /// </summary>
    public string ArtifactsRoot => Path.Combine(CursusDirectory, "runs");

    /// <summary>
    /// Le contexte dans lequel les workflows de ce projet s'exécutent : sa
    /// racine est le workspace. C'est la seule jonction entre le projet et le
    /// moteur — la définition, elle, reste portable d'un projet à l'autre.
    /// </summary>
    public RunContext CreateRunContext() => new(Root);
}
