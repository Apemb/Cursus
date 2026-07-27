# BMAD et les cadres de méthode agentique — état de l'art au 27 juillet 2026

> **Pourquoi ce fichier.** La question posée était simple : la méthode BMAD, ou l'un de ses
> concurrents, peut-elle nous fournir les prompts qui pilotent le passage d'une étape à l'autre —
> voire remplacer la méthode maison ? Ce document cartographie le champ **sans trancher** :
> il rassemble ce qui est établi, dit franchement ce qui ne l'est pas, et pose en §11 ce que le
> dossier met sur la table. La décision, si elle vient, ira dans `decisions.md`.
>
> **Fiabilité des sources.** Deux passes. Une recherche fan-out (104 agents, 110 affirmations
> extraites, 25 soumises à un vote adversarial à trois voix) a produit **8 affirmations confirmées,
> 17 tuées, et zéro affirmation étiquetée `mesuré`**. Une seconde passe a ensuite été menée
> **directement sur les sources primaires** — `curl` sur les dépôts, lecture des prompts réels —
> parce que la première laissait des trous et parce que son vérificateur s'est révélé faillible dans
> un sens précis (voir l'encadré ci-dessous). Chaque affirmation porte donc sa provenance.
>
> **Ce qui n'a pas pu être couvert.** SuperClaude, claude-flow, wshobson/agents, claude-task-master,
> Tessl et Traycer n'ont **rien produit de vérifié** et ne sont pas traités ici. Le corpus officiel
> `anthropics/skills` et la doctrine Anthropic sur l'écriture de skills sont déjà couverts par
> `docs/reference/skills.md` et ne sont pas redoublés.

---

### Un avertissement sur le dispositif lui-même

Le vote adversarial a **réfuté à 0-3 des affirmations qui se vérifient exactes** sur la source
primaire : la fusion des trois personas BMAD en v6.3.0, le retrait du système de « Roles » d'Agent
OS en 2.1.0, la machine à états en frontmatter de la v6, la position anti-gate d'OpenSpec. Le motif
est identifiable : les vérificateurs n'accédaient pas au fichier et **réfutaient par défaut
d'accès**, pas par contradiction. C'est le comportement demandé (« réfute par défaut si tu doutes »)
poussé jusqu'à l'absurde sur un corpus où la source est un `CHANGELOG` de 132 ko.

Conséquence retenue ici, et transférable à nos propres boucles de revue : **un relecteur qui n'a pas
lu la source ne produit pas un verdict, il produit une abstention** — et les traiter à égalité
fabrique des faux négatifs. Cela rejoint `skills.md` §5.1 : sans oracle, la boucle ne crée pas
d'information.

---

## 1. Le premier résultat : il n'y a presque rien à mesurer

**[documenté]** Aucune évaluation empirique indépendante d'aucun de ces cadres n'a été trouvée.
Aucune affirmation `mesuré` n'a survécu à la vérification. Le seul travail académique repéré —
`arXiv:2606.04967`, une comparaison à six dimensions de six cadres — déclare lui-même ses limites :
méthodologie purement documentaire (aucune exécution, aucune lecture des prompts réels), **notateur
unique sans second codeur**, aucune fiabilité inter-juges rapportée, preprint non relu, et **conflit
d'intérêts déclaré** — l'auteur est celui de Reversa, l'un des six cadres notés. Le papier a le
mérite de refuser d'importer les chiffres de productivité des sources primaires : *« when they lack
independent evaluation, they become a limitation, never a number of our own »*.

À toutes fins utiles, sa grille (six dimensions notées 0–2) : Spec Kit 8/12 · **BMAD 10/12** ·
Spec Kitty 9 · OpenSpec 6 · Reversa 6 · Get Shit Done 4. Aucun cadre n'obtient 2 partout ; la
dimension *spécification* est saturée (presque tous à 2), les dimensions *rôles* et *validation*
sont les plus polarisées. À lire comme un jugement d'auteur sur de la documentation promotionnelle,
**jamais comme une mesure**.

**Le seul chiffre du corpus** est auto-rapporté et minuscule : le `CHANGELOG` BMAD v6.10.0 annonce
pour la passe *named-set generalization* de son Edge Case Hunter *« Measured catch-rate improvement
of 50 % to 100 % on a real regression, at a 19 % token cost per run »* **[mesuré, auto-rapporté,
non reproductible]** — un cas, aucun protocole publié.

---

## 2. Le gating — ce qui autorise réellement le passage d'étape

C'est la question qui motivait la recherche. Réponse courte : **presque jamais un verdict tiers, et
jamais une condition sémantique.**

### 2.1 BMAD V4 — l'auto-évaluation avec injonction d'honnêteté

**[documenté, vérifié byte-exact sur la branche V4]** La `story-dod-checklist` est exécutée par
l'agent qui vient d'écrire le code. En-tête, verbatim :

> *« This checklist is for DEVELOPER AGENTS to self-validate their work before marking a story
> complete »* · *« IMPORTANT: This is a self-assessment. Be honest about what's actually done vs
> what should be done »*

et clôture par auto-certification : *« I, the Developer Agent, confirm that all applicable items
above have been addressed »*. La boucle `develop-story` de `dev.md` exécute la checklist, positionne
le statut à `Ready for Review`, puis s'arrête : **le même agent exécute le contrôle et change
l'état**. L'agent QA (Quinn) produit bien un *gate file* `PASS / CONCERNS / FAIL / WAIVED`, mais
**après** le passage en revue, et `qa.md` lui **interdit de toucher au champ Status**. Le tiers
existe, il ne gate pas.

### 2.2 Spec Kit — le comptage de cases, puis une question

**[documenté, lu dans `templates/commands/implement.md`]** Le mécanisme le plus explicite du corpus.
Avant d'implémenter, la commande scanne `checklists/`, compte les lignes `- [ ]` contre `- [X]`,
affiche une table `PASS / FAIL` par checklist, puis :

> *« **STOP** and ask: "Some checklists are incomplete. Do you want to proceed with implementation
> anyway? (yes/no)" · Wait for user response before continuing · If user says "no" or "wait" or
> "stop", halt execution »*

Trois propriétés à retenir : le contrôle est **machine** (un comptage), le verdict est **rendu
visible** (la table), et le franchissement est **humain et explicite** — mais **non bloquant** : un
`yes` passe outre. S'y ajoute un prérequis dur en amont, `check-prerequisites.sh --require-tasks`,
qui échoue si `tasks.md` n'existe pas.

La commande `/analyze` joue le rôle du tiers : **strictement en lecture seule**, elle croise
`spec.md` / `plan.md` / `tasks.md`, classe les écarts par sévérité, et *« Recommend resolving before
implement »* — elle recommande, elle n'autorise pas. Les conflits avec la constitution sont
**automatiquement CRITICAL** et *« require adjustment of the spec, plan, or tasks — not dilution,
reinterpretation, or silent ignoring of the principle »*.

### 2.3 Kiro (AWS) — l'approbation par phase

**[documenté, doc officielle]** Le flux standard a trois phases — requirements, design, tasks — et
*« You approve each one before the next begins »*. C'est le gating humain le plus franc du corpus.
Il est immédiatement doublé d'une porte de sortie : **Quick Spec** génère les trois artefacts d'un
coup *« without approval gates »*. La doc consultée ne détaille pas le mécanisme d'approbation
lui-même, et n'expose pas d'exemple EARS à cet endroit **[incertain — le lien EARS ↔ Kiro circule
largement mais n'a pas été vérifié sur source primaire]**.

### 2.4 OpenSpec — l'absence de gate comme argument de vente

**[documenté, lu dans le README]** OpenSpec revendique l'inverse exact :

> *« **Work fluidly** — update any artifact anytime, no rigid phase gates »*
> *« **vs. Spec Kit** (GitHub) — Thorough but heavyweight. Rigid phase gates, lots of Markdown,
> Python setup. OpenSpec is lighter and lets you iterate freely. »*

Ce qui tient lieu de contrôle est une relecture humaine informelle : *« Your AI writes these; you
review the plan before any code is written. »* Le champ contient donc, sur la même question, deux
positions frontalement opposées, chacune assumée comme un avantage.

### 2.5 Agent OS v3 — le gate délégué à l'outil hôte

**[documenté, discussion #310 et `CHANGELOG` v3.0]** Agent OS a **retiré** l'écriture de spec, le
découpage et l'orchestration en janvier 2026 :

> *« It doesn't make sense to reinvent these core functions, which are much better handled by the
> core tools than 3rd-party frameworks like Agent OS »* · *« Instead of Agent OS commands handling
> spec writing, we defer to Plan Mode in Claude Code »*

Le point de contrôle n'est plus une checklist maison mais **l'acceptation native du Plan Mode** de
l'outil. Le `CHANGELOG` va jusqu'à qualifier le Plan Mode de *« the industry-standard approach to
spec-driven development in 2026+ »*. Nuance à tenir : le sous-agent `spec-verifier` n'est pas
supprimé, il **sort du flux par défaut** et reste appelable à la main.

### 2.6 Ce qui traverse tout le corpus : les gates vérifient la forme, jamais le fond

C'est l'observation la plus solide de ce dossier, et elle vaut pour tous les cadres examinés.

| Cadre | Ce que le gate vérifie réellement |
|---|---|
| BMAD V4 | Que l'agent déclare avoir coché ses cases |
| BMAD v6 (module TEA) | **Qu'un fichier existe** — jamais son contenu |
| Spec Kit | Que les cases `- [X]` sont cochées — jamais leur véracité |
| Kiro | Qu'un humain a cliqué |
| OpenSpec | Rien, par choix assumé |
| Agent OS v3 | Qu'un humain a accepté un plan |

**[documenté, issue BMAD #2275, ouverte le 2026-04-16 sur v6.3.0]** L'illustration la plus nette :
*« bmad-dev-story: ATDD gate checks file existence only — no Gherkin, no E2E enforcement »* ;
*« even when an ATDD checklist file exists, its content is never scanned for Gherkin scenarios
(Given/When/Then). A checklist file with only prose passes silently »* ; impact rapporté :
*« Projects with the TEA module installed and user-facing flows complete multiple epics with zero
E2E tests … despite the tooling being present and configured »*. Motif complémentaire documenté dans
la même issue : des agents **accusent réception** de l'ordre d'exécuter la checklist, puis
l'ignorent et marquent la story terminée.

Corroboration indépendante dans le `CHANGELOG` v6.3.0 : *« Fix checkpoint-preview step-05 advancing
without user confirmation by adding explicit HALT »* — **un gate humain qui fuyait, rattrapé par un
`HALT` écrit en dur**. C'est exactement le mode de défaillance décrit en `skills.md` §1.3 : le faux
succès.

---

## 3. La passation de contexte — le mécanisme le plus transférable du dossier

### 3.1 Le story file de BMAD V4

**[documenté, vérifié byte-exact — l'élément le plus solide de tout le dossier]** Le mécanisme est
**doublement encodé, côté producteur et côté consommateur**.

Producteur — `story-tmpl.yaml`, section `Dev Notes` (`owner: scrum-master`) :

> *« Put enough information in this section so that the dev agent should **NEVER** need to read the
> architecture documents, these notes along with the tasks and subtasks must give the Dev Agent the
> complete context it needs to comprehend with the least amount of overhead the information to
> complete the story, meeting all AC and completing all tasks+subtasks »*

Consommateur — `dev.md` :

> *« CRITICAL: Story has ALL info you will need aside from what you loaded during the startup
> commands. **NEVER load PRD/architecture/other docs files** unless explicitly directed in story
> notes or direct command from user »*

**Qualification importante** : l'interdiction n'est pas absolue. `core-config.yaml` définit
`devLoadAlwaysFiles` = `coding-standards.md`, `tech-stack.md`, `source-tree.md` — trois fragments
d'architecture chargés d'office. La formulation juste est donc : *l'agent d'implémentation n'a pas à
relire les documents de conception **au-delà de trois fichiers de standards***.

C'est la définition opérationnelle de ce que le marketing V4 appelait *« hyper-detailed story
files »* — le terme n'apparaît pas dans le template, il vient du README. **Aucune mesure
n'accompagne le dispositif** ; les critiques publiques portent sur la lourdeur, pas sur l'existence
du mécanisme.

### 3.2 Ce qu'en fait la v6

**[documenté, `CHANGELOG` v6.3.0 et v6.10.0 — vérifié directement]** Le vocabulaire *story* a
disparu du README courant (zéro occurrence sur 127 lignes), mais le mécanisme, lui, s'est
**durci et automatisé** :

- *« Remove `spec-wip.md` singleton; quick-dev now writes directly to `spec-{slug}.md` with **status
  field**, enabling parallel sessions »* ;
- *« **Epic context compilation** for quick-dev step-01: **sub-agent compiles planning docs into
  cached `epic-{N}-context.md`** for story implementation »* — la compilation du contexte devient
  elle-même une étape outillée ;
- *« Previous story continuity: load completed spec from same epic as implementation context »* ;
- et en v6.10.0, `bmad-dev-auto`, *« driven entirely off **spec-frontmatter status** so an
  orchestrator like `bmad-loop` can poll it »*.

**Lecture** : le gating v6 n'est plus une checklist mais **une machine à états portée par le
frontmatter d'un fichier, pollée par un orchestrateur externe**. C'est un déplacement net vers le
déterministe — et c'est, de tout le corpus, ce qui ressemble le plus à ce que Cursus construit.

### 3.3 OpenSpec — l'aveu le plus net

**[documenté, README]** *« **Context hygiene**: OpenSpec benefits from a clean context window.
**Clear your context before starting implementation** and maintain good context hygiene throughout
your session. »* Autrement dit : l'artefact écrit doit être le **seul** canal entre conception et
implémentation, au point qu'on recommande de détruire l'autre.

Son format d'exigence, au passage, est proche de Gherkin :

```
### Requirement: Theme selection
The app SHALL let users switch between light and dark themes, defaulting to the system preference.

#### Scenario: User toggles dark mode
- **WHEN** the user clicks the theme toggle
- **THEN** the app switches to dark mode and persists the choice
```

### 3.4 La convergence

Quatre familles indépendantes convergent sur un même postulat, et **c'est le signal le plus fort du
dossier** : *ce qui n'est pas écrit dans l'artefact n'existe pas pour l'étape suivante*. BMAD
l'impose par interdiction de lecture, OpenSpec par purge du contexte, Spec Kit par prérequis de
fichier, Kiro par artefacts numérotés. Aucun de ces cadres ne fait le pari inverse — celui du
contexte conversationnel conservé.

---

## 4. Les règles persistantes

**[documenté]** Trois formes du même besoin : des règles qui survivent à toutes les étapes.

- **Spec Kit — la constitution** (`.specify/memory/constitution.md`). Le point remarquable est sa
  **gouvernance** : versionnée en **semver** (`MAJOR` = retrait ou redéfinition incompatible d'un
  principe), avec une **propagation obligatoire** aux templates dépendants — la commande de mise à
  jour doit relire `plan-template.md`, `spec-template.md`, `tasks-template.md` et signaler chacun
  `✅ updated` / `⚠ pending`. Elle exige des principes *« declarative, testable, and free of vague
  language ("should" → replace with MUST/SHOULD rationale) »*, et s'interdit de toucher au code
  applicatif.
- **Kiro — les steering files** : règles de projet injectées dans toutes les phases **[documenté,
  mais mécanisme non vérifié sur source primaire]**.
- **Agent OS — la couche `standards`** : c'est ce qui **reste** après la coupe de v3. Le README
  courant définit désormais le projet comme *« a system for injecting your codebase standards and
  writing better specs »*.

**Observation** : c'est la seule couche qu'aucun des projets examinés n'a jamais retirée. Tout le
reste a été taillé ; les règles persistantes, non.

---

## 5. Les rôles — le seul point où la recherche mesure, et le résultat est négatif

### 5.1 Deux mesures, deux résultats négatifs

**[mesuré]** *« When "A Helpful Assistant" Is Not Really Helpful: Personas in System Prompts Do Not
Improve Performances of Large Language Models »* — Zheng, Pei, Logeswaran, Lee, Jurgens, **Findings
of EMNLP 2024**. Protocole : **162 rôles** (6 types de relations, 8 domaines d'expertise), **4
familles de modèles**, **2 410 questions factuelles**. Résultat : *« adding personas in system
prompts does not improve model performance … compared to the control setting where no persona is
added »*. Certains attributs influencent le résultat, mais l'effet est faible et largement
aléatoire ; identifier automatiquement la meilleure persona ne fait **pas mieux qu'un tirage au
sort**.

**[mesuré]** *« Prompting Science Report 4: Playing Pretend: Expert Personas Don't Improve Factual
Accuracy »* — Wharton, 5 décembre 2025. **6 modèles**, benchmarks **GPQA Diamond** et **MMLU-Pro**,
trois conditions : persona experte assortie, persona experte non assortie, persona de faible
connaissance. Résultat : *« persona prompts generally did not improve accuracy »* ; les personas mal
assorties **dégradent** parfois ; les personas de faible connaissance dégradent généralement. Les
auteurs concèdent une utilité **de ton**, pas de justesse.

**[mesuré]** *« Why Do Multi-Agent LLM Systems Fail? »* — Cemri et al. (UC Berkeley, Meta), mars
2025. **1 600+ traces annotées** sur 7 frameworks, 150 traces à double annotation humaine,
**κ = 0,88**. Taxonomie MAST : 14 modes de défaillance en 3 familles — *system design issues*,
*inter-agent misalignment*, *task verification*. Conclusion des auteurs : les gains des systèmes
multi-agents sont minimes et *« identified failures require more sophisticated solutions »*.

### 5.2 Les abandons concordent

**[documenté, vérifié directement dans les `CHANGELOG`]** Les praticiens ont conclu dans le même
sens que les mesures, sans les citer :

- **Agent OS 2.1.0** (21 octobre 2025) : *« Retired the short-lived "roles" system. **Too complex,
  and better handled with standard tooling** »* ; et plus loin : *« That system **added no real
  benefit** over simply using available tooling (like Claude Code's own subagent generator) »*.
- **BMAD v6.3.0** (9 avril 2026) : *« **Consolidate three agent personas into Developer agent
  (Amelia)**: remove Barry quick-flow-solo-dev, Quinn QA agent, and **Bob Scrum Master agent** »* —
  le Scrum Master, c'est-à-dire précisément le rôle qui portait la production des story files.

### 5.3 La limite de ces mesures — à ne pas escamoter

Les deux études sur les personas mesurent la **justesse factuelle sur des questions à réponse
courte**. Elles ne mesurent ni la performance sur du code, ni les tâches longues, ni la
**segmentation du contexte** — qui est peut-être le vrai service rendu par un rôle : non pas rendre
le modèle meilleur, mais **restreindre ce qu'il a le droit de faire et de lire** à cette étape. Le
`dev.md` de BMAD est moins « tu es un développeur » que « tu ne chargeras pas ces fichiers ».
Transposer directement le résultat négatif à cette fonction-là serait un abus.

Une nuance de vocabulaire à tenir également : le retrait de la skill `bmad-investigate` (§6) est un
résultat négatif sur **une skill outillée surajoutée**, pas sur le role prompting.

---

## 6. Le mouvement du champ : ce que les projets ont retiré

**[documenté]** Le matériau le plus informatif du dossier n'est pas ce que ces cadres revendiquent,
mais ce qu'ils ont **enlevé après usage**. Une source contre-promotionnelle est peu suspecte de
marketing — sans être pour autant une mesure : ce sont des auto-déclarations sans protocole.

| Date | Projet | Retiré | Motif, verbatim |
|---|---|---|---|
| 2025-10-21 | Agent OS 2.1.0 | Le système de « Roles » | *« Too complex … added no real benefit over standard tooling »* |
| 2025-10-21 | Agent OS 2.1.0 | *« documentation & verification bloat »* | *« quickly proved unnecessary and inefficient »* |
| **2025-10-29** | **ai-dev-tasks** | **`process-task-list.md`** | *« Since step 3 was removed and **agents no longer need hand-holding**, this file is no longer necessary »* |
| 2026-01-20 | Agent OS 3.0 | Spec, découpage, orchestration | *« Today's frontier models handle spec implementation well on their own »* |
| 2026-04-09 | BMAD 6.3.0 | Scrum Master, QA, solo-dev | Fondus dans l'agent Developer |
| 2026-07-03 | BMAD 6.10.0 | La skill `bmad-investigate` | *« It reached the same conclusions as plain investigation at higher cost; **the case-file artifact didn't justify the overhead** »* |
| 2026-07-03 | BMAD 6.10.0 | L'auditeur de contrats de suppression | *« added cold-start cost for near-zero yield »* |

Deux lignes méritent d'être lues deux fois.

**`process-task-list.md`** était le fichier le plus cité de la famille légère : celui qui imposait
*une sous-tâche à la fois, l'humain valide entre chaque*. Il a été **supprimé** parce que
*« les agents n'ont plus besoin qu'on leur tienne la main »*. Le gate humain fin, celui qu'on cite
comme modèle de discipline, a été jugé obsolète par son propre auteur en octobre 2025.

**`bmad-investigate`** : cycle vie → mort en **sept semaines** (introduite en v6.7.0 le 17 mai 2026,
retirée le 3 juillet). Le motif est double, et la moitié la plus intéressante est souvent omise : le
coût jugé excessif n'est pas seulement celui du raisonnement, c'est celui du **document produit**.

**Contrepoint honnête**, pour ne pas faire dire au tableau une thèse qu'il ne soutient pas : le
mouvement n'est pas monotone. Agent OS a retiré les Roles en 2.1.0 **en les remplaçant** par une
phase `orchestrate-tasks`… supprimée à son tour en 3.0. Et BMAD, en fondant ses personas, a
simultanément **ajouté** de la machinerie ailleurs : `bmad-loop`, `bmad-dev-auto`, un *Blind Hunter*,
un *Edge Case Hunter*, un *anti-consensus club*. Ce qui est retiré, ce sont les **rôles** et les
**checklists déclaratives** ; ce qui est ajouté, ce sont des **vérificateurs outillés et des
machines à états**. C'est un déplacement, pas un dégonflement.

---

## 7. Le découpage en niveaux

**[documenté]** Aucun invariant. BMAD V4 : PRD → epics → stories → tasks/subtasks, avec un rôle
dédié au découpage. Spec Kit : spec → plan → tasks, sans niveau intermédiaire nommé. Kiro :
requirements → design → tasks. OpenSpec : un *change* porte proposal + specs + design + tasks.
Agent OS v3 : plus de découpage du tout, délégué au modèle.

**[documenté]** `arXiv:2606.04967` relève que *rôles* et *validation* sont les dimensions les plus
polarisées entre cadres, là où *spécification* est saturée. Autrement dit : **tout le monde
s'accorde sur l'existence d'un artefact écrit ; personne ne s'accorde sur qui l'écrit ni sur qui le
valide.**

---

## 8. Le packaging

**[documenté]** Tous ces cadres sont, techniquement, **du markdown plus une convention de
répertoire**, installés par un CLI et exposés en slash commands. Spec Kit : `templates/commands/*.md`
(10 commandes, de 7 à 21 ko chacune) plus des scripts `bash`/`ps`/`py` de prérequis, plus un système
de **hooks** déclarés dans `.specify/extensions.yml` (`optional: true|false`, avec un protocole
`EXECUTE_COMMAND:` et l'avertissement — révélateur — que *« Emitting the block alone does not run the
hook »*). BMAD v6 est passé à une *« Skills Architecture »* avec sous-agents natifs et se distribue
via un **marketplace** (`.claude-plugin/marketplace.json`), 34+ workflows dans le module cœur.
Kiro est le seul à exiger **son propre IDE**.

Point de coût **[documenté]** : BMAD recommande explicitement de faire la planification dans un
abonnement web (Gems, GPTs) *« instead of metered IDE tokens, which is a meaningful cost saver on
longer engagements »* — un aveu direct que la phase amont est chère.

---

## 9. Ce qui converge, ce qui diverge

**Converge (signal fort, familles indépendantes)**

1. **L'artefact écrit est le seul canal entre étapes.** §3.4.
2. **Une couche de règles persistantes**, jamais retirée par personne. §4.
3. **Le contrôle porte sur la forme de l'artefact**, jamais sur son contenu. §2.6.
4. **La direction du mouvement en 2025-2026** : moins de rôles déclaratifs, plus de vérificateurs
   outillés et de machines à états. §6.

**Diverge frontalement**

1. **Le gating** : Kiro approuve chaque phase ; OpenSpec en fait une anti-fonctionnalité ; Spec Kit
   demande sans bloquer ; Agent OS délègue à l'hôte ; BMAD V4 s'auto-évalue.
2. **Le casting de rôles** : central chez BMAD V4, supprimé chez Agent OS, inexistant chez Spec Kit.
3. **Le découpage** : de quatre niveaux à zéro.

---

## 10. Ce que ce dossier ne dit pas

- **L'origine des méthodes reste non établie.** La question — récoltées par observation des
  frictions, ou transposées d'en haut depuis Agile ? — n'a **pas** été tranchée : les témoignages de
  genèse de l'auteur de BMAD (podcast Tech Lead Journal ép. 255 : *vibe coding* nocturne sur Cursor,
  fichiers de contraintes ajoutés en réaction aux frictions, six fichiers d'origine ; et
  symétriquement, quinze ans de pratique Agile transposés en rôles) **n'ont pas passé la
  vérification** et sont écartés. L'hypothèse d'une **origine mixte** — mécanique de base récoltée,
  structure de rôles transposée — reste plausible et non prouvée. Elle se trancherait en datant les
  premiers fichiers dans l'historique git, pas en écoutant une interview.
- **Le mécanisme de gating de BMAD v6 n'est décrit ici que par son `CHANGELOG`**, pas par lecture
  des workflows du module BMM sur la version courante.
- **Aucun retour d'expérience indépendant et circonstancié sur le brownfield** ni sur le coût réel en
  tokens. Les issues #446 et #2275 sont des indices, pas une évaluation.
- **Aucune mesure sur l'effet des rôles dans une tâche de code longue** — seulement sur la justesse
  factuelle en QA. §5.3.
- **Rien de vérifié** sur SuperClaude, claude-flow, wshobson/agents, claude-task-master, Tessl,
  Traycer.

---

## 11. Ce que ça met sur la table pour Cursus

Sans trancher — ce sont les questions que le dossier rend légitimes, pas des recommandations.

**Ce qui semble prenable tel quel.**

- Le **double encodage de la passation** (§3.1) : notre plan d'archi et notre test list disent déjà
  *quoi*, mais aucun de nos documents ne dit à l'exécutant **ce qu'il n'a pas le droit d'aller
  relire**. L'interdiction côté consommateur est la moitié du mécanisme, et c'est celle qui nous
  manque.
- La **métaphore « unit tests for English »** de Spec Kit : une checklist valide *la qualité des
  exigences*, pas l'implémentation. Nos DoD par niveau × statut (`D-041`) sont à ce croisement — la
  distinction mérite d'être posée explicitement dans `docs/methode/dod/`.
- La **gouvernance de la constitution** : semver sur les principes, et **propagation obligatoire aux
  documents dépendants avec un état `✅`/`⚠` par fichier**. C'est une version outillée de notre règle
  « le document d'architecture évolue dans le même commit ».
- La **table de statut rendue visible avant de franchir** (§2.2) : bon marché, et c'est ce qui
  transforme un gate silencieux en gate observable.

**Ce que le dossier met en tension avec nos décisions.**

- **`D-038` — méthode → skill.** Trois projets ont retiré la couche « méthode déclarative » en neuf
  mois, dont le gate humain fin d'ai-dev-tasks au motif que *« les agents n'ont plus besoin qu'on
  leur tienne la main »*. Ce qui a été retiré, ce sont les **checklists et les rôles** ; ce qui a été
  ajouté, ce sont des **vérificateurs outillés**. Notre découpage en trois niveaux relève-t-il de la
  première catégorie ou de la seconde ?
- **Le rôle produit comme juge** (`D-036`). Les deux mesures de §5.1 sont négatives sur les personas.
  Elles ne portent pas sur notre usage — nous nommons un **point de vue de validation**, pas un
  costume — mais l'écart mérite d'être écrit avant qu'on nous l'oppose.
- **Le tiers qui prononce la conformité** (`D-041`). Aucun cadre du corpus n'a un tiers *bloquant* :
  Spec Kit recommande, BMAD auto-évalue, Kiro délègue à l'humain. Nous serions donc plus stricts que
  tout ce qui existe — soit c'est notre avantage, soit c'est ce que les autres ont déjà essayé et
  retiré. Le dossier ne permet pas de dire lequel.
- **`D-039` — « un skill se récolte avant de s'écrire ».** Le dossier ne fournit **aucune preuve
  qu'un seul de ces prompts soit “éprouvé”** au sens mesuré. Ce qu'il fournit, ce sont des traces
  d'usage et, plus solidement, des **traces de rétractation**. L'argument « ces prompts sont déjà
  éprouvés, autant les reprendre » n'est donc pas soutenu par les faits — mais l'argument inverse ne
  l'est pas davantage : personne n'a montré qu'une méthode récoltée bat une méthode transposée.

**Ce qui ressemble le plus à ce que nous construisons.** La v6 de BMAD — machine à états portée par
le frontmatter d'un fichier, pollée par un orchestrateur externe (§3.2) — est structurellement
proche du noyau déterministe de Cursus, et l'a atteinte **après** avoir démarré par les rôles. C'est
la trajectoire inverse de la nôtre, arrivée au même endroit.

---

## Sources

**Sources primaires lues directement (2026-07-27)**

- BMAD-METHOD, branche V4 — `bmad-core/templates/story-tmpl.yaml`, `bmad-core/agents/dev.md`,
  `bmad-core/agents/qa.md`, `bmad-core/checklists/story-dod-checklist.md`, `core-config.yaml`
- BMAD-METHOD, `main` — `CHANGELOG.md` (132 ko, v6.0.0-Beta.1 → v6.10.0), `README.md`
- BMAD-METHOD, issue #2275 (gate ATDD), issue #446 (brownfield)
- GitHub Spec Kit — `templates/commands/` : `implement.md`, `analyze.md`, `checklist.md`,
  `constitution.md`, `plan.md`, `specify.md`, `tasks.md`, `converge.md`
- OpenSpec (Fission-AI) — `README.md`
- Agent OS (buildermethods) — `CHANGELOG.md` (1.0.0 → 3.0), `README.md`, discussion #310
- ai-dev-tasks (snarktank) — `README.md`, `create-prd.md`, `generate-tasks.md`, et l'historique de
  `process-task-list.md` (supprimé le 2025-10-29)
- Kiro (AWS) — `kiro.dev/docs/specs/`

**Recherche**

- Zheng, Pei, Logeswaran, Lee, Jurgens — *When "A Helpful Assistant" Is Not Really Helpful*,
  Findings of EMNLP 2024 — `arXiv:2311.10054`
- Wharton — *Prompting Science Report 4: Playing Pretend*, 2025-12-05 — `arXiv:2512.05858`
- Cemri et al. — *Why Do Multi-Agent LLM Systems Fail?* (MAST), 2025-03 — `arXiv:2503.13657`
- *A comparative study of spec-driven development frameworks*, preprint non relu, notateur unique,
  conflit d'intérêts déclaré — `arXiv:2606.04967`

**Secondaires** — nearform.com (entretien avec l'auteur de BMAD), techleadjournal.dev ép. 255,
lennysnewsletter.com (flux en 3 étapes), Hacker News #45935763, Medium (Bogutzky, coût réel de
BMAD), deepwiki.com (v6).
