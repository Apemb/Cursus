using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Execution;
using Cursus.Core.Workflows.Journaling;

namespace Cursus.Core.Projects;

/// <summary>
/// La racine de composition d'un projet ouvert (architecture.md §7.12). Elle
/// possède le journal du projet — qu'elle construit via une fabrique reçue, sans
/// jamais apprendre que c'est du SQLite — et n'accueille que ce qui demande une
/// composition. Pour la marche 3a, une seule capacité : le dernier passage de
/// chaque workflow. Lister et charger restent <see cref="WorkflowCatalog"/>.
///
/// <para>
/// <b>Règle de sens unique</b> : aucun module ne connaît le host. Ses
/// collaborateurs reçoivent la projection, jamais le host lui-même — le lui
/// passer en ferait un Service Locator.
/// </para>
/// <para>
/// <b><see cref="IDisposable"/></b> : imposé par le code, pas par le style. Le
/// journal SQLite détient une connexion unique non synchronisée ; ouvrir un autre
/// projet, c'est disposer ce host et en reconstruire un, jamais le muter.
/// </para>
/// </summary>
public sealed class ProjectHost : IDisposable
{
    private readonly WorkflowCatalog _catalog;
    private readonly IRunJournalReader _journal;
    private readonly WorkflowLauncher _launcher;

    /// <param name="journal">
    /// La fabrique du journal du projet. Le host l'appelle une fois et détient le
    /// résultat ; c'est <c>Cursus.Persistence</c> qui la lie au vrai
    /// <c>SqliteRunJournal</c>, pour que le câblage concret n'existe qu'en un lieu.
    /// </param>
    /// <param name="launcher">
    /// Le lanceur du projet, câblé par le même préréglage sur le <b>même</b>
    /// journal que le lecteur — une seule instance en mémoire, une seule connexion
    /// en SQLite : ce qui a été lancé se relit sans qu'un second magasin diverge,
    /// et la disposition reste unique.
    /// </param>
    public ProjectHost(Project project, Func<IRunJournalReader> journal, WorkflowLauncher launcher)
    {
        _catalog = new WorkflowCatalog(project);
        _journal = journal();
        _launcher = launcher;
    }

    /// <summary>
    /// Le dernier passage de chaque workflow connu du projet — sa trace la plus
    /// récente dans le journal, ou aucune s'il n'a jamais tourné.
    /// </summary>
    /// <summary>
    /// Lance le workflow <paramref name="workflowId"/> du projet : charge sa
    /// définition du catalogue et la confie au lanceur, qui provisionne, exécute
    /// et journalise en estampillant cette provenance. Le run se relit ensuite via
    /// <see cref="LastRunPerWorkflow"/> — c'est la boucle 3a↔3b.
    /// </summary>
    /// <remarks>
    /// Chemin heureux : la définition est supposée valide. Un workflow illisible au
    /// lancement (fermer <c>LoadResult</c> en union, le refuser proprement) relève
    /// de la marche engrenage de configuration, seule à confronter l'utilisateur au
    /// <c>ValidationReport</c>.
    /// </remarks>
    public Task<WorkflowRun> LaunchAsync(
        string workflowId,
        RunTrigger? trigger = null,
        IProgress<WorkflowEvent>? observer = null,
        CancellationToken cancellationToken = default)
    {
        var definition = _catalog.Load(workflowId).Definition!;
        return _launcher.LaunchAsync(definition, workflowId, trigger, observer, cancellationToken);
    }

    /// <summary>
    /// Les événements d'un run, ordonnés — l'alimentation <b>relecture</b> de la
    /// projection d'écran (l'autre étant le flux live de <see cref="LaunchAsync"/>).
    /// Passe par le host pour que la présentation ne connaisse ni SQLite ni le
    /// disque : elle plie ces événements dans la même <c>RunProjection</c>.
    /// </summary>
    public IReadOnlyList<JournalEntry> ReadEvents(string runId) => _journal.ReadEvents(runId);

    public IReadOnlyList<WorkflowLastRun> LastRunPerWorkflow()
    {
        // ListRuns rend les runs du plus récemment démarré au plus ancien : le
        // premier rattaché à un workflow est donc son dernier passage.
        var runs = _journal.ListRuns();
        return _catalog.List()
                       .Select(workflow => new WorkflowLastRun(
                           workflow,
                           runs.FirstOrDefault(run => run.WorkflowId == workflow.Id)))
                       .ToList();
    }

    public void Dispose()
    {
        if (_journal is IDisposable disposable)
            disposable.Dispose();
    }
}

/// <summary>
/// Un workflow du projet et son dernier passage — <c>null</c> tant qu'il n'a
/// jamais tourné. La façon dont ce résumé se traduit en « échoué hier à 18 h 04 »
/// appartient à la présentation, pas ici.
/// </summary>
public sealed record WorkflowLastRun(WorkflowEntry Workflow, RunSummary? LastRun);
