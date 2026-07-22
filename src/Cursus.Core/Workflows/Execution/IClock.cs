namespace Cursus.Core.Workflows.Execution;

/// <summary>
/// L'heure, injectable. Le noyau n'y touche que pour horodater le journal :
/// c'est le seul endroit où une valeur non déterministe entre dans un run, et
/// la seule raison pour laquelle un test aurait besoin de la contrôler.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

/// <summary>L'horloge de la machine.</summary>
public sealed class SystemClock : IClock
{
    public static IClock Instance { get; } = new SystemClock();

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
