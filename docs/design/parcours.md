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

### 1.2 Deux modes qu'on habite, une configuration qu'on visite

Un projet ouvert se regarde de trois façons — mais elles n'ont pas le même poids, et la maquette du
2026-07-21 a montré que les présenter à égalité contredisait la hiérarchie décidée ici même :

| Destination | Ce qu'on y fait | Statut |
|---|---|---|
| **Run** | Voir ce qui tourne, ce qui a tourné, et lancer ce qui est disponible | mode par défaut |
| **Sessions** | Les terminaux du projet — ce que le dépôt sait déjà faire (§6 d'`architecture.md`) | second mode |
| **Configuration** | Les workflows, leur validation, les réglages du projet | **destination secondaire** |

**Le run est le mode par défaut** — c'est lui qu'on veut voir en arrivant, c'est là qu'on juge le travail
à dispatcher. Cette hiérarchie a une conséquence de planification qui n'est pas anodine : **l'éditeur de
graphe sort du chemin critique**, puisqu'il n'est pas sur la porte d'entrée.

**Run et Sessions s'habitent** — on y reste, on y revient, ils méritent un sélecteur à deux positions.
**La configuration se visite, elle ne s'habite pas** : elle relève d'un bouton d'engrenage **placé à côté
du nom du projet**, pas d'une troisième position de même poids. Le placement n'est pas cosmétique : il dit
de quoi il règle la configuration — *ce* projet, à distinguer de l'écran Application (§1.7), qui règle la
machine.

*Écarté, et l'écart vaut d'être noté* : un sélecteur à trois positions `Run · Sessions · Configuration`.
Il donnait trois destinations de même poids, c'est-à-dire l'inverse de ce que le paragraphe précédent
énonce. Une règle écrite dans un document et démentie par l'écran ne survit pas à sa première relecture.

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

**La surface du haut a deux vues, sœurs et permutables.** La maquette d'un workflow à six étapes avec une
boucle a montré que « les étapes et leur graphe » désignait en fait **deux objets distincts**, que le cas
à deux étapes sans boucle confondait :

| L'objet | Ce qu'il est | Où il vit |
|---|---|---|
| **La définition** | Un graphe. Statique, identique d'un run à l'autre, y compris ce qui n'a pas été parcouru | `.cursus/workflows/*.json` |
| **La trajectoire** | Une **liste ordonnée de visites**. Six étapes avec une boucle en produisent huit | `WorkflowRun.History` — et `StepRun` porte déjà `Iteration` |

Aucune ne subsume l'autre. Le graphe seul répond à *où en est-on dans le workflow* et **montre ce qui n'a
pas encore été parcouru** — ce qu'une liste ne peut structurellement pas faire. La liste seule répond à
*ce qui s'est passé, dans l'ordre*, que le graphe perd en compressant les visites répétées en un nœud. On
ne choisit donc pas : **un basculeur, deux rendus**.

Deux propriétés en découlent, et ce sont elles qui rendent la décision peu coûteuse :

- **la sélection est partagée** — sélectionner `test` dans le graphe puis basculer en liste surligne
  `test · tour 2`. Une seule notion de sélection, deux façons de la désigner : un seul état à tester, et
  deux rendus qui n'ont pas besoin de se connaître ;
- **le panneau du bas reste le détail de la sélection**, dans les deux vues. C'est ce qui interdit d'y
  loger la trajectoire — elle décrit le run entier, pas l'étape sélectionnée.

**Le contrôle d'un run n'est pas un bouton mais un état à trois positions** : *En cours · Arrêt en cours ·
Arrêté*. « Arrêter » est un verbe instantané, or arrêter proprement veut dire laisser l'étape courante se
terminer et n'en démarrer aucune autre — il existe donc un moment où l'arrêt est **demandé mais pas
obtenu**, parfois plusieurs minutes. La position du milieu ne se choisit pas, on y passe ; revenir en
arrière annule la demande. Et **« Arrêté » n'est pas « Échoué »** : le noyau les distingue déjà, en
`Aborted / Canceled`.

Ce contrôle **est l'interrupteur de fin de journée du §1.6, à une autre portée** — sémantique identique,
l'un appliqué à un run, l'autre à l'application. Ils doivent donc se composer, et l'écran doit dire
*lequel des deux* retient un run qui ne repart pas.

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

- **Les forfaits d'API** — au-delà d'un certain pourcentage consommé, on ne veut plus lancer d'étape qui
  appelle un agent. Sans couper ce qui tourne : on **décale** le démarrage de la suivante, et on réessaie
  toutes les quelques minutes.
- **L'interrupteur de fin de journée** — « on finit les étapes en cours et on n'en reprend pas d'autres ».

⚠️ **Le pluriel n'est pas une coquetterie, et il corrige ce document.** Une première rédaction parlait du
forfait *au singulier*. C'est faux dès qu'on cumule Claude Code et Codex — l'usage courant — et c'est déjà
faux avec Claude Code seul, qui expose plusieurs quotas distincts selon le modèle. Trois conséquences :

| Ce qui était supposé | Ce qu'il faut à la place |
|---|---|
| Un quota par fournisseur | Un quota par **couple (fournisseur, modèle)**. Une étape sur Opus peut être bloquée pendant qu'une étape sur Fable passe — dans le *même* run, à deux étapes d'écart |
| Une liste de fournisseurs connue à l'avance | Une clé **ouverte, découverte à l'exécution**. Le quota Sonnet est devenu un quota Fable : un `enum` de modèles serait périmé avant d'être commité. C'est une chaîne rendue par le fournisseur, pas un type du domaine |
| L'application choisit l'agent selon ce qu'il reste | **L'étape le nomme**, comme elle nomme sa commande — tous les modèles ne se valent ni en coût ni en usage. Le choix est de la configuration, pas de l'ordonnancement, et le run **reste reproductible**, ce qu'un choix dynamique lui retirerait |

La conséquence qui remonte au noyau est au §7.13.1 d'`architecture.md` : l'autorisation de démarrer doit
**prendre la ressource en paramètre** plutôt qu'interroger un état global. Un booléen « peut-on lancer ? »
serait faux dès le second modèle installé.

Ils ont la même forme, et c'est la même que la disponibilité d'un workflow sur une tâche : *une étape
demande l'autorisation de démarrer, et le refus doit s'expliquer*. Une étape peut donc être **en attente**,
ce qui n'est ni un échec ni une exécution — un troisième état, dont les conséquences sur le noyau sont
traitées au §7.13.1 de `architecture.md`.

Conséquence de navigation : **l'application a un état propre**, et il doit être visible quelque part —
au minimum le pourcentage de forfait et la position de l'interrupteur.

### 1.7 L'écran Application — le résumé et sa contrepartie

La colonne des projets est un **rail d'icônes**, et son pied porte l'état du §1.6 : les jauges et
l'interrupteur. Le placer là est le seul choix qui le situe **hors de la hiérarchie des projets** au lieu
de le poser à côté — dans une coquille dont tout le reste est *par projet*, c'est ce qui dit sa portée
sans avoir à l'écrire.

*Alternatives écartées à la maquette* : une **barre supérieure globale**, qui occupe toute la largeur pour
deux informations qui changent rarement, et surtout ouvre la voie aux **runs en onglets** — ce qui
rouvrirait la question du routeur que le §4 ferme, et mettrait une session terminal dans un conteneur
recyclable, le piège exact du §5 de `presentation.md`. Et un **pied de colonne nommée**, qui laisse l'état
global visuellement *dans* la liste des projets.

Mais un pied de rail résume, il n'explique pas — or il y a à expliquer : les agents installés, leurs
seuils, les trackers connectés, ce qui a été effectivement résolu de chaque connexion. **Le rail devient
donc un résumé cliquable, et ce qu'il résume vit sur un écran à lui**, sans projet sélectionné :

| Onglet | Contenu | Quand |
|---|---|---|
| **Aperçu** | Ce qui tourne sur tous les projets, les jauges, l'interrupteur — et *pourquoi* une étape attend | avec les écrans qui l'alimentent |
| **Agents** | Les fournisseurs et modèles installés, leur quota, leur seuil | jalon 7 |
| **Sources** | Les trackers connectés — et ce qui a été résolu, pas seulement « connecté » (§7.13.2 d'`architecture.md`) | jalon 7 |
| **Réglages** | Ce qui relève de la machine | selon besoin |

**Ce déplacement rend gratuit ce qui ne l'était pas** : la vue agrégée du §1.8 cesse d'être un raffinement
lointain pour devenir le premier onglet d'un écran qu'il faut construire de toute façon.

⚠️ **Deux prérequis que cet écran ramène, et qu'il ne peut pas contourner.** Les jetons et les réglages
d'agents ne peuvent pas vivre dans `project.json`, qui est **versionné** — un jeton Linear y serait partagé
avec toute l'équipe. Ils relèvent du **registre machine et du trousseau** (§7.10.1 d'`architecture.md`),
que le jalon 5 avait repoussés faute de consommateur : cet écran est ce consommateur. Et ⚠️ **non vérifié
à ce jour** : que Claude Code ou Codex exposent leur consommation de façon lisible par un tiers. Si aucun
ne le fait, la jauge est un compteur tenu par Cursus — donc une estimation, qu'il faudra présenter comme
telle plutôt qu'avec l'autorité d'une mesure.

### 1.8 Ce qui est explicitement un raffinement

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
pour chacun la trace de son dernier passage : quand, et comment il s'est terminé. ⚠️ *Rédaction initiale
corrigée au jalon 6c·3a* : contrairement à ce qui était écrit ici (« le journal le sait déjà, rien à
construire côté noyau »), le journal **ne rattachait aucun run à un workflow** — `WorkflowDefinition` est un
graphe anonyme — et n'exposait pas l'instant de fin. Il a fallu ajouter `WorkflowId` à la provenance du run
et enrichir `RunSummary` (§4.16 d'`architecture.md`). Le reste, en revanche, était bien là : le moteur pose
déjà l'issue, voir ci-dessous.

**Je lance `verifier`.** L'écran de run s'ouvre : ses deux étapes, leur nom métier — « Compiler sans le
moindre avertissement », « Passer toute la suite au vert » — et leur statut.

**J'attends.** `compiler` passe en cours. **Sa sortie défile.** C'est le point qui fait de cet écran autre
chose qu'un sablier : sur trois minutes de compilation puis de tests, savoir *où en est la suite* est toute
la valeur. Un bouton pour arrêter, qui doit tuer l'arbre de process et non le seul `/bin/sh`.

**Ça échoue.** `tester` finit rouge. L'écran affiche **« échoué »**. ⚠️ *Corrigé au 6c·3a* : ce document
supposait que l'état brut dirait `Completed` (« la traversée s'est arrêtée là où il n'y avait plus
d'arête ») et que la présentation devrait le rectifier. C'est faux — le moteur pose déjà `RunState.Failed`
quand l'étape terminale échoue sans arête de secours (`result.IsSuccess ? Completed : Failed`). L'arbitrage
appartient bien à la présentation, mais il est réduit à une **table de libellés** `(RunState, AbortReason)
→ mot` : le noyau a déjà tranché réussi/échoué, l'écran ne fait que le nommer — et « Arrêté » n'est pas
« Échoué », le noyau les sépare aussi. Je lis la sortie sur place, je sélectionne la ligne d'erreur, je la copie.

**Je reviens demain.** Le projet se rouvre, `verifier` porte « échoué hier à 18 h 04 », et ouvrir ce run
passé rend exactement le même écran — mêmes étapes, mêmes sorties. C'est le retour sur investissement du
jalon 4, et la preuve que l'écran de run n'a bien qu'une seule forme.

**Je corrige un workflow.** J'ouvre la configuration par l'engrenage, j'édite le JSON dans *mon* éditeur,
et Cursus me montre **les problèmes de validation à leur emplacement exact** — « l'étape `review` renvoie
vers `dev2`, qui n'existe pas ». Ce n'est pas un éditeur, et ça change quand même l'expérience :
aujourd'hui une faute de frappe dans une arête ne se découvre **qu'au lancement d'un run**. Le coût est
proche de zéro, et c'est ce qui le rend légitime ici — `WorkflowCatalog.Load` rend déjà ce rapport, et
**personne ne l'affiche**.

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

**Ce que la passe de maquettes du 2026-07-21 a tranché en plus.** Les maquettes sont archivées dans
`docs/design/maquettes/jalon-6.html` — **ouvrables dans un navigateur, sans aucune autorité**. Elles ont
servi à décider, elles ne spécifient rien : elles sont en HTML quand Cursus est en XAML, et aucun pipeline
ne traduit l'un vers l'autre. Elles ne seront pas tenues à jour ; ce document l'est. Tout écart entre les
deux se tranche en faveur de ce document.

6. **La coquille est un rail d'icônes**, l'état de l'application en pied de rail, cliquable vers l'écran
   Application (§1.7). Onglets et barre supérieure globale écartés, avec leurs raisons.
7. **La configuration est un engrenage**, pas un troisième mode (§1.2).
8. **Graphe et liste sont deux vues sœurs**, permutables, à sélection partagée (§1.4).
9. **Le contrôle d'un run est un état à trois positions**, pas un bouton — et c'est l'interrupteur de fin
   de journée à une autre portée (§1.4).
10. **Les quotas se comptent par (fournisseur, modèle)**, et l'étape nomme le sien (§1.6).

## 5. Manques assumés du jalon 6

- **Aucune tâche, aucun tracker.** On lance à la main, depuis la liste des workflows.
- **Aucun écran agrégé.** Seulement la contrainte du §1.8, qui ne coûte rien.
- **Une seule des deux vues d'un run : la liste.** ⚠️ Ce n'est plus le pis-aller que ce document décrivait
  (« le graphe reste une liste, faute de l'algorithme de placement »). La liste **est** la bonne
  représentation de la trajectoire, et la maquette a fait tomber deux fois l'argument de coût :
  en lecture seule, le placement est un tri topologique par niveaux — de la logique pure, testable en
  `[Fact]`, sans une ligne d'Avalonia. La réserve qui subsiste est réelle mais bornée : **ce tri suppose un
  graphe acyclique**, et une boucle n'en est pas un — il faut détecter l'arête de retour et l'exclure avant
  de répartir en colonnes. Estimation honnête : **de l'ordre de 150 lignes**, plus le rendu.
  Le graphe reste donc hors de 6c, mais il en devient le **premier candidat juste après** — voir le §7.
- **La configuration ne s'édite pas dans Cursus** : on lit, on valide, on édite ailleurs (§7, palier 1).

## 6. Questions ouvertes

- **Le volume de sortie** — ⚠️ **à moitié refermée**. La partie persistance est réglée, et l'était déjà :
  les sorties **ne vont pas en base**, `RunArtifactStore` les range en fichiers depuis le jalon 4, une
  visite par fichier. Ce que la conception du 2026-07-21 y ajoute est l'argument qui rend ce choix
  structurant plutôt que pratique : **N étapes parallèles = N fichiers, aucune contention**, là où SQLite
  n'a qu'une connexion non synchronisée. Le fichier n'est pas une commodité d'affichage — **c'est ce qui
  rend 6b possible**. Reste ouvert : afficher un fichier de plusieurs milliers de lignes, qui est un
  problème de contrôle, pas de persistance.
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
- **L'état d'un nœud visité plusieurs fois** (avec la vue graphe) : une étape passée deux fois n'a pas *un*
  état — elle peut avoir échoué puis réussi. Montrer le dernier ? Le pire ? Les deux ? La maquette s'en
  tirait par « pastille du dernier passage + compteur d'échecs », ce qui est une convention défendable mais
  à choisir consciemment. Le corollaire est plus gênant : **le clic devient ambigu** — le log de quel tour ?
- **Le modèle nommé par une étape est épuisé** : attendre, ou se rabattre sur un autre ? Attendre est
  cohérent avec le troisième état du §7.13.1 ; se rabattre demande une politique et rend deux exécutions du
  même workflow non comparables. À poser au jalon 7, avec l'`AgentStep`.
- **Ce qu'on montre d'une commande** : les étapes exécutent `/bin/sh -c "dotnet build -warnaserror"`, la
  maquette affichait `dotnet build -warnaserror`. Masquer le shell est défendable, mais c'est une
  troncature choisie, et la règle devra tenir le jour où un workflow lancera un binaire directement.

## 7. L'édition des workflows, en trois paliers

Question posée le 2026-07-21 : « comment envisages-tu la phase de design des workflows, on fait ça dans un
deuxième temps ? ». Oui — mais « deuxième temps » recouvre trois choses dont les coûts n'ont aucun rapport,
et les confondre ferait repousser au jalon 8 quelque chose qui ne coûte presque rien.

| Palier | Ce qu'on peut faire | Ce que ça coûte | Quand |
|---|---|---|---|
| **1. Lire et valider** | Voir les workflows et **les problèmes de validation à leur emplacement exact**. Éditer dans son propre éditeur, recharger | **Quasi nul** — `ValidationReport` existe, est testé, et n'est affiché nulle part. Un surveillant de fichier, et c'est tout | **jalon 6** |
| **2. Éditer par formulaire** | Ajouter une étape, changer sa commande, son `maxVisits`, ses arêtes — chacune un couple (garde, cible) choisi dans une liste. **Aucun canevas** | **Modeste** — des formulaires sur un modèle déjà validé. Couvre la très grande majorité de l'édition réelle sans une ligne de dessin | jalon 8a |
| **3. Le canevas** | Déplacer les nœuds, tirer les arêtes à la souris | **Élevé** — c'est là qu'est tout le coût du jalon 8 : le placement interactif, les arêtes qui se recalculent, la persistance des positions | jalon 8b |

**Un lien à ne pas manquer avec la vue graphe** (§5) : *le placement écrit pour la vue graphe en lecture
est réutilisé par le canevas*. Répartir les nœuds en colonnes est le même calcul, qu'on puisse ensuite les
déplacer ou non. La vue graphe ne sert donc pas que la lecture — **elle avance le jalon 8 d'autant, et
fait tomber le risque de cet algorithme bien avant qu'il soit sur le chemin critique**. C'est ce qui en
fait le premier candidat après 6c, devant les paliers d'édition.
