# Schémas d'architecture — la carte visuelle de Cursus

> **Statut** : compagnon visuel de `architecture.md`, à jour du commit `8f2c961`
> (jalon 6c·3c — l'écran de run, la projection à deux alimentations). Rendu par
> l'aperçu Markdown de Rider, GitHub et VS Code (Mermaid natif, aucune étape de
> build).
>
> **Ce que ce document est, et n'est pas.** `architecture.md` détient la vérité en
> prose — le *pourquoi*, les invariants, les registres. Ce fichier-ci n'ajoute
> **aucune décision** : il donne l'image de ce qui existe, pour les lecteurs qui
> scannent un schéma plus vite qu'un paragraphe. En cas de divergence,
> `architecture.md` fait foi.
>
> **Deux natures de schéma, à ne pas confondre** (voir la légende) :
> - les **cartes d'état** (§1–§5) montrent *ce qui existe aujourd'hui* — elles
>   datent, et se mettent à jour avec le code, comme le graphe de `architecture.md`
>   §1.2 ;
> - la **convention de schéma-delta** (§6) est *permanente* : c'est le gabarit des
>   schémas qu'on met dans les plans de marche, où la couleur dit *ce qui change*.

---

## 0. Comment lire ces schémas — la légende

### 0.1 La couleur

Elle ne veut pas dire la même chose selon la nature du schéma. **C'est la seule
règle à retenir.**

| Contexte | Vert | Ambre | Rouge (pointillé) | Neutre / teinté |
|---|---|---|---|---|
| **Carte d'état** (§1–§5) | — la couleur code la *couche* (APP / DOMAIN / INFRA), pas un changement — | | | |
| **Schéma-delta** (§6, plans) | **ajouté** — le bloc naît dans cette marche | **modifié** — collaborateur touché | **supprimé** | inchangé (contexte) |

