using Cursus.Trackers.Linear;

namespace Cursus.Trackers.Tests.Linear;

/// <summary>
/// La <b>boucle</b> de pagination — et la raison pour laquelle elle ne vit pas dans
/// l'adaptateur HTTP. C'est de la logique, et de la pire espèce : elle casse en
/// <em>silence</em> (curseur non transmis, dernière page perdue, arrêt jamais atteint).
///
/// <para>
/// Elle se teste sans réseau parce que le collecteur reçoit son transport en délégué :
/// aucune interface neuve, aucun <c>HttpMessageHandler</c> simulé. Le faux transport
/// ci-dessous <b>retient les requêtes</b>, ce qui permet de vérifier non seulement le
/// résultat mais le <em>dialogue</em> — la seule façon de prouver qu'un curseur a
/// vraiment été transmis.
/// </para>
/// </summary>
public class LinearBoardCollectorTests
{
    [Fact(DisplayName = "étant donné une page de projets et une page d'issues, quand on collecte, alors le tableau est assemblé")]
    public async Task A_single_page_of_each_yields_the_board()
    {
        // arrange
        var transport = new FakeTransport(
            projectPages: [Projects(("p-1", "Round-trip Linear"))],
            issuePages: [Issues(("CUR-45", "p-1"))]);

        // act
        var board = await new LinearBoardCollector(transport.PostAsync).CollectAsync();

        // assert
        var project = Assert.Single(board);
        Assert.Equal("Round-trip Linear", project.Name);
        Assert.Equal(["CUR-45"], project.Tasks.Select(task => task.Key));
    }

    [Fact(DisplayName = "étant donné des issues sur deux pages, quand on collecte, alors la seconde est demandée au curseur de la première et les deux sont rendues")]
    public async Task Issues_spread_over_two_pages_are_all_collected()
    {
        // arrange — la première page annonce une suite et dit où reprendre
        var transport = new FakeTransport(
            projectPages: [Projects(("p-1", "Round-trip Linear"))],
            issuePages:
            [
                Issues("df571bc6-f6ba-4f1f-9849-4e4bba0f2499", ("CUR-45", "p-1")),
                Issues(("CUR-44", "p-1")),
            ]);

        // act
        var board = await new LinearBoardCollector(transport.PostAsync).CollectAsync();

        // assert — les deux pages figurent au tableau, dans l'ordre où elles sont venues
        Assert.Equal(["CUR-45", "CUR-44"], Assert.Single(board).Tasks.Select(task => task.Key));

        // et le curseur a VRAIMENT voyagé : sans cette vérification, une boucle qui
        // redemande éternellement la première page passerait le test ci-dessus dès lors
        // que le faux transport avance de lui-même.
        var issueQueries = transport.Queries.Where(query => !query.Contains("projects")).ToList();
        Assert.Equal(2, issueQueries.Count);
        Assert.DoesNotContain("after:", issueQueries[0]);
        Assert.Contains("""after: "df571bc6-f6ba-4f1f-9849-4e4bba0f2499" """.Trim(), issueQueries[1]);
    }

    [Fact(DisplayName = "étant donné une API qui rend toujours le même curseur, quand on collecte, alors la collecte s'arrête au lieu de tourner sans fin")]
    public async Task A_cursor_that_never_advances_stops_the_collection()
    {
        // arrange — une seule page préparée, que le transport rend indéfiniment : elle
        // annonce toujours une suite, et toujours au MÊME curseur. C'est le
        // comportement qu'aurait une API qui ignore silencieusement le paramètre
        // « after », et il suffirait à faire tourner la boucle pour toujours.
        var transport = new FakeTransport(
            projectPages: [Projects(("p-1", "Round-trip Linear"))],
            issuePages: [Issues("curseur-qui-n-avance-pas", ("CUR-45", "p-1"))]);

        // act — sans garde, cet appel ne rendrait jamais la main (le fusible du faux
        // transport le fait échouer plutôt que pendre)
        var board = await new LinearBoardCollector(transport.PostAsync).CollectAsync();

        // assert — on s'arrête dès que l'API nous renvoie là où on vient de demander
        var issueQueries = transport.Queries.Count(query => !query.Contains("projects"));
        Assert.Equal(2, issueQueries);

        // ⚠️ Conséquence assumée : la page est comptée deux fois, donc la carte apparaît
        // en double. On ne peut pas faire mieux sans comparer les contenus — et une API
        // qui mentirait ainsi vaut un affichage laid, pas une boucle sans fin.
        Assert.Equal(["CUR-45", "CUR-45"], Assert.Single(board).Tasks.Select(task => task.Key));
    }

