# 2026-08-01 — `revue-spec`, cinquième exécution

> Cinquième tour sur le même artefact, *Un agent pilote Cursus*, le lendemain des quatre premiers.
> Une différence domine, et c'est elle que le tour mesure : **le référentiel a changé**. `D-054` a
> fait passer `docs/methode/dod/feature/spec.md` §1 de **douze à dix-sept cases**, et l'artefact a
> été **refondu puis restructuré sur le gabarit** que cette décision institue. Les deux skills, eux,
> sont inchangés depuis le tour 4 — leur dernier amendement lui est antérieur.
>
> ⚠️ **Comparabilité, et c'est le fait de ce tour.** Les lignes « cases de §1 » **ne se comparent pas
> colonne par colonne** aux quatre tours précédents : elles ne comptent plus la même chose. Toute
> lecture verticale de ces deux lignes est fausse, et elle n'est pas lissée ici.
>
> ⚠️ **Les réserves du tour 4 s'appliquent telles quelles** : *plus de remarques n'est pas mieux*,
> *autant de remarques ne veut pas dire les mêmes*, et rien ne mesure le calibrage d'un relecteur à
> l'autre sur le partage violations dures / jugements. S'y ajoute que **coût et durée du relecteur**
> ne se mesurent toujours pas de l'intérieur.
>
> ⚠️ **Les rubriques 4 et 7 sont laissées à un tiers**, le relecteur ne jugeant pas sa propre sortie
> (`D-039`). Elles ne portent ici que des **faits bruts**, et pas de jugement.

## 1. Ce qui a tourné

`revue-spec` sur le document `Spec — Un agent pilote Cursus`, attaché au projet Linear du même nom,
en colonne `Spec` + `Review Requested`. La carte portait **68 commentaires, 0 ouvert** — les quatre
tours de spec et les treize remarques de la Discovery, tous soldés.

**Le chemin d'exécution, et où est la trace qu'il a servi** : un sous-agent lancé depuis la session
du binôme, qui a **invoqué le skill** `revue-spec` — l'appel est le premier de la trace —, lequel a
passé le mandat au primitif `revue` avec **deux axes ouverts en sous-agents séparés**, lancés dans le
même message donc en parallèle, aucun ne voyant le rapport de l'autre. Traces vérifiables : douze
commentaires sur la **carte** (jamais sur le document, `D-045`), chacun avec son repère calculé, son
axe et son étiquette de confiance ; l'étiquette `Rework Needed` posée sur le projet et
`Review Requested` retirée ; la colonne `Spec` inchangée ; l'`updatedAt` du document inchangé à
`2026-08-01T14:53:50.333Z`, relu après la pose et antérieur à la première remarque.

**La commande, verbatim et rejouable** — depuis la racine du dépôt :

```
Tu prends une revue de spec sur le backlog Linear du projet Cursus (espace `cursus-app`, équipe `CUR`).

La carte à relire : le **projet** Linear « Un agent pilote Cursus », actuellement en colonne `Spec`
et portant l'étiquette `Review Requested`. Son document `Spec` y est attaché.

Invoque le skill `revue-spec` et suis son protocole jusqu'au bout.

Le dépôt est à la racine du projet. Le référentiel de conformité est `docs/methode/dod/feature/spec.md`.

Un fait d'état, que tu ne peux pas établir depuis l'artefact seul et dont tu as besoin pour instruire
la §2 du référentiel : la carte porte **68 remarques de revue, toutes soldées** (`open: 0`), chacune
avec sa réponse en fil. L'accord de l'humain est structurellement en aval de ton passage.

Puis, **une fois la revue close**, écris sa fiche de retour d'expérience :

- fichier `docs/methode/rex/2026-08-01-revue-spec-tour-5.md` ;
- rubriques **fixes** de `docs/methode/rex/README.md`, dans l'ordre, aucune omise — lis-le avant
  d'écrire ;
- ⚠️ **tu ne juges pas ta propre sortie.** Remplis les rubriques **1, 2, 3, 5 et 6** : ce qui a
  tourné (avec la commande verbatim et rejouable), tes chiffres, ta conformité au protocole clause
  par clause **avec ce qui l'atteste**, les frictions rencontrées, ce que le tour a changé. Pour les
  rubriques **4 (qualité de la sortie)** et **7 (verdict pour le skill éprouvé)**, écris uniquement
  « À compléter par un tiers » suivi des **faits bruts** qu'un tiers utilisera pour les remplir. Un
  relecteur qui se décerne son propre verdict est exactement le piège que ce dossier existe pour
  éviter ;
- **sept fiches existent déjà** dans le même dossier, dont quatre de `revue-spec` (tours 1 à 4) :
  prends-les pour gabarit, et rends tes chiffres **comparables aux leurs**. La fiche du tour 4 porte
  un tableau à colonnes Tour 1 / Tour 2 / Tour 3 / Tour 4 — prolonge-le d'une colonne **Tour 5**, en
  reprenant les lignes existantes. Lis aussi les réserves de comparabilité qu'elle écrit sous son
  tableau : elles s'appliquent à toi ;
- ⚠️ **le référentiel a changé entre le tour 4 et toi, et c'est le fait de comparabilité de ce tour.**
  `docs/methode/dod/feature/spec.md` §1 est passé de **12 à 17 cases** (`D-054` : le document suit un
  gabarit de plan, ses titres sont ceux du gabarit, il ne porte pas de table d'auto-vérification, la
  recette s'écrit en Gherkin en annexe B, les invariants n'accueillent que le non-dérivable, le plan
  d'architecture conçoit à l'échelle du système et du module). Les lignes « cases de §1 » du tableau
  ne se comparent donc **pas** colonne par colonne aux tours précédents — écris-le, ne le lisse pas.
  En revanche `revue-spec` et le primitif `revue` sont **inchangés** depuis le tour 4 : leur dernier
  amendement est antérieur ;
- ⚠️ **deux chiffres te sont inaccessibles de l'intérieur** — ta durée de travail totale et tes
  jetons consommés. Écris exactement `⟨à remplir par la session appelante⟩` dans ces deux cellules,
  sans commentaire, et **relève en revanche toi-même** la durée et les jetons de chacun de tes
  sous-agents d'axe, ainsi que ton nombre d'appels d'outils ;
- ⚠️ **aucun chemin personnel** dans la fiche — ce dépôt est public. Remplacer toute redirection par
  un nom de fichier nu ;
- **ne commite pas.**

En retour, rends-moi : le nombre de remarques posées, leur axe, l'étiquette que tu as posée, et —
en une phrase chacune — les remarques les plus lourdes.
```

