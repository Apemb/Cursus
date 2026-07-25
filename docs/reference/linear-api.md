# API Linear — référence sondée

> **Pourquoi ce document existe.** Le client Linear (jambe 2·2b·2) doit se poser sur la forme
> **réelle** de l'API, pas sur ce qu'on en suppose. Tout ce qui suit a été obtenu **par sonde**
> (le 2026-07-25) contre l'espace `cursus-app`, avec une *Personal API key*. Sans ce fichier, la
> connaissance vivrait hors du dépôt et se reperdrait au premier doute.
>
> ⚠️ **Aucun secret ici.** Le jeton vit dans le trousseau (`ISecretStore`, `D-033`), jamais dans le
> dépôt. Les exemples ci-dessous supposent qu'on l'a sous la main.

---

## 1. Comment re-sonder

Un seul endpoint, GraphQL, en POST :

```bash
curl -s -X POST https://api.linear.app/graphql \
  -H "Content-Type: application/json" \
  -H "Authorization: $LINEAR_TOKEN" \
  -d '{"query":"{ viewer { name } organization { name urlKey } }"}'
```

⚠️ **Le jeton se passe brut**, sans préfixe `Bearer` — c'est une *Personal API key*. (Un jeton OAuth,
lui, prendrait `Bearer`. On ne vise pas OAuth : Cursus est un outil de dev mono-utilisateur.)

## 2. L'espace sondé

| | |
|---|---|
| Organisation | `Cursus`, `urlKey` = **`cursus-app`** |
| Équipe | une seule — clé **`CUR`** |
| Projets | 6, un par *feature* |

Convention de l'utilisateur, à respecter par le client : **projet = feature · issue = US ·
sous-tâche = commit** (voir `docs/methode/tickets.md`).

## 3. La maille — et pourquoi elle tombe juste

Linear n'a pas d'« epic » (c'est un mot Jira). La hiérarchie native se lit ainsi :

```
Project  ──►  Issue  ──►  Sub-issue
(feature)     (US)        (commit)
```

- `project.issues` rend **toutes** les issues du projet — **parents et enfants confondus, à plat**.
  La hiérarchie ne se lit donc pas de la liste : elle se **reconstruit** par `parent`/`children`.
- Une sous-issue porte `parent { identifier }` ; une issue-mère porte `children { nodes { … } }`.
- Une issue **sans** `parent` et **sans** `children` est une US isolée, pas une anomalie.

## 4. Le point dur, et sa bonne surprise

**`issue(id: "CUR-12")` accepte l'identifiant humain**, pas seulement l'UUID :

```graphql
{ issue(id:"CUR-12"){ id identifier title description url state { name } } }
```

C'est ce qui rend `ITaskTracker.ReadAsync(key)` direct : la clé que porte le `RunTrigger`
(« CUR-12 ») est utilisable telle quelle, **sans résolution préalable**. Le champ `id` (UUID) reste
rendu, et c'est lui que réclameront les **mutations**.

`description` est du **Markdown**, ce qui tombe bien : `ReadTask` l'écrit dans `TASK.md` (`D-032`).

## 5. Colonnes et étiquettes

Les colonnes sont des **`workflowStates`**, propres à une équipe. Celles de `CUR` :

| Nom | `type` |
|---|---|
| Backlog | `backlog` |
| Todo | `unstarted` |
| Planning · Plan Review · In Progress · Code Review · QA Review | `started` |
| Done | `completed` |
| Canceled | `canceled` · Duplicate `duplicate` |

⚠️ **`position` n'est PAS un ordre de flux.** C'est un flottant de tri d'affichage, et sur cet espace
il est incohérent avec le cycle réel : trié par `position`, `Done` (3) précède `In Progress` (905) et
`Planning` vaut −937. **Conséquence à retenir pour §7.10.6** : l'invariant de sûreté des déclencheurs
auto — *un workflow auto-déclenché doit déplacer vers une colonne strictement postérieure* — **ne peut
pas s'appuyer sur `position`**. Il faudra un ordre déclaré côté Cursus (dans `project.json`), ou se
rabattre sur `type`, qui est grossier mais monotone.

Étiquettes (`issueLabels`) sur cet espace : `Feature`, `Bug`. Rien qui ressemble encore au `Done` /
`Comments` évoqué au §7.10.3 — **le contrat colonnes/étiquettes reste à définir** (c'est `CUR-5`).

## 6. Pagination

Toutes les connexions sont en `first: n` / `after: cursor`, avec
`pageInfo { hasNextPage endCursor }`. **Vérifié non théorique** : un projet de 4 issues rend déjà
`hasNextPage: true` à `first: 2`. Le client devra donc paginer, ou assumer explicitement un plafond —
jamais laisser croire qu'une première page est la liste entière.

## 7. Ce que la sonde n'a pas couvert

À sonder avant de s'y appuyer :

- **les mutations** (`issueUpdate` pour déplacer, `issueAddLabel` pour étiqueter) — elles écrivent
  sur le vrai tableau, donc réservées à `2·2b`+ et à faire sur une issue de test ;
- **l'idempotence** exigée au §7.10.3 : déplacer vers la colonne où la carte est déjà doit réussir —
  à vérifier, pas à supposer ;
- les **limites de débit** (Linear en documente ; aucune rencontrée sur ces requêtes) ;
- le champ `triage` et les projets d'autres équipes (une seule équipe ici).
