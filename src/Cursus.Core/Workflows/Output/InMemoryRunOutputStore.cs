namespace Cursus.Core.Workflows.Output;

/// <summary>
/// Puits volatile : capture en mémoire ce qu'une visite écrit, sans chemin sur
/// disque. Le double des tests de traversée, et le défaut du moteur quand rien
/// n'est persisté — parallèle d'<see cref="InMemoryRunJournal"/>.
/// </summary>
public sealed class InMemoryRunOutputStore : IRunOutputStore
{
    private readonly Dictionary<string, InMemorySink> _sinks = [];

    public IStepOutputSink Open(string runId, string stepId, int iteration)
    {
        var sink = new InMemorySink();
        _sinks[Key(runId, stepId, iteration)] = sink;
        return sink;
    }

    /// <summary>Les octets capturés pour un flux d'une visite — de quoi assert.</summary>
    public byte[] Captured(string runId, string stepId, int iteration, string name) =>
        _sinks[Key(runId, stepId, iteration)].Bytes(name);

    private static string Key(string runId, string stepId, int iteration) =>
        $"{runId}/{stepId}/{iteration}";

    private sealed class InMemorySink : IStepOutputSink
    {
        private readonly MemoryStream _stdout = new();
        private readonly MemoryStream _stderr = new();

        public Stream Stdout => _stdout;

        public Stream Stderr => _stderr;

        public StepOutput Complete() => new([
            Artifact("stdout", _stdout),
            Artifact("stderr", _stderr),
        ]);

        // ToArray fonctionne même après Dispose du flux — l'assertion vient après
        // le using, quand la visite est close.
        public byte[] Bytes(string name) => name switch
        {
            "stdout" => _stdout.ToArray(),
            "stderr" => _stderr.ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Flux inconnu."),
        };

        private static OutputArtifact Artifact(string name, MemoryStream stream) =>
            new(name, Path: null, Size: stream.Length);

        public void Dispose()
        {
            _stdout.Dispose();
            _stderr.Dispose();
        }
    }
}
