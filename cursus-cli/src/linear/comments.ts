import type { LinearClient } from "./client.ts";

export interface CreateCommentInput {
  readonly documentContentId: string;
  readonly body: string;
  /** Le passage cité — absent pour une réponse, qui hérite du fil. */
  readonly quotedText?: string;
  /** Le commentaire auquel on répond. */
  readonly parentId?: string;
}

export interface CreatedComment {
  readonly id: string;
  readonly url: string;
}

const CreateMutation = `
mutation($input: CommentCreateInput!) {
  commentCreate(input: $input) { success comment { id url } }
}`;

/**
 * Pose un commentaire.
 *
 * ⚠️ L'ancre (`documentContentId`) est exigée **même pour une réponse** : mesuré, un
 * `parentId` seul se fait refuser en `INVALID_INPUT` (« exactly one of … must be
 * defined »).
 */
export async function createComment(
  client: LinearClient,
  input: CreateCommentInput,
): Promise<CreatedComment> {
  const { commentCreate } = await client.query<{
    commentCreate: { success: boolean; comment: CreatedComment };
  }>(CreateMutation, {
    input: {
      documentContentId: input.documentContentId,
      body: input.body,
      ...(input.quotedText === undefined ? {} : { quotedText: input.quotedText }),
      ...(input.parentId === undefined ? {} : { parentId: input.parentId }),
    },
  });

  return commentCreate.comment;
}

export interface CommentView {
  readonly id: string;
  readonly body: string;
  readonly quotedText: string | null;
  readonly resolved: boolean;
  readonly parentId: string | null;
  readonly author: string;
  readonly url: string;
}

interface CommentNode {
  readonly id: string;
  readonly body: string;
  readonly quotedText: string | null;
  readonly resolvedAt: string | null;
  readonly parent: { readonly id: string } | null;
  readonly user: { readonly name: string } | null;
  readonly url: string;
}

/**
 * Les commentaires d'un document.
 *
 * ⚠️ On lit par l'**id du document**, alors qu'on écrit contre son `documentContentId` —
 * et `documentContent` n'existe pas à la racine du schéma.
 */
export async function listComments(client: LinearClient, documentId: string): Promise<CommentView[]> {
  const { document } = await client.query<{ document: { comments: { nodes: readonly CommentNode[] } } }>(
    `query($id: String!) {
      document(id: $id) {
        comments { nodes { id body quotedText resolvedAt parent { id } user { name } url } }
      }
    }`,
    { id: documentId },
  );

  return document.comments.nodes.map((node) => ({
    id: node.id,
    body: node.body,
    quotedText: node.quotedText,
    resolved: node.resolvedAt !== null,
    parentId: node.parent?.id ?? null,
    author: node.user?.name ?? "",
    url: node.url,
  }));
}

export interface SettleInput {
  readonly commentId: string;
  readonly documentContentId: string;
  /** Ce qui solde la divergence : reprise faite, ou refus motivé. */
  readonly reason: string;
}

export interface Settlement {
  readonly commentId: string;
  readonly resolvingCommentId: string;
  readonly resolvingCommentUrl: string;
}

/**
 * Solde une divergence : écrit la raison dans le fil, puis résout en la nommant.
 *
 * <p>⚠️ **Les deux temps ne sont pas un confort, ils sont imposés.** Mesuré : un
 * `resolvingCommentId` qui désigne un commentaire *frère* fait rendre un
 * `INTERNAL_SERVER_ERROR` — un 500 nu, qui ressemble à une panne de Linear alors que
 * c'est une faute d'usage. Seule une **réponse du fil** est acceptée.</p>
 *
 * <p>Que la raison soit obligatoire tient la clause de `dod/feature/spec.md` §2 —
 * *« reprise, ou refusée avec sa raison écrite ; une divergence sans suite écrite n'est
 * pas soldée »*. Ici, la règle de méthode n'est pas rappelée, elle est **rendue
 * impossible à contourner** : on ne peut pas solder sans écrire.</p>
 */
export async function settleComment(client: LinearClient, input: SettleInput): Promise<Settlement> {
  const réponse = await createComment(client, {
    documentContentId: input.documentContentId,
    body: input.reason,
    parentId: input.commentId,
  });

  await client.query(
    `mutation($id: String!, $resolvingCommentId: String) {
      commentResolve(id: $id, resolvingCommentId: $resolvingCommentId) {
        success comment { id resolvedAt }
      }
    }`,
    { id: input.commentId, resolvingCommentId: réponse.id },
  );

  return {
    commentId: input.commentId,
    resolvingCommentId: réponse.id,
    resolvingCommentUrl: réponse.url,
  };
}
