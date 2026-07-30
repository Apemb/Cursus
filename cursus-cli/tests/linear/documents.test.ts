import { describe, expect, it } from "vitest";

import { LinearClient } from "../../src/linear/client.ts";
import { listDocuments, readDocument } from "../../src/linear/documents.ts";

/** Un client dont chaque appel rend la réponse préparée suivante. */
function clientRendant(...responses: unknown[]): LinearClient {
  let tour = 0;
  return new LinearClient("jeton", async () => {
    const response = responses[Math.min(tour, responses.length - 1)];
    tour += 1;
    return new Response(JSON.stringify({ data: response }), { status: 200 });
  });
}

describe("listDocuments", () => {
  it("étant donné des documents rattachés diversement, quand on les liste, alors chacun porte ce qui le situe", async () => {
    // arrange
    const client = clientRendant({
      documents: {
        pageInfo: { hasNextPage: false, endCursor: null },
        nodes: [
          {
            id: "d1", title: "Spec", documentContentId: "c1",
            project: { name: "Un agent pilote Cursus" }, issue: null,
          },
          {
            id: "d2", title: "Plan d'archi", documentContentId: "c2",
            project: null, issue: { identifier: "CUR-45" },
          },
        ],
      },
    });

    // act
    const documents = await listDocuments(client);

    // assert
    expect(documents[0]?.projectName).toBe("Un agent pilote Cursus");
    expect(documents[1]?.issueIdentifier).toBe("CUR-45");
  });

  it("étant donné un titre porteur d'une entité HTML, quand on le liste, alors le titre est traduit", async () => {
    // arrange — l'API échappe les noms (mesuré) ; l'écran afficherait « &amp; » sinon
    const client = clientRendant({
      documents: {
        pageInfo: { hasNextPage: false, endCursor: null },
        nodes: [
          { id: "d1", title: "visuel &amp; configuration", documentContentId: "c1", project: null, issue: null },
        ],
      },
    });

    // act
    const documents = await listDocuments(client);

    // assert
    expect(documents[0]?.title).toBe("visuel & configuration");
  });
});

describe("readDocument", () => {
  it("étant donné un document dont le corps contient une entité HTML, quand on le lit, alors le contenu n'est pas traduit", async () => {
    // arrange — le contenu est du Markdown écrit par un humain : « &amp; » peut y être
    // littéral. Le traduire ferait échouer toute citation comparée à ce texte.
    const client = clientRendant({
      document: {
        id: "d1", title: "Plan &amp; suite", documentContentId: "c1",
        content: "On écrit `&amp;` pour parler de l'entité elle-même.",
      },
    });

    // act
    const document = await readDocument(client, "d1");

    // assert
    expect(document.title).toBe("Plan & suite");
    expect(document.content).toBe("On écrit `&amp;` pour parler de l'entité elle-même.");
  });
});
