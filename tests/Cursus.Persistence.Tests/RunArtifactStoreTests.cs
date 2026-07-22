using System.Text;
using Cursus.Core.Workflows;

namespace Cursus.Persistence.Tests;

/// <summary>
/// Où atterrissent les sorties d'une visite, et ce qui les empêche d'atterrir
/// ailleurs. Le magasin est le seul endroit du système qui connaît ces chemins.
/// </summary>
public class RunArtifactStoreTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("cursus-artifacts-").FullName;

    [Fact(DisplayName = "étant donné un puits, quand on écrit sur sa sortie standard puis qu'on le clôt, alors un fichier atterrit sous l'identifiant du run, à l'étape et à l'itération attendues")]
    public void An_opened_sink_lands_standard_output_under_the_run_the_step_and_the_iteration()
    {
        // arrange
        var store = new RunArtifactStore(_root);

        // act
        using (var sink = store.Open("run-1", "compiler", iteration: 2))
        {
            sink.Stdout.Write(Encoding.UTF8.GetBytes("bonjour"));
            sink.Complete();
        }

        // assert
        Assert.True(File.Exists(Path.Combine(_root, "run-1", "compiler.2.stdout")));
    }

    [Fact(DisplayName = "étant donné une sortie rangée par le puits, quand on la relit, alors on retrouve son contenu à l'octet près")]
    public void A_sinked_output_reads_back_byte_for_byte()
    {
        // arrange
        var store = new RunArtifactStore(_root);
        const string content = "ligne une\nligne deux — accentuée\n\tindentée\n";

        // act
        using (var sink = store.Open("run-1", "compiler", 1))
        {
            sink.Stdout.Write(Encoding.UTF8.GetBytes(content));
            sink.Complete();
        }

        // assert
        Assert.Equal(content, store.Read("run-1", "compiler", 1, ArtifactStream.StandardOutput));
    }

    [Fact(DisplayName = "étant donné plusieurs écritures successives sur un même flux, quand on relit, alors elles sont toutes présentes dans l'ordre")]
    public void Successive_writes_to_a_stream_are_all_appended_in_order()
    {
        // arrange
        var store = new RunArtifactStore(_root);

        // act — le fichier est ouvert en ajout au premier octet, pas écrasé au suivant
        using (var sink = store.Open("run-1", "compiler", 1))
        {
            sink.Stdout.Write(Encoding.UTF8.GetBytes("premier "));
            sink.Stdout.Write(Encoding.UTF8.GetBytes("puis "));
            sink.Stdout.Write(Encoding.UTF8.GetBytes("second"));
            sink.Complete();
        }

        // assert
        Assert.Equal("premier puis second", store.Read("run-1", "compiler", 1, ArtifactStream.StandardOutput));
    }

    [Fact(DisplayName = "étant donné un flux sur lequel on n'écrit rien, quand on clôt le puits, alors aucun fichier n'est créé et l'artefact de ce flux a un chemin absent")]
    public void A_stream_never_written_creates_no_file_and_has_no_path()
    {
        // arrange
        var store = new RunArtifactStore(_root);

        // act
        StepOutput output;
        using (var sink = store.Open("run-1", "compiler", 1))
        {
            output = sink.Complete();
        }

        // assert
        Assert.False(File.Exists(Path.Combine(_root, "run-1", "compiler.1.stderr")));
        Assert.Null(output.Artifacts.Single(a => a.Name == "stderr").Path);
    }

    [Fact(DisplayName = "étant donné une visite dont un seul des deux flux reçoit de la sortie, quand on clôt le puits, alors seul ce flux a un fichier, l'autre a un chemin absent")]
    public void Only_the_stream_that_received_output_has_a_file()
    {
        // arrange
        var store = new RunArtifactStore(_root);

        // act
        StepOutput output;
        using (var sink = store.Open("run-1", "compiler", 1))
        {
            sink.Stderr.Write(Encoding.UTF8.GetBytes("ce qui va mal"));
            output = sink.Complete();
        }

        // assert
        Assert.NotNull(output.Artifacts.Single(a => a.Name == "stderr").Path);
        Assert.Null(output.Artifacts.Single(a => a.Name == "stdout").Path);
        Assert.False(File.Exists(Path.Combine(_root, "run-1", "compiler.1.stdout")));
    }

    [Fact(DisplayName = "étant donné deux itérations d'une même étape, quand chacune range sa sortie, alors aucune n'écrase l'autre")]
    public void Two_iterations_of_the_same_step_do_not_overwrite_each_other_through_the_sink()
    {
        // arrange
        var store = new RunArtifactStore(_root);

        // act
        using (var first = store.Open("run-1", "compiler", 1))
        {
            first.Stdout.Write(Encoding.UTF8.GetBytes("premier tour"));
            first.Complete();
        }

        using (var second = store.Open("run-1", "compiler", 2))
        {
            second.Stdout.Write(Encoding.UTF8.GetBytes("second tour"));
            second.Complete();
        }

        // assert
        Assert.Equal("premier tour", store.Read("run-1", "compiler", 1, ArtifactStream.StandardOutput));
        Assert.Equal("second tour", store.Read("run-1", "compiler", 2, ArtifactStream.StandardOutput));
    }

    [Fact(DisplayName = "étant donné un identifiant d'étape contenant un séparateur de chemin, quand on ouvre un puits pour lui, alors l'ouverture est refusée")]
    public void Opening_a_sink_for_a_step_identifier_that_walks_the_filesystem_is_refused()
    {
        // arrange
        var store = new RunArtifactStore(_root);

        // act / assert
        Assert.Throws<ArgumentException>(() => store.Open("run-1", "../../ailleurs", iteration: 1));
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);
}
