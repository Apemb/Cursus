import { mkdtempSync, readFileSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";

import { describe, expect, it } from "vitest";

import { connectionFor, forgetConnection, upsertConnection } from "../../src/config/trackers.ts";

const espace = { id: "ebb668c1", key: "cursus-app", name: "Cursus" };

function dossierVide(): string {
  return mkdtempSync(join(tmpdir(), "cursus-config-"));
}

describe("upsertConnection", () => {
  it("étant donné aucun registre, quand on inscrit une connexion, alors elle reçoit un identifiant utilisable comme clé de trousseau", () => {
    // arrange
    const directory = dossierVide();

    // act
    const connection = upsertConnection(directory, espace);

    // assert — la forme du GUID « n » côté C# : 32 hexadécimaux, sans tiret
    expect(connection.id).toMatch(/^[0-9a-f]{32}$/);
    expect(connection.secretKey).toBe(`tracker:${connection.id}`);
  });

  it("étant donné une connexion déjà inscrite pour cet espace, quand on la réinscrit, alors son identifiant est conservé", () => {
    // arrange — en changer ferait du jeton précédent un secret orphelin au trousseau,
    // et « se reconnecter » est précisément le cas courant
    const directory = dossierVide();
    const première = upsertConnection(directory, espace);

    // act
    const seconde = upsertConnection(directory, espace);

    // assert
    expect(seconde.id).toBe(première.id);
  });

  it("étant donné un registre portant un autre espace, quand on en inscrit un second, alors le premier survit", () => {
    // arrange
    const directory = dossierVide();
    upsertConnection(directory, { id: "autre", key: "autre-espace", name: "Autre" });

    // act
    upsertConnection(directory, espace);

    // assert
    expect(connectionFor(directory, "autre-espace").workspaceKey).toBe("autre-espace");
    expect(connectionFor(directory, "cursus-app").workspaceKey).toBe("cursus-app");
  });

  it("étant donné une connexion inscrite, quand on relit le fichier, alors il garde la forme que lit le registre C#", () => {
    // arrange — l'app Avalonia lit le même fichier : en changer la forme la priverait
    // de ses connexions sans rien dire
    const directory = dossierVide();

    // act
    upsertConnection(directory, espace);

    // assert
    const document = JSON.parse(readFileSync(join(directory, "trackers.json"), "utf8")) as {
      connections: { kind: string; workspace: { key: string; name: string; id: string } }[];
    };
    expect(document.connections[0]?.kind).toBe("linear");
    expect(document.connections[0]?.workspace.key).toBe("cursus-app");
  });

  it("étant donné un registre écrit par l'app, quand on y ajoute une connexion, alors les connexions d'un genre inconnu survivent", () => {
    // arrange — une version ultérieure peut y écrire des trackers que cette CLI ignore ;
    // les perdre en réécrivant le fichier serait une destruction silencieuse
    const directory = dossierVide();
    writeFileSync(
      join(directory, "trackers.json"),
      JSON.stringify({
        connections: [{ id: "jira-1", label: "Jira", kind: "jira", site: "acme" }],
      }),
    );

    // act
    upsertConnection(directory, espace);

    // assert
    const document = JSON.parse(readFileSync(join(directory, "trackers.json"), "utf8")) as {
      connections: { kind: string }[];
    };
    expect(document.connections.map((c) => c.kind).sort()).toEqual(["jira", "linear"]);
  });
});

describe("forgetConnection", () => {
  it("étant donné une connexion inscrite, quand on l'oublie, alors elle disparaît du registre", () => {
    // arrange
    const directory = dossierVide();
    upsertConnection(directory, espace);

    // act
    forgetConnection(directory, "cursus-app");

    // assert
    expect(() => connectionFor(directory, "cursus-app")).toThrowError(/cursus linear login/);
  });
});
