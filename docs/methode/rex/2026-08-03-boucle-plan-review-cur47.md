# La boucle `Plan Review` de `CUR-47`, de bout en bout — et l'essai de relecture greffé dessus

> **Cette fiche couvre une boucle entière, pas un tour.** Quatre temps se sont enchaînés sur le même
> artefact — le plan de design de `CUR-47` : sa production, un essai de relecture interne, la revue de
> colonne, puis la correction et la vérification. **Deux de ces temps ne sont portés par aucun skill** :
> `correction` et `verification` n'existent pas, et tout leur geste s'est reconstruit depuis la DoD.
> Trois fiches dont deux mesureraient un skill inexistant ne se compareraient à rien ; la boucle, elle,
> se comparera à la prochaine.
>
> ⚠️ **Conséquence sur la rubrique 2 : les chiffres s'y ventilent par temps.** C'est ce qui préserve la
> comparabilité pour laquelle ce dossier existe — un total de boucle qui mélangerait un plan de design,
> trois relectures et une vérification ne se comparerait à rien non plus.
>
> ⚠️ **Un essai de méthode est greffé sur la boucle**, et ses mesures sont de la matière de rubrique 2 au
> même titre que le reste : le relecteur interne a tourné **trois fois** sur le même artefact non corrigé,
> pour isoler la variable *dispositif* de la variable *position dans le flux*. C'est cet essai qui a
> produit `D-073` — le relecteur interne devient multi-axes — et l'amendement des skills `discovery` et
> `spec`. Une fiche qui n'en parlerait pas raconterait un cycle ordinaire, ce que ce tour n'a pas été.
>
> ⚠️ **Trois premiers tours réels dans la même boucle** : `plan-design`, `revue-plan`, et le dispositif
> de `D-071`. Aucun n'avait jamais tourné.
>
> **Heures données en heure locale du dépôt**, celle des commits.

## 1. Ce qui a tourné

Quatre temps, dans l'ordre. L'artefact est le même du premier au dernier : le document Linear
**« Plan de design — CUR-47 · La porte s'ouvre »**, attaché à la carte `CUR-47`.

### Temps 1 — `plan-design`, le 2026-08-02

**Le skill `plan-design`**, invoqué **en session**, pas en sous-agent. Le binôme venait d'ouvrir la
journée sur *« j'aimerais continuer sur le dev de la feature du serveur MCP »* ; la carte `CUR-47` était
en `Todo`, seule non bloquée du découpage de la veille.

**La commande, verbatim** :

```
Skill(skill: "plan-design",
      args: "CUR-47 — La porte s'ouvre : le serveur MCP monte, et l'agent liste les projets")
```

Répertoire de départ : la racine du dépôt. **Session non neuve** — c'est la même session qui avait
découpé la feature quelques heures plus tôt, et le régime le veut : concevoir n'est pas relire.

**Trace que le skill a servi** : la carte passée en `Planning` avant l'écriture (18:43), le document
attaché créé d'un bloc (18:44) avec schéma-delta `mermaid` en tête, table « Objets impactés » et section
d'écarts — les étapes 2, 3 et 6 sont observables une à une. L'étape 6 est aussi observable **par sa
violation** : la carte a été poussée en `Plan Review` à 18:44, puis reposée en `Planning` + `Done` à
18:48 (journal 68).

⚠️ **L'artefact relu ensuite n'est pas la sortie de `plan-design`.** Le même soir à 22:36, le chantier
qui a produit `D-070` a repris le plan : sa première version **découpait en six pas nommés** — ce que
`plan-design` §4 lui prescrivait alors, sous le titre *« Découper en pas »* — et les pièges que ces pas
portaient sont redescendus sur les objets. **Toute la boucle de relecture porte donc sur la version
d'après**, et aucun jugement de conformité rendu plus tard ne s'applique au plan tel qu'il est sorti du
skill.

### Temps 2 — l'essai de `D-071`, deux dispositifs sur le même artefact

`D-071` prescrivait qu'un skill de production fasse relire son artefact avant de poser son signal, et
prévoyait explicitement son essai sur `plan-design` et `CUR-47`. Deux relectures internes ont donc été
lancées **sur le plan non corrigé**, en sous-agents `general-purpose`, **modèle `opus` forcé dans les deux
cas** — c'est ce qui rend la comparaison légitime.

⚠️ **Le référentiel était gelé avant la première des trois relectures.** Les trois axes de `revue-plan`
ont été écrits à 10:01 (commit *« la revue de plan applique enfin la DoD que le plan lui annonçait »*) ;
la première relecture part à 11:59. Les trois runs opposent donc le **même** référentiel, et l'écart de
récolte ne peut pas venir de là. Vérifié, parce que c'était la seule chose qui aurait pu invalider
l'essai.

**2a — un relecteur unique**, lancé à 11:59:43. Mandat verbatim, chemins personnels remplacés par
`<racine du dépôt>` :

