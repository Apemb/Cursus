import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";

import { CursusError } from "../errors.ts";

/**
 * Ce qu'un dépôt Cursus déclare du tableau qu'il dessert. Le pendant TypeScript de
 * `TrackerBinding` côté C# : il ne nomme pas une connexion, il décrit ce qu'elle doit
 * desservir — d'où le fait qu'un même dépôt se retrouve sur un autre poste sans rien
 * changer.
 */
export interface LinearBinding {
  readonly workspaceKey: string;
}

interface BindingDocument {
  readonly tracker?: { readonly kind?: string; readonly workspaceKey?: string };
}

/**
 * Lit `<root>/.cursus/project.json` et rend ce qu'il déclare du tracker.
 *
 * ⚠️ Un genre de tracker inconnu est **refusé**, jamais dégradé en binding vide : le
 * registre C# fait le choix inverse (il ignore la connexion) parce qu'il en affiche
 * plusieurs, là où une commande n'en vise qu'une — se taire ici produirait un « aucun
 * jeton configuré » que rien n'expliquerait.
 */
export function readBinding(projectRoot: string): LinearBinding {
  const path = join(projectRoot, ".cursus", "project.json");

  if (!existsSync(path))
    throw new CursusError(
      `${projectRoot} n'est pas un projet Cursus : ${path} est introuvable.`,
    );

  const document = JSON.parse(readFileSync(path, "utf8")) as BindingDocument;
  const tracker = document.tracker;

  if (tracker?.kind !== "linear")
    throw new CursusError(
      `Ce projet est lié à un tracker de genre « ${tracker?.kind ?? "absent"} », ` +
        "que cette commande ne sait pas joindre — seul « linear » est desservi.",
    );

  if (!tracker.workspaceKey)
    throw new CursusError(
      `${path} déclare un tracker Linear sans workspaceKey : impossible de savoir quel espace viser.`,
    );

  return { workspaceKey: tracker.workspaceKey };
}
