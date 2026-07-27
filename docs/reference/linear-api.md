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

⚠️ **L'introspection, elle, ne demande aucun jeton** (mesuré le 2026-07-27). Le même endpoint rend le
schéma sans en-tête `Authorization` — c'est de loin la façon la moins chère de vérifier qu'un champ
existe **avant** d'écrire du code contre lui, et elle ne consomme aucun budget de complexité :

```bash
curl -s -X POST https://api.linear.app/graphql \
  -H "Content-Type: application/json" \
  -d '{"query":"{ __type(name: \"CommentCreateInput\") { inputFields { name } } }"}'
```

Le jeton n'est requis que pour lire ou écrire des **données**. Corollaire pratique : on peut sonder la
forme d'une mutation depuis n'importe où, sans clé et sans rien risquer sur le vrai tableau.

## 2. L'espace sondé

| | |
|---|---|
| Organisation | `Cursus`, `urlKey` = **`cursus-app`** |
| Équipe | une seule — clé **`CUR`** |
| Projets | 6, un par *feature* |

Convention de l'utilisateur, à respecter par le client : **projet = feature · issue = US ·
sous-tâche = commit** (voir `docs/methode/tickets.md`).

## 2bis. ⚠️ Une clé = un espace, et il n'y a rien à choisir

`organization` n'existe qu'au **singulier** dans le schéma — `organizations` est refusé par la
validation GraphQL. Une *Personal API key* est donc attachée à **exactement un workspace**, déterminé
à sa création : le périmètre d'un jeton ne se déclare pas, il se **constate**.

```graphql
{ organization { id name urlKey } }
```

C'est la requête d'épreuve d'un jeton : elle valide la clé *et* identifie l'espace, pour une fraction
du budget de complexité qu'aurait coûté la liste des projets. Sur cet espace : `Cursus` / `cursus-app`,
une seule équipe (`teams` → `CUR`).

Le nom « Personal API key » induit en erreur : la clé est personnelle **et** attachée à un espace, pas
à un utilisateur qui en survolerait plusieurs.

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

## 6. ⚠️ Deux limites de nature différente — on les confond à ses frais

Linear en applique **deux**. Les mélanger conduit à optimiser la mauvaise (mesuré le
2026-07-26, en-têtes de réponse à l'appui) :

| | Ce que c'est | Valeur | Comment on l'apprend |
|---|---|---|---|
| **Complexité par requête** | un plafond *a priori*, calculé sur les `first:` **avant** d'exécuter | **10 000** | un **400** `INPUT_ERROR` — la requête n'a pas tourné |
| **Budget par fenêtre** | ce que les requêtes acceptées ont réellement **consommé** | **3 000 000** | les en-têtes `x-ratelimit-complexity-*` |

Plus un plafond de **2 500 requêtes** par fenêtre (`x-ratelimit-requests-limit`), qui est
la seule limite qu'une pagination longue peut approcher.

⚠️ **Toute réponse porte son coût dans l'en-tête `x-complexity`.** C'est la façon la moins
chère de mesurer : inutile de chercher le mur par dichotomie de 400 — il suffit de lire
l'en-tête d'une requête qui passe.

⚠️ Le corps d'une réponse 400 porte un `userPresentableMessage` qui **donne le chiffre exact**. Tout
diagnostic sur cette API doit lire le corps : le code HTTP seul n'apprend rien.

### 6a. Ce que coûte chaque forme — et pourquoi la forme actuelle est au bord du mur

| Requête | `x-complexity` | Verdict |
|---|---|---|
| `projects(25) × issues(50) × labels(10)` — **celle du client aujourd'hui** | **8 280** | ✅ mais à **83 %** du plafond |
| `projects(50) × issues(100) × labels(10)` | **33 060** | ❌ 400, refusée |
| `issues(250) × labels(10)` — **à la racine** | **9** | ✅ |
| `projects(250)` nu | 300 | ✅ |
| `projects(6)` nu | 8 | ✅ |

Et les deux requêtes que le client compose désormais (mesurées le 2026-07-26, après bascule) :

| Requête du client | `x-complexity` |
|---|---|
| `projects(250) { pageInfo, id, name }` | **600** |
| `issues(250) { … labels(10), project { id } }` | **8** |
| **total pour le tableau entier** | **608** — contre 8 280 |

⚠️ Le poste dominant a changé de camp : c'est désormais la liste des **projets** qui coûte,
et son coût suit sa borne `first:` (le `pageInfo` en double le prix : 300 → 600). Le
descendre serait facile mais imposerait de paginer plus souvent ; à 6 % du plafond, il n'y
a rien à gagner.

⚠️ **Correction d'une affirmation de ce document.** Il disait que `25 × 50` tenait « avec de
la marge » : c'est faux, il n'en reste **1 720**. L'ajout de `labels(first: 10)` en a mangé
une large part — sur la forme haute, le refus mesuré est passé de 22 555 à **33 060**. Un
champ de plus sur cette requête pouvait la faire sauter, et l'aurait fait sans prévenir
autrement que par un 400.

⚠️ **Une anomalie non expliquée, et assumée comme telle.** Le coût de `projects` suit sa
borne `first:` (8 à 6, 300 à 250), tandis que celui d'`issues` **racine** n'en dépend pas du
tout (9 à 50, 100 et 250 indifféremment). Aucune formule connue ne rend compte des deux, et
on ne va pas en inventer une. Le client ne s'appuie donc pas sur la formule mais sur la
**borne du pire cas** : même en supposant le calcul multiplicatif, `issues(250) × labels(10)`
vaut 2 500 estimés, sous le plafond dans les *deux* hypothèses. C'est ce raisonnement — et
non le 9 mesuré, qui pourrait n'être vrai que sur un petit espace — qui justifie la borne
retenue.

## 6bis. Les formes d'échec — sondées

Trois façons distinctes d'échouer, qui appellent trois remèdes différents :

| Cause | HTTP | Ce que dit le corps |
|---|---|---|
| Jeton invalide ou révoqué | **401** | `code: AUTHENTICATION_ERROR` |
| Requête refusée (trop complexe) | **400** | `code: INPUT_ERROR` + le chiffre dans `userPresentableMessage` |
| Entité introuvable | **200** ⚠️ | `errors` peuplé, `data: null` |

⚠️ **La troisième ligne est le piège** : GraphQL rend **200** alors que rien n'a abouti. Conclure au
succès sur le seul code HTTP laisse passer l'échec silencieusement — c'est la présence d'`errors`
qui tranche, jamais le statut.

⚠️ **`message` n'est pas `userPresentableMessage`.** Le premier est laconique (« Query too
complex ») ; le second porte le diagnostic exploitable (« Complexity: 17055… Maximum allowed:
10000 »). Toujours préférer le second, se rabattre sur le premier.

