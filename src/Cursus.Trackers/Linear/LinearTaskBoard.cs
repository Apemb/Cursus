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
    /// Le tableau <b>entier</b>. La composition des requêtes et le suivi des curseurs
    /// appartiennent au <see cref="LinearBoardCollector"/> — ici on ne prête que le POST,
    /// qui est bien tout ce que cette classe sait faire.
    /// </summary>
    public Task<IReadOnlyList<TaskProject>> ListProjectsAsync(CancellationToken cancellationToken = default) =>
        new LinearBoardCollector(PostAsync).CollectAsync(cancellationToken);

    public async Task<TrackerWorkspace> DescribeWorkspaceAsync(CancellationToken cancellationToken = default) =>
        LinearBoardReader.ReadWorkspace(await PostAsync(WorkspaceQuery, cancellationToken).ConfigureAwait(false));

    /// <summary>
    /// Ce qui identifie l'espace du jeton. ⚠️ <c>organization</c> est au <b>singulier</b>
    /// dans le schéma de Linear — une clé est attachée à exactement un espace, il n'y a
    /// donc rien à choisir, seulement à constater.
    /// </summary>
    private const string WorkspaceQuery = """
        query {
          organization {
            id
            name
            urlKey
          }
        }
        """;

    private async Task<string> PostAsync(string query, CancellationToken cancellationToken)
    {
        var token = await _secrets.ReadAsync(_secretKey, cancellationToken).ConfigureAwait(false)
            ?? throw new TrackerNotConfiguredException();

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = JsonContent.Create(new { query }),
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

        return body;
    }
}
