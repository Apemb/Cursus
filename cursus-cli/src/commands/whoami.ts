import { unescapeName } from "../linear/escaping.ts";
import { emit } from "../output.ts";
import { openSession } from "../session.ts";

interface ViewerResponse {
  readonly viewer: { readonly name: string; readonly email: string };
  readonly organization: { readonly name: string; readonly urlKey: string };
}

/**
 * À qui parle-t-on, et de quel espace. Le verbe le plus modeste de la CLI, et le seul
 * qui éprouve **toute** la chaîne de résolution d'un coup — c'est ce qu'on lance en
 * premier quand quelque chose ne marche pas.
 */
export async function whoami(): Promise<void> {
  const { client, connection } = openSession();

  const { viewer, organization } = await client.query<ViewerResponse>(
    "{ viewer { name email } organization { name urlKey } }",
  );

  emit({
    viewer: { name: unescapeName(viewer.name), email: viewer.email },
    organization: { name: unescapeName(organization.name), urlKey: organization.urlKey },
    connection: { label: connection.label, secretKey: connection.secretKey },
  });
}