```
Tu es relecteur. Tu relis un **plan de design** contre sa Definition of Done, dans le dépôt
Cursus (<racine du dépôt>).

## Ce que tu relis
Le document Linear d98d860e-383e-4f92-b2cf-d36756c81124 — « Plan de design — CUR-47 · La porte
s'ouvre ». Lis-le avec mcp__claude_ai_Linear__get_document.
Lis aussi la carte CUR-47 elle-même (mcp__claude_ai_Linear__get_issue) : c'est le ticket que ce
plan est censé servir.

## Ton référentiel
**docs/methode/dod/story/plan-review.md** — applique-le **clause par clause**. C'est le seul
référentiel opposable de ta relecture ; il nomme lui-même ses axes et son critère.

Les documents qu'il te faudra pour instruire ces axes :
- docs/design/architecture.md — l'état présent du code et du découpage ;
- docs/methode/tickets.md — ce que contient un incrément, et ce qui distingue un incrément d'un pas ;
- CLAUDE.md à la racine — les conventions de code et de modélisation du dépôt ;
- le code lui-même, quand une affirmation du plan porte sur ce qui existe. **Vérifie plutôt que de
  croire** : un plan qui décrit un type ou un comportement existant peut se tromper, et c'est le
  genre de défaut que seule une lecture du code attrape.

## Trois interdits, et ils comptent
1. **Ne corrige rien.** Tu ne modifies aucun fichier, tu ne réécris pas le plan, tu ne proposes pas
   de rédaction de remplacement. Tu **constates**.
2. **Ne touche à rien dans Linear.** Aucun commentaire posé, aucune étiquette, aucun déplacement de
   carte, aucune sauvegarde de document. Lecture seule.
3. **N'ouvre aucun fichier sous ~/.claude/projects/** — ni MEMORY.md, ni les fichiers de mémoire.
   Ils contiennent le point de vue de l'auteur du plan, et ils t'ancreraient. Si un extrait t'en est
   présenté d'office dans ton contexte, **signale-le en tête de ton rapport** et n'en tiens pas compte.

## Ce que tu rends
Un rapport en **français**, structuré ainsi :
1. **Verdict** — Done (aucune remarque) ou Rework Needed, avec en une phrase ce qui le décide.
2. **Les constats**, un par bloc, du plus grave au plus léger. Pour chacun : la **clause de la DoD**
   qu'il met en défaut (ou l'axe, si c'est le critère opposable), ce que le plan dit, ce qui ne va
   pas, et **comment tu l'as vérifié** (quel fichier, quel type). Sois précis : un constat qu'on ne
   peut pas re-vérifier ne vaut rien.
3. **Ce que tu as vérifié sans rien trouver** — brièvement, les axes ou clauses que tu as instruits
   et qui passent. C'est ce qui permet de mesurer la couverture de ta relecture.
4. **Tes doutes** — ce sur quoi tu n'as pas su trancher, et pourquoi. Ne les déguise pas en constats.

Ne cherche pas à trouver un nombre de défauts. Un plan sans remarque est un résultat légitime ; un
constat inventé pour remplir la liste coûte plus qu'il ne rapporte.
```

Options : `subagent_type: general-purpose`, `model: opus`, `run_in_background: false`.

**2b — trois axes parallèles plus un agrégateur**, lancé à 14:04, **après** que la revue de colonne a
rendu son verdict. C'est délibéré : l'essai cherchait l'effet du **dispositif**, pas celui de la
fraîcheur de l'artefact, qui n'a pas bougé entre les trois runs. Mandat verbatim, même substitution de
chemin :

```
Tu orchestres une **relecture interne** d'un plan de design, dans le dépôt Cursus
(<racine du dépôt>).

## L'artefact
Le document Linear d98d860e-383e-4f92-b2cf-d36756c81124 — « Plan de design — CUR-47 · La porte
s'ouvre ». Lis-le avec mcp__claude_ai_Linear__get_document. La carte CUR-47 porte le ticket que ce
plan sert : lis sa **description** (mcp__claude_ai_Linear__get_issue).

## Le dispositif — trois axes, en sous-agents parallèles, jamais fusionnés
Lis .claude/skills/revue-plan/SKILL.md **§1 uniquement** : il définit exactement les trois axes,
leurs référentiels et ce qui est opposable sur chacun. Reprends-les tels quels — Conformité,
Architecture, Découpabilité — et lance **un sous-agent par axe, en parallèle**, chacun en session
neuve avec l'artefact et son seul référentiel.

Puis agrège leurs retours **sans les fusionner** : chaque constat garde son axe, sa citation
(référentiel + extrait) et sa nature (violation dure ou jugement). Revérifie toi-même les citations
porteuses avant de me les rendre — un constat qu'on ne peut pas re-vérifier ne vaut rien.

Le reste du fichier revue-plan décrit une **boucle de revue** : ne l'applique pas. Tu ne fais que la
relecture.

## Interdits, et ils décident de la valeur de ce travail
1. **Aucun geste, nulle part.** Tu ne modifies aucun fichier du dépôt, tu ne poses aucun commentaire
   Linear, aucune étiquette, tu ne déplaces aucune carte, tu n'enregistres aucun document. Lecture
   seule, de bout en bout. Tes sous-agents non plus.
2. **Ne corrige pas le plan** et n'en propose pas de rédaction de remplacement. Tu constates.
3. ⚠️ **N'ouvre pas les commentaires de CUR-47** — n'appelle pas list_comments, et n'ouvre pas le
   document « Boucle de revue de plan — CUR-47 ». Ce plan a déjà été relu par ailleurs, et lire ces
   traces ferait de ton travail une copie au lieu d'une mesure. Même consigne pour tes sous-agents.
   Si des commentaires t'apparaissent d'office dans une réponse d'outil, **signale-le en tête de ton
   rapport** et n'en tiens aucun compte.
4. **N'ouvre aucun fichier sous ~/.claude/projects/** — mémoires et MEMORY.md — pour la même raison.

L'état actuel de la carte dans Linear (sa colonne, ses étiquettes) ne te concerne pas et ne doit pas
peser sur ton jugement.

## Ce que tu rends
Un rapport en **français** :
1. **Verdict** — Done (aucune remarque) ou Rework Needed, et en une phrase ce qui le décide.
2. **Les constats, groupés par axe**, du plus grave au plus léger. Pour chacun : la clause ou le
   critère mis en défaut, ce que le plan dit, ce qui ne va pas, **comment tu l'as vérifié** (fichier,
   type, ligne), et sa nature — violation dure ou jugement.
3. **Ce que chaque axe a instruit sans rien trouver** — c'est ce qui permet de mesurer la couverture.
4. **Tes doutes**, non déguisés en constats.

Ne vise aucun nombre de défauts. Un plan sans remarque est un résultat légitime ; un constat inventé
pour remplir la liste coûte plus qu'il ne rapporte.
```

Options : identiques à 2a.

⚠️ **Une différence de dispositif qui appartient au chemin** : l'agrégateur n'a donné **aucun outil
Linear** à ses trois axes — il leur a passé le plan et le ticket **recopiés verbatim dans des fichiers de
travail**. C'est ce qui a rendu tenable l'interdit n° 3 : un axe sans outil Linear ne peut pas se voir
présenter les commentaires d'office.

