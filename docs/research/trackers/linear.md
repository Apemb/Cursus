# Fiche de recherche — Linear : modèle de données des tâches et API

Sources primaires : documentation développeur officielle (`linear.app/developers`, l'ancien domaine
`developers.linear.app` redirige désormais en 301 vers celui-ci) et le **schéma GraphQL publié** dans le
dépôt du SDK officiel (`linear/linear`, `packages/sdk/src/schema.graphql`, ~49 800 lignes, consulté le
2026-07-21). Sauf mention contraire, tout nom de type, de champ ou de mutation cité ci-dessous a été lu
dans ce schéma ou dans une page de documentation effectivement consultée. Les affirmations qui n'ont pas
pu être adossées à une page lue sont marquées `⚠️ non vérifié`.

---

## 1. L'objet « tâche »

Linear appelle la tâche un **`Issue`** (aucun autre terme dans le modèle : pas de « ticket », pas de
« story », pas de « work item »). Le type GraphQL est `type Issue implements Node`.

### Identité — deux identifiants, un seul stable

| Champ | Type | Nature |
|---|---|---|
| `id` | `ID!` | UUID opaque, **stable**, c'est la clé technique |
| `identifier` | `String!` | Clé lisible, forme `TEAM-123` (ex. `BLA-123`) |
| `number` | `Float!` | Le numéro seul, séquentiel **par équipe** |
| `previousIdentifiers` | `[String!]!` | Identifiants précédemment portés par l'issue |
| `url` | `String!` | URL canonique |
| `branchName` | `String!` | Nom de branche git suggéré, dérivé de l'identifiant |

Le point critique pour une abstraction : **`identifier` n'est pas stable**. Il est composé du préfixe
d'équipe et d'un numéro séquentiel par équipe ; déplacer une issue d'équipe (`teamId` est modifiable via
`IssueUpdateInput`) la renumérote. L'existence même du champ `previousIdentifiers`, dont la
documentation du schéma dit pour l'objet analogue `Initiative` qu'il sert « to resolve URLs that
referenced an older identifier », confirme que la clé lisible bouge. **Seul `id` (UUID) doit servir de
clé de corrélation dans un stockage local.**

Subtilité inverse et piégeuse : l'API accepte **les deux formes en entrée**. La documentation d'accueil
précise, à propos de `issueUpdate` : « The id provided can be either be the uuid returned by the creation
query, or the shorthand id like BLA-123 ». La query `issue(id: String!)` est typée `String`, pas `UUID`,
précisément pour cela. En revanche `issueBatchUpdate(ids: [UUID!]!)` est typée `UUID` — donc **exige** les
UUID. L'acceptation de la clé lisible n'est donc pas uniforme.

### Champs canoniques (extraits du type `Issue`)

Scalaires et contenu : `title: String!`, `description: String` (Markdown), `descriptionState`,
`priority: Float!` (numérique) doublé de `priorityLabel: String!` (libellé), `estimate: Float`,
`dueDate: TimelessDate` (date sans heure — type dédié), `trashed: Boolean`, `sharedAccess`.

Relations : `team: Team!` (**obligatoire — une issue appartient toujours à une équipe**),
`state: WorkflowState!`, `assignee: User`, `creator: User`, `delegate: User`, `parent: Issue`,
`children`, `project: Project`, `projectMilestone: ProjectMilestone`, `cycle: Cycle`,
`labels` + `labelIds: [String!]!`, `comments`, `attachments`, `subscribers`, `history`, `reactions`,
`relations` / `inverseRelations`, `agentSessions`.

Horodatages d'étape (nombreux et automatiques, non fixés par le client en temps normal) :
`createdAt!`, `updatedAt!`, `archivedAt`, `startedAt`, `completedAt`, `canceledAt`, `triagedAt`,
`startedTriageAt`, `autoArchivedAt`, `autoClosedAt`, `addedToCycleAt`, `addedToProjectAt`,
`addedToTeamAt`, `snoozedUntilAt`, `slaStartedAt`, `slaBreachesAt`, `slaHighRiskAt`, `slaMediumRiskAt`.

