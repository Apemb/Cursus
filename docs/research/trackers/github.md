# GitHub — modèle de données des tâches (Issues + Projects v2) et API

**Périmètre** : l'*issue* comme objet « tâche », les *Projects v2* comme porteurs des colonnes et
champs personnalisés. Les pull requests ne sont évoquées que là où elles cohabitent avec les issues
dans un projet.

**Convention de fiabilité** : tout énoncé non suivi d'une marque provient d'une page de documentation
effectivement lue (listée en fin de fiche). Les énoncés marqués `⚠️ non vérifié` viennent de la
mémoire d'entraînement et **doivent être re-testés avant d'être encodés dans du code**. Les énoncés
marqués `↪️ déduit` sont des inférences à partir de faits lus (typiquement la sémantique HTTP), pas
des citations.

---

## 1. L'objet « tâche » — l'issue

### Champs canoniques (REST, objet `issue`)

La réponse d'un `GET /repos/{owner}/{repo}/issues/{issue_number}` contient notamment :

`id`, `node_id`, `url`, `repository_url`, `labels_url`, `comments_url`, `events_url`, `html_url`,
`number`, `state`, `state_reason`, `title`, `body`, `user`, `labels`, `assignees`, `milestone`,
`locked`, `active_lock_reason`, `comments`, `pull_request`, `closed_at`, `created_at`, `updated_at`,
`draft`, `closed_by`, `body_html`, `body_text`, `timeline_url`, `type`, `repository`,
`performed_via_github_app`, `author_association`, `reactions`, `sub_issues_summary`,
`parent_issue_url`, `pinned_comment`, `issue_dependencies_summary`.

Trois de ces champs méritent l'attention d'un intégrateur :

- `pull_request` — **une PR est aussi une issue** côté REST. Les endpoints de liste d'issues
  retournent les PR ; leur présence se détecte par ce champ. (Documenté sur la page REST Issues.)
- `sub_issues_summary` et `parent_issue_url` — la hiérarchie est reflétée dans l'objet lui-même.
- `type` — le type d'issue (voir plus bas), lisible et **écrivable** en REST.

Côté GraphQL, l'objet `Issue` expose entre autres : `id`, `number`, `title`, `body`, `state`,
`stateReason`, `url`, `labels`, `assignees`, `milestone`, `issueType`, `parent`, `subIssues`,
`projectItems`, `trackedIssues`.

### Identité — le point structurant

GitHub a **trois identifiants** pour une même issue, et ils ne se valent pas :

| Identifiant | Portée | Stabilité |
|---|---|---|
| `number` | **par dépôt** — l'issue #42 de `a/b` et l'issue #42 de `c/d` n'ont aucun rapport | change lors d'un transfert de dépôt `⚠️ non vérifié` ; l'existence de la mutation `transferIssue(issueId, repositoryId)` rend le cas réel |
| `id` (REST, entier) / `databaseId` | global | stable |
| `node_id` (REST) = `id` (GraphQL, `ID!`) | global | stable ; **c'est la clé qu'attendent toutes les mutations GraphQL et les endpoints Projects** |

**Conséquence pour une abstraction multi-outils** : la clé de corrélation d'une tâche GitHub ne peut
pas être « le numéro ». Elle doit être le triplet `(owner, repo, number)` pour l'affichage humain et
l'URL, et le `node_id` pour toute écriture. Stocker les deux : le `node_id` est indispensable dès
qu'on touche aux Projects (qui ne connaissent que lui côté GraphQL) ; le triplet est indispensable
pour les endpoints REST d'issues, qui ne savent pas adresser par `node_id`.

`html_url` est l'URL canonique visible (`https://github.com/{owner}/{repo}/issues/{number}`)
`↪️ déduit` de la forme des chemins REST ; `url` est l'URL d'API.

### Types d'issues

Fonctionnalité distincte des étiquettes, introduite récemment :

- définis **au niveau de l'organisation**, pas du dépôt ni du projet ;
- jusqu'à **25 types** par organisation ;
- trois types par défaut : *task*, *bug*, *feature*, éditables/désactivables/supprimables ;
- une issue semble n'en porter **qu'un seul à la fois** (le vocabulaire de la doc — « ajouter *un*
  type à une issue » — le suggère sans l'affirmer) ;
- **écrivable par API** : paramètre `type` (string ou null) sur `POST` et `PATCH` d'une issue ; champ
  `issueType` en GraphQL ; événements de webhook `typed` / `untyped`.

C'est la seule « taxonomie » GitHub qui soit **mono-valuée et de portée organisation** — donc la seule
qui ressemble à un « type de tâche » Jira. Elle n'existe pas pour un compte personnel `⚠️ non vérifié`.

### Le ProjectV2Item — l'enveloppe

Une issue placée dans un projet n'y est pas présente directement : elle est **enveloppée** dans un
`ProjectV2Item`, objet à part entière avec sa propre identité.

Champs de `ProjectV2Item` (GraphQL) : `id` (ID!), `content` (`ProjectV2ItemContent` — union
draft issue / issue / pull request), `fieldValueByName`, `fieldValues`
(`ProjectV2ItemFieldValueConnection!`), `isArchived` (Boolean!), `project` (`ProjectV2!`), `type`
(`ProjectV2ItemType!`), `createdAt`, `updatedAt`, `creator`.

`ProjectV2ItemType` : `DRAFT_ISSUE`, `ISSUE`, `PULL_REQUEST`, `REDACTED`.

En REST (endpoints `projectsV2`), l'item porte : `id`, `node_id`, `content_type` (`Issue`,
`PullRequest`, `DraftIssue`), `content`, `creator`, `created_at`, `updated_at`, `archived_at`,
`project_url`, `item_url`, `fields`.

