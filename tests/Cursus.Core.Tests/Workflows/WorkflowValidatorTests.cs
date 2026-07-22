using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Validation;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// Les règles qui décident si un graphe est exécutable. Une seule source de
/// vérité, deux consommateurs : le chargement de fichier, et plus tard
/// l'édition graphique — d'où un rapport agrégé plutôt qu'une exception.
/// </summary>
public class WorkflowValidatorTests
{
    [Fact(DisplayName = "étant donné un graphe cohérent, quand on le valide, alors le rapport ne signale aucun problème")]
    public void A_sound_graph_reports_no_issue()
    {
        // arrange
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B"),
        });

        // act
        var report = WorkflowValidator.Validate(definition);

        // assert
        Assert.Empty(report.Issues);
    }

    [Fact(DisplayName = "étant donné un point d'entrée vide, quand on valide, alors le rapport le signale")]
    public void An_empty_entry_step_is_reported()
    {
        // arrange
        var definition = new WorkflowDefinition("", new[] { Step("A") });

        // act
        var report = WorkflowValidator.Validate(definition);

        // assert
        Assert.Equal(ValidationIssueKind.MissingEntryStep, Assert.Single(report.Issues).Kind);
    }

    [Fact(DisplayName = "étant donné un point d'entrée qui ne désigne aucune étape, quand on valide, alors le rapport le signale en le nommant")]
    public void An_entry_step_pointing_nowhere_is_reported()
    {
        // arrange
        var definition = new WorkflowDefinition("Z", new[] { Step("A") });

        // act
        var report = WorkflowValidator.Validate(definition);

        // assert
        var issue = Assert.Single(report.Issues);
        Assert.Equal(ValidationIssueKind.UnknownEntryStep, issue.Kind);
        Assert.Equal("Z", issue.StepId);
    }

    [Fact(DisplayName = "étant donné une arête dont la cible n'existe pas, quand on valide, alors le rapport le signale en nommant l'étape source")]
    public void An_edge_pointing_to_an_unknown_step_is_reported()
    {
        // arrange
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "Z")),
        });

        // act
        var report = WorkflowValidator.Validate(definition);

        // assert — c'est l'étape source qu'il faut nommer : c'est elle qu'on
        // corrige, et c'est elle que l'éditeur mettra en évidence.
        var issue = Assert.Single(report.Issues);
        Assert.Equal(ValidationIssueKind.UnknownEdgeTarget, issue.Kind);
        Assert.Equal("A", issue.StepId);
        Assert.Contains("Z", issue.Message);
    }

    [Fact(DisplayName = "étant donné deux étapes portant le même identifiant, quand on valide, alors le rapport le signale une fois")]
    public void A_duplicated_step_id_is_reported_once()
    {
        // arrange — une arête vers « A » serait ambiguë : le moteur retiendrait
        // silencieusement la première déclarée.
        var definition = new WorkflowDefinition("A", new[] { Step("A"), Step("A") });

        // act
        var report = WorkflowValidator.Validate(definition);

        // assert
        var issue = Assert.Single(report.Issues);
        Assert.Equal(ValidationIssueKind.DuplicateStepId, issue.Kind);
        Assert.Equal("A", issue.StepId);
    }

    [Fact(DisplayName = "étant donné une étape à l'identifiant vide, quand on valide, alors le rapport le signale")]
    public void An_empty_step_id_is_reported()
    {
        // arrange — une étape sans identifiant est inatteignable par une arête.
        var definition = new WorkflowDefinition("A", new[] { Step("A"), Step("") });

        // act
        var report = WorkflowValidator.Validate(definition);

        // assert
        Assert.Equal(ValidationIssueKind.EmptyStepId, Assert.Single(report.Issues).Kind);
    }

    [Fact(DisplayName = "étant donné une étape dont le nombre de visites maximum est nul, quand on valide, alors le rapport le signale")]
    public void A_step_that_may_never_be_visited_is_reported()
    {
        // arrange — avec maxVisits à 0, le moteur interromprait le run avant
        // même la première exécution de l'étape.
        var definition = new WorkflowDefinition("A", new[]
        {
            new StepDefinition("A", "A", AnyScript, MaxVisits: 0, []),
        });

        // act
        var report = WorkflowValidator.Validate(definition);

        // assert
        var issue = Assert.Single(report.Issues);
        Assert.Equal(ValidationIssueKind.NonPositiveMaxVisits, issue.Kind);
        Assert.Equal("A", issue.StepId);
    }

    [Fact(DisplayName = "étant donné une étape que rien ne mène à atteindre depuis l'entrée, quand on valide, alors le rapport le signale")]
    public void A_step_unreachable_from_the_entry_is_reported()
    {
        // arrange — « orpheline » n'est la cible d'aucune arête.
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "B")),
            Step("B"),
            Step("orpheline"),
        });

        // act
        var report = WorkflowValidator.Validate(definition);

        // assert
        var issue = Assert.Single(report.Issues);
        Assert.Equal(ValidationIssueKind.UnreachableStep, issue.Kind);
        Assert.Equal("orpheline", issue.StepId);
    }

    [Fact(DisplayName = "étant donné une étape atteignable seulement par une arête d'échec, quand on valide, alors elle n'est pas signalée")]
    public void A_step_reached_only_through_a_failure_edge_is_not_reported()
    {
        // arrange — l'atteignabilité est structurelle : elle ne présume pas
        // des codes de sortie, donc une branche de rattrapage compte.
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnFailure, "rattrapage")),
            Step("rattrapage"),
        });

        // act
        var report = WorkflowValidator.Validate(definition);

        // assert
        Assert.Empty(report.Issues);
    }

    [Fact(DisplayName = "étant donné une boucle arrière sur le point d'entrée, quand on valide, alors aucune étape n'est signalée inatteignable")]
    public void A_backward_loop_does_not_trap_reachability()
    {
        // arrange — un cycle ne doit ni faire boucler le parcours, ni masquer
        // l'étape qui suit.
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnFailure, "A"), new Edge(Guard.OnSuccess, "B")),
            Step("B"),
        });

        // act
        var report = WorkflowValidator.Validate(definition);

        // assert
        Assert.Empty(report.Issues);
    }

    [Fact(DisplayName = "étant donné un graphe cumulant trois problèmes distincts, quand on valide, alors le rapport les contient tous les trois")]
    public void All_the_issues_of_a_graph_are_reported_together()
    {
        // arrange — une cible inconnue, un maxVisits nul, une étape orpheline.
        var definition = new WorkflowDefinition("A", new[]
        {
            Step("A", new Edge(Guard.OnSuccess, "Z")),
            new StepDefinition("B", "B", AnyScript, MaxVisits: 0, []),
            Step("orpheline"),
        });

        // act
        var report = WorkflowValidator.Validate(definition);

        // assert — c'est la raison d'être du rapport : tout dire d'un coup,
        // dans l'ordre de déclaration des étapes, pour que l'affichage soit
        // reproductible.
        Assert.False(report.IsValid);
        Assert.Equal(
            new (ValidationIssueKind, string?)[]
            {
                (ValidationIssueKind.UnknownEdgeTarget, "A"),
                (ValidationIssueKind.NonPositiveMaxVisits, "B"),
                (ValidationIssueKind.UnreachableStep, "B"),
                (ValidationIssueKind.UnreachableStep, "orpheline"),
            },
            report.Issues.Select(i => (i.Kind, i.StepId)));
    }

    // --- helpers ---

    private static readonly ScriptSpec AnyScript = new("/usr/bin/true", []);

    private static StepDefinition Step(string id, params Edge[] edges) =>
        new(id, id, AnyScript, MaxVisits: 1, edges);
}
