namespace Cursus.Core.Sessions;

/// <summary>
/// Politique pure de résolution du chemin de shell interactif.
/// Le test d'existence de fichier est injecté pour rester déterministe.
/// </summary>
public static class ShellResolver
{
    public static string Resolve(string? shellEnvVar, Func<string, bool> fileExists)
    {
        if (!string.IsNullOrWhiteSpace(shellEnvVar) && fileExists(shellEnvVar))
            return shellEnvVar;

        if (fileExists("/bin/zsh"))
            return "/bin/zsh";

        return "/bin/bash";
    }
}
