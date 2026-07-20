namespace Cursus.Core.Workflows;

/// <summary>
/// Reçoit les événements d'un run et les rend durables, dans l'ordre.
/// L'écriture est <b>synchrone</b> : une transaction par événement, pour qu'un
/// crash laisse un journal exploitable jusqu'au dernier instant.
/// </summary>
public interface IRunJournal
{
    void Append(string runId, WorkflowEvent @event);
}
