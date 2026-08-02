> **Écrit avant d'avoir été exécuté** — voir l'avertissement en tête de [`cycle.md`](cycle.md).

# Le cycle d'un incrément, colonne par colonne

> **À quoi sert ce fichier.** Il donne, pour chaque colonne d'une **issue sans parent**, ce qui s'y
> fait, quel skill le porte, ce qui doit exister en sortie, et quelle étiquette est posée. C'est le
> niveau qui **porte la charge** : celui où l'on conçoit *comment c'est structuré*, et le seul dont
> le résultat soit recettable par quelqu'un qui ne lit pas le code.
>
> **Ce qu'il ne dit pas.** Le vocabulaire des étiquettes vit dans [`cycle.md`](cycle.md), les
> **gestes** dans les skills, les **critères opposables** dans [`dod/story/`](dod/story/), et ce que
> **contient** un incrément dans [`tickets.md`](tickets.md) §3. Ce qui se passe **dans** un pas est
> dans [`cycle-pas.md`](cycle-pas.md).
>
> **Le niveau se déduit de la structure** : un incrément est une **issue sans `parentId`**.

---

## 1. Le chemin

```
[Backlog] ──► Todo ──► [Planning ──► Plan Review] ──► In Progress ──► Code Review ──► [QA Review] ──► Done
```

Les crochets marquent le **conditionnel**. Un chemin court n'est pas un chemin bâclé.

**Ici, la frontière écriture/revue est une frontière de colonne** — `Planning` › `Plan Review`,
`In Progress` › `Code Review` — et c'est un choix, pas un héritage. Le processus de développement
est stabilisé au point qu'on veuille le **mesurer** : Linear calcule ses temps de cycle sur les
transitions de statut, donc seule une frontière de colonne produit un *« combien de temps pour
sortir de `Code Review` »* comparable avec celui d'une autre équipe. À la feature, où le processus
est encore diffus, le choix est inverse.

⚠️ **Conséquence directe : `Review Requested` ne sert pas à ce niveau.** Une carte qui arrive dans
une colonne de revue **est** à relire — la colonne porte le signal que l'étiquette porte à la
feature. Les étiquettes en usage ici sont donc : *aucune* (à relire), `Rework Needed`,
`Rework Done`, `Done`, plus `Human Review Requested` et `Escalated` sur escalade.

**L'humain n'intervient pas à ce niveau**, sauf sur `QA Review` et sur escalade. Régime *Boucle*
(`tickets.md` §6.3) : agent ⇄ agent, parce qu'il existe ici quelque chose contre quoi trancher —
le plan contre l'architecture, le code contre le standard.

---

## 2. `Backlog` — une salle d'attente à deux populations

Rien ne s'y fait. Un incrément **éligible** n'y passe pas : il naît en `Todo` au découpage de sa
feature. Ce qui y séjourne est de deux natures, et il faut les distinguer parce qu'elles n'ont pas
les mêmes manques ([`dod/story/backlog.md`](dod/story/backlog.md)) :

| Population | Ce que c'est | Ce qui lui manque |
|---|---|---|
| **A** | Née du découpage, mais un `blockedBy` est encore ouvert | Rien — elle satisfait déjà tout ce que `Todo` exige, elle attend |
| **B** | **L'entrée latérale** : le refacto qu'aucune feature ne tire, la dette autonome, l'incrément déporté d'un découpage | **Une spec**, donc une recette de niveau feature. Le trou est déclaré, pas caché |

La population B est la seule voie par laquelle un travail arrive **sans passer par une spec**.

---

## 3. `Todo` — la colonne d'éligibilité

Rien ne s'y fait non plus : c'est un état, celui d'être prenable.
[`dod/story/todo.md`](dod/story/todo.md) tient en deux exigences — plus aucun `blockedBy` ouvert,
et **le contexte tient dans la carte sans la conversation**.

⚠️ La seconde est la clause qui décide de tout le reste. Un agent arrive **sans avoir eu la
conversation** : les six questions de `tickets.md` §3 doivent être répondues sur la carte,
**comportementalement** — jamais un chemin de fichier, jamais un numéro de ligne. Un renvoi au code
se fait par nom de type.

---

## 4. `Planning` — conditionnel

**On y entre seulement si** le changement crée ou supprime une classe, traverse plusieurs modules,
ou implique une découpe non évidente. Sinon on **saute** directement à `In Progress`, et le dire
vaut mieux que traverser la colonne pour la forme.

| État observé | Skill invoqué | Livrable | État posé |
|---|---|---|---|
| `Planning` + *aucune* | [`plan-design`](../../.claude/skills/plan-design/SKILL.md) | Le plan de design, avec son **schéma-delta** `mermaid` en tête, la table « Objets impactés », et le **découpage en pas** | `Done` |
| `Planning` + `Done` | — | — | **`revue-plan` tire** vers `Plan Review` |

