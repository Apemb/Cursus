> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

# DoD — incrément (story), sortie de `Plan Review`

> **La question** : cet incrément peut-il être **tiré** en `In Progress`, c'est-à-dire peut-on le
> découper en pas — puis ouvrir les cycles TDD — sans revenir redemander comment c'est structuré ?
>
> **L'artefact relu ici est le plan de design** — l'échelle des **objets** : lesquels naissent,
> changent ou meurent, et quelles responsabilités ils portent (`D-053`). Au-dessus, le plan
> d'architecture de la spec a tenu l'échelle du système ; en dessous, la test list de chaque pas
> tiendra celle du code. Une case ci-dessous qui semble manquer est peut-être une case d'une autre
> échelle.
>
> **Le flux est tiré.** Une DoD n'est pas une condition de sortie que l'amont s'applique à
> lui-même : c'est **ce que l'aval vérifie avant de tirer**. L'aval, ici, est `decoupage-pas` : s'il
> bute sur une décision de structure que le plan n'a pas prise, il **repose la carte en `Planning`**
> avec le manque nommé, plutôt que de trancher lui-même (`decoupage-pas` §6).
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
- [ ] La **maille visée** est dite : l'ordre de grandeur de pas, les frontières qui tombent des
      objets, et l'ordre là où il est contraint — ⚠️ **pas les pas eux-mêmes**, qui naissent à
      l'entrée en `In Progress` (`D-070`). Un plan qui les énumère est en trop, pas en avance
- [ ] Les **pièges connus** sont accrochés à leur **objet**, jamais à un pas — chacun **nomme
      l'objet** dont il est une propriété. Un piège rattaché à un pas qui n'existe pas encore est un
      piège perdu. ⚠️ **L'endroit ne fait pas partie de la clause** : la cellule de la table ou une
      section « objet par objet » servent l'invariant aussi bien, et une table à quatre colonnes
      cesse d'être lisible passé quelques pièges. Ce qui se coche est le **nom de l'objet**, jamais
      la mise en page
- [ ] Le plan vit **au bon endroit** (`CLAUDE.md` §Où vit le plan) : document attaché si une carte
      porte le travail, fichier avec `> Fichier : <chemin>` en tout premier sinon
- [ ] Le plan ne contient **ni test list, ni instructions ligne à ligne** — la conception s'arrête
      à *comment c'est structuré*, jamais à *comment on tape le code* (`tickets.md` §3)

## 2. La boucle a eu lieu et son issue est lisible

- [ ] `revue` a tourné sur les **trois axes** de `revue-plan` — **Conformité** (§1 de ce document,
      clause par clause), **Architecture** (`docs/design/architecture.md`, où seul un écart **tu**
      est opposable), **Découpabilité** (§3 de ce document) — en sous-agents parallèles **jamais
      fusionnés**
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

> **Un plan est fini quand on peut le découper en pas, et ouvrir le premier cycle TDD, sans revenir
> redemander comment c'est structuré.**

Il se **teste**, et deux fois plutôt qu'une : on tente de tracer les frontières entre pas, puis
d'écrire la test list du premier. Si l'un ou l'autre réclame une décision de structure que le plan
n'a pas prise, le manque est dans le plan — pas dans le découpage, pas dans le pas.

## 4. Ce qui n'est *pas* un critère

- **Un accord obtenu au premier tour.** Un litige résolu au tour deux ou trois n'est pas un plan
  moins bon — la boucle a fonctionné comme prévu (`tickets.md` §6.4, `docs/reference/skills.md`
  §5.3 : la zone utile est deux à trois tours).
- **Les pas eux-mêmes, ni leur test list.** Les pas naissent à l'entrée en `In Progress` (`D-070`),
  la test list à la prise de chacun (`tickets.md` §4) — un plan qui les anticipe a mangé les étapes
  suivantes, il ne les a pas mieux préparées.
- **Un écart à `architecture.md`.** Le document décrit ce qui **est**, pas ce qui doit rester : un
  incrément a le droit de faire évoluer l'architecture, et `CLAUDE.md` demande qu'il mette le
  document à jour dans le commit qui le rend nécessaire. Ce qui est opposable, c'est l'écart **tu** —
  une question tranchée rouverte, un invariant contredit ou une frontière de couches déplacée **sans
  le dire**. Un écart nommé et motivé est conforme (`D-049`).
- **L'unanimité sur la forme.** `revue` liste les divergences, elle ne réécrit pas ; un désaccord
  de jugement documenté et assumé peut sortir en `Done` s'il n'est pas une violation dure.

## 5. Sortie latérale

Aucune : un incrément qui bute en `Plan Review` ne s'annule pas ici, il **repose** — soit un
nouveau tour, soit une escalade. `Canceled` reste un geste de niveau feature (`tickets.md` §6.1),
pas d'incrément.
