# 2026-07-31 — `revue-spec`, troisième exécution

> Troisième tour sur le même artefact, *Un agent pilote Cursus*, le même jour que les deux premiers.
> Une différence, et c'est celle que le tour mesure : **le garde-fou réclamé par la fiche du tour 2
> a été écrit avant de relancer.** `revue` §3 porte désormais *« Confronter chaque figure à la prose
> qui l'entoure, sans exception »*, et `revue-spec` §2 en fait un avertissement propre à l'axe
> Conformité.
>
> ⚠️ **Comparabilité.** Les chiffres de remarques et de conformité se comparent aux deux tours
> précédents. Ceux de **coût et de durée ne se comparent pas** : les tours 1 et 2 mesuraient le
> relecteur *depuis la session appelante*, qui voyait sa durée et ses jetons ; ce tour-ci est écrit
> **par** le relecteur, qui ne voit ni l'un ni l'autre pour lui-même. Ce qu'il peut mesurer est
> détaillé au §2, et n'occupe pas la même ligne.
>
> ⚠️ **Les rubriques 4 et 7 sont laissées à un tiers.** Le relecteur ne juge pas sa propre sortie —
> c'est le motif même de `D-039`, et se décerner un verdict le viderait. Elles portent les faits
> bruts, pas leur appréciation.

## 1. Ce qui a tourné

`revue-spec` sur le document `Spec — Un agent pilote Cursus`, attaché au projet Linear du même nom,
en colonne `Spec` + `Review Requested`. La carte portait **36 commentaires, 0 ouvert** — les deux
tours précédents, tous soldés.

**Le chemin d'exécution, et où est la trace qu'il a servi** : un sous-agent lancé depuis la session
du binôme, qui a **invoqué le skill** `revue-spec`, lequel a passé le mandat au primitif `revue`
avec **deux axes ouverts en sous-agents séparés**, lancés dans le même message donc en parallèle,
aucun ne voyant le rapport de l'autre. Traces vérifiables : seize commentaires sur la **carte**
(jamais sur le document, `D-045`), chacun avec son repère calculé, son axe et son étiquette de
confiance ; l'étiquette `Rework Needed` posée sur le projet ; la colonne `Spec` inchangée.

**La commande, verbatim et rejouable** — depuis la racine du dépôt, agent `general-purpose`,
en arrière-plan :

```
Tu prends une revue de spec sur le backlog Linear du projet Cursus (espace `cursus-app`, équipe `CUR`).

La carte à relire : le **projet** Linear « Un agent pilote Cursus », actuellement en colonne `Spec`
et portant l'étiquette `Review Requested`. Son document `Spec` y est attaché.

Invoque le skill `revue-spec` et suis son protocole jusqu'au bout.

Le dépôt est à la racine du projet. Le référentiel de conformité est `docs/methode/dod/feature/spec.md`.

Puis, **une fois la revue close**, écris sa fiche de retour d'expérience :

- fichier `docs/methode/rex/2026-07-31-revue-spec-tour-3.md` ;
- rubriques **fixes** de `docs/methode/rex/README.md`, dans l'ordre, aucune omise — lis-le avant
  d'écrire ;
- ⚠️ **tu ne juges pas ta propre sortie.** Remplis les rubriques **1, 2, 3, 5 et 6** : ce qui a
  tourné (avec la commande verbatim et rejouable), tes chiffres, ta conformité au protocole clause
  par clause **avec ce qui l'atteste**, les frictions rencontrées, ce que le tour a changé. Pour les
  rubriques **4 (qualité de la sortie)** et **7 (verdict pour le skill éprouvé)**, écris uniquement
  « À compléter par un tiers » suivi des **faits bruts** qu'un tiers utilisera pour les remplir. Un
  relecteur qui se décerne son propre verdict est exactement le piège que ce dossier existe pour
  éviter ;
- deux fiches existent déjà pour les tours 1 et 2 dans le même dossier : prends-les pour gabarit,
  et rends tes chiffres **comparables aux leurs** ;
- ⚠️ **aucun chemin personnel** dans la fiche — ce dépôt est public ;
- **ne commite pas.**

En retour, rends-moi : le nombre de remarques posées, leur axe, l'étiquette que tu as posée, et —
en une phrase chacune — les remarques les plus lourdes.
```