Ordonnancement : `sortOrder: Float!`, `subIssueSortOrder: Float`, `prioritySortOrder: Float!`, et
`boardOrder: Float!` marqué `@deprecated(reason: "Will be removed in near future, please use sortOrder
instead")`.

Il n'y a **pas de champs personnalisés arbitraires** exposés comme un dictionnaire clé/valeur sur
`Issue` dans le schéma consulté (contrairement à ce qu'on attend d'un Jira). Les extensions passent par
`attachments`, `labels`, ou `syncedWith: [ExternalEntityInfo!]`.

---

## 2. Hiérarchie

Il faut distinguer nettement **une vraie hiérarchie récursive** de **rattachements plats**.

### Vraie hiérarchie (récursive, même type parent et enfant)

- **Issue → sous-issues** : `parent: Issue` / `children(...)`. C'est une auto-relation, donc récursive
  par construction. La documentation produit sur les sous-issues indique que « Sub-issues inherit the
  parent issue's team, priority, and project » et qu'une règle d'automatisation existe : « When all
  sub-issues are marked as done, the parent issue will also be marked as done automatically » (cohérent
  avec le champ `autoClosedByParentClosing: Boolean` présent dans `IssueUpdateInput`).
  **Profondeur maximale : non documentée.** La page consultée ne mentionne aucune limite. Ne pas
  supposer une limite ni supposer l'absence de limite. `⚠️ non vérifié`
- **Initiative → sous-initiatives** : `parentInitiative: Initiative`, `subInitiatives(...)`,
  `parentInitiatives(...)`. La documentation produit annonce une imbrication **jusqu'à cinq niveaux**.
- **Team → sous-équipes** : hiérarchique également, **jusqu'à cinq niveaux** d'après la documentation
  produit (fonctionnalité « multi-level sub-teams »). Conséquence notable : les labels sont hérités des
  équipes parentes.

### Rattachements (pas une hiérarchie — une simple appartenance, non récursive côté issue)

- **Projet** : `Issue.project: Project`. Une issue pointe vers **au plus un** projet. `Project` n'a pas
  de parent-projet dans le schéma consulté ; il porte `initiatives(...)`, donc un projet peut être
  rattaché à des initiatives. La chaîne conceptuelle est donc
  `Initiative → Project → Issue`, mais **seul le maillon initiative est récursif**.
- **Jalon de projet** : `Issue.projectMilestone: ProjectMilestone`. Un découpage **à l'intérieur** d'un
  projet, pas un niveau au-dessus de l'issue.
- **Cycle** : `Issue.cycle: Cycle`. Le `Cycle` a `number: Float!`, `name: String`, `startsAt: DateTime!`,
  `endsAt: DateTime!` et `team: Team!` — c'est donc l'analogue du sprint, **borné par une équipe et par
  le temps**. Une issue est dans au plus un cycle.

### Ce qui n'existe pas

**Il n'y a pas d'objet « epic » dans Linear.** Le niveau qu'un Jira appellerait epic est joué soit par
le `Project`, soit par une issue parente. C'est le premier point de friction sérieux pour une
abstraction commune.

---

## 3. Étiquettes

Type `IssueLabel implements Node`. Champs : `id: ID!`, `name: String!`, `color: String!`,
`description: String`, `isGroup: Boolean!`, `parent: IssueLabel`, `team: Team`, `inheritedFrom:
IssueLabel`, `creator: User`, `lastAppliedAt`, `retiredAt`, `retiredBy`.

- **Groupées, sur un seul niveau.** `isGroup` marque un label-groupe, `parent` rattache un label à son
  groupe. Ce n'est donc pas un modèle plat, mais ce n'est pas non plus un arbre profond : le schéma
  n'interdit pas formellement l'imbrication, mais l'interface produit présente les groupes comme un
  unique niveau de regroupement. `⚠️ non vérifié` (profondeur réelle autorisée par le serveur).
