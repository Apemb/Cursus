# Fiche de recherche — GitLab : modèle de données des tâches (Issues + boards) et API

Portée : GitLab SaaS (gitlab.com), API REST v4 et GraphQL. Merge requests hors périmètre.
Convention : toute affirmation non lue dans une page de documentation est marquée `⚠️ non vérifié`.

---

## 1. L'objet « tâche » — l'issue

### Champs canoniques (REST, `GET /projects/:id/issues/:issue_iid`)

Vus dans l'exemple de réponse de la doc :
`id`, `iid`, `project_id`, `title`, `description`, `state`, `created_at`, `updated_at`,
`closed_at`, `closed_by`, `labels`, `milestone`, `assignees`, `author`, `web_url`,
`references`, `type`, `issue_type`, `severity`, `task_completion_status`, `moved_to_id`,
`_links`.

Extrait exact de la doc :

```json
{
  "id": 41,
  "iid": 1,
  "state": "closed",
  "labels": ["foo", "bar"],
  "type": "ISSUE",
  "issue_type": "issue",
  "moved_to_id": null,
  "web_url": "http://gitlab.example.com/my-group/my-project/issues/1",
  "references": { "short": "#1", "relative": "#1", "full": "my-group/my-project#1" },
  "task_completion_status": { "count": 0, "completed_count": 0 },
  "_links": { "self": "…/api/v4/projects/4/issues/41", "notes": "…", "award_emoji": "…", "project": "…" }
}
```

### Identité — le point délicat

Trois identifiants coexistent, et ils n'ont pas la même stabilité :

| Identifiant | Portée | Usage API | Stabilité |
|---|---|---|---|
| `id` | global à l'instance | `GET /issues/:id` — **réservé aux administrateurs** | stable, mais inutilisable par un client ordinaire |
| `iid` | interne au **projet** (`#1`) | c'est lui qu'attendent **tous** les endpoints projet (`/projects/:id/issues/:issue_iid`) | stable tant que l'issue reste dans son projet |
| `references.full` | `groupe/projet#iid` | affichage / résolution textuelle | change si le projet est renommé ou transféré |

Conséquence structurante pour une abstraction : **la clé de travail d'un client REST est le couple
`(project_id, iid)`**, pas un identifiant global. Le `id` global existe mais l'endpoint qui l'accepte
est administrateur seulement.

Deux opérations cassent cette clé :
- `POST /projects/:id/issues/:issue_iid/move` (paramètre `to_project_id`) — l'issue change de projet,
  donc de `iid` ; l'ancienne porte alors `moved_to_id` ;
- `POST /projects/:id/issues/:issue_iid/clone`.

`web_url` est dérivé du chemin du projet : il n'est donc pas plus stable que `references.full`.

### Types d'issue

Le paramètre `issue_type` accepte : `issue`, `incident`, `test_case`, `task`.
Le champ `type` réapparaît en majuscules dans la réponse (`"type": "ISSUE"`) — deux champs
redondants et de casse différente pour la même information.

### Migration vers les work items

- Les work items unifient issues, epics, tasks, objectives/key results, test cases (et incidents,
  cités comme type filtrable). Tier annoncé : **Free, Premium, Ultimate**.
- Les epics ont été migrés vers le framework work items : bêta en **17.2**, GA en **18.1**.
- En **18.10 et ultérieur**, les pages séparées *Plan > Issues* et *Plan > Epics* sont remplacées par
  *Plan > Work items* ; `/issues/:iid` et `/epics/:iid` redirigent vers `/work_items/:iid`.
- L'API REST `issues` reste documentée et opérationnelle. La doc lue **ne contient pas** d'annonce de
  dépréciation de l'API REST Issues ni d'obligation de passer à l'API GraphQL work items.
  ⚠️ non vérifié : le calendrier de dépréciation éventuel de l'API REST Epics.

**Lecture pour Cursus** : le modèle bouge sous les pieds. Une abstraction qui s'adosse à REST
`issues` + `boards` est aujourd'hui sûre, mais le vocabulaire produit (« work item ») diverge déjà du
vocabulaire API (« issue »).

---

