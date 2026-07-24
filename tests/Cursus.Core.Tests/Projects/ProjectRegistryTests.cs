using System.Text.Json;

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

    [Fact(DisplayName = "étant donné un projet inscrit, quand on le renomme, alors la liste reflète le nouveau nom et l'identifiant reste le même")]
    public void Renaming_a_registered_project_updates_its_name_in_the_list()
    {
        // arrange
        var created = ProjectStore.Create(_root, "Ancien nom");
        var registry = new ProjectRegistry(_configDir);
        registry.Add(_root);

        // act
        registry.Rename(_root, "Nouveau nom");

        // assert — l'instantané du registre suit le disque, sans changer l'identité
        var listed = Assert.Single(registry.Projects);
        Assert.Equal("Nouveau nom", listed.Name);
        Assert.Equal(created.Id, listed.Id);
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

    [Fact(DisplayName = "étant donné un registre où l'on a ajouté un projet, quand un nouveau registre s'ouvre sur le même dossier de config, alors il relit le même projet")]
    public void A_new_registry_reloads_the_projects_added_before()
    {
        // arrange
        var project = ProjectStore.Create(_root, "Démo");
        new ProjectRegistry(_configDir).Add(_root);

        // act — un registre neuf, même dossier de configuration
        var reloaded = new ProjectRegistry(_configDir);

        // assert
        Assert.Contains(reloaded.Projects, p => p.Id == project.Id);
    }

    [Fact(DisplayName = "étant donné aucun fichier de configuration, quand un registre s'ouvre, alors sa liste est vide")]
    public void A_registry_without_a_config_file_starts_empty()
    {
        // act — _configDir existe mais ne porte aucun projects.json
        var registry = new ProjectRegistry(_configDir);

        // assert
        Assert.Empty(registry.Projects);
    }

    [Fact(DisplayName = "étant donné un fichier de configuration listant un chemin qui ne résout plus, quand le registre s'ouvre, alors ce chemin est absent de la liste et le fichier n'est pas réécrit")]
    public void A_stored_path_that_no_longer_resolves_is_skipped_but_kept_on_disk()
    {
        // arrange — un projects.json écrit à la main : un projet réel et un
        // chemin fantôme (un dossier qui n'existe pas / n'existe plus)
        ProjectStore.Create(_root, "Démo");
        var ghost = Path.Combine(_configDir, "disparu");
        var configFile = Path.Combine(_configDir, "projects.json");
        File.WriteAllText(configFile, $$"""
            { "projects": [ {{JsonSerializer.Serialize(_root)}}, {{JsonSerializer.Serialize(ghost)}} ] }
            """);

        // act
        var registry = new ProjectRegistry(_configDir);

        // assert — seul le projet réel apparaît...
        Assert.Single(registry.Projects);
        // ...mais le fichier garde les deux entrées : une lecture ne perd rien
        Assert.Contains("disparu", File.ReadAllText(configFile));
    }

    [Fact(DisplayName = "étant donné XDG_CONFIG_HOME défini, quand on résout le dossier de configuration machine, alors il est sous ce dossier")]
    public void The_config_directory_honours_xdg_config_home()
    {
        // act
        var dir = ProjectRegistry.ResolveConfigDirectory("/tmp/xdg", "/home/moi");

        // assert
        Assert.Equal(Path.Combine("/tmp/xdg", "cursus"), dir);
    }

    [Fact(DisplayName = "étant donné XDG_CONFIG_HOME absent, quand on résout le dossier de configuration machine, alors il retombe sous ~/.config")]
    public void The_config_directory_falls_back_to_dot_config()
    {
        // act
        var dir = ProjectRegistry.ResolveConfigDirectory(null, "/home/moi");

        // assert — jamais ~/Library/Application Support : on vise .config explicitement
        Assert.Equal(Path.Combine("/home/moi", ".config", "cursus"), dir);
    }

    [Fact(DisplayName = "étant donné XDG_CONFIG_HOME vide, quand on résout le dossier de configuration machine, alors il retombe sous ~/.config comme le fait le shell")]
    public void An_empty_xdg_config_home_counts_as_unset()
    {
        // act
        var dir = ProjectRegistry.ResolveConfigDirectory("", "/home/moi");

        // assert — parité avec build/reset-data.sh (${XDG_CONFIG_HOME:-$HOME/.config})
        Assert.Equal(Path.Combine("/home/moi", ".config", "cursus"), dir);
    }

    public void Dispose()
    {
        Directory.Delete(_configDir, recursive: true);
        Directory.Delete(_root, recursive: true);
    }
}
