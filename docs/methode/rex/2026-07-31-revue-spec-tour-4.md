# 2026-07-31 — `revue-spec`, quatrième exécution

> Quatrième tour sur le même artefact, *Un agent pilote Cursus*, le même jour que les trois premiers.
> Deux différences avec le tour 3, et ce sont elles que le tour mesure : le skill a été **corrigé**
> entre-temps sur l'étiquette qu'il prescrit (`revue-spec` §4 dit désormais
> `Human Review Requested`, et non plus `Done` — journal 42), et l'artefact a été **repris deux fois
> de plus** depuis, dont une reprise de forme que le relecteur n'avait pas demandée (journal 46, la
> règle *n'écrire que ce qui est décidé*).
>
> ⚠️ **Comparabilité.** Les chiffres de remarques et de conformité se comparent aux trois tours
> précédents. Ceux de **coût et de durée du relecteur** ne se mesurent toujours pas de l'intérieur :
> comme au tour 3, la fiche est écrite **par** le relecteur. La différence est qu'ils sont cette
> fois **relevés par la session appelante**, et les deux cellules concernées portent la mention
> d'attente plutôt qu'un « non mesurable ».
>
> ⚠️ **Les rubriques 4 et 7 sont laissées à un tiers.** Le relecteur ne juge pas sa propre sortie —
> c'est le motif même de `D-039`. Elles portent les faits bruts, pas leur appréciation.

## 1. Ce qui a tourné

`revue-spec` sur le document `Spec — Un agent pilote Cursus`, attaché au projet Linear du même nom,
en colonne `Spec` + `Review Requested`. La carte portait **52 commentaires, 0 ouvert** — les trois
tours précédents et les treize remarques de la Discovery, tous soldés.

**Le chemin d'exécution, et où est la trace qu'il a servi** : un sous-agent lancé depuis la session
du binôme, qui a **invoqué le skill** `revue-spec` — l'appel est le premier de la trace —, lequel a
passé le mandat au primitif `revue` avec **deux axes ouverts en sous-agents séparés**, lancés dans le
même message donc en parallèle, aucun ne voyant le rapport de l'autre. Traces vérifiables : seize
commentaires sur la **carte** (jamais sur le document, `D-045`), chacun avec son repère calculé, son
axe et son étiquette de confiance ; l'étiquette `Rework Needed` posée sur le projet et
`Review Requested` retirée ; la colonne `Spec` inchangée ; l'`updatedAt` du document inchangé à
`15:50:23Z`, antérieur à la première remarque.

**La commande, verbatim et rejouable** — depuis la racine du dépôt, agent `general-purpose`,
en arrière-plan :

```
Tu prends une revue de spec sur le backlog Linear du projet Cursus (espace `cursus-app`, équipe `CUR`).

La carte à relire : le **projet** Linear « Un agent pilote Cursus », actuellement en colonne `Spec`
et portant l'étiquette `Review Requested`. Son document `Spec` y est attaché.

Invoque le skill `revue-spec` et suis son protocole jusqu'au bout.

Le dépôt est à la racine du projet. Le référentiel de conformité est `docs/methode/dod/feature/spec.md`.

Puis, **une fois la revue close**, écris sa fiche de retour d'expérience :

- fichier `docs/methode/rex/2026-07-31-revue-spec-tour-4.md` ;
- rubriques **fixes** de `docs/methode/rex/README.md`, dans l'ordre, aucune omise — lis-le avant
  d'écrire ;
- ⚠️ **tu ne juges pas ta propre sortie.** Remplis les rubriques **1, 2, 3, 5 et 6** : ce qui a
  tourné (avec la commande verbatim et rejouable), tes chiffres, ta conformité au protocole clause
  par clause **avec ce qui l'atteste**, les frictions rencontrées, ce que le tour a changé. Pour les
  rubriques **4 (qualité de la sortie)** et **7 (verdict pour le skill éprouvé)**, écris uniquement
  « À compléter par un tiers » suivi des **faits bruts** qu'un tiers utilisera pour les remplir. Un
  relecteur qui se décerne son propre verdict est exactement le piège que ce dossier existe pour
  éviter ;
- **six fiches existent déjà** dans le même dossier, dont trois de `revue-spec` (tours 1 à 3) :
  prends-les pour gabarit, et rends tes chiffres **comparables aux leurs**. La fiche du tour 3 porte
  un tableau à colonnes Tour 1 / Tour 2 / Tour 3 — prolonge-le d'une colonne **Tour 4**, en
  reprenant les lignes existantes ;
- ⚠️ **deux chiffres te sont inaccessibles de l'intérieur** — ta durée de travail totale et tes
  jetons consommés. Le tour 3 les avait laissés en « non mesurable de l'intérieur » ; cette fois la
  session appelante les relèvera. Écris exactement `⟨à remplir par la session appelante⟩` dans ces
  deux cellules, sans commentaire, et **relève en revanche toi-même** la durée et les jetons de
  chacun de tes sous-agents d'axe, ainsi que ton nombre d'appels d'outils — le tour 3 y était
  parvenu ;
- ⚠️ **aucun chemin personnel** dans la fiche — ce dépôt est public. Remplacer toute redirection par
  un nom de fichier nu ;
- **ne commite pas.**

En retour, rends-moi : le nombre de remarques posées, leur axe, l'étiquette que tu as posée, et —
en une phrase chacune — les remarques les plus lourdes.
```

