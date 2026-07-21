using Cursus.Core.Projects;

namespace Cursus.Core.Tests.Projects;

/// <summary>
/// Le dépôt Cursus est lui-même un projet Cursus. Ces tests sont le garde-fou
/// des workflows commités : sans eux, un exemple pourrirait en silence au
/// premier durcissement du validateur, et le premier écran du jalon 6 ouvrirait
/// sur un projet cassé.
/// </summary>
public class CursusProjectTests
{
    [Fact(DisplayName = "étant donné le dépôt Cursus, quand on y découvre le projet depuis le répertoire d'exécution des tests, alors on obtient un projet dont la racine porte la solution")]
    public void This_repository_is_a_cursus_project()
    {
        // act — la remontée part de bin/Debug/net10.0, soit cinq niveaux sous la racine
        var project = ProjectStore.Discover(AppContext.BaseDirectory);

        // assert
        Assert.Equal("Cursus", project.Name);
        Assert.True(File.Exists(Path.Combine(project.Root, "Cursus.slnx")));
    }

    [Fact(DisplayName = "étant donné le projet de ce dépôt, quand on charge chacun de ses workflows commités, alors aucun ne rapporte de problème de validation")]
    public void The_committed_workflows_all_validate()
    {
        // arrange
        var catalog = new WorkflowCatalog(ProjectStore.Discover(AppContext.BaseDirectory));

        // act
        var loaded = catalog.List().Select(entry => (entry.Id, Result: catalog.Load(entry.Id))).ToList();

        // assert
        Assert.NotEmpty(loaded);
        Assert.All(loaded, item => Assert.True(
            item.Result.Report.IsValid,
            $"Le workflow « {item.Id} » ne valide plus : "
            + string.Join(" ; ", item.Result.Report.Issues.Select(issue => issue.Message))));
    }
}