Trait plein = référence de compilation (« A connaît B »). Trait pointillé annoté =
relation *absente*, *logique*, ou une **implémentation** de port (« l'adaptateur
réalise l'interface »). Trait épais `==>` = **correspondance définition ↔ exécution**
(§3).

### 0.2 L'anatomie d'un nœud

Chaque classe se lit sur deux ou trois lignes, du général au précis :

```
┌─────────────────────────────────┐
│ NomDeClasse            (gras)    │  ← qui c'est
│ sa responsabilité globale (petit)│  ← ce qu'elle fait, toujours
│ + l'incrément de la marche       │  ← DELTA seulement : le comportement neuf
└─────────────────────────────────┘
```

La phrase de responsabilité globale donne le contexte ; sur un schéma-delta, la
ligne `+ …` dit **le seul comportement que cette marche ajoute** à la classe. Les
deux ensemble : on sait ce qu'était la classe et ce qui vient de bouger, sans lire
le plan.

Les **interfaces (ports)** portent la forme hexagonale `⬡` et le stéréotype
`«interface»`.

---

## 1. Vue d'ensemble — les deux moitiés et la jonction manquante

Le fait le plus structurant du dépôt (`architecture.md` §2) : deux moitiés
cohérentes, **aucun pont entre elles**. `WorkflowEngine` n'est appelé que par les
tests ; aucun fichier de `Sessions/` ne mentionne `Workflow`, et réciproquement.

```mermaid
flowchart TB
  subgraph det["Moitié déterministe — testée, bout en bout, sans UI"]
    direction LR
    engine["<b>WorkflowEngine</b><br/><small>traverse le graphe, route sur le code de sortie</small>"]
    proj["<b>Projets & catalogue</b><br/><small>ProjectHost · WorkflowCatalog · Registry</small>"]
    persist["<b>Persistance SQLite</b><br/><small>journal + artefacts durables</small>"]
    engine --- proj
    proj --- persist
  end
  subgraph pty["Moitié sessions / PTY — antérieure au pivot"]
    direction LR
    sess["<b>Sessions terminal</b><br/><small>TerminalSession · ShellResolver</small>"]
    royal["<b>RoyalTerminal</b><br/><small>vrais PTY, moteur VT natif</small>"]
    sess --- royal
  end
  det -. "jonction manquante — §2 · 'comment recoudre' est une question ouverte" .- pty
```

L'app touche **une** de ces moitiés côté noyau depuis 6c·3a : elle *lit* le journal
d'un projet (surface en lecture seule). Le **lanceur** existe côté noyau (6c·3b),
mais l'app ne le *déclenche* pas encore — le câblage à l'écran de run est 6c·3c.

---

## 2. Les couches — APP / DOMAIN / INFRA

La bonne lecture des dépendances. Le domaine (noyau déterministe) ne dépend de
**rien** ; l'app le consomme *par le haut* ; l'infra l'implémente *par le bas*.
Toutes les flèches de dépendance **convergent vers le domaine** — c'est l'inversion
de dépendance, et c'est pourquoi `Persistence` n'est pas « entre » App et Core mais
**dessous**.

```mermaid
flowchart TB
  subgraph APP["APP · présentation"]
    App["<b>Cursus.App</b><br/><small>coquille rail | surface, ViewModels, RoyalTerminal</small>"]
  end
  subgraph DOMAIN["DOMAIN · noyau déterministe — zéro dépendance externe"]
    Workflows["<b>Workflows/</b> <i>(namespace)</i><br/><small>orchestre l'exécution déterministe d'un workflow — modèle §3, services §4</small>"]
    Projects["<b>Projects/</b> <i>(namespace)</i><br/><small>ancre les workflows sur le disque et compose un projet ouvert</small>"]
    Sessions["<b>Sessions/</b> <i>(namespace)</i><br/><small>TerminalSession · ShellResolver — antérieur au pivot</small>"]
    Projects --> Workflows
  end
  subgraph INFRA["INFRA · adaptateurs durables"]
    Persist["<b>Cursus.Persistence</b><br/><small>SqliteRunJournal · RunArtifactStore · SqliteProjectHost</small>"]
  end

  App --> Projects
  App --> Workflows
  App -. "composition (racine) : la seule entorse, câbler le concret" .-> Persist
  Persist -. "implémente les ports" .-> Workflows
  Persist -. "rend un ProjectHost" .-> Projects
  Sessions -. "aucune référence, dans aucun sens" .- Workflows

  classDef pres fill:#3b4b66,color:#fff,stroke:#22304a;
  classDef dom fill:#1f6f4a,color:#fff,stroke:#12432c;
  classDef inf fill:#2a5b7a,color:#fff,stroke:#173a52;
  classDef disc fill:#5a4b8a,color:#fff,stroke:#382c5c;
  class App pres;
  class Workflows,Projects dom;
  class Sessions disc;
  class Persist inf;
```

- Le domaine a **zéro dépendance externe** — sauf `Sessions/`, seul à tirer
  `CommunityToolkit.Mvvm`. Les adaptateurs *réels* (SQLite) vivent en INFRA ; mais
  certains adaptateurs (`ProcessRunner`, `GitWorkspaceProvisioner`, les doubles
  `InMemory…`) **vivent dans l'assembly Core** tout en étant infra *par rôle* —
  ils ne tirent que le framework (voir §2.1).
- L'unique `App → Persistence` est l'**entorse de la racine de composition** :
  `App.axaml.cs` est le seul lieu autorisé à nommer le concret, pour l'injecter à
  ce qui ne connaît que des ports (`architecture.md` §7.12).

### 2.1 Ports & adaptateurs — le couplage par interfaces

Le domaine **définit** les interfaces (ports) ; consommateurs et implémentations
s'y accrochent sans se connaître. Ce schéma montre, pour chaque port, **qui en
dépend** (flèche pleine, dans le domaine) et **qui le réalise** (flèche pointillée
`implémente`, depuis un adaptateur). Le rôle de chaque adaptateur — Core ou
Persistence — est étiqueté.

```mermaid
flowchart LR
  subgraph consumers["Consommateurs (DOMAIN)"]
    direction TB
    Engine["<b>WorkflowEngine</b><br/><small>traverse le graphe, orchestre un run</small>"]
    Host["<b>ProjectHost</b><br/><small>racine de composition d'un projet ouvert</small>"]
    Launcher["<b>WorkflowLauncher</b><br/><small>monte le run ; ProjectHost le compose, l'écran le déclenchera en 6c·3c</small>"]
  end

  subgraph ports["Ports (interfaces, DOMAIN)"]
    direction TB
    IProc{{"«interface» IProcessRunner<br/><small>lancer un script → issue + sortie ruisselée</small>"}}
    IJW{{"«interface» IRunJournal<br/><small>émettre les 5 événements d'un run</small>"}}
    IJR{{"«interface» IRunJournalReader<br/><small>relire les runs passés</small>"}}
    IOut{{"«interface» IRunOutputStore<br/><small>ouvrir un puits de sortie par étape</small>"}}
    IWsp{{"«interface» IWorkspaceProvisioner<br/><small>monter/démonter le worktree isolé</small>"}}
  end

  subgraph adapters["Adaptateurs (réalisations)"]
    direction TB
    Proc["<b>ProcessRunner</b> · Core<br/><small>Process réel, tubes redirigés</small>"]
    Git["<b>GitWorkspaceProvisioner</b> · Core<br/><small>worktree git</small>"]
    InMemJ["<b>InMemoryRunJournal</b> · Core<br/><small>double volatile (journal + lecture)</small>"]
    InMemO["<b>InMemoryRunOutputStore</b> · Core<br/><small>puits volatile</small>"]
    SqlJ["<b>SqliteRunJournal</b> · Persistence<br/><small>journal durable, écriture sérialisée</small>"]
    Art["<b>RunArtifactStore</b> · Persistence<br/><small>sorties sur disque</small>"]
  end

  Engine --> IProc
  Engine --> IJW
  Engine --> IOut
  Host --> IJR
  Launcher --> IWsp

  Proc -. implémente .-> IProc
  Git -. implémente .-> IWsp
  Git --> IProc
  InMemJ -. implémente .-> IJW
  InMemJ -. implémente .-> IJR
  SqlJ -. implémente .-> IJW
  SqlJ -. implémente .-> IJR
  InMemO -. implémente .-> IOut
  Art -. implémente .-> IOut

  classDef port fill:#1f6f4a,color:#fff,stroke:#12432c;
  classDef core fill:#2a5b7a,color:#fff,stroke:#173a52;
  classDef persist fill:#7a5a2a,color:#fff,stroke:#523a17;
  classDef consumer fill:#3b4b66,color:#fff,stroke:#22304a;
  class IProc,IJW,IJR,IOut,IWsp port;
  class Proc,Git,InMemJ,InMemO core;
  class SqlJ,Art persist;
  class Engine,Host,Launcher consumer;
```

Ce que le schéma rend lisible d'un coup :

- **Chaque port a au moins deux réalisations** (une `InMemory…`/Core pour les tests,
  une durable/Persistence) — c'est ce qui rend le domaine testable sans I/O.
- **`GitWorkspaceProvisioner` est à la fois adaptateur et consommateur** : il réalise
  `IWorkspaceProvisioner` *et* dépend d'`IProcessRunner` (il lance `git` via le même
  port que le moteur). L'invariant « aucun `Process.Start` hors de `ProcessRunner` »
  tient donc jusque dans le provisionnement.
