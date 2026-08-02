---
name: decoupage-pas
description: Découper un incrément conçu en pas concrets, et créer leurs sous-tâches, à l'entrée en `In Progress`. À invoquer quand un incrément sort de `Plan Review` portant `Done`, ou quand son plan a été sauté et qu'il attend en `Planning` portant `Done`. Ne pas l'utiliser pour découper une feature en incréments (c'est `decoupage`), ni pour concevoir la structure d'un incrément (c'est `plan-design`), ni pour exécuter un pas (c'est `prendre-un-pas`).
---

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

# Découpage en pas — de l'incrément conçu aux sous-tâches

Ce skill est le **frère de `decoupage`**, une échelle plus bas : là où celui-là coupe une feature en
incréments à l'ouverture de la feature, celui-ci coupe un incrément en pas à l'ouverture de
l'incrément. Même geste, même moment relatif, même interdit — **ordonner n'est pas concevoir**.

**Il ne conçoit pas.** La structure a été décidée en `Planning` par le plan de design, à l'échelle
des objets (`D-053`). Ici, on découpe le chemin qui y mène. Si le découpage réclame une décision de
structure que le plan n'a pas prise, ce n'est pas à toi de la prendre : voir §6.

**Pourquoi maintenant et pas au plan** : ce qu'on apprend au pas 1 change ce qu'on sait au pas 4.
Le plan a visé une maille ; le réel la corrige, et il faut qu'il puisse encore le faire quand les
cartes naissent. Six cartes créées avant qu'une ligne soit écrite sont un *waterfall* à petite
échelle.

## 0. Tirer la carte — c'est toi qui la déplaces

L'incrément t'attend, portant **`Done`**, dans l'une de ces deux colonnes :

- **`Plan Review`** — cas nominal : la revue a rendu son accord et s'est arrêtée là ;
- **`Planning`** — le plan n'était pas dû (`Planning` est conditionnel), et la carte porte une
  phrase disant pourquoi. Elle n'est jamais passée par `Plan Review`, et c'est régulier.

**Tire-la en `In Progress` et retire l'étiquette `Done`** — un seul geste, à la prise. Un ticket
n'est jamais poussé : `Done` n'avance pas une carte, elle **autorise** qu'on l'avance (`cycle.md`
§4).

⚠️ **C'est le seul déplacement d'incrément de ce skill.** Le découpage en pas est le **premier**
travail de `In Progress`, pas ce qui la clôt — exactement comme le découpage d'une feature est le
premier travail de son `In Progress` à elle (`decoupage` §7). Ne repose aucune étiquette sur
l'incrément en sortant : son `Done` viendra quand tous ses pas seront faits.

**Fait quand** : la carte est en `In Progress`, sans étiquette.

## 1. Reprendre la maille visée, sans s'y croire tenu

Le plan de design porte une **intention de maille** — combien de pas environ, et où sont les
frontières que la conception rendait évidentes. C'est un point de départ, **pas un contrat** : même
rapport qu'entre le plan d'architecture d'une spec et le plan de design lui-même (`D-049`), et pour
la même raison — le plan le plus haut est le moins engageant, parce qu'il est le plus loin du
contact.

Tu as donc le droit de couper autrement que ce que la maille visée annonçait. La condition est
celle de `D-049` : **le dire**, dans la carte d'incrément, en une phrase qui nomme l'écart.

**Fait quand** : tu sais ce que le plan visait, et tout écart que tu prends est écrit quelque part.

## 2. Dimensionner sur une fenêtre de contexte fraîche

L'unité opposable, la même qu'à l'échelle du dessus mais mesurée plus court : **un pas tient dans
une fenêtre de contexte fraîche**. S'il faut deviner ce qu'un pas précédent a fait pour continuer
celui-ci, il est trop gros — ou sa frontière est au mauvais endroit.

Et le test à l'envers, celui qui garde la frontière avec le niveau du dessus (`tickets.md` §1) : **si
un pas était recettable seul par quelqu'un qui ne lit pas le code, ce n'est pas un pas** — c'est un
incrément mal découpé, et le signaler vaut mieux que le porter.

**Critère** : pour chaque pas, on peut dire en une phrase ce qu'une session neuve devrait savoir pour
l'exécuter. Si la réponse déborde, scinder.

## 3. Ordonner par les arêtes de blocage

Chaque pas porte son `blockedBy` — les pas qui doivent être `Done` avant qu'il puisse commencer. Un
pas sans blocage ouvert naît en `Todo`, un pas bloqué naît en `Backlog` (`cycle-pas.md` §2–§3). Il
n'y a pas d'ordre total à écrire, seulement des arêtes.

⚠️ **Une dépendance de fichier n'est pas une dépendance de pas.** Deux pas qui touchent le même
fichier ne se bloquent pas l'un l'autre ; ce qui bloque, c'est un pas dont un autre a besoin du
**comportement**. Sur-bloquer sérialise un travail qui pouvait se prendre dans n'importe quel ordre.

**Critère** : chaque pas porte un `blockedBy` explicite, vide ou non — et sa colonne de naissance en
découle mécaniquement, sans arbitrage supplémentaire.

## 4. Écrire dans chaque pas ce qui ne se rattrapera pas

Les trois questions de `tickets.md` §4, dont la deuxième est la seule qui ne se rattrape pas :

1. **Quel est le pas ?** Un titre qui tient en une action.
2. **Pourquoi celui-là, à cette place, et où s'arrête-t-il ?** Tu as, en ce moment précis,
   l'incrément entier en tête et tu vois les frontières entre les pas. Cette vue disparaît avec ta
   session. **Nomme le pas frère** plutôt que de justifier dans l'absolu.
3. **Quel est le piège local ?** S'il y en a un. Sinon, rien — un paragraphe de remplissage coûte
   plus qu'il ne rapporte.

⚠️ **N'écris pas la test list** : elle s'écrit à la prise du pas, et elle est vivante (`tickets.md`
§1.1). Un pas qui arrive avec ses cas déjà posés a mangé l'étape suivante, il ne l'a pas préparée.

**Fait quand** : chaque pas répond aux trois questions, et aucun ne réclame la conversation qui l'a
précédé.

## 5. Relire l'ensemble avant de créer quoi que ce soit

⚠️ **Obligatoire sur tout ensemble de pièces** : un axe de relecture **en lecture seule** qui porte
sur l'**ensemble** du découpage, jamais sur une pièce. Le motif est mesuré : un défaut grave vit
dans l'**intervalle entre deux cartes** — un geste que personne ne porte, une frontière qui laisse
un trou —, donc aucun relecteur d'une pièce isolée ne peut le voir, quel que soit son soin.

Ce que cet axe cherche, et lui seul peut trouver :

- un geste du plan de design qu'**aucun** pas ne porte ;
- un même geste porté par **deux** pas, chacun croyant que l'autre ne le fait pas ;
- un pas dont l'acceptation suppose un comportement qu'un pas **ultérieur** apporte.

Ce n'est **pas une revue** au sens du cycle : la revue est une étape portée par un statut, avec son
aller-retour et son escalade. Ceci est une relecture interne à la production, qui ne déplace rien et
ne pose aucune étiquette.

**Fait quand** : l'ensemble a été relu comme ensemble, et ce qu'il manquait a été corrigé avant que
la moindre carte n'existe.

## 6. Quand le découpage bute sur la structure — reposer, pas décider

Si couper réclame une décision que le plan de design n'a pas prise — quel objet porte telle
responsabilité, où passe telle frontière —, **le manque est dans le plan, pas dans le découpage**.
C'est exactement le critère opposable de `dod/story/plan-review.md` §3, constaté un cran plus tard.

Alors : repose la carte en `Planning`, écris en une phrase la décision qui manque, et arrête-toi.
Trancher toi-même produirait une décision de structure prise par celui qui ordonne — le mélange que
`D-053` sépare précisément.

**Fait quand** : soit rien ne manquait, soit la carte est revenue en `Planning` avec le manque nommé.

## 7. Créer les sous-tâches, et ne toucher à rien d'autre

Chaque pas devient une **sous-tâche** de l'incrément, `blockedBy` posé, née en `Todo` ou en
`Backlog` selon l'étape 3.

⚠️ **Ne pose aucune étiquette sur l'incrément, et ne le déplace plus.** Il est en `In Progress`, tu
l'y as tiré à l'étape 0, et il y reste tant que ses pas courent. C'est `prendre-un-pas` qui prend la
suite, en tirant le premier pas depuis `Todo`.

**Critère** : chaque pas existe comme sous-tâche, et l'incrément est exactement dans l'état où
l'étape 0 l'a mis.

## 8. Ce que ce skill ne fait pas

- **Il ne conçoit pas** — ni objet, ni responsabilité, ni frontière entre couches. Tout cela est
  décidé, ou bien il manque et §6 s'applique.
- **Il n'écrit aucune test list** — elle attend la prise de chaque pas.
- **Il ne relit pas le plan de design** — `revue-plan` l'a fait, avec son référentiel. Constater un
  manque en butant dessus n'est pas relire.
