import { CursusError } from "../errors.ts";

const Endpoint = "https://api.linear.app/graphql";

/** Le transport ; injecté pour que les tests n'aient pas besoin du réseau. */
export type Transport = (url: string, init: RequestInit) => Promise<Response>;

interface GraphQLResponse<T> {
  readonly data?: T;
  readonly errors?: readonly { readonly message?: string }[];
}

/**
 * L'adaptateur HTTP : un seul endpoint, en POST.
 *
 * ⚠️ **Le jeton se passe brut**, sans préfixe `Bearer` — c'est une *Personal API key*.
 * Un jeton OAuth, lui, en prendrait un ; on ne vise pas OAuth, Cursus étant un outil de
 * développement mono-utilisateur.
 */
export class LinearClient {
  // Champs déclarés puis affectés, plutôt que des propriétés de paramètre : ces dernières
  // *produisent* du code, et Node — qui se contente d'effacer les types — ne saurait pas
  // les exécuter.
  private readonly token: string;
  private readonly transport: Transport;

  constructor(token: string, transport: Transport = (url, init) => fetch(url, init)) {
    this.token = token;
    this.transport = transport;
  }

  async query<T>(query: string, variables: Record<string, unknown> = {}): Promise<T> {
    const response = await this.transport(Endpoint, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: this.token,
      },
      body: JSON.stringify({ query, variables }),
    });

    if (response.status === 401 || response.status === 403)
      throw new CursusError(
        "Linear a refusé le jeton de cette connexion. Reconnectez-vous avec « cursus linear login ».",
      );

    if (!response.ok)
      throw new CursusError(`Linear a répondu ${response.status} : ${(await response.text()).slice(0, 300)}`);

    const payload = (await response.json()) as GraphQLResponse<T>;

    if (payload.errors?.length)
      throw new CursusError(
        `Linear a refusé la requête : ${payload.errors.map((e) => e.message ?? "(sans message)").join(" ; ")}`,
      );

    if (!payload.data)
      throw new CursusError("Linear a répondu sans données ni erreur — réponse inexploitable.");

    return payload.data;
  }
}
