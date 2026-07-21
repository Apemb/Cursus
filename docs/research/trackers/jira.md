# Fiche technique — modèle de données des tâches dans **Jira Cloud**

Cible : **Jira Cloud** uniquement (REST API v3 `/rest/api/3/`, Agile API `/rest/agile/1.0/`).
Portée : projets *company-managed* et *team-managed*.

**Convention de fiabilité.** Tout ce qui n'est pas marqué provient d'une page de documentation
Atlassian consultée ou de la **spécification OpenAPI officielle** téléchargée
(`https://developer.atlassian.com/cloud/jira/platform/swagger-v3.v3.json`, version
`1001.0.0-SNAPSHOT-…`, et `https://developer.atlassian.com/cloud/jira/software/swagger.v3.json`,
version `1001.0.0`) — cette spécification est la source la plus fiable de la fiche, car les pages
HTML de référence sont trop volumineuses pour être récupérées intégralement. Les affirmations issues
de ma mémoire d'entraînement, ou d'un fil communautaire plutôt que de la documentation, sont
marquées **⚠️ non vérifié**.

---

## 1. L'objet « tâche »

### Vocabulaire

Historiquement **`issue`** (« ticket »). La documentation utilisateur actuelle d'Atlassian dit
désormais **« work item »** (« élément de travail »), en précisant que les deux termes sont
fonctionnellement équivalents. **L'API, elle, est restée à `issue`** partout : `/rest/api/3/issue`,
schéma `IssueBean`, événements `jira:issue_created`. Pour une abstraction, retenir **`issue`** comme
nom technique et ignorer le renommage marketing.

### Identité : trois choses distinctes

| Chose | Exemple | Stable ? |
|---|---|---|
| `id` | `"10000"` (chaîne contenant un entier) | **Oui.** C'est l'identifiant opaque. |
| `key` | `"ED-24"`, `"SMART-42"` | **Non.** Lisible par un humain, mais mutable. |
| `self` | `https://your-domain.atlassian.net/rest/api/3/issue/10000` | URL de l'API, pas de l'UI |

La clé est composée de la **clé de projet** (alphanumérique, minimum 2 caractères, commence par une
majuscule ; maximum 10 caractères selon le schéma de création de projet) et d'un **compteur
séquentiel** propre au projet.

**Point capital pour une abstraction : la clé change.** Deux causes documentées :

- **déplacement du ticket vers un autre projet** (fonction *Move*) : la clé est réécrite avec la clé
  du projet cible ; Jira conserve l'ancienne clé pour rediriger, mais elle n'est plus visible
  ailleurs que dans l'historique du ticket ;
- **renommage de la clé du projet** : toutes les clés des tickets du projet sont réécrites.

Conséquence de conception : **persister `id`, afficher `key`**. Une base locale indexée sur la clé se
désynchronise silencieusement le jour d'un *Move*. Les endpoints acceptent indifféremment l'un ou
l'autre (`{issueIdOrKey}`), ce qui rend l'erreur facile à ne pas remarquer.

- URL de consultation dans le navigateur : `https://<site>.atlassian.net/browse/<KEY>` — **⚠️ non
  vérifié** (forme connue de mémoire ; les pages consultées mentionnent « l'URL du ticket » sans en
  donner le gabarit). Le champ `self` renvoyé par l'API est l'URL **REST**, pas l'URL humaine.

### Champs canoniques

Tout est logé sous un dictionnaire `fields`. `IssueBean` expose `id`, `key`, `self`, `fields`, plus
`changelog`, `editmeta`, `transitions`, `renderedFields`, `schema`, `names`, `properties`,
`versionedRepresentations`, `operations` selon l'`expand` demandé.

Champs système visibles dans l'exemple officiel de création (`POST /rest/api/3/issue`) :

`project`, `issuetype`, `summary`, `description`, `environment`, `assignee`, `reporter`, `priority`,
`labels`, `components`, `versions` (versions affectées), `fixVersions`, `parent`, `duedate`,
`security`, `timetracking`. S'y ajoutent `status` et `resolution` (en lecture ; voir §4 et §5).

Presque tous les champs référencés sont des **objets porteurs d'un identifiant**, pas des chaînes :
`{"project": {"id": "10000"}}`, `{"issuetype": {"id": "10000"}}`, `{"priority": {"id": "20000"}}`,
`{"fixVersions": [{"id": "10001"}]}`, `{"assignee": {"id": "5b109f2e…"}}`. Les utilisateurs sont
identifiés par **`accountId`** Atlassian (l'identification par nom d'utilisateur ou courriel est
retirée pour raisons de confidentialité — la propriété `leadUserName` d'un composant est
explicitement notée comme supprimée). `labels` est la rare exception : un tableau de chaînes nues.

### Champs personnalisés

- Nommés **`customfield_<id numérique>`** (ex. `customfield_10010`) dans le dictionnaire `fields`.
  **L'identifiant est propre au site** : le même champ « Story points » n'a pas le même numéro sur
  deux instances Jira. Un client doit donc **résoudre les champs à l'exécution** (`GET
  /rest/api/3/field`, `GET /rest/api/3/field/search`) et jamais coder un numéro en dur.
- `FieldDetails` fournit `id`, `key`, `name`, `custom` (booléen), `schema`, `searchable`,
  `orderable`, `navigable`, `clauseNames` (noms utilisables en JQL) et **`scope`** — c'est `scope`
  qui trahit un champ propre à un projet *team-managed*.
- Le **type** du champ décide du format de la valeur : `{"value": "red"}` pour une liste à choix
  unique, un tableau d'identifiants pour un choix multiple, une chaîne pour une date, et **un
  document ADF pour un `textarea`**.
- Découvrir ce qui est autorisé : `GET /rest/api/3/issue/createmeta/{projectIdOrKey}/issuetypes/{issueTypeId}`
  (création) et `GET /rest/api/3/issue/{issueIdOrKey}/editmeta` (édition). Chaque entrée est un
  `FieldMetadata` : `key`, `name`, `required` (booléen), `schema`, `operations` (les opérations
  admises — voir §5), `allowedValues`, `autoCompleteUrl`, `hasDefaultValue`.
- Les champs personnalisés ont des **contextes** (`/rest/api/3/field/{fieldId}/context`) : un même
  champ peut n'exister, ou n'avoir les mêmes options, que pour certains projets et certains types de
  tickets. Un champ « présent » globalement peut donc être invisible sur un ticket donné.

### Format de description : ADF

L'API **v3** impose l'**Atlassian Document Format** — un arbre JSON, pas du texte :

```json
{ "version": 1, "type": "doc", "content": [
  { "type": "paragraph", "content": [ { "type": "text", "text": "Order entry fails." } ] } ] }
