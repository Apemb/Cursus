# Méta-analyse des retours de revue — « Un agent pilote Cursus »

> ## ⚠️ Ce document n'est pas une fiche de retour d'expérience
>
> **Il n'en porte pas les rubriques fixes, et il ne se compare à aucune autre.** Les onze fiches de
> ce dossier mesurent chacune **une exécution** ; celle-ci regarde **une série entière**, par-dessus
> les fiches, et répond à une question qu'aucune d'elles ne peut poser depuis l'intérieur d'un tour :
> *est-ce que ces tours trouvent encore quelque chose qui vaille leur coût ?*
>
> **C'est une conclusion partielle, et chacun de ces trois mots compte :**
>
> - **partielle** — le §4 liste ce qu'elle ne peut pas établir, et cette liste pèse lourd. Le
>   contrefactuel est **reconstruit, jamais observé** : aucun incrément de cette feature n'a été
>   planifié ni codé. Surtout, **le plan de design — l'échelle qui vit entre la spec et le code — est
>   absent du corpus**, et une part inconnue des remarques dites « coûteuses » y aurait été attrapée
>   pour rien ;
> - **sur une série** — celle de `revue-spec`, **un seul artefact, écrit par une seule main**. Rien
>   ici ne se transpose tel quel à `revue-discovery`, `revue-plan` ou `revue-code`, qui n'ont pas été
>   mesurés ainsi ;
> - **conclusion** — elle ne recommande rien et ne juge aucun artefact. Ce qui a été décidé à sa
>   lecture vit dans `decisions.md`, pas ici.
>
> **Pourquoi elle vit dans ce dossier quand même** : la question du dossier est *est-ce que ça
> progresse ?*, et c'est exactement celle-ci — posée à l'échelle où « progresser » cesse de vouloir
> dire « mieux qu'au tour d'avant ». Le nommage suit la convention (date en tête, l'ordre du dossier
> reste l'ordre des tours) ; le titre dit `analyse-serie` et non `tour-N`, ce qui suffit à ne pas la
> confondre.
>
> **Le tableau ligne-à-ligne du §1 est conservé délibérément**, malgré sa longueur : sans lui, les
> agrégats du §2 seraient des totaux invérifiables — le défaut exact que la friction 63 a relevé dans
> la spec, et qu'il serait absurde de commettre dans le document qui l'analyse.

> Classement des **124 remarques racines** posées sur la carte projet, réparties en 2 tours de
> `revue-discovery` et 8 tours de `revue-spec`. Ce document ne juge aucun artefact et ne recommande
> rien : il classe des retours déjà produits, pour que le binôme dispose de matière chiffrée.

## 0. Constitution du corpus, et comment les tours ont été reconstitués

La carte porte **235 commentaires** : **124 racines** (les remarques) et **111 réponses** (les
soldes). Les réponses ne sont pas classées — ce ne sont pas des remarques —, mais elles ont été
lues intégralement, parce qu'elles disent souvent ce que la reprise a *effectivement* changé, ce qui
est la seule façon honnête de renseigner les colonnes *Portée* et *Née d'une reprise*.

`cursus linear comment list` ne rend pas les dates. Les tours ont été reconstitués par les
horodatages de l'API (`createdAt`), qui se répartissent en **dix grappes** séparées de plusieurs
heures, chacune posée en quelques dizaines de secondes :

| Grappe | Date | Racines | Recoupement |
|---|---|---|---|
| Discovery 1 | 2026-07-30 17:27 | 7 | fiche REX : « 7 — 4 dures, 3 jugements » ✔ |
| Discovery 2 | 2026-07-30 18:57 | 6 | fiche REX : « 6 — 1 dure, 5 jugements » ✔ |
| Spec 1 | 2026-07-31 12:17 | 11 | ✔ |
| Spec 2 | 2026-07-31 14:02 | 12 | ✔ |
| Spec 3 | 2026-07-31 15:22 | 16 | ✔ |
| Spec 4 | 2026-07-31 16:33 | 16 | ✔ |
| Spec 5 | 2026-08-01 15:08 | 12 | ✔ |
| Spec 6 | 2026-08-01 16:38 | 19 | ✔ |
| Spec 7 | 2026-08-02 08:33 | 12 | ✔ |
| Spec 8 | 2026-08-02 12:37 | 13 | ✔ |

La série des dix grappes reproduit exactement les décomptes des onze fiches de
`docs/methode/rex/` (Discovery 3 a posé **0** remarque et n'apparaît donc pas). L'attribution est
donc sûre, pas inférée.

**Aucune remarque n'a été écartée comme « non-remarque ».** Les 124 racines sont toutes des
constats ; les accusés et les reprises sont tous des enfants (`parentId` non nul), et la CLI les
rend à plat, ce qui rendait le tri mécanique.

### Amendements à la taxonomie, déclarés

1. **Ajout d'un scope `G — coquille`**, une seule occurrence (`S4-059`, une phrase dupliquée). Aucune
   des six catégories ne l'accueille, et la remarque elle-même écrit « aucune clause à citer ».
2. **`invisible au code` est lu comme « le code ne l'aurait jamais signalée »**, ce qui couvre la
   lisibilité du document *et* l'organisation du travail (maille d'incrément, atterrissage d'une
   clause de recette, rang d'une carte). Sans cet élargissement, une trentaine de remarques
   d'ordonnancement tomberaient toutes en `indécidable`, ce qui masquerait le fait au lieu de le
   mesurer.
3. **Règle d'exclusivité** entre les deux premières valeurs de survie, parce que beaucoup de
   remarques satisfont les deux définitions : `se serait vue seule` = le code la révèle **tôt et sans
   dégât** ; `coûteuse si tardive` = le code la révèle **aussi**, mais après qu'on eut travaillé sur
   la mauvaise base. C'est la lecture qui répond à la question du binôme (« pour moins cher »).
4. **Le référentiel opposé prime le sujet**, comme demandé. Conséquence assumée : la case de DoD
   « le document ne se contredit pas » produit des `A`, pas des `B` ; les cases de gabarit, de
   registres et de généalogie produisent des `B`.

---

## 1. Le tableau de classement

Colonnes : **Tour** · **Repère** · **Scope** · **Portée** (S = structurante, R = rédactionnelle) ·
**Survie au code** (`seule` / `coûteuse` / `invisible` / `indéc.`) · **Reprise ?**

⚠️ La spec a été **renumérotée au tour 5** (refonte sur le gabarit de `D-054`). Les repères des
tours 1–4 (§1…§8.6) et ceux des tours 5–8 (§1.1…§3.3, annexes A/B/C) ne désignent pas les mêmes
sections.

### Discovery — tours 1 et 2 (13 remarques)

| # | Tour | Repère | Scope | P. | Survie | Repr. | Note |
|---|---|---|---|---|---|---|---|
| D1-001 | D1 | §2 — place dans la trajectoire | B | R | invisible | non | Case de DoD non répondue |
| D1-002 | D1 | §2 — « à porter honnêtement » | B | R | invisible | non | Méta-commentaire de méthode |
| D1-003 | D1 | §2 — renvoi sans lien | B | R | invisible | non | Case §5 « les références sont des liens » |
| D1-004 | D1 | §3 — « le SDK C# est stable » | B | R | invisible | non | Verdict de faisabilité tranché hors étape ; le référentiel est la frontière Discovery/Spec, pas le fait |
| D1-005 | D1 | §4 — « atteint donc… pas le déclenchement » | B | R | invisible | non | Piste dépréciée par une conséquence |
| D1-006 | D1 | §3 — appui inégal entre pistes | B | R | invisible | non | Ordre de préférence non énoncé |
| D1-007 | D1 | §4 — « le constat n'écarte aucune piste » | B | R | invisible | non | Glose sur le gabarit |
| D2-008 | D2 | §3 — « rien n'est à construire » | B | R | invisible | **oui** | Vise l'échelle de coût laissée par la reprise du tour 1 |
| D2-009 | D2 | §3 — appui MCP subsistant | B | R | invisible | **oui** | « Tenu en volume, pas en direction » — vise explicitement l'engagement du tour 1 |
| D2-010 | D2 | §3 — « des fichiers écrits ne la traversent pas » | B | R | invisible | non | Objection de clôture |
| D2-011 | D2 | §3 — « la base est en WAL, donc… » | B | R | invisible | non | ⚠️ L'objection est la **prématurité**, pas la fausseté ; le fait s'avérera faux quatre tours plus tard (`S3-040`) |
| D2-012 | D2 | §3 — « c'est ce qui se passe aujourd'hui » | B | R | invisible | non | |
| D2-013 | D2 | §3 — jargon d'architecture non introduit | B | R | invisible | non | Lisibilité depuis le tracker |

### Spec — tour 1 (11 remarques)

| # | Repère | Scope | P. | Survie | Repr. | Note |
|---|---|---|---|---|---|---|
| S1-014 | §1 — coût absolu de la piste retenue | B | R | invisible | non | Case « options arbitrées avec coût » |
| S1-015 | §3 clause 6 — l'authentification n'est nulle part | B | **S** | seule | non | Fait naître le jeton émis à l'activation. Le premier client qui se branche impose de trancher |
| S1-016 | §6 — deux entrées mal rangées | B | R | invisible | non | Fait naître le registre *tranché hors périmètre* |
| S1-017 | §6 — silence sur l'authentification | E | **S** | seule | non | Jumelle de `S1-015`, axe Découpabilité |
| S1-018 | §6 — découverte du port | E | **S** | seule | non | Le `blockedBy` du fondateur ne se calcule pas |
| S1-019 | §3 clause 2 — quel est le second client ? | E | **S** | indéc. | non | Dépend d'un client externe : ni le dépôt ni le document ne le disent |
| S1-020 | §3 clause 3 — référentiel de la parité | E | **S** | coûteuse | non | **Fait naître l'inventaire.** Sans lui, la répartition des ~30 gestes et l'acceptation de chaque incrément se refont |
| S1-021 | §7 — la sérialisation, incrément ou contrainte ? | E | **S** | coûteuse | non | Un verrou rétrofité sur N chemins d'écriture déjà écrits |
| S1-022 | §3 clause 1 — qu'est-ce qui active le serveur ? | E | **S** | seule | non | Le premier code d'activation force la question |
| S1-023 | arbitrage — « deux projets de front » | **D** | **S** | seule | non | `ProjectHost` est `IDisposable`, un seul vit à la fois ; le second projet fait tomber le premier |
| S1-024 | §6 — sort de `CUR-32` et de la carte du défaut | E | R | invisible | non | Mécanique de backlog |

### Spec — tour 2 (12 remarques)

| # | Repère | Scope | P. | Survie | Repr. | Note |
|---|---|---|---|---|---|---|
| S2-025 | §4 — « les gestes existent tous côté noyau » | **D** | **S** | seule | non | `ProjectHost` n'expose ni `Stop` ni `Cancel` : **le compilateur**, immédiatement |
| S2-026 | §8.2 — le lancement ne se traduit pas | **D** | **S** | seule | non | `LaunchAsync` ne rend son `WorkflowRun` qu'à la terminaison : le premier outil de lancement pend |
| S2-027 | §1 — « seule la piste MCP avait été sondée » | B | R | invisible | **oui** | Renvoi qui ne résout pas ; la phrase vient d'une reprise de Discovery |
| S2-028 | §4 — invariant recopié au lieu d'être cité | B | R | invisible | non | |
| S2-029 | annexe — « les six types du §4 » | **D** | R | seule | **oui** | `Tasks/` est un dossier : la première ouverture du dossier le dit |
| S2-030 | §1 — écart sans coût | B | R | invisible | non | |
| S2-031 | §8.6 — la maille des incréments manque | E | **S** | invisible | **oui** | **Fait naître l'intention de maille — retirée en entier au tour 4** |
| S2-032 | §3 clause 3 — clause de conjonction | E | **S** | invisible | **oui** | Sépare la charge (répartie) du référentiel (entier) |
| S2-033 | annexe — « Ouvrir la page d'un workflow » | **A** | **S** | seule | **oui** | L'outil correspondant n'aurait rien à rendre : visible à l'écriture |
| S2-034 | §8.6 — qui possède le host | **C** | **S** | coûteuse | non | Seul `C` du corpus. La fenêtre cesse de posséder son host ; le faire après coup = recabler tous les ViewModels |
| S2-035 | §7 — « mutant » n'est pas défini | E | **S** | coûteuse | **oui** | Verrou dimensionné sur le seul authoring, registre des projets non protégé |
| S2-036 | §2 — la parité intégrale au premier jour | **F** | **S** | invisible | non | Hors mandat. Le code ne dit jamais si c'est la bonne chose à construire |

### Spec — tour 3 (16 remarques)

| # | Repère | Scope | P. | Survie | Repr. | Note |
|---|---|---|---|---|---|---|
| S3-037 | §2 — « l'unique exception » | **A** | R | invisible | non | Trois autres exclusions existent, dont la moitié terminal du produit |
| S3-038 | §6 — cinq types vs six | **A** | R | invisible | **oui** | Solde du tour 2 appliqué à l'annexe seule : « la divergence a changé de section » |
| S3-039 | §4 — « huit types publics de `Tasks/` » | **D** | R | seule | **oui** | Douze types au dépôt ; `TrackerExceptions` n'est pas un type. **Défaut créé par la reprise du tour 2** |
| S3-040 | §4 — « la base est en WAL » | **D** | **S** | coûteuse | non | `architecture.md` §4.13 dit l'inverse ; une seule `SqliteConnection`. Un incrément « suivre un run » aurait été livré, recetté, puis rouvert |
| S3-041 | §8.3 — le noyau hors du process, « trois projets » | **A** | R | invisible | non | Deux pièces : l'une interne, l'autre du dépôt (quatre projets) — le référentiel principal est interne |
| S3-042 | §8.3 — `fen`/`UI` sans ambre, `ACT` sans vert | **A** | R | invisible | non | La figure fait lire comme inchangé le seul projet refactoré |
| S3-043 | §8.6 vs §8.3 — la figure tranche l'ouvert | **A** | **S** | invisible | non | « Une figure affirme, elle n'illustre pas » — formule entrée dans `revue` §3 |
| S3-044 | §8.3 — `SER` n'a qu'une arête sortante | **A** | R | invisible | **oui** | La prose élargie au tour 2, le schéma resté sur l'ancien |
| S3-045 | §8.2 vs §8.4 — qui compose la mutation | **A** | **S** | coûteuse | non | Choix d'architecture pris par une figure ; la logique d'authoring aurait été écrite dans le socle |
| S3-046 | §8.5 — `Cursus.Trackers` absent ; la lambda résout un *workspace* | **D** | **S** | seule | non | La première compilation du projet dédié réclame la référence |
| S3-047 | §1 — renvoi vers un registre vide | **A** | R | invisible | **oui** | Registres restructurés en reprise de `S1-016` |
| S3-048 | §3 — la clause 1 n'atterrit nulle part | **A** | **S** | invisible | **oui** | « La seule » est faux ; la clause 3 avait écrit son justificatif, pas la clause 1 |
| S3-049 | §8.6 — le fondateur doit résoudre un projet | E | **S** | invisible | **oui** | Trois arêtes de blocage incompatibles ; l'incrément 2 disparaît |
| S3-050 | §3 clause 3 — les lots ne partagent pas de critère | E | **S** | invisible | **oui** | Six lignes tombent dans deux lots |
| S3-051 | §7 — ouvrir un projet **écrit** | **D** | **S** | coûteuse | **oui** | `SqliteRunJournal` crée son schéma dans son constructeur ; déplace le premier incrément mutant avant tout authoring |
| S3-052 | §8.5 — « suivre » veut-il dire en vol ? | E | **S** | coûteuse | **oui** | Verse la révision du partage de connexion au périmètre |

### Spec — tour 4 (16 remarques)

| # | Repère | Scope | P. | Survie | Repr. | Note |
|---|---|---|---|---|---|---|
| S4-053 | §8.2 vs §8.3 — façade traversante | **A** | **S** | coûteuse | **oui** | **Fait naître la couche applicative (`D-052`)** — la décision d'architecture la plus lourde de la feature. Née d'arêtes posées en reprise du tour 3 |
| S4-054 | §6 — « Construit : tout le §4 » | B | R | invisible | **oui** | Le §4 porte deux ⚠️ contraires, ajoutés aux tours 2 et 3 |
| S4-055 | §2 — « dans ses six premiers mots » | **A** | R | invisible | **oui** | Ce sont les mots 11 à 14. Glose écrite en reprise de `S3-037` |
| S4-056 | §8.5 — « sept lignes de l'inventaire » | **A** | R | invisible | **oui** | Une des sept n'aura jamais d'outil |
| S4-057 | §8.3 — cinq ambres pour quatre énumérés | **A** | R | invisible | **oui** | Le cadre `ouvert` sans style tombe dans « ne bouge pas » |
| S4-058 | §8.5 — un manque nommé sur deux | **A** | R | invisible | **oui** | |
| S4-059 | §8.4 — phrase dupliquée | **G** | R | invisible | **oui** | Le §8.4 venait d'être refait en reprise de `S3-045` |
| S4-060 | §7 — renvoi nu `(§7.12)` | B | R | invisible | non | La spec n'a pas de §7.12 |
| S4-061 | §8.6 — indicatif mais deux acquis au registre *tranché* | B | R | invisible | **oui** | **Provoque le retrait du §8.6 en entier** |
| S4-062 | §8.2 — l'inventaire ignore ce que l'agent doit *lire* | E | **S** | seule | **oui** | Trois lignes de lecture entrent ; la **définition** du référentiel change |
| S4-063 | §8.6 — le critère contredit son exemple | **A** | R | invisible | **oui** | Soldée par retrait |
| S4-064 | §3 — la ligne « Suivre » ne dit pas *en vol* | E | **S** | coûteuse | **oui** | Le lot serait clos par un outil qui lit un run terminé |
| S4-065 | §8.6 — trois lignes orphelines | E | R | invisible | **oui** | Soldée par retrait |
| S4-066 | §7 — lancer un run écrit massivement | **A** | **S** | coûteuse | **oui** | Définition vs énumération ; deux lancements concurrents non protégés |
| S4-067 | §8.6 — le fondateur ne tient pas dans une session | E | **S** | invisible | **oui** | Soldée par retrait |
| S4-068 | §6 — `CUR-32`, carte ou pas | E | R | invisible | **oui** | Reprise de `S1-024` |

### Spec — tour 5 (12 remarques) — *après la refonte de gabarit `D-054`*

| # | Repère | Scope | P. | Survie | Repr. | Note |
|---|---|---|---|---|---|---|
| S5-069 | annexe C — renvoi au « §5 » | B | R | invisible | non | Résidu de la refonte, pas d'une reprise. ⚠️ Le solde note que `D-052` cite « le §5 de la spec » et que c'est **incorrigible** (append-only) |
| S5-070 | §2.1 — « l'arbitrage complet est en annexe A » | **A** | R | invisible | non | Deux des trois choix sont ailleurs |
| S5-071 | §2.2 — « construits et prouvés (§7.13) » | **D** | R | invisible | non | §7.13 est *tranché non construit* ; c'est §4.13 qu'il fallait citer |
| S5-072 | §2.2 — l'arête `UI --> QRY` | **A** | R | invisible | **oui** | Tranche par un trait la question ouverte du §3.1 ; l'arête est née de la reprise du tour 4 |
| S5-073 | §2.2 — l'éditeur appelle-t-il la même commande ? | **D** | **S** | seule | **oui** | À l'enregistrement, `AjouterEtape` relirait le disque et perdrait *n−1* mutations. Produit `D-055` |
| S5-074 | §1.2 — « Lancer depuis la page » | **A** | **S** | seule | **oui** | Deux paires de doublons ; produit `D-056` (une ligne est une fonctionnalité, pas un geste d'écran) |
| S5-075 | §2.2 — pas de règle d'atterrissage pour la descente | E | **S** | coûteuse | **oui** | Produit `D-059`. ⚠️ **L'arbitrage avait déjà été rendu au tour 3 et perdu avec le §8.6 retiré au tour 4** |
| S5-076 | §3.1 — la maille de la sérialisation | E | **S** | coûteuse | **oui** | Produit `D-058` (verrou global) ; deux des trois branches étaient déjà mortes au dépôt |
| S5-077 | §3.1 — acceptation en lignes ou en outils ? | E | R | invisible | **oui** | Produit `D-056` ; la question restait non bloquante, mais pas pour la raison qu'on lui prêtait |
| S5-078 | §1.2 vs figure — « Ouvrir un projet » | **A** | **S** | seule | **oui** | Produit `D-057` ; la ligne quitte l'inventaire |
| S5-079 | §2.2 — l'activation n'a aucune adresse de recette | E | **S** | invisible | **oui** | Produit le scénario 9 |
| S5-080 | §2.2 — arrêter et lire en vol ne sont recettés nulle part | E | **S** | coûteuse | **oui** | Produit les scénarios 7 et 8 ; sans eux un incrément se clôt sans son comportement |

### Spec — tour 6 (19 remarques)

| # | Repère | Scope | P. | Survie | Repr. | Note |
|---|---|---|---|---|---|---|
| S6-081 | annexe B — « les scénarios 7 et 8 doublent » | **A** | R | invisible | **oui** | Le scénario 1 ne porte aucun arrêt. Note écrite au tour 5 |
| S6-082 | §3.1 — l'état des runs dans trois registres | **A** | **S** | coûteuse | **oui** | Produit `D-061`. Selon le porteur, la fenêtre peut ou non arrêter un run de l'agent : deux features |
| S6-083 | §2.2 — la figure rend l'éditeur impossible | **A** | R | invisible | **oui** | Conséquence directe du retrait d'arête au tour 5 ; soldée par une note, pas par une arête |
| S6-084 | §1.2 — « Ouvrir l'écran des tâches » | **A** | R | invisible | **oui** | Ligne mixte, contre `D-056` |
| S6-085 | annexes A et C — titres maison | B | R | invisible | non | Écart au gabarit `D-054`, non motivé |
| S6-086 | §2.3 — `D-059` non cité | B | R | invisible | **oui** | La règle vient d'être réinscrite au tour 5, sans sa généalogie |
| S6-087 | annexe A — écart sans coût | B | R | invisible | non | Même case que `S2-030`, sur un autre écart, quatre tours plus tard |
| S6-088 | §2.2 — « la fenêtre attend le sien » | **A** | R | invisible | **oui** | Contre `D-058`, rendue au tour 5 |
| S6-089 | §2.2 — le cadre `ouvert` « quitte la présentation » | **A** | R | invisible | **oui** | Trois de ses quatre pièces y naissent |
| S6-090 | §2.2 — la racine absente du `sequenceDiagram` | **A** | R | invisible | **oui** | Le paramètre `projet` n'est jamais consommé ; contre `D-057` |
| S6-091 | §2.2 — « unique appelant » | **D** | R | invisible | non | `architecture.md` §7.12 enregistre l'end-to-end headless comme second appelant. La conclusion tient, l'argument non |
| S6-092 | §2.2 — une requête peut écrire hors du verrou | **A** | **S** | coûteuse | **oui** | Produit `D-062` : la base cesse de naître au premier accès. Course invisible en test mono-thread |
| S6-093 | §3.1 — qui porte l'état des runs | E | **S** | coûteuse | **oui** | Jumelle de `S6-082`, axe Découpabilité |
| S6-094 | §2.3 — « ce qui descend est plus large » | E | R | seule | **oui** | Le solde **réfute le dilemme** : `App.axaml.cs` compose deux étages de natures différentes. Lire le fichier suffisait |
| S6-095 | annexe B — le scénario 4 ne peut pas atterrir entier | E | **S** | invisible | **oui** | Quatre objets, quatre incréments ; la règle remonte au §3.3 |
| S6-096 | §1.2 — « Choisir la connexion » | E | **S** | seule | non | Le solde : « **le code répond aux deux questions**, et il en règle une troisième que la remarque n'avait pas posée » |
| S6-097 | §2.3 — le recablage « écran par écran » | E | R | seule | **oui** | Le solde **réfute la prémisse** par un décompte des six ViewModels : l'éditeur est le plus **petit** |
| S6-098 | §2.3 — le premier incrément mutant est irréductible | E | **S** | invisible | **oui** | Assumé, et son inventaire écrit |
| S6-099 | annexe B — la mise à jour perdue | **F** | **S** | coûteuse | **oui** | **Hors mandat.** Produit `D-063`, que le tour 7 appelle « la clause la plus chère de la feature ». Cas nominal, pas cas de bord |

### Spec — tour 7 (12 remarques)

| # | Repère | Scope | P. | Survie | Repr. | Note |
|---|---|---|---|---|---|---|
| S7-100 | §2.3 — le total de douze points | **D** | **S** | seule | **oui** | Un terme entier manquant (`OpenProjectViewModel`, 3 appels) ; règle de comptage variable. Décompte né au tour 6 |
| S7-101 | §3.1 — deux puces soudées | **A** | R | invisible | **oui** | Pronom sans antécédent ; exclusivité attribuée à la mauvaise règle. Solde : « **né de la reprise de la veille** » |
| S7-102 | §2.2 — « une seule arête sortante » | **A** | R | invisible | **oui** | Le cadre en porte deux |
| S7-103 | §2.2 — « ouvrir un projet » comme geste partagé | **A** | R | invisible | **oui** | `D-055` écrite avant que `D-057` ne retire la ligne |
| S7-104 | §3.1 — « trois manques du noyau » | **A** | R | invisible | **oui** | La couche applicative n'est pas du noyau |
| S7-105 | §2.2 — libellé « Descendu hors de la présentation » | **A** | R | invisible | **oui** | Un participe passé affirme un mouvement pour quatre nœuds dont trois naissent |
| S7-106 | §3.3 — quatre clauses, ordinal contradictoire | **A** | **S** | invisible | **oui** | Le scénario 4 en porte cinq |
| S7-107 | §3.1 — le registre *construit* absent | B | R | invisible | non | Le titre annonce trois registres, la section en substitue un quatrième |
| S7-108 | §2.3 — la bijection objets ↔ incréments | E | **S** | invisible | **oui** | Bute dans les deux sens ; produit `D-065` |
| S7-109 | annexe B — la cinquième clause | **A** | **S** | invisible | **oui** | Née de `D-063` au tour 6 |
| S7-110 | §2.2 — l'ouverture d'un projet non initialisé | E | **S** | coûteuse | **oui** | Aucune clause ne la recette ; produit le scénario 10 |
| S7-111 | §2.2 — l'erreur nommée est un arbitrage produit | **F** | **S** | invisible | **oui** | **Hors mandat.** Produit `D-064` |

### Spec — tour 8 (13 remarques, toutes ouvertes)

| # | Repère | Scope | P. | Survie | Repr. | Note |
|---|---|---|---|---|---|---|
| S8-112 | §2.2 — « à chaque appel, sans état retenu » | **A** | R | invisible | **oui** | L'arête `K->>R` est née de la reprise du tour 6 ; contredit `D-057`/`D-062` |
| S8-113 | §2.3 — renvoi vers une question ouverte inexistante | **A** | R | invisible | **oui** | *lectures/requêtes* contre *écritures/commandes* |
| S8-114 | §2.3 — trois points pour les connexions tracker | **A** | R | seule | **oui** | Le chiffre **quinze** en dépend ; le recablage réel compte les appels. Né de la reprise du tour 7 |
| S8-115 | §2.3 — `D-063` non cité par la règle qui l'énonce | B | R | invisible | **oui** | Pratique inégale dans la même liste |
| S8-116 | §1.2 — « l'écran distingue cinq issues » | **D** | R | seule | **oui** | `TaskBoardViewModel` en documente quatre, deux fois. Ligne née de la reprise du tour 6 |
| S8-117 | §1.1 — « un outil pour chacune » | **A** | R | invisible | **oui** | Se lit comme une bijection ligne ↔ outil, contre `D-056` |
| S8-118 | §2.2 — les `(§2.2)` nus | B | R | invisible | non | Douze renvois vers une section à six intertitres ; résidu de la refonte du tour 5 |
| S8-119 | §3.1 — un item tranché portant sa part ouverte | B | R | invisible | **oui** | Ni dans un registre, ni dans deux |
| S8-120 | §2.3 — un incrément en lecture seule | E | **S** | invisible | **oui** | Ni retenu ni écarté ; trois attributions basculent |
| S8-121 | §3.1 — les charges de l'irréductible | E | **S** | invisible | **oui** | Hôte, adaptateur, activation, jeton absents de l'énumération ; deux dimensionnements incompatibles |
| S8-122 | §2.3 — quinze points sans scénario opposable | E | **S** | coûteuse | **oui** | La plus grosse charge de régression n'a aucune acceptation |
| S8-123 | §3.1 — la huitième question ouverte | E | **S** | invisible | **oui** | Un « geste dédié » est une ligne de plus dans l'inventaire, donc du périmètre |
| S8-124 | §2.2 — qui fait naître le registre des runs | E | **S** | seule | **oui** | Un registre qu'aucun lancement n'alimente n'a rien à arrêter |

### Cas litigieux, notés à part

| # | Tension | Arbitrage retenu |
|---|---|---|
| `S1-015` / `S1-017` | Même défaut (l'authentification absente), deux axes, deux référentiels | `B` pour la case de registre, `E` pour le blocage du découpage. Comptées deux fois, comme deux remarques |
| `S6-082` / `S6-093` | Même défaut (l'état des runs), deux axes | `A` et `E`, idem |
| `S2-034` | Le code établit qui possède le host — lisible en `D` | `C` : ce que la remarque oppose est une **frontière** (la fenêtre cesse de posséder), pas un fait allégué faux |
| `S3-041` | Deux pièces : figure vs légende (interne), et « trois projets » vs le dépôt | `A` — le référentiel principal invoqué est interne |
| `S4-053`, `S6-092`, `S8-114` | Contradiction interne **sur** un sujet d'architecture ou un fait de code | `A`, conformément à la consigne : le référentiel opposé prime le sujet |
| `S1-004` (D1-004), `D2-011` | Faits techniques, mais l'objection est la **prématurité** | `B` — la frontière d'étape est le référentiel |
| `S5-069`, `S6-085`, `S8-118` | Défauts nés de la **refonte de gabarit** du tour 5, pas d'une remarque | `Reprise ? = non`. La distinction est déclarée : « née d'une reprise » vise le texte écrit **en réponse à une remarque** |

---

## 2. Les agrégats

### 2.1 Scope × tour

| Scope | D1 | D2 | S1 | S2 | S3 | S4 | S5 | S6 | S7 | S8 | **Spec** |
|---|---|---|---|---|---|---|---|---|---|---|---|
| **A** — cohérence interne | 0 | 0 | 0 | 1 | 9 | 7 | 4 | 8 | 7 | 4 | **40** |
| **B** — conformité formelle | 7 | 6 | 3 | 3 | 0 | 3 | 1 | 3 | 1 | 3 | **17** |
| **C** — architecture | 0 | 0 | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | **1** |
| **D** — faisabilité technique | 0 | 0 | 1 | 3 | 4 | 0 | 2 | 1 | 1 | 1 | **13** |
| **E** — découpabilité | 0 | 0 | 7 | 3 | 3 | 5 | 5 | 6 | 2 | 5 | **36** |
| **F** — justesse produit (hors mandat) | 0 | 0 | 0 | 1 | 0 | 0 | 0 | 1 | 1 | 0 | **3** |
| **G** — coquille | 0 | 0 | 0 | 0 | 0 | 1 | 0 | 0 | 0 | 0 | **1** |
| **Total** | 7 | 6 | 11 | 12 | 16 | 16 | 12 | 19 | 12 | 13 | **111** |

Regroupements utiles, en part du tour :

| | S1 | S2 | S3 | S4 | S5 | S6 | S7 | S8 |
|---|---|---|---|---|---|---|---|---|
| **A + B** (le document contre lui-même ou contre sa forme) | 27 % | 33 % | 56 % | 63 % | 42 % | 58 % | 67 % | 54 % |
| **C + D** (architecture et faits techniques) | 9 % | 33 % | 25 % | **0 %** | 17 % | 5 % | 8 % | 8 % |
| **E** (découpabilité) | 64 % | 25 % | 19 % | 31 % | 42 % | 32 % | 17 % | 38 % |

### 2.2 Portée × tour

| Portée | D1 | D2 | S1 | S2 | S3 | S4 | S5 | S6 | S7 | S8 | **Spec** |
|---|---|---|---|---|---|---|---|---|---|---|---|
| **Structurante** | 0 | 0 | 8 | 8 | 9 | 5 | 7 | 7 | 6 | 5 | **55** |
| **Rédactionnelle** | 7 | 6 | 3 | 4 | 7 | 11 | 5 | 12 | 6 | 8 | **56** |
| Part structurante | 0 % | 0 % | **73 %** | **67 %** | 56 % | 31 % | 58 % | 37 % | 50 % | 38 % | 50 % |

### 2.3 Survie au code × tour

| Survie | D1 | D2 | S1 | S2 | S3 | S4 | S5 | S6 | S7 | S8 | **Spec** |
|---|---|---|---|---|---|---|---|---|---|---|---|
| *se serait vue seule* | 0 | 0 | 5 | 4 | 2 | 1 | 3 | 3 | 1 | 3 | **22** |
| *coûteuse si tardive* | 0 | 0 | 2 | 2 | 4 | 3 | 3 | 4 | 1 | 1 | **20** |
| *invisible au code* | 7 | 6 | 3 | 6 | 10 | 12 | 6 | 12 | 10 | 9 | **68** |
| *indécidable* | 0 | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | **1** |

### 2.4 Survie × portée (spec seule, 111)

| | *seule* | *coûteuse* | *invisible* | *indéc.* | **Total** |
|---|---|---|---|---|---|
| **Structurante** | 16 | **20** | 18 | 1 | **55** |
| **Rédactionnelle** | 6 | **0** | 50 | 0 | **56** |

### 2.5 Scope × portée (spec seule)

| Scope | Structurante | Rédactionnelle |
|---|---|---|
| A — cohérence interne | 12 | 28 |
| B — conformité formelle | 1 | 16 |
| C — architecture | 1 | 0 |
| D — faisabilité technique | 8 | 5 |
| E — découpabilité | 30 | 6 |
| F — justesse produit | 3 | 0 |
| G — coquille | 0 | 1 |

### 2.6 « Née d'une reprise ? » × tour

| | D1 | D2 | S1 | S2 | S3 | S4 | S5 | S6 | S7 | S8 |
|---|---|---|---|---|---|---|---|---|---|---|
| **oui** | 0 | 2 | 0 | 6 | 9 | **15** | 9 | **15** | **11** | **12** |
| non | 7 | 4 | 11 | 6 | 7 | 1 | 3 | 4 | 1 | 1 |
| **part** | 0 % | 33 % | **0 %** | 50 % | 56 % | **94 %** | 75 % | 79 % | **92 %** | **92 %** |

⚠️ **Divergence assumée avec les fiches REX.** Elles portent, pour la même ligne : `—`, 0, 2, 4,
*non mesurable*, ≥ 6, ≥ 6, ≥ 6 — et se déclarent **minorantes**, leur critère étant la présence d'un
`D-NNN` dans le passage visé. Le critère retenu ici est plus large : *le passage visé a-t-il été
écrit ou réécrit en réponse à une remarque antérieure ?*, imputé d'après les fils de solde, qui
disent presque toujours ce qu'ils ont touché.

---

## 3. Les faits saillants

**① Le corpus se scinde en deux moitiés presque égales, et la ligne de partage est nette.**
55 remarques de spec sur 111 changent ce qu'on va construire ; 56 changent la façon dont le document
le dit. Les 20 remarques « coûteuses si tardives » sont **toutes** structurantes, et 50 des 56
rédactionnelles sont invisibles au code (§2.4). Le processus produit donc deux flux distincts, pas un
flux homogène de qualité décroissante.

**② L'architecture et les faits techniques se vident après le tour 3, mais la découpabilité, non.**
`C + D` vaut 9 remarques sur 39 aux tours 1–3 (23 %) et 5 sur 72 aux tours 4–8 (7 %) ; le tour 4 en
produit **zéro**. En regard, `E` ne décroît pas : 7, 3, 3, 5, 5, 6, 2, **5** — et les 5 du tour 8 sont
**toutes structurantes** (`S8-120` à `S8-124`), dont deux qui contestent le dimensionnement du
premier incrément. L'hypothèse « les tours tardifs ne trouvent plus que de la forme » est vraie pour
l'architecture et fausse pour le découpage.

**③ Trois remarques sur quatre, à partir du tour 4, visent du texte écrit pour répondre à une
remarque antérieure.** 77 des 111 remarques de spec (69 %), et la part passe de 0 % / 50 % / 56 % à
**94 %, 75 %, 79 %, 92 %, 92 %** (§2.6). Aux quatre derniers tours, 47 remarques sur 56 visent la
reprise. Le processus se nourrit de lui-même de façon mesurable et croissante.

**④ Le cas le plus cher de la feature naît d'une reprise, deux tours après la remarque d'origine.**
`S4-053` — la contradiction entre le `flowchart` et la prose sur qui traverse quoi — fait naître la
**couche applicative** (`D-052`), l'objet le plus structurant de la spec. Or ces arêtes avaient été
tracées en reprise de `S3-044`, elle-même posée parce que la reprise du tour 2 avait élargi la prose
sans corriger le schéma. Le même enchaînement se lit sur `D-063` (`S6-099` → `S7-109` → `S8-115`).

**⑤ Un arbitrage rendu au tour 3 a été détruit par la reprise du tour 4, puis retrouvé au tour 5.**
Le §8.6 (intention de maille), créé en reprise de `S2-031`, a absorbé les reprises de `S3-049` et
`S3-050`, puis a été **retiré en entier** au tour 4 pour solder `S4-061`, `S4-063`, `S4-065` et
`S4-067`. La règle d'atterrissage de la descente du socle est partie avec lui, et `S5-075` l'a
rattrapée un tour plus tard — le solde le dit explicitement. Quatre remarques ont été soldées par la
destruction du travail de trois autres.

**⑥ Un fait technique faux a traversé quatre tours de revue.** Le mode WAL présenté comme autorisant
la lecture concurrente est posé en Discovery (`D2-011` — où l'objection portée est la *prématurité*,
pas la fausseté), survit aux tours 1 et 2 de spec, et n'est démenti qu'au tour 3 (`S3-040`), contre
`architecture.md` §4.13 et le `<remarks>` de `SqliteProjectHost`. Le solde le qualifie de « la plus
coûteuse des seize ». La vérification tient en une lecture de `SqliteRunJournal`.

**⑦ Un dénombrement est contesté à cinq tours sur huit, et deux de ces décomptes ont été rendus
faux par leur propre correction.** « six types » (`S2-029`) → « huit types publics de `Tasks/` »,
**faux, écrit en reprise** (`S3-039`, dont le solde note : « corriger un flou par une précision
fausse est pire que le flou ») → « sept lignes » (`S4-056`) → « douze points de traversée »
(`S7-100`) → « quinze, dont trois hors parité » (`S8-114`, née de la reprise du tour 7). Le même
défaut, sous cinq formes, sur six tours.

**⑧ Trois des remarques les plus lourdes sont hors mandat, c'est-à-dire hors de tout référentiel.**
`S2-036` (le périmètre de la première feature), `S6-099` (la mise à jour perdue → `D-063`, que
`S7-109` appelle « la clause la plus chère de la feature ») et `S7-111` (l'erreur nommée → `D-064`).
Aucune n'est mesurée par une case de DoD ; deux des trois arrivent aux tours 6 et 7, c'est-à-dire
tard.

**⑨ Quatre remarques ont été soldées par le code, qui a réfuté leur prémisse ou dissous leur
question.** `S6-094` (le dilemme sur ce qui descend n'a pas lieu d'être : `App.axaml.cs` compose deux
étages de natures différentes), `S6-096` (« **le code répond aux deux questions**, et il en règle une
troisième que la remarque n'avait pas posée »), `S6-097` (le décompte des six ViewModels montre que
l'éditeur est le **plus petit** recablage, non le plus gros), `S5-076` (« deux des trois branches
étaient déjà mortes »). Quatre débats de spec qu'une lecture du dépôt aurait tranchés, dont trois au
même tour.

**⑩ 22 remarques sur 111 auraient été révélées par le code tôt et sans dégât, et elles se
concentrent au début.** 11 sur 39 aux tours 1–3 (28 %), 11 sur 72 aux tours 4–8 (15 %). Les plus
nettes sont mécaniques : `S2-025` (aucune méthode `Stop` à appeler — le compilateur), `S2-026`
(`LaunchAsync` ne rend la main qu'à la fin — le premier appel pend), `S1-023` (un seul host vit à la
fois), `S3-046` (la référence de projet manquante à la compilation), `S8-116` (quatre issues
documentées, pas cinq).

**⑪ La conformité formelle pure (`B`) ne s'éteint pas non plus** : 3, 3, 0, 3, 1, 3, 1, 3 — présente
à sept tours sur huit, 17 remarques au total, dont **16 rédactionnelles et 17 invisibles au code**.
Trois d'entre elles (`S5-069`, `S6-085`, `S8-118`) sont des résidus de la refonte de gabarit du
tour 5, c'est-à-dire d'un changement que la revue n'a pas demandé.

**⑫ Le tour 8 ressemble aux tours 5 et 6, pas à une extinction.** 13 remarques, 5 structurantes,
1 « coûteuse si tardive », 12 sur 13 nées d'une reprise, et 5 remarques de découpabilité toutes
structurantes. La fiche REX du tour 8 relève par ailleurs que c'est **le premier tour sans aucune
remarque née du `flowchart`** depuis le tour 2, et que les jugements y repassent devant les
violations dures (9 contre 4).

---

## 4. Ce que cette analyse ne peut pas établir

**Le contrefactuel n'est pas observé, il est reconstruit.** La colonne *Survie au code* est un
jugement porté depuis le dépôt d'aujourd'hui, sur un code qui n'a jamais été écrit : aucun incrément
de cette feature n'a été planifié ni codé. Dire qu'une remarque « se serait vue seule » est une
inférence, pas une mesure — et personne ne peut vérifier qu'elle aurait effectivement été *vue*
plutôt que contournée.

**L'échelle intermédiaire manque au raisonnement.** Entre la spec et le code il existe un **plan de
design** par incrément, qui tranche les responsabilités et les objets. Une part inconnue des
20 remarques « coûteuses si tardives » aurait été rattrapée là, gratuitement, avant toute ligne de
code. Le corpus ne contient aucun plan de design de cette feature : la question reste entière, et
c'est probablement la plus importante pour le binôme.

**On ne mesure que ce qui a été posé.** Rien ici ne dit ce que les huit tours **n'ont pas vu**. La
fiche du tour 2 relève « défauts trouvés par l'humain, pas par la revue : 1 » ; les autres fiches ne
tiennent pas cette ligne. Le taux de faux négatifs est inconnu, et il conditionne toute lecture du
rendement.

**La colonne « née d'une reprise » est imputée, pas tracée.** Linear ne conserve pas l'historique des
versions d'un document, et aucune remarque ne porte de diff. L'imputation s'appuie sur les fils de
solde — souvent explicites, parfois muets. C'est pourquoi elle diverge des fiches REX : deux critères
différents, tous deux défendables, et aucun des deux vérifiable a posteriori.

**La portée est contaminée par la réponse.** « Structurante » est jugée sur ce que la reprise a
*effectivement* changé. Une remarque à laquelle on a répondu par un arbitrage lourd paraît donc
structurante même si un refus motivé l'aurait close sans dommage. Or **le corpus ne contient
quasiment aucun refus** : les fiches des tours 1 et 2 notent « 11/11 » et « 12/12 retenues, aucun
refus motivé ». Le taux de rétention proche de 100 % rend impossible de distinguer une remarque
fondée d'une remarque simplement acceptée.

**La non-convergence n'est pas décomposée.** L'analyse ne peut pas départager, dans la série
11-12-16-16-12-19-12-13, la part de **l'artefact qui grossit** (une annexe au tour 1, un plan
d'implémentation au tour 2, une refonte de gabarit au tour 5, quatre scénarios de recette aux
tours 5–7, `D-060` au tour 7) de la part du **texte de reprise qui crée ses propres défauts**. Les
deux effets sont présents et de même sens ; aucune mesure du corpus ne les sépare.

**Le coût n'est pas rapporté à la remarque.** Les fiches REX portent durée, jetons et coût **par
tour**, jamais par remarque, et le coût du **solde** — souvent plus lourd que celui de la revue,
puisqu'il ouvre le dépôt et rend des arbitrages — n'est mesuré nulle part. Aucun ratio
valeur/coût n'est donc calculable à partir de ce corpus.

**Enfin, le classement de scope est fragile là où les deux axes convergent.** Les fiches des tours 4
et 7 signalent des constats « trouvés par les deux axes, posés une fois ». Quand une remarque cite à
la fois une contradiction interne et l'impossibilité de découper, le choix `A` plutôt que `E` suit la
consigne (le référentiel prime le sujet), mais il déplace des unités entre deux colonnes que le
binôme lira comme opposées.
