namespace Cursus.Persistence;

/// <summary>Laquelle des deux sorties d'un process on range.</summary>
public enum ArtifactStream
{
    StandardOutput,
    StandardError,
}

/// <summary>
/// Range et relit les sorties d'une visite. Elles ne vont pas en base : un
/// script bavard y ferait grossir le journal sans limite, et un fichier se
/// suit à la trace pendant qu'il s'écrit — ce que voudra l'interface.
/// </summary>
public sealed class RunArtifactStore
{
    private readonly string _root;

    public RunArtifactStore(string root) => _root = root;

    /// <summary>
    /// Range une sortie et rend son chemin, ou <c>null</c> si elle était vide :
    /// la plupart des étapes n'écrivent rien sur stderr, et un fichier vide par
    /// visite noierait le répertoire sans rien apprendre à personne.
    /// </summary>
    public string? Write(string runId, string stepId, int iteration, ArtifactStream stream, string content)
    {
        if (content.Length == 0)
            return null;

        var path = PathFor(runId, stepId, iteration, stream);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }

    /// <summary>Relit une sortie rangée.</summary>
    public string Read(string runId, string stepId, int iteration, ArtifactStream stream) =>
        File.ReadAllText(PathFor(runId, stepId, iteration, stream));

    private string PathFor(string runId, string stepId, int iteration, ArtifactStream stream)
    {
        // Un identifiant d'étape vient d'un fichier de workflow, et le
        // validateur n'en contraint pas la forme : rien n'empêche aujourd'hui
        // un « ../.. » de sortir la sortie du magasin. Le magasin protège donc
        // son propre invariant plutôt que de compter sur un amont.
        EnsureIsASingleSegment(runId, nameof(runId));
        EnsureIsASingleSegment(stepId, nameof(stepId));

        var suffix = stream == ArtifactStream.StandardOutput ? "stdout" : "stderr";
        return Path.Combine(_root, runId, $"{stepId}.{iteration}.{suffix}");
    }

    private static void EnsureIsASingleSegment(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains(Path.DirectorySeparatorChar) ||
            value.Contains(Path.AltDirectorySeparatorChar) || value == "." || value == "..")
        {
            throw new ArgumentException(
                $"Un artefact ne se range que sous un nom simple, jamais un chemin : {value}", parameterName);
        }
    }
}
