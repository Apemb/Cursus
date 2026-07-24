# Charte graphique — Cursus

> Compagnon **visuel de l'identité**, sans autorité sur l'architecture. Il fixe la marque,
> la palette et les icônes ; il complète `schemas.md` (qui, lui, cartographie le *code*).
> Les assets vivent dans **`docs/design/brand/`** ; la maquette d'origine, validée à la main,
> est archivée en **`docs/design/maquettes/identite-visuelle.html`**.
>
> **Statut : la marque « flux » est validée.** Le reste (dégradé exact, gabarit macOS,
> génération des frames) est spécifié ici mais reste à *sortir en assets de production*.

---

## 1 · La marque — le « flux »

Un **diamant fan-out / join** : une entrée à gauche, deux branches, une jonction à droite.
Ce n'est pas une métaphore posée sur le produit — c'est le **modèle d'exécution du noyau**
lui-même (une étape se divise selon le code de sortie, puis les chemins se rejoignent).

Propriété exploitée par l'animation : les quatre arêtes forment le **périmètre fermé** du
diamant (`A→B→D→C→A`), donc une **boucle** — sur laquelle le vert peut tourner.

**Géométrie** (repère `0–100`, la source de tous les rendus) :

| Nœud | Rôle | x | y |
|---|---|---|---|
| A | entrée | 22 | 50 |
| B | branche haute | 50 | 28 |
| D | jonction | 78 | 50 |
| C | branche basse | 50 | 72 |

- **Arêtes** : chemin fermé `M22 50 L50 28 L78 50 L50 72 Z`, `stroke-width: 7`, jointures et
  bouts **arrondis**.
- **Nœuds** : disques `r = 8` aux quatre sommets.
- **Zone de sécurité** : la marque occupe le carré `14–86` (nœuds + rayon). Garder cette
  marge libre ; ne pas la déformer (échelle uniforme uniquement).
- **Périmètre** : `142.44` unités — quatre arêtes égales de `35.61`. *(Sert au calage de
  l'animation, §5.)*

Fichier maître : **`brand/mark.svg`** (monochrome, `currentColor` — teintable partout).

---

## 2 · Palette

Deux registres, à ne pas confondre : **la marque** (indigo) porte l'identité ; **l'état**
(vert / rouge / ambre) est une **sémantique de routage**, jamais un accent décoratif — le
cœur de Cursus route sur le code de sortie, donc ces couleurs *veulent dire* quelque chose.

### Marque

| Rôle | Clair | Sombre |
|---|---|---|
| Indigo (primaire · liens · sélection) | `#4F46E5` | `#8E88F6` |
| Violet (partenaire de dégradé) | `#7C3AED` | `#A78BFA` |
| Indigo voilé (fonds, surbrillance douce) | `#EEEEFB` | `#23223A` |

### État — sémantique de routage

| Rôle | Clair | Sombre |
|---|---|---|
| En cours / réussi (sortie 0) | `#22C55E` | `#34D07B` |
| Échec (sortie ≠ 0 · attention) | `#EF4444` | `#F26D6D` |
| En attente (en file · bloqué) | `#F59E0B` | `#FBB43C` |
| Au repos (rien ne tourne · neutre) | `#9A9AAE` | `#7A7A90` |

### Neutres — à biais indigo (choisis, pas hérités)

| Rôle | Clair | Sombre |
|---|---|---|
| Encre (texte) | `#191826` | `#ECECF2` |
| Ardoise (texte secondaire) | `#6B6A80` | `#A0A0B4` |
| Filet (bordures · séparateurs) | `#E4E3EE` | `#2C2B40` |
| Toile (fond d'application) | `#F4F4F8` | `#121120` |
| Surface (cartes, panneaux) | `#FFFFFF` | `#1B1A2A` |

**Dégradé d'icône** (fixe dans les deux thèmes — c'est un asset produit) :
`#5A52F0 → #4F46E5 (45 %) → #7C3AED`, diagonale.

---

## 3 · Typographie

**San Francisco** (police système macOS) à dessein — c'est celle dans laquelle l'app tourne ;
l'identité doit être **native**, pas exotique. Aucune police à embarquer.

| Rôle | Pile |
|---|---|
| Interface / titres | `-apple-system, BlinkMacSystemFont, "SF Pro Text", system-ui, sans-serif` |
| Données / valeurs hexa | `ui-monospace, "SF Mono", Menlo, monospace` |

Titres en graisse 680–700, interlettrage serré (`-0.02em` à `-0.03em`). Étiquettes en
capitales, interlettrage `+0.1em à +0.14em`. Valeurs alignées en colonnes : `tabular-nums`.

---

## 4 · Icône d'application

Squircle sur **dégradé indigo → violet**, marque **blanche** centrée. Le dégradé ne suit pas
le thème : l'icône est un asset produit, identique en clair et en sombre.

- Master : **`brand/app-icon.svg`** (1024², carré arrondi 824² centré, marge 100).
- **Production macOS** : reporter le dessin dans le **gabarit superellipse d'Apple** (coins
  continus + ombre portée), puis exporter le jeu `.icns` / PNG. Le `border-radius` du master
  est une approximation ; la superellipse exacte est l'asset final.
