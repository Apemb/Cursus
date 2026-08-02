# 2026-08-02 — `revue-spec`, septième exécution

> Septième tour sur le même artefact, *Un agent pilote Cursus*, le lendemain du sixième. Deux
> changements dominent, et ils ne portent pas sur la même chose : **le référentiel a grossi** — §1
> passe de dix-sept à **vingt** cases et change de titre — tandis que **le skill est inchangé depuis
> le tour 6** et le primitif `revue` inchangé depuis le tour 4. C'est le premier tour où ce que le
> relecteur *oppose* a bougé sans que ce qu'il *applique* ne bouge.
>
> ⚠️ **Comparabilité, et c'est le fait de ce tour.** Les deux lignes « cases de §1 » **redeviennent
> incomparables** au tour précédent. Le tour 6 avait pu les comparer au tour 5, référentiel
> identique ; `D-060` a rouvert l'écart. La mention ⚠️ **ne se compare pas** retrouve donc sa
> **portée pleine** sur ces deux lignes — elle ne vaut plus « sauf vers le tour 5 », elle vaut vers
> toute la série.
>
> ⚠️ **Les réserves des tours 4, 5 et 6 s'appliquent telles quelles** : *plus de remarques n'est pas
> mieux*, *autant de remarques ne veut pas dire les mêmes*, et rien ne mesure le calibrage d'un
> relecteur à l'autre sur le partage violations dures / jugements. **Coût et durée du relecteur** ne
> se mesurent toujours pas de l'intérieur.
>
> ⚠️ **La réserve neuve du tour 6 reste ouverte, sans occurrence ici** : la ligne « écartés ou
> fusionnés » avait cessé de mesurer du bruit pour mesurer de l'erreur. Ce tour n'a écarté **aucun**
> constat pour fausseté et en a fusionné un pour doublon — la ligne remesure donc du bruit, comme aux
> tours 3 à 5, et les deux usages ne se comparent toujours pas.
>
> ⚠️ **Les rubriques 4 et 7 sont laissées à un tiers**, le relecteur ne jugeant pas sa propre sortie
> (`D-039`). Elles ne portent ici que des **faits bruts**, et pas de jugement.

## 1. Ce qui a tourné

`revue-spec` sur le document `Spec — Un agent pilote Cursus`, attaché au projet Linear du même nom,
en colonne `Spec` + `Review Requested`. La carte portait **99 commentaires, 0 ouvert** — les six
tours de spec et les treize remarques de la Discovery, tous soldés.

**Le chemin d'exécution, et où est la trace qu'il a servi** : un sous-agent lancé depuis la session
du binôme, qui a **invoqué le skill** `revue-spec` — l'appel est le premier de la trace —, lequel a
passé le mandat au primitif `revue` avec **deux axes ouverts en sous-agents séparés**, lancés dans le
même message donc en parallèle, aucun ne voyant le rapport de l'autre. Traces vérifiables : douze
commentaires sur la **carte** (jamais sur le document, `D-045`), chacun avec son repère calculé, son
axe et son étiquette de confiance ; l'étiquette `Rework Needed` posée sur le projet et
`Review Requested` retirée ; la colonne `Spec` inchangée ; l'`updatedAt` du document relu après la
pose et inchangé à `2026-08-02T07:56:04.545Z`, antérieur à la première remarque.

**La commande, verbatim et rejouable** — depuis la racine du dépôt :

