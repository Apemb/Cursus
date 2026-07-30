import { homedir } from "node:os";

import { readBinding } from "./config/binding.ts";
import { readSecret as readSecretFromKeychain } from "./config/keychain.ts";
import { connectionFor, resolveConfigDirectory, type LinearConnection } from "./config/trackers.ts";
import { CursusError } from "./errors.ts";
import { LinearClient, type Transport } from "./linear/client.ts";

/**
 * Ce dont toute commande a besoin : à qui l'on parle, et de quel espace il s'agit.
 * Rassemblé ici pour que la chaîne de résolution — binding, connexion, jeton — soit
 * décrite une fois et pas dans chaque verbe.
 */
export interface Session {
  readonly client: LinearClient;
  readonly connection: LinearConnection;
}

export interface SessionOptions {
  readonly projectRoot?: string;
  readonly configDirectory?: string;
  /** Injectés par les tests, pour n'avoir besoin ni du trousseau ni du réseau. */
  readonly readSecret?: (key: string) => string | undefined;
  readonly transport?: Transport;
}

export function openSession(options: SessionOptions = {}): Session {
  const projectRoot = options.projectRoot ?? process.cwd();
  const configDirectory =
    options.configDirectory ??
    resolveConfigDirectory(process.env["XDG_CONFIG_HOME"], homedir());
  const readSecret = options.readSecret ?? ((key: string) => readSecretFromKeychain(key));

  const binding = readBinding(projectRoot);
  const connection = connectionFor(configDirectory, binding.workspaceKey);
  const token = readSecret(connection.secretKey);

  // Une connexion déclarée dont le secret a disparu n'est pas une absence de connexion :
  // les deux se réparent au même endroit, mais envoyer chercher la panne du mauvais côté
  // coûte le temps qu'on met à comprendre que le registre, lui, était bon.
  if (!token)
    throw new CursusError(
      `La connexion « ${connection.label} » est déclarée, mais son jeton est absent du trousseau ` +
        `(clé ${connection.secretKey}). Redéposez-le avec « cursus linear login ».`,
    );

  return {
    client: new LinearClient(token, options.transport),
    connection,
  };
}
