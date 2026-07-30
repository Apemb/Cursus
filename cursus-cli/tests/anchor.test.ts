import { describe, expect, it } from "vitest";

import { anchor } from "../src/anchor.ts";

const contenu = [
  "# Plan d'archi — CUR-45",
  "",
  "## Ce que la sonde a établi",
  "",
  "Sondé le 2026-07-26, en-têtes de réponse à l'appui. Le détail est ailleurs.",
  "",
  "Un projet sans issue n'apparaît dans **aucune** issue (voir `Assemble`).",
  "",
  "Puis un assemblage local : regrouper les issues par projet.",
  "",
  "Sondé le 2026-07-26, en-têtes de réponse à l'appui. Le détail est ailleurs.",
].join("\n");

describe("anchor", () => {
  it("étant donné une citation présente une seule fois, quand on l'ancre, alors on obtient le passage à envoyer", () => {
    // arrange
    const citation = "Puis un assemblage local : regrouper les issues par projet.";

    // act
    const ancre = anchor(contenu, citation);

    // assert
    expect(ancre.quotedText).toBe(citation);
  });

  it("étant donné une citation absente du document, quand on l'ancre, alors le refus l'énonce — l'API l'accepterait sans rien dire", () => {
    // arrange — mesuré : Linear rend success:true sur une citation qui ne correspond à
    // rien. Personne d'autre que cette fonction ne peut arrêter le contresens.
    const citation = "Ce passage n'a jamais été écrit dans ce document.";

    // act
    const ancrage = () => anchor(contenu, citation);

    // assert
    expect(ancrage).toThrowError(/ne figure pas/);
  });

  it("étant donné une citation présente deux fois, quand on l'ancre, alors le refus dit combien de fois et demande d'allonger", () => {
    // arrange — l'UI surlignerait la première trouvée, qui n'est pas forcément la visée
    const citation = "Sondé le 2026-07-26, en-têtes de réponse à l'appui.";

    // act
    const ancrage = () => anchor(contenu, citation);

    // assert
    expect(ancrage).toThrowError(/2 fois/);
  });

  it("étant donné une citation dont les blancs diffèrent du document, quand on l'ancre, alors elle est trouvée et c'est le passage du document qui est retenu", () => {
    // arrange — recopier un passage à la main écrase les retours à la ligne. Envoyer la
    // frappe de l'utilisateur plutôt que le texte du document produirait une ancre qui
    // ne surligne rien : Linear compare au caractère près.
    const citation = "## Ce que la sonde a établi   Sondé le 2026-07-26,";

    // act
    const ancre = anchor(contenu, citation);

    // assert
    expect(ancre.quotedText).toBe("## Ce que la sonde a établi\n\nSondé le 2026-07-26,");
  });

  it("étant donné une citation porteuse de caractères d'expression régulière, quand on l'ancre, alors ils sont pris au pied de la lettre", () => {
    // arrange — un plan d'archi est plein de `**gras**`, de (parenthèses) et de [liens]
    const citation = "dans **aucune** issue (voir `Assemble`)";

    // act
    const ancre = anchor(contenu, citation);

    // assert
    expect(ancre.quotedText).toBe(citation);
  });

  it("étant donné une citation vide, quand on l'ancre, alors elle est refusée avant tout appel", () => {
    // arrange — une citation vide correspondrait « partout », donc nulle part
    const citation = "   \n  ";

    // act
    const ancrage = () => anchor(contenu, citation);

    // assert
    expect(ancrage).toThrowError(/vide/);
  });

  it("étant donné une citation trouvée, quand on l'ancre, alors on sait où elle commence dans le document", () => {
    // arrange — de quoi montrer le contexte à l'appelant sans re-chercher
    const citation = "Un projet sans issue";

    // act
    const ancre = anchor(contenu, citation);

    // assert
    expect(contenu.slice(ancre.start, ancre.start + citation.length)).toBe(citation);
  });
});
