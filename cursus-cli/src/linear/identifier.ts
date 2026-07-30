import { CursusError } from "../errors.ts";

/**
 * Un document tel qu'on a besoin de le désigner. Les deux identifiants y figurent
 * ensemble parce qu'ils ne servent pas à la même chose : on **lit** par {@link id}, on
 * **écrit** un commentaire contre {@link documentContentId}.
 */
export interface DocumentSummary {
  readonly id: string;
  readonly title: string;
  readonly documentContentId: string;
  /** Renseigné pour un document de feature — la carte de projet le porte. */
  readonly projectName?: string;
  /** Renseigné pour un document d'incrément — l'issue le porte. */
  readonly issueIdentifier?: string;
}

/**
 * Le document que désigne une référence humaine : un identifiant d'issue (`CUR-45`), un
 * nom de projet, un titre, ou un fragment de titre.
 *
 * <p>Les candidats se cherchent du plus précis au plus lâche, et **le premier rang non
 * vide gagne**. Sans cette gradation, un titre court serait impossible à désigner dès
 * qu'un titre plus long le contient.</p>
 *
 * <p>⚠️ Une référence qui désigne plusieurs documents est **refusée**, jamais arbitrée :
 * une issue porte volontiers sa Discovery, sa Spec et son plan (« un artefact, un
 * document »), et en choisir un au hasard revient à commenter le mauvais une fois sur
 * deux — pour un contresens que l'appelant ne verrait pas.</p>
 */
export function resolveDocument(documents: readonly DocumentSummary[], reference: string): DocumentSummary {
  const cherché = reference.trim().toLocaleLowerCase();

  const rangs: readonly DocumentSummary[][] = [
    documents.filter((d) => d.title.toLocaleLowerCase() === cherché),
    documents.filter((d) => d.issueIdentifier?.toLocaleLowerCase() === cherché),
    documents.filter((d) => d.projectName?.toLocaleLowerCase() === cherché),
    documents.filter((d) => d.title.toLocaleLowerCase().includes(cherché)),
  ];

  const candidats = rangs.find((rang) => rang.length > 0) ?? [];

  if (candidats.length === 0)
    throw new CursusError(
      `Aucun document ne répond à « ${reference} ». Les documents de cet espace sont :\n` +
        documents.map((d) => `  - ${d.title}${situation(d)}`).join("\n"),
    );

  if (candidats.length > 1)
    throw new CursusError(
      `« ${reference} » désigne ${candidats.length} documents. Précisez par le titre :\n` +
        candidats.map((d) => `  - ${d.title}`).join("\n"),
    );

  return candidats[0] as DocumentSummary;
}

function situation(document: DocumentSummary): string {
  if (document.issueIdentifier) return ` (${document.issueIdentifier})`;
  if (document.projectName) return ` (projet « ${document.projectName} »)`;
  return "";
}