## 2. Hiérarchie

Il faut séparer nettement **hiérarchie parent/enfant** et **rattachement latéral**.

### Vraie hiérarchie

- **Task → Issue** : une task est un work item enfant d'une issue. Créée depuis *Plan > Work items*
  (type « Task »), ou par conversion d'une case à cocher de la description (« Convert to child item »),
  ou par rattachement d'une task existante via la section *Child items*. Tier : **Free, Premium,
  Ultimate**. La doc lue ne dit pas explicitement qu'une task possède son propre `iid` — mais
  `issue_type: task` est une valeur acceptée par l'API Issues, ce qui implique qu'une task est bien un
  enregistrement de type issue adressable par `iid`.
- **Epic** : tier **Premium, Ultimate**. Un epic est parent d'une ou plusieurs issues ; il peut aussi
  être parent d'**epics enfants**, et cette imbrication est réservée à **Ultimate**.
- **Profondeur maximale** : ⚠️ non vérifié — la page epics consultée ne donne pas de limite de
  profondeur pour les epics enfants (la valeur souvent citée de 7 niveaux n'a pas été retrouvée).

### Rattachements qui ne sont PAS de la hiérarchie

- **Issue links** (`/projects/:id/issues/:issue_iid/links`) : relation **bidirectionnelle**, types
  `relates_to` (défaut, **Free**), `blocks`, `is_blocked_by`. Création : `target_project_id` +
  `target_issue_iid` obligatoires, `link_type` optionnel. Suppression par `issue_link_id`.
  ⚠️ non vérifié : le tier exact requis pour `blocks`/`is_blocked_by` (la page ne le précise pas ;
  seul `relates_to` est explicitement donné comme Free).

### Conteneurs temporels

- **Milestones** : disponibles au niveau **projet et groupe**, une seule par issue (`milestone_id`).
- **Iterations** : tier **Premium, Ultimate**, **niveau groupe uniquement**, regroupées en *iteration
  cadences* (génération automatique toutes les 1 à 4 semaines, roulement possible des issues non
  terminées). Contraintes : dates de début ET de fin obligatoires, pas de chevauchement dans une même
  cadence.

---

## 3. Étiquettes

### Objet label (REST, `GET /projects/:id/labels`)

`id`, `name`, `color`, `text_color`, `description`, `description_html`, `subscribed`, `priority`,
`is_project_label`, `archived` (introduit en 18.3, GA annoncée 18.10), plus les compteurs
`open_issues_count`, `closed_issues_count`, `open_merge_requests_count`.

### Portée projet vs groupe

- Labels de projet et labels de groupe sont deux familles ; `is_project_label` les distingue dans la
  réponse.
- Les labels de groupe sont **hérités** par les projets du groupe.
- `PUT /projects/:id/labels/:label_id/promote` promeut un label projet en label groupe — et **« The
  label keeps its ID »**, ce qui est une bonne nouvelle pour un cache local.

### CRUD

- Créer : `POST /projects/:id/labels`, **`name` et `color` obligatoires** ; optionnels `description`,
  `priority`, `archived`.
- Modifier : `PUT …/:label_id`, « At least one parameter is required », champs `new_name`, `color`,
  `description`, `priority`, `archived`.
- Supprimer : `DELETE …/:label_id`.

### Scoped labels — le mécanisme sans équivalent

Tier : **Premium, Ultimate**.

- Syntaxe : double deux-points. « A scoped label uses a double-colon (`::`) syntax in its title, for
  example: `workflow::in-review`. »
- **Exclusion mutuelle** : « An issue, merge request, or epic cannot have two scoped labels, of the
  form `key::value`, with the same `key`. » Poser `priority::high` sur une issue portant
  `priority::low` **retire automatiquement** cette dernière.
- **Scopes imbriqués** : « Everything before the last `::` is the scope ». Donc
  `workflow::backend::review` et `workflow::backend::development` s'excluent (même scope
  `workflow::backend`), tandis que `workflow::backend::review` et `workflow::frontend::review`
  coexistent.

