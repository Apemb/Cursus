import { describe, expect, it } from "vitest";

import { resolveDocument, type DocumentSummary } from "../../src/linear/identifier.ts";

const documents: DocumentSummary[] = [
  {
    id: "doc-spec",
    title: "Spec — Un agent pilote Cursus",
    documentContentId: "content-spec",
    target: { kind: "project", id: "p-1", label: "Un agent pilote Cursus" },
  },
  {
    id: "doc-discovery",
    title: "Discovery — Un agent pilote Cursus",
    documentContentId: "content-discovery",
    target: { kind: "project", id: "p-1", label: "Un agent pilote Cursus" },
  },
  {
    id: "doc-plan-45",
    title: "Plan d'archi — Voir tout le tableau, pas sa première page",
    documentContentId: "content-plan-45",
    target: { kind: "issue", id: "i-45", label: "CUR-45" },
  },
  {
    id: "doc-plan-5",
    title: "Plan d'archi — Un workflow déclare les cartes qu'il prend",
    documentContentId: "content-plan-5",
    target: { kind: "issue", id: "i-5", label: "CUR-5" },
  },
];

describe("resolveDocument", () => {
  it("étant donné l'identifiant d'une issue portant un seul document, quand on le résout, alors on obtient ce document", () => {
    // arrange
    const référence = "CUR-45";

    // act
    const document = resolveDocument(documents, référence);

    // assert
    expect(document.id).toBe("doc-plan-45");
  });

  it("étant donné un identifiant d'issue en minuscules, quand on le résout, alors la casse ne fait pas obstacle", () => {
    // arrange — un agent qui compose une commande ne garantit pas la casse
    const référence = "cur-45";

    // act
    const document = resolveDocument(documents, référence);

    // assert
    expect(document.id).toBe("doc-plan-45");
  });

  it("étant donné un fragment de titre propre à un seul document, quand on le résout, alors on obtient ce document", () => {
    // arrange
    const référence = "Discovery";

    // act
    const document = resolveDocument(documents, référence);

    // assert
    expect(document.id).toBe("doc-discovery");
  });

  it("étant donné un nom de projet portant plusieurs documents, quand on le résout, alors le refus énumère les titres à départager", () => {
    // arrange — le projet porte sa Discovery et sa Spec : choisir pour l'appelant, c'est
    // commenter le mauvais artefact une fois sur deux
    const référence = "Un agent pilote Cursus";

    // act
    const résolution = () => resolveDocument(documents, référence);

    // assert
    expect(résolution).toThrowError(/Spec —[\s\S]*Discovery —|Discovery —[\s\S]*Spec —/);
  });

  it("étant donné une référence que rien ne porte, quand on la résout, alors le refus la cite", () => {
    // arrange
    const référence = "CUR-999";

    // act
    const résolution = () => resolveDocument(documents, référence);

    // assert
    expect(résolution).toThrowError(/CUR-999/);
  });

  it("étant donné un titre exact qui est aussi le fragment d'un autre, quand on le résout, alors l'exact l'emporte", () => {
    // arrange — sans cette priorité, un titre court deviendrait impossible à désigner
    const ambigus: DocumentSummary[] = [
      { id: "court", title: "Spec", documentContentId: "c1" },
      { id: "long", title: "Spec — Un agent pilote Cursus", documentContentId: "c2" },
    ];

    // act
    const document = resolveDocument(ambigus, "Spec");

    // assert
    expect(document.id).toBe("court");
  });
});
