# Le flux de développement, et les skills qui le portent

> **À quoi sert ce fichier.** Il donne la vue **étape → skill** : le chemin complet d'un besoin
> jusqu'à son code, qui agit à chaque étape, et quel skill porte la méthode de cette étape.
> Il ne dit pas *ce que contient* un ticket — ça, c'est `tickets.md` — ni *ce qu'on construit* —
> ça, c'est `trajectoire.md`.
>
> **Pourquoi il existe.** Les skills sont auto-découvrables, et un agent n'a pas besoin de cette
> table pour trouver le sien. Le lecteur **humain**, lui, en a besoin : sans elle, le flux
> n'existe nulle part en entier — il est éparpillé en autant de fichiers que d'étapes, et
> personne ne peut répondre à « que se passe-t-il après `Plan Review` ? » sans les ouvrir tous.
>
> **Son autre usage** : c'est la liste de ce qui reste à écrire. Une étape sans skill est une
> étape encore manuelle, et le tableau §4 le dit sans détour.

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

---

## 2. Le flux complet

```
FEATURE   Backlog ──► Discovery ──► Spec ──► In Progress ──► Validation ──► Completed
                          │           │           │
                     (Canceled)   revue spec   découpage
                                                  │
INCRÉMENT                          Backlog ◄──────┴──────► Todo
                                                             │
                                    ┌────────────────────────┘
                                    ▼
                          [Planning ──► Plan Review] ──► In Progress ──► Code Review ──► [QA Review] ──► Done
                                                              │                │
PAS                                              Backlog ──► Todo ──► In Progress ──► Done
```

### La table maîtresse