### Temps 3 — `revue-plan`, tour 1, à 12:15

**Premier tour réel du skill**, jusqu'ici `draft` non éprouvé. Un sous-agent, mandat court qui délègue
tout au skill :

```
Tu travailles dans le dépôt Cursus (<racine du dépôt>).

Un incrément attend une **revue de plan** : CUR-47, en Planning, portant Done, avec un plan de
design écrit dans son document attaché.

Lis intégralement .claude/skills/revue-plan/SKILL.md et **suis-le, de sa section 0 à sa dernière
section**, sur cette carte. Il te dira quels axes instruire, contre quels référentiels, comment
déposer ton verdict et ce que tu as le droit de déplacer.

Le dépôt porte ses conventions dans CLAUDE.md à la racine — il est chargé d'office, mais relis-le
si un point te manque.

Quand tu as fini, rends-moi un compte rendu en **français** : le verdict que tu as posé, les
remarques que tu as déposées (leur substance, pas seulement leur nombre), ce que tu as instruit sans
rien trouver, et les gestes que tu as effectivement faits côté Linear.
```

Options : `subagent_type: general-purpose`, `model: opus`, `run_in_background: false`.

**Trace que le skill a servi** : la carte tirée de `Planning` vers `Plan Review` avec retrait de `Done`
à la prise — l'unique déplacement que §0 autorise ; douze remarques posées **sur la carte** et jamais sur
le document (`D-045`), chacune avec son repère calculé ; un **second document attaché**, « Boucle de revue
de plan — CUR-47 », portant une entrée `## Tour 1` autoportante ; l'étiquette `Rework Needed` ; la carte
laissée **non assignée** et **non déplacée** à la sortie.

### Temps 4 — la correction, puis la vérification

**4a — la correction, menée à la main.** `correction` n'existe pas ; le geste s'est fait **dans la session
du binôme**, sans sous-agent et donc **sans mandat**. ⚠️ **C'est le trou de cette rubrique, et il est
irréparable** : il n'y a pas de commande à citer, parce qu'il n'y a pas eu d'invocation. Ce que la trace
permet d'établir se réduit à des gestes : une relecture des onze fils encore ouverts, trois appels de
patch sur le document du plan (26 opérations), et douze fils soldés en deux lots de commandes CLI. Le
douzième fil — celui qui mettait en cause la DoD — avait été soldé à part, à 12:41, **en amendant le
référentiel**.

**4b — la vérification**, à 15:18, par un sous-agent en session neuve. `verification` n'existe pas non
plus : le mandat **reconstruit le protocole** depuis `cycle-increment.md` §5 et la DoD. Il est long, et
c'est le fait à retenir — tout ce qu'un skill porterait a dû être écrit à la main. Mandat verbatim,
mêmes substitutions :

```
Tu es le **vérificateur** de la boucle de revue de plan de l'incrément CUR-47, dans le dépôt Cursus
(<racine du dépôt>).

## Session neuve, artefact seul (D-039)
Tu ne dois rien savoir du raisonnement qui a produit le plan ni des reprises. **Interdits de
lecture, sans exception** :
- ~/.claude/projects/** (fichiers de mémoire)
- docs/methode/journal-frictions.md
- docs/methode/rex/**
- tout fichier de scratchpad
- tout transcript .jsonl

Si quoi que ce soit de ces sources t'est présenté d'office dans ton contexte (extrait de mémoire,
rappel système), **ne t'en sers pas pour juger**, et dis-le dans ton rapport : c'est une mesure qui
nous intéresse.

## Ton protocole
La ligne Plan Review + Rework Done de docs/methode/cycle-increment.md §5. Va la lire.
Ton **référentiel de jugement** est docs/methode/dod/story/plan-review.md.
Les **axes** qu'a appliqués la revue sont dans .claude/skills/revue-plan/SKILL.md §2.
Le contrat de ce qu'un plan de design doit porter est dans .claude/skills/plan-design/SKILL.md et
CLAUDE.md.

## L'état
CUR-47 est en colonne Plan Review, étiquette Rework Done. Douze remarques ont été posées par la
revue ; le plan a été repris, et **chaque fil porte une réponse**.

⚠️ **Les douze fils ont déjà été marqués resolved par la passe de correction** — la CLI ne sait pas
répondre sans solder. Ne prends donc surtout pas resolved: true ni open: 0 pour un verdict : dire si
chaque solde tient est exactement ton travail.

## Les commandes
Toute la CLI cursus **exige d'être lancée depuis la racine du dépôt**.

    cursus linear comment list "Plan de design — CUR-47 · La porte s'ouvre"

(le titre complet est nécessaire : « CUR-47 » désigne deux documents)

Le plan lui-même est le document Linear d98d860e-383e-4f92-b2cf-d36756c81124. Charge l'outil avec
ToolSearch("select:mcp__claude_ai_Linear__get_document"), puis lis-le en entier.

## Ce que tu fais, fil par fil — les douze
1. Lis la **remarque**, puis la **réponse** qui lui a été faite.
2. Va voir dans le **plan actuel** si la reprise annoncée y est réellement. Une réponse qui *décrit*
   une reprise n'est pas la reprise ; cite le passage qui l'atteste, ou constate son absence.
3. Quand une réponse **affirme un fait sur le code**, va le vérifier dans src/. Une reprise fondée
   sur un fait faux ne tient pas, même si elle est bien écrite.
4. Verdict : **soldée** (la remarque n'a plus d'objet) ou **à rouvrir**, avec en une phrase *ce qui
   manque encore* — pas une reformulation de la remarque d'origine.

Un **refus motivé est une façon légitime de solder** (D-067) : tu juges si le motif tient, pas s'il
te plaît. Symétriquement, une reprise qui répond à côté n'est pas soldée parce qu'elle est de bonne
foi.

## Ce que tu écris dans Linear
- **Si les douze tiennent** : rien à poser dans les fils. Passe l'étiquette de la carte à Done —
  Linear MCP save_issue sur CUR-47, labels: ["Done"]. ⚠️ La liste **remplace** tout l'ensemble ;
  Rework Done est aujourd'hui la seule étiquette de la carte, donc il n'y a rien à préserver.
- **Si tu rouvres** : la CLI n'a **aucun verbe pour dé-solder**. Pose une **remarque neuve** par fil
  rouvert :

      cursus linear comment add "Plan de design — CUR-47 · La porte s'ouvre" -q "<le passage cité,
      verbatim du plan>" -b "<ce qui manque encore>"

  et passe l'étiquette à Rework Needed. Signale dans ton rapport ce que ce contournement t'a coûté.
- ⚠️ **Vérifie l'état après coup en relistant.** La CLI sort en code 0 même quand l'API refuse — ne
  fais jamais confiance à la sortie de tes propres commandes, seulement au compteur relu.

## Ce que tu rends
Un rapport factuel, en **français** (diacritiques complets et corrects), qui servira de matière
brute à un retour d'expérience. Il porte :
1. **Le verdict des douze**, un par ligne : id court du fil, sujet en quelques mots, soldée ou à
   rouvrir, et **la pièce qui l'atteste** — extrait du plan, ou chemin:ligne du code.
2. **Combien de fils t'ont demandé d'ouvrir le code**, et lesquels. C'est une mesure, pas une
   anecdote.
3. **Ce qui t'a coûté cher ou t'a manqué** : outillage absent, clause de référentiel muette sur le
   cas, remarque impossible à juger sans le fil qui l'a produite, ambiguïté de la CLI. Sois précis et
   cite.
4. **Ce que tu as failli conclure à tort**, s'il y a lieu — un fil où la réponse était convaincante
   et le plan ne suivait pas, ou l'inverse.
5. **L'état final de la carte**, relu après ton geste : colonne, étiquette, open / total.

Tu ne réécris **pas** le plan. Tu ne corriges rien. Tu juges, et tu poses l'étiquette que ton
jugement commande.
```

