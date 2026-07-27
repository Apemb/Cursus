> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

# DoD — incrément (story), sortie de `Plan Review`

> **La question** : cet incrément peut-il être **tiré** en `In Progress`, c'est-à-dire ses cycles
> TDD peuvent-ils commencer ?
>
> **Le flux est tiré.** Une DoD n'est pas une condition de sortie que l'amont s'applique à
> lui-même : c'est **ce que l'aval vérifie avant de tirer**. Qui prend le premier pas et bute sur
> un plan insuffisant repose la carte.
>
> **Régime Boucle** (`tickets.md` §6.3), pas Trio : le tiers qui prononce ici est un **agent**, pas
> un humain — l'humain n'entre qu'en **arbitre d'exception**, sur escalade (`tickets.md` §6.4). Le
> *contenu* attendu du plan est en `tickets.md` §3 et `CLAUDE.md` §Méthode de développement ; le
> *dispositif* de la boucle est `.claude/skills/revue-plan/SKILL.md`. Ici : uniquement de quoi il
> faut s'être acquitté pour sortir.

## 1. L'artefact est complet

- [ ] Le plan porte un **schéma-delta** en tête, bloc `mermaid`, conforme à
      `docs/design/schemas.md` §6 (couleurs, anatomie du nœud, la ligne `+ <incrément>` sur chaque
      bloc modifié)
- [ ] La table **« Objets impactés »** couvre tout ce que le schéma colore — rien d'ajouté,
      modifié ou supprimé n'est visible sur le schéma sans l'être dans la table, ni l'inverse
- [ ] Chaque **pas** du découpage répond aux deux questions qui ne se rattrapent pas
      (`tickets.md` §4) : pourquoi celui-là, à cette place, et où il s'arrête — jamais une
      justification dans l'absolu quand un pas frère peut être nommé
- [ ] Le plan vit **au bon endroit** (`CLAUDE.md` §Où vit le plan) : document attaché si une carte
      porte le travail, fichier avec `> Fichier : <chemin>` en tout premier sinon
- [ ] Le plan ne contient **ni test list, ni instructions ligne à ligne** — la conception s'arrête
      à *comment c'est structuré*, jamais à *comment on tape le code* (`tickets.md` §3)

## 2. La boucle a eu lieu et son issue est lisible

- [ ] `revue` a tourné sur les axes de `revue-plan` (le plan contre `architecture.md`, le
      découpage contre la maille, le schéma-delta contre `schemas.md`), en sous-agents parallèles
      **jamais fusionnés**
- [ ] Chaque divergence relevée porte sa **citation** (référentiel + extrait), ou son
      **abstention explicite** si le référentiel manquait
- [ ] Le document de boucle existe si au moins un tour a eu lieu, et chacune de ses entrées est
      **autoportante** — lisible seule, sans remonter l'historique des tours précédents

Deux issues, l'une ferme la colonne, l'autre non :

- [ ] **`Done`** posé par `revue` → la carte reste **non assignée**, elle est tirable
- [ ] **Escalade** → la carte porte une **assignation humaine**, et son dernier tour n'est pas
      encore un accord — elle **n'est pas tirable** tant que l'humain n'a pas tranché et que
      `revue` n'a pas reposé `Done`

## 3. Le critère opposable

> **Un plan est fini quand le premier pas peut ouvrir son cycle TDD sans revenir redemander
> comment c'est structuré.**

Il se **teste** : on tente d'écrire la test list du premier pas. Si elle réclame une décision de
structure que le plan n'a pas prise, le manque est dans le plan — pas dans le pas.

## 4. Ce qui n'est *pas* un critère

- **Un accord obtenu au premier tour.** Un litige résolu au tour deux ou trois n'est pas un plan
  moins bon — la boucle a fonctionné comme prévu (`tickets.md` §6.4, `docs/reference/skills.md`
  §5.3 : la zone utile est deux à trois tours).
- **La test list de chaque pas.** Elle s'écrit à la prise du pas, jamais avant (`tickets.md` §4) —
  un plan qui l'anticipe a mangé l'étape suivante, il ne l'a pas mieux préparée.
- **L'unanimité sur la forme.** `revue` liste les divergences, elle ne réécrit pas ; un désaccord
  de jugement documenté et assumé peut sortir en `Done` s'il n'est pas une violation dure.

## 5. Sortie latérale

Aucune : un incrément qui bute en `Plan Review` ne s'annule pas ici, il **repose** — soit un
nouveau tour, soit une escalade. `Canceled` reste un geste de niveau feature (`tickets.md` §6.1),
pas d'incrément.
