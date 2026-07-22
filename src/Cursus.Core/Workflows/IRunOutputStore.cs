namespace Cursus.Core.Workflows;

/// <summary>
/// Ouvre un puits par visite, avant qu'elle s'exécute — c'est ce qui permet à
/// la sortie de ruisseler pendant le run plutôt que d'être écrite à la fin. Le
/// moteur en dépend comme il dépend d'<see cref="IRunJournal"/> ; la persistance
/// l'implémente sur fichier.
/// </summary>
public interface IRunOutputStore
{
    IStepOutputSink Open(string runId, string stepId, int iteration);
}
