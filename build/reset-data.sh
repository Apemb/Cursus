#!/usr/bin/env bash
#
# Remet Cursus à un état vierge pour le développement : efface les données
# d'exécution, jamais les fichiers versionnés.
#
# Usage :
#   build/reset-data.sh [--here] [--project <chemin>]... [--dry-run] [--yes]
#
# Sans option, seule la configuration machine (~/.config/cursus) est effacée —
# la liste des projets ouverts et, à terme, les réglages de la machine.
#
#   --here        efface aussi le runtime du dépôt courant.
#   --project P   idem, pour un projet désigné (cumulable).
#   --dry-run     montre ce qui serait supprimé, ne supprime rien.
#   --yes         ne demande pas confirmation.
#
# Le « runtime » d'un projet, ce sont ses .cursus/{cursus.db*, runs, worktrees} :
# l'observation locale, gitignorée. project.json et workflows/ — l'intention
# versionnée — ne sont JAMAIS touchés.

set -euo pipefail

# Même résolution que le SpecialFolder.ApplicationData de .NET : XDG d'abord,
# repli sur ~/.config. C'est ce qui garantit que le script vise exactement le
# dossier que l'application écrit.
readonly CONFIG_DIR="${XDG_CONFIG_HOME:-$HOME/.config}/cursus"

DRY_RUN=false
ASSUME_YES=false
PROJECTS=()

while [[ $# -gt 0 ]]; do
    case "$1" in
        --here)      PROJECTS+=("$(pwd)"); shift ;;
        --project)   PROJECTS+=("$2"); shift 2 ;;
        --dry-run)   DRY_RUN=true; shift ;;
        --yes)       ASSUME_YES=true; shift ;;
        *) echo "Option inconnue : $1" >&2; exit 2 ;;
    esac
done

# Supprime une cible en respectant le mode dry-run ; muet si elle n'existe pas.
remove_path() {
    local target="$1"
    [[ -e "$target" ]] || return 0
    if $DRY_RUN; then
        echo "    [dry-run] supprimerait $target"
    else
        rm -rf "$target"
        echo "    supprimé $target"
    fi
}

clean_project() {
    local proj="$1"
    local cursus_dir="$proj/.cursus"
    if [[ ! -d "$cursus_dir" ]]; then
        echo "==> $proj : pas de .cursus/, ignoré"
        return 0
    fi
    echo "==> Runtime du projet : $proj"

    # Les worktrees sont enregistrés dans le dépôt principal. Les effacer du
    # disque sans prévenir git laisse des enregistrements fantômes que seul
    # « git worktree prune » nettoie — d'où l'ordre : retirer les dossiers,
    # puis élaguer.
    if [[ -d "$cursus_dir/worktrees" ]] && git -C "$proj" rev-parse --git-dir >/dev/null 2>&1; then
        remove_path "$cursus_dir/worktrees"
        if $DRY_RUN; then
            echo "    [dry-run] git worktree prune"
        else
            git -C "$proj" worktree prune
            echo "    git worktree prune"
        fi
    fi

    # La base et ses compagnons WAL (« -wal », « -shm »), puis l'historique des
    # runs. Le glob peut ne rien capturer : le garde de remove_path l'absorbe.
    local db
    for db in "$cursus_dir"/cursus.db*; do
        remove_path "$db"
    done
    remove_path "$cursus_dir/runs"
}

echo "Cible :"
echo "  - configuration machine : $CONFIG_DIR"
# Expansion protégée : sous set -u, bash 3.2 (défaut macOS) refuse "${arr[@]}"
# sur un tableau vide. L'idiome ${arr[@]+"${arr[@]}"} rend une liste vide sans
# lever, ce qui est exactement le cas « aucun projet ».
for proj in ${PROJECTS[@]+"${PROJECTS[@]}"}; do
    echo "  - runtime du projet     : $proj/.cursus/{cursus.db*, runs, worktrees}"
done

if ! $ASSUME_YES && ! $DRY_RUN; then
    read -r -p "Confirmer la suppression ? [y/N] " reply
    [[ "$reply" =~ ^[Yy]$ ]] || { echo "Annulé."; exit 0; }
fi

echo "==> Configuration machine"
remove_path "$CONFIG_DIR"

for proj in ${PROJECTS[@]+"${PROJECTS[@]}"}; do
    clean_project "$proj"
done

echo "==> Terminé."
