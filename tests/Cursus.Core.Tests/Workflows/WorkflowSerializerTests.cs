using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Validation;
using Cursus.Core.Workflows.Serialization;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// Le format de fichier : ce qui rend un workflow déclarable hors du C#.
/// Bidirectionnel dès le départ, parce que l'éditeur graphique réécrira le
/// document — et travaillant sur des chaînes, sans jamais toucher au disque.
/// </summary>
public class WorkflowSerializerTests
{
    [Fact(DisplayName = "étant donné un document décrivant une étape unique, quand on le lit, alors on obtient une définition portant cette étape")]
    public void A_document_with_a_single_step_yields_a_definition()
    {
        // arrange
        const string document = """
            {
              "entryStep": "preparer",
              "steps": [
                {
                  "id": "preparer",
                  "name": "Préparer",
                  "maxVisits": 1,
                  "script": { "fileName": "/bin/sh", "arguments": ["-c", "make build"] },
                  "edges": []
                }
              ]
            }
            """;

        // act
        var result = WorkflowSerializer.Read(document);

        // assert
        Assert.True(result.Report.IsValid);
        var step = Assert.Single(result.Definition!.Steps);
        Assert.Equal("preparer", result.Definition.EntryStep);
        Assert.Equal("Préparer", step.Name);
        Assert.Equal("/bin/sh", ((ScriptStep)step).Script.FileName);
        Assert.Equal(new[] { "-c", "make build" }, ((ScriptStep)step).Script.Arguments);
    }

    [Fact(DisplayName = "étant donné un document décrivant deux étapes reliées, quand on le lit, alors l'arête relie la bonne cible")]
    public void A_document_with_linked_steps_yields_the_edges()
    {
        // arrange
        const string document = """
            {
              "entryStep": "A",
              "steps": [
                { "id": "A", "maxVisits": 1,
                  "script": { "fileName": "/usr/bin/true" },
                  "edges": [ { "guard": "success", "target": "B" } ] },
                { "id": "B", "maxVisits": 1, "script": { "fileName": "/usr/bin/true" } }
              ]
            }
            """;

        // act
        var result = WorkflowSerializer.Read(document);

        // assert
        Assert.True(result.Report.IsValid);
        var edge = Assert.Single(result.Definition!.GetStep("A").OutEdges);
        Assert.Equal("B", edge.Target);
        Assert.Equal(Guard.OnSuccess, edge.Guard);
    }

    [Theory(DisplayName = "étant donné une garde écrite en chaîne, quand on lit le document, alors elle correspond à la garde du modèle")]
    [InlineData("success")]
    [InlineData("failure")]
    [InlineData("default")]
    [InlineData("exit:2")]
    public void Each_written_guard_maps_to_its_model_counterpart(string written)
    {
        // arrange
        var expected = written switch
        {
            "success" => Guard.OnSuccess,
            "failure" => Guard.OnFailure,
            "default" => Guard.Default,
            _ => Guard.OnExitCode(2),
        };
        var document = $$"""
            {
              "entryStep": "A",
              "steps": [
                { "id": "A", "maxVisits": 1,
                  "script": { "fileName": "/usr/bin/true" },
                  "edges": [ { "guard": "{{written}}", "target": "B" } ] },
                { "id": "B", "maxVisits": 1, "script": { "fileName": "/usr/bin/true" } }
              ]
            }
            """;

        // act
        var result = WorkflowSerializer.Read(document);

        // assert
        Assert.Equal(expected, Assert.Single(result.Definition!.GetStep("A").OutEdges).Guard);
    }

    [Fact(DisplayName = "étant donné un document dont une garde est inconnue, quand on le lit, alors le rapport le signale et aucune définition n'est rendue")]
    public void An_unknown_guard_is_reported_rather_than_thrown()
    {
        // arrange
        const string document = """
            {
              "entryStep": "A",
              "steps": [
                { "id": "A", "maxVisits": 1,
                  "script": { "fileName": "/usr/bin/true" },
                  "edges": [ { "guard": "peut-etre", "target": "A" } ] }
              ]
            }
            """;

        // act
        var result = WorkflowSerializer.Read(document);

        // assert — l'appelant, éditeur compris, n'a qu'un seul mode d'échec à gérer.
        Assert.Null(result.Definition);
        var issue = Assert.Single(result.Report.Issues);
        Assert.Equal(ValidationIssueKind.UnknownGuard, issue.Kind);
        Assert.Contains("peut-etre", issue.Message);
    }

    [Theory(DisplayName = "étant donné un document qui n'est pas un workflow exploitable, quand on le lit, alors le rapport le signale et aucune définition n'est rendue")]
    [InlineData("ceci n'est pas du json")]
    [InlineData("{ \"entryStep\": ")]
    [InlineData("null")]
    public void A_malformed_document_is_reported_rather_than_thrown(string document)
    {
        // act
        var result = WorkflowSerializer.Read(document);

        // assert
        Assert.Null(result.Definition);
        Assert.Equal(ValidationIssueKind.MalformedDocument, Assert.Single(result.Report.Issues).Kind);
    }

