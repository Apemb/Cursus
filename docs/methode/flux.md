# Le flux de développement, et les skills qui le portent

> **À quoi sert ce fichier.** Il donne la vue **étape → skill** : le chemin complet d'un besoin
> jusqu'à son code, qui agit à chaque étape, et quel skill porte la méthode de cette étape.
> Il ne dit pas *ce que contient* un ticket — ça, c'est `tickets.md` — ni *ce qu'on construit* —
> ça, c'est `trajectoire.md`. Il ne dit pas non plus **comment on parcourt** une étape — l'état
> observé, le skill invoqué, l'étiquette posée en sortie : ça, c'est [`cycle.md`](cycle.md) et les
> trois documents de niveau qu'il sert ([feature](cycle-feature.md), [incrément](cycle-increment.md),
> [pas](cycle-pas.md)), depuis `D-047`.
>
> **Pourquoi il existe.** Les skills sont auto-découvrables, et un agent n'a pas besoin de cette
> table pour trouver le sien. Le lecteur **humain**, lui, en a besoin : sans elle, le flux
> n'existe nulle part en entier — il est éparpillé en autant de fichiers que d'étapes, et
> personne ne peut répondre à « que se passe-t-il après `Plan Review` ? » sans les ouvrir tous.
>
> **Son autre usage** : c'est la liste de ce qui reste à écrire. Une étape sans skill est une
> étape encore manuelle, et le tableau §4 le dit sans détour.

---

> ## ⚠️ Ce fichier est en retard sur les documents de cycle, et le réalignement est **différé**
>
> Depuis `D-047`, les quatre documents de cycle sont la source de vérité sur *comment on parcourt
> une étape*. Ce fichier a été écrit avant eux et n'a pas suivi. Trois écarts connus, au
> 2026-07-31 :
>
> - **la table maîtresse §2 ignore `revue-discovery`**, qui existe et a tourné trois fois ;
> - **elle range `Discovery` et `Spec` en régime *Binôme***, alors que `tickets.md` §6.3 les met
>   tous deux en ***Trio*** — le binôme rédige, un agent tiers prononce la conformité, l'humain
>   engage en tirant. La nuance n'est pas cosmétique : elle dit qui relit ;
> - **le diagramme ne montre pas la revue de discovery**, là où il annote bien celle de la spec.
>
> **Le réalignement est volontairement remis à après le premier cycle complet** feature → incrément
> → pas. Le motif est qu'on ne sait pas encore *jusqu'où* ce fichier doit aller : s'il redit ce que
> les quatre documents de cycle disent, il divergera d'eux avant un mois — et c'est précisément ce
> que `cycle.md` évite en existant. Ce qu'il doit garder est la vue **étape → skill** pour un
> lecteur humain ; ce qu'il doit abandonner à `cycle.md` n'est pas tranché.
>
> **En attendant, en cas de désaccord, les documents de cycle priment sur celui-ci.**

---

## 1. Trois lieux, parce que trois choses varient indépendamment

| Ce qui varie | Où ça vit | Pourquoi là |
|---|---|---|
| **La méthode** | Un **skill** | Elle diffère par équipe, par composition, par maturité. Une équipe avec deux PM et un designer remplace ses skills et ne touche à rien d'autre |
| **La chorégraphie** | Le **workflow** Cursus | Quelles étapes, dans quel ordre, routées sur quoi. C'est ce que le noyau déterministe sait déjà faire |
| **Le contexte** | La **carte** | Ce travail-ci, et lui seul |

**Conséquence pour Cursus** : le moteur ne sait pas ce qu'est un découpage, une spec ou une
test list — exactement comme il ne sait pas ce qu'est un agent. La méthode est de la **donnée
du projet**, jamais du code du produit. Le prompt d'un `AgentStep` n'est donc pas un brief mais
un **pointeur** : quel skill, quelle carte.

