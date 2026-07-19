namespace Cursus.Core.Sessions;

/// <summary>
/// Découverte de l'environnement shell de l'hôte (UI-agnostique, testable).
/// </summary>
public static class ShellEnvironment
{
    /// <summary>
    /// Shell interactif par défaut : la variable <c>SHELL</c> si présente,
    /// sinon <c>/bin/zsh</c> (défaut macOS), sinon <c>/bin/bash</c>.
    /// Adaptateur en bordure : la politique testée vit dans <see cref="ShellResolver"/>.
    /// </summary>
    public static string DefaultShell()
        => ShellResolver.Resolve(Environment.GetEnvironmentVariable("SHELL"), File.Exists);

    /// <summary>Répertoire de travail initial : le home de l'utilisateur.</summary>
    public static string DefaultWorkingDirectory()
        => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
}
