# 2026-08-01 — `revue-spec`, sixième exécution

> Sixième tour sur le même artefact, *Un agent pilote Cursus*, le même jour que le cinquième. Une
> différence domine, et c'est elle que le tour mesure : **le référentiel est inchangé** — dix-sept
> cases en §1, trois en §2 — tandis que **le skill a été corrigé**, et corrigé précisément sur ce
> que le relecteur applique. `revue-spec` §2 ne compte plus les cases : il interdit de les compter
> ailleurs que dans le référentiel. C'est la friction 54, refermée, et ce tour est son premier essai.
>
> ⚠️ **Comparabilité, et c'est le fait de ce tour.** Les deux lignes « cases de §1 » **se comparent
> pour la première fois de la série** — au tour 5 seulement, et à lui seul. Elles restent
> incomparables aux tours 1 à 4, dont le référentiel n'en portait que neuf ou douze. La mention
> ⚠️ **ne se compare pas** est donc **maintenue** sur ces deux lignes, avec sa portée réduite.
>
> ⚠️ **Les réserves des tours 4 et 5 s'appliquent telles quelles** : *plus de remarques n'est pas
> mieux*, *autant de remarques ne veut pas dire les mêmes*, et rien ne mesure le calibrage d'un
> relecteur à l'autre sur le partage violations dures / jugements. **Coût et durée du relecteur** ne
> se mesurent toujours pas de l'intérieur.
>
> ⚠️ **Réserve neuve, et elle vise ce tour en propre** : c'est le premier tour où l'orchestrateur a
> **écarté des constats d'axe pour fausseté factuelle**. Deux des vingt-et-un produits n'ont pas été
> posés. La ligne « écartés » cesse donc de mesurer du bruit de doublon pour mesurer de l'erreur, et
> les deux usages ne se comparent pas.
>
> ⚠️ **Les rubriques 4 et 7 sont laissées à un tiers**, le relecteur ne jugeant pas sa propre sortie
> (`D-039`). Elles ne portent ici que des **faits bruts**, et pas de jugement.

## 1. Ce qui a tourné

`revue-spec` sur le document `Spec — Un agent pilote Cursus`, attaché au projet Linear du même nom,
en colonne `Spec` + `Review Requested`. La carte portait **80 commentaires, 0 ouvert** — les cinq
tours de spec et les treize remarques de la Discovery, tous soldés.

**Le chemin d'exécution, et où est la trace qu'il a servi** : un sous-agent lancé depuis la session
du binôme, qui a **invoqué le skill** `revue-spec` — l'appel est le premier de la trace —, lequel a
passé le mandat au primitif `revue` avec **deux axes ouverts en sous-agents séparés**, lancés dans le
même message donc en parallèle, aucun ne voyant le rapport de l'autre. Traces vérifiables :
dix-neuf commentaires sur la **carte** (jamais sur le document, `D-045`), chacun avec son repère
calculé, son axe et son étiquette de confiance ; l'étiquette `Rework Needed` posée sur le projet et
`Review Requested` retirée ; la colonne `Spec` inchangée ; l'`updatedAt` du document inchangé à
`2026-08-01T16:05:05.687Z`, relu après la pose et antérieur à la première remarque.

**La commande, verbatim et rejouable** — depuis la racine du dépôt :

