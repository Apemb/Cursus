namespace Cursus.Core.Workflows.Execution;

/// <summary>
/// L'instantané d'une tâche du tableau, tel que le tracker le rend en lecture :
/// son identité, son contenu, et là où elle en est. Une donnée neutre — ni le
/// moteur ni l'exécuteur n'y voient de sémantique de tracker, ils la relaient
/// (l'exécuteur en écrit le corps dans le worktree).
/// </summary>
public sealed record TaskCard(
    string Key,
    string Title,
    string Description,
    string Column,
    IReadOnlyList<string> Labels);
