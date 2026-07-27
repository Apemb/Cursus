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