```

`version` (actuellement 1), `type: "doc"` en racine, `content` un tableau ordonné de **nœuds bloc**
(paragraphe, titre, liste…) contenant des **nœuds en ligne** (texte, image) éventuellement porteurs
de **marks** (gras, italique, lien, couleur).

Champs concernés, textuellement d'après la spécification de `POST /rest/api/3/issue` et de
`PUT /rest/api/3/issue/{issueIdOrKey}` : **`description`, `environment`, et tout champ personnalisé
de type `textarea` (multi-lignes)**. Le corps des **commentaires** est également ADF (schéma
`Comment.body`). À l'inverse, **« les champs personnalisés sur une ligne (`textfield`) acceptent une
chaîne et ne gèrent pas l'ADF »** — c'est-à-dire que le même modèle mélange deux régimes de texte.

- L'API **v2** (`/rest/api/2/`) accepte, elle, du texte/wiki markup à la place de l'ADF — **⚠️ non
  vérifié** (souvenir d'entraînement ; la page ADF consultée ne traite pas des alternatives, et je
  n'ai pas ouvert la référence v2). Si c'est confirmé, c'est une porte de sortie importante pour un
  intégrateur qui ne veut pas construire d'arbres ADF.
- Un `renderedBody` / `renderedFields` (HTML) est disponible en lecture via `expand`.

---

## 2. Hiérarchie

### La vraie hiérarchie : les niveaux de type de ticket

Jira attache un **niveau numérique** à chaque *type* de ticket (`IssueTypeDetails.hierarchyLevel`,
plus un booléen `subtask`). Configuration par défaut sur trois niveaux :

| Niveau | Nom par défaut | Rôle |
|---|---|---|
| **1** | Epic | gros morceau de travail |
| **0** | Story (niveau de base : Story, Task, Bug…) | élément standard |
| **−1** | Subtask | découpage fin |

- Les niveaux **au-dessus de 1** (Initiative, etc.) existent, mais **uniquement en Jira Cloud
  Premium et Enterprise**, et la page de configuration précise que les modifications de hiérarchie
  **« s'appliquent à tous les projets company-managed du site »** — donc c'est une configuration
  globale, pas une configuration de projet.
- Avertissement de la même page : changer la structure de hiérarchie **casse les relations
  parent-enfant existantes et est irréversible**.
- `Hierarchy.baseLevelId` est marqué déprécié (« Removing hierarchy level IDs from next-gen APIs »),
  et un avis de dépréciation dédié annonce que **les niveaux de hiérarchie ne seront plus inclus sur
  les tickets**. Autrement dit : ne pas se reposer sur les identifiants de niveau renvoyés dans les
  charges utiles.

### Le lien parent

Le champ **`parent`** (`{"parent": {"key": "PROJ-123"}}` ou par `id`) porte la relation :

- pour un **sous-tâche**, `parent` est **obligatoire** à la création et `issuetype` doit être un type
  de sous-tâche ;
- pour un type standard, `parent` désigne l'epic (ou le niveau au-dessus). La spécification de
  `PUT /rest/api/3/issue/{issueIdOrKey}` note que **« pour les types de ticket standards, le parent
  peut être retiré en posant `update.parent.set.none` à `true` »** — un cas particulier de syntaxe
  qui ne se déduit d'aucune règle générale.
- Dans un projet *next-gen* (= team-managed), la spécification dit que **« n'importe quel ticket peut
  être rendu enfant, pourvu que parent et enfant appartiennent au même projet »**.
- Historiquement le rattachement à un epic passait par un champ personnalisé « Epic Link »
  (`customfield_…`) et non par `parent` en projet company-managed — **⚠️ non vérifié** : la chaîne
  « Epic Link » **n'apparaît nulle part** dans la spécification OpenAPI actuelle, ce qui suggère que
  la migration vers `parent` est faite ; mais je n'ai pas trouvé de page confirmant explicitement le
  basculement ni sa date. À vérifier sur un site réel avant de s'y fier.

### Ce qui **n'est pas** de la hiérarchie

- **Les *issue links*** (`POST /rest/api/3/issueLink`, types gérés par `/rest/api/3/issueLinkType`) :
  une relation **latérale, typée et orientée** entre deux tickets (« blocks / is blocked by »,
  « relates to », « duplicates »…), avec une extrémité `inward` et une extrémité `outward`. Ce n'est
  **pas** une relation de composition : ni la hiérarchie, ni les boards, ni les rapports ne s'en
  servent pour agréger. Ne jamais mapper un lien sur un parent.
- Ils exigent que la fonction *Issue Linking* soit activée sur le site.
- **Les *remote links*** (`/rest/api/3/issue/{issueIdOrKey}/remotelink`) : liens vers des objets
  extérieurs à Jira.

### Regroupements temporels

- **Sprint** — objet de l'Agile API : `POST /rest/agile/1.0/sprint`, `GET|PUT|POST|DELETE
  /rest/agile/1.0/sprint/{sprintId}`, `POST /rest/agile/1.0/sprint/{sprintId}/issue` (« Move issues
  to sprint and rank »). Contraintes documentées : **on ne peut déplacer des tickets que vers des
  sprints ouverts ou actifs**, et **50 tickets au maximum par appel**. Retirer du sprint =
  `POST /rest/agile/1.0/backlog/issue`, décrit comme « équivalent à retirer les sprints futurs et
  actifs d'un ensemble de tickets ». Un sprint appartient à un board.
- **Version / release** — objet de projet (`Version` : `name` unique et obligatoire, `projectId`
  obligatoire à la création, `startDate`/`releaseDate` en ISO 8601 `yyyy-mm-dd`, `released`,
  `archived`, `moveUnfixedIssuesTo`). Deux champs distincts la référencent sur un ticket :
  **`fixVersions`** (version où le correctif atterrit) et **`versions`** (versions affectées). Nuance
  documentée : *« si la version est déjà publiée, une requête de publication est ignorée »*.
- **Epic (Agile API)** — `POST /rest/agile/1.0/epic/{epicIdOrKey}/issue` déplace des tickets vers un
  epic ; **« un ticket ne peut être que dans un seul epic à la fois »**, 50 tickets max par appel, et
  **« cette opération ne fonctionne pas pour les epics dans les projets next-gen »** (= team-managed).
  Donc : pour rattacher à un epic, l'API Agile marche en company-managed seulement, et le champ
  `parent` de l'API plateforme est la voie générale.

---

## 3. Étiquettes

- Champ système **`labels`** : un **tableau de chaînes** dans `fields`
  (`"labels": ["bugfix", "blitz_test"]`). Pas d'objet, pas d'identifiant, pas de couleur, pas de
  description. C'est le champ le plus proche des « labels » des autres outils.
- **Portée : le site entier.** `GET /rest/api/3/label` renvoie la liste **paginée** de toutes les
  étiquettes de l'instance — il n'y a pas de notion d'étiquette « appartenant » à un projet. Une
  étiquette existe du seul fait qu'un ticket la porte : **il n'y a aucun endpoint de création ni de
  suppression d'étiquette**. Elles apparaissent et disparaissent avec les usages.
- **Contrainte : pas d'espace.** Confirmé par la base de connaissances Atlassian : les étiquettes ne
  peuvent pas contenir d'espace, et il faut employer tirets ou tirets bas pour les libellés
  multi-mots ; la raison est que la saisie sépare plusieurs étiquettes par des espaces (« This is my
  Label » deviendrait quatre étiquettes). Une couche d'abstraction **doit donc translittérer** les
  étiquettes venues d'outils qui, eux, tolèrent les espaces.
- Longueur maximale de **255 caractères** : rapportée par un message d'erreur cité sur le forum
  développeurs (« Labels can't have spaces or be more than 255 characters ») — **source
  communautaire, pas documentation officielle**.
- Il existe aussi des **champs personnalisés de type `labels`** (schéma
  `CustomFieldContextDefaultValueLabels`) : plusieurs jeux d'étiquettes distincts peuvent coexister
  sur un même ticket. Une abstraction qui suppose « une seule dimension d'étiquettes » est fausse ici.

### Les composants (`components`) — le champ voisin

- Objet de **projet**, avec identité propre (`ProjectComponent` : `id`, `name` unique dans le projet
  et max 255 caractères, `description`, `project`/`projectId`, `lead`, `assigneeType`), géré par un
  CRUD dédié (`POST|GET|PUT|DELETE /rest/api/3/component/{id}`,
  `GET /rest/api/3/project/{projectIdOrKey}/components`).
- Sur le ticket, `components` est un tableau de **références** : `[{"id": "10000"}]`, pas de chaînes.
- Un composant peut **piloter l'affectation** : `assigneeType` = `PROJECT_LEAD` / `COMPONENT_LEAD` /
  `UNASSIGNED` / `PROJECT_DEFAULT`, avec un `realAssigneeType` de repli si le rôle n'est pas
  renseigné. C'est un effet de bord invisible : poser un composant peut **changer l'assigné**.
- **`enableComponents` est décrit dans le schéma de création de projet comme « utilisé uniquement par
  les projets company-managed »** — indice fort que les composants ne sont pas un concept
  team-managed. (Que le champ soit totalement absent des tickets team-managed : **⚠️ non vérifié**.)

Résumé du contraste : `labels` = taxonomie libre, globale, sans identité, sans droits ;
`components` = taxonomie gouvernée, propre au projet, avec identité stable et effets métier.

---

## 4. États et colonnes — la section décisive

### Statut ≠ colonne. Trois plans distincts.

1. **Le statut** (`status`) — attribut du **ticket**, provenant du **workflow**. Schéma
   `StatusDetails` : `id`, `name`, `description`, `iconUrl`, `statusCategory`, **`scope`**.
2. **Le workflow** — le graphe qui dit *quels statuts existent* et *quels passages sont permis*.
3. **La colonne** — attribut d'un **board**, c'est-à-dire d'une **vue**. Le board a son propre filtre
   JQL ; le même ticket peut apparaître sur plusieurs boards, avec des découpages en colonnes
   différents.

### Catégorie de statut

Chaque statut appartient à une **catégorie** (`StatusCategory` : `id`, `key`, `name`, `colorName`).
Exemples tirés de la spécification : `{"id": 1, "key": "in-flight", "name": "In Progress",
"colorName": "yellow"}` et `{"id": 9, "key": "completed", "colorName": "green"}`. Les trois
catégories, côté utilisateur, sont **To Do / In Progress / Done**. La clé de la catégorie « To Do »
serait `new` (id 2) — **⚠️ non vérifié** (absente des exemples que j'ai lus ; seules `in-flight` et
`completed` y figurent).

C'est la **seule** projection stable et inter-projets d'un statut : les noms de statuts sont
arbitraires et propres à chaque équipe, la catégorie ne l'est pas. **Pour une abstraction commune,
c'est sur `statusCategory` qu'il faut mapper**, pas sur `status.name`.

`GET /rest/api/3/statuses/search` renvoie des statuts avec un `scope` du genre
`{"project": {"id": "1"}, "type": "PROJECT"}` : **en team-managed, les statuts sont portés par le
projet** ; en company-managed ils sont globaux et partagés entre projets via les schémas.

Ne pas confondre **statut** et **résolution** : le statut dit *où* en est le travail, la résolution
dit *comment* il s'est terminé (Done, Won't do, Duplicate, Cannot reproduce). Elle est en général
posée **au moment d'une transition**.

### Mapping colonne → statuts

- `GET /rest/agile/1.0/board/{boardId}/configuration` renvoie `columnConfig` : « la liste des
  colonnes du board, dans l'ordre défini par la configuration ; pour chaque colonne, le **mapping des
  statuts** ainsi que le type de contrainte (`none`, `issueCount`, `issueCountExclSubs`) pour le
  min/max de tickets ».
- **Une colonne peut contenir plusieurs statuts** (documentation utilisateur : « you can assign
  multiple statuses to a single column »). La relation est donc **1 colonne → N statuts**, et
  l'inverse **statut → colonne** n'est défini que dans le contexte d'un board donné.
- Des statuts peuvent **n'être mappés à aucune colonne** : ils vont dans le panneau *Unmapped
  statuses*, et supprimer une colonne y renvoie ses statuts. Un ticket dans un statut non mappé
  **n'apparaît pas** dans les colonnes du board.
- Sémantique du « Done » : **« la dernière colonne à laquelle des statuts sont mappés est traitée
  comme la colonne "Done", ce qui signifie que les tickets qui s'y trouvent sont considérés comme
  déjà terminés »** (spécification Agile) ; la documentation utilisateur redit que « Jira ne
  considère comme terminés que les tickets de la colonne la plus à droite ». C'est une convention
  **positionnelle**, distincte de la catégorie de statut — les deux peuvent diverger.
- La configuration du board renvoie aussi `filter` (le filtre JQL du board), `location` (conteneur :
  `project` ou `user`), `subQuery` (kanban), `estimation` et `ranking`.
- Que glisser une carte d'une colonne à l'autre déclenche une transition de workflow, et ce qui se
  passe si aucune transition n'existe entre les statuts : **la page « Configure columns » ne le dit
  pas** ; je ne l'affirme donc pas. (Que ce soit bien une transition, avec échec possible :
  **⚠️ non vérifié**.)

### Transitions

- `GET /rest/api/3/issue/{issueIdOrKey}/transitions` renvoie les transitions **exécutables par
  l'utilisateur courant, depuis le statut actuel du ticket**. Chaque `IssueTransition` porte : `id`
  (« requis pour spécifier la transition à effectuer »), `name`, `to` (statut d'arrivée),
  **`hasScreen`**, **`isAvailable`**, **`isConditional`** (« le ticket doit remplir des critères avant
  application »), `isGlobal`, `isInitial`, `fields` (avec l'expand `transitions.fields`).
- Avertissement explicite de la spécification : **« si la transition demandée n'existe pas ou ne peut
  pas être effectuée sur le ticket compte tenu de son statut, la réponse renvoie une liste de
  transitions vide »** — pas une erreur. Et **si l'utilisateur n'a pas la permission *Transition
  issues*, la liste renvoyée est vide** alors même que le ticket est lisible. Une liste vide est donc
  ambiguë : elle peut signifier « rien de possible » ou « pas le droit ».
- **Les identifiants de transition ne sont pas des constantes** : ils dépendent du workflow attaché
  au projet et au type de ticket. Il faut les **redécouvrir pour chaque ticket**, pas les mettre en
  cache par nom de statut.

### Le rang

- L'ordre à l'intérieur d'une colonne / d'un backlog est porté par un **champ de rang** (LexoRank),
  distinct de toute date. La configuration du board expose `ranking` = « informations sur le champ
  personnalisé utilisé pour le rang sur ce board ».
- Écriture : **`PUT /rest/agile/1.0/issue/rank`** — « déplace (range) des tickets avant ou après un
  ticket donné ; **50 tickets au maximum** par appel ; si `rankCustomFieldId` n'est pas fourni, le
  champ de rang par défaut est utilisé ». L'opération **peut échouer pour certains tickets** : dans ce
  cas un **207** est renvoyé pour l'ensemble, avec le détail par ticket dans le corps.
- La page d'introduction de l'API Jira Software indique que le champ interne **Rank « ne devrait pas
  être lu ni mis à jour via l'API REST »** — c'est-à-dire : passer par `PUT /rest/agile/1.0/issue/rank`
  (rang **relatif** à un ticket voisin), et **ne pas** traiter le rang comme une valeur qu'on lit,
  trie et réécrit.

---

## 5. Écriture

**Base commune.** Toutes les écritures de champs partagent le schéma `IssueUpdateDetails`, qui a
**deux voies** :

- **`fields`** — « une option directe pour poser une valeur » : `{"summary": "…"}` remplace.
- **`update`** — « une map du nom de champ vers une **liste d'opérations**  » ; opérations du schéma
  `FieldUpdateOperation` : **`set`, `add`, `remove`, `edit`, `copy`**.
- Règle stricte : **« les champs présents dans `update` ne peuvent pas être présents dans `fields` »**
  (et réciproquement). Une même requête ne peut pas traiter le même champ des deux façons.
- Les opérations réellement admises par champ sont dans `FieldMetadata.operations` de `editmeta`.

**Idempotence — constat général.** La chaîne « idempot » **n'apparaît pas une seule fois** dans la
spécification OpenAPI de la plateforme. Il n'existe **aucune clé d'idempotence**, aucun en-tête de
déduplication côté requête. L'idempotence doit donc être raisonnée opération par opération.

### a) Créer un ticket

- **`POST /rest/api/3/issue`** → **201** `{"id":"10000","key":"ED-24","self":"…"}`.
- **Obligatoires** : `fields.project` (`{"id"}` ou `{"key"}`), `fields.issuetype` (`{"id"}`),
  `fields.summary`. Tout le reste dépend de la configuration : **l'ensemble des champs acceptés, et
  ceux marqués `required: true`, se lit dans `GET /rest/api/3/issue/createmeta/…/issuetypes/{id}`** —
  « ce sont les mêmes champs que ceux de l'écran de création ». Un projet peut donc rendre
  obligatoire n'importe quel champ personnalisé, ce qu'aucun client ne peut deviner à l'avance.
- Sous-tâche : `issuetype` doit être un type sous-tâche **et** `parent` doit contenir l'ID ou la clé
  du parent.
- On peut fournir `transition` dès la création (« une transition peut être appliquée, pour amener le
  ticket à une étape du workflow autre que l'étape de départ ») ainsi que des `properties`.
- Permissions : *Browse projects* + *Create issues*.
- **Idempotence : NON.** Rejouer crée un second ticket avec une nouvelle clé. **Aucune** protection
  côté serveur. Le rejeu sûr doit être construit côté client — par exemple via une **propriété de
  ticket** (`/rest/api/3/issue/{id}/properties/{key}`) portant un identifiant d'origine, recherchée en
  JQL (`issue.property`) avant création. Attention alors à la §7 : la recherche n'est pas
  immédiatement cohérente après écriture.
- Codes d'erreur : 400, 401, 403, **422**.

### b) Éditer les champs

- **`PUT /rest/api/3/issue/{issueIdOrKey}`** → **204** (ou 200 avec `returnIssue=true`).
- **Obligatoire** : rien en soi ; le corps est un `IssueUpdateDetails`. Les champs éditables se
  lisent dans **`GET /rest/api/3/issue/{issueIdOrKey}/editmeta`**, qui vérifie **neuf conditions**
  cumulatives, dont : le champ est sur un écran (via écran → schéma d'écran → schéma d'écran par type
  de ticket → schéma de types de ticket) ; le champ est visible dans la *field configuration* ; un
  champ personnalisé a un **contexte valide pour ce projet et ce type de ticket** ; le ticket est
  rattaché à un workflow et **l'étape courante du workflow est éditable** (la propriété de workflow
  `jira.issue.editable` peut être mise à `false`) ; les **permissions de workflow** autorisent
  l'édition du champ.
  → **Autrement dit : l'éditabilité d'un champ dépend du statut courant du ticket.** C'est un couplage
  état↔schéma qui n'existe nulle part ailleurs.
- Note explicite : **« la transition n'est pas supportée ici et est ignorée »**.
- **Idempotence : dépend de l'opération.** `fields: {...}` et `update: {champ: [{set: …}]}` sont
  **idempotents** (poser deux fois la même valeur donne le même état). `add` / `remove` sont
  **convergents** sur des ensembles (rajouter une étiquette déjà présente ne la duplique pas —
  **⚠️ non vérifié**, comportement plausible mais non attesté par une page lue) ; en revanche
  `update: {comment: [{add: …}]}` **n'est pas** idempotent (voir e).
- Chaque écriture réussie ajoute une entrée au **changelog** : rejouer une écriture identique est
  sans effet sur l'état, mais **pollue l'historique et déclenche des webhooks** — donc « sans dégât »
  au sens des données, pas au sens des observateurs. **⚠️ non vérifié** qu'une écriture strictement
  sans changement produise malgré tout un événement.
- Codes : 200, 204, 400, 401, 403, 404, **409**, 422.

### c) Changer de statut

- **`POST /rest/api/3/issue/{issueIdOrKey}/transitions`** → **204**.
- **Obligatoire** : `transition.id`. **Le statut cible ne se pose pas ; on désigne l'arête, pas le
  nœud.** Il faut donc systématiquement : `GET .../transitions` → trouver la transition dont `to.id`
  ou `to.name` correspond au statut visé → poster son `id`.
- **La transition peut exiger des champs** : « effectue une transition et, **si la transition a un
  écran**, met à jour les champs de l'écran de transition ». On fournit alors `fields` / `update`
  dans le même corps ; leur description se lit dans `GET .../transitions?expand=transitions.fields`.
  Cas classique : une transition vers *Done* exigeant une `resolution` (l'exemple officiel pose
  précisément `{"fields": {"resolution": {"name": "Fixed"}}}`).
- Permissions : *Browse projects* + **Transition issues**.
- **Conséquences de conception — c'est le point structurant de la fiche :**
  1. **Un statut n'est pas un champ assignable.** L'abstraction commune ne peut pas exposer
     `SetStatus(ticket, "Done")` et le traduire en un simple `PATCH`. Il faut un modèle
     « transitions disponibles » de premier ordre.
  2. **Le passage peut être tout simplement impossible.** Le workflow peut n'offrir aucune arête du
     statut courant vers le statut visé — sans détour multi-sauts, l'opération échoue. Aucune
     API n'oblige Jira à accepter un statut arbitraire.
  3. **Elle peut exiger des données que l'appelant n'a pas** (résolution, champ personnalisé
     obligatoire sur l'écran de transition), et des **conditions** (`isConditional`) ou des
     validateurs peuvent la refuser au dernier moment.
  4. Signalé par la communauté : **les champs réellement obligatoires d'une transition ne sont pas
     toujours marqués `required` dans la réponse** de `GET .../transitions` — source
     communautaire, à traiter comme un risque et non comme un fait.
- **Idempotence : NON, et de façon franche.** Rejouer la même transition depuis le nouveau statut est
  soit sans effet (transitions vides), soit une erreur, soit — si la transition est **globale**
  (`isGlobal`) ou **bouclée** (`looped`) — **une seconde application réussie**, qui repose la
  résolution et regénère un événement. Le rejeu sûr consiste à **relire le statut courant d'abord** et
  à ne transitionner que si nécessaire : l'état cible est convergent, l'appel ne l'est pas.
- Codes : 204, 400, 401, 404, **409**, 413, 422.

### d) Poser et retirer une étiquette

- **Pose** : `PUT /rest/api/3/issue/{issueIdOrKey}` avec
  `{"update": {"labels": [{"add": "bugfix"}]}}`.
- **Retrait** : même endpoint, `{"update": {"labels": [{"remove": "bugfix"}]}}`.
- **Remplacement complet** : `{"fields": {"labels": ["a","b"]}}` (ou `update` avec `set`).
- **Obligatoire** : la valeur est une **chaîne sans espace** (§3).
- **Idempotence : oui au sens de l'état convergent**, avec `add`/`remove` — c'est précisément
  l'intérêt de la voie `update` sur la voie `fields` : elle **évite l'écrasement concurrent**. Poser
  `fields.labels` écrase les étiquettes posées entre-temps par quelqu'un d'autre ; `add`/`remove`
  non. Pour une synchronisation multi-outils, **toujours préférer `update` pour les collections**.
- Pas d'endpoint dédié aux étiquettes en écriture : `GET /rest/api/3/label` est en lecture seule.

### e) Commenter

- **`POST /rest/api/3/issue/{issueIdOrKey}/comment`** → 201, renvoie le `Comment` créé (`id`, `self`,
  `author`, `created`, `updated`).
- **Obligatoire** : `body` en **ADF**. Optionnels : `visibility` (restriction à un groupe ou un rôle),
  `properties`, `jsdPublic` (Jira Service Management — **par défaut `true`**, un commentaire créé par
  l'API plateforme est donc **public** sur un portail JSM ; pour le rendre privé il faut passer par
  l'API Service Desk).
- Permissions : *Browse projects* + *Add comments*.
- **Idempotence : NON.** Rejouer crée un **doublon** ; le commentaire n'a pas de clé naturelle. Pour
  un rejeu sûr : conserver le `Comment.id` renvoyé et utiliser
  `PUT /rest/api/3/issue/{issueIdOrKey}/comment/{id}` (idempotent), ou marquer le commentaire par une
  **propriété de commentaire** (`/rest/api/3/comment/{commentId}/properties/{key}`).
- Variante : un commentaire peut être ajouté **dans la même requête** qu'une transition ou une
  édition, via `update: {"comment": [{"add": {"body": <ADF>}}]}`. Pratique — mais cela rend
  l'ensemble de la requête non rejouable.
- Un commentaire peut aussi accompagner la **création d'un lien** (`POST /rest/api/3/issueLink`).

### f) Autres écritures utiles

| Opération | Endpoint | Note |
|---|---|---|
| Affecter | `PUT /rest/api/3/issue/{issueIdOrKey}/assignee` | idempotent (`set` d'`accountId`) |
| Créer un lien | `POST /rest/api/3/issueLink` | **« si la requête duplique un lien, la réponse indique que le lien a été créé »** — donc silencieusement idempotent sur le lien, **mais le commentaire éventuellement joint est ajouté à chaque fois** |
| Supprimer un lien | `DELETE /rest/api/3/issueLink/{linkId}` | il faut d'abord retrouver l'ID via `?fields=issuelinks` sur un des deux tickets — la création **ne le renvoie pas** |
| Ranger | `PUT /rest/agile/1.0/issue/rank` | 50 max, 207 partiel |
| Sprint | `POST /rest/agile/1.0/sprint/{sprintId}/issue` | 50 max, sprints ouverts/actifs seulement |
| Propriétés | `PUT /rest/api/3/issue/{issueIdOrKey}/properties/{propertyKey}` | **stockage clé-valeur JSON arbitraire attaché au ticket, idempotent** — le bon endroit pour poser un identifiant de corrélation externe |
| En masse | `POST /rest/api/3/issue/bulk`, `/rest/api/3/bulk/issues/transition`, `/bulk/issues/fields`, `/bulk/issues/move`, `/bulk/issues/delete` | existent, et sont un levier direct contre les quotas |

---

## 6. Authentification

Trois familles, visibles dans le bloc `security` de chaque opération de la spécification :
`basicAuth`, `OAuth2` (avec les portées), et le mode anonyme.

### a) API token + Basic auth

- Identifiants : **adresse de courriel du compte Atlassian + jeton d'API**. « L'authentification par
  mot de passe est dépréciée. »
- Construction : concaténer `email:api_token`, encoder en **Base64**, envoyer
  `Authorization: Basic <chaîne>`.
- Base : **`https://<votre-domaine>.atlassian.net/rest/api/3/…`**.
- La page ne dit **rien** sur l'expiration des jetons. (Qu'Atlassian impose depuis peu une date
  d'expiration bornée à un an sur les nouveaux jetons : **⚠️ non vérifié**.)