- **Portée : organisation OU équipe.** Le champ `team: Team` est **nullable**, et le champ
  `organization` est explicitement `@deprecated(reason: "Workspace labels are identified by their team
  being null.")`. Donc : `team == null` ⇒ label d'espace de travail ; `team != null` ⇒ label d'équipe.
  La documentation produit le confirme. **Aucune portée « par projet ».**
- **Héritage** : `inheritedFrom` et la documentation produit (« Subteams inherit parent team labels »).
- **Multi-valuation : oui**, `Issue.labelIds: [String!]!`, mais **avec une exclusivité intra-groupe** :
  la documentation produit énonce « Only one label from a given label group can be applied to an issue
  at a time ». C'est une contrainte serveur qu'aucun des trois autres outils ne reproduira
  naturellement.
- **Couleur** : `color: String!`, obligatoire sur le label existant, optionnelle à la création.
- **Création à la volée** : mutation `issueLabelCreate`, dont l'input `IssueLabelCreateInput` exige
  seulement `name: String!` ; `teamId`, `color`, `description`, `parentId`, `isGroup` et `id` sont
  optionnels. La documentation produit mentionne aussi une création à la volée depuis l'interface via
  la syntaxe `Type/Bug` ou `Type:Bug`, qui crée **groupe et label** d'un coup — c'est une convention
  d'interface, pas une propriété de l'API. Un groupe est plafonné à **250 labels** (doc produit).
- **Retrait logique** : `retiredAt` / `retiredBy` — un label peut être « retiré » sans être supprimé.

---

## 4. États et colonnes — le point central

**Le statut et la colonne sont le même objet.** C'est la différence structurante avec Jira.

Le type est `WorkflowState implements Node`, avec :

| Champ | Type | Rôle |
|---|---|---|
| `id` | `ID!` | UUID de l'état |
| `name` | `String!` | Nom libre, défini par l'équipe |
| `type` | `String!` | Catégorie normalisée (voir ci-dessous) |
| `position` | `Float!` | Position de la **colonne** dans le board |
| `color` | `String!` | Couleur |
| `team` | `Team!` | **Les états appartiennent à une équipe** |
| `inheritedFrom` | `WorkflowState` | Héritage depuis une équipe parente |

Le schéma documente exactement les catégories : « The type of the state. One of `"triage"`, `"backlog"`,
`"unstarted"`, `"started"`, `"completed"`, `"canceled"`, `"duplicate"` ». C'est une **taxonomie fermée
et stable** — c'est le meilleur point d'accroche pour une abstraction commune, bien meilleur que `name`
qui est libre par équipe.

Le commentaire du schéma est explicite : « Each team has its own set of workflow states ». Il n'existe
donc **pas** de jeu d'états global à l'organisation : deux équipes ont deux jeux d'états disjoints, avec
des UUID différents même pour des états homonymes.

### Pas de workflow contraint

**Aucun type de transition n'existe dans le schéma** (aucun `type *Transition`, aucune mutation de
transition). Le changement d'état se fait en écrivant `stateId` dans `IssueUpdateInput`, comme n'importe
quel autre champ. Il n'y a donc **ni graphe de transitions autorisées, ni écran de transition, ni champ
obligatoire conditionnel à une transition**. Toute transition est légale tant que le `stateId` cible
appartient à l'équipe de l'issue. C'est un modèle radicalement plus permissif que celui de Jira.

### Position dans la colonne — adressable

Oui, et c'est explicite : `Issue.sortOrder: Float!` est modifiable via `IssueCreateInput.sortOrder` et
`IssueUpdateInput.sortOrder`. Trois ordres coexistent sur l'issue :

- `sortOrder` — l'ordre principal (celui qui remplace `boardOrder`, déprécié) ;
- `subIssueSortOrder` — l'ordre de l'issue **au sein de la fratrie sous son parent** ;
- `prioritySortOrder` — un ordre distinct utilisé dans les vues triées par priorité.

