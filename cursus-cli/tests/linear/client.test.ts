import { describe, expect, it } from "vitest";

import { LinearClient } from "../../src/linear/client.ts";
import { unescapeName } from "../../src/linear/escaping.ts";

/** Un transport qui note ce qu'on lui a passé et rend la réponse préparée. */
function transportRendant(response: unknown) {
  const appels: { url: string; init: RequestInit }[] = [];
  const fetch = async (url: string, init: RequestInit) => {
    appels.push({ url, init });
    return new Response(JSON.stringify(response), { status: 200 });
  };
  return { fetch, appels };
}

describe("LinearClient", () => {
  it("étant donné une clé personnelle, quand le client interroge l'API, alors le jeton part brut, sans préfixe Bearer", async () => {
    // arrange — une clé personnelle se passe telle quelle ; seul un jeton OAuth prendrait « Bearer »
    const transport = transportRendant({ data: { viewer: { name: "qui" } } });
    const client = new LinearClient("lin_api_abc", transport.fetch);

    // act
    await client.query("{ viewer { name } }");

    // assert
    const entêtes = transport.appels[0]?.init.headers as Record<string, string>;
    expect(entêtes["Authorization"]).toBe("lin_api_abc");
  });

  it("étant donné une réponse porteuse d'erreurs GraphQL, quand le client la reçoit, alors le refus reprend le message de l'API", async () => {
    // arrange
    const transport = transportRendant({
      errors: [{ message: "Entity not found: Document" }],
    });
    const client = new LinearClient("lin_api_abc", transport.fetch);

    // act
    const appel = client.query("{ document(id: \"absent\") { id } }");

    // assert
    await expect(appel).rejects.toThrowError(/Entity not found: Document/);
  });

  it("étant donné un jeton refusé, quand le client interroge l'API, alors le refus dit comment se reconnecter", async () => {
    // arrange
    const fetch = async () => new Response("Unauthorized", { status: 401 });
    const client = new LinearClient("lin_api_périmé", fetch);

    // act
    const appel = client.query("{ viewer { name } }");

    // assert
    await expect(appel).rejects.toThrowError(/cursus linear login/);
  });
});

describe("unescapeName", () => {
  it("étant donné un nom rendu par l'API, quand on le traduit, alors ses entités HTML redeviennent des caractères", async () => {
    // arrange — mesuré : l'API rend « visuel &amp; configuration » là où la donnée porte « & »
    const rendu = "Finition de l'app — visuel &amp; configuration";

    // act
    const nom = unescapeName(rendu);

    // assert
    expect(nom).toBe("Finition de l'app — visuel & configuration");
  });

  it("étant donné un nom sans entité, quand on le traduit, alors il ressort inchangé", async () => {
    // arrange — le tiret cadratin passe intact, seules les entités sont touchées
    const rendu = "Voir tout le tableau, pas sa première page";

    // act
    const nom = unescapeName(rendu);

    // assert
    expect(nom).toBe(rendu);
  });
});