**Le prompt garde les trois charges du tour 4** — prolonger un tableau existant, relever soi-même
les chiffres des sous-agents, écrire deux cellules en attente. Il en gagne trois qui appartiennent
au chemin d'exécution autant que les options :

- **le fait d'état est fourni par la session appelante**, et non plus inventé en séance par
  l'orchestrateur. C'est le geste que la friction 50 laissait sans doctrine ; il n'a pas de doctrine
  de plus, mais il a changé de main ;
- **l'appelant inscrit lui-même ce qui a bougé dans le référentiel** depuis le tour précédent, et le
  qualifie de fait de comparabilité. Le tour 4 avait dû le faire compléter après coup ;
- **l'interdiction explicite de lisser une ligne devenue incomparable**.

**Deux différences de dispositif, choisies par l'orchestrateur, qui appartiennent au chemin :**

- **la matérialisation intermédiaire des tours 3 et 4 a été abandonnée.** Le document n'a pas été
  extrait dans un fichier de travail : chaque axe a reçu l'identifiant du document Linear et l'a
  chargé lui-même. Conséquence directe — **les deux axes citent l'artefact réel**, et la précaution
  que le tour 4 avait dû prendre (recharger l'original pour vérifier la fidélité de l'extraction)
  n'a plus d'objet. Le fichier `spec-content.md` du tour 4 n'a pas eu d'équivalent ;
- **les deux axes ont été lancés en synchrone**, dans un seul message. Le relecteur n'a donc payé
  **aucun appel d'attente** — le tour 4 en avait payé douze, et la friction 49 est sans occurrence
  ici.

**Ce qui a bougé dans ce que le relecteur applique**, inscrit par l'appelant :

- **`docs/methode/dod/feature/spec.md` §1 est passé de 12 à 17 cases** (`D-054`). Les cases neuves
  portent le gabarit de plan, ses titres, l'interdiction d'une table d'auto-vérification, le Gherkin
  en annexe B, les invariants réduits au non-dérivable, et l'échelle système/module du plan
  d'architecture ;
- **`revue-spec` et `revue` sont inchangés depuis le tour 4.** Leur dernier amendement — la
  correction d'étiquette et la clause `D-051` — est antérieur. ⚠️ **C'est ce qui produit la friction
  neuve du tour** : `revue-spec` §2 continue d'annoncer « les **douze** cases de §1 » et de nommer
  « plan d'implémentation » ce que `D-053` a renommé.

## 2. Chiffres

