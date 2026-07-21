namespace Cursus.Core.Projects;

/// <summary>
/// Levée quand un <c>project.json</c> existe mais ne décrit pas un projet
/// ouvrable. Une exception plutôt qu'un rapport de validation : contrairement à
/// un workflow, un projet qu'on n'ouvre pas n'a aucun écran à alimenter.
/// </summary>
public sealed class InvalidProjectException(string path, string reason)
    : Exception($"Projet Cursus illisible ({path}) : {reason}")
{
    public string Path { get; } = path;
}
