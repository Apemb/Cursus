using Cursus.Core.Workflows.Journaling;
using Cursus.Core.Workflows.Output;
using Cursus.Core.Workflows.Workspaces;

namespace Cursus.Core.Workflows.Execution;

/// <summary>
/// Réifie le montage d'un vrai run — ce que <c>ProjectRunTests</c> faisait à la
/// main. Un lancement : forger l'identité du run, provisionner un workspace isolé
/// à ce nom, monter le moteur sur le journal durable et le store d'artefacts,
/// exécuter en estampillant la provenance de workflow, puis démonter le workspace.
///
/// <para>
/// <b>Un run à la fois</b> par appel : la concurrence est compositionnelle —
/// l'appelant lance plusieurs runs de front, chacun dans son propre worktree,
/// comme le prouve le jalon 6b. Le lanceur ne porte aucun état partagé.
/// </para>
/// </summary>
public sealed class WorkflowLauncher
{
    private readonly WorkflowEngine _engine;
    private readonly IWorkspaceProvisioner _provisioner;

    public WorkflowLauncher(
        IProcessRunner runner,
        IRunJournal journal,
        IRunOutputStore output,
        IWorkspaceProvisioner provisioner)
    {
        _engine = new WorkflowEngine(runner, journal, output);
        _provisioner = provisioner;
    }

    /// <summary>
    /// Lance <paramref name="definition"/> comme run du workflow
    /// <paramref name="workflowId"/> (sa provenance journalisée). Le worktree est
    /// monté neuf à partir de <c>HEAD</c> ; le rejeu d'une ref existante viendra
    /// avec la reprise d'un run interrompu.
    /// </summary>
    public async Task<WorkflowRun> LaunchAsync(
        WorkflowDefinition definition,
        string workflowId,
        RunTrigger? trigger = null,
        IProgress<WorkflowEvent>? observer = null,
        CancellationToken cancellationToken = default)
    {
        var runId = Guid.NewGuid().ToString();

        // Le worktree monte à un emplacement dérivé du runId : c'est ce qui permet
        // de le retrouver depuis le journal. Le refermer (await using) le démonte,
        // quoi qu'il advienne du run — le travail à garder est déjà commité sur sa
        // branche. Montage et démontage s'attendent (I/O git), sans détenir le
        // thread appelant ; ConfigureAwait(false) garde tout hors du contexte UI.
        await using var workspace = await _provisioner
            .ProvisionAsync(runId, new WorkspaceRequest.NewWork("HEAD"), cancellationToken)
            .ConfigureAwait(false);

        return await _engine.ExecuteAsync(
            definition, workspace.Context, runId, trigger, workflowId, observer, cancellationToken)
            .ConfigureAwait(false);
    }
}