Deux conséquences immédiates :

1. **Un item ≠ une issue.** L'`itemId` est propre au couple (projet, issue). Une issue dans trois
   projets a trois `itemId` distincts, et trois jeux de valeurs de champs indépendants.
2. **La draft issue** (`DRAFT_ISSUE`) est une tâche qui n'existe *que* dans le projet : pas de dépôt,
   pas de `number`, pas d'étiquette. Elle peut être convertie en issue plus tard (action de webhook
   `converted`). Un modèle commun doit décider si elle est une « tâche » ou pas.

---

## 2. Hiérarchie

### Sub-issues — une vraie relation

Depuis l'arrivée des *sub-issues*, la parenté est **une relation de première classe dans l'API**, pas
une convention de texte :

- REST dédié :
  - `GET /repos/{owner}/{repo}/issues/{issue_number}/sub_issues`
  - `POST /repos/{owner}/{repo}/issues/{issue_number}/sub_issues` — corps : `sub_issue_id`
  - `DELETE /repos/{owner}/{repo}/issues/{issue_number}/sub_issue` (**singulier**, piège) — corps :
    `sub_issue_id`
  - `PATCH /repos/{owner}/{repo}/issues/{issue_number}/sub_issues/priority` — corps :
    `sub_issue_id` + `after_id` **ou** `before_id`
- GraphQL : `addSubIssue(issueId!, …)`, `removeSubIssue(issueId!, subIssueId!)`,
  `reprioritizeSubIssue(issueId!, subIssueId!, direction!)`.
- Champs de lecture : `parent`, `subIssues` (GraphQL) ; `parent_issue_url`, `sub_issues_summary`
  (REST). Un endpoint REST de récupération du parent a été ajouté en septembre 2025.
- Webhook `sub_issues` avec les actions `parent_issue_added`, `parent_issue_removed`,
  `sub_issue_added`, `sub_issue_removed`.

**Noter l'asymétrie d'identifiants** : le parent est désigné par son `issue_number` (dans le chemin,
donc implicitement dans son dépôt), l'enfant par son `sub_issue_id` (identifiant global). C'est
cohérent avec le fait que l'enfant peut vivre ailleurs.

Limites documentées :

- **100 sous-issues par parent** ;
- **8 niveaux** d'imbrication ;
- un enfant **peut** être dans un autre dépôt que son parent (et, depuis septembre 2025, dans une
  autre organisation) ;
- **un seul parent par issue** : la doc ne l'affirme pas explicitement, mais l'existence d'un champ
  singulier `parent` / `parent_issue_url` et de l'action `parent_issue_removed` le rend très
  probable `⚠️ non vérifié` — à confirmer avant de modéliser un DAG.

Effet de bord annoncé en septembre 2025 : **une sous-issue hérite automatiquement du Project et du
Milestone de son parent**. Un outil qui écrit en masse doit s'attendre à des mutations de champs
qu'il n'a pas demandées.

### Milestones

Rattachement, pas hiérarchie : `milestone` est un champ **simple-valué** de l'issue (`null`, string ou
integer en écriture REST), de **portée dépôt** comme les étiquettes. Webhook `milestone` avec les
actions `closed`, `created`, `deleted`, `edited`, `opened` ; sur l'issue, actions `milestoned` /
`demilestoned`.

Un milestone est donc l'équivalent le plus proche d'un « sprint » ou d'une « version », mais il ne
peut pas franchir la frontière du dépôt.

### Cases à cocher dans le corps markdown

Historiquement, la seule façon de décomposer une issue était une liste `- [ ]` dans le `body`, avec
éventuellement des références `#123` (les « task lists »). Ce mécanisme est **purement textuel** :
aucune relation d'API, la seule trace structurée étant `trackedIssues` en GraphQL. Il est aujourd'hui
supplanté par les sub-issues.

**Pour un modèle commun** : ne jamais parser le markdown pour reconstruire une hiérarchie. Deux issues
peuvent afficher la même décomposition visuelle, l'une structurée et l'autre pas.

### Vraie hiérarchie vs rattachement

