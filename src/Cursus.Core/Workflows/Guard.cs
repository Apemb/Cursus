namespace Cursus.Core.Workflows;

/// <summary>
/// Décide si une arête s'applique à un <see cref="ScriptResult"/> donné.
/// Les variantes sont ajoutées par triangulation, au fur et à mesure des tests.
/// </summary>
public abstract record Guard
{
    public abstract bool Matches(ScriptResult result);

    /// <summary>Vraie si le script a réussi (terminé, code 0).</summary>
    public static Guard OnSuccess { get; } = new SuccessGuard();

    /// <summary>Vraie si le script n'a pas réussi (code non nul, timeout, échec de lancement).</summary>
    public static Guard OnFailure { get; } = new FailureGuard();

    /// <summary>Vraie si le script s'est terminé avec ce code de sortie exact.</summary>
    public static Guard OnExitCode(int code) => new ExitCodeGuard(code);

    /// <summary>Vraie en toutes circonstances — arête de repli.</summary>
    public static Guard Default { get; } = new AlwaysGuard();

    private sealed record SuccessGuard : Guard
    {
        public override bool Matches(ScriptResult result) => result.IsSuccess;
    }

    private sealed record FailureGuard : Guard
    {
        public override bool Matches(ScriptResult result) => !result.IsSuccess;
    }

    private sealed record ExitCodeGuard(int Code) : Guard
    {
        public override bool Matches(ScriptResult result) =>
            result.Outcome == ScriptOutcome.Completed && result.ExitCode == Code;
    }

    private sealed record AlwaysGuard : Guard
    {
        public override bool Matches(ScriptResult result) => true;
    }
}
