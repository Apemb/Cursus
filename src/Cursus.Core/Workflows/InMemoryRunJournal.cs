namespace Cursus.Core.Workflows;

/// <summary>
/// Journal volatile : le double des tests, et le seul que le noyau embarque.
/// Il ne survit pas au process — c'est ce que la persistance ajoute.
/// </summary>
public sealed class InMemoryRunJournal : IRunJournal, IRunJournalReader
{
    private readonly List<JournalEntry> _entries = [];
    private readonly IClock _clock;

    public InMemoryRunJournal(IClock? clock = null) => _clock = clock ?? SystemClock.Instance;

    /// <summary>Tout ce qui a été journalisé, dans l'ordre d'arrivée, runs confondus.</summary>
    public IReadOnlyList<JournalEntry> Entries => _entries;

    public void Append(string runId, WorkflowEvent @event)
    {
        // La séquence est propre à chaque run : c'est elle qui fait foi sur
        // l'ordre, et deux runs concurrents ne doivent pas se la partager.
        var seq = _entries.Count(entry => entry.RunId == runId) + 1;
        _entries.Add(new JournalEntry(runId, seq, _clock.UtcNow, @event));
    }

    public IReadOnlyList<RunSummary> ListRuns() =>
        _entries
            .Where(entry => entry.Event is WorkflowEvent.RunStarted)
            .OrderByDescending(entry => entry.At)
            .Select(entry => Summarize(entry))
            .ToList();

    private RunSummary Summarize(JournalEntry start)
    {
        // Un run sans clôture est un run en cours : son état terminal reste
        // absent, il ne s'invente pas.
        var finish = _entries
            .Where(entry => entry.RunId == start.RunId)
            .Select(entry => entry.Event)
            .OfType<WorkflowEvent.RunFinished>()
            .LastOrDefault();

        return new RunSummary(start.RunId, start.At, finish?.State, finish?.AbortReason);
    }

    public IReadOnlyList<JournalEntry> ReadEvents(string runId) =>
        _entries.Where(entry => entry.RunId == runId).OrderBy(entry => entry.Seq).ToList();
}
