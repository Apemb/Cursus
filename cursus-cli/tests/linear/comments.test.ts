import { describe, expect, it } from "vitest";

import { LinearClient } from "../../src/linear/client.ts";
import { createComment, listComments, settleComment } from "../../src/linear/comments.ts";

/** Un client qui note chaque requête et rend les réponses préparées, dans l'ordre. */
function clientNotant(...responses: unknown[]) {
  const requêtes: { query: string; variables: Record<string, unknown> }[] = [];
  const client = new LinearClient("jeton", async (_url, init) => {
    const corps = JSON.parse(String(init.body)) as {
      query: string;
      variables: Record<string, unknown>;
    };
    requêtes.push(corps);
    const response = responses[Math.min(requêtes.length - 1, responses.length - 1)];
    return new Response(JSON.stringify({ data: response }), { status: 200 });
  });
  return { client, requêtes };
}

describe("createComment", () => {
  it("étant donné une citation et un corps, quand on pose le commentaire, alors l'ancre et la citation partent ensemble", async () => {
    // arrange
    const { client, requêtes } = clientNotant({
      commentCreate: { success: true, comment: { id: "c1", url: "https://…" } },
    });

    // act
    await createComment(client, {
      documentContentId: "content-1",
      quotedText: "le passage exact",
      body: "la divergence",
    });

    // assert
    const variables = requêtes[0]?.variables["input"] as Record<string, unknown>;
    expect(variables["documentContentId"]).toBe("content-1");
    expect(variables["quotedText"]).toBe("le passage exact");
  });

  it("étant donné une réponse dans un fil, quand on la pose, alors elle porte son parent ET l'ancre", async () => {
    // arrange — mesuré : parentId seul est refusé, « exactly one of … must be defined »
    const { client, requêtes } = clientNotant({
      commentCreate: { success: true, comment: { id: "c2", url: "https://…" } },
    });

    // act
    await createComment(client, {
      documentContentId: "content-1",
      body: "reprise faite",
      parentId: "c1",
    });

    // assert
    const variables = requêtes[0]?.variables["input"] as Record<string, unknown>;
    expect(variables["parentId"]).toBe("c1");
    expect(variables["documentContentId"]).toBe("content-1");
  });
});

describe("listComments", () => {
  it("étant donné un document commenté, quand on liste, alors chaque commentaire dit s'il est soldé et s'il est une réponse", async () => {
    // arrange
    const { client } = clientNotant({
      document: {
        comments: {
          nodes: [
            {
              id: "c1", body: "une divergence", quotedText: "un passage",
              resolvedAt: null, parent: null, user: { name: "qui" }, url: "https://…",
            },
            {
              id: "c2", body: "reprise faite", quotedText: null,
              resolvedAt: "2026-07-28T00:00:00Z", parent: { id: "c1" },
              user: { name: "qui" }, url: "https://…",
            },
          ],
        },
      },
    });

    // act
    const commentaires = await listComments(client, "doc-1");

    // assert
    expect(commentaires[0]?.resolved).toBe(false);
    expect(commentaires[1]?.resolved).toBe(true);
    expect(commentaires[1]?.parentId).toBe("c1");
  });
});

describe("settleComment", () => {
  it("étant donné une raison écrite, quand on solde, alors la réponse est posée d'abord et la résolution la nomme", async () => {
    // arrange — mesuré : résoudre en nommant un commentaire *frère* rend un 500 nu. Le
    // commentaire qui solde doit être une réponse du fil, donc créée avant.
    const { client, requêtes } = clientNotant(
      { commentCreate: { success: true, comment: { id: "réponse-1", url: "https://…" } } },
      { commentResolve: { success: true, comment: { id: "c1", resolvedAt: "2026-07-28T00:00:00Z" } } },
    );

    // act
    await settleComment(client, {
      commentId: "c1",
      documentContentId: "content-1",
      reason: "Repris au §3.",
    });

    // assert
    expect(requêtes).toHaveLength(2);
    expect(requêtes[0]?.query).toContain("commentCreate");
    expect(requêtes[1]?.variables["resolvingCommentId"]).toBe("réponse-1");
  });
});
