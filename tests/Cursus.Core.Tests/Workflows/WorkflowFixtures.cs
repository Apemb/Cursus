using Cursus.Core.Workflows;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// Le vocabulaire commun aux tests de traversée : un workspace jetable, un
/// script qui réussit toujours, et de quoi déclarer un graphe en une ligne.
/// </summary>
internal static class WorkflowFixtures
{
    /// <summary>Un workspace neuf, partagé : aucun test ne doit écrire dedans.</summary>
    internal static readonly RunContext Workspace =
        new(Directory.CreateTempSubdirectory("cursus-engine-").FullName);

    internal static readonly ScriptSpec AnyScript = new("/usr/bin/true", []);

    internal static StepDefinition Step(string id, params Edge[] edges) =>
        new(id, id, AnyScript, MaxVisits: 1, edges);

    internal static StepDefinition Step(string id, int maxVisits, params Edge[] edges) =>
        new(id, id, AnyScript, maxVisits, edges);

    internal static ScriptResult Exit(int code) => new(code, ScriptOutcome.Completed);

    /// <summary>
    /// Un moteur dont on ne compte pas relire le journal : les tests de
    /// traversée l'ignorent, mais le moteur en exige un — un run muet est un
    /// run qu'on ne peut pas relire.
    /// </summary>
    internal static WorkflowEngine Engine(IProcessRunner runner) => new(runner, new InMemoryRunJournal());
}

/// <summary>
/// Une horloge pilotée par le test, pour que l'horodatage cesse d'être une
/// inconnue — et qu'on puisse la faire avancer quand l'ordre en dépend.
/// </summary>
internal sealed class TestClock(DateTimeOffset instant) : IClock
{
    public DateTimeOffset UtcNow { get; set; } = instant;
}

/// <summary>
/// Double de test : renvoie des résultats programmés, dans l'ordre ; répète le
/// dernier une fois la liste épuisée (pratique pour « le runner réussit toujours »).
/// </summary>
internal sealed class StubProcessRunner : IProcessRunner
{
    private readonly IReadOnlyList<ScriptResult> _results;
    private int _index;

    public StubProcessRunner(params ScriptResult[] results) => _results = results;

    /// <summary>Si fourni, s'annule une fois l'exécution rendue — comme une annulation survenant pendant le run.</summary>
    public CancellationTokenSource? CancelAfterRun { get; init; }

    /// <summary>Les specs effectivement reçues, dans l'ordre — le moteur les compose avant de les transmettre.</summary>
    public List<ScriptSpec> Executed { get; } = [];

    public Task<ScriptResult> RunAsync(ScriptSpec spec, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Executed.Add(spec);

        var result = _results[Math.Min(_index, _results.Count - 1)];
        _index++;
        CancelAfterRun?.Cancel();
        return Task.FromResult(result);
    }
}
