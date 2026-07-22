using Cursus.Core.Projects;

namespace Cursus.Core.Tests.Projects;

/// <summary>
/// Le registre machine : la liste des projets connus, au-dessus des projets
/// eux-mêmes. Adossé à des <c>.cursus/</c> réels créés en dossier temporaire,
/// et à un dossier de configuration temporaire distinct — le registre écrit sa
/// liste là, jamais dans un projet.
/// </summary>
public sealed class ProjectRegistryTests : IDisposable
{
    private readonly string _configDir = Directory.CreateTempSubdirectory("cursus-config-").FullName;
    private readonly string _root = Directory.CreateTempSubdirectory("cursus-registry-").FullName;

    [Fact(DisplayName = "étant donné un registre vide, quand on ajoute un projet Cursus valide, alors il figure dans la liste")]
    public void Adding_a_valid_project_lists_it()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");
        var registry = new ProjectRegistry(_configDir);

        // act
        registry.Add(_root);

        // assert
        Assert.Contains(registry.Projects, p => p.Id == project.Id);
    }

    [Fact(DisplayName = "étant donné un dossier sans .cursus/, quand on tente de l'ajouter, alors c'est refusé et la liste reste vide")]
    public void Adding_a_directory_without_a_project_is_refused()
    {
        // arrange — _root est un temporaire nu : aucun projet n'y a été créé
        var registry = new ProjectRegistry(_configDir);

        // act / assert
        Assert.Throws<ProjectNotFoundException>(() => registry.Add(_root));
        Assert.Empty(registry.Projects);
    }

    [Fact(DisplayName = "étant donné un projet déjà inscrit, quand on l'ajoute à nouveau sous une autre forme du même chemin, alors il n'est pas dupliqué")]
    public void Adding_the_same_project_twice_does_not_duplicate_it()
    {
        // arrange
        ProjectStore.Create(_root, "Démo");
        var registry = new ProjectRegistry(_configDir);
        registry.Add(_root);

        // act — même chemin, forme non normalisée (un segment « . » superflu)
        registry.Add(Path.Combine(_root, "."));

        // assert
        Assert.Single(registry.Projects);
    }

    [Fact(DisplayName = "étant donné un projet inscrit, quand on le retire, alors il quitte la liste et le dépôt sur disque est intact")]
    public void Removing_a_project_drops_it_without_touching_the_repository()
    {
        // arrange
        ProjectStore.Create(_root, "Démo");
        var registry = new ProjectRegistry(_configDir);
        registry.Add(_root);

        // act
        registry.Remove(_root);

        // assert — hors de la liste, mais le .cursus/ du projet reste intact
        Assert.Empty(registry.Projects);
        Assert.True(File.Exists(Path.Combine(_root, ".cursus", "project.json")));
    }

    public void Dispose()
    {
        Directory.Delete(_configDir, recursive: true);
        Directory.Delete(_root, recursive: true);
    }
}
