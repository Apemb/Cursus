namespace Cursus.Persistence.Tests;

/// <summary>
/// Où atterrissent les sorties d'une visite, et ce qui les empêche d'atterrir
/// ailleurs. Le magasin est le seul endroit du système qui connaît ces chemins.
/// </summary>
public class RunArtifactStoreTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("cursus-artifacts-").FullName;

    [Fact(DisplayName = "étant donné une racine, quand on range la sortie standard d'une visite, alors le fichier est écrit sous l'identifiant du run, à l'étape et à l'itération attendues")]
    public void Standard_output_lands_under_the_run_the_step_and_the_iteration()
    {
        // arrange
        var store = new RunArtifactStore(_root);

        // act
        store.Write("run-1", "compiler", iteration: 2, ArtifactStream.StandardOutput, "bonjour");

        // assert
        Assert.True(File.Exists(Path.Combine(_root, "run-1", "compiler.2.stdout")));
    }

    [Fact(DisplayName = "étant donné une sortie rangée, quand on la relit, alors on retrouve son contenu à l'octet près")]
    public void A_stored_output_reads_back_byte_for_byte()
    {
        // arrange
        var store = new RunArtifactStore(_root);
        const string content = "ligne une\nligne deux — accentuée\n\tindentée\n";

        // act
        store.Write("run-1", "compiler", iteration: 1, ArtifactStream.StandardOutput, content);

        // assert
        Assert.Equal(content, store.Read("run-1", "compiler", iteration: 1, ArtifactStream.StandardOutput));
    }

    [Fact(DisplayName = "étant donné deux itérations d'une même étape, quand on range leurs sorties, alors aucune n'écrase l'autre")]
    public void Two_iterations_of_the_same_step_do_not_overwrite_each_other()
    {
        // arrange
        var store = new RunArtifactStore(_root);

        // act
        store.Write("run-1", "compiler", iteration: 1, ArtifactStream.StandardOutput, "premier tour");
        store.Write("run-1", "compiler", iteration: 2, ArtifactStream.StandardOutput, "second tour");

        // assert
        Assert.Equal("premier tour", store.Read("run-1", "compiler", 1, ArtifactStream.StandardOutput));
        Assert.Equal("second tour", store.Read("run-1", "compiler", 2, ArtifactStream.StandardOutput));
    }

    [Fact(DisplayName = "étant donné la sortie standard et la sortie d'erreur d'une même visite, quand on les range, alors elles restent distinctes")]
    public void The_two_streams_of_a_visit_stay_distinct()
    {
        // arrange
        var store = new RunArtifactStore(_root);

        // act
        store.Write("run-1", "compiler", iteration: 1, ArtifactStream.StandardOutput, "ce qui va bien");
        store.Write("run-1", "compiler", iteration: 1, ArtifactStream.StandardError, "ce qui va mal");

        // assert
        Assert.Equal("ce qui va bien", store.Read("run-1", "compiler", 1, ArtifactStream.StandardOutput));
        Assert.Equal("ce qui va mal", store.Read("run-1", "compiler", 1, ArtifactStream.StandardError));
    }

    [Fact(DisplayName = "étant donné une sortie vide, quand on la range, alors aucun fichier n'est créé")]
    public void An_empty_output_creates_no_file()
    {
        // arrange
        var store = new RunArtifactStore(_root);

        // act
        var path = store.Write("run-1", "compiler", iteration: 1, ArtifactStream.StandardError, "");

        // assert
        Assert.Null(path);
        Assert.False(File.Exists(Path.Combine(_root, "run-1", "compiler.1.stderr")));
    }

    [Fact(DisplayName = "étant donné un identifiant d'étape contenant un séparateur de chemin, quand on range sa sortie, alors le rangement est refusé")]
    public void A_step_identifier_that_walks_the_filesystem_is_refused()
    {
        // arrange
        var store = new RunArtifactStore(_root);

        // act / assert
        Assert.Throws<ArgumentException>(() =>
            store.Write("run-1", "../../ailleurs", iteration: 1, ArtifactStream.StandardOutput, "charge utile"));
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);
}
