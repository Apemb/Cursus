using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Execution;
using Cursus.Core.Workflows.Journaling;
using Cursus.Core.Workflows.Output;
using Cursus.Core.Workflows.Workspaces;

using static Cursus.Core.Tests.Workflows.WorkflowFixtures;

namespace Cursus.Core.Tests.Workflows;

/// <summary>
/// Le lanceur réifie ce que <c>ProjectRunTests</c> faisait à la main : provisionner
/// un workspace isolé, monter le moteur sur ses collaborateurs durables, exécuter en
/// estampillant la provenance, puis démonter. Testé ici avec des doubles en mémoire,
/// comme le moteur.
/// </summary>
public class WorkflowLauncherTests
{
    [Fact(DisplayName = "étant donné un workflow d'une étape réussie et un provisionneur en mémoire, quand on lance, alors un workspace est provisionné et le run rendu est terminé")]
    public async Task Launching_provisions_a_workspace_and_runs_to_a_terminal_state()
    {
        // arrange
        var provisioner = new FakeProvisioner();
        var launcher = new WorkflowLauncher(
            new StubProcessRunner(Exit(0)), new InMemoryRunJournal(), new InMemoryRunOutputStore(), provisioner);
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        var run = await launcher.LaunchAsync(definition, "verifier");

        // assert
        Assert.True(provisioner.Provisioned);
        Assert.Equal(RunState.Completed, run.State);
    }

    [Fact(DisplayName = "étant donné un lancement, quand le run est journalisé, alors son démarrage porte l'id du workflow lancé (le lanceur est le producteur qui manquait)")]
    public async Task The_launch_journals_the_workflow_id_of_the_launched_workflow()
    {
        // arrange
        var journal = new InMemoryRunJournal();
        var launcher = new WorkflowLauncher(
            new StubProcessRunner(Exit(0)), journal, new InMemoryRunOutputStore(), new FakeProvisioner());
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        await launcher.LaunchAsync(definition, "verifier");

        // assert
        var started = Assert.IsType<WorkflowEvent.RunStarted>(journal.Entries[0].Event);
        Assert.Equal("verifier", started.WorkflowId);
    }

    [Fact(DisplayName = "étant donné un workspace provisionné, quand le run se termine, alors le workspace est démonté (aucun worktree qui fuit)")]
    public async Task The_workspace_is_torn_down_once_the_run_ends()
    {
        // arrange
        var provisioner = new FakeProvisioner();
        var launcher = new WorkflowLauncher(
            new StubProcessRunner(Exit(0)), new InMemoryRunJournal(), new InMemoryRunOutputStore(), provisioner);
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        await launcher.LaunchAsync(definition, "verifier");

        // assert
        Assert.True(provisioner.Last!.Disposed);
    }

    [Fact(DisplayName = "étant donné un observateur passé au lancement, quand le run progresse, alors il reçoit le flux d'événements du moteur")]
    public async Task An_observer_passed_to_the_launch_receives_the_engine_stream()
    {
        // arrange
        var observer = new RecordingObserver();
        var launcher = new WorkflowLauncher(
            new StubProcessRunner(Exit(0)), new InMemoryRunJournal(), new InMemoryRunOutputStore(), new FakeProvisioner());
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act
        await launcher.LaunchAsync(definition, "verifier", observer: observer);

        // assert
        Assert.IsType<WorkflowEvent.RunStarted>(observer.Events[0]);
        Assert.IsType<WorkflowEvent.RunFinished>(observer.Events[^1]);
    }

    [Fact(DisplayName = "étant donné un run lancé avec un observateur, quand il démarre, alors le RunStarted émis porte l'identité du run rendu")]
    public async Task The_emitted_run_started_carries_the_returned_run_identity()
    {
        // arrange
        var observer = new RecordingObserver();
        var launcher = new WorkflowLauncher(
            new StubProcessRunner(Exit(0)), new InMemoryRunJournal(), new InMemoryRunOutputStore(), new FakeProvisioner());
        var definition = new WorkflowDefinition("A", new[] { Step("A") });

        // act — le flux live doit être auto-descriptif : dire quel run il ouvre
        var run = await launcher.LaunchAsync(definition, "verifier", observer: observer);

        // assert — la vue tailera les artefacts de ce runId dès le démarrage
        var started = Assert.IsType<WorkflowEvent.RunStarted>(observer.Events[0]);
        Assert.Equal(run.RunId, started.RunId);
    }
}

/// <summary>
/// Provisionneur double : rend un workspace jetable sur un répertoire temporaire
/// réel (le moteur y résout ses sous-chemins), et retient s'il a été démonté.
/// </summary>
internal sealed class FakeProvisioner : IWorkspaceProvisioner
{
    public bool Provisioned { get; private set; }

    public FakeWorkspace? Last { get; private set; }

    public Task<IProvisionedWorkspace> ProvisionAsync(
        string runId, WorkspaceRequest request, CancellationToken cancellationToken = default)
    {
        Provisioned = true;
        return Task.FromResult<IProvisionedWorkspace>(Last = new FakeWorkspace());
    }
}

internal sealed class FakeWorkspace : IProvisionedWorkspace
{
    private readonly string _directory = Directory.CreateTempSubdirectory("cursus-launch-").FullName;

    public FakeWorkspace() => Context = new RunContext(_directory);

    public RunContext Context { get; }

    public bool Disposed { get; private set; }

    public ValueTask DisposeAsync()
    {
        Disposed = true;
        Directory.Delete(_directory, recursive: true);
        return ValueTask.CompletedTask;
    }
}
