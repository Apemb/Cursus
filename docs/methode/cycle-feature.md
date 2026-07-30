> **Écrit avant d'avoir été exécuté** — voir l'avertissement en tête de [`cycle.md`](cycle.md).

# Le cycle d'une feature, colonne par colonne

> **À quoi sert ce fichier.** Il donne, pour chaque colonne d'un **projet** Linear, ce qui s'y
> fait, quel skill le porte, ce qui doit exister en sortie, et quelle étiquette est posée. C'est
> le niveau où l'on décide **quoi construire et si ça vaut le coup**.
>
> **Ce qu'il ne dit pas.** Le vocabulaire des étiquettes et la mécanique de la boucle vivent dans
> [`cycle.md`](cycle.md) — à lire avant celui-ci. Les **gestes** vivent dans les skills, les
> **critères opposables** dans [`dod/feature/`](dod/feature/), et ce que **contient** une feature
> dans [`tickets.md`](tickets.md) §2.
>
> **Le niveau se déduit de la structure** : une feature est un **projet** Linear. Ni étiquette de
> niveau à maintenir, ni convention à faire respecter (`tickets.md` §8).

---

## 1. Le chemin

```
Backlog ──► Discovery ──► Spec ──► In Progress ──► Validation ──► Completed
                 │
            (Canceled)
```

Deux particularités de ce niveau, et elles se tiennent :

- **Aucune colonne de revue.** La revue se joue **dans** `Discovery` et **dans** `Spec`, portée par
  les étiquettes. Le motif est celui de `cycle.md` §1 : ces processus sont encore assez diffus pour
  qu'on veuille déplacer la frontière écriture/revue sans migrer le tableau. À l'incrément, où le
  processus est stabilisé, le choix est inverse.
- **L'humain est dans la production**, pas seulement au bout. Régime *Trio* (`tickets.md` §6.3) :
  un binôme humain ⇄ agent rédige, un agent tiers prononce la **conformité** contre la DoD,
  l'humain prononce la **justesse** en tirant.

⚠️ **La bascule *pas engagé → engagé* tombe à l'entrée en `Spec`, pas en `In Progress`.** Linear
range `Spec`, `In Progress` et `Validation` sous le type `started` — filtrer sur ce type ramène
**trois** colonnes. `Discovery` est `planned`, et c'est le seul endroit où une feature peut encore
mourir sans coût.

---

## 2. `Backlog` — le cap est nommé

Rien ne s'y fait. Le cap est écrit et argumenté, pas encore ordonné dans la trajectoire. Une carte
y attend qu'un humain la tire.

---

## 3. `Discovery` — établir le besoin sans le confondre avec une solution

**Cycle court** : binôme → revue → binôme. Ni correcteur, ni vérificateur, ni `Human Review` — le
motif est en [`cycle.md`](cycle.md) §6, et il tient en une phrase : ce qui manque à une remarque de
Discovery n'est pas de la prose mais de la **matière**, et aucun agent ne va mener l'entretien qui
manque.

| État observé | Skill invoqué | Livrable | État posé |
|---|---|---|---|
| `Discovery` + *aucune* | [`discovery`](../../.claude/skills/discovery/SKILL.md) | Le document `Discovery` publié sur le projet : besoin établi **sans sa solution**, pour qui, pourquoi maintenant, **plusieurs** pistes ouvertes | `Review Requested` |
| `Discovery` + `Review Requested` | `revue-discovery` **(à écrire)** | Les remarques posées sur le projet, ou aucune | `Rework Needed` \| `Done` |
| `Discovery` + `Rework Needed` | [`discovery`](../../.claude/skills/discovery/SKILL.md), **l'humain revient dans la boucle** | Le document repris, et **chaque remarque soldée** par la reprise faite ou le refus motivé | `Review Requested` |
| `Discovery` + `Done` | — | — | l'humain **tire** vers `Spec`, ou vers `Canceled` |

