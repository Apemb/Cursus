using System.Text;
using Cursus.Core.Workflows;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// Le puits volatile : le double des tests de traversée, et le défaut sans
/// persistance. Il capture en mémoire ce qu'une visite écrit, et n'a donc pas
/// de chemin sur disque à rendre.
/// </summary>
public class InMemoryRunOutputStoreTests
{
    [Fact(DisplayName = "étant donné un puits en mémoire, quand un flux y est écrit puis clos, alors le contenu capturé se relit et la taille de l'artefact est celle écrite")]
    public void A_written_stream_reads_back_and_reports_its_size()
    {
        // arrange
        var store = new InMemoryRunOutputStore();
        var payload = Encoding.UTF8.GetBytes("bonjour");

        // act
        StepOutput output;
        using (var sink = store.Open("run-1", "compiler", 1))
        {
            sink.Stdout.Write(payload);
            output = sink.Complete();
        }

        // assert
        Assert.Equal(payload, store.Captured("run-1", "compiler", 1, "stdout"));
        Assert.Equal(payload.Length, output.Artifacts.Single(a => a.Name == "stdout").Size);
    }

    [Fact(DisplayName = "étant donné un flux jamais écrit, quand on clôt le puits, alors l'artefact de ce flux a un chemin absent")]
    public void A_stream_never_written_has_no_path()
    {
        // arrange
        var store = new InMemoryRunOutputStore();

        // act
        StepOutput output;
        using (var sink = store.Open("run-1", "compiler", 1))
        {
            sink.Stdout.Write(Encoding.UTF8.GetBytes("seule la sortie standard"));
            output = sink.Complete();
        }

        // assert
        var stderr = output.Artifacts.Single(a => a.Name == "stderr");
        Assert.Null(stderr.Path);
        Assert.Equal(0, stderr.Size);
    }
}
