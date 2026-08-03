> **Écrit avant d'avoir été exécuté** — voir l'avertissement en tête de [`cycle.md`](cycle.md).

# Le cycle d'un pas, colonne par colonne

> **À quoi sert ce fichier.** Il donne, pour chaque colonne d'une **sous-tâche**, ce qui s'y fait et
> ce qui doit exister en sortie. C'est le niveau qui **prouve** — celui où un comportement passe de
> promis à démontré.
>
> **Ce qu'il ne dit pas.** Les **gestes** vivent dans
> [`prendre-un-pas`](../../.claude/skills/prendre-un-pas/SKILL.md), les **critères opposables** dans
> [`dod/pas/`](dod/pas/), et ce que **contient** un pas dans [`tickets.md`](tickets.md) §4. Ce qui
> englobe un pas est dans [`cycle-increment.md`](cycle-increment.md).
>
> **Le niveau se déduit de la structure** : un pas est une **issue avec un `parentId`**. Elle est
> aussi rattachée au projet, pour rester visible.

---

## 1. Le chemin, et ce qu'il n'a pas

```
Todo ──► In Progress ──► Code Review ──► Done
```

**Les mêmes étiquettes qu'à l'incrément** — les six états du groupe *Advancement Labels*
([`cycle.md`](cycle.md) §2), `Done` et `Rework Needed` en tête. Un pas se signale et se tire comme
n'importe quelle carte : qui finit pose, l'aval tire et retire l'étiquette (`cycle.md` §4).

**Ce qui tire un pas en `Done`, c'est la fusion de sa branche** — le squash de `pas/` dans le
`story/` de son incrément. Le motif se retrouve à chaque niveau : c'est l'arrivée du travail dans la
branche du niveau au-dessus qui fait basculer la carte, jamais la satisfaction de celui qui l'a
écrit. Un pas dont la suite est verte mais dont la branche n'est pas fusionnée reste en
`Code Review`, portant `Done`.

**Une `Code Review`, à l'échelle du pas.** Elle existe, et elle n'est pas celle de l'incrément : ici
on relit la **fonction** — la validité des tests, ce qu'ils prouvent réellement, la formulation des
noms de test, le nommage des variables, la forme du code écrit. L'incrément, lui, relit le
**module** : la classe, le design, la cohérence de l'ensemble, et il a le droit de réclamer des pas
supplémentaires pour corriger. Même colonne, deux échelles — et la plus grande ne rattrape pas la
plus fine, parce qu'elle ne la regarde pas.

**Aucun `Planning`, donc aucun `Plan Review`.** Un pas qui exigerait son plan de design aurait la
taille d'un incrément — c'est le signe qu'il a été mal découpé, pas qu'il lui manque un document.

**Aucune `QA Review` non plus.** Recetter demande quelque chose d'observable par le rôle produit ;
un pas ne l'est pas, et c'est précisément ce qui le distingue d'un incrément
([`tickets.md`](tickets.md) §1).

**Aucun `Backlog` non plus**, ce qui fait de ce chemin le plus court du dépôt — voir §2.

Ce que le pas porte en propre : une **test list**, écrite à sa prise.

---

## 2. `Backlog` — la colonne qui n'existe pas à ce niveau

Elle existe dans Linear, mais **aucun pas n'y entre jamais**, et c'est un refus explicite, pas un
oubli (`D-072`).

Au niveau issue, `Backlog` est l'**entrée latérale** : ce qui arrive **sans parent**, donc sans
spec, donc sans recette de niveau feature — un refacto autonome, une dette. Un pas a toujours un
parent, par définition (`tickets.md` §1). Il n'y a donc rien qui puisse entrer par cette porte, et
la garder ouverte ne ferait que suggérer un état que rien ne produit.

**Un pas bloqué naît donc en `Todo` comme les autres**, portant son `blockedBy`. Le motif complet
est celui de l'incrément ([`cycle-increment.md`](cycle-increment.md) §2) : dans un flux tiré, une
frontière n'existe que si quelqu'un tire à travers, et le déblocage n'est le travail de personne.

⚠️ **Ce qui reste vrai du moment de la naissance** : les pas naissent au **découpage de leur
incrément**, à l'entrée de celui-ci en `In Progress` — porté par
[`decoupage-pas`](../../.claude/skills/decoupage-pas/SKILL.md), jamais plus tôt (`D-070`). **Pas en
`Planning`** : le plan de design ne dit que la **maille visée**, il ne crée aucune sous-tâche. Un
pas figé avant que le premier ne soit exécuté est un *waterfall* à petite échelle — c'est ce qu'on
apprend au pas 1 qui donne sa forme au pas 4.

---

## 3. `Todo` — la colonne de naissance, et le pas suivant d'un incrément en cours

Tous les pas y naissent, bloqués ou non. Deux conditions mécaniques décident ensuite de celui qui
est **tirable** : l'incrément parent est `In Progress`, et plus aucun `blockedBy` n'est ouvert sur
ce pas. Elles se vérifient par **celui qui tire**, jamais par un déplacement que personne ne ferait.

Et une condition de fond, qui est la même qu'à l'incrément :
[`dod/pas/todo.md`](dod/pas/todo.md) §1 — **le contexte tient dans la carte, sans la
conversation**. Le titre tient en une action ; *pourquoi celui-là, à cette place, et où il
s'arrête* est écrit, le frère voisin nommé.

⚠️ **Comportemental, jamais procédural.** Un chemin de fichier ou un numéro de ligne dans la carte
est un défaut : il périme au premier renommage, et il prescrit l'implémentation au lieu de décrire
le comportement attendu.

⚠️ **La test list n'est pas là, et c'est voulu.** Une carte qui la porterait aurait décidé de la
conception avant la prise.

