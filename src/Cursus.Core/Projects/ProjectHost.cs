using Cursus.Core.Workflows;
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

    /// <param name="journal">
    /// La fabrique du journal du projet. Le host l'appelle une fois et détient le
    /// résultat ; c'est <c>Cursus.Persistence</c> qui la lie au vrai
    /// <c>SqliteRunJournal</c>, pour que le câblage concret n'existe qu'en un lieu.
    /// </param>
    public ProjectHost(Project project, Func<IRunJournalReader> journal)
    {
        _catalog = new WorkflowCatalog(project);
        _journal = journal();
    }

    /// <summary>
    /// Le dernier passage de chaque workflow connu du projet — sa trace la plus
    /// récente dans le journal, ou aucune s'il n'a jamais tourné.
    /// </summary>
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
