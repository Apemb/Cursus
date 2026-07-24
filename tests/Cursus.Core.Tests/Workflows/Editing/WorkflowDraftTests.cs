using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Editing;

namespace Cursus.Core.Tests.Workflows.Editing;

/// <summary>
/// La surface d'édition mutable d'un graphe. Son invariant : une opération
/// structurelle (renommer, supprimer une étape) laisse le graphe
/// référentiellement clos — aucune arête, aucun point d'entrée ne désigne une
/// étape que le brouillon ne contient pas de ce fait.
/// </summary>
public class WorkflowDraftTests
{
    [Fact(DisplayName = "étant donné une définition, quand on en fait un brouillon puis qu'on l'exporte sans rien changer, alors on retrouve une définition équivalente")]
    public void Drafting_then_exporting_untouched_yields_an_equivalent_definition()
    {
        // arrange
        var definition = TwoStepsAtoB;

        // act
        var roundTripped = new WorkflowDraft(definition).ToDefinition();

        // assert
        Assert.Equal("A", roundTripped.EntryStep);
        Assert.Equal(new[] { "A", "B" }, roundTripped.Steps.Select(s => s.Id));
        Assert.Equal("B", Assert.Single(roundTripped.GetStep("A").OutEdges).Target);
    }

    [Fact(DisplayName = "étant donné un brouillon où une arête depuis A cible B, quand on renomme B en C, alors l'arête depuis A cible désormais C")]
    public void Renaming_a_step_retargets_the_edges_that_pointed_at_it()
    {
        // arrange
        var draft = new WorkflowDraft(TwoStepsAtoB);

        // act
        draft.RenameStep("B", "C");

        // assert
        Assert.Equal("C", Assert.Single(draft.ToDefinition().GetStep("A").OutEdges).Target);
    }

    [Fact(DisplayName = "étant donné un brouillon, quand on renomme une étape, alors elle porte le nouvel identifiant et l'ancien ne désigne plus aucune étape")]
    public void Renaming_a_step_gives_it_the_new_identifier()
    {
        // arrange
        var draft = new WorkflowDraft(TwoStepsAtoB);

        // act
        draft.RenameStep("B", "C");

        // assert
        var definition = draft.ToDefinition();
        Assert.Contains(definition.Steps, s => s.Id == "C");
        Assert.DoesNotContain(definition.Steps, s => s.Id == "B");
    }

    [Fact(DisplayName = "étant donné un brouillon dont l'étape renommée est le point d'entrée, quand on la renomme, alors le point d'entrée suit le nouvel identifiant")]
    public void Renaming_the_entry_step_moves_the_entry_point()
    {
        // arrange
        var draft = new WorkflowDraft(TwoStepsAtoB);

        // act
        draft.RenameStep("A", "Z");

        // assert
        Assert.Equal("Z", draft.ToDefinition().EntryStep);
    }

    [Fact(DisplayName = "étant donné un brouillon, quand on supprime une étape, alors elle disparaît de la liste des étapes")]
    public void Removing_a_step_drops_it_from_the_list()
    {
        // arrange
        var draft = new WorkflowDraft(TwoStepsAtoB);

        // act
        draft.RemoveStep("B");

        // assert
        Assert.DoesNotContain(draft.ToDefinition().Steps, s => s.Id == "B");
    }

    [Fact(DisplayName = "étant donné un brouillon où une arête depuis A cible B, quand on supprime B, alors l'arête depuis A disparaît")]
    public void Removing_a_step_prunes_the_edges_that_pointed_at_it()
    {
        // arrange
        var draft = new WorkflowDraft(TwoStepsAtoB);

        // act
        draft.RemoveStep("B");

        // assert
        Assert.Empty(draft.ToDefinition().GetStep("A").OutEdges);
    }

    [Fact(DisplayName = "étant donné un brouillon dont le point d'entrée est l'étape supprimée, quand on la supprime, alors le point d'entrée ne la désigne plus")]
    public void Removing_the_entry_step_clears_the_entry_point()
    {
        // arrange
        var draft = new WorkflowDraft(TwoStepsAtoB);

        // act
        draft.RemoveStep("A");

        // assert
        Assert.NotEqual("A", draft.ToDefinition().EntryStep);
    }

    [Fact(DisplayName = "étant donné un brouillon vide, quand on ajoute une étape « Compiler », alors elle porte l'id « compiler » et le libellé « Compiler », et l'id est retourné")]
    public void Adding_a_step_slugs_its_label_into_the_identifier()
    {
        // arrange
        var draft = new WorkflowDraft(Empty);

        // act
        var id = draft.AddStep("Compiler");

        // assert
        Assert.Equal("compiler", id);
        var step = draft.ToDefinition().GetStep("compiler");
        Assert.Equal("Compiler", step.Name);
    }

    [Fact(DisplayName = "étant donné un brouillon qui a déjà une étape « compiler », quand on ajoute une seconde étape de même libellé, alors son id est désambiguïsé et les deux coexistent")]
    public void Adding_a_step_whose_slug_is_taken_disambiguates_the_identifier()
    {
        // arrange
        var draft = new WorkflowDraft(Empty);
        draft.AddStep("Compiler");

        // act
        var id = draft.AddStep("Compiler");

        // assert
        Assert.Equal("compiler-2", id);
        Assert.Equal(new[] { "compiler", "compiler-2" }, draft.ToDefinition().Steps.Select(s => s.Id));
    }

