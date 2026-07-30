import type { LinearClient } from "./client.ts";
import { unescapeName } from "./escaping.ts";
import type { DocumentSummary } from "./identifier.ts";

/** La borne d'une page, mesurée : 250 est le maximum, au-delà l'API rend 400. */
const PageSize = 250;

interface DocumentNode {
  readonly id: string;
  readonly title: string;
  readonly documentContentId: string;
  readonly project: { readonly name: string } | null;
  readonly issue: { readonly identifier: string } | null;
}

interface DocumentsResponse {
  readonly documents: {
    readonly pageInfo: { readonly hasNextPage: boolean; readonly endCursor: string | null };
    readonly nodes: readonly DocumentNode[];
  };
}

const DocumentsQuery = `
query($first: Int!, $after: String) {
  documents(first: $first, after: $after) {
    pageInfo { hasNextPage endCursor }
    nodes { id title documentContentId project { name } issue { identifier } }
  }
}`;

/**
 * Tous les documents de l'espace.
 *
 * <p>La sélection se fait ensuite **localement** (voir `resolveDocument`) plutôt que par
 * un filtre GraphQL : une référence humaine peut viser une issue, un projet ou un titre,
 * et trois filtres serveur là où un balayage local suffit multiplierait les requêtes sans
 * rien gagner — l'énumération est de toute façon nécessaire pour pouvoir *lister les
 * candidats* quand la référence est ambiguë.</p>
 */
export async function listDocuments(client: LinearClient): Promise<DocumentSummary[]> {
  const documents: DocumentSummary[] = [];
  let after: string | null = null;

  for (;;) {
    const page: DocumentsResponse = await client.query<DocumentsResponse>(DocumentsQuery, {
      first: PageSize,
      after,
    });

    for (const node of page.documents.nodes)
      documents.push({
        id: node.id,
        title: unescapeName(node.title),
        documentContentId: node.documentContentId,
        ...(node.project ? { projectName: unescapeName(node.project.name) } : {}),
        ...(node.issue ? { issueIdentifier: node.issue.identifier } : {}),
      });

    if (!page.documents.pageInfo.hasNextPage) return documents;
    after = page.documents.pageInfo.endCursor;
  }
}

export interface DocumentBody {
  readonly id: string;
  readonly title: string;
  readonly documentContentId: string;
  /** Le Markdown, **tel que Linear le rend** — non traduit, voir `unescapeName`. */
  readonly content: string;
}

interface DocumentResponse {
  readonly document: {
    readonly id: string;
    readonly title: string;
    readonly documentContentId: string;
    readonly content: string;
  };
}

/**
 * Le corps d'un document, et les deux identifiants qui vont avec.
 *
 * ⚠️ Le **contenu n'est pas dé-échappé**, à la différence du titre : c'est le texte
 * auquel une citation sera comparée, et une traduction de plus y ferait échouer les
 * correspondances sur tout passage contenant une esperluette.
 */
export async function readDocument(client: LinearClient, id: string): Promise<DocumentBody> {
  const { document } = await client.query<DocumentResponse>(
    "query($id: String!) { document(id: $id) { id title documentContentId content } }",
    { id },
  );

  return {
    id: document.id,
    title: unescapeName(document.title),
    documentContentId: document.documentContentId,
    content: document.content,
  };
}
