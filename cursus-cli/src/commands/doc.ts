import { listDocuments, readDocument } from "../linear/documents.ts";
import { resolveDocument } from "../linear/identifier.ts";
import { emit } from "../output.ts";
import { openSession } from "../session.ts";

/** Les documents de l'espace, avec ce qui situe chacun. */
export async function docList(): Promise<void> {
  const { client } = openSession();
  const documents = await listDocuments(client);

  emit(
    documents.map((document) => ({
      title: document.title,
      id: document.id,
      documentContentId: document.documentContentId,
      ...(document.issueIdentifier ? { issue: document.issueIdentifier } : {}),
      ...(document.projectName ? { project: document.projectName } : {}),
    })),
  );
}

/**
 * Le contenu d'un document, désigné par une référence humaine.
 *
 * <p>C'est le préalable obligé de `comment add` : on ne cite pas un passage exact sans
 * avoir le texte sous les yeux, et Linear ne vérifiant pas les citations, une citation
 * approximative passerait sans rien signaler.</p>
 */
export async function docShow(reference: string): Promise<void> {
  const { client } = openSession();
  const summary = resolveDocument(await listDocuments(client), reference);
  const document = await readDocument(client, summary.id);

  emit({
    title: document.title,
    id: document.id,
    documentContentId: document.documentContentId,
    content: document.content,
  });
}