**L'authentification passe avant l'analyse de la requête** : une requête trop complexe présentée
avec un mauvais jeton rend 401, pas 400. On ne peut donc pas calibrer la complexité sans une clé
valide.

Ces trois formes sont traduites par `LinearFailure` (testé sur ces corps réels) vers
`TrackerRejectedException` / `TrackerUnreachableException`.

## 7. Pagination — mesurée de bout en bout

Toutes les connexions sont en `first: n` / `after: cursor`, avec
`pageInfo { hasNextPage endCursor }`. **Vérifié non théorique** : un projet de 4 issues rend déjà
`hasNextPage: true` à `first: 2`.

| | |
|---|---|
| Page **maximale** | **250** — au-delà, un 400 `INVALID_INPUT` : « *first must not be greater than 250* » |
| Page par défaut | **50**, si l'on ne dit rien |
| Ordre | `createdAt` par défaut, `updatedAt` disponible par `orderBy` |

⚠️ Le refus au-delà de 250 est une **erreur de validation d'argument**, pas un refus de
complexité : les deux plafonds sont indépendants, et le premier atteint gagne.

**`after:` fonctionne — vérifié en chaînant deux appels**, pas en le supposant :
`issues(first: 2)` rend `CUR-45, CUR-44` avec un `endCursor` ; la même requête portant cet
`after:` rend `CUR-43, CUR-42`. Le test vaut la peine : un curseur silencieusement ignoré
rendrait la première page indéfiniment, et **la boucle ne s'arrêterait jamais**.

### 7bis. ⚠️ La pagination imbriquée est le vrai piège — `issues` racine le contourne

`projects { issues }` porte **un curseur par projet**. Paginer sous cette forme demande N
boucles imbriquées, une par projet, chacune avec son propre état — et le coût se multiplie.

`issues` **existe à la racine**, et rend `project { id name }` sur chaque issue. Un seul
curseur, donc **une seule boucle**. C'est la forme retenue par `CUR-45`, et la table du §6a
dit l'écart de coût : 9 contre 8 280.

⚠️ Contrepartie, qui impose **deux** requêtes plutôt qu'une : un projet **sans aucune issue**
n'apparaît dans aucune issue, et disparaîtrait donc du tableau. Les projets se demandent à
part (`projects` nu, bon marché), et les issues s'y raccrochent par `project.id`.

⚠️ Une issue peut n'appartenir à **aucun** projet (`project: null`). Invisible quand on
partait des projets, elle remonte dès qu'on part des issues — il faut donc en décider.

## 8. `filter:` — le filtre serveur

Sondé, et il marche **sur `issues` racine** :