**Le prompt garde l'allègement** des deux béquilles du tour 1 (session neuve, ne pas déplacer la
carte). Il gagne trois charges que le tour 3 n'avait pas, et qui appartiennent au chemin d'exécution
autant que les options : **prolonger un tableau existant d'une colonne** plutôt qu'écrire une fiche
libre, **relever soi-même les chiffres de ses sous-agents**, et **écrire deux cellules en attente**
plutôt que de les déclarer non mesurables. La première est la première tentative de gabarit
inter-tours que le dossier ait reçue ; c'est exactement la piste ouverte par le journal 47.

**La matérialisation intermédiaire du tour 3 a été reconduite** : le document Linear a été écrit dans
un fichier de travail hors du dépôt (`spec-content.md`) avant d'être passé aux deux axes, pour qu'ils
citent des passages exacts. L'axe Conformité a en outre **rechargé le document original** pour
vérifier que l'extraction lui était fidèle — elle l'était, duplication de saisie comprise. C'est une
précaution que le tour 3 n'avait pas prise, et elle a servi : sans elle, la remarque 7 aurait pu être
imputée à l'extraction.

**Un fait d'état a été fourni aux axes que le tour 3 n'avait pas fourni** : l'état des remarques
antérieures (39 posées, `open: 0`, chacune avec sa réponse en fil) et le fait que l'accord de
l'humain est structurellement en aval. Sans lui, les trois cases de §2 ne sont pas opposables depuis
l'artefact seul — c'est ce que le tour 3 avait constaté. La contrepartie est à noter : ce fait est un
**résumé de l'issue des tours précédents**, et il entre dans une session dont la clause §1 veut
qu'elle ne porte que l'artefact.

**Complété par la session appelante — le référentiel n'a pas bougé, les skills si.** Le relecteur lit
la version courante de ce qu'il applique, sans pouvoir savoir ce qui y a changé depuis le tour
précédent ; c'est à l'appelant de l'inscrire, parce que ça appartient au chemin d'exécution.

- **`docs/methode/dod/feature/spec.md` est inchangé depuis le tour 3** — son dernier amendement est
  celui de `D-049`, antérieur aux quatre tours. Les chiffres de l'axe Conformité, et notamment les
  douze cases, restent donc comparables colonne par colonne.
- **`revue-spec` §4 a été corrigé** : le verdict favorable est passé de `Done` à
  `Human Review Requested`. La fiche le relève elle-même, et le §3 constate que le primitif n'a pas
  suivi.
- **`revue` a gagné la clause `D-051`** — *un constat a deux issues, jamais trois* : ou il vaut d'être
  opposé, ou il ne s'écrit nulle part. **Ce tour est le premier à tourner sous cette clause**, et
  c'est un facteur direct de deux chiffres du tableau qui se lisent sinon comme une performance :
  **0 constat écarté** et aucune observation non bloquante. Le tour 3 en avait laissé quatre se
  perdre faute d'un geste pour les poser ; ici, le geste est interdit et le silence est prescrit.
  Ce que la clause fait au décompte n'est pas mesuré et ne l'est par rien : un constat tu ne laisse
  aucune trace, par construction.

## 2. Chiffres

