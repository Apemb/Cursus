---
name: prendre-un-pas
description: Prendre un pas (sous-tâche Linear) de `Todo` jusqu'à sa revue — écrire sa test list à la prise, dérouler le cycle rouge → vert → refactor un test à la fois, committer librement, poser `Done` sur suite verte et zéro warning. À invoquer dès qu'on prend une carte de pas en `Todo`, qu'on exécute une sous-tâche technique du backlog, ou qu'on demande de « prendre le pas suivant ».
---

# Prendre un pas

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

La conception a déjà eu lieu : le plan de design de l'incrément a posé les frontières, et
`decoupage-pas` a créé la carte de ce pas en disant où il s'arrête. Ici, les mains, pas la tête — ce
skill n'invente rien, il **exécute**. Le
standard non négociable, le régime TDD, la frontière testé/non-testé et les conventions de
modélisation sont déjà dans `CLAUDE.md`, chargé par ailleurs : ne pas les répéter, seulement les
tenir.

## 1. Avant le premier test

- **Tirer la carte** : le pas t'attend en `Todo` ; déplace-le en `In Progress` et retire son
  étiquette s'il en porte une. Un ticket n'est jamais poussé — c'est celui qui prend le travail qui
  déplace la carte (`cycle.md` §4). ⚠️ **Tu ne déplaces pas l'incrément parent** : il est déjà en
  `In Progress`, `decoupage-pas` l'y a tiré pour y créer ce pas. Tu ne lui poses pas non plus
  d'étiquette — **sauf si ce pas est le dernier** : celui qui achève le dernier pas pose `Done` sur
  l'incrément, et s'arrête là ; c'est `revue-code` qui le tire ensuite vers `Code Review`, à
  l'échelle du module (`cycle-increment.md` §6).
- Lire la carte en entier : le pas, le pourquoi à cette place, le piège local s'il existe.
- Créer, ou reprendre, la branche `pas/<identifiant du pas>-slug` depuis la branche de la story.
  ⚠️ **Si la branche de story n'existe pas, crée-la d'abord** : aucun skill ne la porte aujourd'hui,
  et le trou est écrit (`D-075` §4). Ne pas le combler en silence — le dire dans le rapport de fin.
- Le pas touche-t-il un objet déjà modélisé, ou en nomme-t-il un nouveau ? Si oui, relire la
  section concernée de `docs/design/architecture.md` — et le vocabulaire §3 de
  `docs/design/schemas.md` s'il s'agit de définition vs exécution — juste ce qui nomme ce que le
  pas va toucher, pas le document entier. Sinon, rien à y chercher.

**Fin de l'étape** : si la carte ne suffit pas à travailler sans la conversation — un « jusqu'où »
reste ouvert — ne pas deviner. Noter la friction dans `docs/methode/journal-frictions.md` et
reposer la carte plutôt que combler en silence.

## 2. Écrire la test list — à la prise, jamais avant

Une ligne par comportement observable, groupées par contexte si plusieurs se dessinent :

`étant donné <état>, quand <action>, alors <conséquence observable>`

**Fin de l'étape** : la liste vit sur la carte et reste vivante pour tout le pas — un cas
découvert au rouge s'y ajoute, rien ne se traite à côté d'elle.

## 3. Dérouler, un test à la fois

Pour chaque ligne de la test list :

- **Rouge** — ajouter ce seul test, lancer la suite, observer l'échec. Il compte comme rouge
  seulement si l'échec porte sur l'assertion écrite (le message de comparaison) — pas sur une
  erreur de compilation, une exception non prévue, ou un test ignoré. Si la raison n'est pas la
  bonne, corriger le test avant de toucher au code de production.
- **Vert** — l'implémentation la plus simple qui fait passer ce test, tricher permis (constante,
  stub). Le test suivant forcera la généralisation.
- **Refactor** — sur le test et sur le code testé, sans changer le comportement. Ici, au vert,
  jamais reporté à la revue. Relancer la suite : elle reste verte.

Un test vert du premier coup est légitime — il verrouille un comportement déjà couvert — mais
l'écrire dans la test list plutôt que le passer sous silence.

**Fin de l'étape** : la test list est épuisée, et chaque ligne qu'elle porte a été vue rouge pour
la bonne raison avant d'être verte.

## 4. Committer

Librement — WIP, correction, refactor : la fusion en squash produit le commit propre, la règle
« un commit par comportement » n'existe plus. Ce qui ne bouge pas : chaque commit reste sur une
suite verte et zéro warning — la liberté porte sur leur nombre, pas sur le standard.

## 5. Clore le pas — trois gestes, puis s'arrêter là

Une fois la test list épuisée, la suite verte et zéro warning, **trois gestes et pas un de plus**
(`D-075`) :

1. **Pose `Done`** sur la carte du pas.
2. **Pousse la branche** `pas/`. Pousser une branche de travail est ordinaire ; seul `main` reste
   interdit sans demande explicite.
3. **Ouvre la PR** vers la branche de story, et **mets son lien sur la carte**.

Le motif du troisième : la revue d'un pas a lieu **sur la PR**, donc la PR doit exister avant elle
(`flux.md` §6). ⚠️ **Elle n'est pas pour autant le lieu des remarques** — celles-ci vivent sur la
carte Linear (`D-045`), et scinder le dossier de revue entre GitHub et Linear le rendrait illisible.
La PR est la surface du **diff** et le véhicule de la **fusion**, rien d'autre.

⚠️ **Ne fusionne pas, et ne déplace pas la carte.** Un pas passe par sa propre `Code Review`,
à l'échelle de la **fonction** — ce que prouvent les tests, leur formulation, le nommage
(`cycle-pas.md` §5). C'est `revue-code` qui tire la carte en `Code Review` à sa prise, et la fusion
n'a lieu qu'**après** son accord : c'est elle qui fait basculer le pas en `Done`, colonne terminale.

Quand ce moment vient, la fusion de `pas/` dans `story/` se fait en **squash**, corps réécrit à la
main — GitHub y colle par défaut la concaténation des WIP, et c'est ce commit-là qui reste dans
l'histoire. Le corps dit le comportement ajouté et les alternatives écartées en cours de route, pas
la liste des commits.

**Fin de l'étape** : la carte porte `Done`, elle est toujours en `In Progress`, et
`docs/methode/dod/pas/code-review.md` est ce contre quoi elle va être relue.
