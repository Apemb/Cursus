namespace Cursus.Core.Sessions;

/// <summary>
/// Description d'une session gérée par Cursus, indépendante de toute
/// technologie d'affichage. La couche UI associe à cette description un
/// contrôle terminal concret ; la logique agentique future s'appuiera sur
/// le même modèle (voir <see cref="SessionKind.Agent"/>).
/// </summary>
public sealed class TerminalSession
{
    public TerminalSession(
        string title,
        string shellPath,
        string workingDirectory,
        SessionKind kind = SessionKind.Shell)
    {
        Id = Guid.NewGuid();
        Title = title;
        ShellPath = shellPath;
        WorkingDirectory = workingDirectory;
        Kind = kind;
        CreatedAt = DateTimeOffset.Now;
    }

    public Guid Id { get; }

    public string Title { get; set; }

    public string ShellPath { get; }

    public string WorkingDirectory { get; }

    public SessionKind Kind { get; }

    public DateTimeOffset CreatedAt { get; }

    /// <summary>Fabrique une session shell avec les défauts de l'hôte.</summary>
    public static TerminalSession CreateShell(string title)
        => new(title, ShellEnvironment.DefaultShell(), ShellEnvironment.DefaultWorkingDirectory());
}