⚠️ **Qui tire dans cette colonne** : celui qui prend l'incrément, depuis `Todo`. Et `plan-design`,
son travail fini, **pose `Done` et s'arrête** — il ne déplace pas la carte en `Plan Review`, pas
plus qu'il ne la saute en `In Progress` quand le plan n'est pas dû. Poser le signal est tout ce
qu'on attend de lui (`cycle.md` §4).

**Où vit le plan** : dans le **document attaché** à la carte, écrit en `Planning`. Linear rend le
`mermaid` nativement, donc le schéma se lit sur la carte — sans fichier intermédiaire à créer puis
à nettoyer, et la revue a lieu au même endroit que le reste.

**Le plan s'écrit ici, pas au découpage.** Le découpage capture ce que lui seul peut savoir — les
frontières entre incréments, vues d'en haut. La conception de chacun attend sa prise : ce qu'on
apprend en faisant le premier change ce qu'on sait au quatrième. Même raison que pour la test list.

**Le plan ne contient ni test list, ni instructions ligne à ligne.** La conception s'arrête où
commence la preuve.

---

## 5. `Plan Review` — la boucle sur le plan

| État observé | Skill invoqué | Livrable | État posé |
|---|---|---|---|
| `Plan Review` + *aucune* | [`revue-plan`](../../.claude/skills/revue-plan/SKILL.md) | Les remarques posées **sur l'issue**, chacune citant son référentiel et l'extrait visé | `Rework Needed` \| `Done` si aucune |
| `Plan Review` + `Rework Needed` | `correction` **(à écrire)** | Le plan repris, **une réponse dans chaque fil** disant la reprise faite ou le refus motivé | `Rework Done` |
| `Plan Review` + `Rework Done` | `verification` **(à écrire)** | Chaque remarque soldée, ou rouverte avec ce qui manque encore | `Rework Needed` \| `Done` si `open` vaut 0 \| `Human Review Requested` + `Escalated` |
| `Plan Review` + `Human Review Requested` | — *(humain)* | Le litige tranché, et sa suite écrite dans le fil | `Rework Needed` \| `Done` |
| `Plan Review` + `Done` | — | — | qui prend le premier pas **tire** vers `In Progress` |

⚠️ **La remarque se pose sur l'issue, jamais sur le document.** `D-045` l'a établi par la mesure :
un commentaire de document ne peut pas être ancré par l'API, il est donc **invisible** dans
l'interface. Ce qui situe une remarque est un **repère calculé** — titre du document, puis section
— que l'appelant ne fournit pas, et ne peut donc ni oublier ni falsifier :

```bash
cursus linear comment add CUR-45 -q "<le passage cité>" -b "<la remarque>"
cursus linear comment list CUR-45 --unresolved
cursus linear comment resolve <id> -w "<la reprise faite, ou le refus et sa raison>"
```

⚠️ **Session neuve obligatoire** (`D-039`) : relire dans la session qui a produit le plan
n'apporte rien. Le gain vient de relire **sur l'artefact seul, sans le fil qui l'a produit**.

**L'escalade est un fait, pas une alarme.** Après deux passes correction/vérification sans
convergence sur une remarque, elle cesse de circuler ; quand plus aucune ne peut avancer, la carte
passe en `Human Review Requested` + `Escalated`. Compter ces occurrences par colonne dit où la boucle
agentique ne tient pas — et la conclusion peut être que le skill de revue est à refaire.

---

## 6. `In Progress` — la série de cycles TDD

L'incrément ne fait rien lui-même : il **délègue à ses pas**, un par un. Ce qui se passe dans
chacun est dans [`cycle-pas.md`](cycle-pas.md).

| État observé | Skill invoqué | Livrable | État posé |
|---|---|---|---|
| `In Progress` + *aucune* | [`prendre-un-pas`](../../.claude/skills/prendre-un-pas/SKILL.md), **une fois par pas** | Chaque pas mené jusqu'à sa colonne `Done` : sa `Code Review` passée à l'échelle de la fonction, puis son commit arrivé en squash dans `story/`, suite verte, **0 warning** | `Done` quand tous les pas sont en `Done` |
| `In Progress` + `Done` | — | — | **`revue-code` tire** vers `Code Review`, à l'échelle du module |

**La documentation se met à jour au fil, pas à la fin.** `architecture.md` dès qu'un type
structurant bouge ou qu'une frontière entre couches change ; `decisions.md` dès qu'une décision
structurante est prise ou renversée. Reporter à `Code Review` ce qui se documente au fil produit
une reconstitution de mémoire, jamais le *pourquoi* qui avait cours au moment du choix.

**La branche** : `story/<identifiant>`, dans laquelle chaque `pas/` arrive **en squash** — voir
`flux.md` §6 et `D-042`. Sur une branche de pas, commiter librement ; le squash produit le commit
propre, et son corps se **réécrit à la main** (GitHub y colle par défaut la concaténation des
messages de WIP, et c'est ce commit-là qui reste dans l'histoire).

