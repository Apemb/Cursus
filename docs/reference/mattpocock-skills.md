# `mattpocock/skills` — un corpus de skills réellement chargés, et son artisanat

> **Pourquoi ce fichier.** Les trois dossiers précédents (`bmad.md`, `symphony.md`, `task-master.md`)
> décrivent des cadres qui *décrivent* une méthode. Celui-ci est le premier qui en **exécute** une :
> 41 skills Claude Code, livrés en plugin, avec un cimetière daté et un dossier de périmètre refusé.
> Les questions posées : comment y conçoit-on une spec en binôme et en solo, comment découpe-t-on en
> sous-tâches, comment relit-on du travail fait, et comment accumule-t-on de la connaissance métier —
> plus deux sujets que le dépôt impose de lui-même : **l'artisanat d'écriture d'un skill** et
> **l'archéologie de ses retraits**.
>
> **Fiabilité des sources.** Passe unique sur la **source primaire** : clone **complet** (314 commits,
> `git log`/`git show`/`git diff` pleinement exploitables), fichiers lus sur disque, prompts cités
> verbatim, tailles et dates comptées. Six lectures indépendantes à périmètres disjoints. Registres :
> **(prompt)** = écrit dans un `SKILL.md` ou une annexe · **(doc)** = affirmé dans `docs/` ou un README
> · **(histoire)** = établi par une commande git · **(absent)** = cherché, inexistant.
>
> **Limites.** `HEAD` = `ed37663`, **2026-07-21**, six jours avant cette lecture. Un seul auteur
> (Matt Pocock, 303 des 314 commits) : c'est l'artisanat **d'une personne**, cohérent mais non
> collectif, et **aucune mesure d'efficacité n'existe** ici non plus. Deux rapprochements de ce
> document sont des **inférences signalées comme telles** (le successeur de `qa` et de
> `request-refactor-plan`), pas des faits git.
>
> **Aucun `D-NNN` n'est écrit ici.** Cartographie, comme les précédents. La décision, si elle vient,
> ira dans `decisions.md`.

---

## Le résultat en une page

Ce corpus est le premier du dossier où **la méthode est effectivement chargée** : les skills sont
distribués en plugin, `deprecated/` est exclu de l'installation par script, et un artefact de
connaissance n'existe que s'il est relu par d'autres skills. Là où Task Master promet en prose,
celui-ci câble — imparfaitement, par de la prose elle aussi, mais **la prose est dans le fichier que
l'agent charge**, pas dans un document qu'il n'ouvrira jamais.

| Question | La réponse du corpus | Registre |
|---|---|---|
| Lever l'ambiguïté | `grilling` — un **primitif** d'entretien, réutilisé par tous les skills qui en ont besoin | (prompt) |
| Qui répond à quoi | Les **faits**, l'agent va les chercher · les **décisions** sont à l'humain | (prompt) |
| Rythme des questions | **Une à la fois**, avec une réponse recommandée. Pas de plafond — refusé explicitement | (prompt) |
| Écrire la spec | `to-spec` — et il **n'interroge pas** : l'alignement a eu lieu avant | (prompt) |
| Découper | Tranches **verticales**, démontrables seules, **taille = une fenêtre de contexte fraîche** | (prompt) |
| Ordonner | Chaque ticket porte ses **blocking edges** ; on travaille la *frontier* | (prompt) |
| Gate humain | Oui, sur la **granularité**, avant publication — et sur l'alignement, avant la spec | (prompt) |
| Briefer un agent | `AGENT-BRIEF` — **durabilité plutôt que précision**, comportemental jamais procédural | (prompt) |
| Relire | **Deux axes en parallèle**, jamais fusionnés : *Standards* et *Spec* | (prompt) |
| Éviter le faux positif | **Citation obligatoire** de la règle et du hunk — pas de vérification empirique | (prompt) |
| Verdict de revue | Aucun. Une liste, avec *hard violation* vs *judgement call* | (prompt) |
| Accumuler le métier | `CONTEXT.md` (glossaire) + ADR **d'un paragraphe**, écrits paresseusement | (prompt) |
| Le chemin de retour | **Au moins quatre skills rechargent `CONTEXT.md`** avant d'explorer le code | (prompt) |
| Ce qui tue un dispositif | Un artefact que **personne ne recharge** — motif reconstruit de la mort d'`ubiquitous-language` | (histoire) |

---

## 1. Concevoir une spec : deux gestes séparés, jamais fondus

### 1.1 `grilling` — l'entretien comme primitif partagé

**(prompt)** Le geste central du corpus n'est pas d'écrire, c'est d'**interroger l'humain**.
`skills/productivity/grilling/SKILL.md:6-12`, verbatim :

> *"Interview me relentlessly about every aspect of this until we reach a shared understanding. Walk
> down each branch of the decision tree, resolving dependencies between decisions one-by-one. For each
> question, provide your recommended answer."*
> *"Ask the questions one at a time, waiting for feedback on each question before continuing. Asking
> multiple questions at once is bewildering."*
> *"If a **fact** can be found by exploring the environment (filesystem, tools, etc.), look it up rather
> than asking me. The **decisions**, though, are mine — put each one to me and wait for my answer."*
> *"Do not act on it until I confirm we have reached a shared understanding."*

Quatre propriétés, chacune un choix :

1. **Le partage fait / décision.** L'agent n'a pas le droit de demander à l'humain ce qu'il peut aller
   lire ; l'humain n'a pas le droit d'être court-circuité sur ce qui s'arbitre. C'est la ligne la plus
   transférable du dossier.
