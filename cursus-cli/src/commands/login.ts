import { homedir } from "node:os";
import { createInterface } from "node:readline";

import { readBinding } from "../config/binding.ts";
import { deleteSecret, writeSecret } from "../config/keychain.ts";
import {
  connectionFor,
  forgetConnection,
  resolveConfigDirectory,
  upsertConnection,
  type TrackerWorkspace,
} from "../config/trackers.ts";
import { CursusError } from "../errors.ts";
import { LinearClient } from "../linear/client.ts";
import { unescapeName } from "../linear/escaping.ts";
import { emit } from "../output.ts";

/**
 * Demande le jeton sans l'afficher.
 *
 * <p>Le seul endroit de cette CLI qui interroge un humain — partout ailleurs, un prompt
 * ferait pendre un agent indéfiniment. D'où la bascule : hors terminal, on lit l'entrée
 * standard et on ne demande rien.</p>
 */
async function askToken(): Promise<string> {
  if (!process.stdin.isTTY) {
    const morceaux: Buffer[] = [];
    for await (const morceau of process.stdin) morceaux.push(morceau as Buffer);
    return Buffer.concat(morceaux).toString("utf8").trim();
  }

  const rl = createInterface({ input: process.stdin, output: process.stdout, terminal: true });

  // Réécrire la ligne à chaque frappe, sans l'écho : un jeton collé au terminal reste
  // sinon dans le tampon de défilement, et dans l'historique de la session.
  const muet = (rl as unknown as { _writeToOutput: (texte: string) => void });
  const écrire = muet._writeToOutput.bind(rl);
  muet._writeToOutput = (texte: string) => {
    écrire(texte.includes("Jeton") ? texte : "");
  };

  try {
    return await new Promise<string>((resolve) => {
      rl.question("Jeton Linear (Personal API key) : ", (réponse) => resolve(réponse.trim()));
    });
  } finally {
    rl.close();
    process.stdout.write("\n");
  }
}

interface ViewerResponse {
  readonly viewer: { readonly name: string };
  readonly organization: { readonly id: string; readonly name: string; readonly urlKey: string };
}

/**
 * Dépose un jeton Linear au trousseau, après l'avoir éprouvé.
 *
 * <p>⚠️ Le jeton est **validé avant d'être rangé** : ranger d'abord ferait découvrir la
 * faute de frappe à la première commande utile, loin de l'endroit où on peut la corriger.
 * L'espace n'est jamais saisi, il se **constate** — une clé personnelle est attachée à
 * exactement un workspace, décidé à sa création.</p>
 */
export async function login(options: { token?: string }): Promise<void> {
  const binding = readBinding(process.cwd());
  const token = options.token ?? (await askToken());

  if (token.length === 0) throw new CursusError("Aucun jeton saisi : rien n'a été déposé.");

  const { viewer, organization } = await new LinearClient(token).query<ViewerResponse>(
    "{ viewer { name } organization { id name urlKey } }",
  );

  if (organization.urlKey !== binding.workspaceKey)
    throw new CursusError(
      `Ce jeton dessert l'espace « ${organization.urlKey} », mais ce dépôt est lié à ` +
        `« ${binding.workspaceKey} ». Une clé personnelle ne couvre qu'un seul espace.`,
    );

  const workspace: TrackerWorkspace = {
    id: organization.id,
    key: organization.urlKey,
    name: unescapeName(organization.name),
  };

  const configDirectory = resolveConfigDirectory(process.env["XDG_CONFIG_HOME"], homedir());
  const connection = upsertConnection(configDirectory, workspace);
  writeSecret(connection.secretKey, token);

  emit({
    connected: unescapeName(viewer.name),
    workspace: workspace.key,
    secretKey: connection.secretKey,
  });
}

/** Retire la connexion de cet espace, et le jeton qu'elle désignait. */
export async function logout(): Promise<void> {
  const binding = readBinding(process.cwd());
  const configDirectory = resolveConfigDirectory(process.env["XDG_CONFIG_HOME"], homedir());

  // Le secret d'abord : oublier la connexion en premier ferait perdre la clé sous
  // laquelle il est rangé, et le laisserait au trousseau sans que rien ne le désigne.
  const connection = connectionFor(configDirectory, binding.workspaceKey);
  deleteSecret(connection.secretKey);
  forgetConnection(configDirectory, binding.workspaceKey);

  emit({ disconnected: binding.workspaceKey });
}