---

## 7. `Code Review` — relire un comportement, pas un commit

| État observé | Skill invoqué | Livrable | État posé |
|---|---|---|---|
| `Code Review` + *aucune* | [`revue-code`](../../.claude/skills/revue-code/SKILL.md) | Les remarques posées sur l'issue, sur **les deux axes**, chacune citant référentiel et extrait | `Rework Needed` \| `Done` si aucune |
| `Code Review` + `Rework Needed` | `correction` **(à écrire)** | Le code repris, une réponse dans chaque fil | `Rework Done` |
| `Code Review` + `Rework Done` | `verification` **(à écrire)** | Chaque remarque soldée ou rouverte ; `dotnet build` **0 warning** et `dotnet test` vert **sur ce diff précisément** | `Rework Needed` \| `Done` si `open` vaut 0 \| `Human Review Requested` + `Escalated` |
| `Code Review` + `Human Review Requested` | — *(humain)* | Le litige tranché, sa suite écrite | `Rework Needed` \| `Done` |
| `Code Review` + `Done` | — | — | tiré vers `QA Review` **ou** `Done` — voir §8 |

**On relit un comportement complet, jamais un commit isolé.** Le diff se relit **d'un bloc** contre
un point fixe identifié explicitement (base de la story pour une PR `pas/`, base de la feature pour
une PR `story/`). C'est aussi ici que la **test list et la formulation des comportements se
raffinent** : un titre de test qui ne suit pas `étant donné / quand / alors`, ou qui décrit mal ce
qu'il vérifie, se corrige ; un comportement observable dans le diff sans test qui le nomme
s'ajoute.

⚠️ **Aucun test désactivé ou marqué à revoir n'a été laissé pour faire passer la suite.** C'est le
seul critère de cette colonne qu'un vérificateur complaisant peut valider sans regarder.

---

## 8. `QA Review` — conditionnel, et le dire vaut mieux que le traverser

Régime **Œil** : humain, irréductiblement — la couche présentation est hors du périmètre testé
(`architecture.md` §7.12).

**Le test décisif** : le diff touche-t-il un fichier de la couche présentation ? Si oui, `QA Review`
est **obligatoire**. Sinon elle est **sautée**, et la carte porte une ligne explicite —
`QA Review : requise` ou `QA Review : sautée`, avec sa raison en une phrase. C'est la personne qui
s'apprête à tirer la carte au-delà de `Code Review` qui décide.

| État observé | Skill invoqué | Livrable | État posé |
|---|---|---|---|
| `QA Review` + *aucune* | — *(humain, s'appuyant sur la skill `run`)* | **L'app lancée pour de vrai**, le comportement que l'incrément promettait rejoué à la main, et la **preuve négative** vérifiée si l'acceptation en portait une | `Rework Needed` \| `Done` |

**Le parcours est rejoué, pas relu.** On ne relit pas le diff ici — c'est fait en `Code Review` — et
on ne teste pas des cas que l'incrément ne promettait pas.

---

## 9. `Done` — l'acceptation cochée case par case

| État observé | Skill invoqué | Livrable | État posé |
|---|---|---|---|
| `Done` | — | Chaque case de l'acceptation cochée **individuellement**, la validation manuelle faite si elle était due | — |

⚠️ **Aucune case ne se coche par déduction depuis le vert de la suite.** Une case d'acceptation
décrit un comportement observable ; le vert prouve que les tests écrits passent, pas que le
comportement promis est là.

---

## 10. Registre

**Construit** : rien de ce cycle n'a tourné. `D-042` a tranché la cascade de branches
`pas/` → `story/` → `feature/`, qui **n'a pas encore été exercée une seule fois**. Les gestes de
remarque sont construits et éprouvés contre le vrai Linear, sur une issue comme sur un projet
(`D-046`).

**Tranché mais pas construit** : la totalité de ce fichier. Les primitifs `correction` et
`verification` n'existent pas, et les skills [`revue-plan`](../../.claude/skills/revue-plan/SKILL.md)
et [`revue-code`](../../.claude/skills/revue-code/SKILL.md) sont en **draft écrit d'avance** — ils
prescrivent encore le geste que `D-045` a supprimé et le compteur de tours textuel que `D-045` §7 a
rendu inutile. Ils sont à reprendre **avant** de servir.

**Questions ouvertes** :

- **Quelle colonne exactement porte l'éligibilité au déclenchement automatique** — `Todo` seul, ou
  `Todo` plus une étiquette (`CUR-5`).
- **Une reprise après échec d'un gate automatisé** renvoie-t-elle la carte en `Todo`, ou la
  laisse-t-elle en `In Progress` marquée ? Question distincte de la boucle de revue, qui elle reste
  dans sa colonne.
- **La maille d'un pas n'est pas connue.** Tant qu'aucun pas n'a été exécuté par un agent, on ne
  sait pas quelle taille est bonne — or c'est précisément ce que le découpage décide en amont.