2. **Une question à la fois**, avec une **réponse recommandée** — donc l'humain arbitre plutôt qu'il ne
   rédige.
3. **Un gate de fin explicite** — l'agent ne passe pas à l'acte sans confirmation. **(histoire)** Il
   n'était pas là au départ : ajouté par *"grilling: add confirmation gate and grill leading word"*.
4. **Un primitif, pas une pratique.** **(doc)** `docs/productivity/grilling.md:31-33` : *"`grilling` is
   the **single source of truth** for the interview technique, split out as a model-invoked
   **primitive** so every skill that needs an interview can reach it instead of reinventing one."*
   `grill-me` (7 lignes) et `grill-with-docs` (7 lignes) ne sont que deux portes d'entrée dessus — la
   seconde *stateful* (elle écrit `CONTEXT.md` et des ADR), la première non.

**(histoire)** Un bug réel a façonné la formulation : *"wayfinder/grilling: stop the agent grilling
itself"*. Des utilisateurs rapportaient que l'agent, sur un ticket de grilling, **s'interrogeait
lui-même** au lieu de se tourner vers l'humain. Cause identifiée : une tournure négative
(*"explore the codebase instead"*) lue comme licence à répondre à la place de l'humain. Corrigée en
affirmatif. J'y reviens en §5.4 — c'est le point où ce corpus et notre `skills.md` se contredisent.

### 1.2 `to-spec` — l'anti-entretien

**(prompt)** La première ligne de `skills/engineering/to-spec/SKILL.md:7` est la négation exacte du
précédent :

> *"Do NOT interview the user — just synthesize what you already know."*

**(doc)** `docs/engineering/to-spec.md:17` : *"It does **not** interview you again. By the time you
reach for it, the alignment work is done."* Trois étapes : explorer le dépôt (vocabulaire du glossaire,
ADR existants) → **esquisser les seams de test** *("the ideal number is one")* et **s'arrêter** —
*"Check with the user that these seams match their expectations"*, seul gate du skill → écrire la spec
et la publier sur le tracker avec l'étiquette `ready-for-agent`.

Le gabarit (`to-spec/SKILL.md:21-75`) : `Problem Statement`, `Solution`, `User Stories` (numérotées,
*"As an <actor>, I want a <feature>, so that <benefit>"*, exigées *"extremely extensive"*),
`Implementation Decisions`, `Testing Decisions`, `Out of Scope`, `Further Notes`. Avec une interdiction
qui vaut règle générale du corpus :

> *"Do NOT include specific file paths or code snippets. They may end up being outdated very quickly."*

> **Le point de méthode** : l'alignement et la rédaction sont **deux skills**, et le second commence
> par interdire le premier. Fondre les deux, c'est laisser l'agent arbitrer en rédigeant — exactement
> ce que notre `D-041` interdit entre Discovery et Spec, obtenu ici par une seule phrase en tête de
> fichier.

### 1.3 Binôme ou solo : la taxonomie HITL / AFK

**(prompt)** La distinction est nommée. `skills/engineering/wayfinder/SKILL.md:75` classe chaque type
de ticket en **HITL** (*"human in the loop, worked with a human who speaks for themselves"*) ou **AFK**
(*"driven by the agent alone"*) : Research = AFK · Prototype, Grilling, Task = HITL (Task pouvant être
les deux). Et l'invariant :

> *"A HITL ticket only resolves through that live exchange; the agent never stands in for the human's
> side of it."*

**(absent, et refusé explicitement)** Il n'existe **aucun mode solo pour produire une spec**. Aucun
skill ne grille *et* rédige sans validation humaine intermédiaire. Le corpus assume que la spec est un
artefact de binôme.

**(prompt)** Le plafond de questions a été demandé et **refusé** — `.out-of-scope/question-limits.md` :

> *"Grilling is intentionally open-ended […] some plans need three questions, some need fifty. A fixed
> cap would either cut off useful exploration on hard problems or feel arbitrary on easy ones."*

avec une distinction de cause qui est le vrai apport : trop de questions **parce que le plan est
sous-spécifié** est *"working as intended"* ; des questions **redondantes** sont *"a prompt-quality
issue, not a quantity issue… belongs in the skill prompt, not in a counter"*. La soupape assumée est le
langage naturel : *"natural-language steering is the intended control surface, not a numeric limit."*
**(histoire)** Motivé par un ticket réel : *"#44 — 'Codex just asked me 200 questions'"*.

### 1.4 Deux corrections à mes propres suppositions

**(doc)** `ask-matt` n'est **pas** du role-prompting incarnant une personne. C'est un **routeur** entre
skills, nommé d'après l'auteur : *"It **does no work itself**. It doesn't grill, write a spec, or fix
anything — it only orients."* Sa seule ligne biographique est `ask-matt/SKILL.md:9` — *"You don't
remember every skill, so ask."*