| | Tour 1 | Tour 2 | Tour 3 | **Tour 4** |
|---|---|---|---|---|
| Durée de travail du relecteur | 727 s | 619 s | non mesurable de l'intérieur | **1 321 s** (relevé par la session appelante) |
| Durée des deux axes | non relevée | non relevée | 591 s et 410 s, en parallèle | **524 s et 553 s**, en parallèle |
| Jetons du relecteur | ~135 000 | 82 447 | non mesurable de l'intérieur | **162 263** (relevé par la session appelante) |
| Jetons des deux axes | non relevés | non relevés | 137 548 + 104 194 = 241 742 | **130 090 + 72 547 = 202 637** |
| Appels d'outils | 36 | 24 | 28 (orchestration) + 37 (axes) = 65 | **34** (orchestration, **hors attente**) **+ 35** (axes) = **69** ; **12** appels d'attente pure en sus (§5) |
| Sous-agents ouverts | 2 | 2 | 2 (un par axe) | **2** (un par axe) |
| Constats produits par les axes | non relevé | non relevé | 20 | **17** (9 Conformité, 8 Découpabilité) |
| Écartés ou fusionnés avant pose | non relevé | non relevé | 4 — 1 écarté, 1 subsumé, 2 fusions | **1** — une seule fusion de doublon inter-axes, **0 écarté** |
| **Remarques posées** | 11 | 12 | 16 | **16** |
| Répartition | 3 Conf · 8 Déc | 6 Conf · 5 Déc · 1 hors mandat | 11 Conf · 5 Déc · 0 hors mandat | **9 Conf · 7 Déc · 0 hors mandat** |
| **Violations dures** | 1 | 2 | 12 (11 Conf, 1 Déc) | **7** (4 Conf, 3 Déc) |
| Jugements | 10 | 9 | 4 (tous Déc) | **9** (5 Conf, 4 Déc) |
| Constats hors mandat — justesse | 3 | 1 | 0 | **0** |
| **Remarques nées d'une figure** | 0 | 0 | 5 (4 du schéma §8.3, 1 du §8.4) | **2** (topologie du §8.3, légende de ses couleurs) |
| Remarques visant une **reprise** du tour précédent | — | 0 | 2 | **4** — 3 établies au fil de solde, 1 sur une section réécrite |
| Cases de §1 évaluées | 9 | 12 | 12 | **12** |
| Cases de §1 enfreintes | 1 | non relevé | 6 — capacité, recette, socle, registres, plan/dépendances, profondeur | **1** au sens de la clause (les trois registres) ; **6 cases** portent au moins une divergence — capacité, registres, vertus, dépendances, schéma, profondeur |
| Cases de §2 | 3, dont 2 en aval | 3 | 3, toutes non opposables depuis l'artefact seul | **3** — 2 **tenues** sur les faits d'état fournis, 1 (l'accord de l'humain) structurellement en aval |
| Carte avant / après | — | — | 36 commentaires, 0 ouvert → 52, 16 ouverts | **52 commentaires, 0 ouvert → 68, 16 ouverts** |
| Étiquette posée | `Rework Needed` | `Rework Needed` | `Rework Needed` | **`Rework Needed`** |

Trois lectures à ne pas faire de ce tableau, dont deux héritées du tour 3. **Plus de remarques n'est
pas mieux**, et *autant* de remarques ne veut pas dire *les mêmes* : les seize de ce tour ne
recoupent aucune des seize précédentes, qui sont toutes soldées. **Le rapport jugements / violations
dures s'est inversé une seconde fois** (10-1, 9-2, 4-12, puis 9-7) sans que rien ne mesure le
calibrage d'un relecteur à l'autre — la réserve écrite par le tour 3 vaut telle quelle. Et **la chute
des remarques nées d'une figure**, de 5 à 2, ne mesure pas un relâchement de la clause : les figures
avaient été reprises entre-temps, et la clause a produit sur ce qui restait.

**Complété par la session appelante — et les deux chiffres qu'elle apporte ne se comparent pas aux
tours 1 et 2.** Le trou que le tour 3 avait laissé ouvert est comblé : 1 321 s et 162 263 jetons.
Mais la charge n'est plus la même. Aux tours 1 et 2, le relecteur relisait ; depuis le tour 3, il
relit **et écrit sa fiche**, ce qui inclut lire le `README.md` du dossier, les trois fiches
antérieures, et rédiger sept rubriques. Une durée qui double par rapport au tour 2 ne dit donc rien
d'un tour plus lent. La seule comparaison légitime que ces deux lignes autorisent est **tour 4 contre
tour 5**, si le dispositif est reconduit à l'identique.

