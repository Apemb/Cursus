import { CursusError } from "./errors.ts";

/**
 * Les codes de sortie. Ils sont **la** façon dont un agent apprend ce qui s'est passé
 * sans lire de prose : distinguer une faute d'usage d'une panne lui dit s'il doit
 * corriger son appel ou réessayer plus tard.
 */
export const ExitCode = {
  Ok: 0,
  /** L'appel était fautif, ou l'état ne s'y prêtait pas — le message dit quoi faire. */
  Refus: 1,
  /** Une panne : rien ne dit que réessayer à l'identique échouerait encore. */
  Panne: 2,
} as const;

/**
 * Rend le résultat. **JSON par défaut** : le consommateur qui dimensionne cette CLI est
 * un agent, et une sortie qu'il faut analyser à la regex est une sortie qu'on casse à la
 * première amélioration de mise en forme. `--human` reste possible commande par commande,
 * là où la lecture à l'œil a un sens.
 */
export function emit(value: unknown): void {
  process.stdout.write(`${JSON.stringify(value, null, 2)}\n`);
}

/**
 * Imprime l'échec et rend le code qui convient. Un {@link CursusError} sort **sans pile** :
 * son message s'adresse à celui qui a tapé la commande. Tout le reste garde la sienne,
 * parce qu'une pile tronquée sur un vrai bug coûte plus cher qu'une pile de trop.
 */
export function fail(error: unknown): number {
  if (error instanceof CursusError) {
    process.stderr.write(`${error.message}\n`);
    return ExitCode.Refus;
  }

  process.stderr.write(`${error instanceof Error ? (error.stack ?? error.message) : String(error)}\n`);
  return ExitCode.Panne;
}

/**
 * Déroule une commande, en traduisant tout échec en code de sortie. Nommée `execute` et
 * non `run` : le point d'entrée de la CLI porte ce nom-là, et deux `run` dans le même
 * fichier de câblage est une collision qui compile mal et se lit encore plus mal.
 */
export async function execute(action: () => Promise<void>): Promise<void> {
  try {
    await action();
  } catch (error) {
    process.exitCode = fail(error);
  }
}
