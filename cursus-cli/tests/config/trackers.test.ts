import { mkdtempSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";

import { describe, expect, it } from "vitest";

import { connectionFor, resolveConfigDirectory } from "../../src/config/trackers.ts";

/** Un dossier de configuration jetable portant le `trackers.json` donné. */
function configDirectoryWith(trackersJson: string): string {
  const directory = mkdtempSync(join(tmpdir(), "cursus-config-"));
  writeFileSync(join(directory, "trackers.json"), trackersJson);
  return directory;
}

const registreCursus = JSON.stringify({
  connections: [
    {
      id: "73ef7861621540fdbcfbb823c17a7abb",
      label: "Cursus",
      kind: "linear",
      workspace: { id: "ebb668c1", key: "cursus-app", name: "Cursus" },
    },
  ],
});

describe("resolveConfigDirectory", () => {
  it("étant donné XDG_CONFIG_HOME posé, quand on résout le dossier de configuration, alors il en dérive", () => {
    // arrange
    const xdg = "/ailleurs/config";

    // act
    const directory = resolveConfigDirectory(xdg, "/Users/qui");

    // assert
    expect(directory).toBe("/ailleurs/config/cursus");
  });

  it("étant donné XDG_CONFIG_HOME vide, quand on résout le dossier de configuration, alors il compte comme non défini", () => {
    // arrange — le shell traite `${XDG_CONFIG_HOME:-$HOME/.config}` ainsi ; s'en écarter
    // ferait viser deux dossiers à l'app et à la CLI
    const xdg = "";

    // act
    const directory = resolveConfigDirectory(xdg, "/Users/qui");

    // assert
    expect(directory).toBe("/Users/qui/.config/cursus");
  });
});

describe("connectionFor", () => {
  it("étant donné un registre desservant l'espace visé, quand on cherche sa connexion, alors on obtient la clé du trousseau", () => {
    // arrange
    const directory = configDirectoryWith(registreCursus);

    // act
    const connection = connectionFor(directory, "cursus-app");

    // assert
    expect(connection.secretKey).toBe("tracker:73ef7861621540fdbcfbb823c17a7abb");
  });

  it("étant donné un registre qui ne dessert pas l'espace visé, quand on cherche sa connexion, alors le refus dit comment en créer une", () => {
    // arrange
    const directory = configDirectoryWith(registreCursus);

    // act
    const recherche = () => connectionFor(directory, "un-autre-espace");

    // assert
    expect(recherche).toThrowError(/cursus linear login/);
  });

  it("étant donné aucun registre sur cette machine, quand on cherche une connexion, alors le refus dit comment en créer une", () => {
    // arrange
    const directory = mkdtempSync(join(tmpdir(), "cursus-config-"));

    // act
    const recherche = () => connectionFor(directory, "cursus-app");

    // assert
    expect(recherche).toThrowError(/cursus linear login/);
  });
});
