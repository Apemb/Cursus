# 2026-07-30 — `revue-discovery`, premier tour du cycle de revue

> **Le premier tour du dispositif de `D-047`, et la première exécution d'un skill de ce dépôt.**
> Rubriques fixes : voir [`README.md`](README.md).
>
> ⚠️ **La reprise décrite en §6 a été jugée insuffisante par le tour suivant** — elle avait nivelé
> le volume des pistes, pas leur direction. Lire cette fiche avec
> [le tour 2](2026-07-30-revue-discovery-tour-2.md), qui rouvre six des sept points soldés ici.

## 1. Ce qui a tourné

| | |
|---|---|
| Skill | `revue-discovery`, **écrit le jour même** |
| Artefact | document `Discovery` de la feature *Un agent pilote Cursus* (projet Linear) |
| Chemin d'exécution | `claude -p` **headless en sous-process**, hors de la session qui avait écrit l'artefact |
| Outils autorisés | `Read`, `Grep`, `Glob`, `Bash`, `mcp__claude_ai_Linear__*` — **ni `Write` ni `Edit`**, pour que l'interdiction de réécrire l'artefact soit tenue par le harnais et non par l'obéissance |

**La commande, verbatim et rejouable** — lancée depuis la racine du dépôt, le `cd` comptant autant
que le reste : c'est lui qui met les skills du projet et le `PATH` de `mise` à portée.

```bash
claude -p "Fais la revue de la Discovery de la feature « Un agent pilote Cursus ». \
C'est un projet Linear de l'équipe CUR, espace cursus-app." \
  --output-format stream-json --verbose \
  --allowedTools Read Grep Glob Bash "mcp__claude_ai_Linear__*" \
  > tour-1.jsonl 2>&1
```

Trois choses s'y jouent, et aucune n'est cosmétique :

- **Le skill n'est pas nommé.** Le brief est ce qu'un agent recevrait de Cursus — la carte, rien de
  plus — sans la conversation qui précède. C'est donc la **description du skill** qui doit le faire
  se déclencher, et c'est elle qu'on met à l'épreuve autant que son corps.
- **`Write` et `Edit` sont absents de l'allowlist**, délibérément : `revue` §6 interdit de réécrire
  l'artefact, et une interdiction tenue par le harnais vaut mieux qu'une interdiction tenue par
  l'obéissance. Le refus, s'il avait lieu, apparaîtrait dans `permission_denials`.
- **`--output-format stream-json --verbose`** est ce qui rend la rubrique 3 vérifiable : sans le
  flux, « il a respecté le protocole » ne serait qu'une affirmation de l'agent sur lui-même.

⚠️ Le fichier de sortie est écrit **hors du dépôt** (répertoire temporaire de session). Ne jamais
recopier son chemin réel ici : il porte le nom d'utilisateur, et ce dépôt est public.

**Trace qu'il a servi** : un appel `Skill` dans le flux, et quatre occurrences de `revue-discovery`.
Le skill s'est donc déclenché **sur sa seule description**. ⚠️ À retenir pour les prochains tours :
`--bare` **n'est pas** le défaut de `claude -p` en 2.1.220 — la découverte de `CLAUDE.md`, des
skills, des hooks et du MCP a bien lieu. La crainte inverse, inscrite avant mesure, ne se vérifie
pas sur cette version.

## 2. Chiffres

| Mesure | Valeur |
|---|---|
| Durée | **8 min 06 s** |
| Tours | 21 |
| Coût | **2,99 $** |
| Tokens de sortie | 30 452 |
| Sous-agents | 3, en parallèle (un par axe) |
| Remarques produites | 7 — 4 violations dures, 3 jugements |
| Permissions refusées | 0 |
| Erreurs d'outil | 0 |
| Commandes échouées | 1, contournée seule |

## 3. Conformité au protocole