**(prompt)** `wayfinder` traite le travail *"too big for one agent session, and wrapped in fog"* : il
tient une **carte de tickets de décision** sur le tracker (*"questions whose resolution is a decision,
not slices of a build to execute"*), avec la règle **"Plan, don't do"** — *"produce decisions, not
deliverables"* — une section **"Not yet specified"** nommée *fog of war* pour les questions pressenties
mais pas encore assez nettes, et **au plus un ticket résolu par session**. Il rend la main à `to-spec`.

---

## 2. Concevoir de bonnes sous-tâches

### 2.1 L'axe de découpage, et une unité de mesure neuve

**(prompt)** `skills/engineering/to-tickets/SKILL.md:29-36`, bloc `<vertical-slice-rules>` :

> *"Each slice cuts a narrow but COMPLETE path through every layer (schema, API, UI, tests) — vertical,
> NOT a horizontal slice of one layer"*
> *"A completed slice is demoable or verifiable on its own"*
> *"Each slice is sized to fit in a single fresh context window"*
> *"Any prefactoring should be done first"*

Trois critères, aucun temporel. Le troisième est le plus intéressant : **la taille se mesure en fenêtre
de contexte**, pas en jours ni en fichiers. C'est une unité *native à l'exécutant*, et elle a l'avantage
d'être opposable — un pas qui ne tient pas dans un contexte frais est trop gros, point.

**(prompt)** L'exception est écrite : *"Wide refactors are the exception to vertical slicing"* — traités
en **expand–contract**, en lots dimensionnés par *blast radius* (par paquet, par dossier), avec un
ticket d'intégration final où *"green is promised only there"*.

### 2.2 L'ordre, et le gate

**(prompt)** Jamais une liste plate : *"Give each ticket its blocking edges — the other tickets that
must complete before it can start. A ticket with no blockers can start immediately."* Publication *"in
dependency order (blockers first)"*, exécution sur la **frontier** (*"any ticket whose blockers are all
done"*), plusieurs agents pouvant la travailler en parallèle sur un vrai tracker.

**(prompt)** Étape 4, *"Quiz the user"* — le gate, littéral :

> *"Does the granularity feel right? (too coarse / too fine)"* · *"Are the blocking edges correct?"* ·
> *"Should any tickets be merged or split further?"* — *"Iterate until the user approves the breakdown."*

Rien n'est publié avant accord.

### 2.3 `AGENT-BRIEF` — ce qu'on met dans un brief pour un agent sans la conversation

C'est le fragment le plus directement applicable à notre pari (« le ticket est l'unique brief d'un futur
`AgentStep` »). **(prompt)** `skills/engineering/triage/AGENT-BRIEF.md`, quatre principes :

1. **"Durability over precision"** — *"Don't reference file paths — they go stale"*, *"Don't reference
   line numbers"*, *"Don't assume the current implementation structure will remain the same"*.
2. **"Behavioral, not procedural"** — l'exemple annoté est plus clair qu'une règle :
   *Good*: `"The SkillConfig type should accept an optional schedule field…"` ·
   *Bad*: `"Open src/types/skill.ts and add a schedule field on line 42"`.
3. **"Complete acceptance criteria"** — *"The agent needs to know when it's done… Each criterion should
   be independently verifiable."*
4. **"Explicit scope boundaries"** — *"This prevents the agent from gold-plating or making assumptions
   about adjacent features."*

Gabarit : `Category`, `Summary`, `Current behavior`, `Desired behavior`, `Key interfaces`,
`Acceptance criteria`, `Out of scope` — **suivi d'un contre-exemple explicitement annoté « bad »**
(référence de ligne, critères absents, formulation vague).

### 2.4 `implement` fait quinze lignes, et c'est un choix

**(prompt)** Intégralement, `skills/engineering/implement/SKILL.md:7-15` :

> *"Implement the work described by the user in the spec or tickets.*
> *Use /tdd where possible, at pre-agreed seams.*
> *Run typechecking regularly, single test files regularly, and the full test suite once at the end.*
> *Once done, use /code-review to review the work.*
> *Commit your work to the current branch."*

**(doc)** `docs/engineering/implement.md:17` : *"It does **not** decide what to build… It is the hands,
not the head — the thinking happened upstream."* Le skill d'implémentation est un **routeur**, parce que
toute la conception a été consommée en amont.

### 2.5 `triage` et le refus écrit

**(prompt)** Machine à états à deux registres : catégorie (`bug`/`enhancement`) et cinq états
(`needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`). Le point qui compte :
**la vérification précède la qualification** — *"Verify before you brief"*, un bug se reproduit, une PR
se `checkout` et s'exécute, avant tout gate.

**(prompt)** `OUT-OF-SCOPE.md` prescrit un fichier **par concept, pas par issue**, en *"short design
document"* (`# Concept`, `## Why this is out of scope`, `## Prior requests`), avec dédup *"by concept
similarity, not keyword"* et une exigence de motif **durable** : jamais *"we're too busy right now"*.

---

## 3. Relire du travail déjà fait

### 3.1 Deux axes, en parallèle, jamais fusionnés

**(prompt)** `skills/engineering/code-review/SKILL.md:6` — la revue porte sur **un diff** contre un point
fixe fourni par l'utilisateur (`git diff <fixed-point>...HEAD`), jamais sur une PR ou un fichier. Deux
lentilles, tournant en **sous-agents parallèles indépendants** pour ne pas se polluer :

- **Standards** — *"does the code follow this repo's documented coding standards?"*, **plus une baseline
  fixe indépendante du dépôt** : les douze code smells de Fowler (*Refactoring*, ch. 3), énumérés un par
  un sous forme *quoi → comment corriger* (Mysterious Name, Duplicated Code, Feature Envy, Data Clumps,
  Primitive Obsession, Repeated Switches, Shotgun Surgery, Divergent Change, Speculative Generality,
  Message Chains, Middle Man, Refused Bequest).
- **Spec** — *"does the code faithfully implement the originating issue / PRD / spec?"*

Et l'agrégation interdit la fusion : *"Do **not** merge or rerank findings"*. **(doc)**
`docs/engineering/code-review.md:15` donne le motif : *"a single blended verdict lets one mask the
other."*