| Mécanisme | Nature |
|---|---|
| sub-issues | **vraie** relation parent/enfant, ≤ 8 niveaux, cross-repo |
| milestone | rattachement N:1, portée dépôt |
| projet | rattachement N:N via `ProjectV2Item` |
| étiquette | rattachement N:N, portée dépôt |
| cases à cocher | rien du tout — du texte |

---

## 3. Étiquettes (`labels`)

- **Portée dépôt.** Les endpoints le disent par leur forme même : `GET|POST /repos/{owner}/{repo}/labels`,
  `PATCH|DELETE /repos/{owner}/{repo}/labels/{name}`.
- Objet `label` : `id` (int64), `node_id`, `url`, `name`, `description` (string ou null), `color`
  (string — hexadécimal sans `#` `⚠️ non vérifié`), `default` (bool).
- **Multi-valuées** : une issue porte un tableau de labels.
- Création : `POST /repos/{owner}/{repo}/labels`, obligatoires `name` **et** `color`.
- **L'identifiant fonctionnel d'une étiquette est son `name`** : c'est le `name` qui figure dans le
  chemin de suppression et de mise à jour, et dans le tableau `labels` d'une issue. Renommer une
  étiquette casse donc toute référence externe stockée par nom.

Endpoints de pose/retrait : voir section 5.

### Conséquence de la portée dépôt pour un agrégateur

C'est le principal écart de modèle. Un outil qui présente une vue unifiée sur N dépôts doit décider :

- soit il expose les étiquettes comme des couples `(dépôt, nom)` — honnête, mais l'utilisateur voit
  cinq fois « bug » avec cinq couleurs ;
- soit il fusionne par nom — pratique, mais la fusion est **fausse** : deux étiquettes homonymes dans
  deux dépôts n'ont ni le même `id`, ni forcément la même description ou couleur, et « poser
  l'étiquette bug » sur une issue d'un dépôt qui ne l'a pas est un cas d'erreur, pas un cas nominal.

Il n'existe **aucune** étiquette de niveau organisation. Le seul vocabulaire transversal disponible
est celui des *types d'issues* (mono-valué, 25 max) et des champs single-select d'un Project (portée
projet). `⚠️ non vérifié` sur l'absence totale d'étiquettes d'organisation — c'est une absence, donc
non démontrable par lecture ; aucune page lue n'en mentionne.

**Création à la volée** : le comportement des paramètres `labels` de `POST`/`PATCH` d'une issue face à
un nom inexistant n'est **pas documenté sur la page lue**. La mémoire d'entraînement dit que GitHub
crée l'étiquette manquante à la volée sur l'endpoint `POST .../issues/{n}/labels`, et qu'un
utilisateur sans droit d'écriture voit ses labels silencieusement ignorés à la création d'issue —
`⚠️ non vérifié`, **à tester**, car un « silencieusement ignoré » est exactement le genre de piège qui
fait mentir une couche d'abstraction.

---

## 4. États et colonnes — le point critique

### L'issue ne connaît que deux états

`state` : **`open` ou `closed`**. C'est tout. Pas de « in progress », pas de workflow.

`state_reason` : `completed`, `not_planned`, `duplicate`, `reopened`, ou `null`. C'est une
qualification de la fermeture (et de la réouverture), pas un état supplémentaire.

Il n'y a **aucun workflow contraint** sur l'issue : pas de transitions autorisées, pas de schéma
d'états, pas de garde. `PATCH state=closed` puis `PATCH state=open` sont toujours acceptés
`↪️ déduit` de l'absence totale de mention de transitions dans la documentation REST des issues.

### La « colonne » vit ailleurs

La colonne d'un tableau kanban est **une option d'un champ single-select du projet**, dont la valeur
est portée par le **`ProjectV2Item`**, pas par l'issue.

- Le champ s'appelle par convention `Status`. Ses options par défaut sont **Todo / In Progress /
  Done** (vues dans les exemples de la page « Using the API to manage Projects », qui renvoie un
  `ProjectV2SingleSelectField` nommé `Status` avec ces options).
- Un champ single-select accepte jusqu'à **50 options**, chacune avec **nom, couleur et description**.
- Un projet accepte jusqu'à **50 champs** au total, métadonnées intégrées comprises.
- Types de champs personnalisés : texte, nombre, date, single select, iteration.
- Les vues sont un habillage : tableau, kanban, roadmap. **La colonne n'est pas un objet** — c'est une
  option de champ, et le regroupement en colonnes est une propriété de la *vue*.
- Un projet accepte jusqu'à **50 000 items**, vues actives et archive confondues.