C'est le mécanisme qui permet à GitLab de simuler un **champ à valeur unique** (un statut, une
priorité) sans avoir de champ statut. Il est purement conventionnel côté données : un scoped label
reste un label ordinaire, seul le moteur d'application impose l'exclusion.

### Multi-valuation et création à la volée

- Une issue porte **n** labels (`labels` est un tableau de chaînes).
- ⚠️ non vérifié : le fait que passer un nom de label inexistant à `POST /projects/:id/issues` crée
  le label à la volée. La page Issues liste `labels` comme paramètre optionnel sans décrire ce
  comportement.

---

## 4. États et colonnes — le point central

### L'état natif est binaire

Une issue a `state` = `opened` ou `closed`. **Il n'existe pas de champ statut** ni de machine à états
métier au niveau de l'issue dans l'API REST lue. Tout ce qui ressemble à un workflow (`À faire`,
`En cours`, `En revue`) est encodé en **étiquettes**, généralement scoped (`workflow::…`).

⚠️ non vérifié : la page *Tasks* mentionne en passant que « status management » nécessite
Premium/Ultimate — il existe donc peut-être un champ *status* de work item récent ; je n'ai pas lu de
page décrivant ce champ ni son exposition API. À vérifier avant de conclure que l'état est
strictement binaire dans le monde work items.

### La colonne est une *liste* de board

Un board (`GET /projects/:id/boards`) porte : `id`, `name`, `hide_backlog_list`, `hide_closed_list`,
`project`, `milestone`, `lists`.

Une **liste** est une colonne. Elle est adossée à un critère :

| Adossement | Paramètre de création | Tier |
|---|---|---|
| Label | `label_id` | Free |
| Assigné | `assignee_id` | Premium, Ultimate |
| Milestone | `milestone_id` | Premium, Ultimate |
| Iteration | `iteration_id` | Premium, Ultimate |

Attributs d'une liste : `id`, `label`, `position`, `max_issue_count`, `max_issue_weight`,
`limit_metric`.

Deux listes permanentes encadrent le board : **Open** (à gauche, tout ce qui n'est dans aucune autre
liste) et **Closed** (à droite). Non déplaçables, mais masquables (`hide_backlog_list`,
`hide_closed_list`).

### Déplacer une carte = poser/retirer une étiquette ? — Oui

La doc utilisateur le confirme : glisser une issue d'une liste à l'autre **modifie les labels et les
assignations** selon les listes source et cible. Exemple donné : d'*Open* vers une liste label →
ajout du label B ; d'une liste label vers *Closed* → retrait du label d'origine **et** fermeture de
l'issue.

Autrement dit : **il n'y a pas d'opération « déplacer » sur l'issue**. L'écriture consiste à
composer `add_labels` / `remove_labels` (et éventuellement `state_event`, `assignee_ids`,
`milestone_id`, `iteration_id`) pour reproduire l'effet du glisser-déposer.

### Ordre dans une liste

Oui, un ordre existe et il est persisté : « You're able to change that order by dragging the issues.
The changed order is saved, so that anybody who visits the same board later sees the reordering. »
Côté API, `PUT /projects/:id/issues/:issue_iid/reorder` existe. ⚠️ non vérifié : ses paramètres
exacts (`move_after_id` / `move_before_id`) et le fait que l'ordre soit propre à un board donné.

Les **listes**, elles, ont un `position` modifiable par `PUT /projects/:id/boards/:board_id/lists/:list_id`.

### Une issue sur plusieurs boards

Oui, et même sur plusieurs listes : « An issue can appear on multiple boards simultaneously if it has
more than one label », chaque liste label affichant indépendamment les issues correspondantes. Il n'y
a donc **aucune contrainte d'unicité de colonne** au niveau des données — l'exclusion mutuelle n'est
obtenue que si l'on utilise des scoped labels (Premium+).

### Nombre de boards

- Projet : « Multiple issue boards allow for more than one issue board for: A project in all tiers ».
- Groupe : un seul board en **Free**, plusieurs en **Premium/Ultimate**. L'API groupe le confirme —
  créer et supprimer un board de groupe est marqué **Tier: Premium, Ultimate**, la lecture étant
  Free.

### Workflow contraint