**Conséquence pour le contexte** : il n'a pas à être recopié. Les invariants du dépôt (0
warning, régime TDD, frontière testé/non-testé §7.12, règle no-nullable) sont dans `CLAUDE.md`,
que Claude Code charge de lui-même ; la méthode d'équipe est dans le skill ; la carte ne porte
que ce qui lui est propre. Trois lieux, aucune redondance.

**Un contre-point utile** : *Symphony* (OpenAI) fait le même pari du tableau-control-plane mais
**remplit la seule case chorégraphie** — un niveau de travail unique, aucun découpage, un
gabarit de prompt par dépôt, zéro skill. Il ne supprime pas le besoin de méthode, il le laisse
à la charge de l'équipe, et déplace donc le goulot sur la qualité du ticket humain. Ce que ces
neuf skills portent est exactement ce qu'il laisse vide. Voir `docs/reference/symphony.md`.

---

## 2. Le flux complet

```
FEATURE   Backlog ──► Discovery ──► Spec ──► In Progress ──► Validation ──► Completed
                          │           │           │
                     (Canceled)   revue spec   découpage
                                                  │
INCRÉMENT     (Backlog) ────────────────────────► Todo
              entrée latérale                      │
              — sans spec —                        │
                                    ┌──────────────┘
                                    ▼
                          [Planning ──► Plan Review] ──► In Progress ──► Code Review ──► [QA Review] ──► Done
                                                              │                │
                                                      découpage en pas         │
                                                              │                │
PAS                                                          Todo ──► In Progress ──► Done
```

⚠️ **Les deux découpages n'ont plus qu'une sortie** (`D-072`) : une carte née d'un découpage naît en
`Todo`, bloquée ou non. `Backlog` ne reçoit plus que l'**entrée latérale** — ce qui arrive sans
spec —, et disparaît complètement au niveau du pas, qui a toujours un parent.

### La table maîtresse

