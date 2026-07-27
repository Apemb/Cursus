# Task Master (`eyaltoledano/claude-task-master`) — anatomie du flux PRD → tâches → exécution

> **Pourquoi ce fichier.** `docs/reference/bmad.md` cartographie les cadres de méthode agentique mais
> annonce explicitement que **claude-task-master n'a rien produit de vérifié** et n'y est pas traité.
> Ce document comble ce trou. La question posée : comment ce projet écrit-il des PRD, les découpe-t-il
> en tâches, estime-t-il leur complexité, les valide-t-il, et consigne-t-il ce qu'il apprend ? Task
> Master est le cadre le plus proche de notre pari — un backlog machine consommé par des agents — donc
> le plus instructif à démonter.
>
> **Fiabilité des sources.** Passe unique **sur la source primaire**, pas sur du web : dépôt cloné,
> fichiers lus sur disque, prompts cités verbatim, chiffres comptés avec `jq`/`git log` et non estimés.
> Six lectures indépendantes (cinq thématiques + une passe de falsification ciblée). Chaque affirmation
> porte son registre : **(code)** = lisible dans le code ou un prompt versionné · **(doc)** = affirmé
> par la documentation ou un prompt sans que le code le montre · **(absent)** = cherché, inexistant.
>
> **Limites du dispositif.** Le clone est **superficiel** (206 commits visibles) : les `git log --follow`
> ne remontent que quelques commits par fichier. Les datations d'apparition sont donc des bornes, pas
> des preuves d'absence historique. `HEAD` observé : `c0c98d3`, **2026-04-23** — le dépôt a environ trois
> mois de retard sur la date de cette étude.
>
> **Aucun `D-NNN` n'est écrit ici.** Comme pour `bmad.md`, c'est une cartographie neutre ; la décision,
> si elle vient, ira dans `decisions.md`.

---

## Le résultat en une page

Task Master est un **générateur de backlog en un appel LLM, plus un porte-notes**. Ce qui est
réellement outillé est étroit et fonctionne ; tout ce qui ressemble à de la *méthode* — gates,
validation, revue, capitalisation — existe uniquement sous forme de **prose adressée au même agent qui
fait le travail**, sans une ligne de code derrière.

| Étape | Ce qui existe | Registre |
|---|---|---|
| Écrire un PRD | Rien. Chat libre humain↔LLM, hors du système. Deux templates posés sur le disque | (code) |
| Lever une ambiguïté | Rien. Le prompt ordonne au modèle de **combler les trous seul** | (code) |
| PRD → tâches | Un appel LLM, un schéma Zod, un seul axe prescrit : l'ordre d'implémentation | (code) |
| Tâche → sous-tâches | Mécanisme **séparé** (`expand`), deux niveaux au total, jamais trois | (code) |
| Estimer la complexité | Un appel LLM, note 1–10, **grille de notation quasi vide** | (code) |
| Seuil de complexité | Décoratif : aucun branchement ne le lit | (code) |
| Valider une tâche | Rien. N'importe qui écrit `done`, pas de machine à états | (code) |
| Vérifier `testStrategy` | Rien. Champ généré, jamais exécuté par quoi que ce soit | (absent) |
| Passer du contexte | **Journal append horodaté** dans `details` — le seul mécanisme vraiment transférable | (code) |
| Capitaliser un apprentissage | Rien au-delà de la note locale. Règles **statiques** | (absent) |
| Mesurer l'efficacité | Rien. Seul le **coût** ($/tokens) est calculé, et n'est même pas persisté | (code) |

Le motif de `bmad.md` §2.6 — « les gates vérifient la forme, jamais le fond » — se retrouve ici sous
une forme **plus dégradée encore** : les gates de Task Master ne vérifient même pas la forme. Ce sont
des listes à puces en langage naturel dans des fichiers de slash-command, que le modèle est libre
d'ignorer, et dont l'échec ne bloque aucun appel.

---

## 1. Le PRD : aucun processus, deux templates, et un piège

### 1.1 Il n'existe aucune capacité de rédaction

**(code)** Aucune commande, aucun outil MCP, aucun skill ne *produit* un PRD. Le seul outillage agit
**après** que le fichier existe : `task-master parse-prd` (`scripts/modules/commands.js:944-995`) lit un
PRD et génère des tâches. La rédaction est renvoyée au chat libre, par une simple instruction de
comportement adressée à l'agent de l'IDE — `.cursor/rules/dev_workflow.mdc:85-90` :

> *"Work with the user to create a detailed PRD file (e.g., `.taskmaster/docs/feature-xyz-prd.txt`)"*

et la documentation l'assume franchement (`apps/docs/getting-started/quick-start/prd-quick.mdx:44-50`) :

> *"You can co-write your PRD with an LLM model using the following workflow: 1. Chat about
> requirements… 2. Show an example PRD… 3. Iterate and refine… This approach works great in Cursor, or
> anywhere you use a chat-based LLM."*