- **Avertissement important pour un outil de bureau** : Atlassian écrit que le Basic auth n'est
  recommandé que pour « des scripts simples et des appels manuels », et que **les applications qui
  collectent les jetons d'API de leurs utilisateurs violent les exigences de sécurité et la politique
  d'usage acceptable d'Atlassian**. Un produit distribué doit donc viser OAuth 2.0 ; l'API token reste
  légitime pour l'usage personnel du propriétaire du jeton.

### b) OAuth 2.0 (3LO)

- Autorisation : `https://auth.atlassian.com/authorize` avec `client_id`, `scope`, `redirect_uri`,
  `state`, et **`prompt=consent`** pour afficher l'écran de permissions.
- Échange : `POST https://auth.atlassian.com/oauth/token` avec `client_id`, `client_secret`, le code.
- **Jeton d'accès** : la réponse contient `expires_in` (en secondes) ; **la documentation ne donne pas
  de valeur fixe**. (Une heure : **⚠️ non vérifié**.)
- **Jeton de rafraîchissement** : réclamer la portée **`offline_access`**. Atlassian utilise des
  **jetons rotatifs** — chaque échange émet un nouveau jeton et **invalide le précédent**. Ils
  expirent après **90 jours d'inactivité**, chaque échange remettant le compteur à 90 jours. Une
  **fenêtre de réutilisation de 10 minutes** évite les faux positifs de détection de compromission en
  cas d'échanges concurrents.
  → Conséquence pour un client de bureau : **le jeton de rafraîchissement stocké doit être remplacé
  atomiquement à chaque rafraîchissement**, et deux instances du même outil partageant le même
  stockage se déconnectent mutuellement hors de la fenêtre de 10 minutes. Il faut aussi un
  rafraîchissement **périodique de fond**, sinon 90 jours d'inactivité déconnectent l'utilisateur.
