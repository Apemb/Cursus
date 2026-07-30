import { CursusError } from "../errors.ts";
import type { CommentTarget } from "./target.ts";

/**
 * Un document tel qu'on a besoin de le désigner.
 *
 * <p>{@link id} sert à **lire** le document, {@link documentContentId} à écrire *dedans* —
 * plus à y poser un commentaire, depuis `D-045` : une remarque se pose sur
 * {@link target}.</p>
 */
export interface DocumentSummary {
  readonly id: string;
  readonly title: string;
  readonly documentContentId: string;
  /**
   * La carte qui porte ce document, et sur laquelle les remarques se posent. Absente
   * seulement si le document flotte, attaché à rien.
   */
  readonly target?: CommentTarget;
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
    documents.filter((d) => étiqueté(d, "issue") === cherché),
    documents.filter((d) => étiqueté(d, "project") === cherché),
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

/**
 * L'étiquette de la cible, si elle est du genre voulu — en minuscules, pour comparer.
 *
 * <p>Le genre est exigé plutôt que déduit de l'étiquette parce que les deux rangs de
 * résolution doivent rester distincts : une issue s'emporte sur un projet, et confondre les
 * deux rendrait cette priorité inobservable.</p>
 */
function étiqueté(document: DocumentSummary, kind: CommentTarget["kind"]): string | undefined {
  return document.target?.kind === kind ? document.target.label.toLocaleLowerCase() : undefined;
}

function situation(document: DocumentSummary): string {
  const cible = document.target;
  if (!cible) return "";

  return cible.kind === "issue" ? ` (${cible.label})` : ` (projet « ${cible.label} »)`;
}
