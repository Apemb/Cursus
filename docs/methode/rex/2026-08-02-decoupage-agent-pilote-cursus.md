# Découpage d'*Un agent pilote Cursus* — et sa relecture par six agents

> **Premier usage réel du skill `decoupage`**, draft non éprouvé jusqu'ici. La fiche couvre
> l'étape entière : le découpage lui-même, puis la relecture multi-agents qui l'a suivi
> immédiatement — laquelle n'est portée par **aucun skill** et n'a donc pas de protocole à
> confronter. Les deux sont dans la même fiche parce qu'ils forment une seule étape du flux :
> l'artefact n'a jamais été considéré comme livré entre les deux.

## 1. Ce qui a tourné

**Le skill `decoupage`**, sur la spec `Done` d'*Un agent pilote Cursus*, pour produire les incréments
de la feature. Puis **six sous-agents** lancés sur les cartes produites, sans skill.

**Trace que le skill a bien été chargé** : ses huit étapes sont observables dans le déroulé — le gel
de la recette (les dix scénarios de l'annexe B relevés et assignés un à un), la tranche verticale
(chaque incrément traverse porte → couche → noyau), le dimensionnement, les arêtes `blockedBy`, le
hors-périmètre « en regard des frères », l'accord de l'humain **avant** toute création de carte, la
création puis la transition de la feature. L'annexe *refactor large* n'a pas été lue : la feature est
fonctionnelle.

### La commande, verbatim

```
Skill(skill: "decoupage",
      args: "Feature « Un agent pilote Cursus » (MCP) — spec Done après 8 tours.
             Découpage candidat à neuf incréments produit par l'axe Découpabilité du tour 8,
             en rubrique 4 de docs/methode/rex/2026-08-02-revue-spec-tour-8.md")
```

Répertoire de départ : la racine du dépôt. Session **non neuve** — le binôme venait d'écrire, dans la
même session, la section `architecture.md` §7.15 qui verse le durable de cette spec. ⚠️ **C'est une
différence de régime avec toutes les fiches de revue** : ici l'auteur du découpage était l'auteur de
la spec, et il n'y avait aucune clause d'isolement à tenir — le découpage n'est pas une revue.

### Les six agents de relecture

Lancés **en un seul message**, donc concurrents, tous en `general-purpose`, modèle hérité de la
session. Cinq lots de contenu **avec droit d'écriture** sur leurs cartes ; un axe de cohérence
**explicitement privé d'écriture**, pour qu'il n'écrase pas le travail des cinq autres pendant
qu'ils éditaient les mêmes objets.

| Agent | Cartes | Écrit ? |
|---|---|---|
| 1 | `CUR-47`, `CUR-48`, `CUR-53` — la porte et les projets | oui |
| 2 | `CUR-49`, `CUR-54`, `CUR-50` — workflows et lecture d'une définition | oui |
| 3 | `CUR-55` → `CUR-58` — les quatre cartes d'authoring | oui |
| 4 | `CUR-51`, `CUR-59`, `CUR-60`, `CUR-61` — les runs | oui |
| 5 | `CUR-52`, `CUR-62`, `CUR-63`, `CUR-64` — trackers et carte latérale | oui |
| 6 | les dix-huit | **non — lecture seule** |

Le prompt des cinq lots de contenu est **le même squelette**, dont voici la forme complète (celle du
lot 1 ; les autres n'en diffèrent que par la liste de cartes, les sections de référentiel à ouvrir,
les faits de code à vérifier nommément, et un ou deux « points d'attention particulière ») :

```
Tu relis trois cartes d'incrément Linear créées aujourd'hui pour la feature
« Un agent pilote Cursus » (espace cursus-app, équipe CUR). Dépôt : <racine du dépôt>

Tes cartes : CUR-47 (la porte + lister les projets), CUR-48 (créer et inscrire un projet),
CUR-53 (renommer et retirer un projet).

Mandat : chicaner leur contenu contre le référentiel, corriger ce dont tu es sûr, remonter ce
dont tu doutes. Tu ne redécoupes pas — les frontières entre incréments ont été validées par
l'humain aujourd'hui. Si tu crois voir un défaut de frontière, tu le remontes sans y toucher.

Le référentiel, dans cet ordre :
1. docs/methode/tickets.md §3 — les six questions que doit contenir un incrément, et ce qu'il
   ne doit pas contenir (pas de comment, pas de test list, pas de recopie du dépôt).
2. La spec : cursus linear doc show "Spec — Un agent pilote Cursus" — ⚠️ à lancer depuis la
   racine du dépôt, sinon la CLI échoue (friction connue). Sortie JSON ; le markdown est dans
   le champ content. L'inventaire est le §1.2, la recette l'annexe B, la répartition le §3.3.
3. docs/design/architecture.md, surtout §7.15, plus toute section que les cartes citent.
4. docs/design/decisions.md — les entrées D-NNN citées.
5. Le code du dépôt, pour tout fait allégué.

Ce que tu cherches, en axes séparés — ne les fonds pas :
- Faits faux. Chaque assertion sur le code doit être vérifiée dans le dépôt : <liste nommée
  des faits à vérifier pour ce lot>. C'est l'axe le plus payant : plusieurs erreurs de bonne
  foi ont déjà été trouvées sur cette feature, écrites de mémoire et démenties par le code.
- Renvois faux ou inutiles. Un D-NNN doit exister et dire ce que la carte lui fait dire. Et un
  renvoi ne doit jamais être nécessaire pour comprendre la phrase qui le porte : si la règle ne
  se tient pas seule, réécris-la. Vérifie aussi les numéros de section architecture.md §X.
- Acceptation. Observable, et correspondant exactement à la part de recette qui revient à cet
  incrément selon le §3.3. ⚠️ Un incrément mutant sans clause de concurrence est conforme — ne
  lui en fabrique pas.
- Le ticket a-t-il mangé le plan ? Il dit quoi et pourquoi, jamais comment.
- Hors-périmètre : il doit nommer les incréments frères, pas dire « hors périmètre ».
- Contradictions internes entre deux passages d'une même carte.

Ce que tu fais de ce que tu trouves :
- Corrige directement ce dont tu es sûr, avec save_issue (patch pour des éditions ciblées).
  Reste chirurgical, n'écrase pas une carte entière pour un mot.
- Ne corrige pas ce qui relève d'un arbitrage : frontière, acceptation qui changerait de sens,
  ligne d'inventaire déplacée. Remonte-le.

Ton rapport final est ta valeur de retour, pas un message à un humain. Pour chaque carte : ce
que tu as corrigé (une ligne par correction) et ce sur quoi tu veux un arbitrage — chaque point
portant sa citation exacte, ce qui cloche, et ton option recommandée.
```

Le prompt de l'axe de cohérence est **structurellement différent** : il énumère **six vérifications
numérotées** (les 31 lignes réparties sans reste · l'atterrissage des scénarios · les arêtes de
blocage · les colonnes de naissance · la réciprocité des hors-périmètre · le test de départage), il
interdit l'écriture en donnant son motif (« d'autres agents éditent ces mêmes cartes en ce moment »),
et il demande un classement final des trois défauts les plus graves.

## 2. Chiffres

**Le découpage.**

| | |
|---|---|
| Durée de bout en bout | ~40 min, découpage + relecture + corrections |
| Incréments produits | **17**, plus 1 carte latérale (`CUR-64`) |
| Incréments du découpage candidat initial | **9** — passés à 17 sur arbitrage de l'humain |
| Lignes d'inventaire réparties | **31 sur 31** atteignables (32 relevées, dont une ⛔) |
| Scénarios de recette assignés | 10 : 1 exempté, 1 réparti en charge, 1 réparti en 5 clauses, 7 adressés |
| Questions posées à l'humain | **10**, en 4 tours de questions |
| Cartes préexistantes inventoriées | **0** — et c'est un défaut, voir §5 |

**La relecture.**

| Agent | Tokens | Appels d'outils | Durée |
|---|---|---|---|
| 1 — porte et projets | 145 857 | 29 | 6 min 23 |
| 2 — workflows et lecture | 150 161 | 37 | 6 min 14 |
| 3 — authoring | 144 639 | 46 | 7 min 01 |
| 4 — runs | 150 672 | 38 | 5 min 46 |
| 5 — trackers | 151 457 | 42 | 6 min 41 |
| 6 — cohérence d'ensemble | 140 070 | 33 | 5 min 40 |
| **Total** | **883 856** | **225** | **7 min 01 de mur** (concurrents) |

**Ce que la relecture a produit** : **~30 corrections** appliquées par les agents sur 13 cartes,
**16 corrections** appliquées ensuite par le binôme sur arbitrage, **2 questions** remontées à
l'humain, **1 carte** fermée en doublon. Aucune frontière d'incrément déplacée par un agent — la
consigne a tenu.

## 3. Conformité au protocole

Clause par clause du skill `decoupage`, avec ce qui l'atteste.

| Étape | Tenue ? | Pièce |
|---|---|---|
| 1. Geler la recette | **oui** | Les dix scénarios de l'annexe B relevés ; chacun assigné ou déclaré exempté. L'axe de cohérence a **revérifié** l'assignation a posteriori : aucun orphelin, aucun dupliqué |
| 2. Trancher verticalement | **oui** | Chaque incrément traverse porte → couche → noyau. Aucun incrément « la sérialisation », aucun « socle », aucun terminal « la parité est complète » — les trois pièges que la spec avait nommément écartés |
| 3. Dimensionner sur une session fraîche | **partiellement** | Le critère a servi à scinder l'authoring, mais **il a manqué son plus gros cas** : `CUR-47` portait quatre scénarios de recette pour une ligne d'inventaire, et c'est l'axe de cohérence qui l'a relevé, pas le dimensionnement |
| 4. Ordonner par les arêtes | **oui, avec deux trous** | 17 arêtes posées, DAG à racine unique, aucun cycle (vérifié par l'axe 6). Deux manques trouvés après coup : `CUR-54` sans lien vers `CUR-50` alors que son acceptation dit « se lit », et une arête `CUR-59 → CUR-60` que le texte de `CUR-60` contredisait |
| 5. Hors-périmètre en nommant les frères | **oui sur le fond, non sur la forme** | Les 40 renvois se résolvent tous. Mais **tous par titre, aucun par identifiant**, et cinq titres cités étaient inexacts **le jour même de leur écriture** — voir journal 67 |
| 6. Faire trancher l'humain avant publication | **oui, strictement** | Aucune carte créée avant l'accord. Quatre tours de questions : les cinq arbitrages portés par la revue, puis la granularité, puis la validation d'ensemble, puis les deux points remontés par la relecture |
| 7. Créer les cartes et transitionner | **oui** | 17 issues rattachées au projet, `blockedBy` posés, `CUR-47` en `Todo` et les seize autres en `Backlog` — colonne **mécaniquement** déduite. Feature passée de `Spec` à `In Progress`, étiquette de revue retirée |
| 8. Ne pas concevoir | **oui** | Aucune test list, aucun plan de design. Les deux passages qui prescrivent (l'identifiant frappé hors du lanceur, le ViewModel qui prend son jeton au registre) sont **repris verbatim de la spec**, où ils sont explicitement des « orientations que le découpage doit connaître » — vérifié par l'agent 4 |

**Une clause du skill n'a pas pu être tenue faute de matière** : l'étape 1 suppose de partir du
découpage candidat existant. **Il n'existait plus** — la fiche du tour 8 en atteste l'existence et le
résume, sans le porter (journal 66). Il a fallu le reconstruire depuis l'inventaire, la recette et la
règle de répartition.

## 4. Qualité de la sortie

> **Jugée par six relecteurs indépendants**, chacun contre le référentiel écrit *et contre le code*,
> puis par le binôme sur ce qui relevait d'un arbitrage. C'est le premier artefact de cette série
> dont la sortie soit jugée par autre chose que son auteur.

**Le découpage lui-même tient.** Les deux vérifications qui pouvaient le condamner passent sans
réserve : les 31 lignes se répartissent **sans reste**, et les cinq clauses du scénario de
concurrence atterrissent sur quatre objets **sans orpheline ni doublon**. Les quatre incréments
mutants sans clause le **disent** au lieu d'en inventer une. Aucune carte ne tombe sous le test de
départage.

**Mais il portait une régression que rien dans le dispositif n'aurait attrapée.** Entre les deux
premiers incréments, plus rien ne matérialisait la base d'un projet : un projet créé depuis la
fenêtre se créait puis refusait de s'ouvrir, un dépôt cloné aussi. Le défaut vient d'une conséquence
mécanique de la coupe — l'incrément en lecture seule emportait le passage de l'ouverture en lecture
pure, puisque c'était lui qui la rendait pure — et la clause de non-régression ne le voyait pas :
elle ne visait que des projets préexistants, qui ont déjà leur base. **Il est corrigé** (`D-068` §2).

⚠️ **Le fait de méthode le plus important de ce tour** : ce défaut ne vivait **dans aucune carte**. Il
vivait dans l'**intervalle** entre deux. Aucun des cinq relecteurs de contenu ne l'a vu ; seul l'axe
qui lisait les dix-huit l'a produit. **Un découpage n'est donc pas relisible carte par carte**, et un
dispositif qui ne relit que des cartes conclura qu'il est conforme.

**Le contraste avec la revue de spec est le second fait, et il est net.** L'analyse de série écrite le
même jour établit que huit tours de `revue-spec` ont rendu **une seule** remarque d'architecture sur
111, et **68 remarques invisibles au code**. Ici, **un** tour sur l'artefact suivant sort une
douzaine de faits faux démentis par le dépôt et un défaut qui aurait cassé la fenêtre en production.
La différence n'est pas le dispositif — c'est que **le découpage se confronte au code, quand la spec
ne se confrontait qu'à elle-même**.

**Ce que les faits faux avaient en commun** : tous écrits de mémoire, dans une session qui venait de
lire la spec et croyait connaître le dépôt. Une coupe attribuée à un `.gitignore` qui ne la porte pas
· un nom de projet réputé vivre à deux endroits · une clôture référentielle qui refuse une arête
qu'elle accepte · `D-021` cité deux fois pour `D-019` · deux cartes revendiquant chacune le même rang
dans une liste de trois · un drapeau `Kill(entireProcessTree)` présenté comme absent alors qu'il est
posé depuis le jalon 6a. **C'est exactement le mode d'échec que la mémoire persistante de ce projet
avait déjà consigné deux fois** — « les deux erreurs venaient de mémoires écrites de tête, non
revérifiées ».

**Ce que la relecture n'a pas pu établir** : si le découpage est *le bon*. Aucun des six n'a de
référentiel pour juger qu'une frontière est meilleure qu'une autre — le seul juge en est l'usage, et
il commencera au premier plan de design. La consigne « tu ne redécoupes pas » a été respectée, ce qui
veut aussi dire que **le découpage n'a été contesté par personne**.

## 5. Frictions

Journal des frictions, entrées **66** (l'instrument du tour 8 avait disparu — un axe de revue qui
produit une pièce durable doit nommer où elle est déposée, sinon la seule trace en est l'éloge) et
**67** (ce qu'un tour de relecture sur le découpage rend, là où huit tours de revue de spec ne
rendaient plus rien ; le défaut qui vit dans l'intervalle et non dans une carte ; les cinq renvois
périmés le jour de leur écriture ; la carte fantôme).

**Deux manques du skill `decoupage`**, tous deux découverts en l'exécutant :

- **il ne dit pas qui répond aux questions que la revue lui a portées.** Cinq remarques de la spec
  avaient été soldées avec le motif *« ça se tranche en coupant »*, et aucune des huit étapes ne les
  réclame. Elles ont été posées à l'humain parce que le binôme les avait en mémoire, pas parce que le
  dispositif les demandait — sur une session neuve, elles auraient été tranchées **en silence** par
  celui qui coupe ;
- **il ne demande pas d'inventorier les cartes déjà présentes dans le projet.** `CUR-32` — le serveur
  MCP en daemon sans fenêtre — est restée vivante et priorisée pendant que le découpage créait la
  carte qui l'absorbe, alors que la spec disait explicitement qu'elle « reste celle qui portera ce
  sujet ». Découpe et backlog existant ne se sont jamais rencontrés.

## 6. Ce que le tour a changé

**Dans `decisions.md`** : `D-068`, qui inscrit les cinq arbitrages du découpage — le plus petit
incrément recettable gagne · un incrément en lecture seule précède le premier mutant et porte la
porte entière · le registre des runs en vol naît avec le lancement · le recablage de la fenêtre est
une charge propre et non une clause de recette · la remise en état d'un projet sans base est *retirer
puis réinscrire*, sans ligne neuve à l'inventaire — plus l'amendement que la relecture lui a imposé.

**Dans le backlog** : 17 incréments, 1 carte latérale, 1 carte fermée en doublon, la feature en
`In Progress`.

**Dans les skills** : **rien encore**, et c'est délibéré. Les deux manques du §5 sont consignés, pas
corrigés — `D-039` demande un second artefact de chaque espèce avant de légiférer, et ce découpage
est le premier.

**Ce qui n'a pas changé et qu'on aurait pu croire** : le gabarit de fiche de rex. Le §5 de ce dossier
dit déjà « renvoi, pas de recopie » ; ce qui a manqué au tour 8 n'était pas une rubrique, c'était un
**dépôt** pour l'artefact — un autre objet, qui ne relève pas de ce gabarit.

## 7. Verdict pour `decoupage`

## **Verdict : promu, avec deux manques nommés**

Le skill a tenu son premier tour réel. Ses huit étapes ont toutes servi, la porte de l'étape 6 —
*rien ne se publie avant accord* — a fonctionné strictement, et le résultat passe les deux
vérifications qui pouvaient le condamner. Il sort de l'état `draft` que `D-045` avait laissé.

**Ce qui empêche un verdict plus net** : les deux manques du §5 sont **structurels**, pas
cosmétiques. Un découpage exécuté par une session neuve, sans le binôme qui avait la spec en tête,
aurait tranché cinq questions en silence et ignoré une carte préexistante. Le skill est donc promu
**pour ce qu'il fait**, et à corriger sur ce qu'il ne demande pas.

⚠️ **Et un doute que cette fiche ne peut pas lever** : la relecture par six agents n'est portée par
**aucun** skill, elle a été improvisée à la demande de l'utilisateur, et c'est elle qui a trouvé le
défaut grave. Si elle devient la norme, alors le verdict porte sur un couple *découpage + relecture*
et non sur `decoupage` seul — et un tour de découpage **sans** relecture ne vaudra pas celui-ci. La
série le dira ; ce tour ne le peut pas.
