# Trajectoire — vers la boucle de dev agentique

> Ce document dit le **chemin à venir**. Il complète, sans recouvrir :
> - `architecture.md` — l'état **présent** (ce qui est construit) ;
> - `decisions.md` — les **pivots passés** (ADR, append-only) ;
> - `schemas.md` — la carte **visuelle** du présent.
>
> C'est un document **vivant** : chaque jambe se précise en s'approchant (pas de *big
> design up front*), et une jambe franchie se raconte au passé — ou migre vers
> `architecture.md` / `decisions.md` quand elle devient un fait ou un pivot.

## La destination

**Cursus se pilote lui-même.** Le nord de la trajectoire est une **boucle de dev
agentique** qui tourne sur le dépôt Cursus :

> une tâche entre (issue d'un tracker) → un **déclencheur** la capte → un **AgentStep**
> travaille dessus → le noyau **vérifie** (build/test) et **route** sur le résultat →
> Cursus **rapporte** et referme la tâche.

C'est le dogfooding au sens fort : l'outil qui gère des workflows agentiques est utilisé
pour construire l'outil qui gère des workflows agentiques.

## Le principe qui ordonne les jambes

**Chaque jambe doit être utilisable dès qu'elle est posée**, pas seulement à l'arrivée.
On ne construit pas la moitié d'un pont : on construit un premier pont, court, qui porte
déjà du trafic — puis on l'allonge. La première boucle *déterministe* rend service avant
le moindre agent ; l'agentique se greffe ensuite sur un socle déjà éprouvé au feu réel.

Corollaire tranché avec l'utilisateur : **on n'amorce pas le tracker (Linear) en premier.**
Tant que Cursus ne sait pas *consommer* une tâche, un projet de tracking n'est que du PM —
la charrue avant les bœufs. Il devient utile à la **jambe 2**, quand le déclencheur existe.

---

## Jambe 1 — la porte de gate déterministe

**But :** Cursus exécute, contre son propre dépôt, le standard non-négociable de
`CLAUDE.md` — `dotnet build` (0 warning) puis `dotnet test` (vert) — sous forme de
workflow, avec routage sur le code de sortie. Rouge → on s'arrête ; vert → on passe.

**Pourquoi elle d'abord :** c'est la boucle la plus honnête que le noyau *déterministe*
sait faire **aujourd'hui**, sans aucun type nouveau. Elle rend service tout de suite (elle
tourne avant chaque commit) et elle **encaisse trois dettes du backlog** en les faisant
vivre plutôt qu'en les cochant :

| Ce qu'elle encaisse | Nature |
|---|---|
| **Preuve PATH sur bundle** ✅ | **Encaissée** (jambe 1) : `ProcessRunner` lançant `dotnet` **nu** sous un `PATH` **vidé** le résout (via `~/.asdf/shims`) et sort 0 — preuve plus dure que le bundle. Béquille `/bin/sh -c` superflue. Ne reste que le double-clic de confirmation sur le `.app`. |
| **Log en streaming intra-étape** ✅ | **Encaissée** (`D-028`) : le puits flush par écriture, le suiveur voit la sortie en direct. |
| **Routage exit-code vécu** ✅ | **Vécu** : la gate `verifier.json` (compiler --succès--> tester) a 44 runs au compteur — le cœur du noyau éprouvé par l'usage, pas par le seul test. |

**Confort d'authoring ✅ :** le champ **« Commande » unique** (1er token = binaire, via `CommandLine`
posé sur `ArgumentLine`, `D-029`) rend l'écriture de `dotnet build -warnaserror` naturelle — et rend la
béquille `/bin/sh -c` inutile à la source. **Fait.**

**Pré-requis noyau :** aucun. Tout existait (parcours, routage, écran de run, PathStrategy).

**Statut :** ✅ **close.** Les trois dettes encaissées + l'authoring naturel. La gate build→test tourne
contre le dépôt, log en direct, routée sur le code de sortie.

---

## Jambe 2 — la boucle agentique

**But :** faire tourner la boucle de la *destination* de bout en bout.

