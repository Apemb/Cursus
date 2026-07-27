---
name: revue-plan
description: Boucle la relecture d'un plan d'archi entre agent de plan et agent de revue jusqu'à accord, et escalade le litige — en s'assignant la carte — après deux ou trois tours sans convergence. Use when un incrément est en Plan Review, quand on relance une revue de plan après une correction, ou quand une carte de Plan Review porte déjà un tour précédent à reprendre.
---

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

# revue-plan

Régime **Boucle** (`tickets.md` §6.3) : agent de plan ⇄ agent de revue, l'humain n'entre qu'en
arbitre d'exception. Ce skill ne relit pas lui-même — il invoque `revue`, le primitif partagé, et
ajoute ce que la boucle réclame en plus : un **litige** qui se compare d'un tour à l'autre, et une
sortie quand il ne converge pas.

## 1. Invoquer `revue`, sur ces axes

Deux ou trois, pas plus — au-delà, aucun ne reçoit assez d'attention :

- **Le plan contre `architecture.md`** — l'état présent, les invariants, les trois registres
  (construit / tranché non construit / question ouverte). Un plan qui rouvre une question déjà
  tranchée, ou ignore un invariant listé, est une divergence ici.
- **Le découpage en pas contre la maille** — chaque pas tient-il dans une fenêtre de contexte
  fraîche (`plan-archi` §4), répond-il à « pourquoi celui-là, à cette place » sans renvoyer à une
  conversation absente ?
- **Le schéma-delta contre `schemas.md` §6** — couleurs correctes, anatomie du nœud respectée, la
  ligne `+` présente sur chaque bloc modifié.

`revue` tourne ces axes en sous-agents parallèles, jamais fusionnés, chacun citant le référentiel
et l'extrait, s'abstenant si l'un manque. Il pose `Done` ou `Rework Needed` — il ne déplace jamais
la carte, et ce skill ne le fait pas non plus tant qu'aucune escalade n'a eu lieu.

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
