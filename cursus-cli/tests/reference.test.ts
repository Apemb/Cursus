import { describe, expect, it } from "vitest";

import { headingAt, reviewBody } from "../src/reference.ts";

/** Un document de méthode réaliste : titres imbriqués et bloc de code clôturé. */
const contenu = [
  "# Discovery — Un agent pilote Cursus",
  "",
  "Le besoin tient en une phrase.",
  "",
  "## §1. Quel besoin, et pour qui ?",
  "",
  "Ouverture, pas un choix. Aucune n'est départagée ici.",
  "",
  "## §2. Comment on le vérifie",
  "",
  "### §2.1 Le standard de qualité",
  "",
  "```bash",
  "# dotnet build ne doit rendre aucun warning",
  "dotnet test",
  "```",
  "",
  "Le seuil est tenu depuis le premier jalon.",
].join("\n");

/** L'offset d'un passage dans le document — ce que `anchor` rend déjà. */
function offsetDe(passage: string): number {
  const trouvé = contenu.indexOf(passage);
  if (trouvé === -1) throw new Error(`Le fixture ne contient pas « ${passage} »`);
  return trouvé;
}

describe("headingAt", () => {
  it("étant donné un passage sous un titre de section, quand on cherche son repère, alors ce titre est rendu sans ses dièses", () => {
    // arrange
    const passage = offsetDe("Ouverture, pas un choix.");

    // act
    const titre = headingAt(contenu, passage);

    // assert
    expect(titre).toBe("§1. Quel besoin, et pour qui ?");
  });

  it("étant donné un passage précédé d'un bloc de code clôturé, quand on cherche son repère, alors un commentaire shell n'y est pas pris pour un titre", () => {
    // arrange — les documents de méthode sont pleins de blocs `bash` où le dièse ouvre un
    // commentaire. Sans suivi des clôtures, le repère citerait « dotnet build … ».
    const passage = offsetDe("Le seuil est tenu");

    // act
    const titre = headingAt(contenu, passage);

    // assert
    expect(titre).toBe("§2.1 Le standard de qualité");
  });

  it("étant donné un passage placé avant tout titre, quand on cherche son repère, alors il n'y en a pas", () => {
    // arrange — un document peut ouvrir sur de la prose. Le repère se réduira au titre du
    // document, ce qui reste une désignation utile.
    const sansTitre = "Une note libre, sans structure.\n\n# Puis un titre, trop tard.";

    // act
    const titre = headingAt(sansTitre, sansTitre.indexOf("Une note"));

    // assert
    expect(titre).toBeUndefined();
  });

  it("étant donné une citation qui est le titre lui-même, quand on cherche son repère, alors c'est ce titre qui situe", () => {
    // arrange — citer un titre pour le chicaner est un cas normal en revue
    const passage = offsetDe("## §2. Comment on le vérifie");

    // act
    const titre = headingAt(contenu, passage);

    // assert
    expect(titre).toBe("§2. Comment on le vérifie");
  });
});

describe("reviewBody", () => {
  it("étant donné un document, une section et une remarque, quand on compose le corps, alors le repère est en italique sur sa propre ligne", () => {
    // arrange — mesuré : l'UI aplatit `quotedText` sur une ligne, donc le repère ne peut
    // pas voyager avec le passage cité. Il vit dans le corps, où le saut de ligne tient.
    const remarque = "Cette ouverture n'est pas départagée, mais la Spec la traite comme acquise.";

    // act
    const corps = reviewBody({
      document: "Discovery — Un agent pilote Cursus",
      heading: "§1. Quel besoin, et pour qui ?",
      remark: remarque,
    });

    // assert
    expect(corps).toBe(
      "*Ref : Discovery — Un agent pilote Cursus › §1. Quel besoin, et pour qui ?*\n\n" + remarque,
    );
  });

  it("étant donné un passage sans section au-dessus, quand on compose le corps, alors le repère se réduit au titre du document", () => {
    // arrange — le document reste une désignation utile à lui seul : c'est lui qui distingue
    // la Discovery de la Spec, qui vivent sur le même porteur.
    const remarque = "La note d'ouverture mériterait d'être datée.";

    // act
    const corps = reviewBody({ document: "Spec — Un agent pilote Cursus", heading: undefined, remark: remarque });

    // assert
    expect(corps).toBe("*Ref : Spec — Un agent pilote Cursus*\n\n" + remarque);
  });
});
