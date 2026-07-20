# RoyalTerminal 0.4.0 — référence d'API sondée

> **Pourquoi ce document existe.** RoyalTerminal ne livre **aucun XML de documentation**. Tout ce qui
> suit a été obtenu par **inspection des assemblies** (le 2026-07-19) et par lecture de la source
> publique. Sans ce fichier, la connaissance vivrait hors du dépôt et se perdrait à la prochaine
> montée de version.
>
> ⚠️ **Périmètre de validité : version 0.4.0.** Rien ici n'est garanti par un contrat de compatibilité.
> À la moindre montée de version, **re-sonder** (méthode au §1) et mettre ce document à jour.

Paquets concernés, tous en `0.4.0` (`src/Cursus.App/Cursus.App.csproj`) :
`RoyalApps.RoyalTerminal.Avalonia`, `RoyalApps.RoyalTerminal.Terminal.Vt.Ghostty`,
`RoyalApps.RoyalTerminal.GhosttySharp.Native.OSX`.

---

## 1. Comment re-sonder

Écrire un dumper jetable utilisant **`System.Reflection.MetadataLoadContext`** — il lit les métadonnées
**sans exécuter** les assemblies, ce qui évite d'avoir à satisfaire toutes leurs dépendances au
chargement.

- Cibles : `~/.nuget/packages/royalapps.royalterminal.*/<version>/lib/net10.0/*.dll`
- Le resolver doit pointer sur le **répertoire de sortie de l'app** (`src/Cursus.App/bin/Debug/net10.0/`)
  pour résoudre Avalonia et le runtime.
- Filtrer les types par mots-clés : `Terminal`, `Pty`, `Session`, `Vt`, `Snapshot`, `ShellIntegration`.

Pour la source, quand les métadonnées ne suffisent pas (comportement, pas signature) :
`raw.githubusercontent.com/royalapplications/RoyalTerminal/main/...`, et l'API GitHub
`git/trees/main?recursive=1` pour lister l'arborescence.

---

## 2. `TerminalControl` — l'API de base

Type : `RoyalTerminal.Avalonia.Controls.TerminalControl`.

Deux constructeurs :

| Ctor | Usage |
|---|---|
| `()` | Moteur VT **managé** — ⚠️ voir le gotcha du §3 |
| 6 arguments | `(ITerminalSessionService, ITerminalInputAdapter, ITerminalSelectionService, ITerminalScrollService, IVtProcessorFactory, IPtyFactory)` — le seul moyen d'injecter le moteur VT natif |

**Démarrer un shell local** : `StartPty(string shell, string workingDirectory, IReadOnlyList<string> arguments)`.
Autres transports : `StartSessionAsync(ITerminalTransportOptions, ct)`, `StartSshAsync`,
`StartTelnetAsync`, `StartSerialAsync`, `StartRawTcpAsync`. Puis `StopPty()`, `SendInput(...)`.

⚠️ **Le PTY doit démarrer quand le contrôle est affiché et mesuré** (bounds non nulles) → brancher sur
l'événement `Loaded`, pas plus tôt. C'est ce que fait `src/Cursus.App/MainWindow.axaml.cs`.

Propriétés utiles : `FontFamilyName`, `TerminalFontSize`, `VtProcessorPreference`
(enum `Auto=0 / Managed=1 / Native=2`, namespace `RoyalTerminal.Terminal`), `IsUsingNativeVtProcessor`,
`HasPty`, `Columns` / `Rows`, `Theme`, `ScrollbackLimit`, `PreserveScrollbackOnSessionStart`,
`ClearScrollback()`.

Namespaces des services par défaut, faciles à chercher au mauvais endroit :

| Type | Namespace |
|---|---|
| `TerminalSessionService` | `RoyalTerminal.Terminal.Services` |
| `DefaultTerminalInputAdapter`, `DefaultTerminalSelectionService`, `DefaultTerminalScrollService` | `RoyalTerminal.Avalonia.Services` |
| `DefaultVtProcessorFactory`, `DefaultPtyFactory` | `RoyalTerminal.Terminal` |

---

## 3. Moteur VT natif — le gotcha à ne pas redécouvrir

**Le constructeur sans paramètre laisse une `DefaultVtProcessorFactory` vide → moteur managé → DECCKM
(« application cursor keys ») mal suivi → les touches fléchées ne sont pas encodées comme les TUI
l'attendent.** libghostty-vt n'est pas un raffinement de performance, c'est une condition de
fonctionnement.

