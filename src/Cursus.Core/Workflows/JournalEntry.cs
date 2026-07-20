namespace Cursus.Core.Workflows;

/// <summary>
/// Un événement tel que le journal l'a enregistré. L'émetteur ne fournit que
/// l'événement : c'est le journal qui pose le numéro de séquence et l'instant.
/// </summary>
/// <param name="Seq">
/// L'ordre autoritaire. Jamais <paramref name="At"/> : une horloge peut reculer.
/// </param>
public sealed record JournalEntry(string RunId, long Seq, DateTimeOffset At, WorkflowEvent Event);