- **Portées classiques** (recommandées en premier par Atlassian) : `read:jira-user`,
  `read:jira-work`, **`write:jira-work`** (créer/éditer des tickets et poster des commentaires),
  `manage:jira-project`, `manage:jira-configuration`. **Portées granulaires** en alternative :
  `read:issue:jira`, `write:issue:jira`, `write:comment:jira`, etc. Consigne officielle : **rester
  sous 50 portées au total**.
- Portées effectivement exigées, lues dans la spécification : `write:jira-work` pour créer/éditer/
  transitionner, `read:jira-work` pour lire les transitions.

### c) L'URL du site fait partie de l'identité

C'est structurel, et c'est propre à Jira :

- En Basic auth, la base est **`https://<site>.atlassian.net`** — le site est dans l'URL.
- En OAuth, on n'appelle **pas** le site directement : on appelle
  **`GET https://api.atlassian.com/oauth/token/accessible-resources`**, qui renvoie **la liste des
  sites autorisés**, chacun avec un champ **`id`** — le **cloudid** — puis on adresse
  **`https://api.atlassian.com/ex/jira/{cloudid}/rest/api/3/…`**.
- Donc : **un même jeton peut couvrir plusieurs sites**, et le choix du site est une donnée
  applicative séparée du jeton. Une « connexion Jira » n'est pas (jeton) mais **(jeton, cloudid)**,
  et l'utilisateur peut devoir choisir. À stocker durablement : `refresh_token` (rotatif), `cloudid`,
  l'URL humaine du site, et le `accountId` de l'utilisateur.

