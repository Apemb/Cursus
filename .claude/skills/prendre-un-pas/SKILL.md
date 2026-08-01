---
name: prendre-un-pas
description: Prendre un pas (sous-tâche Linear) de `Todo` à `Done` — écrire sa test list à la prise, dérouler le cycle rouge → vert → refactor un test à la fois, committer librement, clore sur suite verte et zéro warning. À invoquer dès qu'une carte de pas passe en `In Progress`, qu'on exécute une sous-tâche technique du backlog, ou qu'on demande de « prendre le pas suivant ».
---

# Prendre un pas

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

La conception a déjà eu lieu : le plan de design de l'incrément a posé les frontières, la carte du
pas dit où il s'arrête. Ici, les mains, pas la tête — ce skill n'invente rien, il **exécute**. Le
standard non négociable, le régime TDD, la frontière testé/non-testé et les conventions de
modélisation sont déjà dans `CLAUDE.md`, chargé par ailleurs : ne pas les répéter, seulement les
tenir.

## 1. Avant le premier test

- Lire la carte en entier : le pas, le pourquoi à cette place, le piège local s'il existe.
- Créer, ou reprendre, la branche `pas/<identifiant>-slug` depuis la branche de la story.
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

## 5. Clore le pas

Une fois la test list épuisée et la suite verte : fusionner `pas/` dans `story/` en **squash**, et
réécrire le corps à la main — GitHub y colle par défaut la concaténation des WIP, et c'est ce
commit-là qui reste dans l'histoire. Le corps dit le comportement ajouté et les alternatives
écartées en cours de route, pas la liste des commits.

**Fin de l'étape** : `docs/methode/dod/pas/done.md` coché de bout en bout.
