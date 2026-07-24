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
  referme sa carte : `2·2a` la **couture Core** (port `ITaskTracker`, `TaskOperation` lire/déplacer/
  étiqueter, clé du run par `StepExecutionContext`) ✅ **posée** (`D-032`, TDD contre un stub) ; `2·2b`
  le **client Linear réel** (projet dédié hors Core, secret trousseau) ; `2·2c` l'**authoring UI** (un
  `TaskStepRow` de plus, patron `D-031`) ; `2·2d` la **boucle bout-en-bout** contre le dépôt.
- **Déclencheurs état-tâche** ⏳ : l'auto-déclenchement d'un run sur l'état d'une carte (§7.10.6), après
  le round-trip manuel.

---

## Récapitulatif

| Jambe | Contenu | Pré-requis | Statut |
|---|---|---|---|
| **1 — Porte de gate** | Workflow build→test contre le dépôt ; ~~PATH-bundle~~ ✅, ~~log streaming~~ ✅ (`D-028`), ~~routage vécu~~ ✅, ~~champ Commande~~ ✅ (`D-029`) | aucun | ✅ **close** |
| **2 — Boucle agentique** | ~~`AgentStep`~~ ✅ (`D-030`/`D-031`) · `TaskStep`/round-trip Linear 🔸 (2·2a Core ✅ `D-032`, reste 2·2b/c/d) · déclencheurs état-tâche ⏳ | jambe 1 | 🔸 **en cours** |

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

## Écarté (et pourquoi le noter)

- **Amorcer Linear comme point de départ** — écarté : Cursus ne sait pas encore consommer
  une tâche ; ce serait du PM sans retour de dogfooding avant la jambe 2. Linear entre *à*
  la jambe 2, pas avant. (L'écart est écrit pour ne pas rejouer le débat dans six mois.)
