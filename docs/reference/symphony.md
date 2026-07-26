# Symphony (OpenAI) — le tableau comme control plane, sans méthode

> **Pourquoi ce fichier.** Symphony fait le même pari que Cursus — un tracker qui pilote des
> agents de code — et le résout autrement. Le connaître évite deux erreurs symétriques : croire
> qu'on invente ce qui existe, et copier une simplicité qui a un coût caché. Sondé le
> **26 juillet 2026**.
>
> **Fiabilité des sources.** Le `SPEC.md` a été **lu intégralement** ; c'est la source de tout ce
> qui est affirmé ici sur le fonctionnement. L'annonce officielle d'OpenAI a renvoyé **403** et
> n'a pas pu être lue — la motivation et les chiffres viennent de la couverture presse, et le
> chiffre le plus cité (**+500 % de PR atterries en trois semaines** chez certaines équipes
> OpenAI) ne provient que de blogs tiers, jamais audité. La date exacte de publication est
> incertaine : InfoQ couvre le **17 mai 2026**, d'autres résumés parlent de mars 2026.

---

## 1. Ce que c'est

Un **spec** (`SPEC.md`, Apache 2.0) accompagné d'une **implémentation de référence en Elixir**.
Un service long-running qui interroge le tableau — Linear —, crée un workspace isolé par issue,
y lance un agent Codex, et le laisse tourner jusqu'à la pull request.

La motivation est un goulot d'attention, pas de calcul : au-delà de **trois à cinq sessions Codex
simultanées**, un ingénieur ne suit plus — le changement de contexte devient douloureux, la
mémoire du fil se dégrade, les agents bloqués passent inaperçus. Le tableau devient donc le
*control plane*, et l'humain passe de **superviseur de sessions** à **gestionnaire de travail**.

Effet de bord rapporté et intéressant : le PM et le designer de l'équipe se sont mis à déposer
des demandes directement sur le tableau et à recevoir en retour des paquets de revue incluant des
captures vidéo de la fonctionnalité dans le produit.

---

## 2. Le découpage : un seul niveau, et Symphony ne découpe pas

> *« Le nom `Issue` est générique dans cette spécification ; un adaptateur PEUT le mapper depuis
> un ticket, une carte, un élément de projet, ou un autre objet de travail natif du
> fournisseur. »*

**Une issue = un agent = un workspace = une PR.** Aucun epic, aucune sous-tâche, aucune
composition hiérarchique. Le spec n'offre **aucun mécanisme de découpage ni de recomposition** :
il lit les issues éligibles (`fetch_issues_by_states`) et dispatche. Le découpage est
**entièrement externe, humain, en amont**.

Ce qui tient lieu de flux n'est pas méthodologique mais **machinal** — des états de processus,
pas de raisonnement :

| Registre | États |
|---|---|
| Réclamation (5) | `Unclaimed` · `Claimed` · `Running` · `RetryQueued` · `Released` |
| Tentative (11) | `PreparingWorkspace` → `BuildingPrompt` → `LaunchingAgentProcess` → `InitializingSession` → `StreamingTurn` → `Finishing` → `Succeeded` \| `Failed` \| `TimedOut` \| `Stalled` \| `CanceledByReconciliation` |

**Aucune revue n'est imposée.** Le spec dit seulement qu'un run réussi *« peut se terminer à un
état de handoff défini par le workflow, par exemple `Human Review`, pas nécessairement `Done` »*,
et que la politique d'approbation est *« définie par l'implémentation »*, laquelle **doit la
documenter**.

---

## 3. La méthode : un fichier, un prompt, aucun skill

Le seul artefact de méthode versionné est **un `WORKFLOW.md` par dépôt** :

- **front matter YAML** — tracker, polling, workspace, hooks, agent, codex, concurrence ;
- **corps markdown** — **un unique gabarit de prompt**, rendu par issue.

> *« The workflow file is expected to be repository-owned and version-controlled. »*
> *« Dynamic reload is REQUIRED : le logiciel DOIT détecter les changements de `WORKFLOW.md`, les
> relire et les réappliquer sans redémarrage. »*

Les variables du gabarit sont l'issue normalisée (13 champs : `id`, `identifier`, `title`,
`description`, `priority`, `state`, `branch_name`, `url`, `assignee_id`, `labels[]`,
`blocked_by[]`, `native_ref`, `dispatchable`) plus le numéro de tentative. Rendu en **vérification
stricte** des variables et des filtres.

**Ce que le spec ne contient pas, et ne mentionne jamais** : skills, `AGENTS.md`, bibliothèque de
prompts, prompts composables, séparation entre prompt-de-politique et prompt-de-tâche. Un
fichier, un prompt, tout dedans.

C'est cohérent avec l'écosystème — les skills sont une convention Anthropic, Symphony orchestre
du Codex. Mais c'est aussi, exactement, le régime que `skills.md` §7.9 identifie comme celui qui
gonfle : un document unique qui accumule, avec le biais de primauté qui rend les derniers ajouts
les moins suivis.

---

## 4. La mécanique de reprise — ce qu'il y a à prendre

