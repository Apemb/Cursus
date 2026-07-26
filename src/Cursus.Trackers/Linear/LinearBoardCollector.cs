using Cursus.Core.Tasks;

namespace Cursus.Trackers.Linear;

/// <summary>
/// Rapatrie le tableau <b>en entier</b> — deux requêtes, chacune paginée jusqu'à
/// épuisement, puis assemblées.
///
/// <para>
/// ⚠️ <b>Pourquoi ce n'est pas dans <see cref="LinearTaskBoard"/></b> : cet adaptateur est
/// mince à dessein, et non testé, parce qu'il ne décide rien. Une boucle de pagination,
/// elle, décide — et casse en <em>silence</em> quand elle se trompe : curseur non
/// transmis, dernière page perdue, arrêt jamais atteint. Elle vit donc ici, où son
/// transport est un <b>délégué</b> qu'un test peut fournir sans réseau.
/// </para>
/// </summary>
/// <param name="post">
/// Poste une requête et rend le corps de la réponse. C'est la seule chose que ce
/// collecteur ne sait pas faire, et la seule qui reste non testée.
/// </param>
public sealed class LinearBoardCollector(Func<string, CancellationToken, Task<string>> post)
{
    /// <summary>
    /// Le maximum que l'API accepte (au-delà : 400 « <c>first must not be greater than
    /// 250</c> »). Donc le moins de requêtes possible, donc la moindre latence.
    ///
    /// <para>
    /// Tient le budget de complexité <b>dans les deux hypothèses</b> : même en supposant
    /// le calcul multiplicatif, <c>250 × labels(10)</c> vaut 2 500 estimés contre un
    /// plafond de 10 000 — et la forme imbriquée qu'on remplace en consommait 8 280
    /// (<c>linear-api.md</c> §6a). C'est ce raisonnement du pire cas qui fonde la borne,
    /// pas le coût mesuré, dont la formule reste inexpliquée.
    /// </para>
    /// </summary>
    private const int PageSize = 250;

    public async Task<IReadOnlyList<TaskProject>> CollectAsync(CancellationToken cancellationToken = default)
    {
        var projects = await CollectPagesAsync(ProjectsQuery, LinearBoardReader.ReadProjects, cancellationToken)
            .ConfigureAwait(false);

        var issues = await CollectPagesAsync(IssuesQuery, LinearBoardReader.ReadIssues, cancellationToken)
            .ConfigureAwait(false);

        return LinearBoardReader.Assemble(projects, issues);
    }

    /// <summary>
    /// Suit le curseur d'une connexion jusqu'à épuisement. Générique parce que toutes les
    /// connexions de cette API ont la même forme — un seul foyer de boucle, donc un seul
    /// endroit où se tromper.
    ///
    /// <para>
    /// <b>Aucun plafond de pages</b>, décision du rôle produit : un tableau montré à
    /// moitié est faux quelle que soit la suite, tandis que la lenteur se mesure après.
    /// </para>
    /// </summary>
    private async Task<IReadOnlyList<T>> CollectPagesAsync<T>(
        Func<string?, string> queryFor,
        Func<string, LinearPage<T>> readPage,
        CancellationToken cancellationToken)
    {
        List<T> collected = [];
        string? cursor = null;

        do
        {
            var page = readPage(await post(queryFor(cursor), cancellationToken).ConfigureAwait(false));
            collected.AddRange(page.Items);

            // ⚠️ Garde de non-progression, et c'est la seule protection contre l'infini
            // puisqu'il n'y a pas de plafond de pages. Une API qui ignorerait « after »
            // renverrait éternellement la même page en annonçant chaque fois une suite ;
            // on s'arrête dès qu'elle nous renvoie là où on vient de demander.
            //
            // Le cas d'un curseur qui ALTERNE (A → B → A) n'est pas traité : il faudrait
            // mémoriser tout l'historique pour un scénario que rien n'atteste. Le fusible
            // des tests le rendrait visible s'il se produisait.
            if (page.NextCursor == cursor)
                break;

            cursor = page.NextCursor;
        }
        while (cursor is not null);

        return collected;
    }

    /// <summary>
    /// Les projets <b>sans leurs issues</b> : bon marché, et la seule requête qui garde
    /// visibles les projets vides.
    /// </summary>
    private static string ProjectsQuery(string? cursor) => $$"""
        query {
          projects(first: {{PageSize}}{{After(cursor)}}) {
            pageInfo { hasNextPage endCursor }
            nodes { id name }
          }
        }
        """;

    /// <summary>
    /// Les issues prises <b>à la racine</b> — un seul curseur pour tout le tableau, là où
    /// la forme imbriquée en avait un par projet. Chaque issue dit son projet, ce qui
    /// remplace le groupement que l'API faisait auparavant.
    ///
    /// <para>
    /// ⚠️ <c>labels(first: 10)</c> reste une troncature <b>silencieuse</b> : au-delà de
    /// dix étiquettes, un prédicat visant celle qui manque conclurait à tort que la carte
    /// ne la porte pas. Dette connue, indépendante de la pagination.
    /// </para>
    /// </summary>
    private static string IssuesQuery(string? cursor) => $$"""
        query {
          issues(first: {{PageSize}}{{After(cursor)}}) {
            pageInfo { hasNextPage endCursor }
            nodes {
              identifier
              title
              state { name }
              parent { identifier }
              labels(first: 10) { nodes { name } }
              project { id }
            }
          }
        }
        """;

    private static string After(string? cursor) => cursor is null ? "" : $""", after: "{cursor}" """;
}
