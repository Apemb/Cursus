using System.Text;

using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Output;

namespace Cursus.Persistence;

/// <summary>Laquelle des deux sorties d'un process on range.</summary>
public enum ArtifactStream
{
    StandardOutput,
    StandardError,
}

/// <summary>
/// Suiveur d'un artefact : à chaque <see cref="ReadMore"/>, rend ce qui s'est
/// ajouté au fichier depuis la lecture précédente — c'est <b>lui</b> qui retient
/// la position, pas la vue. Tolère un fichier encore absent (flux muet) : la
/// lecture rend alors le vide, puis le contenu dès qu'il apparaît.
/// </summary>
public sealed class ArtifactTail
{
    private readonly string _path;
    private long _position;

    internal ArtifactTail(string path) => _path = path;

    /// <summary>Le texte ajouté depuis la lecture précédente — vide si rien de neuf, ou si le fichier n'existe pas encore.</summary>
    public string ReadMore()
    {
        if (!File.Exists(_path))
            return "";

        // Partage en lecture/écriture : le puits de la visite écrit encore dans
        // ce fichier pendant qu'on le suit.
        using var file = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        if (_position >= file.Length)
            return "";

        file.Seek(_position, SeekOrigin.Begin);
        var pending = new byte[file.Length - _position];
        var read = file.Read(pending, 0, pending.Length);
        _position += read;
        return Encoding.UTF8.GetString(pending, 0, read);
    }
}

/// <summary>
/// Range et relit les sorties d'une visite. Elles ne vont pas en base : un
/// script bavard y ferait grossir le journal sans limite, et un fichier se
/// suit à la trace pendant qu'il s'écrit — ce que voudra l'interface. La sortie
/// y ruisselle par un puits ouvert <b>avant</b> l'étape (<see cref="Open"/>),
/// fichier créé au premier octet, plutôt qu'écrite d'un bloc à la fin.
/// </summary>
public sealed class RunArtifactStore : IRunOutputStore
{
    private readonly string _root;

    public RunArtifactStore(string root) => _root = root;

    /// <summary>
    /// Ouvre le puits d'une visite : deux flux en ajout, chacun ne créant son
    /// fichier qu'au premier octet reçu — un flux muet ne laisse rien.
    /// </summary>
    public IStepOutputSink Open(string runId, string stepId, int iteration) =>
        new FileSink(
            PathFor(runId, stepId, iteration, ArtifactStream.StandardOutput),
            PathFor(runId, stepId, iteration, ArtifactStream.StandardError));

    /// <summary>Relit une sortie rangée.</summary>
    public string Read(string runId, string stepId, int iteration, ArtifactStream stream) =>
        File.ReadAllText(PathFor(runId, stepId, iteration, stream));

    /// <summary>
    /// Ouvre un suiveur sur la sortie d'une visite : de quoi lire par incréments
    /// ce qui s'y ajoute pendant qu'elle tourne (le log en direct). Le fichier
    /// peut ne pas exister encore — un flux muet ne le crée qu'au premier octet.
    /// </summary>
    public ArtifactTail Follow(string runId, string stepId, int iteration, ArtifactStream stream) =>
        new(PathFor(runId, stepId, iteration, stream));

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

    /// <summary>Le puits fichier d'une visite : deux flux paresseux, refermés ensemble.</summary>
    private sealed class FileSink : IStepOutputSink
    {
        private readonly LazyAppendStream _stdout;
        private readonly LazyAppendStream _stderr;

        public FileSink(string stdoutPath, string stderrPath)
        {
            _stdout = new LazyAppendStream(stdoutPath);
            _stderr = new LazyAppendStream(stderrPath);
        }

        public Stream Stdout => _stdout;

        public Stream Stderr => _stderr;

        public StepOutput Complete()
        {
            _stdout.Flush();
            _stderr.Flush();
            return new StepOutput([_stdout.Artifact("stdout"), _stderr.Artifact("stderr")]);
        }

        public void Dispose()
        {
            _stdout.Dispose();
            _stderr.Dispose();
        }
    }

    /// <summary>
    /// Flux en écriture seule qui n'ouvre son fichier qu'au premier octet : un
    /// flux qui ne reçoit rien ne crée aucun fichier et n'a donc pas de chemin.
    /// </summary>
    private sealed class LazyAppendStream : Stream
    {
        private readonly string _path;
        private FileStream? _file;
        private long _written;

        public LazyAppendStream(string path) => _path = path;

        public OutputArtifact Artifact(string name) =>
            new(name, _file is null ? null : _path, _written);

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureOpen();
            _file!.Write(buffer, offset, count);
            _written += count;
            StreamToDisk();
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureOpen();
            _file!.Write(buffer);
            _written += buffer.Length;
            StreamToDisk();
        }

        /// <summary>
        /// Pousse le buffer managé vers le handle OS après chaque écriture, pour
        /// qu'un suiveur (<see cref="ArtifactTail"/>, autre handle) voie la sortie
        /// <b>en direct</b> et pas seulement à la clôture. <c>flushToDisk: false</c> :
        /// on veut la visibilité inter-handle, pas la durabilité (pas de fsync coûteux
        /// à chaque ligne de sortie de test).
        /// </summary>
        private void StreamToDisk() => _file!.Flush(flushToDisk: false);

        private void EnsureOpen()
        {
            if (_file is not null)
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            _file = new FileStream(_path, FileMode.Append, FileAccess.Write);
        }

        public override void Flush() => _file?.Flush();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _file?.Dispose();
            base.Dispose(disposing);
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