Options : `subagent_type: general-purpose`, **lancé en arrière-plan** (le seul des quatre), modèle hérité
— résolu en `claude-opus-5`.

### Comment ces mandats ont été retrouvés

⚠️ **Ils n'étaient plus dans le contexte** : la session a été compactée entre la correction et la
vérification. Ils ont été **récupérés par grep ciblé sur le transcript de session**, en extrayant le
champ `prompt` des seules invocations de sous-agents. Aucun n'a été reconstitué de mémoire ; les cinq
blocs ci-dessus sont copiés du transcript, à la seule substitution des chemins personnels près. **Le
mandat manquant — celui de la correction — manque parce qu'il n'a jamais existé**, pas parce qu'il a été
perdu. Ce que la manœuvre coûte est en rubrique 5.

## 2. Chiffres

### Temps 1 — `plan-design`

| | |
|---|---|
| Durée, invocation → document déposé | **10 min 01** (18:34:26 → 18:44:27) |
| Appels d'outils | **31** |
| Sous-agents ouverts | **0** |
| Jetons | **non mesurables** — le skill a tourné en session, pas en sous-agent |
| Sorties | 1 document Linear (schéma-delta, table « Objets impactés », arbitrages, écarts, **six pas nommés**), 1 carte passée en `Planning` |
| Gestes à défaire | **1** — la carte poussée en `Plan Review`, reposée 4 min plus tard |
| Reprise ultérieure de l'artefact, hors boucle | **1** — les six pas retirés, 8 pièges redescendus sur leurs objets, le soir même |

### Temps 2 et 3 — l'essai : trois relectures du même artefact

Même plan, **non corrigé entre les trois**, même modèle (`opus`), même référentiel (gelé à 10:01).

| Dispositif | Position | Durée | Jetons | Appels d'outils | Constats de fond |
|---|---|---|---|---|---|
| **1 relecteur** | interne | **404 s** | **118 369** | **21** | **4** (5 rendus, dont 1 procédural écarté) |
| **3 axes parallèles** | interne | **786 s** | ≥ **104 460** | **17** | **10** |
| **3 axes parallèles (`revue-plan`)** | colonne | **1 093 s** | ≥ **136 902** | **38** | **11** comptés · **12** remarques posées |

⚠️ **Les deux dernières lignes de la colonne « Jetons » sont des minorants, la première est complète.**
Les métriques d'un sous-agent **de** sous-agent ne remontent pas à la session appelante : les 104 460 et
136 902 sont ceux du seul agrégateur/orchestrateur, sans les trois axes. **La comparaison de coût entre
les trois dispositifs est donc impossible en l'état** ; seule la comparaison des **récoltes** tient, et
c'est précisément ce que `D-073` mesure. Pour la rendre possible au tour suivant, il faudrait demander à
l'agrégateur de relever lui-même les chiffres de ses axes — ce que les fiches de `revue-spec` font depuis
le tour 3.

⚠️ **12 posées, 11 comptées, et la règle de comptage n'est écrite nulle part.** La mesure de `D-073`
retient 11 constats de fond pour la revue de colonne, quand douze remarques ont été déposées sur la
carte. L'écart est probablement un constat procédural écarté, comme pour le relecteur unique — mais rien
n'en fixe le critère, et la fiche ne peut pas le reconstituer. **À écrire avant le prochain essai**, sans
quoi la ligne « constats de fond » n'est pas reproductible.

**La récolte de la revue, détaillée** : 12 remarques — **3 Conformité**, **5 Architecture**,
**4 Découpabilité** ; **3 violations dures** (l'arête de schéma inversée, la clause de DoD sur les pièges,
la contradiction interne sur les questions ouvertes), **9 jugements**. Aucune remarque ne porte deux axes.
Carte : **0 → 12 commentaires**, `open` 12. Étiquette `Rework Needed`.

**Ce que les deux récoltes internes ont en propre** (`D-073` §3) : les trois axes internes ont **manqué
quatre** constats de la revue et en ont **produit trois** qu'elle n'avait pas vus. L'agrégateur a **retiré
un sous-point faux** de ses propres axes et **produit la contre-preuve** d'un autre — travail vérifiable,
et c'est ce qui distingue un agrégateur d'un greffier.

