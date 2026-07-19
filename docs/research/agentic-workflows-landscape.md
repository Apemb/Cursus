# Recherche — Paysage & abstractions des workflows agentiques

> Recherche de conception pour Cursus (manageur de workflow agentique local-first, C#/.NET 10 + Avalonia, terminal natif — un PTY vivant par session façon TMUX).
> Vague 1 : 6 axes explorés en parallèle. Objectif : comprendre ce qu'est un workflow agentique, quelles abstractions/API il représente, et en déduire le modèle de `Cursus.Core`.
> Date : 2026-07-19.

---

## TL;DR — les 6 enseignements qui comptent

1. **Cursus est dans une niche rare et très récente.** La quasi-totalité du champ isole par **git worktree** (défaut de facto) ou **container**. Le cluster « multiplexeur de PTY/session » (le nôtre) est minuscule et dominé par **Herdr** (« tmux pour agents », apparu juin 2026, 0→15k stars en 3 mois) — notre **jumeau conceptuel**. Cursus serait le **seul de ce cluster en desktop natif .NET/Avalonia** (les autres sont TUI Rust/Go ou Electron).
2. **Tâche > Session.** Enseignement le plus fort (Symphony, Conductor.build, Herdr) : la **session** (le PTY/agent) est un **détail d'exécution jetable** ; l'unité durable et l'unité de supervision, c'est la **Tâche**. Une tâche → 0..N sessions, 0..N PR. Une session qui plante se relance, la tâche survit.
3. **Séparation définition ⟷ exécution, partout.** Abstraction la plus consensuelle de tous les frameworks (LangGraph, Temporal, MAF, CrewAI, Conductor) : un objet déclaratif versionnable (`WorkflowDefinition`) vs une instance identifiée et persistée (`WorkflowRun`). À copier tel quel.
4. **Deux machines à états séparées.** États **métier** visibles par l'humain (Backlog→In Progress→Human Review→Done) vs états **d'orchestration** internes (Unclaimed/Claimed/Running/RetryQueued). Ne pas polluer l'un avec l'autre (Symphony).
5. **HITL = suspension durable + reprise par injection de valeur.** Forme identique dans TOUS les frameworks : `interrupt()`→`resume` (LangGraph), tool approval→`state.approve()` (OpenAI), `WaitCondition`+Signal (Temporal), `RequestPort` (MAF). C'est **le** pattern central pour la supervision humaine de Cursus.
6. **Le cœur d'orchestration reste neutre.** Conductor (Netflix) a ajouté l'IA **sans rien réécrire** : un agent n'est qu'un workflow combinant des « LLM tasks » et des opérateurs classiques. Ne pas câbler l'IA en dur dans le moteur ; garder une **step générique**.

---

## Cartographie du champ (7 familles)

| Famille | Isolation | Exemples | Rapport à Cursus |
|---|---|---|---|
| **Multiplexeurs de terminal agent-natifs** 🎯 | PTY/session (souvent tmux dessous) | **Herdr**, Claude Squad, agent-deck, superset | **Notre niche.** Modèle « 1 PTY par session + état d'agent ». |
| **Apps desktop à worktrees** | git worktree | Conductor.build (Mac, proprio, 22 M$), **Emdash** (Electron OSS), Nimbalyst/Crystal | Cluster le plus dense et financé. Concurrents directs en surface. |
| **Kanban d'agents** | worktree | Vibe Kanban (Bloop, fermé→OSS), agent-kanban | Abstraction « carte = tâche d'agent », pilotage MCP. |
| **Isolation containerisée** | container Docker | **Sculptor** (Imbue, local), container-use (Dagger, MCP) | École « container > worktree ». Même ADN local-first que nous. |
| **Plateformes cloud async** | VM/sandbox cloud | Terragon (fermé), Cursor Background Agents, Devin, Google Jules | Anti-thèse de Cursus (cloud, non supervisé live). |
| **Frameworks / swarms autonomes** | variable | OpenHands, claude-flow, famille « Ralph » | Agents autonomes ; Cursus les orchestrerait, ne les remplace pas. |
| **Dispatch piloté par issue-tracker** | worktree/task | **Symphony** (OpenAI/Linear), Baton (GitHub Issues), MS Conductor (YAML) | Modèle « issue = run d'agent ». Couche tâche au-dessus des sessions. |

**Tendances 2025-2026 :** worktree = isolation par défaut ; multi-provider agnostique = table-stakes ; **MCP** comme couche d'outils/isolation standard ; **« état d'agent »** (blocked/working/done/idle) émerge comme primitive UI (la lacune de tmux) ; tension UX explicite *terminal-natif live* vs *GUI qui abstrait le terminal* ; **persistance de session** (detach/reattach) attendue ; forte **volatilité** (leaders fermés début 2026, survie par l'OSS).

---

## Fiches par cible

### A. swamp-club/swamp — ⚠️ hors-cible mais instructif
Ce n'est **pas** un orchestrateur d'agents de code : c'est un framework d'**automatisation déterministe *opéré par* des agents** (l'agent écrit du YAML ; Swamp ne lance/isole/supervise aucun agent). TypeScript/Deno, AGPL-3.0, local-first, très actif.
- **À retenir :** séparation **type (code TS) ⟷ définition (YAML déclaratif reviewable en Git)** pensée pour l'IA ; état versionné dans `.swamp/` committé ; **data artifacts immuables versionnés avec *lifetimes*** (ephemeral/job/workflow/durée/infinite + GC) ; **ApprovalDecision** comme donnée de première classe ; secrets par référence (vault), jamais inline ; modèle `Run/JobRun/StepRun` avec machine à états explicite ; DAG (`topological_sort` + `concurrency` + `forEach`).
- **Contraste utile :** aucune isolation OS (ni PTY, ni worktree, ni container) — donc **pas de source d'inspiration d'isolation**, mais bon patron de persistance/domaine.

### B. Conductor.build (Melty, YC S24) — le modèle UX cible
App macOS proprio (gratuite, BYO-subscription), lance N agents (Claude Code/Codex/Cursor) en parallèle, **un worktree git par tâche**. Très actif (v0.76, juil. 2026).
- **Abstraction centrale : `Workspace`** = 1 worktree + 1 branche + 1 agent (+ contexte conversation) + 1 terminal + sa vue diff. « Un workspace par feature/ticket ». Nommage auto par noms de villes.
- **Cycle de vie :** créer (depuis rien / branche / PR / **ticket Linear**) → **setup script** (install deps, copie `.env`) → agent tourne (modes Plan/Fast/Supervised) → **run script** (dev server/tests, port via `$CONDUCTOR_PORT`) → **revue diff-first** (commentaires inline, l'agent répond) → PR 1 clic → merge → archive.
- **Config :** `.conductor/settings.toml` **commité**, avec précédence en couches (managed/local/repo/user/defaults, façon VS Code). **MCP délégué à l'agent host** (Conductor n'invente pas de protocole). Intégrations GitHub + Linear.
- **À copier :** le triptyque **setup/run/archive scripts commités + injection de port** (ce qui rend N worktrees *testables* en parallèle) ; **diff-first** comme surface de supervision ; persistance minimale déléguée à git.
- **Pièges signalés :** `git worktree remove` échoue si un process tient des locks → **tuer proprement le PTY avant remove** ; untracked non copié → setup script obligatoire ; N PTY = collisions de ports + explosion mémoire/CPU → **plafond configurable** ; goulot de revue humaine (4 agents = 4× à relire).

### C. Netflix / Orkes Conductor — le vocabulaire d'orchestration éprouvé
Moteur d'orchestration durable (Java, Apache 2.0, ~32k stars), virage « agentique » assumé 2024-2026 **greffé comme de nouveaux task types**, sans réécriture.
- **Double distinction définition ⟷ exécution à deux niveaux :** `WorkflowDefinition`/`WorkflowExecution` ET `TaskDefinition` (gabarit : timeout, retries, rate limit) / `TaskConfiguration` (usage dans un workflow, `taskReferenceName`). SDK **C# officiel**.
- **Vocabulaire de cycle de vie à emprunter tel quel :** `Running/Completed/Failed/Paused/Terminated` + **Pause/Resume, Retry-from-failure** (reprend à la tâche échouée), **Restart** (tout), **Rerun** (même ID, inputs changés).
- **Petit jeu d'opérateurs de contrôle de flux** (grammaire de composition) : `SWITCH`, `FORK/JOIN`, **`DYNAMIC FORK`** (N branches, nombre décidé au runtime — fan-out data-driven, parfait pour « un planner découpe en N sous-tâches → N agents »), `DO_WHILE` (boucle d'agent), `SUB_WORKFLOW`, `HUMAN`/`WAIT` (points d'intervention).
- **Câblage par références** `${taskRef.output.champ}` + workflow variables partagées ; **état durable → pause quasi-gratuite**.
- **À écarter :** toute la couche scale distribuée (Cassandra/Kafka/RBAC) ; polling réseau ; system tasks cloud. Une persistance **SQLite** suffit (Conductor la supporte — preuve que le modèle descend en local).

### D. Paysage (scan) — voir cartographie ci-dessus
**Découverte majeure : Herdr** = jumeau conceptuel (PTY multiplexer + **état d'agent** primitive UI + sessions persistantes detach/reattach + Socket API). Autres proches : **Claude Squad** (worktree+tmux+TUI), **Emdash** (desktop OSS multi-provider, worktree, `$EMDASH_PORT`, SSH remote), **Sculptor** (container local, ADN local-first/ownership). **container-use** (Dagger) = brique d'isolation MCP *intégrable* plutôt que concurrente.

### E. OpenAI Symphony + Linear — la couche « Tâche »
`openai/symphony` : **public, Apache 2.0** (~avril 2026). Pas d'abord un produit, mais un **`SPEC.md`** + implémentation de référence Elixir/BEAM. Bâti sur **Linear comme control plane** (mais tracker-agnostique via adaptateur). Slogan : *« manage work instead of supervising coding agents »*.
- **Contrat d'adaptateur minimal (2 fonctions) :** `fetch_issues_by_states`, `fetch_issues_by_ids` + une **issue normalisée** (id, identifier, title, state, labels, `dispatchable`, `blocked_by`). Preuve qu'on peut brancher un tracker externe OU un store interne derrière la même interface.
- **Deux plans d'états séparés :** orchestration interne (`Unclaimed→Claimed→Running→RetryQueued→Released` + 11 phases d'attempt) invisibles ; états **métier** dans le tracker (Todo→In Progress→**Human Review**→Done). **Le succès d'un agent ≠ fermeture de la tâche** (handoff vers Human Review).
- **Pull, pas push :** on met une issue en état actif ; l'orchestrateur en **polling** (30 s) la ramasse, la *claim*, crée un **workspace isolé par issue**, lance le sous-processus agent. **Orchestrateur sans DB durable** : état re-dérivé du tracker + filesystem au redémarrage.
- **Policy-as-code** `WORKFLOW.md` versionné (front-matter YAML + template de prompt) : tracker config, `polling`, hooks `after_create`/`after_run` (clone/commit/push/PR), `max_concurrent_agents` (+ **par état**), timeouts de stall.
- **Linear « for Agents » :** OAuth `actor=app`, scopes `app:assignable`/`app:mentionable` ; **`AgentSession`** (cycle de vie) + **`AgentActivity`** (thought/action/response/elicitation) ; l'humain reste *assignee*, l'agent est *contributor* ; commentaires = canal de supervision. **Pas de Claude Code natif** → runner OSS **Cyrus** comble le trou.

### F. Frameworks d'orchestration — le modèle mental pour `Cursus.Core`
Comparaison LangGraph / OpenAI Agents SDK / Temporal / Semantic Kernel+MAF (.NET) / CrewAI (+ ADK, Mastra, Pydantic AI, Claude Agent SDK).

**Tableau des vocabulaires (correspondances) :**

| Concept | LangGraph | OpenAI SDK | Temporal | MAF / SK (.NET) | CrewAI |
|---|---|---|---|---|---|
| Définition | `StateGraph` compilé | `Agent`+runner | Workflow Definition | `WorkflowBuilder` / `AIAgent` | `Crew`/`Flow` |
| Exécution | thread (`thread_id`) | run + `Session` | Workflow Execution (`WorkflowId`+`RunId`) | run + `AgentThread` | `kickoff()` |
| État partagé | State + channels/reducers | `RunContext`+Session | Event History (replay) | `AgentThread`+`ChatHistory` ; `WorkflowCheckpoint` | Flow `state`+Memory |
| Checkpoint | par super-step | `RunState.to_json` | event sourcing | `WorkflowCheckpoint` par superstep | `@persist`/`replay` |
| Pause humaine | `interrupt()`→`resume` | tool approval→`state.approve()` | `WaitCondition`+Signal | `RequestInfoExecutor`/`RequestPort` | `human_input=True` |
| Handoff | `Command(goto=)` | `handoff()` | Child Workflow | `HandoffOrchestration` | `allow_delegation` |
| Outil / MCP | `@tool` / adapters | `@function_tool` / `MCPServer*` | Activity | `[KernelFunction]` / `ModelContextProtocol` | `BaseTool` / `MCPServerAdapter` |

**Patterns consensuels (10) :** séparation définition/exécution ; état identifié par un ID de continuité (`thread_id`/`WorkflowId`/`AgentThread`) ; **deux niveaux de mémoire** (State éphémère intra-run vs Memory durable cross-run) ; checkpoint par étape → reprise (⇒ **idempotence des steps**) ; **HITL = suspension durable + reprise par injection** ; handoff = cas particulier d'appel d'outil ; deux styles d'orchestration composables (déclaratif/graphe vs impératif/émergent) ; frontière déterministe / effets isolés ; extensibilité tools + **MCP** quasi-universel ; interception/filtres pour guardrails (veto en n'appelant pas `next`).

**.NET — quoi adopter :**

| Package | Rôle pour Cursus | Verdict |
|---|---|---|
| `Microsoft.Extensions.AI` (`IChatClient`) | Abstraction LLM (pour les appels que Cursus fait lui-même : routeur, résumés) | **Adopter — fondation, GA stable** |
| `Microsoft.Agents.AI` / **MAF Workflows** (`WorkflowBuilder`, `Executor`, `WorkflowCheckpoint`, `RequestInfoExecutor`) | Graphe typé + checkpoint par superstep + HITL de 1ʳᵉ classe | **S'inspirer fortement** (référence de conception, backend optionnel — pas dépendance imposée) |
| `ModelContextProtocol` (+ `AsKernelFunction()`) | Outils MCP en .NET | **Adopter — extensibilité** |
| Filtres SK (`IFunctionInvocationFilter`) | Modèle d'interception approbation/veto | **S'inspirer** |
| SK Process Framework, `AgentGroupChat`, AutoGen | — | **Éviter** (expérimental/déprécié/convergé dans MAF) |

---

## Recommandations pour `Cursus.Core`

**Différence structurante à ne jamais perdre de vue :** contrairement à tous ces frameworks, l'unité d'exécution de Cursus n'est **pas** un appel `IChatClient`/LLM — c'est une **session = un PTY vivant** exécutant un agent de code (type Claude Code) comme **boîte noire**. Donc les abstractions « Agent = LLM + tools + instructions » **ne mappent pas** directement. En revanche, les abstractions de la **couche orchestration** (graphe d'executors, checkpoint, HITL) mappent très bien — à condition de traiter une session PTY comme un **executor/activity opaque et durable**.

**Modèle mental retenu :** *un graphe d'executors durables (façon MAF/LangGraph) au-dessus de sessions PTY traitées comme des activités opaques journalisées (façon Temporal), avec état checkpointé par step, time-travel/fork par `RunId`+`checkpointId`, et HITL de première classe par suspension durable + reprise sur injection de décision.*

1. **Entités du domaine (séparation définition/exécution, non négociable) :**
   - `WorkflowDefinition` — déclaratif, versionné, hashable : graphe de `StepDefinition` + arêtes (séquence/branch/parallèle/sous-workflow).
   - `WorkflowRun` — instance, `RunId` stable ; porte l'état + le journal.
   - `TaskItem` — **l'unité durable de supervision** (au-dessus des sessions) ; une tâche → 0..N sessions, 0..N PR ; états métier `Backlog→Ready→InProgress→HumanReview→Done`.
   - `Session` — un `StepInstance` dont l'executor est un **PTY-backed agent** ; `SessionId` de continuité ; encapsule le process, l'I/O terminal, le cycle de vie ; **jetable/relançable**.
   - `AgentDefinition` vs `AgentInstance` — config (binaire CLI, prompt, outils permis, modèle) vs incarnation dans une session (dualité SK `Agent`/`AgentThread`).
   - `Workspace` — 1 worktree git + 1 branche + la (les) session(s) (modèle Conductor.build).
   - `WorkflowEvent` — journal append-only typé ; `ApprovalDecision` / artefacts (diff, logs) comme données de 1ʳᵉ classe (Swamp).

2. **Deux machines à états séparées** (Symphony) : métier (visible UI Avalonia) vs orchestration (interne, invisible). Ne pas les mélanger.

3. **État & persistance — event-sourcing léger, pas replay pur.** Le replay déterministe intégral de Temporal est **inapplicable** (un agent de code dans un PTY n'est pas rejouable). Adopter le modèle **MAF/LangGraph : snapshot par step (`WorkflowCheckpoint`)** + un **journal d'événements append-only** (audit/reprise fine). `ICheckpointStore` **SQLite** local-first. `RunId`+`checkpointId` monotone → **time-travel/fork**. Distinguer **State** (éphémère) de **Memory** (durable cross-run). Steps **idempotents**.

4. **HITL = citoyen de première classe** (cœur de la supervision) : un `WorkflowRun` peut entrer en `SuspendedAwaitingHuman` avec une charge typée (`ApprovalRequest`/`EditRequest`/`InputRequest`) persistée ; reprise par injection d'une `HumanDecision`. Approbations **collantes** (« don't-ask-again »). Trois canaux distincts (Temporal Signals/Queries/Updates) : **pousser** un input, **lire** l'état sans perturber (pour l'UI), **muter+attendre**. Handoff « Human Review » comme état de 1ʳᵉ classe.

5. **Contrôle de flux — les deux styles :** `WorkflowDefinition` déclaratif (séquence/branch/parallèle + fan-out façon `DYNAMIC FORK`/`Send` pour lancer N sessions, fan-in par step de jointure) ; et une session individuelle « émergente » en interne (l'agent CLI décide seul). Composition par **sous-workflows**.

6. **Isolation & exécution (spécifique Cursus) :** trancher le débat **PTY seul vs + worktree vs + container**. Recommandation pragmatique : PTY natif (déjà là) + **git worktree par workspace** (défaut du marché) avec **scripts setup/run/archive commités + injection de port** (Conductor.build/Emdash) ; garder le container optionnel (intégrer **container-use** via MCP plutôt que réimplémenter). **Tuer proprement le PTY avant `git worktree remove`.**

7. **Extensibilité :** outils via **MCP** (`ModelContextProtocol`, natif .NET) ; **déléguer la config MCP/agent à l'agent host** (Claude Code lit son `.mcp.json`/`CLAUDE.md`) plutôt que réinventer. Point d'interception type filtre SK pour les guardrails.

8. **Tracker :** modèle **interne d'abord** (store local SQLite comme adaptateur par défaut, contrat minimal à 2 fonctions type Symphony), **Linear/GitHub Issues en adaptateurs optionnels** derrière `IIssueSource` — jamais une dépendance du noyau.

**À écarter / adapter :** scale distribuée (Kafka/Cassandra/RBAC) ; replay déterministe pur ; couplage fort à un SaaS de tracking ; Mac-only (Cursus = cross-platform, avantage différenciant vs Conductor.build) ; télémétrie (local-first → opt-in strict).

---

## Cibles pour une vague 2 (deep-dive)

1. **Herdr** 🥇 — jumeau conceptuel : machine à **états d'agent**, detach/reattach, **Socket API**, persistance de session. Le modèle mental à disséquer en priorité.
2. **Claude Squad** 🥈 — référence terminal-first worktree+tmux+TUI ; le cycle de vie session/worktree concret.
3. **Emdash** 🥉 — concurrent desktop OSS le plus abouti (multi-provider, worktree, `$PORT`, SSH remote) ; meilleure source d'inspiration produit (contraste Electron vs Avalonia).
4. **Sculptor (Imbue)** — école **container** vs worktree/PTY ; pour trancher le débat d'isolation.
5. *(Bonus)* **container-use (Dagger)** — brique MCP d'isolation intégrable ; **MAF Workflows .NET** — référence de conception du cœur d'orchestration.

---

---

# Addendum — Vague 2 : mécanismes d'implémentation

> Deep-dive « lentille implémentation » sur les 4 projets les plus proches (**code source lu directement** pour Herdr, Claude Squad, Emdash, Sculptor). Objectif : les mécanismes concrets à porter en **C#/.NET 10 + RoyalTerminal**.

## Décision tranchée : isolation = **PTY natif + git worktree** (pas de container-par-agent)

**Validation externe forte (Sculptor/Imbue) :** ils ont *commencé* en « un container Docker par agent » puis **fait machine arrière vers le worktree local** (`docs/history.md`). Raisons, toutes pertinentes pour Cursus :
- l'isolation par-agent **empêche l'inspection croisée** du travail des agents — or c'est le cœur d'un manageur avec supervision humaine ; le worktree donne diff/compare/merge dans un seul graphe git gratuitement ;
- « most users found the extra isolation confusing » (modèle dedans/dehors, pull/push) ;
- sur **macOS arm64** : Docker Desktop = « wrong choice » (perf I/O bind mounts + **credentials Claude Code dans le keychain macOS inaccessibles depuis le container** → réauth obligatoire).

**Reco Cursus :** PTY natif (déjà là via RoyalTerminal) + **git worktree par workspace** en défaut. Isolation forte = **containeriser l'app entière** en opt-in *global* (une « custom backend command » pluggable, façon Sculptor), **pas** chaque agent. ⚠️ **Écarter `container-use`/Dagger** comme brique par-agent : c'est précisément le modèle abandonné par Sculptor.

> **Complément (Vague 3) :** le worktree isole *le code*, pas *le process*. La couche de confinement du process (ce que l'agent peut lire/écrire/réseau) se fait au niveau OS via **sandbox natif** (Seatbelt macOS + équivalents Linux/Windows), **sans container** — voir « Addendum — Vague 3 » ci-dessous.

## Détection de l'état d'agent — convergence : **hooks d'abord, screen-manifest en fallback**

Les deux jumeaux convergent sur le même principe fort : **ne jamais deviner l'état en scrapant le flux d'octets brut**.
- **Emdash** s'y *interdit explicitement* (« Emdash does not infer agent status from terminal output »). Il monte un **serveur HTTP local**, injecte `EMDASH_HOOK_PORT`/`_TOKEN`/`_PTY_ID` dans l'env de l'agent, **écrit les fichiers de config de hooks** du provider (`.claude/settings.local.json`…), et mappe les events `start/stop/notification/error` → `AgentStatus = idle|working|awaiting-input|error|completed`.
- **Herdr** : 3 niveaux d'autorité — (1) process de premier plan (quel agent) → (2) **hooks autoritaires** (7 agents) → (3) **screen-manifest** (fallback universel, moteur *pur* `fn(screen, osc_title, osc_progress) → état`, règles TOML priorisées : `working`=spinner braille dans le titre **OSC**, `blocked`=matching du prompt de permission, `idle`=boîte de prompt vide stable).

**Reco Cursus** (Claude Code cible prioritaire, et **il supporte les hooks**) :
1. **Primaire = hooks Claude Code** : mapper `Notification`(permission)→`blocked`, `Stop`→`idle/done`, tool en cours→`working`. Fiable, pas de devinette.
2. **Fallback universel = moteur screen-manifest** (~300 lignes pures, cas de test = snapshot d'écran figé → **candidat TDD/xUnit idéal**).
3. **`done` = `idle && !seen`** (pur UI, marqué vu au focus du pane) → donne la sidebar « qui attend d'être regardé ».
4. **`working` = octets PTY qui coulent**, avec **debounce obligatoire** (3 confirmations × 100 ms, cap 700 ms) sinon la sidebar clignote.
5. ✅ **Dépendance dure — LEVÉE** (sonde par réflexion, voir section dédiée ci-dessous). RoyalTerminal expose l'écran rendu en texte, l'OSC titre, l'event octets et le PID — et en bonus la shell-integration OSC 133. Matcher sur l'écran rendu (`TryExportSnapshot`), jamais sur le flux brut.

## Cycle de vie session/worktree (Claude Squad — `smtg-ai/claude-squad`, Go)

**Machine à états à 4 valeurs :** `Running / Ready / Loading / Paused`. Sémantique clé : **`Paused` = worktree *détruit*, branche *conservée*** (peu coûteux en inodes/verrous ; `Resume` reconstruit).
- Layout anti-collision : `worktrees/<branche>_<timestampHex>`. Branche = `BranchPrefix + sessionName` sanitizé.
- Commandes : `git rev-parse HEAD` → `git worktree add -b <branch> <path> <commit>` (create) ; `git worktree remove -f` (pause, garde branche) ; re-`add` (resume) ; `remove -f` + `branch -D` **si `isExistingBranch==false`** + `prune` (kill).
- **Ordre de teardown critique : tuer le process agent AVANT `git worktree remove`** (verrous). `prune` au démarrage pour réconcilier disque ↔ git.
- Persistance **mono-fichier** `state.json` derrière une **interface `IInstanceStorage`** (mockable → TDD). Ne jamais persister le handle PTY ; stocker `RepoPath/BranchName/BaseCommitSHA` pour reconstruire.
- Diff via `BaseCommitSHA` mémorisé à la création (`git diff <base>...HEAD --numstat`).
- **Cursus n'a pas besoin de tmux** (PTY natif déjà là) — mais il faut un **ring-buffer de scrollback par session** pour la preview d'une session non-affichée (tmux offrait 10 000 lignes gratuites via `capture-pane`).

## Injection de port déterministe (Emdash) — le *quick win*

`EMDASH_PORT` = **hash déterministe du chemin du worktree**, range **50000–59990 par pas de 10** → chaque tâche a `$PORT..$PORT+9` (9 ports libres pour front/back/db sans collision entre worktrees). Pas d'allocation dynamique, pas d'état. Injecté dans le PTY de l'agent **et** des lifecycle scripts.
- ⚠️ **Ne PAS utiliser `string.GetHashCode()` en .NET** (randomisé par run → le port changerait à chaque lancement). Réimplémenter leur boucle de hash.

## Abstraction multi-provider (Emdash) — plugin à capacités typées, pas une big-config

- `IAgentProvider` : **metadata** (`id`, `name`…) + **capabilities** (unions discriminées : `PromptDelivery = Argv|StdinPipe|Keystroke|None`, `Sessions`, `AutoApprove`, `Hooks`, `Mcp`, `HostDependency`…) + **behaviors**.
- **`BuildStandardCommand(ctx, spec) → (cmd, args[], env)`** : logique pure de flags (fresh vs resume, session UUID interne vs id natif du provider, `--flag value` vs `--flag=value`, dédup, quoting POSIX). **À porter quasi ligne à ligne** — plus gros ROI côté « multi-provider ». Ex. Claude : `resumeFlag='--resume'`, `sessionIdFlag='--session-id'`, `modelFlag='--model'`, `autoApproveFlag='--dangerously-skip-permissions'`.
- **`HostDependency`** : détection binaire (`binaryNames:['claude']`) + `installCommands` par OS → « détecte les CLI installés ».
- **Deux constructeurs d'env** : `BuildTerminalEnv` (hérite tout, feel natif) vs **`BuildAgentEnv` (allowlist stricte** des clés API + PATH ré-enrichi). Sécurité : pas de fuite d'env vers l'agent.
- **Livraison du prompt initial** : `argv` (Claude), `stdin-pipe`, ou **keystroke injection** (taper le prompt après une période de silence `QUIET_PERIOD_MS=800ms` pour les TUI qui n'acceptent pas de prompt au boot).
- **ACP (Agent Client Protocol)** : 2ᵉ transport (JSON-RPC type-LSP) pour une UI de chat structurée (diffs, tool calls) au lieu du TUI brut. **Reporter en v2** — commencer par le mode terminal/TUI brut (RoyalTerminal rend le TUI de Claude tel quel).

## Modèle de données 3 entités (Emdash) — affine `Cursus.Core`

Confirme et raffine le « Tâche > Session » de la vague 1 :
- **`Task`** — unité de travail durable ; `status`, `linkedIssue` (Linear/GitHub…), `type: task|automation-run`.
- **`Workspace`** — *où* ça tourne ; `kind: worktree|project-root|byoi`, `location: local|remote`, `path`, `branchName`, `linesAdded/Deleted`.
- **`Conversation`** — **une session d'agent** attachée à une tâche ; `providerId`, `sessionId` (id natif pour resume), `model`, `agentStatus`, `agentStatusSeen`. **Une tâche → N conversations** (essayer plusieurs agents).
- Gros prompts *spillés* vers un fichier markdown temporaire (limite d'arguments OS).

## Remote/SSH (Emdash) — à prévoir dans l'archi, même si v1 local-only

Interface **`IExecutionContext`** (`exec/execStreaming/dispose`, `supportsLocalSpawn`) abstrait *host+cwd* → `Local` vs `Ssh` ; « consumers have no knowledge of local vs remote ». En remote, worktree + PTY + dev server tournent sur la machine distante, l'UI reste locale, et **`PortForwardService`** tunnelise `$PORT` distant → local. Prévoir cette couture tôt (SSH.NET côté C#) même sans l'implémenter en v1.

## Détails PTY à connaître (Sculptor + Emdash)

- **Lancer le PTY via `posix_spawn`, pas `fork()`** (Sculptor) → évite le deadlock *fork-in-multithread* (réel dès qu'on mêle threads + PTY). À vérifier côté implémentation RoyalTerminal.
- Le PTY doit **survivre aux déconnexions UI** (buffer ~1 MB rejoué à la reconnexion) — modèle « à la VS Code ».
- **macOS GUI** : le PATH hérité est tronqué → **ré-enrichir via un login shell** au démarrage ; `SSH_AUTH_SOCK` absent → détecteur de fallback pour l'auth git/SSH des agents.
- Ne jamais affaiblir le **quoting/escaping shell** (POSIX + Windows) : centraliser un `ShellEscape`.

## Serveur détaché & Socket API (Herdr) — archi optionnelle, plus tard

- **Detach/reattach + survie SSH** = un **process serveur séparé** détenant les PTY + socket Unix (NDJSON). Choix *lourd* : v1 mono-process Avalonia OK (on perd la survie-SSH mais garde tout le reste) ; ça se rajoute après (le TUI Herdr n'est qu'un client du socket).
- **Socket API + primitive `wait agent-status <pane> --status done|blocked`** : c'est ce qui transforme Cursus de *viewer* en **orchestrateur** (un agent superviseur attend qu'un sous-agent finisse/bloque). Cible de la trajectoire agentique, pas de la v1.

## Sonde RoyalTerminal — blocage levé (2026-07-19)

Sonde par réflexion (`MetadataLoadContext`, dumper jetable dans `scratchpad/rtprobe` — resolver pointé sur le bin de l'app pour résoudre Avalonia). **Les 4 dépendances dures sont couvertes par `TerminalControl` 0.4.0, et il y a mieux que prévu.** Détail API complet dans la mémoire `cursus-royalterminal-reference`.

| Besoin | API RoyalTerminal |
|---|---|
| **Écran rendu** (input du moteur) | `TryExportSnapshot(TerminalSnapshotExportFormat.PlainText, ref {Unwrap=true, TrimTrailingWhitespace=true}, out string)` → écran en **texte déwrappé** (= `capture-pane -p -J`). Bas niveau : `Screen : TerminalScreen` → `GetViewportRow(i).ReadOnlyCells`. |
| **OSC titre** (spinner braille) | event `TitleChanged : EventHandler<string>` |
| **Octets reçus** (`working`) | event `DataReceived` (`Data : ReadOnlyMemory<byte>`) → compteur de seq de contenu |
| **PID enfant** (process 1er plan) | `terminal.Pty.ChildPid : int` (+ `IsRunning`, `ProcessExited`) |

**Bonus décisifs :**
- **Shell integration OSC 133 intégrée & bootstrappable** : event `ShellIntegrationEventReceived` → `Kind ∈ {PromptStarted, NewCommand, OutputStarted, CommandFinished(+ExitCode), WorkingDirectoryChanged…}` + `CommandLine`/`WorkingDirectory`. `TerminalShellIntegrationBootstrapBuilder.Build({Shell=Bash|Zsh|Fish|PowerShell, EmitSemanticPrompt, EmitWorkingDirectory})` génère le snippet à injecter. → **cycle de vie de commande fiable pour les sessions shell, sans scraping.**
- **TUI plein écran** : `ModeChanged`(`AlternateScreen`, `ApplicationCursorKeys`) / `Screen.AlternateBufferActive`. ⚠️ **PAS fiable pour « Claude tourne »** : le renderer *classic* (défaut) de Claude Code rend inline dans le buffer principal (alimente le scrollback) ; l'alt screen n'est que le mode opt-in `/tui fullscreen`. Utile pour vim/less, ou si on force le fullscreen. Impact audit : en classic la conversation rendue est dans le scrollback (capturable) ; en fullscreen non.
- **Injection de frappes** (auto-yes) : `SendInput(string)` / `IPty.Write`.
- **Env par session** : `IPty.Start(shell, cols, rows, wd, Dictionary<string,string> env, args)` prend un dict d'env, mais `StartPty(shell, wd, args)` **ne l'expose pas** → descendre au `PtyFactory`/`IPty` ou `StartSessionAsync` pour injecter `CURSUS_PORT`/hooks/allowlist. À creuser au câblage.
- **Persistance intégrée** (peut éviter du code) : `JsonFileTerminalWorkspaceStore` (layout tabs/panes/windows), `…SessionProfileStore`, `…CommandHistoryStore`. Scrollback géré (`ScrollbackLimit`, `PreserveScrollbackOnSessionStart`).

**Bilan :** on passe d'un signal à **trois, combinables par priorité** — (1) hooks Claude Code, (2) shell-integration OSC 133, (3) moteur screen-manifest. Le moteur (3) reste `fn(screen, oscTitle) → AgentState`, **pur → TDD xUnit sur snapshots figés**.

## Premier jalon recommandé (ordre TDD)

0. ~~**Sonde RoyalTerminal**~~ — ✅ FAIT (voir section ci-dessus). Blocage levé.
1. **Moteur de détection d'état** (`Cursus.Core`, **TDD pur**) — *prochaine action* : `fn(screen, oscTitle) → AgentState` + debounce working→idle + `done = idle && !seen`. Cas de test = snapshots d'écran figés de `claude`. Indépendant de l'UI et de RoyalTerminal. **À entrer via un plan gaté léger** (skill tdd, palier lourd) : inventaire objets + responsabilités + test list avant le 1ᵉʳ test.
2. **Cycle de vie Workspace/worktree** (`Cursus.Core`, TDD sur la machine à états + un `IGit`/`IWorktreeStore` mockés) : `Running/Ready/Loading/Paused`, `Paused`=worktree détruit/branche gardée, ordre de teardown.
3. **Injection de port déterministe** + `BuildAgentEnv` (allowlist) — petits, purs, TDD trivial.
4. **Abstraction `IAgentProvider` + `BuildStandardCommand`** (TDD sur la génération d'args pour Claude fresh/resume). Inclut **`IProcessConfinement`** (impl `srt` par défaut, no-op fallback, SBPL natif en échappatoire) : la commande finale devient `srt … <agent>` avec settings généré par worktree ; **sandbox interne de Claude Code laissé OFF** (pas de double-sandbox). Purement testable (génération de commande + settings). Câblage terminal via `IPty.Start`/`StartSessionAsync` (pas `StartPty`).
5. Puis câblage UI Avalonia + hooks Claude Code (serveur HTTP local) ; ACP, remote SSH, serveur détaché = itérations ultérieures. **Valider empiriquement** sur la machine cible : injection de frappes (`SendInput`) + confinement `srt` sur Darwin 25.x (gotcha Tahoe).

---

# Addendum — Vague 3 : Isolation OS & sandbox (macOS + cross-platform)

> Trois agents en parallèle (2026-07-19) : (A) mécanisme OS Seatbelt/SBPL, (B) sandbox des CLI d'agents & risque de double-sandbox, (C) RoyalTerminal peut-il lancer un binaire de confinement dans le PTY. Objectif : compléter le worktree (isolation du *code*) par un confinement du *process* (ce que l'agent peut lire/écrire/réseau), **sans container**.

## Décision : confinement OS **best-effort** via `@anthropic-ai/sandbox-runtime` (`srt`), derrière `IProcessConfinement`

**Verdict des 3 angles — ils convergent :**
- **Le terminal ne bloque rien (agent C, source lue).** RoyalTerminal lance le process PTY par **`forkpty()` + `execvp()` DIRECT** (`UnixPty.cs`) — **aucun `/bin/sh -c`**. On peut donc mettre `sandbox-exec` ou `srt` comme « shell » et l'agent réel en arguments, sans quoting/globbing à gérer (chaque token = un élément de liste `argv`). Le paramètre `shell` n'est pas validé comme vrai shell.
- **Le mécanisme OS est viable (agent A).** macOS **Seatbelt** = enforcement **noyau** (TrustedBSD MAC), **hérité par tous les process enfants** (on confine `claude`, tout ce qu'il spawne hérite). Marche sur Sequoia **et Tahoe** (Darwin 25.x). **Déprécié mais sans remplacement** (`sandbox_init` déprécié depuis ~10.8, `sandbox-exec` affiche un warning depuis Sequoia) — utilisé en prod par **Chrome, Codex CLI, Claude Code** → retrait brutal improbable (les casserait tous). SBPL non documenté (rétro-ingénierie).
- **La brique existe déjà, cross-platform (agent B).** Anthropic publie **`@anthropic-ai/sandbox-runtime` (`srt`)** : confine **tout le process** (pas seulement Bash) via les primitives natives — **Seatbelt (macOS)**, **bubblewrap+seccomp (Linux)**, compte dédié + WFP (**Windows, alpha**). Mêmes primitives que le sandbox interne de Claude Code, extraites en package *process-agnostic*.

**Choix retenu :** `srt` = implémentation **par défaut** de `IProcessConfinement` (délègue le cross-OS à Anthropic). Rationale : rend le confinement **cross-platform gratuitement** (sinon il faudrait réécrire Seatbelt + bubblewrap + Windows nous-mêmes) ; **dépendance Node quasi gratuite dans le cas nominal** (Claude Code se distribue déjà via npm → Node déjà présent). On garde un **SBPL natif maison comme échappatoire documenté** derrière la même interface (si `srt` trop instable, ou pour un contrôle réseau très fin). No-op de fallback si sandbox désactivé.
- ⚠️ Nuances assumées : port **Windows de `srt` = alpha** (parité macOS/Linux seulement) ; Node **non gratuit** pour un futur provider non-Node ; **config `srt` en beta** (format susceptible d'évoluer) → **épingler la version** + générer le settings JSON depuis notre modèle (rattrapage en un point).

## ⚠️ Règle d'or : ne JAMAIS double-sandboxer (Seatbelt ne s'imbrique pas)

Un process déjà sous `sandbox-exec` **ne peut pas** relancer `sandbox-exec` (EPERM) — confirmé par les contournements de **Bazel** (`processwrapper-sandbox` quand déjà sandboxé) et **Homebrew** (`sandbox_apply_container: Operation not permitted`).
- Le sandbox **interne** de Claude Code confine **uniquement les commandes Bash filles** (pas le process `claude` lui-même : file tools, MCP, hooks tournent hors sandbox). Il est **OFF par défaut** (`/sandbox` ou `sandbox.enabled:true` pour l'activer).
- **Donc, quand Cursus confine de l'extérieur (`srt`) : laisser le sandbox interne OFF.** Ne jamais injecter `sandbox.enabled:true` ni surtout `failIfUnavailable:true` (sinon échec Bash en cascade / refus de démarrer). Un seul Seatbelt à la fois.
- Pattern d'écosystème confirmé : **Codex CLI** fait pareil (Seatbelt, sandboxe ses commandes filles, pas lui-même) → l'isolation « du process entier » est déléguée à une couche externe = **le rôle de Cursus**.

## Conséquences d'implémentation

- **Câblage terminal :** passer par **`IPty.Start(...)` / `StartSessionAsync(PtyTransportOptions{Command,Environment})`**, PAS `StartPty` (qui force `Environment:null`), pour injecter l'env du process sandboxé. Piège : l'env est **fusionné par-dessus** celui de l'app hôte (pas vierge) — pour un sandbox strict, en tenir compte. `execvp` échoué → exit **127** (binaire introuvable) ; passer des **chemins absolus** (pas d'expansion `~`/`$VAR`).
- **HITL sur échec de sandbox :** les EPERM (FS) / refus réseau imposés par *notre* profil ne sont **pas** traduits en demandes de permission par Claude Code (le sandbox n'est plus le sien). → Cursus doit **surveiller stderr/exit codes**, remonter une demande d'autorisation à l'utilisateur, **élargir** le profil (domaine/chemin) et relancer. (Se branche sur le HITL de première classe déjà prévu.)
- **Réseau à grain fin faible en SBPL** (`network*` est tout-ou-rien) : pour « autoriser `api.anthropic.com` et rien d'autre », `srt` (comme Codex/michaelneale) utilise un **proxy localhost hors sandbox**. Bloquer par défaut `~/.ssh`, `~/.aws` (lisibles par défaut sinon).
- **Gotcha à tester sur la machine cible (Darwin 25.5.0 / Tahoe) :** des échecs d'init Seatbelt y sont rapportés (issues Claude Code #55849, #26095) — valider le confinement sur cette version précise avant de s'y fier.
- **Traiter comme defense-in-depth, pas frontière de sécurité forte.** Pour une vraie frontière il faudrait une VM (Virtualization.framework) — écarté (contraire à l'archi). Investir tôt dans l'outillage **trace/`log_denials`** (échecs opaques = piège #1).

## Sources Vague 3
- Anthropic `@anthropic-ai/sandbox-runtime` : github.com/anthropic-experimental/sandbox-runtime · Claude Code sandboxing : code.claude.com/docs/en/sandboxing & /sandbox-environments · anthropic.com/engineering/claude-code-sandboxing
- Seatbelt/SBPL : Chromium `sandbox/mac/seatbelt_sandbox_design.md` · HackTricks macOS Sandbox · Blazakis « The Apple Sandbox » (ise.io) · codeberg.org/majick/sandbox-exec-sandboxing · man sandbox_init(3)
- Dépréciation sans remplacement : github.com/apple/containerization/issues/737
- Non-imbrication : bazel.build/docs/sandboxing · github.com/orgs/Homebrew/discussions/59
- Codex CLI (Seatbelt) : developers.openai.com/codex/concepts/sandboxing · simonwillison.net/2025/Nov/9/codex-sandbox-investigation · deepwiki.com/openai/codex/5.6-sandboxing-implementation
- Gotcha Tahoe : github.com/anthropics/claude-code/issues/55849 & /26095 · Anti-exfiltration : github.com/michaelneale/agent-seatbelt-sandbox
- RoyalTerminal (source) : `RoyalTerminal.Terminal.Pty.Unix/Terminal/UnixPty.cs` (forkpty+execvp) · `PtyTerminalTransport.cs` (ResolveCommand sans validation shell)

---

## Sources principales
- Awesome list (~150 projets) : github.com/andyrewlee/awesome-agent-orchestrators · comparatif : augmentcode.com/tools/open-source-agent-orchestrators
- Herdr : herdr.dev · Conductor.build : conductor.build/docs · Sculptor : imbue.com/sculptor · container-use : github.com/dagger/container-use · Emdash : github.com/generalaction/emdash
- Symphony : github.com/openai/symphony (`SPEC.md`) · Linear Agents : linear.app/developers/agents · Cyrus (Claude Code↔Linear)
- Orkes Conductor : orkes.io/content (core-concepts, operators, ai-orchestration)
- LangGraph : docs.langchain.com/oss/python/langgraph · OpenAI Agents SDK : openai.github.io/openai-agents-python · Temporal : docs.temporal.io (develop/dotnet) · MAF/SK : learn.microsoft.com/agent-framework & /semantic-kernel · CrewAI : docs.crewai.com