**(absent)** Vérifié : aucun `SKILL.md`, aucun répertoire `skills/` dans le dépôt. Les ~50 commandes
livrées (`packages/claude-code-plugin/commands/`, `.claude/commands/go/`, `.cursor/commands/`,
`packages/tm-profiles/src/slash-commands/`) ont toutes été lues sur ce critère : **aucune n'instruit un
agent à poser des questions, itérer ou remplir un template de PRD**. `init-project.md` enchaîne sur
`parse-prd` si un PRD est fourni ; `learn.md` pointe vers `init <prd-file>` ; le reste consomme.

**(absent)** Aucun rôle PM ou analyste. Le plugin ne définit que trois agents —
`task-orchestrator.md`, `task-executor.md`, `task-checker.md` — et tous trois interviennent **en aval**,
sur l'exécution de tâches déjà générées.

### 1.2 Les deux templates, et le piège du RPG

**(code)** `scripts/init.js:938-944` copie les **deux** templates inconditionnellement, sans flag ni
question :

- `assets/example_prd.txt` (47 lignes) — six sections balisées `<context>`/`<PRD>` : `# Overview`,
  `# Core Features`, `# User Experience`, `# Technical Architecture`, `# Development Roadmap`,
  `# Logical Dependency Chain`, `# Risks and Mitigations`, `# Appendix`. Chaque section porte une
  consigne entre crochets, dont celle-ci qui vaut d'être notée — `example_prd.txt:29` :
  *"Do not think about timelines whatsoever -- all that matters is scope"*.
