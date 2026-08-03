---
name: plan-design
description: Produit le plan de design d'un incrément — schéma-delta, table des objets impactés, maille visée — et gate la première ligne de code derrière sa validation. Use when un incrément entre en Planning, quand un changement va créer ou supprimer une classe, traverser plusieurs modules, ou impliquer une découpe non évidente, ou quand on demande explicitement de planifier ou d'écrire le plan de design d'un incrément. Ne pas l'utiliser pour créer les sous-tâches de pas (c'est `decoupage-pas`, à l'entrée en In Progress).
---

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

# plan-design

Ce skill **gate** : tant que son plan n'est pas écrit, aucun test rouge sur cet incrément n'a de
raison d'exister. `CLAUDE.md` l'exige dès qu'un changement crée ou supprime une classe, traverse
plusieurs modules, ou implique une découpe non évidente.

**L'échelle est celle des objets** (`D-053`) : lesquels naissent, changent ou meurent, et quelles
responsabilités ils portent. Les deux échelles voisines ne sont pas la tienne, et empiéter coûte
dans les deux sens :

- **au-dessus**, le **plan d'architecture** de la spec a tenu le système et le module. Il est
  d'ensemble et **indicatif** : tu as le droit de t'en écarter au contact du réel, à condition de le
  dire — ne le rejoue pas, ne le contredis pas en silence ;
- **en dessous**, le **découpage en pas** tracera le chemin, et la **test list** de chaque pas
  tiendra le code, fichier par fichier. Le premier a lieu à l'entrée en `In Progress`
  (`decoupage-pas`), la seconde à la prise du pas — jamais ici, ni l'un ni l'autre.

## 1. Décider si l'étape a lieu

`Planning` est **conditionnel** (`tickets.md` §6.2). Relis l'incrément en `Todo` contre les trois
critères ci-dessus.

- Aucun ne s'applique → écris-le en une phrase dans l'incrément (« pas de plan, changement local à
  une classe ») pour que le lecteur suivant sache que ce n'est pas un oubli, **pose `Done` et
  arrête-toi**. Ne saute pas la carte en `In Progress` toi-même : c'est `decoupage-pas` qui l'y
  tire, pour y découper les pas (`cycle-increment.md` §4).
- Au moins un s'applique → continue.

**Fait quand** : la décision est écrite quelque part, jamais silencieuse.

## 2. Choisir où vit le plan

- L'incrément est **porté par une carte** (cas nominal) → le plan est le **document attaché**,
  écrit maintenant, en `Planning`. Linear rend le mermaid nativement.
- L'incrément **n'est porté par aucune carte** → le plan est un fichier, et sa **toute première
  ligne**, avant le titre, est `> Fichier : <chemin absolu>` — le schéma n'existe que dans
  l'aperçu de ce fichier.

**Fait quand** : le plan existe à l'endroit que ce choix désigne, pas ailleurs.

## 3. Le schéma-delta, en tête du plan

Un bloc `mermaid`, jamais rendu dans un terminal. La convention — couleurs, anatomie d'un nœud,
la ligne `+ <incrément>` sur un bloc modifié — vit dans `docs/design/schemas.md` §0 et §6 :
**va la lire avant de dessiner**, ne la recopie pas ici.

Le schéma se lit sur le vocabulaire de `schemas.md` §3 (déclaré vs produit) : dis, pour chaque
bloc touché, s'il déplace la définition ou l'exécution.

**Fait quand** : la table « Objets impactés » a son équivalent visuel — chaque bloc coloré
(ajouté/modifié/supprimé), chaque bloc modifié porte sa ligne `+`, aucun bloc n'est ambigu sur son
registre.

## 4. Dire la maille visée — sans créer les pas

⚠️ **Tu ne découpes pas en pas ici, et tu ne crées aucune sous-tâche.** Le découpage effectif a lieu
à l'entrée en `In Progress`, porté par `decoupage-pas`, pour la raison qui interdit déjà d'écrire les
test lists d'avance : ce qu'on apprend au pas 1 change ce qu'on sait au pas 4 (`D-070`).

Ce que ce plan doit dire, c'est ce que **seule la conception sait** et que le découpage ne
retrouverait pas :

- **la maille visée** — combien de pas, en ordre de grandeur, et pourquoi cette taille-là. L'unité
  opposable : un pas tient dans **une fenêtre de contexte fraîche**. Vérifie aussi le test de
  `tickets.md` §1 à l'envers — si un pas était recettable seul par quelqu'un qui ne lit pas le code,
  ce n'est pas un pas, c'est un incrément mal découpé ;
- **les frontières que la conception rend évidentes** — celles qui tombent des objets eux-mêmes.
  *« La descente du socle ne se mêle à rien d'autre »* est une frontière de conception ; *« puis on
  câble l'interrupteur »* est un ordre d'exécution, et il ne t'appartient pas ;
- **l'ordre contraint, là où il l'est** — quand un objet doit exister avant qu'un autre puisse être
  monté. Ailleurs, dis que l'ordre est indifférent plutôt que d'en inventer un.

**Les pièges restent ici, accrochés à leur objet.** Un piège local — une connexion non thread-safe,
un arrêt qu'il faut attendre, un chemin qu'il ne faut pas recomposer à la main — est une propriété
de l'**objet**, pas du pas qui le touche. Le renvoyer à un pas qui n'existe pas encore, c'est le
perdre.

**Ce qui le préserve est le nom de l'objet, pas l'endroit où tu l'écris.** Dans la cellule de la
table « Objets impactés » quand il tient en une ligne ; dans une section « objet par objet » quand
il y en a plusieurs ou qu'ils demandent un paragraphe — une table à quatre colonnes cesse d'être
lisible passé quelques pièges, et un lecteur saute alors la colonne entière. Les deux formes
satisfont la DoD, qui ne coche que le nom de l'objet.

**Fait quand** : un lecteur sait en quel ordre de grandeur de pas cet incrément se fait, quelles
frontières tombent de la conception, et aucun piège connu ne dépend d'un pas pour survivre.

## 5. Une découpe non évidente ne se tranche pas seul

Si le plan hésite entre plusieurs façons radicalement différentes de couper les responsabilités —
la **forme** des objets eux-mêmes — ne choisis pas seul.
Lis [`CONCEVOIR-DEUX-FOIS.md`](CONCEVOIR-DEUX-FOIS.md) et lance-le : c'est le cas qui le mérite.
Le cas courant (frontières déjà lisibles) n'y va pas.

## 6. Terminer l'étape

Écris le plan (schéma-delta + table « Objets impactés » + maille visée), puis **passe-le contre
`docs/methode/dod/story/plan-review.md`** — c'est le référentiel que `revue-plan` appliquera, clause
par clause, et il n'existe aucune raison de le découvrir après coup. **Ne jamais recopier ses cases
ici** : une copie d'un référentiel diverge de lui en silence (journal 54). Le faire sur le plan
**fini**, pas en le rédigeant — viser une grille en écrivant produit un plan qui coche.

**Pose ensuite `Done` sur la carte, et arrête-toi là.** ⚠️ **Ne déplace pas la carte en
`Plan Review`** : `Done` n'avance pas une carte, elle **autorise** qu'on l'avance, et c'est
`revue-plan` qui la tire à sa prise, en retirant l'étiquette (`cycle.md` §4). Un ticket n'est
jamais poussé. Le plan reste donc en `Planning`, portant `Done`, jusqu'à ce que la revue vienne le
chercher — et ce skill ne juge pas son propre plan.

**Fait quand** : le plan est complet, il a été passé contre la DoD, l'étiquette dit `Done` — et la
colonne dit toujours `Planning`.
