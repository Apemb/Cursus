namespace Cursus.Core.Tasks;

/// <summary>
/// L'espace de travail auquel un jeton donne accès — l'« organisation » chez Linear.
///
/// <para>
/// ⚠️ Il ne se <b>choisit</b> pas, il se <b>constate</b> : une clé Linear est attachée
/// à exactement un workspace (le schéma n'expose <c>organization</c> qu'au singulier),
/// et c'est donc la clé qui détermine le périmètre, jamais l'utilisateur.
/// </para>
/// </summary>
/// <param name="Key">
/// L'identifiant lisible de l'espace (l'<c>urlKey</c> chez Linear, p. ex.
/// « cursus-app ») — ce qui permet à l'utilisateur de reconnaître un jeton parmi
/// d'autres. L'<paramref name="Id"/>, lui, est ce qui désigne sans ambiguïté.
/// </param>
public sealed record TrackerWorkspace(string Id, string Key, string Name);
