namespace Cursus.Core.Workflows;

/// <summary>
/// Levée quand un sous-chemin déclaré par une étape désigne un emplacement
/// hors de la racine du run. La comparaison porte sur les chemins normalisés :
/// elle ne suit pas les liens symboliques, et n'est donc pas un confinement OS.
/// </summary>
public sealed class PathEscapesWorkspaceException(string subdirectory, string workspaceRoot)
    : Exception($"Le sous-chemin « {subdirectory} » sort du workspace du run : {workspaceRoot}")
{
    public string Subdirectory { get; } = subdirectory;

    public string WorkspaceRoot { get; } = workspaceRoot;
}