| # | Étape | Niveau | Qui agit | Régime | Skill |
|---|---|---|---|---|---|
| 1 | **Discovery** | Feature | Humain + agent | Binôme | `discovery` |
| 2 | **Spec** | Feature | Humain + agent | Binôme | `spec` |
| 3 | **Revue de spec** | Feature | Agent tiers | Validation | `revue-spec` |
| 4 | **Découpage** | Feature → incréments | Agent | Production | `decoupage` |
| 5 | **Planning** | Incrément | Agent | Production | `plan-design` |
| 6 | **Plan Review** | Incrément | Agent ⇄ agent | Boucle + escalade | `revue-plan` |
| 7 | **Découpage en pas** | Incrément → pas | Agent | Production | `decoupage-pas` |
| 8 | **In Progress** | Pas | Agent | Production | `prendre-un-pas` |
| 9 | **Code Review** — la **fonction** | Pas | Agent ⇄ agent | Boucle + escalade | `revue-code` |
| 10 | **Code Review** — le **module** | Incrément | Agent ⇄ agent | Boucle + escalade | `revue-code` |
| 11 | **QA Review** | Incrément | Humain | Œil | — *(s'appuie sur la skill `run`)* |
| 12 | **Validation** | Feature | Humain | Œil | — |

Les étapes **11** et **12** n'ont pas de skill **par décision, pas par retard** : ce sont les
deux jugements sans référentiel opposable (`tickets.md` §6.3). Un agent qui les porterait
rendrait un verdict qu'il n'a pas les moyens de fonder.

**Les étapes 4 et 7 sont le même geste à deux échelles** — couper, et déposer des frontières avant
qu'elles ne disparaissent avec la session qui les a vues. L'une ouvre le `In Progress` d'une
feature, l'autre celui d'un incrément ; ni l'une ni l'autre ne conçoit (`D-070`).

**Les étapes 9 et 10 sont la même colonne à deux échelles**, et le même skill les sert contre deux
référentiels distincts : au **pas**, ce que prouve un test, sa formulation, le nommage
(`dod/pas/code-review.md`) ; à l'**incrément**, la découpe en classes, le design, la cohérence de
l'ensemble (`dod/story/code-review.md`) — et l'incrément seul a le droit de réclamer des **pas
supplémentaires**. ⚠️ La grande échelle ne rattrape pas la fine : un relecteur qui parcourt le diff
de plusieurs pas ne relit pas chaque nom de variable.

### Ce que chaque étape reçoit et produit

⚠️ **Cette table se lit du point de vue du tireur.** Aucune étape n'avance sa propre carte : elle
pose son signal et s'arrête, et c'est l'aval nommé ci-dessous qui tire — en retirant l'étiquette
(`cycle.md` §4).

| # | Entrée | Sortie | Ce que l'étape pose, et qui tire ensuite |
|---|---|---|---|
| 1 | Un cap nommé | Un **besoin** établi, des pistes ouvertes, **aucun choix** | pose `Done` ; **l'humain tire** vers `Spec`, ou vers `Canceled` |
| 2 | Un besoin | Une **spec** : options arbitrées, capacité énoncée, **recette définie** | pose `Review Requested` ; le relecteur tire à sa prise |
| 3 | Une spec | Un verdict, ou des divergences à reprendre en 2 | pose `Rework Needed`, ou `Human Review Requested` si aucune remarque — **jamais un déplacement** ; après accord, **l'humain tire** vers `In Progress` |
| 4 | Une spec validée | N **incréments** avec leurs **frontières** et leur ordre | ne pose **rien** sur la feature : elle reste en `In Progress` tant que ses incréments courent ; les incréments naissent tous en `Todo`, bloqués ou non |
| 5 | Un incrément éligible | Un **plan de design** avec son schéma-delta, et la **maille visée** | pose `Done` ; **`revue-plan` tire** vers `Plan Review` à sa prise |
| 6 | Un plan | Un accord, ou un litige | pose `Done`, ou **assigne l'humain** ; **`decoupage-pas` tire** vers `In Progress` |
| 7 | Un incrément conçu | N **pas** avec leurs frontières et leurs arêtes de blocage | ne pose **rien** sur l'incrément : il reste en `In Progress` tant que ses pas courent ; les pas naissent tous en `Todo`, bloqués ou non |
| 8 | Un pas | Une **test list**, des cycles TDD, un commit | pose `Done` ; **`revue-code` tire** vers `Code Review` à sa prise |
| 9 | Le diff d'un pas | Un accord sur la **fonction**, ou un litige | pose `Done`, ou **assigne** ; **la fusion** de `pas/` dans `story/` bascule le pas en `Done` |
| 10 | Un comportement complet | Un accord sur le **module**, ou un litige | pose `Done`, ou **assigne** ; **qui recette tire** vers `QA Review` — ou vers `Done` si elle se saute |
| 11 | L'app lancée | Le parcours refait à la main | pose `Done` ; colonne **terminale** |
| 12 | La feature entière | Recettée **contre sa spec** | pose `Done` ; `Completed` est **terminal** |

**Quand une feature devient `Done`** : quand tous ses incréments sont faits, validés et **fusionnés
dans sa branche `feature/`**, et que la feature entière devient recettable d'un bloc. Pas à la fin
du découpage — à ce moment-là rien n'est construit, et une feature « tirable » vers `Validation`
sans code serait un mensonge. Quand la feature n'a pas de branche propre (elle ne s'impose pas,
`D-042`), c'est l'arrivée du dernier incrément dans `main` qui vaut fusion.

**Les colonnes terminales sont la seule exception au flux tiré** : `Done` pour un pas ou un
incrément, `Completed` pour une feature. Aucun travail ne commence après elles, donc rien ne peut
les atteindre en tirant — celui qui pose le dernier `Done` y déplace la carte (`cycle.md` §4).