### 3.2 Pas de verdict, et un garde-fou par citation

**(absent)** Vérifié par grep : ni *"block"*, ni *"severity"*, ni *"false positive"* dans le skill. La
revue **produit une liste**, jamais un pass/fail : deux sections `## Standards` / `## Spec`, et une
ligne de synthèse — *"total findings per axis"*. Aucune intégration CI, aucun code de sortie.

**(prompt)** Contre le relecteur qui invente, le corpus ne choisit pas la vérification empirique mais la
**traçabilité forcée** : *"cite the standard (file + the rule)"* et *"quote the hunk"* côté Standards,
*"Quote the spec line for each finding"* côté Spec. Plus un étiquetage de confiance :

> *"Distinguish hard violations from judgement calls — documented-standard breaches can be hard, but
> baseline smells are always judgement calls."*

C'est un choix assumé et différent de celui de `triage`/`diagnosing-bugs`, qui eux **reproduisent** avant
de qualifier.

### 3.3 L'ancrage du relecteur : la question tranchée par la séparation

**(prompt)** Étape 2 : le relecteur *Spec* **reçoit l'intention**, via une recherche hiérarchisée —
références d'issue dans les messages de commit → chemin fourni par l'utilisateur → fichier de spec sous
`docs/`, `specs/`, `.scratch/` → sinon demander. Et s'il n'y a rien :

> *"If they say there isn't one, the **Spec** sub-agent will skip and report 'no spec available'."*

**(doc)** — plutôt qu'inventer des exigences. L'axe *Standards*, lui, ne dépend d'aucune intention : il
porte sa baseline même sans contexte.

> **Ce que ça résout.** Notre `docs/reference/skills.md` §5.1 posait un dilemme : donner au relecteur
> l'intention de l'auteur l'**ancre**, la lui cacher le prive du référentiel. La réponse d'ici est de ne
> pas trancher globalement mais de **séparer les axes** — celui qui juge la conformité à l'intention la
> reçoit, celui qui juge la qualité intrinsèque ne la voit jamais. Et les deux ne sont pas fusionnés en
> un avis, précisément pour que l'un ne masque pas l'autre.

### 3.4 Deux dispositifs annexes qui valent d'être notés

