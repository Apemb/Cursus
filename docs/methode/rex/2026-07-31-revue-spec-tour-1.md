# 2026-07-31 — `revue-spec`, première exécution réelle

> Premier tour de l'étape `Spec` du cycle feature, sur *Un agent pilote Cursus*. Le skill n'avait
> jamais tourné : `cycle-feature.md` §8 le rangeait en *tranché mais pas construit*, comme les
> temps ③ et ④ qu'il appelle.
>
> ⚠️ **La DoD a changé le jour même, après le tour.** La revue a jugé contre **9 cases** de
> `dod/feature/spec.md` §1 ; `D-049` en a ajouté **trois** (le plan d'implémentation) dans la
> soirée. La fiche suivante ne sera donc pas comparable case à case avec celle-ci.

## 1. Ce qui a tourné

`revue-spec` sur le document `Spec — Un agent pilote Cursus`, attaché au projet Linear du même nom,
alors en colonne `Spec` + `Review Requested`.

**Le chemin d'exécution**, et où est la trace qu'il a servi : un sous-agent lancé en arrière-plan
depuis la session du binôme, qui a **invoqué le skill**, lequel a passé le mandat au primitif
`revue` avec deux axes ouverts **en sous-agents séparés**. Traces vérifiables : onze commentaires
sur la **carte** (pas sur le document, `D-045`), chacun portant son repère calculé et son étiquette
de confiance ; et l'étiquette `Rework Needed` posée sur le projet, la colonne inchangée.

**La commande, verbatim et rejouable** — depuis la racine du dépôt, agent `general-purpose`, en
arrière-plan :

```
Tu prends une revue de spec sur le backlog Linear du projet Cursus (espace `cursus-app`, équipe `CUR`).

La carte à relire : le **projet** Linear « Un agent pilote Cursus », actuellement en colonne `Spec`
et portant l'étiquette `Review Requested`. Son document `Spec` y est attaché.

Invoque le skill `revue-spec` et suis son protocole jusqu'au bout.

Deux contraintes de méthode, non négociables :

- Tu n'as **pas** participé à l'écriture de cette spec, et tu ne dois pas chercher à reconstituer
  comment elle a été produite ni quelle intention l'a guidée. Relis l'**artefact seul**, contre son
  référentiel — c'est la condition qui donne sa valeur à la relecture (`D-039`). Connaître
  l'intention de l'auteur t'ancrerait sur elle.
- Un skill **ne déplace jamais la carte** : tu poses l'étiquette qui convient et tu t'arrêtes là.

Le dépôt est à la racine du projet. Le référentiel de conformité est `docs/methode/dod/feature/spec.md`.

En retour, rends-moi : le nombre de remarques posées, leur axe, l'étiquette que tu as posée, et —
en une phrase chacune — les remarques les plus lourdes.
```

⚠️ **Ce prompt porte une redondance qui fausserait une comparaison** : les deux contraintes y sont
rappelées à la main alors qu'elles sont **déjà dans les skills** (`revue-spec` §1 pour la session
neuve, `revue` §8 pour ne pas déplacer la carte). On ne peut donc pas conclure de ce tour que les
skills les portent seuls. **Le prochain tour doit les retirer du prompt** — c'est la seule façon de
mesurer ce que le skill tient sans béquille.

## 2. Chiffres

| | |
|---|---|
| Durée de travail réelle | **727 s** (~12 min) — voir journal 34, la seconde notification en annonçait 3 807 |
| Jetons du sous-agent | **~135 000** |
| Appels d'outils | **36** |
| Sous-agents ouverts | **2** (un par axe), lancés par le primitif `revue` |
| Remarques posées | **11** — 3 Conformité (dont **1 violation dure**), 8 Découpabilité |
| Constats écartés hors mandat | **3**, non posés sur la carte |
| Cases de conformité évaluées | **12** — 9 de §1, 3 de §2, dont 2 en aval par construction |
| Cases tenues | **9 sur 10** évaluables |
| Tours nécessaires | **1** pour produire, 1 pour reprendre |
| Remarques retenues à la reprise | **11 sur 11** — **aucun refus motivé** |

## 3. Conformité au protocole

Clause par clause, avec ce qui l'atteste.

| Clause | Tenue | Ce qui l'atteste |
|---|---|---|
| Session neuve, sur l'artefact seul (`revue-spec` §1, `D-039`) | **oui** | Le fil de rédaction ne lui a jamais été transmis. Il a de lui-même écarté les commentaires Linear du projet — qui portent les deux tours de revue de la Discovery — pour ne pas ancrer les axes |
| Exactement deux axes, jamais fondus (§2) | **oui** | Deux sous-agents séparés, aucun ne voyant le rapport de l'autre ; deux rapports reçus distinctement |
| Deux citations par constat (`revue` §3) | **oui** | Chaque remarque porte le passage cité et son référentiel |
| Écarter la justesse (§3) | **oui** | 3 constats rangés sous « hors mandat — justesse », non posés sur la carte |
| Étiqueter la confiance (`revue` §5) | **oui** | Chaque remarque porte *violation dure* ou *jugement* |
| Lister sans réécrire (`revue` §6) | **oui** | Aucune modification du document |
| Poser l'étiquette, ne jamais déplacer (§4, `revue` §8) | **oui** | `Rework Needed` posé, colonne `Spec` inchangée |
| Poser la remarque sur la carte, pas le document (`D-045`) | **oui, mais** | Fait — **et le skill ne le prescrit nulle part**. Voir journal 33 |