**Conception et ordonnancement alternent, et ne se mélangent jamais** (`D-070`). Les étapes 4 et 7
ordonnancent — elles coupent et déposent des frontières, sans rien concevoir. Les étapes 2, 5 et 8
conçoivent, à trois échelles de plus en plus fines. Chaque conception s'écrit **à la prise** de ce
qu'elle conçoit, et chaque découpage **à l'ouverture** de ce qu'il coupe : ce qu'on apprend en
faisant le premier incrément change ce qu'on sait au quatrième, et ce qu'on apprend au pas 1 change
ce qu'on sait au pas 4.

**L'escalade** (étapes 6, 9 et 10) : après deux ou trois tours sans convergence, **la carte
s'assigne à l'humain**. Non assignée, elle boucle. Voir `tickets.md` §6.4 pour les trois
exigences qui rendent une escalade utilisable.

---

## 3. Où vivent les skills

Dans **`.claude/skills/<nom>/SKILL.md`** du dépôt de travail. Claude Code les charge de
lui-même ; ils se versionnent avec le code, se relisent en revue, et divergent naturellement
d'un dépôt à l'autre. Cursus n'a rien à distribuer, stocker ni modéliser.

**Ce flux est Claude Code exclusif, et c'est assumé** : le contenu d'un skill est du markdown
portable, son mécanisme de chargement ne l'est pas. C'est un choix de l'utilisateur de Cursus,
pas une contrainte du produit — l'utilisateur est garant du fait que ses workflows ont du sens
sur son harnais.

**Comment on les écrit** : `docs/reference/skills.md` — état de l'art sondé le 25 juillet 2026 sur
l'anatomie d'un skill, sa validation et son entretien, avec chaque affirmation étiquetée *mesuré*
/ *documenté* / *folklore*. Quatre de ses constats contraignent l'architecture avant la rédaction
(§1 de cette note) : la fiabilité par étape se compose en `pass^k`, `--bare` va cesser de charger
les skills automatiquement, le faux succès est le mode de défaillance dominant, et un skill
personnel écrase silencieusement son homonyme du dépôt.

---

## 4. Les skills à écrire

| Skill | Étape | Ce qu'il porte | État |
|---|---|---|---|
| `discovery` | 1 | Établir le besoin sans le confondre avec une solution ; ouvrir des pistes sans arbitrer ; savoir conclure *on ne fait pas* | [**draft**](../../.claude/skills/discovery/SKILL.md) |
| `spec` | 2 | Arbitrer les options avec faisabilité et coût, **écrire les écarts**, énoncer la capacité, définir la recette | [**draft**](../../.claude/skills/spec/SKILL.md) |
| `revue-spec` | 3 | Valider une spec qu'on n'a pas co-écrite. Lister les divergences, **ne pas réécrire** | [**draft**](../../.claude/skills/revue-spec/SKILL.md) |
| `decoupage` | 4 | Produire les incréments et leurs **frontières** ; déposer dans chacun le hors-périmètre **en nommant les frères** | [**draft**](../../.claude/skills/decoupage/SKILL.md) |
| `plan-design` | 5 | Le plan gaté de `CLAUDE.md` : schéma-delta, blocs touchés, **maille visée** | [**draft**](../../.claude/skills/plan-design/SKILL.md) |
| `revue-plan` | 6 | La boucle : verdict structuré, compteur de tours, escalade par assignation | [**draft**](../../.claude/skills/revue-plan/SKILL.md) |
| `decoupage-pas` | 7 | Produire les pas et leurs arêtes ; relire l'**ensemble** avant de créer la moindre carte ; reposer plutôt que trancher ce que le plan a laissé ouvert | [**draft**](../../.claude/skills/decoupage-pas/SKILL.md) |
| `prendre-un-pas` | 8 | Test list, cycles TDD (rouge observé *pour la bonne raison*), commit argumenté | [**draft**](../../.claude/skills/prendre-un-pas/SKILL.md) |
| `revue-code` | 9 | Relire un **comportement**, pas un commit ; raffiner la test list et la formulation des comportements | [**draft**](../../.claude/skills/revue-code/SKILL.md) |

