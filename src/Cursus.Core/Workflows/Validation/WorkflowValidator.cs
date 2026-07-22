namespace Cursus.Core.Workflows.Validation;

/// <summary>
/// Répond à « ce graphe est-il exécutable ? » par la liste exhaustive de ce qui
/// l'en empêche, dans l'ordre de déclaration des étapes. Les règles émergent
/// des tests, une à une.
/// </summary>
public static class WorkflowValidator
{
    public static ValidationReport Validate(WorkflowDefinition definition)
    {
        var known = definition.Steps.Select(s => s.Id).ToHashSet();
        var issues = new List<ValidationIssue>();

        issues.AddRange(EntryStepIssues(definition, known));
        issues.AddRange(DuplicateIdIssues(definition));
        issues.AddRange(definition.Steps.SelectMany(step => StepIssues(step, known)));

        // L'atteignabilité n'a de sens qu'à partir d'un point d'entrée valide ;
        // sinon tout le graphe serait rapporté inatteignable, en pure cascade.
        if (issues.All(i => i.Kind is not (ValidationIssueKind.MissingEntryStep or ValidationIssueKind.UnknownEntryStep)))
            issues.AddRange(UnreachableStepIssues(definition));

        return new ValidationReport(issues);
    }

    private static IEnumerable<ValidationIssue> EntryStepIssues(
        WorkflowDefinition definition, IReadOnlySet<string> known)
    {
        if (string.IsNullOrWhiteSpace(definition.EntryStep))
            yield return new ValidationIssue(
                ValidationIssueKind.MissingEntryStep,
                "Le workflow ne désigne aucun point d'entrée.");
        else if (!known.Contains(definition.EntryStep))
            yield return new ValidationIssue(
                ValidationIssueKind.UnknownEntryStep,
                $"Le point d'entrée « {definition.EntryStep} » ne correspond à aucune étape.",
                definition.EntryStep);
    }

    private static IEnumerable<ValidationIssue> DuplicateIdIssues(WorkflowDefinition definition) =>
        definition.Steps
            .GroupBy(s => s.Id)
            .Where(g => g.Count() > 1)
            .Select(g => new ValidationIssue(
                ValidationIssueKind.DuplicateStepId,
                $"L'identifiant « {g.Key} » est porté par {g.Count()} étapes.",
                g.Key));

    private static IEnumerable<ValidationIssue> StepIssues(StepDefinition step, IReadOnlySet<string> known)
    {
        if (string.IsNullOrWhiteSpace(step.Id))
            yield return new ValidationIssue(
                ValidationIssueKind.EmptyStepId,
                "Une étape n'a pas d'identifiant.");

        if (step.MaxVisits < 1)
            yield return new ValidationIssue(
                ValidationIssueKind.NonPositiveMaxVisits,
                $"L'étape « {step.Id} » autorise {step.MaxVisits} visite(s) : elle ne s'exécuterait jamais.",
                step.Id);

        foreach (var edge in step.OutEdges.Where(e => !known.Contains(e.Target)))
            yield return new ValidationIssue(
                ValidationIssueKind.UnknownEdgeTarget,
                $"L'étape « {step.Id} » a une arête vers « {edge.Target} », qui n'existe pas.",
                step.Id);
    }

    private static IEnumerable<ValidationIssue> UnreachableStepIssues(WorkflowDefinition definition)
    {
        var reachable = Reachable(definition);

        // Une étape sans identifiant est déjà signalée pour ça : la dire aussi
        // inatteignable n'ajouterait qu'un doublon à corriger.
        return definition.Steps
            .Where(s => !string.IsNullOrWhiteSpace(s.Id) && !reachable.Contains(s.Id))
            .Select(s => new ValidationIssue(
                ValidationIssueKind.UnreachableStep,
                $"Aucun chemin ne mène à l'étape « {s.Id} » depuis le point d'entrée.",
                s.Id));
    }

    /// <summary>
    /// Les étapes atteignables depuis le point d'entrée en suivant les arêtes,
    /// quelle que soit leur garde : une étape rattrapée par un seul chemin
    /// d'échec est atteignable. Les cycles sont absorbés par l'ensemble visité.
    /// </summary>
    private static HashSet<string> Reachable(WorkflowDefinition definition)
    {
        var reached = new HashSet<string>();
        var pending = new Stack<string>([definition.EntryStep]);

        while (pending.TryPop(out var id))
        {
            if (!reached.Add(id))
                continue;

            var step = definition.Steps.FirstOrDefault(s => s.Id == id);
            foreach (var edge in step?.OutEdges ?? [])
                pending.Push(edge.Target);
        }

        return reached;
    }
}