### Temps 4a — la correction, à la main

| | |
|---|---|
| Durée, première relecture des fils → `Rework Done` posé | **15 min 30** (14:37:49 → 14:53:19) |
| Appels d'outils | **21** |
| Sous-agents ouverts | **0** |
| Jetons | **non séparables** — la passe a tourné dans la session du binôme |
| Fils soldés | **12** — 1 isolé à 12:41, puis 4 + 7 en deux lots |
| Appels de patch sur le plan | **3**, totalisant **26 opérations** |
| Échecs silencieux | **1** — un identifiant tronqué refusé par l'API, sans arrêter le lot (journal 81) |
| Documents du dépôt amendés | **2** — la DoD `plan-review.md` et le skill `plan-design` |

### Temps 4b — la vérification, en sous-agent

| | |
|---|---|
| Durée | **298 s** |
| Jetons | **122 805** |
| Appels d'outils | **20** |
| Sous-agents ouverts | **1** |
| Fils jugés | **12 — 12 soldés, 0 rouvert** |
| Fils ayant exigé d'ouvrir `src/` | **5 sur 12** |
| Fils ayant exigé un référentiel du dépôt hors du plan | **4 sur 12** |
| Fils jugeables sur le plan seul | **3 sur 12** |
| Remarques neuves posées | **0** |
| État final | `Plan Review` + `Done`, non assignée, `open` 0 / `total` 12 |

⚠️ **Le chiffre le plus parlant de ce tableau est « 3 sur 12 »** : un quart seulement des fils se
jugeait sur l'artefact. **Une vérification de plan est un travail de dépôt, pas un travail de document** —
c'est la première mesure de ce genre dans le dossier, et elle n'a pas d'équivalent côté `revue-spec`, où
l'analyse de série avait établi que 68 remarques sur 111 étaient **invisibles au code**.

### La boucle entière

| | |
|---|---|
| Durée calendaire | **2026-08-02 18:34 → 2026-08-03 15:23**, avec de longs intervalles hors boucle |
| Somme des segments mesurés | **≈ 71 min** de travail effectif, dont 47 min d'agents |
| Sous-agents ouverts | **3 de premier rang**, **plus 3 axes** sous le second et **3** sous la revue |
| Tours de boucle | **1** — aucune escalade, aucun second tour |
| Remarques, de bout en bout | **12 posées, 12 soldées, 0 refusée, 0 rouverte** |

⚠️ **« 0 refusée » n'est pas un résultat neutre.** `D-067` a fait du refus motivé une issue de plein
droit, précisément parce que huit tours de revue de spec avaient retenu 98 remarques sur 98. Cette
boucle-ci, la première d'après, retient **12 sur 12**. Une des douze s'est pourtant soldée en donnant
tort au référentiel plutôt qu'à l'artefact — c'est une troisième issue, pas un refus (journal 84).

## 3. Conformité au protocole

### `plan-design`, jugé contre le skill **tel qu'il était le 2026-08-02**

⚠️ **Deux de ses clauses d'aujourd'hui n'existaient pas à l'exécution** : `D-069` (le ticket n'est jamais
poussé) a été écrit le soir même à 19:22, et `D-070` (le plan ne découpe plus en pas) à 22:35. Juger le
plan contre le skill d'aujourd'hui serait un anachronisme.