```
Tu prends une revue de spec sur le backlog Linear du projet Cursus (espace `cursus-app`, équipe `CUR`).

La carte à relire : le **projet** Linear « Un agent pilote Cursus », actuellement en colonne `Spec`
et portant l'étiquette `Review Requested`. Son document `Spec` y est attaché.

Invoque le skill `revue-spec` et suis son protocole jusqu'au bout.

Le dépôt est à la racine du projet. Le référentiel de conformité est `docs/methode/dod/feature/spec.md`.

Un fait d'état, que tu ne peux pas établir depuis l'artefact seul et dont tu as besoin pour instruire
la §2 du référentiel : la carte porte **80 remarques de revue, toutes soldées** (`open: 0`), chacune
avec sa réponse en fil. L'accord de l'humain est structurellement en aval de ton passage.

Puis, **une fois la revue close**, écris sa fiche de retour d'expérience :

- fichier `docs/methode/rex/2026-08-01-revue-spec-tour-6.md` ;
- rubriques **fixes** de `docs/methode/rex/README.md`, dans l'ordre, aucune omise — lis-le avant
  d'écrire ;
- ⚠️ **tu ne juges pas ta propre sortie.** Remplis les rubriques **1, 2, 3, 5 et 6** : ce qui a
  tourné (avec la commande verbatim et rejouable), tes chiffres, ta conformité au protocole clause
  par clause **avec ce qui l'atteste**, les frictions rencontrées, ce que le tour a changé. Pour les
  rubriques **4 (qualité de la sortie)** et **7 (verdict pour le skill éprouvé)**, écris uniquement
  « À compléter par un tiers » suivi des **faits bruts** qu'un tiers utilisera pour les remplir. Un
  relecteur qui se décerne son propre verdict est exactement le piège que ce dossier existe pour
  éviter ;
- **huit fiches existent déjà** dans le même dossier, dont **cinq** de `revue-spec` (tours 1 à 5) :
  prends-les pour gabarit, et rends tes chiffres **comparables aux leurs**. La fiche du tour 5 porte
  un tableau à colonnes Tour 1 → Tour 5 — prolonge-le d'une colonne **Tour 6**, en reprenant les
  lignes existantes. Lis aussi les réserves de comparabilité qu'elle écrit sous son tableau : elles
  s'appliquent à toi, y compris la mention « ⚠️ ne se compare pas » que deux lignes portent ;
- ⚠️ **ce qui a bougé depuis le tour 5, et c'est le fait de comparabilité de ce tour-ci.** Le
  référentiel `docs/methode/dod/feature/spec.md` est **inchangé** (dix-sept cases en §1, trois en
  §2) — c'est donc le premier tour de la série dont les lignes « cases de §1 » se comparent au tour
  précédent. En revanche **le skill `revue-spec` a été corrigé ce matin**, et la correction porte
  précisément sur ce que tu appliques : sa clause §2 annonçait « les douze cases de §1 » et nommait
  « plan d'implémentation » ce que `D-053` a renommé *plan d'architecture* ; elle ne donne plus
  aucun nombre et renvoie au référentiel. C'est la friction 54 du journal, refermée. Le primitif
  `revue`, lui, est inchangé depuis le tour 4 ;
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

**Le prompt garde les cinq charges du tour 5** — le fait d'état fourni par l'appelant, l'inscription
par l'appelant de ce qui a bougé, l'interdiction de lisser une ligne incomparable, le relevé des
chiffres des sous-agents, les deux cellules en attente. Il en gagne deux qui appartiennent au chemin
d'exécution autant que les options :

- **l'appelant nomme la mention `⚠️ ne se compare pas` et demande qu'elle soit reprise**, là où le
  tour 5 avait dû l'inventer. La charge d'incomparabilité est devenue une consigne, pas une trouvaille ;
- **l'appelant inscrit une correction de skill, et non plus un changement de référentiel.** C'est le
  premier tour dont le fait de comparabilité porte sur **l'instrument** plutôt que sur la règle.

**Deux différences de dispositif, choisies par l'orchestrateur, qui appartiennent au chemin :**

- **la matérialisation intermédiaire est revenue**, après avoir été abandonnée au tour 5. Le document
  a été chargé une fois depuis Linear puis transcrit dans un fichier de travail hors dépôt, que les
  deux axes ont lu — garantissant qu'ils relisent **rigoureusement le même texte**, ce que deux
  chargements indépendants ne garantissent pas. ⚠️ **La précaution du tour 4 redevenait donc
  applicable, et elle a été payée autrement** : plutôt qu'un rechargement de contrôle, chaque
  citation a été vérifiée **unique** dans l'extraction par un `grep -F` avant pose, puis **résolue
  par l'outil de pose contre le document réel**. Les dix-neuf ancrages ont été acceptés du premier
  coup — c'est une attestation de fidélité plus forte qu'une comparaison, puisqu'elle porte sur le
  document vivant et non sur une copie ;
- **les deux axes ont été lancés en synchrone**, dans un seul message, comme au tour 5. **Aucun appel
  d'attente**, et la friction 49 reste sans occurrence.

**Ce qui a bougé dans ce que le relecteur applique**, inscrit par l'appelant :

- **`docs/methode/dod/feature/spec.md` est inchangé** — dix-sept cases en §1, trois en §2, recomptées
  à la main dans le fichier et par l'axe Conformité indépendamment ;
- **`revue-spec` §2 a été corrigé**, et c'est le fait du tour. La clause ne donne plus de nombre :
  elle porte désormais l'interdiction inverse — « ⚠️ **Compter les cases dans le référentiel, jamais
  ici.** […] il l'a donné, et le nombre a menti » — et nomme *plan d'architecture*. **Friction 54,
  refermée** ;
- **le primitif `revue` est inchangé depuis le tour 4.** ⚠️ C'est ce qui laisse intact l'écart
  d'étiquette : `revue` §8 dit toujours « `Done` ou `Rework Needed` » là où `revue-spec` §4 dit
  `Human Review Requested`.

## 2. Chiffres

| | Tour 1 | Tour 2 | Tour 3 | Tour 4 | Tour 5 | **Tour 6** |
|---|---|---|---|---|---|---|
| Durée de travail du relecteur | 727 s | 619 s | non mesurable de l'intérieur | 1 321 s | 1 223 s | **1 687 s** |
| Durée des deux axes | non relevée | non relevée | 591 s et 410 s, en parallèle | 524 s et 553 s, en parallèle | 474 s et 371 s, en parallèle | **635 s et 319 s**, en parallèle |
| Jetons du relecteur | ~135 000 | 82 447 | non mesurable de l'intérieur | 162 263 | 161 967 | **210 460** |
| Jetons des deux axes | non relevés | non relevés | 137 548 + 104 194 = 241 742 | 130 090 + 72 547 = 202 637 | 116 893 + 118 474 = 235 367 | **166 886 + 100 721 = 267 607** |
| Appels d'outils | 36 | 24 | 28 + 37 = 65 | 34 (hors attente) + 35 = 69 ; 12 d'attente en sus | 38 (orchestration, 0 d'attente, fiche comprise) + 19 (axes) = 57 | **35** (orchestration, **0 d'attente**, fiche comprise) **+ 20** (axes) = **55** |
| Sous-agents ouverts | 2 | 2 | 2 (un par axe) | 2 (un par axe) | 2 (un par axe) | **2** (un par axe) |
| Constats produits par les axes | non relevé | non relevé | 20 | 17 (9 Conf, 8 Déc) | 13 (6 Conf, 7 Déc) | **21** (13 Conformité dont 1 hors mandat, 8 Découpabilité) |
| Écartés ou fusionnés avant pose | non relevé | non relevé | 4 — 1 écarté, 1 subsumé, 2 fusions | 1 — une fusion, 0 écarté | 1 — une fusion de doublon inter-axes, 0 écarté | **2 — 2 écartés pour fausseté factuelle, 0 fusion** ⚠️ *ne mesure pas la même chose* (voir sous le tableau) |
| **Remarques posées** | 11 | 12 | 16 | 16 | 12 | **19** |
| Répartition | 3 Conf · 8 Déc | 6 Conf · 5 Déc · 1 hors mandat | 11 Conf · 5 Déc · 0 hors mandat | 9 Conf · 7 Déc · 0 hors mandat | 6 Conf · 6 Déc · 0 hors mandat | **11 Conf · 7 Déc · 1 hors mandat** |
| **Violations dures** | 1 | 2 | 12 (11 Conf, 1 Déc) | 7 (4 Conf, 3 Déc) | 6 (toutes Conformité) | **13** (11 Conf, 2 Déc) |
| Jugements | 10 | 9 | 4 (tous Déc) | 9 (5 Conf, 4 Déc) | 6 (tous Découpabilité) | **5** (tous Découpabilité) |
| Constats hors mandat — justesse | 3 | 1 | 0 | 0 | 0 | **1** |
| **Remarques nées d'une figure** | 0 | 0 | 5 | 2 | 4 | **4** — 1 du `flowchart`, 2 du `sequenceDiagram`, 1 de la légende des couleurs ; une cinquième (Découpabilité) s'appuie sur la figure comme l'une de ses trois pièces |
| Remarques visant une **reprise** du tour précédent | — | 0 | 2 | 4 | non mesurable ce tour | **au moins 6**, mesuré par la présence de `D-056`–`D-059` dans le passage visé ou son chapeau ⚠️ *minorant* (voir sous le tableau) |
| Cases de §1 évaluées | 9 | 12 | 12 | 12 | 17 ⚠️ ne se compare pas | **17** ⚠️ **ne se compare pas** aux tours 1–4 ; **se compare au tour 5** |
| Cases de §1 enfreintes | 1 | non relevé | 6 | 1 au sens de la clause ; 6 cases portant une divergence | 0 au sens de la clause ; 4 cases portant une divergence ⚠️ ne se compare pas | **2** au sens de la clause — *titres du gabarit*, *options arbitrées avec leur coût* ; **5 cases** portent au moins une divergence — les deux précédentes plus *trois registres*, *socle nommé*, *au moins un schéma*. **0 omission silencieuse** ⚠️ **ne se compare pas** aux tours 1–4 ; **se compare au tour 5** |
| Cases de §2 | 3, dont 2 en aval | 3 | 3, toutes non opposables depuis l'artefact seul | 3 — 2 tenues, 1 en aval | 3 — 2 tenues sur le fait d'état fourni, 1 en aval | **3** — 2 **tenues** sur le fait d'état fourni, 1 (l'accord de l'humain) structurellement en aval |
| Carte avant / après | — | — | 36 → 52, 16 ouverts | 52 → 68, 16 ouverts | 68 → 80, 12 ouverts | **80 commentaires, 0 ouvert → 99, 19 ouverts** |
| Étiquette posée | `Rework Needed` | `Rework Needed` | `Rework Needed` | `Rework Needed` | `Rework Needed` | **`Rework Needed`** |

**Les deux lignes « cases de §1 » se comparent enfin, et seulement sur un pas.** Neuf, douze, douze,
douze, dix-sept, dix-sept : la mention d'incomparabilité **reste**, mais elle ne vaut plus que vers
la gauche du tour 5. Ce que la comparaison tour 5 → tour 6 autorise, et rien de plus : **0 → 2 cases
enfreintes au sens de la clause**, et **4 → 5 cases portant au moins une divergence**, sur un
référentiel identique et un artefact repris entre les deux. C'est le premier point de la série où
ces deux lignes mesurent le même objet deux fois.

⚠️ **Les deux cases enfreintes ne sont pas du même ordre que les divergences.** Une case « enfreinte
au sens de la clause » est ici une clause du référentiel opposable telle quelle — les titres
d'annexe ne sont pas ceux du gabarit ; un écart est arbitré sans son coût quand la DoD l'exige. Les
trois autres cases portent des divergences **internes** à l'artefact, que la case seule ne détecte
pas. **Aucune omission silencieuse** — le seul cas que la DoD interdit — n'a été trouvée, pour le
second tour consécutif.

**La ligne « écartés » a changé de nature, et il faut l'écrire plutôt que d'aligner un chiffre.**
Aux tours 3 à 5, elle comptait du **bruit** — doublons inter-axes, constats subsumés. Ce tour n'a
fusionné aucun doublon et a écarté **deux constats faux** :

- l'axe Conformité opposait que « **trois** manques du noyau » (§3.1) contredisait le « deux des
  trois manques » du tableau des dépendances et le « les **deux** gestes » de l'annexe B. Vérification
  faite : les trois passages sont **cohérents** — le tableau dit bien *deux des trois*, et l'annexe B
  parle des deux qui sont des *gestes*. Le constat reposait sur une lecture, pas sur une contradiction ;
- l'axe Découpabilité opposait que la clause « deux worktrees » du scénario 4 contredit la mesure
  d'annexe C (« git refuse `already checked out` »). Vérification faite dans `architecture.md` : le
  provisionnement du travail neuf est **en HEAD détaché**, précisément « pour éviter le refus git
  *branch already checked out* quand deux runs partent de la même base — un test le prouve ». La
  clause est tenable, et le socle y répond par renvoi.

⚠️ **Les deux constats portaient leurs deux citations**, comme `revue` §3 l'exige. Les citations
étaient exactes ; c'est l'**inférence** qui était fausse. Aucune clause du protocole n'aurait
attrapé ces deux-là.

**La ligne des reprises redevient mesurable, et elle est minorée.** L'artefact a été repris entre le
tour 5 et celui-ci, et la reprise a produit cinq décisions (`D-055` à `D-059`). **Six remarques au
moins** visent un passage qui porte l'une d'elles ou dont le chapeau la cite : les lignes de
l'inventaire (`D-056`), la règle de descente (`D-059`), la note de verrou du `sequenceDiagram`
(`D-058`), le §1.3 (`D-057`), le §2.3 « fût-elle implicite » (`D-057`), et les questions ouvertes sur
les hosts gardés (`D-057`). ⚠️ **Le chiffre est un minorant assumé** : le relecteur n'a pas la version
antérieure du document et ne peut pas dater un passage qui ne cite aucune décision.

**La hausse de 12 à 19 ne s'interprète pas seule.** La série est désormais
**11, 12, 16, 16, 12, 19** — elle ne converge pas, et elle vient de repartir vers le haut après sa
seule baisse. Trois causes se superposent sans que rien ne les départage : un artefact **repris cinq
fois**, donc dense en texte neuf jamais relu ; un skill **corrigé** juste avant le tour, dont la
clause interdit désormais de s'arrêter à un compte figé ; et un relecteur différent. ⚠️ **Une quatrième
cause est propre à ce tour et tire dans l'autre sens** : deux constats ont été écartés, donc la
récolte brute était de vingt-et-un.

**Ce que la ligne des appels d'outils mesure.** Trente-cinq appels d'orchestration, **tous des appels
de travail**, l'écriture de cette fiche comprise, et zéro appel d'attente — les axes ayant été lancés
en synchrone. La comparaison légitime est **38 (tour 5) contre 35 (tour 6)**, à charge égale : relire
**et** écrire la fiche. Trois appels de moins pour sept remarques de plus. ⚠️ **Six de ces
trente-cinq sont des appels de vérification** — la confrontation au dépôt des assertions d'axe (§4) —,
poste qu'aucun tour antérieur n'avait isolé ; la matérialisation intermédiaire, elle, a coûté un
appel (l'écriture du fichier de travail) et en a économisé deux (les axes n'ont pas rechargé le
document chacun de son côté).

**Complété par la session appelante — les deux cellules, et ce qu'elles disent.** Durée **1 687 s**
(1 223 s au tour 5, **+38 %**) et **210 460 jetons** pour le relecteur (161 967, **+30 %**). Le tour
entier — relecteur plus axes — a coûté **478 067 jetons**, contre 397 334 au tour 5, soit **+20 %**.

⚠️ **C'est le tour le plus cher de la série, et le moins cher par remarque.** 25 161 jetons et 89 s
par remarque posée, contre 33 111 et 102 s au tour 5 : la hausse du total est **moins que
proportionnelle** à celle de la récolte. Une seule chose est établie par là — le coût ne croît pas
avec le rang du tour ; rien ne dit que ces remarques-ci valent celles du tour 5, et ce partage
appartient à la rubrique 4.

⚠️ **Les compteurs d'appels divergent encore, et davantage** : le relecteur en relève 35, l'appelant
en mesure **39**. Écart de quatre, contre un seul au tour 5 (39 contre 38) — même valeur côté
appelant les deux fois. La ligne du tableau reste celle du relecteur, par cohérence avec les tours
précédents ; ce qu'on ne sait pas, c'est **ce que le compteur externe compte en plus**, et aucun des
six tours ne permet de le dire.

## 3. Conformité au protocole

Clause par clause, avec ce qui l'atteste.

| Clause | Tenue | Ce qui l'atteste |
|---|---|---|
| Session neuve, sur l'artefact seul (`revue-spec` §1, `D-039`) | **oui, sous trois réserves nommées** | Le fil de rédaction n'a pas été transmis ; **les 80 commentaires soldés de la carte n'ont été lus par personne** — le fait d'état venant de l'appelant, l'orchestrateur n'a jamais ouvert un fil de reprise. ⚠️ Réserve 1 : la mémoire automatique de la session résume l'artefact par ses conclusions et sa trajectoire (journal 43), inchangé. ⚠️ Réserve 2, **atténuée** : la mémoire n'annonce plus qu'un défaut est planté dans l'artefact — elle porte désormais l'interdiction inverse, écrite après le tour 5 (« ne jamais écrire dans ces fichiers ce qu'une revue à venir doit trouver »). Elle **mentionne encore l'existence du gotcha** dans son index de pointeurs, mais le fichier qui le détaille n'a pas été ouvert. ⚠️ Réserve 3 : le fait d'état reste un résumé de l'issue des tours précédents. **Aucune des trois n'a été transmise aux axes**, qui n'ont reçu que l'artefact, les référentiels et le fait d'état chiffré |
| Exactement deux axes, jamais fondus (`revue-spec` §2, `revue` §2) | **oui** | Deux sous-agents lancés dans le même message, chacun ignorant l'existence de l'autre ; deux rapports reçus distinctement, chacun se clôturant sur son propre verdict d'axe ; aucun rapport de synthèse qui reclasse. **Les deux remarques doublonnées n'ont pas été fusionnées** — voir sous le tableau |
| Les cases de §1 et les trois de §2, clause par clause | **oui, et sans écart du skill au référentiel** | L'axe Conformité rend deux tableaux de couverture — **dix-sept** lignes puis trois — avant sa liste de constats, et nomme pour chaque case où elle se lit dans l'artefact. ⚠️ **C'est le premier tour où le skill lui-même porte la bonne consigne** : `revue-spec` §2 ne donne plus de nombre et prescrit « Compter les cases dans le référentiel, jamais ici ». Le mandat a relayé cette interdiction aux axes ; le décompte a été fait **trois fois indépendamment** — par l'orchestrateur dans le fichier, par l'axe, et par l'appelant — et les trois donnent dix-sept |
| Deux citations par constat (`revue` §3) | **oui** | Chacune des dix-neuf remarques porte son référentiel (fichier + clause, ou le second passage de l'artefact quand la contradiction est interne) et l'extrait visé, côte à côte. ⚠️ **Et c'est précisément ce qui n'a pas suffi** : les deux constats écartés portaient leurs deux citations |
| **Confronter chaque figure à la prose** (`revue` §3, `revue-spec` §2) | **oui** | Le mandat des **deux** axes nommait les deux blocs `mermaid` et le bloc `gherkin`, et demandait de les relire nœud par nœud, arête par arête, flèche par flèche, couleur par couleur, en vérifiant **dans les deux sens** que tout ce que la prose énumère existe dans la figure et réciproquement. **Quatre des dix-neuf remarques en sortent** : la fenêtre qui n'a aucun chemin de lecture dans le `flowchart`, la note « la fenêtre attend **le sien** » qui suppose deux verrous là où `D-058` n'en pose qu'un, le `sequenceDiagram` qui omet la racine que chaque appel doit résoudre, et la légende qui fait « quitter la présentation » à trois pièces qu'elle vient de déclarer naissantes |
| Écarter la justesse (`revue-spec` §3) | **oui, avec matière** | L'axe Découpabilité a rendu sa section *hors mandat — justesse* explicitement vide ; l'axe Conformité y a versé **un** constat — la mise à jour perdue entre l'éditeur *stateful* et une écriture MCP, que le verrou sérialise sans la préserver. Posé comme remarque distincte sous l'intitulé « hors mandat — justesse », conformément à `revue` §6 (« y compris un constat de justesse, qui appelle l'arbitrage de l'humain »), et **hors** des deux axes. Aucune ligne des deux rapports d'axe ne discute si la spec est un bon choix |
| Étiqueter la confiance (`revue` §5) | **oui** | 13 *violation dure*, 5 *jugement*, 1 *hors mandat — justesse*, aucune ligne ambiguë. ⚠️ **Le partage ne coïncide plus avec les axes**, contrairement au tour 5 : l'axe Découpabilité a produit **deux violations dures** (les deux contradictions internes qui bloquent une frontière) en plus de ses cinq jugements. La DoD §3 annonce que cet axe rend « par nature des **jugements** » ; le mandat autorisait explicitement l'exception pour une contradiction interne, et elle a servi |
| Lister sans réécrire (`revue` §6) | **oui** | `updatedAt` du document relu après la pose : `2026-08-01T16:05:05.687Z`, inchangé et antérieur à la première remarque. Aucune remarque ne propose un texte de remplacement ; les sept de Découpabilité posent **la question que le découpage devrait revenir poser**, elles n'y répondent pas |
| Deux issues, jamais trois (`revue` §6, `D-051`) | **oui** | 21 constats produits, 19 posés, **2 écartés parce que faux**. Aucun constat n'est écrit ailleurs que sur la carte ; aucune « observation non bloquante » ; **les deux écartés ne figurent nulle part dans l'artefact ni sur la carte** — ils ne vivent que dans cette fiche, comme mesure. Les deux axes ont reçu la clause dans leur mandat, avec le test *est-ce que quelqu'un doit répondre ?* |
| Poser la remarque sur la carte (`revue` §6, `D-045`) | **oui, sans rappel** | 19 remarques posées par `cursus linear comment add`, ancrées avec leur repère calculé ; `open` passe de 0 à 19, `total` de 80 à 99. Aucune sur le document |
| Poser l'étiquette, jamais déplacer (`revue-spec` §4, `revue` §8) | **oui** | `Rework Needed` seul sur le projet, `Review Requested` retiré ; `status` reste `Spec` |
| Pas d'escalade, pas de compteur de tours (`revue-spec` §4) | **oui** | Aucune assignation, aucune tentative de convergence au-delà de ce passage, alors même que c'est le sixième |

**Une clause corrigée a tenu, et c'est le fait du tour.**

- **`revue-spec` §2 ne compte plus les cases**, et interdit de les compter ailleurs que dans le
  référentiel. La clause porte désormais son propre motif — « il l'a donné, et le nombre a menti » —
  et le renvoi à `D-054` et à la friction 54. **Premier tour où le skill seul suffit** : au tour 5,
  c'est le mandat de l'appelant qui rattrapait l'écart ; ici, le mandat n'a fait que relayer ce que
  le skill prescrit. Le vocabulaire est aussi à jour — *plan d'architecture*, `D-053`.
- ⚠️ **Ce que ce tour ne peut pas établir** : que la correction aurait suffi **sans** le mandat.
  L'appelant a écrit lui-même que le référentiel porte dix-sept cases. Le skill et le mandat disaient
  la même chose ; on ne sait pas lequel des deux a porté.

**Une clause reste non tenable telle qu'écrite, pour la troisième fois.**

- **`revue` §8 dit toujours « `Done` ou `Rework Needed` »** là où `revue-spec` §4 dit
  `Human Review Requested`. Sans effet ici — dix-neuf remarques imposent `Rework Needed` des deux
  côtés —, l'écart est **intact**, troisième occurrence sans conséquence observée. Le cas où il mord
  reste le tour sans aucune remarque, qui ne s'est toujours pas présenté en six tours. Friction 48.

**Et la contradiction du protocole est atteinte pour la quatrième fois — résolue dans l'autre sens.**
`revue` §2 interdit de fondre les axes, `revue` §6 veut une remarque par constat. Les deux axes ont
trouvé **deux fois le même passage** : l'état des runs en vol (contradiction de registres pour l'un,
frontière indécidable pour l'autre) et les lignes de l'inventaire des Tâches (geste d'écran pour
l'un, acceptation indécidable pour l'autre). **Le tour 5 avait fusionné ; ce tour n'a pas fusionné** —
quatre remarques posées, deux par passage, avec des citations distinctes et des questions distinctes,
au motif que `revue` §2 est une interdiction et `revue` §6 une exigence. ⚠️ **Rien n'arbitre entre
les deux traitements**, et la série porte désormais les deux. C'est ce qui rend la friction 44
décidable : il existe maintenant deux exécutions opposées à comparer.

## 4. Qualité de la sortie

> **À compléter par un tiers.** Le relecteur ne juge pas sa propre sortie (`D-039`). Ce qui suit
> n'est que la matière brute.

**Les faits bruts.**

*Le sort des dix-neuf remarques n'est pas connu au moment d'écrire.* Les cinq tours précédents
affichent **67 retenues sur 67**, aucun refus motivé.

*Treize violations dures*, dont **onze sur l'axe Conformité** et deux sur l'axe Découpabilité.
Leur nature se répartit ainsi :

- **deux clauses du référentiel enfreintes telles quelles**, opposables sans interprétation : les
  titres des annexes A et C ne sont pas ceux du gabarit de `D-054` et l'écart n'est motivé nulle
  part ; et l'écart *tests de bout en bout* est le seul des quatre à ne porter **aucun élément de
  coût**, alors que l'annexe A annonce l'axe pour les cinq pistes ;
- **un renvoi manquant** : la règle de descente du socle est le dispositif de `D-059`, et `D-059`
  n'est cité nulle part, alors que les quatre autres décisions du même jour le sont et que la règle
  voisine cite `D-057`. `D-059` ouvre en notant que cet arbitrage **avait déjà été perdu une fois** ;
- **un renvoi pris en défaut contre le dépôt** : « son **unique appelant** est la classe
  d'application », donné comme « vérifié au code » et fondant une conséquence « pas optionnelle »,
  alors qu'`architecture.md` §7.12 enregistre l'end-to-end headless comme second appelant **et comme
  preuve du critère** ;
- **neuf contradictions internes**, opposables sans le dépôt — dont **quatre nées des figures**
  (la fenêtre sans chemin de lecture ; « la fenêtre attend **le sien** » sous un verrou global ;
  le `sequenceDiagram` sans la racine ; la légende qui fait descendre ce qu'elle déclare naissant),
  deux sur l'inventaire (une ligne qui nomme un geste d'écran que le §1.2 exclut ; l'état des runs
  en vol rangé dans trois registres), une sur la recette (le scénario 7 ne double aucun segment du
  scénario 1, qui ne porte pas d'arrêt), et deux sur l'axe Découpabilité (la requête qui écrit hors
  verrou ; l'état des runs en vol comme frontière indécidable).

*Cinq jugements, tous sur l'axe Découpabilité*, et ils visent trois endroits :

- **le §3.3**, dont la règle « chaque scénario atterrit dans au moins un incrément » ne s'applique
  pas au scénario 4, dont les quatre clauses portent sur quatre objets livrés par quatre incréments
  — la seule instruction qui règle le cas vivant en annexe B, là où la DoD exige les règles
  d'atterrissage « dans le corps » ;
- **le §2.3**, deux fois : le quantum de la descente du socle (le tableau des dépendances l'élargit
  au trousseau et au registre des trackers, dont le premier incrément n'a aucun usage) et la règle
  de recablage « écran par écran », dont l'éditeur — le plus gros morceau — est le contre-exemple,
  puisqu'il n'a qu'un point de traversée ;
- **l'irréductibilité du premier incrément mutant** : les deux motifs du §2.3 ferment, chacun pour
  une raison juste, les deux seules coupes qui le réduiraient, et il concentre alors la descente du
  socle, la couche applicative, le verrou global, le recablage et deux clauses du scénario 4.

*Le constat hors mandat — justesse* : le verrou global garantit la non-corruption mais pas qu'une
étape ajoutée par l'agent **survive** à l'enregistrement de l'éditeur, qui réécrit un brouillon
complet ouvert avant elle. La clause du scénario 4 est satisfaite pendant que le geste de l'agent
disparaît en silence. Posé sans être tranché.

*Les deux constats écartés*, à consigner parce qu'ils mesurent le taux d'erreur des axes et non le
bruit : la fausse contradiction sur « trois manques du noyau », et la fausse contradiction sur les
worktrees, démentie par `architecture.md` (provisionnement en HEAD détaché, avec son test). **21
produits, 19 posés, 0 fusionné.** ⚠️ **Un tiers doit peser les deux sens** : deux constats faux sur
vingt-et-un est le premier taux d'erreur d'axe jamais mesuré dans ce dossier — et le fait qu'ils
aient été attrapés est aussi une mesure, celle d'une vérification que le protocole ne prescrit pas.

*Les deux verdicts d'axe*, rendus séparément :

- **Conformité — désaccord.** « L'artefact porte des divergences opposables dont plusieurs
  contradictions internes concentrées sur les deux figures et la recette. »
- **Découpabilité — désaccord.** « Le découpage trace ses frontières et son ordre sans difficulté,
  mais il ne peut pas donner leur acceptation ni leur orientation technique aux deux incréments qui
  portent le risque. »

*Ce que l'axe Découpabilité a produit comme instrument* : un découpage candidat complet — **sept
incréments**, leurs frontières, leur ordre, leurs arêtes de blocage, et la recette rattachée à
chacun. Les huit achoppements en sont dérivés, et non énoncés d'avance. C'est la pièce qui permet à
un tiers de vérifier que la tentative a eu lieu, comme le demande §3 de la DoD (« il se **teste** »).

*Ce que la revue a vérifié dans le dépôt* : l'orchestrateur a confronté au dépôt **six** assertions
d'axe avant de poser — le gabarit des titres d'annexe dans `tickets.md`, l'existence et le contenu
de `D-059`, la règle de renvoi de `tickets.md` §5, `architecture.md` §7.12, `D-057`, et le mode de
provisionnement des worktrees. **Quatre ont tenu, deux ont été démenties.** Le tour 5 en avait
confronté quatre et en avait pris une en défaut ; le tour 4 une douzaine sans en démentir aucune ;
le tour 3 trois sur trois.

*Un fait que le tiers doit connaître avant de juger, et qui n'est pas dans l'artefact* : la mémoire
persistante de la session **ne dit plus** qu'un défaut est planté dans la spec — elle porte au
contraire, depuis le tour 5, l'interdiction explicite d'y écrire ce qu'une revue à venir doit
trouver. Son index mentionne toutefois encore l'existence d'un « gotcha du test annoncé dans la
mémoire », dans la ligne qui pointe vers le fichier de travail de cette feature ; **ce fichier n'a
pas été ouvert**, et le relecteur ignore s'il subsiste un point planté et lequel. La condition du
tour 5 — *le relecteur sait qu'il est testé, sans savoir sur quoi* — est donc **atténuée sans être
levée**, et un tiers doit décider ce que cela vaut.

*Ce que le dispositif n'a pas produit, et qui reste le mode d'échec nommé aux tours 3, 4 et 5* :
aucune des dix-neuf remarques ne demande si une section devait exister. Elles chicanent toutes
**dans** le cadre de l'artefact.

## 5. Frictions

Journal des frictions, entrées **43** (la mémoire automatique dément la clause de session neuve —
**quatrième occurrence, mais atténuée** : la mémoire porte désormais l'interdiction issue du tour 5
et n'annonce plus de défaut planté ; elle résume toujours l'artefact par ses conclusions),
**44** (deux axes sur le même passage, sans règle — **quatrième occurrence**, et la première
**résolue dans l'autre sens** : pas de fusion, deux remarques par passage doublonné ; la série porte
maintenant les deux traitements), **48** (corriger une instance laisse le primitif porter l'ancienne
clause — **troisième occurrence**, `revue` §8 dit toujours `Done`), **50** (la clause de session
neuve n'a pas de doctrine sur les faits d'état — **troisième occurrence**, sans variation : le fait
est venu de l'appelant, comme au tour 5).

**Trois entrées sont sans occurrence ce tour, et c'est mesuré** : **49** (l'attente d'un sous-agent
coûte des appels qui ne produisent rien) — zéro appel d'attente, les axes ayant été lancés en
synchrone, seconde fois consécutive ; **45** (la pièce la plus contestable est la moins citable) —
quatre remarques ancrées sur des passages de prose commentant les figures, et aucune n'a buté ;
**55** (la citation d'ancrage bute sur les marques d'emphase) — **dix-neuf poses, dix-neuf
acceptées du premier coup**, la vérification d'unicité par `grep -F` ayant été faite avant pose et
les citations choisies courtes et hors des marques d'emphase.

**Une friction neuve**, à numéroter au journal par la session appelante et **non recopiée ici** :
*un axe rend un constat faux qui porte pourtant ses deux citations, et aucune clause ne prescrit de
le vérifier*. Deux constats sur vingt-et-un ont été écartés après confrontation au dépôt ; dans les
deux cas les citations étaient exactes et l'inférence fausse. `revue` §3 fait des deux citations le
« garde-fou contre le constat plausible mais faux, **préféré ici à la vérification empirique** » — ce
tour mesure que le garde-fou ne suffit pas, et que la vérification empirique a dû être faite quand
même, par l'orchestrateur et de sa propre initiative.

## 6. Ce que le tour a changé

- **La friction 54 est refermée, et ce tour est son premier essai.** `revue-spec` §2 ne compte plus
  les cases et interdit de les compter ailleurs que dans le référentiel ; le vocabulaire est aligné
  sur `D-053`. Le décompte a été fait trois fois indépendamment et a donné dix-sept les trois fois.
  ⚠️ **Ce que l'essai n'établit pas** : que le skill aurait suffi sans le mandat, qui disait la même
  chose. Un tour où l'appelant ne réinscrirait pas le nombre trancherait ; celui-ci non.
- **La matérialisation intermédiaire est revenue, avec une garantie neuve.** Le tour 5 l'avait
  supprimée pour que les axes citent l'artefact réel. Elle revient ici pour garantir que les deux
  axes lisent **le même texte au caractère près** — et la fidélité est attestée autrement : dix-neuf
  citations vérifiées uniques dans l'extraction, puis **résolues par l'outil de pose contre le
  document vivant**, dix-neuf acceptations du premier coup. Le compromis du tour 4 (extraire, puis
  recharger pour vérifier) est donc dépassé : l'ancrage lui-même fait la vérification.
- **La friction 44 a reçu son second traitement, opposé au premier.** Le tour 5 avait fusionné les
  doublons inter-axes en une remarque ; ce tour ne les a pas fusionnés — quatre remarques pour deux
  passages, avec des citations et des questions distinctes. Ce n'est pas un progrès, c'est un
  **matériau** : la friction est ouverte depuis le tour 3 faute de pouvoir comparer, et il existe
  maintenant deux exécutions à comparer sur le même artefact.
- **La ligne « écartés ou fusionnés » a cessé de mesurer le bruit pour mesurer l'erreur**, et c'est
  neuf. Aucun tour antérieur n'avait écarté un constat pour fausseté factuelle — le tour 3 en avait
  écarté un, sans que la fiche en dise le motif. Le dossier peut désormais chiffrer un **taux
  d'erreur d'axe** : 2 sur 21.
- **Les deux lignes « cases de §1 » ont retrouvé un point de comparaison**, pour la première fois
  depuis que `D-054` les avait cassées. La mention ⚠️ *ne se compare pas* a été **conservée avec sa
  portée réduite** plutôt que retirée : elle vaut vers les tours 1 à 4 et non vers le tour 5. C'est
  la première fois que le gabarit inter-tours doit exprimer une incomparabilité **partielle**.
- **Le gabarit inter-tours a tenu une troisième fois**, prolongé d'une sixième colonne. Une ligne a
  dû recevoir une seconde mention d'incomparabilité (« écartés ») pour un motif neuf : ce n'est pas
  le référentiel qui a bougé, c'est ce que la ligne compte.
- **La spec, elle, n'a pas changé** : la revue liste, elle ne réécrit pas. Dix-neuf remarques
  ouvertes attendent la reprise, et l'étiquette `Rework Needed` dit de ne pas tirer.
- **Rien n'a été changé dans les skills par ce tour.** La friction neuve vise le primitif `revue` —
  son §3 et la suffisance des deux citations — mais aucune décision n'a été prise ici.

## 7. Verdict pour `revue-spec`

> **À compléter par un tiers.** Les quatre issues de `D-043` amendé — *promu*, *corrigé par le
> journal*, *retiré*, *tué par un fait* — supposent un juge qui n'est pas l'exécutant. Ce qui suit
> n'est que la matière brute.

**Les faits bruts.**

- **Six tours, 86 remarques posées** (11 + 12 + 16 + 16 + 12 + 19). Les cinq premiers affichent
  **67 retenues sur 67**, aucun refus motivé. **Le sort des 19 de ce tour n'est pas connu** au moment
  d'écrire.
- **`revue-spec` était promu après les tours 1 à 4, et le tour 5 avait relevé une clause fausse.**
  Cette clause est **corrigée**, et ce tour est le premier à tourner avec le skill à jour. Les douze
  clauses du §3 ci-dessus ont chacune leur pièce.
- **La correction du skill a produit exactement ce qu'elle visait, et le tour ne peut pas prouver
  qu'elle était nécessaire.** Le skill et le mandat de l'appelant portaient tous deux la bonne
  consigne. Un tiers doit dire si l'essai compte comme validation de la correction, ou s'il faut un
  tour où l'appelant se tait.
- **La réserve sur l'étiquette reste levée côté instance et intacte côté primitif** : `revue-spec` §4
  dit `Human Review Requested`, `revue` §8 dit toujours `Done`. Le cas où l'écart mord — un tour sans
  aucune remarque — ne s'est pas présenté en six tours.
- **La série ne converge pas, et elle vient de repartir vers le haut** : 11, 12, 16, 16, 12, 19. La
  sixième valeur est la plus haute de la série. Elle suit une reprise qui a produit **cinq décisions
  structurantes** (`D-055` à `D-059`) et un volume important de texte neuf ; **six remarques au moins
  visent ce texte neuf**. Un tiers dispose maintenant de six points pour dire si le signal d'arrêt
  peut venir de la boucle agent, ou s'il ne peut venir que de l'humain au temps ⑤. ⚠️ **Et d'un
  motif mécanique que la série antérieure ne portait pas** : chaque reprise produit du texte que
  personne n'a relu, donc une récolte pour le tour suivant. Le protocole n'a aucune sortie autre
  qu'un tour à zéro remarque.
- **Le taux d'erreur des axes est mesuré pour la première fois** : 2 constats faux sur 21, tous deux
  porteurs de leurs deux citations. Un tiers doit dire si cela vise `revue` §3 — qui préfère
  explicitement les deux citations à la vérification empirique — ou l'exécution des axes.
- **Le partage violations dures / jugements a cessé de coïncider avec les axes.** Le tour 5 était le
  premier à voir une coïncidence exacte ; ce tour la défait, l'axe Découpabilité ayant produit deux
  violations dures. Reste à dire si le tour 5 était un calibrage plus juste ou une coïncidence.
- **La condition « le relecteur sait qu'il est testé » est atténuée sans être levée** (§4). Elle
  affaiblissait la comparaison du tour 5 dans les deux sens ; elle l'affaiblit moins ici, et rien ne
  quantifie de combien.
- **La friction neuve n'est pas dans le geste de `revue-spec`** — elle vise le primitif `revue` et
  la suffisance de sa règle des deux citations. C'est la **troisième fois consécutive** que les
  frictions neuves d'un tour se logent au-dessus ou au-dessous du skill éprouvé, jamais dedans.
- **Ce que ce tour n'établit toujours pas** : rien du cycle au-delà du temps ②. Ce qui suit
  `Rework Needed` est la reprise par le binôme, et elle n'a jamais été jouée par un skill.
- **La réserve qui ne se lève pas** : six tours sur **le même artefact**, écrit par le binôme, avec
  un skill de la même main. Le tour utile suivant reste celui que les tours 4 et 5 nommaient — un
  premier passage de `revue-spec` sur une spec que le binôme n'aura pas rédigée.
