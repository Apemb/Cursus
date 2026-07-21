using Cursus.Core.Projects;
using Cursus.Core.Workflows;

namespace Cursus.Core.Tests.Projects;

/// <summary>
/// Ce qu'un projet contient comme workflows, et comment on en charge un. Toute
/// la traduction JSON reste au sérialiseur ; le catalogue n'apporte que le
/// disque et l'identité.
/// </summary>
public class WorkflowCatalogTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("cursus-catalog-").FullName;

    [Fact(DisplayName = "étant donné un projet sans workflow, quand on liste, alors la liste est vide")]
    public void A_fresh_project_holds_no_workflow()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");

        // act
        var entries = new WorkflowCatalog(project).List();

        // assert
        Assert.Empty(entries);
    }

    [Fact(DisplayName = "étant donné deux documents déposés, quand on liste, alors chacun est identifié par son nom de fichier sans extension")]
    public void A_workflow_is_identified_by_its_file_name()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "deployer", AnyDocument);
        Deposit(project, "verifier", AnyDocument);

        // act
        var entries = new WorkflowCatalog(project).List();

        // assert
        Assert.Equal(["deployer", "verifier"], entries.Select(entry => entry.Id));
    }

    [Fact(DisplayName = "étant donné des documents dont les noms ne sont pas dans l'ordre, quand on liste, alors ils reviennent triés par identifiant")]
    public void Listing_is_ordered_by_identifier()
    {
        // arrange — l'ordre d'énumération du système de fichiers n'est garanti nulle part
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "zeta", AnyDocument);
        Deposit(project, "alpha", AnyDocument);
        Deposit(project, "mu", AnyDocument);

        // act
        var entries = new WorkflowCatalog(project).List();

        // assert
        Assert.Equal(["alpha", "mu", "zeta"], entries.Select(entry => entry.Id));
    }

    [Fact(DisplayName = "étant donné un fichier qui n'est pas un document JSON dans le dossier des workflows, quand on liste, alors il est ignoré")]
    public void Files_that_are_not_documents_are_not_workflows()
    {
        // arrange — un dossier versionné finit toujours par accueillir un README
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "deployer", AnyDocument);
        File.WriteAllText(Path.Combine(project.WorkflowsDirectory, "README.md"), "# Les workflows du projet");

        // act
        var entries = new WorkflowCatalog(project).List();

        // assert
        Assert.Equal("deployer", Assert.Single(entries).Id);
    }

    [Fact(DisplayName = "étant donné un workflow déposé, quand on le charge par son identifiant, alors on obtient la définition qu'il décrit")]
    public void A_workflow_is_read_from_disk_by_its_identifier()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "deployer", AnyDocument);

        // act
        var loaded = new WorkflowCatalog(project).Load("deployer");

        // assert
        Assert.True(loaded.Report.IsValid);
        Assert.Equal("seule", loaded.Definition!.EntryStep);
    }

    [Fact(DisplayName = "étant donné un workflow au graphe invalide, quand on le charge, alors on obtient le rapport de ses problèmes et aucune définition")]
    public void An_invalid_workflow_yields_its_report_rather_than_an_exception()
    {
        // arrange — une arête qui pointe vers une étape absente
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "casse", BrokenDocument);

        // act
        var loaded = new WorkflowCatalog(project).Load("casse");

        // assert
        Assert.Null(loaded.Definition);
        Assert.Contains(loaded.Report.Issues, issue => issue.Kind == ValidationIssueKind.UnknownEdgeTarget);
    }

    [Fact(DisplayName = "étant donné un workflow invalide déposé à côté d'un valide, quand on liste, alors les deux apparaissent")]
    public void One_broken_document_does_not_hide_the_others()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "bon", AnyDocument);
        Deposit(project, "casse", BrokenDocument);

        // act
        var entries = new WorkflowCatalog(project).List();

        // assert — sinon un seul fichier fautif rendrait le projet entier inutilisable
        Assert.Equal(["bon", "casse"], entries.Select(entry => entry.Id));
    }

    [Fact(DisplayName = "étant donné un identifiant qu'aucun fichier ne porte, quand on le charge, alors le chargement échoue en signalant le fichier absent")]
    public void Loading_an_unknown_identifier_fails()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");

        // act / assert
        Assert.Throws<FileNotFoundException>(() => new WorkflowCatalog(project).Load("fantome"));
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    // --- helpers ---

    /// <summary>Un graphe valide dont le détail n'importe pas au catalogue.</summary>
    private const string AnyDocument = """
        {
          "entryStep": "seule",
          "steps": [
            { "id": "seule", "name": "Seule", "maxVisits": 1,
              "script": { "fileName": "/bin/true", "arguments": [] }, "edges": [] }
          ]
        }
        """;

    /// <summary>Structurellement lisible, sémantiquement faux : l'arête ne mène nulle part.</summary>
    private const string BrokenDocument = """
        {
          "entryStep": "seule",
          "steps": [
            { "id": "seule", "name": "Seule", "maxVisits": 1,
              "script": { "fileName": "/bin/true", "arguments": [] },
              "edges": [ { "guard": "success", "target": "fantome" } ] }
          ]
        }
        """;

    private static void Deposit(Project project, string id, string document) =>
        File.WriteAllText(Path.Combine(project.WorkflowsDirectory, $"{id}.json"), document);
}
