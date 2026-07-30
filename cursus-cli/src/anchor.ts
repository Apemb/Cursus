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
 * <p>⚠️ **Cette fonction est la seule garde qui existe.** Mesuré le 2026-07-28 : Linear
 * accepte n'importe quel `quotedText`, y compris absent du document, et répond
 * `success: true`. Le type `Comment` ne porte aucun champ de position — l'ancrage est une
 * recherche de texte faite à l'affichage. Une citation fausse produit donc un commentaire
 * qui *paraît* situé sans l'être, ce qui est pire qu'un commentaire ouvertement flottant.</p>
 *
 * <p>Ce qui est rendu est le passage **du document**, jamais la frappe de l'appelant :
 * Linear compare au caractère près, donc renvoyer la recopie ferait une ancre qui ne
 * surligne rien.</p>
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
        "Linear l'accepterait sans rien dire et le commentaire ne surlignerait rien — " +
        "relisez le passage avec « cursus linear doc show ».",
    );

  if (débuts.length > 1)
    throw new CursusError(
      `Cette citation figure ${débuts.length} fois dans le document : « ${aperçu(quote)} ». ` +
        "Allongez-la jusqu'à ce qu'elle ne désigne plus qu'un seul passage — sinon le " +
        "commentaire se poserait sur la première occurrence, qui n'est pas forcément la vôtre.",
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