Ce sont des **flottants**, pattern classique d'insertion entre deux voisins sans réindexer. Il n'y a pas
d'opération « déplacer avant l'issue X » : c'est au client de calculer le flottant. `IssueCreateInput`
expose aussi `preserveSortOrderOnCreate: Boolean`.

---

## 5. Écriture

Toutes les mutations sont GraphQL, sur le même endpoint. Les charges utiles sont typées `IssuePayload!`,
`CommentPayload!`, etc.

| Opération | Mutation | Obligatoire |
|---|---|---|
| Créer une tâche | `issueCreate(input: IssueCreateInput!)` | **`teamId: String!` — c'est le seul champ obligatoire.** `title` est `String` (nullable !) |
| Créer en lot | `issueBatchCreate(input: IssueBatchCreateInput!)` | « Creates a list of issues in one transaction » |
| Éditer | `issueUpdate(id: String!, input: IssueUpdateInput!)` | `id` ; tous les champs de l'input sont optionnels (patch partiel) |
| Éditer en lot | `issueBatchUpdate(ids: [UUID!]!, input: IssueUpdateInput!)` | « Can't be more than 50 at a time » |
| Changer d'état / de colonne | `issueUpdate` avec `stateId` (et/ou `sortOrder`) | pas de mutation dédiée |
| Poser une étiquette | `issueAddLabel(id: String!, labelId: String!)` | les deux |
| Retirer une étiquette | `issueRemoveLabel(id: String!, labelId: String!)` | les deux |
| Poser/retirer en lot | `issueUpdate` avec `labelIds` (remplacement total), ou `addedLabelIds` / `removedLabelIds` (delta) | — |
| Commenter | `commentCreate(input: CommentCreateInput!)` | **aucun champ n'est `!`** : `body`, `issueId`, `parentId`, `projectId`, `initiativeId`… tous optionnels au niveau du schéma ; la cible et le corps sont validés côté serveur |
| Archiver / supprimer | `issueArchive(id, trash)` / `issueDelete(id, permanentlyDelete)` | `id`. La suppression est une corbeille avec « grace period of 30 days » ; `permanentlyDelete` est réservé aux admins |

Remarque sur `IssueCreateInput` : `title` étant nullable, une création sans titre est acceptée par le
typage. Ne pas s'appuyer là-dessus — valider côté client. `⚠️ non vérifié` (comportement serveur réel
d'une création sans titre).

### Idempotence — le point à surveiller

**Il n'existe aucune clé d'idempotence sur les mutations d'issue, de label ou de commentaire.** Une
recherche exhaustive du terme `idempot` dans le schéma ne remonte que trois cas, tous hors périmètre :
`favoriteDelete` et `viewPreferencesDelete` (idempotents parce que la suppression d'un inexistant réussit
silencieusement), et `idempotencyKey: String` sur la **création d'application OAuth**. Cette dernière est
la seule vraie clé d'idempotence de l'API, et elle ne concerne pas les tâches.

Conséquences pratiques pour un client de bureau :

- **`issueCreate` n'est PAS idempotent.** Un rejeu après timeout crée un doublon. **Le seul levier est
  `IssueCreateInput.id: String`** : le client fournit lui-même l'UUID. Un rejeu avec le même UUID ne
  peut pas créer une seconde ligne (l'unicité de la clé primaire s'y oppose). Le comportement exact du
  rejeu — erreur de conflit ou retour de l'existant — n'est pas documenté sur la page consultée
  `⚠️ non vérifié`, mais le pattern est le même sur `IssueLabelCreateInput.id` et
  `CommentCreateInput.id`, qui exposent tous deux un `id` client. **C'est le mécanisme à adopter
  systématiquement dans Cursus** : générer l'UUID côté client avant l'appel, le journaliser, puis
  appeler.
- **`issueUpdate` est naturellement idempotent** : c'est un patch en écriture absolue (« mettre `stateId`
  à X »), donc rejouable sans dégât. Attention : il écrase inconditionnellement, il n'y a pas de
  contrôle d'optimistic concurrency (pas de champ de version ni d'`If-Match` observé). Deux clients
  concurrents s'écrasent en dernier-arrivé-gagne.
