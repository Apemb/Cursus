using Cursus.Core.Projects;
using Cursus.Core.Tasks;

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

    [Fact(DisplayName = "étant donné un projet créé, quand on le renomme puis qu'on le rouvre, alors son nom est le nouveau et son identifiant est inchangé")]
    public void Renaming_a_project_rewrites_its_name_and_keeps_its_identity()
    {
        // arrange
        var created = ProjectStore.Create(_root, "Ancien nom");

        // act — le renommage ne change que le libellé, jamais l'identité
        ProjectStore.Rename(created, "Nouveau nom");
        var reopened = ProjectStore.Open(_root);

        // assert
        Assert.Equal("Nouveau nom", reopened.Name);
        Assert.Equal(created.Id, reopened.Id);
    }

    // --- le tableau de tâches que le dépôt déclare viser ---

    [Fact(DisplayName = "étant donné un projet fraîchement créé, quand on l'ouvre, alors il ne déclare aucun tracker")]
    public void A_fresh_project_declares_no_tracker()
    {
        // arrange
        ProjectStore.Create(_root, "Démo");

        // act
        var project = ProjectStore.Open(_root);

        // assert — une déclaration absente est une valeur qui manque, pas un genre de
        // projet distinct : un dépôt sans tableau reste un dépôt
        Assert.Null(project.Tracker);
    }

    [Fact(DisplayName = "étant donné un projet, quand on lui déclare un tracker, alors le projet rendu porte la déclaration")]
    public void Declaring_a_tracker_yields_a_project_that_carries_it()
    {
        // arrange
        var created = ProjectStore.Create(_root, "Démo");

        // act
        var declared = ProjectStore.DeclareTracker(created, new LinearBinding("cursus-app"));

        // assert — l'appelant doit obtenir le projet frais sans relire le disque ;
        // l'ancien, immuable, garde son absence de déclaration
        Assert.Equal(new LinearBinding("cursus-app"), declared.Tracker);
        Assert.Null(created.Tracker);
    }

    [Fact(DisplayName = "étant donné un projet dont le tracker est déclaré, quand on le rouvre depuis le disque, alors la déclaration est relue")]
    public void A_declared_tracker_survives_being_written_and_read_back()
    {
        // arrange
        var created = ProjectStore.Create(_root, "Démo");
        ProjectStore.DeclareTracker(created, new LinearBinding("cursus-app"));

        // act — la déclaration est versionnée : c'est tout son intérêt, elle doit
        // atterrir dans le fichier que la revue lira
        var reopened = ProjectStore.Open(_root);

        // assert
        var declaration = Assert.IsType<LinearBinding>(reopened.Tracker);
        Assert.Equal("cursus-app", declaration.WorkspaceKey);
    }

    [Fact(DisplayName = "étant donné un projet dont le tracker est déclaré, quand on le renomme depuis un instantané qui l'ignore, alors la déclaration survit")]
    public void Renaming_from_a_stale_snapshot_preserves_the_declared_tracker()
    {
        // arrange — l'instantané d'avant la déclaration, tel que le registre machine en
        // garde un depuis le démarrage de l'application
        var stale = ProjectStore.Create(_root, "Ancien nom");
        ProjectStore.DeclareTracker(stale, new LinearBinding("cursus-app"));

        // act — le registre renomme depuis sa liste en mémoire, qui ne sait rien de la
        // déclaration posée entre-temps
        ProjectStore.Rename(stale, "Nouveau nom");

        // assert — un écrivain partiel de project.json relit le disque avant d'écrire ;
        // sans cela la déclaration s'effacerait sans un mot, et le projet cesserait de
        // viser un tableau au moment le moins soupçonnable
        var reopened = ProjectStore.Open(_root);
        Assert.Equal("Nouveau nom", reopened.Name);
        Assert.Equal(new LinearBinding("cursus-app"), reopened.Tracker);
    }

    [Fact(DisplayName = "étant donné un document dont le genre de tracker est inconnu, quand on l'ouvre, alors aucune déclaration n'est rendue")]
    public void An_unknown_tracker_kind_is_ignored_rather_than_degraded()
    {
        // arrange — le dépôt d'un collègue, versionné par une version qui sait joindre un
        // tracker que celle-ci ignore
        var created = ProjectStore.Create(_root, "Démo");
        File.WriteAllText(
            created.ProjectFilePath,
            """{"id":"p-1","name":"Démo","tracker":{"kind":"jira","site":"acme"}}""");

        // act
        var reopened = ProjectStore.Open(_root);

        // assert — ignoré, jamais dégradé : viser un tableau approximatif enverrait des
        // gestes au mauvais endroit, là où n'en viser aucun se voit et se corrige
        Assert.Null(reopened.Tracker);
        Assert.Equal("Démo", reopened.Name);
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
