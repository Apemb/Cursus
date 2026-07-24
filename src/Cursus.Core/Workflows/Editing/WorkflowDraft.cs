namespace Cursus.Core.Workflows.Editing;

/// <summary>
/// La surface d'édition mutable d'un graphe. <see cref="WorkflowDefinition"/> est
/// un instantané immuable, parfait à exécuter mais inutilisable à éditer : ce
/// type porte l'identité mutable et les opérations qui préservent l'intégrité
/// référentielle du graphe pendant qu'on le remanie.
/// </summary>
public sealed class WorkflowDraft
{
    public WorkflowDraft(WorkflowDefinition definition)
    {
        EntryStep = definition.EntryStep;
        _steps = [.. definition.Steps];
    }

    private readonly List<StepDefinition> _steps;

    /// <summary>Le point d'entrée en cours d'édition.</summary>
    public string EntryStep { get; private set; }

    /// <summary>
    /// Renomme une étape et fait suivre toute référence : les arêtes qui la
    /// visaient sont retargées. Sans quoi le renommage laisserait le graphe
    /// incohérent avec lui-même.
    /// </summary>
    public void RenameStep(string oldId, string newId)
    {
        if (EntryStep == oldId)
            EntryStep = newId;

        for (var i = 0; i < _steps.Count; i++)
            if (_steps[i].Id == oldId)
                _steps[i] = _steps[i] with { Id = newId };

        MapEdges(e => e.Target == oldId ? e with { Target = newId } : e);
    }

    /// <summary>
    /// Supprime une étape et purge toute référence : les arêtes qui la visaient
    /// disparaissent, le point d'entrée se vide s'il la désignait. Les références
    /// suivent le sort de leur cible — le graphe reste clos plutôt que de laisser
    /// pendre des arêtes vers un fantôme créé par la suppression elle-même.
    /// </summary>
    public void RemoveStep(string id)
    {
        if (EntryStep == id)
            EntryStep = "";

        _steps.RemoveAll(s => s.Id == id);

        MapEdges(e => e.Target == id ? null : e);
    }

    /// <summary>Fige la surface de travail en un instantané que le catalogue persiste.</summary>
    public WorkflowDefinition ToDefinition() => new(EntryStep, [.. _steps]);

    /// <summary>
    /// L'opération référentielle commune : réécrit les arêtes sortantes de chaque
    /// étape. Une transformation qui rend <c>null</c> purge l'arête ; c'est ainsi
    /// que renommer les retarge et que supprimer les retire.
    /// </summary>
    private void MapEdges(Func<Edge, Edge?> transform)
    {
        for (var i = 0; i < _steps.Count; i++)
            _steps[i] = _steps[i] with
            {
                OutEdges = [.. _steps[i].OutEdges.Select(transform).OfType<Edge>()],
            };
    }
}
