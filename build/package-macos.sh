#!/usr/bin/env bash
#
# Construit Cursus.app, un bundle macOS installable dans /Applications.
#
# Usage :  build/package-macos.sh [--install]
#
# Sans argument, le bundle est produit dans build/out/Cursus.app.
# Avec --install, il est en plus copié dans /Applications.

set -euo pipefail

readonly REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly OUT_DIR="${REPO_ROOT}/build/out"
readonly APP="${OUT_DIR}/Cursus.app"
readonly RID="osx-arm64"

echo "==> Publication (${RID}, self-contained)"
rm -rf "${OUT_DIR}"
mkdir -p "${APP}/Contents/MacOS" "${APP}/Contents/Resources"

# Pas de trimming : il casse régulièrement Avalonia, qui résout ses contrôles
# et ses convertisseurs par réflexion. Un bundle de 120 Mo ne gêne personne ici.
dotnet publish "${REPO_ROOT}/src/Cursus.App" \
    --configuration Release \
    --runtime "${RID}" \
    --self-contained true \
    -p:PublishTrimmed=false \
    --output "${APP}/Contents/MacOS" \
    --verbosity quiet

cp "${REPO_ROOT}/build/Info.plist" "${APP}/Contents/Info.plist"

echo "==> Vérification des bibliothèques natives"
# libghostty-vt porte le moteur VT natif. Si elle manque, RoyalTerminal
# retombe SILENCIEUSEMENT sur son moteur managé, qui suit mal DECCKM : les
# flèches ne sont plus encodées comme les TUI l'attendent. Un bundle sans
# cette bibliothèque se lance très bien et se comporte mal — d'où ce garde-fou.
#
# libe_sqlite3 porte le moteur SQLite (SQLitePCLRaw). Depuis que Cursus.App
# référence Cursus.Persistence (jalon 6c·3a), ouvrir le journal d'un projet la
# charge ; absente du bundle, l'app se lance mais lève dès qu'on sélectionne un
# projet. Ce contrôle, volontairement absent tant que la référence n'existait
# pas (il aurait échoué sur un faux positif), devient dû ici.
for lib in libghostty-vt.dylib libAvaloniaNative.dylib libSkiaSharp.dylib libe_sqlite3.dylib; do
    if [[ ! -f "${APP}/Contents/MacOS/${lib}" ]]; then
        echo "ERREUR : ${lib} absente du bundle." >&2
        exit 1
    fi
done

echo "==> Signature ad-hoc"
# Signature ad-hoc (« - ») : suffisante pour exécuter l'app sur la machine qui
# l'a construite, mais NON notarisée. Gatekeeper la refusera sur une autre
# machine — la distribution exigerait un compte développeur Apple.
codesign --force --deep --sign - "${APP}" 2>/dev/null

echo "==> Bundle prêt : ${APP} ($(du -sh "${APP}" | cut -f1))"

if [[ "${1:-}" == "--install" ]]; then
    echo "==> Installation dans /Applications"
    rm -rf "/Applications/Cursus.app"
    cp -R "${APP}" "/Applications/Cursus.app"
    # Le bundle n'a jamais été téléchargé, donc pas de quarantaine à retirer.
    echo "==> Installé : /Applications/Cursus.app"
fi