Une ligne sans lien est une étape encore manuelle — c'est l'unique état d'avancement que ce
document a le droit de porter. **`draft` n'est pas `écrit`** : le fichier existe et se charge, mais
il a été rédigé d'après l'état de l'art au lieu d'être récolté sur une exécution réelle, contre ce
que prescrit la sous-section suivante. Chaque draft porte cet aveu en tête. Le premier usage réel
promeut ou corrige, et c'est le journal des frictions qui tranche — pas le fichier.

Deux **primitifs** s'ajoutent aux neuf, parce que trois skills réinventaient le même geste :
[`interrogatoire`](../../.claude/skills/interrogatoire/SKILL.md) porte l'entretien — les *faits* sont à la
charge de l'agent, les *décisions* reviennent à l'humain, une question à la fois — et
[`revue`](../../.claude/skills/revue/SKILL.md) porte la mécanique commune aux trois relectures :
**au moins deux** axes jamais fondus — `revue-spec` en fournit deux, `revue-plan` trois —, citation
obligatoire du référentiel et de l'extrait, abstention explicite quand le référentiel manque. Les autres les invoquent au lieu de les recopier.

**Ordre** : `prendre-un-pas` d'abord. C'est le plus petit périmètre, l'erreur y coûte un commit,
il ne dépend d'aucun autre — on peut lui tendre un pas écrit à la main — et il rend tout de
suite le signal qui manque : *une carte de pas contient-elle assez pour qu'un agent travaille
sans avoir eu la conversation ?* `decoupage-pas` est plus tentant et c'est le mauvais premier pas :
tant qu'aucun pas n'a été exécuté par un agent, on ne sait pas quelle **maille** de pas est
bonne — or c'est précisément ce qu'il décide.

### Comment on les écrit — la ligne de base d'abord (`D-039`)

**On n'écrit pas un skill puis on l'éprouve.** On exécute la tâche **sans** skill, on tient un
journal des frictions, et le journal écrit le skill. Trois étapes :

1. **Exécuter** l'incrément à la main, selon la méthode que le skill devra porter.
2. **Journaliser les frictions** au fil de l'eau — chaque correction, chaque étape sautée, chaque
   précision demandée qui aurait dû être sur la carte. Une ligne brute par occurrence.
3. **Écrire**, à partir du journal et de rien d'autre, une fois deux ou trois passages observés.

La raison n'est pas la prudence, c'est l'évaluabilité : un skill écrit d'avance se teste sur des
cas imaginés, où l'exécution avec et sans skill marque pareil — le signal est nul. Un cas tiré
d'un échec vécu discrimine par construction. C'est la règle du dépôt appliquée à la méthode :
**pas de production sans un rouge observé qui la réclame.**

Le terrain retenu pour la première récolte est l'incrément **`2·2c`** — le dogfooding *est* la
ligne de base, donc le produit avance pendant qu'on récolte.

Le matériel de référence — anatomie, budgets, patrons, validation, et les quatre faits qui
contraignent l'architecture avant la rédaction — vit dans `docs/reference/skills.md`.

---

## 5. Registre

**Construit** : rien d'**éprouvé**. Les onze skills existent en **draft** (les neuf étapes, plus
les primitifs `interrogatoire` et `revue`) et les douze DoD sont écrites — mais aucun n'a servi sur un
travail réel, donc tout cela est du *tranché non validé*, pas du construit. Aucune étape n'est
automatisée.

**Tranché mais pas construit** : le flux et ses régimes (`D-036`), les trois lieux, l'escalade
par assignation, le rangement des skills dans `.claude/skills/`, la méthode d'écriture des
skills (`D-039`).

**Tranché ailleurs, pas ici** : le trio de la spec (étapes 1–3) a tourné sur quelques tickets
hors de ce dépôt.

**Refermé** : *distinguer trois tours sur le même litige de trois tours qui dérivent*. La
question était mal posée — ce qui compte n'est pas le tour mais le **contexte**. Relire dans la
même session n'apporte rien ; relire dans une session neuve, **sur l'artefact seul, sans le
prompt qui l'a produit**, gagne nettement, et davantage encore sur les erreurs critiques
(`D-039`). Une relecture est donc une session neuve, pas une itération de plus.

