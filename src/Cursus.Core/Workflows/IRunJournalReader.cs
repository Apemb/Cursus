namespace Cursus.Core.Workflows;

/// <summary>
/// Relit ce qu'un journal a gardé. Séparée de <see cref="IRunJournal"/> pour
/// que le moteur ne puisse pas lire : il produit l'histoire, il ne la consulte
/// jamais.
/// </summary>
public interface IRunJournalReader
{
    /// <summary>Les runs connus, du plus récemment démarré au plus ancien.</summary>
    IReadOnlyList<RunSummary> ListRuns();

    /// <summary>Les événements d'un run, dans l'ordre de leur numéro de séquence.</summary>
    IReadOnlyList<JournalEntry> ReadEvents(string runId);
}