| Étape | Tenue ? | Pièce |
|---|---|---|
| 1. Décider si l'étape a lieu | **oui** | Incrément créant huit objets et deux projets — le cas nominal, aucune hésitation à consigner |
| 2. Choisir où vit le plan | **oui** | Document attaché à la carte, branche nominale de `CLAUDE.md` §*Où vit le plan*. Confirmé par la revue (clause « emplacement » tenue) |
| 3. Le schéma-delta en tête | **oui, exactement** | Les deux `classDef` sont **identiques au caractère près** au bloc « à recopier tel quel » de `schemas.md` ; les trois blocs ambre portent leur ligne `+`, aucun bloc vert ou neutre n'en porte. Vérifié deux fois — par l'axe Conformité de la revue et par celui de la relecture interne |
| 4. Découper en pas *(libellé d'alors)* | **oui** | Six pas nommés, chacun portant ses pièges. La clause a été **supprimée le soir même** par `D-070`, et le plan repris en conséquence |
| 5. Ne pas trancher seul une découpe non évidente | **sans objet** | Les frontières étaient lisibles ; `CONCEVOIR-DEUX-FOIS.md` n'avait pas à être lancé |
| 6. Terminer l'étape | **partiellement** | Le plan a été passé contre la DoD et `Done` posé ; mais **la carte a été poussée en `Plan Review`** (journal 68). La clause qui l'interdit a été écrite le soir même |

### `revue-plan`, premier tour réel

| Étape | Tenue ? | Pièce |
|---|---|---|
| 0. Tirer la carte, retirer `Done` | **oui** | Carte passée de `Planning` à `Plan Review`, `Done` retiré **au même geste**, à la prise |
| 0. Vérifier qu'un plan existe | **oui** | Le document a été chargé avant tout déplacement |
| 1. Invoquer `revue` sur **trois** axes | **oui** | Trois sous-agents parallèles, sessions neuves, jamais fusionnés. Chaque remarque nomme son axe en tête et cite son référentiel |
| 1. Aller lire `schemas.md` §6, ne pas juger de mémoire | **oui** | L'axe Conformité cite le bloc `classDef` du fichier et compare caractère par caractère |
| 1. Axe Découpabilité — **tenter réellement** | **oui, et c'est ce qui a payé** | Un découpage en pas a été tracé, puis la test list du premier écrite : **4 cas sur 6 s'écrivent**, et c'est en écrivant le premier test que l'axe a buté sur la clé de la garde. Les deux constats les plus graves de toute la boucle viennent de là |
| 1. Écart à `architecture.md` : seul l'écart **tu** est opposable | **oui** | Deux écarts nommés et motivés par le plan (`§7.15.3`, `§7.12`) laissés hors remarque ; l'écart **tu** sur `ProjectsTool` opposé |
| 1. `revue` ne déplace jamais la carte | **oui** | Carte laissée en `Plan Review` à la sortie |
| 2. Document de boucle, entrée autoportante | **oui** | Second document créé — « Boucle de revue de plan — CUR-47 » —, entrée `## Tour 1` portant verdict par axe, point en litige de chacun, et ce qui n'a pas été opposé |
| 3. Compter les tours sur la carte | **sans objet** | Premier tour, aucun compteur à lire |
| 4. Boucler ou escalader | **oui** | `Rework Needed` posé, carte **non assignée** — pas d'escalade au premier tour, ce que le skill prescrit |
| Poser la remarque sur la carte, jamais sur le document | **oui** | 12 remarques via la CLI, chacune avec son repère calculé ; `updatedAt` du plan inchangé par la revue |

**Une clause du skill a été mise en défaut par son propre exercice**, et le skill l'a bien supporté : la
clause de DoD *« les pièges sont dans la table »* a produit une **violation dure sur un plan qui tenait
l'invariant** — ses huit pièges nommaient chacun leur objet, dans une section dédiée. Le relecteur n'a pas
tranché seul : il a posé *« lequel des deux documents change »* en citant les trois formulations
concurrentes (journal 75, 76).

### `correction` et `verification` — aucun protocole à confronter

`cycle-increment.md` §5 leur donne **un livrable et un état posé, aucun protocole**. Ce qui se vérifie se
réduit donc à ces deux colonnes :

| Ce que §5 exige | Tenu ? | Pièce |
|---|---|---|
| `correction` → « le plan repris, **une réponse dans chaque fil** » | **oui** | 12 fils portent une réponse ; 26 opérations de patch sur le plan |
| `correction` → poser `Rework Done` | **oui** | Étiquette posée à 14:53 |
| `verification` → « chaque remarque soldée, ou rouverte avec ce qui manque » | **oui** | 12 verdicts, chacun avec sa pièce — extrait du plan, ou `fichier:ligne` du code |
| `verification` → « `Done` si `open` vaut 0 » | **tenu, et le critère est vide** | `open` valait **0 avant** que la vérification commence, la CLI ne sachant pas répondre sans solder. Un vérificateur qui n'aurait rien lu aurait rempli le critère à l'identique (journal 82) |

## 4. Qualité de la sortie

> **Jugée par quatre dispositifs indépendants** — un relecteur unique, trois axes agrégés, la revue de
> colonne à trois axes, puis un vérificateur en session neuve — dont **aucun** n'est l'auteur du plan.
> C'est le second artefact de cette série dont la sortie soit jugée par autre chose que son auteur, après
> le découpage de la veille.

**Le plan tient, et ce qui l'atteste est du code, pas un accord.** Les deux axes qui pouvaient le
condamner passent : **tous les faits de code allégués par le plan sont exacts**, vérifiés un par un par
l'axe Architecture de la revue — résidence de `ProjectWorkspace`, disposition par la coquille,
`ResolveConfigDirectory` et le `$XDG_CONFIG_HOME` vide, `SqliteProjectHost.Open`, l'absence totale de
point d'arrêt, la parenté du panneau tracker. Et **aucun fait de code affirmé dans les douze réponses de
la correction ne s'est révélé faux**, vérifié à son tour par le vérificateur. ⚠️ **C'est l'exact inverse
du mode d'échec du découpage de la veille**, où une douzaine d'assertions écrites de mémoire avaient été
démenties par le dépôt. La différence tenable : le plan de design décrit des objets qu'il faut ouvrir
pour nommer.

**Les deux défauts les plus graves étaient tous deux invisibles à un relecteur unique**, et tous deux
auraient coûté cher au premier cycle TDD :

- **la clé de la garde** — `ProjectWorkspaces` garantit *au plus un host par projet* sans que rien ne dise
  par quoi deux résolutions sont « le même projet ». `Project` est scellée sans `Equals`, et `Rename`
  substitue une instance neuve : garder par instance ouvre une seconde connexion SQLite au premier
  renommage, c'est-à-dire exactement le défaut que la garde existe pour empêcher. **Le test « quand on
  résout deux fois le même projet, alors le même workspace est rendu » ne s'écrit pas** ;
- **une ou deux instances de `ProjectRegistry`** — le registre charge son instantané une fois à la
  construction et ne relit jamais le disque. Deux instances divergent au premier ajout, et `list_projects`
  rendrait une liste périmée **pour toute la session**. Défaut silencieux qu'aucun test de pas n'attrape,
  et qui fait échouer l'acceptation de l'incrément à la recette.

**Ce que la revue de colonne a trouvé et que la relecture interne n'a pas vu**, et réciproquement : les
deux récoltes ne se recouvrent pas (`D-073` §3). Le gain de la colonne n'est donc **pas quantitatif —
10 contre 11 — mais combinatoire**. C'est le résultat le moins prévu de l'essai, et celui qui sauve la
colonne : trois issues étaient possibles, la colonne devient une formalité, elle double la relecture
interne, ou elle voit autre chose. C'est la troisième.

**Ce qui n'est pas tranché, et qu'il faut écrire net.** Les deux relectures ont **divergé sur l'axe
Conformité** : la revue y a posé trois remarques — dont une bijection schéma ↔ table tenue pour rompue ;
la relecture interne a déclaré **les six clauses tenues**, bijection comprise, après l'avoir appariée
nœud par nœud. **Laquelle a raison n'est pas établi à cette date**, et la fiche ne peut pas le trancher :
il faudrait relire le schéma et la table à trois, contre la même version du document, ce que personne n'a
fait. Ce que le cas apprend en revanche est solide — *un référentiel opposable rend la **question**
délégable, pas la **réponse** convergente* (journal 77).

**Le solde le plus faible des douze, nommé par le vérificateur lui-même** : la remarque sur l'objet
portant le scénario « N clients, une instance » était **ancrée sur la section « La maille visée »**, et
cette section n'a pas été touchée. Le solde tient parce que `McpServerHost` est désormais nommé partout
et tombe sous une frontière annoncée — mais la pièce citée n'est pas celle que la remarque visait.

**Et un solde qui a failli être rouvert à tort**, dans l'autre sens : la remarque disait
« `ShellViewModel` **reçoit** un `ProjectRegistry` par constructeur », la réponse disait
« `ForCurrentUser()` est appelé **inline dans le constructeur de la coquille** ». Les deux se lisent comme
contradictoires ; c'est `App.axaml.cs` qui a tranché, et la réouverture aurait été fausse. ⚠️ **La pièce
qui a évité l'erreur est le code, pas le fil** — c'est le meilleur argument de cette boucle pour la clause
« va vérifier dans `src/` » du mandat de vérification.

**Ce que la boucle n'établit pas** : si le plan est *le bon*. Aucun des quatre dispositifs n'a de
référentiel pour juger qu'une découpe d'objets est meilleure qu'une autre ; le seul juge en sera
`decoupage-pas`, puis le premier cycle TDD. Et **un seul tour** a eu lieu : la mécanique de convergence du
skill — deux ou trois tours, puis escalade — n'a **pas été éprouvée du tout**.

## 5. Frictions

Journal des frictions, entrées **75** à **88**, regroupées sous quatre en-têtes de cette boucle — *premier
tour de `revue-plan`, et l'essai de `D-071`* (**75** à **78**), *première correction de plan, à la main*
(**79** à **81**), *première vérification de plan, à la main* (**82** à **86**). **Non recopiées ici.**

Une entrée antérieure vaut pour le temps 1 : **68** (le skill qui pousse une carte), consignée la veille
et déjà corrigée par `D-069`.

**Deux frictions portent sur le dossier `rex/` lui-même**, et sont nées de la rédaction de cette
fiche — **87** (une fiche écrite après compaction perd la rubrique 1, et le remède par extraction du
champ `prompt` ne couvre pas les gestes menés en session) et **88** (la règle de comptage des
« constats de fond » n'existe pas : l'essai compare 4 · 10 · 11 quand les dispositifs ont rendu
5 · 10 · 12). La seconde est de la même famille que la **65**.

**Une entrée est éprouvée plutôt que contournée, et il faut le noter** : **85** (la mémoire fuite d'office
dans un mandat qui l'interdit) trouve ici sa **septième** occurrence, et **la première où la parade se
laisse mesurer** — la mémoire avait été délibérément vidée de son contenu avant le tour, l'extrait présenté
au vérificateur annonçait **l'état** et **pas la réponse**, et le vérificateur a pu le constater et le
déclarer. C'est la confirmation directe de la friction 64 : on ne peut pas empêcher la fuite, on ne tient
que **ce qu'il y a à fuiter**.

## 6. Ce que la boucle a changé

**Dans `decisions.md`** : **`D-073`**, qui amende `D-071` — le relecteur interne devient **multi-axes**,
sur les axes de la revue qui suivra, avec un **agrégateur distinct du binôme** qui revérifie et peut
retirer un constat. L'entrée porte la table de mesure de l'essai et, ce qui compte autant, **ce que la
mesure ne dit pas** : la colonne de revue survit, pour un motif combinatoire.

**Dans les skills** :

- **`discovery` §7** et **`spec` §8** passent du singulier au pluriel — un agent de relecture qui lance
  un sous-agent par axe, trois pour `revue-discovery`, deux pour `revue-spec` ;
- **`plan-design` §4** gagne le critère de choix entre cellule de table et section « objet par objet »,
  contrepartie explicite de l'amendement de la DoD ;
- **`revue-plan` §1** avait gagné ses **trois axes** juste avant le tour — auparavant il ne citait la DoD
  nulle part, et quatre clauses du §1 n'étaient l'objet d'aucun axe.

**Dans les référentiels** : **`dod/story/plan-review.md`** perd le *« dans la table »* de sa clause sur les
pièges — **ce qui se coche est le nom de l'objet, jamais la mise en page** — et gagne, en §4, qu'un écart
à `architecture.md` n'est opposable que s'il est **tu** ; nommé et motivé, il est conforme. ⚠️ **C'est la première fois qu'un tour de revue de ce dépôt
solde une remarque en corrigeant son propre référentiel**, et le geste a été explicitement instruit par le
relecteur plutôt que tranché par lui.

**Dans le journal** : quatorze entrées, **75 à 88**.

**Dans le backlog** : `CUR-47` en `Plan Review` portant `Done`, non assignée donc **tirable** ; deux
documents attachés — le plan repris et le document de boucle. La suite est `decoupage-pas`, qui tirera vers
`In Progress` et **retirera l'étiquette en tirant**.

**Six commits sur `main` le 2026-08-03** portent cette boucle : *la revue de plan applique enfin la DoD que
le plan lui annonçait* (10:01) · *un document se fait relire avant de lâcher sa carte* (10:34, `D-071` —
écrit **avant** l'essai qu'il prescrit) · *une clause de DoD cochait une mise en page* (12:41) · *le
relecteur interne passe au pluriel* (14:29, `D-073`) · *ce que la première correction de plan apprend*
(14:53) · *la vérification tient les douze, et découvre que son critère de sortie est vide* (15:26). Les
quatre autres commits du jour appartiennent à des chantiers voisins — la vérification de `D-070` et
`D-072`.

**Ce qui n'a pas changé et qu'on aurait pu croire** : `correction` et `verification` n'ont **pas** été
écrits. C'est délibéré — `D-039` veut que le journal écrive les skills, et la matière du premier jet vient
d'être récoltée. Les écrire dans la foulée reviendrait à écrire un dispositif d'avance, ce que le dépôt
s'interdit depuis `D-045`.

## 7. Verdicts, un par skill éprouvé

### `revue-plan` — **promu**

Premier tour réel, et le skill a fait ce qu'il prescrit, clause par clause. Ses trois axes ont tourné
séparément, l'axe Découpabilité a **réellement tenté** le découpage et la test list — c'est de là que
sortent les deux constats les plus graves de toute la boucle —, le document de boucle existe et son entrée
est autoportante, et l'unique déplacement de carte a eu lieu à la prise. **Il sort de l'état `draft` que
`D-045` avait laissé.**

**Ce qui empêche un verdict sans réserve, et ce n'est pas cosmétique** :

- **son axe Conformité a divergé** avec une relecture indépendante contre le même référentiel, sur trois
  remarques dont une bijection schéma ↔ table. Rien n'a tranché. Traiter un verdict de conformité comme
  une **mesure** plutôt que comme un **avis** est donc infondé, et `tickets.md` §6.3 — qui déclare la
  conformité délégable — dit une chose vraie dont il tire une conséquence trop forte ;
- **sa boucle n'a pas été éprouvée.** Un seul tour. La convergence sur deux ou trois tours, la dérive du
  litige, l'escalade par assignation : rien de tout cela n'a tourné. Le verdict porte sur **la première
  passe** du skill, pas sur la boucle qui lui donne son nom.

⚠️ **Et un doute que cette fiche ne peut pas lever** : le tour a été précédé, quinze minutes plus tôt, de
la réécriture de ses propres axes. Le skill éprouvé est **neuf du jour**, et rien ne dit ce qu'aurait
rendu la version d'avant — celle qui ne citait sa DoD nulle part.

### `plan-design` — **promu, corrigé par le journal sur deux points**

Premier tour réel lui aussi, et **c'est l'artefact qui rend le verdict** : le plan a traversé quatre
relectures indépendantes sans qu'un seul de ses faits de code soit démenti, et ses clauses de forme —
schéma-delta recopié au caractère près, bijection avec la table, absence de test list, emplacement —
passent. Un plan qui survit à ça au premier essai n'est pas un draft.

**Les deux corrections, l'une et l'autre déjà portées** :

- **il a poussé sa carte** en `Plan Review` au lieu de s'arrêter sur `Done` (journal 68). La règle
  n'avait alors que sa moitié amont ; `D-069` a écrit l'aval le soir même, et `plan-design` §6 porte
  désormais l'interdit ;
- **sa §4 ne disait pas où vivent les pièges**, ce qui a fabriqué une violation dure chez un plan
  conforme. Elle porte maintenant le critère de choix entre cellule et section, en contrepartie de
  l'amendement de la DoD.

⚠️ **Ce que ce verdict ne couvre pas** : le plan relu n'est pas celui que le skill a produit. Sa §4
d'alors — *« Découper en pas »* — a été supprimée par `D-070` le soir même, et les six pas retirés du
document. **Le geste central du skill au moment où il a tourné n'existe plus**, et aucune des quatre
relectures ne l'a jugé. Le prochain plan sera le premier tour réel de `plan-design` **dans sa forme
actuelle**.

### `correction` — **pas de verdict : il n'existe pas. Ce que la boucle lui a écrit, en revanche, est solide**

Trois clauses lui sont acquises, chacune payée par une erreur ou une quasi-erreur observée :

- **lire les douze remarques avant d'en reprendre une.** Reprendre un fil en a rendu un deuxième sans
  objet et **déplacé** un troisième ; répondre dans l'ordre de la liste aurait produit trois réponses
  incohérentes entre elles, chacune juste isolément (journal 80). C'est le symétrique exact de l'axe
  d'ensemble en revue ;
- **avant de choisir entre les branches qu'une remarque propose, chercher ce qui rend le choix inutile.**
  Deux remarques se sont soldées en faisant **disparaître** l'écart plutôt qu'en le documentant
  (journal 79) ;
- **vérifier le compteur `open` après un lot, jamais la sortie du script** : un identifiant tronqué a
  échoué **sans** interrompre le lot, la CLI sortant en 0 (journal 81).

Une quatrième clause tombe de la friction 83 et vise la **forme** des réponses : une reprise ne peut se
fonder que sur des artefacts que l'étape suivante peut ouvrir — le plan, le code, un référentiel, une
entrée `decisions.md`. Le journal des frictions et les fiches de ce dossier **n'en sont pas**.

### `verification` — **pas de verdict non plus, mais son critère de sortie est tué par un fait**

`cycle-increment.md` §5 lui donne pour critère « `Done` si `open` vaut 0 ». **Ce critère était déjà rempli
avant que la vérification commence** : `cursus linear comment resolve` est le seul verbe qui écrit dans un
fil, la correction a donc soldé les douze **en y répondant**, et un vérificateur qui n'aurait rien lu
aurait rendu le même état. **C'est exactement la quatrième issue que ce dossier a nommée** — une mesure
qui invalide un geste central **avant tout usage** —, à ceci près qu'elle frappe une clause de référentiel
plutôt qu'un skill, puisque le skill n'existe pas encore.

Deux issues, et il faut en choisir une **avant** d'écrire le skill (journal 82) : donner à la CLI un verbe
qui **répond sans solder** — et alors c'est la vérification qui solde, ce qui rétablit l'accord entre le
geste et la mesure —, ou renoncer au compteur comme critère et exiger une trace écrite par fil. **La
première est meilleure** : elle rétablit l'accord au lieu d'ajouter une cérémonie.

Deux autres manques lui sont écrits :

- **une troisième façon de solder existe, et aucun référentiel ne la prévoit** — amender le référentiel
  que la remarque invoquait. `D-067` en nomme deux, la reprise et le refus motivé ; celle-ci n'est ni
  l'une ni l'autre. Le bon critère, trouvé sur pièce : **vérifier l'amendement, pas la remarque**
  (journal 84) ;
- **l'appariement remarque ⇄ réponse est manuel** — 37 Ko de JSON à plat, sans regroupement ni tri, les
  douze couples reconstitués à la main par `parentId`, au prix d'un aller-retour de plus. Le format de
  sortie d'un outil de revue est une décision de méthode : ce qu'il rend difficile à lire, l'étape
  suivante le lira mal (journal 86).