**Questions ouvertes** :

- La répartition du contexte entre `CLAUDE.md`, le skill et la carte est une **hypothèse**, à
  confirmer au premier round-trip réel (`tickets.md` §7). Elle est fragilisée par un fait
  documenté : en mode `--bare`, appelé à devenir le défaut de `claude -p`, **`CLAUDE.md` n'est
  pas chargé** (`docs/reference/skills.md` §1.2).
- Le refacto orphelin entre par le `Backlog` des issues, mais **sans spec** — donc sans recette
  de niveau feature. Aucune étape de ce flux ne le couvre, et `D-072` **isole ce trou** en faisant
  de cette population la seule que la colonne accueille. Un précédent existe : le skill
  `to-tickets` de Matt Pocock traite le refacto large comme **l'exception nommée** au découpage
  vertical, avec expand–migrate–contract sur des tickets séparés.
- Les sept points laissés ouverts à dessein par `D-039` (`docs/reference/skills.md` §10) —
  régime `--bare`, grain des skills, qui parle à Linear, langue du `description`, nom en
  collision, forme du verdict de revue, compteurs à nommer.

---

## 6. Où le flux touche git (`D-042`)

*Cette section vient après le registre parce que renuméroter aurait cassé un renvoi `flux.md §5`
logé dans `decisions.md`, qui est append-only et donc incorrigible.*

### La correspondance

Un niveau de ticket, une branche, une PR. Le nom porte l'identifiant Linear, ce qui suffit à Linear
pour rattacher seul la branche et la PR à sa carte — une couture de moins à coder.

**Qui crée quelle branche** (`D-076`) — une règle : **une branche naît avec l'`In Progress` de son
niveau, créée par le skill qui y travaille en premier**. C'est la même main qui crée les cartes du
niveau en dessous.

| Branche | Créée par | Depuis |
|---|---|---|
| `feature/` — **seulement si la spec en a décidé une** | `decoupage` | `main` |
| `story/` | `decoupage-pas` | la branche de feature s'il y en a une, sinon `main` |
| `pas/` | `prendre-un-pas` | la branche de story |

**Qui ouvre la PR d'un pas, et quand** (`D-075`) : `prendre-un-pas`, **juste après avoir posé
`Done`**. Le pas se clôt sur trois gestes — l'étiquette, la branche poussée, la PR ouverte vers la
branche de story — et la carte reçoit le lien. Le motif est celui qui fonde la strate elle-même : si
la revue d'un pas a lieu sur la PR (voir plus bas), la PR doit exister **avant** la revue.

**Qui fusionne un pas, et ouvre la PR d'une story** (`D-076`) : `revue-code`, **à l'accord**. La
fusion *est* ce qui tire le pas vers `Done`, et un pas accordé n'a aucun autre aval — il n'existe
pas d'étape après sa revue. Si c'était le **dernier** pas, la même main pose `Done` sur l'incrément
et ouvre la PR de la story : c'est le seul instant où la branche de story contient tout l'incrément.

⚠️ **La symétrie s'arrête là.** Un incrément a des étapes **après** sa `Code Review` — `QA Review`,
puis sa colonne terminale —, donc fusionner à l'accord de la revue de module ferait entrer une
story que la recette n'a pas vue. **Qui fusionne `story/` et qui ouvre la PR d'une feature n'a donc
pas de porteur**, et l'aura quand le premier incrément atteindra sa colonne terminale.

| Niveau | Branche | PR vers | Fusion |
|---|---|---|---|
| Pas | `pas/CUR-46-slug` | la story | **squash** |
| Incrément *(story)* | `story/CUR-45-slug` | la feature | **rebase puis fast-forward** |
| Feature | `feature/CUR-xx-slug` | `main` | **rebase puis `--no-ff`** |