**(prompt)** `DESIGN-IT-TWICE.md` (d'après Ousterhout — *"your first idea is unlikely to be the best"*) :
3 sous-agents ou plus **en parallèle**, chacun sous une **contrainte de conception différente**
(minimiser l'interface · maximiser la flexibilité · optimiser pour l'appelant le plus commun · ports &
adapters), produisant chacun une interface *"radically different"* pour le **même** candidat. Ce qui est
comparé, ce sont les interfaces **entre elles**, sur trois axes fixés — *"depth… locality… and seam
placement"* — puis une recommandation assumée, éventuellement hybride.

**(prompt)** `hitl-loop.template.sh` (42 lignes) est le **dixième et dernier recours** de
`diagnosing-bugs` : *"HITL bash script. Last resort. If a human must click, drive them with
scripts/hitl-loop.template.sh so the loop is still structured."* Deux fonctions : `step()` qui bloque sur
`read -r -p "[Enter when done]"`, et `capture()` qui lit une réponse dans une variable. Sortie en
`KEY=VALUE`, reparsable. Le motif du script plutôt que du prompt : un prompt ne peut ni **bloquer de
façon synchrone** sur une action humaine réelle, ni **garantir un format de sortie**. Même le pire
chemin reste une boucle mécaniquement exploitable.

### 3.5 Les dépréciés de la revue

**(histoire)** `qa` (130 l.) et `request-refactor-plan` (68 l.) sont partis le **2026-04-28**, dans un
déplacement en bloc. `qa` était une session où l'utilisateur signalait des bugs en langage naturel et où
l'agent **déposait directement** les issues — `"Do NOT ask the user to review first — just file and
share URLs"`. Aucun motif écrit par skill n'existe **(absent)** : le `README.md` de `deprecated/` dit
seulement *"Skills I no longer use."*

**Inférence signalée comme telle** : le recouvrement fonctionnel avec `triage` est net — d'un rôle « QA
qui dépose et fait confiance » vers un vérificateur qui **reproduit avant de qualifier**. Mais aucun
`git mv` ne l'établit ; c'est une lecture de contenu, pas une preuve d'historique.

---

## 4. Accumuler de la connaissance métier

C'est la question sur laquelle tous les cadres précédents échouaient. Celui-ci y répond, et la réponse
tient en une phrase : **un artefact de connaissance ne vaut que par le nombre de skills qui le
rechargent.**

### 4.1 Les deux artefacts, et leur frugalité

**(prompt)** `domain-modeling` produit `CONTEXT.md` (glossaire) et des ADR. Sa première ligne délimite
son propre périmètre, ce qui est rare :

> *"Merely reading `CONTEXT.md` for vocabulary is not this skill — that's a one-line habit any skill can
> do. This skill is for when you're changing the model, not just consuming it."*

`CONTEXT-FORMAT.md` : entrées `**Terme**:` / définition en 1-2 phrases / `_Avoid_: <synonymes>`, avec un
filtre net — *"Only include terms specific to this project's context. General programming concepts […]
don't belong."*

`ADR-FORMAT.md` : `docs/adr/NNNN-slug.md`, *"1-3 sentences: what's the context, what did we decide, and
why"*, suivi de la ligne qui donne le ton du corpus entier :

> *"That's it. An ADR can be a single paragraph."*

Et un seuil d'écriture à trois conditions — irréversible **et** surprenant **et** arbitrage réel : *"If
any of the three is missing, skip the ADR."* Plus une règle de création paresseuse : *"Create files
lazily — only when you have something to write."*

### 4.2 Le chemin de retour, câblé dans quatre skills

**(prompt)** C'est le point décisif, et il est vérifiable par grep. `CONTEXT.md` et les ADR sont relus
**en dur, avant exploration du code**, par :

- `diagnosing-bugs/SKILL.md:10` — *"When exploring the codebase, read `CONTEXT.md` (if it exists) to get
  a clear mental model of the relevant modules, and check ADRs in the area you're touching."*
- `tdd/SKILL.md:10` — même formule, appliquée au **nommage des tests**.
- `improve-codebase-architecture` — lit avant d'explorer, réutilise le vocabulaire pour nommer les
  modules, et **réécrit** `CONTEXT.md` si un renommage introduit un terme absent du glossaire.
- `triage/SKILL.md:76` — invoque `/domain-modeling` en cours de triage.

Boucle fermée : un skill écrit, quatre relisent, un réécrit quand il découvre un terme neuf.

### 4.3 `ubiquitous-language` est mort de n'être relu par personne

**(histoire)** Créé le 2026-03-16, déprécié le **2026-04-28**. Il faisait une extraction ponctuelle en
fin de conversation, écrivait `UBIQUITOUS_LANGUAGE.md` avec tableaux `| Term | Definition | Aliases to
avoid |`, une section `## Relationships`, un dialogue d'exemple, des ambiguïtés signalées — et prévoyait
même sa propre relecture (`## Re-running`).

**Le diagnostic** : `UBIQUITOUS_LANGUAGE.md` **n'était référencé par aucun autre skill que lui-même**
(vérifié par grep). Une boucle fermée sur une seule conversation, qui n'irriguait ni `tdd`, ni
`diagnosing-bugs`, ni rien. Son remplaçant, `domain-modeling` (créé onze jours **avant** la
dépréciation), déplace la capture d'un mode *extraction ponctuelle à la demande* vers un mode *continu et
consommé par d'autres*.

**(absent)** Aucun ADR, aucun message de commit n'énonce ce motif — il est reconstruit par corrélation de
dates et par grep. Je le signale : c'est une lecture, cohérente et étayée, pas une déclaration.

> **La règle qu'on peut en tirer**, et elle est opposable : *un artefact de connaissance dont aucun autre
> skill ne cite le chemin est une archive.* Le test se fait par grep, en une commande, et il est binaire.

### 4.4 Les limites, dites franchement

**(absent)** Rien de mécanique ne force la relecture : ni hook, ni linter, ni CI. Le chemin de retour est
**porté par la prose de chaque skill**, avec dégradation silencieuse assumée —
`setup-matt-pocock-skills/domain.md:11` : *"If any of these files don't exist, proceed silently."*
L'`ADR-0001` formalise même la gradation : dépendance **dure** (`to-spec`, `to-tickets`, `triage` :
pointeur de setup explicite) vs **molle** (`tdd`, `diagnosing-bugs` : *"reference 'the project's domain
glossary'… in vague prose only. If the docs aren't there, the skill still works"*).

**(histoire)** Et le dépôt ne s'applique pas entièrement sa propre discipline : son `CONTEXT.md` racine
porte deux sections (`## Relationships`, `## Flagged ambiguities`) qui **n'existent pas** dans
`CONTEXT-FORMAT.md` mais qui sont, verbatim, celles du skill déprécié `ubiquitous-language` — des
fossiles que la transition n'a jamais nettoyés. Il n'a par ailleurs jamais fait tourner
`setup-matt-pocock-skills` sur lui-même (pas de `docs/agents/domain.md`, pas de bloc `## Agent skills`
dans son `CLAUDE.md`).

**(histoire)** Signe de vitalité tout de même, contrairement à Task Master : `CONTEXT.md` créé le
2026-04-28, **modifié 3 fois**, dernier changement à J-8 du `HEAD`. L'ADR-0001 a été révisé sur près de
trois mois.

### 4.5 `teach` — la capitalisation d'apprentissage, transposable

**(prompt)** Le plus gros skill du dépôt (140 l. + 4 formats) vise l'apprentissage **humain**, mais sa
mécanique se transpose. Les `learning-records` sont décrits comme *"the teaching equivalent of ADRs: they
capture non-obvious lessons, key insights, and stated prior knowledge that will steer future sessions."*
Trois déclencheurs d'écriture — compréhension démontrée, connaissance préalable révélée, méconception
corrigée — et une exclusion qui vaut d'être retenue :

> *"Material that was merely covered. Coverage is not learning. Wait for evidence."*

Et ils **sont relus systématiquement** : la section *Zone Of Proximal Development* calcule la prochaine
leçon en *"Reading their learning-records"*.

**(prompt)** À l'inverse, `handoff` (16 l.) écrit son document dans le **répertoire temporaire de l'OS**,
hors dépôt : *"Do not duplicate content already captured in other artifacts (specs, plans, ADRs, issues,
commits, diffs). Reference them by path or URL instead."* Bonne règle de non-duplication, mais chemin de
retour **nul par construction** — c'est une passation, pas une accumulation. Assumé.

---

## 5. Bonus — l'artisanat du skill

Directement utile aux huit skills que nous avons à écrire.

### 5.1 Le principe fondateur

**(prompt)** `writing-great-skills/SKILL.md:7` :

> *"A skill exists to wrangle determinism out of a stochastic system. **Predictability** — the agent
> taking the same **process** every run, not producing the same output — is the root virtue; every lever
> below serves it."*

