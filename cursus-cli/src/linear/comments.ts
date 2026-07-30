import type { LinearClient } from "./client.ts";
import type { CommentTarget } from "./target.ts";

export interface CreateCommentInput {
  /** La carte qui reçoit la remarque — un projet ou une issue, jamais un document. */
  readonly target: CommentTarget;
  readonly body: string;
  /** Le passage cité — absent pour une réponse, qui hérite du fil. */
  readonly quotedText?: string;
  /** Le commentaire auquel on répond. */
  readonly parentId?: string;
}

/**
 * Le champ de `CommentCreateInput` qui désigne la cible.
 *
 * <p>C'est ici, et ici seulement, que l'étiquette du type redevient une forme de document
 * JSON — l'adaptateur traduit, le modèle n'a jamais eu à porter le discriminant.</p>
 */
function champDeCible(target: CommentTarget): Record<string, string> {
  return target.kind === "project" ? { projectId: target.id } : { issueId: target.id };
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
 * Pose un commentaire sur la carte que désigne la cible.
 *
 * ⚠️ La cible est exigée **même pour une réponse** : mesuré, un `parentId` seul se fait
 * refuser en `INVALID_INPUT` (« exactly one of … must be defined »). Mesuré aussi, et c'est
 * la bonne nouvelle : `parentId` accompagné de `projectId` ou d'`issueId` est accepté.
 */
export async function createComment(
  client: LinearClient,
  input: CreateCommentInput,
): Promise<CreatedComment> {
  const { commentCreate } = await client.query<{
    commentCreate: { success: boolean; comment: CreatedComment };
  }>(CreateMutation, {
    input: {
      ...champDeCible(input.target),
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
 * Les remarques posées sur une cible, réponses comprises.
 *
 * <p>⚠️ **La requête racine filtrée est obligatoire, ce n'est pas un choix de style.**
 * Mesuré : `project.comments` rend une liste **vide sans lever d'erreur** — un décompte
 * bâti dessus dirait « aucune remarque » sur un projet qui en porte dix.</p>
 *
 * <p>Le filtre a la même forme pour un projet et pour une issue, mesuré sur les deux : d'où
 * un seul chemin de lecture, là où l'on aurait pu croire devoir en écrire deux.</p>
 *
 * <p>Les réponses arrivent **à plat**, au même niveau que les remarques qu'elles soldent,
 * leur `parent` renseigné. C'est à l'appelant de ne compter que les racines.</p>
 */
export async function listTargetComments(
  client: LinearClient,
  target: CommentTarget,
): Promise<CommentView[]> {
  const { comments } = await client.query<{ comments: { nodes: readonly CommentNode[] } }>(
    `query($target: ID!) {
      comments(filter: { ${target.kind}: { id: { eq: $target } } }, first: 250) {
        nodes { id body quotedText resolvedAt parent { id } user { name } url }
      }
    }`,
    { target: target.id },
  );

  return comments.nodes.map((node) => ({
    id: node.id,
    body: node.body,
    quotedText: node.quotedText,
    resolved: node.resolvedAt !== null,
    parentId: node.parent?.id ?? null,
    author: node.user?.name ?? "",
    url: node.url,
  }));
}

/**
 * Les remarques encore ouvertes : les **racines** non soldées.
 *
 * <p>⚠️ **Une réponse n'est jamais une remarque ouverte, et le piège est mesuré** : la
 * réponse qui solde un fil a son propre `resolvedAt` **nul**. La compter ferait que la porte
 * du cycle de revue — *zéro remarque ouverte* — ne se fermerait jamais, chaque solde en
 * ajoutant une.</p>
 */
export function unresolvedRoots(comments: readonly CommentView[]): CommentView[] {
  return comments.filter((comment) => comment.parentId === null && !comment.resolved);
}

export interface SettleInput {
  readonly commentId: string;
  /** La cible du fil — la réponse doit la porter, `parentId` seul étant refusé. */
  readonly target: CommentTarget;
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
    target: input.target,
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
