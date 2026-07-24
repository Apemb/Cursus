using Cursus.Core.Projects;
using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Validation;

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

    [Fact(DisplayName = "étant donné un brouillon cassé sauvegardé sur disque, quand on l'ouvre depuis le catalogue, alors on récupère sa définition parsée et le rapport de ses problèmes")]
    public void Opening_a_broken_draft_yields_its_parsed_definition_and_report()
    {
        // arrange — le trou de ·2a : un brouillon cassé se sauvegarde, on veut le rouvrir pour l'éditer
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "casse", BrokenDocument);

        // act
        var opened = new WorkflowCatalog(project).Open("casse");

        // assert — là où Load rendait null, Open rend le graphe à corriger
        Assert.NotNull(opened.Definition);
        Assert.Contains(opened.Report.Issues, issue => issue.Kind == ValidationIssueKind.UnknownEdgeTarget);
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

    [Fact(DisplayName = "étant donné un projet sans ce workflow, quand on crée un workflow d'un identifiant, alors il apparaît dans la liste")]
    public void Creating_a_workflow_makes_it_appear_in_the_listing()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");
        var catalog = new WorkflowCatalog(project);

        // act
        catalog.Create("deployer");

        // assert
        Assert.Equal("deployer", Assert.Single(catalog.List()).Id);
    }

    [Fact(DisplayName = "étant donné un workflow fraîchement créé, quand on le charge, alors son rapport signale l'absence de point d'entrée")]
    public void A_freshly_created_workflow_is_born_a_draft()
    {
        // arrange — brouillons permis : un workflow naît vide, donc invalide mais éditable
        var project = ProjectStore.Create(_root, "Démo");
        var catalog = new WorkflowCatalog(project);
        catalog.Create("deployer");

        // act
        var loaded = catalog.Load("deployer");

        // assert
        Assert.Null(loaded.Definition);
        Assert.Contains(loaded.Report.Issues, issue => issue.Kind == ValidationIssueKind.MissingEntryStep);
    }

    [Fact(DisplayName = "étant donné un identifiant déjà porté, quand on crée à nouveau, alors le catalogue refuse sans toucher au fichier existant")]
    public void Creating_over_an_existing_identifier_is_refused()
    {
        // arrange — un document déjà déposé sous cet identifiant
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "deployer", AnyDocument);
        var catalog = new WorkflowCatalog(project);

        // act
        var refusal = Assert.Throws<WorkflowAlreadyExistsException>(() => catalog.Create("deployer"));

        // assert — le fichier existant n'a pas été écrasé par un brouillon vide
        Assert.Equal("deployer", refusal.Id);
        Assert.True(catalog.Load("deployer").Report.IsValid);
    }

    [Fact(DisplayName = "étant donné une définition valide, quand on la sauvegarde puis qu'on la recharge, alors on retrouve la définition")]
    public void A_saved_definition_survives_a_round_trip_through_disk()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");
        var catalog = new WorkflowCatalog(project);

        // act
        catalog.Save("deployer", SingleStepWorkflow);
        var loaded = catalog.Load("deployer");

        // assert
        Assert.True(loaded.Report.IsValid);
        Assert.Equal("seule", loaded.Definition!.EntryStep);
    }

    [Fact(DisplayName = "étant donné une définition invalide, quand on la sauvegarde, alors le fichier est écrit quand même et le rechargement en rapporte le problème")]
    public void Saving_an_invalid_definition_persists_it_as_a_draft()
    {
        // arrange — une arête qui pointe vers une étape absente : le graphe est cassé
        var project = ProjectStore.Create(_root, "Démo");
        var catalog = new WorkflowCatalog(project);
        var broken = new WorkflowDefinition(
            "seule",
            [new StepDefinition("seule", "Seule", new ScriptSpec("/bin/true", []), 1,
                [new Edge(Guard.OnSuccess, "fantome")])]);

        // act — brouillons permis : Save ne valide pas
        catalog.Save("casse", broken);
        var loaded = catalog.Load("casse");

        // assert — le fichier est bien là (on le relit), et son rapport signale l'arête cassée
        Assert.Null(loaded.Definition);
        Assert.Contains(loaded.Report.Issues, issue => issue.Kind == ValidationIssueKind.UnknownEdgeTarget);
    }

    [Fact(DisplayName = "étant donné un workflow existant, quand on sauvegarde une autre définition sous le même identifiant, alors le contenu est remplacé")]
    public void Saving_over_an_existing_workflow_replaces_its_content()
    {
        // arrange — un brouillon vide, invalide
        var project = ProjectStore.Create(_root, "Démo");
        var catalog = new WorkflowCatalog(project);
        catalog.Create("deployer");

        // act — Save est un upsert : il remplace le contenu existant
        catalog.Save("deployer", SingleStepWorkflow);

        // assert — le brouillon vide a laissé place à la définition valide
        Assert.True(catalog.Load("deployer").Report.IsValid);
    }

    [Fact(DisplayName = "étant donné un workflow existant, quand on le supprime, alors il disparaît de la liste")]
    public void Deleting_a_workflow_removes_it_from_the_listing()
    {
        // arrange — deux workflows, pour vérifier qu'on ne supprime que le bon
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "deployer", AnyDocument);
        Deposit(project, "verifier", AnyDocument);
        var catalog = new WorkflowCatalog(project);

        // act
        catalog.Delete("deployer");

        // assert
        Assert.Equal("verifier", Assert.Single(catalog.List()).Id);
    }

    [Fact(DisplayName = "étant donné un identifiant qu'aucun fichier ne porte, quand on le supprime, alors la suppression échoue en signalant le fichier absent")]
    public void Deleting_an_unknown_identifier_fails()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");

        // act / assert — même convention que Load : l'invariant violé est celui du système de fichiers
        Assert.Throws<FileNotFoundException>(() => new WorkflowCatalog(project).Delete("fantome"));
    }

    [Fact(DisplayName = "étant donné un workflow existant, quand on le renomme, alors l'ancien identifiant disparaît et le nouveau porte le même contenu")]
    public void Renaming_a_workflow_moves_it_under_the_new_identifier()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "deployer", AnyDocument);
        var catalog = new WorkflowCatalog(project);

        // act
        catalog.Rename("deployer", "livrer");

        // assert — un seul workflow, sous le nouvel identifiant, avec le contenu d'origine
        Assert.Equal("livrer", Assert.Single(catalog.List()).Id);
        Assert.Equal("seule", catalog.Load("livrer").Definition!.EntryStep);
    }

    [Fact(DisplayName = "étant donné un nouvel identifiant déjà porté, quand on renomme, alors le catalogue refuse sans écraser la cible")]
    public void Renaming_onto_an_existing_identifier_is_refused()
    {
        // arrange — deux workflows distincts ; l'un valide, l'autre cassé, pour repérer un écrasement
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "deployer", AnyDocument);
        Deposit(project, "livrer", BrokenDocument);
        var catalog = new WorkflowCatalog(project);

        // act
        var refusal = Assert.Throws<WorkflowAlreadyExistsException>(() => catalog.Rename("deployer", "livrer"));

        // assert — la cible n'a pas été remplacée par la source ; les deux subsistent
        Assert.Equal("livrer", refusal.Id);
        Assert.Equal(["deployer", "livrer"], catalog.List().Select(entry => entry.Id));
        Assert.Null(catalog.Load("livrer").Definition);
    }

    [Fact(DisplayName = "étant donné un identifiant contenant un séparateur de chemin, quand on crée, alors le catalogue refuse")]
    public void An_identifier_that_escapes_the_workflows_directory_is_refused()
    {
        // arrange — « ../evil » sortirait du dossier des workflows une fois combiné au chemin
        var project = ProjectStore.Create(_root, "Démo");
        var catalog = new WorkflowCatalog(project);

        // act / assert
        Assert.Throws<InvalidWorkflowIdException>(() => catalog.Create("../evil"));
    }

    [Fact(DisplayName = "étant donné un identifiant vide, quand on crée, alors le catalogue refuse")]
    public void An_empty_identifier_is_refused()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");
        var catalog = new WorkflowCatalog(project);

        // act / assert — un identifiant vide ne désigne aucun fichier
        Assert.Throws<InvalidWorkflowIdException>(() => catalog.Create("   "));
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

    /// <summary>La contrepartie « modèle » de <see cref="AnyDocument"/> : un graphe valide à une étape.</summary>
    private static WorkflowDefinition SingleStepWorkflow =>
        new("seule", [new StepDefinition("seule", "Seule", new ScriptSpec("/bin/true", []), 1, [])]);

    private static void Deposit(Project project, string id, string document) =>
        File.WriteAllText(Path.Combine(project.WorkflowsDirectory, $"{id}.json"), document);
}
