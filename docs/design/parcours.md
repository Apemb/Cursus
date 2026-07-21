# Parcours utilisateur

Ce document dit **ce qu'un humain fait avec Cursus**, et dans quel ordre. Il ne dit ni comment c'est
découpé en classes — c'est `presentation.md` — ni ce qui est construit — c'est `architecture.md`.

Il se lit en **deux registres, tenus séparés à dessein** :

- **§1 — la cible**, telle qu'elle a été décrite en conception le 2026-07-21. Elle n'est pas un plan de
  développement : c'est la forme finale vers laquelle les jalons convergent, et qui permet de juger si
  une décision d'aujourd'hui ferme une porte de demain.
- **§3 — le parcours réduit du jalon 6**, qui est ce qui sera réellement construit, et qui est beaucoup
  plus petit.

Confondre les deux ferait promettre au jalon 6 des écrans qu'il ne livrera pas. Les tenir séparés permet
au contraire de vérifier une chose précise et utile : **que le petit soit un sous-ensemble du grand, et
non une autre application**.

---

## 1. La cible

### 1.1 Plusieurs projets, ouverts en même temps

Un run n'est pas une commande, c'est une **attente longue** : plusieurs dizaines de minutes, parfois des
heures, et davantage encore le jour où des automatisations les enchaîneront. Une application qui n'ouvre
qu'un projet à la fois obligerait à choisir lequel surveiller — ce qui est exactement le service qu'elle
doit rendre.

Cursus ouvre donc **N projets simultanément**, listés dans une colonne à gauche, à la manière des espaces
de travail de Slack : ils se rechargent tous au démarrage, on en ajoute, on en retire. Retirer un projet
de la liste ne touche jamais le dépôt qu'il désigne.

Conséquence directe : **plusieurs runs tournent en parallèle dans la même application**, appartenant à des
projets différents — et, sur un même projet, potentiellement à plusieurs tâches.

### 1.2 Deux modes sur un projet, et le run est la porte d'entrée

Un projet ouvert se regarde de deux façons :

| Mode | Ce qu'on y fait |
|---|---|
| **Run** | Voir ce qui tourne, ce qui a tourné, et lancer ce qui est disponible |
| **Configuration** | Concevoir les workflows, leurs déclencheurs, les réglages du projet |

**Le run est le mode par défaut** — c'est lui qu'on veut voir en arrivant, c'est là qu'on juge le travail
à dispatcher. La configuration se visite, elle ne s'habite pas. Cette hiérarchie a une conséquence de
planification qui n'est pas anodine : **l'éditeur de graphe sort du chemin critique**, puisqu'il n'est pas
sur la porte d'entrée.

### 1.3 Le mode run se regarde par deux bouts

**La vue tâches.** Où en sont les tâches : leur statut, leurs étiquettes, et sur chacune ce qui est
*exécutable* ou déjà *en cours*. C'est la vue de décision — celle qui répond à « qu'est-ce que je lance
maintenant ». C'est aussi la projection pure décrite au §7.10.2 de `architecture.md` : la source est le
tracker, Cursus ne la duplique pas.

**La vue workflows.** La liste de tous les workflows qui tournent ou ont tourné sur ce projet. C'est la
vue de suivi — celle qui répond à « où en est ce que j'ai lancé ».

Les deux mènent au même endroit.

### 1.4 La vue d'un run

Depuis une tâche ou depuis la liste, on ouvre un run. La forme visée est celle d'un pipeline de CI façon
GitLab :

- **en haut**, les étapes et leur graphe, chacune portant son statut ;
- **en bas**, le détail de l'étape sélectionnée : **son log en direct**, et ce que son type d'étape a de
  particulier à montrer.

Cette forme a une propriété qui la rend robuste : l'écran d'un run **en cours** et celui d'un run
**terminé** sont le même écran. Seule la source change — un flux d'événements dans un cas, la relecture du
journal dans l'autre. Un seul objet de projection, deux alimentations.

### 1.5 D'où viennent les tâches

Les tâches viennent **d'ailleurs**. Cursus les observe, il n'en est pas propriétaire : leur état de vérité
est chez le tracker, et Cursus n'attache à leur identité externe que ce qui est à lui — les runs qui ont
tourné dessus.

L'ordre d'arrivée est tranché :

1. **Linear** — la cible immédiate, les projets personnels y sont.
2. **Jira** — indispensable à terme, c'est le tracker du travail salarié.
3. **Un flux de tâches local**, plus tard et au besoin, pour démarrer un projet sans dépendre d'un tracker.

Mais **les deux premiers s'étudient avant que le port ne se dessine**. Une abstraction née d'une seule
implémentation en épouse la forme, et Jira la ferait alors éclater. La recherche préalable supprime ce
risque à sa source ; elle porte sur quatre points, ceux où les deux modèles peuvent diverger assez pour
casser une abstraction : la maille commune d'un « état », la modélisation des étiquettes, le coût d'un
déplacement idempotent, et ce que l'authentification impose de stocker.