| # | Étape | Niveau | Qui agit | Régime | Skill |
|---|---|---|---|---|---|
| 1 | **Discovery** | Feature | Humain + agent | Binôme | `discovery` |
| 2 | **Spec** | Feature | Humain + agent | Binôme | `spec` |
| 3 | **Revue de spec** | Feature | Agent tiers | Validation | `revue-spec` |
| 4 | **Découpage** | Feature → incréments | Agent | Production | `decoupage` |
| 5 | **Planning** | Incrément | Agent | Production | `plan-archi` |
| 6 | **Plan Review** | Incrément | Agent ⇄ agent | Boucle + escalade | `revue-plan` |
| 7 | **In Progress** | Pas | Agent | Production | `prendre-un-pas` |
| 8 | **Code Review** | Incrément | Agent ⇄ agent | Boucle + escalade | `revue-code` |
| 9 | **QA Review** | Incrément | Humain | Œil | — *(s'appuie sur la skill `run`)* |
| 10 | **Validation** | Feature | Humain | Œil | — |

Les étapes **9** et **10** n'ont pas de skill **par décision, pas par retard** : ce sont les
deux jugements sans référentiel opposable (`tickets.md` §6.3). Un agent qui les porterait
rendrait un verdict qu'il n'a pas les moyens de fonder.

### Ce que chaque étape reçoit et produit

| # | Entrée | Sortie | Transition de carte |
|---|---|---|---|
| 1 | Un cap nommé | Un **besoin** établi, des pistes ouvertes, **aucun choix** | `Backlog → Discovery`, puis `→ Spec` ou `→ Canceled` |
| 2 | Un besoin | Une **spec** : options arbitrées, capacité énoncée, **recette définie** | `Discovery → Spec` |
| 3 | Une spec | Un verdict, ou des divergences à reprendre en 2 | reste en `Spec` jusqu'à l'accord |
| 4 | Une spec validée | N **incréments** avec leurs **frontières** et leur ordre | `Spec → In Progress` ; les incréments naissent en `Todo` ou `Backlog` |
| 5 | Un incrément éligible | Un **plan d'archi** avec son schéma-delta, et le **découpage en pas** | `Todo → Planning → Plan Review` |
| 6 | Un plan | Un accord, ou un litige | `Plan Review → In Progress`, ou **assignation à l'humain** |
| 7 | Un pas | Une **test list**, des cycles TDD, un commit | `Todo → In Progress → Done` |
| 8 | Un comportement complet | Un accord, ou un litige | `Code Review → QA Review`/`Done`, ou **assignation** |
| 9 | L'app lancée | Le parcours refait à la main | `QA Review → Done` |
| 10 | La feature entière | Recettée **contre sa spec** | `In Progress → Validation → Completed` |

**Le plan d'archi s'écrit à l'étape 5, pas à l'étape 4.** Le découpage capture ce que lui seul
peut savoir — les frontières entre incréments, vues d'en haut, et qui disparaîtraient avec la
session qui les a produites. La conception de chacun attend sa prise : ce qu'on apprend en
faisant le premier change ce qu'on sait au quatrième. Même raison que pour la test list, qui
attend elle aussi la prise de son pas.

**L'escalade** (étapes 6 et 8) : après deux ou trois tours sans convergence, **la carte
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

---

## 4. Les skills à écrire

| Skill | Étape | Ce qu'il porte | État |
|---|---|---|---|
| `discovery` | 1 | Établir le besoin sans le confondre avec une solution ; ouvrir des pistes sans arbitrer ; savoir conclure *on ne fait pas* | **à écrire** |
| `spec` | 2 | Arbitrer les options avec faisabilité et coût, **écrire les écarts**, énoncer la capacité, définir la recette | **à écrire** |
| `revue-spec` | 3 | Valider une spec qu'on n'a pas co-écrite. Lister les divergences, **ne pas réécrire** | **à écrire** |
| `decoupage` | 4 | Produire les incréments et leurs **frontières** ; déposer dans chacun le hors-périmètre **en nommant les frères** | **à écrire** |
| `plan-archi` | 5 | Le plan gaté de `CLAUDE.md` : schéma-delta, blocs touchés, découpage en pas | **à écrire** |
| `revue-plan` | 6 | La boucle : verdict structuré, compteur de tours, escalade par assignation | **à écrire** |
| `prendre-un-pas` | 7 | Test list, cycles TDD (rouge observé *pour la bonne raison*), commit argumenté | **à écrire** |
| `revue-code` | 8 | Relire un **comportement**, pas un commit ; raffiner la test list et la formulation des comportements | **à écrire** |

Remplacer « à écrire » par un lien vers le fichier au fur et à mesure. Une ligne sans lien est
une étape encore manuelle — c'est l'unique état d'avancement que ce document a le droit de
porter.

**Ordre proposé** : `prendre-un-pas` d'abord. C'est le plus petit périmètre, l'erreur y coûte un
commit, il ne dépend d'aucun autre — on peut lui tendre un pas écrit à la main — et il rend
tout de suite le signal qui manque : *une carte de pas contient-elle assez pour qu'un agent
travaille sans avoir eu la conversation ?* Le découpage est plus tentant et c'est le mauvais
premier pas : tant qu'aucun pas n'a été exécuté par un agent, on ne sait pas quelle **maille**
de pas est bonne — or c'est précisément ce que le découpage décide.

---

## 5. Registre

**Construit** : rien. Aucun skill n'existe, aucune étape n'est automatisée.

**Tranché mais pas construit** : le flux et ses régimes (`D-036`), les trois lieux, l'escalade
par assignation, le rangement des skills dans `.claude/skills/`.

**Tranché ailleurs, pas ici** : le trio de la spec (étapes 1–3) a tourné sur quelques tickets
hors de ce dépôt.

**Questions ouvertes** :

- Distinguer *trois tours sur le même litige* de *trois tours qui dérivent de sujet* — ils se
  comptent pareil et ne valent pas pareil.
- La répartition du contexte entre `CLAUDE.md`, le skill et la carte est une **hypothèse**, à
  confirmer au premier round-trip réel (`tickets.md` §7).
- Le refacto orphelin entre par le `Backlog` des issues, mais **sans spec** — donc sans recette
  de niveau feature. Aucune étape de ce flux ne le couvre.