Trois briques manquent, à décomposer en sous-jambes quand on s'en approchera (on ne fige
pas leur découpe ici) :

- **`AgentStep`** — un `StepKind` de plus, greffé sur le noyau (D-012, ère fan-out/join).
  C'est le vrai cap technique du projet.
- **Déclencheurs état-tâche** — l'onglet Déclencheurs du hub (§7.10.6) : un run s'amorce
  sur l'état d'une tâche, plus seulement sur un clic. L'ère *tracker*.
- **Intégration tracker (Linear)** — c'est **ici** que créer le projet Linear paie : la
  source des tâches et la cible des rapports. Semé trop tôt, il ne sert à rien ; semé ici,
  il ferme la boucle.

**Pré-requis :** la jambe 1 posée (le socle déterministe éprouvé au réel).

**Statut :** **en cours**, décomposée à mesure qu'on s'en approche :

- **2·1 — `AgentStep`** ✅ : le kind agent headless (`claude --model … -p …`), son Core (`D-030`) et
  son authoring UI (`D-031`). Le vrai cap technique, franchi.
- **2·2 — le round-trip tracker (Linear)** 🔸 en cours. Un `TaskStep` (3e kind) consomme une tâche et
  referme sa carte :
  - `2·2a` la **couture Core** (port `ITaskTracker`, `TaskOperation` lire/déplacer/étiqueter, clé du
    run par `StepExecutionContext`) ✅ **posée** (`D-032`, TDD contre un stub) ;
  - `2·2b` **l'écran des tâches** 🔸 — `·1` le **trousseau** ✅ (`D-033`, `ISecretStore` sur
    `/usr/bin/security`) · `·2` le **client Linear en lecture** ✅ (`Cursus.Trackers`, port sœur
    `ITaskBoard`, arbre reconstruit, prouvé au réel) · `·3a` **la saisie du jeton dans l'UI** ·
    `·3b` l'**écran** (4e module de la surface projet). En lecture seule : le client s'éprouve sans
    jamais écrire sur le vrai tableau ;

    > **Pourquoi `·3a` existe** (tranché avec l'utilisateur, 2026-07-25) : l'écran ne peut rien
    > montrer sans jeton, et le ranger à la main serait précisément la **béquille** que la jambe 1 a
    > appris à ne pas poser (`/bin/sh -c`, retirée ensuite). On configure donc par l'interface dès
    > le premier affichage, plutôt que d'ajouter un geste manuel qu'il faudrait défaire.
  - `2·2c` **« lancer ce workflow sur cette tâche »** — rebrancher `RunTrigger.ForTask` ;
  - `2·2d` l'**authoring UI** (un `TaskStepRow` de plus, patron `D-031`) ;
  - `2·2e` la **boucle bout-en-bout** contre le dépôt.

  ⚠️ **Ré-ordonnée** (`D-033`). L'authoring venait d'abord ; il aurait meublé une pièce sans porte —
  `RunTrigger.ForTask` n'a aucun appelant en production, donc l'étape-tâche aurait été composable et
  **structurellement inerte**. L'écran d'abord, le geste ensuite : c'est ce que `architecture.md`
  §7.10.4 disait déjà (*le client existe de toute façon, puisque l'écran des actions disponibles
  impose d'interroger le tableau*), et que la première rédaction de cette liste avait égaré.
- **Déclencheurs état-tâche** ⏳ : l'auto-déclenchement d'un run sur l'état d'une carte (§7.10.6), après
  le round-trip manuel.

---

## Récapitulatif

| Jambe | Contenu | Pré-requis | Statut |
|---|---|---|---|
| **1 — Porte de gate** | Workflow build→test contre le dépôt ; ~~PATH-bundle~~ ✅, ~~log streaming~~ ✅ (`D-028`), ~~routage vécu~~ ✅, ~~champ Commande~~ ✅ (`D-029`) | aucun | ✅ **close** |
| **2 — Boucle agentique** | ~~`AgentStep`~~ ✅ (`D-030`/`D-031`) · `TaskStep`/round-trip Linear 🔸 (2·2a Core ✅ `D-032` · 2·2b écran des tâches 🔸 : trousseau ✅ `D-033` + client lecture ✅, reste ·3a saisie du jeton / ·3b écran · puis 2·2c/d/e) · déclencheurs état-tâche ⏳ | jambe 1 | 🔸 **en cours** |

## Plus loin — directions voulues, pas encore ordonnées

Des caps désirés « à terme » qui ne sont pas (encore) sur le chemin critique numéroté
ci-dessus. On les inscrit pour ne pas les perdre ; on les ordonnera quand leur tour
approchera.

- **Tests E2E de l'application.** Aujourd'hui la couche présentation (§7.12) est validée à
  la main, incrément par incrément. Des tests E2E automatiseraient ce filet : piloter l'app
  pour de vrai — ouvrir un projet, éditer un workflow, lancer un run, lire la trajectoire —
  et vérifier le résultat sans œil humain. Synergique avec le point suivant : **un mode
  pilotable est aussi un mode testable.**

- **Mode headless + barre de menus + serveur MCP — « à la Docker Desktop ».** Faire passer
  Cursus d'une app *fenêtre-d'abord* à un **service tâche-de-fond** :
  - il tourne **démarré**, présent et pilotable depuis la **barre de menus macOS** ;
  - il expose un **serveur MCP local** : un agent (ou un autre outil) peut le piloter par
    protocole — créer/lancer un workflow, lire un run — **sans fenêtre** ;
  - la **fenêtre principale devient optionnelle** : on l'ouvre au besoin, on la ferme sans
    tuer le service.

  Ce cap **longe la destination** sans être sur son chemin critique : un serveur MCP qui
  expose Cursus, c'est précisément la surface par laquelle un **agent pilote Cursus** ; et
  le headless, c'est ce qui laisse les **déclencheurs** (jambe 2) tourner en tâche de fond.
  À ordonner quand la jambe 2 se dessinera — les deux se renforcent.

  > Ce qui suit est **pressenti, pas construit** — une cible de forme, pas une décision
  > actée. Le seul point déjà tranché en discussion est le **nom** `Host` (voir plus bas) ;
  > le reste attend l'implémentation, qui ouvrira son `D-NNN` dans `decisions.md`.

  **Le point de bascule — le transport MCP.** « Tourner en arrière-plan » *et* « être
  accessible par MCP » se réconcilient par un seul choix : le transport. Le mode **stdio**
  (le client MCP *spawn* le serveur comme sous-processus) est incompatible avec la
  résidence — le serveur n'y vit que le temps du client. Le mode **HTTP local**
  (*Streamable HTTP*, le serveur écoute, le client s'y branche) est celui d'un daemon. On
  vise donc HTTP ; un **shim stdio** minuscule (un pont sans logique qui relaie vers le
  daemon) reste possible pour un client qui ne parlerait que stdio. Le SDK C# officiel
  `ModelContextProtocol` gère les deux transports et se pose sur le *generic host* .NET.

  **La forme pressentie — daemon + UI, deux process (Architecture B).** Aujourd'hui Cursus
  est un **monolithe** : `Cursus.App` (Avalonia) est à la fois la fenêtre, le composition
  root, l'hôte du moteur et le consommateur direct de la persistance. La cible sépare un
  **daemon headless** (source de vérité, sans dépendance GUI, maintenu vivant par un
  *LaunchAgent* launchd) d'une **UI** qui s'y branche et va et vient librement. C'est
  exactement le modèle Docker Desktop — un `dockerd` résident, une GUI qui n'est qu'un
  visage — et il **épouse la frontière `Core` / `App` déjà tenue** depuis le premier jalon.

  **Deux surfaces distinctes sur le daemon**, parce que leurs deux consommateurs n'ont ni
  les mêmes contraintes ni les mêmes droits :

  | | Surface **MCP** | Surface **contrôle UI** |
  |---|---|---|
  | Qui s'y branche | clients MCP tiers (Claude Code/Desktop) | la propre UI de Cursus |
  | Protocole | MCP — **imposé de l'extérieur** | le nôtre, libre |
  | Transport | **TCP loopback** (`127.0.0.1`) — les clients MCP attendent une URL `http://` | **socket Unix** — pas de port, accès par permissions de fichier |
  | Exposition | un port ouvert → à protéger (bind strict, jeton) | un fichier → l'OS filtre déjà |

  On ne *choisit* pas le transport de la surface MCP (il est dicté par ce que les clients
  savent consommer) ; on choisit entièrement celui de la surface UI, d'où le socket Unix,
  plus sûr et sans port à réserver. Variante à garder en tête : **zéro port par défaut** —
  tout sur le socket Unix, le port TCP n'étant ouvert que sur demande (ou jamais, si l'on
  ne passe que par le shim stdio). Arbitrage commodité ⇄ exposition, à trancher plus tard.

  **MCP activable en paramètre.** Le serveur MCP est un **adaptateur monté
  conditionnellement par le daemon** : réglage *off* → aucun port bindé, aucune surface MCP
  n'existe. Ni le noyau ni l'UI n'ont à savoir que MCP existe — le levier vit dans le seul
  daemon.

  **Le repackage visé.** Les trois packages actuels (`Core` / `Persistence` / `App`)
  deviennent quatre, le long des couches d'une architecture hexagonale :

  ```
                   Core     (domaine + moteur + PORTS ; zéro dépendance sortante)
                  ▲  ▲  ▲
      ┌───────────┘  │  └───────────┐
    Infra          Host           UI
  (adaptateurs   (daemon :        (Avalonia, PROCESS séparé,
   SORTANTS :     composition       client du socket)
   SQLite,        root, launchd,        │  socket Unix
   client claude, monte MCP+socket) ◄───┘
   PTY)
  ```

  | Package | Rôle | Origine |
  |---|---|---|
  | **`Cursus.Core`** | domaine, moteur, **ports** | inchangé |
  | **`Cursus.Infra`** | adaptateurs **sortants** : persistance SQLite, client `claude`, PTY | ex-`Persistence`, élargi |
  | **`Cursus.Host`** | le **daemon** : composition root, cible launchd, monte les adaptateurs **entrants** (serveur MCP, listener socket) et décide lesquels selon la config | **neuf** (extrait de l'actuel `App`) |
  | **`Cursus.UI`** | la façade Avalonia, dans son propre process | ex-`App`, allégé |

  **Pourquoi `Host` et pas `App`.** Le daemon est la substance, la fenêtre n'en est qu'un
  visage — mais le mot « App » trahit ce modèle : pour tout lecteur, « l'app » est ce qu'on
  ouvre, donc la fenêtre. Avoir une `UI` visible et une `App` invisible inverse l'intuition,
  et collide avec le bundle `.app` macOS. `Host` est idiomatique .NET (le *generic host*) et
  dit ce que c'est : ce qui héberge et câble. Docker ne nomme jamais son daemon « the app ».

  **La vraie couture à défaire.** Le renommage n'est pas qu'un `mv` de `.csproj` :
  aujourd'hui `App → Persistence` (la fenêtre lit la base **en direct**). Dans la cible,
  **seul le daemon touche la persistance** ; l'UI passe par le socket. L'arête `UI → Infra`
  **se coupe** — c'est le vrai travail derrière la bascule A→B, et ce qui la rend plus qu'un
  changement cosmétique.

  **Le status item.** Le `TrayIcon` d'Avalonia (cross-plateforme, icône + menu) suffit pour
  démarrer ; un rendu riche (popover, indicateur de run *live*) demanderait le natif
  `NSStatusItem` via interop — à n'envisager que si l'ambition le réclame.

## Écarté (et pourquoi le noter)

- **Amorcer Linear comme point de départ** — écarté : Cursus ne sait pas encore consommer
  une tâche ; ce serait du PM sans retour de dogfooding avant la jambe 2. Linear entre *à*
  la jambe 2, pas avant. (L'écart est écrit pour ne pas rejouer le débat dans six mois.)
