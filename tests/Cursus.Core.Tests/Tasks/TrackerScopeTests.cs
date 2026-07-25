using Cursus.Core.Tasks;

namespace Cursus.Core.Tests.Tasks;

/// <summary>
/// La portée d'une connexion sait <b>filtrer</b> ce que le tracker rend. La règle vit
/// ici plutôt que dans l'écran : elle servira à l'écran des tâches comme au choix
/// d'une tâche à lancer, et une règle recopiée à deux endroits diverge.
/// </summary>
public class TrackerScopeTests
{
    private static readonly IReadOnlyList<TaskProject> Board =
    [
        new TaskProject("id-robustesse", "Robustesse d'exécution", []),
        new TaskProject("id-e2e", "Tests E2E", []),
        new TaskProject("id-daemon", "Daemon et MCP", []),
    ];

    [Fact(DisplayName = "étant donné une portée « tout l'espace », quand on filtre le tableau, alors tous les projets passent")]
    public void The_whole_workspace_keeps_everything()
    {
        // arrange
        var scope = new TrackerScope.WholeWorkspace();

        // act
        var kept = scope.Filter(Board);

        // assert
        Assert.Equal(3, kept.Count);
    }

    [Fact(DisplayName = "étant donné une portée restreinte à deux projets, quand on filtre le tableau, alors seuls ces projets passent")]
    public void A_selection_keeps_only_what_it_names()
    {
        // arrange
        var scope = new TrackerScope.SelectedProjects(["id-e2e", "id-daemon"]);

        // act
        var kept = scope.Filter(Board);

        // assert
        Assert.Equal(["Tests E2E", "Daemon et MCP"], kept.Select(project => project.Name));
    }
}
