# Les quatre trackers, superposés

Synthèse comparative de Linear, Jira Cloud, GitHub (Issues + Projects v2) et GitLab (Issues + boards),
menée le 2026-07-21 en vue du port de tâches de Cursus (§7.10.2 et §7.10.3 de l'architecture).

Les quatre fiches détaillées vivent à côté de ce document : [`linear.md`](linear.md),
[`jira.md`](jira.md), [`github.md`](github.md), [`gitlab.md`](gitlab.md). Elles ont été produites en
parallèle sur une **grille imposée en huit points**, précisément pour être superposables — ce document
est cette superposition, il ne la remplace pas.

**Ce document ne dessine pas le port.** C'était une décision explicite : on regarde d'abord ce que le
terrain impose, on dessine ensuite. Sa dernière section liste les arbitrages que le port devra trancher,
sans les trancher.

## Fiabilité

Chaque fiche marque `⚠️ non vérifié` ce qui vient d'une mémoire d'entraînement plutôt que d'une page
lue. Trois d'entre elles se sont adossées à une source machine plutôt qu'à la documentation rendue :

| Outil | Source de vérité employée |
|---|---|
| Linear | schéma GraphQL du SDK officiel (~49 800 lignes), grepé localement |
| Jira | spécifications OpenAPI officielles (plateforme v3 + Agile 1.0), téléchargées |
| GitHub | documentation officielle + journal des changements |
| GitLab | documentation officielle |

Cette précaution a déjà payé une fois : le mandat donné à l'agent GitHub affirmait que Projects v2 est
GraphQL-seulement. **C'est faux depuis le 11 septembre 2025** — une API REST `projectsV2` existe.
L'agent a lu au lieu de confirmer la prémisse. À garder en tête pour les points encore marqués non
vérifiés : ils ne sont pas des détails, ce sont les endroits où le rapport peut mentir.

---

## 1. Le tableau de correspondance

| Concept | Linear | Jira Cloud | GitHub | GitLab |
|---|---|---|---|---|
| **La tâche** | `Issue` | `issue` (produit : « work item ») | `issue` | `issue` (produit : « work item ») |
| **Conteneur obligatoire** | équipe (`teamId`) | projet + type de ticket | dépôt | projet |
| **Identifiant stable** | `id` (UUID) | `id` (chaîne numérique) | `node_id` | `(instance, project_id, iid)` |
| **Identifiant lisible** | `BLA-123` — **mute** au changement d'équipe | `ED-24` — **mute** au *Move* ou renommage de projet | `#42` — local au dépôt, **mute** au transfert | `#1` — local au projet, **mute** au `move` |
| **Statut** | `WorkflowState` sur l'issue | `status` sur le ticket, issu du workflow | **rien** (`open`/`closed` seulement) | **rien** (`opened`/`closed` seulement) |
| **Colonne** | = le statut, avec sa `position` | vue d'un board : 1 colonne → **N** statuts | option d'un champ single-select du `ProjectV2Item` | liste de board adossée à **une étiquette** |
| **Changer d'état** | écrire `stateId` | `POST /transitions` avec une arête découverte | écrire un champ de l'item du projet | poser/retirer des étiquettes |
| **Transition refusable** | **non** | **oui** — conditions, validateurs, champs d'écran | non | non |
| **Étiquettes : portée** | organisation ou équipe | **site entier**, sans identité ni couleur | **dépôt** | projet ou groupe |
| **Étiquettes : exclusivité** | oui, intra-groupe (natif) | non | non | oui, via *scoped labels* — **Premium+** |
| **Sous-tâches** | `parent`/`children`, profondeur non documentée | niveaux de type (`hierarchyLevel`), Epic/Story/Subtask | *sub-issues* : 100/parent, **8 niveaux**, cross-dépôt | *tasks* enfants d'issue ; epics **Premium+** |
| **Regroupement temporel** | `Cycle` (par équipe) | Sprint (Agile API) + Version | Milestone (par dépôt) | Milestone ; Iteration **Premium+** |
| **Position dans la colonne** | `sortOrder` (flottant, calculé par le client) | rang LexoRank, endpoint **relatif**, 50 max, 207 partiel | `updateProjectV2ItemPosition(afterId)` | `reorder`, paramètres ⚠️ non vérifiés |
| **Transport** | GraphQL seul | REST (deux surfaces : plateforme + Agile) | REST + GraphQL, partiellement disjoints | REST v4 + GraphQL |
| **Idempotence en création** | aucune — sauf `id` fourni par le client | **aucune** | aucune | aucune |
| **Format du texte** | Markdown | **ADF** (arbre JSON) | Markdown | Markdown |

---

## 2. Les six lignes de fracture

C'est la section qui compte. Un port se dessine sur ce qui *ne* se recouvre *pas*.

### 2.1 La colonne n'est pas au même endroit — et n'a même pas la même arité

C'est la fracture principale, et elle est plus profonde qu'un simple écart d'implémentation.

- **Linear** fusionne statut et colonne en un seul objet. Une tâche a exactement une colonne.
- **Jira** les sépare : le statut est sur le ticket, la colonne est une propriété du *board*, et un board
  agrège **N statuts par colonne**. Un même ticket a autant de colonnes que de boards qui le capturent.
  Des statuts peuvent n'être mappés à aucune colonne — le ticket devient alors invisible sur ce board.
- **GitHub** met la colonne hors de la tâche : c'est une valeur de champ portée par le `ProjectV2Item`,
  l'enveloppe d'une issue dans un projet donné. Une issue vit dans **N projets**, avec N jeux de valeurs.
- **GitLab** n'a pas de colonne du tout : une liste de board est adossée à une étiquette, et rien
  n'empêche une issue de porter deux étiquettes de deux listes du même board.

La conclusion se formule en une phrase : **« quel est le statut de cette tâche ? » n'a de réponse chez
aucun des quatre sans préciser un contexte** — sauf chez Linear, le seul où la question est bien posée.
Chez les trois autres, la bonne unité n'est pas la tâche mais le **couple (tâche, tableau)**.

Corollaire pour Cursus : un modèle qui expose `Task.Status` ment sur trois outils sur quatre. Ce n'est
pas une imprécision qu'on rattrape dans l'adaptateur — c'est une erreur d'arité.

### 2.2 Jira est le seul qui peut refuser

Chez Linear, GitHub et GitLab, changer d'état est une écriture de champ (ou d'étiquette) qui réussit
toujours. Chez Jira, c'est une opération d'une autre nature :

