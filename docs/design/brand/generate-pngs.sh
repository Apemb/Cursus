#!/usr/bin/env bash
# Génère les PNG/.icns de production depuis les SVG maîtres de ce dossier.
#
#   ./generate-pngs.sh
#
# Rasteriseur requis (le premier trouvé est utilisé) :
#   brew install librsvg     # rsvg-convert  (recommandé)
#   brew install inkscape
#   pip install cairosvg
#
# Sorties dans ./out :
#   Cursus.icns + Cursus.iconset/   — icône d'application (via iconutil, macOS)
#   app-icon-1024.png               — master plein cadre
#   cursusTemplate.png @2x @3x      — gabarit barre de menus (repos)
#   frames/flow_NN.png              — frames de l'animation « flux »
#
# NB gabarit : `cursusTemplate*.png` est noir + alpha ; nommé « …Template.png »,
# macOS le teinte automatiquement selon la barre. Les frames « flux » cuisent le
# vert par-dessus une base BASE (noire par défaut = barre claire) ; pour une barre
# sombre, régénérer avec BASE=white. La pastille d'état (attente/échec) se compose
# au runtime, elle n'est pas rasterisée ici.
set -euo pipefail
cd "$(dirname "$0")"
OUT=out
BASE="${BASE:-black}"     # base des frames d'animation (black | white)
GREEN="#22C55E"
PERIM=142.44              # périmètre du diamant (cf. charte-graphique.md §1)
FRAMES="${FRAMES:-20}"

mkdir -p "$OUT"

# --- choix du rasteriseur -----------------------------------------------------
if command -v rsvg-convert >/dev/null 2>&1; then
  raster() { rsvg-convert -w "$1" -h "$1" "$2" -o "$3"; }
elif command -v inkscape >/dev/null 2>&1; then
  raster() { inkscape "$2" -w "$1" -h "$1" -o "$3" >/dev/null 2>&1; }
elif command -v cairosvg >/dev/null 2>&1; then
  raster() { cairosvg "$2" -W "$1" -H "$1" -o "$3"; }
else
  echo "Aucun rasteriseur SVG trouvé." >&2
  echo "Installe : brew install librsvg  |  brew install inkscape  |  pip install cairosvg" >&2
  exit 1
fi

# --- 1) icône d'application → .iconset → .icns --------------------------------
ICONSET="$OUT/Cursus.iconset"
mkdir -p "$ICONSET"
for s in 16 32 128 256 512; do
  raster "$s"          app-icon.svg "$ICONSET/icon_${s}x${s}.png"
  raster "$((s * 2))"  app-icon.svg "$ICONSET/icon_${s}x${s}@2x.png"
done
raster 1024 app-icon.svg "$OUT/app-icon-1024.png"
if command -v iconutil >/dev/null 2>&1; then
  iconutil -c icns "$ICONSET" -o "$OUT/Cursus.icns"
  echo "→ $OUT/Cursus.icns"
else
  echo "→ $ICONSET (iconutil absent : .icns non généré — outil macOS)"
fi

# --- 2) gabarit barre de menus (repos) ---------------------------------------
raster 18 tray-idle.svg "$OUT/cursusTemplate.png"
raster 36 tray-idle.svg "$OUT/cursusTemplate@2x.png"
raster 54 tray-idle.svg "$OUT/cursusTemplate@3x.png"
echo "→ $OUT/cursusTemplate.png (+@2x @3x)"

# --- 3) frames de l'animation « flux » ---------------------------------------
# Chaque frame = le diamant avec un stroke-dashoffset figé ; le vert révélé
# balaie le périmètre. On émet un SVG statique par frame puis on rasterise.
mkdir -p "$OUT/frames"
i=0
while [ "$i" -lt "$FRAMES" ]; do
  off=$(awk "BEGIN{printf \"%.2f\", -$PERIM * $i / $FRAMES}")
  n=$(printf "%02d" "$i")
  tmp="$OUT/frames/.frame_$n.svg"
  cat > "$tmp" <<SVG
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100">
  <defs>
    <mask id="m" maskUnits="userSpaceOnUse">
      <rect width="100" height="100" fill="#000"/>
      <path d="M22 50 L50 28 L78 50 L50 72 Z" fill="none" stroke="#fff" stroke-width="24"
            stroke-linejoin="round" stroke-linecap="round"
            stroke-dasharray="71.22 71.22" stroke-dashoffset="$off"/>
    </mask>
  </defs>
  <g fill="$BASE" stroke="$BASE">
    <path d="M22 50 L50 28 L78 50 L50 72 Z" fill="none" stroke-width="7"
          stroke-linejoin="round" stroke-linecap="round"/>
    <circle cx="22" cy="50" r="8"/><circle cx="50" cy="28" r="8"/>
    <circle cx="78" cy="50" r="8"/><circle cx="50" cy="72" r="8"/>
  </g>
  <g mask="url(#m)">
    <path d="M22 50 L50 28 L78 50 L50 72 Z" fill="none" stroke="$GREEN" stroke-width="7"
          stroke-linejoin="round" stroke-linecap="round"/>
    <g fill="$GREEN"><circle cx="22" cy="50" r="8"/><circle cx="50" cy="28" r="8"/>
      <circle cx="78" cy="50" r="8"/><circle cx="50" cy="72" r="8"/></g>
  </g>
</svg>
SVG
  raster 36 "$tmp" "$OUT/frames/flow_$n.png"
  rm -f "$tmp"
  i=$((i + 1))
done
echo "→ $OUT/frames/flow_00..$(printf '%02d' $((FRAMES - 1))).png ($FRAMES frames, base=$BASE)"
