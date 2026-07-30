/**
 * Un échec dont le message s'adresse à celui qui a tapé la commande — par opposition à
 * une panne, dont la trace s'adresse à celui qui débogue.
 *
 * <p>La distinction porte la sortie : un `CursusError` s'imprime seul, sans pile, et
 * sort en code 1 ; tout le reste remonte entier, parce qu'une pile tronquée sur un bug
 * réel coûte plus cher qu'une pile de trop.</p>
 */
export class CursusError extends Error {
  constructor(message: string) {
    super(message);
    this.name = "CursusError";
  }
}