```
Tu prends une revue de spec sur le backlog Linear du projet Cursus (espace `cursus-app`, équipe `CUR`).

La carte à relire : le **projet** Linear « Un agent pilote Cursus », actuellement en colonne `Spec`
et portant l'étiquette `Review Requested`. Son document `Spec` y est attaché.

Invoque le skill `revue-spec` et suis son protocole jusqu'au bout.

Le dépôt est à la racine du projet. Le référentiel de conformité est `docs/methode/dod/feature/spec.md`.

Un fait d'état, que tu ne peux pas établir depuis l'artefact seul et dont tu as besoin pour instruire
la §2 du référentiel : la carte porte **99 remarques de revue, toutes soldées** (`open: 0`), chacune
avec sa réponse en fil. L'accord de l'humain est structurellement en aval de ton passage.

Puis, **une fois la revue close**, écris sa fiche de retour d'expérience :

- fichier `docs/methode/rex/2026-08-02-revue-spec-tour-7.md` ;
- rubriques **fixes** de `docs/methode/rex/README.md`, dans l'ordre, aucune omise — lis-le avant
  d'écrire ;
- ⚠️ **tu ne juges pas ta propre sortie.** Remplis les rubriques **1, 2, 3, 5 et 6** : ce qui a
  tourné (avec la commande verbatim et rejouable), tes chiffres, ta conformité au protocole clause
  par clause **avec ce qui l'atteste**, les frictions rencontrées, ce que le tour a changé. Pour les
  rubriques **4 (qualité de la sortie)** et **7 (verdict pour le skill éprouvé)**, écris uniquement
  « À compléter par un tiers » suivi des **faits bruts** qu'un tiers utilisera pour les remplir. Un
  relecteur qui se décerne son propre verdict est exactement le piège que ce dossier existe pour
  éviter ;
- **neuf fiches existent déjà** dans le même dossier, dont **six** de `revue-spec` (tours 1 à 6) :
  prends-les pour gabarit, et rends tes chiffres **comparables aux leurs**. La fiche du tour 6 porte
  un tableau à colonnes Tour 1 → Tour 6 — prolonge-le d'une colonne **Tour 7**, en reprenant les
  lignes existantes. Lis aussi les réserves de comparabilité qu'elle écrit sous son tableau : elles
  s'appliquent à toi, y compris la mention « ⚠️ ne se compare pas » que deux lignes portent ;
- ⚠️ **ce qui a bougé depuis le tour 6, et c'est le fait de comparabilité de ce tour-ci.** Le
  référentiel `docs/methode/dod/feature/spec.md` **a changé** : sa §1 est passée de **dix-sept à
  vingt cases** (§2 reste à trois), et elle a été renommée. Recompte-les toi-même dans le fichier.
  Conséquence directe sur ton tableau : les lignes « cases de §1 » **redeviennent incomparables** au
  tour précédent, alors que le tour 6 avait pu les comparer au tour 5 — la mention « ⚠️ ne se compare
  pas » doit donc retrouver sa portée pleine sur ces lignes, et tu dois l'écrire. En revanche le
  skill `revue-spec` est **inchangé depuis le tour 6**, et le primitif `revue` **inchangé depuis le
  tour 4** ;
- ⚠️ **un second changement, qui ne porte pas sur ce que tu appliques mais sur ce que l'artefact a
  subi** : les skills de production (`spec`, `discovery`, `plan-design`) renvoient désormais à leur
  DoD, et `docs/methode/cycle-feature.md` exige qu'une reprise de spec soit **balayée** contre le
  reste du document. La reprise qui a répondu à tes prédécesseurs est la première à avoir été
  produite sous ce régime. Note-le en rubrique 1 ; n'en tire aucune conclusion, c'est la rubrique 4
  d'un tiers qui le fera ;
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

**Le prompt garde les sept charges du tour 6** — le fait d'état fourni par l'appelant, l'inscription
par l'appelant de ce qui a bougé, la mention `⚠️ ne se compare pas` nommée et à reprendre, le relevé
des chiffres des sous-agents, les deux cellules en attente, l'interdiction de chemin personnel,
l'interdiction de commiter. Il en gagne deux :

- **l'appelant inscrit un changement de *portée* d'une mention d'incomparabilité**, et non plus
  seulement son existence. Le tour 6 avait dû réduire la portée de la mention ; ici l'appelant
  demande de la **rétablir en plein**, et dit pourquoi. La charge d'incomparabilité est devenue une
  consigne versionnée, avec son sens de variation ;
- **l'appelant inscrit un second changement qui ne vise pas l'instrument mais l'artefact** — le
  régime de production sous lequel la reprise a été écrite —, en interdisant explicitement d'en tirer
  une conclusion. C'est le premier tour dont le prompt sépare *ce qui a bougé chez le juge* de *ce
  qui a bougé chez l'auteur*.

**Deux différences de dispositif, choisies par l'orchestrateur, qui appartiennent au chemin :**

- **la matérialisation intermédiaire est reconduite**, comme au tour 6 : le document a été chargé une
  fois depuis Linear puis transcrit dans un fichier de travail hors dépôt, que les deux axes ont lu.
  La fidélité est attestée de la même façon — chaque citation vérifiée **unique** par un `grep -o -F`
  avant pose, puis **résolue par l'outil de pose contre le document vivant**. **Douze ancrages,
  douze acceptations du premier coup**, chacun résolu vers une section nommée ;
- **les deux axes ont été lancés en arrière-plan**, dans un seul message, et non en synchrone comme
  aux tours 5 et 6. Les rapports sont arrivés par notification. **Aucun appel d'attente non plus** —
  mais un appel a été dépensé à charger un outil de veille qui n'a finalement pas servi, ce qu'aucun
  tour synchrone n'aurait payé.

**Ce qui a bougé dans ce que le relecteur applique**, inscrit par l'appelant :

- **`docs/methode/dod/feature/spec.md` a changé**, et c'est le fait du tour. Sa §1 passe de dix-sept
  à **vingt** cases et son titre de *L'artefact est complet* à ***L'artefact est complet, et il tient
  debout***. Les trois cases neuves sont *le document ne se contredit pas*, *les faits allégués sont
  vrais*, *toute règle issue d'une décision la cite*. Le décompte a été fait **deux fois
  indépendamment** — par l'orchestrateur dans le fichier, par l'axe Conformité — et donne vingt les
  deux fois. §2 reste à trois ;
- **le skill `revue-spec` est inchangé depuis le tour 6.** Sa clause qui interdit de compter les
  cases ailleurs que dans le référentiel a donc tourné une **seconde** fois, et cette fois-ci sur un
  référentiel qui **avait bougé sans prévenir le skill** — le cas exact que la friction 54 décrivait.
  ⚠️ **Différence avec le tour 6, et elle est décisive** : l'appelant a bien réinscrit le nombre, mais
  la clause du skill était cette fois **la seule protection contre un skill périmé**, puisque le
  référentiel avait changé la veille au soir ;
- **le primitif `revue` est inchangé depuis le tour 4.** ⚠️ C'est ce qui laisse intact l'écart
  d'étiquette : `revue` §8 dit toujours « `Done` ou `Rework Needed` » là où `revue-spec` §4 dit
  `Human Review Requested`.

**Ce qui a bougé dans ce que l'artefact a subi**, inscrit par l'appelant et rapporté sans conclusion :

- **les trois skills de production renvoient désormais à leur DoD** (`spec`, `discovery`,
  `plan-design`), là où ils la citaient zéro fois ;
- **`docs/methode/cycle-feature.md` exige qu'une reprise de spec soit balayée** contre le reste du
  document, « une décision à la fois, sur le document entier » ;
- **la reprise qui a répondu au tour 6 est la première produite sous ce régime.** Elle a produit
  trois décisions — `D-061`, `D-062`, `D-063`. Aucune conclusion n'est tirée ici : c'est la rubrique
  4 d'un tiers.

## 2. Chiffres

| | Tour 1 | Tour 2 | Tour 3 | Tour 4 | Tour 5 | Tour 6 | **Tour 7** |
|---|---|---|---|---|---|---|---|
| Durée de travail du relecteur | 727 s | 619 s | non mesurable de l'intérieur | 1 321 s | 1 223 s | 1 687 s | **1 599 s** |
| Durée des deux axes | non relevée | non relevée | 591 s et 410 s, en parallèle | 524 s et 553 s, en parallèle | 474 s et 371 s, en parallèle | 635 s et 319 s, en parallèle | **826 s et 609 s**, en parallèle |
| Jetons du relecteur | ~135 000 | 82 447 | non mesurable de l'intérieur | 162 263 | 161 967 | 210 460 | **179 658** |
| Jetons des deux axes | non relevés | non relevés | 137 548 + 104 194 = 241 742 | 130 090 + 72 547 = 202 637 | 116 893 + 118 474 = 235 367 | 166 886 + 100 721 = 267 607 | **142 185 + 119 456 = 261 641** |
| Appels d'outils | 36 | 24 | 28 + 37 = 65 | 34 (hors attente) + 35 = 69 ; 12 d'attente en sus | 38 (orchestration, 0 d'attente, fiche comprise) + 19 (axes) = 57 | 35 (orchestration, 0 d'attente, fiche comprise) + 20 (axes) = 55 | **37** (orchestration, **0 d'attente**, fiche comprise) **+ 49** (axes) = **86** |
| Sous-agents ouverts | 2 | 2 | 2 (un par axe) | 2 (un par axe) | 2 (un par axe) | 2 (un par axe) | **2** (un par axe) |
| Constats produits par les axes | non relevé | non relevé | 20 | 17 (9 Conf, 8 Déc) | 13 (6 Conf, 7 Déc) | 21 (13 Conformité dont 1 hors mandat, 8 Découpabilité) | **13** (8 Conformité, 5 Découpabilité dont 1 hors mandat) |
| Écartés ou fusionnés avant pose | non relevé | non relevé | 4 — 1 écarté, 1 subsumé, 2 fusions | 1 — une fusion, 0 écarté | 1 — une fusion de doublon inter-axes, 0 écarté | 2 — 2 écartés pour fausseté factuelle, 0 fusion ⚠️ *ne mesure pas la même chose* | **1 — une fusion de doublon inter-axes, 0 écarté** ⚠️ *remesure du bruit, pas de l'erreur* (voir sous le tableau) |
| **Remarques posées** | 11 | 12 | 16 | 16 | 12 | 19 | **12** |
| Répartition | 3 Conf · 8 Déc | 6 Conf · 5 Déc · 1 hors mandat | 11 Conf · 5 Déc · 0 hors mandat | 9 Conf · 7 Déc · 0 hors mandat | 6 Conf · 6 Déc · 0 hors mandat | 11 Conf · 7 Déc · 1 hors mandat | **8 Conf · 3 Déc · 1 hors mandat** — la première remarque de Conformité porte **aussi** le constat de Découpabilité (fusion) |
| **Violations dures** | 1 | 2 | 12 (11 Conf, 1 Déc) | 7 (4 Conf, 3 Déc) | 6 (toutes Conformité) | 13 (11 Conf, 2 Déc) | **9** (7 Conf dont la fusionnée, 2 Déc) |
| Jugements | 10 | 9 | 4 (tous Déc) | 9 (5 Conf, 4 Déc) | 6 (tous Découpabilité) | 5 (tous Découpabilité) | **2** (1 Conf, 1 Déc) |
| Constats hors mandat — justesse | 3 | 1 | 0 | 0 | 0 | 1 | **1** |
| **Remarques nées d'une figure** | 0 | 0 | 5 | 2 | 4 | 4 | **2** — le libellé du cadre `ouvert` et le compte des arêtes sortantes du cadre `fen`, toutes deux du `flowchart` ; une troisième (les *trois manques du noyau*) s'appuie sur la figure comme l'une de ses trois pièces |
| Remarques visant une **reprise** du tour précédent | — | 0 | 2 | 4 | non mesurable ce tour | au moins 6 ⚠️ *minorant* | **au moins 6**, mesuré par la présence de `D-061`–`D-063` dans le passage visé ou son chapeau ⚠️ *minorant* (voir sous le tableau) |
| Cases de §1 évaluées | 9 | 12 | 12 | 12 | 17 ⚠️ ne se compare pas | 17 ⚠️ ne se compare pas aux tours 1–4 ; se compare au tour 5 | **20** ⚠️ **ne se compare pas** — portée pleine rétablie (voir sous le tableau) |
| Cases de §1 enfreintes | 1 | non relevé | 6 | 1 au sens de la clause ; 6 cases portant une divergence | 0 au sens de la clause ; 4 cases portant une divergence ⚠️ ne se compare pas | 2 au sens de la clause ; 5 cases portant une divergence ⚠️ ne se compare pas aux tours 1–4 ; se compare au tour 5 | **2** au sens de la clause — *le document ne se contredit pas*, *les faits allégués sont vrais*, **toutes deux neuves du jour** ; **3 cases** portent au moins une divergence (les deux précédentes plus *les trois registres*). **0 omission silencieuse** ⚠️ **ne se compare pas** — portée pleine rétablie |
| Cases de §2 | 3, dont 2 en aval | 3 | 3, toutes non opposables depuis l'artefact seul | 3 — 2 tenues, 1 en aval | 3 — 2 tenues sur le fait d'état fourni, 1 en aval | 3 — 2 tenues sur le fait d'état fourni, 1 en aval | **3** — 2 **tenues** sur le fait d'état fourni, 1 (l'accord de l'humain) structurellement en aval |
| Carte avant / après | — | — | 36 → 52, 16 ouverts | 52 → 68, 16 ouverts | 68 → 80, 12 ouverts | 80 → 99, 19 ouverts | **99 commentaires, 0 ouvert → 111, 12 ouverts** |
| Étiquette posée | `Rework Needed` | `Rework Needed` | `Rework Needed` | `Rework Needed` | `Rework Needed` | `Rework Needed` | **`Rework Needed`** |

**Les deux lignes « cases de §1 » redeviennent incomparables, et c'est le fait de ce tour.** Neuf,
douze, douze, douze, dix-sept, dix-sept, **vingt** : le tour 6 avait gagné le premier point de
comparaison de la série ; `D-060` l'a repris le soir même. La mention ⚠️ *ne se compare pas*
**retrouve sa portée pleine** — elle vaut vers toute la série, et non plus « sauf vers le tour 5 ».
C'est la seconde fois en deux tours que le gabarit inter-tours doit exprimer une variation de
**portée** plutôt qu'une variation de valeur.

⚠️ **Et l'incomparabilité est ici plus profonde qu'un changement de dénominateur.** Aux tours
précédents, §1 gagnait des cases qui se vérifiaient **là où la réponse atterrit**. Les trois cases
ajoutées par `D-060` ne se cochent pas en lisant une section : elles se vérifient en **confrontant
deux endroits**. Les **deux** cases enfreintes de ce tour sont exactement deux de ces trois-là. Une
comparaison de « cases enfreintes » entre un référentiel de dix-sept et un de vingt ne compare donc
pas seulement des proportions, mais deux natures de vérification.

⚠️ **Les deux cases enfreintes ne sont pas du même ordre que les divergences.** Neuf des douze
remarques opposent une **contradiction interne** — que la DoD nomme désormais explicitement, là où
elle ne vivait que dans `revue` §3 jusqu'à la veille. La troisième case portant une divergence
(*les trois registres*) est un jugement, non une clause enfreinte. **Aucune omission silencieuse** —
le seul cas que la DoD interdit — n'a été trouvée, pour le troisième tour consécutif.

**La ligne « écartés » remesure du bruit, et il faut l'écrire.** Le tour 6 avait écarté deux constats
pour **fausseté factuelle**, inaugurant la mesure d'un taux d'erreur d'axe. Ce tour n'en a écarté
aucun : les six assertions confrontées au dépôt ont toutes tenu. Il a en revanche **fusionné un
doublon inter-axes**, comme aux tours 3 à 5. La ligne mesure donc de nouveau du bruit, et la valeur
`1` de ce tour ne se compare pas à la valeur `2` du précédent.

**La fusion, et le critère qui l'a décidée — c'est un matériau pour la friction 44.** Les deux axes
ont trouvé le **même défaut** avec **exactement la même citation** : les douze points de traversée du
§2.3. Le tour 5 avait fusionné, le tour 6 avait refusé de fusionner. Ce tour **fusionne**, et le
motif est nommable : le tour 6 justifiait son refus par « des citations distinctes et des questions
distinctes » — ici la citation est **identique au caractère près**, et le second axe ne fait
qu'ajouter une conséquence au constat du premier. Le critère qui se dégage n'est pas *fusionner ou
non*, mais **fusionner quand la citation est identique, poser deux remarques quand elle ne l'est
pas**. Deux autres passages ont été trouvés par les deux axes — la règle de répartition du scénario 4
et le compte de ses clauses — et n'ont **pas** été fusionnés, leurs citations et leurs questions
différant.

**La ligne des reprises reste mesurable et reste minorée.** La reprise entre le tour 6 et celui-ci a
produit trois décisions (`D-061` à `D-063`). **Six remarques au moins** visent un passage qui en
porte une : le paragraphe recollé sous `D-063`, la cinquième clause du scénario 4 née de `D-063`, la
règle « celle-là seule » du §2.3 qui cite `D-062`, la clause Gherkin d'enregistrement périmé, et les
deux remarques sur la matérialisation de la base (`D-062`). ⚠️ **Le chiffre est un minorant assumé** :
le relecteur n'a pas la version antérieure du document et ne peut pas dater un passage qui ne cite
aucune décision.

**La baisse de 19 à 12 ne s'interprète pas seule.** La série est désormais
**11, 12, 16, 16, 12, 19, 12** — elle ne converge pas, et elle vient de redescendre à sa valeur du
tour 5 après son maximum. Quatre causes se superposent sans que rien ne les départage : un
référentiel **élargi de trois cases** (qui devrait pousser vers le haut), une reprise produite pour
la première fois sous un régime de **balayage** (dont l'effet attendu est vers le bas), un artefact
repris **six** fois, et un relecteur différent. ⚠️ **Aucune de ces quatre n'est isolée par ce tour.**

**Ce que la ligne des appels d'outils mesure.** Trente-sept appels d'orchestration, l'écriture de
cette fiche comprise, et zéro appel d'attente. La comparaison légitime est **35 (tour 6) contre 37
(tour 7)**, à charge égale. ⚠️ **Le poste qui grossit est celui des axes, pas celui de
l'orchestration** : 20 → 49 appels d'axe, soit un doublement et demi, l'axe Conformité en ayant
consommé 34 à lui seul pour instruire les trois cases neuves contre le dépôt. ⚠️ **Trois de ces
trente-sept sont des appels perdus** : un chargement d'outil de veille resté inutilisé, un décompte
de citations lancé depuis un répertoire où l'outil de pose refuse de tourner, et un `grep` mal formé.
**Six sont des appels de vérification** — la confrontation au dépôt des assertions d'axe (§4) —,
même poste qu'au tour 6.

**Complété par la session appelante — les deux cellules, et ce qu'elles disent.** Durée **1 599 s**
(1 687 s au tour 6, **−5 %**) et **179 658 jetons** pour le relecteur (210 460, **−15 %**). Le tour
entier — relecteur plus axes — a coûté **441 299 jetons**, contre 478 067 au tour 6, soit **−8 %**.

⚠️ **Le tour coûte moins et coûte plus : c'est le plus cher par remarque de toute la série mesurée.**
**36 775 jetons et 133 s par remarque posée**, contre 25 161 et 89 s au tour 6, 33 111 et 102 s au
tour 5, 22 806 et 83 s au tour 4. La baisse du total est **moins que proportionnelle** à celle de la
récolte, exactement l'inverse du tour précédent. Une seule chose est établie par là — le rendement
par jeton n'est pas monotone sur la série ; **rien ne dit que la récolte est plus mince parce que
l'artefact est meilleur**, et ce partage appartient à la rubrique 4.

⚠️ **Un fait de coût qui ne se lit pas dans le total** : le poste qui a grossi est celui des **axes en
appels** (20 → 49) alors que leurs **jetons ont baissé** (267 607 → 261 641). Un axe a donc payé
davantage d'allers-retours pour moins de texte — signature d'une vérification **contre le dépôt**
plutôt que d'une lecture d'artefact, ce que les trois cases neuves de `D-060` exigent.

⚠️ **Les compteurs d'appels divergent, mais l'écart se referme** : le relecteur en relève 37,
l'appelant en mesure **38**. Écart de **un**, contre quatre au tour 6 et un au tour 5. La ligne du
tableau reste celle du relecteur, par cohérence avec les tours précédents ; ce que le compteur
externe compte en plus reste indéterminé, et aucun des sept tours ne permet de le dire.

## 3. Conformité au protocole

Clause par clause, avec ce qui l'atteste.

| Clause | Tenue | Ce qui l'atteste |
|---|---|---|
| Session neuve, sur l'artefact seul (`revue-spec` §1, `D-039`) | **oui, sous quatre réserves nommées** | Le fil de rédaction n'a pas été transmis ; **les 99 commentaires soldés n'ont pas été ouverts** pour instruire la revue — le fait d'état venant de l'appelant. ⚠️ Réserve 1 : la mémoire automatique de la session résume l'artefact par ses conclusions et sa trajectoire (journal 43), inchangé. ⚠️ Réserve 2 : la mémoire porte l'interdiction issue du tour 5 et n'annonce plus de défaut planté ; elle mentionne encore l'existence d'un gotcha dans son index, et **le fichier qui le détaillerait n'a pas été ouvert**. ⚠️ Réserve 3 : le fait d'état reste un résumé de l'issue des tours précédents. ⚠️ **Réserve 4, neuve et à consigner comme friction** : le décompte final des remarques, fait avec l'outil de listage **après** la pose des douze, a rendu dans sa sortie le **corps d'un fil de réponse d'un tour antérieur**, lu incidemment. Il est survenu **après** que les douze constats ont été produits et posés, et n'a donc pu en influencer aucun ; il n'en reste pas moins que compter les remarques d'une carte expose son historique. **Aucune des quatre n'a été transmise aux axes**, qui n'ont reçu que l'artefact, les référentiels et le fait d'état chiffré |
| Exactement deux axes, jamais fondus (`revue-spec` §2, `revue` §2) | **oui** | Deux sous-agents lancés dans le même message, chacun ignorant l'existence de l'autre ; deux rapports reçus distinctement, chacun se clôturant sur son propre verdict d'axe ; aucun rapport de synthèse qui reclasse. **Une remarque fusionnée** porte les deux axes nommés dans son en-tête, sans que l'un absorbe l'autre — voir sous le tableau des chiffres |
| Les cases de §1 et les trois de §2, clause par clause | **oui, et c'est le tour où la clause a payé** | L'axe Conformité rend deux tableaux de couverture — **vingt** lignes puis trois — avant sa liste de constats, et nomme pour chaque case son verdict. ⚠️ **Le référentiel avait grossi la veille au soir, et le skill l'ignorait** : c'est précisément le scénario que la friction 54 décrivait, et la clause « ⚠️ Compter les cases dans le référentiel, jamais ici » a tenu. Le décompte a été fait **deux fois indépendamment** — orchestrateur et axe — et donne vingt les deux fois |
| Deux citations par constat (`revue` §3) | **oui** | Chacune des douze remarques porte son référentiel (fichier + clause, ou le second passage de l'artefact quand la contradiction est interne) et l'extrait visé, côte à côte. Neuf des douze sont des contradictions internes, où les deux pièces sont deux passages du même document |
| **Confronter chaque figure à la prose** (`revue` §3, `revue-spec` §2) | **oui** | Le mandat des **deux** axes nommait les figures — les deux blocs `mermaid`, les tables, le bloc `gherkin` — et demandait de les confronter au texte qui les entoure, « une figure n'illustre pas, elle affirme ». **Deux des douze remarques en sortent** : le libellé du cadre `ouvert` (« **Descendu** hors de la présentation »), qui affirme un déplacement là où la prose écrit trois lignes plus bas que trois de ses quatre nœuds y naissent ; et le compte des arêtes sortantes du cadre `fen`, où la prose dit « une » et la figure en trace deux — la seconde étant celle que le paragraphe d'au-dessus annonce lui-même. Une troisième s'appuie sur le `flowchart` comme pièce d'appui |
| Écarter la justesse (`revue-spec` §3) | **oui, avec matière** | L'axe Conformité a rendu sa section *hors mandat — justesse* explicitement vide ; l'axe Découpabilité y a versé **un** constat — faire de l'ouverture d'un projet sans base une erreur nommée est un arbitrage d'expérience, tranché en passant dans une phrase de plan technique. Posé comme remarque distincte sous l'intitulé « hors mandat — justesse », conformément à `revue` §6, et **hors** des deux axes. Aucune ligne des deux rapports d'axe ne discute si la spec est un bon choix |
| Étiqueter la confiance (`revue` §5) | **oui** | 9 *violation dure*, 2 *jugement*, 1 *hors mandat — justesse*, aucune ligne ambiguë. ⚠️ **Le partage ne coïncide pas avec les axes**, comme au tour 6 : l'axe Découpabilité a produit **deux violations dures** (deux contradictions internes qui bloquent une frontière) en plus de son jugement. La DoD §3 annonce que cet axe rend « par nature des **jugements** » ; le mandat autorisait explicitement l'exception pour une contradiction interne, et elle a servi |
| Lister sans réécrire (`revue` §6) | **oui** | `updatedAt` du document relu après la pose : `2026-08-02T07:56:04.545Z`, inchangé et antérieur à la première remarque. Aucune remarque ne propose un texte de remplacement ; les trois de Découpabilité posent **la question que le découpage devrait revenir poser**, elles n'y répondent pas |
| Deux issues, jamais trois (`revue` §6, `D-051`) | **oui** | 13 constats produits, 12 posés, **1 fusionné**, **0 écarté**. Aucun constat n'est écrit ailleurs que sur la carte ; aucune « observation non bloquante ». Les deux axes ont reçu la clause dans leur mandat, avec le test *est-ce que quelqu'un doit répondre ?* |
| Poser la remarque sur la carte (`revue` §6, `D-045`) | **oui, sans rappel** | 12 remarques posées par `cursus linear comment add`, ancrées avec leur repère calculé et leur section résolue ; `open` passe de 0 à 12, `total` de 99 à 111. Aucune sur le document |
| Poser l'étiquette, jamais déplacer (`revue-spec` §4, `revue` §8) | **oui** | `Rework Needed` seul sur le projet, `Review Requested` retiré ; `status` reste `Spec` |
| Pas d'escalade, pas de compteur de tours (`revue-spec` §4) | **oui** | Aucune assignation, aucune tentative de convergence au-delà de ce passage, alors même que c'est le septième |

**Une clause corrigée a payé pour la première fois, et c'est le fait du tour.**

- **`revue-spec` §2 interdit de compter les cases ailleurs que dans le référentiel**, depuis la
  correction de la friction 54. Le tour 6 l'avait éprouvée sur un référentiel **inchangé** — donc
  sans que la clause ait rien à empêcher. Ce tour l'éprouve sur un référentiel **qui avait bougé la
  veille au soir**, en passant de dix-sept à vingt cases, sans que le skill en sache rien. Un
  relecteur fidèle à l'ancienne rédaction se serait arrêté à un nombre écrit dans le skill et aurait
  manqué les trois cases neuves — **dont deux portent les seules violations de clause du tour**.
- ⚠️ **Ce que ce tour n'établit toujours pas** : que la clause aurait suffi **sans** le mandat.
  L'appelant a de nouveau écrit lui-même le nombre. La démonstration attend un tour où l'appelant se
  tait, et elle l'attend depuis deux tours.

**Une clause reste non tenable telle qu'écrite, pour la quatrième fois.**

- **`revue` §8 dit toujours « `Done` ou `Rework Needed` »** là où `revue-spec` §4 dit
  `Human Review Requested`. Sans effet ici — douze remarques imposent `Rework Needed` des deux
  côtés —, l'écart est **intact**, quatrième occurrence sans conséquence observée. Le cas où il mord
  reste le tour sans aucune remarque, qui ne s'est toujours pas présenté en sept tours. Friction 48.

**Et la contradiction du protocole est atteinte pour la cinquième fois — cette fois avec un
critère.** `revue` §2 interdit de fondre les axes, `revue` §6 veut une remarque par constat. Les deux
axes ont trouvé **trois fois le même passage**. Un cas a été fusionné (citation identique au
caractère près), deux ne l'ont pas été (citations et questions distinctes). C'est la première fois
que les deux traitements coexistent **dans un même tour**, et donc la première fois qu'un critère de
départage est observable plutôt que postulé. La friction 44 gagne un troisième point de mesure —
tour 5 fusionne, tour 6 refuse, tour 7 fait les deux selon la citation.

## 4. Qualité de la sortie

> **Complété par le binôme**, qui n'a pas participé à la revue et n'en a connu la sortie qu'une fois
> l'étiquette posée. Le relecteur ne juge pas sa propre sortie (`D-039`) ; la matière brute qu'il a
> laissée suit le jugement, inchangée.

**Jugée contre quoi.** Contre le dépôt pour ce qui s'y vérifie, contre l'artefact lui-même pour les
contradictions internes. ⚠️ **Le jugement est partiel et le restera jusqu'à la reprise** : instruire
les douze remarques *est* la reprise, et la rubrique ne peut pas l'anticiper sans la faire. Deux ont
été vérifiées ici — les deux plus lourdes —, les dix autres ne sont pas jugées.

**Les deux vérifiées sont vraies, et l'une est un fait que le binôme avait lui-même établi.**

- *Les « douze points de traversée »* : `OpenProjectViewModel` appelle `_catalog.CreateFromTitle`,
  `.Rename` et `.Delete` en direct — trois écritures qu'aucun des cinq termes du §2.3 ne couvre,
  quand `WorkflowEditorViewModel` n'appelle bien que `_catalog.Save`. Le décompte de douze avait été
  fait à la main par le binôme la veille, **en ouvrant le dépôt**, et corrigé deux fois avant d'être
  écrit ; il omettait un ViewModel entier. C'est exactement le défaut que vise la case *les faits
  allégués sont vrais*, ajoutée au référentiel douze heures plus tôt.
- *Le paragraphe recollé* : la puce `D-063` se termine par « ce qui n'arrive pas aujourd'hui. — les
  deux portes arrêtent n'importe quel run », et la puce `D-061` qui la précède n'a ni motif ni point
  final. « Seule entorse au sans-état du serveur » figure bien **deux fois** dans le document, à deux
  endroits qui s'ignorent.

**Ce que cela vaut.** La sortie est bonne, et pour un motif qui n'est pas le nombre de remarques :
**elle mord là où le référentiel venait d'être élargi**, sans que le relecteur ait su que c'était le
point d'intérêt. Neuf remarques sur douze opposent une contradiction interne — la catégorie que la
DoD ne nommait pas avant-hier et que seul le skill du juge rendait opposable. La correspondance entre
ce que `D-060` a ajouté et ce que le tour a récolté est le fait le plus solide de la fiche.

⚠️ **Ce que le tiers refuse d'inférer.** Que la baisse de 19 à 12 mesure un artefact meilleur. Le
coût par remarque est le **plus élevé de la série** (36 775 jetons, 133 s), et la part des remarques
visant la reprise précédente a **monté en proportion** — au moins 6 sur 19 au tour 6, au moins 6 sur
12 ici. Une récolte plus mince, plus chère, et plus concentrée sur le texte le plus récent : ces trois
faits ne composent pas le portrait d'un document qui converge. Ils ne composent pas non plus celui
d'un document qui se dégrade. **Le tour ne tranche pas, et le prochain non plus s'il est lu seul.**

⚠️ **Une réserve sur le zéro écarté.** 0 constat faux sur 13, contre 2 sur 21 au tour précédent — le
chiffre est bon, mais il ne se compare pas franchement : neuf des douze remarques sont des
contradictions internes, qui s'opposent **sans ouvrir le dépôt** et n'exposent donc pas le relecteur
au mode d'erreur que la friction 58 décrit. Le taux tombe en partie parce que la matière a changé de
nature. Deux points ne font pas une tendance.

**Les faits bruts** *(laissés par le relecteur, inchangés)***.**

*Le sort des douze remarques n'est pas connu au moment d'écrire.* Les six tours précédents affichent
**86 retenues sur 86**, aucun refus motivé.

*Neuf violations dures*, dont **sept sur l'axe Conformité** (l'une portant aussi le constat de
Découpabilité) et deux sur l'axe Découpabilité. Leur nature se répartit ainsi :

- **un fait allégué pris en défaut contre le dépôt** : le §2.3 donne « douze points de traversée »
  ventilés en cinq termes, et aucun ne couvre *Workflows d'un projet* — dont trois lignes écrivent
  sur le disque (`OpenProjectViewModel` appelle `_catalog.CreateFromTitle`, `.Rename` et `.Delete` en
  direct, quand `WorkflowEditorViewModel` n'appelle que `_catalog.Save`, le point unique décrit). La
  règle de comptage change en outre d'un terme à l'autre : *Tâches* et *Run* comptent les écritures,
  *Projets* et *Connexions tracker* comptent les lignes d'inventaire, lecture comprise ;
- **un paragraphe recollé sous la mauvaise décision** : le développement du registre des runs en vol
  (`D-061`) se trouve soudé, après un tiret orphelin, à la puce de `D-063` — laissant la puce de
  `D-061` sans motif ni point final, un pronom sans antécédent, et **deux passages revendiquant la
  même exclusivité** (« la seule entorse au sans-état du serveur ») ;
- **sept contradictions internes** de plus, opposables sans le dépôt : deux nées des figures (le
  libellé du cadre `ouvert`, le compte des arêtes du cadre `fen`) ; *ouvrir un projet* donné en
  exemple de commande partagée alors que deux autres sections le placent hors périmètre et déclarent
  qu'il n'écrit rien ; « trois manques **du noyau** » au §3.1 quand l'annexe B en nomme deux et que
  trois passages placent le troisième hors du noyau ; le scénario 4 qui porte cinq clauses là où la
  règle de répartition en compte quatre, avec un ordinal (« la première clause ») qui contredit le
  §2.2 (« la dernière clause ») ; et, côté Découpabilité, la bijection *objets ↔ incréments mutants*
  impossible dans les deux sens, plus la clause d'enregistrement périmé que la règle ne nomme pas.

*Deux jugements*, un par axe :

- **le §3.1**, intitulé *Les trois registres*, qui présente trois blocs dont le registre **construit**
  n'est pas — remplacé par un *Tranché hors périmètre*, sans que l'écart soit motivé ni renvoyé ;
- **l'erreur nommée d'un projet non initialisé**, comportement observable que le §2.2 introduit et
  qu'aucune clause de l'annexe B ne recette — laissant l'incrément qui le porte sans acceptation sur
  ce point, sur aucune des deux portes.

*Le constat hors mandat — justesse* : faire de l'ouverture d'un projet sans base une erreur est une
décision produit autant que technique. Un projet inscrit dont la base a disparu — dépôt recloné,
machine changée — cesse de s'ouvrir et exige un geste explicite. Posé sans être tranché.

*Aucun constat écarté pour fausseté.* **13 produits, 12 posés, 1 fusionné, 0 écarté.** Le tour 6
avait mesuré 2 constats faux sur 21 ; les six assertions confrontées au dépôt ce tour-ci ont toutes
tenu. ⚠️ **Un tiers doit peser ce que cela vaut** : le poste de vérification était le même (six
confrontations), mais la matière était différente — neuf des douze remarques sont des contradictions
**internes**, qui n'appellent aucune ouverture du dépôt pour être opposées.

*Les deux verdicts d'axe*, rendus séparément :

- **Conformité — désaccord.** « L'artefact tient dix-neuf des vingt cases de §1 sur le fond, mais
  échoue sur les deux cases de cohérence — sept passages qui ne peuvent pas être vrais ensemble. »
- **Découpabilité — désaccord.** « La règle de répartition du scénario 4 suppose une correspondance
  un-à-un entre ses clauses, quatre objets et quatre incréments mutants, que le découpage ne peut pas
  réaliser. »

*Ce que l'axe Découpabilité a produit comme instrument* : un découpage candidat complet — **huit
incréments**, leurs frontières, leur orientation technique, leur acceptation, leurs arêtes de
blocage, et une table de couverture rattachant chaque scénario Gherkin à son incrément. **Les 31
lignes atteignables de l'inventaire s'y répartissent sans reste.** Les quatre achoppements en sont
dérivés, et non énoncés d'avance. C'est la pièce qui permet à un tiers de vérifier que la tentative a
eu lieu, comme le demande §3 de la DoD (« il se **teste** »).

*Ce que la revue a vérifié dans le dépôt* : l'orchestrateur a confronté au dépôt **six** éléments
avant de poser — le nombre de cases de §1 et le nouveau titre de la section, l'existence de `D-061` à
`D-063`, la date et le contenu du changement de référentiel, les trois appels d'écriture du catalogue
dans `OpenProjectViewModel` contre l'appel unique de `WorkflowEditorViewModel`, l'historique git des
deux skills, et l'unicité des douze citations. **Les six ont tenu, aucune n'a été démentie.** L'axe
Conformité avait de son côté confronté au code une vingtaine d'assertions de l'artefact et les donne
toutes pour vraies.

*Un fait que le tiers doit connaître avant de juger* : **l'artefact relu est le premier produit sous
le régime de balayage** de `cycle-feature.md`, et les trois skills de production renvoient désormais
à leur DoD. Le nombre de remarques est passé de 19 à 12 entre le tour précédent et celui-ci, sur un
référentiel simultanément **élargi de trois cases**. Ces deux mouvements vont en sens contraire et
**rien dans ce tour ne les sépare**.

*Un second fait, sur la nature de ce que la revue a trouvé* : **neuf des douze remarques opposent une
contradiction interne**, et deux des trois cases neuves du référentiel sont précisément celles qui
rendent ces contradictions opposables depuis la DoD. Avant `D-060`, elles ne l'étaient que par
`revue` §3 — c'est-à-dire depuis le skill du juge, et invisibles à l'auteur.

*Ce que le dispositif n'a pas produit, et qui reste le mode d'échec nommé aux tours 3 à 6* : aucune
des douze remarques ne demande si une section devait exister. Elles chicanent toutes **dans** le
cadre de l'artefact.

## 5. Frictions

Journal des frictions, entrées **43** (la mémoire automatique dément la clause de session neuve —
**cinquième occurrence**, atténuée comme au tour 6 : la mémoire porte l'interdiction issue du tour 5,
mentionne encore l'existence d'un gotcha dans son index, et le fichier qui le détaille n'a pas été
ouvert), **44** (deux axes sur le même passage, sans règle — **cinquième occurrence**, et la
première où **les deux traitements coexistent dans un même tour** : une fusion sur citation
identique, deux non-fusions sur citations distinctes), **48** (corriger une instance laisse le
primitif porter l'ancienne clause — **quatrième occurrence**, `revue` §8 dit toujours `Done`),
**50** (la clause de session neuve n'a pas de doctrine sur les faits d'état — **quatrième
occurrence**, sans variation : le fait est venu de l'appelant, comme aux tours 5 et 6).

**Trois entrées sont sans occurrence ce tour, et c'est mesuré** : **49** (l'attente d'un sous-agent
coûte des appels qui ne produisent rien) — zéro appel d'attente, troisième fois consécutive, mais
voir la friction neuve ci-dessous ; **45** (la pièce la plus contestable est la moins citable) —
deux remarques ancrées sur des passages commentant les figures, aucune n'a buté ; **55** (la citation
d'ancrage bute sur les marques d'emphase) — **douze poses, douze acceptées du premier coup**, la
vérification d'unicité par `grep -o -F` ayant été faite avant pose et les citations choisies hors des
marques d'emphase.

**Trois frictions neuves**, numérotées au journal par la session appelante et **non recopiées ici** :
**60** (compter les remarques d'une carte en expose le contenu, et le protocole demande ce décompte
sans dire quand — sans effet ce tour, le décompte ayant suivi la pose), **61** (l'outil de pose exige
d'être appelé depuis un projet Cursus, ce qui contrarie la matérialisation hors dépôt recommandée
depuis le tour 4), **62** (la friction 49 reste à zéro occurrence tout en ayant coûté un appel de
préparation — angle mort de la métrique).

## 6. Ce que le tour a changé

- **La correction de la friction 54 a payé pour la première fois, et ce tour en est la démonstration
  partielle.** Le tour 6 avait éprouvé la clause sur un référentiel inchangé — elle n'avait rien à
  empêcher. Ici le référentiel avait grossi la veille au soir, de dix-sept à vingt cases, et **deux
  des trois cases neuves portent les seules violations de clause du tour**. Un skill qui aurait
  gardé un nombre en dur les aurait manquées. ⚠️ **Ce que l'essai n'établit toujours pas** : que la
  clause aurait suffi sans le mandat, qui réinscrivait le nombre. Deux tours de suite, la
  démonstration bute sur le même point.
- **La mention ⚠️ *ne se compare pas* a changé de sens de variation.** Le tour 6 l'avait **réduite**
  pour la première fois ; ce tour la **rétablit en plein**. Le gabarit inter-tours sait maintenant
  exprimer une portée qui va et vient, et non seulement une incomparabilité qui se lève.
- **La friction 44 a reçu son troisième traitement, et le premier qui porte un critère.** Le tour 5
  fusionnait, le tour 6 refusait ; ce tour fait **les deux dans le même passage de revue**, et le
  départage est nommable : citation identique → une remarque, citations distinctes → deux remarques.
  Ce n'est pas une décision — aucune n'a été prise ici —, c'est le premier matériau qui rende la
  friction tranchable sans arbitrer entre deux tours entiers.
- **La ligne « écartés ou fusionnés » a repris son ancien sens**, un tour après en avoir changé.
  Elle porte désormais deux mentions d'incomparabilité pour deux motifs opposés, et la fiche doit
  dire à chaque tour ce qu'elle compte. C'est le second signe que cette ligne mesure mal.
- **Le taux d'erreur d'axe est à zéro ce tour**, après le 2/21 du tour 6. Le dossier a maintenant
  deux points ; il ne dit pas encore si la vérification empirique de l'orchestrateur — que le
  protocole ne prescrit pas — est ce qui fait la différence, ou si c'est la nature des constats.
- **Le gabarit inter-tours a tenu une quatrième fois**, prolongé d'une septième colonne, sans qu'une
  ligne ait dû être créée ou supprimée.
- **La spec, elle, n'a pas changé** : la revue liste, elle ne réécrit pas. Douze remarques ouvertes
  attendent la reprise, et l'étiquette `Rework Needed` dit de ne pas tirer.
- **Rien n'a été changé dans les skills ni dans les DoD par ce tour.** Les trois frictions neuves
  visent l'outillage de pose, le protocole de comptage et le mode de lancement des axes — aucune ne
  vise le geste de `revue-spec`.

## 7. Verdict pour `revue-spec`

> **Complété par le binôme.** Les quatre issues de `D-043` amendé — *promu*, *corrigé par le
> journal*, *retiré*, *tué par un fait* — supposent un juge qui n'est pas l'exécutant. La matière
> brute laissée par le relecteur suit le verdict, inchangée.

## **Verdict : promu, confirmé** — et l'instrument n'est plus ce qui limite la série

`revue-spec` était déjà promu ; ce tour ne le remet pas en cause et ajoute la pièce qui manquait.
**C'est le premier tour où l'instrument reste fixe pendant que la règle bouge**, et il a suivi la
règle : les vingt cases ont été recomptées dans le référentiel, et **les deux seules violations de
clause du tour tombent sur deux des trois cases neuves**. Un skill qui aurait gardé son compte en dur
— ce qu'il faisait jusqu'au tour 6 — les aurait manquées toutes les deux. La friction 54 est refermée
en pratique, et non plus seulement sur le papier.

⚠️ **La question ouverte au tour 6 reste ouverte, à l'identique** : le mandat de l'appelant
réinscrivait le nombre de cases, donc rien n'établit que la clause aurait suffi seule. Deux tours de
suite, la démonstration bute au même endroit. **Le remède est à portée** — retirer le nombre du
mandat au tour 8 — et il ne coûte rien.

**Ce que le verdict ne couvre pas, et c'est là qu'est le sujet.** Aucune des trois frictions neuves
ne vise le geste de `revue-spec` : elles visent l'outillage de pose, le protocole de comptage et le
mode de lancement des axes. **Quatrième tour consécutif** où les frictions se logent au-dessus ou
au-dessous du skill, jamais dedans. Un skill dont les défauts ne se trouvent plus est un skill dont on
a cessé d'apprendre — non parce qu'il est parfait, mais parce que **le dispositif ne l'éprouve plus
que sur le même artefact, écrit par la même main**. La réserve nommée aux tours 4, 5 et 6 ne se lève
toujours pas, et elle est désormais la principale : *le tour utile suivant est un premier passage sur
une spec que le binôme n'aura pas rédigée.*

**Une seconde chose que sept tours n'établissent pas** : rien du cycle au-delà du temps ②. La reprise
qui suit `Rework Needed` n'a jamais été jouée par un skill, et c'est elle qui produit le texte dont
la revue suivante récolte la moitié de ses remarques.

**Les faits bruts** *(laissés par le relecteur, inchangés)***.**

- **Sept tours, 98 remarques posées** (11 + 12 + 16 + 16 + 12 + 19 + 12). Les six premiers affichent
  **86 retenues sur 86**, aucun refus motivé. **Le sort des 12 de ce tour n'est pas connu** au moment
  d'écrire.
- **Le skill est inchangé depuis le tour 6, et il a tourné sur un référentiel qui avait changé.**
  C'est la première fois de la série que l'instrument reste fixe pendant que la règle bouge. Les
  douze clauses du §3 ci-dessus ont chacune leur pièce.
- **La clause corrigée au tour 6 a eu, ce tour, quelque chose à empêcher.** Deux des trois cases
  neuves du référentiel portent les seules violations de clause du tour ; un compte figé dans le
  skill les aurait manquées. Un tiers doit dire si cela vaut validation de la correction, sachant
  que le mandat de l'appelant réinscrivait le nombre — la question posée au tour 6 reste ouverte au
  tour 7, à l'identique.
- **La réserve sur l'étiquette reste levée côté instance et intacte côté primitif** : `revue-spec` §4
  dit `Human Review Requested`, `revue` §8 dit toujours `Done`. Le cas où l'écart mord — un tour sans
  aucune remarque — ne s'est pas présenté en sept tours.
- **La série ne converge toujours pas, et elle vient de redescendre** : 11, 12, 16, 16, 12, 19, 12.
  La septième valeur égale la cinquième. ⚠️ **Deux mouvements de sens contraire se superposent sur ce
  tour** : le référentiel s'élargit de trois cases (vers le haut) et la reprise est la première
  produite sous le régime de balayage de `cycle-feature.md` (vers le bas). Un tiers dispose de sept
  points, et d'aucun moyen dans ce tour de séparer les deux causes.
- **Le motif mécanique nommé au tour 6 n'est pas infirmé** : chaque reprise produit du texte que
  personne n'a relu, donc une récolte pour le tour suivant. **Six remarques au moins** visent le
  texte né de la reprise (`D-061` à `D-063`), dont la plus grave — un paragraphe recollé sous la
  mauvaise décision, laissant deux passages revendiquer la même exclusivité. Un tiers doit dire si le
  balayage a réduit cette part ou non ; la fiche du tour 6 mesurait « au moins 6 » sur 19, celle-ci
  mesure « au moins 6 » sur 12.
- **Le taux d'erreur d'axe est de 0 sur 13 ce tour**, contre 2 sur 21 au tour 6. Un tiers doit dire
  si l'écart tient à la nature des constats — neuf contradictions internes, qui n'exigent aucune
  ouverture du dépôt — ou à autre chose.
- **Le partage violations dures / jugements ne coïncide pas avec les axes**, pour le second tour
  consécutif : l'axe Découpabilité a produit deux violations dures. La proportion de jugements chute
  fortement — 5 sur 19 au tour 6, **2 sur 12** ici. Reste à dire si c'est un calibrage, une propriété
  de l'artefact, ou l'effet des trois cases neuves qui rendent opposable ce qui relevait du jugement.
- **La condition « le relecteur sait qu'il est testé » est dans le même état qu'au tour 6** —
  atténuée sans être levée, et rien ne la quantifie.
- **Les trois frictions neuves ne sont pas dans le geste de `revue-spec`** — elles visent l'outillage
  de pose, le protocole de comptage des remarques et le mode de lancement des axes. C'est la
  **quatrième fois consécutive** que les frictions neuves d'un tour se logent au-dessus ou au-dessous
  du skill éprouvé, jamais dedans.
- **Ce que ce tour n'établit toujours pas** : rien du cycle au-delà du temps ②. Ce qui suit
  `Rework Needed` est la reprise par le binôme, et elle n'a jamais été jouée par un skill.
- **La réserve qui ne se lève pas** : sept tours sur **le même artefact**, écrit par le binôme, avec
  un skill de la même main. Le tour utile suivant reste celui que les tours 4, 5 et 6 nommaient — un
  premier passage de `revue-spec` sur une spec que le binôme n'aura pas rédigée.