Trois précisions sur ce qui est mesuré, faute de quoi la ligne se lira de travers :

- **la métrique exclut les sous-agents d'axe.** Les deux axes pèsent à eux seuls 202 637 jetons, plus
  que les 162 263 portés à la ligne — le compte est bien celui de l'orchestrateur seul. Le total réel
  du tour est de **364 900 jetons**, à comparer aux 241 742 + inconnu du tour 3 ;
- **rien n'établit que les tours 1 et 2 mesuraient la même chose.** Leurs chiffres viennent aussi de
  la session appelante, mais leurs axes n'ayant pas été relevés, on ne peut pas vérifier que
  l'exclusion jouait déjà. La comparaison verticale de cette ligne reste donc suspecte sur ses deux
  premières colonnes ;
- **les appels d'outils ne concordent pas** : la session appelante en compte **49**, le relecteur
  **46** (34 hors attente + 12 d'attente). L'écart de 3 n'est pas expliqué, et il n'est pas lissé
  ici — deux compteurs qui divergent de 6 % sur la même exécution valent d'être notés avant qu'on
  bâtisse une comparaison dessus.

Clause par clause, avec ce qui l'atteste.

| Clause | Tenue | Ce qui l'atteste |
|---|---|---|
| Session neuve, sur l'artefact seul (`revue-spec` §1, `D-039`) | **oui, sous deux réserves nommées** | Le fil de rédaction n'a pas été transmis ; les commentaires de la carte n'ont été lus, par l'orchestrateur, **que pour établir l'état de §2**, et jamais transmis aux axes autrement que sous forme de trois faits chiffrés. ⚠️ Réserve 1 : la mémoire automatique de la session résume l'artefact par ses conclusions (journal 43), inchangé. ⚠️ Réserve 2, **neuve** : fournir « 39 remarques, toutes soldées » est un résumé de l'issue des tours précédents, et c'est l'orchestrateur qui a choisi de le fournir — aucune clause ne l'y autorise ni ne le lui interdit |
| Exactement deux axes, jamais fondus (`revue-spec` §2, `revue` §2) | **oui** | Deux sous-agents lancés dans le même message, chacun ignorant l'existence de l'autre ; deux rapports reçus distinctement, chacun se clôturant sur son propre verdict d'axe ; aucun rapport de synthèse qui reclasse |
| Les douze cases de §1 et les trois de §2, clause par clause | **oui** | L'axe Conformité rend deux tableaux de couverture — douze lignes puis trois — avant sa liste de constats, et nomme pour chaque case où elle se lit dans l'artefact |
| Deux citations par constat (`revue` §3) | **oui** | Chacune des seize remarques porte son référentiel (fichier + clause, ou le second passage de l'artefact quand la contradiction est interne) et l'extrait visé |
| **Confronter chaque figure à la prose** (`revue` §3, `revue-spec` §2) | **oui** | Le mandat des **deux** axes nommait les blocs `mermaid` **et les tableaux**, et demandait de les relire nœud par nœud, flèche par flèche, couleur par couleur, ligne par ligne. Extension par rapport au tour 3, qui ne l'avait demandé qu'à l'axe Conformité et ne mentionnait pas les tableaux. **Quatre constats en sont sortis** — la topologie `SER --> …` du §8.3 (trouvée **par les deux axes**), la légende des couleurs, et deux tableaux : le décompte « sept lignes » du §8.5 et sa dernière ligne |
| Écarter la justesse (`revue-spec` §3) | **oui, sans matière** | Les deux axes ont rendu leur section *hors mandat — justesse* explicitement vide. Aucune ligne des deux rapports ne discute si la spec est un bon choix |
| Étiqueter la confiance (`revue` §5) | **oui** | 7 *violation dure*, 9 *jugement*, aucune ligne ambiguë. Une remarque sans clause à citer (la duplication de saisie) est étiquetée *jugement* et le dit |
| Lister sans réécrire (`revue` §6) | **oui** | `updatedAt` du document à `2026-07-31T15:50:23.879Z`, antérieur à la première remarque ; aucune remarque ne propose un texte de remplacement. Les remarques posent des **questions à revenir poser**, elles n'y répondent pas |
| Deux issues, jamais trois (`revue` §6, `D-051`) | **oui** | 17 constats produits, 16 posés, 1 fusionné dans un autre. Aucun constat n'est écrit ailleurs que sur la carte ; aucune « observation non bloquante » |
| Poser la remarque sur la carte (`revue` §6, `D-045`) | **oui, sans rappel** | 16 remarques posées par `cursus linear comment add`, ancrées avec leur repère ; `open` passe de 0 à 16, `total` de 52 à 68. Aucune sur le document |
| Poser l'étiquette, jamais déplacer (`revue-spec` §4, `revue` §8) | **oui** | `Rework Needed` seul sur le projet, `Review Requested` retiré ; `status` reste `Spec` |
| Pas d'escalade, pas de compteur de tours (`revue-spec` §4) | **oui** | Aucune assignation, aucune tentative de convergence au-delà de ce passage, alors même que c'est le quatrième |

**Une clause n'a pas pu être tenue telle qu'écrite, et une autre l'a été pour la première fois.**

- **Le tour 3 avait relevé que `revue-spec` §4 prescrivait `Done` ; c'est corrigé** (journal 42), et
  le skill lu ce tour-ci dit bien `Human Review Requested`. **Mais le primitif `revue` §8 dit
  toujours `Done` ou `Rework Needed`.** La correction a été appliquée à l'instance et pas au
  primitif, et les deux se lisent dans la même exécution. Sans effet ici — seize remarques imposent
  `Rework Needed` des deux côtés — mais l'écart est intact, déplacé d'un fichier à l'autre.
- **`revue` §2 et §6 se contredisent quand deux axes trouvent le même passage** (journal 44),
  seconde occurrence : la topologie du §8.3 a été trouvée par les deux axes, avec deux conséquences
  différentes — une contradiction interne pour l'un, une frontière d'incrément indécidable pour
  l'autre. Fusionné à la main en une remarque unique portant les deux lectures, sans qu'aucune clause
  l'autorise. Une remarque, deux axes : le tableau des chiffres la compte une fois, du côté
  Conformité.

## 4. Qualité de la sortie

> **À compléter par un tiers.** Le relecteur ne juge pas ce qu'il vient de produire — c'est le motif
> de `D-039`, et le faux accord du binôme de `tickets.md` §6.3. Ce qui suit est **factuel et
> antérieur à toute reprise** : aucune des seize remarques n'a encore reçu de réponse.

**Les faits bruts qu'un tiers utilisera.**

*Le sort des seize remarques n'est pas connu.* Les trois tours précédents affichent 11/11, 12/12 et
16/16 retenues, aucun refus motivé. Ce tour n'a pas encore été repris ; c'est le chiffre manquant.

*Sept violations dures, dont quatre sont des contradictions internes vérifiables sans le dépôt* —
deux passages de l'artefact qui ne peuvent pas être vrais ensemble, chacun cité :

- le schéma du §8.3 fait de `SER` l'**unique appelant** du noyau (`SER --> CAT`, `--> STORE`,
  `--> TASKS`, `--> HOST`, et aucune arête de `MCP` vers le noyau), alors que §8.2 écrit « il donne
  son tour, il ne sait pas ce qui se fait pendant » et que §8.4 dessine `M->>C` en direct ;
- le §6 ouvre par « **Construit** — tout le §4 », alors que le §4 marque deux items d'un ⚠️ disant
  « ce n'est donc pas du socle », et que le registre *tranché non construit* du même §6 les reprend ;
- le §2 annonce que la seconde restriction de la phrase de capacité est « dans ses six premiers
  mots », puis désigne *piloter des workflows*, qui en est le onzième au quatorzième ;
- le §8.5 justifie la dépendance à `Cursus.Trackers` par « les quatre [lignes] des **Connexions
  tracker** », dont la quatrième est celle que le §3 clause 3 déclare n'avoir jamais d'outil.

*Trois violations dures sur l'axe Découpabilité*, dont deux portent sur ce qui n'existe pas :

- l'inventaire de l'annexe, déclaré « le référentiel opposable, et le seul », ne recense **aucune
  lecture pure** — pas de *lister les projets*, *lister les workflows*, *lire les arêtes* — alors que
  §8.2 rend cette dernière obligatoire et que §8.6 confie « la lecture des workflows » au fondateur ;
- le ⚠️ du §8.6 pose « un périmètre d'objets, pas une nature de geste », puis, deux lignes plus bas,
  « le lot 1 ne prend que la lecture des workflows et des runs » — l'objet *workflow* se retrouve
  dans trois lots ;
- l'exemption de la clause 1 emporte la **seule** recette de la lecture en vol, la ligne d'inventaire
  censée la rattraper ne disant pas « en vol » là où ses deux voisines disent « passés ».

*Quatre remarques visent une reprise du tour 3, pas le texte d'origine* — trois établies en
recoupant le fil de solde des remarques du tour 3 : la ligne « sept lignes de l'inventaire en
dépendent » (§8.5), le critère de coupe « périmètre d'objets » (§8.6) et « le fondateur porte la
descente du socle » (§8.6) ont tous trois été **écrits en réponse** à une remarque du tour 3. La
quatrième vise le §8.6 dans son ensemble, entièrement réécrit à ce tour. C'est la seconde fois que ce
mode de défaillance est mesuré, et il est en hausse : 2 au tour 3, 4 ici.

*Le seul constat non posé*, à consigner parce qu'il mesure le bruit : la topologie du §8.3, trouvée
par les deux axes, a été fusionnée en une remarque unique. **Aucun constat n'a été écarté** — 17
produits, 16 posés. Le tour 3 en écartait 4 sur 20.

*Les deux verdicts d'axe*, rendus séparément :

- **Conformité — désaccord.** « Quatre violations dures, dont la plus lourde est que le schéma de
  composition du §8.3 fait du point de passage l'appelant du noyau, ce que §8.2 interdit et que §8.4
  dessine à l'inverse. »
- **Découpabilité — désaccord.** « Le découpage ne peut ni répartir la surface de lecture que le §8.2
  exige et que l'inventaire n'inventorie pas, ni appliquer un critère de coupe qui se contredit dans
  sa propre phrase, ni recetter la lecture en vol que l'exemption de la clause 1 laisse sans clause. »

*Ce que l'axe Découpabilité a produit et qu'aucun tour précédent n'avait produit* : un découpage
complet et écrit — quatre lots, leurs `blockedBy`, une table d'atterrissage des six clauses de
recette et une table des trente lignes d'inventaire. Les points d'achoppement en sont dérivés, et
non énoncés à l'avance. C'est la pièce qui permet à un tiers de vérifier que la tentative a eu lieu,
comme le demande §3 de la DoD (« il se **teste** »).

*Ce que la revue a vérifié dans le dépôt, et qui a tenu* : l'axe Conformité a confronté au code une
douzaine d'assertions de la spec — surface publique de `ProjectHost`, `RunViewModel.Stop()`,
connexion unique de `SqliteRunJournal`, appelant unique de `SqliteProjectHost.Open`, emplacement de
`ProjectWorkspace`, douze types publics dans `Tasks/`, `WorkflowDraft.RenameStep`, `ProvisionAsync`,
quatre projets de production, version d'Avalonia, et les quatre renvois à `architecture.md`.
**Aucune n'a pris la spec en défaut.** C'est un renversement par rapport au tour 3, où trois
assertions sur trois étaient démenties.

## 5. Frictions

Journal des frictions, entrées **43** (la mémoire automatique dément la clause de session neuve —
inchangé), **44** (deux axes sur le même passage, sans règle — **seconde occurrence**, donc le seuil
de `D-039` est atteint), **45** (la pièce la plus contestable est la moins citable — **seconde
occurrence** : les deux remarques nées du §8.3 sont, comme au tour 3, ancrées sur la prose voisine
faute de pouvoir viser une ligne de nœud), **47** (rien ne tient la forme d'un artefact d'un tour à
l'autre — ce tour en est la **première mise à l'épreuve**, le prompt ayant imposé de prolonger un
tableau existant plutôt que d'écrire librement).

**Trois frictions neuves**, numérotées au journal par la session appelante et **non recopiées ici** :
**48** (corriger une instance laisse le primitif porter l'ancienne clause — troisième occurrence du
motif des entrées 41 et 42, avec son aggravation propre), **49** (l'attente d'un sous-agent coûte
douze appels d'outils qui ne produisent rien, et fausse la ligne du tableau qui les compte), **50**
(la clause de session neuve n'a pas de doctrine sur les faits d'état qu'il faut bien fournir pour
instruire §2 — le geste a marché, et c'est ce qui le rend gênant).

## 6. Ce que le tour a changé

- **Le remède du tour 3 a été élargi et il a de nouveau produit.** La clause de confrontation
  figure ↔ prose a été passée aux **deux** axes et étendue **aux tableaux** — le tour 3 ne l'avait
  donnée qu'à Conformité, et seulement pour les blocs `mermaid`. Quatre des seize remarques en
  viennent, dont deux qui ne seraient pas nées d'une lecture de schéma : le décompte « sept lignes »
  et la dernière ligne du tableau des dépendances.
- **Le mode de défaillance nommé par le tour 3 est confirmé et mesuré.** « Une remarque ancrée sur un
  passage se solde sur ce passage, alors que le défaut peut vivre ailleurs » — quatre remarques de ce
  tour visent du texte écrit *en réponse* au tour précédent, contre deux au tour 3. Deux occurrences
  font une tendance mesurée, pas encore une règle.
- **Le premier gabarit inter-tours a été imposé et il a tenu.** Le tableau des chiffres du tour 3 a
  été prolongé d'une colonne au lieu d'être réinventé ; c'est la piste ouverte par le journal 47,
  éprouvée pour la première fois — sur une fiche `rex/`, pas encore sur une spec.
- **La spec, elle, n'a pas changé** : la revue liste, elle ne réécrit pas. Seize remarques ouvertes
  attendent la reprise, et l'étiquette `Rework Needed` dit de ne pas tirer.
- **Rien n'a été changé dans les skills par ce tour.** La friction 44 atteint pourtant sa seconde
  occurrence, seuil que `D-039` pose pour écrire — c'est au binôme de décider si le primitif `revue`
  gagne une règle de recouvrement inter-axes, et si sa clause §8 rejoint `cycle-feature.md`.

## 7. Verdict pour `revue-spec`

> **À compléter par un tiers**, avec la même réserve qu'au §4 : les quatre issues de `D-043` amendé —
> *promu*, *corrigé par le journal*, *retiré*, *tué par un fait* — supposent un juge qui n'est pas
> l'exécutant.

**Les faits bruts qu'un tiers utilisera.**

- **Quatre tours, 55 remarques posées** (11 + 12 + 16 + 16). Les trois premiers affichent 39 retenues
  sur 39, aucun refus motivé. **Le sort des 16 de ce tour n'est pas connu** au moment d'écrire.
- **Le skill a été relancé sans béquille pour la troisième fois**, et le §3 ci-dessus donne pour
  chacune des douze clauses la pièce qui atteste qu'elle a tenu.
- **La réserve du tour 3 sur l'étiquette est levée côté instance et intacte côté primitif** :
  `revue-spec` §4 dit désormais `Human Review Requested`, `revue` §8 dit toujours `Done`. Le cas où
  l'écart mord — un tour sans aucune remarque — ne s'est toujours pas présenté en quatre tours.
- **La série ne converge toujours pas** : 11, 12, 16, 16. La quatrième valeur est la première à ne
  pas monter, et elle porte quatre remarques nées de la reprise précédente. Un tiers dispose
  maintenant de quatre points pour dire si le signal d'arrêt peut venir de la boucle agent, ou s'il
  ne peut venir que de l'humain au temps ⑤ — c'est l'hypothèse écrite par le tour 3.
- **Un déplacement dans la nature des trouvailles, que le tiers seul peut apprécier.** Au tour 3,
  trois assertions de la spec sur trois étaient démenties par le dépôt. Ici, une douzaine
  d'assertions ont été confrontées au code et **aucune n'a été prise en défaut** : les sept
  violations dures sont toutes des **contradictions internes**, opposables sans le dépôt. Reste à
  dire si cela signifie que l'artefact a cessé de mentir sur le code et se contredit encore, ou que
  ce relecteur a moins cherché du côté du code.
- **Deux réserves neuves, et aucune n'est dans le geste de relecture** : corriger une instance laisse
  le primitif porter l'ancienne clause, et la clause de session neuve n'a pas de doctrine sur les
  faits d'état qu'il faut bien fournir pour instruire §2.
- **Ce que ce tour n'établit toujours pas** : rien du cycle au-delà du temps ②. Ce qui suit
  `Rework Needed` est la reprise par le binôme, et elle n'a jamais été jouée par un skill.
