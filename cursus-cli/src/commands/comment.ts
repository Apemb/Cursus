import { anchor } from "../anchor.ts";
import { CursusError } from "../errors.ts";
import { createComment, listComments, settleComment } from "../linear/comments.ts";
import { listDocuments, readDocument } from "../linear/documents.ts";
import { resolveDocument } from "../linear/identifier.ts";
import { emit } from "../output.ts";
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
 * Pose un commentaire ancré sur un passage du document.
 *
 * <p>La citation est **vérifiée contre le contenu réel** avant tout appel : Linear
 * accepterait n'importe quoi sans le signaler (voir `anchor`).</p>
 */
export async function commentAdd(
  reference: string,
  options: { quote: string; body: string },
): Promise<void> {
  const { client } = openSession();
  const summary = resolveDocument(await listDocuments(client), reference);
  const document = await readDocument(client, summary.id);

  const quote = await valueOrStdin(options.quote);
  const body = await valueOrStdin(options.body);
  const ancre = anchor(document.content, quote);

  const comment = await createComment(client, {
    documentContentId: document.documentContentId,
    quotedText: ancre.quotedText,
    body,
  });

  emit({
    posted: comment.id,
    url: comment.url,
    document: document.title,
    quotedText: ancre.quotedText,
  });
}

/** Les commentaires d'un document, et lesquels restent ouverts. */
export async function commentList(
  reference: string,
  options: { unresolved?: boolean },
): Promise<void> {
  const { client } = openSession();
  const summary = resolveDocument(await listDocuments(client), reference);
  const commentaires = await listComments(client, summary.id);

  const retenus = options.unresolved
    ? commentaires.filter((commentaire) => !commentaire.resolved)
    : commentaires;

  emit({
    document: summary.title,
    open: commentaires.filter((commentaire) => !commentaire.resolved).length,
    total: commentaires.length,
    comments: retenus,
  });
}

/**
 * Solde une divergence en écrivant sa raison.
 *
 * <p>L'ancre nécessaire à la réponse se retrouve depuis le commentaire lui-même :
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

  const { comment } = await client.query<{
    comment: { documentContentId: string | null; resolvedAt: string | null };
  }>("query($id: String!) { comment(id: $id) { documentContentId resolvedAt } }", { id: commentId });

  if (!comment.documentContentId)
    throw new CursusError(
      `Le commentaire ${commentId} n'est pas posé sur un document — cette commande ne solde que ceux-là.`,
    );

  const settlement = await settleComment(client, {
    commentId,
    documentContentId: comment.documentContentId,
    reason,
  });

  emit({ resolved: settlement.commentId, by: settlement.resolvingCommentId, url: settlement.resolvingCommentUrl });
}