Le branchement, centralisé dans `src/Cursus.App/Terminals/NativeTerminalFactory.cs` :

```csharp
var vtFactory = new DefaultVtProcessorFactory(
    new INativeVtProcessorProvider[] { new GhosttyVtProcessorProvider() });
```

`GhosttyVtProcessorProvider` (namespace `RoyalTerminal.Terminal`) expose `IsAvailable` et un
`static Prewarm()`. La dylib native (arm64/x64) est copiée en sortie par le paquet
`GhosttySharp.Native.OSX`.

⚠️ Ce paquet est **spécifique macOS** : c'est la raison pour laquelle `Cursus.App` ne tourne
aujourd'hui que sur macOS. Un portage exigera un `INativeVtProcessorProvider` par OS.

---

## 4. Signaux pour la détection d'état d'agent

Les quatre dépendances dures identifiées par la recherche sont **toutes couvertes**, et il y a mieux.

### 4.1 Les quatre signaux requis

| Besoin | API |
|---|---|
| **Écran rendu** (entrée du moteur screen-manifest) | `TryExportSnapshot(TerminalSnapshotExportFormat.PlainText, ref TerminalSnapshotExportOptions { Unwrap = true, TrimTrailingWhitespace = true }, out string snapshot)` |
| **Titre OSC** (le spinner braille de Claude) | événement `TitleChanged : EventHandler<string>` |
| **Octets reçus** (« ça coule donc ça travaille ») | événement `DataReceived : EventHandler<TerminalDataEventArgs>` (`Data : ReadOnlyMemory<byte>`) |
| **PID enfant** (process de premier plan, à la Herdr) | `Pty : IPty` → `IPty.ChildPid : int`, `IsRunning` ; événement `ProcessExited : EventHandler<int>` |

`TryExportSnapshot` en `PlainText` + `Unwrap` est **l'équivalent exact de `tmux capture-pane -p -J`**.
Formats disponibles : `PlainText=0`, `StyledVt=1`, `Html=2`.

⚠️ **Ne pas faire du cell-walking.** L'accès bas niveau existe (`Screen : TerminalScreen` →
`GetViewportRow(i) : TerminalRow` → `ReadOnlyCells : ReadOnlySpan<TerminalCell>`) mais `TerminalCell`
n'expose publiquement que `HasContent` : le texte n'est pas atteignable par là. Le snapshot texte est
la seule voie praticable.

### 4.2 Shell integration OSC 133 — intégrée *et* amorçable

Événement `ShellIntegrationEventReceived : EventHandler<TerminalShellIntegrationEventArgs>` livrant
`TerminalShellIntegrationEvent { Kind, CommandLine, WorkingDirectory, ExitCode?, TimestampUtc }`.

`Kind` ∈ `{ PromptStarted, NewCommand, InputStarted, OutputStarted, CommandFinished (+ExitCode),
WorkingDirectoryChanged, FreshLine, … }`.

Le snippet shell à injecter est **généré par la bibliothèque** :
`TerminalShellIntegrationBootstrapBuilder.Build(TerminalShellIntegrationBootstrapOptions { Shell = Bash|Zsh|Fish|PowerShell, EmitSemanticPrompt = true, EmitWorkingDirectory = true })`.