**Le prompt reste allégé** des deux béquilles que le tour 1 portait (session neuve, ne pas déplacer
la carte) — comme au tour 2, et pour la même raison. Il gagne en revanche une charge que les deux
premiers n'avaient pas : **écrire la fiche `rex/` dans la foulée**, avec l'interdit d'auto-jugement.
C'est une réponse directe à l'entrée 40 du journal (*une fiche écrite le lendemain perd ce qu'aucune
trace ne rattrape*), et il faut le noter comme une variation du chemin d'exécution : la session qui
relit n'est plus seulement une session de revue.

**Une matérialisation intermédiaire** que les deux tours précédents n'ont pas documentée : le
document Linear a été **écrit dans un fichier de travail hors du dépôt** avant d'être passé aux deux
axes, pour qu'ils citent des passages exacts plutôt que du markdown ré-échappé par l'API. C'est un
détail d'outillage, et c'est ce qui rend les citations *verbatim* fiables au moment de poser.

## 2. Chiffres

| | Tour 1 | Tour 2 | **Tour 3** |
|---|---|---|---|
| Durée de travail du relecteur | 727 s | 619 s | **non mesurable de l'intérieur** |
| Durée des deux axes | non relevée | non relevée | **591 s et 410 s**, en parallèle |
| Jetons du relecteur | ~135 000 | 82 447 | **non mesurable de l'intérieur** |
| Jetons des deux axes | non relevés | non relevés | **137 548 + 104 194 = 241 742** |
| Appels d'outils | 36 | 24 | **28** (orchestration) **+ 37** (axes) = **65** |
| Sous-agents ouverts | 2 | 2 | **2** (un par axe) |
| Constats produits par les axes | non relevé | non relevé | **20** (14 Conformité, 6 Découpabilité) |
| Écartés ou fusionnés avant pose | non relevé | non relevé | **4** — 1 écarté, 1 subsumé, 2 fusions de doublons inter-axes |
| **Remarques posées** | 11 | 12 | **16** |
| Répartition | 3 Conf · 8 Déc | 6 Conf · 5 Déc · 1 hors mandat | **11 Conf · 5 Déc · 0 hors mandat** |
| **Violations dures** | 1 | 2 | **12** (11 Conf, 1 Déc) |
| Jugements | 10 | 9 | **4** (tous Déc) |
| Constats hors mandat — justesse | 3 | 1 | **0** |
| **Remarques nées d'une figure** | 0 | 0 | **5** (4 du schéma §8.3, 1 du §8.4) |
| Remarques visant une **reprise** du tour précédent | — | 0 | **2** |
| Cases de §1 évaluées | 9 | 12 | **12** |
| Cases de §1 enfreintes | 1 | non relevé | **6** — capacité, recette, socle, registres, plan/dépendances, profondeur |
| Cases de §2 | 3, dont 2 en aval | 3 | **3**, toutes **non opposables depuis l'artefact seul** |
| Carte avant / après | — | — | **36 commentaires, 0 ouvert → 52, 16 ouverts** |
| Étiquette posée | `Rework Needed` | `Rework Needed` | **`Rework Needed`** |

Deux lectures à ne pas faire de ce tableau. **Plus de remarques n'est pas mieux** : l'artefact a
gagné une section entière entre le tour 1 et le tour 2, et il a été repris deux fois — un document
plus long offre plus de prise. Et **le basculement du rapport jugements / violations dures** (10-1,
puis 9-2, puis 4-12) ne mesure pas une sévérité croissante : les figures produisent presque
mécaniquement des violations dures, puisqu'une contradiction interne est opposable sans clause
externe (`revue` §3).