### d) Forge / Connect

Applications hébergées par Atlassian, installées sur le site, avec leur propre modèle d'autorisation
(JWT pour Connect, plateforme managée pour Forge) et déclaration statique de webhooks. Elles peuvent
en outre **contourner la sécurité d'écran** (`overrideScreenSecurity`, `overrideEditableFlag`) si
l'app agit avec la permission *Administer Jira*. Peu pertinent pour un client de bureau, mais c'est
la raison pour laquelle certaines capacités semblent exister dans l'API sans être accessibles.

---

## 7. Transport et limites

### REST

JSON sur HTTPS. Deux surfaces distinctes : **`/rest/api/3/`** (plateforme) et **`/rest/agile/1.0/`**
(boards, sprints, epics, rang, backlog) — la seconde n'existe qu'avec Jira Software et **couvre des
concepts que la première ignore**. Il existe aussi `/rest/software/1.0/` (quelques endpoints de
board) et des API DevOps (`devinfo`, `deployments`, `featureflags`…).

### Pagination — trois régimes coexistent

1. **Offset classique** — `startAt` / `maxResults`, réponse avec `startAt`, `maxResults`, `total`,
   `isLast`, parfois `nextPage`, et `values`. C'est le régime de `GET /rest/api/3/label`,
   `GET /rest/api/3/statuses/search`, etc.
