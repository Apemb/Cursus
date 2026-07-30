# Retours d'expérience — une fiche par exécution

> **La question à laquelle ce dossier répond** : *est-ce que ça progresse ?* Pas *qu'est-ce qui a
> coincé* — ça, c'est [`journal-frictions.md`](../journal-frictions.md).

## Pourquoi un troisième document

Trois documents parlent de la méthode, et les confondre les ferait diverger :

| Document | Ce qu'il porte | Sa forme |
|---|---|---|
| [`journal-frictions.md`](../journal-frictions.md) | ce qui a coincé, **au fil de l'eau** | une ligne brute par occurrence, aucune structure |
| [`decisions.md`](../../design/decisions.md) | ce qu'on a **tranché** | append-only, une entrée par décision |
| **`rex/`** (ici) | ce qu'une exécution a **produit et coûté** | **rubriques fixes**, une fiche par tour |

La différence qui justifie le dossier : **une fiche est comparable à la suivante.** Le journal
répond « qu'est-ce qui nous a gênés » ; il ne peut pas répondre « le deuxième tour a-t-il mieux
marché que le premier », parce que rien n'y est mesuré deux fois de la même façon.

**Une fiche ne recopie jamais le journal** — elle renvoie à ses numéros d'entrée. Une friction
consignée deux fois est une friction qu'on comptera deux fois.

## Ce qu'une fiche doit porter, et dans cet ordre

Les rubriques sont **fixes**. Une rubrique sans matière porte « sans objet », jamais un silence —
un trou dans une fiche casse la comparaison qui est sa seule raison d'être.

1. **Ce qui a tourné** — quel skill, sur quel artefact, et **par quel chemin d'exécution**. Ce
   dernier point n'est pas de la bureaucratie : le mode d'échec redouté est *« la méthode a l'air
   en place et n'est pas chargée »*, et il est muet. Dire *où est la trace qu'il a servi*.

   **La commande y figure verbatim, et rejouable** — prompt complet, options, répertoire de départ.
   C'est ce qui distingue une fiche d'un compte rendu : deux tours ne se comparent que si l'on peut
   établir qu'ils ont été lancés pareil, et un tour ne se rejoue que s'il est écrit. Les options ne
   sont pas un détail d'invocation, elles **sont** le chemin d'exécution — ce qu'on autorise et ce
   qu'on refuse à l'agent fait partie de ce qu'on mesure. ⚠️ **Aucun chemin personnel** : ce dépôt
   est public. Remplacer les redirections par un nom de fichier nu.
2. **Chiffres** — durée, tours, coût, sous-agents, sorties produites. C'est la rubrique qui rend
   deux fiches comparables ; la remplir même quand elle paraît anecdotique.
3. **Conformité au protocole** — le skill a-t-il fait ce qu'il prescrit ? Clause par clause, avec
   ce qui l'atteste. Un « oui » sans pièce ne vaut rien.
4. **Qualité de la sortie** — jugée par qui, contre quoi. Distincte de la conformité : un skill
   peut être suivi à la lettre et produire du vide.
5. **Frictions** — **renvoi** aux numéros du journal. Pas de recopie.
6. **Ce que le tour a changé** — dans les skills, les documents, l'outillage. Si rien n'a changé,
   l'écrire : un tour qui ne change rien est une information.
7. **Verdict pour le skill éprouvé** — `D-043` en nommait trois (**promu** / **corrigé par le
   journal** / **retiré**) et le terrain en a révélé une quatrième : **tué par un fait**, quand une
   mesure invalide son geste central avant tout usage (`D-045` l'a fait aux quatre skills de revue).

## Nommage

`AAAA-MM-JJ-<ce-qui-a-tourné>.md` — la date d'abord, pour que l'ordre du dossier soit l'ordre des
tours.