Autrement dit : `Status` n'a rien de privilégié dans le modèle. C'est un champ single-select comme un
autre, qu'on peut renommer, dont on peut supprimer les options, et qu'un projet pourrait ne pas
avoir. **Un outil ne doit pas coder en dur « le champ Status »** ; il doit le résoudre par nom (via
`fieldValueByName` ou l'endpoint REST `fields`) et gérer son absence.

### Ordre dans une colonne

Oui, la position est adressable : mutation `updateProjectV2ItemPosition(projectId: ID!, itemId: ID!,
afterId: ID)`. `afterId` est **nullable** — l'omettre place vraisemblablement l'item en tête
`⚠️ non vérifié`. L'ordre est donc un ordre *relatif par insertion*, pas un entier de rang exposé.
Le webhook `projects_v2_item` a une action `reordered`, ce qui confirme que le réordonnancement est un
événement observable.

### Automatisations

Il existe des automatisations intégrées, mais ce sont des **déclencheurs qui posent une valeur de
champ**, pas une machine à états :

- mettre le statut à *Todo* quand un item est ajouté ;
- mettre le statut à *Done* quand une issue est fermée ou une PR fusionnée ;
- ajout automatique d'items d'un dépôt selon un filtre ; archivage automatique.

Aucune contrainte de transition n'est documentée : on peut passer d'une option à n'importe quelle
autre. La documentation lue **ne dit pas** si ces automatisations se déclenchent aussi sur des
modifications faites par API — à tester, car c'est ce qui décide si `close(issue)` provoque en cascade
un déplacement de colonne qu'on n'a pas demandé.

### Une issue dans plusieurs projets

C'est le cas nominal, pas l'exception : `Issue.projectItems` est une **connexion** (pluriel). Chaque
projet a son propre `ProjectV2Item`, ses propres champs, son propre statut, son propre ordre.

Il n'existe donc **pas de réponse** à la question « quel est le statut de cette issue ? » — seulement
à « quel est son statut *dans le projet P* ». Toute abstraction commune qui expose un `Status` unique
doit exiger un contexte de projet, ou choisir arbitrairement (et documenter le choix).

---

## 5. Écriture

### Créer une issue

- REST : `POST /repos/{owner}/{repo}/issues` — **obligatoire : `title`**. Optionnels : `body`,
  `assignees` (array de string), `milestone` (null/string/int), `labels` (array), `type`
  (string/null).
- GraphQL : `createIssue` — non-null : `repositoryId` (ID!), `title` (String!).
- **Non idempotent.** Deux appels identiques créent deux issues. Aucune clé d'idempotence
  documentée sur la page lue. Un outil rejouable doit tenir son propre registre de corrélation
  (ex. marqueur dans le `body` ou table locale) et vérifier avant de créer. Attention aussi aux
  *secondary rate limits* sur les endpoints créateurs de contenu (voir §7).

### Éditer les champs

- REST : `PATCH /repos/{owner}/{repo}/issues/{issue_number}` — **aucun paramètre obligatoire** hormis
  ceux du chemin. Accepte `title`, `body`, `state`, `state_reason`, `milestone`, `labels`,
  `assignees`, `type`.
- GraphQL : `updateIssue` — non-null : `id` (ID!).
- **Idempotent** `↪️ déduit` : ce sont des affectations de valeur absolue (le `PATCH` remplace,
  `labels` et `assignees` étant des tableaux complets, pas des deltas). Rejouer le même corps
  reconverge sur le même état. Attention : c'est aussi un **écrasement** — envoyer `labels` écrase la
  liste entière, y compris ce qu'un autre acteur a posé entre-temps (perte de mise à jour
  silencieuse, il n'y a pas d'ETag conditionnel documenté sur la page lue).

### Déplacer de colonne (écrire un champ de `ProjectV2Item`)

Deux voies, **désormais** :

- **GraphQL** : `updateProjectV2ItemFieldValue(projectId: ID!, fieldId: ID!, itemId: ID!,
  value: ProjectV2FieldValue!)`. Forme de `value` selon le type : `{ text: "…" }` pour texte/nombre/
  date, `{ singleSelectOptionId: "…" }` pour un single-select, `{ iterationId: "…" }` pour une
  itération. Pour vider : `clearProjectV2ItemFieldValue(projectId!, fieldId!, itemId!)`.
- **REST** (annoncé le 11 septembre 2025) :
  `PATCH /orgs/{org}/projectsV2/{project_number}/items/{item_id}` — corps : `fields` (requis), tableau
  d'objets `{ id, value }`. « For text, number, and date fields, provide the new value directly. For
  single select and iteration fields, provide the ID of the option or iteration. »

Dans les deux cas, **il faut connaître trois identifiants** : le projet, le champ, et l'item. Aucun ne
se devine à partir de l'issue : il faut lister les projets, lister les champs (et leurs options), puis
retrouver l'item. C'est trois requêtes de résolution avant la moindre écriture — à mettre en cache.

**Idempotent** `↪️ déduit` : affectation d'une valeur absolue par identifiant d'option.

Opérations voisines : `addProjectV2ItemById(projectId: ID!, contentId: ID!)` (REST : `POST
.../items` avec `type` + `id`, ou `type` + `owner` + `repo` + `number`),
`deleteProjectV2Item(projectId!, itemId!)`, `archiveProjectV2Item(projectId!, itemId!)`,
`addProjectV2DraftIssue(projectId: ID!, title: String!, body, assigneeIds)`.