## 3. Conformité au protocole

Clause par clause, avec ce qui l'atteste.

| Clause | Tenue | Ce qui l'atteste |
|---|---|---|
| Session neuve, sur l'artefact seul (`revue-spec` §1, `D-039`) | **oui, sous réserve nommée** | Le fil de rédaction n'a pas été transmis, et les 36 commentaires de la carte n'ont été lus **qu'après** que les deux axes eurent rendu leurs rapports — l'ordre est visible dans le déroulé. ⚠️ La réserve n'est pas dans le skill : voir journal 43 |
| Exactement deux axes, jamais fondus (`revue-spec` §2, `revue` §2) | **oui** | Deux sous-agents lancés dans le même message, chacun ignorant l'existence du rapport de l'autre ; deux rapports reçus distinctement, aucun rapport de synthèse qui reclasse |
| Les douze cases de §1 et les trois de §2, clause par clause | **oui** | L'axe Conformité rend un tableau case par case avant sa liste de constats |
| Deux citations par constat (`revue` §3) | **oui** | Chacune des seize remarques porte son référentiel (fichier + clause, ou le second passage de l'artefact) et l'extrait visé |
| **Confronter chaque figure à la prose** (`revue` §3, `revue-spec` §2) | **oui** | Le mandat de l'axe Conformité nommait les deux blocs `mermaid` et demandait de relire chaque nœud et chaque arête. **Cinq remarques en sont sorties** — nœud hors du cadre `proc`, sous-graphe `fen` non colorié, `socle` et `ETAT` qui tranchent ce que §8.6 laisse ouvert, `SER --> CAT` unique arête, et le geste d'authoring déplacé au §8.4 |
| Écarter la justesse (`revue-spec` §3) | **oui, sans matière** | Les deux axes ont rendu « rien » sous *hors mandat — justesse*. Aucune ligne des deux rapports ne discute si la spec est un bon choix |
| Étiqueter la confiance (`revue` §5) | **oui** | 12 *violation dure*, 4 *jugement*, aucune ligne ambiguë |
| Lister sans réécrire (`revue` §6) | **oui** | Document Linear inchangé — `updatedAt` du document est antérieur à la première remarque ; aucune remarque ne propose un texte de remplacement |
| Deux issues, jamais trois (`revue` §6, `D-051`) | **oui** | 4 constats sur 20 ne sont **pas** écrits sur la carte, et ils ne sont écrits nulle part ailleurs non plus. Aucune « observation non bloquante » |
| Poser la remarque sur la carte (`revue` §6, `D-045`) | **oui, sans rappel** | 16 remarques posées par `cursus linear comment add`, ancrées avec leur repère ; `open` passe de 0 à 16. Aucune sur le document |
| Poser l'étiquette, jamais déplacer (`revue-spec` §4, `revue` §8) | **oui** | `Rework Needed` seul sur le projet, `Review Requested` retiré ; `status` reste `Spec` |
| Pas d'escalade, pas de compteur de tours (`revue-spec` §4) | **oui** | Aucune assignation, aucune tentative de convergence au-delà de ce passage |

**Deux clauses n'ont pas pu être tenues telles qu'écrites, et le motif n'est pas le relecteur.**

- **`revue-spec` §4 prescrit `Done` ou `Rework Needed` ; le cycle ne lui laisse plus ce choix.**
  `cycle-feature.md` §4 donne au relecteur de `Spec` les sorties `Rework Needed` ou
  `Human Review Requested`. Sans effet ici — seize remarques imposent `Rework Needed` des deux
  côtés — mais l'écart est réel, et il porte exactement sur le cas *sans remarque*. Journal 42.
- **`revue` §2 et §6 se contredisent quand deux axes trouvent le même passage.** Deux fois sur vingt
  constats. Fusionné à la main, sans qu'aucune clause l'autorise. Journal 44.

## 4. Qualité de la sortie

> **Complété après la reprise**, par le binôme auteur. La mesure est objective — combien de
> remarques il retient, combien il refuse — mais l'appréciation ne l'est pas : celui qui juge ici est
> celui que la revue visait. À lire avec cette réserve.

**Seize remarques sur seize retenues, aucun refus motivé.** Troisième tour d'affilée à ce score
(11/11, 12/12, 16/16). Cinq exigeaient un arbitrage qui n'appartenait pas au relecteur et qui a été
rendu par l'humain ; les onze autres se corrigeaient sur pièces.

**Trois affirmations ont été vérifiées dans le code avant reprise, et les trois tenaient** : le WAL
qui ne protège rien faute d'une seconde connexion, les douze types de `Tasks/` là où la spec en
comptait huit, et le `ProjectWorkspace` — et non le host — que résout réellement `App.axaml.cs`.

**Le résultat le plus fort de ce tour n'est pas un chiffre mais une provenance.** Cinq remarques
naissent des blocs `mermaid`, que deux tours avaient traversés sans rien y voir. La clause ajoutée à
`revue` §3 le matin même — *une figure n'illustre pas, elle affirme* — a produit exactement ce pour
quoi elle a été écrite, sur l'artefact qui l'avait rendue nécessaire. Parmi ces cinq : une figure qui
tranchait par un trait deux questions que le §8.6 déclarait ouvertes.

**Deux remarques visent des reprises du tour 2, pas le texte d'origine**, et elles désignent un mode
de défaillance qu'aucune fiche n'avait relevé : corriger « plus `Tasks/` » en « huit types publics »
a remplacé un flou par une erreur, et solder une remarque sur l'annexe a laissé le §6 porter le même
défaut. **Une remarque ancrée sur un passage se solde sur ce passage, alors que le défaut peut vivre
ailleurs.**

⚠️ **Ce que ce tour ne permet pas de conclure.** Douze violations dures contre deux au tour
précédent, avec l'axe Conformité entièrement en dur. Deux lectures restent ouvertes — les reprises du
tour 2 ont réellement cassé six cases sur douze, ou ce relecteur calibre plus sévèrement la frontière
entre *violation dure* et *jugement*. Les deux échantillons vérifiés étaient bien des violations ;
deux ne font pas douze, et rien dans le dispositif ne mesure le calibrage d'un relecteur à l'autre.

### Les faits bruts relevés par le relecteur

**À compléter par un tiers.** Le relecteur ne juge pas ce qu'il vient de produire ; c'est le motif de
`D-039`, et un verdict auto-décerné vaudrait exactement ce que vaut le faux accord du binôme
(`tickets.md` §6.3). Ce qui suit est la matière brute, sans appréciation.

**Ce qu'il y a à mesurer**, quand la reprise aura eu lieu : combien des **16** remarques sont
retenues, combien refusées avec leur raison écrite. Les deux tours précédents affichent 11/11 et
12/12.

**Les faits bruts, par catégorie.**

*Trois affirmations de la spec que le dépôt dément* — vérifiées dans le code par l'orchestrateur,
pas seulement rapportées par l'axe :

- « **La base est en WAL, posé au schéma : un lecteur ne gêne pas l'écrivain** » (§4, registre
  *construit*). `architecture.md` §4.13 écrit l'inverse sur ce même journal : « La **lecture
  concurrente d'un run en cours** (connexion de lecture séparée en WAL) reste **non supportée** ».
  `SqliteRunJournal` détient une seule `SqliteConnection`, commentée « non thread-safe » ; le verrou
  ne couvre que `Append` ; le pragma WAL ne protège qu'entre connexions distinctes, et il n'y en a
  pas deux. La clause 1 de la recette (« le lancer, lire sa trajectoire ») pose la surface MCP
  exactement sur ce cas.
