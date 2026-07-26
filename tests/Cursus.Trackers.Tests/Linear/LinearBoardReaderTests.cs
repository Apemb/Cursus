using Cursus.Trackers.Linear;

namespace Cursus.Trackers.Tests.Linear;

/// <summary>
/// La seule part <b>testée</b> du client Linear : la traduction de ce que l'API rend
/// vers le modèle de domaine. Elle est pure, et c'est là que vit la logique — la
/// sonde (<c>docs/reference/linear-api.md</c>) a montré que <c>project.issues</c> rend
/// les issues <b>à plat</b>, parents et enfants confondus : l'arbre n'est pas donné,
/// il se reconstruit. Le transport HTTP, lui, reste un adaptateur mince non testé,
/// prouvé au réel — le simuler ne prouverait rien.
///
/// <para>
/// Les fragments JSON ci-dessous sont <b>copiés de la sonde réelle</b>, pas inventés :
/// un double qui diverge de l'API ne teste que lui-même.
/// </para>
/// </summary>
public class LinearBoardReaderTests
{
    [Fact(DisplayName = "étant donné une réponse d'organisation, quand on lit le workspace, alors on obtient son identité")]
    public void A_workspace_is_read_from_its_organization()
    {
        // arrange — corps réel : une clé Linear voit exactement une organisation, et
        // c'est elle qui identifie ce à quoi le jeton donne accès
        const string json = """
            {"data":{"organization":{"id":"ebb668c1-554c-4941-a124-3eaf885611b4",
              "name":"Cursus","urlKey":"cursus-app"}}}
            """;

        // act
        var workspace = LinearBoardReader.ReadWorkspace(json);

        // assert
        Assert.Equal("ebb668c1-554c-4941-a124-3eaf885611b4", workspace.Id);
        Assert.Equal("cursus-app", workspace.Key);
        Assert.Equal("Cursus", workspace.Name);
    }

    [Fact(DisplayName = "étant donné un projet, quand on lit la réponse, alors son identifiant est rendu")]
    public void A_project_carries_its_identifier()
    {
        // arrange — l'identifiant est ce qui permettra de désigner un projet dans une
        // portée de connexion ; son nom, lui, peut changer et se répéter
        const string json = """
            {"data":{"projects":{"nodes":[
              {"id":"a1b2c3d4-0000-4444-8888-abcdefabcdef","name":"Robustesse d'exécution","issues":{"nodes":[]}}
            ]}}}
            """;

        // act
        var projects = LinearBoardReader.Read(json);

        // assert
        Assert.Equal("a1b2c3d4-0000-4444-8888-abcdefabcdef", Assert.Single(projects).Id);
    }

    [Fact(DisplayName = "étant donné un projet sans issue, quand on lit la réponse, alors le projet est rendu, vide")]
    public void A_project_without_issues_is_still_a_project()
    {
        // arrange — un projet vide n'est pas une absence de projet : il doit apparaître
        const string json = """
            {"data":{"projects":{"nodes":[
              {"name":"Robustesse d'exécution","issues":{"nodes":[]}}
            ]}}}
            """;

        // act
        var projects = LinearBoardReader.Read(json);

        // assert
        var project = Assert.Single(projects);
        Assert.Equal("Robustesse d'exécution", project.Name);
        Assert.Empty(project.Tasks);
    }

    [Fact(DisplayName = "étant donné deux issues sans parent, quand on lit la réponse, alors les deux sont au premier rang")]
    public void Issues_without_a_parent_sit_at_the_first_rank()
    {
        // arrange
        const string json = """
            {"data":{"projects":{"nodes":[
              {"name":"Robustesse d'exécution","issues":{"nodes":[
                {"identifier":"CUR-39","title":"Le check des prérequis","state":{"name":"Backlog"},
                 "parent":null,"children":{"nodes":[]}},
                {"identifier":"CUR-38","title":"L'annulation tue tout l'arbre","state":{"name":"Todo"},
                 "parent":null,"children":{"nodes":[]}}
              ]}}
            ]}}}
            """;

        // act
        var projects = LinearBoardReader.Read(json);

        // assert
        var tasks = Assert.Single(projects).Tasks;
        Assert.Equal(["CUR-39", "CUR-38"], tasks.Select(task => task.Key));
        Assert.Equal("Le check des prérequis", tasks[0].Title);

        // chacune porte SA colonne — c'est ce que l'écran affiche sur le ticket
        Assert.Equal(["Backlog", "Todo"], tasks.Select(task => task.Column));
    }

