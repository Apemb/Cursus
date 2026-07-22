# Cursus

Manageur de workflow agentique en construction — une application de bureau qui orchestre des agents
de code tournant dans de vrais terminaux, avec supervision humaine.

L'état réel du projet, les décisions et la trajectoire vivent dans **[`docs/design/architecture.md`](docs/design/architecture.md)**.
À lire avant toute intervention non triviale.

## Prérequis

.NET SDK 10 (épinglé par `global.json`) et macOS pour l'application — le noyau et ses tests sont
portables POSIX.

## Développer

```bash
dotnet build                          # 0 warning attendu
dotnet test                           # suite entièrement verte attendue
dotnet run --project src/Cursus.App
```

## Installer l'application

```bash
build/package-macos.sh --install      # construit Cursus.app et l'installe dans /Applications
```

Sans `--install`, le bundle est simplement produit dans `build/out/`.

```bash
build/uninstall-macos.sh              # retire Cursus.app de /Applications (idempotent)
build/reset-data.sh --here            # efface le runtime : config machine + .cursus du dépôt courant
```

`reset-data.sh` remet Cursus à un état vierge pour le développement — il efface la configuration
machine (`~/.config/cursus`) et, avec `--here` ou `--project`, le runtime d'un projet (base, runs,
worktrees), **sans jamais toucher au versionné** (`project.json`, `workflows/`). `--dry-run` montre
ce qui serait supprimé ; `--purge` sur la désinstallation enchaîne les deux.

> L'application est signée **ad-hoc**, sans notarisation : elle s'exécute sur la machine qui l'a
> construite, mais Gatekeeper la refusera ailleurs. Une étape de workflow qui invoque un binaire
> installé par Homebrew ou `asdf` échouera dans l'application installée alors qu'elle fonctionne en
> `dotnet run` — le `PATH` d'une application lancée depuis le Finder est tronqué
> ([architecture.md §6.6](docs/design/architecture.md)).