- « **les huit types publics de `Tasks/`** » (§4). Les huit noms cités sont les huit **fichiers** du
  dossier. `TrackerExceptions` n'est pas un type — le fichier en porte trois — et le dossier compte
  **douze** types publics, `LinearBinding` et `LinearConnection` compris.
- « **Aucune dépendance sortante ajoutée** » (§8.5). **`Cursus.Trackers` n'est nommé nulle part dans
  la spec** — ni au §4, ni dans le cadre `noyau` du schéma §8.3, ni dans la table des dépendances —
  alors que sept lignes de l'inventaire de l'annexe en dépendent.

*Cinq remarques nées des figures, sur un artefact où c'était le trou nommé du tour 2.* La fiche du
tour 2 §7 posait le remède (« À écrire avant un tour 3 ») ; il a été écrit, et il a produit. Le
détail est au §3 ci-dessus.

*Deux remarques qui visent une reprise du tour 2, pas le texte d'origine.* C'est le fait le plus
inattendu du tour, et le seul qu'aucune fiche précédente ne pouvait porter :

- la remarque du tour 2 sur le décompte des types est soldée par « L'annexe cesse de parler des « six
  types du §4 » ». L'annexe a bien été corrigée ; **le §6 dit toujours « aucun des six types »**,
  face à un §4 qui en énumère cinq. La divergence a changé de section, pas disparu ;