- **`IRunJournal` (écrire) et `IRunJournalReader` (lire) sont deux ports séparés** :
  le moteur *émet* et ne *lit* jamais (`architecture.md` invariant 8) ; `ProjectHost`
  ne fait que lire. Un même objet (`SqliteRunJournal`, `InMemoryRunJournal`) réalise
  les deux, mais aucun consommateur ne voit les deux faces.

---

## 3. Le modèle du domaine — définition vs exécution

Le cœur du noyau, et la structure à avoir en tête pour reviewer un plan. « Workflow »
n'est pas une classe : c'est une **paire**. À gauche, ce qu'on **déclare** (statique,
portable, versionné dans `.cursus/`) ; à droite, ce que la traversée **produit**
(dynamique, jetable, journalisé). Le même dédoublement se rejoue à l'étage *étape* —
et c'est ce parallélisme qui dit, pour un delta, si on touche le déclaré ou le
produit.

```mermaid
flowchart LR
  subgraph def["DÉFINITION — déclarée · statique · portable"]
    direction TB
    WDef["<b>WorkflowDefinition</b><br/><small>le graphe : point d'entrée + étapes</small>"]
    SDef["<b>StepDefinition</b><br/><small>un nœud : script, MaxVisits, arêtes sortantes</small>"]
    Edge["<b>Edge + Guard</b><br/><small>arête gardée : telle issue → telle cible</small>"]
    Spec["<b>ScriptSpec</b><br/><small>ce qu'on lance : fichier, args, env, timeout</small>"]
    WDef -->|"1..* Steps"| SDef
    SDef -->|"0..* OutEdges"| Edge
    SDef -->|"1 Script"| Spec
  end

  subgraph run["EXÉCUTION — produite · dynamique · journalisée"]
    direction TB
    WRun["<b>WorkflowRun</b><br/><small>le déroulé : RunState + historique des visites</small>"]
    SRun["<b>StepRun</b><br/><small>une visite : (StepId, Iteration, Result, Output)</small>"]
    Res["<b>ScriptResult</b><br/><small>ce que le process a fait : code, issue, durée, IsSuccess</small>"]
    Out["<b>StepOutput</b><br/><small>artefacts laissés par la visite</small>"]
    WRun -->|"0..* history"| SRun
    SRun -->|"1 Result"| Res
    SRun -->|"1 Output"| Out
  end

  Engine(["<b>WorkflowEngine</b><br/><small>traverse une définition, produit un run</small>"])
  WDef -.->|"exécuté par"| Engine
  Engine -.->|"produit"| WRun

  WDef ==>|"1 déf → N runs (relancée dans le temps)"| WRun
  SDef ==>|"1 déf → N visites dans UN run (Iteration ≤ MaxVisits)"| SRun
  Spec ==>|"lancée → rendue"| Res
  Edge -.->|"Guard.Matches lit le"| Res

  classDef defcls fill:#2a5b7a,color:#fff,stroke:#173a52;
  classDef runcls fill:#1f6f4a,color:#fff,stroke:#12432c;
  classDef eng fill:#3b4b66,color:#fff,stroke:#22304a;
  class WDef,SDef,Edge,Spec defcls;
  class WRun,SRun,Res,Out runcls;
  class Engine eng;
```