**Ce que le cycle court échange.** Il n'y a pas de vérificateur : c'est la **revue suivante** qui
tient ce rôle, en relisant l'artefact repris. Le binôme solde donc ses propres remarques, ce qui
serait complaisant dans un cycle complet — ici c'est tenable parce qu'un tour de revue de plus
suit toujours, et parce que le relecteur voit le fil entier.

**Les deux axes de `revue-discovery` sont déjà écrits**, dans
[`dod/feature/discovery.md`](dod/feature/discovery.md) : *l'artefact est complet* (§1) et *aucun
arbitrage n'a été rendu* (§2). Le second est le moins intuitif et le plus utile — une piste
présentée avec une raison de ne pas la prendre a déjà été écartée, et la Discovery a mangé la Spec.

**Sortie légitime vers `Canceled`** : *on ne fait pas*, ou *le besoin n'est pas celui-là*. Elle
mérite une phrase disant pourquoi — un abandon non expliqué se re-proposera.

---

## 4. `Spec` — arbitrer, et écrire ce qu'on écarte

**Cycle complet.** C'est ici que les temps ③ et ④ existent : une remarque de spec désigne un manque
**dans l'artefact** — un écart non écrit, une capacité formulée en liste de tâches, une recette
absente — et ça se reprend en relisant l'artefact contre son référentiel.

| État observé | Skill invoqué | Livrable | État posé |
|---|---|---|---|
| `Spec` + *aucune* | [`spec`](../../.claude/skills/spec/SKILL.md) | Le document `Spec` : options **arbitrées** (faisabilité, coût), **écarts écrits**, capacité énoncée à l'indicatif, **recette définie**, socle et pré-requis nommés, trois registres tenus | `Review Requested` |
| `Spec` + `Review Requested` | [`revue-spec`](../../.claude/skills/revue-spec/SKILL.md) | Les remarques posées sur le projet | `Rework Needed` \| `Human Review` si aucune |
| `Spec` + `Rework Needed` | `correction` **(à écrire)** | Le document repris, **une réponse dans chaque fil** disant la reprise faite ou le refus motivé | `Rework Done` |
| `Spec` + `Rework Done` | `verification` **(à écrire)** | Chaque remarque soldée, ou rouverte avec ce qui manque encore | `Rework Needed` \| `Human Review` si `open` vaut 0 (+ `Escalated` si une remarque a atteint son 3ᵉ désaccord) |
| `Spec` + `Human Review` | — *(humain)* | Ses propres remarques posées, ou l'accord | `Rework Needed` \| `Done` |
| `Spec` + `Done` | — | — | l'humain **tire** vers `In Progress` |

⚠️ **La session de revue doit être neuve.** `D-039` a mesuré que relire dans la même session
n'apporte rien : le gain vient de relire **sur l'artefact seul, sans le fil qui l'a produit**, et
il est plus net encore sur les erreurs critiques. Une relecture est donc une session neuve, pas une
itération de plus — c'est une contrainte d'exécution, pas une préférence.

⚠️ **Le piège du binôme.** L'agent qui a co-écrit la spec ne la valide pas — et si on lui demande
un verdict, **il le donnera**. Un faux accord est pire qu'aucune relecture : il donne le sentiment
d'avoir été contredit.

**Pas d'escalade vers l'humain au sens de l'incrément** : il est déjà dans la pièce, au temps ⑤.
`Escalated` reste posée quand une remarque a épuisé ses deux passes — non pour convoquer, mais
pour que le fait se compte.

---

## 5. `In Progress` — le découpage a eu lieu et un incrément a démarré

| État observé | Skill invoqué | Livrable | État posé |
|---|---|---|---|
| `In Progress` + *aucune* | [`decoupage`](../../.claude/skills/decoupage/SKILL.md) | N incréments existent comme cartes rattachées au projet, chacun avec ses **frontières**, son `blockedBy`, et le hors-périmètre **nommant les frères** | `Done` |