| | Tour 1 | Tour 2 | Tour 3 | Tour 4 | **Tour 5** |
|---|---|---|---|---|---|
| Durée de travail du relecteur | 727 s | 619 s | non mesurable de l'intérieur | 1 321 s | **1 223 s** (relevé par la session appelante) |
| Durée des deux axes | non relevée | non relevée | 591 s et 410 s, en parallèle | 524 s et 553 s, en parallèle | **474 s et 371 s**, en parallèle |
| Jetons du relecteur | ~135 000 | 82 447 | non mesurable de l'intérieur | 162 263 | **161 967** (relevé par la session appelante) |
| Jetons des deux axes | non relevés | non relevés | 137 548 + 104 194 = 241 742 | 130 090 + 72 547 = 202 637 | **116 893 + 118 474 = 235 367** |
| Appels d'outils | 36 | 24 | 28 + 37 = 65 | 34 (hors attente) + 35 = 69 ; **12** d'attente en sus | **38** (orchestration, **0 d'attente**, fiche comprise) **+ 19** (axes) = **57** |
| Sous-agents ouverts | 2 | 2 | 2 (un par axe) | 2 (un par axe) | **2** (un par axe) |
| Constats produits par les axes | non relevé | non relevé | 20 | 17 (9 Conf, 8 Déc) | **13** (6 Conformité, 7 Découpabilité) |
| Écartés ou fusionnés avant pose | non relevé | non relevé | 4 — 1 écarté, 1 subsumé, 2 fusions | 1 — une fusion, 0 écarté | **1** — une fusion de doublon inter-axes, **0 écarté** |
| **Remarques posées** | 11 | 12 | 16 | 16 | **12** |
| Répartition | 3 Conf · 8 Déc | 6 Conf · 5 Déc · 1 hors mandat | 11 Conf · 5 Déc · 0 hors mandat | 9 Conf · 7 Déc · 0 hors mandat | **6 Conf · 6 Déc · 0 hors mandat** |
| **Violations dures** | 1 | 2 | 12 (11 Conf, 1 Déc) | 7 (4 Conf, 3 Déc) | **6** (toutes Conformité) |
| Jugements | 10 | 9 | 4 (tous Déc) | 9 (5 Conf, 4 Déc) | **6** (tous Découpabilité) |
| Constats hors mandat — justesse | 3 | 1 | 0 | 0 | **0** |
| **Remarques nées d'une figure** | 0 | 0 | 5 | 2 | **4** — 2 du `flowchart`, 1 du `sequenceDiagram`, 1 de la légende des couleurs |
| Remarques visant une **reprise** du tour précédent | — | 0 | 2 | 4 | **non mesurable ce tour** (voir sous le tableau) |
| Cases de §1 évaluées | 9 | 12 | 12 | 12 | **17** ⚠️ **ne se compare pas** |
| Cases de §1 enfreintes | 1 | non relevé | 6 | 1 au sens de la clause ; 6 cases portant une divergence | **0** au sens de la clause (aucune omission silencieuse) ; **4 cases** portent au moins une divergence — *suit le plan* (×2), *socle nommé*, *plan d'architecture*, *au moins un schéma* ⚠️ **ne se compare pas** |
| Cases de §2 | 3, dont 2 en aval | 3 | 3, toutes non opposables depuis l'artefact seul | 3 — 2 tenues, 1 en aval | **3** — 2 **tenues** sur le fait d'état fourni, 1 (l'accord de l'humain) structurellement en aval |
| Carte avant / après | — | — | 36 → 52, 16 ouverts | 52 → 68, 16 ouverts | **68 commentaires, 0 ouvert → 80, 12 ouverts** |
| Étiquette posée | `Rework Needed` | `Rework Needed` | `Rework Needed` | `Rework Needed` | **`Rework Needed`** |

**Les deux lignes « cases de §1 » sont incomparables verticalement, et c'est le fait de ce tour.**
Neuf, douze, douze, douze, puis dix-sept : la dernière valeur ne mesure pas un relecteur plus
exhaustif, elle mesure un référentiel plus long. Le « 0 case enfreinte » de la colonne 5 ne se lit
pas non plus comme un progrès sur le « 6 » du tour 3 — les cinq cases neuves de `D-054` décrivent le
**gabarit** que l'artefact venait d'adopter, et une spec restructurée sur un gabarit le respecte par
construction. Les cases coûteuses de ce tour n'étaient pas les neuves.

**La ligne des reprises est perdue pour ce tour, et il faut le dire plutôt que d'y mettre zéro.**
L'artefact a été **refondu puis restructuré** entre le tour 4 et celui-ci, hors tour de revue
(journal 51, 52, 53). Les douze remarques visent donc, sans exception, du texte postérieur au tour 4,
et la ligne ne distingue plus rien. Ce qui reste mesurable et vaut d'être noté : **deux des douze
remarques sont directement causées par la restructuration** — le renvoi à un « §5 » que la
renumérotation a supprimé, et le renvoi à une annexe A créée par la relégation des arbitrages, qui
n'arbitre pas les trois choix qu'on lui attribue. Une refonte hors revue produit sa propre récolte,
et elle n'est pas petite : un sixième du tour.

**La baisse de 16 à 12 ne s'interprète pas seule.** Trois causes possibles se superposent sans que
rien ne les départage : un artefact repris quatre fois puis refondu, un référentiel dont les cases
neuves étaient satisfaites d'avance, et un relecteur différent. La série est désormais
**11, 12, 16, 16, 12** — elle ne monte plus, mais elle ne converge pas vers zéro non plus.

**Ce que la ligne des appels d'outils mesure enfin.** Le tour 4 avait dû isoler douze appels
d'attente pure et noter que la ligne ne mesurait plus ce qu'elle prétendait. En lançant les deux axes
en synchrone dans un seul message, ce tour ramène ce coût à **zéro** : les 38 appels sont tous des
appels de travail, l'écriture de la fiche comprise. La comparaison légitime est **34 (tour 4, hors
attente) contre 38 (tour 5)**, et non 46 contre 38.

