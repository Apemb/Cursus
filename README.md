# Cursus

Projet d'expérimentation dont la cible finale est un **manageur de workflow agentique** — l'orchestration de sessions et d'agents IA au sein d'une application de bureau.

La première étape est un **équivalent de TMUX au feeling « application native »** : une liste de sessions à gauche, un panneau terminal à droite.

## Vision

| Étape | Objectif |
|-------|----------|
| **1. Terminal manager** | Ouvrir, lister et basculer entre des sessions shell dans une UI native. |
| **2. Sessions avancées** | Persistance, détacher/rattacher façon TMUX, layouts. |
| **3. Orchestrateur agentique** | Lancer et coordonner des agents IA (CLI, process) comme des sessions de premier ordre. |

## Stack

- **[.NET 10](https://dotnet.microsoft.com/)** (C#) — géré via [asdf](https://asdf-vm.com/) (`dotnet-core 10.0.302`).
- **[Avalonia](https://avaloniaui.net/) 12** — UI cross-platform, rendu Skia, légère (pas de Chromium embarqué).
- **[RoyalTerminal](https://github.com/royalapplications/RoyalTerminal)** *(à intégrer)* — émulateur de terminal complet : rendu Skia, moteur VT `libghostty-vt`, PTY (`forkpty` / ConPTY), transports SSH/TCP/telnet/serial.

Le terminal sera abstrait derrière une interface (`ITerminalSession`) pour éviter tout couplage dur, en vue de la partie agentique.

## Structure

```
Cursus.slnx                 Solution (format XML .NET 10)
src/
└── Cursus.App/             Application Avalonia (UI)
```

> `Cursus.Core` (logique de sessions) viendra s'ajouter au fur et à mesure.

## Prérequis

- .NET SDK **10.0+** (`dotnet --version`).
- macOS (cible actuelle) ; Avalonia reste cross-platform.

## Build & lancement

```bash
dotnet build Cursus.slnx           # compiler
dotnet run --project src/Cursus.App # lancer l'application
```

## État

Scaffolding validé : l'application compile et se lance sur macOS (écran de validation avec un compteur). Prochaine étape : intégrer RoyalTerminal et poser la structure « sessions / terminal ».
