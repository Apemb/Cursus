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