    [Fact(DisplayName = "étant donné un document dont le graphe est invalide, quand on le lit pour édition, alors la définition parsée est rendue malgré tout et le rapport en porte le problème")]
    public void An_invalid_graph_is_still_returned_for_editing()
    {
        // arrange — une arête vise une étape absente : parsable, mais invalide
        const string document = """
            {
              "entryStep": "A",
              "steps": [
                { "id": "A", "maxVisits": 1,
                  "script": { "fileName": "/usr/bin/true" },
                  "edges": [ { "guard": "success", "target": "fantome" } ] }
              ]
            }
            """;

        // act
        var parsed = WorkflowSerializer.ReadEditable(document);

        // assert — contrairement à Read, la définition survit à son invalidité
        Assert.Equal("A", parsed.Definition!.EntryStep);
        Assert.Equal(ValidationIssueKind.UnknownEdgeTarget, Assert.Single(parsed.Report.Issues).Kind);
    }

    [Fact(DisplayName = "étant donné un document malformé, quand on le lit pour édition, alors aucune définition n'est rendue et le rapport signale le document malformé")]
    public void A_malformed_document_yields_no_definition_even_for_editing()
    {
        // act — sans graphe parsable, l'éditeur n'a rien à mettre à l'écran
        var parsed = WorkflowSerializer.ReadEditable("ceci n'est pas du json");

        // assert
        Assert.Null(parsed.Definition);
        Assert.Equal(ValidationIssueKind.MalformedDocument, Assert.Single(parsed.Report.Issues).Kind);
    }

    [Fact(DisplayName = "étant donné un script déclarant environnement, délai et sous-chemin, quand on lit le document, alors l'étape les porte")]
    public void Environment_timeout_and_subdirectory_are_carried_over()
    {
        // arrange
        const string document = """
            {
              "entryStep": "A",
              "steps": [
                { "id": "A", "maxVisits": 1, "workingSubdirectory": "backend",
                  "script": { "fileName": "/bin/sh", "arguments": ["-c", "make test"],
                              "environment": { "CI": "1" }, "timeoutSeconds": 300 } }
              ]
            }
            """;

        // act
        var step = WorkflowSerializer.Read(document).Definition!.GetStep("A");

        // assert
        Assert.Equal("backend", step.WorkingSubdirectory);
        Assert.Equal("1", ((ScriptStep)step).Script.Environment!["CI"]);
        Assert.Equal(TimeSpan.FromMinutes(5), ((ScriptStep)step).Script.Timeout);
    }

    [Fact(DisplayName = "étant donné un script sans délai, sous-chemin ni description, quand on lit le document, alors l'étape n'en déclare aucun")]
    public void An_omitted_timeout_subdirectory_or_description_stays_absent()
    {
        // arrange
        const string document = """
            {
              "entryStep": "A",
              "steps": [ { "id": "A", "maxVisits": 1, "script": { "fileName": "/usr/bin/true" } } ]
            }
            """;

        // act
        var step = WorkflowSerializer.Read(document).Definition!.GetStep("A");

        // assert — l'absence de délai signifie « aucune limite », pas « zéro » ;
        // une description omise est simplement absente.
        Assert.Null(((ScriptStep)step).Script.Timeout);
        Assert.Null(step.WorkingSubdirectory);
        Assert.Null(step.Description);
    }

    [Fact(DisplayName = "étant donné un document dont une étape porte une description, quand on le lit, alors l'étape porte cette description")]
    public void A_step_description_is_carried_over()
    {
        // arrange — « name » est le titre court, « description » la phrase longue.
        const string document = """
            {
              "entryStep": "A",
              "steps": [
                { "id": "A", "name": "Compiler", "description": "Compiler sans le moindre avertissement",
                  "maxVisits": 1, "script": { "fileName": "/usr/bin/true" } }
              ]
            }
            """;

        // act
        var step = WorkflowSerializer.Read(document).Definition!.GetStep("A");

        // assert
        Assert.Equal("Compiler", step.Name);
        Assert.Equal("Compiler sans le moindre avertissement", step.Description);
    }

    [Fact(DisplayName = "étant donné un document d'étape de kind agent, quand on le lit, alors on obtient un AgentStep portant harness, modèle et prompt")]
    public void An_agent_step_document_yields_an_agent_step()
    {
        // arrange
        const string document = """
            {
              "entryStep": "corriger",
              "steps": [
                {
                  "id": "corriger",
                  "name": "Corriger",
                  "kind": "agent",
                  "maxVisits": 1,
                  "agent": { "harness": "Claude Code", "model": "opus", "prompt": "Corrige les tests rouges" },
                  "edges": []
                }
              ]
            }
            """;

        // act
        var step = WorkflowSerializer.Read(document).Definition!.GetStep("corriger");

        // assert
        var agent = Assert.IsType<AgentStep>(step);
        Assert.Equal("Claude Code", agent.HarnessName);
        Assert.Equal("opus", agent.ModelId);
        Assert.Equal("Corrige les tests rouges", agent.Prompt);
    }

