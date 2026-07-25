using System.Net.Http.Json;
using System.Text.Json;

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
    private readonly string _workspace;
    private readonly HttpClient _http;

    /// <param name="workspace">
    /// L'espace Linear (son <c>urlKey</c>, p. ex. « cursus-app »). Sert à retrouver le
    /// jeton : la clé du trousseau est <c>linear:&lt;workspace&gt;</c>, jamais indexée
    /// par projet — le jeton appartient au compte (§7.10.1).
    /// </param>
    public LinearTaskBoard(ISecretStore secrets, string workspace, HttpClient? http = null)
    {
        _secrets = secrets;
        _workspace = workspace;
        _http = http ?? new HttpClient();
    }

    /// <summary>La clé sous laquelle vit le jeton d'un espace — même convention en lecture qu'en écriture.</summary>
    public static string SecretKeyOf(string workspace) => $"linear:{workspace}";

    public async Task<IReadOnlyList<TaskProject>> ListProjectsAsync(CancellationToken cancellationToken = default)
    {
        var token = await _secrets.ReadAsync(SecretKeyOf(_workspace), cancellationToken).ConfigureAwait(false)
            ?? throw new TrackerNotConfiguredException(_workspace);

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = JsonContent.Create(new { query = Query }),
        };

        // Le jeton se passe brut : c'est une clé personnelle, pas un jeton OAuth (qui
        // prendrait « Bearer »). Vérifié à la sonde — docs/reference/linear-api.md §1.
        request.Headers.TryAddWithoutValidation("Authorization", token);

        string body;
        try
        {
            using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            // Le corps accompagne le code : un « 400 » nu n'apprend rien à qui doit
            // corriger, alors que Linear dit précisément ce qu'il reproche.
            if (!response.IsSuccessStatusCode)
                throw new TrackerUnreachableException(
                    $"Linear a répondu {(int)response.StatusCode} — {Excerpt(body)}");
        }
        catch (HttpRequestException failure)
        {
            // La panne réseau devient une exception de DOMAINE dès la frontière : la
            // surface n'a pas à connaître HttpRequestException pour réagir.
            throw new TrackerUnreachableException(failure.Message);
        }

        // GraphQL répond 200 même en cas d'erreur applicative (jeton révoqué, champ
        // inconnu) : le code HTTP ne suffit pas à conclure au succès.
        EnsureNoGraphQlErrors(body);

        return LinearBoardReader.Read(body);
    }

    /// <summary>Un extrait du corps, assez long pour diagnostiquer, assez court pour un message.</summary>
    private static string Excerpt(string body) =>
        body.Length <= 400 ? body : body[..400] + "…";

    private static void EnsureNoGraphQlErrors(string body)
    {
        using var document = JsonDocument.Parse(body);
        if (!document.RootElement.TryGetProperty("errors", out var errors) || errors.GetArrayLength() == 0)
            return;

        var first = errors[0].TryGetProperty("message", out var message) ? message.GetString() : null;
        throw new TrackerUnreachableException(first ?? "erreur GraphQL sans message.");
    }
}