    [Fact(DisplayName = "étant donné des projets sur deux pages, quand on collecte, alors les deux pages figurent au tableau")]
    public async Task Projects_spread_over_two_pages_are_all_collected()
    {
        // arrange — ⚠️ ce test naît VERT : la boucle est générique, les deux connexions
        // ayant la même forme. Il est là parce que la symétrie mérite d'être verrouillée :
        // rien n'empêcherait une refonte future de spécialiser une des deux et d'oublier
        // l'autre — et notre espace, avec ses 6 projets, ne le montrerait jamais au réel.
        var transport = new FakeTransport(
            projectPages:
            [
                Projects("318513ed-fd96-4403-a21f-dac24e48d405", ("p-1", "Round-trip Linear")),
                Projects(("p-2", "Tests E2E")),
            ],
            issuePages: [Issues(("CUR-45", "p-1"))]);

        // act
        var board = await new LinearBoardCollector(transport.PostAsync).CollectAsync();

        // assert
        Assert.Equal(["Round-trip Linear", "Tests E2E"], board.Select(project => project.Name));
        Assert.Contains("""after: "318513ed-fd96-4403-a21f-dac24e48d405" """.Trim(), transport.Queries[1]);
    }

    /// <summary>
    /// Un transport factice : rend les pages préparées dans l'ordre, en distinguant les
    /// deux connexions sur le contenu de la requête, et garde trace de tout ce qu'on lui
    /// a demandé.
    /// </summary>
    private sealed class FakeTransport(IReadOnlyList<string> projectPages, IReadOnlyList<string> issuePages)
    {
        private int _projectCalls;
        private int _issueCalls;

        public List<string> Queries { get; } = [];

        /// <summary>
        /// ⚠️ Fusible : sans lui, une boucle qui ne s'arrête pas ferait <b>pendre la
        /// suite</b> au lieu d'échouer. Un test qui ne finit jamais n'apprend rien ; une
        /// exception dit tout de suite laquelle des boucles s'est emballée.
        /// </summary>
        private const int Fuse = 20;

        public Task<string> PostAsync(string query, CancellationToken cancellationToken)
        {
            Queries.Add(query);

            if (Queries.Count > Fuse)
                throw new InvalidOperationException(
                    $"La collecte n'a pas convergé : {Fuse} requêtes postées. Dernière : {query}");

            // La requête des projets ne demande pas d'issues : c'est ce qui les
            // distingue, et c'est aussi tout l'intérêt de la découper en deux.
            var page = query.Contains("projects")
                ? projectPages[Math.Min(_projectCalls++, projectPages.Count - 1)]
                : issuePages[Math.Min(_issueCalls++, issuePages.Count - 1)];

            return Task.FromResult(page);
        }
    }

    private static string Projects(params (string Id, string Name)[] projects) => Projects(null, projects);

    /// <param name="nextCursor">
    /// Non nul pour annoncer une suite. ⚠️ <c>endCursor</c> est écrit <b>même quand
    /// <c>hasNextPage</c> est faux</b> : c'est ce que fait la vraie API, et c'est le
    /// piège que la boucle doit éviter.
    /// </param>
    private static string Projects(string? nextCursor, params (string Id, string Name)[] projects)
    {
        var nodes = string.Join(",", projects.Select(p => $$"""{"id":"{{p.Id}}","name":"{{p.Name}}"}"""));

        return Connection("projects", nextCursor, nodes);
    }

    private static string Issues(params (string Key, string ProjectId)[] issues) => Issues(null, issues);

    private static string Issues(string? nextCursor, params (string Key, string ProjectId)[] issues)
    {
        var nodes = string.Join(",", issues.Select(i => $$"""
            {"identifier":"{{i.Key}}","title":"{{i.Key}}","state":{"name":"Todo"},
             "parent":null,"labels":{"nodes":[]},"project":{"id":"{{i.ProjectId}}","name":"peu importe"} }
            """));

        return Connection("issues", nextCursor, nodes);
    }

    /// <summary>
    /// L'enveloppe commune aux deux connexions — elles ont la même forme, ce qui est la
    /// raison d'être de <c>LinearPage&lt;T&gt;</c>. Les accolades fermantes sont séparées
    /// à dessein : collées, elles entreraient en conflit avec les délimiteurs
    /// d'interpolation. JSON tolère les espaces, le compilateur non.
    /// </summary>
    private static string Connection(string name, string? nextCursor, string nodes) => $$"""
        {"data":{"{{name}}":{
          "pageInfo":{"hasNextPage":{{(nextCursor is null ? "false" : "true")}},"endCursor":"{{nextCursor ?? "fin"}}"},
          "nodes":[{{nodes}}]
          }
        }
        }
        """;
}
