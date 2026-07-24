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
réécrire la traversée. Acté par deux commits au même horodatage (`a74d3cc` doc,
`e683139` code).

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
- *Garder le `Task.Run` (le correctif précédent, `daf8750`)* — déguise le mensonge au lieu de le
  réparer ; c'est ce que l'utilisateur a justement refusé.
- *`ProvisionAsync` async mais `Dispose` synchrone (`IDisposable`)* — le démontage resterait un
  sync-over-async : à moitié corrigé, et incohérent. D'où `IAsyncDisposable`.
- *`ConfigureAwait(false)` sans rendre le provisionnement async* — ne corrige rien : le blocage est
  dans le préfixe *synchrone*, avant tout `await`.

**Conséquences.** Refactor de forme : comportement identique, 210 tests verts après passage des
signatures et des doubles aux formes async (`await using`, `ProvisionAsync`, `DisposeAsync`,
`ThrowsAsync`). L'écriture du journal court désormais sur un thread du pool (déjà le cas depuis
`daf8750`) ; la lecture concurrente d'un run en cours reste non supportée (connexion SQLite unique,
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