- `assets/example_prd_rpg.txt` (511 lignes dans sa variante `.md`) — méthode **RPG** (*Repository
  Planning Graph*, papier Microsoft Research), richement instrumentée : blocs `<instruction>`, exemples
  *good*/*bad*, sections `<functional-decomposition>`, `<structural-decomposition>`,
  `<dependency-graph>`, `<implementation-roadmap>`.

Le choix entre les deux est un **message texte** post-init (`scripts/init.js:1244`) : *"Simple projects:
Use `example_prd.txt` / Complex systems: Use `example_prd_rpg.txt`"*. Pas un branchement.

Le RPG mérite un arrêt, parce qu'il **ressemble à une compétence outillée et n'en est pas une**. Sa
discipline est réelle et bien écrite — d'abord les capacités, ensuite la structure de code, ensuite les
dépendances :

> *"Now think about CAPABILITIES (what the system DOES), not code structure yet"* (l. 64)
> *"NOW think about code organization. Map capabilities to actual file/folder structure"* (l. 131-132)
> *"This is THE CRITICAL SECTION for Task Master parsing. Define explicit dependencies between modules.
> This creates the topological order for task execution"* (l. 199-201)

Trois faits **(code)** défont la promesse de la dernière citation :

1. **Aucun code ne charge ce fichier.** Grep exhaustif de `example_prd` sur tout le dépôt : chaque
   occurrence est une copie disque→disque (`init.js:939,943`), une constante de chemin
   (`src/constants/paths.js:28-29`), ou du texte affiché à un agent lui disant d'aller le lire
   (`mcp-server/src/core/direct-functions/initialize-project.js:93`). **Jamais** un `readFile` suivi
   d'un envoi en `systemPrompt`.
2. **`parse-prd` ignore structurellement le format.** Grep `RPG`/`rpg` sur
   `scripts/modules/task-manager/parse-prd/*.js`, `src/prompts/parse-prd.json`,
   `src/schemas/parse-prd.js` → zéro résultat. Aucun `{{#if}}` conditionné au format : un PRD RPG est
   traité exactement comme un PRD simple. La section `task-master-integration` du template (l. 468-489)
   **prétend** le contraire — c'est une affirmation **(doc)** contredite par le code.
3. **Ses auteurs ne l'utilisent jamais.** Les cinq PRD réels du dépôt (`prd.txt` 528 l.,
   `prd-tm-start.txt` 90 l., `loop-prd.md` 425 l., `task-template-importing-prd.txt` 470 l.,
   `test-prd.txt` 7 l.) suivent tous le template **simple**, plus ou moins fidèlement. Aucune balise
   `<functional-decomposition>` nulle part.

> **Le piège, formulé** : un document assez richement écrit pour passer pour un prompt système, qu'aucun
> chemin d'exécution ne charge, et que ses auteurs n'ont jamais employé. Seule une lecture du code le
> distingue d'une fonctionnalité. C'est exactement le genre d'artefact que nos propres skills peuvent
> devenir si rien ne les charge.

### 1.3 L'ambiguïté est absorbée, jamais levée

**(code)** Le prompt système de `parse-prd` (`src/prompts/parse-prd.json:59`) contient l'instruction
inverse d'un gate de clarification :

> *"10. Focus on filling in any gaps left by the PRD or areas that aren't fully specified, while
> preserving all explicit requirements"*
> *"Infer title, description, details, and test strategy for each task based **only** on the PRD content."*

**(absent)** Aucune instruction du type « si l'information manque, demande » dans les prompts de
rédaction ou de parsing. Les seules clauses de clarification du dépôt vivent dans les agents
d'**exécution** — `packages/claude-code-plugin/agents/task-orchestrator.md:101,108` (*"Task Ambiguity:
Request clarification from user before proceeding"*), `task-executor.md:59` — donc après que le
découpage a déjà figé les hypothèses.

Le flag `--research` (Perplexity) **ne revient pas vers l'humain** non plus : le bloc conditionnel
(`parse-prd.json:59`, `{{#if research}}`) demande au modèle de chercher *"the latest technologies,
libraries, frameworks and best practices"* et de détecter *"technical challenges, security concerns, or
scalability issues not explicitly mentioned"* — soit **combler les trous mieux, mais toujours seul**.

Confirmation technique : `handleNonStreamingService`
(`scripts/modules/task-manager/parse-prd/parse-prd-non-streaming.js:33-40`) fait un unique
`generateObjectService(...)` et retourne le JSON. Pas de tour de dialogue, pas d'itération.

---

## 2. PRD → tâches : un seul axe, et un invariant garanti par le prompt

### 2.1 La mécanique

**(code)** `task-master parse-prd <fichier>` → `parsePRDCore`
(`scripts/modules/task-manager/parse-prd/parse-prd.js:39`) → **un seul appel** `generateObjectService`
avec schéma Zod. Un mode streaming existe mais est **mort par construction** :
`const ENABLE_STREAMING = false;` (`parse-prd-config.js:75`).

Le schéma effectivement utilisé (`parse-prd-config.js:17-42`) :

```js
export const prdSingleTaskSchema = z.object({
  id: z.number(),
  title: z.string().min(1),
  description: z.string().min(1),
  details: z.string(),
  testStrategy: z.string(),
  priority: z.enum(TASK_PRIORITY_OPTIONS),
  dependencies: z.array(z.number()),
  status: z.string()
});
```

Deux détails utiles. D'abord, **pas de champ `subtasks`** : le parsing ne produit qu'un seul niveau.
Ensuite, une précaution qui mérite d'être reprise — `src/schemas/base-schemas.js:14-17` :

> *"The `metadata` field […] is intentionally EXCLUDED from all AI schemas. This ensures AI operations
> cannot overwrite user metadata."*

**Dette repérée** : un second schéma (`src/schemas/parse-prd.js`, `ParsePRDResponseSchema`) est
référencé dans `src/schemas/registry.js` mais **branché sur aucun exécuteur** — vestige d'une couche
`tm-core` non câblée.

### 2.2 L'axe de découpage : l'ordre d'implémentation, et rien d'autre

**(code)** `src/prompts/parse-prd.json:59-60`, verbatim, la partie qui porte la logique :

> *"Analyze the provided PRD content and generate {{#if (gt numTasks 0)}}approximately
> {{numTasks}}{{else}}an appropriate number of{{/if}} top-level development tasks. If the complexity or
> the level of detail of the PRD is high, generate more tasks relative to the complexity of the PRD […]
> Guidelines:
> 2. Each task should be atomic and focused on a single responsibility […]
> 3. Order tasks logically - consider dependencies and implementation sequence
> 4. Early tasks should focus on setup, core functionality first, then advanced features
> 5. Include clear validation/testing approach for each task
> 6. Set appropriate dependency IDs (a task can only depend on tasks with lower IDs […])
> 7. Assign priority (high/medium/low) based on criticality and dependency order"*

Un seul axe est donc **prescrit** : l'ordre chronologique d'implémentation. Aucun axe fonctionnel, par
couche ou par fichier n'est imposé — c'est laissé au modèle. Sur leur propre backlog, le résultat
observé mélange couche technique et composant (« Implement Task Data Structure », « Develop CLI
Foundation », « Integrate Anthropic Claude API ») : conséquence constatée, pas prescrite.

Une clause donne au PRD le pouvoir de reprendre la main (guideline 9) : *"If the PRD contains specific
requirements for libraries, database schemas, frameworks, tech stacks […] STRICTLY ADHERE […] and do
not discard them under any circumstance"*.

> **Le point le plus élégant du système.** La contrainte « *a task can only depend on tasks with lower
> IDs* » combinée à une numérotation séquentielle rend l'**acyclicité vraie par construction** — pas
> vérifiée après coup. Un invariant obtenu par une clause de prompt plutôt que par un validateur.

### 2.3 Nombre de tâches, niveaux, dépendances

**(code)** Le nombre est un paramètre humain **optionnel à repli modèle** : `-n, --num-tasks`
(`commands.js:993`). Fourni, le prompt dit *"approximately"* — jamais *"exactly"*, donc le modèle garde
de la marge ; absent, c'est *"an appropriate number of"* et le modèle décide seul.

**(code)** **Deux niveaux, jamais trois.** `BaseTaskSchema` et `SubtaskSchema`
(`src/schemas/base-schemas.js:41-51`). Vérifié empiriquement : aucune sous-sous-tâche sur les 9 tags du
`tasks.json` réel. Le `SubtaskSchema` diffère de façon révélatrice — **pas de `priority`**, `status`
restreint, et des longueurs minimales plus dures (`title` ≥ 5 car., `description` ≥ 10,
`details` ≥ 20) absentes du niveau supérieur.

Le passage tâche→sous-tâches est un **mécanisme distinct** : `task-master expand --id=<n>`
(`expand-task.js`, prompt `src/prompts/expand-task.json`). Son prompt n'ajoute **aucun critère de
découpage** au-delà de *"specific, actionable subtasks that can be implemented sequentially"*
(`expand-task.json:81`) — même logique séquentielle, un grain plus bas. L'essentiel de ses instructions
porte sur la contiguïté des identifiants : *"MUST be sequential integers starting EXACTLY from
{{nextSubtaskId}} […] DO NOT use any other numbering pattern!"* (l. 71).

**(code)** Les dépendances sont posées **par le LLM dans le même appel**, sans passe dédiée. Une
validation déterministe existe **a posteriori et séparément** : `validate-dependencies` /
`fix-dependencies` (`scripts/modules/dependency-manager.js`), avec détection de cycle en DFS
(`isCircularDependency`, `findCycles` dans `utils.js:1468-1507`) et **réparation automatique** qui
retire les arêtes fautives (*"Breaking circular dependency: Removing […]"*). Ces deux mécanismes sont
du code, pas du modèle.

### 2.4 Le gate, et ce que produit vraiment le découpage

**(code)** Le seul gate entre PRD et tâches est **anti-écrasement de fichier**, pas de relecture :
`confirmTaskOverwrite` (`commands.js:1044-1055`) si le tag contient déjà des tâches, sinon l'erreur
*"Tag '{{targetTag}}' already contains {{n}} tasks. Use --force to overwrite or --append"*. Rien
n'empêche des tâches générées d'être écrites sans qu'un humain les ait lues.

**(code, chiffré)** Ce que ça donne sur leur propre backlog — `.taskmaster/tasks/tasks.json`, 12 071
lignes, 9 tags, **182 tâches** au total. Tag `master` (le développement réel du produit) :

| Mesure | Valeur |
|---|---|
| Tâches | 93 (57 `done`, 33 `pending`, 2 `deferred`, 1 `cancelled`) |
| Tâches avec sous-tâches | 74 |
| Sous-tâches par parent | min 1 · **moyenne 7,2** · **max 45** |
| Dépendances par tâche | min 0 · moyenne 0,73 · max 7 |
| Profondeur | 2 niveaux, aucune exception |

Deux lectures. La moyenne de 0,73 dépendance par tâche dit que le graphe est **très pauvre** — presque
une liste ordonnée, malgré tout l'appareil de détection de cycles. Et **45 sous-tâches sous un seul
parent** dit qu'aucun critère ne distingue un niveau de l'autre : le grain dérive librement, faute de
définition de ce qu'est une tâche par rapport à une sous-tâche.

---

## 3. La complexité : une note sans grille, et un seuil décoratif

### 3.1 La mécanique et l'artefact

**(code)** `task-master analyze-complexity` (`commands.js:1936`,
`scripts/modules/task-manager/analyze-task-complexity.js:48`) : filtre les tâches actives → contexte
optionnel (`ContextGatherer`) → un appel `generateObjectService` → écrit
`.taskmaster/reports/task-complexity-report[_<tag>].json`.

Schéma (`src/schemas/analyze-complexity.js:3-18`) :

```js
z.object({
  taskId: z.number().int().positive(),
  taskTitle: z.string(),
  complexityScore: z.number().min(1).max(10),
  recommendedSubtasks: z.number().int().nonnegative(),
  expansionPrompt: z.string(),
  reasoning: z.string()
}).strict();
```

### 3.2 La grille est quasi vide

**(code)** `src/prompts/analyze-complexity.json:47-48`. Tout ce qui tient lieu de critères
d'évaluation :

> *"You are an expert software architect and project manager analyzing task complexity. Your analysis
> should consider implementation effort, technical challenges, dependencies, and testing requirements."*

puis, côté `user` :

> *"Analyze the following tasks to determine their complexity (1-10 scale) and recommend the number of
> subtasks for expansion. Provide a brief reasoning and an initial expansion prompt for each."*

Le reste du prompt — la majorité de sa longueur — décrit le **format de sortie JSON**, pas les critères.
Quatre mots-clés, sans définition, sans pondération, **sans ancrage** (à quoi ressemble un 3, à quoi
ressemble un 8), sans critère opérationnel (fichiers touchés, surface d'API, nouveauté technique,
couplage). **(absent)** Aucune rubrique à dix niveaux nulle part dans le dépôt, aucun exemple few-shot.

Deux enrichissements conditionnels seulement : `hasCodebaseAnalysis` (accès Glob/Grep/Read pour ancrer
l'estimation dans le code réel — le plus substantiel) et `useResearch` (*"Consider current best
practices, common implementation patterns, and industry standards"*).

**(absent)** Effort, risque et incertitude sont **confondus** dans un chiffre unique. Le prompt mêle
*implementation effort* et *technical challenges* sans les distinguer, et rien ne sépare « je ne sais pas
combien ça coûte » de « je sais que c'est gros ».

### 3.3 Le seuil ne branche rien

**(code)** Bornes 1–10 (imposées par Zod). Seuil par défaut **5**, cohérent en trois points
(`analyze-task-complexity.js:51`, `commands.js:1948`, `mcp-server/src/tools/analyze.js:36`). Paliers
d'affichage : Low < 5, Medium 5–7, High 8–10 (`ui.js:1898-1904`).

Ce que le score fait réellement :

1. **Nombre de sous-tâches** — `expand-task.js:193-194` lit `taskAnalysis.recommendedSubtasks`, mais
   seulement si aucun `--num` explicite n'est passé, sinon repli sur `defaultSubtasks: 5`.
2. **Prompt d'expansion** — `expansionPrompt` est injecté tel quel dans le prompt d'`expand`, et
   `reasoning` reformaté en contexte additionnel (`expand-task.js:168,237-288`).
3. **`expand --all`** — `expand-all-tasks.js:92-96` filtre **uniquement** sur le statut et l'absence de
   sous-tâches. **Le seuil n'y intervient jamais.** Grep exhaustif : `thresholdScore` /
   `complexityThreshold` n'apparaissent que dans le calcul du rapport et l'affichage CLI, **jamais dans
   un `if` de gating**.

> **Le seuil est décoratif.** Un nombre configurable, documenté, affiché — qui ne déclenche rien. C'est
> le type de faux affordance le plus coûteux : l'utilisateur croit régler un comportement.

**(absent)** Aucune formule ne lie `complexityScore` à `recommendedSubtasks` : les deux sortent
indépendamment du même appel, et aucun test ne vérifie leur cohérence.

**(absent)** Aucune calibration a posteriori. Ni `actualEffort`, ni durée réelle, ni comparaison
prédit/constaté. Le seul mécanisme voisin, `scope-up`/`scope-down`
(`scripts/modules/task-manager/scope-adjustment.js:63-118`), relance `analyzeTaskComplexity` **après une
modification délibérée du périmètre** — c'est une ré-estimation, pas un retour d'expérience.

### 3.4 La nuance honnête : malgré tout, les scores discriminent

**(code, chiffré)** Sur les deux plus gros rapports réels de `.taskmaster/reports/` (9 fichiers) :

| Rapport | n | Distribution | Min–Max | Moyenne |
|---|---|---|---|---|
| `autonomous-tdd-git-workflow` (2025-10-07) | 23 | `3,4,4,5,5,5,5,6,6,6,6,6,6,6,7,7,7,7,7,8,8,8,9` | 3–9 | 6,13 |
| `loop` (2026-01-08) | 18 | `1,2,2,2,3,3,3,3,4,4,4,4,4,5,5,5,6,7` | 1–7 | 3,72 |

Chacun couvre 6–7 points sur 10, sans écrasement au centre, et les `reasoning` varient en substance —
p. ex. score 9 : *"Very high complexity due to implementing the complete TDD red-green-commit cycle with
AI integration, retry logic, timeout handling, and git operations."* contre score 3 pour une tâche de
documentation. **Un modèle non guidé produit tout de même un ordre plausible.** Ce qui manque n'est pas
la capacité de discriminer, c'est la **reproductibilité** : rien ne garantit que le même travail noté
deux fois reçoive le même chiffre, et aucune mesure ne l'établit.

---

## 4. La validation : il n'y en a aucune en code

### 4.1 Les statuts, sans machine à états

**(code)** Six valeurs (`src/constants/task-status.js:3-4`) : `pending`, `in-progress`, `review`,
`done`, `deferred`, `cancelled`. `isValidTaskStatus()` (l. 26-28) ne vérifie que **l'appartenance à la
liste**. `setTaskStatus` (`set-task-status.js:32-35`) rejette une valeur hors énum et **ne contraint
aucune transition** : `updateSingleTaskStatus` écrit `task.status = newStatus`, point.

**(code)** Qui met une tâche à `done` ? N'importe qui appelle `task-master set-status --id=<id>
--status=done` — le code ne distingue pas l'humain de l'agent. Le seul contrôle réel après coup est
`validateTaskDependencies(data.tasks)` (`set-task-status.js:126`), qui vérifie la **cohérence du
graphe**, jamais l'achèvement du travail.

### 4.2 Les gates sont de la prose

**(doc)** `packages/claude-code-plugin/commands/to-done.md:11-14` liste des « Pre-Completion Checks » :
*"Verify test strategy was followed"*, *"Check if all subtasks are complete"*, *"Validate acceptance
criteria met"*. La section « Execution » qui suit (l. 16-20) n'exécute que la commande brute, **sans
avoir vérifié quoi que ce soit**. Même structure pour `to-review.md:22-39`. Ce sont des listes à puces
lues par le modèle qui vient d'écrire le code, et dont l'échec ne bloque rien.

**(absent)** `testStrategy` n'est **jamais exécuté**. Le champ est généré par trois prompts
(`parse-prd.json:59`, `add-task.json:64`, `expand-task.json:71`) et lu par **aucun** runner, hook ou
script. Un texte à suivre de bonne foi.

**(absent)** Aucun rôle reviewer/QA outillé. `review` est une valeur d'énum comme les autres, décrite
au même titre dans `set-task-status` (`mcp-server/src/tools/set-task-status.js:40`). Aucun code ne
vérifie qui l'a écrit ni qui peut en sortir. Aucun second agent n'y est câblé.

### 4.3 Le seul quasi-gate, et il est en langage naturel

**(doc)** `.claude/commands/go/ham.md` (*Hamster Automated Management*) décrit la boucle la plus
aboutie : `tm show → set-status in-progress → implémentation → lint/typecheck → set-status done →
tm list`, avec commit et PR (l. 82-136). La phrase qui compte, l. 132 :

> *"When the subtask is done, run lint and typecheck, mark the task as done **if it passes**, and commit."*

C'est le seul endroit du dépôt où une condition d'achèvement est énoncée — et elle reste une **phrase
adressée au modèle**, pas un code qui empêcherait l'appel en cas d'échec. L'humain n'intervient qu'à
l'invocation, et une fois explicitement (l. 134, *"Confirm with the human when doing this"* pour scinder
les PR).

**(doc)** `command-pipeline.md` et `smart-workflow.md` décrivent des pipelines conditionnels
(`if:blocked-tasks-freed`, `retry:3:commit`). **(absent)** Aucune classe `Pipeline`/`Conditional`
correspondante en JS/TS : ce sont des spécifications de prompt sans moteur.

---

## 5. La passation de contexte : le mécanisme à récupérer

C'est la partie où Task Master fait quelque chose de juste, et de directement transférable.

**(code)** Le canal est le champ `details` de la tâche ou sous-tâche, dans `tasks.json`, alimenté en
**append horodaté**. `update-subtask-by-id.js:355-356` :

```js
const timestamp = new Date().toISOString();
const formattedBlock = `<info added on ${timestamp}>\n${generatedContentString.trim()}\n</info added on ${timestamp}>`;
```

Les blocs sont réellement présents dans leur `tasks.json` (p. ex. `2025-05-01T21:59:10.551Z` sur la
sous-tâche 5 du tag `master`), concaténés à l'existant — pas substitués.

**(code)** Le prompt fait de la nature *delta* une exigence explicite —
`src/prompts/update-subtask.json:63` :

> *"Based only on the user's request and all the provided context (including existing details if
> relevant to the request), GENERATE the new text content that should be added to the subtask's details.
> Focus only on generating the substance of the update. […] Return only the newly generated text content
> as a plain string. […] Your string response should NOT include any of the subtask's original details,
> unless the user's request explicitly asks to rephrase, summarize, or directly modify existing text."*

Le fichier se tague lui-même `"tags": ["update", "subtask", "append", "logging"]` (l. 9) : l'intention
assumée est **un journal, pas un état**. `update-task.json` a symétriquement un mode `append` distinct
(*"Subtask updates are always append mode"*, `update-subtask-by-id.js:103`) opposé au mode par défaut
qui réécrit intégralement.

**(code)** La règle de non-réécriture du passé va plus loin — `src/prompts/update-tasks.json:46` :

> *"7. If an existing completed subtask needs to be changed/undone based on the new context, DO NOT
> modify it directly. 8. Instead, add a new subtask that clearly indicates what needs to be changed or
> replaced"*

C'est la mécanique de remontée de blocage : plutôt que de falsifier un travail déjà `done`, on ajoute une
sous-tâche corrective. Même règle en singulier dans `update-task.json:62`. **(absent)** Aucune détection
automatique de blocage : `updatePrompt` est un paramètre obligatoire fourni par l'appelant — c'est
l'agent qui doit décider d'invoquer la cascade.

**Combien d'agents, en réalité ?** **(absent de preuve d'agents concurrents.)** Rien n'indique une
orchestration simultanée sur une même tâche. `update-subtask`/`update-task` sont des appels ponctuels
émis séquentiellement par l'agent en session, qui relit ensuite ses propres `details` via
`currentDetails`. La question « comment les agents communiquent entre eux » se réduit donc ici à :
**comment une succession de sessions s'écrit des notes horodatées à son futur soi.**

**(doc)** Les `tags` (`docs/command-reference.md:244-297`) vont dans le sens inverse d'une
communication : *"Tags provide complete isolation - tasks in different tags don't interfere with each
other"*, avec création depuis la branche git courante (`add-tag --from-branch`). C'est une **cloison**
pour travail parallèle, pas un canal.

---

## 6. L'amélioration continue : le résultat est qu'il n'y en a pas

### 6.1 Ce qui est écrit sur le disque

**(code)** 49 fichiers sous `.taskmaster/`, **aucun gitignoré** — tout est versionné, y compris
`state.json` et un `userId` codé en dur (`config.json:23`) avec des URL `localhost:3000`, traces
d'usage local du mainteneur livrées telles quelles. L'inventaire : `tasks/tasks.json` (l'état),
`reports/*.json` (9 rapports de complexité, purement *a priori*), `config.json`, `state.json`,
`docs/research/*.md` (6 transcripts de `research --save`), `templates/`, exports statiques
`tasks/task_*.txt`.

Ce sont des **données d'état** et des **archives brutes**. Aucun journal d'apprentissage.

### 6.2 Les règles sont statiques — la démonstration

**(code)** Le candidat idéal existe : `assets/rules/self_improve.mdc`, titré *"Guidelines for
continuously improving Cursor rules…"*, qui prescrit d'ajouter ou modifier une règle quand un motif
apparaît trois fois ou plus. C'est un **fichier de prompt statique livré avec l'outil**, et la preuve
est dans l'historique :

- `git log -- assets/rules/self_improve.mdc` → **un seul commit**, jamais retouché ;
- `diff assets/rules/self_improve.mdc .cursor/rules/self_improve.mdc` → **identique**, et la copie
  installée n'a elle aussi qu'un commit.

> **Même en se dogfoodant, Task Master n'a jamais appliqué sa propre règle d'auto-amélioration des
> règles.** L'installation (`src/profiles/base-profile.js:12-23` + `rule-transformer`) ne fait qu'une
> transformation de **format** par éditeur — jamais de contenu issu de l'usage.

### 6.3 Les promesses sans code

**(doc)** Les slash-commands promettent une capitalisation : `to-done.md:29` *"Update CLAUDE.md with
learnings"*, l. 44 *"Capture lessons learned"* ; `auto-implement-tasks.md:93` *"Log lessons learned"* ;
`to-cancelled.md:22` *"Document lessons learned"*. **(absent)** Vérifications : `grep -rln "velocity"`
sur `scripts/`, `src/`, `mcp-server/` → **zéro** ; aucune écriture programmatique dans `CLAUDE.md`
nulle part. Ces lignes sont des vœux adressés au modèle.

**(absent)** `docs/research/*.md` est écrit et **jamais relu** : le chemin n'apparaît dans le code
(`commands.js:2066`, `research.js:958`) que côté `saveToFile`. Aucun chemin de lecture ne le réinjecte
en contexte.

**(code)** `context/chats/` : deux exports bruts de sessions Cursor (mai 2025), 3 commits, dernier
2025-11-14, **aucune référence dans le code**. Archive morte.

### 6.4 La télémétrie mesure le coût, pas la qualité — et ne le persiste pas

**(code)** `src/telemetry/sentry.js` : erreurs et opérations IA vers Sentry (DSN en dur l. 69), opt-out
`anonymousTelemetry`. `scripts/modules/ai-services-unified.js:892-941` (`logAiUsage`) calcule
`inputTokens`, `outputTokens`, `totalCost` — mais l. 937 :

> *"TODO (Subtask 77.2): Send telemetryData securely to the external endpoint"*

Jamais implémenté. Les données ne sont que `console.log`-guées en mode debug et affichées ponctuellement
en CLI. **Rien n'est écrit sur disque, rien n'est versionné, et rien ne mesure la qualité du travail —
seulement son prix.**

### 6.5 Le dogfooding : réel, puis éteint

**(code, chiffré)** 182 tâches sur 9 tags : le projet s'est authentiquement piloté avec son propre
outil. Mais `git log -- .taskmaster` ne compte que **5 commits**, du 2025-11-14 au **2026-01-11**
(`c2d6c18`), alors que `HEAD` est du **2026-04-23**. Soit **plus de trois mois** sans que `.taskmaster/`
bouge, pendant que le dépôt continuait d'être développé. *(Réserve : clone superficiel — la fenêtre
observable est de 206 commits ; l'arrêt est net sur cette fenêtre.)*

---

## 7. Ce que ça confirme de `bmad.md`, et ce que ça y ajoute

**Confirmation.** Le constat de `bmad.md` §2.6 — *le gating de tout le corpus vérifie la forme, jamais
le fond* — tient, et Task Master en est le cas extrême : il ne vérifie même pas la forme. Aucun tiers
n'est bloquant ; l'agent qui produit est l'agent qui atteste. Deuxième confirmation, donc, que
`D-041` (le tiers prononce la conformité, l'humain la justesse) nous rend **plus stricts que l'état de
l'art** — et non en retard sur lui.

**Confirmation.** *Aucune mesure n'existe* : Task Master n'apporte, lui non plus, aucune donnée sur
l'efficacité de son propre process. La seule chose qu'il instrumente est le coût en dollars, et ce
compteur n'est même pas persisté.

**Ajout — un écart doc/code plus large que dans BMAD.** Ici il ne s'agit plus de prompts optimistes mais
d'artefacts entiers qui simulent une fonctionnalité : un template de 511 lignes qui déclare piloter le
parsing sans être chargé par une ligne de code, un seuil configurable qu'aucun `if` ne lit, un moteur de
pipeline conditionnel qui n'existe qu'en Markdown, un `TODO` de télémétrie ouvert derrière une fonction
qui a l'air de journaliser. Le corpus se lit comme s'il faisait beaucoup ; il fait peu, et bien, sur un
périmètre étroit.

**Ajout — le seul mécanisme substantiel du dossier est la passation de contexte** (§5), qui converge
avec le *story file* de BMAD V4 (`bmad.md` §3.1) tout en étant **plus simple et plus solide** : append
horodaté, delta seul, interdiction de réécrire un travail terminé.

---

## 8. Ce que ça met sur la table pour Cursus

Rien ici ne réclame de décision immédiate. Ce qui suit est ce que le dossier rend disponible.

1. **Le journal append horodaté, à reprendre presque tel quel.** Trois propriétés à retenir ensemble :
   l'écriture est un **append** jamais une réécriture ; le prompt exige explicitement de ne renvoyer que
   le **delta** ; et un travail marqué terminé ne se corrige pas, il se **complète par un correctif
   nouveau**. Ce triptyque est directement transférable à ce que notre flux tiré demande : un `Rework
   Needed` doit accumuler pourquoi, pas remplacer l'histoire. Notre `decisions.md` append-only obéit
   déjà au même principe, un cran plus haut.

2. **Un invariant par le prompt plutôt que par un validateur.** « *A task can only depend on tasks with
   lower IDs* » rend l'acyclicité vraie par construction. Le motif vaut d'être gardé en tête pour la
   génération de graphes de définition : contraindre l'espace de sortie coûte une phrase, valider après
   coup coûte un module (ils ont **les deux**, et le validateur ne trouve donc jamais rien à faire dans
   le cas nominal).

3. **Un chiffre qu'aucun branchement ne lit finit par mentir.** Le seuil de complexité à 5, documenté,
   configurable, affiché, et jamais consulté par `expand --all`, est l'avertissement le plus net du
   dossier. S'il devait exister un score chez nous, il doit **router** — sinon c'est un ornement, et
   pire, une fausse affordance de réglage.

4. **Le grain a besoin d'un juge, et ils n'en ont pas.** 93 tâches, 7,2 sous-tâches en moyenne, **45**
   sous un seul parent, 0,73 dépendance par tâche : rien dans leurs prompts ne dit ce qui distingue un
   niveau de l'autre, donc rien ne tient le grain. Notre critère — *recettable par quelqu'un qui ne lit
   pas le code* — est précisément la pièce manquante, et ce dossier en donne le contre-exemple chiffré.

5. **Une grille de notation vide produit quand même un ordre plausible, mais non reproductible.** Leurs
   scores discriminent (1→9, moyenne 3,7 et 6,1 sur deux lots) avec quatre mots-clés en guise de
   critères. À retenir dans les deux sens : ne pas surinvestir dans une rubrique élaborée pour obtenir
   un classement grossier, mais ne pas confondre « le classement a l'air juste » avec « la même chose
   notée deux fois reçoit le même chiffre » — ce second point, personne ne l'a mesuré.

6. **Le piège du template riche, appliqué à nos skills.** Le RPG est un document soigné, méthodologique,
   citant un papier de recherche — que rien ne charge et que personne n'utilise. Le test à passer sur
   chacun des 8 skills à écrire est donc mécanique : *quel chemin d'exécution le charge, et où est la
   trace qu'il a servi ?* Un skill qu'aucun chargeur n'ouvre est un document, et il faut l'appeler
   ainsi.

7. **Leur échec valide notre journal des frictions — et l'avertit.** `self_improve.mdc`, une règle
   prescrivant d'améliorer les règles, jamais modifiée en six mois par ses propres auteurs : c'est
   exactement le sort qui guette `docs/methode/journal-frictions.md` (16 entrées). Le signal de santé
   n'est pas le contenu du fichier, c'est la **date de son dernier commit** — et corollaire, si un
   apprentissage ne remonte jamais en règle chargée, le journal devient une archive. Idem pour leur
   `docs/research/` : écrit, jamais relu, jamais réinjecté.

8. **Le dogfooding mort est un indicateur, pas une anecdote.** Trois mois de commits sur le produit sans
   un seul commit sur son propre backlog. Notre pari de dogfooding se mesurera au même endroit.

---

## Sources

Toutes les affirmations proviennent du dépôt `eyaltoledano/claude-task-master`, `HEAD` = `c0c98d3`
(**2026-04-23**), clone **superficiel** (206 commits observables), lu sur disque le **2026-07-27**.

Fichiers les plus cités : `src/prompts/{parse-prd,expand-task,analyze-complexity,update-subtask,update-task,update-tasks}.json` ·
`scripts/modules/task-manager/parse-prd/*` · `scripts/modules/task-manager/{expand-task,expand-all-tasks,analyze-task-complexity,set-task-status,scope-adjustment}.js` ·
`scripts/modules/{commands,dependency-manager,ai-services-unified,ui}.js` · `scripts/init.js` ·
`src/schemas/{base-schemas,analyze-complexity,parse-prd}.js` · `src/constants/task-status.js` ·
`src/profiles/base-profile.js` · `src/telemetry/sentry.js` · `assets/{example_prd.txt,example_prd_rpg.txt,rules/self_improve.mdc}` ·
`packages/claude-code-plugin/{commands,agents}/*.md` · `.claude/commands/go/ham.md` ·
`.cursor/rules/dev_workflow.mdc` · `.taskmaster/{tasks/tasks.json,reports/*.json,templates/*,docs/*}` ·
`apps/docs/getting-started/quick-start/prd-quick.mdx` · `docs/command-reference.md`.

Documents compagnons : `docs/reference/bmad.md` (le champ des cadres de méthode) ·
`docs/reference/skills.md` (l'écriture de skills) · `docs/reference/symphony.md` (l'autre pari
tracker-as-control-plane).