---

## 4. `In Progress` — la test list, puis les cycles

| État observé | Skill invoqué | Livrable | État posé |
|---|---|---|---|
| `In Progress` + *aucune* | [`prendre-un-pas`](../../.claude/skills/prendre-un-pas/SKILL.md) | La test list écrite **à la prise**, puis un cycle par test, puis le commit | `Done` |

**La test list s'écrit ici et vit ici.** Jamais d'avance : ce qu'on apprend au troisième test change
ce qu'on sait qu'il faut écrire au sixième. Elle reflète l'état **final** — tout cas découvert en
cours de cycle y est ajouté, y compris ceux qu'on n'avait pas vus au départ.

**Un cycle, trois temps, et le premier est celui qu'on saute** :

1. **Rouge observé — et pour la bonne raison.** Un test qui échoue parce que le module n'existe pas
   encore n'est pas un rouge valable : c'est une erreur de chargement. Il faut voir échouer
   l'**assertion**, quitte à créer un bouchon d'abord. C'est la vérification que le test teste bien
   ce qu'il prétend.
2. **Vert par l'implémentation la plus simple**, quitte à tricher — le test suivant force la
   généralisation.
3. **Refactor une fois vert**, sur le code de test comme sur le code testé.

**Un test à la fois.** Jamais de code de production sans un test rouge qui le réclame.

**Sur la branche `pas/<identifiant>`, commiter librement** — WIP, correction, refactor. Le squash
de fusion vers `story/` produit le commit propre ; ce n'est plus une discipline à tenir, c'est une
mécanique.

---

## 5. `Code Review` — relire la fonction, pas le module

| État observé | Skill invoqué | Livrable | État posé |
|---|---|---|---|
| `Code Review` + *aucune* | [`revue-code`](../../.claude/skills/revue-code/SKILL.md), **à l'échelle du pas** | Les remarques posées sur la carte du pas, chacune citant son référentiel | `Rework Needed` \| `Done` si aucune |
| `Code Review` + `Rework Needed` | `correction` **(à écrire)** | Le pas repris, une réponse dans chaque fil | `Rework Done` |
| `Code Review` + `Rework Done` | `verification` **(à écrire)** | Chaque remarque soldée, ou rouverte avec ce qui manque | `Rework Needed` \| `Done` si `open` vaut 0 |
| `Code Review` + `Done` | — | — | **`revue-code` fusionne** `pas/` dans `story/` à l'accord, ce qui tire vers `Done` (`D-076`) |

**Ce qui se relit ici** : la validité des tests — prouvent-ils ce que leur titre annonce —, la
formulation des noms de test contre la convention `étant donné / quand / alors`, le nommage, la
forme du code. **Ce qui ne s'y relit pas** : le design, la découpe en classes, la cohérence de
l'ensemble. Ceux-là attendent la `Code Review` de l'**incrément**, qui voit ce qu'un pas seul ne
montre pas — et qui peut réclamer des pas supplémentaires, ce qu'une revue de pas ne fait jamais.

⚠️ **La plus grande échelle ne rattrape pas la plus fine.** Un relecteur d'incrément qui parcourt un
diff de plusieurs pas ne relit pas le nom de chaque variable ; compter sur lui pour le faire, c'est
n'avoir personne. C'est le motif qui donne à ce niveau sa propre revue, alors qu'un pas est plus
petit qu'un comportement observable.

---

## 6. `Done` — le mécanique, et ce qu'il ne prouve pas

| Ce qui est exigé | Où c'est écrit |
|---|---|
| Commit fait, arrivé en squash dans `story/`, **corps réécrit à la main** | `flux.md` §6, `D-042` |
| `dotnet test` entièrement verte | `CLAUDE.md` |
| `dotnet build` **zéro warning**, y compris dans les tests | `CLAUDE.md` |

⚠️ **Le vert ne prouve pas le rouge.** Un test écrit après le code passe aussi, et rien dans la
suite ne distingue les deux. C'est ce que [`dod/pas/done.md`](dod/pas/done.md) §2 demande de tenir
en plus du mécanique : chaque test de la test list a été **vu rouge pour la bonne raison** avant
d'être vert. C'est la seule exigence de ce niveau que rien n'automatise, et donc la seule qui
repose entièrement sur l'honnêteté de qui exécute.

---

⚠️ **`Done` est une colonne terminale** : aucun travail ne commence après elle, donc rien ne peut
l'atteindre en tirant. C'est l'unique exception à la règle du flux tiré, et elle est écrite en
[`cycle.md`](cycle.md) §4 — ici, c'est la fusion de la branche qui fait le geste.

---

## 7. Registre

**Construit** : rien de ce cycle n'a tourné **avec un agent**. Le régime TDD lui-même est en
revanche tenu depuis le premier jalon, à la main, sur toute la logique métier — c'est le seul
morceau de ces trois documents qui décrive une pratique réellement éprouvée, et non un dispositif
imaginé.

**Tranché mais pas construit** : la prise d'un pas **par un agent**, sur la seule foi de sa carte.
Le skill [`prendre-un-pas`](../../.claude/skills/prendre-un-pas/SKILL.md) est un **draft écrit
d'avance**, contre ce que `D-039` prescrit.

**Question ouverte, et c'est la principale** : *une carte de pas contient-elle assez pour qu'un
agent travaille sans avoir eu la conversation ?* C'est le signal que ce niveau doit rendre en
premier — raison pour laquelle `prendre-un-pas` est le skill à éprouver avant tous les autres :
c'est le plus petit périmètre, l'erreur y coûte un commit, il ne dépend d'aucun autre, et on peut
lui tendre un pas écrit à la main.
