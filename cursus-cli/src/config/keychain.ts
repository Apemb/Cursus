import { spawnSync } from "node:child_process";

import { CursusError } from "../errors.ts";

/**
 * Le service sous lequel Cursus range ses entrées. Fixé par `KeychainSecretStore` côté
 * C# : la CLI et l'app visent le **même** item, sinon un `login` fait ici resterait
 * invisible là-bas.
 */
const Service = "cursus";

/** Code que `security` rend quand l'entrée cherchée n'existe pas. */
const ItemNotFound = 44;

/**
 * ⚠️ **Le gotcha de `security`, et la raison de cet encodage** : à la lecture,
 * `find-generic-password -w` rend la valeur **en hexadécimal** dès qu'elle contient un
 * octet hors ASCII imprimable — un accent suffit. Sans préfixe et sans signal : la
 * valeur remonterait donc **silencieusement fausse**, ce qui est pire qu'une erreur, et
 * indétectable à la relecture (un secret qui serait lui-même une chaîne hexadécimale est
 * indiscernable de la forme encodée).
 *
 * <p>D'où le choix de ne jamais laisser `security` arbitrer : on range du base64,
 * toujours ASCII imprimable, donc toujours rendu tel quel. Contrepartie assumée : la
 * valeur n'est plus lisible à l'œil dans « Trousseaux d'accès », et un secret déposé à
 * la main hors de Cursus ne se relit pas.</p>
 */
function encode(value: string): string {
  return Buffer.from(value, "utf8").toString("base64");
}

function decode(stored: string): string {
  return Buffer.from(stored, "base64").toString("utf8");
}

/**
 * Lance `security`. Le trousseau visé se passe en **dernier** argument — position que
 * `security` impose à toutes ses sous-commandes.
 */
function security(argumentList: string[], keychain?: string): { code: number; output: string } {
  const result = spawnSync("/usr/bin/security", keychain ? [...argumentList, keychain] : argumentList, {
    encoding: "utf8",
  });

  if (result.error) throw result.error;
  return { code: result.status ?? 1, output: result.stdout };
}

function ensure(code: number, verb: string): void {
  if (code !== 0)
    throw new CursusError(`Impossible de ${verb} le secret dans le trousseau : security a rendu ${code}.`);
}

/** Le secret rangé sous cette clé, ou `undefined` s'il n'y en a pas. */
export function readSecret(key: string, keychain?: string): string | undefined {
  const { code, output } = security(
    ["find-generic-password", "-s", Service, "-a", key, "-w"],
    keychain,
  );

  // L'absence est un cas nominal (aucun jeton configuré) : elle se rend, elle ne lève pas.
  if (code === ItemNotFound) return undefined;

  ensure(code, "lire");

  // security termine la valeur par un saut de ligne, qui n'appartient pas au secret.
  return decode(output.trimEnd());
}

export function writeSecret(key: string, value: string, keychain?: string): void {
  // -U : mettre à jour l'entrée si elle existe. Sans lui, security refuse tout doublon —
  // et « reconfigurer son jeton » est le cas d'usage le plus courant.
  const { code } = security(
    ["add-generic-password", "-U", "-s", Service, "-a", key, "-w", encode(value)],
    keychain,
  );

  ensure(code, "écrire");
}

export function deleteSecret(key: string, keychain?: string): void {
  const { code } = security(["delete-generic-password", "-s", Service, "-a", key], keychain);

  // Effacer ce qui n'est pas là, c'est déjà le résultat voulu.
  if (code === ItemNotFound) return;

  ensure(code, "effacer");
}
