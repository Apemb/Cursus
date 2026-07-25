using Cursus.Core.Tasks;
using Cursus.Trackers.Linear;

namespace Cursus.Trackers.Tests.Linear;

/// <summary>
/// Le verdict d'échec : ce qui décide, à partir d'une réponse ratée, <b>quelle</b>
/// exception de domaine lever. La distinction n'est pas cosmétique — « ton jeton est
/// refusé » et « Linear ne répond pas » appellent deux gestes de réparation
/// différents, et confondre les deux envoie l'utilisateur vérifier son réseau pendant
/// que sa clé est révoquée.
///
/// <para>
/// Les corps ci-dessous sont <b>copiés de sondes réelles</b> (jeton invalide, requête
/// trop complexe) : un double qui diverge de l'API ne teste que lui-même.
/// </para>
/// </summary>
public class LinearFailureTests
{
    [Fact(DisplayName = "étant donné une réponse 401, quand on juge l'échec, alors le jeton est déclaré refusé")]
    public void An_unauthorized_response_blames_the_token()
    {
        // arrange — corps réel d'une sonde avec un jeton invalide
        const string body = """
            {"errors":[{"message":"Authentication required, not authenticated",
              "extensions":{"code":"AUTHENTICATION_ERROR","statusCode":401,
              "userPresentableMessage":"You need to authenticate to access this operation."}}]}
            """;

        // act
        var failure = LinearFailure.From(401, body);

        // assert
        Assert.IsType<TrackerRejectedException>(failure);
    }

    [Fact(DisplayName = "étant donné une réponse 400 portant un message présentable, quand on juge l'échec, alors c'est ce message qui est rapporté")]
    public void A_rejected_query_reports_what_Linear_says_about_it()
    {
        // arrange — corps réel d'une sonde à bornes trop larges (projects 50 × issues 100)
        const string body = """
            {"errors":[{"message":"Query too complex","extensions":{"type":"invalid input",
              "code":"INPUT_ERROR","statusCode":400,"userError":true,
              "userPresentableMessage":"The query is too complex. Complexity: 17055.000000000004. Maximum allowed complexity: 10000.",
              "http":{"status":400}}}]}
            """;

        // act
        var failure = LinearFailure.From(400, body);

        // assert — « message » seul dirait « Query too complex » sans le chiffre qui
        // permet de recalibrer : c'est le message présentable qui porte le diagnostic
        Assert.IsType<TrackerUnreachableException>(failure);
        Assert.Contains("Complexity: 17055", failure.Message);
    }

    [Fact(DisplayName = "étant donné une réponse 200 portant des erreurs GraphQL, quand on juge l'échec, alors le tableau est déclaré injoignable")]
    public void A_successful_status_does_not_mean_a_successful_query()
    {
        // arrange — corps réel : GraphQL répond 200 même quand rien n'a abouti
        const string body = """
            {"errors":[{"message":"Entity not found: Issue","path":["issue"],
              "extensions":{"type":"invalid input","code":"INPUT_ERROR","statusCode":400,
              "userError":true,"userPresentableMessage":"Could not find referenced Issue."}}],"data":null}
            """;

        // act
        var failure = LinearFailure.From(200, body);

        // assert — conclure au succès sur le seul code HTTP laisserait passer un échec
        Assert.IsType<TrackerUnreachableException>(failure);
        Assert.Contains("Could not find referenced Issue.", failure.Message);
    }

    [Fact(DisplayName = "étant donné une réponse 200 sans erreur, quand on juge l'échec, alors il n'y a rien à lever")]
    public void A_sound_response_is_not_a_failure()
    {
        // arrange
        const string body = """
            {"data":{"projects":{"nodes":[{"name":"Robustesse d'exécution","issues":{"nodes":[]}}]}}}
            """;

        // act
        var failure = LinearFailure.From(200, body);

        // assert
        Assert.Null(failure);
    }

    [Fact(DisplayName = "étant donné une réponse en panne dont le corps n'est pas du JSON, quand on juge l'échec, alors le corps est rapporté sans que le diagnostic plante")]
    public void A_body_that_is_not_json_is_still_reported()
    {
        // arrange — une passerelle en panne rend de l'HTML, pas du GraphQL
        const string body = "<html><body><h1>502 Bad Gateway</h1></body></html>";

        // act
        var failure = LinearFailure.From(502, body);

        // assert — c'est au moment où tout va mal que le diagnostic doit tenir debout
        Assert.IsType<TrackerUnreachableException>(failure);
        Assert.Contains("502 Bad Gateway", failure.Message);
    }
}