2. **Curseur** — la recherche JQL : **`GET|POST /rest/api/3/search/jql`** utilise **`nextPageToken`**
   et **ne renvoie plus de `total`**. Les anciens `GET|POST /rest/api/3/search` sont en retrait.
3. Certaines listes ne sont **pas** paginées (`GET /rest/api/3/status`, `GET /rest/api/3/statuscategory`
   renvoient un tableau brut).

**Cohérence lecture-après-écriture : non garantie.** La spécification de `/rest/api/3/search/jql` dit
textuellement que **« les mises à jour récentes peuvent ne pas être immédiatement visibles dans les
résultats »**, et offre un paramètre **`reconcileIssues`** (jusqu'à **50 identifiants**) pour forcer
une garantie de cohérence forte sur ces tickets. C'est un piège classique : créer un ticket puis le
chercher immédiatement en JQL peut ne rien renvoyer.

Autres paramètres notables de la recherche : `fields` (sous-ensemble de champs — indispensable pour
la performance), `expand`, `properties` (**5 au maximum**), `fieldsByKeys`, `failFast`.

### Quotas de débit — chiffres documentés

**Trois systèmes appliqués simultanément.**

1. **Quota horaire en points**
   - Palier 1 (défaut) : **65 000 points/heure**, dans un **pool global partagé entre tous les
     locataires**.
   - Palier 2 (pool par locataire, après revue par Atlassian) : Free **65 000** ; Standard
     **100 000 + 10 × utilisateurs** ; Premium **130 000 + 20 × utilisateurs** ; Enterprise
     **150 000 + 30 × utilisateurs** — **plafonné à 500 000 points/heure**.
   - Coût : **1 point** de base par requête ; **+1 point** par objet de domaine en GET ; **+2 points**
     par objet d'identité/accès en GET ; les **écritures coûtent 1 point** sans surcoût par objet.
2. **Rafales, par seconde** : **GET 100 rps, POST 100 rps, PUT 50 rps, DELETE 50 rps** par défaut,
   certains endpoints ayant des limites propres de **5 à 400 rps**.
3. **Écritures par ticket** : **20 opérations / 2 secondes** et **100 opérations / 30 secondes**.
   → C'est celle-là qui mord dans un outil de synchronisation : marteler un même ticket est limité
   bien avant le quota global.

**Réponse 429**, en-têtes : `Retry-After` (secondes), `X-RateLimit-Limit`, `X-RateLimit-Remaining`,
`X-RateLimit-Reset` (ISO 8601), et **`RateLimit-Reason`** ∈ `jira-quota-global-based`,
`jira-quota-tenant-based`, `jira-burst-based`, `jira-per-issue-on-write` — cette dernière permet de
savoir *lequel* des trois systèmes a mordu, et donc quelle stratégie de repli appliquer. Des en-têtes
`Beta-RateLimit-Policy` / `Beta-RateLimit` existent, informatifs et non appliqués.

**Le trafic authentifié par jeton d'API n'est pas soumis** au système de points : il reste régi par
les limites de rafale existantes. Les nouvelles limites s'appliquent aux applications Forge, Connect
et OAuth 2.0 (3LO).

Notons que le palier 1 est un **pool global partagé** : le débit disponible dépend du comportement
des autres intégrations, pas seulement du sien.

### Webhooks

- **Trois modes d'enregistrement** : interface d'administration ; **API REST**
  (`/rest/api/3/webhook`, réservée aux applications Connect / OAuth 2.0, **100 webhooks max par app
  et par locataire**) ; module déclaré dans le descripteur Forge/Connect.
- **Événements** : `jira:issue_created`, `jira:issue_updated`, `jira:issue_deleted`,
  `comment_created|updated|deleted`, `issue_property_set|deleted`,
  `sprint_created|updated|started|closed|deleted`, plus version, pièce jointe, board, projet.
- **Filtrage JQL** possible, mais sur un **sous-ensemble restreint de clauses** : `issueKey`,
  `project`, `issuetype`, `status`, `priority`, `assignee`, `reporter`, `issue.property` et les champs
  epic ; opérateurs `=`, `!=`, `IN`, `NOT IN` seulement. **Les événements sprint et version ignorent
  le filtre JQL** malgré son exigence dans le schéma.
- **Charge utile** : `timestamp`, `webhookEvent`, plus les données de l'entité ; pour un ticket,
  `issue_event_type_name`, `user`, `issue`, et **`changelog` (uniquement sur mise à jour)**. Les
  formes de `issue` et `user` sont celles de l'API REST, **sauf** que l'utilisateur est amputé de sa
  locale, de son courriel, de ses groupes et de ses rôles applicatifs.
- **Livraison** : jusqu'à **5 retentatives** avec retrait aléatoire de **5 à 15 minutes**. Les
  webhooks « primaires » sont censés être livrés en **moins de 30 secondes** ; les « secondaires »
  (issus d'opérations en masse) jusqu'à **15 minutes**.
- **Déduplication** : en-tête **`X-Atlassian-Webhook-Identifier`, constant à travers les
  retentatives**, plus `X-Atlassian-Webhook-Retry`. C'est le seul mécanisme d'idempotence explicite
  de toute cette API — et il est du côté réception, pas émission.
- **Expiration** : les webhooks enregistrés dynamiquement **expirent 30 jours** après création ou
  rafraîchissement ; il faut appeler l'endpoint *Extend webhook life*. Un client qui n'a pas tourné
  depuis un mois se retrouve silencieusement sourd.
- **Concurrence** : **20 requêtes simultanées** par locataire/hôte d'URL pour le flux primaire, **10**
  pour le secondaire.
- **Signature** : HMAC-SHA256 optionnelle pour les webhooks d'administration, en-tête
  `X-Hub-Signature` au format `method=signature` ; jeton porteur pour les apps OAuth 2.0.

---

## 8. Pièges et singularités — ce qui ne se traduira pas ailleurs

1. **Changer de statut n'est pas écrire un champ.** C'est *la* singularité de Jira. Il faut
   `GET /transitions` puis `POST /transitions` avec un `transition.id` **découvert à l'exécution**,
   variable d'un projet à l'autre. Le passage peut être **interdit par le workflow**, refusé par une
   **condition** ou un **validateur**, ou **exiger des champs** sur l'écran de transition (résolution
   notamment). Une abstraction commune ne peut pas exposer un simple « poser l'état » : elle doit soit
   exposer les transitions comme un concept de premier ordre, soit accepter d'échouer là où les autres
   outils réussissent. Corollaire perfide : quand la transition est impossible, `GET /transitions`
   renvoie une **liste vide** au lieu d'une erreur — et une liste vide signifie aussi « permission
   manquante ».

2. **Le schéma des champs dépend du contexte, et même du statut courant.** Ce qui est obligatoire à la
   création dépend du projet **et** du type de ticket (`createmeta`) ; ce qui est éditable dépend en
   plus de l'**étape de workflow où le ticket se trouve** (`editmeta`, condition n° 7 :
   `jira.issue.editable`). Aucun des trois autres outils n'a de « champ obligatoire arbitraire décidé
   par l'administrateur du projet », ni de champ qui **cesse d'être éditable parce que le ticket a
   changé d'état**. Toute écriture doit donc être précédée d'une interrogation de métadonnées, ou
   savoir échouer proprement sur un 400/422.