- **`issueAddLabel` / `issueRemoveLabel` sont sémantiquement idempotents** (ajouter deux fois le même
  label ne peut produire qu'un rattachement) — mais le succès du second appel n'est pas garanti par la
  documentation consultée. `⚠️ non vérifié`
- **`issueUpdate` avec `labelIds` est un remplacement total**, donc idempotent mais destructif pour un
  label posé entre-temps par quelqu'un d'autre. `addedLabelIds` / `removedLabelIds` sont le choix sûr
  pour un client concurrent.
- **`commentCreate` n'est pas idempotent** sans `id` client — un rejeu duplique le commentaire.

---

## 6. Authentification

Deux mécanismes, documentés sur `linear.app/developers/graphql` et
`linear.app/developers/oauth-2-0-authentication`.

### Clé d'API personnelle

En-tête `Authorization: <API_KEY>` — **sans le préfixe `Bearer`**, contrairement à OAuth. Générée depuis
les réglages de sécurité du compte. Destinée aux scripts et à l'usage personnel. Aucune expiration
documentée sur la page consultée. `⚠️ non vérifié` (rotation, révocation programmatique).

### OAuth 2.0

En-tête `Authorization: Bearer <ACCESS_TOKEN>`. Portées documentées :

| Portée | Sens |
|---|---|
| `read` | « Read access for the user's account. This scope will always be present. » — toujours accordée |
| `write` | Écriture générale |
| `issues:create` | « Allows creating new issues and their attachments » |
| `comments:create` | « Allows creating new issue comments » |
| `timeSchedule:write` | Horaires |
| `admin` | « Full access to admin level endpoints » |
| `app:assignable`, `app:mentionable` | Portées liées aux agents (une issue peut être assignée à l'application) |

`issues:create` et `comments:create` permettent une intégration en écriture **minimale** sans demander
`write` — c'est le bon réflexe pour Cursus si l'outil ne fait que créer.

**Durée de vie** : le jeton d'accès standard vaut environ 24 h (`"expires_in": 86399`), avec un
**refresh token** délivré à l'autorisation initiale et échangeable sur `/oauth/token`. Une « grace period »
de 30 minutes autorise le rejeu d'une demande de rafraîchissement en cas d'échec réseau — utile pour un
client de bureau au réveil de veille. Le flux *client credentials* ne délivre **pas** de refresh token ;
ses jetons durent 30 jours et sont invalidés par la rotation du secret client.

**Notion d'acteur** : un paramètre `actor` détermine la propriété des ressources créées ; `actor=app`
signifie « Resources are created as the application » — les issues et commentaires apparaissent au nom de
l'application, pas de l'utilisateur. C'est le mode « service account / agent ». Jusqu'à 1000 jetons
*client credentials* en parallèle par application, à condition de portées identiques.

**Ce qu'un client de bureau doit stocker durablement** : le `refresh_token` (secret de longue durée, à
mettre dans le trousseau système, jamais sur disque en clair) et, en cache court, l'`access_token` avec
son instant d'expiration. Le `client_secret` ne doit **pas** vivre dans un binaire distribué — un client
de bureau public est un client OAuth « public », le secret y est extractible. `⚠️ non vérifié` (support
de PKCE par Linear : non observé sur la page consultée).

---

## 7. Transport et limites

**GraphQL exclusivement**, un endpoint unique (`https://api.linear.app/graphql`). Il n'y a **pas d'API
REST** pour les tâches. Le schéma complet est publié dans le SDK officiel et explorable via Apollo Studio.

### Pagination

Curseurs style Relay. Chaque connexion expose `edges { node cursor }` **et** un raccourci `nodes`
(« mirroring GitHub's GraphQL approach »). `pageInfo` porte `hasNextPage` et `endCursor` ; on passe
`endCursor` en `after` pour la page suivante. Arguments `first`/`after` (avant) et `last`/`before`
(arrière). **Défaut : 50 éléments** sans argument. Tri par `createdAt` par défaut, `orderBy: updatedAt`
disponible — c'est ce dernier qu'il faut pour une synchronisation incrémentale. Les archives ne
remontent qu'avec `includeArchived: true`.

### Quotas — chiffres officiels

Algorithme *leaky bucket* (recharge continue à `LIMIT_AMOUNT / LIMIT_PERIOD`).

| Authentification | Requêtes / h | Complexité / h | Portée du compteur |
|---|---|---|---|
| Clé d'API | 5 000 | 3 000 000 pts | par utilisateur |
| Application OAuth | 5 000 | 2 000 000 pts | par utilisateur/app-user |
| Non authentifié | 600 | 100 000 pts | par adresse IP |

**Complexité maximale d'une requête unique : 10 000 points.** Le calcul est documenté : « Each property
is 0.1 point, each object is 1 point and any connection multiplies its children's points based on the
given pagination argument, or the default 50 ». **Le facteur multiplicatif des connexions est le piège :
demander une connexion imbriquée sans borner `first` applique un ×50 implicite par niveau.** Toujours
expliciter `first`.

Certains endpoints ont en outre une limite propre, plus basse, avec une fenêtre différente.

**En-têtes de réponse** (12 au total) : `X-RateLimit-Requests-{Limit,Remaining,Reset}`,
`X-RateLimit-Endpoint-Requests-{Limit,Remaining,Reset}` + `X-RateLimit-Endpoint-Name`, `X-Complexity`,
`X-RateLimit-Complexity-{Limit,Remaining,Reset}`.

**Dépassement : HTTP 400** (et non 429), avec le code `RATELIMITED` dans le tableau `errors` du corps.
Un client qui ne regarde que le code HTTP confondra un dépassement de quota avec une requête malformée —
il faut inspecter `errors[].extensions` pour distinguer.

### Notification de changement

**Webhooks** (`linear.app/developers/webhooks`). Modèles souscriptibles : Issues, Issue attachments,
Issue comments, Issue labels, Comment reactions, Projects, Project updates, Documents, Initiatives,
Initiative updates, Cycles, Customers, Customer requests, Users, plus les événements « Issue SLA » et
« OAuthApp revoked ».

Charge utile POST : `action` (`create` / `update` / `remove`), `type`, `actor`, `createdAt`, `data`
(entité sérialisée), `url`, et — précieux — **`updatedFrom`, les valeurs précédentes en cas de mise à
jour**. Cela permet de reconstruire un delta sans relire l'entité.

En-têtes : `Linear-Delivery` (UUID du message), `Linear-Event`, `Linear-Signature`
(HMAC-SHA256 hexadécimal du corps **brut**, signé avec le secret du webhook, à comparer en temps
constant), `Linear-Timestamp` (Unix, millisecondes). Le champ `webhookTimestamp` doit être validé à
±1 minute environ pour contrer le rejeu.

**Livraison au plus une fois n'est pas garantie ; Linear ne garantit pas l'exactement-une-fois.** Un
échec (serveur indisponible, réponse au-delà de **5 secondes**, statut non-200) déclenche jusqu'à
**3 relances**, espacées de **1 minute, 1 heure, 6 heures**. Un webhook durablement muet peut être
désactivé automatiquement. **Le consommateur doit donc déduplicquer sur `Linear-Delivery`** et répondre
en moins de 5 s (accuser réception, traiter en tâche de fond).

Un client de bureau n'a en général pas d'URL publique : le webhook suppose un point d'entrée joignable.
À défaut, il reste le **polling** avec `orderBy: updatedAt` et un filtre sur `updatedAt`. Aucune
souscription GraphQL temps réel publique n'a été observée dans la documentation consultée. `⚠️ non vérifié`

---

## 8. Pièges et singularités — ce qui ne se traduira pas ailleurs

1. **L'équipe est obligatoire et structurante partout.** `Issue.team: Team!`, `WorkflowState.team:
   Team!`, `Cycle.team: Team!`. Le seul champ obligatoire à la création est `teamId`, pas même le titre.
   Une abstraction commune doit donc porter une notion de « conteneur obligatoire » que GitHub (dépôt),
   GitLab (projet) et Jira (projet) remplissent différemment — et surtout : **les états de Linear sont
   par équipe**, donc « À faire » n'a pas le même UUID d'une équipe à l'autre. Un mapping d'états
   figé au niveau de l'organisation est impossible ; il faut mapper via `WorkflowState.type`, la seule
   taxonomie fermée et stable.

2. **Statut = colonne de board.** Il n'y a qu'un seul objet. Chez Jira, statut et colonne sont deux
   choses distinctes (une colonne agrège plusieurs statuts) ; chez GitHub Projects, le champ « Status »
   est un champ de projet **détaché** de l'état ouvert/fermé de l'issue. Le modèle abstrait devra
   choisir : soit il adopte le modèle Linear (fusionné) et devra synthétiser la colonne chez les autres,
   soit il sépare et devra dupliquer chez Linear.

3. **Aucune transition contrainte.** Il n'existe aucun type de transition dans le schéma. « Passer une
   tâche à l'état X » est un simple champ. Jira, à l'inverse, impose un graphe de transitions, une API
   dédiée (`/transitions`) et parfois des champs obligatoires à l'écran de transition. **Une abstraction
   qui modélise le changement d'état comme une simple écriture de champ fonctionnera chez Linear et
   cassera chez Jira.** Prévoir une opération « changer d'état » de premier ordre, pas un `SetField`.

4. **`identifier` (`BLA-123`) est instable et l'API l'accepte quand même.** Le confort d'appeler
   `issueUpdate(id: "BLA-123")` est un piège pour un cache local : après un déplacement d'équipe, la
   clé ne désigne plus rien (ou pire, désigne une autre issue si le numéro a été réattribué chez le
   voisin). Journaliser l'UUID, toujours. Noter aussi l'incohérence interne : `issueBatchUpdate` exige
   des `UUID!` là où `issueUpdate` accepte une `String`.

5. **Zéro clé d'idempotence sur les écritures de tâches.** Le seul mécanisme est l'`id` fourni par le
   client dans `IssueCreateInput` / `CommentCreateInput` / `IssueLabelCreateInput`. C'est un pattern
   différent d'une clé d'idempotence classique (pas de rejeu qui renvoie le résultat mémorisé, mais une
   collision de clé primaire). **Pour Cursus, cela impose de générer l'UUID côté client avant l'appel et
   de le persister dans le journal avant émission** — un ordre d'opérations que les trois autres outils
   n'imposeront peut-être pas, mais qui est le plus sûr partout.

6. **Exclusivité intra-groupe des étiquettes.** « Only one label from a given label group can be applied
   to an issue at a time. » Poser `Type/Bug` retire silencieusement `Type/Feature`. Une abstraction qui
   traite les étiquettes comme un ensemble libre produira des retraits invisibles. Il faut modéliser le
   groupe (`IssueLabel.parent`, `isGroup`) et la portée nullable de `team` — deux notions qu'un modèle
   d'étiquettes plates ne peut pas exprimer.

7. **Trois ordres de tri simultanés en flottants.** `sortOrder`, `subIssueSortOrder`,
   `prioritySortOrder`, plus `boardOrder` déprécié. Aucune opération relationnelle (« placer après
   l'issue X ») : le client calcule le flottant, avec le risque classique d'épuisement de précision
   après de nombreuses insertions au même point. Peu d'outils exposent une position adressable ; c'est
   une capacité que l'abstraction devra rendre optionnelle.

8. **Pas d'epic.** `Initiative → Project → Issue`, avec récursivité **uniquement** sur `Initiative` (5
   niveaux) et sur `Issue` (profondeur non documentée). Un « epic » Jira n'a pas d'équivalent unique :
   selon l'usage, il se projette sur un `Project` ou sur une issue parente. Choisir, documenter le
   choix, et accepter que le retour ne sera pas fidèle.

9. **Héritage silencieux à la création de sous-issue.** « Sub-issues inherit the parent issue's team,
   priority, and project. » Créer une sous-issue en ne fournissant que `teamId` et `parentId` produit
   une issue dont `project` et `priority` ne sont pas ceux qu'on a demandés (rien) mais ceux du parent.
   Et l'auto-clôture du parent quand toutes les sous-issues sont terminées signifie qu'**une écriture
   sur une sous-issue peut modifier une autre issue** — un webhook arrivera pour le parent sans qu'on
   l'ait touché.

10. **Dépassement de quota renvoyé en HTTP 400.** Non-standard, et facile à confondre avec une erreur de
    requête. Le coût est en outre en **points de complexité**, avec un multiplicateur ×50 implicite par
    connexion non bornée : une requête qui « marche » en développement peut brûler le budget horaire en
    production. Toujours expliciter `first`, toujours lire `X-Complexity`.

11. **Beaucoup d'horodatages dérivés et non écrivables en pratique.** `startedAt`, `completedAt`,
    `canceledAt`, `triagedAt`… sont maintenus par le serveur au fil des changements d'état. Un modèle
    abstrait qui les traite comme des champs libres écrira des valeurs qui seront écrasées.

12. **`dueDate: TimelessDate`** — une date sans heure ni fuseau, type scalaire dédié. Le mapper sur un
    `DateTimeOffset` C# introduira un décalage d'un jour selon le fuseau. Utiliser `DateOnly`.

13. **Notion d'agent de première classe.** `Issue.agentSessions`, `Issue.delegate: User`, portées
    `app:assignable` / `app:mentionable`, `actor=app`. Linear modélise déjà l'assignation d'une tâche à
    une application — pertinent pour Cursus, mais **sans équivalent** chez les trois autres. À ne pas
    faire remonter dans l'abstraction commune ; à traiter comme une capacité spécifique.

---

## Sources consultées

- <https://linear.app/developers/graphql> — page d'accueil de l'API : authentification, objet Issue,
  identifiant vs UUID, exemples de requêtes et mutations.
- <https://linear.app/developers/rate-limiting> — quotas chiffrés, calcul de complexité, en-têtes,
  code d'erreur `RATELIMITED`.
- <https://linear.app/developers/pagination> — pagination Relay, `pageInfo`, défaut de 50, `orderBy`.
- <https://linear.app/developers/webhooks> — modèles souscriptibles, charge utile, signature
  HMAC-SHA256, politique de relance.
- <https://linear.app/developers/oauth-2-0-authentication> — portées OAuth, durée de vie des jetons,
  refresh token, `actor=app`.
- <https://raw.githubusercontent.com/linear/linear/master/packages/sdk/src/schema.graphql> — schéma
  GraphQL officiel publié dans le SDK (source de tous les noms de types, champs, inputs et mutations
  cités, ainsi que des docstrings sur `WorkflowState.type`, `boardOrder`, `issueBatchUpdate`,
  `IssueLabel.organization`).
- <https://linear.app/docs/parent-and-sub-issues> — héritage équipe/priorité/projet, auto-clôture ;
  ne documente **pas** de profondeur maximale.
- <https://linear.app/docs/labels> — portée espace de travail vs équipe, groupes de labels, exclusivité
  intra-groupe, plafond de 250 labels par groupe, création à la volée.
- <https://linear.app/docs/sub-initiatives> et <https://linear.app/changelog/2026-04-09-multi-level-sub-teams>
  — imbrication sur cinq niveaux pour les initiatives et les sous-équipes (relevés via résultats de
  recherche sur le domaine officiel, pages non ouvertes intégralement).

*Note : l'ancien domaine `developers.linear.app` renvoie un 301 vers `linear.app/developers`. Les liens
en circulation vers l'ancienne documentation restent valides par redirection.*