Aucun. Pas de transitions autorisées/interdites, pas de champ obligatoire à la transition, pas de
résolution. Le seul verrou est l'exclusion mutuelle des scoped labels.

### Lire le contenu d'une colonne

L'API boards documente le CRUD des boards et des listes, mais je **n'ai pas trouvé** d'endpoint REST
renvoyant les issues d'une liste. En pratique, lire une colonne = `GET /projects/:id/issues?labels=…`
(en respectant le scope du board : milestone, weight, assignee). ⚠️ non vérifié : l'absence d'un tel
endpoint est une observation par omission, pas une affirmation de la doc.

---

## 5. Écriture

| Opération | Endpoint REST | Obligatoire | Idempotence |
|---|---|---|---|
| Créer une issue | `POST /projects/:id/issues` | **`title`** | **Non.** Chaque appel crée une issue. Aucune clé d'idempotence documentée. Un rejeu duplique. |
| Éditer les champs | `PUT /projects/:id/issues/:issue_iid` | au moins un champ | Oui de fait : écriture de valeurs absolues, rejouable. |
| Déplacer de colonne | *aucun endpoint dédié* → `PUT …/issues/:issue_iid` avec `add_labels`/`remove_labels` (+ `state_event`, `assignee_ids`, `milestone_id`, `iteration_id` selon le type de liste) | — | Oui, si l'on exprime la cible en add/remove : rejouable sans dégât. |
| Poser une étiquette | `PUT …/issues/:issue_iid` avec `add_labels` | — | Oui, ajout d'un label déjà présent = sans effet ⚠️ non vérifié (comportement plausible mais non affirmé par la doc). |
| Retirer une étiquette | idem avec `remove_labels` | — | Idem. |
| Commenter | `POST /projects/:id/issues/:issue_iid/notes` | **`body`** (max 1 000 000 caractères) | **Non.** « Each POST request generates a new note » ; aucune déduplication. |
| Fermer | `PUT …/issues/:issue_iid` avec `state_event=close` | — | Oui. |
| Rouvrir | `PUT …/issues/:issue_iid` avec `state_event=reopen` | — | Oui. |

### `labels` vs `add_labels` / `remove_labels`

C'est le point le plus utile de cette section : l'API Issues expose **les trois** :
- `labels` — **remplace l'intégralité** du jeu d'étiquettes (perte silencieuse de tout label posé
  entre-temps par un tiers) ;
- `add_labels` — ajoute ;
- `remove_labels` — retire.

Un client concurrent doit **toujours** préférer `add_labels`/`remove_labels`. Utiliser `labels`
équivaut à un write sans compare-and-swap.

Autres endpoints d'écriture notables : `POST …/issues/:issue_iid/move` (`to_project_id`),
`POST …/issues/:issue_iid/clone`, `PUT …/issues/:issue_iid/reorder`,
`DELETE /projects/:id/issues/:issue_iid`.

Il n'existe pas, dans les pages lues, de mécanisme d'ETag / `If-Match` / version optimiste sur les
issues. ⚠️ non vérifié : l'absence totale de contrôle de concurrence.

### GraphQL

Le endpoint est `/api/graphql`, mutations possibles avec le scope `api`. ⚠️ non vérifié : les noms
exacts des mutations (`updateIssue`, `issueSetLabels`, `issueMoveList`, `boardListCreate`…) — la page
de référence GraphQL n'a pas pu être parcourue en détail. Toutes les mutations reçoivent leurs
arguments dans un objet unique `input` et retournent au moins un champ `errors`.

---

## 6. Authentification

Méthodes acceptées par l'API REST :

| Méthode | Transmission |
|---|---|
| OAuth 2.0 | `Authorization: Bearer <token>` ou paramètre `access_token` |
| Personal access token (PAT) | en-tête **`PRIVATE-TOKEN`** (recommandé) ou `Authorization: Bearer` |
| Project access token | identique au PAT |
| Group access token | identique au PAT |
| Job token (CI) | en-tête `JOB-TOKEN` (`CI_JOB_TOKEN`) |
| Cookie de session | `_gitlab_session` |
| Impersonation token / `sudo` | administrateurs |