**La réserve est la seule qui compte** : ni `revue-spec` ni `revue` §8 ne disent **comment** poser
une remarque. Le relecteur a trouvé `cursus linear comment add` seul, en lisant `cycle-feature.md`.
Le protocole a donc été tenu **par exploration, pas par conception**.

## 4. Qualité de la sortie

Jugée par le binôme auteur à la reprise, contre le document — et le chiffre est net : **11 remarques
sur 11 retenues, aucun refus motivé.** Pour un premier tour, sur un artefact que son auteur croyait
complet, c'est le résultat le plus fort qu'une fiche ait porté jusqu'ici.

Trois constats ont touché des défauts que **ni l'agent auteur ni l'humain n'avaient vus** :

- **La violation dure était réelle et coûteuse** — une clause de recette (« un appel sans jeton
  valide est refusé ») engageait un mécanisme absent des trois registres. Le motif qui la rend grave
  est celui que la DoD vise : `Validation` l'aurait rendue opposable sans que rien en amont dise
  contre quoi.
- **Le relecteur est allé au-delà du document.** Sur le pré-requis déclaré inexistant, il a vérifié
  **dans le code** que `ProjectHost` est `IDisposable` et qu'un seul host vit à la fois, puis l'a
  confronté à `architecture.md` §7.13. Rien dans le mandat ne l'y obligeait.
- **Il a trouvé une incohérence entre deux sections** que la rédaction avait rendue invisible : la
  recette comptait les gestes *de la fenêtre*, le socle ceux *du noyau*. Vérification faite à la
  reprise, il avait raison dans les deux sens — `WorkflowDraft.RenameStep` existe au noyau sans
  qu'aucune surface de la fenêtre l'expose.

**Ce qu'on ne peut pas conclure de ce tour** : que le skill tient seul. Le prompt rappelait deux de
ses clauses (voir §1), et la comparabilité en souffre.

## 5. Frictions

Journal des frictions, entrées **32** (les temps ③ et ④ prescrits sans skill), **33** (le geste
central absent, et pourquoi c'est pire qu'un geste mort), **34** (la durée trompeuse d'un
sous-agent).

## 6. Ce que le tour a changé

Beaucoup, mais **il faut séparer ce que la revue a causé de ce qu'elle a seulement prouvé.**

- **`D-049` — la spec devient fonctionnelle *et* technique**, avec une 8ᵉ question et son plan
  d'implémentation. ⚠️ **Le grief vient de l'utilisateur, pas de la revue** : il a relevé qu'entre
  la spec et le premier plan d'archi, rien ne conçoit ni ne fait valider la structure d'ensemble. Ce
  que la revue a apporté est la **preuve empirique**, le jour même — sa remarque sur les N
  `ProjectHost` ne naît d'aucun incrément, mais de leur conjonction.
- **Sept fichiers de méthode amendés** : `tickets.md`, `dod/feature/spec.md`, `cycle-feature.md`,
  les skills `spec` et `revue-spec`, `CLAUDE.md`, `decisions.md`.
- **`revue-spec` compte désormais douze cases de §1 au lieu de neuf.**
- **La spec elle-même** : six arbitrages rendus au second tour, une annexe inventoriant les gestes
  de la fenêtre — devenue le référentiel opposable de la parité —, un troisième registre
  (*tranché hors périmètre*) qui sépare ce qui n'est pas décidé de ce qu'on a décidé de ne pas poser.

## 7. Verdict pour `revue-spec`

**Promu, sous une réserve nommée.**

Le skill a produit onze remarques toutes retenues, tenu chaque clause de son protocole, et trouvé
trois défauts que le binôme ne voyait pas. C'est le critère de `D-043`, et il est atteint largement.

La réserve, qui n'annule pas la promotion mais doit être levée avant le prochain tour : **son geste
central est absent de son texte** (journal 33). Il a fonctionné parce que le relecteur a exploré le
dépôt, ce qui n'est pas une propriété du skill mais une chance. Deux choses à faire avant de le
relancer — y écrire le geste `cursus linear comment add`, et **retirer du prompt les deux clauses
qu'il porte déjà**, pour mesurer ce qu'il tient sans béquille.

**Ce que ce tour n'établit pas** : les temps ③ et ④ n'ont pas été joués par un skill. `correction` a
été jouée à la main par le binôme, `verification` n'a pas eu lieu du tout au moment d'écrire cette
fiche. Le cycle complet de `Spec` reste donc **non éprouvé au-delà de son temps ②**.
