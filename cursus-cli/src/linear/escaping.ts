/**
 * Rend à un **nom** ses caractères, là où l'API GraphQL de Linear renvoie des entités
 * HTML.
 *
 * <p>Mesuré : le projet nommé `… visuel & configuration` revient `… visuel &amp;
 * configuration`. Ce n'est pas la donnée stockée qui porte l'entité — le MCP Linear rend
 * `&` sur le même espace — c'est **cette API** qui l'échappe. Le tiret cadratin, lui,
 * passe intact : seules les entités sont touchées.</p>
 *
 * <p>⚠️ **À n'appliquer qu'aux noms** — titres, libellés de projet, noms d'issue. Surtout
 * pas au **contenu** d'un document : un Markdown peut légitimement porter `&amp;`, et le
 * dé-échapper corromprait le texte que l'auteur a écrit. La citation qu'on ancre étant
 * comparée à ce contenu, une traduction de trop y ferait échouer toutes les
 * correspondances.</p>
 */
export function unescapeName(rendered: string): string {
  return rendered
    .replaceAll("&lt;", "<")
    .replaceAll("&gt;", ">")
    .replaceAll("&quot;", '"')
    .replaceAll("&#39;", "'")
    .replaceAll("&#x27;", "'")
    // `&amp;` en dernier : le traiter d'abord ferait d'un `&amp;lt;` littéral un `<`.
    .replaceAll("&amp;", "&");
}