Ce qui rend le provider local secondaire — et non un raccourci vers une deuxième implémentation — c'est
qu'il devrait fournir **sa propre interface de création et d'édition**, que Linear et Jira apportent avec
eux. À noter tout de même : **créer et éditer ne sont pas dans le port**. Cursus lit et annote, il ne
rédige pas. Le provider local restera donc bon marché le jour où il arrivera, et il servira surtout de
fixture de test, exerçable sans réseau ni jeton.

### 1.6 L'application a un état, et il pilote les projets

Deux besoins qui ne relèvent d'aucun projet en particulier :

- **Le forfait d'API** — au-delà d'un certain pourcentage consommé, on ne veut plus lancer d'étape qui
  appelle un agent. Sans couper ce qui tourne : on **décale** le démarrage de la suivante, et on réessaie
  toutes les quelques minutes.
- **L'interrupteur de fin de journée** — « on finit les étapes en cours et on n'en reprend pas d'autres ».

Ils ont la même forme, et c'est la même que la disponibilité d'un workflow sur une tâche : *une étape
demande l'autorisation de démarrer, et le refus doit s'expliquer*. Une étape peut donc être **en attente**,
ce qui n'est ni un échec ni une exécution — un troisième état, dont les conséquences sur le noyau sont
traitées au §7.13.1 de `architecture.md`.

Conséquence de navigation : **l'application a un état propre**, et il doit être visible quelque part —
au minimum le pourcentage de forfait et la position de l'interrupteur.

### 1.7 Ce qui est explicitement un raffinement

**Une vue « maison », agrégée sur tous les projets** : ce qui tourne partout, plus les informations
globales du §1.6.

Elle n'est **pas** un prérequis : elle serait la consolidation d'écrans qui existent déjà, et elle
renverrait de toute façon vers une vue projet. Elle ne se construit pas en premier, mais **rien ne doit
lui fermer la porte**. Ce qui se traduit par une seule contrainte, et elle est gratuite :

> La racine multi-projets doit pouvoir **énumérer les runs actifs de tous les projets ouverts**.

Elle est de toute façon nécessaire pour en ouvrir plusieurs ; il suffit de ne pas lui interdire cette
lecture.

---

## 2. Ce que la cible fait apparaître, et son statut

Trois objets que le noyau ne connaît pas, et une décision qu'elle renverse.

| Ce qu'elle exige | Statut aujourd'hui |
|---|---|
| **Une racine au-dessus de `ProjectHost`** — qui charge la liste des projets, en ouvre et en ferme, et agrège leurs runs | N'existe pas. C'est le **registre machine** du §7.10.1, que le jalon 5 avait repoussé au jalon 7 et que la cible ramène au premier plan. La règle de sens unique tient : cette racine construit les `ProjectHost`, aucun `ProjectHost` ne la connaît |
| **La tâche** — un objet portant statut, étiquettes, et sur lequel des workflows *deviennent disponibles* | N'existe pas. Le noyau connaît `WorkflowDefinition`, `StepDefinition`, `WorkflowRun`, `StepRun`, et rien d'autre. C'est le plus gros morceau de la cible (§7.10.2 à §7.10.6) |
| **La sortie en direct** | Impossible en l'état : `ProcessRunner` n'obtient la sortie qu'à la mort du process (§4.4), et le journal n'émet qu'aux frontières d'étape (§4.10). C'est le trou §9.2-4 |
| **Des runs concurrents** | Contredit par le code : `SqliteRunJournal` détient une `SqliteConnection` unique et son `Append` n'a aucun verrou. Deux runs simultanés sur un même projet ne lèveraient pas — ils corrompraient par intermittence (§9.2-14) |

**Un dernier point, qui n'est pas un objet mais une forme.** « Ce workflow est exécutable sur cette tâche »,
« le forfait est consommé à 80 % » et « l'interrupteur de fin de journée est baissé » sont trois instances
d'une seule chose : **une autorisation demandée avant le démarrage d'une étape, dont le refus s'explique**.
Les construire séparément produirait trois systèmes de conditions qui ne composent pas — et trois façons
différentes d'expliquer à l'écran pourquoi rien ne bouge. La disponibilité (§7.10.6, dont la forme n'est pas
conçue) devra donc l'être en tenant compte des deux autres cas d'emploi, même s'ils n'arrivent que bien plus
tard. Le mécanisme et ses pièges sont au §7.13.1 de `architecture.md`.

---

## 3. Le parcours du jalon 6

Réduit à ce qu'un humain doit pouvoir faire pour que la boucle se ferme : *lancer un workflow depuis
Cursus et voir ce qu'il fait*. Aucune tâche, aucun tracker, aucun écran agrégé.