GraphQL accepte les mêmes tokens (`Authorization: Bearer`, ou paramètres `access_token` /
`private_token`) et le cookie de session.

### Portées (scopes) — liste documentée

`api` (« complete read and write access to the API for the token's scope »), `read_api` (lecture
seule), `read_user`, `read_repository`, `write_repository`, `read_registry`, `write_registry`,
`read_virtual_registry`, `write_virtual_registry`, `create_runner`, `manage_runner`, `ai_features`,
`k8s_proxy`, `admin_mode`, `read_service_ping`, `sudo`, **`self_rotate`** (« Grants permission to
rotate this token. Cannot rotate other tokens. »).

Pour Cursus : lecture des tâches → `read_api` ; écriture (création, labels, commentaires) → `api`.
GraphQL : `read_api` pour les queries, `api` pour les mutations.

### Ce qu'un client de bureau doit stocker

1. **L'URL de l'instance.** GitLab est auto-hébergeable : `https://gitlab.com` n'est qu'une instance
   parmi d'autres, et les endpoints REST/GraphQL sont relatifs à cette base
   (`https://<site>/api/v4`, `https://<site>/api/graphql`). **L'URL fait partie de l'identité de
   connexion** — deux comptes sur deux instances ne sont pas interchangeables, et un `project_id`
   n'a de sens que relativement à une instance.
2. Le token (PAT, project/group access token) ou le couple access/refresh token OAuth.
3. Le type de token, car il détermine l'expiration et la stratégie de renouvellement.

### Expiration / rotation

- **Les PAT doivent avoir une date d'expiration** : la création de tokens sans expiration a été
  supprimée en **16.0**.
- Sans date saisie, l'expiration est fixée à **365 jours**. Durée maximale par défaut : 365 jours,
  extensible à 400 jours via feature flag depuis **17.6** ; en Ultimate, un administrateur peut
  configurer une durée maximale.
- Les tokens expirent **à minuit UTC** à la date indiquée.
- **Rotation** : possible (nouveau token, mêmes permissions et scopes) ; rotation depuis l'interface
  introduite en **17.7** ; le scope `self_rotate` existe pour qu'un token puisse se renouveler
  lui-même. ⚠️ non vérifié : l'endpoint REST exact de rotation (`POST
  /personal_access_tokens/self/rotate` selon la mémoire — non lu).
- **OAuth 2.0** : les tokens expirent au bout de **deux heures**, renouvelables via `refresh_token`.

Conséquence : un client de bureau doit prévoir **au minimum** une gestion d'expiration annuelle des
PAT (avec avertissement à l'utilisateur), et une boucle de refresh de 2 h s'il choisit OAuth.

---

## 7. Transport et limites

### REST vs GraphQL

- REST v4 : `https://<site>/api/v4`. Couvre issues, labels, notes, issue links, **boards de projet et
  de groupe** (CRUD boards + CRUD listes), mais pas — d'après ce que j'ai lu — la lecture du contenu
  d'une colonne.
- GraphQL : `https://<site>/api/graphql`, pattern **Relay** (connections, curseurs), page maximale de
  **100** enregistrements pour la plupart des connections. La doc **n'affirme pas** que GraphQL est
  l'API primaire, ni ne documente une parité fonctionnelle avec REST.
- ⚠️ non vérifié : le détail de ce que GraphQL couvre pour les boards (types `Board`, `BoardList`,
  mutations associées) — non lu dans la référence.

### Pagination REST

**Offset** (défaut) : `page` (défaut 1), `per_page` (défaut **20**, max **100**). En-têtes `Link`
(`prev`, `next`, `first`, `last`) et `x-page`, `x-per-page`, `x-prev-page`, `x-next-page`, `x-total`,
`x-total-pages`.
Piège majeur : **au-delà de 10 000 enregistrements**, GitLab **ne renvoie plus** `x-total`,
`x-total-pages` ni le lien `rel="last"`.

**Keyset** : `pagination=keyset` + `order_by` + `sort` (`asc`/`desc`), `per_page` (défaut 20, max
100). Le `Link` contient un curseur ; en-têtes `X-NEXT-CURSOR` et `X-PREV-CURSOR`. C'est le mode à
privilégier pour une synchronisation complète.