Trois faits que le parallélisme rend évidents (`architecture.md` §3, §4.1) :

- **Deux multiplicités, à deux échelles — à ne pas confondre.** *Dans le temps*, une
  `WorkflowDefinition` se relance autant qu'on veut : chaque lancement est un nouveau
  `WorkflowRun` (`1 → N`, le lien étant la provenance `workflow_id` posée en 6c·3a).
  *À l'intérieur d'un seul run*, une même `StepDefinition` peut être revisitée — la
  boucle gardée — donnant autant de `StepRun`, d'où `Iteration`, borné par
  `MaxVisits`. La première multiplicité, c'est « relancer un workflow » ; la seconde,
  « boucler dans un run ».
- **Le `Guard` est le seul pont déclaré → produit.** Une arête (côté définition) lit
  un `ScriptResult` (côté exécution) pour choisir la suite : `Guard.Matches(result)`.
  C'est là, et nulle part ailleurs, que le graphe consulte le déroulé.
- **`StepRun` n'a ni état, ni identité, ni horodatage** : c'est un simple record
  `(StepId, Iteration, Result, Output)`. L'état d'une visite se *déduit* de son
  `ScriptResult`. La seule machine à états du dépôt est celle de `WorkflowRun`
  (`Completed` / `Failed` / `Aborted`).

---

## 4. Le noyau — vocabulaire racine et sept services

`Cursus.Core.Workflows` range le fourre-tout d'origine (43 fichiers, jadis à plat) en
un **vocabulaire racine** que tout le monde importe (le modèle §3 en fait partie), plus
**sept sous-namespaces** de services. Chaque service dépend de la racine ; les
exceptions suivent l'invariant qu'elles protègent (`architecture.md` §4).

```mermaid
flowchart TB
  root["<b>Cursus.Core.Workflows (racine)</b><br/><small>le vocabulaire que tout le monde importe — dont le modèle §3</small><br/>Graphe : WorkflowDefinition · StepDefinition · Edge · Guard<br/>Run : WorkflowRun · StepRun · RunSummary · RunTrigger · WorkflowEvent<br/>Script/sortie : ScriptSpec · ScriptResult · ScriptOutcome · StepOutput · OutputArtifact"]

  Exec["<b>…Execution</b><br/><small>lance et route un run</small><br/>WorkflowEngine · WorkflowLauncher · RunContext · IProcessRunner · ProcessRunner · PathStrategy · IClock"]
  Proj["<b>…Projection</b><br/><small>deux projections sœurs, event-fed</small><br/>RunProjection · RunVisit · RunControl<br/>GraphProjection · GraphNode · GraphEdge"]
  Ser["<b>…Serialization</b><br/><small>JSON ⟷ modèle</small><br/>WorkflowSerializer · WorkflowDocument"]
  Val["<b>…Validation</b><br/><small>validité du graphe</small><br/>WorkflowValidator · ValidationReport"]
  Jou["<b>…Journaling</b><br/><small>écrire et relire un run</small><br/>IRunJournal · IRunJournalReader · InMemoryRunJournal · JournalEntry"]
  Out["<b>…Output</b><br/><small>puits de sortie par étape</small><br/>IRunOutputStore · IStepOutputSink · InMemoryRunOutputStore"]
  Wsp["<b>…Workspaces</b><br/><small>worktree isolé d'un run (provisionnement async)</small><br/>IWorkspaceProvisioner · GitWorkspaceProvisioner"]

  Exec --> root
  Proj --> root
  Ser --> root
  Val --> root
  Jou --> root
  Out --> root
  Wsp --> root
  Ser --> Val
  Exec <-->|"le moteur provisionne,<br/>le provisioner s'exécute"| Wsp
  Exec --> Jou
  Exec --> Out
```

