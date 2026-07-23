namespace Cursus.Core.Workflows.Execution;

/// <summary>
/// Rend résoluble, depuis l'app installée au <c>PATH</c> graphique tronqué
/// (§9.2-15), un binaire d'<c>asdf</c>/Homebrew/<c>git</c>. <see cref="ProcessRunner"/>
/// ne lance aucun shell de login ; cette stratégie <b>enrichit</b> le <c>PATH</c>
/// de racines connues, en les <b>ajoutant</b> — jamais en retirant ni en réordonnant
/// l'existant, pour qu'un run qui marchait déjà (dev, <c>dotnet test</c>, où le
/// <c>PATH</c> est complet) se comporte à l'identique.
/// </summary>
public sealed class PathStrategy
{
    private readonly IReadOnlyList<string> _knownRoots;

    public PathStrategy(IReadOnlyList<string> knownRoots) => _knownRoots = knownRoots;

    /// <summary>
    /// Les racines usuelles d'une machine de dev macOS/Linux qu'un <c>PATH</c>
    /// graphique tronqué laisse tomber : les shims d'<c>asdf</c>, les <c>bin</c> de
    /// Homebrew (Apple Silicon et Intel), et les emplacements système standards.
    /// Ajoutées en queue, elles ne priment jamais sur ce que l'utilisateur a réglé.
    /// </summary>
    public static PathStrategy Default
    {
        get
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return new PathStrategy(
            [
                Path.Combine(home, ".asdf", "shims"),
                "/opt/homebrew/bin",
                "/usr/local/bin",
                "/usr/bin",
                "/bin",
            ]);
        }
    }

    /// <summary>
    /// Le <c>PATH</c> courant, suivi des racines connues encore absentes. L'existant
    /// garde sa place et sa précédence ; on ne fait qu'ajouter en queue. Sert aux
    /// <b>petits-fils</b> : un <c>npm</c> lancé par une étape y trouvera son <c>node</c>.
    /// </summary>
    public string Enrich(string? currentPath)
    {
        var entries = (currentPath ?? "").Split(Path.PathSeparator);
        var missing = _knownRoots.Where(root => !entries.Contains(root));
        return string.Join(Path.PathSeparator, entries.Concat(missing));
    }

    /// <summary>
    /// Le chemin <b>absolu</b> d'une commande nue, cherchée dans le <c>PATH</c> courant
    /// puis les racines connues — car .NET ne consulte pas le <c>PATH</c> de
    /// <c>StartInfo.Environment</c> pour résoudre l'exécutable direct (constaté). Une
    /// commande déjà chemin (contenant un séparateur) est rendue telle quelle ; une
    /// commande introuvable aussi, pour laisser <see cref="ProcessRunner"/> produire un
    /// <c>LaunchFailed</c> net plutôt que de masquer l'échec.
    /// </summary>
    public string Resolve(string command, string? currentPath)
    {
        if (command.Contains(Path.DirectorySeparatorChar))
            return command;

        var directories = (currentPath ?? "").Split(Path.PathSeparator).Concat(_knownRoots);
        foreach (var directory in directories)
        {
            if (directory.Length == 0)
                continue;

            var candidate = Path.Combine(directory, command);
            if (File.Exists(candidate))
                return candidate;
        }

        return command;
    }
}
