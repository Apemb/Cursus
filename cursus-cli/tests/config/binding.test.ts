import { mkdtempSync, mkdirSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";

import { describe, expect, it } from "vitest";

import { readBinding } from "../../src/config/binding.ts";

/** Un dépôt jetable portant un `.cursus/project.json` au contenu donné. */
function projectRootWith(projectJson: string): string {
  const root = mkdtempSync(join(tmpdir(), "cursus-cli-"));
  mkdirSync(join(root, ".cursus"));
  writeFileSync(join(root, ".cursus", "project.json"), projectJson);
  return root;
}

describe("readBinding", () => {
  it("étant donné un projet Cursus lié à Linear, quand on lit son binding, alors on obtient la clé de l'espace", () => {
    // arrange
    const root = projectRootWith(
      JSON.stringify({
        id: "19a58c5d",
        name: "Cursus",
        tracker: { kind: "linear", workspaceKey: "cursus-app" },
      }),
    );

    // act
    const binding = readBinding(root);

    // assert
    expect(binding.workspaceKey).toBe("cursus-app");
  });

  it("étant donné un projet lié à un tracker d'un autre genre, quand on lit son binding, alors le refus nomme le genre rencontré", () => {
    // arrange
    const root = projectRootWith(
      JSON.stringify({ name: "Cursus", tracker: { kind: "jira", site: "acme" } }),
    );

    // act
    const lecture = () => readBinding(root);

    // assert — dégrader en un binding vide ferait échouer la suite sans dire pourquoi
    expect(lecture).toThrowError(/jira/);
  });

  it("étant donné un dossier qui n'est pas un projet Cursus, quand on lit son binding, alors le refus nomme le fichier attendu", () => {
    // arrange
    const root = mkdtempSync(join(tmpdir(), "cursus-cli-"));

    // act
    const lecture = () => readBinding(root);

    // assert — le ENOENT brut de Node dit le chemin, pas ce qu'il fallait faire
    expect(lecture).toThrowError(/n'est pas un projet Cursus/);
  });
});