**Pas de cycle de revue ici, et ce n'est pas un oubli** : le skill `decoupage` §6 fait trancher
l'humain sur la granularité **avant publication**. Le jugement a lieu dans le geste, pas après.

⚠️ **Deux DoD gouvernent cette transition, et il faut les lire dans l'ordre.**
[`dod/feature/spec.md`](dod/feature/spec.md) dit si la spec est finie ;
[`dod/feature/in-progress.md`](dod/feature/in-progress.md) dit si le découpage a eu lieu **et
qu'au moins un incrément a réellement démarré**. Le second ne peut pas être vérifié avant que le
découpage soit fait — c'est-à-dire après le passage. C'est une friction connue de la forme *tirée*,
et pas un défaut de rédaction : la colonne `In Progress` contient à la fois l'acte de découper et
l'état d'avoir découpé.

Toute clause de la recette de la spec doit atterrir dans **au moins un** incrément. C'est la seule
vérification qui empêche une feature de se terminer en ayant perdu une promesse en route.

---

## 6. `Validation` — recetter contre la spec, item par item

Régime **Œil** : humain, irréductiblement. Aucun skill, et c'est une décision, pas un retard — il
n'existe aucun référentiel opposable pour *est-ce que ça fait vraiment ce qu'on avait promis*.

| État observé | Skill invoqué | Livrable | État posé |
|---|---|---|---|
| `Validation` + *aucune* | — *(humain)* | Le document `Spec` rouvert, **chaque item de la recette rejoué** un par un contre le produit livré, avec un verdict à trois issues jamais une prose libre | `Rework Needed` \| `Done` |

⚠️ **Ce n'est pas redondant avec les `QA Review` déjà passées.** Chaque niveau se recette contre
son **propre** artefact : le pas contre sa test list, l'incrément contre son acceptation, la
feature contre sa **spec**. Toutes les stories peuvent être vertes sans que la capacité promise
soit là — et c'est ce qui fait de la spec un contrat plutôt qu'un document d'intention. Aucune
conclusion ne se tire du fait que les incréments sont `Done`.

**Un seul manquement suffit à reposer la carte.** `Validation` ne fait pas la moyenne.

---

## 7. `Completed` — acter la jambe

| État observé | Skill invoqué | Livrable | État posé |
|---|---|---|---|
| `Completed` | — *(humain)* | Tous les incréments portent `Done`, `trajectoire.md` acte la jambe, un `D-NNN` est écrit si un arbitrage structurant a été tranché | — |

⚠️ **Le numéro d'un `D-NNN` se prend à l'écriture, jamais à la lecture** : relire la fin de
`decisions.md` au moment d'écrire, pas au moment d'avoir décidé. Le journal est **append-only**, et
deux entrées portant le même numéro ne se corrigent pas proprement.

---

## 8. Registre

**Construit** : rien de ce cycle n'a tourné. `Discovery` est occupée par une feature réelle — *Un
agent pilote Cursus* — dont les remarques de revue ont été posées **sur le document**, donc
invisibles (`D-045`) ; elles sont à reposer sur le projet avant que ce cycle puisse servir.

**Tranché mais pas construit** : la totalité de ce fichier. Deux skills du chemin n'existent pas
(`revue-discovery`, et les primitifs `correction` / `verification` que `Spec` réclame), et quatre
étiquettes sur six sont à créer.

**Éprouvé ailleurs, pas ici** : le trio `Discovery` → `Spec` → revue a tourné sur quelques tickets
hors de ce dépôt, sans les étiquettes et sans agent tiers.

**Questions ouvertes** :

- **Le cycle court de `Discovery` suppose qu'un agent tiers sache juger « aucun arbitrage n'a été
  rendu ».** C'est l'axe le plus subtil des treize DoD, et le premier candidat à produire un faux
  succès.
- **Le refacto orphelin n'a pas de spec**, donc pas de recette de niveau feature — il entre
  latéralement au `Backlog` des incréments (`tickets.md` §6.2) et échappe entièrement à ce fichier.