### Quotas gitlab.com

Chiffres documentés :
- trafic **API authentifié** pour un utilisateur : **2 000 requêtes par minute** ;
- trafic **non authentifié** depuis une IP : **500 requêtes par minute**.

En-têtes `RateLimit-*` : la page renvoie vers *User and IP rate limits* sans les détailler ;
⚠️ non vérifié quant aux noms exacts des en-têtes.
⚠️ non vérifié : un quota GraphQL spécifique (en points/minute) — non documenté sur les pages lues.

### Webhooks

- Configurables **par projet** (rôle Maintainer/Owner) et **par groupe** (rôle Owner, tier
  **Premium/Ultimate**). Si les deux existent, les deux se déclenchent pour un événement du projet.
- Authentification du récepteur : **signing token** recommandé (HMAC-SHA256, en-tête
  `webhook-signature`) ou **secret token** hérité, en clair dans `X-Gitlab-Token`.
- Fiabilité : désactivation temporaire après **4 échecs consécutifs**, permanente après **40**. La
  doc prévient de possibles **événements dupliqués** en cas de timeout — le consommateur doit être
  idempotent.
- ⚠️ non vérifié : la liste exacte des types d'événements issues (`issue`, `note`,
  `confidential_issue`, `confidential_note`) — la page lue ne les énumère pas.

---

## 8. Pièges et singularités — ce qui ne se traduira pas ailleurs

1. **La colonne n'existe pas comme donnée de l'issue.** Chez Linear (`state`) et Jira (`status` +
   transitions), la position dans le flux est un champ de la tâche. Chez GitLab, c'est une **jointure
   entre un label et une liste de board**. Conséquence directe : « quel est le statut de cette
   tâche ? » n'a **pas de réponse hors contexte d'un board** ; il faut connaître le board pour savoir
   quelle famille de labels lire. Une abstraction commune doit soit imposer une convention
   (`workflow::*`), soit rendre le board obligatoire dans le contexte de lecture.

2. **Écrire une colonne = poser/retirer des labels — non atomique.** Un déplacement de colonne est un
   `add_labels` + `remove_labels` dans le même PUT ; si la liste cible est adossée à un assigné ou à
   une itération, c'est encore autre chose qu'il faut écrire. L'opération unique `moveTo(column)` des
   autres outils devient ici une **compilation** vers un jeu de champs dépendant du type de liste.

3. **Rien n'empêche une issue d'être dans deux colonnes à la fois.** L'exclusion mutuelle n'existe
   qu'avec les **scoped labels**, donc uniquement à partir de **Premium**. En Free, une abstraction
   « une tâche a un statut » est structurellement invalidable par les données. Le client doit être
   capable de représenter et de réparer l'état « deux labels de la même famille ».

4. **Les scoped labels n'ont aucun équivalent.** `key::value`, avec scopes imbriqués (`scope` =
   tout ce qui précède le **dernier** `::`). C'est un champ typé simulé par convention de nommage.
   Le mapper vers un champ statut ailleurs est faisable ; l'inverse (importer un statut Jira dans
   GitLab) suppose de **créer** les labels et de dépendre du tier.

5. **La segmentation par palier de licence est un écart de modèle, pas de quota.** Sur Free
   disparaissent : scoped labels, epics, iterations, listes par assigné/milestone/itération,
   boards de groupe multiples, webhooks de groupe. Une abstraction ne peut pas se contenter d'un
   « adaptateur GitLab » : il lui faut une **capability matrix** interrogée au runtime, ou une
   dégradation explicite. Et la licence n'est pas déductible du seul token — ⚠️ non vérifié :
   l'existence d'un endpoint fiable exposant le plan sur gitlab.com.

6. **L'identité est un triplet, pas un identifiant.** `(instance URL, project_id, iid)`. Le `id`
   global n'est adressable que par un administrateur. Le `move` entre projets **change l'identité**
   de la tâche — les autres outils gardent en général une clé stable après déplacement. Il faut donc
   suivre `moved_to_id` pour ne pas perdre le fil.