3. **ADF.** La description et les commentaires sont un **arbre JSON**, pas du Markdown. Les trois
   autres outils sont, à ma connaissance, en Markdown. Il faut un convertisseur Markdown ↔ ADF, et
   ce convertisseur est **lossy dans les deux sens** (panneaux, mentions, cartes intelligentes n'ont
   pas d'équivalent Markdown ; les tables Markdown étendues n'ont pas d'équivalent direct). Pire : la
   règle « ADF pour `textarea`, chaîne nue pour `textfield` » signifie que **le format dépend du type
   du champ personnalisé**, pas du champ lui-même.

4. **`customfield_<n>` est un identifiant propre au site.** Le même champ métier n'a pas le même
   identifiant chez deux clients. Impossible de coder quoi que ce soit en dur ; il faut une phase de
   résolution par nom, avec le risque d'homonymie, et une gestion des **contextes** de champ.

5. **company-managed vs team-managed** — le même produit, deux modèles :
   - **statuts** : globaux et partagés en company-managed ; **portés par le projet** en team-managed
     (visible dans `scope: {type: "PROJECT", project: {...}}`) — donc deux statuts nommés « In
     Review » dans deux projets team-managed sont **deux objets différents** ;
   - **champs personnalisés** : de même, `FieldDetails.scope` distingue les champs propres au projet ;
   - **composants** : `enableComponents` est « utilisé uniquement par les projets company-managed » ;
   - **epics** : `POST /rest/agile/1.0/epic/{id}/issue` **« ne fonctionne pas pour les epics dans les
     projets next-gen »** — le rattachement passe alors par le champ `parent` ;
   - **hiérarchie** : les niveaux personnalisés « s'appliquent à tous les projets **company-managed**
     du site » ;
   - la distinction se lit sur le projet via **`simplified: true`** et `style`, et à la création via
     `ScopePayload.type` (**`GLOBAL` ou vide = company-managed, `PROJECT` = team-managed**).
   → Un client doit **brancher son comportement sur le style du projet**. C'est un écart interne à
   Jira, sans aucun équivalent chez les trois autres.

6. **La clé lisible est mutable.** Un *Move* vers un autre projet ou un renommage de clé de projet
   réécrit `ED-24` en `NEW-77`. La redirection existe mais l'ancienne clé n'est plus visible que dans
   l'historique. **Indexer sur `id`.**

7. **Étiquettes sans espace, sans identité, sans portée.** Pas de couleur, pas de description, pas de
   création explicite ; portée site entier ; et un tiret ou un tiret bas est obligatoire pour les
   libellés multi-mots. Toute étiquette importée d'un autre outil doit être translittérée — et la
   translittération n'est pas réversible.

8. **Deux rôles pour ce que d'autres appellent « label ».** `labels` (libre) et `components`
   (gouverné, avec ID, propre au projet, **et pouvant modifier l'assigné via `assigneeType`**). Un
   mapping naïf « tag → label » perd les composants ; un mapping « tag → composant » déclenche des
   effets de bord d'affectation.

9. **Colonne ≠ statut, et le board est une vue.** Une colonne agrège **N statuts** ; des statuts
   peuvent n'être mappés **à aucune** colonne (invisibles sur le board) ; et la sémantique « terminé »
   est **positionnelle** (« la dernière colonne à laquelle des statuts sont mappés »), donc
   potentiellement en désaccord avec `statusCategory`. Le même ticket a autant de « colonnes » que de
   boards qui le capturent par leur filtre JQL. **Seule `statusCategory` (To Do / In Progress / Done)
   est une projection stable et inter-projets** — c'est le seul point d'ancrage raisonnable pour une
   abstraction.

10. **Le rang est un objet à part**, écrit par un endpoint **relatif** (`PUT /rest/agile/1.0/issue/rank`,
    « avant ou après tel ticket »), sur une **autre API**, avec **50 tickets max**, un **207 partiel**
    possible, et une consigne explicite de ne **pas** lire/écrire le champ Rank directement.
    L'ordonnancement n'est donc ni un entier qu'on pose, ni une propriété du ticket qu'on lit.

11. **Aucune idempotence côté serveur.** Pas de clé d'idempotence dans toute la spécification.
    Créations et commentaires produisent des doublons au rejeu. Les seuls leviers : la voie
    `update: {champ: [{add|remove}]}` (convergente sur les collections, et **immunisée contre
    l'écrasement concurrent**, contrairement à `fields`), les **propriétés de ticket** comme clé de
    corrélation externe, et `X-Atlassian-Webhook-Identifier` côté réception.

12. **La recherche n'est pas cohérente après écriture.** `/rest/api/3/search/jql` prévient que les
    mises à jour récentes peuvent ne pas apparaître, et propose `reconcileIssues` (≤ 50 ids). Un
    « créer puis retrouver par JQL » naïf est un bogue en attente.

13. **La pagination change de régime selon l'endpoint** (offset ailleurs, **curseur `nextPageToken`
    sans `total`** sur la recherche JQL). Une couche générique « page N sur M » ne tient pas : il n'y
    a plus de `total` sur le chemin le plus utilisé.

14. **L'identité de connexion est un couple (jeton, site).** Le cloudid s'obtient par
    `accessible-resources`, et un même jeton peut couvrir plusieurs sites. À cela s'ajoutent des
    **jetons de rafraîchissement rotatifs** (invalidation du précédent, fenêtre de 10 minutes, mort
    après 90 jours d'inactivité) : un client de bureau doit réécrire son secret à chaque
    rafraîchissement, éviter deux instances concurrentes sur le même stockage, et rafraîchir
    périodiquement même sans activité utilisateur.

15. **Contrainte contractuelle sur les jetons d'API** : Atlassian déclare que les applications qui
    collectent les jetons d'API de leurs utilisateurs violent ses exigences de sécurité. Le chemin
    « demander une clé API à l'utilisateur », parfaitement acceptable chez d'autres outils, est
    formellement découragé ici pour un produit distribué.

16. **Les *issue links* ne sont pas une hiérarchie**, et leur API est asymétrique : la création ne
    renvoie **pas** l'identifiant du lien (il faut le repêcher par `?fields=issuelinks`), et
    dupliquer un lien renvoie un succès — mais **le commentaire joint, lui, est ajouté à chaque
    appel**.

17. **`jsdPublic` par défaut à `true`** : sur un projet Jira Service Management, un commentaire posté
    via l'API plateforme est **visible du demandeur**. Pour un commentaire interne il faut l'API
    Service Desk. Un outil qui commente automatiquement peut publier vers un client externe sans
    l'avoir voulu.

18. **Le quota par défaut est un pool *partagé entre locataires*** (65 000 points/h au palier 1) : le
    débit disponible dépend d'intégrations tierces sur lesquelles on n'a aucune prise. Et la limite
    la plus contraignante en pratique est celle **par ticket** (20 écritures / 2 s).

---

## Sources consultées

Spécifications OpenAPI officielles (téléchargées et interrogées directement — source principale) :

- https://developer.atlassian.com/cloud/jira/platform/swagger-v3.v3.json (Jira Cloud platform REST
  API, version `1001.0.0-SNAPSHOT-532ed230ff9aa83f9c3039b776559e831302b5dd`)
- https://developer.atlassian.com/cloud/jira/software/swagger.v3.json (Jira Software Cloud REST API,
  version `1001.0.0`)

Documentation développeur :

- https://developer.atlassian.com/cloud/jira/platform/rate-limiting/
- https://developer.atlassian.com/cloud/jira/platform/webhooks/
- https://developer.atlassian.com/cloud/jira/platform/oauth-2-3lo-apps/
- https://developer.atlassian.com/cloud/jira/platform/basic-auth-for-rest-apis/
- https://developer.atlassian.com/cloud/jira/platform/scopes-for-oauth-2-3LO-and-forge-apps/
- https://developer.atlassian.com/cloud/jira/platform/apis/document/structure/
- https://developer.atlassian.com/cloud/jira/software/rest/intro/
- https://developer.atlassian.com/cloud/jira/platform/deprecation-notice-hierarchy-levels/ (référencée
  depuis la recherche et depuis la spécification, non ouverte intégralement)
