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
public sealed record RunSummary(
    string RunId,
    DateTimeOffset StartedAt,
    RunState? State = null,
    AbortReason? AbortReason = null);