`addProjectV2ItemById` est **idempotent de fait** `⚠️ non vérifié` : ajouter deux fois la même issue
ne devrait pas créer deux items (un item est identifié par son contenu dans un projet donné) — mais la
doc lue ne l'affirme pas ; à tester, car la réponse conditionne toute stratégie de reprise.

### Poser / retirer une étiquette

- Ajouter : `POST /repos/{owner}/{repo}/issues/{issue_number}/labels`, corps `labels` (array).
  **Additif** → **idempotent** `↪️ déduit` (reposer une étiquette déjà présente ne peut pas la
  dupliquer, un tableau de labels étant un ensemble).
- Remplacer : `PUT /repos/{owner}/{repo}/issues/{issue_number}/labels`, corps `labels`.
  **Idempotent mais destructif** — écrase l'ensemble.
- Retirer une : `DELETE /repos/{owner}/{repo}/issues/{issue_number}/labels/{name}`.
  Idempotent au sens de l'état final ; renvoie probablement 404 si absente `⚠️ non vérifié`.
- Retirer toutes : `DELETE /repos/{owner}/{repo}/issues/{issue_number}/labels`.
- GraphQL : `addLabelsToLabelable(labelableId: ID!, …)`,
  `removeLabelsFromLabelable(labelableId: ID!, …)` — le tableau `labelIds` est présumé requis
  `⚠️ non vérifié` (la page lue ne liste comme non-null que `labelableId`).

**Le piège** : l'étiquette doit préexister *dans le dépôt de l'issue*. Poser « bug » suppose de savoir
que ce dépôt-là a une étiquette nommée « bug ».

### Commenter

