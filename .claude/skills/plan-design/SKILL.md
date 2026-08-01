---
name: plan-design
description: Produit le plan de design d'un incrément — schéma-delta, table des objets impactés, découpage en pas — et gate la première ligne de code derrière sa validation. Use when un incrément entre en Planning, quand un changement va créer ou supprimer une classe, traverser plusieurs modules, ou impliquer une découpe non évidente, ou quand on demande explicitement de planifier ou d'écrire le plan de design d'un incrément.
---

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

# plan-design

Ce skill **gate** : tant que son plan n'est pas écrit, aucun test rouge sur cet incrément n'a de
raison d'exister. `CLAUDE.md` l'exige dès qu'un changement crée ou supprime une classe, traverse
plusieurs modules, ou implique une découpe non évidente.

**L'échelle est celle des objets** (`D-053`) : lesquels naissent, changent ou meurent, et quelles
responsabilités ils portent. Les deux échelles voisines ne sont pas la tienne, et empiéter coûte
dans les deux sens :

- **au-dessus**, le **plan d'architecture** de la spec a tenu le système et le module. Il est
  d'ensemble et **indicatif** : tu as le droit de t'en écarter au contact du réel, à condition de le
  dire — ne le rejoue pas, ne le contredis pas en silence ;
- **en dessous**, la **test list** de chaque pas tiendra le code, fichier par fichier. Elle s'écrit
  à la prise du pas, jamais ici.

## 1. Décider si l'étape a lieu

`Planning` est **conditionnel** (`tickets.md` §6.2). Relis l'incrément en `Todo` contre les trois
critères ci-dessus.

- Aucun ne s'applique → saute directement à `In Progress`, écris-le en une phrase dans l'incrément
  (« pas de plan, changement local à une classe ») pour que le lecteur suivant sache que ce n'est
  pas un oubli.
- Au moins un s'applique → continue.

**Fait quand** : la décision est écrite quelque part, jamais silencieuse.

## 2. Choisir où vit le plan

- L'incrément est **porté par une carte** (cas nominal) → le plan est le **document attaché**,
  écrit maintenant, en `Planning`. Linear rend le mermaid nativement.
- L'incrément **n'est porté par aucune carte** → le plan est un fichier, et sa **toute première
  ligne**, avant le titre, est `> Fichier : <chemin absolu>` — le schéma n'existe que dans
  l'aperçu de ce fichier.

**Fait quand** : le plan existe à l'endroit que ce choix désigne, pas ailleurs.

## 3. Le schéma-delta, en tête du plan

Un bloc `mermaid`, jamais rendu dans un terminal. La convention — couleurs, anatomie d'un nœud,
la ligne `+ <incrément>` sur un bloc modifié — vit dans `docs/design/schemas.md` §0 et §6 :
**va la lire avant de dessiner**, ne la recopie pas ici.

Le schéma se lit sur le vocabulaire de `schemas.md` §3 (déclaré vs produit) : dis, pour chaque
bloc touché, s'il déplace la définition ou l'exécution.

**Fait quand** : la table « Objets impactés » a son équivalent visuel — chaque bloc coloré
(ajouté/modifié/supprimé), chaque bloc modifié porte sa ligne `+`, aucun bloc n'est ambigu sur son
registre.

## 4. Découper en pas

Le pas est **entièrement technique** (`tickets.md` §4) : il n'a pas son propre plan, celui-ci a
déjà posé ses frontières. Pour chaque pas, écris seulement ce que le découpage sait et que la
prise du pas ne pourra plus retrouver :

- un titre qui tient en une action ;
- **pourquoi celui-là, à cette place, et où il s'arrête** — la question qui ne se rattrape pas
  (`tickets.md` §4) ; nommer le pas frère plutôt que justifier dans l'absolu ;
- le piège local, s'il y en a un — sinon rien.

N'écris ni test list (elle s'écrit à la prise du pas) ni comment coder chaque pas.

**La maille** : un pas tient dans **une fenêtre de contexte fraîche** — s'il faut deviner ce
qu'un pas précédent a fait pour continuer, il est trop gros. Vérifie aussi le test de
`tickets.md` §1 à l'envers : si un pas était recettable seul par quelqu'un qui ne lit pas le
code, ce n'est pas un pas, c'est un incrément mal découpé.

**Fait quand** : chaque pas a sa raison d'être *à cette place*, l'ordre entre pas est explicite
(dépendance ou indifférence assumée), et aucun pas ne réclame la conversation qui l'a précédé.

## 5. Une découpe non évidente ne se tranche pas seul

Si l'étape 4 hésite entre plusieurs façons radicalement différentes de couper les responsabilités
— pas seulement l'ordre des pas, la **forme** des objets eux-mêmes — ne choisis pas seul.
Lis [`CONCEVOIR-DEUX-FOIS.md`](CONCEVOIR-DEUX-FOIS.md) et lance-le : c'est le cas qui le mérite.
Le cas courant (frontières déjà lisibles) n'y va pas.

## 6. Terminer l'étape

Écris le plan (schéma-delta + table + pas), place la carte en `Plan Review`. C'est `revue-plan`
qui prend le relais — ce skill ne juge pas son propre plan.

**Fait quand** : la carte porte le plan complet et sa colonne dit `Plan Review`.
