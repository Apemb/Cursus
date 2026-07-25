using System.Net.Http.Json;

using Cursus.Core.Secrets;
using Cursus.Core.Tasks;

namespace Cursus.Trackers.Linear;

/// <summary>
/// Le tableau Linear, en <b>lecture</b>. Adaptateur volontairement <b>mince</b> : il
/// compose une requête, la poste, et confie la traduction à
/// <see cref="LinearBoardReader"/> — qui, lui, est testé. Rien à décider ici, donc
/// rien à simuler : sa preuve est le réel (§7.12, même partage que <c>D-017</c>).
/// </summary>
public sealed class LinearTaskBoard : ITaskBoard
{
    private const string Endpoint = "https://api.linear.app/graphql";

    /// <summary>
    /// Ce qu'on demande au tableau. ⚠️ On ne demande <b>pas</b> <c>children</c>, bien que
    /// l'API le rende : <c>parent</c> suffit à reconstruire l'arbre (les deux disent la
    /// même arête, vue des deux bouts) et demander moins allège la réponse. Le
    /// <c>pageInfo</c>, lui, n'est pas décoratif — il porte l'aveu de troncature.
    ///
    /// <para>
    /// ⚠️ <b>Linear plafonne la complexité d'une requête à 10 000</b>, et elle se
    /// multiplie sur les <c>first:</c> imbriqués : <c>50 × 100</c> vaut 22 555 et se fait
    /// refuser en 400. Les bornes ci-dessous (25 × 50) tiennent avec de la marge. Les
    /// relever <b>sans recalibrer</b> casserait l'appel — et c'est précisément pourquoi
    /// la troncature se dit (<c>IsTruncated</c>) au lieu de se compenser par des bornes
    /// toujours plus hautes.
    /// </para>
    /// </summary>
    private const string Query = """
        query {
          projects(first: 25) {
            nodes {
              id
              name
              issues(first: 50) {
                pageInfo { hasNextPage }
                nodes {
                  identifier
                  title
                  state { name }
                  parent { identifier }
                }
              }
            }
          }
        }
        """;

    private readonly ISecretStore _secrets;
    private readonly string _secretKey;
    private readonly HttpClient _http;

    /// <param name="secretKey">
    /// La clé sous laquelle le trousseau garde le jeton de cette connexion — voir
    /// <see cref="SecretKeyOf"/>.
    /// </param>
    public LinearTaskBoard(ISecretStore secrets, string secretKey, HttpClient? http = null)
    {
        _secrets = secrets;
        _secretKey = secretKey;
        _http = http ?? new HttpClient();
    }

    /// <summary>
    /// La clé sous laquelle vit le jeton d'une connexion — même convention en lecture
    /// qu'en écriture.
    ///
    /// <para>
    /// ⚠️ Indexée par <b>connexion</b>, jamais par espace : une clé Linear couvre soit
    /// le compte, soit un projet, donc deux connexions peuvent viser le même espace.
    /// Sous une clé d'espace, la seconde écraserait le jeton de la première en silence.
    /// </para>
    /// </summary>
    public static string SecretKeyOf(string connectionId) => $"linear:{connectionId}";

    public async Task<IReadOnlyList<TaskProject>> ListProjectsAsync(CancellationToken cancellationToken = default)
    {
        var token = await _secrets.ReadAsync(_secretKey, cancellationToken).ConfigureAwait(false)
            ?? throw new TrackerNotConfiguredException();

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = JsonContent.Create(new { query = Query }),
        };

        // Le jeton se passe brut : c'est une clé personnelle, pas un jeton OAuth (qui
        // prendrait « Bearer »). Vérifié à la sonde — docs/reference/linear-api.md §1.
        request.Headers.TryAddWithoutValidation("Authorization", token);

        int statusCode;
        string body;
        try
        {
            using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            statusCode = (int)response.StatusCode;
            body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException failure)
        {
            // La panne réseau devient une exception de DOMAINE dès la frontière : la
            // surface n'a pas à connaître HttpRequestException pour réagir.
            throw new TrackerUnreachableException(failure.Message);
        }

        // Le verdict — « jeton refusé » ou « tableau injoignable » — se décide ailleurs,
        // là où il est testé. Ici on poste et on lève ce qu'on nous rend.
        if (LinearFailure.From(statusCode, body) is { } failed)
            throw failed;

        return LinearBoardReader.Read(body);
    }
}