| Clause | Verdict | Ce qui l'atteste |
|---|---|---|
| `revue-discovery` §1 — session neuve | ✅ | process séparé, aucun accès au fil d'écriture |
| §2 — **trois** axes, jamais fondus | ✅ | trois rapports distincts ; Complétude **en accord**, les deux autres en désaccord |
| `revue` §2 — un sous-agent par axe, en parallèle, aveugles l'un à l'autre | ✅ | les trois `Agent` sont trois `tool_use` d'un **même** message, exécutions entrelacées ; chaque prompt porte « tu ne verras pas les autres axes » |
| `revue` §3 — les deux citations | ✅ | chaque remarque porte sa clause de DoD **et** l'extrait visé |
| `revue` §5 — étiqueter la confiance | ✅ | 4 « violation dure », 3 « jugement », aucune ambiguïté |
| `revue` §6 — ne rien réécrire | ✅ | aucune tentative de `Write`/`Edit` (et le harnais l'interdisait) |
| §5 — remarques **sur la carte**, par la CLI | ✅ | 7 commentaires sur le *projet*, via `cursus linear comment add`, repère calculé ; **aucun** sur le document |
| §6 — poser l'étiquette, jamais déplacer | ✅ | `labels: ["Rework Needed"]`, `status: Discovery` inchangé |

**Aucune clause enfreinte.** C'est le résultat le plus inattendu du tour : le skill a été écrit le
matin, sans jamais avoir tourné.

## 4. Qualité de la sortie

Jugée par l'auteur de l'artefact (le binôme) et par l'humain, contre la DoD.

**Les sept remarques sont justes.** Trois des quatre violations dures visent du texte écrit le jour
même par le binôme, et l'une d'elles **avait été explicitement défendue comme conforme** devant
l'humain, quelques minutes plus tôt — le « donc » qui transforme un fait en conséquence
(journal 21). Le binôme avait écrit lui-même, dans le skill, que ce débordement passe « déguisé en
constat », et l'a commis en le sachant.

Le jugement le plus utile ne vise aucune phrase :

> *« Un lecteur qui n'a pas eu la conversation sort de la section 3 avec un ordre de préférence
> qu'aucune phrase n'a énoncé. »*

Il nomme un arbitrage produit par l'**inégalité d'instruction** entre pistes — une piste sondée
reçoit trois éléments porteurs, une piste non sondée n'est décrite que par ce qui n'existe pas.
Aucune clause de la DoD ne couvre ce cas ; c'est un jugement au sens strict, et il est meilleur que
les violations dures.

**Ce qui n'a pas été jugé** : personne n'a vérifié que le relecteur n'a rien *manqué*. Une revue
qui ne trouve rien et une revue qui ne cherche pas se ressemblent, et ce tour ne les départage pas.

## 5. Frictions

Renvoi au [journal](../journal-frictions.md), entrées **17 à 27**. Les quatre qui touchent
l'outillage : le `slugId` non résolu par la CLI (22), l'absence de geste d'étiquette côté CLI (23),
l'impossibilité de citer un passage pour une remarque de **manque** (24), et la signature des
remarques par le porteur de la clé (25).

La plus instructive n'est pas technique : **la friction 17 est la répétition à l'identique de la
friction 9**, à quatre jours d'écart, sur le même sujet, corrigée par la même personne. Le journal
portait l'avertissement et n'a pas été relu. Rien ne le recharge au bon moment.

## 6. Ce que le tour a changé

- `revue-discovery` : le geste de remarque, hérité mort de `revue-spec`, réinjecté **avant** le tour
  (journal 20) — sans quoi le tour aurait mesuré un skill faux.
- `cycle.md`, `cycle-feature.md`, `cycle-increment.md` : renommage de ⑤, registres corrigés.
- `linear-api.md` §10h : trois murs consignés, dont la règle « provoquer l'erreur plutôt que lire
  une absence ».
- `D-048` écrit.
- Ce dossier, créé pour que le tour suivant ait quelque chose contre quoi se comparer.

**Et l'artefact, dans la foulée.** Les sept remarques ont toutes été soldées le jour même, `open`
retombé à 0, `Review Requested` reposée — un tour ① → ② → ③ → ② bouclé. Trois remarques étaient de
la rédaction pure ; deux ont demandé un arbitrage humain, et les deux dernières allaient par paire
(le désaveu et la phrase dont il était le symptôme), ce que le relecteur avait lui-même établi.

**Ce que la reprise a appris, et que le tour seul ne disait pas.** Les deux arbitrages ont été
tranchés **par le bas** — retirer l'excès plutôt qu'instruire les autres pistes au même niveau —
avec le même motif dans les deux cas : équilibrer par le haut, en Discovery, c'est franchir la même
frontière dans l'autre sens. La matière retirée est allée au brouillon de `Spec`, qui existe
précisément pour ça (journal 16). **Une règle qui n'a pas d'exutoire pousse à détruire ce qu'on
vient de comprendre** ; celle-ci en a un, et il a servi pour la deuxième fois.

## 7. Verdict pour `revue-discovery`

**Promu**, au sens de `D-043` — première des quatre issues. Il a tourné en autonomie complète, sans
enfreindre une clause, et a produit des remarques que son propre auteur n'avait pas vues.

⚠️ **Ce que ce verdict ne dit pas.** Un seul tour, sur un artefact **écrit par celui qui a écrit le
skill** — la coïncidence gonfle probablement la conformité. Et `D-039` demande deux ou trois
passages avant de conclure. Le verdict est donc *promu sous réserve du deuxième tour*, sur une
Discovery que le binôme n'aura pas rédigée.