1. `GET /transitions` pour découvrir les arêtes praticables **depuis le statut courant** ;
2. choisir celle dont le `to` correspond à la cible ;
3. `POST /transitions` avec son `id`, qui n'est ni stable ni devinable, et varie par projet.

Le passage peut être absent du workflow, refusé par une condition ou un validateur, ou **exiger des
champs** (typiquement une résolution). Et le piège le plus vicieux : **quand rien n'est possible, ou
quand la permission manque, `GET /transitions` renvoie une liste vide — pas une erreur.** Les deux cas
sont indiscernables.

Un port qui modélise le changement d'état comme un `SetField` fonctionne partout sauf là où ça compte.
Un port qui modélise « les transitions disponibles » comme un concept de premier ordre n'a rien à y
mettre pour les trois autres. Aucune des deux formes n'est gratuite.

### 2.3 Les étiquettes sont quatre objets différents portant le même nom

| | Identité | Portée | Couleur | Création | Exclusivité |
|---|---|---|---|---|---|
| Linear | objet avec UUID | organisation ou équipe | oui | mutation dédiée | **oui**, intra-groupe |
| Jira | **aucune** — une chaîne nue | **site entier** | non | aucune (elle naît de l'usage) | non |
| GitHub | objet, mais adressé par son **nom** | **dépôt** | oui | endpoint dédié | non |
| GitLab | objet avec id | projet ou groupe | oui | endpoint dédié | via `::` (**Premium+**) |

Trois conséquences concrètes :

- **Jira n'accepte pas les espaces** dans une étiquette (la saisie les traite comme des séparateurs).
  Toute étiquette importée d'ailleurs doit être translittérée, et **la translittération n'est pas
  réversible**.
- **GitHub n'a aucun vocabulaire d'étiquettes transversal.** Un agrégateur multi-dépôts voit cinq fois
  « bug », avec cinq identifiants et cinq couleurs. Fusionner par nom est *faux* ; poser « bug » sur une
  issue d'un dépôt qui ne l'a pas est une erreur, pas un cas nominal.
- **L'exclusivité est native chez Linear et payante chez GitLab.** Sur un GitLab Free, l'invariant
  « une tâche a un statut » est structurellement invalidable par les données : le port doit savoir
  *représenter* et *réparer* l'état « deux étiquettes de la même famille ».

### 2.4 Aucun identifiant n'est à la fois lisible et stable

Les quatre offrent une clé lisible — `BLA-123`, `ED-24`, `#42`, `#1` — et **les quatre la font muter** :
changement d'équipe chez Linear, *Move* ou renommage de projet chez Jira, transfert de dépôt chez GitHub,
`move` entre projets chez GitLab. Linear et Jira acceptent d'ailleurs la clé lisible en entrée d'API,
ce qui rend l'erreur facile à ne pas voir : le code marche jusqu'au jour où quelqu'un déplace une tâche.

**Ce que Cursus persiste doit être opaque ; ce qu'il affiche doit être re-résolu.** Et chez GitLab
l'identifiant opaque global n'est même pas adressable par un client ordinaire — la clé de travail y est
un triplet incluant l'URL de l'instance.

### 2.5 Aucun n'offre d'idempotence en création

Le mot « idempotence » est **absent de la spécification OpenAPI complète de Jira**. Aucun des quatre
n'expose de clé d'idempotence sur la création d'une tâche ou d'un commentaire. Un rejeu après timeout
duplique, partout.

Les leviers disponibles diffèrent, et c'est là que ça devient intéressant pour un moteur qui rejoue :

| Outil | Levier de rejeu sûr |
|---|---|
| Linear | **l'`id` UUID est fourni par le client** dans l'input de création — le seul mécanisme natif des quatre |
| Jira | propriété de ticket (`/properties/{key}`) portant une clé de corrélation, cherchée avant création… mais la recherche JQL **n'est pas cohérente après écriture** |
| GitHub | registre de corrélation local, ou marqueur dans le corps |
| GitLab | registre de corrélation local, ou marqueur dans la description |

Cette contrainte ne s'arrête pas au port : **elle remonte jusqu'au journal**. Si une étape de workflow
crée une tâche, l'identifiant (ou la clé de corrélation) doit être engendré et **journalisé avant
l'appel**, sinon la reprise après crash duplique. C'est un ordre d'opérations, donc une contrainte de
conception du moteur, pas seulement de l'adaptateur.

Corollaire moins évident, unanime celui-là : **écrire une collection entière écrase le travail des
autres.** `labels` chez GitLab et Jira, `PUT labels` chez GitHub, `labelIds` chez Linear remplacent tout.
Les quatre offrent une voie additive (`add_labels`/`remove_labels`, `update:{add|remove}`, `POST labels`,
`addedLabelIds`) — c'est la seule à employer, car aucun des quatre n'a de contrôle de concurrence
optimiste. Partout, dernier arrivé gagne.

### 2.6 Ce qui décide de la forme n'est pas toujours l'outil

Trois écarts *internes* à un même outil, aussi grands que les écarts entre outils :

- **Jira : company-managed vs team-managed.** En team-managed, les statuts et les champs deviennent
  scopés au projet (deux « In Review » dans deux projets sont deux objets distincts), les composants
  disparaissent, et l'API Agile des epics ne fonctionne plus. Le port doit brancher son comportement sur
  `project.simplified`.
- **GitLab : le palier de licence est un écart de modèle.** Free perd les scoped labels, les epics, les
  itérations, les listes par assigné. Ce n'est pas un quota, c'est une absence de concepts. Il faut une
  matrice de capacités interrogée à l'exécution — et ⚠️ *rien ne dit qu'on puisse lire le palier depuis
  un jeton*.
- **GitHub : la fracture d'authentification.** Le scope `repo` ne couvre **pas** les Projects (il faut
  `project`), et les jetons *fine-grained* ne semblent pas couvrir les projets appartenant à un
  utilisateur (⚠️ non vérifié — absence, donc à tester). Le choix du mécanisme d'authentification décide
  de ce que l'outil pourra faire, avant la première requête.

