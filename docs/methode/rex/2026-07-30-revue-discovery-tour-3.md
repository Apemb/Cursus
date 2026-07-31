# 2026-07-30 — `revue-discovery`, tour 3 : la boucle se ferme, et découvre son angle mort

> Troisième et dernier tour sur *Un agent pilote Cursus*, après
> [tour 1](2026-07-30-revue-discovery-tour-1.md) et [tour 2](2026-07-30-revue-discovery-tour-2.md).
> C'est le tour qui rend **`Done`** — la seule boucle du dispositif qui soit allée jusqu'au bout.
>
> ⚠️ **Fiche écrite après coup, le 2026-07-31**, la séance ayant été close sans qu'elle le soit. Les
> rubriques qui reposent sur la trace d'exécution sont **incomplètes et le disent** : ce qui suit
> vient de la mémoire de séance et de ce que Linear porte encore, pas d'un journal rejouable. Une
> fiche partielle vaut mieux qu'un tour non consigné, mais elle se compare moins bien que les deux
> autres — c'est le prix de l'avoir écrite en retard, et c'est la leçon la plus transférable de
> cette fiche-ci.

## 1. Ce qui a tourné

`revue-discovery` sur le document `Discovery — Un agent pilote Cursus`, carte en colonne `Discovery`
+ `Review Requested`, après la seconde reprise du binôme.

**La commande n'a pas été conservée.** Les deux tours précédents la portent verbatim ; celle-ci
manque, et rien ne permet d'affirmer qu'elle était identique. C'est exactement le trou que le
`README.md` du dossier annonce comme fatal à la comparaison — *« deux tours ne se comparent que si
l'on peut établir qu'ils ont été lancés pareil »*. Il est donc probable, mais non établi, que ce
tour soit comparable aux deux autres.

**Où est la trace qu'il a servi** : les réponses déposées dans les fils Linear de la carte, et
l'étiquette `Done` posée sur le projet — dont la conséquence est visible aujourd'hui encore,
puisque la carte a ensuite été **tirée vers `Spec`**, où elle vit depuis.

## 2. Chiffres

| Mesure | Tour 1 | Tour 2 | Tour 3 |
|---|---|---|---|
| Durée | 8 min 06 s | 9 min 33 s | **6 min 27 s** |
| Coût | 2,99 $ | 3,54 $ | **2,71 $** |
| Remarques posées | 7 — 4 dures, 3 jugements | 6 — 1 dure, 5 jugements | **0** |
| Verdict | `Rework Needed` | `Rework Needed` | **`Done`** |
| Tours d'outils | 21 | 31 | *non conservé* |
| Tokens de sortie | 30 452 | 32 417 | *non conservé* |
| Sous-agents | 3, parallèles | 3, parallèles | *non conservé* |
| Permissions refusées | 0 | 6 | *non conservé* |

**Le fait saillant tient en une ligne : le tour le plus rapide et le moins cher est celui qui a le
plus vérifié.** Il ne s'est pas contenté de relire l'artefact — il a contrôlé les engagements de
reprise **ligne à ligne** avant de conclure qu'aucun solde n'était de complaisance. C'est le
contraire de ce qu'on redoute d'un verdict `Done` bon marché.

⚠️ Le décompte « onze engagements » vient de la mémoire de séance. Aujourd'hui la carte porte
**36 commentaires, `open` à 0** — racines et réponses confondues, la commande ne les sépare pas
rétrospectivement.

## 3. Conformité au protocole

| Clause | Tenue | Ce qui l'atteste |
|---|---|---|
| Session neuve (`D-039`) | **oui** | Session distincte de la reprise, comme aux deux tours précédents |
| Trois axes, jamais fondus (`revue-discovery`) | *non vérifiable* | La trace des sous-agents n'a pas été conservée |
| Étiqueter la confiance (`revue` §5) | **sans objet** | Aucun constat posé |
| Lister sans réécrire (`revue` §6) | **oui** | Document inchangé |
| Poser l'étiquette, ne jamais déplacer (`revue` §8) | **oui** | `Done` posé, colonne `Discovery` inchangée ; c'est l'humain qui a tiré ensuite |
| Vérifier les soldes plutôt que les croire | **oui, au-delà du prescrit** | Contrôle ligne à ligne des engagements — aucune clause ne le demandait |