| Forme | Verdict |
|---|---|
| `issues(filter: { labels: { name: { eq: "Feature" } } })` | ✅ accepté (0 résultat ici, voir l'avertissement ci-dessous) |
| `issues(filter: { project: { null: false } })` | ✅ accepté — **non documenté**, mesuré |

Comparateurs (documentés, non tous mesurés) : `eq` `neq` `in` `nin` partout ; `lt` `lte`
`gt` `gte` sur nombres et dates ; `contains` `startsWith` `endsWith` et leurs variantes
`IgnoreCase` / `not…` sur les chaînes ; `null` sur les champs optionnels.

⚠️ **Aucune carte de cet espace ne porte d'étiquette** au 2026-07-26 — toutes les sondes
rendent `labels: { nodes: [] }`. La lecture des étiquettes (`TaskSummary.Labels`) est donc
prouvée par ses tests unitaires sur fragments réels, **pas encore par une carte réelle
étiquetée**. À reprendre quand le prédicat de `CUR-5` en aura besoin.

## 9. ⚠️ L'API rend les noms HTML-échappés

Mesuré : le projet nommé `Finition de l'app — visuel & configuration` revient du GraphQL
comme `…visuel &amp; configuration`. Le tiret cadratin, lui, passe intact — seules les
**entités HTML** sont touchées. Le MCP Linear, interrogé sur le même espace, rend `&` : ce
n'est donc pas la donnée stockée qui porte l'entité, c'est **cette API** qui l'échappe.

Conséquence directe, et visible : **l'écran des tâches de Cursus affiche `&amp;`**. À
dé-échapper à la traduction, là où le reste de la lecture se fait — pas dans la vue, sinon
chaque affichage devra s'en souvenir.

## 10. Commentaires — les ancrer, et les solder

> Sondé le 2026-07-27, **par introspection du schéma seulement**. Aucune de ces mutations n'a été
> exécutée : ce qui suit dit ce que l'API *permet*, pas ce qu'elle *fait*. La distinction vaut plus
> ici qu'ailleurs — un commentaire écrit est visible de tous sur la carte, et ne se retire pas
> discrètement.

**Le motif de la sonde.** Le MCP Linear (`save_comment`) ne sait **ni ancrer un commentaire, ni le
résoudre** — son input n'a ni ancre ni champ de résolution, et le `quotedText` qu'il rend en lecture
est à sens unique. GraphQL sait faire les deux. C'est ce qui départage les deux voies dès qu'un agent
doit rendre une relecture d'artefact sur la carte plutôt que dans un terminal.

### 10a. L'ancre

`CommentCreateInput` porte `quotedText` **et** `documentContentId`. Et `Document` expose
`documentContentId` en champ direct — inutile de traverser `DocumentContent` pour l'obtenir :

```graphql
{ document(id: "…") { documentContentId } }

mutation {
  commentCreate(input: {
    documentContentId: "…"
    quotedText: "le passage exact, tel qu'il figure dans le document"
    body: "la divergence, en Markdown"
  }) { comment { id } }
}
```

Les autres ancres du même input, non sondées : `issueId`, `projectId`, `initiativeId`,
`projectUpdateId`, `initiativeUpdateId`, `postId`, et `parentId` pour répondre dans un fil.

### 10b. Le solde

```
commentResolve(id, resolvingCommentId)
commentUnresolve(id)
```

⚠️ `resolvingCommentId` nomme **quel commentaire solde la divergence**, pas seulement qu'elle l'est.
C'est très exactement la clause de `docs/methode/dod/feature/spec.md` §2 — *« reprise, ou refusée avec
sa raison écrite ; une divergence sans suite écrite n'est pas soldée »*. Le modèle de données porte
déjà la sémantique de la méthode : il n'y a rien à simuler à côté.

Corollaire de répartition, si un agent relit : il **pose**, l'humain **résout**. Ce n'est pas une
limite technique — c'est la DoD qui le veut (*« l'humain prononce l'accord »*).

### 10c. Deux champs à éprouver avant de bâtir dessus

`CommentCreateInput` porte aussi `createAsUser` et `displayIconUrl` : de quoi faire signer un
commentaire par un relecteur **nommé** plutôt que par le porteur de la clé — ce qui rendrait une revue
tierce lisible comme telle sur la carte. Ces champs sont, chez d'autres API, réservés aux jetons
d'application ; avec une *Personal API key*, c'est à vérifier **tôt**, avant d'avoir construit autour.

## 11. Ce que la sonde n'a pas couvert

À sonder avant de s'y appuyer :

- **les mutations** (`issueUpdate` pour déplacer, `issueAddLabel` pour étiqueter) — elles écrivent
  sur le vrai tableau, donc réservées à `2·2b`+ et à faire sur une issue de test ;
- **les mutations de commentaire du §10** — leur *schéma* est sondé, leur *exécution* ne l'est pas.
  Notamment : ce que Linear fait d'un `quotedText` qui ne correspond à aucun passage du document, et
  si `createAsUser` est accepté d'une *Personal API key* ;
- **l'idempotence** exigée au §7.10.3 : déplacer vers la colonne où la carte est déjà doit réussir —
  à vérifier, pas à supposer ;
- les **limites de débit** en pratique : les plafonds sont connus (§6), mais aucune fenêtre n'a
  été poussée jusqu'au 429 ;
- la **stabilité du curseur** sous écriture concurrente : si une carte est créée entre deux pages,
  rien ne dit si elle est vue deux fois, une fois, ou pas du tout ;
- le champ `triage` et les projets d'autres équipes (une seule équipe ici).
