import { CursusError } from "./errors.ts";

/**
 * Une citation retrouvée dans un document : le passage **tel qu'il y figure**, et où il
 * commence.
 */
export interface Anchor {
  /** Ce qu'il faut envoyer à Linear — le texte du document, pas la frappe de l'appelant. */
  readonly quotedText: string;
  /** L'indice du premier caractère dans le contenu d'origine. */
  readonly start: number;
}

/** Le texte, blancs réduits, avec l'indice d'origine de chaque caractère conservé. */
interface Normalized {
  readonly text: string;
  readonly positions: readonly number[];
}

/**
 * Réduit toute séquence de blancs à une espace unique, en gardant trace de l'origine.
 *
 * <p>C'est ce qui permet de retrouver un passage recopié à la main : la recopie écrase
 * les retours à la ligne d'un Markdown, sans que l'intention change.</p>
 */
function normalize(text: string): Normalized {
  const characters: string[] = [];
  const positions: number[] = [];
  let previousWasBlank = true; // vrai au départ : un blanc initial ne produit rien

  for (let index = 0; index < text.length; index += 1) {
    const character = text.charAt(index);

    if (/\s/u.test(character)) {
      if (!previousWasBlank) {
        characters.push(" ");
        positions.push(index);
      }
      previousWasBlank = true;
      continue;
    }

    characters.push(character);
    positions.push(index);
    previousWasBlank = false;
  }

  // Une espace finale ne doit pas peser dans la comparaison ni dans le passage rendu.
  if (characters.at(-1) === " ") {
    characters.pop();
    positions.pop();
  }

  return { text: characters.join(""), positions };
}

/**
 * Retrouve une citation dans un document, et rend le passage à envoyer.
 *
 * <p>⚠️ **Cette fonction est la seule garde qui existe.** Mesuré : Linear accepte n'importe
 * quel `quotedText`, y compris absent du document, et répond `success: true`.</p>
 *
 * <p><b>Son motif a changé sans que son code change, et c'est la partie à ne pas mal
 * relire.</b> On a d'abord cru que « l'ancrage est une recherche de texte faite à
 * l'affichage » — c'était faux, et `D-045` l'a renversé par la mesure : l'ancre est une
 * marque `inlineComment` dans l'état Yjs du document, que seul le client écrit. Aucune
 * citation, juste ou fausse, ne produit d'ancrage par l'API.</p>
 *
 * <p>Ce que cette fonction garantit est donc plus modeste et plus important : que la citation
 * **désigne un seul passage**. Privée d'ancrage visuel, une remarque n'est plus située que
 * par son repère calculé et son texte cité ; une citation ambiguë ne se remarque plus à
 * l'œil, alors qu'un surlignage au mauvais endroit sautait aux yeux. La garde a gagné en
 * importance en perdant sa raison d'origine.</p>
 *
 * <p>Ce qui est rendu reste le passage **du document**, jamais la frappe de l'appelant : la
 * citation s'affiche telle qu'elle a été envoyée, donc une recopie approximative se lirait
 * comme une citation infidèle du document.</p>
 */
export function anchor(content: string, quote: string): Anchor {
  const cherché = normalize(quote);

  if (cherché.text.length === 0)
    throw new CursusError(
      "La citation est vide : elle correspondrait à n'importe quel endroit du document, donc à aucun.",
    );

  const document = normalize(content);
  const débuts: number[] = [];

  for (let from = 0; ; ) {
    const found = document.text.indexOf(cherché.text, from);
    if (found === -1) break;
    débuts.push(found);
    // Avancer d'un seul caractère : deux occurrences peuvent se chevaucher.
    from = found + 1;
  }

  if (débuts.length === 0)
    throw new CursusError(
      `Cette citation ne figure pas dans le document : « ${aperçu(quote)} ». ` +
        "Linear l'accepterait sans rien dire, et la remarque citerait un passage que personne " +
        "ne pourrait retrouver — relisez le passage avec « cursus linear doc show ».",
    );

  if (débuts.length > 1)
    throw new CursusError(
      `Cette citation figure ${débuts.length} fois dans le document : « ${aperçu(quote)} ». ` +
        "Allongez-la jusqu'à ce qu'elle ne désigne plus qu'un seul passage — sinon le repère " +
        "calculé sera celui de la première occurrence, qui n'est pas forcément la vôtre, et " +
        "rien à l'écran ne le signalera.",
    );

  const début = débuts[0] as number;
  const start = document.positions[début] as number;
  const dernier = document.positions[début + cherché.text.length - 1] as number;

  return { quotedText: content.slice(start, dernier + 1), start };
}

function aperçu(quote: string): string {
  const aplati = quote.replace(/\s+/gu, " ").trim();
  return aplati.length > 60 ? `${aplati.slice(0, 60)}…` : aplati;
}