**Complété par la session appelante — la comparaison que le tour 4 attendait a lieu, et elle ne dit
pas ce qu'on croyait qu'elle dirait.** Le tour 4 avait écrit que « la seule comparaison légitime que
ces deux lignes autorisent est tour 4 contre tour 5, **si le dispositif est reconduit à
l'identique** ». Le prompt l'a été, à la lettre près des trois charges neuves ; **le dispositif
interne, non** — le relecteur a supprimé de lui-même la matérialisation intermédiaire et lancé ses
axes en synchrone. La charge est pourtant restée la même des deux côtés : relire **et** écrire la
fiche. Ce qui se compare, alors :

- **1 321 s → 1 223 s** et **162 263 → 161 967 jetons**. Un écart de 7 % sur la durée, de **0,2 %**
  sur les jetons. Deux relecteurs différents, sur un artefact refondu entre-temps, contre un
  référentiel qui a gagné cinq cases, avec un dispositif interne changé sur deux points — et le coût
  d'orchestration ne bouge pas. C'est le premier indice que la ligne mesure la **forme du protocole**
  et non le contenu de l'artefact ;
- **le total réel du tour est de 397 334 jetons** (161 967 + 235 367), contre 364 900 au tour 4. La
  ligne du tableau reste celle de l'orchestrateur seul : ce sont les **axes** qui ont coûté 16 % de
  plus, pas l'orchestration ;
- **les deux compteurs d'appels d'outils divergent encore, et bien moins** : la session appelante en
  compte **39**, le relecteur **38**. L'écart tombe de 3 (tour 4) à 1. Il n'est toujours pas expliqué,
  et il n'est toujours pas lissé.

## 3. Conformité au protocole

Clause par clause, avec ce qui l'atteste.

