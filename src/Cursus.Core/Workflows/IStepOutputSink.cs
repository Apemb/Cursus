namespace Cursus.Core.Workflows;

/// <summary>
/// Le puits d'une visite : deux flux où déverser les sorties pendant qu'elles
/// s'écrivent, et, une fois close, le <see cref="StepOutput"/> de ce qui a été
/// rangé. Orienté script (deux flux) parce que seul le script existe — l'agent
/// apportera sa propre capture, et c'est <b>lui</b> qu'on retouchera, pas la
/// valeur produite.
/// </summary>
public interface IStepOutputSink : IDisposable
{
    Stream Stdout { get; }

    Stream Stderr { get; }

    /// <summary>Clôt l'écriture et rend ce qui a été rangé.</summary>
    StepOutput Complete();
}