---

## 3. Le noyau réellement commun

Après soustraction, ce sur quoi les quatre se recouvrent vraiment est mince — mais net :

- une tâche a un **identifiant opaque stable**, un **titre**, une **description en texte long**, une
  **URL humaine**, un **auteur**, des **horodatages** de création et de mise à jour ;
- elle est **ouverte ou fermée** — le seul état que les quatre partagent réellement ;
- elle porte **zéro à N étiquettes** et **zéro à N assignés** ;
- elle appartient à **un conteneur obligatoire** (équipe, projet, dépôt) ;
- elle accepte des **commentaires** ;
- elle a **au plus un parent** et zéro à N enfants ;
- il existe une **projection stable de son avancement** en trois catégories — mais elle porte quatre
  noms différents et ne se lit pas au même endroit :

| Outil | Où lire l'avancement de façon stable |
|---|---|
| Linear | `WorkflowState.type` — taxonomie **fermée** : `triage`, `backlog`, `unstarted`, `started`, `completed`, `canceled`, `duplicate` |
| Jira | `statusCategory` — To Do / In Progress / Done (les *noms* de statuts sont arbitraires) |
| GitHub | rien de natif — `open`/`closed`, ou une convention sur le champ `Status` d'un projet |
| GitLab | rien de natif — `opened`/`closed`, ou une convention sur `workflow::*` |

