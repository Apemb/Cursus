export interface ReviewBodyInput {
  /** Le titre du document visé — c'est lui qui départage la Discovery de la Spec. */
  readonly document: string;
  /** La section qui surplombe le passage, absente si le passage précède tout titre. */
  readonly heading: string | undefined;
  /** Ce que la revue a à dire. */
  readonly remark: string;
}

/**
 * Le corps d'une remarque de revue : son repère, puis la remarque.
 *
 * <p>⚠️ **Le repère est dans le corps, et c'est mesuré, pas préféré.** L'UI de Linear
 * **aplatit `quotedText` sur une ligne** — un repère glissé dans le passage cité se
 * collerait au texte cité, illisible. Le corps, lui, garde ses sauts de ligne.</p>
 *
 * <p>Il est **calculé** par l'appelant depuis l'offset du passage, jamais frappé : `D-045`
 * ayant privé les remarques d'ancrage visuel, le repère est tout ce qui situe une remarque
 * — un agent ne doit pouvoir ni l'omettre ni le falsifier.</p>
 */
export function reviewBody(input: ReviewBodyInput): string {
  const repère = input.heading === undefined ? input.document : `${input.document} › ${input.heading}`;

  return `*Ref : ${repère}*\n\n${input.remark}`;
}

/** Un titre ATX : un à six dièses, une espace, puis le texte. */
const Titre = /^#{1,6}\s+(.*)$/u;

/** L'ouverture ou la fermeture d'un bloc de code, en dièses ou en tildes. */
const Clôture = /^\s{0,3}(?:```|~~~)/u;

/**
 * Le titre de section qui surplombe un passage, sans ses dièses — ou `undefined` si le
 * passage précède le premier titre.
 *
 * <p>⚠️ **Les blocs de code sont exclus, et ce n'est pas du zèle.** Mesuré sur un document
 * de méthode : un `# dotnet build …` dans un bloc `bash` était retenu comme titre, et le
 * repère de la remarque citait une ligne de shell. Le repère étant calculé précisément pour
 * qu'un agent ne puisse pas le falsifier, un repère faux serait cru sans être vérifié.</p>
 */
export function headingAt(content: string, start: number): string | undefined {
  let dernier: string | undefined;
  let dansUnBloc = false;
  let offset = 0;

  for (const ligne of content.split("\n")) {
    if (offset > start) break;

    if (Clôture.test(ligne)) dansUnBloc = !dansUnBloc;
    else if (!dansUnBloc) {
      const titre = Titre.exec(ligne);
      if (titre) dernier = titre[1]?.trim();
    }

    offset += ligne.length + 1; // +1 pour le saut de ligne que split a mangé
  }

  return dernier;
}
