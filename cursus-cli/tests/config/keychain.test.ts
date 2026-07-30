import { execFileSync } from "node:child_process";
import { tmpdir } from "node:os";
import { join } from "node:path";

import { afterAll, beforeAll, describe, expect, it } from "vitest";

import { deleteSecret, readSecret, writeSecret } from "../../src/config/keychain.ts";

// Un trousseau jetable plutôt que celui de l'utilisateur — même parti pris que les
// tests du KeychainSecretStore côté C#.
const keychain = join(tmpdir(), `cursus-cli-test-${process.pid}.keychain`);

beforeAll(() => {
  execFileSync("/usr/bin/security", ["create-keychain", "-p", "motdepasse", keychain]);
  execFileSync("/usr/bin/security", ["unlock-keychain", "-p", "motdepasse", keychain]);
});

afterAll(() => {
  execFileSync("/usr/bin/security", ["delete-keychain", keychain]);
});

describe("le trousseau", () => {
  it("étant donné un secret écrit, quand on le relit, alors on obtient la valeur déposée", () => {
    // arrange
    writeSecret("tracker:aller-retour", "lin_api_abc123", keychain);

    // act
    const relu = readSecret("tracker:aller-retour", keychain);

    // assert
    expect(relu).toBe("lin_api_abc123");
  });

  it("étant donné un secret contenant des accents, quand on le relit, alors il revient intact", () => {
    // arrange — la raison d'être de l'encodage : sans lui, la valeur reviendrait en
    // hexadécimal, silencieusement, et le jeton serait faux sans que rien ne le dise
    writeSecret("tracker:accents", "jeton-accentué-éàü", keychain);

    // act
    const relu = readSecret("tracker:accents", keychain);

    // assert
    expect(relu).toBe("jeton-accentué-éàü");
  });

  it("étant donné aucun secret sous cette clé, quand on le lit, alors l'absence se rend et ne lève pas", () => {
    // arrange — « aucun jeton configuré » est un cas nominal, pas un incident
    const clé = "tracker:jamais-écrit";

    // act
    const relu = readSecret(clé, keychain);

    // assert
    expect(relu).toBeUndefined();
  });

  it("étant donné un secret effacé, quand on le relit, alors il a disparu", () => {
    // arrange
    writeSecret("tracker:éphémère", "à-effacer", keychain);

    // act
    deleteSecret("tracker:éphémère", keychain);

    // assert
    expect(readSecret("tracker:éphémère", keychain)).toBeUndefined();
  });

  it("étant donné un secret déjà présent, quand on le réécrit, alors la nouvelle valeur remplace l'ancienne", () => {
    // arrange — reconfigurer son jeton est le cas d'usage le plus courant
    writeSecret("tracker:remplacé", "ancien", keychain);

    // act
    writeSecret("tracker:remplacé", "nouveau", keychain);

    // assert
    expect(readSecret("tracker:remplacé", keychain)).toBe("nouveau");
  });

  it("étant donné une valeur accentuée rangée en clair, quand security la relit, alors elle revient en hexadécimal — le piège que l'encodage évite", () => {
    // arrange — ce test caractérise `security`, pas notre code : il verrouille la raison
    // du base64, pour que personne ne le retire en le croyant superflu
    execFileSync("/usr/bin/security", [
      "add-generic-password", "-U", "-s", "cursus", "-a", "tracker:en-clair",
      "-w", "accentué", keychain,
    ]);

    // act
    const brut = execFileSync("/usr/bin/security", [
      "find-generic-password", "-s", "cursus", "-a", "tracker:en-clair", "-w", keychain,
    ]).toString().trim();

    // assert
    expect(brut).not.toBe("accentué");
    expect(brut).toMatch(/^[0-9a-f]+$/);
  });
});
