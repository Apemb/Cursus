---
name: revue-plan
description: Valider la conformité d'un plan de design contre `docs/methode/dod/story/plan-review.md`, en bouclant entre agent de plan et agent de revue jusqu'à accord, et escalader le litige — en s'assignant la carte — après deux ou trois tours sans convergence. Use when un incrément est en Plan Review, quand un incrément attend en Planning portant `Done`, quand on relance une revue de plan après une correction, ou quand une carte de Plan Review porte déjà un tour précédent à reprendre.
---

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

# revue-plan

Régime **Boucle** (`tickets.md` §6.3) : agent de plan ⇄ agent de revue, l'humain n'entre qu'en
arbitre d'exception. Ce skill ne relit pas lui-même — il invoque `revue`, le primitif partagé, et
ajoute ce que la boucle réclame en plus : un **litige** qui se compare d'un tour à l'autre, et une
sortie quand il ne converge pas.

## 0. Tirer la carte — c'est toi qui la déplaces

L'incrément t'attend en **`Planning`, portant `Done`** : `plan-design` a posé son signal et s'est
arrêté là, parce qu'un ticket n'est jamais poussé (`cycle.md` §4). **Tire-la en `Plan Review` et
retire l'étiquette `Done`** — un seul geste, à la prise. La colonne dit *« ça se fait ici »*, donc
elle ne doit basculer qu'au moment où tu commences réellement.

⚠️ **C'est le seul déplacement de carte de ce skill.** Il a lieu à la **prise**, jamais à la sortie :
une fois le verdict rendu, la carte reste en `Plan Review` portant `Done`, et c'est `decoupage-pas`
qui la tirera vers `In Progress` pour y découper les pas (`cycle-increment.md` §5).

**Fait quand** : la carte est en `Plan Review`, sans étiquette.

## 1. Invoquer `revue`, sur ces axes

Fournir l'artefact — le document `Planning` attaché à l'incrément — et exactement **trois** axes.
Deux sont adossés à la DoD, le troisième n'a pas de clause et c'est assumé.

**Axe Conformité** (référentiel : `docs/methode/dod/story/plan-review.md` §1, clause par clause).
Une clause non tenue **et** non déclarée sans objet est une **violation dure**. Les deux qui se
manquent le plus facilement, parce qu'elles portent sur ce qui **n'est pas** dans le document :

- ⚠️ **le plan ne porte pas les pas eux-mêmes**, seulement la **maille visée** — ils naissent à
  l'entrée en `In Progress` (`D-070`) ; un plan qui les énumère est en trop, pas en avance ;
- ⚠️ **les pièges connus sont accrochés à leur objet**, jamais à un pas. Un piège rattaché à un pas
  qui n'existe pas encore est un piège perdu, et c'est précisément ce que la clause protège.

La clause du schéma-delta renvoie à `docs/design/schemas.md` §6 (couleurs, anatomie du nœud, ligne
`+` sur chaque bloc modifié) : **aller la lire**, ne pas juger de mémoire.

**Axe Architecture** (référentiel : `docs/design/architecture.md` — l'état présent, les invariants,
les trois registres). ⚠️ **Un écart n'est pas une divergence ; un écart tu en est une.**
`architecture.md` décrit ce qui est, pas ce qui doit rester : un incrément a le droit de le faire
évoluer, et `CLAUDE.md` demande même qu'il le mette à jour dans le commit qui le rend nécessaire.
Ce qui est opposable, c'est donc le **silence** — un plan qui rouvre une question tranchée, contredit
un invariant listé ou déplace une frontière de couches **sans le dire** ; jamais le fait qu'il le
fasse. Un écart nommé, motivé, et dont la mise à jour du document est prévue, est conforme (`D-049`).
Ce sont par nature des **jugements** : il n'existe pas de clause à citer pour « cet écart n'est pas
justifié ».

**Axe Découpabilité** (référentiel : `docs/methode/dod/story/plan-review.md` §3, le critère
opposable). Ce n'est pas cochable : tenter réellement de tracer les frontières entre pas, puis
d'écrire la test list du premier, et signaler **chaque endroit où il faudrait revenir demander
comment c'est structuré**. Ce que cet axe trouve, c'est ce sur quoi `decoupage-pas` buterait ensuite
— et un manque trouvé ici coûte un tour de revue, là où le même manque trouvé plus tard renvoie la
carte en `Planning` (`decoupage-pas` §6).

`revue` tourne ces axes en sous-agents parallèles, jamais fusionnés, chacun citant le référentiel
et l'extrait, s'abstenant si l'un manque. Il pose `Done` ou `Rework Needed` — il ne déplace jamais
la carte, et ce skill ne le fait plus non plus : son unique déplacement a eu lieu à l'étape 0, en la
tirant. **Pas même sur escalade** — escalader, c'est *assigner* la carte (§4), jamais la bouger.

**Fait quand** : `revue` a rendu son verdict sur les axes retenus.

## 2. Écrire le verdict du tour, comparable au suivant

`Rework Needed` transforme la carte en lieu de dialogue, pas seulement en brief (`tickets.md`
§6.4). Tiens un **second document attaché** à la carte — distinct du plan, un artefact par
document (`D-041`) — avec une entrée par tour :

```
## Tour N
Accord / Désaccord
Axe : <lequel des trois ci-dessus>
Point en litige : <une phrase, autoportante>
```

« Autoportante » est la contrainte qui compte : quelqu'un qui ouvre ce document sans avoir suivi
la boucle doit comprendre le litige **actuel** en lisant la dernière entrée seule, jamais en
remontant l'historique des tours précédents.

**Fait quand** : l'entrée du tour existe, et un lecteur qui n'ouvre qu'elle sait ce qui bloque.

## 3. Compter les tours, sur la carte

Le compteur, c'est le nombre d'entrées `## Tour N` dans ce document — aucun champ Linear à
inventer, `tickets.md` §8 n'en liste aucun pour ça. Avant de relancer un tour, compte les entrées
déjà écrites.

## 4. Boucler ou escalader

- **`Done`**, ou désaccord réel résolu par une révision du plan → un nouveau tour reprend à
  l'étape 1, ou la boucle s'arrête si `revue` a posé `Done`.
- **Deux ou trois tours en `Rework Needed` sans convergence** — le même litige revient, ou pire,
  il dérive vers un autre sujet (`flux.md` §5, question encore ouverte sur cette distinction) →
  **escalade**.

Escalader n'a rien à inventer : **assigne la carte à l'humain**. Non assignée, elle boucle ;
assignée, elle attend un humain (`tickets.md` §6.4, §8). Aucune colonne ne bouge, aucune étiquette
d'avancement ne se pose — l'assignation *est* le signal.

**Fait quand** : soit `Done` est posé et la carte reste non assignée (tirable), soit elle porte
une assignation humaine et son dernier tour est autoportant.