    [Fact(DisplayName = "étant donné un brouillon avec une étape, quand on la désigne comme point d'entrée, alors l'export la porte comme entrée")]
    public void Setting_the_entry_step_records_it()
    {
        // arrange
        var draft = new WorkflowDraft(Empty);
        var id = draft.AddStep("Compiler");

        // act
        draft.SetEntryStep(id);

        // assert
        Assert.Equal("compiler", draft.ToDefinition().EntryStep);
    }

    [Fact(DisplayName = "étant donné un brouillon, quand on désigne comme entrée un id encore absent, alors l'entrée est posée telle quelle")]
    public void Setting_the_entry_to_an_absent_step_is_permitted()
    {
        // arrange
        var draft = new WorkflowDraft(Empty);

        // act
        draft.SetEntryStep("pas-encore-la");

        // assert
        Assert.Equal("pas-encore-la", draft.ToDefinition().EntryStep);
    }

    [Fact(DisplayName = "étant donné un brouillon avec une étape, quand on lui affecte un ScriptSpec, alors l'export porte ce script")]
    public void Setting_a_step_script_records_it()
    {
        // arrange
        var draft = new WorkflowDraft(Empty);
        var id = draft.AddStep("Compiler");

        // act
        draft.SetScript(id, new ScriptSpec("/usr/bin/make", ["build"]));

        // assert
        Assert.Equal("/usr/bin/make", draft.ToDefinition().GetStep(id).Script.FileName);
    }

    [Fact(DisplayName = "étant donné un brouillon, quand on affecte un script à un id absent, alors une UnknownStepException est levée")]
    public void Setting_a_script_on_an_absent_step_throws()
    {
        // arrange
        var draft = new WorkflowDraft(Empty);

        // act / assert
        Assert.Throws<UnknownStepException>(() => draft.SetScript("fantome", new ScriptSpec("/bin/true", [])));
    }

    [Fact(DisplayName = "étant donné un brouillon avec deux étapes A et B, quand on ajoute une arête gardée de A vers B, alors l'export porte cette arête sortante sur A")]
    public void Adding_an_edge_records_it_on_the_source_step()
    {
        // arrange
        var draft = new WorkflowDraft(Empty);
        draft.AddStep("A");
        draft.AddStep("B");

        // act
        draft.AddEdge("a", Guard.OnSuccess, "b");

        // assert
        var edge = Assert.Single(draft.ToDefinition().GetStep("a").OutEdges);
        Assert.Equal("b", edge.Target);
        Assert.Equal(Guard.OnSuccess, edge.Guard);
    }

    [Fact(DisplayName = "étant donné un brouillon, quand on ajoute une arête depuis un id absent, alors une UnknownStepException est levée")]
    public void Adding_an_edge_from_an_absent_step_throws()
    {
        // arrange
        var draft = new WorkflowDraft(Empty);

        // act / assert
        Assert.Throws<UnknownStepException>(() => draft.AddEdge("fantome", Guard.OnSuccess, "b"));
    }

    [Fact(DisplayName = "étant donné un brouillon, quand on ajoute une arête vers une cible encore absente, alors l'arête est posée telle quelle")]
    public void Adding_an_edge_to_an_absent_target_is_permitted()
    {
        // arrange
        var draft = new WorkflowDraft(Empty);
        draft.AddStep("A");

        // act
        draft.AddEdge("a", Guard.OnSuccess, "pas-encore-la");

        // assert
        Assert.Equal("pas-encore-la", Assert.Single(draft.ToDefinition().GetStep("a").OutEdges).Target);
    }

    [Fact(DisplayName = "étant donné une étape à plusieurs arêtes, quand on en retire une par son index, alors seule celle-là disparaît")]
    public void Removing_an_edge_by_index_drops_only_that_one()
    {
        // arrange
        var draft = new WorkflowDraft(Empty);
        draft.AddStep("A");
        draft.AddEdge("a", Guard.OnSuccess, "x");
        draft.AddEdge("a", Guard.OnFailure, "y");

        // act
        draft.RemoveEdge("a", 0);

        // assert
        var edge = Assert.Single(draft.ToDefinition().GetStep("a").OutEdges);
        Assert.Equal("y", edge.Target);
    }

    [Fact(DisplayName = "étant donné un brouillon avec deux étapes A et B, quand on renomme A en « B », alors une DuplicateStepIdException est levée et le graphe reste inchangé")]
    public void Renaming_a_step_onto_a_taken_identifier_is_refused()
    {
        // arrange
        var draft = new WorkflowDraft(TwoStepsAtoB);

        // act / assert
        Assert.Throws<DuplicateStepIdException>(() => draft.RenameStep("A", "B"));
        Assert.Equal(new[] { "A", "B" }, draft.ToDefinition().Steps.Select(s => s.Id));
    }

    /// <summary>Un brouillon sans point d'entrée ni étape — ce que <c>catalog.Create</c> fait naître.</summary>
    private static WorkflowDefinition Empty => new("", []);

    /// <summary>Un graphe minimal à deux étapes reliées : A réussit vers B.</summary>
    private static WorkflowDefinition TwoStepsAtoB =>
        new("A",
        [
            new StepDefinition("A", "A", new ScriptSpec("/bin/true", []), 1,
                [new Edge(Guard.OnSuccess, "B")]),
            new StepDefinition("B", "B", new ScriptSpec("/bin/true", []), 1, []),
        ]);
}