⚠️ **L'identifiant d'un pas n'est pas celui de sa story.** La table portait `pas/CUR-45-3-slug` — un
identifiant de story suivi d'un **rang** —, forme écrite quand un pas n'avait pas de carte à lui.
`D-069` lui en a donné une, et `decoupage-pas` les crée comme sous-tâches que Linear numérote : le
nom porte donc l'identifiant **du pas**, ce que la règle juste au-dessus exigeait déjà.

**`feature/` n'est pas systématique.** Elle se décide **en Spec**, feature par feature, sur une
seule question : *cette feature expose-t-elle une surface qui doit apparaître d'un bloc ?* Si non,
ses stories vont directement sur `main` — chaque incrément étant « livrable seul, suite verte »
(`tickets.md` §1), `main` n'est jamais laissé à moitié fait. Le défaut est donc **non**, et l'écrire
dans la spec en fait une décision plutôt qu'un rituel.

### Les trois pièges du mode de fusion

**Le fast-forward n'en est pas un.** Il n'est possible que si la cible n'a pas divergé. Deux stories
d'une même feature en parallèle : la seconde exige un rebase avant de pouvoir avancer en FF. C'est
« rebase puis FF », toujours — pas « FF » avec un rebase occasionnel.

**Le corps du squash se réécrit à la main.** GitHub le pré-remplit avec la concaténation des
messages de WIP. Or c'est ce commit-là qui reste dans l'histoire, et ce dépôt écrit ses messages
longs à dessein — le titre ne peut pas porter le raisonnement ni les alternatives écartées. Accepter
le défaut, c'est perdre en silence ce que la convention de commit exige.

**Le rebase réécrit les hashes.** D'où la règle ci-dessous, qui n'est pas séparable de celle-ci.

### Ce que le squash autorise, et qui change le cycle

L'agent **commite librement pendant un pas** — WIP, correction de revue, refactor — puisque le
squash produit le commit propre à la fusion. Deux conséquences :

- il existe des **points de reprise** en cours de cycle, que la règle « un commit par comportement
  terminé » interdisait — elle a été retirée de `CLAUDE.md`, devenue sans objet ;
- la **revue d'un pas peut avoir lieu après le commit**, sur la PR, sans polluer l'historique. C'est
  ce qui justifie la strate `pas/` — et c'est une justification **datée** : elle sert à récolter le
  matériau du skill `revue-code` (`D-039`). À rejuger une fois ce skill rodé.

La propriété qu'exigeait cette règle n'est pas perdue, elle est **produite autrement** : un commit
de `main` est un pas squashé, donc un comportement terminé, suite verte et zéro warning. Ce n'est
plus une discipline que quelqu'un tient, c'est une conséquence de la mécanique de fusion.

### Le travail sans carte va directement sur `main`

Le travail sur *la façon de travailler* — méthode, documentation, outillage — n'est porté par aucune
carte, donc aucune branche ne lui correspond : il se commite sur `main`. La cascade ci-dessus ne
régit que le **code**, qui lui vient toujours d'un ticket.

C'est une exception assumée, pas un oubli, et elle recoupe une question ouverte de la §5 — le refacto
orphelin, qui entre par le `Backlog` des issues sans spec. Si le cas devenait assez fréquent pour
peser, ce sera le moment de légiférer ; le faire maintenant serait de la méthode sur cas imaginé,
que `D-039` proscrit.

### Les documents ne citent plus de hashes

**On écrit `CUR-45`, jamais un hash de commit.** Le rebase des branches rend tout hash écrit pendant
le développement caduc à la fusion ; et dans `decisions.md`, append-only, un hash périmé est
incorrigible. L'identifiant Linear survit à toute réécriture et dit *quel travail*, quand un hash ne
dit que *quel objet git*.

Pour désigner un état précis du code — besoin qui ne s'est pas encore présenté — la réponse est un
**tag**, pas un hash. Les hashes déjà écrits restent : ils pointent sur `main`, qui ne bouge plus.