- la même solde écrit « Vérifié — `Tasks/` contient bien huit **fichiers** publics », et le document
  a écrit « huit **types** publics ». La reprise a introduit le fait faux que la remarque suivante
  relève.

*Une contradiction que l'axe a produite et que l'orchestrateur a écartée*, à consigner parce qu'elle
mesure le bruit : le nœud `SER` colorié en ambre alors que le §6 dit qu'aucune synchronisation
n'existe. Écartée parce que la légende du schéma la réconcilie explicitement (« doit exister pour de
bon plutôt que d'être un effet du thread UI ») — l'auteur avait pré-répondu. 1 constat sur 20.

*Le verdict des deux axes* : désaccord sur les deux. Conformité — six cases de §1 enfreintes, dont
trois affirmations démenties par le dépôt, plus sept contradictions internes dont cinq portées par
les figures. Découpabilité — le découpage bute sur deux frontières que la spec ne trace pas
(fondateur ⇄ socle partagé, et « lecture seule » ⇄ lots de domaine) et ne peut placer ni la clause 1
ni la clause 4 de la recette sans trancher à la place de l'auteur.

## 5. Frictions

Journal des frictions, entrées **42** (le skill prescrit une étiquette que `D-050` lui a retirée),
**43** (la mémoire automatique dément la clause de session neuve), **44** (deux axes sur le même
passage, sans règle), **45** (la pièce la plus contestable est la moins citable).

## 6. Ce que le tour a changé

- **Le garde-fou du tour 2 est éprouvé.** `revue` §3 et `revue-spec` §2 avaient gagné, entre les deux
  tours, la clause de confrontation figure ↔ prose. Elle a produit **cinq des seize remarques**,
  toutes en violation dure, sur un artefact où les deux tours précédents avaient lu les mêmes blocs
  sans rien y trouver. C'est le seul changement du dispositif entre le tour 2 et le tour 3, et c'est
  le seul auquel un écart de résultat puisse s'imputer.
- **Le journal gagne quatre entrées** (42 à 45), dont deux qui ne visent pas le relecteur mais le
  dispositif autour de lui : une divergence skill ↔ cycle laissée par `D-050`, et un canal d'ancrage
  — la mémoire automatique de la session — qu'aucune clause de skill ne peut fermer.
- **La spec, elle, n'a pas encore changé** : la revue liste, elle ne réécrit pas. Seize remarques
  ouvertes attendent la reprise, et l'étiquette `Rework Needed` dit de ne pas tirer.
- **Rien n'a été changé dans les skills par ce tour.** Les deux frictions qui le mériteraient
  (entrées 42 et 44) sont consignées, pas corrigées — `D-039` demande deux ou trois passages avant
  d'écrire, et c'est le premier passage pour chacune.

