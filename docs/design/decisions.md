# Journal de décisions (ADR)

> **À quoi sert ce fichier, et pourquoi il existe à part.** `architecture.md` décrit
> l'état **présent** et le pourquoi du **découpage actuel** ; par construction il est
> réécrit quand la structure bouge, et **ne garde pas** les décisions périmées (un
> plan abandonné n'y survit qu'en note « périmé »). git porte le *quoi* et le *quand*,
> mais un commit n'est ni navigable comme un récit, ni annotable, ni reliable à un
> autre. Ce journal comble le trou : le **récit des décisions dans le temps**.
>
> **Règle d'or : append-only.** On n'y **réécrit jamais** une entrée. Une décision
> dépassée n'est pas effacée — elle reçoit le statut *Superséedée par [N]* et une
> nouvelle entrée explique le revirement. C'est ce qui le rend **basse maintenance** :
> à l'inverse d'un document d'état, il ne peut pas « prendre du retard ».
>
> **Ce qu'on y consigne** : les pivots, les décisions structurantes avec leurs
> **alternatives écartées** (l'écart vaut autant que le choix), et les revirements.
> **Pas** le détail d'implémentation (il vit dans `architecture.md` §X, cité en fin
> d'entrée) ni la chronologie fine (git).
>
> **Format d'une entrée** : `## D-NNN — Titre` · *Statut* · *Contexte* (le problème) ·
> *Décision* · *Alternatives écartées* (et pourquoi) · *Conséquences* · *Renvoi*.

---

## D-001 — Modèle orienté agents d'abord

**Statut** : Reporté — superséedé **comme point de départ** par [D-002] (le pivot). Non
mort : l'`AgentStep` en ressuscitera des pans.

**Contexte.** La première trajectoire partait du plus visible (une app Avalonia de
sessions terminal réelles), puis une phase de recherche a produit un modèle métier
complet orienté agents.

**Décision (initiale).** Construire autour de `Task > Workspace > Session`, quatre
machines à états, HITL de première classe, capture de scrollback, confinement OS.
Consigné dans `docs/design/modele-metier.md`.

**Pourquoi on l'a quitté.** Voir [D-002]. En un mot : câbler l'IA au cœur du moteur
condamnait à réécrire ce moteur, et le modèle agent n'est pas testable sans PTY ni
heuristiques.

**Conséquences.** `modele-metier.md` reste un document **cible**, pas l'état courant.
Les briques (HITL, worktree, détection d'état) sont **reportées, non écartées**.

**Renvoi** : `architecture.md` §3 · `docs/research/agentic-workflows-landscape.md`.

---

## D-002 — Le pivot : un noyau déterministe d'abord

**Statut** : Accepté (2026-07-20). Supersède [D-001] comme point de départ.

**Contexte.** Comment construire un manageur de workflow agentique sans se condamner
à réécrire le cœur d'orchestration quand l'IA arrivera ?

**Décision.** Construire **d'abord** un moteur qui parcourt un graphe d'étapes-scripts
et route chaque étape sur son code de sortie, **sans jamais savoir ce qu'est un
agent**. L'`AgentStep` se greffera plus tard comme un `StepKind` de plus, sans
réécrire la traversée. Acté par deux commits au même horodatage (`2516e39` doc,
`f4be0fa` code).

**Alternatives écartées.**
- *Agent-first* ([D-001]) — Netflix/Orkes Conductor a greffé l'IA **sans** réécrire
  son moteur (agents = nouveaux *task types*) ; câbler l'IA en dur, c'est se condamner
  à la réécriture.
- *Tout construire d'un bloc* — le déterministe est **intégralement testable** (contrat
  fermé `(cmd,args,env,cwd) → (exit,stdout,stderr,durée)`, aucun PTY) ; le mêler à
  l'agent perdrait cette propriété.

**Conséquences.** Pas de PTY (un `Process` à sorties redirigées) ; aucune question de
persistance de flux (« le flux **est** l'artefact ») ; **une** machine à états au lieu
de quatre. ⚠️ Limite assumée : une boucle purement déterministe ne fait que
**retry/poll/until** ; la boucle auto-réparatrice `Verify → Dev` **exige l'agent**.

**Renvoi** : `architecture.md` §3 · `docs/design/noyau-deterministe.md`.

---

## D-003 — Trajectoire révisée : dogfooding en tête, `Cursus.Cli` écartée

**Statut** : Accepté (2026-07-20). Supersède le plan à 5 jalons.

**Contexte.** Le pivot [D-002] s'accompagnait d'un plan linéaire à 5 jalons
(moteur → `ProcessRunner` → loader → journal → **UI en dernier**).

**Décision.** Réordonner en 0-4-5-6-7 : le **packaging `.app` macOS remonte en tête**
(jalon 0), parce que **l'utilisateur veut dogfooder Cursus sur Cursus** — et un bundle
installable expose des risques d'environnement qu'aucun `dotnet run` ne montre.

**Alternatives écartées.**
- *Garder l'UI en dernier* — repoussait la découverte des risques d'environnement à la
  toute fin.
- *`Cursus.Cli` comme point d'entrée précoce* (proposée) — aurait permis de dogfooder
  dès le jalon 5 sans UI, mais c'est **un second point d'entrée à maintenir** pour
  « pas assez d'urgence ». Le dogfooding attend donc l'UI.

**Conséquences.** Le jalon 0 n'apporte **aucune fonction** : il sert à *observer*
quatre risques anticipés (natives RoyalTerminal, `PATH` GUI tronqué, `SSH_AUTH_SOCK`,
cwd). Deux se sont révélés infirmés, deux réels (dont le `PATH` tronqué, → trou
§9.2-15).

**Renvoi** : `architecture.md` §9.4, §6.6.

---

## D-004 — Format de fichier JSON, pas YAML

**Statut** : Accepté (2026-07-20).

**Contexte.** Choisir le format des fichiers de workflow (`.cursus/workflows/*.json`).

**Décision.** **JSON** via `System.Text.Json` (zéro dépendance).

**Alternatives écartées.**
- *YAML / YamlDotNet* — l'argument décisif n'est **pas** la lisibilité mais le
  **round-trip** : l'éditeur graphique réécrira le fichier, et un **YAML réécrit par
  une machine perd commentaires et mise en forme à chaque sauvegarde**. JSON n'a pas
  ce défaut, et reste tapable à la main.

**Conséquences.** Le document est optimisé pour la machine, pas pour l'humain — assumé,
puisque l'éditeur viendra. Gardes en chaînes préfixées (`"exit:2"`), extensibles sans
changer la forme.

**Renvoi** : `architecture.md` §4.2, §7.4.

---

## D-005 — Isolation d'un run : worktree git, jamais copie ni container-par-agent

**Statut** : Accepté (recherche 2026-07-19, construit au jalon 6b 2026-07-22).

**Contexte.** Faire coexister plusieurs runs sur un même projet sans qu'ils s'écrasent.
La collision n'est ni dans les logs (par `runId`) ni dans la base (sérialisée), mais
dans **ce que les scripts écrivent** — le code, l'état git — dont Cursus ne choisit pas
les noms.

**Décision.** Isoler **tout** run dans son propre **worktree git** (`NewWork(base)` en
HEAD détaché, `Review(ref)` en checkout), lancé **via `IProcessRunner`**.

**Alternatives écartées.**
- *Container-par-agent* — Sculptor l'a essayé puis **abandonné** (gêne l'inspection
  croisée, Docker Desktop macOS = perf + creds keychain inaccessibles). Container fort
  = containeriser l'app entière en opt-in global, jamais par-agent.
- *Copier le workspace* — jetterait le **contexte git** dont l'agent a besoin pour
  committer.

**Conséquences.** `git` devient une dépendance externe du noyau, mais lancée via
`IProcessRunner` (l'invariant « aucun `Process.Start` hors de `ProcessRunner` » tient),
et son absence est signalée (`GitNotAvailableException`). **L'identité du run devient
une entrée du moteur** (`runId`), car l'appelant monte le worktree *avant* le run — le
moteur reste innocent du provisionnement (invariant 8).

**Renvoi** : `architecture.md` §4.13, §4.8 (inv. 8-10).

---

## D-006 — Découplage UI : VIPER écarté, `ProjectHost` réifié

**Statut** : Accepté (2026-07-21). Construit côté lecture au jalon 6c·3a.

**Contexte.** Découpler la logique du framework UI pour permettre un mode headless/CLI
sans réécrire de métier. L'utilisateur a proposé **VIPER** (souvenir iOS).

**Décision.** Appliquer l'**Humble Object** : un `ObservableObject` de CommunityToolkit
est déjà du **POCO testable en xUnit nu**. Et réifier la racine de composition en
**`ProjectHost`** (nom choisi par l'utilisateur), possédant le journal qu'il construit
via une fabrique injectée, `IDisposable`. Le critère (formulé par lui : *« l'UI n'est
qu'une façon d'instancier la logique et d'afficher les données »*) est rendu
**exécutable par deux tests d'architecture** : `Cursus.Core` sans assembly Avalonia, et
un end-to-end headless.

**Alternatives écartées.**
- *VIPER* — le *Presenter* n'existe que parce qu'UIKit n'a pas de binding, le *Router*
  que parce qu'UIKit pilote une pile de contrôleurs : deux moteurs **éteints en
  Avalonia**. L'intuition (découpler) était juste ; la déclinaison iOS, inutile.
- *Façade unique* / *« l'appelant compose »* — l'utilisateur a proposé mieux : la racine
  réifiée. Son argument sur le risque du singleton était juste — `SqliteRunJournal`
  détient une connexion unique non synchronisée.

**Conséquences.** `ProjectHost` naît en `Cursus.Core` ; `Cursus.Persistence` fournit le
préréglage (seul lieu des deux mondes). Règle de sens unique : aucun module ne connaît
le host, les collaborateurs reçoivent la projection.

**Renvoi** : `architecture.md` §7.12 · `docs/design/presentation.md`.

---

## D-007 — Le jalon 6 (jonction UI) scindé en 6a / 6b / 6c

**Statut** : Accepté (2026-07-21).

**Contexte.** La jonction UI mêlait trois choses (sortie en flux, runs concurrents,
présentation) de maturité très inégale.

**Décision.** Scinder : **6a** (sortie en flux, noyau seul), **6b** (runs concurrents,
persistance seule), **6c** (jonction UI, elle-même décomposée en marches suivant le flux
utilisateur).

**Alternatives écartées.**
- *Un jalon 6 monolithique* — 6a et 6b se font **intégralement en TDD sans une ligne
  d'Avalonia** ; un jalon monolithique construirait l'écran de run **deux fois** (une
  fois autour d'un chronomètre, une fois autour d'un flux).

**Conséquences.** Réserve signalée et acceptée : ça **retarde le premier pixel de deux
jalons**. 6c décomposé en marches (loader → ouvrir en mode run → dernier passage →
lancer → sortie qui défile → run passé → config).

**Renvoi** : `architecture.md` §9.4 · `docs/design/parcours.md`.

---

## D-008 — Le tracker est la source de vérité des tâches, pas SQLite

**Statut** : Accepté sur le principe (2026-07-21). Six arbitrages ouverts pour le
jalon 7.

**Contexte.** Où vit l'état durable d'une tâche ? Recherche élargie par l'utilisateur à
**quatre trackers** (Linear, Jira, GitHub, GitLab) et au **CRUD complet**.

**Décision.** L'état durable vit dans les **systèmes de référence** — git (branche, PR)
et le **tracker** (issue properties Jira, etc.), modèle Symphony « re-dériver du tracker
+ filesystem ». Adaptateurs derrière `IIssueSource`, jamais dépendance du noyau.

**Alternatives écartées.**
- *Un blackboard/store Cursus entre workflows* — dupliquerait l'état que le tracker et
  git détiennent déjà, et forcerait une synchronisation.
- *La maille `(colonne, étiquettes)`* pour le déclenchement — **n'existe que chez
  Linear** ; ailleurs l'avancement appartient au couple (tâche, tableau). Erreur
  d'arité, pas écart d'adaptateur.

**Conséquences.** Deux contraintes remontent jusqu'au **moteur** : aucune idempotence en
création (clé de corrélation à **journaliser avant** l'appel, sinon la reprise duplique)
et aucune concurrence optimiste (écritures de collection **en delta**). Révise
`modele-metier.md` (le tracker, pas SQLite).

**Renvoi** : `architecture.md` §7.10 · `docs/research/trackers/synthese.md`.

---

## D-009 — Quota, interrupteur, disponibilité : une seule autorisation avant l'étape

**Statut** : Tranché sur le principe (2026-07-21). Construction future (avec l'`AgentStep`).

**Contexte.** Trois besoins énoncés par l'utilisateur « en passant » : le **quota d'API**
qui met une étape *en pause* plutôt que d'échouer ; un **interrupteur global de fin de
journée** (« on finit ce qui tourne, on ne reprend rien ») ; la **disponibilité** d'une
carte.

**Décision.** Les traiter comme **trois instances d'une seule chose** : une
**autorisation demandée avant le démarrage d'une étape**, prenant la ressource en
paramètre, dont le refus doit s'expliquer. Cela introduit dans le noyau un **troisième
état d'étape** — *pas encore, et ce n'est pas un échec* — qui n'existe pas encore.

**Alternatives écartées.**
- *Trois mécanismes séparés* — ne composeraient pas ; on rebâtirait la même logique
  trois fois.
- *Quota clé par fournisseur* — la bonne maille est **(fournisseur, modèle)** (un quota
  Sonnet devenu quota Fable). Donc **clé ouverte découverte à l'exécution**, jamais un
  `enum`.

**Conséquences.** Le fait générateur est le **`StepKind`** (un `AgentStep` consomme du
quota par nature) → « ce workflow n'a pas d'IA » se *déduit* au lieu de se cocher.
L'étape **nomme son agent et son modèle**, ce qui garde le run reproductible. Viendra
avec l'`AgentStep`.

**Renvoi** : `docs/design/parcours.md` §7.13.1.

---

## D-010 — Pas de monade `Result<T,E>` générale

**Statut** : Accepté (2026-07-23).

**Contexte.** Faut-il introduire une monade `Result`/`Either` pour distinguer échecs
métier et exceptions techniques ?

**Décision.** **Non** à un `Result<T,E>` général. La distinction *est déjà faite*, et par
des **types spécifiques mieux qu'une monade générique** : `ScriptResult`/`ScriptOutcome`
(un code ≠ 0 est une issue normale qu'on route, jamais une exception), `ValidationReport`
(échecs **accumulés** en valeur — une `Either` court-circuiterait au premier), et les
**exceptions** pour les invariants techniques/environnementaux.

**Alternatives écartées.**
- *Un `Result<T,E>` uniforme* — **diluerait** la frontière exception-vs-valeur (tentation
  de replier le technique dans le canal-valeur) ; et C# héberge mal les monades (pas de
  do-notation ni de HKT ; `?.`/pattern matching/`switch` suffisent).

**Conséquences.** Seul candidat retenu, à traiter **sous la pression du vrai consommateur
UI** (marche engrenage de configuration) : fermer `LoadResult` en **union fermée**
(`sealed record Loaded/Rejected` + `switch` exhaustif), **pas** une monade. L'asymétrie
« projet illisible lève / workflow illisible rend un `LoadResult` » reste voulue.

**Renvoi** : `architecture.md` §4.6.

---

## D-011 — Le moteur émet un flux de progression, aux mêmes points que le journal

**Statut** : Accepté (2026-07-23).

**Contexte.** L'écran de run (6c·3c) doit montrer les étapes défiler **en direct**. Le
moteur, jusqu'ici, ne rend son `WorkflowRun` qu'à la fin et ne journalise qu'en écriture
durable : aucune source live à laquelle une UI puisse s'abonner. Comment exposer
l'avancement d'un run *pendant* qu'il tourne ?

**Décision.** `ExecuteAsync` gagne un **observateur optionnel** (`IProgress<WorkflowEvent>`)
où le moteur **pousse chaque événement à mesure**. L'invariant qui fait la valeur du choix :
l'émission passe par **un unique point** (`Emit`) qui, dans le même geste, journalise **et**
notifie l'observateur — si bien que le flux éphémère et le journal durable **ne peuvent pas
diverger** (même séquence, même ordre, par construction). Le journal reste la vérité ; le
flux n'est qu'une dérivation vivante, pour l'UI. Réutilise le vocabulaire `WorkflowEvent`.

**Alternatives écartées.**
- *L'UI relit/tail le journal, moteur inchangé* — coupleraît l'UI au schéma SQLite,
  imposerait un polling, rendrait mal le temps réel (latence, pas d'événement « poussé »).
- *Un type d'événement dédié à l'UI, distinct de `WorkflowEvent`* — dupliquerait le
  vocabulaire et **rouvrirait** le risque de divergence que l'invariant « même point
  d'émission » ferme.
- *Une interface maison `IRunObserver` portant le `runId`* — inutile : un observateur est
  créé **par run**, il sait déjà lequel il observe. `IProgress<T>` de la BCL suffit et son
  implémentation `Progress<T>` marshallera vers le thread d'UI en 6c·3c.

**Conséquences.** Le port est éphémère et facultatif : un run headless n'en fournit pas et
se déroule à l'identique. L'écran de run de 6c·3c s'y branchera comme premier des deux flux
(l'autre étant le *tail* du fichier d'artefact).

**Renvoi** : `architecture.md` §7.12 ; plan de marche 6c·3b.

---

## D-012 — Parallélisme d'étapes reporté ; l'écran de run est choisi pour y survivre

**Statut** : Reporté (2026-07-23).

**Contexte.** La maquette de l'écran de run (6c·3c) a fait surgir une question concrète :
faire tourner deux étapes **de front** — les tests back et front côte à côte pour gagner du
temps. Le noyau ne sait pas le faire. Le moteur est une traversée **séquentielle** (un seul
`cursor`, §4.3) ; les arêtes d'une `StepDefinition` sont des **choix exclusifs** sur un code
de sortie, pas des successeurs concurrents. Le `Fork`/`Join` était déjà listé comme question
ouverte « tranchée sur le principe, non planifiée » (§9.3) — mais sans motivation ni lien aux
invariants qu'il rouvre.

**Décision.** Ne **rien construire** maintenant : le parallélisme (fan-out `Fork` → N étapes
concurrentes, `Join` fan-in qui attend que toutes finissent) reste une question ouverte, que
cette entrée **motive concrètement** sans la planifier. Elle dépasse 6c·3c et probablement
l'`AgentStep`.

La décision qui vaut d'être consignée est l'autre : **l'écran de run est dessiné pour
survivre au parallélisme sans reshape.** Le pipeline déroule la **traversée** (une visite = un
nœud — c'est déjà comme ça qu'une boucle se rend, en répétition), et le log ne dépend que du
**nœud sélectionné** (un fichier d'artefact par visite, 6a). Deux étapes concurrentes ne sont
alors que deux nœuds « en cours » côte à côte, chacun sélectionnable avec son log — le manque
est **dans le noyau**, jamais dans la vue.

**Alternatives écartées.**
- *Construire fan-out/join maintenant, tant qu'on tient l'écran* — rouvrirait deux invariants
  (cursor unique, arêtes exclusives) et une machine à états au beau milieu d'un jalon de
  présentation : exactement le mélange « changer le noyau » + « brancher l'UI » que la scission
  du jalon 6 a refusé (§9.4).
- *Dessiner l'écran autour d'un chemin unique, plus simple à court terme* — le condamnerait à
  un reshape le jour du fan-out. La traversée-déroulée ne coûte pas plus cher et encaisse les
  **deux** répétitions du même graphe : la boucle (séquentielle) **et** le parallèle (concurrent).

**Conséquences.** Quand le fan-out arrivera, il touchera `StepDefinition` (arêtes concurrentes
vs exclusives), le moteur (N `cursor`), le journal (des visites entrelacées, plus strictement
ordonnées par un chemin unique) — **mais pas la forme de l'écran**. Le vrai point dur sera la
**jonction** : la barrière fan-in, et surtout que faire quand une branche échoue pendant qu'une
autre tourne encore. À rouvrir avec l'`AgentStep`.

**Renvoi** : `architecture.md` §9.3 (ligne `Fork`/`Join`), §4.3 ; maquette 6c·3c.

---

## D-013 — La projection de run est un objet unique, source-agnostique (deux alimentations, un fold)

**Statut** : Tranché, construit (2026-07-23).

**Contexte.** L'écran de run doit servir **deux cas** : un run *en cours* (flux live poussé par
le lanceur, `D-011`) et un run *passé* (relu du journal, `ReadEvents`). Le parcours (§1.4) les
veut identiques — « un seul écran, deux sources ». Restait à choisir *où* cette identité se
réalise : dans un objet partagé, ou dans deux chemins qui convergent à l'affichage.

**Décision.** **Un seul objet** — `RunProjection` (`Workflows/Projection/`, Core testable) — qui
plie une séquence de `WorkflowEvent` en trajectoire de visites + statut + sélection + contrôle,
**sans savoir d'où vient la séquence**. Les deux alimentations entrent par la **même porte**
(`Apply`) : le flux live et la relecture partagent le fold. C'est le pendant *consommation* de
`D-011`, qui avait fait converger *émission* (journal et flux) par un `Emit` unique.

Corollaire construit dans la même marche : **le flux porte le `runId` dès l'ouverture**
(`RunStarted.RunId`). Le tail du log est indexé par le runId ; sans lui dans le flux, un
observateur live ne saurait où suivre les artefacts (`LaunchAsync` forge le runId en interne et
ne le rend qu'à la fin). La relecture, elle, le connaît déjà (clé de `ReadEvents`) — et le codec
le **restitue depuis la clé de ligne**, jamais rédupliqué dans le payload.

**Alternatives écartées.**
- *Deux objets, un « live » et un « relecture »* — dupliquerait le fold et rouvrirait le risque
  de divergence, le mal même que `D-011` a fermé côté émission.
- *Plier dans le `RunViewModel` (App, non testé §7.12)* — enfouirait la seule vraie logique de
  l'écran sous le tapis « présentation non testée ». Le fold est de la **logique**, il vit en Core
  et se teste en TDD.

**Conséquences.** La coïncidence des deux alimentations est **prouvée** par un end-to-end headless
(plier le flux d'un vrai run == plier sa relecture), à la **précision de la durée près** — le
journal la range en secondes-double (`RunEventCodec`), lossy par conception : c'est une métrique
d'affichage, pas une entrée de routage. Tout le reste (étape, itération, en cours/clos, code de
sortie, issue, état) est bit-à-bit égal. Le `RunViewModel` n'est plus qu'un adaptateur mince :
`StartLive` sur le flux, `Replay` sur la relecture, une seule classe.

**Renvoi** : `architecture.md` §4.18, §7.12 ; `D-011` ; parcours §1.4.

---

## D-014 — Stratégie `PATH` : enrichir de racines connues **et résoudre en absolu**, sans shell de login

**Statut** : Tranché, construit (2026-07-23) — preuve sur bundle restante.

**Contexte.** Une app macOS installée hérite d'un `PATH` GUI **tronqué** (mesuré à vide par
`launchctl getenv PATH`, §6.6). `ProcessRunner` ne lance aucun shell de login pour le ré-enrichir :
une étape appelant un binaire d'`asdf`/Homebrew — voire `git` (§4.13) — échoue en `LaunchFailed`
hors développement. Le trou ne mord que depuis le **bouton** de l'app installée, né en 6c·3c — d'où
son échéance ici. Trois options étaient sur la table (§6.6).

**Décision.** **Enrichir le `PATH` de racines connues** (shims `asdf`, `bin` Homebrew Intel/Apple
Silicon, emplacements système), en **ajoutant en queue** — jamais retirer ni réordonner, pour qu'un
run qui marchait déjà (dev, `dotnet test`) se comporte à l'identique. `PathStrategy` (pur, testé)
porte la logique ; `ProcessRunner` l'applique au lancement.

⚠️ **Le point durable, payé par un test rouge** : `Process.Start` **ne consulte pas** le `PATH` de
`StartInfo.Environment` pour résoudre l'exécutable direct. Enrichir ce `PATH` ne répare donc que les
**petits-fils** (un `npm` y trouve son `node`) ; la commande directe, elle, doit être **résolue en
chemin absolu** par nous (`Resolve`) et posée sur `StartInfo.FileName`. Sans ça, `LaunchFailed`
silencieux depuis le bundle, invisible sous `dotnet test`.

**Alternatives écartées.**
- *Exiger des chemins absolus dans les définitions* — hostile à l'auteur, et casse le partage git
  d'un `project.json` entre machines aux `asdf`/`brew` rangés ailleurs.
- *Ré-enrichir via un shell de login* (`sh -lc`) — un process de plus **par étape**, et un `PATH`
  moins prévisible que des racines explicites.

**Conséquences.** La part pure est testée (enrichissement sans doublon, résolution d'un binaire hors
`PATH` minimal, commande introuvable rendue verbatim pour un `LaunchFailed` net). Reste **la preuve
sur l'app installée** — un binaire d'`asdf`/Homebrew/`git` qui tourne enfin depuis le bundle —, seule
vérif que `dotnet test` ne peut pas donner. Voisin tracé : le **check des prérequis Cursus** (git,
`claude`…), même logique pure, restitution qui attend sa surface (§9.2-15).

**Renvoi** : `architecture.md` §9.2-15, §6.6, §7.12.

---

## D-015 — Provisionnement vraiment asynchrone ; `ConfigureAwait(false)` en bibliothèque, jamais dans la vue

**Statut** : Tranché, construit (2026-07-23). Renverse le « provisionnement synchrone » d'origine.

**Contexte.** À l'usage, cliquer « Lancer » gelait l'UI. Un premier correctif avait enveloppé le
lancement dans un `Task.Run` côté vue — déceptif : il déguisait un contrat async mensonger au
lieu de le réparer. La cause racine était unique : `GitWorkspaceProvisioner` attendait le
sous-process git par `_runner.RunAsync(...).GetAwaiter().GetResult()` — du **sync-over-async** —,
appelé *avant* le premier `await` de `LaunchAsync`. Une méthode `async` s'exécute synchronement
jusqu'à sa première vraie suspension : ce préfixe (montage git, écritures SQLite, `Process.Start`)
tournait sur le thread d'UI. À l'origine, le montage avait été fait synchrone à dessein (« git
worktree est bref, c'est du montage »). Cette hypothèse tombe dès qu'un thread d'UI l'appelle.

**Décision.** Rendre la chaîne **asynchrone de bout en bout**, en deux volets indissociables :

1. **Zéro sync-over-async.** `Provision` → `ProvisionAsync` (l'`await` remplace le `GetResult`) ;
   `IProvisionedWorkspace` passe d'`IDisposable` à **`IAsyncDisposable`**, pour que le démontage
   (git worktree remove) s'`await` aussi (`await using`) au lieu de bloquer. Plus **aucun**
   `.GetAwaiter().GetResult()` dans le noyau.

2. **`ConfigureAwait(false)` sur chaque `await` de la bibliothèque** (provisioner, launcher,
   engine, runner) : les continuations ne capturent pas le contexte de l'appelant et courent sur
   le pool. Une chaîne async *sans* ça ferait rejouer chaque étape (Emit → SQLite, `Process.Start`
   suivant) sur le thread d'UI — le premier volet seul ne suffit pas.

Conséquence : la vue fait un simple `await host.LaunchAsync(...)`, **sans `Task.Run`**. Seul un
préfixe synchrone minimal (un `Process.Start` de git, ~ms) court sur le thread d'UI avant la
première suspension ; tout le reste court sur le pool.

**Règle miroir, côté présentation.** Le `RunViewModel` **ne** met **pas** `ConfigureAwait(false)` :
sa continuation (le `finally` : arrêt du `DispatcherTimer`, `PullLog` sur une propriété bindée) doit
précisément revenir sur le thread d'UI. `ConfigureAwait(false)` est un outil de **bibliothèque**, pas
de vue.

**Alternatives écartées.**
- *Garder le `Task.Run` (le correctif précédent, `7996368`)* — déguise le mensonge au lieu de le
  réparer ; c'est ce que l'utilisateur a justement refusé.
- *`ProvisionAsync` async mais `Dispose` synchrone (`IDisposable`)* — le démontage resterait un
  sync-over-async : à moitié corrigé, et incohérent. D'où `IAsyncDisposable`.
- *`ConfigureAwait(false)` sans rendre le provisionnement async* — ne corrige rien : le blocage est
  dans le préfixe *synchrone*, avant tout `await`.

**Conséquences.** Refactor de forme : comportement identique, 210 tests verts après passage des
signatures et des doubles aux formes async (`await using`, `ProvisionAsync`, `DisposeAsync`,
`ThrowsAsync`). L'écriture du journal court désormais sur un thread du pool (déjà le cas depuis
`7996368`) ; la lecture concurrente d'un run en cours reste non supportée (connexion SQLite unique,
§7.13). « L'UI ne gèle plus » reste vérifié à la main (pas de harnais Avalonia headless, §9.2-11).

**Renvoi** : `architecture.md` §4.13, §7.8, §9.2 ; `D-011` (le flux qui alimente l'écran).

---

## D-016 — L'UI se découpe en modules recomposables, pas en écrans monolithiques

**Contexte.** Toute la logique vit dans `Cursus.Core` : projections source-agnostiques (`D-013`),
`ProjectHost` comme racine de composition (§7.12). L'utilisateur en tire un constat juste — cet
emplacement rend l'UI *déplaçable* : un mode headless, une CLI, ou une autre disposition d'écrans se
construisent sans réécrire une ligne de métier. Mais il pointe la moitié manquante : **ça ne suffit
pas si la couche présentation elle-même est faite d'écrans monolithiques**. Un écran qui possède tout
son contenu fige une disposition qu'on voudra bouger à l'usage, et le rend indolore côté *données*
tout en le laissant coûteux côté *vue*.

**Décision.** Chaque brique d'écran — trajectoire/graphe, liste, log de la visite sélectionnée,
contrôle à trois positions, liste des workflows, historique d'un workflow — est un **composant adossé
à sa propre projection/adaptateur**, qui **ignore quel écran l'héberge** et quels autres composants
l'entourent. Un écran est une **composition** de ces briques, jamais leur propriétaire. Corollaire :
on réarrange les surfaces — déplacer le graphe, sortir l'historique d'un workflow dans un sous-écran
ou le déplier en place — **sans toucher la logique**, pour trancher les dispositions *à l'usage*
plutôt que sur plan.

C'est l'**extension côté présentation** du principe déjà acté côté noyau au §7.12 (« un module par
capacité », « règle de sens unique : aucun module ne connaît la racine »). La sélection partagée
graphe/liste (parcours §1.4) en est déjà une instance : deux rendus d'un seul état, qui n'ont pas
besoin de se connaître.

**Pourquoi maintenant.** La discussion de navigation — *où vit l'historique complet d'un workflow*,
(a) le déplier dans la vue workflows vs (b) un sous-écran propre — a montré qu'on ne veut pas figer
sur plan des agencements qui se jugent à l'usage. Ce découpage rend le report **gratuit** : essayer
(a) puis (b) ré-agence des briques, il ne refond pas d'écran.

**Alternatives écartées.**
- *Écrans monolithiques (un ViewModel par écran, propriétaire de tout son contenu)* — fige la
  disposition ; changer d'agencement = réécrire l'écran. C'est précisément ce que l'emplacement de la
  logique dans `Core` rend indolore côté données et que ce découpage rend indolore côté vue.
- *Un routeur / une pile de navigation pour « bouger » les écrans* — déjà écarté au parcours §4 (pas
  de routeur, §D implicite) : la recomposition se fait par **composition de vues**, pas par navigation.

**Conséquences.** Contrainte **sur le neuf**, aucune dette rétroactive : l'écran de run l'honore déjà
(`RunViewModel`/`RunVisitRow`, graphe/liste comme vues sœurs à sélection partagée). Chaque brique se
teste par sa projection `Core` ; la vue reste non testée (§7.12). Ne pas confondre avec la
testabilité : c'est de la **recomposabilité d'agencement**, orthogonale à elle.

**Renvoi** : `architecture.md` §7.12 (directive de découpage, côté présentation), §4.18 ;
`parcours.md` §1.4, §4 ; `D-013` (la projection source-agnostique, la brique type).

---

## D-017 — Le calcul de disposition du graphe vit en Core, testé ; le pixel reste en App, non testé

**Contexte.** La vue graphe existe mais **rend brut** (§4.18, §9.4) : les nœuds empilés en flux
vertical, ordre de définition, arêtes en texte. Le premier chantier de la passe visuelle est un
**layout véritable** — placer les étapes en 2D, dessiner des connecteurs, montrer les boucles. Se
posait la question : *ce placement est-il de la vue (non testée, §7.12) ou du calcul (testé) ?* Et
s'il est du calcul, **où** vit-il, alors qu'il n'existe pas de projet `Cursus.App.Tests` ?

**Décision.** On **coupe le layout en deux** à la frontière testé/non-testé :

- **La grille abstraite** — `(colonne, ligne)` par nœud, arêtes classées avant/retour — est un
  **calcul pur de la structure**, à propriétés de correction (la profondeur respecte les arêtes ; un
  cycle ne fait pas diverger ; un îlot reçoit une place). Elle vit en **Core**, dans
  `Workflows.Projection` (`GraphLayout`), **testée** (`GraphLayoutTests`), sans une ligne d'Avalonia,
  **sans pixel**.
- **Le pixel** — largeur de colonne, hauteur de ligne, taille de nœud, tracé des connecteurs — est du
  **réglage à l'œil**. Il vit en **App** (`RunGraphViewModel` multiplie la grille par ses constantes,
  `RunView.axaml` trace sur un `Canvas`), **non testé** (§7.12).

**Pourquoi.** La frontière testé/non-testé (§7.12) ne suit pas la frontière Core/App par hasard : elle
suit *ce qui a une bonne réponse vérifiable*. La colonne d'un nœud dans un DAG **est un fait** (le
plus-long-chemin) ; la largeur d'une colonne en pixels **est un goût**. Mettre le calcul en Core lui
donne sa couverture là où elle vaut ; laisser le pixel en App évite de tester un réglage. `GraphLayout`
est **statique** (fonction de la structure) là où `GraphProjection` est **dynamique** (plie le flux) :
deux responsabilités, deux types — pas une extension.

**Alternatives écartées.**
- *Layout côté vue (converter / code-behind)* — non testable, alors que c'est l'algorithme qui **mérite**
  la couverture (cycles, convergences, îlots). On perd le test sur exactement la partie fragile.
- *Créer `Cursus.App.Tests` pour y tester le layout* — un projet entier pour une pièce de structure pure
  sans Avalonia ; sa place est en Core, là où le support d'affichage testé (`GraphProjection`) vit déjà.
- *Étendre `GraphProjection` avec la disposition* — mêle géométrie statique et statut dynamique,
  recalcule à chaque événement une disposition inchangée. Type séparé.
- *Layout Sugiyama complet (minimisation des croisements)* — au-delà du besoin d'un premier layout
  véritable ; l'ordre de définition dans la colonne suffit et laisse la place à un raffinement ultérieur.

**Conséquences.** `GraphLayout` rend `Placements` (`NodePlacement`), `Edges` (`LaidOutEdge` avec
`IsBackEdge`), `ColumnCount`, `RowCount`. `GraphEdgeRow` (l'arête-texte par nœud) disparaît au profit
de connecteurs tracés au niveau graphe. Applique la frontière §7.12 à un cas neuf ; ne renverse rien.

**Renvoi** : `architecture.md` §4.18 (`GraphLayout`, la sœur statique), §7.12 (frontière testé/non-testé) ;
`D-016` (module + projection dédiés, qu'on prolonge ici) ; `schemas.md` §5.2.

## D-018 — Le `name` d'une étape est un titre court ; une `description` optionnelle porte la phrase longue

**Contexte.** On ouvre l'arc **authoring** (créer/modifier projet · workflow · étape), et le premier
jalon touche le record que l'éditeur éditera : `StepDefinition`. Or son champ `Name` servait, dans les
faits, de **phrase descriptive** — les workflows commités portaient `"name": "Compiler sans le moindre
avertissement"`. Deux symptômes : le graphe, qui affiche `Name` dans une boîte, **débordait** (contourné
à la passe visuelle par une mesure du libellé + ellipse) ; et la trajectoire, qui affiche l'`id`,
divergeait du graphe sur *ce qu'est* le libellé d'une étape. Construire des formulaires d'étape sur ce
modèle, c'était les bâtir sur un champ qu'on savait mal taillé.

**Décision.** On **sépare les deux registres** dans `StepDefinition` :
- **`Name` redevient un titre court** (« Compiler », « Tester ») — ce que le graphe met dans une boîte ;
- une **`Description?` optionnelle** porte le texte long, ce que `Name` charriait avant.

Le champ est ajouté **en fin** du record (`… WorkingSubdirectory = null, Description = null`) : optionnel
donc compatible avec toutes les constructions positionnelles existantes. Le format de fichier gagne une
clé `description`, placée **après `name`** dans le DTO (ordre JSON naturel), écrite **explicitement même
nulle** (convention du format, cf. `environment`/`timeoutSeconds`/`workingSubdirectory`).

**Pourquoi.** Deux besoins distincts — un libellé qui tient dans un nœud, une explication qui peut être
longue — méritent deux champs, pas un seul surchargé. Trancher **maintenant**, avant l'éditeur, évite de
dessiner des formulaires sur un modèle à migrer ensuite (double coût). Et le débordement du graphe se
règle **à la source, par la donnée** : un `Name` court rend une boîte courte, sans toucher une ligne de
vue (la mesure + ellipse restent en filet).

**Alternatives écartées.**
- *Garder `name` = phrase et régler l'affichage côté vue* (troncature, retour ligne) — soigne le symptôme,
  laisse le modèle faux, et n'offre nulle part où écrire une vraie description.
- *Insérer `Description` juste après `Name` dans le record* (plus naturel sémantiquement) — churnerait
  chaque construction positionnelle de la suite pour un gain esthétique ; l'ajout en fin ne casse rien.
- *Faire descendre `Description` dans les projections* (`GraphNode`, `RunVisit`) pour l'afficher tout de
  suite — YAGNI : rien ne la consomme encore, l'éditeur la lira depuis la définition. Reporté, non écarté.

**Conséquences.** `StepDefinition` gagne `Description?` ; `WorkflowSerializer` la lit et l'écrit ;
`StepDocument` gagne le champ. Les 2 workflows commités sont **migrés** (`name` court + `description`).
`WorkflowValidator` **inchangé** (la description ne porte aucun invariant). La trajectoire affiche encore
l'`id` : l'**unifier sur `Name`** (faire descendre `Name` dans `RunProjection`) est la polish « cohérence
graphe↔liste », **différée** hors de ce jalon.

**Renvoi** : `architecture.md` §4.1 (le record), §4.2 (format + convention des nuls) ; `schemas.md` §5.2 ;
prochaine pierre de l'arc — `D-01x` à venir : persistance de catalogue + modèle d'édition (brouillons permis).

---

## D-019 — Le catalogue passe en lecture + écriture ; « brouillons permis » vit dans un `Save` qui ne valide pas

**Contexte.** L'arc **authoring** se poursuit, Core d'abord. Après le modèle « titre court » (`D-018`),
il restait une **asymétrie** : `WorkflowCatalog` savait *lister* et *charger* (`List`/`Load`), le
sérialiseur savait *réécrire* (`WorkflowSerializer.Write`, testé, aller-retour prouvé) — mais **personne
ne persistait**. Aucun code ne créait, ne sauvegardait, ne supprimait ni ne renommait un workflow sur le
disque. L'éditeur à venir n'avait nulle part où écrire. Par ailleurs, une bifurcation de l'arc avait été
tranchée : **brouillons permis** — on doit pouvoir persister un workflow *même invalide*, la validation
devenant un diagnostic vivant plutôt qu'un péage à l'écriture.

**Décision.** Le **chemin d'écriture vit sur `WorkflowCatalog`** (pas dans un nouveau store), par
symétrie avec `Load` : la même classe qui apporte disque + identité pour lire les apporte pour écrire, et
délègue la traduction au sérialiseur (`Load`→`Read`, `Save`→`Write`). Quatre méthodes :
- `Create(id)` fait naître un **brouillon vide** (`entryStep: ""`, `steps: []`) — invalide mais éditable ;
- `Save(id, def)` persiste **sans valider** — c'est là, et nulle part ailleurs, que « brouillons permis »
  se réalise ;
- `Delete(id)` · `Rename(old, new)`.

Deux invariants gardés, deux exceptions nommées : refuser d'écraser une identité déjà prise
(`WorkflowAlreadyExistsException`, sur `Create` et `Rename`) ; rejeter un id qui échapperait au dossier
des workflows (`InvalidWorkflowIdException`, garde au **point de choke unique** `PathOf`). L'absence d'un
fichier, elle, laisse remonter le `FileNotFoundException` du framework — convention déjà tenue par `Load`.

**Pourquoi.** « Brouillons permis » **ne coûte aucun type neuf** : `WorkflowDefinition` n'est *que de la
donnée* (une arête vers une étape absente est représentable), et l'invariant de validité ne vit que dans
`LoadResult` (couplage `Definition != null ⟺ IsValid`). Un `Save` qui appelle `Write` sans passer par le
validateur persiste donc un graphe cassé sans effort ; c'est le *chargement* qui en rapporte les
problèmes. Loger l'écriture sur le catalogue plutôt que dans un store séparé évite de disperser la
responsabilité « les workflows d'un projet » sur deux types. La garde de légalité d'id est **load-bearing**
dès que l'éditeur (·3) fera transiter de la saisie utilisateur : un id avec séparateur ferait atterrir le
fichier hors du dossier.

**Alternatives écartées.**
- *Un `WorkflowStore` statique façon `ProjectStore`* — casserait la symétrie avec `Load` et couperait en
  deux la responsabilité du catalogue.
- *Faire valider `Save`* (refuser d'écrire un graphe cassé) — contredit frontalement « brouillons permis » :
  l'éditeur doit pouvoir sauver un travail en cours, la validité est un diagnostic, pas un péage.
- *Naître avec un squelette « valide »* (une étape placeholder) — présomptueux, et faux dès que
  l'utilisateur veut autre chose ; naître vide est l'honnête traduction du brouillon.
- *Slugifier un libellé humain en identifiant dans le catalogue* — la transformation libellé→id est
  l'affaire de la couche éditeur (·3) ; le catalogue **rejette** un id illégal, il ne le répare pas.

**Ce que ce jalon ne fait PAS (·2b, à venir).** Le **`WorkflowDraft` mutable** et ses invariants
référentiels (renommer un Id d'étape réécrit les arêtes qui le ciblent ; supprimer une étape gère les
arêtes pendantes) ; le **chargement non validé éditable** (relâcher le couplage de `LoadResult` pour
*rouvrir* un brouillon cassé en édition — ici `Save` écrit un graphe invalide, mais le rouvrir passe encore
par `Load`, qui rend `Definition == null`). C'est précisément le trou que ·2b referme.

**Conséquences.** `WorkflowCatalog` gagne 4 méthodes + une garde `PathOf` ; deux exceptions neuves dans
`Cursus.Core.Projects`. `WorkflowSerializer` et `LoadResult` **inchangés**. `WorkflowCatalogTests` : 8 → 20.
Suite 232 → **244** (216 Core + 28 Persistence).

**Renvoi** : `architecture.md` §4.7 (la table du catalogue) ; prochaine pierre — ·2b : le draft mutable et
le chargement non validé éditable.

---

## D-020 — Le brouillon mutable garde le graphe clos ; le chargement éditable est une porte sœur, pas un élargissement de `LoadResult`

**Contexte.** `D-019` a donné au catalogue le chemin d'écriture et « brouillons permis » (un `Save` qui
ne valide pas), mais a laissé **un trou explicite** : un brouillon cassé se **sauvegarde**, on ne peut pas
le **rouvrir pour l'éditer**. Deux manques. (1) Rien ne modélise un graphe *en cours d'édition* :
`WorkflowDefinition` est un record immuable — instantané parfait, surface de travail nulle ; remanier à la
main sur des records risque à chaque geste de laisser une arête pendante. (2) Le chargement *nie* le
brouillon cassé : `Load`→`Read`→`LoadResult` couple `Definition != null ⟺ valide`, donc rouvrir un
invalide rend `Definition == null` — on lit le rapport, pas le graphe à corriger.

**Décision.** Deux ajouts.
- **`WorkflowDraft`** (nouvelle sous-couche `Cursus.Core.Workflows.Editing`) : surface mutable dont
  l'invariant est *une opération structurelle laisse le graphe référentiellement clos*. `RenameStep`
  retarge toute arête vers l'ancien id et fait suivre le point d'entrée ; `RemoveStep` purge les arêtes
  entrantes et vide le point d'entrée s'il visait l'étape supprimée. Construction `new(definition)`, export
  `ToDefinition()`. Pas de `StepDraft`/`EdgeDraft` : les records immuables sont reconstruits par `with`, le
  draft n'apporte qu'identité mutable + `MapEdges` (l'opération référentielle commune, un `null` purge).
- **Chargement éditable en porte sœur.** `WorkflowSerializer.ReadEditable` rend la définition parsée
  **même invalide** (null seulement si le *parsing* échoue), pairée à son rapport dans un record neuf
  **`ParsedWorkflow`** — jumeau de `LoadResult` *de forme*, d'invariant *opposé* (`Definition != null ⟺ le
  document a parsé`). `Read` se **réécrit comme un rabat** de `ReadEditable`. `WorkflowCatalog.Open` est la
  sœur de `Load` : `Load`→`Read` pour exécuter, `Open`→`ReadEditable` pour éditer.

**Pourquoi une porte *sœur* et non un `LoadResult` élargi.** Le couplage « non-null ⟺ valide » n'est pas
décoratif : le **chemin de lancement en dépend** (`ProjectHost` fait `Load(...).Definition!`,
`SqliteRunJournal` fait `loaded.Definition ?? throw`, les tests d'exécution consomment `Definition!`).
Relâcher le couplage sur `LoadResult` casserait le run. On garde donc `LoadResult`/`Read` **intacts**
(projection validité-couplée, pour exécuter) et on ouvre à côté une projection **parse-couplée** (pour
éditer). Faire de `ReadEditable` le *primitif* et de `Read` son rabat évite deux chemins de parse à
maintenir en miroir — la dette que l'aller-retour `ToGuard`/`WriteGuard` nous avait apprise ; la suite du
sérialiseur restant verte prouve que le rabat n'a rien changé.

**La bifurcation de `RemoveStep` : purger, pas laisser pendre.** Retenu : la suppression purge les arêtes
entrantes, le graphe reste clos. Écarté : *laisser pendre et laisser le validateur signaler
`UnknownEdgeTarget`* — séduisant car aligné sur « le rapport est un diagnostic vivant », mais cela viderait
`RemoveStep` de sa valeur (à peine plus qu'un `List.Remove`) et ferait payer à l'utilisateur le nettoyage
d'un dégât causé par le modèle, pas par lui. La symétrie qui tranche : les références **suivent le sort de
leur cible** — renommer les retarge, supprimer les retire. Un brouillon reste possible (entrée vidée →
`MissingEntryStep`), mais jamais incohérent avec lui-même. Distinct : une arête que l'utilisateur *tapera*
vers une étape pas-encore-créée (édition d'arête, ·3) est un autre cas, que le validateur attrapera.

**Alternatives écartées.**
- *Élargir `LoadResult` d'un champ « définition parsée »* — polluerait le résultat du chemin d'exécution
  d'une préoccupation d'éditeur, et deux audiences (run/édition) mélangées dans un type.
- *Un tuple `(WorkflowDefinition?, ValidationReport)` au lieu de `ParsedWorkflow`* — nulle part où écrire
  l'invariant inversé, qui est *tout* l'enjeu (le prochain lecteur supposerait « non-null ⟺ valide »).
- *`Open` rend un `WorkflowDraft`* — coupler le catalogue/sérialiseur au modèle d'édition ; `Open` rend
  `ParsedWorkflow`, c'est l'appelant (·3) qui fera `new WorkflowDraft(parsed.Definition)`.
- *`StepDraft`/`EdgeDraft` mutables en miroir* — explosion de types parallèles pour zéro gain.

**Ce que ce jalon ne fait PAS (·3).** La surface de *construction* du draft (ajouter une étape, poser
l'entrée, éditer un script ou une arête) ; la **slugification** libellé→id ; toute UI. `RenameStep` vers un
id déjà porté (collision) est laissé au déroulé de ·3, cohérence « brouillons permis » ⇒ l'admettre
(le validateur signalera `DuplicateStepId`).

**Conséquences.** Sous-couche `Editing` neuve avec `WorkflowDraft` ; `ParsedWorkflow` et `ReadEditable`
dans `Serialization` ; `Read` devient un rabat ; `WorkflowCatalog` gagne `Open`. `LoadResult` et tout le
chemin de lancement **inchangés**. Suite 244 → **254** (226 Core + 28 Persistence).

---

## D-021 — L'unicité d'id est un invariant que le brouillon *tient* ; la validité du graphe, une propriété qu'il *tolère*

**Contexte.** `D-020` a doté `WorkflowDraft` de quoi *remanier* un graphe (rename, remove) mais laissé sa
surface de *construction* « au déroulé de ·3 » : rien pour ajouter une étape, poser l'entrée, décrire un
script, tracer une arête. En câblant `AddStep`, une question surgit — que faire de deux étapes au même
libellé (« Compiler » deux fois) ? `D-020` avait esquissé un penchant provisoire (« cohérence brouillons
permis ⇒ admettre la collision, le validateur signalera `DuplicateStepId` »). En construisant, ce penchant
se révèle **faux**.

**Décision.** Deux régimes d'invariant, distingués.
- **L'unicité d'id est *tenue* par le brouillon.** Ses propres opérations — `RemoveStep`, `RenameStep`, le
  ciblage d'arête — travaillent **par id** ; deux étapes sous un même id les rendraient *indéfinies*
  (supprimer l'id en enlèverait deux ; une arête vers l'id viserait laquelle ?). Ce n'est donc pas de la
  *validité de graphe* que le brouillon peut déléguer au validateur : c'est une condition de cohérence de
  ses propres gestes, du même ordre que la clôture référentielle de `D-020`. Concrètement : `AddStep`
  **désambiguïse** (id slugué rendu libre par suffixe : `compiler`, `compiler-2`), `RenameStep` vers un id
  pris **refuse** (`DuplicateStepIdException`, neuve dans `Editing`).
- **La validité du graphe reste *tolérée*.** Entrée non posée, cible d'arête inexistante, script vide : le
  brouillon les laisse passer, le validateur les rapporte — « brouillons permis ». D'où l'asymétrie des
  gardes de construction : le **sujet** d'une opération doit exister (`SetScript`/`AddEdge` lèvent
  `UnknownStepException` sur un id/`from` absent — on n'édite pas une étape fantôme), sa **référence** peut
  pendre (`SetEntryStep` et la *cible* d'`AddEdge` acceptent un id encore à créer — exactement la ligne de
  `D-020` : « une arête que l'utilisateur *tape* est un autre cas »).

**L'asymétrie `AddStep` désambiguïse / `RenameStep` refuse — assumée.** Même invariant, traitements
opposés, pour une raison : `AddStep` **dérive** l'id d'un titre (l'ajuster en douce est sans surprise —
l'utilisateur n'a pas choisi cet id), tandis que `RenameStep` est un **choix d'id délibéré** (le
désambiguïser en `build-2` trahirait ce que l'utilisateur a tapé ; mieux vaut refuser et le laisser
choisir). Ceci **supersède** le penchant « admettre » de `D-020`.

**Les ids restent humains (slug), pas opaques.** *Alternative écartée* : des ids opaques (`s1`, `s2`),
collision-free et qui rendraient `RenameStep` presque inutile. Rejetée parce que `WorkflowDefinition` est
*« reviewable en Git »* : `compiler` se relit en diff, `s1` non. Ce choix justifie rétroactivement la
machinerie de retargage de `D-020` — si les ids sont dérivés de libellés éditables, les faire suivre est
la raison d'être du brouillon. D'où **`Slug`** (helper Core pur, `label → id` : minuscules, diacritiques
dépliés pour le français, `[a-z0-9-]` seul), réutilisé pour l'id d'étape **et** l'id de fichier de
workflow (où le rejet des séparateurs le rend légal au regard de `PathOf`, `D-019`).

**Ce que ce jalon ne fait PAS (·3b et au-delà).** Toute UI (VM d'édition, XAML, non testé §7.12) ; la
couche qui compose slug-de-titre → `catalog.Create` et décide si éditer un libellé re-slugifie l'id ;
l'édition de `Description`/`MaxVisits`/`WorkingSubdirectory` (purs `with` sans enjeu référentiel) ; le repli
d'un slug **vide** (un id vide passe, le validateur signale `EmptyStepId` — cohérent avec « tolérée ») ;
le volet projet (create/rename). Signaler un `FileName` vide comme invalide reste un trou de validation
connu, hors sujet.

**Conséquences.** `Slug` + `DuplicateStepIdException` neufs dans `Editing` ; `WorkflowDraft` gagne
`AddStep`/`SetEntryStep`/`SetScript`/`AddEdge`/`RemoveEdge` et une garde de collision sur `RenameStep`.
Aucun autre type touché (records du domaine réutilisés tels quels). `SlugTests` (6), `WorkflowDraftTests`
7 → 18. Suite 254 → **271** (Noyau 161 → 178, Core 243, Persistence 28).

**Renvoi** : `architecture.md` §4.2 (le sérialiseur, les deux portes), §4.7 (le catalogue), §4.x (la
sous-couche `Editing`) ; `schemas.md` §5.2 ; prochaine pierre — ·3 : l'éditeur (UI) et le projet.

## D-022 — L'id de fichier d'un workflow est le slug de son titre : une règle nommée en Core, qui refuse la collision

**Contexte.** L'arc authoring a désormais tout le Core pour créer/remanier un graphe ; il faut le câbler à
une UI (jalon ·3b). La toute première action de l'éditeur — *créer un workflow* — pose une question :
d'où vient l'id de son fichier ? `catalog.Create(id)` prend un id déjà formé ; l'utilisateur, lui, tape un
**titre**. `D-021` avait *anticipé* que la composition « slug-de-titre → `Create` » vivrait en glu VM (non
testée, §7.12).

**Décision — la composition devient une méthode Core testable, `WorkflowCatalog.CreateFromTitle(title) → id`.**
`Slug.From(title)` puis `Create(id)`, l'id **retourné** pour que l'appelant ouvre aussitôt l'éditeur
dessus. C'est la **jumelle symétrique** de `WorkflowDraft.AddStep`, qui slugifie de même le titre d'une
*étape* et retourne son id : la même règle — « l'identifiant est le slug du libellé humain » — s'applique
au fichier d'un workflow comme au nœud d'un graphe. La sortir de la présentation lui donne un nom et trois
tests, plutôt que de la diluer dans un ViewModel.

**Refuse la collision, ne désambiguïse pas.** `CreateFromTitle` s'appuie sur le refus que `Create` porte
déjà (`WorkflowAlreadyExistsException` via `RefuseToOverwrite`) : deux titres qui slugifient pareil ne
produisent pas `build` puis `build-2`, le second est refusé. C'est le **côté « refuse » de l'asymétrie de
`D-021`** (`AddStep` désambiguïse / `RenameStep` refuse) appliqué au fichier : le nom d'un fichier de
workflow est un **choix délibéré** de l'utilisateur (comme un rename d'étape), pas un id dérivé en masse
(comme un ajout d'étape). Un titre qui ne retient aucun caractère (`"#!/"`) slugifie en chaîne vide et
tombe sur l'`InvalidWorkflowIdException` du choke `PathOf` (`D-019`) — aucune garde neuve.

**Ceci supersède l'anticipation « glu VM » de `D-021`.** La règle méritait un lieu et un test ; le reste de
·3b (le ViewModel qui appelle `CreateFromTitle` et traduit son refus en message) demeure, lui, de la
présentation non testée.

**Ce que cette décision ne tranche PAS.** La structure de l'UI éditeur elle-même — où vit le module, comment
il se compose avec run, où le catalogue est monté — relève de `D-023`, prise quand ce module est bâti.

**Conséquences.** `WorkflowCatalog` gagne `CreateFromTitle` (dépend de `Slug`, déjà Core) ;
`WorkflowCatalogTests` 21 → 24. Suite 271 → **274** (Core 243 → 246, Noyau inchangé à 178, Persistence 28).

**Renvoi** : `architecture.md` §4.7 (le catalogue) ; `schemas.md` §5.2 ; suite du jalon ·3b — le module
éditeur (`D-023`).

## D-023 — L'éditeur est un troisième module de la surface projet, sœur du run ; le catalogue vit dans le workspace

**Contexte.** `D-022` a posé la règle Core de création. Restait à câbler *l'UI* de l'édition (jalon ·3b) :
où vit le module, comment il se compose avec l'existant, où le catalogue est monté. Ces choix sont de la
présentation — non testée (§7.12) — mais structurants, d'où cette entrée.

**Le module éditeur est le troisième contenu d'une même surface, sans routeur.** `D-016` avait posé que
la surface d'un projet ouvert est faite de **modules recomposables**, pas d'écrans monolithiques, et que
run passé = run en cours = même écran. ·3b prolonge exactement ce mécanisme : `OpenProjectViewModel`
tenait déjà `CurrentRun`/`IsShowingRun` (liste ⇄ run) ; il gagne `CurrentEditor`/`IsShowingEditor` **par
le même patron**, plus un `IsShowingList` calculé. Les deux modules sont **mutuellement exclusifs** —
ouvrir l'un ferme l'autre — et la liste s'affiche quand aucun n'occupe la surface. Aucun routeur, aucune
réification de navigation : on étend le mécanisme éprouvé plutôt que d'en inventer un. *Alternative
écartée* : un `WorkflowEditorView` séparé (fichier `.axaml`/`.axaml.cs` comme `RunView`) — inutile ici,
le module tient dans un `DataTemplate` inline ; on le réifiera si sa complexité l'exige.

**`WorkflowEditorViewModel` est un adaptateur mince sur `WorkflowDraft`.** Monté par une fabrique
`Open(id, catalog, onSaved)` (sœur de `RunViewModel.StartLive`/`Replay` — la coquille ne construit jamais
le brouillon, elle reçoit l'éditeur câblé). Toute la logique métier — unicité d'id tenue, validité
tolérée, retarge/purge d'arêtes — **reste dans le brouillon** (Core, déjà TDD `D-021`) ; le VM ne fait que
binder les gestes, **re-projeter** le brouillon en lignes après chaque mutation structurelle, et **valider
en direct** (`WorkflowValidator.Validate`, dont le rapport agrégé a toujours été conçu « pour qu'un éditeur
affiche tout d'un coup », §7.6). C'est la même symétrie que run : un VM d'app adossé à une capacité Core.

**Le catalogue vit dans `ProjectWorkspace`, délibérément hors du host.** `ProjectHost` documente « lister et
charger restent `WorkflowCatalog` » — le router par le host en ferait un Service Locator. Le catalogue
rejoint donc `ProjectWorkspace`, le bundle « ce dont l'écran d'un projet a besoin, monté d'un bloc », aux
côtés du host et du magasin d'artefacts ; c'est la racine de composition (`App.axaml.cs`, le `Project` en
main) qui construit `new WorkflowCatalog(project)`. Un wrapper sans état, sans connexion à disposer.

**Deux détails d'UI qui portent une décision.** (1) Le **renommage d'un workflow reste en glu de VM**
(`Slug.From(titre)` + `catalog.Rename`), là où la *création* est promue en Core (`CreateFromTitle`) : la
naissance d'un workflow est la règle qui méritait un nom et un test, un déplacement de fichier est
mécanique — si la duplication de `Slug.From` gêne, un `RenameFromTitle` testé l'absorbera. (2) Les **champs
de script d'une étape sont locaux**, poussés au brouillon par « Appliquer » et non à la frappe : re-projeter
à chaque touche recréerait la ligne en cours d'édition. La validation reste « live » sur les mutations
*structurelles* (ajout/retrait d'étape ou d'arête, choix d'entrée), qui seules changent le rapport.

**Ce que ·3b ne fait PAS.** Renommer une étape après création (re-slug d'id), la garde `Code = n` à
l'arête (elle exige une saisie numérique — les trois gardes sans paramètre suffisent au minimal),
réordonner, éditer `Description`/`MaxVisits`, la vue graphe *en édition* (l'éditeur est un formulaire
structuré, pas un canevas — `GraphProjection`/`GraphLayout` restent des vues de **lecture** d'un run). Le
volet **projet** (create/rename) est la prochaine et dernière pierre de l'arc authoring, plan distinct.

**Conséquences.** App seule : `WorkflowEditorViewModel`, `StepEditorRow`, `EdgeEditorRow` neufs ;
`ProjectWorkspace` gagne `Catalog` ; `OpenProjectViewModel` gagne le volet catalogue et le 3ᵉ module ;
`WorkflowRowViewModel` devient observable (renommage inline) ; `MainWindow.axaml` gagne le volet et le
template éditeur. Aucun test neuf (présentation, §7.12) ; suite inchangée à **274 verts**, build 0 warning.

**Renvoi** : `architecture.md` §4.21 (le module éditeur), §1.1 (la surface projet) ; `schemas.md` §5.2 ;
prochaine pierre — le **volet projet**, fin de l'arc authoring.

## D-024 — La ligne d'arguments de l'éditeur honore les guillemets : `ArgumentLine`, jumeau de `Slug`

**Contexte.** `D-023` avait posé, comme *simplification assumée* de l'éditeur minimal, que le champ
d'arguments d'une étape se découpe aux espaces — « un argument contenant une espace est hors de portée ».
La validation manuelle a montré que cette simplification **casse le cas le plus courant** : lancer une
commande shell via `zsh -c "commande"`. Le flag `-c` de zsh prend le *seul argument suivant* comme
commande ; découpé aux espaces, `zsh -c "dotnet build"` devient `zsh` `-c` `dotnet` `build` → zsh exécute
la commande `dotnet` (sans argument) et passe `build` en paramètre positionnel ignoré. Symptôme observé :
« echo n'affiche rien », `dotnet build` lance `dotnet` tout court — le workflow entier silencieusement
faux.

**Décision — un tokenizer qui honore les guillemets, `ArgumentLine` (Core/Editing), TDD.** `Parse(line) →
argv` découpe aux blancs *sauf à l'intérieur* d'une région entre `"…"` ou `'…'` (guillemets retirés) ;
`Format(argv) → line` re-guillemette les tokens qui portent une espace, un guillemet, ou sont vides, pour
que **`Parse∘Format` soit l'identité** sur les cas courants. Pur, statique, **jumeau symétrique de
`Slug`** : les deux vivent dans `Editing`, transforment un texte humain en modèle exact, et sont **testés**
parce que leur correction se casse en silence (le quoting est exactement ce genre de chose). `Slug`
traduit un libellé en identifiant ; `ArgumentLine`, une ligne de saisie en `argv`.

**Ce qui est écarté.** Pas d'échappement backslash (`\"`) : un guillemet d'une sorte est littéral à
l'intérieur de l'autre (`"it's"`, `'say "hi"'`), ce qui couvre les cas réels ; un token contenant les
**deux** sortes de guillemets ne round-trip pas — cas pathologique **assumé**, documenté. *Alternative
écartée* : garder le champ « arguments » découpé aux espaces et documenter la limite — rejetée, elle rend
`zsh -c` (donc toute commande shell) inexprimable, ce n'est pas une limite acceptable pour un éditeur.
*Autre écartée* : un tokenizer en glu de VM (App non testée) — rejetée pour la même raison que `Slug`
n'y est pas : le quoting mérite des tests, et `Editing` est sa maison.

**Ceci supersède la « simplification assumée » de `D-023`** (arguments hors de portée d'une espace). Le
reste de `D-023` tient.

**Conséquences.** `ArgumentLine` neuf dans `Editing` ; `WorkflowEditorViewModel` (`UpdateScript`/
`FlushScripts`) et `StepEditorRow` (affichage) l'emploient à la place du découpage/jointure naïfs.
`ArgumentLineTests` **17**. Suite 274 → **291** (Noyau 178 → 195, Core 263, Persistence 28). Build 0 warning.

**Renvoi** : `architecture.md` §4.21 (l'éditeur), §4 (namespace `Editing`) ; `schemas.md` §5 (nœud
`Editing`).

## D-025 — Renommer un projet est du Core testé ; le rail « ajoute ou crée » d'un seul geste — l'arc authoring est clos

**Contexte.** La surface d'un projet supposait qu'un projet **existe déjà** : le rail savait *ajouter* un
`.cursus/` présent et *retirer* une entrée, mais ni **créer** ni **renommer**. `ProjectStore.Create`
existait et était testé depuis le jalon 5, jamais câblé à l'UI ; renommer n'existait nulle part. Ce
jalon — le **volet projet** — ferme les deux trous et clôt l'arc **authoring** (workflows d'abord, projet
en dernier).

**Décision 1 — renommer vit en Core (`ProjectStore.Rename`), pas en glu VM.** À l'inverse du renommage de
*workflow* (`D-022`), resté en glu VM parce qu'il ne composait que des primitives **déjà existantes**
(`Slug.From` + `catalog.Rename`), renommer un projet n'a **aucune** primitive disponible : c'est réécrire
`project.json` avec un nouveau `Name` en **préservant l'`Id`**. C'est de l'écriture de la disposition d'un
projet — la responsabilité *unique* de `ProjectStore`. Sans préservation de l'`Id`, le registre machine ne
reconnaîtrait plus le même projet. La règle « réécrire le nom sans toucher à l'identité » mérite un nom et
un test — même raisonnement que `D-022` pour `CreateFromTitle`.

**Décision 2 — `ProjectRegistry.Rename` tient l'instantané.** Le nom vit sur disque, pas dans
`projects.json` (qui ne liste que des racines) : n'écrire que le disque laisserait `_registry.Projects`
sur l'ancien nom, et le prochain `SyncProjects` le **ressusciterait**. Le registre gagne donc
`Rename(root, newName)` = `ProjectStore.Rename` + **remplace son instantané** pour cette racine, **sans**
réécrire son fichier (les racines ne bougent pas). Il rend le `Project` frais.

**Décision 3 — créer reste une composition de surface, pas de `ProjectRegistry.Create`.**
`ProjectStore.Create` (pose le `.cursus/`) puis `_registry.Add` (inscrit + relit un instantané frais)
suffisent : `Add` rafraîchit déjà l'instantané. **Asymétrie assumée** — créer passe par `Add`, renommer
par `Rename` (qui, lui, porte une logique que seul le registre peut tenir : le rafraîchissement).

**Décision 4 — un seul bouton « Ajouter un projet », qui ajoute *ou* crée.** *D'abord tranché* en deux
gestes distincts (Créer / Ajouter, refus traduit en message). La **validation manuelle l'a renversé** :
plus ergonomique d'un seul bouton. Le sélecteur rend un dossier ; si le dossier porte déjà un `.cursus/`,
`OpenOrCreateProject` l'inscrit et l'ouvre ; sinon, le refus `ProjectNotFoundException` du registre devient
une **bifurcation vers la création** (champ nom **pré-rempli du nom feuille du dossier**, éditable) plutôt
qu'un message d'erreur. Ceci **supersède** l'anticipation « créer ≠ ajouter, deux boutons » du plan.
*Gotcha payé* : le sélecteur rend souvent un chemin à **séparateur final** (`…/Projet/`), qui vide
`Path.GetFileName` — d'où `Path.TrimEndingDirectorySeparator` avant d'extraire le nom.

**Décision 5 — `ProjectRowViewModel`, symétrique de `WorkflowRowViewModel`.** Le rail bindait des `Project`
**nus** (Core, immuables) : nulle part où loger l'état d'un renommage inline. On introduit une ligne
bindable mince (enveloppe un `Project` mutable, `Name` dérivé qui se re-notifie au swap, état
`IsEditing`/`DraftName`) — même patron que la ligne de workflow, dans l'esprit « UI en modules
recomposables » (`D-016`). Le geste réel reste au parent (`ShellViewModel.RenameProject`), qui seul touche
le registre ; renommer **met à jour la ligne en place** (projet frais poussé via `Applied`), sans
reconstruire le rail — la sélection courante survit.

**Ce qui est écarté / hors jalon.** Supprimer le *dépôt* d'un projet (retirer reste *oublier*, jamais
détruire) ; valider/contraindre le nom (le modèle pose « le nom n'est qu'un libellé » — `Rename` reste
permissif en Core, l'UI n'ignore qu'un renommage à blanc) ; distinguer un projet déplacé d'un supprimé
(registre machine complet). Trou connu conservé : renommer le projet **ouvert** ne rafraîchit pas le titre
de sa surface (figé à l'ouverture) — mineur, la ligne du rail, elle, suit.

**Conséquences.** `ProjectStore.Rename` + `ProjectRegistry.Rename` neufs (Core, TDD) ; `ProjectRowViewModel`
neuf (App, non testé §7.12) ; `ShellViewModel` rail en lignes + flux créer + `RenameProject` +
`OpenOrCreateProject`. Core 263 → **265** (`ProjectStoreTests` 13 → 14, `ProjectRegistryTests` 10 → 11) ;
noyau **inchangé** (tests sous `Projects/`, hors `Workflows/`). Suite 291 → **293**. Build 0 warning.

**Renvoi** : `architecture.md` §4 (`ProjectStore`/`ProjectRegistry`), §7.13 (la coquille), le trou « volet
projet » refermé ; `schemas.md` (rail).

---

## D-026 — Le workflow devient un lieu : une page par workflow, l'historique en est la première section

**Contexte.** L'arc *authoring* clos (`D-025`), direction ouverte. L'utilisateur demande un écran
d'historique des runs. La recherche montre le terrain à moitié bâti : `IRunJournalReader.ListRuns()` rend
déjà **tous** les runs (plus récent d'abord, testé SQLite) et `RunViewModel.Replay(RunSummary, …)` rouvre
déjà **n'importe quel** run dans l'écran de run existant. `ProjectHost` n'exposait que
`LastRunPerWorkflow()` — il jetait tout sauf la tête. La discussion (maquette validée, artifact `47687d22`)
élargit le cadrage et **tranche une question que `D-016` avait laissée ouverte à dessein** : *où vit
l'historique complet d'un workflow* — (a) déplié dans la liste, (b) un sous-écran propre.

**Décision.**
1. **Portée par workflow.** L'index projet reste la liste des workflows ; l'historique vit *dans* le
   contexte d'un workflow, pas dans un journal global.
2. **Le workflow devient un lieu.** Cliquer le corps d'une ligne ouvre **sa page** — un hub à onglets
   (`WorkflowPageViewModel`) qui **compose** des modules qui s'ignorent (honneur concret de `D-016`, non un
   monolithe) : *Historique* (neuf) et *Étapes* (l'éditeur existant, **replié** ici tel quel). *Graphe* et
   *Déclencheurs* sont annoncés « à venir », hors jalon. C'est la **variante (b)**, choisie parce qu'elle
   **amorce le hub** — (a) et le panneau latéral collaient l'historique à la liste, à défaire plus tard.
3. **Une seule couture Core, testée : `ProjectHost.RunsOf(workflowId)`** — jumeau de `LastRunPerWorkflow`,
   filtre `ListRuns()` sur le `WorkflowId`, ordre conservé. Mince, mais c'est au **host** d'exposer les
   requêtes de runs : la surface ne parle jamais au journal (règle de sens unique). Filtrer côté VM aurait
   percé cette frontière et cassé la symétrie avec l'accesseur voisin.
4. **La surface échange `liste / run / page`, sans routeur.** Le module éditeur **plat** de
   `OpenProjectViewModel` (`CurrentEditor`) est remplacé par `CurrentWorkflowPage` ; l'éditeur y devient
   l'onglet *Étapes*. Lancer depuis la page ouvre le run ; **le fermer revient à la page** (le workflow
   reste le contexte courant), et l'historique s'y rafraîchit — le passage qui vient de finir y figure.
5. **`RunRowViewModel`**, une ligne de run bindable (verdict + date), **partagée** entre la liste (dernier
   passage) et l'historique (chaque passage) : le formatage du verdict, jadis dans `WorkflowRowViewModel`,
   y est extrait — une seule règle de libellé.

**Alternatives écartées.**
- *Historique global (tous workflows mêlés)* — la portée par workflow colle au cadrage `D-016` et à la
  ligne « dernier passage » déjà par workflow.
- *Dépli en place (a) / panneau latéral* — collent l'historique à la liste sans amorcer le hub.
- *Filtrer `ListRuns()` côté VM* — viole la règle de sens unique, casse la symétrie avec l'accesseur voisin.
- *Construire le hub complet (graphe statique, déclencheurs) tout de suite* — *big design up front* ;
  cibles reportées (§7.10.6). On sème la page, on ne meuble pas d'onglets vides.

**Conséquences.** Contrainte sur le neuf, aucune dette rétroactive. La couture Core est testée
(`ProjectHostTests` +1, Core 265 → **266**, noyau **inchangé**, suite 293 → **294**, build 0 warning) ; la
page, la ligne de run et le reparentage de l'éditeur sont de la présentation, non testés (§7.12), validés à
la main. Le hub est la **base d'accueil** des facettes à venir : un onglet *Graphe* (via `GraphLayout`,
déjà pur sur une définition) et un onglet *Déclencheurs* (cron, état de tâche) s'y ajouteront comme
modules, sans toucher les autres — ce que `D-016` a été conçu pour rendre gratuit.

**Renvoi** : `architecture.md` §4.23 (la page du workflow), §4 (`ProjectHost` + `RunsOf`), le §4.16/§7.12
du hub de composition ; `schemas.md` (surface `liste / run / page`, si carte d'état et non delta figé).

## D-027 — Le graphe de définition est le header de l'onglet Étapes, pas un onglet

**Statut** : accepté, construit.

**Contexte.** `D-026` a ouvert le hub du workflow et notait, parmi les facettes à venir, « un onglet
*Graphe* (via `GraphLayout`, déjà pur sur une définition) ». En avançant sur cette facette, la discussion
a renversé cette note : le graphe de la **définition** n'est pas un frère de l'onglet *Étapes*, c'est **une
seconde vue des mêmes étapes**. On édite le graphe en texte dans l'éditeur ; le montrer en image à côté,
dans un onglet séparé, le détacherait de ce qu'il représente. Deux graphes coexistent déjà et ne doivent
pas être confondus : celui d'un **run** (event-fed, `GraphProjection`, coloré par l'état, vit dans l'écran
de run) et celui de la **définition** (statique, `GraphLayout`, la forme seule). C'est le second dont il
est question ici.

**Décisions.**
1. **Le graphe de définition coiffe l'éditeur (header), il n'est pas un onglet.** Formes complémentaires :
   le DAG est large-et-court (bon bandeau), l'éditeur étroit-et-long (liste qui défile). Le header est
   borné et défile seul (il ne pousse pas la liste) ; il se replie quand le workflow est vide. **Supersède**
   la note de backlog « onglet Graphe statique » de `D-026`.
2. **Un module App neuf, statique : `DefinitionGraphViewModel`.** Sœur sans-projection de
   `RunGraphViewModel` : prend une `WorkflowDefinition`, la pose en pixels, expose `Nodes`/`Connectors`/
   `Canvas*`/`HasNodes`. Une méthode `Show(definition)` que l'éditeur rappelle dans son `Project()` (déjà
   exécuté après chaque mutation, calculant déjà la définition) — la silhouette suit l'édition.
3. **La géométrie « grille → pixels » reçoit un foyer unique : `GraphGeometry`** (helper statique App).
   Elle vivait entremêlée au statut dans `RunGraphViewModel.Rebuild` ; extraite, elle sert **les deux**
   graphes, qui rendent alors identiquement (une boîte d'étape fait la même taille qu'on édite ou qu'on
   exécute). C'est le foyer de la « géométrie » que `D-017` situe en App. `GraphConnectorRow` est réutilisé
   (non-emprunté → tracé gris statique) ; il gagne une **tête de flèche** à la pointe — un triangle plein
   dans un tracé à part (jamais tireté, rempli de la couleur du trait) qui rend la direction de l'arête
   explicite, sur les **deux** graphes puisqu'ils partagent le connecteur. Le nœud de définition est nu
   (`DefinitionNodeRow`, ni glyphe ni statut), `GraphNodeRow` restant réservé au run.
4. **Les orphelines s'affichent.** En édition, une étape neuve est orpheline avant qu'on ne tire son arête ;
   la cacher empêcherait de la câbler. Garantie **déjà tenue au Core** (`GraphLayout` place toute étape,
   îlot compris — `GraphLayoutTests`) : l'App n'a qu'à dessiner toutes les `Placements`. `GraphGeometry`
   **saute** en revanche une arête pendante (cible inexistante, tolérée par l'éditeur `D-021`) — pas de
   point d'arrivée à tracer ; le validateur signale déjà la référence en texte.

**Alternatives écartées.**
- *Onglet Graphe séparé* — détacherait le graphe des étapes qu'il montre (voir contexte).
- *Split côte-à-côte (éditeur | graphe)* — l'éditeur est large (nom + script + args + arêtes) ; le graphe
  à sa gauche l'étrangle. Le header vertical respecte les deux ratios naturels.
- *Dupliquer la géométrie dans le nouveau VM* — deux copies de math-pixels qui divergent ; l'extraction
  donne un foyer unique et des rendus identiques.
- *Réutiliser `GraphNodeRow` pour la définition* — son glyphe ○ « non visité » n'a pas de sens hors run.
- *Clic sur un nœud → sa ligne* — reporté (demande une couture de sélection graphe↔liste ; l'overlay de
  run ne l'a pas non plus).

**Conséquences.** Incrément **entièrement présentation** (§7.12), **aucun test neuf** (la seule garantie —
l'orpheline placée — est déjà verrouillée au Core), validé à la main comme chaque incrément UI. Compteurs
**inchangés** (noyau 195 / Core 266 / suite 294). Le run graph est re-câblé sur `GraphGeometry` sans
changement de comportement (re-validé à la main). L'authoring gagne sa seconde vue ; le hub garde une
facette de moins à meubler (le graphe est absorbé, restent les déclencheurs en trajectoire).

**Renvoi** : `architecture.md` (le graphe de définition, près de l'overlay de run ; `GraphGeometry` foyer
des pixels) ; `GraphLayout`/`GraphLayoutTests` (orphelines) ; `D-017` (grille testée / pixels non testés),
`D-026` (le hub dont ceci meuble un onglet).

## D-028 — Le puits d'artefact ruisselle sans flush de l'appelant : flush par écriture, visibilité et non durabilité

**Statut** : accepté, construit. Première brique de la **jambe 1** (`trajectoire.md`).

**Contexte.** La trajectoire vers la boucle de dev agentique fait passer Cursus par une **porte de gate
déterministe** (jambe 1) : faire tourner `dotnet build` puis `dotnet test` contre son propre dépôt. Cette
jambe encaisse trois dettes en les *vivant* ; la première est le **log en streaming intra-étape**. En
l'attaquant, on découvre que le doc l'affirmait déjà (« la sortie ruisselle pendant qu'un script tourne »,
jalon 6a) mais que la pratique le démentait : `LazyAppendStream.Write` déposait les octets dans le buffer
managé du `FileStream`, que `Complete()` ne flushait qu'**à la clôture** de l'étude. Un suiveur
(`ArtifactTail`, autre handle) lisant `file.Length` voyait donc le vide jusqu'au bout — la sortie surgissait
d'un bloc. Les tests de tail existants **masquaient** le trou en appelant `sink.Stdout.Flush()` à la main :
un lecteur en aurait conclu que le flush était requis. Un test rouge sans ce flush l'a démasqué.

**Décision.** `LazyAppendStream` pousse son buffer managé vers le handle OS **après chaque écriture**, via
`Flush(flushToDisk: false)`. C'est exactement ce qu'il faut pour qu'un **autre handle** (le suiveur) voie la
sortie en la relisant : la visibilité inter-handle, pas la durabilité disque. `Complete()` garde son flush
de clôture (désormais défensif). Les `Flush()` manuels des tests de tail sont retirés : ils spécifient à
présent le vrai contrat — *le streaming est intrinsèque*, l'appelant n'a rien à flusher.

**Alternatives écartées.**
- *`FileOptions.WriteThrough`* — fsync à chaque écriture : durabilité disque dont on n'a aucun besoin (une
  sortie de test n'est pas une donnée à préserver contre une coupure), au prix d'un coût d'I/O par ligne.
  On veut voir la sortie, pas la survivre à un crash.
- *Buffer `FileStream` à 1 octet (quasi-non-bufferisé)* — dégénéré et opaque ; un `Flush` explicite après
  écriture **dit** son intention là où un buffer minuscule la cache dans un paramètre de constructeur.
- *Laisser l'appelant flusher (le statu quo masqué)* — c'était le piège : la pompe du `ProcessRunner`
  (`CopyToAsync`) ne flushe jamais, personne n'était donc placé pour le faire au bon moment.

**Conséquences.** Changement d'une seule classe (`LazyAppendStream`, imbriquée dans `RunArtifactStore`),
aucun type neuf, **brief inline** (pas de gate). +1 test Persistence (28 → 29) ; noyau 195 / Core 266
inchangés ; suite **294 → 295**. La promesse du jalon 6a tient enfin au réel. Reste à la jambe 1 : la
**preuve PATH sur bundle** (manuelle), le **routage exit-code vécu**, et l'**authoring du workflow de
gate** lui-même.

**Renvoi** : `architecture.md` §4.12 (le magasin de sortie en flux) ; `trajectoire.md` (jambe 1, la dette
encaissée) ; jalon 6a (`IStepOutputSink`, le puits ouvert avant l'étape).

## D-029 — Le champ « Commande » unique : une ligne, 1er token = binaire ; `CommandLine` posé sur `ArgumentLine`

**Statut** : accepté, construit. Confort d'authoring de la **jambe 1** (`trajectoire.md`) — la ferme.

**Contexte.** La jambe 1 est substantiellement encaissée : streaming (`D-028`), **preuve PATH sur bundle**
faite au réel (PATH vidé, `dotnet` nu se résout via `~/.asdf/shims`, sort 0 — la béquille `/bin/sh -c` est
superflue), routage exit-code **vécu** (`verifier.json`, 44 runs). Restait le confort d'authoring, avec un
motif concret : les étapes de la gate passent par `/bin/sh -c "dotnet build -warnaserror"` alors que
l'éditeur oblige à répartir une commande sur **deux champs** (`FileName` + `Arguments`). Écrire
`dotnet build -warnaserror` d'une seule ligne rend l'invocation directe naturelle.

**Décisions.**
1. **Un seul champ « Commande » remplace les deux.** Le 1er token est le binaire, le reste ses arguments.
   Invocation **directe**, sans shell.
2. **Un type Core neuf, pur et testé : `CommandLine` (`Cursus.Core.Workflows.Editing`).** Jumeau
   d'`ArgumentLine`/`Slug`. `Parse(line) → (FileName, Arguments)` et `Format(fileName, args) → line`, tous
   deux **posés sur `ArgumentLine`** (tokeniseur + requoteur réutilisés) : `CommandLine` n'ajoute que le
   *rôle spécial du premier token*. Deux invariants verrouillés au Core : le **vide toléré**
   (`Parse("  ") → ("", [])`, `Format("", []) → ""` — pas les guillemets vides qu'`ArgumentLine`
   produirait ; brouillon permis `D-020`/`D-021`) et l'**aller-retour** `Format∘Parse = id`.
3. **Le parse vit dans le VM éditeur, `CommandLine` est sa couture.** `UpdateScript(id, command)`
   (au lieu de `(id, fileName, arguments)`) appelle `CommandLine.Parse` ; `FlushScripts` suit.
   `StepEditorRow` troque `_fileName`+`_arguments` contre `_command` (patron « champ local poussé à la
   perte de focus » de `D-024`, repris tel quel). Le noyau et le format JSON **ne changent pas** :
   `ScriptSpec(fileName, args)` reste le stockage, `CommandLine` n'est qu'une **vue d'édition**.

**Ce que ça supersède.** L'anticipation de backlog « champ Commande = zéro changement noyau » : le moteur
reste bien intact, mais un invariant **testé** dans la sous-couche d'édition vaut mieux qu'un découpage muet
en glu VM. (Le « noyau » au sens du moteur déterministe n'est pas touché ; seule `Editing` gagne un helper.)

**Alternatives écartées.**
- *Trois champs (garder les deux + ajouter « Commande »)* — ambiguïté sur lequel fait foi ; le champ unique
  **remplace**.
- *Le champ unique = mode shell (`zsh -c "toute la ligne"`)* — **décision de modèle distincte**, hors
  périmètre. Pour une gate déterministe, l'invocation directe est l'aligné ; le shell reste exprimable via
  `/bin/sh` en 1er token (`CommandLine` le parse : l'arg cité survit), et aura son propre toggle le jour venu.
- *Split tête/reste en pure glu VM (zéro Core)* — les cas limites (vide, binaire cité, round-trip) sont un
  invariant méritant le Core ; le seul bout testable de l'incrément.
- *Réécrire le tokeniseur dans `CommandLine`* — `ArgumentLine` fait déjà le travail fin ; dupliquer le ferait
  diverger.

**Conséquences.** `CommandLine` + 12 tests (Core 266 → 278) ; câblage App **non testé** (§7.12, validé main).
Suite **295 → 307**, 0 warning. La béquille `/bin/sh` n'est pas interdite — elle devient inutile. **Jambe 1
close** : ses trois dettes encaissées + l'authoring naturel.

**Renvoi** : `D-024` (`ArgumentLine`, le patron du champ local), `D-021`/`D-020` (le vide toléré), `D-028`
(jambe 1, la brique précédente) ; `architecture.md` (sous-couche `Editing`, l'éditeur) ; `trajectoire.md`
(jambe 1 close).

---

## D-030 — L'`AgentStep` headless : kind polymorphe, exécuteur par type, harness nommé

**Statut** : Tranché et construit (Core) — 2026-07-24. UI d'authoring (2·1b) et dogfood réel à suivre.

**Contexte.** La jambe 2 (boucle agentique) s'ouvre par son premier vrai type d'étape non-script.
La recette de `architecture.md` §5 était écrite mais non construite, et elle laissait trois questions
ouvertes que cette marche tranche d'un coup : *comment* coudre l'agent (§2.2), *comment* modéliser un
2e kind, et *dans quel ordre* (Task avant Agent, disait §5).

**Décision.** Quatre arbitrages, dans l'ordre où ils commandent le reste.

1. **Headless d'abord, PTY reportée.** Le premier `AgentStep` lance un agent en mode non-interactif
   (`claude --model … -p …`, tubes redirigés, code de sortie) : c'est un process au contrat
   `ScriptResult`, donc **routable par les gardes existantes** sans généraliser `Guard`, et
   `ExecuteAsync` ne change pas de logique. La couture PTY de §2.2 (session vivante, le différenciateur
   §1) est **reportée à « plus tard indéterminé »**. Ce qui rend la boucle `Verify → Dev` (§3.1) vivante
   contre le dépôt Cursus tout de suite, sur un socle éprouvé.

2. **Le kind est un *type*, pas un discriminant-propriété.** `StepDefinition` devient **abstraite** ;
   `ScriptStep` et `AgentStep` en héritent, chacun ne portant que ses propres propriétés, non-nulles. Le
   `kind` (`"agent"`/`"script"`, absent = script par retombée) vit **dans le document JSON seulement** :
   le sérialiseur, en adaptateur, construit le bon sous-type. **Supersède** le « discriminant `StepKind` »
   littéral de §5, au titre de la convention de modélisation inscrite dans `CLAUDE.md` (pas de nullable
   pour distinguer des variantes de type).

3. **L'exécution vit dans un exécuteur par type, pas sur la définition.** `IStepExecutor` réalisé par
   kind (`ScriptStepExecutor`, `AgentStepExecutor`), chacun tenant **ses propres** collaborateurs ; le
   moteur route sur le *type* de l'étape (`CanExecute`) et délègue. La définition reste **donnée pure** —
   le vocabulaire racine ne dépend pas de la couche Execution. C'est le pari central du pivot (§3, §5)
   payé au réel : greffer le kind n'a **pas touché la boucle de traversée**, seulement allongé la liste
   d'exécuteurs du moteur (câblés dès son ctor, l'agent est un kind de première classe). 2·2 (`TaskStep`)
   ajoutera son `TaskStepExecutor` + son client tracker **sans toucher le moteur ni le vocabulaire**.

4. **Le harness agentique est un concept nommé, pas un enum Claude.** `AgenticHarness` porte son `Name`
   (« Claude Code ») et sa liste de `Models` (`AgentModel` = `Id` + `Label`) ; l'`AgentStep` ne le
   référence que par identifiants, le catalogue racine (`AgenticHarness.ClaudeCode`) en est la source, et
   l'**invocation** réelle vit dans l'exécuteur. Donnée déclarée d'un côté, comportement d'exécution de
   l'autre.

**Ce que ça supersède.** Le « discriminant `StepKind` » et l'ordre `Script → Task → Agent` de §5. L'ordre
retenu est **`Agent → Task`** : `TaskStep` dépend d'un client tracker (Linear) que la trajectoire a écarté
d'ouvrir en premier ; l'agent, lui, ne dépend que du noyau déterministe déjà là.

**Alternatives écartées.**
- *Une méthode `StepDefinition.CreateNewRun(ctx)` sur la définition (polymorphie « le type se lance
  lui-même »)* — élégante en apparence, mais elle ferait **dépendre le vocabulaire racine de la couche
  Execution** (le `ctx` porte runner, harness, puits), et forcerait un contexte fourre-tout grossissant à
  chaque kind. La séparation définition/exécution du pivot vaut mieux.
- *Un `PtyProcessRunner` derrière `IProcessRunner` (couture §2.2 n°1)* — force un PTY interactif dans un
  contrat non-interactif (pas de code de sortie fiable sans convention). Reportée avec la session PTY.
- *Un enum `ClaudeModel` en dur* — ancre Claude dans le vocabulaire ; le concept `AgenticHarness` nommé
  laisse la place à un 2e harness sans refonte du modèle.
- *Un binaire `cursor-agent` externe piloté comme un script* — pas de type du tout, mais alors l'agent
  n'est qu'un `ScriptStep` déguisé : perd la donnée « prompt + modèle » que §4.9 veut voir dépendre de la
  sortie d'avant (2·3), et la validation propre au kind.

**Conséquences.** Core **278 → 287** (+9 : sérialiseur, validation ×2, exécuteur ×2, moteur ×2, retombée).
Suite **307 → 316**, 0 warning. Deux natures de validation neuves (`EmptyAgentPrompt`, `UnknownAgentModel`).
Le format JSON gagne `kind` + charge `agent` (retombée qui garde valides les fichiers d'avant). Restent hors
marche : l'UI d'authoring (2·1b), le vrai binaire `claude` (dogfood manuel), les références inter-étapes
(2·3), la session PTY.

**Renvoi** : `architecture.md` §5 (la recette, désormais construite pour l'agent), §2.2 (couture headless
tranchée, PTY ouverte), §4.1 (carte des fichiers) ; `CLAUDE.md` (convention Modélisation) ; `trajectoire.md`
(jambe 2) ; `D-012` (le fan-out/join à rouvrir avec l'agent), `D-029` (jambe 1, la brique précédente).

## D-031 — L'authoring d'une étape-agent : portes sœurs au brouillon, ligne d'éditeur polymorphe

**Statut** : Tranché et construit — 2026-07-24. Valide 2·1b, ouvre l'authoring de l'agentique à l'écran.

**Contexte.** D-030 avait donné au Core l'`AgentStep` (donnée, exécuteur, sérialisation, validation) et le
moteur la route déjà. Mais rien ne savait en *composer* une : `WorkflowDraft` ne construisait que des
`ScriptStep`, et l'UI ne montrait que le champ commande. Une étape-agent posée à la main dans le JSON
s'éditait et se lançait, mais l'éditeur restait muet sur ce kind. 2·1b comble ce trou — le dernier verrou
avant le dogfood du vrai binaire `claude`.

**Décision.** Quatre choix, chacun un prolongement d'un idiome déjà tenu plutôt qu'une forme neuve.

1. **Le brouillon construit l'agent par des portes sœurs.** `AddAgentStep(name)` jumelle `AddStep` (même
   `Uniquify(Slug.From(name))`, mais l'étape naît confiée au seul harness connu, `AgenticHarness.ClaudeCode`,
   sur son premier modèle, prompt vide — brouillon permis). `SetPrompt`/`SetModel` jumellent `SetScript` :
   même choke `IndexOf` (le sujet doit exister), et le **cast sur `AgentStep`** garde l'invariant de kind
   comme le cast sur `ScriptStep` le gardait déjà. Aucun type neuf au Core — juste trois opérations sur la
   surface d'édition existante.

2. **La ligne d'éditeur devient un type par kind — la convention no-nullable jusque dans la vue.**
   `StepEditorRow` passe **abstraite** (le commun : titre, id, arêtes, tracé/suppression) ; `ScriptStepRow`
   ne porte que sa commande, `AgentStepRow` que son modèle + son prompt, chacun non-nul. Une fabrique
   statique `StepEditorRow.For(step)` dispatche par type — le VM ne connaît plus les kinds. La règle de
   `CLAUDE.md` (pas de nullable pour distinguer des variantes de type) s'applique **au-delà du domaine**,
   dans une VM §7.12 : une ligne « script *ou* agent » est une variante de type, pas un objet à
   `Command?`/`Prompt?`/`Model?` + booléen.

3. **Le flush au save devient polymorphe.** `StepEditorRow.Flush()` abstraite ; chaque sous-type repousse
   ses champs locaux (`Save` boucle `row.Flush()`). Cela **retire** l'ancienne `FlushScripts` qui appelait
   `SetScript` sur *toutes* les lignes — laquelle aurait levé `InvalidCastException` sur une ligne-agent
   dès qu'un agent coexiste avec un script. Le dispatch par type appartient au type, pas à un `switch` dans
   le VM.

4. **Le kind se choisit à la création, figé ensuite.** Un `ComboBox` « Script / Agent » à côté du champ
   Titre route `AddStep` vers `AddStep`/`AddAgentStep` du brouillon. Changer le kind d'une étape = la
   supprimer + recréer.

**Ce que ça supersède.** Rien de tranché ; cela **construit** l'anticipation de D-030 (« l'UI d'authoring
2·1b à suivre ») et de la note de `SetScript` (« le jour d'un `AgentStep`, une opération sœur prendra le
relais » — c'est fait).

**Alternatives écartées.**
- *Un `AddStep(name, kind)` paramétré par enum* — le kind est un **type**, pas un drapeau ; passer un enum
  trahirait la convention no-nullable/no-discriminant. Les portes sœurs sont l'idiome du dépôt
  (`Read`/`ReadEditable`, `Load`/`Open`, `Create`/`CreateFromTitle`).
- *Un `SetAgent(id, modelId, prompt)` unique* — deux contrôles UI indépendants (dropdown + zone de texte)
  qui poussent ensemble = le piège « Appliquer » de D-024. Deux setters, un par contrôle.
- *Garder un `StepEditorRow` unique à champs nullables + booléen `IsAgent`* — laisse représentables des
  états illégaux et force l'AXAML à toggler des visibilités sur des nullables. Proscrit par `CLAUDE.md`.
- *Convertir le kind en place (bascule par ligne)* — exigerait une op « changer de kind » sur le brouillon
  et compliquerait la re-projection, pour un geste rare. **Décision utilisateur** : kind figé à la création.

**Conséquences.** Core **287 → 292** (+5 : construction agent + les deux gardes `IndexOf`). Suite
**316 → 321**, 0 warning. Côté App (§7.12, non testé, validé à la main — agent ajouté à un workflow réel,
round-trip modèle + prompt vérifié) : deux ViewModels de ligne neufs, `StepEditorRow` abstraite, dropdown de
modèle à `SelectedValueBinding` (Avalonia 12, affiche le libellé, retient l'id). Reste hors marche : le vrai
binaire `claude` (dogfood), les références inter-étapes (2·3), la session PTY.

**Renvoi** : `architecture.md` §7.12 (la ligne d'éditeur polymorphe), §4.1 (les VM de ligne) ;
`CLAUDE.md` (convention Modélisation, désormais illustrée jusque dans la vue) ; `D-030` (le Core de
l'agent), `D-024` (le piège « Appliquer » évité), `D-021` (`AddStep`/`Slug`, dont `AddAgentStep` est le
jumeau).

## D-032 — La couture tracker en Core : `TaskStep`, sa clé par le contexte, son geste par le stub

**Contexte.** La jambe 2 (boucle agentique) réclame le 3e `StepKind`, le `TaskStep` (§7.10.4), pour que
Cursus consomme une vraie tâche d'un tracker et referme sa carte. L'utilisateur a tranché : cible **Linear**,
ambition **aller-retour complet** (déplacer la carte en entrée, poser l'étiquette en sortie). Cette entrée
couvre **2·2a** : la couture posée **dans le Core**, TDD contre un tracker stub — aucun réseau, aucun secret.
Le client Linear réel (**2·2b**) suivra dans un **projet dédié hors Core** (miroir de `Cursus.Persistence`,
décision d'emplacement actée avec l'utilisateur), pour que la dépendance HTTP/GraphQL ne franchisse pas la
frontière de `Workflows/`.

**Ce qui est tranché.**

1. **La clé de tâche atteint l'exécuteur par un `StepExecutionContext`.** `IStepExecutor.ExecuteAsync` ne
   reçoit plus un `workingDirectory` nu mais un contexte `{ WorkingDirectory, TaskKey? }`. La tâche visée
   n'est **pas** dans la définition (qui reste portable, §7.3) : elle vient du `RunTrigger` du run, que le
   moteur thread désormais dans `TraverseAsync` et pose sur le contexte de chaque visite. Seul le
   `TaskStepExecutor` lit la clé ; Script/Agent lisent `context.WorkingDirectory` (comportement inchangé,
   cascade mécanique). C'est aussi le **foyer futur des références `${ref.output}`** (§4.9) — la sortie
   d'une étape précédente y viendra sans nouvelle rupture de signature.

2. **Un seul `TaskStep`, portant une `TaskOperation` variante-de-type.** `abstract record TaskOperation` +
   `ReadTask` / `MoveCard(Column)` / `ApplyLabel(Label)`, chacune ne portant que sa donnée, toutes non-nulles.
   Un seul `TaskStepExecutor` route sur le type de l'opération. La convention no-nullable de `CLAUDE.md`
   s'applique à l'opération comme au kind.

3. **`ReadTask` écrit `TASK.md` dans le worktree ; l'agent le lit par convention.** Le corps de la carte
   descend dans le système de fichiers du run (la mémoire partagée entre étapes, §4.9), le prompt de
   l'`AgentStep` disant « lis `TASK.md` ». Pas de machinerie `${ref.output}` pour ce premier pont — la
   convention fichier suffit à fermer la boucle, et la décision 1 garde la porte ouverte pour plus tard.

4. **Une opération sans clé, ou un tracker qui lève, échoue de façon routable — jamais une exception.**
   Un `MoveCard` dans un run manuel (pas de `TaskKey`), un tracker injoignable : `ScriptResult` d'échec,
   visible au journal, routable par une arête de secours, borné par `maxVisits` (§7.10.3). La **définition
   reste valide** — l'absence de clé est un fait de run, pas de graphe (aucune issue de validation pour ça).
   Corollaire : le moteur tient un **tracker optionnel** (ctor), défaut = null-object `UnconfiguredTaskTracker`
   qui refuse tout geste, de sorte qu'une définition à étape-tâche reste lançable **avant que Linear existe**.

**Ce que ça supersède.** Rien de tranché ; cela **construit** ce que §7.10.4 annonçait (« `TaskStep` reste
le cobaye idéal du prochain passage »), et **avance** la note « il faudra rouvrir les références pour
l'`AgentStep` » (§4.9) en posant leur véhicule (`StepExecutionContext`) sans encore les câbler.

**Alternatives écartées.**
- *Un 5e paramètre `string? taskKey` sur `ExecuteAsync`* — plus petit diff, mais fige la signature à *une*
  donnée de run ; le contexte-objet accueillera les références sans nouvelle rupture d'interface.
- *La convention fichier/env pour la clé* (le moteur l'écrit, le step la relit du disque) — hacky pour une
  donnée que le moteur tient déjà structurée.
- *Trois kinds séparés* (`TaskReadStep`/`TaskMoveStep`/`TaskLabelStep`) — la convention no-nullable vise les
  variantes ; `TaskOperation` en *est* une, et §7.10.4 dit « un exécuteur de plus » (singulier).
- *`MoveCard`/`ApplyLabel` par deux champs nullables sur `TaskStep`* — états illégaux représentables, proscrit
  par `CLAUDE.md`.
- *Câbler `${ref.output}` maintenant* — le construire avant d'en avoir prouvé le besoin ; reporté-non-écarté.

**Conséquences.** Core **292 → 303** (+11 : exécuteur 5, moteur 1, sérialisation 3, validation 2). Suite
**321 → 332**, 0 warning. Aucune surface : 2·2a est pure Core, l'increment utilisable arrive à 2·2b/c.
Nouveaux types : `TaskStep`, `TaskOperation`, `ITaskTracker`, `TaskCard`, `StepExecutionContext`,
`TaskStepExecutor` (+ le null-object `UnconfiguredTaskTracker`). Deux valeurs d'enum : `EmptyTaskMoveColumn`,
`EmptyTaskLabel`. Le journal n'a **pas** bougé — `exit_code` n'a jamais été promu en colonne (§7.10.4), le
payload de `StepFinished` d'une tâche s'ajoutera en branche quand 2·2b/c le réclamera.

**Renvoi** : `architecture.md` §5 (le 3e kind parcouru), §7.10.4 (le `TaskStep` natif, désormais en
construction), §4.9 (`StepExecutionContext` amorce des références) ; `trajectoire.md` §Jambe 2 ; `D-030`
(le Core de l'agent, précédent parcours de la recette), `D-031` (l'authoring agent, dont 2·2c reprendra le
patron de ligne polymorphe pour le `TaskStepRow`).

---

## D-033 — L'écran avant le geste : la jambe 2·2 ré-ordonnée, et le trousseau en Core

**Contexte.** `D-032` a posé la couture tracker en Core et annonçait la suite : `2·2b` le client Linear
réel, `2·2c` l'authoring UI (`TaskStepRow`), `2·2d` la boucle bout-en-bout. Le plan d'authoring était
rédigé et prêt quand l'utilisateur a posé la question qui l'a renversé : *« si on ne sait pas lister les
tâches disponibles, on va avoir un problème sur quelle tâche déplacer, non ? »*

La vérification lui a donné raison, et durement : **`RunTrigger.ForTask` n'a aucun appelant en
production**. `RunViewModel` lance sans déclencheur, donc `TaskKey` est toujours nul, donc
`TaskStepExecutor` refuse tout geste (« Aucune tâche associée à ce run »). Soigner l'authoring d'une
opération dans cet état, c'était **meubler une pièce sans porte** : une étape composable et
structurellement inerte.

**Ce qui est tranché.**

1. **L'écran des tâches précède le geste.** `2·2b` devient *trousseau → client Linear en lecture →
   écran* ; `2·2c` rebranche `RunTrigger.ForTask` (« lancer ce workflow sur cette tâche ») ; l'authoring
   de l'opération n'arrive qu'en `2·2d`. Cet ordre **honore le principe ordonnateur de la trajectoire** —
   chaque jambe utilisable dès qu'elle est posée : voir son tableau dans Cursus rend service sans un seul
   run, là où une étape-tâche inerte ne rend rien.

2. **Ce n'était pas une découverte, mais un oubli.** `architecture.md` §7.10.4 le disait déjà — *le client
   Linear existe de toute façon, puisque calculer l'écran des actions disponibles impose d'interroger le
   tableau* — et le jalon 7 du §9.4 nomme **l'écran** avant le geste. C'est la décomposition de
   `trajectoire.md`, écrite plus tard, qui avait égaré l'écran en route. Leçon consignée : quand une
   décomposition fine contredit en silence une section ancienne du document d'architecture, c'est la
   décomposition qu'il faut soupçonner en premier.

3. **La question « quelle tâche ? » n'est pas un trou de modèle mais un trou d'alimentation.** La réponse
   est tranchée depuis §7.3 : la clé vient du `RunTrigger` du run, **jamais de la définition**, pour que
   celle-ci reste portable. Mettre la clé dans l'étape aurait « réglé » le problème en cassant cette
   portabilité. Ce qui manquait n'était pas un champ, c'était un **chemin d'alimentation** — d'où l'écran,
   qui est précisément ce qui remplira le déclencheur.

4. **Le trousseau vit dans le Core, en `Secrets/`** (`ISecretStore` + `KeychainSecretStore`). Le « zéro
   dépendance externe » de `Workflows/` (§1.2) vise les **paquets NuGet**, pas les binaires du système :
   `ProcessRunner` lance déjà des process. S'adosser à `/usr/bin/security` respecte donc la propriété tout
   en suivant la convention d'I/O du dépôt — et rend l'adaptateur **véritablement testé** plutôt que
   simulé. La clé est opaque au port ; sa convention `<provider>:<workspace>` **ne porte pas le projet**,
   le token appartenant au compte (§7.10.1).

5. **Deux ports, pas un** (tranché, construit en `2·2b·2`). `ITaskTracker` est le port du **geste**,
   collaborateur du `TaskStepExecutor` ; lister est un besoin de **surface**. Un `ITaskBoard` sœur portera
   la **requête**. **Écarté** : un port unique « tout tracker » — séduisant parce qu'un seul adaptateur
   Linear se trouve derrière, mais c'est confondre l'implémentation (une classe) avec le contrat (deux
   consommateurs aux besoins disjoints) ; tout stub d'exécution devrait alors implémenter une requête dont
   il n'a que faire. C'est le patron de la **porte sœur** déjà employé au `D-020`.

**⚠️ Gotcha payé au rouge.** `security find-generic-password -w` rend la valeur **en hexadécimal** dès
qu'elle contient un octet hors ASCII imprimable — tabulation, saut de ligne, ou un simple **accent** —
sans préfixe ni signal. La valeur remonte donc *silencieusement fausse*, ce qui est pire qu'une erreur, et
c'est **indétectable à la relecture** : un secret qui serait littéralement une chaîne hexadécimale (un
hash, une clé) est indiscernable de la forme encodée. D'où le choix de ne jamais laisser `security`
arbitrer — on range du base64, toujours imprimable. Contrepartie assumée : valeur illisible à l'œil dans
« Trousseaux d'accès », et un secret déposé à la main hors de Cursus ne se relit pas.

**Écarté.** Le **repli sur fichier en clair** quand le trousseau est indisponible (§7.10.1 le disait
déjà) — *un fallback silencieux est exactement la façon dont les secrets finissent commités*. Écarté aussi,
le **token en variable d'environnement** pour amorcer le client plus vite : la jambe 1 a déjà payé le prix
d'une béquille (`/bin/sh -c`) qu'il a fallu retirer ensuite.

**Renvoi** : `architecture.md` §7.10.1 (trousseau, désormais construit ; le gotcha `security`),
§7.10.4 (l'écran des actions disponibles, qui l'annonçait), §9.4 jalon 7 ; `trajectoire.md` §Jambe 2·2
(la décomposition ré-ordonnée) ; `D-032` (la couture qu'elle prolonge), `D-020` (le patron de la porte
sœur), `D-031` (l'authoring agent, dont `2·2d` reprendra le patron de ligne polymorphe).

---

## D-034 — Une connexion, pas un espace : le jeton se constate avant de se ranger

**Contexte.** La marche `2·2b·3a` devait offrir une UI de saisie du jeton, l'espace Linear restant en
dur (`cursus-app`) et la clé de trousseau valant `linear:<espace>` — forme héritée du `D-033`. Le plan
était écrit, son schéma-delta aussi.

**Ce qui l'a renversé.** Une remarque de l'utilisateur au moment de valider : *une clé Linear couvre soit
tout le compte, soit un projet*. Trois conséquences, dont une correction de sûreté.

1. **Plusieurs connexions coexistent** — l'UI ne peut pas être un formulaire à un jeton. Le registre
   devient une **liste**.
2. **La clé de trousseau `linear:<espace>` est un défaut**, pas une simplification : deux connexions
   vers le même espace (une clé de compte, une clé de projet) s'y seraient **écrasées mutuellement, en
   silence**. Elle devient `tracker:<id de connexion>`, l'identifiant étant attribué par le registre —
   et la convention vit sur `TrackerConnection.SecretKey`, parce que laisser chaque appelant composer
   cette clé, c'est laisser deux d'entre eux la composer différemment.
3. **La portée d'un jeton n'est pas déclarable, elle est constatable.** On ne demande donc plus l'espace
   du tout : on colle le jeton, on l'éprouve, et on montre ce qu'il donne à voir. La notion d'espace
   **disparaît du modèle** — ce que le plan initial n'aurait jamais atteint en partant d'un champ à
   remplir.

**Le défaut que l'UI a révélé en amont.** Sonder un jeton invalide rend **401** ; le client en faisait un
`TrackerUnreachableException`. Diagnostic faux *et* remède faux — on part vérifier son réseau pendant que
sa clé est révoquée. D'où `TrackerRejectedException`, et l'extraction du verdict d'échec dans
`LinearFailure` (pur, testé sur des corps réels). Constat général : **c'est la première marche qui
consomme une couture pour de bon qui en révèle les confusions** ; les trois exceptions de domaine
n'existaient que sur le papier tant que rien n'affichait leur différence.

**Tranché.**

- **Registre global**, jumeau de `ProjectRegistry` — un jeton ne dépend d'aucun dépôt. Il n'y a aucune
  raison qu'une machine tienne deux registres selon des règles différentes.
- **`TrackerScope` par le type** (`WholeWorkspace` | `SelectedProjects`), jamais une liste vide valant
  « tout » : une sélection vide serait ambiguë. La portée **sait filtrer**, pour que l'écran des tâches
  et le futur choix d'une tâche à lancer ne divergent pas.
- **Kind inconnu → tout l'espace.** Montrer trop de projets se remarque et se corrige ; une connexion
  muette n'explique rien.
- **Le jeton n'est rangé qu'après l'inscription**, sous la clé qu'elle vient de donner. L'ordre inverse
  aurait obligé, en cas d'abandon ou de refus, à revenir effacer un secret orphelin — un nettoyage qu'on
  oublie une fois sur deux. D'où `TransientSecretStore` (App), qui laisse éprouver sans rien déposer.
- **Le panneau appartient à la coquille**, superposé à la fenêtre entière, et non à la surface d'un
  projet : ce qu'on y configure ne dépend d'aucun projet ouvert.

**Superséde.** `D-033` sur deux points : la clé de trousseau (`linear:<workspace>` → `tracker:<id>`) et
`TrackerNotConfiguredException(workspace)`, désormais sans paramètre — la connexion est connue de qui
appelle.

**Revient sur.** L'écart de `DeleteAsync` prononcé quelques heures plus tôt dans la même marche : il
manquait un besoin, le retrait d'une connexion l'a apporté. Sans effacement, le trousseau accumule des
secrets que plus rien ne désigne. Idempotent à dessein — l'effacement sert aussi à rattraper un échec de
configuration, et lever alors ferait échouer le rattrapage lui-même.

**Écarté.** Cocher les projets **avant** de connaître le jeton (impossible : c'est le jeton qui détermine
ce qui est visible) ; une vraie **fenêtre de préférences ⌘,** (probablement la forme finale, mais une
fenêtre et son cycle de vie pour un panneau qui ne porte encore que le tracker, c'est payer d'avance) ;
**persister le lien projet Cursus ↔ connexion** maintenant — forme devinée tant que l'écran des tâches ne
l'a pas réclamée.

**Renvoi** : `architecture.md` §7.10.1 (les connexions, la clé par connexion), `docs/reference/linear-api.md`
§6bis (les trois formes d'échec sondées) ; `D-033` (ce qu'elle supersède), `D-021` (l'attribution d'id par
le propriétaire de l'unicité, même geste qu'`AddStep`), `D-016` (le panneau comme module de coquille).

---

## D-035 — Une clé = un espace : le genre d'une connexion est un type, pas un champ

**Contexte.** `D-034` faisait cocher des **projets** Linear à la configuration d'une connexion. La
validation manuelle l'a rejeté : le périmètre utile est le **workspace** — un projet Linear est une
*epic* au sens Jira, un cran trop bas.

**Ce que la sonde a tranché.** Le schéma Linear n'expose `organization` qu'au **singulier** ;
`organizations` est refusé par la validation GraphQL. Une clé est donc attachée à **exactement un
espace**, déterminé à sa création. La question « quel espace ? » n'était pas mal réglée : elle
n'existait pas. Il n'y a rien à sélectionner, seulement à **constater** — ce que `D-034` avait déjà
pressenti pour la portée, sans aller jusqu'au bout : la même phrase valait un cran plus haut.

**Supprimé.** `TrackerScope` et son filtrage. Sans choix, plus de portée à modéliser, à sérialiser ni
à faire survivre au redémarrage. Épreuve d'un jeton également simplifiée : `{ organization { … } }`
valide la clé *et* dit ce qu'elle dessert, là où on listait tout le tableau pour la seule satisfaction
de savoir si l'on était connecté — pour une fraction du budget de complexité.

**Tranché — `TrackerConnection` devient une variante par le type.** Ce n'est pas la symétrie avec
`StepDefinition` qui l'impose, c'est son absence de solution de rechange : le générique aurait porté un
`Workspace`, concept purement Linear, dans le type que Jira utilisera aussi — un site Atlassian n'est
pas un workspace, un `owner/repo` GitHub non plus. On serait retombé sur des champs optionnels
mutuellement exclusifs. Chaque sous-type ne porte donc que ce qui identifie une connexion *chez son
tracker*. `LinearConnection` vit **en Core** bien que l'adaptateur HTTP vive dans `Cursus.Trackers` :
c'est de la donnée, au même titre qu'`AgenticHarness.ClaudeCode` nomme un harnais concret sans
l'implémenter (`D-030`).

**Conséquence sur le registre.** `Add` reçoit un **constructeur** (`id => connexion`) au lieu d'un
couple de valeurs : le registre garde ce dont il répond — l'unicité de l'identifiant, qui désigne le
jeton au trousseau — et laisse à l'appelant ce que lui seul sait, le genre à bâtir. Même partage qu'au
`D-021`, où l'unicité d'un id appartient à qui la tient.

**Renversé depuis `D-034`.** Un `kind` inconnu au chargement est désormais **ignoré**, là où la portée
retombait sur une valeur par défaut. La nuance tient à ce qui est représenté : une portée approximative
reste utilisable, une connexion dont on ignore le tracker ne l'est pas — en fabriquer une ferait
échouer chaque usage sans dire pourquoi.

**Écarté.** La sélection des **équipes** Linear (`teams`), seul découpage réel sous l'espace : aucun
besoin ne la réclame, et la découpe polymorphe la rend bon marché le jour venu — elle se posera sur
`LinearConnection` seule, sans toucher au générique. Écarté aussi, **graver le lien un-pour-un** entre
connexion et projet Cursus : Linear l'est par nature, mais c'est une propriété de Linear et non du
modèle, et Jira range plusieurs projets sous un même site.

**Ce que la marche enseigne.** Deux renversements en une journée, tous deux venus de la même source :
la confrontation d'un modèle à l'API réelle et à l'usage réel. Le premier (`D-034`) a supprimé la
notion d'espace d'un plan qui voulait la faire saisir ; le second supprime la notion de portée d'un
modèle qui voulait la faire choisir. **Un modèle qui demande à l'utilisateur ce que le système peut
constater est presque toujours un modèle en avance sur ce qu'il sait.**

**Renvoi** : `architecture.md` §7.10.1 (les connexions, désormais polymorphes),
`docs/reference/linear-api.md` §2bis (une clé = un espace) ; `D-034` (ce qu'il révise), `D-030`
(le concept concret nommé en Core), `D-031` (le patron de ligne polymorphe, repris pour l'affichage),
`D-021` (l'unicité tenue par qui en répond).

---

## D-036 — Un artefact par niveau, un régime par nature de jugement : l'humain remonte vers la spec

**Contexte.** `docs/methode/tickets.md` (`4251057`, `ca2b800`) posait trois niveaux — feature /
incrément / pas — et une matrice statut × niveau. Une passe de clarification menée avec l'utilisateur
(2026-07-25) a montré qu'il lui manquait trois choses : **quel artefact** chaque niveau produit, **qui
juge** cet artefact, et un flux qui soit celui de l'espace plutôt que celui d'usine.

Écarté d'emblée comme méthode : **confronter le gabarit aux tickets existants**. Ils ont été écrits à
partir de ce même fichier, par le même auteur ; la comparaison n'aurait mesuré que la fidélité de la
copie.

**Tranché — un artefact par niveau, et ce ne sont pas trois formes du même « plan ».** La **feature**
produit une **spec**, la **story** un **plan d'archi** (celui de `CLAUDE.md`, avec son schéma-delta),
le **pas** une **test list**. Ils n'ont ni le même régime de fraîcheur ni le même moment d'écriture :
le plan d'archi est écrit **une fois, au découpage**, par qui a la vue d'ensemble ; la test list se
génère **à la prise du pas**, et reste vivante — en TDD, un cas découvert au rouge s'y ajoute.

Ce que ça corrige : §3 du document interdit la test list au backlog (« *elle naît du plan, pas du
backlog* »). Juste pour une story — qui énumérerait ses cas aurait mangé le plan. **Faux pour un pas**,
dont la test list est le contenu même. La règle vise l'amont du découpage, pas la test list en soi.

**Tranché — le lecteur du test de départage est le rôle produit.** « *Si on le livrait seul, quelqu'un
le remarquerait-il ?* » laissait chacun choisir son lecteur : l'utilisateur de l'app, le développeur du
lendemain, l'agent. C'est le **PM** — une story est *recettable par un rôle produit*. Le test n'était
pas trop sévère (un client Linear en lecture seule reste un pas), il était **sous-spécifié**.

**Renversé — le rendez-vous humain n'est plus sur le chemin.** §6 affirmait que `Plan Review` et
`QA Review` sont *les deux endroits où le travail s'arrête et attend un humain*, et qu'un agent ne
pousserait jamais jusqu'à `Done` « **par construction et non par prudence temporaire** ». Cette
formule est **révisée** : `Plan Review` et `Code Review` sont des **boucles agent ⇄ agent** (agent de
plan contre agent de revue), et l'humain n'est convoqué qu'en **arbitre d'exception**, après deux ou
trois tours sans accord. Il n'est plus sur le chemin, il est sur la sortie de secours.

L'escalade ne demande aucun mécanisme neuf : **escalader, c'est s'assigner la carte**. Une carte en
revue non assignée boucle ; assignée, elle attend un humain. Pas de colonne, pas d'étiquette.

**Tranché — la frontière n'est pas de niveau, elle est de nature de jugement.** La ligne cherchée
n'était pas entre story et pas. Est **délégable à un désaccord entre agents** tout ce qui a un
référentiel opposable : le plan contre l'architecture, la test list contre le comportement attendu, le
code contre le standard — deux agents peuvent converger parce qu'il existe quelque chose contre quoi
trancher. Reste **irréductiblement humain** ce qui n'en a pas : la **spec** (aucun agent ne juge que
c'est *ça* qu'on veut construire) et **l'œil** de la validation de présentation (§7.12).

**Tranché — la spec se produit à trois, et le producteur ne se valide pas.** Un **binôme
humain ↔ agent** rédige (l'agent réduit les angles morts et accélère le raisonnement) ; un **agent de
revue distinct** valide. La séparation production / validation tient, avec l'humain **du côté de la
production**. Corollaire à ne pas perdre : un agent de binôme à qui l'on demande un verdict **le
donnera** — un faux accord est pire qu'aucune relecture, puisqu'il donne le sentiment d'avoir été
contredit. Sa posture est celle du régime de *Vérification* de `CLAUDE.md` : lister les divergences,
ne pas trancher. Et **pas d'escalade à ce niveau** : si le relecteur refuse, l'humain est déjà dans la
pièce. Le compteur de tours ne concerne que `Plan Review` et `Code Review`.

Régime : **tranché, éprouvé ailleurs, pas ici.** Le dispositif a tourné sur quelques tickets hors de
ce dépôt ; aucun agent n'a encore parcouru une boucle sur `cursus-app`.

**Tranché — chaque niveau se recette contre son propre artefact.** Le pas contre sa **test list** (le
vert), la story contre son **acceptation** (`QA Review`), la feature contre sa **spec**
(`Validation`). C'est ce qui empêche `Validation` d'être redondante avec les QA déjà passées :
**toutes les stories peuvent être vertes sans que la capacité promise soit là**. Corollaire qui engage
la rédaction : la spec n'est pas un document d'intention, c'est **le contrat contre lequel on
recettera**.

**Corrigé — le flux des features était celui d'usine.** Le document décrivait
`Backlog → Planned → In Progress → Completed` : ce sont les **noms par défaut de Linear**. Le flux réel
est `Backlog · Discovery · Spec · In Progress · Validation · Completed`.

La frontière `Discovery` | `Spec` n'est pas un moment du raisonnement mais un **changement de
composition** : `Discovery` réunit produit et UX autour de la seule question *à quel besoin
répond-on ?*, quitte à ouvrir sur des solutions possibles ; `Spec` y ajoute **la tech et la QA**, et
c'est pourquoi **l'arbitrage y vit** — on n'arbitre pas une faisabilité sans la tech. Garder
`Discovery` à part, c'est se réserver le droit de **tuer une feature avant d'avoir dépensé le moindre
arbitrage technique**. Et la QA à la table de `Spec` est **d'où descend l'acceptation** : elle définit
comment la feature se recettera, le découpage répartit ensuite cette recette entre les stories.

**Tranché — `Backlog` porte deux fonctions selon le niveau.** Au niveau **projet**, c'est le début du
flux nominal. Au niveau **issue**, c'est une **salle d'attente** : une story *éligible* n'y passe pas,
puisqu'elle naît en `Todo` au découpage de sa feature. Y séjournent celles qui attendent une dépendance
— et surtout celles qui **n'ont pas de parent** : le refacto qu'aucune fonctionnalité ne tire, la dette
autonome, plus les stories explicitement déportées d'un découpage. C'est **l'entrée latérale du
backlog**, la seule voie par laquelle un travail arrive sans passer par une spec, et elle règle le sort
du refacto orphelin qui n'avait jusqu'ici aucune porte.

**Conséquence — le ticket cesse d'être un brief pour devenir un lieu de dialogue.** On le pensait en
entrée (le contexte) et en sortie (l'acceptation) ; la boucle en fait aussi le **journal d'une
négociation**. Trois exigences en découlent, qui n'existent nulle part aujourd'hui : un **verdict
structuré** (accord / désaccord *et le point en litige* — de la prose ne se compte pas d'un tour à
l'autre) ; un **compteur de tours** ; un litige **reconstituable en une minute** par qui arrive sans
avoir suivi la boucle — sans quoi l'escalade coûte plus cher que d'avoir relu dès le début.

**Acquis sans effort, notés pour ne pas être redécouverts.** (1) Le **niveau d'une carte se déduit de
sa structure** — projet = feature, issue sans parent = story, issue avec `parentId` = pas : ni
étiquette à maintenir, ni convention à faire respecter, **un contrat machine de moins** dans
`project.json`. (2) **Linear rend le mermaid nativement** (`/diagram`, ou un bloc ` ```mermaid `
collé) : un plan peut vivre dans le document attaché à la carte, schéma-delta rendu. La règle de
`CLAUDE.md` exigeant que le plan « indique son propre chemin de fichier en tout premier » n'existait
que pour compenser l'absence de rendu inline — elle tombe.

**Écarté.** L'**assignation comme discriminant du travail de spec** (proposée quand on croyait qu'il
manquait une colonne entre `Backlog` et le début du travail : `Discovery` et `Spec` existent, la parade
répondait à un problème inventé). Une **colonne de revue de spec** supplémentaire : `Validation` et le
trio suffisent, et une colonne de plus sur un flux de six serait disproportionnée. Le **vocabulaire
epic / US / sous-tâche** : accepté comme équivalent de travail (Jira), mais les mots du dépôt restent
*feature / incrément / pas*, parce qu'« epic » désigne couramment un conteneur thématique sans fin —
exactement ce que la définition « un cap qui se ferme » refuse.

**Reste ouvert.** Distinguer **trois tours sur le même litige** de **trois tours qui dérivent de
sujet** : ils se comptent pareil, ne valent pas pareil, et le second — l'échec grave — ressemble à du
progrès. Relève de la conception d'agent, pas du gabarit. Ouvert aussi : le refacto orphelin a
désormais une entrée, mais **pas de spec, donc pas de recette de niveau feature**.

**Renvoi** : `docs/methode/tickets.md` (§2, §3, §4 et §6, à réécrire sur cette base), `CLAUDE.md`
(§Méthode de développement — le chemin de fichier du plan, la test list rattachée au plan),
`architecture.md` §7.12 (la frontière testé / validé à l'œil, seul jugement qui ne se délègue pas),
§7.10.5 (le contrat machine, distinct de la convention) ; `D-033` (l'autocritique de `CUR-15`, née de
la même frontière ticket / plan).

---

## D-037 — La déclaration est versionnée, le jeton est machine : ce qui rend une divergence visible

**Contexte.** L'écran des tâches (`2·2b·3b`) devait savoir quelle connexion interroger. Le §7.10.1
laissait la question ouverte à dessein — *« le lien se posera quand l'écran aura montré ce dont il a
besoin »*. Il en a eu besoin, et la question qui restait n'était pas *s'il faut un lien* mais **à quel
niveau de stockage il vit**.

**Ce qui a renversé le premier plan.** Ma proposition initiale était de **ne rien persister** : un
sélecteur de connexion en session, auto-choisi s'il n'y en a qu'une, en invoquant la leçon du `D-035`
(ne pas modéliser en avance de ce qu'on sait). Une remarque de l'utilisateur l'a défaite en deux
phrases : *partager les réglages du tracker entre les membres d'une équipe est pertinent, surtout si on
partage les déclencheurs — et ça permet de signifier à l'utilisateur qu'il a connecté quelque chose de
différent.*

Le second point est celui que je n'avais pas vu, et il est décisif. **Un appariement rangé au registre
machine *est* la vérité, donc il ne peut jamais être faux.** Il n'a rien à quoi se comparer. Une
déclaration versionnée, elle, crée un écart **observable** entre ce que le dépôt dit viser et ce que ce
poste sait joindre — exactement la forme d'erreur qui coûte cher autrement : un run déplaçant une carte
dans le mauvais espace sans qu'un mot l'ait annoncé. La persistance n'était donc pas du confort
d'ergonomie qu'on peut reporter ; elle était la condition d'un diagnostic.

Le premier point est un argument de cohérence : les prédicats de disponibilité vivent déjà dans
`project.json` (§7.10.3). Un déclencheur partagé nommant une colonne, dont l'espace où trouver cette
colonne serait un réglage machine invisible, serait à moitié partagé — la moitié relisible en revue, la
moitié devinée.

**Tranché.**

- **Le lien se coupe en deux moitiés** à deux niveaux différents : la **déclaration** (`TrackerBinding`,
  nœud `tracker` de `project.json`, versionnée) et la **connexion** (`TrackerConnection` + jeton au
  trousseau, machine). Ce n'est pas une duplication : ce sont deux faits distincts dont la comparaison
  est le produit utile.
- **`TrackerBinding` est une variante par le type**, comme `TrackerConnection` au `D-035` et pour la
  même raison : ce qui identifie un tableau *chez son tracker* n'est vrai nulle part ailleurs. Le
  générique n'aurait porté que des champs vides. C'est aussi ce qui interdit de graver un lien
  un-pour-un — Linear l'est par nature, mais c'est une propriété de Linear, pas du modèle.
- **`Project.Tracker` est nullable, et c'est légitime.** La convention proscrit le nullable pour
  distinguer des *types d'objets*, pas pour une *valeur qui peut manquer* : un dépôt sans tableau reste
  un dépôt. La variante, elle, est portée par les sous-types.
- **L'appariement sans discrimination**, par deux membres abstraits qui se répondent :
  `TrackerBinding.Matches(connexion)` et `TrackerConnection.ToBinding()`. Deux membres virtuels pour un
  seul sous-type de chaque côté, c'est cher à première vue ; ce qu'on achète est qu'aucun `switch` sur le
  genre de tracker ne remonte dans l'App, et qu'une `JiraConnection` apportera les siens sans que
  l'écran bouge.
- **On apparie sur la clé lisible** (`cursus-app`), pas sur l'identifiant opaque : un fichier versionné
  dont le contenu ne se relit pas en revue perd la raison d'être qui l'y a mis. Contrepartie assumée —
  renommer l'espace rompt l'appariement, ce qui se **signale** comme divergence au lieu de suivre en
  silence. C'est le comportement voulu, pas un défaut.
- **La déclaration s'écrit comme conséquence d'un choix**, jamais par un champ à remplir : le `D-035`
  poussé d'un cran. On montre les connexions, et désigner l'une d'elles l'inscrit (`ToBinding`).

**Le piège trouvé par la relecture, pas par l'usage.** `ProjectRegistry.Rename` renomme depuis son
instantané en mémoire, chargé au démarrage, et `ProjectStore.Rename` réécrivait le document **entier**
depuis ce `Project`. Dès que `project.json` porte une donnée de plus, cette écriture l'efface sans un
mot — on déclare son tableau, on renomme le projet, la déclaration a disparu, et rien ne relie les deux
gestes dans l'esprit de qui les pose. Le remède est un invariant **local** et non une précaution
d'appelant : **un écrivain partiel de `project.json` relit le disque avant d'écrire**
(`ProjectStore.Rewrite`, unique chemin de réécriture et unique point de sérialisation d'un projet
existant). Contrepartie assumée : réécrire exige un document lisible — un `project.json` invalide fait
désormais échouer le renommage au lieu de l'écraser.

**Écarté.**

- **Ne rien persister** (ma proposition) — défaite par l'argument de la divergence ci-dessus.
- **L'appariement au registre machine** (`projects.json` gagnant un `trackerConnectionId`) : plus court
  d'un type, mais rien à partager en revue et **rien à quoi se comparer**.
- **Écrire l'`id` de connexion dans `project.json`** : cet identifiant est un `Guid` attribué par le
  registre *machine*. Versionné, il serait faux chez tout collègue — piège qu'il valait mieux nommer
  que découvrir.
- **Un `TrackerResolver` en Core** qui apparierait par discrimination : il concentrerait au même endroit
  la connaissance de tous les trackers, exactement ce que la découpe par le type disperse.
- **Une ligne d'enrobage pour les tâches de l'arbre** : rien n'y porte d'état à ce stade. Une
  `TaskRowViewModel` viendra au `2·2c`, quand « lancer ce workflow sur cette tâche » lui donnera quelque
  chose à porter — l'introduire maintenant serait deviner sa forme.
- **La délaison** (revenir à « aucun tableau déclaré ») : redéclarer couvre tout besoin connu.
- **Le sondage périodique** de l'écran : il brûlerait le quota d'API en silence, et
  l'auto-déclenchement sur l'état d'une carte reste la question ouverte du §7.10.6.

**Ce que la marche enseigne.** Le `D-035` concluait qu'*un modèle qui demande à l'utilisateur ce que le
système peut constater est presque toujours en avance sur ce qu'il sait*. Cette marche en donne la
borne : **certaines choses ne se constatent pas.** Qu'un dépôt suive tel tableau est une intention
d'équipe, que rien dans l'API ne révèle — la demander est correct, et la partager l'est aussi. La
question utile n'est donc pas « peut-on éviter de demander ? » mais « qui détient la réponse ». Ici :
l'équipe, dans son dépôt.

**Renvoi** : `architecture.md` §7.10.1 (le lien projet ↔ tableau, la question ouverte refermée),
`trajectoire.md` (`2·2b` close) ; `D-035` (la variante par le type, et la leçon dont ceci est la borne),
`D-034` (les deux clés pour une même cible, situation que l'écran doit savoir traiter), `D-033` (l'ordre
« l'écran avant le geste »), `D-024` (la validation manuelle comme seule preuve d'un module §7.12).

---

## D-038 — La méthode est de la donnée du projet : trois lieux, et le plan qui attend sa prise

**Contexte.** Mettre le flux à plat pour savoir *quels skills écrire* (`docs/methode/flux.md`, neuf) a
produit deux choses que `D-036` n'avait pas : où la méthode doit vivre, et une contradiction interne
qu'il fallait lever.

**Tranché — trois lieux, parce que trois choses varient indépendamment.** La **méthode** vit dans un
**skill** (elle diffère par équipe, par composition, par maturité) ; la **chorégraphie** dans le
**workflow** Cursus (quelles étapes, dans quel ordre, routées sur quoi — ce que le noyau déterministe
sait déjà faire) ; le **contexte** dans la **carte**.

C'est le pari central du dépôt d'un cran plus haut : le moteur ne sait pas ce qu'est un agent, il ne
saura pas non plus ce qu'est un découpage, une spec ou une test list. **La méthode est de la donnée du
projet, jamais du code du produit.** Le prompt d'un `AgentStep` cesse d'être un brief pour devenir un
**pointeur** — quel skill, quelle carte.

Conséquence pratique : l'automatisation du flux **ne demande presque aucun développement dans
Cursus**. Claude Code charge déjà `.claude/skills/` du dépôt de travail ; la méthode se versionne avec
le code de l'équipe, se relit en revue, et diverge d'un dépôt à l'autre sans que Cursus ait à
distribuer, stocker ni modéliser quoi que ce soit.

**Corollaire — la question ouverte de `tickets.md` §7 se dissout.** On se demandait si les rappels de
contexte (0 warning, régime TDD, frontière §7.12, no-nullable) devaient vivre *dans chaque carte* ou
*dans l'amorce de l'agent*. **Ni l'un ni l'autre** : ils sont dans `CLAUDE.md`, que Claude Code charge
seul ; la méthode d'équipe est dans le skill ; la carte ne porte que ce qui lui est propre. Reste une
hypothèse jusqu'au premier round-trip réel, mais elle est plus propre que les deux branches posées.

**Assumé — le flux est Claude Code exclusif.** Le contenu d'un skill est du markdown portable, son
mécanisme de chargement ne l'est pas. Tranché avec l'utilisateur : c'est un choix de **l'utilisateur de
Cursus**, garant du sens de ses propres workflows, pas une contrainte que le produit doive abstraire.
`AgenticHarness.ClaudeCode` (`D-030`) nommait déjà un harnais concret ; on assume la même franchise un
étage plus haut.

**Révisé depuis `D-036` — le plan d'archi s'écrit à la prise, pas au découpage.** `D-036` affirmait
qu'il est « écrit une fois, au découpage, par qui a la vue d'ensemble ». Incompatible avec le chemin
que l'espace porte — `Todo → [Planning → Plan Review] → In Progress` place `Planning` **après** la
naissance de la carte. La mise à plat a forcé le choix, et c'est la prise qui l'emporte, pour la raison
qui valait déjà pour la test list : **ce qu'on apprend en faisant le premier incrément change ce qu'on
sait au quatrième**.

## D-039 — Un skill se récolte avant de s'écrire : la ligne de base est le premier livrable

**Contexte.** `D-038` a posé *où* vit la méthode et *quels* skills écrire ; restait *comment* les
écrire. Une recherche en cinq sondes (doc officielle, corpus open source, fiabilité headless,
outillage d'évaluation, travail produit et entretien de corpus) a été menée avant d'en rédiger un
seul. Son matériel est consigné dans **`docs/reference/skills.md`** — matériel externe sondé, avec
chaque affirmation étiquetée *mesuré* / *documenté* / *folklore*, parce que la littérature du domaine
mélange les trois sans prévenir et que quatre conseils universellement répétés s'y sont révélés faux.

**Tranché — l'ordre d'écriture s'inverse.** On n'écrit pas un skill puis on l'éprouve. On **exécute la
tâche sans skill**, on tient un **journal des frictions** — chaque correction, chaque étape sautée,
chaque précision qui aurait dû être sur la carte —, et **le journal écrit le skill**. Deux sources
indépendantes prescrivent exactement cette séquence : Anthropic (*« créez les évaluations AVANT
d'écrire une documentation extensive »*, la ligne de base se mesure **sans** le skill) et ETH Zürich
(arXiv:2602.11988 — *partir d'un fichier vide et ajouter les règles une par une, sur erreurs répétées
observées*).

La raison profonde est qu'un skill écrit d'avance ne peut pas être évalué. Le mode d'échec numéro un
mesuré sur l'outillage officiel est le **100 %/100 %** : sur des cas imaginés, l'exécution avec et sans
skill marque pareil et le signal est nul. Un cas tiré d'un échec vécu discrimine par construction.
C'est la transposition, à la méthode, de la règle que le dépôt s'applique déjà au code : **pas de
production sans un rouge observé qui la réclame**.

**Conséquence sur le premier pas.** `flux.md` §4 proposait `prendre-un-pas` en tête. Le choix tient —
c'est le seul qu'on puisse exécuter à la main sans dépendre d'un autre — mais **la première action
n'est plus d'écrire, c'est d'exécuter**. Terrain retenu : l'incrément **`2·2c`** (« lancer ce workflow
sur cette tâche »), qui ferme la boucle E2E de la jambe 2 et traverse Core et App. Le dogfooding
*est* la ligne de base : le produit avance pendant qu'on récolte.

**Écarté — écrire les huit skills d'affilée.** Huit documents fondés sur des échecs imaginés, dont on
ne saurait pas lesquels sont des *no-op* (une ligne qui ne change rien par rapport au comportement par
défaut, et coûte quand même). L'anti-patron est observable jusque dans le corpus officiel d'Anthropic :
des skills qui ne portent qu'un tableau de définitions, sans instruction ni critère d'arrêt.

**Écarté — commencer par `decoupage`**, plus tentant parce que plus haut dans le flux. Tant qu'aucun
pas n'a été exécuté par un agent, on ne sait pas quelle **maille** de pas est bonne — or c'est
exactement ce que le découpage décide. L'argument préexistait dans `flux.md` §4 ; la recherche le
confirme sans le modifier.

**Écarté — outiller l'évaluation d'abord.** L'outil officiel existe (`skill-creator`, modes
Eval/Improve/Benchmark depuis mars 2026, `evals/evals.json` versionné à côté du `SKILL.md`), mais il
coûte deux exécutions d'agent par cas et n'a pas de CLI — c'est un **rituel de jalon**, pas un test
unitaire. Et 74 % des équipes en production reposent principalement sur du jugement humain
(arXiv:2512.04123, ICML 2026). Un projet solo qui regarde plutôt que de construire un harnais n'est pas
en retard sur l'état de l'art.

**Referme une question ouverte de `flux.md` §5.** On se demandait comment distinguer *trois tours sur
le même litige* de *trois tours qui dérivent de sujet*. La réponse est mesurée, et elle déplace la
question : ce qui compte n'est pas le tour, c'est le **contexte**. Relire dans la même session
n'apporte rien (p = 0,11) et dégrade légèrement ; relire **dans un contexte frais, sans le prompt de
génération**, gagne 4 points de F1 et **11 points sur les erreurs critiques** (*Cross-Context Review*,
arXiv:2603.12123 — préprint, N = 30, à confirmer). Donner au relecteur l'intention de l'auteur
**l'ancre**. Corollaire pour `revue-plan` et `revue-code` : une relecture est une **session neuve sur
l'artefact seul**, pas une itération de plus dans la conversation qui l'a produit.

**Conforte `tickets.md` §6.4, pour une meilleure raison.** Le plafond de deux ou trois tours avant
escalade était justifié par le coût. La vraie raison est que le **taux de consensus fallacieux
remonte** au-delà — 3,9 % au tour 2, 5,1 % au tour 5 (arXiv:2510.12697). Un plafond de tours est une
mesure de **qualité**, pas d'économie.

**Non tranché à dessein.** Sept questions restent ouvertes (`docs/reference/skills.md` §10) : le
régime `--bare`, le grain des huit skills, qui parle à Linear, la langue du `description`, le nom en
collision, la forme du verdict de revue, les compteurs à nommer. Elles se trancheront mieux avec un
skill réel sous les yeux ; les décider maintenant serait spéculer.

**Deux faits d'architecture à ne pas perdre de vue**, tous deux documentés et tous deux silencieux le
jour où ils mordront. **(1)** `--bare` — qui saute la découverte des skills, hooks, MCP et `CLAUDE.md`
— **deviendra le défaut de `claude -p`** ; le pointeur de `D-038` cessera alors de résoudre, et le
pipeline ne plantera pas : il produira des artefacts qui ne respectent plus les conventions du projet.
**(2)** Un skill **personnel** écrase silencieusement son homonyme de projet, ce qui entame l'argument
« la méthode se relit en revue » de `D-038` : ce qui est relu n'est pas nécessairement ce qui
s'exécute.

Ce que le découpage capture n'est donc pas la conception mais **les frontières** — ce qui est dans cet
incrément, ce qui n'y est pas, l'ordre, les dépendances. C'est la part de la vue d'ensemble qui ne se
recalcule pas, et elle vit dans la description des cartes (`tickets.md` §3, question 6). L'argument de
`D-036` — *la mémoire du découpeur meurt avec sa session* — reste entier ; il désignait simplement le
mauvais artefact.

**Tranché — deux étapes n'auront jamais de skill.** `QA Review` et `Validation` : les deux jugements
sans référentiel opposable. L'absence de skill y est une **décision**, pas un retard, et le tableau de
`flux.md` §4 doit le dire — sans quoi le prochain lecteur la lira comme un trou à combler.

**Ordre d'écriture des skills.** `prendre-un-pas` d'abord : plus petit périmètre, erreur à un commit,
aucune dépendance, et il rend tout de suite le signal qui manque — *une carte de pas contient-elle assez
pour qu'un agent travaille sans avoir eu la conversation ?* Le **découpage écarté comme premier
skill**, bien qu'il soit le plus tentant : tant qu'aucun pas n'a été exécuté par un agent, on ignore
quelle **maille** de pas est bonne, or c'est exactement ce que le découpage décide.

**Renvoi** : `docs/methode/flux.md` (la vue étape → skill, et la liste de ce qui reste à écrire),
`tickets.md` §1/§3/§6.2 (corrigés du même coup) ; `D-036` (ce qu'il révise), `D-030` (le harnais
concret nommé sans être abstrait), `D-012` (le moteur qui ne sait pas ce qu'est un agent — même pari,
un cran plus haut).

---

## D-040 — Renverser la lecture du tableau : deux requêtes plutôt qu'un curseur par projet

**Date** : 2026-07-26 · **Statut** : construit (`CUR-45`, §4.24) · **Portée** : `Cursus.Trackers`

**Le problème n'était pas l'affichage.** L'écran des tâches montrait la première page du tableau et
l'avouait (`TaskProject.IsTruncated`). L'enjeu réel est en aval : le prédicat de déclenchement
(`CUR-5`) évaluera sur ce que le client rapatrie. Une carte éligible mais hors page ne serait **jamais
proposée, sans qu'aucune erreur ne s'affiche** — un faux négatif silencieux, pire qu'une panne.

**L'ordre a été tranché contre ma recommandation, et à raison.** J'avais proposé de faire le prédicat
d'abord, au motif qu'un filtre serveur rendrait la pagination rare. L'utilisateur a répondu : *« elle
sera toujours utile quoi qu'il se passe ; le risque est que ce soit lent parfois, et on regardera à ce
moment-là. »* Mon argument supposait que la pagination puisse devenir **inutile**, ce qu'elle ne peut
pas : un tableau montré à moitié est faux quelle que soit la suite, tandis que la lenteur se mesure
après.

### Ce que la sonde a corrigé dans nos propres écrits

La décision reposait sur une croyance fausse consignée dans `linear-api.md` §6 : que `projects(25) ×
issues(50)` tenait « avec de la marge ». **Elle était à 8 280 sur 10 000**, et le `labels(first: 10)`
ajouté la veille en avait mangé une large part. Un champ de plus faisait sauter l'écran des tâches,
sans autre avertissement qu'un 400.

Deux apprentissages durables, tous deux mesurés :

- **il y a deux limites**, et les confondre conduit à optimiser la mauvaise : une complexité *par
  requête* (10 000, calculée a priori sur les `first:`, qui refuse avant d'exécuter) et un budget *par
  fenêtre* (3 000 000, consommé). Le §6 n'en décrivait qu'une, sous le nom de l'autre ;
- **toute réponse porte son coût dans `x-complexity`**. Nous avions cherché le mur par dichotomie de
  400 ; il suffisait de lire un en-tête.

### La forme retenue, et la correction du ticket

Le ticket disait « renverser la requête sur `issues` racine ». **C'était incomplet** : un projet sans
issue n'est nommé par aucune issue, et disparaîtrait de l'écran — la garantie « un projet vide n'est
pas une absence de projet » existe depuis le premier jour du client. D'où **deux** requêtes, chacune
paginée sur son curseur : les projets **nus** (600, et les vides survivent) plus les issues racine (8,
un seul curseur au lieu d'un par projet). **608 contre 8 280.**

### Trois décisions de fond

**1. La boucle est un objet testé, pas une boucle dans l'adaptateur.** `LinearTaskBoard` est mince et
non testé parce qu'il ne décide rien ; une pagination décide, et casse en **silence** — curseur non
transmis, dernière page perdue, arrêt jamais atteint, aucun des trois ne lève d'exception.
`LinearBoardCollector` reçoit son transport en **délégué** : aucune interface neuve, aucun
`HttpMessageHandler` simulé, et le faux transport **retient les requêtes**. Ce dernier point n'est pas
un détail de confort : sans lui, une boucle qui redemande éternellement la première page passe le test,
dès lors que le double avance de lui-même. La doctrine du client s'en trouve resserrée — le seul
non-testé est désormais le POST.

**2. `IsTruncated` est supprimé, non conservé à `false`.** Un aveu qui ne peut plus être vrai est un
mensonge inverse, et un champ mort induit le prochain lecteur en erreur. **Écarté** : le garder « au
cas où » (`D-035`, ne pas modeler en avance). S'il faut un jour un plafond de sécurité, l'aveu qu'on
remettra dira la vérité *de ce plafond* — ce qui n'est pas la même proposition.

Corollaire : **aucun plafond de pages**, conformément à l'arbitrage produit. La seule protection contre
l'infini est une **garde de non-progression** — on s'arrête si l'API renvoie le curseur avec lequel on
vient de demander. Elle est indispensable parce que Linear rend un `endCursor` **plein sur la dernière
page** : `hasNextPage` décide, jamais la présence du curseur.

**3. Les issues sans projet sont écartées.** Linear en autorise ; invisibles quand on partait des
projets, elles remontent quand on part des issues. `TaskProject` est *le* regroupement du modèle : une
carte hors projet n'a aucun rang où aller, et **ne pas l'afficher est exactement le comportement
d'avant** — donc zéro régression. **Écarté** : un pseudo-projet « Sans projet », qui serait une
fonctionnalité neuve déguisée en correction.

### Trous connus, actés

- une issue dont le projet n'est dans aucune page lue est **perdue sans bruit**. Le cas est réel (deux
  requêtes successives), et il contredit un principe tenu ailleurs — « une tâche absente ne se
  remarque pas ». Figé dans un test qui le nomme, plutôt que corrigé par un rattrapage inventé ;
- un curseur qui **alterne** (A → B → A) échapperait à la garde ; rien ne l'atteste ;
- `labels(first: 10)` reste une troncature **silencieuse**, indépendante de la pagination.

### Une friction de méthode, consignée pour le skill à récolter

Deux fois de suite — hier sur `ReadLabels`, aujourd'hui sur la lecture de `project` — j'ai écrit une
garde de tolérance **sans avoir observé le rouge qui la réclame**. Elle était bien réclamée les deux
fois (vérifié à rebours : 6 tests tombent quand on la retire), mais l'observation manquait, et la
vérifier coûtait dix secondes. Ce n'est pas la règle qui manque, c'est le réflexe de la tenir quand le
code « évident » se présente. Matière pour `D-039`, qui veut qu'un skill se **récolte** sur des
frictions réelles avant de s'écrire.

**Renvoi** : `docs/reference/linear-api.md` §6 (les deux limites), §6a (les coûts), §7bis (la
pagination imbriquée), §9 (les noms HTML-échappés) ; `architecture.md` §4.24, §7.10.2 (le modèle pull
qu'il honore enfin) ; `CUR-46` (le `&amp;`) ; `CUR-5` (le prédicat qui consommera ce tableau complet) ;
`D-035` (ne pas modeler en avance, invoqué deux fois ici).

---

## D-041 — Le flux est tiré, donc l'étiquette dit la fin et la colonne dit le présent

**Contexte.** Première exécution du flux de `flux.md` sur un cas réel — la Discovery de la feature
« Un agent pilote Cursus ». L'exercice a buté avant d'avoir commencé : rien ne disait **à quoi
ressemble la fin d'une étape**. `tickets.md` §2.2 listait sept questions, §6.1 posait trois
exigences de sortie, et les deux ne se recouvraient pas. Quatre décisions en sont sorties, liées
entre elles : chacune est la conséquence de la précédente.

### 1. Le flux est tiré, pas poussé

Une carte entre dans une colonne quand le travail de cette colonne **commence** ; celui qui prend le
travail tire la carte à lui. La convention était **déjà pratiquée sans être écrite** — §6.1 disait
que « la bascule pas engagé → engagé tombe à l'entrée en `Spec` », ce qui n'a de sens qu'en flux
tiré. Faute de l'avoir lue, la première Discovery a été produite avec sa carte restée en `Backlog`.

Ce que ça change, et qui n'est pas cosmétique : **la colonne ne peut plus dire « c'est fini »**, elle
dit « ça se fait ici ». Une étape achevée dont personne n'a encore pris la suite n'a aucun moyen de
se signaler.

### 2. D'où l'étiquette : `Done` ne pousse pas la carte, elle autorise qu'on la tire

Le groupe *Advancement Labels* (`Done`, `Rework Needed`, mutuellement exclusifs) comble exactement
ce trou. Un agent peut l'apposer **sans danger**, parce qu'un avis sur un artefact est révocable et
sans effet de bord — contrairement à un déplacement de colonne, qui engage. Aucun impact machine
aujourd'hui ; c'est en revanche la matière du prédicat de `CUR-5`, et la réponse, au niveau feature,
à la question que §6.3 laissait ouverte (« quelle colonne porte l'éligibilité — `Todo` seul, ou
`Todo` + une étiquette »).

Frontière à tenir (§8) : « un projet portant `Done` a une spec conforme » est une **convention** ;
« le label `Done` porte tel identifiant » est un **contrat machine**, et il ne vit pas dans la DoD.

### 3. Les critères sortent dans `docs/methode/dod/<niveau>/<statut>.md`

Répondre à « cette feature peut-elle être prise ? » imposait de charger 460 lignes décrivant aussi
le fonctionnement des pas. Le grief est **cardinal, pas grammatical** (`D-039`) : c'est le nombre de
règles chargées qui dégrade leur suivi, et le biais de primauté achève celles du bas.

Grain retenu : **niveau × statut**, parce qu'un même statut exige trois choses différentes selon le
niveau — le chemin porte le niveau, et l'ambiguïté du mot `Backlog` disparaît sans qu'on ait à
l'expliquer. La matrice §6 **devient l'index** et cesse d'être le contenu ; le détail vit dans les
fichiers. Une case sans lien est une étape dont les critères ne sont pas écrits, état lisible et
légitime — même patron que `flux.md` §4.

**Écarté** : loger la DoD dans le skill de l'étape. Le relecteur la lit *pour vérifier contre elle*,
et l'humain pour savoir si une carte peut bouger : ni l'un ni l'autre n'exécute le skill. Dupliquée,
elle diverge.

**N'ont été écrites que les deux DoD dont l'exécution a révélé le besoin** (`feature/discovery.md`,
`feature/spec.md`). Écrire les quinze autres d'avance serait la méthode-sur-cas-imaginé que `D-039`
proscrit.

### 4. Le tiers prononce sur la conformité, l'humain sur la justesse — révision de `D-036`

`D-036` disait « un agent de revue distinct **valide** ». §6.3 disait deux paragraphes plus bas que
la spec **n'est pas délégable** et que la posture du relecteur est de « lister les divergences, ne
pas trancher ». Contradiction réelle, et les deux formulations parlaient de deux objets distincts :

- la **conformité** — l'artefact est-il complet et opposable ? Référentiel : la DoD. Délégable ;
- la **justesse** — est-ce *ça* qu'on veut construire ? Aucun référentiel. Irréductible.

Le tiers pose l'étiquette ; l'humain engage en tirant la carte. **Ce partage n'était pas disponible
avant la décision 3** : sans DoD écrite, le tiers n'avait rien contre quoi trancher, et la posture
« lister sans trancher » était la seule tenable. Une décision a rendu l'autre possible.

### 5. Un artefact, un document

La discovery et la spec sont **deux** documents Linear. Motif principal, et il n'est pas le volume :
réunies, elles invitent à **arbitrer en rédigeant le besoin** — exactement ce que `Discovery`
s'interdit, et ce qui lui donne sa valeur (tuer une feature avant d'avoir dépensé un arbitrage
technique). S'y ajoutent des fraîcheurs opposées (l'une meurt, l'autre est un contrat vivant jusqu'à
`Validation`) et une lisibilité gratuite : une feature tuée en discovery a un document et pas
l'autre. Le plan d'archi observait déjà cette règle sans qu'elle soit nommée.

### Ce que l'exécution a coûté, et pourquoi c'est le sujet

Seize frictions journalisées en une session (`docs/methode/journal-frictions.md`), dont trois que
personne n'aurait imaginées à froid : la Discovery a dû **désarbitrer** des conclusions produites
avant elle ; le tableau de pistes que j'avais inventé **appelait le pré-arbitrage** par sa seule
colonne de commentaire ; et l'artefact s'adressait au dépôt (chemins de fichiers, numéros de jalons
périssables, méta-commentaires de méthode) au lieu de son lecteur dans le tracker. C'est la ligne de
base que `D-039` réclamait avant d'écrire le moindre skill.

**Renvoi** : `docs/methode/dod/` · `docs/methode/journal-frictions.md` · `tickets.md` §1, §6, §6.3,
§8 · `flux.md` §2 · `D-036` (révisé sur le verdict de revue) · `D-039` (la récolte) · `CUR-5` (le
prédicat qui lira ces étiquettes).

## D-042 — Une branche par niveau de ticket, et les docs cessent de citer des hashes

**Contexte.** Le travail se faisait sur `main`, en direct. Trois changements simultanés ont rendu
cette pratique intenable : le dépôt est devenu **public** avec un remote GitHub, sous Apache-2.0, le
backlog est passé au grain **feature** (six projets Linear), et la
trajectoire prévoit des **agents qui poussent leur travail** — plusieurs, en parallèle, chacun sur
un pas. Or la méthode ne disait pas un mot de git : ni `flux.md`, ni `tickets.md`, ni
`architecture.md` ne mentionnaient une branche, une PR ou une fusion. Le flux s'arrêtait au tracker.

### 1. Trois niveaux de branches, trois modes de fusion

Un niveau de ticket, une branche, une PR :

| Branche | Fusionnée dans | Mode | Ce que le mode préserve |
|---|---|---|---|
| `pas/CUR-45-3-slug` | la story | **squash** | un commit propre par pas, quel que soit le désordre en amont |
| `story/CUR-45-slug` | la feature | **rebase puis fast-forward** | les commits de pas, sans commit de fusion parasite |
| `feature/CUR-xx-slug` | `main` | **rebase puis `--no-ff`** | l'historique complet, et un point de fusion qui nomme la feature |

**Le fast-forward n'est pas un mode de fusion mais une contrainte** : il n'est possible que si la
cible n'a pas divergé. Dès que deux stories d'une même feature avancent en parallèle — ce que le
travail par agents vise — la seconde exige un rebase préalable. La règle honnête est donc « rebase
puis FF », et elle est écrite ainsi pour que personne ne découvre la marche en la manquant.

### 2. Le squash au niveau pas découple *commiter* de *avoir fini*

Le motif n'est pas la lisibilité, qui n'en est qu'un effet. `CLAUDE.md` exigeait « un commit par
comportement terminé (suite verte, refactor fait) » — ce qui **interdisait de commiter en cours de
cycle**, donc privait de tout point de reprise, et faisait d'un retour de revue arrivé après le
commit une pollution permanente de l'historique.

Le squash dissout les deux : l'agent commite librement pendant le pas (WIP, correction de revue,
refactor), et le commit propre est produit **par la fusion**, pas par la discipline. Conséquence qui
justifie à elle seule la strate : **la revue d'un pas peut avoir lieu après le commit**, sur une PR,
sans que l'histoire en garde la trace.

Corollaire à ne pas manquer : c'est le **corps** du squash qui porte le raisonnement et les écarts,
jamais le titre. GitHub pré-remplit ce corps avec la concaténation des messages de WIP — c'est
exactement le bruit qu'on voulait éviter, et il faut donc le **réécrire à la main** à chaque fusion.

### 3. Les documents cessent de citer des hashes de commit

C'est la **condition de viabilité** du point 1, pas un ajustement de style. Rebaser une branche
réécrit tous ses hashes ; or `CLAUDE.md` impose que la documentation se mette à jour « au fil, pas à
la fin ». Tout hash écrit pendant le développement pointerait donc sur un objet mort au moment de la
fusion — systématiquement, à chaque feature.

Et le coup de grâce : **ce document est append-only**. Un hash périmé dans une entrée `D-NNN` ne
peut pas être corrigé sans violer la règle qui le fonde. La dérogation prise une fois pour le
remappage consécutif à la réécriture d'historique était exceptionnelle ; répétée à chaque feature,
elle abroge la règle.

Désormais on cite l'identifiant Linear — `CUR-45`. Il est **plus stable** (il survit à toute
réécriture), **plus informatif** (il porte le raisonnement de la carte, quand un hash ne porte qu'un
diff) et cohérent avec le tracker comme control plane. Ce qu'il ne fait pas : désigner un état
précis du code. Si ce besoin apparaît, la réponse est un **tag**, pas un hash.

Les hashes déjà écrits restent — ils désignent des commits de `main`, qui ne bougeront plus.

### Écarté — l'incrément fusionné directement dans `main`

Défendu longuement, et sur un argument tiré de la méthode elle-même : `tickets.md` §1 définit un
incrément comme « **livrable seul, suite verte** », propriété qui rend la branche de feature
redondante — si chaque incrément est livrable seul, `main` n'est jamais à moitié fait.

**Ce que cet argument rate** : une feature peut exposer une **surface qui doit apparaître d'un
bloc**. « Un agent pilote Cursus » en est l'exemple — chaque outil MCP est un incrément vert et
livrable, mais publier la moitié des outils sur un dépôt public, c'est publier une API bancale.
« Livrable seul » y est vrai techniquement et faux pour l'utilisateur.

La réponse canonique à ce cas est le **feature flag** — le code atterrit, la surface n'est pas
branchée. Écarté aussi, mais pour une raison purement conjoncturelle : **aucun mécanisme de flag
n'existe dans le code**, et le construire est un chantier que rien d'autre ne réclame aujourd'hui.
La branche de feature est, à cette date, strictement moins chère. Si un flag apparaît un jour pour
d'autres motifs, ce choix mérite d'être rejugé.

### Ce qui reste daté, et doit être rejugé

**La strate `pas/` est instrumentale, pas structurelle.** Elle ne se justifie pas par l'ingénierie —
une branche pour un commit relu par une seule personne est de la cérémonie — mais par la **récolte**
que `D-039` impose avant d'écrire un skill : la PR de pas est le matériau qui servira à écrire
`revue-code`, parce qu'elle produit une trace opposable ligne à ligne que ni Linear ni une boucle
intra-session ne donnent.

Elle a donc une **condition de sortie** : le jour où `revue-code` est écrit et rodé, la question « la
branche de pas sert-elle encore à quelque chose ? » doit être reposée. Faute de l'écrire ici, elle
deviendrait permanente par inertie, et personne ne saurait plus pourquoi elle existe.

**Renvoi** : `flux.md` §6 (la convention opérationnelle) · `tickets.md` §8 (la correspondance
Linear) · `CLAUDE.md` §Branches — qui **remplace** l'ancienne §Commits, devenue sans objet : le
grain du commit n'est plus une discipline mais une conséquence du squash · `D-036` (les trois
niveaux) · `D-039` (la récolte avant le skill) · `D-041` (le flux tiré).

---

## D-043 — Drafter les skills contre `D-039` : ce qui a un référentiel se traduit, le reste se récolte

**Contexte.** `D-039` avait renversé l'ordre d'écriture des skills : on exécute la tâche **sans**
skill, on tient un journal des frictions, et le journal écrit le skill. Le motif n'était pas la
prudence mais l'**évaluabilité** — un skill écrit d'avance se teste sur des cas imaginés, où
l'exécution avec et sans marque pareil. Trois mois plus tard, l'état réel était : **seize entrées de
journal, zéro skill**, et une seule étape du flux réellement exécutée (la Discovery de `D-041`).

Deux faits ont changé l'arbitrage. D'abord, l'étude de trois corpus extérieurs
(`docs/reference/{bmad,task-master,mattpocock-skills}.md`) a comblé le trou qui rendait l'écriture
risquée : on ne savait pas ce qu'est un bon `SKILL.md`, on le sait désormais. Ensuite, le
contre-exemple de Task Master a nommé le risque **inverse** de celui que `D-039` couvrait — un
artefact d'apprentissage que rien ne recharge devient une archive, et leur propre règle
d'auto-amélioration des règles n'a jamais été modifiée en six mois. Un journal parfait que personne
ne consomme perd contre un skill imparfait qui tourne.

### 1. Le test qui départage, et qui n'est pas un compromis

**Le contenu existe-t-il déjà ailleurs, écrit et validé ?**

- **Oui → drafter.** C'est une *traduction* — transformer un référentiel en processus qui le
  satisfait — et il existe quelque chose contre quoi juger le résultat.
- **Non → récolter.** Sans référentiel, on ne sait pas distinguer un bon skill d'un skill plausible.
  C'est exactement l'argument de `D-039`, et il n'est **pas** réfuté ici.

Ce test n'est pas neuf : `D-041` l'appliquait sans le nommer, en n'écrivant que les deux DoD dont
l'exécution avait révélé le besoin.

### 2. Ce qui a été fait, et sous quelle réserve

Dix skills et douze DoD, draftés d'un bloc. Seuls `discovery` et `spec` passaient le test ; les
autres ont été draftés quand même, contre la règle, au motif du §Contexte. La réserve est donc
**inscrite dans les artefacts eux-mêmes** : chaque fichier porte en tête l'aveu qu'il a été écrit
d'après l'état de l'art au lieu d'être récolté, et que le journal des frictions prime sur lui en cas
de désaccord. `flux.md` §5 les classe en *tranché non validé* — **`draft` n'est pas `écrit`**.

C'est la contrepartie qui rend la dérogation acceptable : un draft qui ne dit pas qu'il est un draft
serait indiscernable d'une méthode validée dans trois semaines, et c'est précisément le piège que
`mattpocock-skills.md` documente sous le nom de *template riche que rien ne charge*.

### 3. Deux primitifs, parce que trois skills réinventaient le même geste

`interrogatoire` porte l'entretien — les **faits** sont à la charge de l'agent, les **décisions**
reviennent à l'humain, une question à la fois, avec une réponse recommandée. `revue` porte la
mécanique commune aux trois relectures : deux axes **jamais fondus**, citation obligatoire du
référentiel et de l'extrait, **abstention explicite** quand le référentiel manque.

L'extraction est faite d'emblée plutôt qu'après coup, sur la foi d'une mesure extérieure : dans le
corpus de Matt Pocock, le skill qui invoque le primitif d'entretien est passé de 79 à **7 lignes** le
jour où ce primitif a été extrait. Le coût de l'extraction tardive est donc connu, et il est payé
par tous les appelants.

### 4. Ce que les DoD gagnent à épouser les branches

Les DoD vivent en `docs/methode/dod/{feature,story,pas}/` — `story/` et non `incrément/`, pour que
les trois répertoires **coïncident avec les préfixes de branche de `D-042`**. Un seul vocabulaire de
niveau sert désormais au tracker, aux branches et aux DoD.

### 5. Le droit de ne pas écrire un fichier

Quatre fichiers ont été refusés avec leur motif — `feature/backlog` (aucun aval ne tire la carte,
donc rien à vérifier), `pas/backlog`, `pas/in-progress`, et l'annexe des code smells de `revue-code`
(le modèle les connaît ; en charger la liste serait un no-op).

**Une case sans DoD est un état lisible ; un fichier creux est un mensonge.** Cette clause vaut
au-delà des DoD : c'est la règle qui empêche un corpus de méthode de se remplir pour paraître
complet.

### Ce qui reste daté, et doit être rejugé

**La dérogation est ponctuelle, pas un renversement de `D-039`.** Son argument d'évaluabilité tient
toujours : ces drafts n'ont **aucune** validation, et rien ne dit encore lequel améliore quoi que ce
soit. Le premier travail de code réel — qui inaugurera aussi la cascade de `D-042`, jamais appliquée
— est leur mise à l'épreuve. Trois issues possibles par skill, et il faut les nommer maintenant pour
ne pas se contenter de la première : **promu** (il a servi tel quel), **corrigé par le journal**, ou
**retiré** parce qu'il décrivait un geste que personne ne fait.

Si la troisième issue domine, alors `D-039` avait raison sur toute la ligne et c'est cette entrée-ci
qu'il faudra superséder.

**Renvoi** : `flux.md` §4 (les dix drafts et les deux primitifs) et §5 (le registre) · `tickets.md`
§6.1–§6.2 (les douze DoD reliées) · `docs/reference/mattpocock-skills.md` (l'artisanat, et la
mesure du primitif extrait) · `docs/reference/task-master.md` (le risque inverse, chiffré) ·
`D-039` (la récolte, non réfutée) · `D-041` (les DoD, et le test appliqué sans être nommé) ·
`D-042` (les préfixes de branche que les DoD épousent).

---

## D-044 — Une CLI TypeScript pour les commentaires de revue, et le trousseau devient un contrat entre deux langages

> ⚠️ **`D-045` corrige une inférence de cette entrée**, écrite le surlendemain et avant que celle-ci
> soit commitée : l'ancrage d'un commentaire de document est **impossible** par l'API. Le §1 ci-dessous
> a été redressé en conséquence ; le reste — l'arbitrage de stack, la couture du trousseau, la forme de
> `--with` — tient sans changement.

**Contexte.** La méthode veut qu'une revue rende ses divergences **sur la carte**, ancrées au
passage qu'elles visent (`dod/feature/spec.md` §2). Deux voies existaient. Le **MCP Linear** ne sait
pas résoudre un commentaire — son input n'a pas de champ de résolution. **GraphQL** le sait. Rien ne
l'exploitait, et les revues empilaient donc des `## Tour N` dans un document, faute d'outil.

### 1. La sonde d'abord, et elle a renversé l'hypothèse — deux fois

Le §10 de `linear-api.md` décrivait `quotedText` comme une **ancre**. Mesuré le 2026-07-28, sur le
document du plan de `CUR-45` : **c'est faux**. Quatre citations envoyées — une exacte, une
**inventée de toutes pièces**, une ambiguë, une à cheval sur deux blocs — **quatre acceptées**,
`success: true`. Et le type `Comment` ne porte **aucun** champ positionnel : ni offset, ni
intervalle. `quotedText` est un `String`.

On en avait conclu que l'ancrage était « une recherche de texte faite à l'affichage ». **Cette
conclusion était fausse**, et la seconde campagne du 2026-07-30 l'a établie : l'ancre est une marque
`inlineComment` dans l'état Yjs du document, qu'aucune API n'écrit. Le récit de ce second renversement
et ce qu'il coûte sont en `D-045` ; ce qui compte ici, c'est la **leçon de dispositif** : une mutation
qui réussit ne dit pas ce que l'utilisateur voit, et une inférence tirée d'une mesure juste reste une
inférence. Deux jours ont passé avant qu'elle soit confrontée à l'interface — parce que rien dans le
protocole de sonde n'exigeait d'y regarder.

Conséquence qui décide toute la conception, et qui **survit** au renversement : **la validation de la
citation est le travail**, et les mutations n'en sont pas. Personne d'autre que le client ne peut
empêcher un commentaire qui *paraît* situé sans l'être. Ce qui a changé, c'est le bénéficiaire de
cette garde — non plus Linear, qui ne fait rien de la citation, mais l'humain et l'agent qui liront.
Corollaire à vivre : une citation est une **empreinte**, pas une référence ; le document édité, elle
se périme en silence.

Deux autres mesures ont plié la forme des commandes. `resolvingCommentId` doit désigner une
**réponse du fil** : un commentaire frère fait rendre un `INTERNAL_SERVER_ERROR` — un 500 nu, qui
ressemble à une panne alors que c'est une faute d'usage. Et `parentId` **n'exempte pas** de l'ancre.
Un solde s'écrit donc forcément en deux temps.

### 2. TypeScript, et ce que l'arbitrage a réellement pesé

L'argument attendu pour un `Cursus.Cli` en .NET était la réutilisation. Il ne tenait pas :
`Cursus.Trackers/Linear` est en **lecture seule**, la couche de mutation était à écrire quelle que
soit la stack. Restait `ISecretStore` — or son implémentation shelle `/usr/bin/security`, donc son
contrat est **observable de l'extérieur** et atteignable depuis n'importe quel langage.

D'où le choix de TypeScript, et le prix payé, qui est réel : une seconde stack au dépôt, un second
cycle build/test à tenir vert, et surtout **le format du trousseau devient une couture entre deux
langages** — service `cursus`, compte `tracker:{id}`, valeur en **base64 d'UTF-8**. Ce dernier point
n'est pas cosmétique : `security -w` rend la valeur en hexadécimal dès qu'un octet sort de l'ASCII
imprimable, sans le signaler. Un client qui « simplifierait » en rangeant en clair casserait l'autre
en silence, et seulement sur les jetons contenant un accent.

Écarté également : **un jeton propre à la CLI**. Un seul `login` sert l'app et la ligne de commande ;
le prix est que la CLI dépend de la forme du registre machine, qu'elle doit donc réécrire à
l'identique — y compris en **préservant les connexions d'un genre qu'elle ignore**.

### 3. `--with` prend la raison, pas un identifiant

Le plan prévoyait `resolve --with <commentId>`. La sonde l'a rendu impraticable : l'identifiant
devrait déjà désigner une réponse du fil. La commande prend donc **le texte** de la raison, crée la
réponse ancrée, puis résout en la nommant.

Ce n'est pas un repli. La clause de `dod/feature/spec.md` §2 — *« reprise, ou refusée avec sa raison
écrite ; une divergence sans suite écrite n'est pas soldée »* — cesse d'être une règle qu'on rappelle
pour devenir une **contrainte qu'on ne peut pas contourner** : on ne solde pas sans écrire, puisque
l'écrit *est* l'argument. Le garde-fou porte sur ce qu'on écrit, jamais sur qui appelle — un
garde-fou d'appelant a été écarté pour cela : un agent lit le message d'erreur et passe le flag.

### 4. Le régime, et l'occasion manquée

Ce travail est tombé sous l'**exception outillage** de `CLAUDE.md` : aucune carte, `main` en direct.
Le plan d'archi a donc été un fichier, gaté avant la première ligne de code, conformément à la règle
qui ne dépend pas du niveau de ticket.

⚠️ **Conséquence à assumer** : `D-043` désignait « le premier travail de code réel » comme l'épreuve
des dix skills en draft **et** l'inauguration de la cascade de branches de `D-042`. Ce chantier était
un candidat, et il ne les a éprouvés ni l'un ni l'autre. Les deux restent donc **non éprouvés**, et
la clause datée de `D-043` reste ouverte. Si l'outillage continue d'absorber les occasions
d'éprouver la méthode, c'est l'exception elle-même qu'il faudra rejuger — `CLAUDE.md` prévoit déjà
ce moment (*« si ce cas devenait fréquent au point de mériter sa propre règle »*).

**Renvoi** : `D-045` (le second renversement, et le porteur des remarques) ·
`docs/reference/linear-api.md` §10 (les mesures, et le §10d qui porte le renversement) ·
`cursus-cli/README.md` (les verbes) · `architecture.md` §7.14 (la couture du trousseau) ·
`D-033` (le trousseau, et le refus d'un repli en clair) · `D-042` et `D-043` (les deux épreuves
laissées en attente).

## D-045 — Les remarques de revue quittent le document : un agent ne peut pas ancrer, donc il commente la carte

**Contexte.** `D-044` a doté le dépôt d'une CLI dont le geste central était de poser une divergence
**ancrée** sur un passage d'un document Linear. Le lendemain de son écriture, l'utilisateur a posé un
commentaire avec elle et l'a vu apparaître **« resolved »** dans l'application desktop, alors que
l'API le donnait ouvert. Ce petit écart a ouvert une seconde campagne de sonde, et elle a renversé le
mécanisme entier.

### 1. Ce que la mesure a établi

`Comment.resolvedAt`, `resolvingUser`, `resolvingCommentId`, `hideInLinear` : tous nuls ou faux. Le
commentaire n'était **pas** résolu. La citation existait dans le document au caractère près —
vérifiée dans le Markdown, puis dans l'état de l'éditeur décodé. Rien ne justifiait l'affichage.

L'utilisateur a alors posé un second commentaire **depuis l'interface, sur le même passage**. Contrôle
expérimental parfait : les deux objets `Comment` sont ressortis **indiscernables** — même
`quotedText`, même `documentContentId`, même auteur, mêmes nuls. La différence n'était donc pas dans
le commentaire. Elle était dans le **document** :

- `documentContent.contentState` porte l'état [Yjs](https://github.com/yjs/yjs) de l'éditeur, et il
  avait grossi de 448 caractères **175 ms avant** la création du commentaire d'interface ;
- ce qui s'y était ajouté est une marque `inlineComment` portant `{"commentId":…,"resolved":false}` ;
- sur les neuf commentaires du document, **deux** portaient une marque — exactement les **deux** que
  l'interface affichait. Les sept autres étaient rangés avec les résolus.

**Le cas qui isole la cause** est celui posé par la CLI : son passage était intact, et il ne
s'affichait pas. Ce n'est donc pas la disparition du texte qui décide, c'est la marque. Et les six
autres avaient bien eu la leur : l'utilisateur avait **réécrit** ces passages, et Yjs supprime les
marques **avec** le texte qui les porte.

Trois faits complètent le tableau. La marque est écrite par le **client**, jamais par le serveur —
`commentResolve` ne l'a pas touchée, et l'application l'a rattrapée 18 s plus tard. L'interface
décide l'affichage résolu sur `Comment.resolvedAt`, pas sur la marque — mesuré, un solde par l'API se
voit **en temps réel**. Et `DocumentUpdateInput` n'accepte que du Markdown : réécrire un document par
l'API reconstruit l'état, donc **détruit toutes les marques** — ce que fait `save_document` du MCP,
l'outil qu'un agent prend pour appliquer des corrections.

### 2. La conséquence, et elle est structurelle

**Aucun chemin programmatique ne pose la marque** : ni `commentCreate`, dont les 18 champs d'input
n'ont rien de positionnel, ni `documentUpdate`, ni donc le MCP, qui passe par la même API. Un agent ne
peut pas rendre une divergence **visible** sur un document Linear. Ce n'est pas une lacune de la CLI,
c'est une limite de l'API — et elle disqualifie le geste que `D-044` avait mis au centre.

⚠️ Pire que l'échec : `comment add` **réussit**. Le commentaire existe, porte la bonne citation, et
personne ne le voit. Un agent qui l'appelle croit avoir parlé.

Un second argument, indépendant, condamne l'ancrage pour l'usage visé : le cycle de revue voulu
comporte une étape de **correction**, qui par construction réécrit les passages que la revue a visés.
Elle détruirait donc les ancres avant que l'étape de vérification n'arrive. **Même si un agent savait
ancrer, l'ancre ne survivrait pas au tour suivant.**

### 3. Le porteur : le projet, ou l'issue

Les remarques se posent désormais sur ce qui **porte** le document. Mesuré : le rattachement épouse
exactement les trois niveaux de `tickets.md`.

| Document | Champ | Ancre du commentaire |
|---|---|---|
| Discovery, Spec | `document.project` | `projectId` |
| Plan d'archi | `document.issue` | `issueId` |

Le porteur est donc **déduit**, jamais choisi : c'est une lecture. Ces deux ancres n'ont rien à
ancrer, donc rien qui puisse échouer — un commentaire de projet ou d'issue est visible sans marque,
accepte `quotedText`, se répond en fil et se solde. Vérifié de bout en bout sur une issue, l'interface
affichant l'en-tête « Resolution » sur la réponse qui solde.

⚠️ **Le solde n'a pas été mesuré sur un projet**, seulement sur une issue. Or le projet est le porteur
de Discovery et de Spec, donc le cas nominal. Rien ne suggère une différence — `Comment` est le même
type — mais c'est à éprouver avant de bâtir le cycle dessus.

### 4. La forme d'une remarque

```
*Ref : Discovery › §1. Quel besoin, et pour qui ?*

<la remarque>
```
avec le passage visé dans `quotedText`.

Trois désignateurs, et **l'agent n'en écrit aucun** : le titre du document se déduit du document
visé ; le repère de section est **calculé** — le titre le plus proche au-dessus du passage ; la
citation est recopiée du document et refusée si elle y apparaît plus d'une fois. Ce dernier point
répond à l'objection de l'utilisateur, qui voyait qu'une citation générique ne dit pas de quoi elle
parle : l'ambiguïté est **refusée à l'écriture**, avec le nombre d'occurrences, ce qui force à élargir
jusqu'à ce que la citation désigne. L'agent-correcteur retrouve alors le passage par simple recherche
de texte, sans ancre et sans deviner.

**Écarté après mesure : le repère dans `quotedText`.** C'était la proposition de l'utilisateur, et elle
était meilleure — elle gardait le corps pur et collait le repère au passage. Quatre variantes ont été
posées et regardées dans l'interface ; elle tombe sur un fait : **l'interface aplatit `quotedText` sur
une seule ligne**. Les sauts de ligne sont bien stockés, l'API les rend intacts, mais rien ne se met en
page dedans. Le repère va donc là où le Markdown est rendu — le corps. Ce qui préserve au passage une
propriété utile : `quotedText` **est** exactement le passage du document, donc aucun découpage à faire,
ni par la CLI ni par un client qui lirait l'API directement.

La forme du repère — italique discret, préfixé `Ref :` — est **de l'utilisateur**, contre un en-tête en
gras que l'agent proposait. Le motif est celui de quelqu'un qui lira ces remarques tous les jours : le
repère est une métadonnée, il ne doit pas concurrencer la remarque.

### 5. Le cycle : quatre agents en boucle fermée, l'humain en cinquième

```
① écriture  ② revue  ③ correction  ④ vérification     ⑤ l'humain
   agent A     agent B    agent C       agent D            relit du dégrossi
   └──────── boucle fermée, tourne jusqu'à ────────┘        │
              zéro remarque ouverte                          └─ ses remarques → ③ puis ④
```

Un agent **différent** à chaque temps — exigence de l'utilisateur, et elle rejoint la privation
d'ancrage déjà tranchée pour `revue-code` : celui qui juge n'a pas eu la conversation de celui qui a
écrit.

**Écarté : l'humain au temps ②**, en parallèle du relecteur. Il verrait le document plus tôt, donc
pourrait redresser une trajectoire avant qu'elle ne coûte cher — mais il lirait du brut et
signalerait ce que l'agent allait attraper. **Écarté aussi : l'humain en simple gate final**, qui
approuve ou renvoie sans commenter — le moins coûteux en attention, mais il perd le geste de pointer
une phrase précise, celui que l'utilisateur a passé la matinée à faire.

### 6. La terminaison : deux tours, puis escalade

Une remarque a droit à **deux** passes correction/vérification. Au troisième désaccord elle est
**escaladée** : elle reste ouverte, marquée comme telle, et l'humain tranche au temps ⑤.

**Écarté : le dernier mot au vérificateur**, sans plafond. Le plus simple à écrire, et rien ne remonte
— mais rien ne garantit l'arrêt, et un vérificateur mal calibré fait tourner le correcteur
indéfiniment sur une exigence qu'il ne sait pas satisfaire. Le coût resterait invisible jusqu'à ce
qu'il explose. **Écarté aussi : l'escalade au premier désaccord** — prévisible, mais elle remonte du
bruit qu'un second tour absorberait sans l'humain.

Le second mode d'échec — le vérificateur **complaisant**, qui solde tout et referme la boucle sans
rien avoir obtenu — n'a pas de garde-fou neuf : il est couvert par le régime déjà tranché
(*le relecteur chicanier est le régime nominal*, `skills.md` §5.5) et par la DoD, qui donne deux issues
à une divergence — reprise, **ou** refus motivé.

### 7. Le compteur de tours n'a plus besoin d'être écrit

Les drafts de `D-043` prévoyaient un **second document attaché** à la carte, portant des en-têtes
`## Tour N`, faute de champ Linear pour compter. Il devient inutile : chaque réponse dans le fil d'une
remarque **est** un tour. Le compteur se **compte** au lieu de se déclarer — donc aucun agent ne peut
le fausser, et il n'y a plus d'artefact à créer puis à nettoyer.

### 8. Ce que cette décision coûte, et le registre où elle est

**Tranché, non construit.** Rien de ce qui précède n'existe en code.

- **`comment add` est à refaire** : il poste contre le `documentContentId`, donc dans le vide. Il doit
  résoudre le porteur, calculer le repère, et poster sur le projet ou l'issue.
- **`comment resolve` refuse aujourd'hui** un commentaire qui n'est pas sur un document — un garde qui
  visait la justesse et qui interdit maintenant le cas nominal.
- **`anchor.ts` change de métier sans changer de code** : il ne prépare plus une ancre, il garantit
  qu'une citation désigne. Le refus de l'ambiguïté en devient *plus* important.
- **Quatre des dix skills en draft** — `revue`, `revue-spec`, `revue-plan`, `revue-code` — reposent sur
  un geste qui n'existe pas, et sur le compteur textuel que le §7 supprime.
- **La revue en cours de la Discovery « Un agent pilote Cursus » est désancrée** : ses sept remarques
  existent et se lisent, mais hors du texte. Elles ne sont pas perdues, elles sont déplacées.

⚠️ **`D-043` trouve ici sa première épreuve, et par la négative.** Sa clause datée nommait trois
issues par skill à la première mise à l'épreuve — promu, corrigé par le journal, retiré. Aucun des
quatre skills de revue n'a été *exécuté* ; c'est le **terrain** qui a invalidé leur geste central avant
qu'ils ne servent. Ce n'est aucune des trois issues prévues, et cela vaut d'être noté : un draft peut
mourir d'un fait, pas seulement d'un usage. `D-042` (la cascade de branches), lui, reste toujours non
éprouvé — ce travail est encore tombé sous l'exception outillage.

**Renvoi** : `D-044` (la CLI, et l'inférence que cette entrée corrige) ·
`docs/reference/linear-api.md` §10d (la marque), §10e (le porteur), §10f (`project.comments` qui rend
vide), §10g (la réécriture qui détruit l'ancrage) · `cursus-cli/README.md` ·
`docs/methode/tickets.md` §1 (les trois niveaux que le rattachement épouse) ·
`D-041` (le partage conformité/justesse dans une revue) · `D-043` (les drafts, et sa clause datée).

---

## D-046 — La cible d'une remarque est un type, et le repère se calcule (2026-07-30)

`D-045` a tranché *où* une remarque de revue se pose : sur la carte, jamais sur le document. Cette
entrée consigne ce que la **construction** a appris, et deux décisions que `D-045` ne pouvait pas
prendre parce qu'elles ne se voient qu'en écrivant le code.

### 1. Ce que la mesure a ajouté à `D-045`

Le solde n'avait été éprouvé que sur une **issue**, alors que le porteur nominal d'une Discovery est un
**projet** — `D-045` §3 le signalait comme non mesuré. Mesuré depuis, et sans surprise : `commentCreate`
avec `projectId` est accepté, `parentId` et `projectId` **ensemble** aussi (là où `parentId` seul est
refusé sur un document), et `commentResolve` nommant la réponse du fil fonctionne à l'identique.

Un fait a en revanche évité une découpe inutile : `comments(filter: { project: … })` et
`comments(filter: { issue: … })` ont **la même forme**. Un seul chemin de lecture suffit, là où l'on
pouvait croire devoir en écrire deux.

⚠️ **Et une trouvaille qui change un comportement.** La réponse qui solde un fil a son propre
`resolvedAt` **nul**. Un décompte naïf des remarques ouvertes d'une carte compterait donc les réponses
de solde : la porte du cycle de revue — *zéro remarque ouverte* — ne se fermerait **jamais**, chaque
solde en ajoutant une. Le piège existait déjà dans `commentList`, où il était inoffensif : sur un
document, les fils étaient rares et le décompte décoratif. Il devient faux dès qu'il gouverne une porte.

**La leçon de dispositif, qui est la même que celle de `D-045` d'un cran plus loin** : là, une mutation
qui réussit ne disait pas ce que l'utilisateur voit ; ici, un décompte juste sur les données d'hier
devient faux sur celles de demain, sans qu'aucune ligne ne change. **Mesurer le nominal, pas seulement
le cas qu'on avait sous la main.**

### 2. La variante est portée par le type — et l'écart de lettre est assumé

`DocumentSummary` portait `projectName?` et `issueIdentifier?` : deux optionnels mutuellement
exclusifs, exactement ce que `CLAUDE.md` proscrit. Ils étaient tolérables tant qu'ils **décoraient
l'affichage** ; ils sont devenus porteurs du **routage d'écriture**, et l'état « les deux renseignés »
a cessé d'être inoffensif. Remplacés par un `CommentTarget` :

```ts
export type CommentTarget =
  | { readonly kind: "project"; readonly id: string; readonly label: string }
  | { readonly kind: "issue"; readonly id: string; readonly label: string };
```

**L'écart de lettre, écrit plutôt que tu.** La convention dit que le discriminant « vit dans le
document JSON seulement ». Ici, `kind` vit dans le modèle. En C#, le sous-typage discrimine sans
champ ; en TypeScript sans classes, **l'étiquette *est* le mécanisme de sous-typage**, et l'union
interdit les deux états illégaux — ce que la règle vise. L'esprit est tenu, la lettre non. Le
discriminant *de Linear*, lui, reste bien confiné à l'adaptateur : `targetFrom` lit les deux champs
nuls du JSON, `champDeCible` les reconstruit à l'écriture, et rien entre les deux ne les connaît.

*Nuance conservée* : l'**absence** de cible est un optionnel légitime (`Option`), pas une troisième
variante. Un document peut flotter, attaché à rien ; `requireTarget` refuse alors franchement, parce
qu'il n'y a nulle part où poser la remarque.

**Alternative écartée** : garder `documentContentId` comme troisième genre de cible, pour que
`comment resolve` continue de solder les sept remarques déposées avant le reciblage. Écartée parce
qu'elle aurait porté le geste périmé **dans le type même**, là où le type est justement ce qui doit
rendre le geste périmé irreprésentable. Les sept remarques se **reposent** ; elles ne se maintiennent
pas. C'est une dette de données, pas une dette de conception.

### 3. Ce que `list` liste, et pourquoi la carte l'emporte sur le document

`comment list <réf>` prend une référence de **document** mais liste les remarques de sa **carte**, qui
est partagée : une Discovery et une Spec vivent sur le même projet, donc les remarques des deux
apparaissent, chacune portant son repère `*Ref :*`.

Le motif n'est pas la simplicité d'implémentation, c'est la **porte du cycle** : elle se ferme par
carte et non par document — c'est le projet qu'on juge dégrossi, pas chacun de ses artefacts
séparément. Un décompte par document aurait donné une porte que rien ne ferme jamais toute entière.

**Alternative écartée** : filtrer sur le préfixe `*Ref : <titre du document>*`. Filtrer sur du corps de
texte est fragile — une remarque écrite à la main sans le repère disparaîtrait du décompte —, et cela
irait contre l'usage même du décompte.

### 4. Le repère exclut les blocs de code, et ce n'est pas du zèle

Le repère est le titre ATX le plus proche **au-dessus** du passage. ⚠️ Les documents de méthode sont
pleins de blocs clôturés où un dièse en début de ligne est un **commentaire shell** : sans suivi des
clôtures, le repère citait `dotnet build ne doit rendre aucun warning`. Le cas s'est présenté **dès la
première épreuve réelle** — un passage cité à l'intérieur d'un bloc `mermaid`, dont le repère est
resté, correctement, le titre qui surplombe le bloc.

Ce n'est pas un détail de robustesse : le repère est calculé précisément pour qu'un agent **ne puisse
pas le falsifier**. Un repère faux serait donc cru sans être vérifié — la garantie se retournerait
contre son objet.

**Alternative écartée** : chaîner les titres ancêtres (`§1 › §1.2`). Le titre le plus proche est le
plus précis, et le **titre complet du document** — retenu plutôt qu'un raccourci — lève déjà
l'ambiguïté que la chaîne visait, puisque c'est lui qui départage la Discovery de la Spec.

### 5. Le registre, et ce qui reste

**Construit et éprouvé** contre l'API réelle, sur les deux genres de cible : `comment add|list|resolve`.
61 tests verts, typecheck propre. La garde d'`anchor.ts` a été exercée pour de vrai au premier essai —
le passage que je citais avait été réécrit depuis, et elle l'a dit.

**Ce qui reste ouvert**, et qu'il ne faut pas croire fait :

- **Les sept remarques de la Discovery** sont toujours sur le document, donc invisibles. À reposer.
- **Les quatre skills de revue** prescrivent encore le geste que `D-045` a supprimé.
- **Le cycle à cinq temps** de `D-045` §5 n'existe pas : la CLI en fournit les gestes, pas
  l'enchaînement.
- **`D-042`** (la cascade de branches) reste **toujours** non éprouvé : ce travail est encore tombé
  sous l'exception outillage. C'est la troisième fois qu'il passe à côté.

**Une dérogation à noter, faute de quoi elle passerait pour un oubli.** Les commandes ne portent pas de
tests unitaires : `openSession()` est appelé dans leur corps, il n'y a pas de couture, et en inventer
une n'était pas dans le plan validé. La logique qui méritait des tests a donc été **extraite** —
`headingAt`, `reviewBody`, `targetFrom`, `requireTarget`, `unresolvedRoots` — et les commandes réduites
à du câblage, couvert par l'épreuve bout en bout. La frontière est celle que le module avait déjà ; elle
n'a pas été choisie ici, elle a été suivie.

**Renvoi** : `D-045` (où une remarque se pose, et le cycle) · `D-044` (la CLI) ·
`docs/reference/linear-api.md` §10d–§10g · `cursus-cli/README.md` · `architecture.md` §7.14 ·
`D-043` (les drafts, dont quatre restent à reprendre) · `D-042` (la cascade, encore non éprouvée).

---

## D-047 — Le cycle de revue prend un vocabulaire d'états, et les documents de cycle deviennent le référentiel qui manquait (2026-07-30)

`D-045` §5 a tranché un cycle de revue à cinq temps sans dire par quoi une carte passe d'un temps au
suivant. Le vocabulaire disponible — `Done` et `Rework Needed`, hérités de `D-041` — en couvre deux
sur cinq. Cette entrée comble le trou, et acte trois documents qui n'existaient pas.

### 1. Le motif : rendre un échec attribuable

Le dispositif entier — dix skills en draft, douze DoD, un flux à dix étapes — n'a **jamais tourné**.
La question qui bloquait n'était pas *comment l'exécuter* mais *comment savoir ce qui a raté* :
faute de référentiel écrit, un mauvais résultat ne se range ni dans « le cycle est mauvais », ni
dans « le skill est mauvais », ni dans « la méthode est mauvaise ».

`D-039` semblait interdire d'écrire ces documents d'avance — *on n'écrit pas un skill puis on
l'éprouve*. La distinction qui lève l'objection : `D-039` interdit d'écrire d'avance **l'exécutant**,
pas le **référentiel contre lequel sa sortie est jugée**. C'est exactement l'argument de `D-041`
§6.3 — sans référentiel écrit, le tiers n'a rien contre quoi trancher. Les documents de cycle sont à
la boucle ce que les DoD sont à la revue.

### 2. Deux axes, et le choix se fait par niveau

| Axe | Ce qu'il porte | Ce qu'il permet |
|---|---|---|
| **Colonne** | quel travail se fait | Linear calcule ses temps de cycle sur les **transitions de statut** — seul axe qui produise une métrique comparable hors du projet |
| **Étiquette** | où en est le cycle de ce travail | Se déplace sans migrer le tableau — reste remodelable tant qu'un processus n'est pas stabilisé |

Le critère qui départage : **ce processus est-il stabilisé au point de mériter d'être mesuré, ou
encore assez diffus pour qu'on veuille le déplacer ?** D'où une asymétrie assumée entre niveaux — à
l'**incrément**, une frontière de colonne sépare écriture et revue (`Planning` › `Plan Review`,
`In Progress` › `Code Review`) ; à la **feature**, non.

**Conséquence non évidente** : `Review Requested` ne sert qu'au niveau feature. À l'incrément, la
colonne de revue porte déjà le signal — une carte qui y arrive *est* à relire.

**Écarté : porter les temps sur l'axe colonne partout.** Le tableau devient littéralement lisible,
mais il faut ~3 colonnes par étape de travail, soit six créations. **Écarté aussi : tout sur
l'étiquette**, qui prive de la seule mesure transposable à une autre équipe.

### 3. Six états, plus un qualificatif

Cinq étiquettes **mutuellement exclusives** dans `Advancement Labels` — Linear impose l'exclusivité
au sein d'un groupe, et c'est ce qui fait de cet axe un état et non un sac. L'absence d'étiquette
est le sixième état, et l'état initial.

| Temps | Étiquette | Sens |
|---|---|---|
| ① | *aucune* | l'artefact s'écrit |
| ② | `Review Requested` | écrit, à relire contre sa DoD |
| ③ | `Rework Needed` | des remarques sont ouvertes, à reprendre |
| ④ | `Rework Done` | les reprises sont faites, à vérifier une par une |
| ⑤ | `Human Review` | la boucle agent est sèche, l'humain relit |
| ⑥ | `Done` | zéro remarque ouverte, la carte est tirable |

Et **hors groupe**, donc cumulable : `Escalated` — la boucle agentique n'est pas arrivée au bout
seule. Elle ne remplace pas l'état, elle le qualifie.

**Écarté : `Arbitration Needed` comme état alternatif.** Nommer l'acte attendu dans une étiquette
exclusive obligeait à choisir entre *dire ce qu'il faut faire* et *dire que la boucle a échoué*.
Les séparer sur deux axes garde les deux, et rend le second **comptable** : le nombre d'`Escalated`
par colonne dit où la boucle ne tient pas — et la conclusion peut être que le skill de revue est à
refaire, pas que l'artefact était mauvais.

**Écarté : l'assignation comme signal d'escalade** (`tickets.md` §6.4, *« ni colonne, ni
étiquette »*). Un signal qu'un humain doit poser à la main est un signal qu'il oubliera — même motif
que le retrait du compteur de tours en `D-045` §7. L'assignation survit comme **routage** (*qui*),
jamais comme état (*quoi*). `tickets.md` §6.4 est à amender.

### 4. Un agent correcteur ne se justifie que là où la correction est textuelle

En `Spec`, `Plan Review`, `Code Review`, une remarque désigne un manque **dans l'artefact** : ça se
reprend en relisant l'artefact et son référentiel. En `Discovery`, ce qui manque est de la
**matière** — un entretien qui n'a pas eu lieu, une hypothèse non testée. L'état de l'art de la
discovery continue le dit sans détour : l'alignement s'y fait par la preuve et non par le document,
et la production est portée par un binôme ou un trio, jamais par une chaîne producteur → correcteur.

D'où **deux formes de cycle** :

| Forme | Où | Temps |
|---|---|---|
| **Court** | `Discovery` | ① binôme → ② revue → ① binôme → … → ⑥ |
| **Complet** | `Spec`, `Plan Review`, `Code Review` | ① → ② → ③ → ④ → (② …) → ⑤ → ⑥ |

Le cycle court échange le vérificateur contre le **tour de revue suivant** : le binôme solde ses
propres remarques, ce qui serait complaisant ailleurs, mais reste tenable parce qu'une relecture
suit toujours et que le relecteur voit le fil entier.

**Conséquence à ne pas rater** : `Rework Needed` convoque *celui qui a écrit* — donc **l'humain** en
`Discovery`, où il est dans le binôme. La même étiquette n'appelle pas le même acteur selon le
niveau.

### 5. La table de transition ne nomme pas son exécutant

Chaque document de cycle porte, par colonne, une table `état observé → skill invoqué → livrable →
état posé`. Elle ne dit jamais **qui** la parcourt : un humain aujourd'hui, Cursus demain, texte
identique. La table **est** le workflow, écrite d'avance dans une forme qui n'aura pas à être
traduite — c'est le seul choix de forme irréversible de ces documents.

L'étiquette est posée par **l'agent qui finit son temps**, faute d'autorité centrale — seul régime
qui fonctionne sans moteur, donc seul qui marche aujourd'hui. Un skill ne déplace **jamais** la
carte (`revue` §8) : déplacer est l'acte de celui qui tire, et c'est ce qui rend le flux tiré
observable plutôt que déclaratif.

**Écarté : un skill `aiguiller` dès maintenant.** Un moteur écrit en prompt, donc non déterministe —
exactement ce que le noyau de Cursus existe pour ne pas être — et à jeter le jour où Cursus fait le
travail. **Écarté aussi : écrire les documents à la deuxième personne**, en mode d'emploi ; les
trois seraient à réécrire au moment où on en aura le moins envie.

### 6. Trois skills de plus, et quatre étiquettes à créer

Le cycle réclame quatre mandats distincts, et deux n'avaient **aucun** skill. Ils deviennent des
**primitifs** — même patron que `revue`, pour le même motif : le geste est invariant, seul le
référentiel change.

| Skill | Rôle | État |
|---|---|---|
| `correction` | ③ — lire les remarques ouvertes, reprendre, répondre dans chaque fil | à écrire |
| `verification` | ④ — pour chaque remarque, la solder ou la rouvrir | à écrire |
| `revue-discovery` | ② en `Discovery` — ses deux axes sont déjà rédigés dans sa DoD | à écrire |

Portant le total à **treize**. Étiquettes à créer : `Review Requested`, `Rework Done`,
`Human Review`, `Escalated`. La colonne d'issue `In Review` est orpheline — elle n'apparaît dans
aucun document et le devient définitivement, les temps vivant sur l'étiquette.

**Écarté : loger ③ dans les skills de production** (`spec`, `plan-archi` réinvoqués avec les
remarques). Charger un skill de production pour une reprise de trois lignes, et lui ajouter à chacun
une section « reprise sur remarques » qui divergerait des autres.

### 7. Ce que cette décision coûte, et le registre où elle est

Elle écrit **quatre fichiers de méthode décrivant un dispositif dont aucun tour n'a tourné** — le
reproche exact que `D-043` s'est fait à lui-même. La différence assumée : ces documents ne
*produisent* rien, ils servent de mètre. Un mètre faux se corrige au premier écart mesuré ; un
exécutant faux produit du travail faux en silence.

Le vocabulaire est **commun aux trois niveaux**, d'où un quatrième fichier `cycle.md` qui le porte
seul — le tripler aurait garanti sa divergence.

**Registre** : tout est *tranché non construit*, sauf les gestes de remarque (`D-046`, éprouvés
contre le vrai Linear sur un projet comme sur une issue) et le régime TDD du niveau pas, tenu depuis
le premier jalon. **Reste ouvert** : les deux passes avant escalade sont un chiffre repris de
`D-045`, jamais mesuré ; distinguer trois tours sur le même litige de trois tours qui dérivent ; le
vérificateur complaisant n'a pas de garde-fou propre.

**Renvoi** : `D-045` (le cycle à cinq temps, et pourquoi la remarque quitte le document) · `D-041`
(le flux tiré, les deux verdicts, `Advancement Labels`) · `D-039` (la ligne de base, la session
neuve) · `D-046` (les gestes construits) · `D-036` (les trois niveaux) ·
`docs/methode/cycle.md` et les trois documents de niveau.

---

## D-048 — Le vocabulaire d'états rencontre le vrai Linear : un nom change, une famille manquait (2026-07-30)

`D-047` a posé six états et les a déclarés créés. Le premier temps ① réellement joué — la Discovery
d'*Un agent pilote Cursus* — a montré qu'ils ne l'étaient qu'à moitié, et qu'un des six noms se
lisait dans deux sens. Cette entrée corrige les deux points, et **supersède `D-047` §3** sur le seul
nom.

### 1. `Human Review` devient `Human Review Requested`

Le renommage vient de l'utilisateur, et l'argument qui l'a emporté n'est pas celui qu'il avançait.
La série des six mélangeait deux natures : `Review Requested` et `Rework Needed` nomment ce qui est
**attendu**, `Rework Done` et `Done` nomment ce qui est **fait**. `Human Review` était l'intrus — il
nommait une **activité**, donc se lisait *« la relecture humaine est demandée »* autant que *« elle
a eu lieu »*. Sur un état, l'ambiguïté est fatale, et d'autant plus que `Done` le suit
immédiatement.

`Human Review Requested` régularise la série : ② et ⑤ deviennent deux demandes symétriques, ce
qu'elles sont exactement — l'une appelle un relecteur agent, l'autre appelle l'humain quand la
boucle agent est sèche.

**Ce que le renommage coûte, et pourquoi il force cette entrée.** Treize occurrences amendées dans
`cycle.md`, `cycle-feature.md`, `cycle-increment.md`. Côté Linear, l'étiquette de **projet** porte
le nouveau nom dès sa création, et celle d'**issue** a été renommée **à la main** le même jour —
aucune API ne renomme une étiquette. Mais `D-047` §3 porte l'ancien nom **pour toujours** — le journal est
append-only. Sans
cette entrée, les documents de méthode et le journal se contrediraient sans que rien ne les relie.
⚠️ **Ne pas propager le renommage à `docs/research/` ni à `reference/symphony.md`** : `Human Review`
y désigne l'état de *Symphony*, pas le nôtre.

### 2. Le vocabulaire vit deux fois, parce que Linear a deux familles d'étiquettes

`D-047` §3 supposait une création unique. Faux : **Linear sépare strictement les étiquettes d'issue
et les étiquettes de projet**, et une étiquette d'issue ne s'applique pas à un projet, fût-ce à nom
identique. Or une **feature est un projet** (`tickets.md` §8) — donc le niveau qui porte ses états
*uniquement* par étiquette, faute de colonne de revue (`D-047` §2), était précisément le seul à ne
pas les avoir. Les six ont été créées côté projet le 2026-07-30.

**L'appartenance au groupe ne se lit pas, elle se mesure.** `list_issue_labels` rend le champ
`parent`, `list_project_labels` **ne le rend pas** — et conclure de son absence que les étiquettes
de projet sont hors groupe est une erreur, commise puis corrigée en provoquant l'échec : poser deux
étiquettes du même groupe sur un projet rend un `400` qui nomme le conflit. L'exclusivité tient donc
des deux côtés, et avec elle le mécanisme qui fait de cet axe un **état** plutôt qu'un sac.

**Aucune API ne crée d'étiquette de projet**, ni ne renomme une étiquette d'issue : ce sont des
gestes d'interface. Troisième mur de la même journée, après l'impossibilité d'ancrer un commentaire
(`D-045`) et celle d'en supprimer la racine. Le motif se répète assez pour être nommé : **l'API
Linear couvre le travail courant, pas l'administration de l'espace** — tout dispositif de méthode
qui suppose un agent capable de préparer son propre tableau se trompe.

### 3. `revue-discovery` existe, et il porte trois axes

Le skill manquant du cycle feature est écrit. `cycle-feature.md` §3 lui annonçait **deux** axes —
*l'artefact est complet* (§1 de sa DoD) et *aucun arbitrage n'a été rendu* (§2). Il en porte
**trois** : le §5 de la même DoD, *l'artefact s'adresse à son lecteur, pas au dépôt*, est cochable
et n'était couvert par aucun des deux. Le fondre dans la complétude aurait mélangé deux natures de
jugement, ce que le primitif `revue` §2 interdit — un défaut de forme rangé à côté d'un défaut de
fond emprunte son poids.

**Le piège qu'il a fallu défaire en l'écrivant, et qui attend les quatre autres.** Écrit en calquant
`revue-spec`, il a hérité du geste que `D-045` a tué : poser une remarque *sur le document*. Il a
fallu réinjecter à la main le geste vivant — `cursus linear comment add` sur la **carte**. Les
quatre skills de revue restant à reprendre partiront du même faux départ si on les calque : leur
squelette est bon, leur geste central est mort.

### 4. Registre

**Construit** : les six étiquettes des deux côtés, groupées, exclusivité mesurée ·
`revue-discovery` · `Review Requested` posée sur une vraie carte — **première pose réelle du
vocabulaire**, à la main par le binôme, pas par un skill.

**Ce que cette entrée n'établit pas** : `revue-discovery` n'a pas tourné. Aucune boucle complète non
plus. Le renommage lui-même n'est pas éprouvé — il corrige une ambiguïté de lecture repérée à froid,
pas une confusion observée en usage.

**Renvoi** : `D-047` (les six états, superseded sur le nom de ⑤) · `D-045` (pourquoi la remarque
quitte le document) · `D-046` (les gestes construits) · `D-041` (le flux tiré) ·
`docs/reference/linear-api.md` §10h (les trois murs, mesurés) · `docs/methode/cycle.md` §8.

---

## D-049 — La spec est fonctionnelle *et* technique : elle porte un plan d'implémentation (2026-07-31)

`D-036` a posé « un artefact par niveau » : la feature arbitre dans sa **spec**, l'incrément conçoit
dans son **plan d'archi**, le pas prouve dans sa **test list**. Le découpage en trois tient toujours.
Ce qui ne tenait pas, c'est la conséquence qu'on en avait tirée — que la feature ne conçoive **rien**.
Cette entrée **amende `D-036`** sur ce seul point.

### 1. Le trou : personne ne conçoit ni ne valide la structure d'ensemble

Relevé par l'utilisateur, en Spec d'*Un agent pilote Cursus*. Sa formulation initiale — « le
découpage est automatisé, donc il n'y a pas de validation » — est **inexacte** : `decoupage` §6
impose un accord explicite de l'humain **avant toute création de carte**, et `cycle-feature.md` §5
le revendique (*« le jugement a lieu dans le geste, pas après »*).

Mais la conclusion tenait, pour une raison plus précise : ce sur quoi l'humain tranche à ce
moment-là, ce sont **trois questions de découpe** — granularité, arêtes de blocage, fusionner ou
scinder. **Aucune ne porte sur la structure technique.** Et chaque plan d'archi ne voit que son
propre incrément, relu contre `architecture.md` mais jamais contre ses frères. Donc entre la spec et
le premier plan, **aucune instance ne regarde l'ensemble** — et rien ne l'écrit.

**La preuve est arrivée le jour même, par la revue.** Sa remarque la plus lourde — *piloter deux
projets de front suppose N `ProjectHost` vivants, que `architecture.md` §7.13 range en TRANCHÉ NON
CONSTRUIT* — ne naît d'aucun incrément en particulier : elle naît de leur **conjonction**. Ni la
spec ni un plan local ne l'attrapent naturellement. Second symptôme, dans le même document : les
décisions structurantes de la séance (l'application porte le serveur, N clients, le projet en
paramètre, la sérialisation des mutations) traversent **tous** les incréments, et s'étaient logées à
moitié dans la spec faute d'un endroit où aller.

### 2. Ce qui est tranché

**La spec gagne une huitième question** — *comment ça va marcher ?* — et son **plan
d'implémentation** : les solutions techniques envisageables, celle qu'on priorise et pourquoi,
comment on compte la concevoir, les **grandes dépendances** à ajouter ou modifier, le tout porté par
**au moins un schéma**. Il vient en dernier parce qu'il se nourrit des sept réponses précédentes.

**La frontière avec le plan d'archi ne bouge pas, elle se nomme.** Les deux ne se recouvrent pas :

| | Plan d'implémentation (feature) | Plan d'archi (incrément) |
|---|---|---|
| Portée | L'**ensemble** | **Ce** changement-ci |
| Ce qu'il montre | Que ça **peut** marcher, et comment c'est **censé** marcher | Comment c'est **structuré** |
| Autorité | **Indicative** | **Engageante** |
| Quand | En `Spec` | À la prise, en `Planning` |

⚠️ **Sa profondeur est celle qui valide, pas celle qui prescrit.** Un plan d'archi qui s'en écarte au
contact du réel est **dans son droit** — il le dit, c'est tout. C'est la clause qui empêche cette
entrée de réintroduire un *waterfall* par la porte de la feature, et elle est de l'utilisateur :
*« ce n'est pas forcément à appliquer à la lettre plus tard, parce qu'au contact du réel on peut
penser à de meilleures façons de faire »*.

**Corollaire sur les faits.** Un plan qui affirme la faisabilité doit la **mesurer** quand elle est
mesurable. La spec MCP a établi ainsi que Kestrel et Avalonia cohabitent dans un `WinExe` .NET 10,
qu'un bind sur le port 0 résout la découverte d'endpoint, et que deux connexions SQLite
concurrentes attendent 30 s puis échouent **sans corrompre**. Trois affirmations qui, citées de
mémoire, auraient été fausses ou approximatives.

### 3. Ce qui a été écarté

- **Un artefact séparé entre la spec et le découpage**, avec son propre cycle de revue. Écarté par
  l'utilisateur : *« la spec est fonctionnelle ET technique, l'un ne peut pas aller sans l'autre »*.
  Un second document aurait rejoué la séparation que `D-041` a payée entre discovery et spec, sans
  le motif qui la justifiait là-bas.
- **Élargir le gate du découpage** à la cohérence technique. Moins cher, mais le jugement serait
  resté **oral** — sans trace écrite ni relecture tierce, donc sans rien à opposer en `Validation`.
- **Laisser le premier incrément porter la structure d'ensemble.** L'ordre de prise aurait décidé
  qui porte la charge, et le premier plan serait devenu illisible.
- **Supprimer le plan d'archi d'incrément**, la spec portant déjà la conception. Écarté : `Planning`
  et `Plan Review` se seraient vidés, et la seule strate qui **s'engage** aurait disparu.

### 4. Ce que ça coûte

Amendés le même jour : `tickets.md` (§1 le tableau des artefacts et la règle « aucun plan
d'avance », §2.2 la question 8, « ce qu'une feature ne contient pas »), `dod/feature/spec.md` (§1
trois cases, §4 la levée d'ambiguïté), `cycle-feature.md` (§4 le livrable et le motif), le skill
`spec` (une étape de plus), et `CLAUDE.md` aux deux endroits qui énoncent les registres.

### 5. Registre

**Construit** : les six documents amendés.

**Ce que cette entrée n'établit pas** : aucun plan d'implémentation n'a encore été écrit. La spec
d'*Un agent pilote Cursus* est la première à en devoir un — elle a été rédigée **avant** cette
décision, et en manque donc par construction. ⚠️ C'est une douzième remarque à solder, plus lourde
que les onze de sa revue, et le premier usage dira si la profondeur « valide sans prescrire » se
tient à l'écriture ou reste un vœu.

**Renvoi** : `D-036` (les trois registres, amendé ici) · `D-041` (un artefact, un document —
et pourquoi on n'a pas ouvert un troisième) · `D-039` (la session neuve, qui a produit la revue
révélatrice) · `docs/methode/tickets.md` §2.2 q.8 · `docs/design/schemas.md` (la convention visuelle).

## D-050 — `Spec` bascule en cycle court : le critère n'était pas la nature de la correction, mais la présence de l'humain (2026-07-31)

`D-047` a doté le cycle de revue d'un vocabulaire à six états, et `cycle.md` §6 a réparti les
colonnes entre **cycle court** (① → ② → ① → … → ⑥) et **cycle complet** (① → ② → ③ → ④ → …), en
rangeant `Spec` du côté complet. Cette entrée **renverse ce placement**, et remplace le critère qui
l'avait produit. `cycle-increment.md` n'est pas touché.

### 1. Ce que le critère prédisait, et ce que deux exécutions ont donné

Le critère écrit était : *« un agent correcteur ne se justifie que là où la correction est
textuelle »*. Il rangeait `Discovery` à part — ce qui y manque est de la **matière**, pas de la
prose — et mettait `Spec` avec `Plan Review` et `Code Review`.

`Spec` d'*Un agent pilote Cursus* a tourné **deux fois** le 2026-07-31, et **ni `correction` ni
`verification` n'ont jamais servi** — non par manque de temps, mais parce que rien ne les appelait :

- le temps ③ a été joué **à la main par le binôme**, deux fois, c'est-à-dire exactement le geste
  du cycle court ;
- le temps ④ n'a **jamais eu lieu**. Au tour 2, un second passage de revue l'a remplacé, et il a
  rendu **davantage** qu'une vérification : aucune des onze remarques du tour 1 rouverte — donc la
  reprise tenait — et douze défauts neufs, dont deux violations dures sur des passages que le
  tour 1 avait lus sans rien y trouver.

**Le chiffre qui tranche** : sur les douze remarques du tour 2, **cinq portent littéralement la
ligne « La question à reposer »**, et une sixième est un constat de justesse. La moitié des
remarques d'une revue de spec ne demandent donc **aucune correction textuelle** — elles demandent
un arbitrage : ce que « mutant » désigne, quelle branche pour le cycle de vie des hosts, quelle
maille pour trente gestes, comment se répartit une clause de recette. Un agent `correction` lancé
seul là-dessus aurait produit de la prose lisse sur des questions ouvertes — le **faux succès** que
`docs/reference/skills.md` désigne comme le mode de défaillance dominant.

La Spec est donc, sur ce point, **bien plus près de la Discovery que du code**.

### 2. Ce qui est tranché

**`Spec` passe en cycle court.** Les temps ③ et ④ y disparaissent : `Rework Needed` convoque le
binôme, qui reprend et repose `Review Requested`. La porte de sortie reste mécanique — `open` vaut
zéro — et `Human Review Requested` reste ce qui ferme la boucle quand elle n'avance plus.

**Le critère de `cycle.md` §6 est remplacé** :

| Ancien critère | Nouveau critère |
|---|---|
| *La correction est-elle textuelle ?* | ***L'humain est-il dans la production ?*** |

Là où il l'est — `Discovery` et `Spec`, régime *Trio* (`tickets.md` §6.3) — le binôme reprend,
parce qu'il est le seul à pouvoir trancher, et **la revue suivante tient le rôle du vérificateur**.
Là où il ne l'est pas — `Plan Review` et `Code Review`, où un agent écrit seul — ③ et ④ gardent
leur sens : il n'y a personne pour arbitrer, et personne pour rattraper.

⚠️ **`correction` et `verification` restent à écrire.** Ils cessent d'être réclamés par `Spec`, pas
par l'incrément. Ce qui change est leur ordre de priorité, pas leur existence.

**Le rattrapage par la revue suivante n'est pas une hypothèse.** `cycle.md` §8 l'avait déjà mesuré
en `Discovery`, dans le sens qui compte : la reprise du binôme y était *sincère et fausse*, et les
six remarques du tour 2 rouvraient toutes des points qu'il croyait soldés. En `Spec`, il a joué
dans l'autre sens, et c'est aussi informatif.

### 3. Ce qui a été écarté

**Garder ④ en le rendant étroit** — un vérificateur qui ne relit que les fils. Écarté parce que
c'est précisément ce que le tour 2 a fait **en mieux** : une revue relit l'artefact, donc elle voit
ce qu'une reprise a cassé ailleurs, ce qu'un vérificateur de fils ne regarde pas.

**Garder ③ pour décharger l'humain.** Écarté par le chiffre du §1 : la moitié des remarques ne sont
pas déchargeables, et un correcteur qui traite les six autres laisse un artefact à moitié repris
qu'il faut de toute façon reprendre en binôme.

**Aligner aussi l'incrément.** Écarté : le motif de ce renversement est la présence de l'humain, et
elle n'y est pas. Y appliquer la même conclusion serait reproduire l'erreur qu'on corrige —
généraliser un critère au-delà de ce qui l'a établi.

### 4. Ce que ça coûte, et ce qui n'est pas mesuré

**Un tour de revue complet coûte ~620 s et ~82 000 jetons** là où une vérification ciblée coûterait
une fraction. Le pari n'est rentable que tant qu'un tour trouve beaucoup ; **le jour où un tour
rendra deux remarques cosmétiques, c'est le signal qu'il fallait la vérification étroite**.

**Il manque un critère d'arrêt mesuré.** En cycle court la porte est « une revue ne trouve rien »,
et aucune boucle n'y est arrivée — ni en `Discovery`, ni en `Spec`. C'est `Human Review Requested`
qui doit la fermer, par un jugement humain, pas l'épuisement du relecteur.

**Deux exécutions ne sont pas une loi.** Les deux reprises étaient bonnes ; le cas qu'on n'a pas vu
est celui d'une reprise complaisante que la revue suivante laisserait passer.

### 5. Registre

**Construit** : rien — c'est une décision de méthode, et son effet est un retrait.
**Tranché non construit** : `correction` et `verification` pour l'incrément.
**Question ouverte** : le seuil qui dirait qu'un tour de revue ne vaut plus son prix.

## D-051 — Un relecteur n'a pas d'observation non bloquante : il oppose, ou il se tait (2026-07-31)

Friction 38 du journal, relevée au tour 3 de `revue-discovery` : le relecteur avait **quatre
observations non bloquantes** qu'il n'a déposées nulle part, parce que les poser aurait fait monter
`open` et interdit le `Done` qu'il s'apprêtait à rendre. Le geste *« poser une observation sans
rouvrir la porte »* n'existait ni dans la CLI ni dans le cycle. La question était : faut-il le
créer ?

### 1. Ce qui est tranché : non, et le motif n'est pas le coût

**Un constat de revue a deux issues, jamais trois** : ou bien il vaut d'être opposé — c'est une
remarque, elle ouvre `open`, elle se solde —, ou bien il ne le vaut pas, et il n'existe pas.

L'argument décisif est de l'utilisateur, et il déplace le problème : **si un document est validé
comme complet et auto-portant, une remarque posée à côté ne peut qu'ajouter du bruit.** Elle dit
implicitement *« le document ne suffit pas »* au moment même où le verdict dit qu'il suffit. Les
deux ne peuvent pas être vrais ensemble — et c'est le verdict qui est opposable, pas la note en
marge.

Ce que ça préserve, et qui vaut plus qu'une observation : **le `Done` reste un contrat lisible.**
Qui prend une carte marquée `Done` sait qu'il n'a rien d'autre à lire que l'artefact. Autoriser un
canal parallèle transformerait cette garantie en *« l'artefact, plus ce qui traîne dans les fils »*,
et le suivant devrait trier — ce qui est exactement le travail qu'un `Done` promet de lui épargner.

### 2. Ce qui a été écarté

**Un geste `observation`** — poser puis solder dans le même appel, avec un marqueur en tête du
corps, pour que `open` ne bouge pas. Techniquement presque gratuit, et c'était la recommandation de
l'agent. Écartée par le motif ci-dessus : le problème n'était pas la faisabilité du geste mais sa
compatibilité avec ce que `Done` affirme.

**La variante restreinte** — une observation légitime seulement si elle est **adressée à quelqu'un
de nommé** (le découpeur, l'auteur du plan d'archi, la validation), une observation « en général »
étant du bruit. Écartée avec la précédente : le test est bon, mais il ne répond pas à l'objection —
une observation adressée reste une chose à lire en plus de l'artefact.

**Corollaire assumé** : ce que le relecteur voit sans vouloir l'opposer est **perdu**, et c'est le
prix consenti. Si un tour montre un jour qu'on perd quelque chose de cher, c'est le seuil de
l'opposition qui est mal réglé — pas le canal qui manque.

### 3. Ce qu'il ne faut pas y confondre

**Le constat de justesse n'est pas une observation.** *« Est-ce la bonne chose à construire »* n'a
pas de référentiel dans une DoD et revient à l'humain (`tickets.md` §6.3), mais il **appelle une
réponse** — donc il se pose sur la carte, il compte dans `open`, et il se solde par l'arbitrage
rendu. C'est ce qui s'est passé au second tour de la spec d'*Un agent pilote Cursus*, et c'était le
bon geste. La différence tient en une question : **est-ce que quelqu'un doit répondre ?** Si oui,
c'est une remarque, quel que soit son axe. Si non, ça n'existe pas.

### 4. Ce qu'on ne sait pas

**Les quatre observations du tour 3 sont perdues** — on a donc tranché sans jamais voir ce qu'elles
contenaient, ni si le découpage en aurait eu besoin. La décision se prend sur le principe, pas sur
la mesure, et c'est une faiblesse qu'il faut écrire : le cas qui la renverserait est celui d'un
découpage qui bute sur quelque chose qu'un relecteur avait vu et tu.

### 5. Registre

**Construit** : la règle est écrite dans `revue` §6 ; elle ne demande aucun outillage — c'est une
décision qui **retire** une option, pas qui en ajoute une.
**Question ouverte** : le seuil de l'opposition. Rien ne dit aujourd'hui ce qui mérite d'ouvrir
`open`, sinon le jugement du relecteur et les référentiels de son axe.

## D-052 — Une couche applicative naît de la parité : commandes et requêtes, et le verrou avec elles (2026-08-01)

Tranché par l'utilisateur en reprise du **quatrième tour** de `revue-spec` sur *Un agent pilote
Cursus*. La remarque qui l'a déclenché opposait le schéma §8.3 de la spec — où toutes les arêtes vers
le noyau partaient du « point de passage » — à sa prose, qui affirmait deux fois l'inverse : *« il
donne son tour, il ne sait pas ce qui se fait pendant »*. Le relecteur demandait laquelle des deux
pièces avait tort. **Aucune des deux** : ce qui manquait était un niveau.

### 1. Ce qui est tranché

**Entre les deux portes — la fenêtre et le serveur MCP — et le noyau, une couche applicative porte
les gestes.** Elle se divise en **commandes** (ce qui écrit) et en **requêtes** (ce qui lit), et
**c'est la commande qui détient le verrou**. Le « point de passage » disparaît en tant que tel.

Trois raisons, et la première est celle qui a emporté la décision :

- **un geste s'écrit une fois.** *Ajouter une étape* est une commande, appelée par l'outil MCP comme
  par l'éditeur. Sans elle, une seconde porte réécrit ce que la première portait déjà — et deux
  implémentations d'un même geste divergent, ce que la parité existe précisément pour empêcher ;
- **le verrou devient non contournable par construction.** La protection actuelle est
  *accidentelle* : elle tient au seul fait que tous les appelants d'édition sont des commandes de
  ViewModel, donc sur le thread UI. Un verrou qu'il faut *penser à prendre* reproduit cette
  fragilité en explicite ; une écriture qui n'est pas une commande, elle, n'existe pas ;
- **les requêtes ne verrouillent rien**, ce qui rend la lecture d'un run en vol possible sans faire
  attendre le run.

**La couche entre au périmètre de la feature**, elle n'en est pas un pré-requis — au même titre que
la construction des N hosts, et pour le même motif : en faire une carte externe aurait bloqué la
feature sur une priorisation. Elle descend hors de la présentation avec la racine de composition.

### 2. Le fait mesuré qui borne la décision

**Les deux portes n'ont pas la même granularité de geste**, et c'est vérifié dans le code, pas
supposé. `WorkflowEditorViewModel` est *stateful* : il ouvre son brouillon au montage, le mute en
mémoire, et **n'écrit qu'à son `Save`**. L'outil MCP, lui, est atomique — *ouvrir → muter → sauver* à
chaque appel.

D'où la borne : **la couche ne porte que le geste atomique**, et l'éditeur ne la traverse qu'en son
`Save`, seul moment où il touche le disque. Rien ne change pour qui se sert de la fenêtre.

### 3. Ce qui a été écarté

**Le point de passage nu** — un verrou que chaque appelant consulte avant d'écrire, la composition
du geste restant dans l'outil. C'est ce que la spec portait jusqu'ici, avec cet argument : *« si le
socle partagé ouvrait le brouillon, le mutait et rendait l'identifiant, il porterait la logique
d'authoring — et il faudrait l'y écrire une fois par outil »*. **L'argument est retourné** : une
commande par geste, écrite une fois et appelée par les deux portes, est le but et non le coût.

**Rendre l'éditeur atomique lui aussi** — un seul chemin d'écriture, aucune divergence possible.
Écarté parce que ce serait **changer le produit pour servir l'architecture** : plus de bouton
*Enregistrer*, plus d'état « brouillon non enregistré », et chaque frappe touchant le disque.

**Deux formes dans la couche** — des commandes atomiques pour le serveur, une session d'édition
ouverte pour la fenêtre. Écarté : deux chemins d'écriture à tenir cohérents, donc le risque même que
la parité existe pour éviter.

**La couche en feature séparée**, dont celle-ci dépendrait. Écarté par le §5 de la spec : cela
renverse son « aucun pré-requis » et bloque la feature sur une carte à créer et à prioriser.

### 4. Registres

**Construit** : rien. Il n'existe **aucune couche applicative** dans le dépôt — les gestes vivent
dans les ViewModels.
**Tranché, non construit** : la couche elle-même, sa division commandes / requêtes, et le verrou
porté par la commande. ⚠️ **La spec qui l'inscrit est en `Review Requested` et n'a pas passé le
temps ⑤** — `architecture.md` ne l'enregistre donc pas encore ; l'y écrire avant que l'humain
prononce ferait passer un projet pour un fait.
**Question ouverte** : dans quel projet la couche atterrit — avec le reste du socle partagé, la
question est commune ; sa maille de sérialisation — par projet, par workflow, globale ; et jusqu'où
les ViewModels passent par les requêtes, la décision ne les y obligeant que pour leurs écritures.

### 5. À ne pas confondre avec `CUR-28`

Le repackage `Core` / `Infra` / `Host` / `UI` déplace des **assemblies** le long d'une hexagonale ;
cette décision ajoute un **niveau** qui n'existe nulle part. Les deux se touchent — la couche vivra
quelque part que le repackage nommerait — sans se recouvrir, et `CUR-28` reste hors périmètre, à
rediscuter en Discovery de la résidence.

---

## D-053 — Trois échelles de conception, et leurs noms remis à l'endroit ; la spec enregistre l'arbitrage, elle ne l'exerce pas (2026-08-01)

Tranché par l'utilisateur, en reprise de la spec d'*Un agent pilote Cursus*. Deux corrections
distinctes, faites dans la même séance : l'une porte sur **qui arbitre**, l'autre sur **combien
d'échelles de conception existent et comment elles se nomment**. Cette entrée **amende `D-036` et
`D-049`** sur le vocabulaire, sans rien retirer de leur substance.

⚠️ **Clé de lecture des entrées antérieures.** `decisions.md` est append-only : `D-036` et `D-049`
gardent l'ancien vocabulaire, qui désigne **l'inverse** du nouveau. Table de correspondance, à
appliquer en lisant toute entrée antérieure au 2026-08-01 :

| Avant (`D-036`, `D-049`) | Après | Niveau |
|---|---|---|
| « plan d'implémentation » (de la spec) | **plan d'architecture** | Feature, en `Spec` |
| « plan d'archi » (de l'incrément) | **plan de design** | Incrément, en `Planning` |
| skill `plan-archi` | skill `plan-design` | — |

### 1. La spec n'arbitre pas : le binôme arbitre, la spec en porte la trace

`tickets.md` §1 écrivait « la **feature** arbitre *quelle solution et si elle vaut le coup*, dans sa
**spec** » — formulation qui fait du document l'agent de l'action. C'est faux, et pas seulement
grammaticalement : **l'arbitrage est un acte du binôme humain ↔ agent**, posé en colonne `Spec`. Le
document en est l'**enregistrement**.

Trois conséquences, dont la deuxième est la seule qui change un geste :

**Elle fonde §6.3, qui posait sans motiver.** « La spec n'est pas délégable, aucun agent ne juge que
c'est *ça* qu'on veut construire » : on sait désormais pourquoi. L'humain est du côté de la
**production** (`D-050`) parce que l'arbitrage lui appartient — ce n'est pas une commodité de
composition, c'est la nature de l'acte.

**Elle borne ce que le relecteur tiers peut cocher.** La DoD disait « les options sont
**arbitrées** », ce qui laisse croire à un jugement sur le fond. Le relecteur ne prononce jamais que
l'arbitrage est *bon* : il vérifie qu'il est **écrit, argumenté, et que les écarts le sont aussi**.
C'est une case de **traçabilité**, pas de justesse — exactement le partage conformité / justesse
posé par `D-041`, appliqué ici au contenu même de la spec.

**Elle change le diagnostic quand la case ne se coche pas.** Lire « la spec est mal écrite » mène à
réécrire ; lire « le binôme n'a pas tranché » mène à retourner interroger. Ce ne sont pas les mêmes
remèdes, et le second est le bon.

### 2. Trois échelles, pas deux — et le découpage n'en est pas une

La conception se fait à **trois** échelles, et une quatrième étape s'y glissait sans en être une :

| Échelle | Quand | Artefact | Décide | Ne décide pas |
|---|---|---|---|---|
| **Architecture** — système / module | Feature, en `Spec` | la spec, moitié technique | Composants, frontières entre eux, dépendances externes | La forme des objets |
| *(ordonnancement)* | Feature → `In Progress` | les **cartes** d'incrément | Vers où chaque incrément va, son acceptation, l'ordre, les blocages | **Rien de structurel** |
| **Design** — objets / classes | Incrément, en `Planning` | le **plan de design** | Objets qui naissent, changent, meurent ; leurs responsabilités ; le schéma-delta ; l'ordre des pas | La test list, le code |
| **Implémentation** — code | Pas, à sa prise | la **test list** | Les cas à prouver, fichier par fichier | — |

**Le découpage n'est pas une échelle de conception, c'est un ordonnancement.** Il livre à chaque
incrément sa **direction** et son **acceptation** — jamais sa structure, et surtout pas ses pas. Le
dépôt tenait déjà la règle (`plan-archi` §4 : *« n'écris ni test list ni comment coder chaque pas »*
; `cycle-increment.md` : *« le plan s'écrit ici, pas au découpage »*), mais nulle part elle n'était
énoncée **positivement** : on la déduisait d'une liste de « ce que la feature ne contient pas ». Une
règle qui ne s'obtient que par soustraction ne survit pas à un lecteur pressé.

**Ordonner des pas n'est pas les concevoir**, et c'est pourquoi le découpage en pas reste au plan de
design sans contredire ce qui précède : il donne à chaque pas son titre, sa raison d'être *à cette
place* et son piège local — pas ses cas de test.

### 3. Les noms étaient à l'envers

`D-049` a nommé « plan d'implémentation » l'artefact de la **feature** (le plus haut) et laissé
« plan d'archi » à l'**incrément** (le plus bas). C'est l'inverse de l'usage : *architecture* désigne
le niveau système, *design* le niveau objets, *implémentation* le code.

Le symptôme était visible dans le document lui-même : `tickets.md` portait **trois ⚠️** dont le seul
travail était de prévenir « ne confondez pas ces deux plans ». Un nommage qui exige trois
avertissements est un nommage qui travaille contre son lecteur.

**Ce qui a emporté la décision n'est pas l'esthétique, c'est la destination du projet.** Cursus vise
à ce qu'un **agent consomme ces tickets** (`tickets.md` §Pourquoi il existe). Un agent arrive avec
le vocabulaire du corpus mondial déjà appris — où *architecture* est toujours la couche haute et
*design patterns* vit au niveau classe. Un dépôt qui inverse ces deux mots paie l'inversion **à
chaque brief, indéfiniment**. Le renommage se paie une fois.

L'ordre obtenu a en outre la vertu d'être **monotone en portée** — système → objets → code — donc
retenable sans exception à mémoriser.

### 4. Ce qui a été écarté

**Garder les noms et se contenter d'écrire l'échelle.** Coût quasi nul, et c'était l'option
raisonnable si le dépôt n'était lu que par ses auteurs. Écartée pour la raison du §3 : elle conserve
les trois ⚠️ **à perpétuité** et laisse chaque agent futur buter sur la même inversion.

**Mettre le design au-dessus de l'architecture** (« design, archi, implémentation »), lecture
défendable dans le vocabulaire produit. Écartée : elle nous met à contre-courant du corpus dont les
agents héritent, et casse la monotonie de portée qui rend l'ordre mémorisable.

**« Plan de conception »** pour le niveau intermédiaire, plus français que « design ». Écarté parce
que *conception* est le terme **générique** qui englobe les trois échelles ; l'employer pour une
seule fabrique sa propre ambiguïté, à la place de celle qu'on retire.

**N'inverser que deux noms** — architecture pour la spec, implémentation pour l'incrément. Écarté
parce que le §2 vient précisément d'établir qu'il y a **trois** échelles : appeler « implémentation »
le niveau des objets laisserait le niveau du code sans nom.

⚠️ **Friction assumée** : le répertoire `docs/design/` emploie « design » au sens générique et
contient `architecture.md` — l'architecture s'y trouve donc rangée sous le design. Renommer le
répertoire coûterait plus que la gêne qu'il cause ; on l'assume et on ne le renomme pas.

### 5. Registres

**Construit** : le renommage est propagé dans la documentation de méthode (`tickets.md`, les DoD,
les documents de cycle, `flux.md`, `CLAUDE.md`) et dans les skills, `plan-archi` devenant
`plan-design`.

**Tranché, non construit** : rien de neuf — les deux corrections sont documentaires, aucun code ne
les porte.

**Volontairement laissé en l'état** : les fiches de `docs/methode/rex/` et les entrées antérieures de
ce journal. Ce sont des **archives datées** — elles décrivent des exécutions passées avec le
vocabulaire de leur jour, et les réécrire falsifierait ce qu'on lisait alors. La table du préambule
est la clé qui les rend lisibles.

---

## D-054 — La spec reçoit un plan standard : fonctionnel, puis technique, et l'arbitrage en annexe (2026-08-01)

Tranché par l'utilisateur, en reprise de la spec d'*Un agent pilote Cursus*. Son constat : les titres
du document étaient **maison** — « L'inventaire », « Les vertus qui doivent survivre », « Comment ça
va marcher » — et *« surprenants pour tout le monde »*. Un lecteur qui arrive ne sait pas ce qu'il va
trouver sous eux, et un agent qui les consomme encore moins.

### 1. Le plan

```
1. Spécification fonctionnelle — comment ça va fonctionner
   1.1 La solution retenue
   1.2 Spécifications fonctionnelles détaillées
   1.3 Hors périmètre fonctionnel

2. Spécification technique — comment on va le construire
   2.1 Les choix, en bref
   2.2 Le plan d'architecture
   2.3 Les invariants à ne pas casser

3. État des décisions et dépendances
   3.1 Les trois registres
   3.2 Le pré-requis

Annexes
   A. L'arbitrage technico-fonctionnel
   B. Les scénarios de recette, en Gherkin
   C. Les mesures de faisabilité
```

### 2. Ce que le plan ne remplace pas : les huit questions

**Les huit questions de `tickets.md` §2.2 restent le référentiel de complétude ; le plan n'est que la
forme.** La distinction est ce qui rend le changement compatible avec l'existant, et elle a été
établie avant lui : les huit questions sont des **angles**, pas des compartiments — rien ne dit où un
fait s'établit plutôt que d'être mentionné, et c'est ce qui produit la redite constatée en passe
globale sur cette même spec.

Un plan par-dessus ne les contredit donc pas : il dit **où** chaque réponse atterrit par défaut, là
où les questions disent **ce qui** doit avoir été répondu. La DoD continue de se cocher sur les
questions.

| Question (`tickets.md` §2.2) | Sa section par défaut |
|---|---|
| 1. Quelles options, à quel coût ? | Annexe A |
| 2. Qu'est-ce qu'on construit ? | §1.1 |
| 3. Comment le recettera-t-on ? | Annexe B |
| 4. Où en est-on déjà ? | §2.2 |
| 5. Quel est le pré-requis ? | §3.2 |
| 6. Qu'est-ce qui est tranché ? | §3.1 |
| 7. Quelles vertus doivent survivre ? | §2.3 |
| 8. Comment ça va marcher ? | §2.1 et §2.2 |

⚠️ **Cette table vit ici, pas dans les specs.** L'ancienne spec en portait une en tête, à titre
d'auto-vérification : c'est une checklist de conformité, donc le travail du **relecteur** contre la
DoD, pas celui du document. Une table de correspondance logée dans l'artefact qu'elle décrit finit
par diverger de lui, et elle ment alors avec l'autorité d'un sommaire.

### 3. Trois arbitrages de contenu, tranchés en même temps

**La recette part en annexe, en Gherkin.** Le format est conventionnel — *étant donné / quand /
alors* se lit sans qu'on explique comment le lire, y compris par qui ne connaît pas le dépôt. Il
double la convention de titre de test déjà en vigueur (`CLAUDE.md`). La recette reste le référentiel
de `Validation` ; seule sa place et sa forme changent.

⚠️ **Les règles d'atterrissage ne partent pas en Gherkin et restent dans le corps.** Qu'une clause
soit exemptée de tomber dans un incrément, ou qu'elle se réparte en charge sans se répartir en
référentiel, sont des **instructions au découpage** — pas des scénarios. Les convertir les rendrait
illisibles et les ferait disparaître de là où le découpeur les lit.

**L'inventaire n'est pas converti en Gherkin : il devient la §1.2.** C'était la proposition initiale,
et elle a été écartée sur un motif précis : un inventaire de parité est un **ensemble de
comparaison** — clos, daté, cochable ligne à ligne — et non une suite de scénarios. Trente-cinq
Gherkins de la forme *« quand l'agent liste les workflows, alors il obtient la liste »* seraient
répétitifs au point d'être illisibles, et perdraient l'exhaustivité qui fait toute la valeur de la
liste. S'y ajoute que plusieurs de ses lignes ne sont pas des comportements — l'exception barrée, ou
l'écart d'un geste qui existe au noyau sans être exposé. La bonne lecture est plus simple : **un
inventaire de ce que le produit doit permettre *est* une spécification fonctionnelle détaillée.** Il
ne changeait pas de nature, il était mal rangé — placé après tout le technique alors qu'il est du
fonctionnel pur.

**Les vertus sont purgées, pas supprimées.** La proposition initiale était de retirer la section, les
vertus vivant déjà dans le dépôt. Vrai pour la plupart — *zéro warning, suite verte, TDD* sont dans
`CLAUDE.md`, *le noyau sans dépendance sortante* dans `architecture.md` —, et les y recopier est du
bruit. Mais certaines sont des invariants **de la feature** et ne sont écrits nulle part ailleurs :
sur cette spec, *aucune seconde porte d'authoring* (la raison d'être même de la feature) et *aucun
geste d'écriture ne peut en corrompre un autre* (qui porte la définition d'un incrément *mutant*,
donc une instruction directe au découpage). La règle retenue : **n'y écrire que ce qui n'est pas
dérivable du dépôt**, et renvoyer pour le reste.

### 4. Ce qui a été écarté

**Garder les titres maison.** Ils portaient une intention réelle et des formulations affinées par
quatre tours de revue. Écarté : la destination du projet est qu'un **agent** consomme ces documents,
et un titre que seul l'auteur comprend est un coût payé à chaque lecture — le même raisonnement qu'en
`D-053` sur le vocabulaire des plans.

**Convertir aussi l'inventaire en Gherkin**, pour n'avoir qu'un format de recette. Écarté au §3.

**Supprimer la section des vertus.** Écarté au §3 : deux des six n'existent nulle part ailleurs, et
`tickets.md` q.7 comme la DoD les exigent nommément.

**Laisser le plan à ce seul document, et voir à l'usage** — ce qu'aurait recommandé `D-039` (le
journal écrit le skill, après deux ou trois passages). Écarté par l'utilisateur : sans inscription
dans la méthode, la prochaine spec réinvente sa structure, et surtout le **skill** `spec` continue de
produire l'ancienne — or c'est lui qui pilotera l'agent.

### 5. Registres

**Construit** : le plan est inscrit dans `tickets.md` §2.2, dans `dod/feature/spec.md` et dans le
skill `spec` ; la spec d'*Un agent pilote Cursus* y est portée.

**Tranché, non construit** : rien.

**Question ouverte** : le format Gherkin n'a **jamais été exercé** ici — aucune `Validation` n'a
tourné sur ce dépôt. On ne sait donc pas si six scénarios suffisent à recetter une feature de cette
taille, ni où passe la frontière entre un scénario et une règle d'atterrissage quand le cas est moins
net que sur cette spec-ci. À confronter à la première `Validation` réelle.

## D-055 — Le partage d'une commande vaut pour les gestes de même granularité, pas pour l'authoring (2026-08-01)

Tranché par l'utilisateur en reprise du **cinquième tour** de `revue-spec` sur *Un agent pilote
Cursus*. La remarque opposait deux passages de la spec : le commentaire du diagramme de séquence
affirmait que « l'éditeur de la fenêtre appelle la même commande au moment de son enregistrement »,
alors que l'avertissement sur la granularité, deux paragraphes plus haut, décrivait un éditeur
*stateful* qui « ne traverse la couche qu'en ce point ».

**`D-052` n'est pas renversé — il est borné.** L'entrée précédente reste valide dans son geste comme
dans ses écarts ; ce que la présente ajoute est la portée de sa **première raison**.

### 1. Le fait qui borne

À son enregistrement, l'éditeur détient un brouillon portant *n* mutations faites en mémoire.
Appeler *ajouter une étape* à ce moment-là relirait le disque et perdrait les *n − 1* autres. Il
appelle donc une **autre** commande — celle qui enregistre un brouillon complet, que l'inventaire de
la spec porte déjà comme ligne distincte.

⚠️ **Ce qui avait été mal lu, et qui ne pose pas problème** : cette seconde commande est **atomique
elle aussi**. L'état *stateful* vit dans le ViewModel, jamais dans la couche. L'écart de `D-052` §3
contre « deux formes dans la couche — des commandes atomiques pour le serveur, une session d'édition
ouverte pour la fenêtre » **tient donc intact** : ce sont deux commandes atomiques dans une seule
couche, et non deux formes de couche.

### 2. Ce qui est tranché

**Le partage d'une même commande par les deux portes vaut là où leurs granularités coïncident** —
ouvrir un projet, lancer ou arrêter un run, lier un tracker. Là, la raison n°1 de `D-052` s'applique
telle qu'elle est écrite.

**Pour l'authoring, elles ne coïncident pas, et ce n'est pas l'unicité de la commande qui empêche la
divergence : c'est la couche d'édition que les deux chemins traversent.** Elle existe déjà, ses
invariants sont construits, et la spec la porte comme invariant sous le nom *aucune seconde porte
d'authoring*.

Autrement dit, la protection est **plus forte que le mécanisme annoncé**, pas plus faible : elle ne
dépend pas de ce qu'un futur appelant pense à réutiliser la bonne commande.

### 3. Ce qui a été écarté

**Reformuler la première raison de `D-052`** en « toute écriture est une commande qui porte son
verrou, et l'authoring passe par la couche d'édition ». Plus exact d'un mot, mais l'entrée avait un
jour, et le motif y était celui que l'utilisateur avait lui-même proposé contre les deux options
présentées. Une reformulation aussi rapide efface la trace de ce qui a été pensé quand.

**Changer l'exemple du diagramme de séquence** pour un geste réellement partagé par les deux portes.
Écarté : *ajouter une étape* est le geste le plus démonstratif de la spec — il porte le brouillon,
l'instantané et la désambiguïsation du titre. Le remplacer par *ouvrir un projet* aurait acheté la
cohérence de la figure au prix de tout ce qu'elle enseigne.

**Rendre l'éditeur atomique** — déjà écarté par `D-052` §3, et pour la même raison : ce serait
changer le produit pour servir l'architecture.

### 4. Registres

**Construit** : rien — la couche applicative n'existe pas encore.

**Tranché, non construit** : la borne est inscrite dans la spec, sous le diagramme de séquence et
dans l'avertissement de granularité.

**Question ouverte** : combien de gestes partagent réellement leur commande entre les deux portes.
Quatre sont nommés ; l'inventaire n'a pas été parcouru ligne à ligne pour le vérifier, et ce compte
décide de ce que la première raison de `D-052` couvre en pratique.

## D-056 — La parité est de capacité, pas de forme : même modèle, mêmes règles, chemins libres (2026-08-01)

Énoncé par l'utilisateur pendant la reprise du cinquième tour de `revue-spec`, en marge de `D-055`
et en le généralisant : *« ce n'est pas forcément à prendre au pied de la lettre le fait que tous les
gestes faits par l'humain au travers de l'UI doivent être possibles par le MCP »*.

### 1. Ce qui est tranché

La clause de parité de la feature *Un agent pilote Cursus* — *tout ce qu'un humain fait dans la
fenêtre pour piloter des workflows, un agent le fait aussi* — s'entend **par fonctionnalité, jamais
par geste**.

Les deux portes n'ont ni le même mode de consommation ni les mêmes moyens : la fenêtre est un client
lourd, avec un état qui persiste entre deux gestes, des flux qu'elle suit en continu et des choses
qu'elle montre ; le serveur répond en HTTP, sans état, un appel à la fois. **Il est donc attendu que
le même résultat s'obtienne par des chemins différents, et parfois en plus d'étapes.**

Ce qui ne dévie pas, et ce sont les deux seules choses opposables :

- **toute fonctionnalité reste atteignable des deux côtés** — *modifier un workflow* en est une ;
- **les deux chemins passent par le même modèle**, donc valident les mêmes règles métier.

### 2. Ce que ça change à ce qui était écrit

**L'invariant de la spec change de portée.** Il disait *aucune seconde porte d'authoring* et ne
couvrait donc que la couche d'édition ; il dit désormais **aucune seconde porte, sur aucun agrégat**.
La raison est la même que pour l'authoring — un chemin qui n'entre pas par le modèle ne valide pas
ses règles — mais elle ne se limitait pas à lui.

**Ça déplace ce qui garantit la parité**, et cela referme le point laissé ouvert par `D-055` : la
protection ne tient pas à ce que les deux portes appellent la même commande — elles ne le peuvent pas
partout —, elle tient à ce qu'aucune n'ait d'autre entrée que le modèle.

**Ça change enfin la nature de l'inventaire de la spec.** Il liste des **fonctionnalités
atteignables**, pas des gestes d'écran ; deux entrées qui nomment le même résultat obtenu depuis deux
écrans sont une fonctionnalité, pas deux. C'est le critère qui manquait pour opposer cet inventaire.

### 3. Ce qui a été écarté

**Lire la clause au pied de la lettre**, geste par geste. Écarté par le fait : la fenêtre compose
plusieurs lectures dans un même écran et s'appuie sur un état que le protocole n'a pas. Une parité de
geste serait soit invérifiable, soit satisfaite par des outils qui singent l'écran au lieu de servir
l'agent.

**Assouplir jusqu'à « l'essentiel est couvert ».** Écarté : sans référentiel clos, la clause
redevient invérifiable — ce que la spec constate déjà en écrivant qu'une phrase de parité ne se
recette pas telle quelle.

### 4. Registres

**Construit** : rien de neuf — l'invariant vise du code à écrire.

**Tranché, non construit** : la formulation est inscrite dans la spec, sous la clause de parité et
dans les invariants.

**Question ouverte** : ce que « même fonctionnalité » recouvre quand un écran compose plusieurs
gestes du noyau en un seul mouvement. Le cas ne s'est pas encore présenté autrement que sur des
doublons de nommage.

## D-057 — Les hosts sont gardés, et l'agent ne les ouvre pas (2026-08-01)

Tranché par l'utilisateur en reprise du cinquième tour de `revue-spec` sur *Un agent pilote Cursus*.
La remarque opposait la ligne d'inventaire *ouvrir un projet — faire construire son host, ce sans quoi
aucun autre geste du projet n'est adressable* au schéma de composition de la même spec, où chaque
commande et chaque requête résolvent seules par la racine.

### 1. Les faits, établis dans le code et non supposés

`SqliteProjectHost.Open` construit un `SqliteRunJournal` dont le **constructeur** crée le répertoire,
**ouvre une connexion SQLite et crée le schéma**. Le host est `IDisposable`, et le disposer ferme la
connexion. Cette connexion est **unique et non thread-safe** ; elle est sérialisée par un `Lock` en
processus.

Conséquence directe : résoudre à la demande **sans garder** signifierait une connexion neuve par
appel — donc plusieurs écrivains concurrents sur le même fichier, c'est-à-dire le cas mesuré où
SQLite attend puis remonte un `SQLITE_BUSY` visible après le délai par défaut de la bibliothèque.

### 2. Ce qui est tranché

**Les hosts sont gardés.** La racine construit à la première résolution et conserve ; ce n'est pas un
choix de confort, c'est la recette qui l'impose — le scénario de concurrence de la spec interdit
nommément qu'un `SQLITE_BUSY` remonte à l'agent.

**L'agent n'ouvre pas.** *Ouvrir un projet* quitte l'inventaire : c'est une mécanique, pas une
fonctionnalité, et `D-056` rend la distinction opposable. Le geste rejoint la navigation pure au titre
hors périmètre, avec sa raison propre inscrite — il *change* bien quelque chose, mais l'agent n'a pas
à le demander.

**Aucune arête de blocage n'en naît.** Le découpage n'a pas à faire dépendre les incréments de projet
d'un incrément « ouverture », et aucune clause d'acceptation ne porte sur l'ordre des appels.

### 3. Le motif, et il vaut au-delà de ce cas

**Exiger un « ouvrir » préalable porterait un ordre d'appels, donc de l'état entre deux appels.** La
spec pose ailleurs que rien ne vit entre deux appels — c'est ce qui rend la surface compatible avec un
protocole sans état. Un ordre imposé est de l'état conversationnel sous un autre nom.

Le même raisonnement avait déjà servi, sans être nommé : *ouvrir la page d'un workflow* a été retiré
au motif qu'un agent adresse par identifiant à chaque appel. Le présent cas est le même, sur un autre
objet.

### 4. Ce qui a été écarté

**Le geste explicite.** L'agent ouvre avant d'adresser. Écarté pour l'état conversationnel qu'il
introduit, et pour l'arête de blocage qu'il ferait naître de tout incrément de projet vers celui-ci.

**Les deux — résolution implicite plus ouverture explicite**, cette dernière servant à vérifier qu'un
projet est ouvrable ou à préchauffer. Écarté : plus de surface à construire et à recetter pour un
usage qu'aucun scénario ne demande aujourd'hui. À rouvrir le jour où un agent aura besoin de savoir
*avant* d'agir si un projet est utilisable.

### 5. Registres

**Construit** : la garde n'existe pas — la fenêtre possède aujourd'hui son host, un seul, et la racine
n'est pas descendue hors de la présentation.

**Tranché, non construit** : la garde et le retrait de la ligne d'inventaire sont inscrits dans la
spec.

**Question ouverte** : **où** vivent les hosts gardés, qui les possède, et si un host inactif se
ferme. C'est ce qui reste de la question du cycle de vie, et cela relève d'un plan de design.

## D-058 — Le verrou des commandes est global, parce que la concurrence attendue est légère (2026-08-01)

Tranché par l'utilisateur en reprise du cinquième tour de `revue-spec` sur *Un agent pilote Cursus* :
*« il n'y aura pas 36 appels simultanés, c'est de la concurrence légère, et des opérations rapides —
donc il ne faut pas sur-concevoir la solution »*. La remarque opposait que la maille, rangée en
question ouverte, décide de la recettabilité d'une clause du scénario de concurrence.

### 1. Les faits, relevés dans le code

Les objets écrits n'ont pas tous la même portée, et la question telle qu'elle était posée — *par
projet, par workflow, globale* — en manquait une :

| Écrivain | Portée | Protection actuelle |
|---|---|---|
| `ProjectRegistry` | **globale** (`ForCurrentUser`, un fichier par utilisateur) | aucune |
| `TrackerRegistry` | **globale** (idem) | aucune |
| `WorkflowCatalog` | par projet, un fichier par workflow | aucune |
| `SqliteRunJournal` | par projet, une connexion | un verrou interne, déjà là |

**Deux des branches proposées étaient donc déjà mortes** : ni une maille par workflow ni une maille
par projet ne protègent les deux registres globaux, alors que le scénario de concurrence exige
nommément que deux créations simultanées laissent deux projets et non un registre tronqué.

### 2. Ce qui est tranché

**Un seul verrou, pour toutes les écritures.** Toute commande le prend.

Le motif est le régime d'usage, pas la beauté du modèle : la concurrence attendue est légère — deux
portes, quelques clients — et chaque écriture est brève : une transaction d'un seul événement, ou la
réécriture d'un fichier de définition. Le code le notait déjà pour le verrou du journal, *négligeable
devant un lancement de process*.

⚠️ **Ce que le verrou ne tient pas** : la durée d'un run. La commande de lancement démarre, rend
l'identifiant et lâche — elle ne tient pas les heures que le run peut durer.

### 3. Ce qui a été écarté

**Un verrou par ressource écrite** — global pour les deux registres, par projet pour son journal, son
catalogue et ses artefacts. Sérialise le strict minimum, et c'est la maille exacte. Écarté parce
qu'elle demande une table verrou ↔ ressource à tenir et un jugement à chaque commande neuve, plus une
règle d'ordre de prise dès qu'une commande touche deux ressources — sous peine d'interblocage. C'est
une **discipline**, exactement ce que `D-052` voulait remplacer par une construction.

**Laisser la maille en question ouverte** et déplacer la clause de concurrence vers `Validation`.
Écarté : ce serait la seconde clause exemptée sur six, et deux exemptions commencent à vider
l'acceptation répartie de sa substance.

### 4. Ce qui rend la décision peu coûteuse à reprendre

**La maille est invisible de l'extérieur.** La raffiner plus tard ne change aucun outil, aucune
clause de recette, aucune frontière d'incrément — c'est une décision de plan de design que la spec
n'aurait pas eu à porter, si elle ne conditionnait pas une acceptation.

### 5. Registres

**Construit** : rien — seul `SqliteRunJournal` porte aujourd'hui un verrou, et il ne couvre que sa
propre connexion.

**Tranché, non construit** : la maille est inscrite dans la spec, au registre du même nom.

**Question ouverte** : aucune sur ce point.

## D-059 — La descente du socle se paie une fois, par le premier incrément qui en a besoin (2026-08-01)

Tranché par l'utilisateur en reprise du cinquième tour de `revue-spec` sur *Un agent pilote Cursus*.
La remarque opposait que la spec donne explicitement sa règle d'atterrissage à la sérialisation, et
n'en donne aucune à la descente du socle hors de la présentation ni au recâblage des ViewModels —
alors que ce sont les deux charges structurelles de la feature.

⚠️ **Cet arbitrage avait déjà été rendu, et il a été perdu.** L'utilisateur l'avait tranché en reprise
du troisième tour, sous la forme *« le fondateur absorbe la descente du socle — pas d'incrément socle
séparé, il échouerait au test de départage »*. Il vivait dans la section de la spec qui portait
l'intention de maille, laquelle a été **retirée en entier** en reprise du quatrième tour, au motif
qu'une spec n'a pas à décrire les lots. Le motif du retrait était juste ; il a emporté avec lui une
règle qui n'était pas un lot. Voir le journal des frictions pour ce que cela dit du geste de retrait.

### 1. Ce qui est tranché

**Le premier incrément qui a besoin du socle le fait descendre ; les suivants en héritent.** Le
recâblage des ViewModels suit la même règle, écran par écran, à mesure que les gestes correspondants
passent par les commandes.

Ce n'est pas un incrément. Seul, il ne livrerait rien d'observable — la spec écrit elle-même que
*rien ne change pour qui se sert de la fenêtre* —, et il échouerait au test de départage
(`tickets.md` §1) pour le motif exact déjà opposé à un incrément « sérialisation » et à un incrément
terminal « la parité est complète ».

### 2. La distinction qui compte, et que l'on aurait pu manquer

**La descente et la sérialisation n'ont pas la même forme, et les écrire pareil tromperait :**

- la **sérialisation** est une contrainte que **chaque** incrément mutant respecte, à chaque
  écriture, pour toujours ;
- la **descente** est un déplacement qui se fait **une fois** — le premier la paie, les autres
  trouvent le code en place.

Les ranger sous une même formulation ferait croire que chaque incrément déplace quelque chose.

### 3. Ce qui a été écarté

**Un incrément propre**, pris avant les autres. Rendrait la charge visible et la sortirait du chemin
critique. Écarté au test de départage : du code déplacé, aucun comportement neuf.

**La même formulation que la sérialisation**, pour n'avoir qu'une règle à retenir. Écarté au §2 :
inexact, et l'inexactitude porte précisément sur ce qui distingue les deux.

### 4. Registres

**Construit** : rien.

**Tranché, non construit** : la règle est inscrite dans la spec, au voisinage de celle de la
sérialisation.

**Question ouverte** : aucune sur la règle. **Où** le socle atterrit — dans quel projet — reste
ouvert et relève d'un plan de design.

---

## D-060 — La sortie de `Spec` reste à zéro remarque ; c'est le référentiel qui monte (2026-08-01)

Tranché par l'utilisateur après le sixième tour de `revue-spec` sur *Un agent pilote Cursus*, devant
une série qui ne descend pas : 11, 12, 16, 16, 12, 19 remarques. `cycle-feature.md` ne fait passer
`Spec` en `Human Review Requested` **que si un tour ne rend aucune remarque** ; en six tours, le zéro
n'est jamais arrivé, et la sixième valeur est la plus haute.

**L'issue proposée était d'assouplir la sortie** — sortir quand aucune remarque ne bloque le
découpage, la conformité rédactionnelle se soldant sans redemander un tour. Elle est **écartée**, et
la question retournée : *si zéro remarque est plus dur que la DoD, c'est peut-être la DoD qu'il faut
adapter*.

**Le relevé lui donne raison, et le chiffre est net.** Sur les dix-neuf remarques du sixième tour,
**deux** opposent une case de la DoD. Sept opposent une **contradiction interne**, opposable par
`revue` §3 — *quand la contradiction est interne, l'artefact est son propre référentiel* — et que la
DoD ne mentionne nulle part. Sept relèvent de la découpabilité, qui n'est pas cochable. Trois citent
`tickets.md` ou `architecture.md`. **Les dix-sept cases n'ont donc produit qu'un dixième de ce qui
a été opposé.**

**Le défaut n'est pas la sévérité de la revue, c'est que l'auteur et le relecteur ne lisent pas le
même document.** Mesuré sur les skills : `spec`, `discovery` et `plan-design` citent leur DoD
**zéro** fois ; `revue-spec` et `revue-discovery` la citent quatre et cinq fois. Aucun skill de
production ne connaît le référentiel qui le jugera — le symptôme le plus net vivant dans
`revue-spec` §2, où l'exigence de cohérence figure ⇄ prose est écrite pour le juge et pour lui seul.

**Ce que la décision fait**, dans l'ordre où cela mord :

1. **`dod/feature/spec.md` §1 gagne les trois exigences qui étaient opposées sans être écrites** —
   le document ne se contredit pas ; les faits allégués sont vrais ; toute règle issue d'une décision
   la cite. Elles ne se cochent pas en lisant une section : elles se vérifient en **confrontant deux
   endroits**, ou le dépôt. C'est pourquoi elles manquaient, et pourquoi elles se manquent en
   écrivant ;
2. **chaque skill de production renvoie à sa DoD**, en dernière étape, sur le document **fini** —
   jamais en rédigeant. Un renvoi, jamais une recopie : recopier un référentiel est ce qui a produit
   la friction 54 ;
3. **`cycle-feature.md` §4 inscrit le balayage après reprise** — une reprise produit des décisions,
   et une décision périme des phrases écrites avant elle. Six des dix-neuf remarques du sixième tour
   étaient de cette nature.

**Ce que la décision n'accepte pas.** L'objection est réelle : donner la grille à celui qui écrit
produit parfois un document qui coche plutôt qu'un bon document. Elle est traitée par le **moment** —
la DoD se lit une fois le document fini — et par ce que la DoD dit déjà d'elle-même : ce qui s'y
coche est une trace, pas un jugement, et une case qui ne se coche pas signale un binôme qui n'a pas
tranché, pas une rédaction à reprendre.

**Écarté, et pourquoi.** *Assouplir la sortie* : cela aurait rendu le cycle vert sans rien apprendre
à celui qui écrit — la même spec aurait produit les mêmes défauts au tour suivant, avec l'accord en
plus. *Supprimer les renvois `D-NNN` des documents de méthode*, envisagé au même moment : un renvoi
ADR ne meurt jamais, `decisions.md` étant append-only. La règle retenue à la place est plus étroite
et vaut partout — **un renvoi ADR ne doit jamais être nécessaire pour comprendre la phrase qui le
porte**. S'il faut ouvrir l'entrée pour savoir ce que la règle dit, c'est la règle qui est mal
écrite ; à ce compte-là, un renvoi périmé coûte une lecture inutile, jamais un contresens.

**Ce que la décision ne prétend pas.** Qu'elle fera converger la série. Elle rend le zéro **visé**,
là où il n'était pas visible ; rien ne dit qu'il sera atteint, et le seul tour qui trancherait est
un premier passage sur une spec que le binôme n'aura pas rédigée.

**Question ouverte** : le même écart existe-t-il aux deux autres niveaux ? `revue-plan` ne cite
aucune DoD, alors que `dod/story/plan-review.md` existe — mesuré, non instruit.

---

## D-061 — Le registre des runs en vol est unique, et il vit dans le socle partagé (2026-08-01)

Tranché par l'utilisateur en reprise du sixième tour de `revue-spec` sur *Un agent pilote Cursus*. La
revue opposait que la spec rangeait l'état des runs en vol **dans trois endroits à la fois** : tranché
au §3.1 (« le serveur le détient, seule entorse au sans-état »), dessiné dans le socle partagé par la
figure et par le tableau des dépendances, et déclaré **question ouverte** douze lignes plus bas. Les
deux lectures ne livrent pas la même feature.

**Le code a réduit la question avant qu'elle soit posée**, et c'est ce qui l'a rendue tranchable en un
seul arbitrage :

- `ProjectHost.LaunchAsync` reçoit son `CancellationToken` en paramètre — le host ne retient rien ;
- le `CancellationTokenSource` vit dans le ViewModel de l'écran de run, **seul endroit du dépôt qui
  détienne de quoi arrêter** ;
- **la lecture en vol ne dépend pas de cet état** : elle passe par la révision du partage de connexion
  du journal, donc par le disque. Les deux portes lisent tout run en vol dans les deux options.

La question ne portait donc pas sur *qui voit les runs* — les deux les voient dans tous les cas —
mais sur *l'agent peut-il arrêter un run lancé depuis la fenêtre*. C'est ce que le nœud du schéma
disait déjà de l'état : « un run, **de quoi l'arrêter** ».

**Tranché : un registre unique, dans le socle partagé.** Les deux portes arrêtent n'importe quel run,
quelle que soit celle qui l'a lancé. Deux motifs, et le premier est un invariant déjà écrit :

- **« Aucune seconde porte, sur aucun agrégat »** (§2.3, l'invariant qui porte la parité). Deux
  registres d'arrêt étanches sont deux portes sur le même concept ;
- **le besoin de la Discovery** — *toute observation passe par quelqu'un qui regarde l'écran et
  rapporte*. En dogfooding, un run lancé à la main serait inarrêtable par l'agent.

**Écarté : chaque porte arrête les siens.** Coût de construction nul, et la recette le permettait
telle qu'écrite — le scénario 7 posait « un run en vol, **lancé par un agent** ». La parité de
capacité (`D-056`) était même tenue : chaque porte sait arrêter. C'est l'usage qui a tranché contre,
pas la conformité.

**Ce que la décision coûte, et le découpage doit le savoir** : le ViewModel de l'écran de run cesse de
posséder son jeton d'annulation et le prend au registre. C'est le recablage que porte l'incrément qui
apporte l'arrêt — inscrit au §2.2 comme orientation technique.

**Le balayage a fait tomber un passage de plus**, et c'est le premier essai du geste que `D-060`
institue : le scénario 7 posait « lancé par un agent », ce qui suggérait que seuls les siens sont
arrêtables. Il pose désormais le cas qui prouve la règle — « lancé depuis la fenêtre ».

**Question ouverte** : aucune sur la règle. *Dans quel projet* le socle atterrit reste ouvert, et
relève d'un plan de design — c'est une question déjà listée, que celle-ci ne déplace pas.

---

## D-062 — La base d'un projet naît d'un geste explicite, jamais d'une lecture (2026-08-01)

Tranché avec l'utilisateur en reprise du sixième tour de `revue-spec` sur *Un agent pilote Cursus*.
La revue opposait trois passages inconciliables : *les requêtes ne verrouillent rien* (§2.2), *chaque
commande et chaque requête résolvent seules* (§1.3), et *est mutant tout geste écrivant… jusqu'à
l'ouverture d'un projet, fût-elle implicite, qui crée sa base et son schéma si elle manque* (§2.3).
Une requête sur un projet jamais résolu écrivait donc hors de toute commande.

**La question posée par l'utilisateur a déplacé le remède** : *est-il légitime qu'un `SELECT` crée un
fichier SQLite, alors que créer un projet est plus qu'une base — il y a au moins un JSON en plus ?*
Non, et le dépôt le dit déjà de deux façons :

- `ProjectStore.Create` pose `workflows/`, `project.json` et un `.gitignore` qui ignore `cursus.db*`
  avec ses compagnons WAL. **La création connaît la base par son nom et ne la crée pas** ;
- le commentaire de ce même `.gitignore` énonce la coupe qui tranche : *l'intention est versionnée,
  l'observation est locale*. Une base est de l'observation.

**Corollaire qui rend la règle non triviale** : `cursus.db*` étant gitignoré, **le cas courant est un
projet sans base**. Un dépôt cloné porte son `project.json` et ses workflows, jamais sa base ;
`ProjectStore.Create` n'a tourné qu'une fois, sur le poste du créateur. Le geste qui adopte un projet
sur *ce* poste est l'inscription au registre machine — c'est là que la base locale doit naître.

**Tranché** : la base est matérialisée par un geste explicite, et il y en a deux — *créer un projet*,
et *inscrire au registre de ce poste un projet déjà versionné*. Les deux sont des écritures, donc des
commandes, donc déjà sous le verrou global (`D-058`) : **aucun mécanisme neuf, pas de second verrou**.
Le journal s'ouvre en `ReadWrite` ; un fichier manquant devient une erreur nommée — *ce projet n'est
pas initialisé sur ce poste* — là où `ReadWriteCreate` rendait une base vide en silence, qui se lit
comme un projet sans historique.

**La course est dissoute, pas protégée.** Plus rien n'est écrit à la première résolution, donc il n'y
a plus à décider quel verrou la couvre.

**Ce qui reste, et c'est plus étroit** : la racine garantit **au plus un host par projet**. Le verrou
du journal protège une **connexion**, pas un fichier — deux instances vivantes pour la même base sont
deux verrous qui ne se voient pas. C'est de l'unicité de cache, pas de la sérialisation. ⚠️ Cette
garantie existe **déjà** aujourd'hui, mais par accident : la coquille de la fenêtre n'ouvre qu'un
projet à la fois, et aucun type ne l'exprime. La feature la rend explicite parce qu'elle l'ôte.

**Deux erreurs relevées en chemin, et toutes deux par le balayage de `D-060`.**

1. L'inscription d'un projet existant **manquait à l'inventaire du §1.2**, alors que la fenêtre
   l'expose — `OpenOrCreateProject` inscrit, et bifurque vers la création si le dossier ne porte pas
   de `.cursus/`. Un trou de la parité que six tours de revue n'avaient pas relevé, trouvé en
   instruisant une autre remarque.
2. La session appelante a tenu, à plusieurs reprises, que l'absence de `busy_timeout` rendrait un
   `SQLITE_BUSY` immédiat. **L'annexe C de la spec mesurait l'inverse** — la bibliothèque réessaie
   une trentaine de secondes. Deux créations concurrentes aboutissent donc toutes deux, et le danger
   n'est pas l'échec visible mais la dégradation différée que laissent deux instances. La conclusion
   tenait, l'argument non — le motif exact de la case *les faits allégués sont vrais* que `D-060`
   venait d'inscrire dans la DoD.

**Écarté.** *Faire prendre le verrou global aux requêtes* : cela marchait, au prix de sérialiser
toute lecture avec toute écriture pour un cas qui ne survient qu'une fois par projet et par
démarrage. *Un verrou interne à la racine* : recevable — la revue l'écartait au motif qu'il serait
« un verrou que chaque appelant devrait penser à prendre », ce qui est faux, un tel verrou étant pris
par la racine et non par l'appelant ; il devient simplement sans objet une fois la course dissoute.

**Question ouverte** : que fait l'inscription d'un projet dont la base existe déjà — rien, ou une
vérification de schéma ? Il n'existe aucun mécanisme de migration dans le dépôt, et cette décision
n'en crée pas. Relève d'un plan de design.

---

## D-063 — L'enregistrement d'un brouillon périmé échoue, plutôt que d'écraser en silence (2026-08-02)

Tranché par l'utilisateur en reprise du sixième tour de `revue-spec` sur *Un agent pilote Cursus*.
Le constat venait de la seule remarque **hors mandat** du tour — un point de justesse, qui n'a par
définition aucun référentiel dans la DoD et revient donc à l'humain.

**Le défaut.** Le verrou global garantit qu'aucune écriture n'en corrompt une autre. Il ne garantit
pas qu'un geste **survive**. L'éditeur de la fenêtre ouvre son brouillon au montage, le mute en
mémoire, et son enregistrement repose la définition **entière** : il écrase donc l'étape que l'agent
a ajoutée entre-temps. Les deux écritures sont sérialisées et le graphe reste cohérent — **la clause
du scénario 4 est satisfaite pendant que le geste de l'agent disparaît en silence.**

**Ce qui a emporté l'arbitrage** : le cas n'est pas de bord. La feature existe pour qu'un agent
travaille pendant que l'humain regarde ; un éditeur ouvert sur un workflow que l'agent modifie est
donc le cas **nominal**, et le défaut tombe exactement sur le scénario d'usage visé.

**Tranché** : l'enregistrement d'un brouillon **périmé** échoue, en le disant. L'éditeur retient à
l'ouverture de quoi reconnaître la version qu'il a lue et la confronte avant d'écrire. **Aucune
fusion** — la règle rend le conflit *visible*, elle ne le résout pas. Elle vaut pour toute écriture
qui repose un document complet ouvert plus tôt, quelle que soit la porte.

**Ce que ça coûte, et le découpage doit le savoir** : un enregistrement peut désormais **échouer**,
ce qui n'arrive jamais aujourd'hui. Les deux portes doivent savoir le dire — l'écran par un message
et un rechargement proposé, l'outil MCP par une erreur exploitable.

**Le motif existe déjà dans le dépôt, et sa parade n'est pas transposable.** `architecture.md` note
que tout écrivain partiel de `project.json` relit le disque avant d'écrire, sans quoi un renommage
depuis un instantané effaçait la déclaration en silence. Ici l'éditeur n'écrit pas partiellement : il
repose un document entier. La relecture ne suffit donc pas, d'où la détection.

**Écarté.** *Accepter au premier jour et l'écrire comme risque connu* : coût nul, mais le défaut
frappe le cas d'usage central. *Le porter en carte séparée* — l'argument était que le problème
préexiste sous la forme fenêtre-contre-fenêtre et que la feature l'expose plutôt qu'elle ne le crée ;
écarté parce que rien n'aurait garanti la prise de la carte avant la mise en service.

**Question ouverte** : la forme de ce que l'éditeur retient — horodatage, empreinte, ou numéro de
version porté par la définition. Relève d'un plan de design.

---

## D-064 — Un projet inscrit dont la base a disparu ne s'ouvre pas, et le dit (2026-08-02)

**Contexte.** `D-062` a fait de l'ouverture d'un projet une lecture pure : la base naît de deux
gestes explicites — *créer un projet*, *inscrire au registre de ce poste un projet déjà versionné* —
et plus jamais du premier accès. Le motif était la concurrence : une course qu'on **dissout** vaut
mieux qu'une course qu'on protège.

**Ce que la revue a opposé.** L'arbitrage a une conséquence d'expérience que `D-062` ne prononce
pas, et qui ne relève pas de la concurrence : un projet **déjà inscrit** dont le fichier de base a
disparu — dépôt recloné, dossier nettoyé, machine changée — cesse de s'ouvrir et exige un geste
explicite. Le cas est nominal, non exceptionnel : `cursus.db*` est gitignoré, donc **tout dépôt
cloné arrive sans base**. La spec tranchait cela en passant, dans une phrase de plan technique.

**Tranché** : le comportement est confirmé, et prononcé pour lui-même. Un fichier manquant est une
**erreur nommée** — *ce projet n'est pas initialisé sur ce poste* — sur les deux portes.

**Le motif, qui n'est pas celui de `D-062`.** Une base vide silencieuse **ment** : elle se lit comme
un projet sans historique, et rien ne distingue pour l'usager « je n'ai jamais rien lancé ici » de
« mes runs sont sur l'autre machine ». L'erreur nommée dit la vérité et se répare ; le silence égare
et ne se répare pas, parce que personne ne sait qu'il y a quelque chose à réparer.

**Ce que ça engage** : un scénario de recette de plus (le dixième), qui tombe sur le premier
incrément mutant — celui qui fait de l'ouverture une lecture pure, donc celui qui fait apparaître
l'erreur. Sans lui, cet incrément livrerait sur les deux portes un comportement que `Validation` ne
rejouerait pas : la charge de parité prouve qu'un outil **répond**, jamais qu'il répond ce qu'on
attend.

**Écarté.** *Assumer sans recetter* — cohérent avec l'idée que la recette ne couvre pas tout cas
d'erreur, mais le découpage n'aurait alors rien à donner en acceptation à l'incrément qui change le
mode d'ouverture. *Adoucir en proposant la matérialisation à l'ouverture* — plus doux pour l'usager
d'un dépôt fraîchement cloné, écarté parce que cela réintroduit l'écriture au premier accès que
`D-062` venait de supprimer, avec la course qu'elle porte.

**Question ouverte** : par quel geste l'usager remet le projet en état. Les deux gestes qui
matérialisent une base supposent un projet *pas encore inscrit*, et celui-ci l'est.

---

## D-065 — La recette de concurrence se répartit sans bijection : zéro, une, ou plusieurs clauses par incrément (2026-08-02)

**Contexte.** La spec d'*Un agent pilote Cursus* posait une règle d'atterrissage pour son scénario
de concurrence : ses clauses portent sur des objets, et **chaque incrément mutant emporte la clause
de l'objet qu'il livre, et celle-là seule**. Le motif tient toujours — sans elle, le scénario
tomberait entier sur le dernier incrément, dont l'acceptation porterait sur du code recetté
plusieurs incréments plus tôt.

**Ce que la tentative de découpage a montré.** La règle supposait une correspondance un-à-un entre
objets et incréments, et elle échoue dans les deux sens :

- **un incrément, deux clauses** — le registre des projets et la base d'un projet naissent des deux
  **mêmes** gestes (`D-062`), donc aucune frontière de découpage ne les sépare ;
- **un incrément, aucune clause** — créer, renommer et supprimer un workflow, ou déclarer une
  liaison de tracker, rendent un incrément mutant sans qu'aucune clause porte leur objet.

**Tranché** : la règle devient *chaque incrément mutant emporte les clauses des objets qu'il rend
écrivables — zéro, une, ou plusieurs*. L'exclusivité tombe, la bijection avec elle.

**Le corollaire qui compte le plus** : un incrément mutant sans clause est **conforme**, et il ne
faut pas lui en inventer une. Une clause de recette naît d'un risque de concurrence identifié,
jamais d'une case à remplir — la règle décrit un atterrissage, elle ne distribue pas des quotas.

**Écarté.** *Rétablir la bijection en élargissant la recette* — deux clauses de plus pour les objets
qui n'en ont pas ; écarté parce qu'elles auraient été écrites pour satisfaire une règle, non pour
couvrir un risque, et qu'une recette qui grossit ainsi cesse d'être opposable. *Déclarer solidaires
les deux clauses qui visent le même objet* — sauve le compte, laisse l'échec de l'autre sens intact.

---

## D-066 — Le serveur MCP exige un jeton, parce que le loopback n'est pas une frontière de confiance (2026-08-02)

**Contexte.** La spec d'*Un agent pilote Cursus* pose que le serveur émet un jeton en montant et le
publie là où le client trouve déjà l'endpoint. La règle était tranchée, recettée — le scénario 6
exige qu'un appel sans jeton valide soit refusé — et **son motif n'était écrit nulle part**. Une
relecture l'a relevé : ses voisins du même registre citent tous leur entrée, celle-ci n'en avait pas.

**Ce que le motif JetBrains couvre, et ce qu'il ne couvre pas.** L'hébergement — l'instance qui
tourne porte le serveur, le stdio n'étant qu'un proxy mince — et la **publication** de l'endpoint,
généré par l'application parce qu'il dépend de l'instance : ces deux-là sont repris de JetBrains et
vérifiés. ⚠️ **Le jeton, lui, ne l'est pas.** La documentation de l'IDE décrit une configuration à
copier, sans qu'un jeton y apparaisse ; rien ne permet de dire qu'il en émet un. Le jeton est **notre
ajout**, et l'emprunt ne s'étend pas jusque-là.

**Tranché** : le serveur émet un jeton au montage, l'exige à chaque appel, et le publie par le même
canal que l'endpoint. Aucune saisie, rien au trousseau, et il meurt avec la session.

**Le motif** : une écoute en loopback **n'est pas une frontière de confiance**. Tout ce qui tourne
sur la machine peut atteindre `127.0.0.1`, y compris une page ouverte dans un navigateur qui forge
des requêtes vers un port local. Or ce que le serveur expose n'est pas une lecture anodine : c'est la
parité complète — lancer des runs, écrire des workflows, déclarer des liaisons. Le jeton distingue
**le client que l'humain a configuré** de *n'importe quoi qui tourne sur la machine*, et il coûte une
ligne au montage.

⚠️ **Ce qu'il ne protège pas, et il ne faut pas le surestimer.** Un process qui s'exécute avec les
droits de l'utilisateur peut lire le canal de publication, donc le jeton. La frontière tenue est
celle du **process qui ne peut pas lire ce fichier** — au premier chef le code exécuté dans un
navigateur, qui forge des requêtes réseau mais ne lit pas le disque local.

**Écarté.** *Aucun jeton, le loopback suffit* — c'est la position que le motif ci-dessus réfute : le
loopback borne l'origine réseau, pas l'appelant. *Un jeton saisi par l'humain et rangé au trousseau*
— `ISecretStore` existe pour les secrets qu'un tiers émet (un tracker) et que l'humain doit
transcrire ; celui-ci est produit par la machine, personne n'a à le connaître, et le faire saisir
ajouterait un geste sans rien protéger de plus. *Un jeton persistant entre sessions* — il faudrait
alors le révoquer et le renouveler ; mourir avec la session rend ces deux mécanismes inutiles.

**Question ouverte** : la forme de la publication — un fichier de configuration, un réglage affiché à
copier, ou les deux. Elle est commune à l'endpoint et au jeton, et la spec la porte déjà comme telle.

## D-067 — Une remarque de revue vraie n'est pas automatiquement une remarque à traiter (2026-08-02)

**Contexte.** La spec d'*Un agent pilote Cursus* a été relue **huit fois**, produisant 11, 12, 16,
16, 12, 19, 12 puis 13 remarques. La série ne converge pas, alors que le cycle de revue exige **zéro
remarque** pour sortir. Trois causes étaient plausibles — une feature trop grosse, un processus de
spec sur-exhaustif, un critère de sortie qui ne termine pas.

**La mesure qui tranche.** Les 124 remarques posées ont été classées une à une par scope, portée et
survie au contact du code (`docs/methode/rex/2026-08-02-analyse-serie-revue-spec.md`). Sur les 111
remarques de spec :

- **une seule** relève de l'architecture, treize de la faisabilité, et ces deux scopes s'épuisent
  après le troisième tour — 23 % des remarques des tours 1 à 3, 7 % des tours 4 à 8 ;
- **68 sont invisibles au code** : rien dans une implémentation ne les aurait signalées. Vingt
  seulement auraient coûté cher découvertes tard ;
- **69 % visent du texte écrit par une reprise antérieure**, et 92 % sur les deux derniers tours. Le
  nombre de remarques nées de la reprise vaut **6 aux tours 6, 7 et 8**, sur des récoltes de 19, 12
  et 13.

**Le fait qui explique le mécanisme, et il vise le binôme.** **98 remarques retenues sur 98, aucun
refus motivé jamais enregistré.** Chaque objection était traitée comme un fait ; chaque reprise
écrivait du texte neuf ; le tour suivant y trouvait sa récolte. La boucle ne divergeait pas parce que
l'artefact était mauvais — **elle s'alimentait elle-même**.

**Tranché** : une remarque de revue est retenue si elle change **ce qu'on va construire** ou **ce que
le découpage peut décider**. Une remarque vraie qui ne change que la façon dont le document se lit
est **refusée, avec son motif écrit en fil**. Le refus devient une issue de plein droit, à côté de la
reprise.

**Ce que cela ne dit pas.** Ce n'est pas une licence à ignorer les remarques gênantes : le refus se
motive, en fil, sur la carte, et il est aussi opposable que la reprise. Une remarque refusée reste
vraie — c'est sa conséquence qui est jugée nulle, pas son constat.

**Deux corollaires.**

- **Le critère de sortie du cycle change de nature.** « Zéro remarque » redevient atteignable, parce
  que solder inclut désormais refuser. Sans cela, le critère exigeait qu'un document cesse d'appeler
  des remarques, ce que rien ne garantit d'un texte long relu par un agent frais.
- **La découpabilité ne se règle pas par ce chemin.** C'est le seul scope qui ne s'épuise pas — 36
  remarques en huit tours, dont les 5 du tour 8, toutes structurantes. Elle ne se prouve pas en
  relisant : ses remarques deviennent des **entrées du découpage**, tranchées en coupant puis en
  codant.

**Écarté** : *continuer jusqu'à zéro* — la mesure montre que le point fixe n'existe pas tant que la
reprise engendre sa propre récolte. *Réduire la taille de la feature* — une spec deux fois plus
petite bouclerait pareil, avec trois remarques par tour au lieu de six ; le mécanisme est
indifférent à la taille. *Supprimer la revue de spec* — ses deux premiers tours ont rendu
l'architecture et la faisabilité, qui sont exactement ce qu'on ne veut pas découvrir dans le code.

**Question ouverte** : où s'arrête la série. Rien ici ne fixe un nombre de tours ; le critère est
qualitatif — quand un tour ne rend plus que des remarques invisibles au code, il est le dernier.

---

## D-068 — Le découpage tranche ce que la spec lui avait laissé, et une relecture lui reprend une frontière (2026-08-02)

**Contexte.** La spec d'*Un agent pilote Cursus* a été découpée le jour même de son `Done`. Cinq
remarques du huitième tour de revue avaient été soldées avec le motif *« ça se tranche en coupant »* :
elles attendaient donc le découpage, et aucune n'avait de référentiel ailleurs. Le découpage a produit
dix-sept incréments (`CUR-47` → `CUR-63`) plus une carte latérale (`CUR-64`), les 31 lignes atteignables
de l'inventaire réparties sans reste.

### 1. Ce qui est tranché, et par qui

**Le plus petit incrément recettable gagne, toujours** — énoncé par l'utilisateur, et c'est la règle la
plus large de cette entrée : *« à chaque fois que l'on peut avoir un petit incrément recettable, on le
prend »*, parce qu'un petit incrément **traverse le flux plus facilement**. Appliquée, elle a fait
passer le découpage de neuf incréments à dix-sept. La coupe se fait entre une **lecture** et une
**écriture**, ou entre deux objets que le rôle produit nomme séparément. ⚠️ **Sa borne** : trois lignes
d'un même objet dont aucune ne se distingue pour le rôle produit ne se coupent pas — le seul cas
assumé, celui des connexions tracker.

**Un incrément en lecture seule précède le premier mutant, et il porte la porte entière** — hôte HTTP,
adaptateur MCP, activation, jeton. Sans elle, aucune ligne n'est atteignable, donc aucun incrément
n'est recettable : la porte ne pouvait pas arriver après. La mettre dans le premier mutant en aurait
fait une carte portant la porte, le socle, la couche, le verrou et la base d'un coup.

**Le registre des runs en vol naît avec le lancement, pas avec l'arrêt.** La commande de lancement doit
de toute façon rendre l'identifiant sans attendre la fin du run : elle tient déjà quelque chose, et
elle inscrit chaque run **avec de quoi l'arrêter**. L'incrément de l'arrêt ajoute le **geste** — au
noyau, où il n'existe pas — et le recablage de l'écran ; il ne crée ni le registre ni son contenu.
L'inverse aurait fait revenir un incrément sur du code livré deux incréments plus tôt.

**Le recablage de la fenêtre est une charge propre à chaque incrément, pas une clause de recette.**
Chaque incrément qui recable emporte *les gestes correspondants de la fenêtre continuent de
fonctionner*, jumelle de la charge de parité. **Écarté** : un scénario de non-régression ajouté à la
recette — il aurait fallu rouvrir une spec `Done`, et la clause serait tombée entière sur le dernier
incrément, dont l'acceptation aurait alors porté sur du code recetté bien plus tôt.

**La remise en état d'un projet dont la base a disparu est *retirer puis réinscrire*.** Aucune ligne
n'est ajoutée à l'inventaire, donc le référentiel opposable de la parité ne bouge pas. **Écarté** : un
geste dédié — plus direct pour l'usager, mais il ajoutait une ligne au référentiel, ce qu'un plan de
design n'a pas le droit de faire et ce que seul ce moment-ci pouvait décider.

### 2. Ce qu'une relecture a repris au découpage, et le motif

Le découpage avait d'abord placé dans l'incrément en lecture seule le **passage de l'ouverture en
lecture pure** et le scénario du projet dont la base a disparu — conséquence mécanique de la coupe,
puisque c'est cet incrément-là qui rendait l'ouverture pure. **C'était un défaut, et il est rendu au
premier incrément mutant**, où la spec l'adressait déjà.

**Le fait qui l'établit, vérifié dans le code** : la création d'un projet pose `workflows/`,
`project.json` et le `.gitignore`, et **ne crée pas la base** ; c'est le constructeur du journal, en
`ReadWriteCreate`, qui la matérialise au premier accès. Passer l'ouverture en `ReadWrite` **avant** les
deux gestes qui matérialisent la base laissait donc un intervalle où **aucun projet neuf ni fraîchement
cloné ne s'ouvrait** — ni par l'agent, ni dans la fenêtre. La clause de non-régression de l'incrément
ne l'attrapait pas : elle ne visait que des projets préexistants, qui ont déjà leur base.

⚠️ **Ce que le cas apprend, et qui vaut au-delà** : une coupe entre deux incréments **déplace les
comportements avec elle**, et un comportement déplacé peut cesser d'être réparable. Ce n'est pas la
frontière qui était fausse — l'incrément en lecture seule reste le bon premier — c'est le contenu qui
l'avait suivie sans qu'on vérifie ce qu'il laissait derrière lui.

**Écarté** : *rendre l'incrément de la porte mutant sur ce seul point* — il aurait écrit, ce qui
détruit la propriété qui justifiait la coupe ; *fusionner les deux incréments* — retour à la carte
monstre que la coupe existait pour éviter.

### 3. Registres

**Construit** : rien. Le découpage ne livre que des cartes.

**Tranché, non construit** : les cinq arbitrages du §1, et le déplacement du §2. Ils vivent dans les
cartes, et cette entrée les rend opposables ailleurs qu'à travers elles.

**Question ouverte** : aucune sur ces points. Celles que les incréments portent — où atterrit le socle,
la forme de la publication de l'endpoint, la forme du témoin de version, ce que fait l'inscription d'un
projet dont la base existe — relèvent des plans de design, et chaque carte nomme celle qui la ferme.