- **Plein-cadre** (favicon, web, iOS) : passer le fond à `x=0 y=0 w=1024 h=1024 rx≈230`.

---

## 5 · Icône de barre de menus

macOS attend une **image-gabarit** : une silhouette **monochrome** que le système teinte
(sombre sur barre claire, claire sur barre sombre). La couleur n'a droit qu'à **un seul
endroit** — le point d'état.

| État du daemon | Traitement | Asset |
|---|---|---|
| **Au repos** | Silhouette monochrome, aucune couleur | `brand/tray-idle.svg` |
| **Run en cours** | Le vert remplit le tracé et **tourne** (moitié verte / moitié mono) | `brand/tray-active-animated.svg` (référence) |
| **En file / bloqué** | Point ambre fixe, composé par-dessus le gabarit | — (pastille runtime) |
| **Dernier run échoué** | Pastille rouge jusqu'à consultation | `brand/tray-attention.svg` (aperçu) |

**L'animation « flux ».** Une bande révélatrice de **la moitié du périmètre** balaie la
boucle : devant elle le vert apparaît (nœud, arête, nœud), derrière elle le monochrome
reprend. Toujours 50 % vert qui avance, 50 % qui s'éteint, en rotation.

- Bande : `stroke-dasharray: 71.22 71.22` (moitié pleine / moitié vide du périmètre `142.44`).
- Mouvement : `stroke-dashoffset` animé `0 → -142.44`, linéaire, ≈ **2,8 s** le tour, sens
  `A→B→D→C`. *(Sens et durée réglables d'une ligne.)*
- **Mouvement réduit** respecté : figer à mi-course → une moitié verte statique.

**Livraison.** Avalonia n'anime pas le tray nativement → générer **≈ 16–24 frames** du
mouvement et les **permuter sur un timer** ; timer coupé au repos. La **pastille colorée**
(en file / échec) se **compose au runtime** par-dessus le gabarit — elle ne peut pas vivre
dans une image-gabarit, qui est monochrome.

---

## 6 · Index des fichiers

| Fichier | Contenu |
|---|---|
| `brand/mark.svg` | Marque maîtresse, monochrome (`currentColor`) |
| `brand/app-icon.svg` | Icône d'application 1024² (dégradé + marque blanche) |
| `brand/tray-idle.svg` | Gabarit barre de menus — au repos |
| `brand/tray-active-animated.svg` | Référence de l'animation « flux » (à débiter en frames) |
| `brand/tray-attention.svg` | Aperçu de l'état « attention » (pastille composée au runtime) |
| `brand/generate-pngs.sh` | Rasterise les SVG en `.icns` / PNG / frames (voir en-tête du script) |
| `maquettes/identite-visuelle.html` | Maquette d'origine validée (palette + icônes + animation) |
