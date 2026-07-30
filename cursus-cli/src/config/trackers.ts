import { randomUUID } from "node:crypto";
import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";

import { CursusError } from "../errors.ts";

/** Le nom du fichier, fixé par `TrackerRegistry` côté C#. */
const FileName = "trackers.json";

/**
 * Une connexion Linear connue de cette installation. Le jeton n'est pas ici : il vit au
 * trousseau, désigné par {@link secretKey}.
 */
export interface LinearConnection {
  readonly id: string;
  readonly label: string;
  readonly workspaceKey: string;
  /** La clé du trousseau — sa forme est celle de `TrackerConnection.SecretKey`. */
  readonly secretKey: string;
}

/** L'espace que dessert une connexion, tel que le registre l'inscrit. */
export interface TrackerWorkspace {
  readonly id: string;
  readonly key: string;
  readonly name: string;
}

interface ConnectionDocument {
  readonly id?: string;
  readonly label?: string;
  readonly kind?: string;
  readonly workspace?: TrackerWorkspace;
  /** Ce qu'écrit une version qui connaît d'autres trackers — à préserver tel quel. */
  readonly [autre: string]: unknown;
}

interface RegistryDocument {
  readonly connections?: readonly ConnectionDocument[];
}

function readRegistry(configDirectory: string): ConnectionDocument[] {
  const path = join(configDirectory, FileName);
  if (!existsSync(path)) return [];
  return [...((JSON.parse(readFileSync(path, "utf8")) as RegistryDocument).connections ?? [])];
}

function writeRegistry(configDirectory: string, connections: readonly ConnectionDocument[]): void {
  mkdirSync(configDirectory, { recursive: true });
  writeFileSync(
    join(configDirectory, FileName),
    `${JSON.stringify({ connections }, null, 2)}\n`,
    "utf8",
  );
}

/**
 * Le dossier de configuration machine de Cursus : `$XDG_CONFIG_HOME/cursus` s'il est
 * posé, sinon `<home>/.config/cursus`.
 *
 * ⚠️ Une valeur **vide** compte comme non définie — c'est ce que fait le shell avec
 * `${XDG_CONFIG_HOME:-$HOME/.config}`, et c'est ce que fait `ProjectRegistry` côté C#.
 * S'en écarter ferait viser deux dossiers différents à l'app et à cette CLI, pour un
 * symptôme (« aucune connexion ») que rien n'expliquerait.
 */
export function resolveConfigDirectory(xdgConfigHome: string | undefined, home: string): string {
  const configHome = xdgConfigHome ? xdgConfigHome : join(home, ".config");
  return join(configHome, "cursus");
}

/**
 * La connexion qui dessert l'espace visé.
 *
 * Une connexion d'un genre inconnu est **ignorée** plutôt que refusée — ici, le choix
 * inverse de {@link readBinding} est le bon : le fichier peut légitimement décrire des
 * trackers qu'une version ultérieure sait joindre, et n'en retenir aucun ne casse rien
 * tant qu'un Linear s'y trouve.
 */
export function connectionFor(configDirectory: string, workspaceKey: string): LinearConnection {
  const found = readRegistry(configDirectory).find(
    (connection) => connection.kind === "linear" && connection.workspace?.key === workspaceKey,
  );

  if (!found?.id)
    throw new CursusError(
      `Aucune connexion ne dessert l'espace « ${workspaceKey} » sur cette machine. ` +
        "Créez-en une avec « cursus linear login ».",
    );

  return {
    id: found.id,
    label: found.label ?? "",
    workspaceKey,
    secretKey: `tracker:${found.id}`,
  };
}

/**
 * Inscrit la connexion desservant cet espace, ou rend celle qui existe déjà.
 *
 * <p>⚠️ **L'identifiant d'une connexion existante est conservé**, jamais réattribué : il
 * désigne le jeton au trousseau, et en changer laisserait le secret précédent orphelin
 * sous une clé que plus rien ne relit. Or « se reconnecter » est le cas courant.</p>
 *
 * <p>Les connexions d'un genre que cette CLI ne sait pas joindre sont **recopiées telles
 * quelles** : le fichier est partagé avec l'app, et le réécrire en les perdant serait une
 * destruction silencieuse.</p>
 */
export function upsertConnection(
  configDirectory: string,
  workspace: TrackerWorkspace,
): LinearConnection {
  const connections = readRegistry(configDirectory);
  const existante = connections.find(
    (connection) => connection.kind === "linear" && connection.workspace?.key === workspace.key,
  );

  const id = existante?.id ?? randomUUID().replaceAll("-", "");
  const document: ConnectionDocument = {
    id,
    label: workspace.name,
    kind: "linear",
    workspace,
  };

  const suivantes = existante
    ? connections.map((connection) => (connection === existante ? document : connection))
    : [...connections, document];

  writeRegistry(configDirectory, suivantes);

  return { id, label: workspace.name, workspaceKey: workspace.key, secretKey: `tracker:${id}` };
}

/**
 * Oublie la connexion desservant cet espace.
 *
 * ⚠️ Le jeton qu'elle désigne vit au trousseau : l'appelant doit l'effacer aussi, sans
 * quoi le secret reste orphelin.
 */
export function forgetConnection(configDirectory: string, workspaceKey: string): void {
  const connections = readRegistry(configDirectory);
  writeRegistry(
    configDirectory,
    connections.filter(
      (connection) => !(connection.kind === "linear" && connection.workspace?.key === workspaceKey),
    ),
  );
}