    [Fact(DisplayName = "étant donné un document d'étape sans kind, quand on le lit, alors on obtient un ScriptStep — la retombée qui garde valides les fichiers d'avant les kinds")]
    public void A_step_document_without_kind_yields_a_script_step()
    {
        // arrange — aucun « kind » déclaré, comme tout workflow écrit avant les kinds
        const string document = """
            {
              "entryStep": "A",
              "steps": [ { "id": "A", "maxVisits": 1, "script": { "fileName": "/usr/bin/true" } } ]
            }
            """;

        // act
        var step = WorkflowSerializer.Read(document).Definition!.GetStep("A");

        // assert
        Assert.IsType<ScriptStep>(step);
    }

    [Fact(DisplayName = "étant donné une définition portant une étape-agent, quand on l'écrit puis qu'on la relit, alors le kind, le harness, le modèle et le prompt sont préservés")]
    public void An_agent_step_round_trips_through_the_document()
    {
        // arrange
        var definition = new WorkflowDefinition("corriger", new StepDefinition[]
        {
            new AgentStep(
                "corriger", "Corriger", "Claude Code", "opus", "Corrige les tests rouges",
                MaxVisits: 1, []),
        });

        // act
        var reread = WorkflowSerializer
            .Read(WorkflowSerializer.Write(definition)).Definition!.GetStep("corriger");

        // assert
        var agent = Assert.IsType<AgentStep>(reread);
        Assert.Equal("Claude Code", agent.HarnessName);
        Assert.Equal("opus", agent.ModelId);
        Assert.Equal("Corrige les tests rouges", agent.Prompt);
    }

    [Fact(DisplayName = "étant donné un document au repos, quand on le lit puis qu'on le réécrit, alors on retrouve le même document")]
    public void Reading_then_writing_a_document_reproduces_it()
    {
        // arrange — tout le vocabulaire du format, pour que l'aller-retour
        // porte sur autre chose qu'un cas dégénéré. C'est la garantie dont
        // l'éditeur graphique dépendra : il réécrira ce fichier à chaque
        // sauvegarde, et ne doit rien en perdre.
        const string document = """
            {
              "entryStep": "preparer",
              "steps": [
                {
                  "id": "preparer",
                  "name": "Préparer",
                  "description": null,
                  "kind": "script",
                  "maxVisits": 1,
                  "script": {
                    "fileName": "/bin/sh",
                    "arguments": [
                      "-c",
                      "make build"
                    ],
                    "environment": {
                      "CI": "1"
                    },
                    "timeoutSeconds": 300
                  },
                  "agent": null,
                  "edges": [
                    {
                      "guard": "success",
                      "target": "tester"
                    },
                    {
                      "guard": "exit:2",
                      "target": "preparer"
                    }
                  ],
                  "workingSubdirectory": "backend"
                },
                {
                  "id": "tester",
                  "name": "Tester",
                  "description": null,
                  "kind": "script",
                  "maxVisits": 3,
                  "script": {
                    "fileName": "/usr/bin/make",
                    "arguments": [
                      "test"
                    ],
                    "environment": null,
                    "timeoutSeconds": null
                  },
                  "agent": null,
                  "edges": [
                    {
                      "guard": "failure",
                      "target": "tester"
                    }
                  ],
                  "workingSubdirectory": null
                }
              ]
            }
            """;

        // act
        var rewritten = WorkflowSerializer.Write(WorkflowSerializer.Read(document).Definition!);

        // assert
        Assert.Equal(document, rewritten);
    }

    [Fact(DisplayName = "étant donné une définition construite en code, quand on l'écrit puis qu'on la relit, alors le document réécrit est identique au premier")]
    public void Writing_then_reading_a_definition_reproduces_it()
    {
        // arrange — l'égalité des records ne couvre pas les listes, comparées
        // par référence : c'est donc le document qui fait foi.
        var definition = new WorkflowDefinition("A", new[]
        {
            new ScriptStep(
                "A", "Analyser",
                new ScriptSpec("/bin/sh", ["-c", "make lint"], Timeout: TimeSpan.FromSeconds(30)),
                MaxVisits: 2,
                [new Edge(Guard.OnExitCode(7), "B"), new Edge(Guard.Default, "A")],
                WorkingSubdirectory: "src",
                Description: "Analyser le code sous toutes ses coutures"),
            new ScriptStep("B", "Broyer", new ScriptSpec("/usr/bin/true", []), MaxVisits: 1, []),
        });

        // act
        var written = WorkflowSerializer.Write(definition);
        var rewritten = WorkflowSerializer.Write(WorkflowSerializer.Read(written).Definition!);

        // assert
        Assert.Equal(written, rewritten);
    }
}