**Linear et Jira offrent une catégorie normalisée ; GitHub et GitLab n'en ont aucune.** Chez ces deux
derniers, tout avancement plus fin que ouvert/fermé est une *convention*, pas une donnée. Le port ne
pourra pas la déduire — il devra la faire configurer, ou s'en passer.

---

## 4. Ce que ça impose à Cursus

Sans dessiner le port, sept conséquences tombent directement du terrain.

1. **La disponibilité d'une tâche ne peut pas se prédiquer sur « colonne + étiquettes » tel quel.**
   Le prédicat du §7.10.6 supposait cette maille ; elle n'existe telle quelle que chez Linear. Chez les
   trois autres il faut d'abord fixer un **contexte de tableau**, qui n'est pas une préférence
   d'affichage mais une **donnée de configuration du projet Cursus**.
2. **Ce qu'on stocke est opaque, ce qu'on affiche est re-résolu.** Aucun raccourci : la clé lisible est
   mutable chez les quatre.
3. **Créer, c'est journaliser avant d'appeler.** Sans idempotence serveur, la reprise après crash exige
   que la clé de corrélation existe dans le journal avant l'émission de la requête.
4. **Écrire une collection se fait toujours en delta**, jamais en remplacement — sinon Cursus efface
   les annotations des humains qui travaillent en même temps.
5. **Un client de bureau ne reçoit pas de webhooks.** Aucune URL publique. Les quatre outils supposent un
   récepteur joignable ; il faudra **poller**, ce qui remet les quotas au centre — et le plus mordant
   n'est jamais le quota nominal mais le plafond d'écriture : **80 créations/minute et 500/heure** chez
   GitHub, **20 écritures / 2 s par ticket** chez Jira.