La précision *process, not output* est ce qui rend le critère utilisable : on ne demande pas la même
sortie, on demande le même chemin.

### 5.2 Les leviers, verbatim

- **La description fait le déclenchement** : *"Front-load the skill's leading word — the description is
  where it does its invocation work."* et *"One trigger per branch. Synonyms that rename a single branch
  are duplication."* Patron observé sur les skills model-invoked : verbe d'ouverture + *"Use when the
  user wants…, mentions…, asks for…"*, une branche distincte par item. Exemple —
  `tdd/SKILL.md:3` : *"Test-driven development. Use when the user wants to build features or fix bugs
  test-first, mentions 'red-green-refactor', or wants integration tests."*
- **Le critère d'achèvement par étape** : *"Make it checkable… and, where it matters, exhaustive ('every
  modified model accounted for', not 'produce a change list') — a vague criterion invites premature
  completion."*
- **Le leading word** : *"A leading word is a compact concept already living in the model's pretraining
  that the agent thinks with while running the skill (e.g. lesson, fog of war, tracer bullets)."* Avec
  deux exemples de compression : *"'fast, deterministic, low-overhead' -> tight"* et *"'a loop you
  believe in' -> red"*.
- **Le no-op test, élagage phrase par phrase** : *"hunt no-ops sentence by sentence, not just line by
  line: run the no-op test on each sentence in isolation, and when one fails, delete the whole sentence
  rather than trim words from it. Be aggressive — most prose that fails should go, not be rewritten."*
- **La divulgation progressive** : *"Branching is the cleanest disclosure test: inline what every branch
  needs, and push behind a pointer what only some branches reach."* Le mécanisme est un **lien Markdown
  littéral** que l'agent suit, jamais un import — et *"A context pointer's wording, not its target,
  decides when and how reliably the agent reaches the material."*
- **La granularité** : *"Granularity is how finely you divide skills, and each cut spends one of the two
  loads, so split only when the cut earns it."* (les deux charges : contexte pour les skills
  model-invoked, charge cognitive humaine pour les user-invoked).

### 5.3 Les faits de structure

**(histoire, chiffré)** 41 skills, **2 823 lignes**, min 7, **médiane 75**, max 140. **Aucune longueur
cible chiffrée n'est prescrite** — la contrainte passe par le mode d'échec nommé **Sprawl**. Le
`GLOSSARY.md` du méta-skill formalise chaque terme avec une section `_Avoid_:` listant les synonymes
proscrits, exactement comme `CONTEXT-FORMAT.md` le fait pour le métier.

**(prompt)** Composition : *"Dependencies are expressed as `/skill`-style prose invocation ('Run the
`/grilling` skill'), not deep `../other-skill/FILE.md` cross-references."* Et une règle de hiérarchie :
*"A user-invoked skill may invoke model-invoked skills, but it can never reach another user-invoked
skill."*

