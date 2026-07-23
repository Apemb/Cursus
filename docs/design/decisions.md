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