## 7. Verdict pour `revue-spec`

> **Complété après la reprise**, avec la même réserve qu'au §4.

**Promu, et confirmé une seconde fois** — mais le verdict le plus utile porte sur autre chose que le
skill.

Trois tours, **39 remarques posées, 39 retenues, aucun refus motivé.** Le skill tient ses clauses
sans les béquilles du tour 1, et le remède écrit après le tour 2 a produit dès son premier usage.
C'est le critère de `D-043`, atteint trois fois.

**Ce que la série dit et que le skill ne contrôle pas : la boucle ne converge pas.** 11, puis 12,
puis 16. Chaque reprise crée sa part de défauts neufs — deux des seize en viennent directement — et
un quatrième tour trouverait probablement encore quelque chose. Ce n'est pas un échec du relecteur :
c'est la limite d'un artefact de cette taille relu contre une DoD de douze cases. **Le signal
d'arrêt ne viendra pas de la boucle agent**, qui a montré trois fois qu'elle sait toujours produire ;
il viendra de l'humain, au temps ⑤.

**Trois réserves, aucune sur le geste de relecture** :

- `revue-spec` §4 prescrivait `Done` là où `D-050` réserve ce verdict à l'humain — **corrigé le jour
  même** (journal 42) ;
- la clause de session neuve est démentie par la mémoire de projet, qui se charge avant toute lecture
  et résume l'artefact par ses conclusions (journal 43). **Aucune clause de skill ne peut fermer ce
  canal** — c'est la façon dont la session est construite qui devra changer, et cela vaut
  rétroactivement pour les trois tours ;
- `revue` §2 et §6 se contredisent quand deux axes trouvent le même passage (journal 44), et le motif
  est structurel : plus les axes sont bons, plus ils se recouvrent.

**Les faits bruts relevés par le relecteur.** Les quatre issues de `D-043` amendé — *promu*, *corrigé par le
journal*, *retiré*, *tué par un fait* — supposent un juge qui n'est pas l'exécutant.

**Les faits bruts qu'un tiers utilisera.**

- **Trois tours, 39 remarques posées** (11 + 12 + 16). Les deux premiers affichent 23 retenues sur
  23, aucun refus motivé. **Le sort des 16 de ce tour n'est pas connu au moment d'écrire** — la
  reprise n'a pas eu lieu, et c'est le chiffre qui manque pour trancher.
- **Le skill a été relancé sans béquille pour la seconde fois** : les deux clauses que le tour 1
  rappelait à la main ne sont pas dans le prompt, et le §3 ci-dessus donne pour chacune la pièce qui
  atteste qu'elle a tenu.
- **La réserve du tour 2 est levée, et son remède a produit** : cinq remarques nées des figures là où
  deux tours en avaient trouvé zéro. Reste à un tiers de dire si ces cinq remarques valent — le
  relecteur ne peut pas le dire de ses propres constats.
- **Deux réserves neuves, et aucune des deux n'est dans le skill** : `revue-spec` §4 prescrit une
  étiquette que `cycle-feature.md` ne lui laisse plus (journal 42), et la clause de session neuve de
  `revue-spec` §1 est démentie par un canal que le skill ne connaît pas (journal 43). La seconde
  touche la condition qui donne sa valeur à toute la mécanique.
- **Une réserve qui vise le primitif, pas l'instance** : `revue` §2 et §6 se contredisent dès que
  deux axes trouvent le même passage (journal 44). Le motif est structurel — plus les axes sont bons,
  plus ils se recouvrent — donc il reviendra.
- **Ce que ce tour n'établit toujours pas** : rien du cycle au-delà du temps ②. `D-050` a supprimé
  les temps ③ et ④ de `Spec` ; ce qui suit `Rework Needed` est la reprise par le binôme, et elle n'a
  jamais été jouée par un skill.