## 4. Qualité de la sortie

**Zéro remarque est le résultat le plus difficile à juger**, et il faut le dire plutôt que de le
célébrer : un accord unanime ressemble à s'y méprendre à une revue qui ne cherche pas.

Le seul contre-indice sérieux est le travail de vérification lui-même — contrôler les engagements un
par un est un effort que « ne pas chercher » ne produit pas. Il est réel, mais il reste un indice.

**Et les trois tours ont porté sur le même artefact**, écrit par le binôme qui éprouvait le skill,
avec un skill de la même main. Le tour utile suivant n'était pas un quatrième passage ici : c'était
un premier passage sur une discovery que le binôme n'aurait pas rédigée. Il n'a pas eu lieu.

### Ce que ce tour a découvert et qu'aucun autre n'aurait pu

**Le dispositif est aveugle à la sur-correction, et le trou est dans le référentiel.** La clause
*« ce qu'on en sait **factuellement** »* vit dans le skill [`discovery`](../../../.claude/skills/discovery/SKILL.md)
§3, alors que les trois axes de `revue-discovery` sont adossés à la **DoD** (§1, §2, §5). **Aucun axe
ne porte cette clause** — donc aucune revue ne peut détecter une section 3 vidée de sa substance.

Or c'est précisément ce que les reprises ont fait : les cinq pistes ont été réduites à des
définitions nues pour satisfaire l'axe *« aucun arbitrage n'a été rendu »*, et les trois tours ont
validé sans pouvoir voir le risque inverse. **Une revue qui ne sait chicaner que dans un sens
produit mécaniquement la dérive opposée**, et elle la certifie.

C'est le constat le plus lourd des trois fiches, et il ne met en cause ni le relecteur ni le skill :
il met en cause la DoD.

## 5. Frictions

Journal des frictions, entrées **38** (un verdict `Done` avale ses jugements) et **39** (la clause
qui vit dans le skill au lieu de la DoD n'est opposable par personne). L'entrée **31**, écrite au
tour 2, vaut aussi pour celui-ci : le dossier `rex/` n'a pas été ouvert par le relecteur.

⚠️ Ces deux entrées ont été écrites le 2026-07-31, avec les numéros disponibles à ce moment-là — les
numéros 32 à 34, qui leur étaient destinés, avaient été pris entre-temps par une autre séance. Une
friction consignée en retard perd sa place dans l'ordre, pas son contenu.

## 6. Ce que le tour a changé

- **La première boucle complète du dispositif** : ① → ② → ① → ② → ① → ② → ⑥, et la carte tirée.
  `cycle.md` §8 l'a affirmée impossible pendant un jour de plus, faute de cette fiche.
- **Rien dans les skills, rien dans les documents** — et c'est ce qui rend la découverte du §4
  coûteuse : elle est restée dans une mémoire de séance jusqu'au lendemain.
- **Quatre observations non bloquantes ont été énoncées et perdues.** Les déposer aurait rouvert
  `open` et interdit le `Done` ; le geste *« poser une observation sans rouvrir la porte »* n'existe
  ni dans la CLI ni dans le cycle (journal 38).

## 7. Verdict pour `revue-discovery`

**Promu — confirmé par le tour qui referme la boucle.**

Trois tours, treize remarques, un verdict de sortie qui vérifie au lieu de croire. Le skill fait ce
qu'il annonce, et le fait sans être nommé (découverte par description, établie au tour 2).

**La réserve n'est pas sur lui.** Son axe le plus subtil — *aucun arbitrage n'a été rendu* — est
adossé à une DoD qui n'a pas de clause symétrique, et le skill ne peut pas mieux juger que son
référentiel. Le remède appartient à [`dod/feature/discovery.md`](../dod/feature/discovery.md) : tant
que la matière factuelle n'y est pas exigée, un artefact peut passer les trois axes en ayant perdu
ce qui faisait sa valeur.