- https://developer.atlassian.com/cloud/jira/platform/change-notice-removing-hierarchy-level-ids-from-next-gen-apis/
  (référencée depuis la spécification, non ouverte)

Documentation utilisateur / administration :

- https://support.atlassian.com/jira-software-cloud/docs/what-is-an-issue/
- https://support.atlassian.com/jira-software-cloud/docs/what-are-team-managed-and-company-managed-projects/
- https://support.atlassian.com/jira-cloud-administration/docs/what-are-issue-statuses-priorities-and-resolutions/
- https://support.atlassian.com/jira-cloud-administration/docs/configure-the-issue-type-hierarchy/
- https://support.atlassian.com/jira-software-cloud/docs/configure-columns/
- https://support.atlassian.com/jira/kb/how-to-create-and-use-labels-in-jira-cloud/
- https://confluence.atlassian.com/jirakb/creating-multiple-word-labels-779160786.html
- https://support.atlassian.com/jira/kb/moved-issues-no-longer-redirect-from-previous-issue-key-or-url-in-jira/
- https://support.atlassian.com/automation/kb/how-to-store-the-old-issue-key-when-an-issue-is-moved-from-one-project-to/

Sources communautaires (explicitement signalées comme telles dans le corps de la fiche) :

- https://community.developer.atlassian.com/t/when-getting-transitions-from-rest-api-not-all-required-fields-are-marked-as-required/54252
- https://community.developer.atlassian.com/t/labels-cant-have-spaces-or-be-more-than-255-characters-in-forge-custom-fields-after-upgrading-to-new-ui/55277
- https://community.developer.atlassian.com/t/jira-cloud-rest-api-v3-search-jql-slower-fetching-with-nextpagetoken-no-totalissues-any-workarounds/90176

Pages dont la récupération a échoué (trop volumineuses pour l'outil, contournées par la spécification
OpenAPI) : `.../rest/v3/api-group-issues/`, `.../rest/v3/intro/`, `.../rest/v3/api-group-labels/`.
