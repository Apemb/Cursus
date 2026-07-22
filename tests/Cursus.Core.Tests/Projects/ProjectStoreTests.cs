using Cursus.Core.Projects;

namespace Cursus.Core.Tests.Projects;

/// <summary>
/// La disposition <c>.cursus/</c> et sa traduction dans les deux sens. Les
/// chemins littéraux sont assertés ici et nulle part ailleurs : ce dossier est
/// versionné, donc sa forme est un contrat envers les dépôts qui le portent.
/// </summary>
public class ProjectStoreTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("cursus-project-").FullName;

    [Fact(DisplayName = "étant donné un répertoire vierge, quand on y crée un projet, alors le fichier de projet et le dossier des workflows existent")]
    public void Creating_a_project_lays_out_the_cursus_directory()
    {
        // act
        ProjectStore.Create(_root, "Démo");

        // assert
        Assert.True(File.Exists(Path.Combine(_root, ".cursus", "project.json")));
        Assert.True(Directory.Exists(Path.Combine(_root, ".cursus", "workflows")));
    }

    [Fact(DisplayName = "étant donné un projet créé, quand on lit son identifiant, alors il n'est pas vide")]
    public void A_created_project_carries_an_identity()
    {
        // act
        var project = ProjectStore.Create(_root, "Démo");

        // assert
        Assert.False(string.IsNullOrWhiteSpace(project.Id));
    }

    [Fact(DisplayName = "étant donné deux projets créés, quand on compare leurs identifiants, alors ils diffèrent")]
    public void Two_projects_never_share_an_identity()
    {
        // arrange
        var elsewhere = Directory.CreateDirectory(Path.Combine(_root, "voisin")).FullName;

        // act
        var one = ProjectStore.Create(_root, "Démo");
        var other = ProjectStore.Create(elsewhere, "Démo");

        // assert
        Assert.NotEqual(one.Id, other.Id);
    }

    [Fact(DisplayName = "étant donné un projet créé, quand on le rouvre, alors son nom et son identifiant sont ceux de la création")]
    public void A_project_survives_being_written_and_read_back()
    {
        // arrange
        var created = ProjectStore.Create(_root, "Démo accentuée");

        // act
        var reopened = ProjectStore.Open(_root);

        // assert
        Assert.Equal(created.Id, reopened.Id);
        Assert.Equal("Démo accentuée", reopened.Name);
    }

    [Fact(DisplayName = "étant donné un répertoire qui porte déjà un projet, quand on tente d'en créer un second, alors la création est refusée")]
    public void Creating_over_an_existing_project_is_refused()
    {
        // arrange
        var created = ProjectStore.Create(_root, "Démo");

        // act / assert — l'identité du projet en place ne doit pas être remplacée
        Assert.Throws<InvalidOperationException>(() => ProjectStore.Create(_root, "Autre"));
        Assert.Equal(created.Id, ProjectStore.Open(_root).Id);
    }

    [Fact(DisplayName = "étant donné un projet créé, quand on lit le fichier d'exclusion git de son dossier, alors il écarte le journal, les artefacts et les worktrees des runs")]
    public void A_created_project_keeps_its_observations_out_of_git()
    {
        // act
        ProjectStore.Create(_root, "Démo");

        // assert
        var ignored = File.ReadAllLines(Path.Combine(_root, ".cursus", ".gitignore"));
        Assert.Contains("cursus.db*", ignored);
        Assert.Contains("runs/", ignored);
        // les worktrees des runs vivent sous .cursus/ mais ne se committent jamais
        Assert.Contains("worktrees/", ignored);
    }

    // --- ouverture et découverte ---

    [Fact(DisplayName = "étant donné un répertoire sans dossier de projet, quand on l'ouvre, alors l'ouverture échoue en signalant l'absence")]
    public void Opening_a_directory_that_holds_no_project_fails()
    {
        // act / assert
        Assert.Throws<ProjectNotFoundException>(() => ProjectStore.Open(_root));
    }

    [Fact(DisplayName = "étant donné un fichier de projet qui n'est pas un document de projet, quand on l'ouvre, alors l'ouverture échoue en signalant un projet invalide")]
    public void Opening_a_project_file_that_is_not_json_fails()
    {
        // arrange
        ProjectStore.Create(_root, "Démo");
        File.WriteAllText(Path.Combine(_root, ".cursus", "project.json"), "ceci n'est pas du JSON");

        // act / assert
        Assert.Throws<InvalidProjectException>(() => ProjectStore.Open(_root));
    }

    [Fact(DisplayName = "étant donné un fichier de projet sans identifiant, quand on l'ouvre, alors l'ouverture échoue en signalant un projet invalide")]
    public void Opening_a_project_without_an_identity_fails()
    {
        // arrange — du JSON parfaitement valide, mais qui ne dit pas quel projet c'est
        ProjectStore.Create(_root, "Démo");
        File.WriteAllText(Path.Combine(_root, ".cursus", "project.json"), """{ "name": "Démo" }""");

        // act / assert
        Assert.Throws<InvalidProjectException>(() => ProjectStore.Open(_root));
    }

    [Fact(DisplayName = "étant donné un projet ouvert, quand on lit sa racine, alors c'est le répertoire qui contient son dossier de projet")]
    public void The_workspace_root_is_deduced_from_where_the_project_directory_sits()
    {
        // arrange — la racine n'est écrite nulle part : elle se déduit
        ProjectStore.Create(_root, "Démo");

        // act
        var project = ProjectStore.Open(_root);

        // assert
        Assert.Equal(_root, project.Root);
        Assert.Equal(Path.Combine(_root, ".cursus"), project.CursusDirectory);
    }

    [Fact(DisplayName = "étant donné un projet ouvert, quand on lit ses emplacements, alors le journal et les artefacts sont dans son dossier de projet")]
    public void A_project_knows_where_its_journal_and_artifacts_live()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");

        // act / assert — base et sorties au même endroit : sauvegardées ou
        // détruites ensemble, sinon le journal prétend être complet à tort
        Assert.Equal(Path.Combine(_root, ".cursus", "cursus.db"), project.DatabasePath);
        Assert.Equal(Path.Combine(_root, ".cursus", "runs"), project.ArtifactsRoot);
        // les worktrees isolés des runs vivent sous le même dossier
        Assert.Equal(Path.Combine(_root, ".cursus", "worktrees"), project.WorktreesRoot);
    }

    [Fact(DisplayName = "étant donné un sous-répertoire profond d'un projet, quand on découvre le projet depuis là, alors on retrouve le projet racine")]
    public void Discovery_walks_up_until_it_finds_a_project()
    {
        // arrange
        var created = ProjectStore.Create(_root, "Démo");
        var deep = Directory.CreateDirectory(Path.Combine(_root, "src", "un", "deux")).FullName;

        // act
        var found = ProjectStore.Discover(deep);

        // assert
        Assert.Equal(created.Id, found.Id);
        Assert.Equal(_root, found.Root);
    }

    [Fact(DisplayName = "étant donné un répertoire hors de tout projet, quand on y découvre un projet, alors la découverte échoue en signalant l'absence")]
    public void Discovery_that_reaches_the_root_without_finding_anything_fails()
    {
        // arrange — un temporaire vierge : aucun ancêtre jusqu'à la racine du disque
        var orphan = Directory.CreateDirectory(Path.Combine(_root, "orphelin")).FullName;

        // act / assert
        Assert.Throws<ProjectNotFoundException>(() => ProjectStore.Discover(orphan));
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);
}
