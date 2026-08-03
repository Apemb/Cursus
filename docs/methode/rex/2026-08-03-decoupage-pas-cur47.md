# Premier tour de `decoupage-pas` — l'incrément `CUR-47` en huit pas

> **Le skill n'avait jamais tourné.** Écrit le 2026-08-02 avec `D-070`, qui lui a confié le
> découpage en pas que `plan-design` portait jusque-là. Cette fiche est son premier tour réel.
>
> **Heures en heure locale du dépôt**, celle des commits et des horodatages Linear.

## 1. Ce qui a tourné

**Le skill `decoupage-pas`**, invoqué **en session**, pas en sous-agent. La commande, verbatim :

```
Skill(skill: "decoupage-pas",
      args: "CUR-47 — La porte s'ouvre : le serveur MCP monte, et l'agent liste les projets")
```

Répertoire de départ : la racine du dépôt. **Session non neuve** — c'est la même session qui venait
de faire vérifier la boucle de revue du plan par un sous-agent, et le régime le veut : ordonner
n'est pas relire.

**Trace que le skill a servi**, étape par étape et observable dans Linear :

| Heure | Geste | L'étape qui le prescrit |
|---|---|---|
| 13:48:36 | `CUR-47` passe de `Plan Review` à `In Progress`, étiquette `Done` retirée dans le **même** appel | §0 — tirer, un seul geste |
| 13:50:46 → 13:52:26 | Huit sous-tâches créées, toutes en `Todo`, `blockedBy` posé à la création | §3, §7 |
| 13:52:41 | La carte d'incrément est amendée : écart de maille, point d'arrêt en deux temps, répartition des mises à jour de documentation | §1 — « le dire » |

Deux minutes dix séparent la prise de la première carte : c'est le temps de la lecture du plan, du
découpage et de la relecture d'ensemble. Aucune carte n'existait avant cette relecture, comme §5
l'exige.

**Ce que le skill n'a pas eu à faire** : §6 (reposer en `Planning` quand le découpage bute sur une
décision de structure) ne s'est pas déclenché — le plan avait tranché tout ce dont le découpage
avait besoin.

## 2. Chiffres

| Grandeur | Valeur |
|---|---|
| Durée de bout en bout | **4 min 05** (13:48:36 → 13:52:41) |
| Dont lecture du plan, découpage et relecture d'ensemble | **2 min 10** avant la première carte |
| Pas produits | **8** |
| Maille annoncée par le plan de design | **5 à 6** — écart de **+ 33 à 60 %** |
| Arêtes de blocage | **9** |
| Pas prenables immédiatement (sans blocage) | **2** — indépendants l'un de l'autre |
| Pas au bout de la chaîne la plus longue | **4** (socle → racine → … → interrupteur) |
| Sous-agents | **0** |
| Appels d'outil | **12**, dont 9 écritures Linear et 3 vérifications de relecture |
| Cartes rendues bloquées par un pas qui n'existait pas encore | **0** |

**Répartition des pas par nature** : 3 de socle (`CUR-65`, `CUR-66`, `CUR-67`), 4 de seconde porte
(`CUR-68` à `CUR-71`), 1 de présentation (`CUR-72`).

## 3. Conformité au protocole

Clause par clause, avec ce qui l'atteste.

| Étape | Tenue | Pièce |
|---|---|---|
| **§0** Tirer, retirer l'étiquette, un seul geste | **oui** | Un unique appel pose `In Progress` et vide les étiquettes ; l'incrément n'a jamais porté `Done` en `In Progress` |
| **§1** Reprendre la maille sans s'y croire tenu, et **dire** l'écart | **oui** | Écart de 5–6 → 8 écrit sur la carte d'incrément avec son motif (trois acceptations de natures différentes) |
| **§2** Dimensionner sur une fenêtre fraîche | **oui** | Chaque pas dit « où il s'arrête » ; aucun ne suppose de deviner ce qu'un précédent a fait |
| **§3** `blockedBy` explicite, naissance en `Todo` | **oui** | Vérifié après coup sur `CUR-70` et `CUR-72`, les deux seuls multi-bloqués ; les huit sont en `Todo`, aucun en `Backlog` |
| **§4** Les trois questions, **pas frère nommé**, aucune test list | **oui** | Chaque carte nomme au moins un frère par son titre ; aucune ne porte de cas de test |
| **§5** Relire l'**ensemble** avant de créer quoi que ce soit | **oui, et elle a payé** | Voir §4 de cette fiche — un geste orphelin trouvé, un geste dédoublé désamorcé |
| **§6** Reposer plutôt que décider | **sans objet** | Aucune décision de structure ne manquait |
| **§7** Créer les sous-tâches, ne toucher à rien d'autre | **oui** | L'incrément est exactement dans l'état où §0 l'a mis : `In Progress`, sans étiquette |
| **§8** Ne pas concevoir, ne pas relire le plan | **oui** | Aucun objet, aucune responsabilité, aucune frontière de couche n'a été décidée ici |

⚠️ **Une clause est tenue sans être éprouvée** : §2 affirme qu'un pas tient dans une fenêtre de
contexte fraîche. Rien ne le **vérifie** avant que le premier pas ne soit pris. Le dimensionnement
de ce tour est une prédiction, et c'est `prendre-un-pas` qui la mesurera.

## 4. Qualité de la sortie

