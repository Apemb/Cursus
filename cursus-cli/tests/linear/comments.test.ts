import { describe, expect, it } from "vitest";

import { LinearClient } from "../../src/linear/client.ts";
import { createComment, listTargetComments, settleComment, unresolvedRoots } from "../../src/linear/comments.ts";
import type { CommentTarget } from "../../src/linear/target.ts";

const projet: CommentTarget = { kind: "project", id: "p-1", label: "Un agent pilote Cursus" };
const issue: CommentTarget = { kind: "issue", id: "i-45", label: "CUR-45" };

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
  it("étant donné une cible de projet, quand on pose la remarque, alors elle part contre le projet et non contre le document", async () => {
    // arrange — `D-045` : un commentaire de document ne peut pas être ancré, donc il est
    // invisible. La remarque va sur la carte qui porte le document.
    const { client, requêtes } = clientNotant({
      commentCreate: { success: true, comment: { id: "c1", url: "https://…" } },
    });

    // act
    await createComment(client, { target: projet, quotedText: "le passage exact", body: "la divergence" });

    // assert
    const variables = requêtes[0]?.variables["input"] as Record<string, unknown>;
    expect(variables["projectId"]).toBe("p-1");
    expect(variables["documentContentId"]).toBeUndefined();
    expect(variables["quotedText"]).toBe("le passage exact");
  });

  it("étant donné une cible d'issue, quand on pose la remarque, alors c'est l'issue qui la reçoit", async () => {
    // arrange — un plan d'archi est porté par son issue, mesuré sur les deux plans de l'espace
    const { client, requêtes } = clientNotant({
      commentCreate: { success: true, comment: { id: "c1", url: "https://…" } },
    });

    // act
    await createComment(client, { target: issue, body: "la divergence" });

    // assert
    const variables = requêtes[0]?.variables["input"] as Record<string, unknown>;
    expect(variables["issueId"]).toBe("i-45");
    expect(variables["projectId"]).toBeUndefined();
  });

  it("étant donné une réponse dans un fil, quand on la pose, alors elle porte son parent ET sa cible", async () => {
    // arrange — mesuré : parentId seul est refusé, « exactly one of … must be defined ».
    // Mesuré aussi : parentId et projectId ensemble sont acceptés.
    const { client, requêtes } = clientNotant({
      commentCreate: { success: true, comment: { id: "c2", url: "https://…" } },
    });

    // act
    await createComment(client, { target: projet, body: "reprise faite", parentId: "c1" });

    // assert
    const variables = requêtes[0]?.variables["input"] as Record<string, unknown>;
    expect(variables["parentId"]).toBe("c1");
    expect(variables["projectId"]).toBe("p-1");
  });
});

describe("listTargetComments", () => {
  it("étant donné une cible commentée, quand on liste, alors chaque remarque dit si elle est soldée et si elle est une réponse", async () => {
    // arrange — ⚠️ mesuré : `project.comments` rend une liste **vide sans erreur**. Seule
    // la requête racine filtrée voit les commentaires d'un projet, et elle a la même forme
    // pour une issue — un seul chemin de lecture suffit donc pour les deux cibles.
    const { client, requêtes } = clientNotant({
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
    });

    // act
    const commentaires = await listTargetComments(client, projet);

    // assert
    expect(requêtes[0]?.variables["target"]).toBe("p-1");
    expect(commentaires[0]?.resolved).toBe(false);
    expect(commentaires[1]?.resolved).toBe(true);
    expect(commentaires[1]?.parentId).toBe("c1");
  });

  it("étant donné une cible d'issue, quand on liste, alors le filtre porte sur l'issue", async () => {
    // arrange
    const { client, requêtes } = clientNotant({ comments: { nodes: [] } });

    // act
    await listTargetComments(client, issue);

    // assert
    expect(requêtes[0]?.query).toContain("issue:");
    expect(requêtes[0]?.variables["target"]).toBe("i-45");
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
    await settleComment(client, { commentId: "c1", target: projet, reason: "Repris au §3." });

    // assert
    expect(requêtes).toHaveLength(2);
    expect(requêtes[0]?.query).toContain("commentCreate");
    expect(requêtes[1]?.variables["resolvingCommentId"]).toBe("réponse-1");
  });
});

describe("unresolvedRoots", () => {
  it("étant donné un fil soldé, quand on compte les remarques ouvertes, alors la réponse qui l'a soldé n'en est pas une", () => {
    // arrange — ⚠️ mesuré : la réponse qui solde a `resolvedAt` **nul**. La compter ferait
    // que la porte du cycle — « zéro remarque ouverte » — ne fermerait jamais.
    const commentaires = [
      { id: "c1", body: "", quotedText: null, resolved: true, parentId: null, author: "", url: "" },
      { id: "réponse", body: "", quotedText: null, resolved: false, parentId: "c1", author: "", url: "" },
      { id: "c2", body: "", quotedText: null, resolved: false, parentId: null, author: "", url: "" },
    ];

    // act
    const ouvertes = unresolvedRoots(commentaires);

    // assert
    expect(ouvertes.map((c) => c.id)).toEqual(["c2"]);
  });
});