**(prompt, extrait d'un commit de suppression)** Le principe le plus net sur le partage skill/doc, tiré
de *"cut the two-readings explainer from SKILL.md"* : *"It documented what the skill does rather than
steering the agent."* Donc — **un `SKILL.md` dirige l'agent ; il n'explique pas le skill.**
L'explication vit dans `docs/`, pour l'humain. C'est une quatrième adresse à ajouter aux trois lieux de
notre `D-038`.

**(doc / absent)** Cycle de vie : buckets `engineering` · `productivity` · `misc` (*"kept around but
rarely used, not promoted"*) · `personal` · `in-progress` (*"not ready to ship… excluded from the plugin
and the top-level README until they graduate"*) · `deprecated`. Le mot *graduate* est écrit, **le critère
ne l'est pas**. Le trajet n'est pas à sens unique : `teach` est descendu en `in-progress` puis remonté ;
un même commit promeut `review → code-review` et rétrograde `decision-mapping`.

### 5.4 Le point où ce corpus contredit notre dossier `skills.md`

**(prompt)** `writing-great-skills/SKILL.md:83` prescrit :

> *"steering by prohibition backfires: don't think of an elephant names the elephant and makes it more
> available, not less. Prompt the positive… keep a prohibition only as a hard guardrail you can't phrase
> positively."*

Or notre `docs/reference/skills.md` §8.3 range précisément cette croyance parmi ce **qui circule et
n'est pas établi** — le chiffre qui la véhicule n'a aucune méthode, et la seule mesure directe ne conclut
à rien.

**Comment tenir les deux sans en sacrifier un :**

1. Ce corpus n'apporte pas une mesure, il apporte **une panne datée** — l'agent qui se grillait lui-même,
   causé par une tournure négative, corrigé en affirmatif. Une anecdote d'ingénierie vaut moins qu'une
   mesure, mais **plus qu'une statistique sans méthode** : elle est vérifiable dans le diff.
2. Les deux dossiers **convergent sur le remède**. Notre `skills.md` conclut que le grief est **cardinal,
   pas grammatical** (c'est le nombre de règles chargées qui nuit). Le `no-op test` phrase par phrase de
   §5.2 est exactement un remède cardinal — et il est appliqué avec une agressivité que nous n'avons pas.
3. La formulation prudente à retenir, donc : *préférer l'affirmatif quand c'est possible* est un conseil
   d'écriture peu coûteux et sans contre-indication ; en faire une **loi** n'a toujours aucun support. Ils
   le disent d'ailleurs eux-mêmes en gardant l'exception — *"a hard guardrail you can't phrase
   positively"*.

---

## 6. Bonus — l'archéologie, et ce qu'elle montre

**(histoire)** Né le 2026-02-03 (sept skills d'un coup), `HEAD` au 2026-07-21. Rythme mensuel : février
10 · mars 11 · avril 42 · mai 36 · juin 62 · **juillet 153 en trois semaines**. Aucun creux de plus de
deux semaines. 60 branches distantes ouvertes, dont une quinzaine préfixées `wayfinder-*`. **Ce corpus
accélère** — contraste frontal avec le dogfooding éteint de Task Master.

**Les skills maigrissent-ils ? Oui, par extraction — pas par simple élagage :**

| Skill | Naissance | `HEAD` | Mouvement |
|---|---|---|---|
| `tdd` | 82 l. | **36 l.** | annexes extraites vers `codebase-design` / `domain-modeling` |
| `grill-with-docs` (ex `domain-model`) | 79 l. | **7 l.** | réduit à une invocation de deux primitifs |
| `improve-codebase-architecture` | 76 l. | 71 l. | stable |
| `ask-matt` | 59 l. | 78 l. | **grossit** — c'est le routeur |
| `teach` | 87 l. | 140 l. | **grossit** — le plus gros workflow |

Le mécanisme est visible en un commit (2026-05-31) : suppression de `tdd/deep-modules.md`,
`tdd/interface-design.md`, `improve-codebase-architecture/LANGUAGE.md` **et** création simultanée de
`codebase-design/SKILL.md` (114 l.) et `domain-modeling/SKILL.md` (74 l.). Le vocabulaire technique
**migre vers des skills de référence partagés** ; il ne s'évapore pas. Même logique pour *"tdd: drop the
refactor stage"* — *"Refactoring belongs to the review stage, not the implementation loop"*.

**(prompt)** `.out-of-scope/` : trois refus écrits en **48 heures fin avril**, jamais rouverts depuis
trois mois, chacun répondant à une **demande externe précise** (`#99`, `#44`, `#106`) plutôt qu'à un
principe anticipé. C'est un journal figé de non-décisions, pas un espace vivant — mais chaque entrée
porte un motif durable, ce qui la rend opposable.

**(histoire)** `deprecated/` n'est pas décoratif : les skills retirés sont **purgés de
`.claude-plugin/plugin.json`** et **exclus du script d'installation** `link-skills.sh`. « Déprécié » veut
dire *exclu de la distribution*. Nuance à ne pas manquer : le dossier a aussi servi de **sas vers la
suppression pure** — `triage-issue` y est entré et en est sorti par la porte du néant sept heures plus
tard.

**Les vrais retraits sont avoués sans habillage** : *"`caveman` was a duplicate of another skill I was
testing and was never meant to be public."* · *"`zoom-out` went unused in practice, so it's been removed
from the repo."* Aucun de ces retraits n'est présenté comme un pivot stratégique.

---

## 7. Ce que ça change dans le dossier existant

**Confirmation, troisième source indépendante.** Aucun **tiers** ne bloque : `code-review` produit une
liste, jamais un verdict. `D-041` (le tiers prononce la conformité) nous rend toujours plus stricts que
l'état de l'art.

**Mais une nuance qui manquait, et qui corrige le trait de `bmad.md` §2.6.** Ce corpus a de **vrais
gates humains** — l'agent s'arrête et attend : sur l'alignement (*"Do not act on it until I confirm"*),
sur les seams de test, sur la granularité du découpage (*"Iterate until the user approves"*). Ils sont
tous situés aux **frontières de planification**, jamais aux frontières d'achèvement. La formule juste
n'est donc pas « le champ n'a pas de gates » mais : **le champ gate l'entrée dans le travail, pas sa
sortie.** Nos DoD, elles, gatent la sortie — c'est là que nous sommes seuls, et c'est plus précis que ce
que nous avions écrit.

**Contradiction assumée, §5.4** : sur les formulations négatives, ce corpus prescrit ce que notre
`skills.md` classe en folklore. Résolution proposée : conseil d'écriture peu coûteux, oui ; loi, non.

**Divergence de méthode à ne pas adopter en silence** : ils ont **sorti le refactor du cycle TDD** pour
le confier à la revue. Notre `CLAUDE.md` l'y garde. Leur motif est cohérent (le refactor a besoin du
recul d'une relecture), le nôtre aussi (refactorer au vert, tant que le contexte est chaud). À rejuger
seulement si notre `revue-code` devient assez rodé pour absorber cette charge — pas avant.

**Le contraste avec Task Master est net et instructif** : là-bas, un template de 511 lignes que rien ne
charge ; ici, un `CONTEXT.md` de 30 lignes cité par quatre skills. La différence n'est pas la qualité de
la rédaction — elle est **dans le nombre de chemins de retour**.

---

## 8. Ce que ça met sur la table pour Cursus

1. **Séparer l'entretien de la rédaction, par une phrase.** `to-spec` s'ouvre sur *"Do NOT interview the
   user — just synthesize what you already know."* C'est notre frontière `Discovery | Spec` (`D-041`)
   obtenue au coût d'une ligne, et placée là où l'agent la lira. À reprendre presque tel quel dans le
   futur skill de spec.

2. **Le partage fait / décision, à mettre dans tous nos skills interactifs.** *Les faits, l'agent va les
   chercher ; les décisions sont à l'humain, une à la fois, avec une réponse recommandée.* C'est la règle
   qui empêche à la fois l'agent de faire perdre son temps à l'humain et de trancher à sa place. Elle
   répond aussi, par avance, à la question ouverte n°1 de `tickets.md` (où vivent les rappels de
   contexte) : ce qui est **factuel** n'a pas besoin d'être rappelé, il se retrouve.

3. **Un primitif d'entretien plutôt qu'un entretien par skill.** Huit skills qui réinventent chacun leur
   façon de questionner, c'est huit divergences. `grilling` est un skill de 12 lignes que les autres
   invoquent. Candidat sérieux pour être écrit **avant** `prendre-un-pas` — il est plus petit, sans
   dépendance, et il servira à tous les autres.

4. **La taille d'un pas se mesure en fenêtre de contexte fraîche.** Critère opposable, natif à
   l'exécutant, et complémentaire du nôtre (*recettable par quelqu'un qui ne lit pas le code*, qui juge
   le **niveau**, pas la taille). Les deux ensemble couvrent ce qui manquait cruellement à Task Master
   (§ leurs 45 sous-tâches sous un parent).

5. **`AGENT-BRIEF` est le gabarit que notre pari réclame.** « Durabilité plutôt que précision »,
   « comportemental jamais procédural », le contre-exemple annoté *bad* — c'est exactement ce qu'il faut
   à un ticket qui deviendra l'unique brief d'un `AgentStep`. Et ça converge avec l'interdiction déjà
   prise chez nous (`D-042` : plus jamais de hash dans la doc) : mêmes causes, même remède, appliqué aux
   chemins et aux numéros de ligne.

6. **La revue en deux axes non fusionnés** résout notre dilemme d'ancrage (`skills.md` §5.1) : l'axe qui
   juge la conformité reçoit l'intention ; l'axe qui juge la qualité intrinsèque ne la voit jamais ; et
   on interdit de les fondre, *"a single blended verdict lets one mask the other"*. À reprendre dans
   `revue-code`, avec nos propres axes (conformité à la DoD · qualité intrinsèque · §7.12 à l'œil).

7. **L'abstention explicite plutôt que l'invention.** *"the Spec sub-agent will skip and report 'no spec
   available'"*. C'est précisément le remède au gotcha que nous avions payé sur `bmad.md` — un relecteur
   sans accès à la source doit produire une **abstention**, pas un verdict. Ici c'est écrit dans le
   prompt, pas découvert après coup.

8. **Le test du chemin de retour, en une commande.** Un artefact de connaissance dont **aucun skill ne
   cite le chemin** est une archive : c'est ce qui a tué `ubiquitous-language`, et c'est le sort que
   `task-master.md` prédisait pour notre `journal-frictions.md`. Le test est binaire et se fait par grep.
   Corollaire opérationnel : quand un skill naîtra du journal, il devra **citer le journal**, ou la
   boucle `D-039` restera ouverte.

9. **L'ADR d'un paragraphe, et le seuil à trois conditions.** *"That's it. An ADR can be a single
   paragraph."* — avec écriture seulement si c'est **irréversible, surprenant, et un arbitrage réel**.
   Notre `decisions.md` est nettement plus verbeux ; ce n'est pas forcément un défaut (il porte le
   *pourquoi dans le temps*, pas seulement la décision), mais le **seuil à trois conditions** mérite
   d'être confronté à notre pratique — nous écrivons parfois un `D-NNN` là où une ligne
   d'`architecture.md` suffirait.

10. **L'artisanat, pour nos huit skills** : *predictability = même processus, pas même sortie* comme
    critère · un **leading word** par skill · un **critère d'achèvement vérifiable** par étape · le
    **no-op test** phrase par phrase, agressif · l'annexe pour ce que **seules certaines branches**
    atteignent · et la règle la plus économique du lot — **un `SKILL.md` dirige, un `docs/` explique**.
    Cette dernière est une quatrième adresse à ajouter aux trois lieux de `D-038`.

11. **Ce qu'ils n'ont pas et que nous devrions garder** : aucun critère écrit de graduation
    `in-progress → stable`, ni de dépréciation. Nos DoD par niveau × statut sont précisément l'outil qui
    manque ici — c'est le seul point du dossier où nous sommes en avance, autant le savoir.

---

## Sources

Dépôt `mattpocock/skills`, `HEAD` = `ed37663` (**2026-07-21**), clone **complet** (314 commits, 1 auteur
principal), lu sur disque le **2026-07-27**.

Fichiers les plus cités : `skills/productivity/{grilling,grill-me,writing-great-skills}/` ·
`skills/engineering/{to-spec,to-tickets,implement,triage,tdd,code-review,codebase-design,domain-modeling,diagnosing-bugs,wayfinder,ask-matt,grill-with-docs,improve-codebase-architecture,setup-matt-pocock-skills}/`
· `skills/deprecated/{qa,request-refactor-plan,ubiquitous-language,design-an-interface}/` ·
`.out-of-scope/{question-limits,mainstream-issue-trackers-only,setup-skill-verify-mode}.md` ·
`.agents/{invocation.md,writing-docs.md,adr/000{1,2}-*.md}` · `CONTEXT.md` · `docs/engineering/*.md` ·
`docs/productivity/*.md` · `.claude-plugin/plugin.json` · `scripts/link-skills.sh` · `.changeset/*.md`.

Documents compagnons : `docs/reference/skills.md` (la doctrine d'écriture des skills — ce fichier-ci en
est le pendant *praticien*, et le contredit sur un point, §5.4) · `docs/reference/bmad.md` (le champ des
cadres) · `docs/reference/task-master.md` (le contre-exemple : la méthode en prose non chargée) ·
`docs/reference/symphony.md`.
