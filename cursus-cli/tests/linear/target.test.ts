import { describe, expect, it } from "vitest";

import { requireTarget, targetFrom } from "../../src/linear/target.ts";

describe("targetFrom", () => {
  it("étant donné un document attaché à un projet, quand on cherche sa cible, alors c'est le projet, avec son identifiant", () => {
    // arrange — mesuré : une Discovery et une Spec sont attachées au projet, jamais à une issue
    const noeud = { project: { id: "p-1", name: "Un agent pilote Cursus" }, issue: null };

    // act
    const cible = targetFrom(noeud);

    // assert
    expect(cible).toEqual({ kind: "project", id: "p-1", label: "Un agent pilote Cursus" });
  });

  it("étant donné un document attaché à une issue, quand on cherche sa cible, alors c'est l'issue, désignée par son identifiant lisible", () => {
    // arrange — mesuré : les deux plans d'archi de l'espace sont portés par leur issue
    const noeud = { project: null, issue: { id: "i-1", identifier: "CUR-45" } };

    // act
    const cible = targetFrom(noeud);

    // assert
    expect(cible).toEqual({ kind: "issue", id: "i-1", label: "CUR-45" });
  });

  it("étant donné un document attaché à rien, quand on cherche sa cible, alors il n'y en a pas", () => {
    // arrange — un document flottant existe ; c'est l'appelant qui décide ce qu'il en fait
    const noeud = { project: null, issue: null };

    // act
    const cible = targetFrom(noeud);

    // assert
    expect(cible).toBeUndefined();
  });
});

describe("requireTarget", () => {
  it("étant donné un document attaché à rien, quand on exige sa cible, alors le refus dit quoi faire de ce document", () => {
    // arrange — un document flottant n'a aucun endroit où recevoir une remarque, et
    // l'appelant ne peut pas le deviner de lui-même
    const flottant = { title: "Note libre" };

    // act
    const exigence = () => requireTarget(flottant);

    // assert
    expect(exigence).toThrowError(/Note libre[\s\S]*attach/);
  });

  it("étant donné un document attaché, quand on exige sa cible, alors on l'obtient", () => {
    // arrange
    const porté = { title: "Spec", target: { kind: "project", id: "p-1", label: "Projet" } } as const;

    // act
    const cible = requireTarget(porté);

    // assert
    expect(cible.id).toBe("p-1");
  });
});