| Clause | Tenue | Ce qui l'atteste |
|---|---|---|
| Session neuve, sur l'artefact seul (`revue-spec` §1, `D-039`) | **oui, sous trois réserves nommées** | Le fil de rédaction n'a pas été transmis ; les commentaires de la carte **n'ont été lus par personne** ce tour-ci — le fait d'état de §2 étant fourni par l'appelant, l'orchestrateur n'a pas eu à les ouvrir, ce qui est plus strict que le tour 4. ⚠️ Réserve 1 : la mémoire automatique de la session résume l'artefact par ses conclusions (journal 43), inchangé. ⚠️ Réserve 2, **aggravée** : cette même mémoire annonce qu'**un point de fond est laissé exprès dans l'artefact pour voir si la revue l'attrape** — le relecteur sait donc qu'il est testé, et sur quel registre. ⚠️ Réserve 3 : le fait d'état reste un résumé de l'issue des tours précédents, simplement fourni par l'appelant au lieu d'être choisi par l'orchestrateur. **Aucune de ces trois n'a été transmise aux axes**, qui n'ont reçu que l'artefact, les référentiels et le fait d'état chiffré |
| Exactement deux axes, jamais fondus (`revue-spec` §2, `revue` §2) | **oui** | Deux sous-agents lancés dans le même message, chacun ignorant l'existence de l'autre ; deux rapports reçus distinctement, chacun se clôturant sur son propre verdict d'axe ; aucun rapport de synthèse qui reclasse |
| Les cases de §1 et les trois de §2, clause par clause | **oui, avec un écart du skill au référentiel** | L'axe Conformité rend deux tableaux de couverture — **dix-sept** lignes puis trois — avant sa liste de constats, et nomme pour chaque case où elle se lit dans l'artefact. ⚠️ **Le skill en annonce douze** : c'est le mandat, corrigé par l'appelant, qui a porté les dix-sept, pas `revue-spec` §2 |
| Deux citations par constat (`revue` §3) | **oui** | Chacune des douze remarques porte son référentiel (fichier + clause, ou le second passage de l'artefact quand la contradiction est interne) et l'extrait visé, côte à côte |
| **Confronter chaque figure à la prose** (`revue` §3, `revue-spec` §2) | **oui** | Le mandat des **deux** axes nommait les deux blocs `mermaid` et demandait de les relire nœud par nœud, arête par arête, couleur par couleur, y compris de vérifier **dans les deux sens** l'exhaustivité de la légende « tout ce qui est ambre est nommé ici ». **Quatre des douze remarques en sortent** : l'arête `UI --> QRY` qui tranche une question ouverte, le `sequenceDiagram` qui fait appeler à l'éditeur une commande que sa granularité lui interdit, `CMD --> RACINE` qui vide « Ouvrir un projet » de son motif, et la légende verte qui compte l'activation dans ce qui naît sans lui donner de recette |
| Écarter la justesse (`revue-spec` §3) | **oui, sans matière** | Les deux axes ont rendu leur section *hors mandat — justesse* explicitement vide. Aucune ligne des deux rapports ne discute si la spec est un bon choix |
| Étiqueter la confiance (`revue` §5) | **oui** | 6 *violation dure*, 6 *jugement*, aucune ligne ambiguë. Le partage tombe exactement sur les axes — Conformité n'a produit que des violations dures, Découpabilité que des jugements, ce que la DoD §3 annonce (« Ce sont par nature des **jugements** ») |
| Lister sans réécrire (`revue` §6) | **oui** | `updatedAt` du document relu après la pose : `2026-08-01T14:53:50.333Z`, antérieur à la première remarque. Aucune remarque ne propose un texte de remplacement ; les six de Découpabilité posent **la question que le découpage devrait revenir poser**, elles n'y répondent pas |
| Deux issues, jamais trois (`revue` §6, `D-051`) | **oui** | 13 constats produits, 12 posés, 1 fusionné dans un autre. Aucun constat n'est écrit ailleurs que sur la carte ; aucune « observation non bloquante ». Les deux axes ont reçu la clause dans leur mandat, avec le test *est-ce que quelqu'un doit répondre ?* |
| Poser la remarque sur la carte (`revue` §6, `D-045`) | **oui, sans rappel** | 12 remarques posées par `cursus linear comment add`, ancrées avec leur repère calculé ; `open` passe de 0 à 12, `total` de 68 à 80. Aucune sur le document |
| Poser l'étiquette, jamais déplacer (`revue-spec` §4, `revue` §8) | **oui** | `Rework Needed` seul sur le projet, `Review Requested` retiré ; `status` reste `Spec` |
| Pas d'escalade, pas de compteur de tours (`revue-spec` §4) | **oui** | Aucune assignation, aucune tentative de convergence au-delà de ce passage, alors même que c'est le cinquième |

**Deux clauses n'ont pas pu être tenues telles qu'écrites.**

- **`revue-spec` §2 prescrit d'instruire « les douze cases de §1 », et il y en a dix-sept.** Le skill
  fige un décompte que son référentiel a changé, et ajoute deux erreurs de vocabulaire : « les trois
  dernières de §1 portent le **plan d'implémentation** » — elles sont désormais **quatre**, et
  `D-053` a renommé ce plan *plan d'architecture*. Un relecteur qui suivrait le skill à la lettre
  s'arrêterait à douze cases et manquerait les cinq neuves. **Ici, c'est le mandat de l'appelant qui
  a corrigé**, pas le protocole. C'est le motif des entrées 41, 42 et 48 à sa quatrième occurrence,
  dans l'autre sens : ce n'est plus une correction qui ne redescend pas au primitif, c'est **un
  référentiel qui bouge sans que ce qui le cite le sache**.
- **`revue` §8 dit toujours « `Done` ou `Rework Needed` »** là où `revue-spec` §4 dit
  `Human Review Requested`. Sans effet ici — douze remarques imposent `Rework Needed` des deux
  côtés —, l'écart est **intact**, seconde occurrence sans conséquence observée. Le cas où il mord
  reste le tour sans aucune remarque, qui ne s'est toujours pas présenté en cinq tours.

**Et une contradiction du protocole est atteinte pour la troisième fois.** `revue` §2 interdit de
fondre les axes, `revue` §6 veut une remarque par constat : les deux axes ont trouvé **le même
défaut** — les doublons de l'inventaire du §1.2 — avec deux conséquences différentes, une
contradiction interne pour l'un, une acceptation indécidable pour l'autre. Fusionné à la main en une
remarque unique portant les deux lectures et le disant, sans qu'aucune clause l'autorise. Comptée une
fois, du côté Conformité.

## 4. Qualité de la sortie

> **À compléter par un tiers.** Le relecteur ne juge pas sa propre sortie (`D-039`). Ce qui suit
> n'est que la matière brute.

**Les faits bruts.**

*Le sort des douze remarques n'est pas connu* au moment d'écrire. Les quatre tours précédents
affichent 11/11, 12/12, 16/16 et 16/16 retenues, aucun refus motivé — soit **55 sur 55**.

*Six violations dures, toutes sur l'axe Conformité, et leur nature a basculé par rapport au tour 4* :
**trois sont des renvois qui ne résolvent pas**, là où le tour 4 n'en portait aucun et n'avait produit
que des contradictions internes.

- l'annexe C écrit « Ces cinq mesures sont ce qui autorise le **§5** à affirmer que ça peut
  fonctionner » ; **le document n'a pas de §5** — il s'arrête au §3 puis aux annexes ;
- le §2.1 écrit « Trois choix structurent la construction ; leur **arbitrage complet est en annexe
  A** » ; l'annexe A n'arbitre que la piste et l'hébergement — le projet dédié est arbitré au §2.2,
  la couche applicative dans `D-052` ;
- le §2.2 écrit « les runs concurrents sont **construits et prouvés** (`architecture.md` §7.13) » ;
  `architecture.md` §7.13 est titrée « **TRANCHÉ, NON CONSTRUIT** ». La section qui l'atteste est la
  §4.13, « **CONSTRUIT** (jalon 6b) », que la spec cite correctement deux lignes plus bas.

*Trois contradictions internes*, opposables sans le dépôt, deux passages qui ne peuvent pas être
vrais ensemble :

- le `flowchart` trace `UI --> QRY` alors que §3.1 range « **jusqu'où les ViewModels passent par les
  commandes** » en question ouverte et que la légende ne parle que d'écritures — la figure tranche ce
  que la prose laisse ouvert, ce que le document se reproche lui-même deux paragraphes plus bas
  (« une figure affirme, même quand la prose se tait ») ;
- le `sequenceDiagram` et son commentaire font appeler par l'éditeur « la même commande au moment de
  son enregistrement », alors que le paragraphe d'au-dessus décrit un éditeur *stateful* qui « ne
  traverse la couche qu'en ce point » — appeler `AjouterEtape` à la sauvegarde relirait le disque et
  perdrait N mutations en mémoire ;
- le §1.2 porte « Lancer un workflow » **et** « Lancer depuis la page », « Rouvrir un run passé en
  relecture » **et** « Rouvrir la trajectoire d'un run terminé », alors que le §1.3 pose qu'« un agent
  adresse le workflow par son identifiant à chaque appel » — quatre lignes pour deux gestes, dans le
  référentiel que le scénario 3 déclare « opposable et le seul ».

*Six jugements sur l'axe Découpabilité, dont trois visent la même affirmation de la spec.* Le §3.1
déclare « **Aucune** [question ouverte] **ne bloque le découpage** : ce sont des questions de
structure locale » ; l'axe en oppose trois qui décident chacune d'une acceptation ou d'une arête de
blocage — la maille de sérialisation (dont dépend la dernière clause du scénario 4, celle du
`SQLITE_BUSY` que l'annexe C a mesuré), la forme du catalogue d'outils (dont dépend l'acceptation
répartie du §3.3, « les lignes que j'apporte ont leur outil »), et le cycle de vie des hosts (dont
dépend l'existence même de la ligne « Ouvrir un projet »).

*Les deux autres jugements portent sur ce que la spec ne dit nulle part* : la **descente du socle
hors de la présentation** et le recâblage des ViewModels n'ont aucune règle d'atterrissage, alors que
la sérialisation en a reçu une explicite au §2.3 ; et **deux des trois manques du noyau** — arrêter
un run, lire un run en vol — n'ont aucune clause de recette qui les prouve, le seul scénario qui
exerce la lecture en vol étant celui que le §3.3 exempte vers `Validation`.

*Le sixième jugement* relève que l'**activation** est peinte en vert dans « ce qui naît », est
pré-condition de trois scénarios, et n'est ni une ligne du §1.2 ni un scénario qui atterrit : elle
échappe aux deux mécanismes d'atterrissage que la spec définit.

*Le seul constat non posé*, à consigner parce qu'il mesure le bruit : les doublons du §1.2, trouvés
par les deux axes, fusionnés en une remarque unique. **Aucun constat n'a été écarté** — 13 produits,
12 posés.

*Les deux verdicts d'axe*, rendus séparément :

- **Conformité — désaccord.** « Six écarts opposables subsistent, dont trois renvois qui ne résolvent
  pas et trois contradictions internes. »
- **Découpabilité — désaccord.** « La charge structurelle de la feature — descente du socle,
  recâblage des ViewModels — et l'acceptation des deux gestes neufs du noyau n'ont d'adresse dans
  aucun incrément, et trois questions déclarées non bloquantes décident chacune d'une acceptation ou
  d'une arête de blocage. »

*Ce que l'axe Découpabilité a produit comme instrument* : un découpage candidat complet — sept
incréments, leurs frontières, leur ordre, et la recette rattachée à chacun. Les sept achoppements en
sont dérivés, et non énoncés d'avance. C'est la pièce qui permet à un tiers de vérifier que la
tentative a eu lieu, comme le demande §3 de la DoD (« il se **teste** »).

*Ce que la revue a vérifié dans le dépôt* : l'axe Conformité a confronté au dépôt les renvois à
`architecture.md`, et **un sur quatre a été pris en défaut** (§7.13 pour les runs concurrents). Le
tour 4 avait confronté une douzaine d'assertions **sans en prendre aucune** en défaut ; le tour 3 en
avait démenti trois sur trois. La spec ayant depuis cessé d'énumérer le dépôt (journal 52), la
surface exposée à ce type d'erreur s'est réduite aux seuls renvois — et c'est là que la seule erreur
subsiste.

*Un fait que le tiers doit connaître avant de juger, et qui n'est pas dans l'artefact* : la mémoire
persistante de la session annonce qu'**un point de fond a été laissé exprès dans la spec pour voir si
la revue l'attrape**. Le relecteur l'a donc su avant de commencer, sans savoir lequel — et ne l'a pas
transmis aux axes. Un tiers qui sait lequel c'était peut établir s'il figure parmi les douze
remarques ; le relecteur, non. **Le candidat le plus substantiel produit par ce tour** est la
contradiction du `sequenceDiagram` (remarque 5) : elle atteint le motif affiché de la couche
applicative — « deux écritures d'un même geste divergent » — en montrant que le partage de commande
qu'il invoque est impossible pour la porte fenêtre telle que la spec la décrit.

> **Complété par la session appelante — quel était le point planté, et ce que la revue en a fait.**
> C'est un **fait vérifiable**, pas le jugement que la rubrique attend d'un tiers : celui qui l'écrit
> est celui qui a planté le point, et il est mal placé pour dire ce que sa capture vaut.
>
> **Le point planté était le §3.1**, qui déclare ses questions ouvertes « de structure locale, qui
> appartiennent aux plans de design des incréments ». Le binôme en avait identifié **trois** qui ne
> le sont pas — la maille de la sérialisation, le cycle de vie des hosts, et le projet où atterrit le
> socle partagé. Les renvoyer aux plans locaux rouvre le trou que `D-049` avait bouché.
>
> Ce que la revue en a rendu, ligne à ligne :
>
> - **la maille de la sérialisation** — attrapée, nommément, avec sa conséquence sur la clause
>   `SQLITE_BUSY` du scénario 4, que le binôme n'avait pas reliée ;
> - **le cycle de vie des hosts** — attrapé, nommément, avec sa conséquence sur l'existence de la
>   ligne « Ouvrir un projet » ;
> - **le projet où atterrit le socle** — **pas sous cette forme.** La revue l'atteint par l'autre
>   bord : *la descente du socle hors de la présentation n'a aucune règle d'atterrissage*, ce qui vise
>   le même trou depuis le découpage plutôt que depuis le registre des questions ouvertes ;
> - **la forme du catalogue d'outils** — trouvée en plus, et le binôme ne l'avait pas vue.
>
> Ce que ce relevé n'établit pas, et qu'un tiers seul peut trancher : le relecteur **savait qu'un
> point était planté**, sans savoir lequel. Une capture faite en cherchant n'a pas la même valeur
> qu'une capture faite en lisant, et rien ici ne les distingue.

*Ce que le dispositif n'a pas produit, et qui reste le mode d'échec nommé aux tours 3 et 4* : aucune
des douze remarques ne demande si une section devait exister. Elles chicanent toutes **dans** le
cadre de l'artefact.

## 5. Frictions

Journal des frictions, entrées **43** (la mémoire automatique dément la clause de session neuve —
**troisième occurrence, et aggravée** : la mémoire n'annonce plus seulement les conclusions de
l'artefact, elle annonce qu'un défaut y est planté pour tester la revue), **44** (deux axes sur le
même passage, sans règle — **troisième occurrence**, le seuil de `D-039` étant dépassé depuis la
deuxième), **48** (corriger une instance laisse le primitif porter l'ancienne clause — **seconde
occurrence**, `revue` §8 dit toujours `Done`), **50** (la clause de session neuve n'a pas de doctrine
sur les faits d'état — **seconde occurrence**, avec une variation : le fait a été fourni par
l'appelant et non choisi par l'orchestrateur, ce qui déplace la main sans combler le trou).

**Deux entrées sont sans occurrence ce tour, et c'est mesuré** : **49** (l'attente d'un sous-agent
coûte des appels qui ne produisent rien) — zéro appel d'attente, les axes ayant été lancés en
synchrone ; et **45** (la pièce la plus contestable est la moins citable) — une remarque a pu être
**ancrée directement sur une ligne du bloc `mermaid`**, l'arête `UI --> QRY`, ce que les tours 3 et 4
n'avaient pas réussi à faire.

**Deux frictions neuves**, numérotées au journal par la session appelante et **non recopiées ici** :
**54** (un référentiel qui bouge périme silencieusement le skill qui le cite — motif **inverse** de
celui des entrées 41, 42 et 48, et c'est ce qui le rend neuf), **55** (la citation d'ancrage bute sur
les marques d'emphase, et `revue` §6 ne documente que la tolérance aux blancs).

## 6. Ce que le tour a changé

- **La suppression de la matérialisation intermédiaire a tenu.** Les deux axes ont chargé le document
  Linear eux-mêmes au lieu de lire une extraction. Conséquence directe : la précaution du tour 4 —
  recharger l'original pour vérifier la fidélité de l'extraction — n'a plus d'objet, et aucune
  remarque ne peut plus être imputée à une extraction. Un fichier de travail hors dépôt en moins.
- **La friction 49 a été soldée par le dispositif, sans rien changer aux skills.** Lancer les deux
  axes en synchrone dans un seul message ramène le coût d'attente de douze appels à zéro. La ligne
  « appels d'outils » du tableau mesure de nouveau ce qu'elle prétend mesurer.
- **La friction 45 a reçu son premier contre-exemple.** Une remarque est ancrée sur `UI --> QRY`,
  c'est-à-dire sur une ligne interne d'un bloc `mermaid` — la pièce que les tours 3 et 4 déclaraient
  incitable et qu'ils avaient dû ancrer sur la prose voisine. La contrainte réelle n'est donc pas
  « une figure ne se cite pas », mais « une citation doit être unique dans le document », ce que les
  identifiants de nœud satisfont souvent.
- **Le premier gabarit inter-tours a tenu une seconde fois, et sous contrainte.** Le tableau du tour 4
  a été prolongé d'une colonne alors même que **deux de ses lignes ont cessé d'être comparables** et
  qu'une troisième est devenue non mesurable. Le gabarit a survécu à un changement de référentiel,
  ce qu'aucun tour n'avait encore éprouvé — mais au prix de trois mentions d'incomparabilité dans le
  tableau lui-même. C'est la première mesure de ce que coûte un gabarit quand ce qu'il mesure bouge.
- **La refonte hors revue a produit sa propre récolte, et c'est neuf.** Deux des douze remarques
  n'existent que parce que l'artefact a été restructuré entre les tours : un renvoi vers une section
  supprimée par la renumérotation, un renvoi vers une annexe créée par la relégation. **Une passe de
  forme hors tour de revue introduit des défauts de fond de renvoi** — c'est la première fois que le
  dossier peut le chiffrer.
- **La spec, elle, n'a pas changé** : la revue liste, elle ne réécrit pas. Douze remarques ouvertes
  attendent la reprise, et l'étiquette `Rework Needed` dit de ne pas tirer.
- **Rien n'a été changé dans les skills par ce tour.** Les deux frictions neuves visent l'une le lien
  DoD → skill, l'autre l'outillage de pose ; aucune ne vise le geste de `revue-spec`.

## 7. Verdict pour `revue-spec`

> **À compléter par un tiers.** Les quatre issues de `D-043` amendé — *promu*, *corrigé par le
> journal*, *retiré*, *tué par un fait* — supposent un juge qui n'est pas l'exécutant. Ce qui suit
> n'est que la matière brute.

**Les faits bruts.**

- **Cinq tours, 67 remarques posées** (11 + 12 + 16 + 16 + 12). Les quatre premiers affichent
  **55 retenues sur 55**, aucun refus motivé. **Le sort des 12 de ce tour n'est pas connu** au moment
  d'écrire.
- **`revue-spec` était promu après les tours 1 à 4.** Les douze clauses du §3 ci-dessus ont chacune
  leur pièce ce tour-ci encore.
- **Une clause du skill est fausse sur son propre référentiel** : `revue-spec` §2 annonce douze cases
  quand la DoD en porte dix-sept, et nomme « plan d'implémentation » ce que `D-053` a renommé. C'est
  le premier tour où **le skill aurait fait manquer une part du référentiel** si le mandat de
  l'appelant ne l'avait pas corrigé. Un tiers doit dire si cela relève de *corrigé par le journal*
  ou d'autre chose.
- **La réserve sur l'étiquette reste levée côté instance et intacte côté primitif** : `revue-spec` §4
  dit `Human Review Requested`, `revue` §8 dit toujours `Done`. Le cas où l'écart mord — un tour sans
  aucune remarque — ne s'est pas présenté en cinq tours.
- **La série ne converge toujours pas** : 11, 12, 16, 16, 12. La cinquième valeur baisse pour la
  première fois, mais **elle n'est pas comparable aux précédentes** : l'artefact a été refondu et
  restructuré entre-temps, hors tour de revue. Un tiers dispose maintenant de cinq points pour dire
  si le signal d'arrêt peut venir de la boucle agent, ou s'il ne peut venir que de l'humain au
  temps ⑤.
- **Un déplacement dans la nature des trouvailles, que le tiers seul peut apprécier.** Le tour 3
  démentait trois assertions de code sur trois ; le tour 4 n'en démentait aucune sur une douzaine et
  ne produisait que des contradictions internes ; ce tour produit **trois renvois faux et trois
  contradictions internes**, exactement moitié-moitié. La spec ayant cessé d'énumérer le dépôt
  (journal 52), la surface d'erreur factuelle s'est réduite aux renvois — et c'est là que les erreurs
  se sont déplacées.
- **Le partage violations dures / jugements coïncide exactement avec les axes** pour la première
  fois : 6 violations dures toutes en Conformité, 6 jugements tous en Découpabilité. Les tours 3 et 4
  mélangeaient. Reste à dire si c'est un calibrage plus juste ou une coïncidence à deux axes.
- **Le relecteur savait qu'un défaut était planté**, sans savoir lequel (§4). Aucun tour précédent
  n'a tourné dans cette condition, et elle affaiblit la comparaison avec eux dans les deux sens : un
  relecteur averti cherche plus, et un relecteur averti peut aussi s'arrêter en croyant avoir trouvé.
- **Deux frictions neuves, et aucune n'est dans le geste de relecture** : un référentiel qui bouge
  périme le skill qui le cite, et l'outillage de pose bute sur les marques d'emphase. C'est la
  seconde fois consécutive que les frictions neuves d'un tour se logent **au-dessus et au-dessous**
  du skill, jamais dedans.
- **Ce que ce tour n'établit toujours pas** : rien du cycle au-delà du temps ②. Ce qui suit
  `Rework Needed` est la reprise par le binôme, et elle n'a jamais été jouée par un skill.
- **La réserve qui ne se lève pas** : cinq tours sur **le même artefact**, écrit par le binôme, avec
  un skill de la même main. Le tour utile suivant reste celui que le tour 4 nommait — un premier
  passage de `revue-spec` sur une spec que le binôme n'aura pas rédigée.
