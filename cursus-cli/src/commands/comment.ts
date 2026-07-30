import { anchor } from "../anchor.ts";
import { CursusError } from "../errors.ts";
import {
  createComment,
  listTargetComments,
  settleComment,
  unresolvedRoots,
} from "../linear/comments.ts";
import { listDocuments, readDocument } from "../linear/documents.ts";
import { resolveDocument } from "../linear/identifier.ts";
import { requireTarget, targetFrom, type TargetFields } from "../linear/target.ts";
import { emit } from "../output.ts";
import { headingAt, reviewBody } from "../reference.ts";
import { openSession } from "../session.ts";

/**
 * Lit l'entrée standard quand la valeur est `-`.
 *
 * <p>Un corps de commentaire est du Markdown, souvent long et multi-ligne : le passer en
 * argument obligerait l'appelant à l'échapper pour le shell, ce qui est exactement là où
 * un agent se trompe.</p>
 */
async function valueOrStdin(value: string): Promise<string> {
  if (value !== "-") return value;

  const morceaux: Buffer[] = [];
  for await (const morceau of process.stdin) morceaux.push(morceau as Buffer);
  return Buffer.concat(morceaux).toString("utf8");
}

/**
 * Pose une remarque de revue sur la carte qui porte le document.
 *
 * <p>⚠️ **Elle ne se pose pas sur le document, et ce n'est pas un raccourci.** `D-045` a
 * établi qu'un commentaire de document ne peut pas être ancré par l'API : il est invisible.
 * La remarque va donc sur le projet ou l'issue, et ce qui la **situe** est un repère
 * calculé — titre du document, puis section — que l'appelant ne fournit pas et ne peut donc
 * ni oublier ni falsifier.</p>
 *
 * <p>La citation reste **vérifiée contre le contenu réel** avant tout appel, et sa garde a
 * gagné en importance : privée d'ancrage visuel, une citation ambiguë ne se remarque plus
 * à l'œil (voir `anchor`).</p>
 */
export async function commentAdd(
  reference: string,
  options: { quote: string; body: string },
): Promise<void> {
  const { client } = openSession();
  const summary = resolveDocument(await listDocuments(client), reference);
  const target = requireTarget(summary);
  const document = await readDocument(client, summary.id);

  const quote = await valueOrStdin(options.quote);
  const remark = await valueOrStdin(options.body);
  const ancre = anchor(document.content, quote);
  const section = headingAt(document.content, ancre.start);

  const comment = await createComment(client, {
    target,
    quotedText: ancre.quotedText,
    body: reviewBody({ document: document.title, heading: section, remark }),
  });

  // Le repère est rendu tel qu'il a été calculé : c'est la seule façon pour l'appelant de
  // constater où sa remarque a atterri, puisqu'il ne l'a pas écrit.
  emit({
    posted: comment.id,
    url: comment.url,
    [target.kind]: target.label,
    document: document.title,
    section: section ?? null,
    quotedText: ancre.quotedText,
  });
}

/**
 * Les remarques posées sur la carte qui porte un document, et lesquelles restent ouvertes.
 *
 * <p>La référence désigne un **document**, mais ce qui est listé est sa **carte** — et la
 * carte est partagée : une Discovery et une Spec vivent sur le même projet, donc les
 * remarques des deux apparaissent. C'est voulu, et c'est cohérent avec la porte du cycle de
 * revue, qui se ferme par carte et non par document : c'est le projet qu'on juge dégrossi.
 * Chaque remarque porte son repère `*Ref :*`, qui dit de quel document elle parle.</p>
 */
export async function commentList(
  reference: string,
  options: { unresolved?: boolean },
): Promise<void> {
  const { client } = openSession();
  const summary = resolveDocument(await listDocuments(client), reference);
  const target = requireTarget(summary);
  const commentaires = await listTargetComments(client, target);
  const ouvertes = unresolvedRoots(commentaires);

  emit({
    [target.kind]: target.label,
    open: ouvertes.length,
    total: commentaires.filter((commentaire) => commentaire.parentId === null).length,
    comments: options.unresolved ? ouvertes : commentaires,
  });
}

/**
 * Solde une divergence en écrivant sa raison.
 *
 * <p>La cible nécessaire à la réponse se retrouve depuis le commentaire lui-même :
 * demander à l'appelant de la fournir serait lui faire porter un identifiant qu'il n'a
 * aucune raison de connaître.</p>
 */
export async function commentResolve(commentId: string, options: { with: string }): Promise<void> {
  const { client } = openSession();
  const reason = (await valueOrStdin(options.with)).trim();

  if (reason.length === 0)
    throw new CursusError(
      "Une divergence ne se solde pas sans suite écrite : donnez la reprise faite, ou le refus et sa raison.",
    );

  const { comment } = await client.query<{ comment: TargetFields }>(
    "query($id: String!) { comment(id: $id) { project { id name } issue { id identifier } } }",
    { id: commentId },
  );

  const target = targetFrom(comment);

  if (!target)
    throw new CursusError(
      `Le commentaire ${commentId} n'est posé ni sur un projet ni sur une issue. S'il est posé ` +
        "sur un document, il est invisible dans l'interface (`D-045`) : reposez la remarque sur la " +
        "carte avec « cursus linear comment add », puis soldez celle-là.",
    );

  const settlement = await settleComment(client, { commentId, target, reason });

  emit({ resolved: settlement.commentId, by: settlement.resolvingCommentId, url: settlement.resolvingCommentUrl });
}
