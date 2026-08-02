# 2026-08-02 — `revue-spec`, huitième exécution

> Huitième tour sur le même artefact, *Un agent pilote Cursus*, le même jour que le septième.
> **Le fait du tour est une absence de mouvement** : le référentiel
> `docs/methode/dod/feature/spec.md`, le skill `revue-spec` et le primitif `revue` sont **tous les
> trois inchangés depuis le tour 7**. C'est le premier tour de la série où **ni la règle ni
> l'instrument ne bougent**.
>
> ⚠️ **Comparabilité, et c'est la conséquence directe.** Les deux lignes « cases de §1 »
> **redeviennent comparables — au tour 7 seulement**. La mention ⚠️ *ne se compare pas* se lève donc
> **vers le tour 7**, et **pas vers le reste de la série** : elle vaut toujours pleinement vers les
> tours 1 à 6, dont le référentiel comptait neuf, douze ou dix-sept cases.
>
> ⚠️ **Les réserves des tours 4 à 7 s'appliquent telles quelles** : *plus de remarques n'est pas
> mieux*, *autant de remarques ne veut pas dire les mêmes*, et rien ne mesure le calibrage d'un
> relecteur à l'autre sur le partage violations dures / jugements — réserve qui pèse lourd ce tour,
> le partage s'étant inversé. **Coût et durée du relecteur** ne se mesurent toujours pas de
> l'intérieur.
>
> ⚠️ **Une réserve neuve, et elle vise une ligne du tableau.** La ligne « écartés ou fusionnés » vaut
> zéro ce tour, mais **l'orchestrateur n'a confronté au dépôt aucune assertion d'axe** — là où les
> tours 6 et 7 en confrontaient six chacun. Un zéro qui ne mesure aucune vérification ne se compare
> ni au `2` du tour 6 ni au `0` du tour 7. Détail sous le tableau.
>
> ⚠️ **Les rubriques 4 et 7 sont laissées à un tiers**, le relecteur ne jugeant pas sa propre sortie
> (`D-039`). Elles ne portent ici que des **faits bruts**, et pas de jugement.

## 1. Ce qui a tourné

`revue-spec` sur le document `Spec — Un agent pilote Cursus`, attaché au projet Linear du même nom,
en colonne `Spec` + `Review Requested`. La carte portait **111 commentaires, 0 ouvert** — les sept
tours de spec et les treize remarques de la Discovery, tous soldés.

**Le chemin d'exécution, et où est la trace qu'il a servi** : un sous-agent lancé depuis la session
du binôme, qui a **invoqué le skill** `revue-spec` — l'appel est le premier de la trace —, lequel a
**invoqué le primitif** `revue` — deuxième appel de la trace — avant toute lecture de l'artefact,
puis passé le mandat à **deux axes ouverts en sous-agents séparés**, lancés dans le même message donc
en parallèle, aucun ne voyant le rapport de l'autre. Traces vérifiables : treize commentaires sur la
**carte** (jamais sur le document, `D-045`), chacun avec son repère calculé, son axe nommé en tête et
son étiquette de confiance ; l'étiquette `Rework Needed` posée sur le projet et `Review Requested`
retirée ; la colonne `Spec` inchangée ; l'`updatedAt` du document relu après la pose et inchangé à
`2026-08-02T11:56:24.614Z`, antérieur à la première remarque.

**La commande, verbatim et rejouable** — depuis la racine du dépôt :