    [Fact(DisplayName = "étant donné une issue enfant d'une autre, quand on lit la réponse, alors elle pend de sa mère et non du premier rang")]
    public void A_child_issue_hangs_from_its_parent_only()
    {
        // arrange — ⚠️ l'enfant précède sa mère dans la liste, comme le fait la vraie
        // API : un algorithme en un seul passage, qui supposerait la mère déjà vue,
        // perdrait l'enfant.
        const string json = """
            {"data":{"projects":{"nodes":[
              {"name":"Round-trip Linear","issues":{"nodes":[
                {"identifier":"CUR-12","title":"ReadTask contre l'API réelle","state":{"name":"Backlog"},
                 "parent":{"identifier":"CUR-6"},"children":{"nodes":[]}},
                {"identifier":"CUR-6","title":"Le client Linear réel","state":{"name":"Todo"},
                 "parent":null,"children":{"nodes":[{"identifier":"CUR-12"}]}}
              ]}}
            ]}}}
            """;

        // act
        var projects = LinearBoardReader.Read(json);

        // assert — la sous-tâche ne doit apparaître qu'à UN endroit : sous sa mère
        var tasks = Assert.Single(projects).Tasks;
        var parent = Assert.Single(tasks);
        Assert.Equal("CUR-6", parent.Key);
        Assert.Equal("CUR-12", Assert.Single(parent.Children).Key);
    }

    [Fact(DisplayName = "étant donné une sous-issue dont la mère est absente de la réponse, quand on lit, alors elle remonte au premier rang plutôt que d'être perdue")]
    public void An_orphan_child_surfaces_rather_than_vanishing()
    {
        // arrange — la mère « CUR-6 » n'est pas dans la réponse : page tronquée, ou
        // mère rattachée à un autre projet. Les deux arrivent pour de vrai.
        const string json = """
            {"data":{"projects":{"nodes":[
              {"name":"Round-trip Linear","issues":{"nodes":[
                {"identifier":"CUR-12","title":"ReadTask contre l'API réelle","state":{"name":"Backlog"},
                 "parent":{"identifier":"CUR-6"},"children":{"nodes":[]}}
              ]}}
            ]}}}
            """;

        // act
        var projects = LinearBoardReader.Read(json);

        // assert — suspendre à une mère absente reviendrait à ne l'afficher nulle part.
        // Une tâche montrée au mauvais rang se remarque ; une tâche disparue, non.
        var orphan = Assert.Single(Assert.Single(projects).Tasks);
        Assert.Equal("CUR-12", orphan.Key);
    }

    [Fact(DisplayName = "étant donné une issue étiquetée, quand on lit la réponse, alors elle porte ses étiquettes")]
    public void An_issue_carries_its_labels()
    {
        // arrange — la moitié de la maille (colonne, étiquettes) que le prédicat de
        // déclenchement observe. Linear rend les étiquettes en connexion, comme tout
        // le reste : « labels » enveloppe des « nodes ».
        const string json = """
            {"data":{"projects":{"nodes":[
              {"name":"Round-trip Linear","issues":{"nodes":[
                {"identifier":"CUR-12","title":"ReadTask contre l'API réelle","state":{"name":"Todo"},
                 "parent":null,"labels":{"nodes":[{"name":"Feature"},{"name":"cursus:ready"}]}}
              ]}}
            ]}}}
            """;

        // act
        var projects = LinearBoardReader.Read(json);

        // assert
        var task = Assert.Single(Assert.Single(projects).Tasks);
        Assert.Equal(["Feature", "cursus:ready"], task.Labels);
    }

    [Fact(DisplayName = "étant donné une issue dont la réponse ne dit rien des étiquettes, quand on la lit, alors elle n'en porte aucune plutôt que de faire échouer la lecture")]
    public void An_issue_without_the_labels_field_carries_none()
    {
        // arrange — le champ est absent, pas vide : c'est ce que rend toute requête qui
        // ne l'a pas demandé. La lecture ne doit pas dépendre d'une requête particulière,
        // sinon enrichir la sélection casserait la traduction au lieu de l'enrichir.
        const string json = """
            {"data":{"projects":{"nodes":[
              {"name":"Round-trip Linear","issues":{"nodes":[
                {"identifier":"CUR-12","title":"ReadTask","state":{"name":"Todo"},"parent":null}
              ]}}
            ]}}}
            """;

        // act
        var projects = LinearBoardReader.Read(json);

        // assert
        Assert.Empty(Assert.Single(Assert.Single(projects).Tasks).Labels);
    }

    [Fact(DisplayName = "étant donné une réponse dont les issues débordent d'une page, quand on la lit, alors le projet se dit tronqué")]
    public void A_project_whose_issues_overflow_says_so()
    {
        // arrange — la sonde a montré hasNextPage dès first:2 sur un projet de 4 issues.
        // Le cas est donc ordinaire, pas exotique.
        const string json = """
            {"data":{"projects":{"nodes":[
              {"name":"Round-trip Linear","issues":{
                "pageInfo":{"hasNextPage":true},
                "nodes":[
                  {"identifier":"CUR-12","title":"ReadTask","state":{"name":"Backlog"},
                   "parent":null,"children":{"nodes":[]}}
                ]}}
            ]}}}
            """;

        // act
        var projects = LinearBoardReader.Read(json);

        // assert — un écran qui affiche une page en la faisant passer pour la liste
        // entière ment sans le dire ; c'est au modèle de porter l'aveu.
        Assert.True(Assert.Single(projects).IsTruncated);
    }
}
