namespace Cursus.Core.Workflows;

/// <summary>
/// Ce qu'on sait d'un run sans avoir relu ses événements : de quoi dresser une
/// liste. Le détail se demande run par run, à <see cref="IRunJournalReader"/>.
/// </summary>
public sealed record RunSummary(string RunId, DateTimeOffset StartedAt);
