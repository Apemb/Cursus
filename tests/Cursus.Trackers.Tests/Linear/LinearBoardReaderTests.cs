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

    [Fact(DisplayName = "étant donné une page de projets nus, quand on la lit, alors chaque projet porte son identifiant et son nom")]
    public void A_page_of_bare_projects_yields_its_projects()
    {
        // arrange — la requête bon marché de CUR-45 : les projets SANS leurs issues.
        // C'est elle qui garde les projets vides visibles, que la requête sur les
        // issues racine ferait disparaître (voir linear-api.md §7bis).
        const string json = """
            {"data":{"projects":{
              "pageInfo":{"hasNextPage":false,"endCursor":"318513ed-fd96-4403-a21f-dac24e48d405"},
              "nodes":[
                {"id":"95b9f60f-b09b-477a-8837-7aac896a21e5","name":"Robustesse d'exécution"},
                {"id":"47a006a9-2743-475b-bc42-c28b420472f5","name":"Tests E2E de l'application"}
              ]}}}
            """;

        // act
        var page = LinearBoardReader.ReadProjects(json);

        // assert
        Assert.Equal(
            ["95b9f60f-b09b-477a-8837-7aac896a21e5", "47a006a9-2743-475b-bc42-c28b420472f5"],
            page.Items.Select(project => project.Id));
        Assert.Equal(
            ["Robustesse d'exécution", "Tests E2E de l'application"],
            page.Items.Select(project => project.Name));
    }

    [Fact(DisplayName = "étant donné une page qui annonce une suite, quand on la lit, alors elle porte le curseur où reprendre")]
    public void A_page_that_announces_more_carries_the_cursor_to_resume_from()
    {
        // arrange — ⚠️ « hasNextPage: true » a été observé sur la connexion « issues »,
        // pas sur « projects » : cet espace n'a que 6 projets, il n'en déborde jamais.
        // La forme de pageInfo, elle, est la MÊME sur toute connexion de cette API —
        // c'est ce que le type générique acte.
        const string json = """
            {"data":{"projects":{
              "pageInfo":{"hasNextPage":true,"endCursor":"318513ed-fd96-4403-a21f-dac24e48d405"},
              "nodes":[{"id":"95b9f60f-b09b-477a-8837-7aac896a21e5","name":"Robustesse d'exécution"}]}}}
            """;

        // act
        var page = LinearBoardReader.ReadProjects(json);

        // assert — sans ce curseur, la page suivante est inatteignable et le tableau
        // reste amputé sans le dire. C'est tout l'objet de CUR-45.
        Assert.Equal("318513ed-fd96-4403-a21f-dac24e48d405", page.NextCursor);
    }

    [Fact(DisplayName = "étant donné une dernière page, quand on la lit, alors elle ne porte aucun curseur bien que la réponse en contienne un")]
    public void A_last_page_carries_no_cursor_even_though_the_response_holds_one()
    {
        // arrange — ⚠️ LE PIÈGE, mesuré : Linear rend un « endCursor » plein alors même
        // que « hasNextPage » est faux. Ce fragment est celui de la sonde du 2026-07-26,
        // à un projet près. Lire le curseur sans regarder hasNextPage donnerait une
        // page suivante là où il n'y en a pas — donc une BOUCLE SANS FIN, qui
        // redemanderait éternellement la même dernière page.
        const string json = """
            {"data":{"projects":{
              "pageInfo":{"hasNextPage":false,"endCursor":"318513ed-fd96-4403-a21f-dac24e48d405"},
              "nodes":[{"id":"95b9f60f-b09b-477a-8837-7aac896a21e5","name":"Robustesse d'exécution"}]}}}
            """;

        // act
        var page = LinearBoardReader.ReadProjects(json);

        // assert — c'est « hasNextPage » qui décide, jamais la présence du curseur
        Assert.Null(page.NextCursor);
    }

    [Fact(DisplayName = "étant donné une page d'issues racine, quand on la lit, alors chaque issue porte le projet dont elle pend")]
    public void A_page_of_root_issues_says_which_project_each_hangs_from()
    {
        // arrange — la connexion « issues » prise à la RACINE, pas sous un projet :
        // un seul curseur pour tout le tableau, et chaque issue dit son projet. C'est
        // ce raccrochage qui remplace le groupement que l'API faisait pour nous.
        const string json = """
            {"data":{"issues":{
              "pageInfo":{"hasNextPage":true,"endCursor":"df571bc6-f6ba-4f1f-9849-4e4bba0f2499"},
              "nodes":[
                {"identifier":"CUR-45","title":"Voir tout le tableau, pas sa première page",
                 "state":{"name":"Todo"},"parent":null,"labels":{"nodes":[]},
                 "project":{"id":"ac8d4db8-01d3-4010-b5ac-e3f323434e33","name":"Round-trip Linear (jambe 2·2)"}},
                {"identifier":"CUR-43","title":"Renommer le projet ouvert rafraîchit son titre de surface",
                 "state":{"name":"Backlog"},"parent":null,"labels":{"nodes":[]},
                 "project":{"id":"2e4b71e6-a6ca-4be2-87c4-91dd66a69a99","name":"Finition de l'app"}}
              ]}}}
            """;

        // act
        var page = LinearBoardReader.ReadIssues(json);

        // assert — l'identifiant, pas le nom : un nom se renomme et se répète
        Assert.Equal(
            ["ac8d4db8-01d3-4010-b5ac-e3f323434e33", "2e4b71e6-a6ca-4be2-87c4-91dd66a69a99"],
            page.Items.Select(issue => issue.ProjectId));

        // et le reste de la carte se lit comme avant — la forme de l'issue n'a pas
        // changé en remontant à la racine, seul son contexte a changé
        Assert.Equal(["CUR-45", "CUR-43"], page.Items.Select(issue => issue.Key));
        Assert.Equal(["Todo", "Backlog"], page.Items.Select(issue => issue.Column));
        Assert.Equal("df571bc6-f6ba-4f1f-9849-4e4bba0f2499", page.NextCursor);
    }

    [Fact(DisplayName = "étant donné une issue qui n'appartient à aucun projet, quand on la lit, alors elle ne dit aucun projet plutôt que de faire échouer la lecture")]
    public void An_issue_belonging_to_no_project_says_so()
    {
        // arrange — Linear autorise une issue sans projet (« project: null »). Le cas
        // était invisible tant qu'on partait des projets ; il remonte dès qu'on part
        // des issues racine, et il ne doit pas casser la page entière pour autant.
        //
        // ⚠️ Ce test naît VERT : la tolérance a été écrite au pas précédent, réclamée
        // par un autre rouge (6 tests de `Read`, dont les fragments imbriqués n'ont pas
        // de champ « project » — vérifié en la retirant). Il est ici pour verrouiller
        // le comportement sous son PROPRE nom, pas pour le faire naître.
        const string json = """
            {"data":{"issues":{
              "pageInfo":{"hasNextPage":false,"endCursor":"df571bc6-f6ba-4f1f-9849-4e4bba0f2499"},
              "nodes":[
                {"identifier":"CUR-45","title":"Voir tout le tableau","state":{"name":"Todo"},
                 "parent":null,"labels":{"nodes":[]},"project":null}
              ]}}}
            """;

        // act
        var page = LinearBoardReader.ReadIssues(json);

        // assert
        Assert.Null(Assert.Single(page.Items).ProjectId);
    }

    [Fact(DisplayName = "étant donné des projets et des issues, quand on assemble, alors chaque issue se raccroche au projet qu'elle nomme")]
    public void Assembling_hangs_each_issue_under_the_project_it_names()
    {
        // arrange — le groupement que l'API faisait pour nous quand on partait des
        // projets ; il se fait désormais ici, sur l'identifiant que chaque issue porte.
        BareProject[] projects = [new("p-1", "Round-trip Linear"), new("p-2", "Robustesse")];
        FlatIssue[] issues =
        [
            new("CUR-45", "Voir tout le tableau", "Todo", null, [], "p-1"),
            new("CUR-38", "L'annulation tue tout l'arbre", "Backlog", null, [], "p-2"),
            new("CUR-44", "L'écran des tâches", "Done", null, [], "p-1"),
        ];

        // act
        var assembled = LinearBoardReader.Assemble(projects, issues);

        // assert — l'ordre des projets est celui de leur page ; celui des tâches, celui
        // de la leur
        Assert.Equal(["Round-trip Linear", "Robustesse"], assembled.Select(project => project.Name));
        Assert.Equal(["CUR-45", "CUR-44"], assembled[0].Tasks.Select(task => task.Key));
        Assert.Equal(["CUR-38"], assembled[1].Tasks.Select(task => task.Key));
    }

    [Fact(DisplayName = "étant donné une sous-issue listée avant sa mère, quand on assemble, alors elle pend d'elle et non du premier rang")]
    public void Assembling_hangs_a_child_under_its_parent_whatever_the_order()
    {
        // arrange — ⚠️ l'enfant PRÉCÈDE sa mère, comme le fait la vraie API. Un
        // algorithme en un seul passage, qui supposerait la mère déjà vue, la perdrait.
        BareProject[] projects = [new("p-1", "Round-trip Linear")];
        FlatIssue[] issues =
        [
            new("CUR-12", "ReadTask contre l'API réelle", "Backlog", "CUR-6", [], "p-1"),
            new("CUR-6", "Le client Linear réel", "Todo", null, [], "p-1"),
        ];

        // act
        var assembled = LinearBoardReader.Assemble(projects, issues);

        // assert — la sous-tâche ne doit apparaître qu'à UN endroit : sous sa mère
        var parent = Assert.Single(Assert.Single(assembled).Tasks);
        Assert.Equal("CUR-6", parent.Key);
        Assert.Equal("CUR-12", Assert.Single(parent.Children).Key);
    }

    // Les quatre tests qui suivent naissent VERTS, et c'est assumé : ils verrouillent
    // sous le nom de l'assemblage des garanties que la lecture imbriquée tenait déjà et
    // dont `Group` hérite. Ils n'ont pas à faire naître du code — ils ont à empêcher que
    // la disparition de `Read` (pas 5) emporte silencieusement ce qu'il garantissait.

    [Fact(DisplayName = "étant donné un projet sans aucune issue, quand on assemble, alors il figure quand même, vide")]
    public void Assembling_keeps_a_project_that_has_no_issue()
    {
        // arrange — LA raison pour laquelle la lecture demande les projets à part. Partir
        // des issues seules ferait disparaître ce projet, puisqu'aucune ne le nomme.
        BareProject[] projects = [new("p-1", "Round-trip Linear"), new("p-2", "Tests E2E")];
        FlatIssue[] issues = [new("CUR-45", "Voir tout le tableau", "Todo", null, [], "p-1")];

        // act
        var assembled = LinearBoardReader.Assemble(projects, issues);

        // assert — un projet vide n'est pas une absence de projet
        Assert.Equal(2, assembled.Count);
        Assert.Empty(assembled[1].Tasks);
        Assert.Equal("Tests E2E", assembled[1].Name);
    }

    [Fact(DisplayName = "étant donné une issue sans projet, quand on assemble, alors elle n'apparaît sous aucun projet")]
    public void Assembling_drops_an_issue_that_belongs_to_no_project()
    {
        // arrange — Linear autorise une issue hors projet. TaskProject étant LE
        // regroupement du modèle, une telle carte n'a aucun rang où aller.
        BareProject[] projects = [new("p-1", "Round-trip Linear")];
        FlatIssue[] issues =
        [
            new("CUR-45", "Voir tout le tableau", "Todo", null, [], "p-1"),
            new("CUR-99", "Une carte hors projet", "Todo", null, [], null),
        ];

        // act
        var assembled = LinearBoardReader.Assemble(projects, issues);

        // assert — la jeter n'est pas une régression : elle était déjà invisible quand
        // la lecture partait des projets. L'afficher serait une fonctionnalité neuve.
        Assert.Equal(["CUR-45"], Assert.Single(assembled).Tasks.Select(task => task.Key));
    }

    [Fact(DisplayName = "étant donné une issue dont le projet n'est pas dans la liste, quand on assemble, alors elle est écartée")]
    public void Assembling_drops_an_issue_whose_project_is_unknown()
    {
        // arrange — les deux listes sont lues par deux requêtes successives : une carte
        // créée entre les deux, dans un projet créé entre les deux, nommerait un projet
        // que la liste ne porte pas. Rare, mais réel.
        BareProject[] projects = [new("p-1", "Round-trip Linear")];
        FlatIssue[] issues = [new("CUR-99", "Créée entre deux requêtes", "Todo", null, [], "p-inconnu")];

        // act
        var assembled = LinearBoardReader.Assemble(projects, issues);

        // assert — ⚠️ TROU CONNU, acté ici plutôt que découvert plus tard : la carte est
        // perdue SANS BRUIT, ce qui contredit le principe tenu ailleurs (« une tâche
        // absente ne se remarque pas »). Inventer un projet de rattrapage serait modeler
        // en avance ; le comportement est donc figé et visible dans ce test.
        Assert.Empty(Assert.Single(assembled).Tasks);
    }

    [Fact(DisplayName = "étant donné une sous-issue dont la mère manque, quand on assemble, alors elle remonte au premier rang plutôt que d'être perdue")]
    public void Assembling_surfaces_an_orphan_rather_than_losing_it()
    {
        // arrange — la mère « CUR-6 » n'est pas là : rattachée à un autre projet, ou pas
        // encore lue. L'arbre se reconstruit projet par projet, donc le cas est ordinaire.
        BareProject[] projects = [new("p-1", "Round-trip Linear")];
        FlatIssue[] issues = [new("CUR-12", "ReadTask contre l'API réelle", "Backlog", "CUR-6", [], "p-1")];

        // act
        var assembled = LinearBoardReader.Assemble(projects, issues);

        // assert — suspendre à une mère absente reviendrait à ne l'afficher nulle part
        Assert.Equal("CUR-12", Assert.Single(Assert.Single(assembled).Tasks).Key);
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
