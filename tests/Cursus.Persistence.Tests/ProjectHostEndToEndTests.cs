using System.Diagnostics;

using Cursus.Core.Projects;
using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Projection;

namespace Cursus.Persistence.Tests;

/// <summary>
/// Les deux tests que §7.12 exige pour rendre <c>ProjectHost</c> exécutable : des
/// end-to-end <b>headless</b>, sur une <b>vraie</b> base SQLite, sans instancier
/// Avalonia. Ils forcent le préréglage de <c>Cursus.Persistence</c> à suffire —
/// lire le passé d'un projet (6c·3a), puis <b>lancer</b> un run et le relire (6c·3b).
/// </summary>
public sealed class ProjectHostEndToEndTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("cursus-host-e2e-").FullName;

    [Fact(DisplayName = "étant donné un run journalisé dans la vraie base d'un projet, quand on ouvre un ProjectHost via le préréglage et lit le dernier passage, alors il rend ce run et son état, sans Avalonia")]
    public void A_project_host_reads_the_last_passage_over_a_real_database()
    {
        // arrange — un vrai projet sur disque, un run écrit dans sa vraie base
        var project = ProjectStore.Create(_root, "Démo");
        Deposit(project, "verifier");
        using (var journal = new SqliteRunJournal(project.DatabasePath))
        {
            journal.Append("r1", new WorkflowEvent.RunStarted(
                AnyDefinition, "/tmp", RunTrigger.Manual, WorkflowId: "verifier"));
            journal.Append("r1", new WorkflowEvent.RunFinished(RunState.Failed));
        }

        // act — le host rouvre la même base par le seul préréglage
        using var host = SqliteProjectHost.Open(project);
        var passage = host.LastRunPerWorkflow().Single(passage => passage.Workflow.Id == "verifier");

        // assert
        Assert.Equal("r1", passage.LastRun!.RunId);
        Assert.Equal(RunState.Failed, passage.LastRun!.State);
    }

    [Fact(DisplayName = "étant donné un projet-dépôt-git ouvert par le préréglage, quand on lance un de ses workflows puis relit le dernier passage, alors le run atteint son état terminal et se relit rattaché à ce workflow, sans Avalonia")]
    public async Task A_project_host_launches_a_workflow_and_reads_it_back_over_a_real_database()
    {
        // arrange — un vrai projet, un workflow qui écrit un fichier, le tout érigé
        // en dépôt git (le provisionnement monte un worktree à partir de HEAD)
        var project = ProjectStore.Create(_root, "Démo");
        File.WriteAllText(
            Path.Combine(project.WorkflowsDirectory, "verifier.json"),
            """
            {
              "entryStep": "verifier",
              "steps": [
                { "id": "verifier", "name": "Vérifier", "maxVisits": 1,
                  "script": { "fileName": "/bin/sh", "arguments": ["-c", "echo ok > resultat.txt"] },
                  "edges": [] }
              ]
            }
            """);
        InitRepository();

        // act — le host ouvert par le seul préréglage lance, puis relit son propre journal
        using var host = SqliteProjectHost.Open(project);
        var run = await host.LaunchAsync("verifier");
        var passage = host.LastRunPerWorkflow().Single(passage => passage.Workflow.Id == "verifier");

        // assert — le run a abouti, et le « jamais lancé » de 3a s'est rempli
        Assert.Equal(RunState.Completed, run.State);
        Assert.Equal(run.RunId, passage.LastRun!.RunId);
        Assert.Equal(RunState.Completed, passage.LastRun!.State);
    }

    [Fact(DisplayName = "étant donné un run lancé par le préréglage, quand on plie son flux live et qu'on plie sa relecture ReadEvents, alors les deux projections coïncident — même écran, deux alimentations, sans Avalonia")]
    public async Task Folding_the_live_flux_and_folding_the_replay_yield_the_same_projection()
    {
        // arrange — un vrai projet-dépôt-git avec un workflow qui aboutit
        var project = ProjectStore.Create(_root, "Démo");
        File.WriteAllText(
            Path.Combine(project.WorkflowsDirectory, "verifier.json"),
            """
            {
              "entryStep": "verifier",
              "steps": [
                { "id": "verifier", "name": "Vérifier", "maxVisits": 1,
                  "script": { "fileName": "/bin/sh", "arguments": ["-c", "echo ok > resultat.txt"] },
                  "edges": [] }
              ]
            }
            """);
        InitRepository();

        // act — une alimentation *live* : la projection se plie au fil de l'émission
        using var host = SqliteProjectHost.Open(project);
        var live = new ProjectingObserver();
        var run = await host.LaunchAsync("verifier", observer: live);

        // ... et une alimentation *relecture* : la même projection, pliée du journal
        var replay = new RunProjection();
        foreach (var entry in host.ReadEvents(run.RunId))
            replay.Apply(entry.Event);

        // assert — les deux projections décrivent le même écran. La durée est
        // neutralisée : le journal la range en secondes-double (lossy, assumé —
        // c'est une métrique, pas une entrée de routage), seul axe où flux et
        // relecture peuvent différer ; tout le reste doit être bit-à-bit égal.
        Assert.Equal(live.Projection.State, replay.State);
        Assert.Equal(live.Projection.AbortReason, replay.AbortReason);
        Assert.Equal(
            live.Projection.Trajectory.Select(StripDuration),
            replay.Trajectory.Select(StripDuration));
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    // --- helpers ---

    /// <summary>Une visite dont la durée est remise à zéro — pour comparer flux et relecture hors de l'imprécision assumée du journal.</summary>
    private static RunVisit StripDuration(RunVisit visit) =>
        visit.Result is null ? visit : visit with { Result = visit.Result with { Duration = default } };

    /// <summary>
    /// L'alimentation live sous forme testable : un <see cref="IProgress{T}"/>
    /// <b>synchrone</b> (l'émission du moteur l'est) qui plie chaque événement dans
    /// sa projection au fil de l'eau — exactement ce que fera le RunViewModel.
    /// </summary>
    private sealed class ProjectingObserver : IProgress<WorkflowEvent>
    {
        public RunProjection Projection { get; } = new();

        public void Report(WorkflowEvent value) => Projection.Apply(value);
    }

    /// <summary>Érige la racine du projet en dépôt git avec un commit initial — la base des worktrees.</summary>
    private void InitRepository()
    {
        Git("init");
        Git("config", "user.email", "test@cursus.dev");
        Git("config", "user.name", "Cursus Test");
        Git("add", "-A");
        Git("commit", "-m", "commit initial");
    }

    /// <summary>git piloté en direct pour le décor — hors production, l'invariant 3 ne s'y applique pas.</summary>
    private void Git(params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = _root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo)!;
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"git {string.Join(' ', arguments)} a échoué ({process.ExitCode}) : {stderr}");
    }

    private static void Deposit(Project project, string id) =>
        File.WriteAllText(Path.Combine(project.WorkflowsDirectory, $"{id}.json"), AnyDocument);

    /// <summary>Un graphe valide : la définition figée repasse par le validateur à la relecture SQLite.</summary>
    private static WorkflowDefinition AnyDefinition => new("A", new[]
    {
        new StepDefinition("A", "A", new ScriptSpec("/usr/bin/true", []), MaxVisits: 1, []),
    });

    private const string AnyDocument = """
        {
          "entryStep": "seule",
          "steps": [
            { "id": "seule", "name": "Seule", "maxVisits": 1,
              "script": { "fileName": "/bin/true", "arguments": [] }, "edges": [] }
          ]
        }
        """;
}
