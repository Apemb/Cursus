#!/usr/bin/env bash
#
# Désinstalle Cursus.app de /Applications. Idempotent : ne se plaint pas si
# l'app n'est pas installée — pensé pour un cycle installer/désinstaller répété
# pendant le développement.
#
# Usage :  build/uninstall-macos.sh [--purge]
#
#   --purge  efface aussi la configuration machine (~/.config/cursus), en
#            déléguant à reset-data.sh. Sans --purge, les données survivent à
#            la désinstallation, comme pour toute app.

set -euo pipefail

readonly REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly APP="/Applications/Cursus.app"

if [[ -d "$APP" ]]; then
    rm -rf "$APP"
    echo "==> Désinstallé : $APP"
else
    echo "==> Rien à faire : $APP absent"
fi

if [[ "${1:-}" == "--purge" ]]; then
    echo "==> Purge de la configuration machine"
    "${REPO_ROOT}/build/reset-data.sh" --yes
fi
