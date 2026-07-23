namespace Cursus.Core.Workflows;

/// <summary>
/// Ce qu'on sait d'un run sans avoir relu ses événements : de quoi dresser une
/// liste. Le détail se demande run par run, à <see cref="IRunJournalReader"/>.
/// </summary>
/// <param name="State">
/// Absent tant que le run n'a pas été clos. ⚠️ Un run tué par un crash machine
/// reste indiscernable d'un run en cours — la reprise après incident n'est pas
/// traitée.
/// </param>
/// <param name="EndedAt">
/// L'instant de clôture, absent tant que le run n'est pas clos. Va de pair avec
/// <paramref name="State"/> : les deux apparaissent au <c>RunFinished</c>.
/// </param>
/// <param name="WorkflowId">
/// Le workflow du catalogue dont ce run est issu, ou <c>null</c> pour un run que
/// rien n'a nommé. C'est lui qui permet de retrouver « le dernier passage de
/// verifier » sans relire les événements.
/// </param>
public sealed record RunSummary(
    string RunId,
    DateTimeOffset StartedAt,
    RunState? State = null,
    AbortReason? AbortReason = null,
    DateTimeOffset? EndedAt = null,
    string? WorkflowId = null);
