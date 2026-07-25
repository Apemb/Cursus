using System.Diagnostics;

using Cursus.Core.Secrets;

namespace Cursus.Core.Tests.Secrets;

/// <summary>
/// Tests d'I/O réels : ils parlent au vrai <c>/usr/bin/security</c>, comme la
/// convention du dépôt l'impose pour les I/O (adossées aux binaires POSIX du
/// système, cible macOS/Linux).
///
/// <para>
/// ⚠️ Ils travaillent sur un <b>trousseau dédié</b>, créé et détruit par le test —
/// jamais sur le trousseau de connexion. Un test qui laisse des entrées dans le
/// trousseau personnel de celui qui le lance est un test hostile ; et viser un
/// trousseau à part évite aussi toute boîte de dialogue d'autorisation.
/// </para>
/// </summary>
public sealed class KeychainSecretStoreTests : IDisposable
{
    private const string KeychainPassword = "cursus-test";

    private readonly string _keychain = Path.Combine(
        Path.GetTempPath(), $"cursus-test-{Guid.NewGuid():N}.keychain");

    public KeychainSecretStoreTests() => Security("create-keychain", "-p", KeychainPassword, _keychain);

    public void Dispose() => Security("delete-keychain", _keychain);

    [Fact(DisplayName = "étant donné un secret écrit sous une clé, quand on le relit, alors on retrouve sa valeur")]
    public async Task A_written_secret_reads_back()
    {
        // arrange
        var store = new KeychainSecretStore(_keychain);

        // act
        await store.WriteAsync("linear:acme", "lin_api_secret");
        var read = await store.ReadAsync("linear:acme");

        // assert
        Assert.Equal("lin_api_secret", read);
    }

    [Fact(DisplayName = "étant donné une clé jamais écrite, quand on la lit, alors rien n'est rendu")]
    public async Task An_absent_secret_reads_as_nothing()
    {
        // arrange
        var store = new KeychainSecretStore(_keychain);

        // act
        var read = await store.ReadAsync("linear:jamais-configure");

        // assert — l'absence est un état ordinaire de l'application (« aucun token
        // configuré »), pas un incident : elle se rend en null, sans lever.
        Assert.Null(read);
    }

    [Fact(DisplayName = "étant donné un secret déjà écrit, quand on en écrit un autre sous la même clé, alors le second gagne")]
    public async Task Writing_twice_under_one_key_keeps_the_second()
    {
        // arrange
        var store = new KeychainSecretStore(_keychain);
        await store.WriteAsync("linear:acme", "premier_token");

        // act — reconfigurer son token est le geste le plus courant, pas un cas limite
        await store.WriteAsync("linear:acme", "second_token");

        // assert
        Assert.Equal("second_token", await store.ReadAsync("linear:acme"));
    }

    [Fact(DisplayName = "étant donné un secret à caractères spéciaux, quand on l'écrit puis le relit, alors il revient intact")]
    public async Task A_secret_with_shell_metacharacters_survives_the_round_trip()
    {
        // arrange — deux familles de pièges d'un coup : ce que le shell mangerait
        // (espaces, guillemets, $, backticks) et ce qui fait basculer security en
        // hexadécimal à la relecture (tabulation, accents). Un token d'API n'a aucune
        // raison d'être un identifiant sage.
        var store = new KeychainSecretStore(_keychain);
        const string awkward = "lin_api $(whoami) `id` \"quoted\" 'single' && écho\tfin";

        // act
        await store.WriteAsync("linear:acme", awkward);

        // assert — l'argv est passé token par token, jamais réinterprété par un shell
        Assert.Equal(awkward, await store.ReadAsync("linear:acme"));
    }

    [Fact(DisplayName = "étant donné un secret rangé, quand on l'efface, alors le relire ne rend plus rien")]
    public async Task An_erased_secret_is_gone()
    {
        // arrange — retirer une connexion doit emporter son jeton, sans quoi le
        // trousseau accumule des secrets que plus rien ne désigne
        var store = new KeychainSecretStore(_keychain);
        await store.WriteAsync("linear:acme", "lin_api_secret");

        // act
        await store.DeleteAsync("linear:acme");

        // assert
        Assert.Null(await store.ReadAsync("linear:acme"));
    }

    [Fact(DisplayName = "étant donné une clé jamais écrite, quand on l'efface, alors rien n'est levé")]
    public async Task Erasing_what_was_never_there_is_not_a_failure()
    {
        // arrange
        var store = new KeychainSecretStore(_keychain);

        // act / assert — l'effacement suit un échec de configuration aussi bien qu'un
        // retrait : il doit être idempotent, sinon le rattrapage d'erreur lève à son tour
        await store.DeleteAsync("linear:jamais-configure");
    }

    /// <summary>
    /// Le montage et le démontage du trousseau de test — hors du sujet mesuré, donc
    /// hors du <see cref="KeychainSecretStore"/> : on ne se sert pas du sujet pour
    /// préparer son propre terrain.
    /// </summary>
    private static void Security(params string[] arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo("/usr/bin/security") { UseShellExecute = false },
        };

        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);

        process.Start();
        process.WaitForExit();
    }
}
