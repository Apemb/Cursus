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

    /// <summary>Un graphe minimal à deux étapes reliées : A réussit vers B.</summary>
    private static WorkflowDefinition TwoStepsAtoB =>
        new("A",
        [
            new StepDefinition("A", "A", new ScriptSpec("/bin/true", []), 1,
                [new Edge(Guard.OnSuccess, "B")]),
            new StepDefinition("B", "B", new ScriptSpec("/bin/true", []), 1, []),
        ]);
}