→ **Cycle de vie de commande fiable pour les sessions shell, sans scraping.** Le grain fin
(« l'agent attend une permission ») reste du ressort des hooks ou du screen-manifest.

### 4.3 Détecter un TUI plein écran — et pourquoi ce n'est pas le signal qu'on croit

Événement `ModeChanged : EventHandler<TerminalModeState>` (`AlternateScreen`, `ApplicationCursorKeys`…)
ou propriété `Screen.AlternateBufferActive`.

⚠️ **`AlternateScreen == true` n'est PAS un signal fiable « Claude Code tourne ».** Le renderer
**« classic » (défaut) de Claude Code rend *inline* dans le buffer principal** et alimente donc le
scrollback natif ; l'alt screen n'est que le mode **opt-in `/tui fullscreen`**. L'heuristique ne vaut
donc que pour `vim`/`less`/`htop`, ou si l'on force soi-même le fullscreen.

Variables d'environnement pertinentes : `CLAUDE_CODE_DISABLE_ALTERNATE_SCREEN=1` force le classic ;
`CLAUDE_CODE_NO_FLICKER=1` accompagne le fullscreen. Les sessions background et `claude attach` sont
**toujours** en fullscreen.

**Conséquence pour la capture** : en classic, la conversation rendue est **dans le scrollback**, donc
capturable par snapshot ; en fullscreen, non — il faudra passer par le JSONL sidecar ou les hooks.

### 4.4 Injection de frappes

`SendInput(string)` ou `IPty.Write(...)` — c'est le mécanisme d'un futur « auto-yes ».

### 4.5 Persistance déjà fournie

`ITerminalWorkspaceStore` / `JsonFileTerminalWorkspaceStore` (+ `TerminalWorkspaceDocument` :
tabs/panes/windows), `ITerminalSessionProfileStore`, `ITerminalCommandHistoryStore`, chacun avec une
implémentation `JsonFile…`.

RoyalTerminal sérialise donc déjà layout, profils et historique. Le modèle métier
(`Task`/`Workspace`/`Session`) reste à nous, mais **la couche layout-terminal peut s'appuyer dessus**
plutôt que d'être réécrite.

---

## 5. Lancer un binaire arbitraire dans le PTY

Point sondé **sur la source** (`RoyalTerminal.Terminal.Pty.Unix/Terminal/UnixPty.cs`), décisif pour le
confinement OS envisagé (`srt`, `sandbox-exec`).

**RoyalTerminal lance le process PTY par `forkpty()` + `execvp()` DIRECT — aucun `/bin/sh -c`.**

Conséquences :

- Le paramètre `shell` **n'est pas validé** comme étant un vrai shell : c'est un chemin passé tel quel
  à `execvp`. On peut donc lancer **n'importe quel exécutable** comme process de PTY (par exemple `srt`
  ou `/usr/bin/sandbox-exec`), avec l'agent réel en arguments.
- C'est de l'`argv[]`, pas une ligne de commande : **aucun quoting, échappement ou globbing à gérer**.
  Chaque token est un élément de liste.
- `shell` sans `/` → recherche dans le `PATH` ; avec `/` → exec direct.
- **Échec d'exec** (binaire introuvable) → l'enfant fait `_exit(127)` → `ProcessExited` rend **127**,
  sans message. Vérifier le chemin absolu avant de conclure à autre chose.

---

## 6. Injection d'environnement — le piège

⚠️ **`StartPty(shell, wd, args)` force `Environment: null`** (`TerminalControl.cs`). Impossible de
passer un environnement personnalisé par cette voie.

Pour injecter un env (port attribué, hooks, allowlist, environnement d'un process confiné), deux
chemins :

1. **Bas niveau** — `IPty.Start(shell, cols, rows, wd, Dictionary<string,string> environment, args)`,
   via `IPtyFactory` / `DefaultPtyFactory` ;
2. **Transport** — `StartSessionAsync(PtyTransportOptions { Command = TerminalCommandSpec(bin, args), Environment = envDict, WorkingDirectory, Dimensions })`.

Détails de comportement : l'environnement est **fusionné par-dessus celui de l'app hôte**
(`setenv` avec `overwrite=1`), il ne part pas d'une base vierge ; `TERM` est forcé à `xterm-256color` ;
`workingDirectory` provoque un `chdir` avant l'exec et doit être **absolu**.

---

## 7. Conséquence d'architecture

Trois signaux de détection d'état sont disponibles, à combiner **par priorité décroissante** :

1. **Hooks Claude Code** — primaire, état interne fin ;
2. **Shell integration OSC 133** (§4.2) — cycle de commande fiable, sessions shell ;
3. **Moteur screen-manifest** — fallback universel, sur `TryExportSnapshot(PlainText, Unwrap)` +
   `TitleChanged` + `DataReceived` + `AlternateScreen`.

Le moteur (3) est une fonction pure `fn(écran, titre OSC) → état` : **entièrement testable en xUnit sur
des snapshots figés**, sans terminal. C'est ce qui en fait un bon candidat TDD le jour où le monde
agent sera abordé.

Voir `docs/design/architecture.md` §6 pour la place de tout ceci dans le dépôt, et
`docs/research/agentic-workflows-landscape.md` pour les preuves externes qui ont conduit à ces choix.