- REST : `POST /repos/{owner}/{repo}/issues/{issue_number}/comments` `⚠️ non vérifié` (endpoint non lu
  — la page Issues consultée ne couvre que les issues elles-mêmes ; la page « Issue comments » existe
  mais n'a pas été ouverte).
- GraphQL : `addComment(subjectId: ID!, body: String!)` — vu et vérifié.
- **Non idempotent** : rejouer crée un doublon. Même remarque que pour la création d'issue.
- Webhook `issue_comment`, actions `created`, `deleted`, `edited`, `pinned`, `unpinned`.

### Fermer / rouvrir

- REST : `PATCH .../issues/{issue_number}` avec `state: "closed"` (+ éventuellement
  `state_reason: "completed" | "not_planned" | "duplicate"`), ou `state: "open"`
  (`state_reason: "reopened"`).
- GraphQL : `closeIssue(issueId: ID!)`, `reopenIssue(issueId: ID!)`.
- **Idempotent** `↪️ déduit` : fermer une issue déjà fermée reconverge sur le même état. Mais
  l'opération n'est pas *sans effet* : elle peut déclencher une automatisation de projet (statut →
  Done) et génère des événements de timeline / webhooks à chaque appel `⚠️ non vérifié` pour
  l'émission de webhook sur un no-op.

### Récapitulatif d'idempotence

| Opération | Rejouable sans dégât ? |
|---|---|
| Créer une issue | **Non** — duplication |
| Commenter | **Non** — duplication |
| `PATCH` d'issue (titre, corps, assignés, jalon, type) | Oui, mais **écrase** (perte de mise à jour concurrente) |
| `PUT` labels | Oui, écrase |
| `POST` labels (additif) | Oui |
| `DELETE` label | Oui |
| Fermer / rouvrir | Oui (effets de bord d'automatisation possibles) |
| `updateProjectV2ItemFieldValue` | Oui |
| `addProjectV2ItemById` | Probablement `⚠️ non vérifié` |
| `addSubIssue` | Probablement `⚠️ non vérifié` |
| `updateProjectV2ItemPosition` | Oui (position absolue relative à `afterId`) |

---

## 6. Authentification

Quatre mécanismes coexistent, et leur traitement des Projects diffère de celui des issues.

### PAT classique (scopes OAuth)

- `repo` — « full access to public and private repositories including read and write access to code,
  commit statuses, repository invitations, collaborators, deployment statuses, and repository
  webhooks ». La page lue **ne mentionne pas explicitement les issues** dans cette énumération, mais
  `repo` est en pratique le scope qui les couvre `⚠️ non vérifié`.
- `public_repo` — même chose limité aux dépôts publics.
- **`project`** — « Grants read/write access to user and organization projects ».
- **`read:project`** — lecture seule.

**Point important : `repo` ne suffit pas pour les Projects.** Il faut ajouter `project` (ou
`read:project`), ce que la page « Using the API to manage Projects » confirme explicitement. C'est un
scope *séparé*, à demander dès le départ — un utilisateur qui a créé son jeton sans lui devra le
recréer.

### PAT *fine-grained*

- Issues : permission **de dépôt** « Issues », en `read` ou `write`.
- Projects : permission **d'organisation** « Projects », en `read` ou `write`. Il n'y a **pas** de
  permission « Projects » au niveau dépôt dans la page lue.
- **Trou identifié** : aucune permission ne semble couvrir les **projets appartenant à un
  utilisateur** (par opposition à une organisation). Un projet personnel serait donc inaccessible à un
  PAT fine-grained `⚠️ non vérifié` — c'est une absence, à confirmer par test. Si elle se confirme,
  c'est un argument fort pour supporter aussi le PAT classique.
- Les PAT fine-grained ont une **expiration obligatoire** `⚠️ non vérifié`.

### GitHub App

Quotas plus élevés (§7), permissions granulaires équivalentes à celles des PAT fine-grained, jetons
d'installation **de courte durée** (1 h `⚠️ non vérifié`) dérivés d'un JWT signé par une clé privée.

### OAuth App

Scopes identiques au PAT classique. Quota par app (§7).

### Ce qu'un client de bureau doit stocker durablement

Selon le mécanisme :

- **PAT** (classique ou fine-grained) : la chaîne du jeton, dans le trousseau du système (Keychain sur
  macOS, Secret Service sur Linux) — jamais dans un fichier de configuration. Prévoir une date
  d'expiration et un message clair à la révocation (401).
- **OAuth App / GitHub App user-to-server** : un `refresh_token` durable + un `access_token`
  éphémère `⚠️ non vérifié` (le détail du flux n'a pas été lu). C'est la seule voie qui donne une
  rotation propre sans réintervention de l'utilisateur.
- Dans tous les cas : le **scope effectif** du jeton (pour détecter à l'avance qu'un jeton sans
  `project` ne permettra pas de déplacer une carte, plutôt que d'échouer au moment de l'écriture).

---

## 7. Transport et limites

### REST vs GraphQL — l'affirmation « Projects v2 = GraphQL seulement » est PÉRIMÉE

C'est le point à corriger le plus fermement dans toute documentation antérieure à septembre 2025.

Le 11 septembre 2025, GitHub a annoncé **une API REST pour les Projects**. Elle permet de « List
projects and get information about a specific project, its fields, and its items », « Add and delete
issues and pull requests from a project », « Update field values for project items ».

Endpoints REST Projects v2 lus :

| Objet | Endpoints |
|---|---|
| Projets | `GET /orgs/{org}/projectsV2`, `GET /orgs/{org}/projectsV2/{project_number}`, idem sous `/users/{username}/` — **lecture seule** |
| Champs | `GET`/`POST /orgs/{org}/projectsV2/{project_number}/fields`, `GET .../fields/{field_id}`, idem `/users/{username}/` |
| Items | `GET`/`POST .../items`, `GET`/`PATCH`/`DELETE .../items/{item_id}`, `GET .../views/{view_number}/items` |

Aucune mention de *preview* ou de *deprecation* sur les pages lues. Il subsiste néanmoins des zones
GraphQL-seulement `⚠️ non vérifié` : réordonnancement d'item (`updateProjectV2ItemPosition` n'a pas
d'équivalent REST dans les pages lues), archivage d'item, création de draft issue, modification d'un
champ existant ou de ses options (les POST créent, aucun endpoint de mise à jour de champ n'est
documenté).

**Recommandation pour Cursus** : REST pour les issues (mieux documenté, plus simple à cacher),
GraphQL pour les Projects — ne serait-ce que parce que le monde GraphQL est celui des `node_id`, que
les issues exposent déjà, ce qui évite un aller-retour de résolution. Prévoir néanmoins que la couche
d'accès puisse mélanger les deux transports pour un même cas d'usage.

À noter : les **Projects (classic)** ont fait l'objet d'un avis de sunset en mai 2024 — ne pas les
implémenter.

### Pagination

- REST : pagination par pages/curseurs via l'en-tête `Link` `⚠️ non vérifié` (page « Using pagination
  in the REST API » non lue).
- GraphQL : connexions à curseurs. `first` et `last` doivent être **entre 1 et 100**. Un appel ne peut
  demander plus de **500 000 nœuds** au total.

### Quotas — chiffres documentés

**REST — limites primaires**

| Acteur | Limite |
|---|---|
| Non authentifié | 60 requêtes/heure |
| PAT authentifié | 5 000 requêtes/heure |
| GitHub App (installation) | base 5 000/h, +50/h par dépôt au-delà de 20 et +50/h par utilisateur au-delà de 20, **plafond 12 500/h** |
| GitHub App, Enterprise Cloud | 15 000/h |
| OAuth App (client id/secret, données publiques) | 5 000/h par app (15 000/h en Enterprise Cloud) |
| `GITHUB_TOKEN` d'Actions | 1 000/h par dépôt (15 000/h en Enterprise Cloud) |

En-têtes de réponse : `x-ratelimit-limit`, `x-ratelimit-remaining`, `x-ratelimit-used`,
`x-ratelimit-reset`, `x-ratelimit-resource`, et `retry-after` en cas de limite secondaire.

**GraphQL — limites primaires, en points**

| Acteur | Limite |
|---|---|
| Utilisateur | 5 000 points/heure |
| Utilisateur Enterprise Cloud | 10 000 points/heure |
| Installation de GitHub App | 5 000 points/heure (10 000 en Enterprise) |
| `GITHUB_TOKEN` d'Actions | 1 000 points/heure par dépôt |

**Calcul du coût en points** : « Add up the number of requests needed to fulfill each unique
connection in the call. Assume every request will reach the `first` or `last` argument limits. Divide
the number by **100** and round the result to the nearest whole number. » Le coût minimum d'un appel
est de **1 point**.

Conséquence pratique : une requête GraphQL qui traverse plusieurs connexions imbriquées
(projet → items → fieldValues) coûte cher *en supposant le pire*, indépendamment de ce qu'elle
retourne réellement. Baisser `first` ne réduit pas seulement la latence, il réduit **la facture**.
Interroger `rateLimit { limit remaining used resetAt }` dans la même requête pour instrumenter.

**Limites secondaires (les deux transports)**

- pas plus de **100 requêtes concurrentes** ;
- REST : **900 points/minute** ; GraphQL : **2 000 points/minute** ;
- **90 secondes de temps CPU par 60 secondes de temps réel** ;
- **80 requêtes créatrices de contenu par minute** et **500 par heure**.

Ce dernier chiffre est le plus contraignant pour un outil qui synchronise : créer des issues ou des
commentaires en masse butera sur 500/heure bien avant de buter sur 5 000 requêtes/heure.

### Webhooks

| Événement | Actions | Permission |
|---|---|---|
| `issues` | `assigned`, `closed`, `deleted`, `demilestoned`, `edited`, `field_added`, `field_removed`, `labeled`, `locked`, `milestoned`, `opened`, `pinned`, `reopened`, `transferred`, `typed`, `unassigned`, `unlabeled`, `unlocked`, `unpinned`, `untyped` | « Issues » (dépôt), lecture |
| `issue_comment` | `created`, `deleted`, `edited`, `pinned`, `unpinned` | « Issues » (dépôt), lecture |
| `label` | `created`, `deleted`, `edited` | « Metadata » (dépôt), lecture |
| `milestone` | `closed`, `created`, `deleted`, `edited`, `opened` | « Issues » ou « Pull requests » (dépôt) |
| `projects_v2` | `closed`, `created`, `deleted`, `edited`, `reopened` | « Projects » (**organisation**), lecture |
| `projects_v2_item` | `archived`, `converted`, `created`, `deleted`, `edited`, `reordered`, `restored` | « Projects » (organisation), lecture |
| `projects_v2_status_update` | `created`, `deleted`, `edited` | « Projects » (organisation), lecture |
| `sub_issues` | `parent_issue_added`, `parent_issue_removed`, `sub_issue_added`, `sub_issue_removed` | « Issues » (dépôt), lecture |

La charge utile de `projects_v2_item` contient un objet `projects_v2_item` et un objet `changes`, tous
deux requis. Le détail des champs de `changes` n'était pas lisible sur la page consultée.

Deux remarques importantes :

1. **Les webhooks de projet s'abonnent au niveau organisation**, ceux d'issues au niveau dépôt. Un
   outil qui veut suivre « une tâche et sa colonne » doit donc poser **deux abonnements de portées
   différentes**, avec des permissions différentes.
2. Un client de bureau ne peut en général pas recevoir de webhooks (pas d'URL publique). Il devra
   probablement **poller** — d'où l'importance des quotas ci-dessus et d'un filtre `since` sur les
   listes d'issues `⚠️ non vérifié`.

---

## 8. Pièges et singularités — ce qui ne se traduira pas ailleurs

1. **Le statut n'est pas sur la tâche.** C'est *la* singularité. Chez GitHub, l'issue n'a que
   `open`/`closed` ; la colonne est une valeur de champ portée par le `ProjectV2Item`. Une abstraction
   commune qui définit `Task.Status` doit décider ce que ce statut *est* chez GitHub : l'état binaire
   (fidèle mais inutile), ou la valeur du champ `Status` d'un projet (utile mais non canonique,
   puisqu'elle dépend du projet choisi). **Cette décision doit être prise avant d'écrire la première
   interface.** Corollaire : écrire un statut nécessite un contexte de projet que les autres outils
   n'exigent pas.

2. **Une issue vit dans N projets simultanément**, avec N jeux de valeurs de champs indépendants et N
   positions. Il n'existe pas de « statut de l'issue ». Un modèle 1:1 tâche↔statut ment.

3. **Les étiquettes sont par dépôt.** Pas de vocabulaire d'étiquettes global. Un agrégateur multi-dépôts
   fait face à des homonymes qui ne sont pas la même chose, et poser une étiquette suppose de l'avoir
   d'abord créée dans le bon dépôt. Les types d'issues (portée organisation, 25 max, mono-valués) sont
   le seul vocabulaire transversal, et il est bien plus étroit.

4. **Trois identifiants concurrents**, dont le seul « joli » (`number`) est local au dépôt et peut
   changer au transfert. Toute clé de corrélation doit être le `node_id`.

5. **Les colonnes ne sont pas des objets.** Ce sont des options d'un champ single-select, dont le nom
   (« Status ») n'a rien d'obligatoire. Un projet peut n'en avoir aucune, en avoir 50, ou nommer le
   champ autrement. Ne pas coder en dur.

6. **Aucun workflow contraint.** Pas de transitions autorisées, pas de champs obligatoires par état,
   pas de résolution imposée. Là où Jira refuse une transition, GitHub accepte tout. Une abstraction
   qui expose « transitions disponibles » n'aura rien à mettre chez GitHub, et devra probablement
   inverser la responsabilité : exposer *toutes* les options et laisser l'outil rejeter.

7. **Les pull requests se mélangent aux issues** — dans REST (champ `pull_request` sur l'objet issue)
   comme dans un projet (`ProjectV2ItemType.PULL_REQUEST`). Filtrer explicitement, sinon la liste des
   « tâches » contient des PR.

8. **Les draft issues** n'existent que dans le projet : ni dépôt, ni numéro, ni étiquette, ni URL
   d'issue. Convertibles en vraies issues, ce qui change leur identité. Aucun équivalent naturel
   ailleurs `⚠️ non vérifié` pour les trois autres outils.

9. **Résoudre avant d'écrire.** Déplacer une carte demande `projectId` + `fieldId` + `optionId` +
   `itemId`, dont aucun n'est déductible de l'issue. C'est trois requêtes de résolution et un cache à
   invalider — coût structurel absent des API où « changer le statut » est un `PATCH` sur la tâche.

10. **Le quota GraphQL se compte en points calculés sur le pire cas**, pas en requêtes. Une couche
    d'accès naïve qui demande `first: 100` partout paie plein tarif même si le projet contient trois
    items. Et les **500 créations de contenu par heure** plafonnent toute synchronisation en masse
    bien avant le quota nominal.

11. **Effets de bord non demandés** : une sous-issue hérite du projet et du jalon de son parent ; les
    automatisations intégrées repositionnent le statut à la fermeture. Une écriture peut donc en
    provoquer d'autres, invisibles depuis l'appel.

12. **Portées d'abonnement hétérogènes** : issues au niveau dépôt, projets au niveau organisation.
    Suivre une tâche de bout en bout demande deux permissions et deux abonnements.

13. **Fracture d'authentification** : `repo` ne donne pas accès aux Projects (il faut `project`), et
    les PAT fine-grained ne semblent pas couvrir les projets appartenant à un utilisateur. Le choix du
    mécanisme d'authentification détermine ce que l'outil pourra faire, avant même la première
    requête.

---

## Sources consultées

- https://docs.github.com/en/rest/issues/issues
- https://docs.github.com/en/rest/issues/sub-issues
- https://docs.github.com/en/rest/issues/labels
- https://docs.github.com/en/rest/projects/projects
- https://docs.github.com/en/rest/projects/items (et `?apiVersion=2022-11-28`)
- https://docs.github.com/en/rest/projects/fields
- https://docs.github.com/en/rest/using-the-rest-api/rate-limits-for-the-rest-api
- https://docs.github.com/en/rest/authentication/permissions-required-for-fine-grained-personal-access-tokens
- https://docs.github.com/en/graphql/reference/projects
- https://docs.github.com/en/graphql/reference/issues
- https://docs.github.com/en/graphql/overview/rate-limits-and-node-limits-for-the-graphql-api
- https://docs.github.com/en/issues/planning-and-tracking-with-projects/automating-your-project/using-the-api-to-manage-projects
- https://docs.github.com/en/issues/planning-and-tracking-with-projects/learning-about-projects/about-projects
- https://docs.github.com/en/issues/planning-and-tracking-with-projects/understanding-fields/about-single-select-fields
- https://docs.github.com/en/issues/planning-and-tracking-with-projects/automating-your-project/using-the-built-in-automations
- https://docs.github.com/en/issues/planning-and-tracking-with-projects/automating-your-project/archiving-items-automatically (via recherche — limite de 50 000 items)
- https://docs.github.com/en/issues/tracking-your-work-with-issues/using-issues/adding-sub-issues
- https://docs.github.com/en/issues/tracking-your-work-with-issues/configuring-issues/managing-issue-types-in-an-organization
- https://docs.github.com/en/apps/oauth-apps/building-oauth-apps/scopes-for-oauth-apps
- https://docs.github.com/en/webhooks/webhook-events-and-payloads
- https://github.blog/changelog/2025-09-11-a-rest-api-for-github-projects-sub-issues-improvements-and-more/
- https://github.blog/changelog/2024-05-23-sunset-notice-projects-classic/ (via recherche — sunset de Projects classic)

**Pages non ouvertes, à consulter avant implémentation** : REST « Issue comments », REST
« Milestones », REST « Using pagination », OAuth device flow / GitHub App user-to-server tokens,
schéma détaillé de la charge utile `projects_v2_item`.
