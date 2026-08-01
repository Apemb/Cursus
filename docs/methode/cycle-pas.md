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
[Backlog] ──► Todo ──► In Progress ──► Done
```

**Aucune étiquette, à aucun moment.** C'est le seul des trois niveaux dans ce cas, et c'est écrit
noir sur blanc dans [`dod/pas/done.md`](dod/pas/done.md) §4 : *le pas n'a pas de tiers qui juge*.

**Aucun cycle de revue non plus** — et ce n'est pas un raccourci. On ne relit pas un commit isolé,
on relit un **comportement** ; or un pas est par construction plus petit qu'un comportement
observable. La revue existe donc, elle a simplement lieu **là où son effet devient observable** :
en `Code Review`, au niveau de l'incrément.

**Aucun plan de design non plus.** Un pas qui exigerait son plan de design aurait la taille d'un
incrément — c'est le signe qu'il a été mal découpé, pas qu'il lui manque un document.

Ce que le pas porte à la place, et qui lui est propre : une **test list**, écrite à sa prise.

---

## 2. `Backlog` — créé au découpage, son tour n'est pas venu

Rien ne s'y fait. Les pas naissent au **découpage de leur incrément**, en `Planning` — jamais
avant, puisque c'est le plan de design qui les produit.

---

## 3. `Todo` — le pas suivant d'un incrément en cours

Deux conditions mécaniques : l'incrément parent est `In Progress`, et plus aucun `blockedBy` n'est
ouvert sur ce pas.

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
| `In Progress` | [`prendre-un-pas`](../../.claude/skills/prendre-un-pas/SKILL.md) | La test list écrite **à la prise**, puis un cycle par test, puis le commit | — *(aucune étiquette à ce niveau)* |

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

## 5. `Done` — le mécanique, et ce qu'il ne prouve pas

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

## 6. Registre

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