7. **Projet vs groupe est une dualité omniprésente.** Labels, boards, milestones existent aux deux
   niveaux ; iterations et epics **uniquement** au niveau groupe. Un label de projet peut être promu
   en label de groupe (en gardant son `id`), ce qui change silencieusement sa portée. Il n'y a pas
   d'équivalent propre chez Linear (team/workspace s'en rapproche le plus) et Jira modélise cela tout
   autrement (project + scheme).

8. **Deux vocabulaires en cours de divergence** : le produit parle de *work items* (issue, task, epic,
   objective… fusionnés depuis 18.1/18.10) tandis que l'API REST parle toujours d'*issues* avec un
   `issue_type`. Choisir REST aujourd'hui, c'est parier sur la persistance d'une surface qui n'est
   plus celle du produit.

9. **Aucune idempotence en création.** Ni pour les issues ni pour les commentaires : aucun mécanisme
   de clé d'idempotence documenté. Un rejeu après timeout duplique. Un outil qui écrit doit tenir sa
   propre table de corrélation (ex. marqueur dans la description ou dans le corps du commentaire).

10. **`labels` écrase tout.** Si l'abstraction expose un `setLabels(...)` naïf qui se traduit par le
    paramètre `labels`, elle perdra les labels posés par d'autres utilisateurs entre la lecture et
    l'écriture. Toujours passer par `add_labels`/`remove_labels`.

11. **Pas de contrôle de concurrence visible.** Aucun ETag / version optimiste rencontré sur les
    endpoints issues — dernier écrivain gagne.

12. **La pagination perd ses totaux au-delà de 10 000 enregistrements**, ce qui casse toute barre de
    progression naïve sur les gros groupes. Utiliser la pagination keyset.

---

## Sources consultées

- https://docs.gitlab.com/api/issues/ — API REST Issues (champs, CRUD, `state_event`, `add_labels`/`remove_labels`, `issue_type`, move/clone/reorder)
- https://docs.gitlab.com/api/issues/#single-issue — exemple JSON complet (`references`, `_links`, `moved_to_id`), endpoint `GET /issues/:id`
- https://docs.gitlab.com/api/boards/ — boards de projet, listes, scopes de liste et tiers
- https://docs.gitlab.com/api/group_boards/ — boards de groupe, tiers de création/suppression
- https://docs.gitlab.com/user/project/issue_board/ — comportement du glisser-déposer, listes Open/Closed, ordre, multi-boards, nombre de boards par tier
- https://docs.gitlab.com/api/labels/ — objet label, CRUD, promotion en label de groupe
- https://docs.gitlab.com/user/project/labels/ — scoped labels : syntaxe, exclusion mutuelle, scopes imbriqués, tier
- https://docs.gitlab.com/user/work_items/ — initiative work items, types, migration 18.10
- https://docs.gitlab.com/user/tasks/ — tasks comme work items enfants
- https://docs.gitlab.com/user/group/epics/ — epics, tiers, epics enfants (Ultimate), migration 17.2/18.1
- https://docs.gitlab.com/user/group/iterations/ — iterations, cadences, tier, niveau groupe
- https://docs.gitlab.com/api/issue_links/ — issue links, types de lien
- https://docs.gitlab.com/api/notes/ — notes (commentaires) sur issues
- https://docs.gitlab.com/api/rest/authentication/ — méthodes d'authentification REST
- https://docs.gitlab.com/security/tokens/access_token_scopes/ — liste des scopes
- https://docs.gitlab.com/user/profile/personal_access_tokens/ — expiration, rotation, 16.0/17.6/17.7
- https://docs.gitlab.com/api/rest/#pagination — pagination offset et keyset, seuil des 10 000
- https://docs.gitlab.com/api/graphql/ — endpoint, authentification, Relay, page max 100
- https://docs.gitlab.com/user/gitlab_com/#rate-limits-on-gitlabcom — 2 000 req/min authentifié, 500 req/min non authentifié
- https://docs.gitlab.com/user/project/integrations/webhooks/ — webhooks projet/groupe, signature, désactivation après échecs
