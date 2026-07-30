import { mkdirSync, mkdtempSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";

import { describe, expect, it } from "vitest";

import { openSession } from "../src/session.ts";

/** Un dépôt lié à `cursus-app`, et un registre qui le dessert. */
function environnement(): { projectRoot: string; configDirectory: string } {
  const projectRoot = mkdtempSync(join(tmpdir(), "cursus-cli-"));
  mkdirSync(join(projectRoot, ".cursus"));
  writeFileSync(
    join(projectRoot, ".cursus", "project.json"),
    JSON.stringify({ name: "Cursus", tracker: { kind: "linear", workspaceKey: "cursus-app" } }),
  );

  const configDirectory = mkdtempSync(join(tmpdir(), "cursus-config-"));
  writeFileSync(
    join(configDirectory, "trackers.json"),
    JSON.stringify({
      connections: [
        { id: "abc123", label: "Cursus", kind: "linear", workspace: { key: "cursus-app" } },
      ],
    }),
  );

  return { projectRoot, configDirectory };
}

describe("openSession", () => {
  it("étant donné une connexion dont le jeton est au trousseau, quand on ouvre la session, alors elle porte la connexion desservant l'espace du dépôt", () => {
    // arrange
    const { projectRoot, configDirectory } = environnement();

    // act
    const session = openSession({
      projectRoot,
      configDirectory,
      readSecret: () => "lin_api_abc",
    });

    // assert
    expect(session.connection.workspaceKey).toBe("cursus-app");
  });

  it("étant donné une connexion dont le jeton a disparu du trousseau, quand on ouvre la session, alors le refus distingue ce cas d'une absence de connexion", () => {
    // arrange — la connexion est déclarée, mais le secret n'est plus là : dire « aucune
    // connexion » enverrait chercher le problème au mauvais endroit
    const { projectRoot, configDirectory } = environnement();

    // act
    const ouverture = () =>
      openSession({ projectRoot, configDirectory, readSecret: () => undefined });

    // assert
    expect(ouverture).toThrowError(/trousseau/);
  });
});