6. **Ce qu'on stocke durablement est un secret rotatif, pas une chaîne figée.** Jira fait tourner ses
   jetons de rafraîchissement (le précédent est invalidé, fenêtre de rejeu de 10 minutes, mort après
   90 jours d'inactivité) ; GitLab impose une expiration aux jetons personnels ; l'identité de connexion
   inclut l'URL du site chez Jira et GitLab. Ce n'est pas « stocker un jeton dans le trousseau », c'est
   **réécrire un secret atomiquement**, avec ce que ça implique si deux instances de Cursus tournent.
7. **Atlassian décourage contractuellement** la collecte des jetons d'API des utilisateurs par une
   application distribuée. Le chemin « demande ta clé API et colle-la » — parfaitement acceptable chez
   les trois autres — est formellement à éviter pour Jira dès que Cursus est distribué. Tant qu'il
   s'agit du propriétaire du jeton sur sa machine, c'est légitime.

---

## 5. Ce que le port devra trancher

Six arbitrages, laissés ouverts à dessein.

1. **L'unité qui porte l'avancement.** La tâche, ou le couple (tâche, tableau) ? La fracture 2.1 pousse
   vers le couple ; c'est plus lourd, et ça rend le tableau obligatoire dans presque toute lecture.
2. **Ce qu'est « déplacer ».** Une opération de premier ordre qui peut *échouer* (forme Jira), ou une
   écriture de champ qui réussit toujours (forme des trois autres) ? Le premier choix impose aux trois
   autres une abstraction qu'ils ne remplissent pas ; le second ment sur Jira.
3. **La finesse de l'avancement.** S'en tenir aux trois catégories que seuls deux outils fournissent, ou
   faire configurer une correspondance par projet Cursus (ce que GitHub et GitLab exigent de toute façon) ?
4. **Ce qu'on fait des capacités absentes.** Une matrice interrogée à l'exécution, ou une dégradation
   silencieuse ? GitLab Free l'impose déjà, GitHub le pose sur l'authentification, Jira sur le style de
   projet.
5. **Où vit la clé de corrélation** que l'absence d'idempotence rend nécessaire — journal de run,
   table dédiée, ou propriété stockée côté tracker (seul Jira l'offre proprement) ?
6. **Le périmètre réel de l'écriture.** La recherche a couvert le CRUD complet, mais rien n'oblige le
   port à l'exposer. §7.10.3 disait « Cursus lit et annote » ; ce document ne contredit pas ce choix, il
   dit seulement ce que coûterait de l'élargir.

---

## 6. À vérifier avant d'implémenter

Les points que les fiches marquent `⚠️ non vérifié` et qui **changeraient une décision** s'ils tombaient
dans l'autre sens :

- **GitHub** — les jetons *fine-grained* couvrent-ils les projets d'un compte personnel ? Si non, il faut
  supporter aussi le jeton classique. (C'est une absence : non démontrable par lecture, seulement par test.)
- **GitLab** — existe-t-il un champ *status* sur les nouveaux *work items* ? La page *Tasks* mentionne une
  « gestion de statut » Premium sans la décrire. Si ce champ existe, la fracture 2.1 se réduit d'un cran.
- **GitLab** — peut-on lire le palier de licence depuis un jeton ? Sans ça, la matrice de capacités doit
  se déduire d'échecs, ce qui est nettement plus laid.
- **Linear** — rejouer une création avec le même `id` client : conflit, ou retour de l'existant ? C'est la
  différence entre « une vraie idempotence » et « une erreur à rattraper ».
- **Jira** — le rattachement à un epic passe-t-il désormais par `parent` en company-managed ? La chaîne
  « Epic Link » a disparu de la spécification OpenAPI, ce qui le suggère sans le prouver.
- **Jira** — une écriture strictement sans changement émet-elle quand même un webhook et une entrée de
  changelog ? Décide si un rejeu est « sans dégât » du point de vue des humains qui regardent le ticket.
- **Les quatre** — reposer une étiquette déjà présente réussit-il silencieusement ? Plausible partout,
  attesté nulle part.