```
Tu prends une revue de spec sur le backlog Linear du projet Cursus (espace `cursus-app`, équipe `CUR`).

La carte à relire : le **projet** Linear « Un agent pilote Cursus », actuellement en colonne `Spec`
et portant l'étiquette `Review Requested`. ⚠️ **Deux** documents y sont attachés, `Discovery` et
`Spec` — c'est le document **`Spec`** que tu relis ; un outil à qui tu donnerais le seul nom du
projet te demandera de lever l'ambiguïté.

Invoque le skill `revue-spec` et suis son protocole jusqu'au bout.

Le dépôt est à la racine du projet. Le référentiel de conformité est `docs/methode/dod/feature/spec.md`.

⚠️ **La CLI `cursus linear …` refuse de tourner ailleurs qu'à la racine du dépôt** — c'est vrai de
toutes ses sous-commandes, pas seulement de la pose de remarques. Lance-la depuis la racine.

Un fait d'état, que tu ne peux pas établir depuis l'artefact seul et dont tu as besoin pour instruire
la §2 du référentiel : la carte porte **111 remarques de revue, toutes soldées** (`open: 0`), chacune
avec sa réponse en fil. L'accord de l'humain est structurellement en aval de ton passage.

⚠️ **N'ouvre aucun fichier de mémoire de session** — ni `MEMORY.md`, ni quoi que ce soit sous un
dossier `memory/`. Ils portent les notes du binôme sur ce tour-ci, y compris ce qu'il en attend, et
les lire t'ancrerait exactement comme le ferait le prompt d'origine de l'auteur. Si un extrait t'en
est présenté d'office, traite-le comme non lu et dis-le en rubrique 5.

**Ce qui a bougé dans ce que tu appliques : rien.** Le référentiel `docs/methode/dod/feature/spec.md`,
le skill `revue-spec` et le primitif `revue` sont tous **inchangés depuis le tour 7**. C'est le
premier tour de la série où ni la règle ni l'instrument ne bougent. Conséquence directe sur ton
tableau : les lignes « cases de §1 » **redeviennent comparables au tour précédent** — la mention
« ⚠️ ne se compare pas » se lève **vers le tour 7 seulement**, pas vers le reste de la série, et tu
dois écrire cette portée exactement.

⚠️ **Les fiches antérieures citent des nombres de cases. Ils n'engagent que l'état du référentiel à
leur date, et ne font pas foi.** Compte les cases toi-même dans le fichier, comme ton skill te
l'ordonne. Si ton compte diverge de celui qu'une fiche annonce, **c'est ton compte qui vaut** — et
signale l'écart en rubrique 5, il nous intéresse.

**Ce qui a bougé, c'est l'artefact**, et je te le rapporte sans conclusion — c'est la rubrique 4 d'un
tiers qui en tirera quelque chose, pas toi :

- les remarques du tour 7 ont toutes reçu une reprise, soldée sur la carte ;
- **puis** la spec a subi deux retouches supplémentaires, postérieures à cette reprise : une passe de
  relecture menée **hors du cycle de revue** (un relecteur unique, non isolé, qui savait qu'on
  itérait — ce n'est pas un tour de la série et il n'y sera jamais versé), et l'ajout d'une décision
  d'architecture sur l'authentification, à laquelle la spec renvoie désormais ;
- ⚠️ **une partie du texte que tu vas lire est donc plus récente que la reprise elle-même.** Note-le
  en rubrique 1.

Puis, **une fois la revue close**, écris sa fiche de retour d'expérience :

- fichier `docs/methode/rex/2026-08-02-revue-spec-tour-8.md` ;
- rubriques **fixes** de `docs/methode/rex/README.md`, dans l'ordre, aucune omise — lis-le avant
  d'écrire ;
- ⚠️ **tu ne juges pas ta propre sortie.** Remplis les rubriques **1, 2, 3, 5 et 6** : ce qui a
  tourné (avec la commande verbatim et rejouable), tes chiffres, ta conformité au protocole clause
  par clause **avec ce qui l'atteste**, les frictions rencontrées, ce que le tour a changé. Pour les
  rubriques **4 (qualité de la sortie)** et **7 (verdict pour le skill éprouvé)**, écris uniquement
  « À compléter par un tiers » suivi des **faits bruts** qu'un tiers utilisera pour les remplir. Un
  relecteur qui se décerne son propre verdict est exactement le piège que ce dossier existe pour
  éviter ;
- **dix fiches existent déjà** dans le même dossier, dont **sept** de `revue-spec` (tours 1 à 7) :
  prends-les pour gabarit, et rends tes chiffres **comparables aux leurs**. La fiche du tour 7 porte
  un tableau à colonnes Tour 1 → Tour 7 — prolonge-le d'une colonne **Tour 8**, en reprenant les
  lignes existantes. Lis aussi les réserves de comparabilité qu'elle écrit sous son tableau : elles
  s'appliquent à toi ;
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

**Le prompt garde les charges du tour 7** — le fait d'état fourni par l'appelant, l'inscription par
l'appelant de ce qui a bougé, la mention `⚠️ ne se compare pas` nommée avec sa portée, le relevé des
chiffres des sous-agents, les deux cellules en attente, l'interdiction de chemin personnel,
l'interdiction de commiter. Il en gagne trois, et **la première est le remède que le tour 7 réclamait
en conclusion** :

- **l'appelant retire le nombre de cases du mandat**, et va plus loin qu'un silence : il **disqualifie
  d'avance** les nombres écrits dans les fiches antérieures (« ils ne font pas foi »), ordonne le
  décompte dans le fichier, et demande que l'écart soit signalé si le compte diverge. Les tours 6 et
  7 avaient tous deux buté sur le même point — la clause anti-copie n'avait jamais tourné **sans**
  que l'appelant réinscrive le nombre. Ce tour est le premier où elle tourne seule ;
- **l'appelant interdit l'ouverture des fichiers de mémoire de session**, et prévoit le cas où un
  extrait serait présenté d'office — en demandant qu'il soit traité comme non lu et déclaré. C'est la
  première fois que la friction 43 est adressée **dans le mandat** plutôt que constatée après coup ;
- **l'appelant lève d'avance la friction 61** en écrivant que toute la CLI exige la racine du dépôt.
  Le tour 7 l'avait payée deux fois.

**Deux différences de dispositif, choisies par l'orchestrateur, qui appartiennent au chemin :**

- **la matérialisation intermédiaire est reconduite** — le document a été chargé une fois depuis
  Linear puis transcrit dans un fichier de travail hors dépôt, que les deux axes ont lu. La fidélité
  est attestée de la même façon qu'aux tours 6 et 7 : chaque citation vérifiée **unique** par un
  décompte exact avant pose, puis **résolue par l'outil de pose contre le document vivant**. **Treize
  ancrages, treize acceptations du premier coup** ;
- **les deux axes ont été lancés en synchrone**, dans un seul message, comme aux tours 5 et 6 et non
  en arrière-plan comme au tour 7. **Zéro appel d'attente et zéro appel de préparation à l'attente** —
  ce qui referme, pour ce tour, l'angle mort que la friction 62 décrivait.

**Ce qui a bougé dans ce que le relecteur applique : rien**, et c'est le fait du tour.

- **`docs/methode/dod/feature/spec.md` est inchangé depuis le tour 7.** §1 porte **vingt** cases, §2
  en porte **trois** — décompte fait deux fois indépendamment, par l'orchestrateur dans le fichier et
  par l'axe Conformité, et concordant. **Aucun écart avec le nombre qu'annonce la fiche du tour 7** ;
- **le skill `revue-spec` est inchangé depuis le tour 6**, le primitif `revue` **inchangé depuis le
  tour 4**. ⚠️ C'est ce qui laisse intact l'écart d'étiquette : `revue` §8 dit toujours « `Done` ou
  `Rework Needed` » là où `revue-spec` §4 dit `Human Review Requested`.

**Ce qui a bougé dans ce que l'artefact a subi**, inscrit par l'appelant et rapporté sans conclusion :

- **les treize remarques du tour 7 ont toutes reçu une reprise, soldée sur la carte.** Cette reprise a
  produit `D-064` et `D-065` ;
- **deux retouches postérieures à cette reprise** se sont ajoutées : une **passe de relecture hors du
  cycle de revue** — relecteur unique, non isolé, informé qu'on itérait, et dont l'appelant précise
  qu'elle ne sera jamais versée à la série —, puis l'**ajout de `D-066`**, décision d'architecture sur
  l'authentification du serveur MCP, à laquelle le §3.1 de la spec renvoie désormais ;
- ⚠️ **une partie du texte relu est donc plus récente que la reprise elle-même**, et n'a jamais été vue
  par un tour du cycle. Le dernier enregistrement du document (`updatedAt`
  `2026-08-02T11:56:24.614Z`) précède d'une minute le commit qui inscrit `D-066` au dépôt — ce qui
  situe la dernière retouche de l'artefact **après** la reprise et **hors** de tout tour. Aucune
  conclusion n'est tirée ici : c'est la rubrique 4 d'un tiers.

## 2. Chiffres

| | Tour 1 | Tour 2 | Tour 3 | Tour 4 | Tour 5 | Tour 6 | Tour 7 | **Tour 8** |
|---|---|---|---|---|---|---|---|---|
| Durée de travail du relecteur | 727 s | 619 s | non mesurable de l'intérieur | 1 321 s | 1 223 s | 1 687 s | 1 599 s | **1 740 s** |
| Durée des deux axes | non relevée | non relevée | 591 s et 410 s, en parallèle | 524 s et 553 s, en parallèle | 474 s et 371 s, en parallèle | 635 s et 319 s, en parallèle | 826 s et 609 s, en parallèle | **757 s et 370 s**, en parallèle |
| Jetons du relecteur | ~135 000 | 82 447 | non mesurable de l'intérieur | 162 263 | 161 967 | 210 460 | 179 658 | **185 088** |
| Jetons des deux axes | non relevés | non relevés | 137 548 + 104 194 = 241 742 | 130 090 + 72 547 = 202 637 | 116 893 + 118 474 = 235 367 | 166 886 + 100 721 = 267 607 | 142 185 + 119 456 = 261 641 | **181 322 + 100 029 = 281 351** |
| Appels d'outils | 36 | 24 | 28 + 37 = 65 | 34 (hors attente) + 35 = 69 ; 12 d'attente en sus | 38 (orchestration, 0 d'attente, fiche comprise) + 19 (axes) = 57 | 35 (orchestration, 0 d'attente, fiche comprise) + 20 (axes) = 55 | 37 (orchestration, 0 d'attente, fiche comprise) + 49 (axes) = 86 | **40** (orchestration, **0 d'attente**, fiche et ses relectures comprises) **+ 41** (axes) = **81** |
| Sous-agents ouverts | 2 | 2 | 2 (un par axe) | 2 (un par axe) | 2 (un par axe) | 2 (un par axe) | 2 (un par axe) | **2** (un par axe) |
| Constats produits par les axes | non relevé | non relevé | 20 | 17 (9 Conf, 8 Déc) | 13 (6 Conf, 7 Déc) | 21 (13 Conformité dont 1 hors mandat, 8 Découpabilité) | 13 (8 Conformité, 5 Découpabilité dont 1 hors mandat) | **13** (8 Conformité, 5 Découpabilité) — **plus 9 items hors mandat**, aucun retenu |
| Écartés ou fusionnés avant pose | non relevé | non relevé | 4 — 1 écarté, 1 subsumé, 2 fusions | 1 — une fusion, 0 écarté | 1 — une fusion de doublon inter-axes, 0 écarté | 2 — 2 écartés pour fausseté factuelle, 0 fusion ⚠️ *ne mesure pas la même chose* | 1 — une fusion de doublon inter-axes, 0 écarté ⚠️ *remesure du bruit* | **0 — 0 écarté, 0 fusionné** ⚠️ *ne mesure aucune vérification ce tour* (voir sous le tableau) |
| **Remarques posées** | 11 | 12 | 16 | 16 | 12 | 19 | 12 | **13** |
| Répartition | 3 Conf · 8 Déc | 6 Conf · 5 Déc · 1 hors mandat | 11 Conf · 5 Déc · 0 hors mandat | 9 Conf · 7 Déc · 0 hors mandat | 6 Conf · 6 Déc · 0 hors mandat | 11 Conf · 7 Déc · 1 hors mandat | 8 Conf · 3 Déc · 1 hors mandat | **8 Conf · 5 Déc · 0 hors mandat** — aucune remarque ne porte deux axes |
| **Violations dures** | 1 | 2 | 12 (11 Conf, 1 Déc) | 7 (4 Conf, 3 Déc) | 6 (toutes Conformité) | 13 (11 Conf, 2 Déc) | 9 (7 Conf, 2 Déc) | **4** (toutes Conformité) |
| Jugements | 10 | 9 | 4 (tous Déc) | 9 (5 Conf, 4 Déc) | 6 (tous Découpabilité) | 5 (tous Découpabilité) | 2 (1 Conf, 1 Déc) | **9** (4 Conf, 5 Déc) |
| Constats hors mandat — justesse | 3 | 1 | 0 | 0 | 0 | 1 | 1 | **0 posés** — 9 items examinés par les deux axes, aucun n'appelant de réponse |
| **Remarques nées d'une figure** | 0 | 0 | 5 | 2 | 4 | 4 | 2 | **2** — une du `sequenceDiagram` (l'annotation *sans état retenu*), une d'une **table** (les points de traversée contre le paragraphe qui l'annonce) ; une troisième s'appuie sur le `flowchart` comme pièce d'appui. ⚠️ **Zéro née du `flowchart`**, première fois depuis le tour 2 |
| Remarques visant une **reprise** du tour précédent | — | 0 | 2 | 4 | non mesurable ce tour | au moins 6 ⚠️ *minorant* | au moins 6 ⚠️ *minorant* | **au moins 6**, mesuré par la présence de `D-064`–`D-066` dans le passage visé ou son chapeau, ou par la réécriture démontrable du passage (le décompte *quinze*) ⚠️ *minorant* |
| Cases de §1 évaluées | 9 | 12 | 12 | 12 | 17 ⚠️ ne se compare pas | 17 ⚠️ ne se compare pas aux tours 1–4 ; se compare au tour 5 | 20 ⚠️ ne se compare pas | **20** ⚠️ **se compare au tour 7 seulement** ; ne se compare pas aux tours 1–6 |
| Cases de §1 enfreintes | 1 | non relevé | 6 | 1 au sens de la clause ; 6 cases portant une divergence | 0 au sens de la clause ; 4 cases portant une divergence ⚠️ ne se compare pas | 2 au sens de la clause ; 5 cases portant une divergence ⚠️ ne se compare pas aux tours 1–4 | 2 au sens de la clause ; 3 cases portant une divergence ⚠️ ne se compare pas | **2** au sens de la clause — *le document ne se contredit pas*, *toute règle issue d'une décision la cite* ; **4 cases** portent au moins une divergence (les deux précédentes, plus *les faits allégués sont vrais* et *les trois registres*). **0 omission silencieuse** ⚠️ **se compare au tour 7 seulement** |
| Cases de §2 | 3, dont 2 en aval | 3 | 3, toutes non opposables depuis l'artefact seul | 3 — 2 tenues, 1 en aval | 3 — 2 tenues sur le fait d'état fourni, 1 en aval | 3 — 2 tenues sur le fait d'état fourni, 1 en aval | 3 — 2 tenues sur le fait d'état fourni, 1 en aval | **3** — 2 **tenues** sur le fait d'état fourni, 1 (l'accord de l'humain) structurellement en aval |
| Carte avant / après | — | — | 36 → 52, 16 ouverts | 52 → 68, 16 ouverts | 68 → 80, 12 ouverts | 80 → 99, 19 ouverts | 99 → 111, 12 ouverts | **111 commentaires, 0 ouvert → 124, 13 ouverts** |
| Étiquette posée | `Rework Needed` | `Rework Needed` | `Rework Needed` | `Rework Needed` | `Rework Needed` | `Rework Needed` | `Rework Needed` | **`Rework Needed`** |

**Les deux lignes « cases de §1 » gagnent un point de comparaison, et un seul.** Neuf, douze, douze,
douze, dix-sept, dix-sept, vingt, **vingt** : pour la seconde fois de la série, deux tours consécutifs
opposent le même référentiel. La mention ⚠️ *ne se compare pas* **se lève vers le tour 7** et **reste
pleine vers les tours 1 à 6**. C'est la troisième fois en trois tours que le gabarit inter-tours doit
exprimer une variation de **portée** — réduite au tour 6, rétablie au tour 7, réduite ici — et la
première où la variation vient d'une **absence** de changement plutôt que d'un changement.

⚠️ **Et le point de comparaison gagné est plus fort que celui du tour 6.** Le tour 6 comparait deux
tours dont le référentiel était identique mais dont le **mandat** réinscrivait le nombre. Ici le
mandat ne le réinscrit pas — il le disqualifie — et le compte est néanmoins **le même**, obtenu deux
fois indépendamment dans le fichier. Les deux valeurs `20` sont donc comparables *et* établies par le
même geste.

**La ligne « écartés ou fusionnés » vaut zéro pour un motif qui n'est pas celui du tour 7, et il faut
l'écrire.** Le tour 7 avait mesuré `0 écarté` **après** avoir confronté au dépôt six assertions
d'axe. Ce tour n'en a confronté **aucune** : l'orchestrateur s'est reposé sur les vérifications que
l'axe Conformité déclare avoir menées lui-même — une vingtaine d'assertions de code, les dix-neuf
renvois `D-0NN`, les cinq renvois `architecture.md` et une dizaine de comptes. **Le zéro de ce tour ne
mesure donc pas un taux d'erreur d'axe** ; il mesure une absence de contrôle indépendant. La
comparaison légitime avec le `2/21` du tour 6 et le `0/13` du tour 7 n'existe pas. C'est une friction
neuve, consignée en rubrique 5.

**La ligne « fusionnés » vaut zéro alors que le cas s'est présenté, et c'est le fait méthodologique du
tour.** Les deux axes ont trouvé **le même passage** — la ligne des connexions tracker du tableau des
points de traversée — avec **exactement la même citation**, au caractère près. Le critère que la fiche
du tour 7 avait dégagé (« citation identique → une remarque, citations distinctes → deux remarques »)
commandait donc une fusion. **Elle n'a pas eu lieu**, et le motif est nommable : les deux constats
n'opposent pas le même référentiel — l'un oppose la case §1 *le document ne se contredit pas* (le
critère d'exclusion énoncé n'est pas appliqué), l'autre oppose le §3 (aucune clause de recette ne
couvre le recablage que cette ligne charge). Les fondre aurait fait porter à une seule remarque deux
axes que `revue` §2 interdit de fondre. **Le critère du tour 7 est donc insuffisant tel qu'il est
écrit** : la citation ne suffit pas à décider, il faut y ajouter le référentiel opposé. Quatrième
point de mesure de la friction 44, et le premier qui **contredit** le critère du tour précédent.

**La série ne converge toujours pas.** Elle est désormais **11, 12, 16, 16, 12, 19, 12, 13**.
⚠️ **Ce tour est le premier où aucune cause instrumentale ne se superpose** : le référentiel n'a pas
bougé, le skill non plus, le primitif non plus. Ce qui a bougé est **l'artefact seul** — une reprise
complète, plus une relecture hors cycle, plus une décision neuve. **Trois causes subsistent donc, et
ce tour n'en isole aucune** : l'artefact a été repris sept fois, il a subi une retouche que personne
n'a relue, et le relecteur est différent. ⚠️ **Aucune conclusion n'est tirée ici** sur ce que
`12 → 13` signifie.

**La part des remarques visant la reprise précédente reste autour de la moitié.** Au moins **6 sur
13**, contre au moins 6 sur 19 au tour 6 et au moins 6 sur 12 au tour 7. Les six : les deux remarques
sur le décompte *quinze* (chiffre né de la reprise du tour 7, qui corrigeait le *douze* faux),
l'invariant `D-063` du §2.3 sans sa généalogie (bullet réécrit par cette même reprise), la question
ouverte nommée avec deux sujets (même paragraphe réécrit), l'incrément irréductible dont la coupe non
mutante n'est pas réfutée (bullet portant `D-062` et la clause de `D-065`), et le geste de remise en
état d'un projet sans base (entrée née de `D-064`). ⚠️ **Le chiffre est un minorant assumé** : le
relecteur n'a pas la version antérieure du document et ne peut pas dater un passage qui ne cite aucune
décision. ⚠️ **Une septième remarque vise le texte le plus récent de tous** — celui de `D-066`, ajouté
hors reprise — sans que sa citation y soit ancrée ; elle n'est pas comptée.

**Ce que la ligne des appels d'outils mesure.** Quarante appels d'orchestration, l'écriture de cette
fiche et ses relectures comprises, zéro appel d'attente et **zéro appel de préparation à l'attente** —
le retour au lancement synchrone referme l'angle mort de la friction 62. La comparaison légitime est
**37 (tour 7) contre 40 (tour 8)**, à charge égale. ⚠️ **Le poste des axes redescend fortement** :
49 → 41 appels, et l'écart entre les deux axes se creuse — 34 pour Conformité, **7** pour
Découpabilité, qui a produit son découpage candidat quasiment sans sortir de l'artefact. ⚠️ **Deux des
quarante sont des appels perdus** : une lecture du document par la CLI, qui ne rend pas l'`updatedAt`
dont la fiche a besoin, et un appel d'API dont la sortie a dépassé la limite de restitution.
⚠️ **Zéro appel de vérification indépendante** — le poste qui valait six aux tours 6 et 7 est vide ce
tour.

**Complété par la session appelante — les deux cellules.** **1 740 s** et **185 088 jetons**, relevés
par les compteurs de la session appelante à la fin du tour.

Les jetons des deux axes, eux, sont relevés par le relecteur : **281 351**, le plus haut de la série
(267 607 au tour 6, 261 641 au tour 7), pour **41** appels seulement. ⚠️ **L'axe Conformité consomme
64 % du total à lui seul** (181 322), signature de la confrontation au dépôt que les trois cases de
cohérence exigent.

**Le tour le plus cher de la série en absolu, et pourtant le premier dont le coût unitaire baisse.**
466 439 jetons au total — relecteur plus axes —, contre 441 299 au tour 7 et 429 574 au tour 6. Mais
rapporté aux remarques posées : **35 880 jetons par remarque, contre 36 775 au tour 7**. C'est la
première baisse depuis que la ligne existe, et elle est mince — 2 %. Le tour ne renverse pas la
tendance, il l'arrête.

⚠️ **La durée par remarque, elle, ne bouge pas du tout** : **133,8 s**, contre 133,25 s au tour 7. Deux
tours de suite au même chiffre à la demi-seconde près, sur des récoltes de tailles différentes (12 et
13) et des durées différentes (1 599 s et 1 740 s). Le rapprochement est trop étroit pour deux points ;
on ne sait pas s'il tient une régularité ou une coïncidence, et il faudra un tour 9 pour le dire.
Aucune conclusion n'en est tirée ici.

⚠️ **L'écart entre les deux compteurs d'appels se rouvre.** Le relecteur relève **40** appels
d'orchestration, la session appelante en compte **42** — deux de plus, contre un seul d'écart au tour
7. Le poste des axes recule fortement à charge égale (49 → 41) tout en consommant **plus** de jetons
(261 641 → 281 351) : moins d'appels, plus lourds, l'inverse exact du mouvement du tour 7.

## 3. Conformité au protocole

Clause par clause, avec ce qui l'atteste.

| Clause | Tenue | Ce qui l'atteste |
|---|---|---|
| Session neuve, sur l'artefact seul (`revue-spec` §1, `D-039`) | **oui, sous quatre réserves nommées** | Le fil de rédaction n'a pas été transmis ; **les 111 commentaires soldés n'ont pas été ouverts pour instruire la revue** — le fait d'état venant de l'appelant. ⚠️ Réserve 1 : un **extrait de la mémoire automatique a été présenté d'office** dans le contexte de session. Le fichier n'a **jamais été ouvert**, l'extrait est traité comme non lu et **n'a pas été transmis aux axes** ; il portait néanmoins les attentes du binôme sur ce tour-ci (friction 43, sixième occurrence, **aggravée** — détail en rubrique 5). ⚠️ Réserve 2 : le fait d'état chiffré vient de l'appelant et reste un résumé de l'issue des tours précédents (friction 50). ⚠️ Réserve 3 : **vérifier ce fait d'état a exposé le contenu de la carte** — le listage des fils a rendu le corps de deux remarques d'époque `Discovery`, lues incidemment (friction 60, seconde occurrence). Survenu **après** que les deux axes ont rendu leurs rapports et après la lecture intégrale de l'artefact ; aucun constat n'en dépend. ⚠️ Réserve 4 : l'appelant inscrit lui-même ce que l'artefact a subi depuis le tour 7. **Aucune des quatre n'a été transmise aux axes**, qui n'ont reçu que l'artefact, les référentiels et le fait d'état chiffré |
| Exactement deux axes, jamais fondus (`revue-spec` §2, `revue` §2) | **oui, et le tour l'a éprouvé** | Deux sous-agents lancés dans le même message, chacun ignorant l'existence de l'autre ; deux rapports reçus distinctement, chacun se clôturant sur son propre verdict d'axe ; aucun rapport de synthèse qui reclasse. ⚠️ **Le cas limite s'est présenté** : les deux axes ont produit la **même citation au caractère près**. Deux remarques distinctes ont été posées, chacune nommant son axe et son référentiel en tête — **aucune remarque de ce tour ne porte deux axes** |
| Les cases de §1 et les trois de §2, clause par clause | **oui, et c'est le tour où la clause tourne seule** | L'axe Conformité rend deux tableaux de couverture — **vingt** lignes puis trois — avant sa liste de constats, et nomme pour chaque case son verdict. ⚠️ **Le mandat ne réinscrivait pas le nombre** : il disqualifiait explicitement ceux des fiches antérieures et ordonnait le décompte dans le fichier. Le décompte a été fait **deux fois indépendamment** — orchestrateur par comptage exact sur le fichier, axe Conformité par lecture — et donne **vingt** les deux fois, sans écart avec ce qu'annonce la fiche du tour 7 |
| Deux citations par constat (`revue` §3) | **oui** | Chacune des treize remarques porte son référentiel (fichier + clause, ou le second passage de l'artefact quand la contradiction est interne) et l'extrait visé, côte à côte. Quatre sont des contradictions internes, où les deux pièces sont deux passages du même document |
| **Confronter chaque figure à la prose** (`revue` §3, `revue-spec` §2) | **oui** | Le mandat des **deux** axes nommait les figures — les deux blocs `mermaid`, les tables, le bloc `gherkin` — et demandait de les confronter au texte qui les entoure, « une figure n'illustre pas, elle **affirme** ». Le mandat de Conformité allait plus loin en exigeant la confrontation **nœud par nœud, arête par arête et couleur par couleur** du `flowchart` contre l'énumération de la prose. **Deux des treize remarques en sortent** : l'annotation *à chaque appel, sans état retenu* du `sequenceDiagram`, qui nie la garde de la racine que trois passages de prose posent ; et la table des points de traversée, dont la ligne des connexions tracker enfreint le critère d'exclusion que le paragraphe d'annonce énonce. Une troisième s'appuie sur le `flowchart` comme pièce d'appui. ⚠️ **Le `flowchart` n'a produit aucune remarque en propre**, pour la première fois depuis le tour 2 — il avait produit 5, 2, 4, 4 et 2 remarques aux tours 3 à 7 |
| Écarter la justesse (`revue-spec` §3) | **oui, avec matière** | Les deux axes ont rendu leur section *hors mandat — justesse* : **six items** côté Conformité (le projet dédié, la maille du verrou, l'arbitrage de la base disparue, l'exclusion du secret, le renoncement à stdio, le dimensionnement de l'incrément irréductible), **trois** côté Découpabilité (l'inférence non mesurée de la dernière clause du scénario 4, l'écart du rapport de validation entre les deux portes, le coût usager de la base disparue). **Aucun n'a été retenu**, chacun échouant au test de `revue` §6 — *est-ce que quelqu'un doit répondre ?* Le plus proche du seuil, l'arbitrage de la base disparue, **est le hors-mandat du tour 7** : il a reçu sa réponse, `D-064`, que la spec porte désormais avec son coût explicite. Aucune ligne des deux rapports d'axe ne discute si la spec est un bon choix |
| Étiqueter la confiance (`revue` §5) | **oui** | 4 *violation dure*, 9 *jugement*, aucune ligne ambiguë. ⚠️ **Le partage coïncide cette fois avec les axes**, contrairement aux tours 6 et 7 : les quatre violations dures sont toutes sur Conformité, et les cinq constats de Découpabilité sont tous des jugements — ce que la DoD §3 annonce (« ce sont par nature des **jugements** »). Le mandat autorisait pourtant explicitement l'exception pour une contradiction interne ; elle n'a pas servi |
| Lister sans réécrire (`revue` §6) | **oui** | `updatedAt` du document relu après la pose : `2026-08-02T11:56:24.614Z`, inchangé et **antérieur à la première remarque**. `git status` ne montre aucun fichier du dépôt modifié par la revue. Aucune remarque ne propose un texte de remplacement ; les cinq de Découpabilité posent **la question que le découpage devrait revenir poser**, elles n'y répondent pas. Les mandats des deux axes leur interdisaient explicitement d'écrire un fichier ou de poser une remarque |
| Deux issues, jamais trois (`revue` §6, `D-051`) | **oui** | 13 constats produits, **13 posés, 0 écarté, 0 fusionné**. Aucun constat n'est écrit ailleurs que sur la carte ; aucune « observation non bloquante » ; les neuf items hors mandat ont été **écartés faute de réponse à appeler**, jamais consignés comme notes. Les deux axes ont reçu la clause dans leur mandat, avec le test *est-ce que quelqu'un doit répondre ?* ⚠️ **Réserve** : le `0 écarté` ne vaut pas taux d'erreur, aucune assertion d'axe n'ayant été confrontée au dépôt par l'orchestrateur |
| Poser la remarque sur la carte (`revue` §6, `D-045`) | **oui, sans rappel** | 13 remarques posées par `cursus linear comment add`, ancrées avec leur repère calculé ; `open` passe de 0 à 13, `total` de 111 à 124. Aucune sur le document |
| Poser l'étiquette, jamais déplacer (`revue-spec` §4, `revue` §8) | **oui** | `Rework Needed` seul sur le projet, `Review Requested` retiré ; `status` reste `Spec` |
| Pas d'escalade, pas de compteur de tours (`revue-spec` §4) | **oui** | Aucune assignation, aucune tentative de convergence au-delà de ce passage, alors même que c'est le huitième |

**La clause anti-copie a tourné seule, et c'est ce que deux tours attendaient.**

- **`revue-spec` §2 interdit de compter les cases ailleurs que dans le référentiel.** Les tours 6 et
  7 l'avaient éprouvée avec un mandat qui **réinscrivait le nombre** — la clause n'avait donc jamais
  été la seule source. Ce tour est le premier où l'appelant **retire** le nombre, et va plus loin en
  disqualifiant ceux des fiches antérieures. Le décompte a été fait dans le fichier, deux fois, et
  donne vingt.
- ⚠️ **Ce que l'essai établit, et ce qu'il n'établit pas.** Il établit que la clause suffit **quand le
  référentiel n'a pas bougé** : le compte obtenu sans aide est le bon. Il n'établit **pas** qu'elle
  suffirait sur un référentiel qui aurait changé depuis la dernière fiche — le cas du tour 7, où
  l'écart aurait été détectable. Ce tour n'avait aucun écart à détecter, et il ne pouvait pas en
  fabriquer un.

**Une clause reste non tenable telle qu'écrite, pour la cinquième fois.**

- **`revue` §8 dit toujours « `Done` ou `Rework Needed` »** là où `revue-spec` §4 dit
  `Human Review Requested`. Sans effet ici — treize remarques imposent `Rework Needed` des deux
  côtés —, l'écart est **intact**, cinquième occurrence sans conséquence observée. Le cas où il mord
  reste le tour sans aucune remarque, qui ne s'est toujours pas présenté en huit tours. Friction 48.

**Et la contradiction du protocole est atteinte pour la sixième fois — cette fois elle infirme le
critère du tour précédent.** `revue` §2 interdit de fondre les axes, `revue` §6 veut une remarque par
constat. Les deux axes ont trouvé **un** passage commun, avec une citation **identique au caractère
près**. Le tour 7 en avait déduit le critère *citation identique → fusion*. Ce tour **ne fusionne
pas**, parce que les deux constats opposent deux référentiels différents et que les fondre aurait
fondu les axes. La friction 44 gagne son quatrième point de mesure, et le critère dégagé au tour 7 est
montré **incomplet** : la citation ne décide pas seule.

## 4. Qualité de la sortie

> **Complété par le binôme.** Le relecteur ne juge pas sa propre sortie (`D-039`) ; la matière brute
> qu'il a laissée suit le jugement, inchangée.

### Le jugement

**Les deux remarques les plus lourdes sont vraies, et l'une d'elles est accablante pour la reprise
précédente, pas pour l'artefact.** La figure qui annote « sans état retenu » une flèche dont trois
passages de prose disent qu'elle *garde* est un défaut réel et vérifiable en une lecture ; la garde
est ce qui tient la dernière clause du scénario 4. Quant au critère d'exclusion que sa propre table
enfreint : **le chiffre *quinze* est celui que la reprise du tour 7 avait posé pour corriger un
*douze* faux**, et il est faux à son tour, d'une autre façon. Deux corrections successives sur le même
agrégat, deux erreurs. C'est la friction 63 qui se paie une seconde fois, et elle démontre son propre
motif — un total ne se recontrôle pas, il se recopie.

**La pièce la plus utile du tour n'est pas une remarque.** L'axe Découpabilité a produit un
**découpage candidat complet** — neuf incréments, frontières, recette, rattachement de chaque
scénario, et les 31 lignes atteignables de l'inventaire réparties sans reste. C'est la première fois
qu'un axe rend un **instrument réutilisable** plutôt qu'une liste de manques, et c'est aussi ce qui
rend ses cinq achoppements crédibles : ils sont *dérivés* d'une tentative, pas énoncés d'avance. Cette
sortie survivra au tour qui l'a produite, ce qu'aucune remarque ne fait.

**Le « 0 écarté » ne vaut rien ce tour, et il faut le dire net.** Aucune assertion d'axe n'a été
confrontée au dépôt, là où les tours 6 et 7 en vérifiaient une demi-douzaine. Le chiffre est
identique et ne mesure pas la même chose — c'est exactement la friction 65, relevée par le relecteur
sur lui-même, ce qui est à son crédit.

⚠️ **Le fait qui pèse le plus lourd ne concerne pas le relecteur mais le binôme** : **98 remarques
retenues sur 98**, jamais un refus motivé. Sept tours durant, chaque objection a été traitée comme un
fait. Cela retire toute portée à la qualité apparente de la récolte — on ne peut pas distinguer une
remarque fondée d'une remarque simplement acceptée —, et cela explique une part de la
non-convergence : chaque reprise ouvrait du terrain neuf, et le tour suivant y trouvait de quoi
écrire. **Le défaut est du côté qui répond, pas du côté qui pose.**

**Ce que le tour vaut, replacé dans la série.** Le partage s'inverse — 4 violations dures contre 9 au
tour 7 — et la veine que `D-060` avait ouverte paraît s'épuiser. L'analyse de série écrite le même
jour (`2026-08-02-analyse-serie-revue-spec.md`) situe ce résultat : sur les 111 remarques de spec,
**une seule** relève de l'architecture, et **68 sont invisibles au code**. Ce tour ne fait pas
exception. Il est bien exécuté, ses remarques sont vraies, et **il trouve majoritairement des choses
que le code n'aurait jamais signalées** — ce qui est un jugement sur le dispositif, pas sur le
relecteur.

### La matière brute laissée par le relecteur

**Les faits bruts.**

*Le sort des treize remarques n'est pas connu au moment d'écrire.* Les sept tours précédents affichent
**98 retenues sur 98**, aucun refus motivé.

*Quatre violations dures*, toutes sur l'axe Conformité, et toutes des contradictions internes :

- **une figure qui nie un mécanisme** : le `sequenceDiagram` annote la flèche de résolution « à chaque
  appel, sans état retenu », quand le §1.3, le §3.1 (`D-057`) et le §2.2 posent que la racine
  « construit à la première résolution et **garde** », et que cette garde est ce qui empêche la
  dernière clause du scénario 4 de tomber ;
- **une question ouverte nommée avec deux sujets incompatibles** : le §2.3 renvoie à « *jusqu'où les
  lectures passeront par les requêtes* (§3.1) », quand le §3.1 porte « *Jusqu'où les ViewModels
  passent par les commandes* ». Le renvoi ne résout pas, et l'entrée du §3.1 se referme dans sa propre
  glose ;
- **un critère d'exclusion que la table qu'il annonce enfreint** : le §2.3 exclut « les deux écritures
  au trousseau, la saisie d'un secret étant hors parité », puis compte trois points pour les
  connexions tracker, dont deux appartiennent au geste que le §1.2 marque ⛔ comme l'unique exception à
  la parité. **Le chiffre *quinze* en dépend** — treize si le critère s'applique ;
- **deux invariants énoncés sans citer la décision qui les a tranchés** : le troisième invariant du
  §2.3 est `D-063` mot pour mot et ne le cite pas, alors que la décision est citée en §3.1 et §3.3 —
  partout **sauf** dans la section dont le remaniement ferait disparaître la règle ; le premier porte
  la portée de `D-056` en ne citant que `D-055`.

*Neuf jugements*, quatre sur Conformité et cinq sur Découpabilité :

- Conformité — « cinq issues » de l'écran des Tâches contre **quatre** documentées deux fois au code ;
  le §1.1 qui attribue un outil par ligne quand le §3.1 range la forme du catalogue en question
  ouverte et que le §3.3 tranche « un outil qui couvre trois lignes en solde trois » ; une douzaine de
  renvois `(§2.2)` posés **au milieu du §2.2**, qui en porte six intertitres, et quatre renvois `(§3)`
  visant en réalité le §3.1 ; `CUR-32` rangé en *tranché* tout en annonçant qu'il « garde sa question »,
  laquelle n'est versée à aucun registre ;
- Découpabilité — le §2.3 qui ne réfute qu'**une** coupe de l'incrément irréductible et laisse ouverte
  la seule qui déplace les frontières (un incrément **en lecture seule**, observable et recettable,
  qui hériterait de la descente du socle et créerait la base par une *requête*) ; l'énumération de ce
  que porte cet incrément, qui **omet la porte** — hôte HTTP, adaptateur MCP, activation, jeton — sans
  laquelle aucune de ses lignes n'est atteignable, laissant deux dimensionnements incompatibles ; les
  **quinze points de traversée** qui changent la fenêtre sans qu'aucune clause de l'annexe B ne les
  recette, cas littéral sur les connexions tracker dont deux points n'auront **jamais** de ligne
  d'inventaire ; la question ouverte du geste de remise en état, dont l'une des deux issues **ajoute
  une ligne à l'inventaire** — donc au référentiel opposable de la parité — ce qu'un plan de design ne
  peut pas faire ; le registre des runs en vol, attribué à l'incrément de l'arrêt alors qu'aucun
  lancement ne l'alimenterait.

*Aucun constat hors mandat — justesse posé.* Neuf items ont été examinés par les deux axes et aucun
n'a été retenu. Le plus proche du seuil est celui que le tour 7 avait posé — l'arbitrage de la base
disparue — désormais tranché par `D-064` et porté par la spec avec son coût usager explicite.

*Aucun constat écarté pour fausseté, et aucune vérification indépendante.* **13 produits, 13 posés, 0
fusionné, 0 écarté.** ⚠️ **Un tiers doit peser ce que cela vaut** : contrairement aux tours 6 et 7,
l'orchestrateur **n'a confronté au dépôt aucune assertion d'axe**. Le chiffre repose entièrement sur
les vérifications que l'axe Conformité déclare avoir faites — une vingtaine d'assertions de code, les
dix-neuf renvois `D-0NN`, cinq renvois `architecture.md`, une dizaine de comptes — et qu'il donne
toutes pour concluantes, quatre exceptions près qui sont devenues des remarques.

*Les deux verdicts d'axe*, rendus séparément :

- **Conformité — désaccord.** « Quatre violations dures subsistent : une figure qui nie la garde de la
  racine, une question ouverte nommée avec deux sujets incompatibles derrière un renvoi qui ne la
  porte pas, un critère d'exclusion que le tableau enfreint et dont dépend le chiffre quinze, et deux
  invariants énoncés sans citer la décision qui les a tranchés. »
- **Découpabilité — désaccord.** « Le découpage bute deux fois — il ne peut savoir ni si un incrément
  non mutant a le droit de précéder l'irréductible, ni si l'irréductible contient la porte sans
  laquelle ses propres lignes ne sont atteignables — et il ne peut pas écrire l'acceptation du
  recablage de la fenêtre, que quinze points de traversée chargent et qu'aucune clause de recette ne
  couvre. »

*Ce que l'axe Découpabilité a produit comme instrument* : un découpage candidat complet — **neuf
incréments**, leurs frontières, ce que chacun livre d'observable, leur recette, et le rattachement de
chaque scénario Gherkin. **Les 31 lignes atteignables de l'inventaire s'y répartissent sans reste**
(32 lignes relevées, dont une ⛔). Les cinq achoppements en sont dérivés, et non énoncés d'avance.
C'est la pièce qui permet à un tiers de vérifier que la tentative a eu lieu, comme le demande §3 de la
DoD (« il se **teste** »). L'axe rapporte aussi ce qu'il a éprouvé **sans** retenir de constat : la
répartition clause par clause du scénario 4, l'atterrissage des scénarios 7 et 8, la liberté du
catalogue, l'écart du §1.3 sur le renommage d'une étape, et la branche `feature/`.

*Un fait que le tiers doit connaître avant de juger* : **rien de ce que le relecteur applique n'a
bougé** — référentiel, skill et primitif sont ceux du tour 7. Ce qui a bougé est **l'artefact seul** :
une reprise complète des treize remarques du tour 7, **puis** une passe de relecture menée hors du
cycle par un relecteur unique et non isolé, **puis** l'ajout de `D-066`. Le nombre de remarques passe
de 12 à 13. ⚠️ **C'est le premier tour de la série où aucune cause instrumentale ne se superpose à la
variation** — mais trois causes non instrumentales subsistent, et ce tour n'en isole aucune.

*Un second fait, sur la nature de ce que la revue a trouvé* : **le partage violations dures /
jugements s'est inversé** — 9 VD et 2 jugements au tour 7, **4 VD et 9 jugements** ici. La proportion
de contradictions internes chute de 9 sur 12 à **4 sur 13**. Un tiers doit dire si cela tient à
l'artefact, au calibrage du relecteur, ou à l'épuisement d'une veine que `D-060` avait ouverte.

*Un troisième fait, sur les figures* : le `flowchart` de composition **n'a produit aucune remarque en
propre**, pour la première fois depuis le tour 2 ; il en avait produit 5, 2, 4, 4 et 2 aux tours 3 à
7. Le tour 7 lui avait consacré deux remarques (le libellé du cadre `ouvert`, le compte des arêtes du
cadre `fen`), toutes deux reprises. Les deux remarques de figure de ce tour naissent du
`sequenceDiagram` et d'une **table**.

*Un quatrième fait, sur la part visant la reprise* : **au moins 6 sur 13**, après au moins 6 sur 19 et
au moins 6 sur 12. Le numérateur est stable à travers trois tours pendant que le dénominateur varie.

*Ce que le dispositif n'a pas produit, et qui reste le mode d'échec nommé aux tours 3 à 7* : aucune
des treize remarques ne demande si une section devait exister. Elles chicanent toutes **dans** le
cadre de l'artefact.

## 5. Frictions

Journal des frictions, entrées **43** (la mémoire automatique dément la clause de session neuve —
**sixième occurrence, et aggravée** : le mandat interdisait explicitement d'ouvrir tout fichier de
mémoire, et le fichier n'a **jamais été ouvert** ; un **extrait a néanmoins été présenté d'office**
dans le contexte de session, portant les attentes du binôme sur ce tour-ci — dont le fait que le
mandat devait retirer le nombre de cases, et le signal que le binôme comptait surveiller. L'extrait
est traité comme non lu, n'a pas été transmis aux axes, et **ne portait pas le nombre de cases** — la
mesure de la clause anti-copie y survit. ⚠️ **Ce que l'occurrence apprend est neuf** : une interdiction
d'ouvrir ne protège pas d'une présentation d'office, et la clause de session neuve n'a pas de doctrine
pour ce cas), **44** (deux axes sur le même passage, sans règle — **quatrième point de mesure**, et le
premier qui **infirme** le critère du tour 7 : citation identique au caractère près, **non fusionnée**,
parce que les deux constats opposent deux référentiels distincts), **48** (corriger une instance laisse
le primitif porter l'ancienne clause — **cinquième occurrence**, `revue` §8 dit toujours `Done`),
**50** (la clause de session neuve n'a pas de doctrine sur les faits d'état — **cinquième occurrence**,
sans variation : le fait est venu de l'appelant), **60** (compter les remarques d'une carte en expose
le contenu — **seconde occurrence, et le remède proposé au tour 7 ne l'aurait pas évitée** : ce n'est
pas l'attestation du mouvement de la carte qui a déclenché le listage, c'est la **vérification du fait
d'état fourni**, geste que la clause de session neuve rend au contraire souhaitable. Sans effet sur
les constats, tous produits par les axes avant ce listage).

**Trois entrées sont sans occurrence ce tour, et c'est mesuré** : **45** (la pièce la plus contestable
est la moins citable) — treize poses, aucune n'a buté ; **49** et **62** (l'attente d'un sous-agent
coûte des appels, et la métrique a un angle mort) — **zéro appel d'attente et zéro appel de
préparation**, le retour au lancement synchrone refermant l'angle mort ; **55** (la citation
d'ancrage bute sur les marques d'emphase) — **treize poses, treize acceptées du premier coup**, et
⚠️ **deux des citations contenaient délibérément des marques d'emphase** (`**cinq**`, `**Le document
ne se contredit pas**` dans le corps), sans que l'ancrage bute. C'est la première fois que la friction
est éprouvée plutôt que contournée.

**Une entrée est levée par le mandat plutôt que par l'outillage** : **61** (toute la CLI exige d'être
appelée depuis la racine du dépôt) — **zéro occurrence**, l'appelant l'ayant écrit dans le prompt.
⚠️ **La friction n'est pas corrigée, elle est portée par le prompt** : elle se repaiera au premier
mandat qui omettra l'avertissement.

**Deux frictions neuves**, numérotées depuis au journal — **64** et **65** — et **non recopiées ici** :

- **« zéro constat écarté » ne distingue pas un tour vérifié d'un tour non vérifié.** Les tours 6 et 7
  confrontaient au dépôt une demi-douzaine d'assertions d'axe avant de poser, et la ligne mesurait un
  taux d'erreur. Ce tour n'a confronté aucune assertion et rend le **même chiffre**. ⚠️ Le point n'est
  pas la vérification manquante — le protocole ne la prescrit nulle part, c'est un geste que les tours
  6 et 7 ont ajouté d'eux-mêmes — c'est que **la ligne du tableau ne dit pas si elle a eu lieu**. Deux
  dispositifs de fiabilité différente rendent une valeur identique, et la fiche doit l'écrire en prose
  à chaque tour pour que le chiffre veuille dire quelque chose. Même famille que la friction 62.
- **Le listage des remarques rend un tableau dont le décompte brut contredit ses propres compteurs.**
  L'outil rend `total: 111` et `open: 0` en tête, mais un tableau de **222** entrées dont **111** ont
  `resolved: false` — les réponses en fil, qui ne portent pas la résolution de leur racine. Un
  décompte naïf sur le tableau contredit donc frontalement le fait d'état du mandat, et il a fallu
  deux appels pour établir que les compteurs de l'outil avaient raison. ⚠️ **Le coût réel est
  ailleurs** : le relecteur a failli consigner une divergence sur l'état de la carte — c'est-à-dire
  ouvrir un litige sur le seul fait qu'il ne peut pas établir lui-même.

**Sur le décompte des cases, demandé explicitement par le mandat** : le compte fait dans le fichier
donne **vingt** cases en §1 et **trois** en §2. **Aucun écart** avec ce qu'annonce la fiche du tour 7.
Le décompte a été refait indépendamment par l'axe Conformité, qui donne les mêmes valeurs.

**Sur l'extrait de mémoire présenté d'office** : déclaré ici comme le mandat l'exige. Aucun fichier de
mémoire n'a été ouvert, ni par l'orchestrateur ni par les axes — dont les mandats portaient la même
interdiction, étendue à `docs/methode/rex/` et `journal-frictions.md` pour éviter qu'ils lisent les
attentes des tours précédents. L'orchestrateur, lui, a lu `rex/README.md`, la fiche du tour 7 et le
journal des frictions **après** la pose de l'étiquette, pour écrire cette fiche.

## 6. Ce que le tour a changé

- **La clause anti-copie de `revue-spec` §2 a tourné sans filet, et c'est le remède que le tour 7
  réclamait.** Les tours 6 et 7 avaient tous deux buté sur le même point : le mandat réinscrivait le
  nombre de cases, donc rien n'établissait que la clause aurait suffi. Ce tour retire le nombre et va
  plus loin — il disqualifie ceux des fiches. Le compte obtenu dans le fichier est le bon, et il l'est
  deux fois. ⚠️ **Ce que l'essai ne peut pas établir** : que la clause détecterait un référentiel qui
  aurait bougé. Le référentiel n'a pas bougé ce tour ; il n'y avait aucun écart à trouver.
- **La mention ⚠️ *ne se compare pas* a varié de portée pour le troisième tour consécutif**, et pour la
  première fois **parce que rien n'a bougé**. Réduite au tour 6, rétablie au tour 7, réduite ici — au
  tour 7 seulement. Le gabarit inter-tours sait désormais exprimer une portée qui suit l'état du
  référentiel plutôt que son changement.
- **Le critère de fusion dégagé au tour 7 est infirmé au tour suivant.** « Citation identique → une
  remarque » commandait une fusion ce tour ; elle n'a pas eu lieu, parce que fondre aurait fondu les
  axes. Le critère utile n'est donc pas la citation mais **le référentiel opposé** : deux constats qui
  citent le même passage contre deux référentiels différents sont deux remarques. Ce n'est pas une
  décision — aucune n'a été prise ici —, c'est le premier matériau qui **corrige** le critère au lieu
  de l'accumuler.
- **La ligne « écartés ou fusionnés » est prise en défaut une troisième fois**, et pour un troisième
  motif : après avoir mesuré du bruit (tours 3–5), de l'erreur (tour 6), puis du bruit de nouveau
  (tour 7), elle rend ici un zéro qui ne mesure **rien du tout**. Trois signes en trois tours que
  cette ligne mesure mal.
- **Le retour au lancement synchrone referme l'angle mort de la friction 62** — zéro appel d'attente
  **et** zéro appel de préparation. Le dossier dispose maintenant des deux dispositifs mesurés dans
  les mêmes termes : arrière-plan au tour 7 (0 d'attente, 1 de préparation), synchrone ici (0 et 0).
- **La friction 55 est éprouvée pour la première fois plutôt que contournée** : deux citations
  d'ancrage portaient des marques d'emphase, et les treize poses ont été acceptées du premier coup.
- **Le gabarit inter-tours a tenu une cinquième fois**, prolongé d'une huitième colonne, sans qu'une
  ligne ait dû être créée ou supprimée.
- **La spec, elle, n'a pas changé** : la revue liste, elle ne réécrit pas. Treize remarques ouvertes
  attendent la reprise, et l'étiquette `Rework Needed` dit de ne pas tirer.
- **Rien n'a été changé dans les skills ni dans les DoD par ce tour.** Les deux frictions neuves visent
  la **métrique** de la fiche et l'**outillage** de listage — aucune ne vise le geste de `revue-spec`.
  **Cinquième tour consécutif.**

## 7. Verdict pour `revue-spec`

> **Complété par le binôme.** Les quatre issues de `D-043` amendé — *promu*, *corrigé par le
> journal*, *retiré*, *tué par un fait* — supposent un juge qui n'est pas l'exécutant. La matière
> brute laissée par le relecteur suit le verdict, inchangée.

## **Verdict : promu, confirmé** — et c'est la dernière fois que ce verdict apprend quelque chose

`revue-spec` a fait ce qu'il prescrit, sur un tour où **rien de ce qu'il applique n'avait bougé**.
Le mandat a enfin retiré le nombre de cases, et la clause a rendu le bon compte seule.

**La friction 54 est close, et il faut deux tours pour le dire.** Le relecteur objecte à juste titre
que son essai ne montre pas qu'un écart serait détecté — le référentiel n'avait pas bougé. Mais c'est
le tour 7 qui portait cette moitié-là : le référentiel avait grossi la veille, la clause l'a suivi, et
les deux seules violations du tour tombaient sur des cases neuves. Les deux essais couvrent donc les
deux cas — **détecter un changement** au tour 7, **ne pas fabriquer d'erreur** au tour 8 —, et le
second n'était concluant qu'à condition que le mandat se taise, ce qu'il a fait. La friction est
refermée en pratique et sur les deux faces.

⚠️ **Ce que le verdict ne couvre plus, et c'est désormais tout le sujet.** **Cinquième tour
consécutif** où aucune friction neuve ne vise le geste du skill : elles visent l'injection de
contexte (64) et une ligne de tableau (65). Le tour 7 avait nommé le mode d'échec — *« le dispositif
ne l'éprouve plus que sur le même artefact, écrit par la même main »*. Ce tour le confirme et
l'aggrave, parce qu'il ajoute une mesure que les précédents n'avaient pas : sur les 111 remarques de
la série, **une seule** relève de l'architecture et **68 sont invisibles au code**
(`2026-08-02-analyse-serie-revue-spec.md`).

**D'où une issue que `D-043` n'a pas prévue, et qui n'appartient pas à ce dossier.** Le skill n'est ni
à corriger, ni à retirer, ni tué par un fait : **il est bon, et il est bien employé, et son emploi ne
paie plus.** Un tel constat ne se juge pas au niveau du skill — un instrument ne peut pas dire qu'on
l'utilise trop. La question a changé d'échelle, et ce qui s'ensuit se décide dans `decisions.md`, pas
ici.

**Le fait qui commande cette lecture, et qui ne vise pas le relecteur** : **98 remarques retenues sur
98 en sept tours, aucun refus motivé jamais enregistré.** Tant que la partie qui répond accepte tout,
la partie qui pose ne peut pas être évaluée — et la boucle ne peut pas converger, puisque chaque
reprise écrit du texte neuf que le tour suivant récolte. Le numérateur le dit sans ambiguïté : **6, 6,
6** remarques nées de la reprise, trois tours de suite, sur des récoltes de 19, 12 et 13.

### La matière brute laissée par le relecteur

**Les faits bruts.**

- **Huit tours, 111 remarques posées** (11 + 12 + 16 + 16 + 12 + 19 + 12 + 13). Les sept premiers
  affichent **98 retenues sur 98**, aucun refus motivé. **Le sort des 13 de ce tour n'est pas connu**
  au moment d'écrire.
- **C'est le premier tour où ni la règle ni l'instrument ne bougent.** Référentiel, skill et primitif
  sont ceux du tour 7. Les douze clauses du §3 ci-dessus ont chacune leur pièce.
- **La question ouverte aux tours 6 et 7 est refermée à moitié.** Le mandat a retiré le nombre de
  cases, comme la fiche du tour 7 le demandait, et la clause a rendu le bon compte seule. Mais le
  référentiel n'ayant pas bougé, **l'essai ne montre pas que la clause détecterait un écart** — il
  montre qu'elle ne fabrique pas d'erreur quand il n'y en a pas. Un tiers doit dire si cela suffit à
  clore la friction 54.
- **La réserve sur l'étiquette reste levée côté instance et intacte côté primitif** : `revue-spec` §4
  dit `Human Review Requested`, `revue` §8 dit toujours `Done`. Le cas où l'écart mord — un tour sans
  aucune remarque — ne s'est pas présenté en huit tours.
- **La série ne converge pas** : 11, 12, 16, 16, 12, 19, 12, **13**. ⚠️ **Ce tour est le premier sans
  cause instrumentale** — rien de ce que le relecteur applique n'a bougé. Trois causes non
  instrumentales subsistent, qu'aucune mesure de ce tour ne sépare : un artefact repris sept fois, une
  **retouche hors cycle** qu'aucun tour n'a relue, et un relecteur différent.
- **Le motif mécanique nommé au tour 6 n'est pas infirmé** : chaque reprise produit du texte que
  personne n'a relu, donc une récolte pour le tour suivant. **Au moins 6 remarques sur 13** visent le
  texte né de la reprise du tour 7 (`D-064`, `D-065`, et le décompte *quinze* qui corrigeait le
  *douze* faux). Le numérateur vaut **6 pour le troisième tour consécutif**, sur des dénominateurs de
  19, 12 puis 13. Un tiers doit dire ce que la stabilité de ce numérateur signifie.
- **Un fait neuf pour ce motif** : une remarque vise le passage introduit **hors reprise** — la
  décision d'authentification —, c'est-à-dire du texte que ni la reprise ni aucun tour n'a produit.
  La passe de relecture hors cycle n'a pas non plus empêché les quatre violations dures.
- **Le taux d'erreur d'axe n'est pas mesuré ce tour.** 0 constat écarté sur 13, mais **aucune
  confrontation indépendante au dépôt** par l'orchestrateur, contre six aux tours 6 et 7. Le dossier a
  donc deux points mesurés (2/21, puis 0/13) et un troisième qui n'en est pas un.
- **Le partage violations dures / jugements s'est inversé** : 4 VD et 9 jugements, contre 9 et 2 au
  tour 7. Il **coïncide en revanche avec les axes** pour la première fois depuis le tour 5 — toutes
  les VD sur Conformité, tous les jugements de Découpabilité étiquetés comme tels, ce que la DoD §3
  annonce. Reste à dire si c'est un calibrage de relecteur, une propriété de l'artefact, ou
  l'épuisement de la veine que les trois cases de `D-060` avaient ouverte au tour 7.
- **Le `flowchart` n'a produit aucune remarque en propre**, première fois depuis le tour 2, après
  5, 2, 4, 4 et 2. Ses deux défauts du tour 7 avaient été repris. Les deux remarques de figure de ce
  tour viennent du `sequenceDiagram` et d'une table.
- **La condition « le relecteur sait qu'il est testé » est dans le même état qu'aux tours 6 et 7** —
  atténuée sans être levée, et rien ne la quantifie. ⚠️ **Un élément neuf la dégrade** : un extrait de
  mémoire présenté d'office portait les attentes du binôme sur ce tour-ci. Il ne portait pas le nombre
  de cases, ce qui préserve la mesure principale ; un tiers doit dire ce qu'il fait au reste.
- **Les deux frictions neuves ne sont pas dans le geste de `revue-spec`** — elles visent la métrique de
  la fiche et l'outillage de listage. C'est la **cinquième fois consécutive** que les frictions neuves
  d'un tour se logent au-dessus ou au-dessous du skill éprouvé, jamais dedans.
- **Ce que ce tour n'établit toujours pas** : rien du cycle au-delà du temps ②. Ce qui suit
  `Rework Needed` est la reprise par le binôme, et elle n'a jamais été jouée par un skill — alors même
  que ce tour ajoute un troisième régime non outillé, la **relecture hors cycle**, dont l'effet n'est
  mesuré nulle part.
- **La réserve qui ne se lève pas** : huit tours sur **le même artefact**, écrit par le binôme, avec un
  skill de la même main. Le tour utile suivant reste celui que les tours 4 à 7 nommaient — un premier
  passage de `revue-spec` sur une spec que le binôme n'aura pas rédigée.
