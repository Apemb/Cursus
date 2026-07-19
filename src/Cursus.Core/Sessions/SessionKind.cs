namespace Cursus.Core.Sessions;

/// <summary>
/// Nature d'une session. Pour l'instant seul <see cref="Shell"/> existe ;
/// <see cref="Agent"/> est réservé à la future orchestration agentique.
/// </summary>
public enum SessionKind
{
    Shell,
    Agent,
}