---

## 5. Les coutures vivantes

### 5.1 La couture de lecture — ce qui tourne bout en bout aujourd'hui (6c·3a)

Sélectionner un projet dans le rail ouvre sa surface et affiche, pour chaque
workflow, son dernier passage. C'est la **première** chaîne noyau → UI complète
(l'écran de run en est la seconde, §5.3). La règle de sens unique tient : la
surface reçoit la *projection*, jamais le host.

```mermaid
sequenceDiagram
  actor U as Utilisateur
  participant Shell as ShellViewModel
  participant Preset as SqliteProjectHost
  participant Host as ProjectHost
  participant Cat as WorkflowCatalog
  participant Jour as SqliteRunJournal
  participant Surf as OpenProjectViewModel

  U->>Shell: sélectionne un projet
  Shell->>Shell: dispose l'ancien host (1 connexion / projet)
  Shell->>Preset: Open(project)
  Preset->>Host: new(project, () => new SqliteRunJournal(db))
  Shell->>Host: LastRunPerWorkflow()
  Host->>Cat: List()
  Cat-->>Host: workflows (depuis le disque)
  Host->>Jour: ListRuns()
  Jour-->>Host: runs, du plus récent au plus ancien
  Host-->>Shell: [WorkflowLastRun] (workflow × son dernier run)
  Shell->>Surf: new(nom, projection)
  Surf-->>U: chaque workflow + « réussi / échoué / jamais lancé »
```

Le verdict français (`réussi`/`échoué`/`jamais lancé`) est calculé par
`WorkflowRowViewModel` — **présentation, non testée** (`architecture.md` §7.12).

### 5.2 La couture d'exécution — le moteur et ses collaborateurs

Construite et testée dans le noyau ; le **lanceur** (`WorkflowLauncher`, 6c·3b) en
est désormais l'appelant de production, composé par `ProjectHost.LaunchAsync`.
L'**observateur** est câblé à l'écran de run (6c·3c, §5.3) : c'est le flux live.
Le lanceur provisionne le worktree *avant* et le démonte *après* : l'isolation
n'entre pas dans le moteur (invariant 8).

```mermaid
flowchart TB
  launcher["<b>WorkflowLauncher.LaunchAsync</b><br/><small>forge le runId, assemble le moteur, estampille la provenance</small>"]
  prov["<b>IWorkspaceProvisioner.ProvisionAsync</b><br/><small>GitWorkspaceProvisioner — async, IAsyncDisposable, zéro sync-over-async (D-015)</small>"]
  engine["<b>WorkflowEngine.ExecuteAsync</b><br/><small>(def, RunContext, runId, trigger?, workflowId?, observer?, ct)</small>"]
  runner["<b>IProcessRunner</b><br/><small>ProcessRunner : Process réel, tubes redirigés, tue l'arbre à l'annulation</small>"]
  journal["<b>IRunJournal</b><br/><small>rend durables 5 événements aux frontières d'étape</small>"]
  output["<b>IRunOutputStore</b><br/><small>puits ouvert avant chaque étape, sortie qui ruisselle</small>"]
  observer["<b>IProgress&lt;WorkflowEvent&gt;</b><br/><small>flux live consommé par l'écran de run (6c·3c, §5.3)</small>"]

  launcher -->|"1 · provisionne le worktree par runId"| prov
  prov -.->|"RunContext = racine du worktree isolé"| launcher
  prov -->|"git via"| runner
  launcher -->|"2 · ExecuteAsync"| engine
  engine -->|"par étape : RunAsync(ScriptSpec, ct)"| runner
  engine -->|"Emit : RunStarted · StepStarted · StepFinished · EdgeChosen · RunFinished"| journal
  engine -.->|"Emit : même point, même ordre"| observer
  engine -->|"ruisselle stdout/stderr"| output
  launcher -->|"3 · démonte le worktree"| prov
```

### 5.3 La couture de l'écran de run — une projection, deux alimentations (6c·3c)

L'écran d'un run *en cours* et celui d'un run *passé* sont **le même écran** : une
seule `RunProjection` (Core testable) plie une séquence de `WorkflowEvent` en
trajectoire + statut + contrôle, sans savoir d'où elle vient. Deux alimentations
entrent par la **même porte** `Apply` — le flux live (§5.2) ou la relecture
`ReadEvents` — et donnent la même projection (`D-013`, prouvé end-to-end). Le
**log**, lui, est un second flux, distinct du pipeline : il tail le fichier
d'artefact de la **visite sélectionnée** (vif si elle tourne, figé sinon).

```mermaid
flowchart TB
  live["<b>flux live</b><br/><small>ProjectHost.LaunchAsync + IProgress (marshalé thread UI)</small>"]
  replay["<b>relecture</b><br/><small>ProjectHost.ReadEvents(runId)</small>"]
  proj["<b>RunProjection</b><br/><small>Apply : plie en trajectoire de visites + statut + contrôle 3 positions. Source-agnostique. Porte le runId (RunStarted).</small>"]
  vm["<b>RunViewModel</b><br/><small>adaptateur (§7.12) : StartLive OU Replay ; commande d'arrêt → CancellationToken</small>"]
  tail["<b>ArtifactTail</b><br/><small>RunArtifactStore.Follow(runId, visite) — tiré par un minuteur</small>"]
  view["<b>RunView.axaml</b><br/><small>trajectoire déroulée en haut, log sur fond terminal en bas</small>"]

  live -->|"Apply(event)"| proj
  replay -->|"Apply(event)"| proj
  proj --> vm
  vm -->|"log ← visite sélectionnée"| tail
  vm --> view
```

Tout ce qui est *au-dessus* de `RunViewModel` (la vue, le câblage) est
**présentation non testée** (§7.12) ; `RunProjection`, le contrôle 3 positions, le
tail et la coïncidence des deux alimentations sont, eux, du noyau/persistance
**testé**.

---

## 6. La convention de schéma-delta pour les plans

C'est la partie **permanente** de ce document. Un schéma-delta accompagne le plan
d'une marche : il colorie la table « Objets impactés » (*ajouté / modifié /
supprimé*) pour qu'on voie d'un coup d'œil **ce qui naît, ce qui bouge autour, ce
qui ne bouge pas** — et, sur chaque bloc modifié, **le comportement neuf** (ligne
`+ …`, cf. l'anatomie §0.2). Il se lit sur le vocabulaire du modèle §3 : un delta
qui touche `StepDefinition` (le déclaré) ne dit pas la même chose qu'un delta sur
`StepRun` (le produit).

### 6.1 Les trois registres et l'anatomie d'un nœud

```mermaid
flowchart LR
  N["<b>Bloc ajouté</b><br/><small>sa responsabilité, qui naît entière</small>"]
  M["<b>Collaborateur modifié</b><br/><small>sa responsabilité globale, inchangée</small><br/><b>+</b> <small>l'incrément de comportement de la marche</small>"]
  S["<b>Bloc supprimé</b><br/><small>ce qu'il portait, et qui part</small>"]
  C["<b>Contexte inchangé</b><br/><small>présent pour situer, ne bouge pas</small>"]
  N -->|"nouvelle arête"| M
  M --> C
  M -.-> S

  classDef added fill:#2f7d4f,stroke:#1c4d30,color:#fff;
  classDef changed fill:#c98a2b,stroke:#8a5d16,color:#fff;
  classDef removed fill:#b3402f,stroke:#7a2a1e,color:#fff,stroke-dasharray:5 5;
  class N added;
  class M changed;
  class S removed;
```

Bloc de couleurs à recopier tel quel à la fin d'un schéma-delta :

```
classDef added   fill:#2f7d4f,stroke:#1c4d30,color:#fff;
classDef changed fill:#c98a2b,stroke:#8a5d16,color:#fff;
classDef removed fill:#b3402f,stroke:#7a2a1e,color:#fff,stroke-dasharray:5 5;
class <NoeudsAjoutés>   added;
class <NoeudsModifiés>  changed;
class <NoeudsSupprimés> removed;
```

### 6.2 Exemple travaillé — la marche 3a (déjà faite)

Voici le schéma-delta qu'aurait porté le plan de 6c·3a. Sur chaque bloc modifié, la
ligne `+ …` isole le seul comportement que la marche ajoute — le reste de la classe
est déjà décrit par sa responsabilité globale au-dessus.

```mermaid
flowchart TB
  subgraph App["Cursus.App"]
    Shell["<b>ShellViewModel</b><br/><small>coquille rail | surface</small><br/><b>+</b> <small>ouvre/dispose le ProjectHost du projet choisi</small>"]
    OpenVM["<b>OpenProjectViewModel</b><br/><small>la surface d'un projet ouvert</small><br/><b>+</b> <small>reçoit la projection dernier-passage</small>"]
    RowVM["<b>WorkflowRowViewModel</b><br/><small>traduit un run en « réussi / échoué / jamais lancé »</small>"]
  end
  subgraph Core["Cursus.Core"]
    Host["<b>ProjectHost</b><br/><small>racine de composition d'un projet ouvert ; joint workflows × runs</small>"]
    LastRun["<b>WorkflowLastRun</b><br/><small>un workflow et son dernier run (ou aucun)</small>"]
    Catalog["<b>WorkflowCatalog</b><br/><small>liste/charge les workflows du disque</small>"]
    Summary["<b>RunSummary</b><br/><small>ce qu'on sait d'un run sans relire ses événements</small><br/><b>+</b> <small>EndedAt et WorkflowId</small>"]
    Started["<b>WorkflowEvent.RunStarted</b><br/><small>l'ouverture d'un run (trigger, workspace)</small><br/><b>+</b> <small>workflow_id, la provenance</small>"]
  end
  subgraph Persist["Cursus.Persistence"]
    Preset["<b>SqliteProjectHost</b><br/><small>préréglage : lie la fabrique de journal au SQLite</small>"]
    Journal["<b>SqliteRunJournal</b><br/><small>journal durable, écriture sérialisée</small><br/><b>+</b> <small>colonne workflow_id, lit ended_at</small>"]
    Codec["<b>RunEventCodec</b><br/><small>événements ⟷ payload JSON</small><br/><b>+</b> <small>encode/décode workflow_id</small>"]
  end

  Shell -->|"construit à la sélection"| Preset
  Preset -->|"Func&lt;IRunJournalReader&gt;"| Host
  Host --> Catalog
  Host -->|"ListRuns"| Journal
  Host --> LastRun
  Shell -.->|"projection"| OpenVM
  OpenVM --> RowVM
  Journal --> Summary
  Started -->|"via"| Codec
  Codec --> Journal

  classDef added fill:#2f7d4f,stroke:#1c4d30,color:#fff;
  classDef changed fill:#c98a2b,stroke:#8a5d16,color:#fff;
  class Host,LastRun,Preset,RowVM added;
  class Summary,Started,Codec,Journal,OpenVM,Shell changed;
```

On lit sans prose : trois blocs naissent (vert), la moitié persistance et
l'événement de run se font retoucher pour **porter l'identité de workflow** (ambre,
la ligne `+ …` dit précisément quoi), et `WorkflowCatalog` ne bouge pas. C'est
l'information de la table « Objets impactés » d'un plan — ici scannable.

### 6.3 Où le schéma-delta vit, et quand il périme

- **Dans le plan de marche** (`.claude/plans/<nom>.md`) : je le mets en tête, tu
  l'ouvres dans Rider pendant la validation. Je peux aussi publier le plan en
  Artifact si tu veux le voir colorié dans un navigateur.
- **Dans la §4.x de `architecture.md`** qui décrit la marche, *si* elle aide — mais
  un delta a une durée de vie d'une marche ou deux : au bout de deux marches,
  « ajouté » ne l'est plus. Les cartes d'état §1–§5 ci-dessus, elles, sont
  permanentes et se maintiennent. **Ne pas laisser un vieux delta traîner en carte
  d'état** : c'est la seule façon pour ce document de mentir.
