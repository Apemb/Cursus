using System.Text.Json;

using Cursus.Core.Tasks;

namespace Cursus.Trackers.Linear;

/// <summary>
/// Le verdict d'échec d'une réponse Linear : pur, testé, sœur de
/// <see cref="LinearBoardReader"/>. Ce qui décide vit ici ; le transport, lui, reste
/// mince et non testé.
/// </summary>
public static class LinearFailure
{
    /// <summary>
    /// L'exception à lever pour cette réponse, ou <c>null</c> si elle est bonne.
    /// </summary>
    public static Exception? From(int statusCode, string body)
    {
        if (statusCode == Unauthorized)
            return new TrackerRejectedException();

        var complaint = Complaint(body);

        // ⚠️ GraphQL répond 200 même quand rien n'a abouti (entité introuvable, champ
        // refusé) : le code HTTP seul ne conclut jamais au succès. C'est la présence
        // d'« errors » qui tranche.
        if (statusCode is >= 200 and < 300)
            return complaint is null ? null : new TrackerUnreachableException(complaint);

        return new TrackerUnreachableException(complaint ?? Excerpt(body));
    }

    private const int Unauthorized = 401;

    /// <summary>
    /// Ce que Linear reproche, ou <c>null</c> s'il ne reproche rien. ⚠️ Le champ
    /// <c>message</c> est laconique (« Query too complex ») là où
    /// <c>userPresentableMessage</c> porte le chiffre qui permet de recalibrer — d'où
    /// la préférence pour le second, et le repli sur le premier.
    /// </summary>
    private static string? Complaint(string body)
    {
        // Un corps peut n'être pas du JSON du tout : une passerelle en panne rend de
        // l'HTML. C'est précisément quand tout va mal que le diagnostic doit tenir —
        // planter ici remplacerait le message utile par une pile d'exception.
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(body);
        }
        catch (JsonException)
        {
            return null;
        }

        using (document)
        {
            if (!document.RootElement.TryGetProperty("errors", out var errors)
                || errors.ValueKind is not JsonValueKind.Array
                || errors.GetArrayLength() == 0)
                return null;

            var first = errors[0];
            if (first.TryGetProperty("extensions", out var extensions)
                && extensions.TryGetProperty("userPresentableMessage", out var presentable))
                return presentable.GetString();

            return first.TryGetProperty("message", out var message)
                ? message.GetString()
                : null;
        }
    }

    /// <summary>Un extrait du corps : assez long pour diagnostiquer, assez court pour un message.</summary>
    private static string Excerpt(string body) =>
        body.Length <= 400 ? body : body[..400] + "…";
}