**J'ouvre Cursus.** Mes projets se rechargent — pour l'instant, il n'y en a qu'un ou deux, et la colonne de
gauche les liste. Un bouton pour en ajouter : un sélecteur de dossier, qui refuse ce qui ne porte pas de
`.cursus/`.

**Je choisis un projet.** Il s'ouvre sur son mode run, qui montre ses workflows — `build`, `verifier` — et
pour chacun la trace de son dernier passage : quand, et comment il s'est terminé. Le journal SQLite le sait
déjà, il n'y a rien à construire côté noyau pour l'afficher.

**Je lance `verifier`.** L'écran de run s'ouvre : ses deux étapes, leur nom métier — « Compiler sans le
moindre avertissement », « Passer toute la suite au vert » — et leur statut.

**J'attends.** `compiler` passe en cours. **Sa sortie défile.** C'est le point qui fait de cet écran autre
chose qu'un sablier : sur trois minutes de compilation puis de tests, savoir *où en est la suite* est toute
la valeur. Un bouton pour arrêter, qui doit tuer l'arbre de process et non le seul `/bin/sh`.

**Ça échoue.** `tester` finit rouge. L'écran affiche **« échoué »** — et pas l'état brut du run, qui dira
`Completed` parce que la traversée s'est arrêtée là où il n'y avait plus d'arête. Ce sont deux vérités
différentes : le noyau a raison sur la traversée, l'écran a raison sur le résultat, et l'arbitrage
appartient à la présentation. Je lis la sortie sur place, je sélectionne la ligne d'erreur, je la copie.

**Je reviens demain.** Le projet se rouvre, `verifier` porte « échoué hier à 18 h 04 », et ouvrir ce run
passé rend exactement le même écran — mêmes étapes, mêmes sorties. C'est le retour sur investissement du
jalon 4, et la preuve que l'écran de run n'a bien qu'une seule forme.

---

## 4. Ce que ce parcours tranche

1. **La liste chronologique du run ne partage pas la surface des terminaux.** Question laissée ouverte par
   `presentation.md` §8 et le §9.3 de `architecture.md`. Réponse : non. Les workflows et les sessions sont
   deux modes, et il n'y a jamais deux surfaces en compétition pour le même panneau. **Corollaire : pas de
   routeur** — la coquille montre l'un ou l'autre.
2. **Le run est la porte d'entrée d'un projet**, la configuration est un mode secondaire. L'éditeur de
   graphe cesse d'être sur le chemin critique.
3. **L'écran d'un run en cours et celui d'un run passé sont le même écran.** Une projection, deux
   alimentations.
4. **L'écran arbitre le résultat d'un run**, il ne recopie pas `RunState`.
5. **La sortie en direct n'est pas un raffinement** — c'est ce qui distingue l'écran de run d'un sablier
   décoré, et donc ce qui décide s'il vaut la peine d'être construit.

## 5. Manques assumés du jalon 6

- **Aucune tâche, aucun tracker.** On lance à la main, depuis la liste des workflows.
- **Aucun écran agrégé.** Seulement la contrainte du §1.6, qui ne coûte rien.
- **Le graphe reste une liste.** Le layout de graphe est un algorithme, pas un contrôle ; il a son propre
  jalon, partagé avec l'éditeur dont il est le vrai coût (§9.4). La forme « étapes en haut, détail en bas »
  se tient parfaitement avec une liste ordonnée.
- **La configuration se fait à la main**, en éditant les fichiers de `.cursus/workflows/`.

## 6. Questions ouvertes

- **Le volume de sortie.** `dotnet test` produit quelques milliers de lignes. Les afficher est un problème ;
  les journaliser ligne par ligne dans SQLite en est un autre, et les deux n'appellent pas la même réponse.
  À trancher avec la sortie en flux, pas après.
- **La mort de l'arbre de process à l'annulation.** `/bin/sh -c "dotnet test"` engendre un `dotnet` fils, et
  `Process.Kill()` sans `entireProcessTree` le laisserait orphelin. ⚠️ À vérifier au câblage, non vérifié à
  ce jour.
- **Retirer un projet de la liste alors qu'un run y tourne** — refuser, ou annuler puis retirer ?
- **La granularité de la vue tâches** (cible) : une tâche montre-t-elle ses runs, ou seulement le dernier ?
  Sans réponse tant que la tâche n'existe pas.
- **La maille de l'interrupteur de fin de journée** (cible) : suspendre au niveau de l'**étape** laisse des
  runs figés en milieu de parcours toute la nuit — branche créée, carte affichant « en cours de dev », ce
  qui est peut-être exactement ce qu'on veut. Suspendre au niveau du **run** laisse un état plus propre au
  prix d'une fin de journée plus longue. Argumentée au §7.13.1 de `architecture.md`, non tranchée.
