---
name: revue
description: Primitif de relecture partagé par revue-spec, revue-plan et revue-code — chicaner un artefact contre un référentiel, en axes séparés jamais fondus, sans le réécrire. Ne se déclenche jamais seul : utiliser quand une instance de revue invoque ce protocole (« Suis le protocole du skill `revue` »), avec l'artefact, le référentiel de chaque axe, et le nombre d'axes.
---

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

Un **primitif**, pas une revue en soi : `revue-spec`, `revue-plan` et `revue-code` (`flux.md` §2)
l'invoquent chacune avec son propre artefact et son propre référentiel. Ce fichier fixe ce que les
trois doivent tenir, pour qu'un désaccord ne dépende jamais de laquelle a été prise. Le geste
central est de **chicaner**, pas de juger : contester point par point, chaque objection appuyée
sur une citation, jamais un avis global qui absorbe le détail. Ce que ce protocole ne couvre pas :
l'escalade (régime *Boucle* seulement, `tickets.md` §6.4 — au compteur de tours et à l'assignation
de décider l'instance appelante) et la question de savoir *qui* juge quoi n'est pas ici — c'est
l'axe et son référentiel, fournis par l'instance, qui le disent.

## 1. Recevoir le mandat

Avant toute lecture de l'artefact, exiger de l'appelant : l'artefact à relire, **au moins deux
axes**, et pour chacun un référentiel opposable (un fichier, une clause) ou la mention explicite
qu'il n'y en a pas.

Complet quand : chaque axe reçu porte un référentiel nommé, ou une abstention est déjà actée avant
de commencer (§4).

## 2. Lancer les axes en parallèle, sans jamais les fondre

Faire tourner chaque axe dans son propre sous-agent, en parallèle, sans qu'aucun ne voie le
rapport d'un autre avant de conclure. Un verdict mélangé laisse un axe masquer l'autre : deux
jugements de nature différente — une clause enfreinte, une appréciation sans clause — se prêtent
leur force l'un à l'autre dès qu'ils partagent un paragraphe.

Complet quand : autant de rapports distincts que d'axes, jamais un rapport de synthèse qui
reclasse ou fusionne.

## 3. Citer les deux pièces

Ne consigner une divergence que si elle porte **le référentiel** (fichier et clause précise) **et**
**l'extrait de l'artefact** visé, côte à côte. L'un sans l'autre n'est pas une divergence, c'est
une impression — le garde-fou contre le constat plausible mais faux, préféré ici à la vérification
empirique.

Complet quand : aucune ligne du rapport n'affirme un écart sans ses deux citations.

## 4. S'abstenir plutôt qu'inventer

Si le référentiel d'un axe manque, écrire pour cet axe **« référentiel absent »**, et rien de
plus. Un relecteur qui n'a pas la source produit une abstention, jamais un jugement — c'est le
remède écrit d'avance au relecteur qui, faute de référentiel, invente une exigence qui n'existe
nulle part.

Complet quand : un axe sans référentiel ne contient aucune divergence, seulement la mention
d'abstention.

## 5. Étiqueter la confiance

Faire porter à chaque constat l'une de deux étiquettes, jamais une ambiguïté entre les deux :
**violation dure** (une clause du référentiel est enfreinte, opposable telle quelle) ou
**jugement** (une appréciation sans clause à citer). Un jugement présenté comme une violation dure
emprunte une autorité qu'il n'a pas.

Complet quand : chaque ligne du rapport porte l'une des deux étiquettes, sans exception.

## 6. Lister sur la carte, sans réécrire l'artefact

Ne pas toucher à l'artefact — ni correction, ni reformulation à la place de l'auteur. Posture du
régime *Vérification* (`CLAUDE.md`) : relire contre une source de vérité et rapporter l'écart, pas
réécrire.

Chaque constat retenu se pose **sur la carte**, une remarque par constat :

```bash
cursus linear comment add "<titre du document>" --quote "<le passage visé>" --body -
```

Le passage cité doit figurer dans le document, **et une seule fois** — la commande en calcule le
repère, et refuse une citation qui n'y figure pas comme une citation qui y figure plusieurs fois.
Les blancs, eux, sont tolérés : recopier un passage écrase ses retours à la ligne, et c'est prévu.
`--body -` lit la remarque sur l'entrée standard, ce qui évite d'échapper le markdown.

⚠️ **Jamais sur le document, toujours sur la carte** (`D-045`) : l'API Linear ne sait pas ancrer un
commentaire de document, et réécrire un document désancre tous les siens. Une remarque posée là où
l'auteur va écrire sa reprise est une remarque qui disparaît au moment de servir.

Complet quand : le rapport ne contient aucun texte destiné à remplacer un passage de l'artefact,
et chaque constat retenu existe comme remarque sur la carte.

## 7. Rendre un verdict structuré

Faire tenir la sortie finale sous une forme fixe, comparable d'un tour à l'autre : par axe,
**accord** ou **désaccord**, et si désaccord, **le point en litige** en une phrase. De la prose
libre ne se compare pas — un tour suivant ne peut pas dire s'il a progressé.

Complet quand : le verdict de chaque axe se relit sans reparser un paragraphe narratif.

## 8. Poser l'étiquette, jamais déplacer la carte

Poser `Done` ou `Rework Needed` (groupe *Advancement Labels*, mutuellement exclusives) sur la
carte, sans jamais la déplacer de colonne. Un avis est révocable et sans effet de bord ; déplacer
une carte engage, et ce n'est pas au relecteur de le faire.

Complet quand : la colonne de la carte est inchangée après la revue — seule l'étiquette a bougé.
