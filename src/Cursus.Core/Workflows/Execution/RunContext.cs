namespace Cursus.Core.Workflows.Execution;

/// <summary>
/// La racine du workspace dans laquelle se déroule un run, et la résolution
/// des sous-chemins déclarés par les étapes. Aucun script ne tourne hors de
/// cette racine ; elle n'appartient pas à la définition du workflow, qui reste
/// portable d'un projet à l'autre.
/// </summary>
public sealed class RunContext
{
    public RunContext(string workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(workspaceRoot))
            throw new ArgumentException("La racine du workspace est obligatoire.", nameof(workspaceRoot));

        if (!Path.IsPathRooted(workspaceRoot))
            throw new ArgumentException(
                $"La racine du workspace doit être un chemin absolu : {workspaceRoot}", nameof(workspaceRoot));

        WorkspaceRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(workspaceRoot));

        if (!Directory.Exists(WorkspaceRoot))
            throw new DirectoryNotFoundException(
                $"La racine du workspace n'existe pas : {WorkspaceRoot}");
    }

    /// <summary>Racine absolue et normalisée : tous les scripts du run s'exécutent dedans.</summary>
    public string WorkspaceRoot { get; }

    /// <summary>
    /// Traduit le sous-chemin relatif déclaré par une étape en répertoire de
    /// travail absolu. Sans sous-chemin, l'étape tourne à la racine.
    /// </summary>
    public string Resolve(string? subdirectory)
    {
        if (string.IsNullOrWhiteSpace(subdirectory))
            return WorkspaceRoot;

        var resolved = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(Path.Combine(WorkspaceRoot, subdirectory)));

        // Le préfixe seul ne suffit pas : un voisin nommé « <racine>-autre »
        // commence par la racine sans être dedans. D'où le séparateur exigé.
        if (resolved != WorkspaceRoot &&
            !resolved.StartsWith(WorkspaceRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new PathEscapesWorkspaceException(subdirectory, WorkspaceRoot);

        return resolved;
    }
}
