namespace Cursus.Core.Workflows;

/// <summary>
/// Abstraction du lancement d'un process. Le moteur délègue ici et ne fait
/// jamais de <c>Process.Start</c> lui-même — ce qui le rend entièrement
/// testable sur un double renvoyant des résultats programmés. L'implémentation
/// réelle (System.Diagnostics.Process) arrive au jalon 2.
/// </summary>
public interface IProcessRunner
{
    ScriptResult Run(ScriptSpec spec);
}