C'est la partie la plus aboutie du spec, et elle traite des problèmes que `RunTrigger.ForTask`
rencontrera.

| Situation | Traitement |
|---|---|
| Sortie **propre** du worker | Retry de **continuation** après un délai fixe court (**1 s**), `attempt = 1` |
| Sortie **anormale** | Backoff exponentiel `min(10000 × 2^(attempt−1), max_retry_backoff_ms)`, défaut plafonné à **5 min** |
| **Inactivité** > `stall_timeout_ms` (défaut 5 min) | Le worker est tué, un retry est mis en file |
| Avant chaque re-dispatch | **Re-lecture de l'issue** : terminale → nettoyage du workspace et libération ; active → re-dispatch si un slot est libre ; sinon → libération |
| Entre deux tours | Re-vérification du tracker ; plus active → sortie |
| Workspace | **Réutilisé** entre runs pour la même issue ; un run réussi ne le supprime pas |

Et la règle d'économie de contexte, qui mérite d'être reprise telle quelle :

> *« Le premier tour DEVRAIT utiliser le prompt de tâche complet. Les tours de continuation
> DEVRAIENT n'envoyer que la consigne de continuation au fil existant, **pas renvoyer le prompt
> original déjà présent dans l'historique**. »*

La distinction sortie-propre / sortie-anormale est le point fin : elles ne veulent pas dire la
même chose, donc elles n'appellent pas le même délai. Une seule politique de retry les
confondrait.

---

## 5. Ce que ça dit de Cursus

### Ce que Symphony valide

- **Le tableau comme control plane.** Le pari n'est pas isolé : OpenAI l'a fait, à son échelle.
- **La méthode comme donnée du projet** (`D-038`). `WORKFLOW.md` *repository-owned,
  version-controlled*, rechargé à chaud — le même argument, formulé indépendamment.
- **La cible daemon** (`trajectoire.md` §Plus loin, `D-033` pressenti). Symphony *est* un service
  résident qui poll et dispatche : preuve d'existence de la forme visée.
- **Le handoff non terminal.** `Human Review` comme état de sortie légitime d'un run réussi —
  c'est la `QA Review` de `flux.md`, sous un autre nom.

### Où les deux divergent

| | Symphony | Cursus |
|---|---|---|
| Niveaux | **1** (issue) | **3** — feature / incrément / pas (`D-036`) |
| Découpage | externe, humain, hors système | étape du flux, portée par un skill |
| Méthode | **un** `WORKFLOW.md` par dépôt | **huit** skills (`flux.md` §4) |
| Artefacts | aucun | spec · plan d'archi · test list |
| Échec persistant | **retry** puis abandon sur timeout | **escalade** par assignation après 2–3 tours |
| Revue | non imposée, définie par l'implémentation | boucle agent ⇄ agent + deux rendez-vous humains |

**Sur l'échec, la divergence n'est pas cosmétique.** Symphony réessaie ; Cursus escalade. Vu ce
que mesure `skills.md` §5.3 — le taux de consensus fallacieux qui **remonte** avec les tours —
l'escalade a le meilleur appui empirique. Le retry de Symphony convient à une panne
d'infrastructure, pas à un désaccord de fond.

### La question honnête

Symphony est **beaucoup plus simple** — un niveau au lieu de trois, un prompt au lieu de huit
skills, aucune boucle de revue — et revendique des gains massifs. Est-ce que le dispositif de
Cursus achète quelque chose, ou est-ce de la cérémonie ?

La réponse tient dans ce que Symphony dit de lui-même : **il déplace le goulot, il ne le supprime
pas.** La couverture presse est explicite — les développeurs *« conservent une responsabilité
cruciale : examiner ces problèmes avant que Symphony ne les assigne pour exécution »*, et le
bénéfice principal est la réduction du **coût d'erreur**, lequel consiste surtout à **jeter du
travail terminé**.

Autrement dit : **quand la machine ne découpe pas, la qualité du ticket humain devient le facteur
limitant.** C'est précisément l'objet de `tickets.md` et de `D-036`, que Symphony laisse
entièrement à la charge de l'équipe. Les deux ne sont donc pas concurrents mais superposés —
dans le vocabulaire de `D-038`, **Symphony remplit la case *chorégraphie* et laisse la case
*méthode* vide.**

---

## Sources

[openai/symphony](https://github.com/openai/symphony) ·
[`SPEC.md`](https://github.com/openai/symphony/blob/main/SPEC.md) (**lu intégralement**) ·
[Annonce OpenAI](https://openai.com/index/open-source-codex-orchestration-symphony/) (**403, non
lue**) · [InfoQ, 17 mai 2026](https://www.infoq.com/news/2026/05/openai-symphony-agents/) ·
[Tessl](https://tessl.io/blog/openai-open-sources-symphony-a-spec-for-orchestrating-codex-agents/) ·
[DevOps.com](https://devops.com/openai-debuts-symphony-to-orchestrate-coding-agents-at-scale/) ·
[MindStudio](https://www.mindstudio.ai/blog/openai-symphony-spec-linear-agent-control-plane-500-percent-pr-increase)
(source unique du chiffre des +500 %, non audité)