**Jugée par personne, et c'est le constat le plus important de cette fiche.** Un découpage en pas
n'a aucune colonne de revue en aval — le skill l'écrit lui-même : sa §5 « tient lieu de la porte
humaine » que `decoupage` §6 impose une échelle plus haut. La seule évaluation de ce découpage est
celle que son auteur a faite de son propre travail.

**Ce que la relecture d'ensemble a trouvé, et que nul relecteur d'une pièce n'aurait pu voir :**

- **Un geste que personne ne portait.** Le plan de design contenait une section « Ce que cet
  incrément met à jour dans la documentation », nommant quatre gestes — deux sections
  d'`architecture.md`, une troisième en trois volets, et une entrée `decisions.md`. Cette section
  existait précisément parce qu'une remarque de revue l'avait exigée. **Aucun des huit pas ne la
  portait** : chacun décrivait du code. Les quatre gestes ont été répartis sur `CUR-65`, `CUR-66`,
  `CUR-67` et `CUR-71`.
- **Un geste que deux pas auraient dédoublé.** Le point d'arrêt de l'application naît dans
  `CUR-67` avec un seul geste (disposer la racine) et en reçoit un second dans `CUR-72` (arrêter
  l'interrupteur). Sans mention explicite dans **les deux** cartes, chacun aurait supposé que
  l'autre le porte — ou l'aurait recréé.
- **Une acceptation dont le montage dépend d'un pas non bloquant.** Le test de `CUR-70`
  (`list_projects`) doit porter le jeton si `CUR-69` (l'admission) est déjà fait, et pas sinon. Les
  deux ne se bloquent pas — sur-bloquer les aurait sérialisés sans raison —, donc la carte dit de
  **vérifier l'état plutôt que le supposer**.

**Ce qui n'a pas été évalué** : la justesse du dimensionnement, la pertinence des pièges recopiés
depuis le plan, et le fait que la chaîne critique compte quatre pas là où deux branches auraient
pu être plus courtes. Rien de tout cela n'a de juge avant le premier pas pris.

## 5. Frictions

Journal des frictions, entrées **89** à **91**, sous l'en-tête *premier tour de `decoupage-pas`
(`CUR-47`)*. **Non recopiées ici.**

- **89** — la précaution écrite en amont ne se transmet pas toute seule à l'aval ;
- **90** — l'étape 5 n'a aucun dispositif, et c'est devenu une incohérence du corpus le jour même ;
- **91** — la maille visée était basse de 40 %, et l'écart n'était pas une erreur (point de mesure,
  pas règle).

## 6. Ce que le tour a changé

**Dans le backlog** : huit sous-tâches, `CUR-65` à `CUR-72`, toutes en `Todo`, neuf arêtes de
blocage. `CUR-47` est en `In Progress`, sans étiquette — état où `decoupage-pas` §7 exige qu'il
reste tant que ses pas courent. Sa description gagne trois paragraphes : l'écart de maille et son
motif, le point d'arrêt en deux temps, et la répartition des quatre mises à jour de documentation.

**Dans le journal** : trois entrées, **89 à 91**.

**Dans les skills** : **rien**, et c'est délibéré. Le tour a produit une question — faut-il donner
un dispositif à l'axe d'ensemble de §5 ? — et non une réponse ; `D-039` veut que le journal écrive
les skills, pas qu'un tour les réécrive dans sa foulée.

**Dans les référentiels** : rien. `dod/story/` ne porte aucune DoD de découpage en pas, puisqu'il
n'y a pas de colonne de revue à ce niveau.

## 7. Verdict pour le skill éprouvé

### `decoupage-pas` — **promu, avec une réserve nommée**

Il **sort de l'état `draft`**. Neuf clauses sur neuf sont tenues, huit avec pièce et une sans objet ;
le geste de prise de §0 s'est fait en un appel, ce qui est exactement la moitié aval de `D-069`
qu'un skill antérieur avait manquée ; et §5 — la clause la plus coûteuse à respecter, parce qu'elle
oblige à tout relire avant de créer la première carte — a **rattrapé un geste orphelin dès son
premier tour**. Un skill dont la clause la plus exigeante paie immédiatement n'est pas un draft.

**La réserve, et elle est structurelle** : l'axe d'ensemble de §5 est fait **par l'auteur du
découpage**, en session, sur son propre travail. C'est le seul axe d'ensemble du dépôt dans ce cas,
et il se trouve au seul endroit du flux où **personne ne relit en aval**. Le jour même, `D-073`
établissait l'inverse pour `discovery` et `spec` — un sous-agent par axe, en session neuve, avec un
agrégateur distinct du binôme — au motif mesuré qu'une auto-évaluation « ne voit pas ce que son
auteur ne peut pas voir ».

Ce tour s'en est bien sorti. **Un succès ne mesure pas un dispositif** : rien ici ne dit ce qu'un
relecteur indépendant aurait trouvé en plus, et l'essai de `D-073` a précisément montré qu'un
relecteur unique rate ce que trois axes trouvent. La question est ouverte et elle est écrite au
journal (89, 90) : généraliser `D-073` à cette étape, ou écrire pourquoi ce cas s'en dispense.

⚠️ **Ce que ce verdict ne couvre pas** : la justesse du découpage lui-même. Un découpage ne se juge
qu'à l'usage — quand un pas déborde de sa fenêtre, quand une arête de blocage manque, quand deux
pas se marchent dessus. `prendre-un-pas` rendra ce verdict-là, et il faudra revenir ici pour le
noter.
