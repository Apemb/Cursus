using System.Diagnostics;
using System.Text;

namespace Cursus.Core.Secrets;

/// <summary>
/// Le trousseau du système, adossé à <c>/usr/bin/security</c> — cohérent avec la
/// convention du dépôt d'adosser les I/O aux binaires POSIX plutôt qu'à une liaison
/// native (§7.10.1).
///
/// <para>
/// Les entrées sont rangées sous le <b>service</b> <c>cursus</c>, la clé servant de
/// compte : le trousseau reste ainsi lisible dans « Trousseaux d'accès », et tout ce
/// que Cursus y range se retrouve d'une seule recherche.
/// </para>
///
/// <para>
/// ⚠️ <b>Aucun repli sur un fichier en clair</b> si le trousseau est indisponible :
/// un repli silencieux est exactement la façon dont les secrets finissent commités
/// (§7.10.1). L'échec doit rester franc.
/// </para>
/// </summary>
public sealed class KeychainSecretStore : ISecretStore
{
    private const string Service = "cursus";

    /// <summary>Code que <c>security</c> rend quand l'entrée cherchée n'existe pas.</summary>
    private const int ItemNotFound = 44;

    private readonly string? _keychain;

    /// <param name="keychain">
    /// Le trousseau visé ; à défaut, celui de connexion. Les tests s'en servent pour
    /// travailler sur un trousseau jetable plutôt que sur celui de l'utilisateur.
    /// </param>
    public KeychainSecretStore(string? keychain = null) => _keychain = keychain;

    public async Task<string?> ReadAsync(string key, CancellationToken cancellationToken = default)
    {
        var (exitCode, output) = await SecurityAsync(
            cancellationToken, "find-generic-password", "-s", Service, "-a", key, "-w").ConfigureAwait(false);

        // L'absence est un cas nominal (aucun token configuré) : elle se rend en null,
        // pas en exception. Tout autre code est un vrai incident et doit remonter.
        if (exitCode == ItemNotFound)
            return null;

        Ensure(exitCode, "lire");

        // security termine la valeur par un saut de ligne, qui n'appartient pas au secret.
        return Decode(output.TrimEnd('\n'));
    }

    public async Task WriteAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        // -U : mettre à jour l'entrée si elle existe. Sans lui, security refuse tout
        // doublon — et « reconfigurer son token » est le cas d'usage le plus courant.
        var (exitCode, _) = await SecurityAsync(
            cancellationToken, "add-generic-password", "-U", "-s", Service, "-a", key, "-w", Encode(value))
            .ConfigureAwait(false);

        Ensure(exitCode, "écrire");
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var (exitCode, _) = await SecurityAsync(
            cancellationToken, "delete-generic-password", "-s", Service, "-a", key).ConfigureAwait(false);

        // Effacer ce qui n'est pas là, c'est déjà le résultat voulu. L'effacement suit
        // aussi bien un retrait de connexion qu'un échec de configuration : lever ici
        // ferait échouer le rattrapage d'erreur lui-même.
        if (exitCode == ItemNotFound)
            return;

        Ensure(exitCode, "effacer");
    }

    /// <summary>
    /// ⚠️ <b>Le gotcha de <c>security</c></b>, et la raison de cet encodage : à la
    /// lecture, <c>find-generic-password -w</c> rend la valeur <b>en hexadécimal</b>
    /// dès qu'elle contient un octet hors ASCII imprimable — une tabulation, un saut
    /// de ligne, ou n'importe quel accent. Sans préfixe et sans signal : la valeur
    /// remonterait donc <b>silencieusement fausse</b>, ce qui est pire qu'une erreur.
    /// Et on ne peut pas le détecter à la relecture — un secret qui serait
    /// littéralement une chaîne hexadécimale (un hash, une clé) est indiscernable de
    /// la forme encodée.
    ///
    /// <para>
    /// D'où le choix de ne jamais laisser <c>security</c> arbitrer : on range du
    /// base64, toujours ASCII imprimable, donc toujours rendu tel quel. Contrepartie
    /// assumée : la valeur n'est plus lisible à l'œil dans « Trousseaux d'accès », et
    /// un secret déposé à la main hors de Cursus ne se relit pas.
    /// </para>
    /// </summary>
    private static string Encode(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    private static string Decode(string stored) => Encoding.UTF8.GetString(Convert.FromBase64String(stored));

    private static void Ensure(int exitCode, string verb)
    {
        if (exitCode != 0)
            throw new InvalidOperationException(
                $"Impossible d'{verb} le secret dans le trousseau : security a rendu {exitCode}.");
    }

    /// <summary>
    /// Lance <c>security</c> et rend son code de sortie et sa sortie standard. Le
    /// trousseau visé se passe en dernier argument — position que <c>security</c>
    /// impose pour toutes ses sous-commandes.
    /// </summary>
    private async Task<(int ExitCode, string Output)> SecurityAsync(
        CancellationToken cancellationToken, params string[] arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo("/usr/bin/security")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
        };

        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);

        if (_keychain is { } keychain)
            process.StartInfo.ArgumentList.Add(keychain);

        process.Start();

        // Lire avant d'attendre : un tube plein bloquerait le process avant sa sortie.
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        return (process.ExitCode, output);
    }
}
