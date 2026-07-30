import { CursusError } from "../errors.ts";

/**
 * Ce sur quoi une remarque de revue se pose : la **carte**, jamais le document.
 *
 * <p>`D-045` a établi qu'un commentaire de document ne peut pas être ancré par l'API — la
 * marque `inlineComment` vit dans l'état Yjs et seul le client l'écrit. Une remarque posée
 * sur un document est donc invisible. Les remarques visent la carte qui porte le document :
 * le **projet** pour une Discovery ou une Spec, l'**issue** pour un plan d'archi.</p>
 *
 * <p>La variante est portée par le **type**, pas par deux champs optionnels exclusifs : un
 * état « les deux renseignés » n'a aucun sens et ne doit pas être représentable
 * (`CLAUDE.md` §Conventions). L'étiquette `kind` est ici le mécanisme de sous-typage, à
 * défaut d'héritage — et elle ne vient pas du document JSON, où Linear expose au contraire
 * deux champs nuls dont {@link targetFrom} est l'adaptateur.</p>
 */
export type CommentTarget =
  | { readonly kind: "project"; readonly id: string; readonly label: string }
  | { readonly kind: "issue"; readonly id: string; readonly label: string };

/** Les deux champs que Linear expose sur un document, dont un seul est renseigné. */
export interface TargetFields {
  readonly project: { readonly id: string; readonly name: string } | null;
  readonly issue: { readonly id: string; readonly identifier: string } | null;
}

/**
 * La cible d'un document, ou `undefined` s'il n'est attaché à rien.
 *
 * <p>L'absence est un **optionnel**, pas une troisième variante : un document flottant
 * existe, et c'est à l'appelant de dire ce qu'il en fait — poser une remarque devient
 * impossible, mais lister le document reste légitime.</p>
 */
export function targetFrom(fields: TargetFields): CommentTarget | undefined {
  if (fields.project) return { kind: "project", id: fields.project.id, label: fields.project.name };
  if (fields.issue) return { kind: "issue", id: fields.issue.id, label: fields.issue.identifier };

  return undefined;
}

/**
 * La cible d'un document, ou un refus franc s'il flotte.
 *
 * <p>Le paramètre est décrit par sa forme plutôt que par `DocumentSummary` : ce dernier
 * importe déjà {@link CommentTarget}, et le nommer ici ne servirait qu'à fabriquer un cycle
 * entre les deux modules.</p>
 */
export function requireTarget(document: {
  readonly title: string;
  readonly target?: CommentTarget;
}): CommentTarget {
  if (document.target) return document.target;

  throw new CursusError(
    `Le document « ${document.title} » n'est attaché ni à un projet ni à une issue : il n'y a ` +
      "nulle part où poser la remarque. Depuis `D-045`, une remarque se pose sur la carte qui " +
      "porte le document, jamais sur le document lui-même. Attachez-le, puis reprenez.",
  );
}
