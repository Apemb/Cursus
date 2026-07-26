# Écrire des skills pour un flux agentique — état de l'art au 25 juillet 2026

> **À quoi sert ce fichier.** Il rassemble ce qu'on sait de l'écriture, de la validation et de
> l'entretien des *Agent Skills* Claude Code, en vue des huit skills que `docs/methode/flux.md`
> liste. C'est du **matériel externe sondé**, comme `linear-api.md` et `royalterminal-0.4.0.md` :
> il rapporte l'état du monde à une date, il ne décide rien pour Cursus. Les décisions qui en
> découleront iront dans `decisions.md`.
>
> **Comment le lire.** Chaque affirmation porte son étiquette :
>
> - **✅ mesuré** — étude, benchmark, ou documentation officielle qui engage son auteur ;
> - **📄 documenté** — la doc l'affirme sans le mesurer ; c'est un fait sur le produit, pas sur le monde ;
> - **⚠️ folklore** — conseil répandu, cohérent, **sans mesure publiée**. Utilisable, pas opposable.
>
> Cette distinction est le principal apport du document. La littérature sur le sujet mélange les
> trois sans prévenir, et plusieurs conseils universellement répétés se sont révélés faux (§8.3).
>
> **Comment il se périme.** Vite. Les skills ont environ un an d'existence publique, la doc bouge
> tous les mois, et trois des faits du §1 sont des changements **annoncés mais pas encore
> survenus**. Relire les §1 et §7 avant toute décision qui s'y adosse.

---

## 1. Ce qui contraint avant tout le reste

Quatre faits touchent l'architecture, pas la rédaction. Ils se tranchent avant d'écrire une ligne
de skill, parce qu'en dépendent des choix qu'on ne défait pas ensuite.

### 1.1 `pass^k` — la fiabilité par étape est la variable dominante

**✅** Anthropic distingue `pass@k` (réussir au moins une fois sur k essais) de **`pass^k`**
(réussir les k fois), et donne le repère : **un agent à 75 % de réussite par tentative a 42 % de
`pass^3`**.

Le flux de `flux.md` a **dix étapes**. Ce que ça donne, si chaque étape est indépendante :

| Fiabilité par étape | Chaîne de 3 | Chaîne de 10 |
|---|---|---|
| 75 % | 42 % | 5,6 % |
| 90 % | 73 % | 35 % |
| 95 % | 86 % | 60 % |
| 99 % | 97 % | 90 % |

Deux conséquences. D'abord, **la métrique pertinente pour un pipeline est `pass^k`**, et une
étape « qui marche à peu près » ne marche pas. Ensuite, les points d'arrêt humains (`QA Review`,
`Validation`) ne sont pas seulement des jugements irréductibles : ils **coupent la chaîne en
segments plus courts**, et c'est un argument de fiabilité qui s'ajoute à celui de `D-036`.

### 1.2 `--bare` va devenir le défaut de `claude -p`

**📄** La doc headless est explicite : sans `--bare`, `claude -p` **charge exactement le même
contexte qu'une session interactive** — hooks, skills, plugins, serveurs MCP, mémoire automatique
et `CLAUDE.md`, depuis le répertoire de travail *et* depuis `~/.claude`. Et : *« `--bare` est le
mode recommandé pour les appels scriptés et SDK, et **deviendra le défaut de `-p`** dans une
version future. »*

Ce que `--bare` change :

| | Sans `--bare` (défaut actuel) | Avec `--bare` |
|---|---|---|
| `CLAUDE.md` | chargé | **non chargé** |
| Skills du projet et de `~/.claude` | découverts | **aucun** |
| Serveurs MCP | découverts | `--mcp-config` seulement |
| Hooks | découverts | aucun |
| Authentification | OAuth + trousseau | **`ANTHROPIC_API_KEY` ou `apiKeyHelper`** |
| Reproductible entre machines | non | **oui** |

Le mode bare conserve Bash, la lecture et l'édition de fichiers. Tout le reste se charge par
`--append-system-prompt-file`, `--settings`, `--mcp-config`, `--agents`, `--plugin-dir`.

**L'enjeu pour Cursus.** `D-038` fait reposer l'architecture sur « le prompt d'un `AgentStep` est
un pointeur : quel skill, quelle carte ». Ce pointeur cesse de résoudre en bare. Deux choses en
découlent :

- un pipeline qui repose **implicitement** sur le chargement automatique cassera **silencieusement**
  le jour de la bascule — l'agent tournera, produira un artefact, et cet artefact ne respectera
  plus les conventions du projet. C'est un faux succès (§1.3) provoqué par un changement
  d'infrastructure ;
- **passer à `--bare` maintenant** et rendre les dépendances explicites coûte un inventaire ponctuel
  et rend le run lisible : ce qui est chargé est écrit dans la ligne de commande.

**📄** Ce qui continue de marcher en `-p` : **`/nom-du-skill` dans la chaîne de prompt** est
expansé par Claude Code avant l'exécution. L'invocation explicite est un chemin viable, et sans
doute plus sûr qu'un déclenchement par `description` pour un pipeline. C'est aussi la voie
officielle côté GitHub Actions, où le champ `prompt` accepte `/nom-du-skill` ou `/plugin:skill`.

### 1.3 Le faux succès — le mode de défaillance dominant

**✅** *From Confident Closing to Silent Failure* (arXiv:2606.09863, juin 2026, préprint). Le
*false success* est l'écart entre l'affirmation « c'est fait » et l'état réel de l'environnement.
Contrairement à un crash, **l'agent présente l'interaction comme résolue** et la défaillance se
propage en aval.

| Mesure | Valeur |
|---|---|
| τ²-bench (9 876 trajectoires, 8 familles de modèles frontier) | **45–48 % de tous les échecs** |
| τ²-bench, domaine à **double contrôle** (vérificateur externe indépendant) | **3 %** |
| AppWorld (vérité terrain = état de base) | **75,8 % des échecs** |
| Dispersion inter-modèles | 13 % à 89 % |
| Modèles à raisonnement | **aucune protection** (Qwen3-Max-Thinking à 79 %) |

**L'écart 45–48 % → 3 % vient de la présence d'un vérificateur externe, pas d'un meilleur
prompt.** C'est le levier de premier ordre, et il ne s'obtient pas en réécrivant un skill.

**✅ Et un juge LLM est un mauvais détecteur de faux succès** : aucune configuration (5 juges,
5 stratégies de prompt, spécification complète de la tâche) ne dépasse **0,65 AUROC** sur
τ²-bench, et **0,54** sur AppWorld — à peine mieux que le hasard. Des détecteurs **TF-IDF**
atteignent **0,83–0,95 AUROC**, tournent **3 300× plus vite**, et détectent 4 à 8× plus de faux
succès. Pour la question « l'agent a-t-il vraiment fait ce qu'il dit », **un détecteur statistique
bête bat un juge LLM**.

**Pour Cursus, la bonne nouvelle est que le vérificateur externe existe déjà** : `dotnet build`
sans warning et `dotnet test` entièrement vert *sont* l'oracle. Chaque critère qui bascule du
jugement vers la vérification retire de la surface au faux succès.

### 1.4 La précédence des skills — un piège immédiat

**📄** L'ordre est **entreprise > personnel > projet**. Un skill de `~/.claude/skills/` **écrase**
le skill commité du même nom, silencieusement.

**Constaté sur ce poste le 25 juillet 2026** : `~/.claude/skills/tdd` existe, et le dépôt n'a pas
de `.claude/skills/`. Le jour où un skill `tdd` sera commité, il sera neutralisé. À régler avant
d'écrire.

C'est aussi une faille pour l'argument de `D-038` (« la méthode se relit en revue et suit le
dépôt ») : ce qui est relu n'est pas nécessairement ce qui s'exécute.

---

## 2. Anatomie d'un skill

### 2.1 Le répertoire

Un skill est un **répertoire** dont le seul fichier obligatoire est `SKILL.md`. Trois rôles pour
les annexes, tels que le standard les nomme :

- `scripts/` — code **exécuté, jamais chargé en contexte** ; seule sa sortie coûte des tokens ;
- `references/` — documentation chargée à la demande ;
- `assets/` — gabarits, données.

**📄** Emplacements : `.claude/skills/<nom>/SKILL.md` (projet), `~/.claude/skills/` (personnel),
`<plugin>/skills/` (plugin). Découverte **ascendante** depuis le répertoire de démarrage jusqu'à
la racine du dépôt ; découverte **descendante à la demande** quand Claude touche un fichier d'un
sous-répertoire (nom qualifié par chemin : `apps/web:deploy`). Détection à chaud : éditer un
`SKILL.md` prend effet sans redémarrer.

**📄 Les slash commands ont fusionné dans les skills.** `.claude/commands/x.md` et
`.claude/skills/x/SKILL.md` produisent tous deux `/x` et fonctionnent pareil. Il n'y a plus
d'arbitrage à faire ; `.claude/commands/` est un format hérité.

### 2.2 Le frontmatter — deux régimes qu'il ne faut pas confondre

**Le standard ouvert** (`agentskills.io/specification`) définit **6 champs** : `name` (≤ 64 car.,
minuscules/chiffres/tirets, **doit correspondre au nom du répertoire**), `description`
(≤ 1 024 car.), `license`, `compatibility`, `metadata`, `allowed-tools`.

**Claude Code en accepte 17**, tous optionnels. Les sept qui comptent pour nous :

| Champ | Effet |
|---|---|
| `description` | Ce que voit le modèle **en permanence**. Si omis, prend le premier paragraphe du corps |
| `when_to_use` | Phrases-déclencheurs additionnelles. **Propre à Claude Code**, absent du standard |
| `disable-model-invocation` | `true` → **retire la description du contexte permanent** ; seul `/nom` invoque |
| `allowed-tools` / `disallowed-tools` | Concession de permission pour le tour / retrait du pool |
| `model`, `effort` | Modèle et effort de raisonnement pour le reste du tour |
| `context: fork` | Exécution dans un sous-agent isolé |
| `paths` | Globs limitant l'activation automatique |

**📄 Piège de nommage.** Le standard exige que `name` corresponde au répertoire ; Claude Code
traite `name` comme un simple libellé d'affichage et **prend la commande du nom de répertoire**.
Aucune source ne dit ce qui se passe quand ils divergent. La seule position sûre est de les garder
identiques.

### 2.3 Le chargement progressif, et ses budgets

**📄** Trois niveaux, nommés explicitement *progressive disclosure* :

| Niveau | Quand | Coût | Contenu |
|---|---|---|---|
| 1 — métadonnées | **toujours**, au démarrage | ~100 tokens/skill | `name` + `description` |
| 2 — instructions | au déclenchement | < 5 000 tokens visés | corps du `SKILL.md` |
| 3 — ressources | à la lecture | nul jusque-là | annexes ; les scripts n'entrent qu'en sortie |

**Les budgets documentés**, éparpillés sur quatre pages officielles :

| Contrainte | Valeur | Nature |
|---|---|---|
| `description` | 1 024 caractères | **dure** (validation) |
| `description` + `when_to_use` dans le listing | tronqué à **1 536** caractères | troncature silencieuse |
| Corps du `SKILL.md` | **< 500 lignes**, < 5 000 tokens | recommandation |
| Listing complet des skills | **1 % de la fenêtre de contexte** | réglable |
| Après auto-compaction | **5 000 premiers tokens** par skill, budget commun 25 000 | mécanique |
| Profondeur des références | **un seul niveau** | recommandation forte |

**📄 Le débordement du listing est le piège le plus vicieux** : *« le listing contient toujours
chaque nom de skill, mais s'il y en a beaucoup, Claude Code **raccourcit les descriptions** pour
tenir dans le budget, ce qui peut **retirer les mots-clés dont Claude a besoin** pour apparier
votre demande. […] Il supprime les descriptions **en commençant par les skills que vous invoquez
le moins**. »* Diagnostic : `/doctor`, la ligne « Skills » de `/context`, et un avertissement sous
`--debug`.

**Ce qui périme la limite des 500 lignes.** Après une compaction, seuls les **5 000 premiers
tokens** de chaque skill sont réattachés. Un skill de 490 lignes n'y survit pas. Les tailles
observées dans les corpus sérieux confirment une cible bien plus basse :

| Corpus | Médiane du `SKILL.md` |
|---|---|
| `mattpocock/skills` | **4,6 ko** |
| `obra/superpowers` | 6,9 ko |
| `anthropics/knowledge-work-plugins` (212 skills) | 5,4 ko |
| `deanpeters/Product-Manager-Skills` (70 skills) | **15,4 ko** |

Un corpus dont la **médiane** est à 15 ko a un problème systémique, pas quelques aberrations.

### 2.4 Le corps reste en contexte, et n'est jamais relu

**📄** *« Quand vous ou Claude invoquez un skill, le contenu rendu du `SKILL.md` entre dans la
conversation comme un seul message et **y reste pour le reste de la session**. […] Claude Code
**ne relit pas le fichier aux tours suivants**, donc écrivez ce qui doit s'appliquer tout au long
d'une tâche comme des **instructions permanentes plutôt que des étapes ponctuelles**. »*

C'est la contrainte de rédaction la plus structurante, et la plus contre-intuitive : le réflexe
naturel — une procédure numérotée qu'on consomme — est le mauvais registre.

---

## 3. Écrire la description

Elle est payée **à chaque requête de chaque session**, y compris quand le skill ne sert pas. Le
corps n'est payé qu'une fois déclenché. Ce sont deux économies opposées, donc deux styles
d'écriture dans un même fichier.

**📄 La forme canonique** est `[verbe : ce que ça fait] + « Use when » [déclencheurs]` :

```yaml
description: Extract text and tables from PDF files, fill forms, merge documents.
  Use when working with PDF files or when the user mentions PDFs, forms, or document extraction.
```

**📄 Troisième personne pour le *quoi*, impératif pour le *quand*.** La doc plateforme dit
« toujours à la troisième personne » ; la page de calibrage dit « formule à l'impératif ». Ce
n'est pas une contradiction : ce qui est proscrit des deux côtés, c'est la première et la deuxième
personne (« I can help you… », « You can use this to… »).

**📄 Une branche par déclencheur, jamais deux synonymes.** Le contre-exemple observé dans le
corpus officiel non-codage : `Trigger with "recruiting update", "candidate pipeline", "how many
candidates", "hiring status"` — quatre formules pour **la même** branche. Elles gonflent la
description sans ajouter de couverture, et les guillemets créent une fausse impression de contrat
alors que l'appariement est sémantique.

**📄 Écrire pour ne PAS se déclencher.** *« Si des requêtes qui ne devraient pas déclencher le
font, la description est trop large. **Ajoutez de la spécificité sur ce que le skill ne fait
pas**, ou clarifiez la frontière avec les capacités voisines. »*

**📄 Un skill que le modèle sait déjà faire ne se déclenchera pas**, quelle que soit la qualité de
la description : *« une demande simple en une étape comme "lis ce PDF" peut ne pas déclencher un
skill PDF même si la description correspond parfaitement, parce que l'agent sait le faire avec ses
outils de base. »*

**📄 La méthode de calibrage officielle** : ~20 requêtes (8–10 `should_trigger: true`, 8–10
`false`), **3 exécutions chacune**, seuil de déclenchement à 0,5, **split train 60 % / validation
40 %** pour éviter le sur-apprentissage. Les négatifs utiles sont les **quasi-collisions** ; les
négatifs évidents ne testent rien. Mise en garde : ne pas ajouter les mots-clés des requêtes
échouées — c'est du sur-apprentissage ; trouver la catégorie qu'elles représentent.

**✅** *SkillResolve-Bench* (arXiv:2606.10388) mesure exactement le risque qui nous attend avec
huit skills : l'**ambiguïté de capacité** entre skills dont les descriptions se recouvrent.

---

## 4. Écrire le corps

### 4.1 Le principe directeur

**📄** *« La fenêtre de contexte est un bien public. […] **Hypothèse par défaut : Claude est déjà
très intelligent.** N'ajoutez que le contexte qu'il n'a pas. Interrogez chaque élément : "Claude
a-t-il vraiment besoin de cette explication ?" "Puis-je supposer qu'il sait cela ?" **"Ce
paragraphe justifie-t-il son coût en tokens ?"** »*

Le test de coupe, en une question : **« l'agent se tromperait-il sans cette instruction ? »** Si
non, couper. Le vocabulaire de Pocock nomme le défaut : un **no-op** est une ligne qui ne change
rien par rapport au comportement par défaut, et qui coûte quand même. Son exemple : *relentless*
fonctionne (**leading word** — un mot déjà chargé au pré-entraînement qui ancre une région de
comportement pour un token) ; *be thorough* est un no-op.

L'anti-patron correspondant, observé jusque dans le corpus officiel d'Anthropic : le skill
**« fiche encyclopédique »** — un tableau de définitions et une liste de métriques, **aucune
instruction, aucune étape, aucun critère d'arrêt, aucune sortie attendue**. Le modèle connaît
déjà le contenu ; le skill ne change rien.

### 4.2 Calibrer la liberté, partie par partie

**📄** L'analogie officielle : *pont étroit avec des falaises des deux côtés* (une seule voie sûre
→ instructions exactes) contre *champ ouvert sans danger* (plusieurs chemins mènent au but →
direction générale, on fait confiance). **« La plupart des skills sont un mélange. Calibrez chaque
partie indépendamment. »**

**📄** Et l'avertissement de sur-contrainte : les auteurs *« penchent vers la sur-contrainte, parce
que des instructions rigides paraissent plus sûres. Elles ne le sont pas ; elles échouent
autrement »* — la lettre suivie mais le cas limite manqué, ou la règle **sur-appliquée** là où il
fallait un jugement. Pour les parties à liberté haute, **expliquer le pourquoi bat une directive
rigide**.

### 4.3 Les patrons qui reviennent dans les corpus sérieux

| Patron | Quand | Observé chez |
|---|---|---|
| **Section « Gotchas »** | Toujours. *« Le contenu à plus fort signal de n'importe quel skill »* | Anthropic, obra |
| **Tableau à trois colonnes** *revendication → preuve exigée → preuve insuffisante* | Vérification | obra |
| **Checklist numérotée convertie en todos** | Opérations multi-étapes | obra |
| **Porte dure** typographiée (`<HARD-GATE>`, « The Iron Law ») | Point de non-retour | obra |
| **Découplage poignée / noyau** | Plusieurs entrées dans un même geste | Pocock |
| **Adaptateur par variante** (`issue-tracker-{github,gitlab,local}.md`) | Variabilité d'environnement | Pocock |
| **Dégradation gracieuse** sur outil absent | Dépendance externe optionnelle | Anthropic |
| **Section « When NOT to use this »** | Frontière avec les skills voisins | rare |

Le patron **Gotchas** mérite sa citation, parce qu'il définit ce qui y entre : *« ce ne sont pas
des conseils généraux ("gérez les erreurs correctement") mais des **corrections concrètes à des
erreurs que l'agent commettra sans qu'on le lui dise**. […] **Gardez les gotchas dans le
`SKILL.md`**, où l'agent les lit avant de rencontrer la situation. Un fichier de référence séparé
marche si vous lui dites quand le charger, mais pour un problème non évident, **l'agent peut ne
pas reconnaître le déclencheur**. »* Et la boucle d'entretien qui va avec : *« quand un agent
commet une erreur que vous devez corriger, ajoutez la correction à la section gotchas. »*

Le patron **découplage poignée / noyau**, tel qu'observé, tient en trois fichiers : un skill
`grilling` porte la doctrine (~1 ko) ; deux skills de **147 et 245 octets** y entrent
différemment, l'un nu, l'autre composé avec un second skill. Les poignées portent
`disable-model-invocation: true`, donc **leur description sort du contexte permanent**. Ajouter
une variante coûte sept mots.

### 4.4 Les interdits documentés

**📄** Ce qu'Anthropic dit de ne pas mettre :

1. **De l'information datée** (« si vous faites ceci avant août 2025… ») → une section
   « Old patterns » repliée dans un `<details>` ;
2. **De la terminologie flottante** — un seul terme par concept ;
3. **Des chemins Windows** — toujours des barres obliques ;
4. **Un menu d'options** (« vous pouvez utiliser A, ou B, ou C… ») → un défaut avec une porte de sortie ;
5. **Des références imbriquées** (SKILL.md → a.md → b.md) : *« Claude peut utiliser `head -100`
   pour prévisualiser plutôt que lire les fichiers entiers, **d'où une information incomplète** »* ;
6. **Des constantes injustifiées** (`TIMEOUT = 47  # pourquoi 47 ?`) : *« si vous ne connaissez pas
   la bonne valeur, comment Claude la déterminerait-il ? »* ;
7. **Des scripts qui délèguent l'erreur au modèle** — « résous, ne diffère pas » ;
8. **Un outil MCP sans son préfixe de serveur** ;
9. **Une réponse plutôt qu'une méthode** : *« un skill doit enseigner **comment aborder** une
   classe de problèmes, pas **quoi produire** pour une instance. »*

### 4.5 Autoriser explicitement l'abandon

**✅** *Check Yourself Before You Wreck Yourself* (arXiv:2510.16492, oct. 2025, rév. juin 2026),
12 modèles, framework ToolEmu. Une instruction explicite d'abandon en cas d'incertitude :

- **+0,39 en sûreté** (échelle 0–3), **+0,64** sur les modèles propriétaires ;
- **−0,03** en complétion de tâche. Négligeable.

C'est l'un des très rares résultats à la fois mesuré, quasi gratuit, et transposable en une ligne.

**Et il comble un trou spécifique au headless.** **📄** L'outil `AskUserQuestion` est **retiré de
tout sous-agent**, sans possibilité de le rétablir, et refusé sous le mode de permission
`dontAsk`. L'agent **ne peut pas demander**. Sans autorisation d'abandon, un agent bloqué n'a que
deux sorties : inventer, ou déclarer fini. **Les deux sont des faux succès.**

### 4.6 Le nombre de règles, pas leur polarité

**✅** *IFScale* (arXiv:2507.11538, juillet 2025) : jusqu'à 500 instructions simultanées, 20
modèles, 7 fournisseurs. Les meilleurs modèles frontier plafonnent à **68 % de précision à 500
instructions**, avec dégradation continue et **biais de primauté** — ce qui est tôt dans le prompt
est mieux suivi.

**Conséquence directe sur le geste réflexe** : dans un document qui grossit par ajouts en fin de
fichier, **les règles les plus récentes — celles ajoutées après l'incident du jour — sont
structurellement les moins bien suivies.** L'ajout ne marche pas, ce qui pousse à ajouter encore.

**⚠️ En revanche, la supériorité des instructions positives sur les négatives n'est pas établie.**
Le chiffre qui circule (« reformuler les interdits en affirmations réduit les violations de
moitié ») n'a **aucune méthode publiée**. La seule mesure directe trouvée (arXiv:2603.26830, mars
2026) ne conclut à **aucune différence significative**. Les deux corpus de référence sont en
désaccord frontal sur ce point — l'un empile `<EXTREMELY-IMPORTANT>` et les capitales, l'autre
écrit *« la négation se retourne contre vous »*.

Ce qui tient, c'est autre chose : **une interdiction sans alternative laisse l'espace des actions
sous-déterminé**. Et surtout, **le problème est cardinal, pas grammatical** — reformuler trente
interdits en trente affirmations ne réglerait rien.

---

## 5. Les boucles de revue

C'est la section qui porte les étapes 6 (`Plan Review`) et 8 (`Code Review`) de `flux.md`. La
conception naïve — « je mets un relecteur, il relit, on itère » — est celle que les données
servent le plus mal.

### 5.1 Sans oracle, la boucle ne crée pas d'information

**✅** *Large Language Models Cannot Self-Correct Reasoning Yet* (ICLR 2024, DeepMind) : sans
retour externe, les LLM peinent à s'auto-corriger, et **parfois leurs performances se dégradent
après auto-correction**.

Rien ne garantit qu'un second appel apporte l'information que le premier n'avait pas. Le levier
n'est ni « plus de tours » ni « un meilleur modèle » : c'est **fabriquer un oracle**. Rendre le
critère d'acceptation d'un plan aussi mécaniquement vérifiable que possible — le schéma de sortie
est rempli, les fichiers cités existent, la test list couvre chaque assertion, le diff annoncé
correspond au diff réel.

### 5.2 Le contexte séparé bat la répétition — et c'est mesuré

**✅** *Cross-Context Review* (arXiv:2603.12123, mars 2026, **préprint mono-auteur, N = 30
artefacts, 150 erreurs injectées** — à lire avec la prudence que commande cet échantillon, mais le
protocole isole exactement la bonne variable).

| Condition | Dispositif | F1 | Précision | Rappel |
|---|---|---|---|---|
| **CCR** | Session neuve, **artefact seul** | **28,6 %** | 31,5 % | 27,1 % |
| **SR** | Relecture dans la même session | 24,6 % | 25,8 % | 24,2 % |
| **SA** | Session neuve **avec le prompt de génération** | 23,8 % | 27,4 % | 21,8 % |
| **SR2** | Relecture **répétée, contexte identique** | 21,7 % | 21,0 % | 22,7 % |

Trois lectures :

1. **CCR bat toutes les baselines** (p < 0,01), et l'écart se creuse sur les **erreurs critiques**
   (**+11 points**) alors qu'il converge sur les erreurs mineures.
2. **SR2 n'apporte rien sur SR** (p = 0,11) et fait même moins bien. *« La répétition seule
   n'aide pas — c'est la séparation de contexte qui produit le bénéfice. »*
3. **SA fait moins bien que CCR**, et la seule différence est que SA reçoit le prompt de
   génération. **Donner au relecteur l'intention de l'auteur l'ancre.**

Le point 3 mérite attention : SA correspond au comportement **par défaut** du harnais (un
sous-agent reçoit une description de tâche composée par le parent). CCR est un cran plus loin.

**⚠️→✅ Corollaire sur la complaisance** : l'anonymat de la paternité fait presque disparaître le
biais d'auto-préférence. CCR — artefact seul, sans le prompt de génération — réalise une
anonymisation naturelle. Ce n'est probablement pas un effet secondaire du protocole, c'est une
partie de la raison pour laquelle il gagne.

### 5.3 Deux à trois tours, et pas plus

**✅** *Multi-Agent Debate with Adaptive Stability Detection* (arXiv:2510.12697, préprint) :

- dès le **tour 2**, les distributions basculent vers un motif **bimodal** — les agents soit
  s'alignent complètement, soit **échouent collectivement**. La boucle ne converge pas *vers la
  vérité*, elle converge *vers un accord*, et l'accord peut être faux ;
- stabilisation en **2 à 7 tours** selon le benchmark ; perte d'exactitude de l'arrêt adaptatif
  par rapport au débat complet : **−0,67 % à 0,00 %**. Les tours au-delà de la stabilisation ne
  rapportent rien ;
- **le taux de consensus fallacieux ne décroît pas** : minimum au tour 2 (**3,9 %**), puis
  **remontée à 5,1 % au tour 5**.

**Un plafond de tours n'est donc pas une mesure d'économie, c'est une mesure de qualité.** La
« deux ou trois boucles avant escalade » de `tickets.md` §6.4 tombe pile dans la zone utile — et
la raison de s'arrêter est meilleure que celle qu'on croyait : au-delà, on paie des tokens pour
augmenter la probabilité d'un accord faux.

### 5.4 Répliquer un relecteur ne sert à rien ; diversifier sert, mais moins qu'on croit

**✅** *Nine Judges, Two Effective Votes* (arXiv:2605.29800, mai 2026, préprint) :

| Mesure | Valeur |
|---|---|
| Indépendance effective d'un panel de 9 juges | **2,18 votes** (24,2 %) |
| Corrélation moyenne inter-juges | φ = 0,391 |
| Panel de 9 contre le meilleur juge seul | **72,0 % contre 71,8 %** |
| Valeur marginale des juges 6 à 9 | +0,22 vote effectif |
| Part de l'indépendance captée par les 5 premiers | **90 %** |

*« Si trois juges non corrélés sont d'accord, l'erreur jointe est faible — mais seulement s'ils
sont en désaccord sur les bonnes choses. S'ils sont d'accord sur tout, vous avez acheté un verdict
trois fois. »*

**Ce qui doit différer, c'est le raisonnement, pas l'étiquette.** Changer le nom du rôle dans le
prompt ne décorrèle pas les erreurs. Les trois leviers dont un effet est mesuré : changer de
**modèle**, changer de **contexte** (CCR), changer de **dimension d'évaluation**.

**✅** Sur ce dernier point : *Quantifying and Mitigating Self-Preference Bias* (arXiv:2604.22891,
avril 2026, 20 modèles) — une **grille structurée multi-dimensionnelle** réduit le biais
d'auto-préférence de **31,5 %**. Et un constat contre-intuitif du même papier : *« les capacités
avancées sont souvent non corrélées, voire négativement corrélées, avec un faible biais
d'auto-préférence »* — **monter en gamme de modèle ne règle pas le problème.**

**✅** Argument négatif complémentaire : *MAST* (arXiv:2503.13657, UC Berkeley, 7 frameworks,
200+ tâches, κ = 0,88) recense les **rôles d'agents en doublon** parmi ses 14 modes de
défaillance. Deux relecteurs sans mandat distinct ne sont pas deux relectures.

### 5.5 Le relecteur chicanier est le régime nominal

**✅** La **précision** de la relecture plafonne à **31,5 %** dans la meilleure condition testée —
sur un corpus où les erreurs avaient été **délibérément injectées**. Autrement dit, **environ deux
tiers des signalements sont des faux positifs**, même dans le meilleur protocole.

**⚠️** Le motif de mitigation qui circule — exiger du relecteur un **niveau de confiance et une
sévérité par constat**, et **filtrer en aval plutôt qu'en amont** — est de la guidance produit
cohérente, non mesurée. Le raisonnement : un modèle qui suit littéralement « ne signale que le
critique » supprime des constats réels et fait chuter le rappel, même quand sa capacité de
détection a progressé.

### 5.6 Discussion ≠ décomposition

**✅** *Rethinking the Bounds of LLM Reasoning* (arXiv:2402.18272) : *« un LLM en agent unique avec
des prompts forts peut atteindre une performance similaire à la meilleure approche de discussion
existante »* — et la discussion multi-agents ne l'emporte **que lorsqu'il n'y a pas de
démonstration dans le prompt**.

**✅** Anthropic, à l'inverse, mesure un système orchestrateur-travailleurs **+90,2 %** contre un
agent unique — mais à **~15× le coût en tokens** d'un chat.

Les deux mesurent des choses différentes : le premier teste la **discussion** (plusieurs agents
délibérant sur le même objet), le second la **décomposition** (sous-espaces disjoints, synthèse).
**Une relecture est une délibération, pas une décomposition** — c'est donc le premier résultat qui
s'y applique. La couverture s'achète en multi-agents ; le jugement, non.

### 5.7 La forme de sortie d'un verdict

**✅** Ce qui dégrade le raisonnement n'est pas le format, c'est **de contraindre pendant la
délibération**. CRANE (**ICML 2025**) le montre théoriquement — une grammaire restrictive réduit
la classe de complexité accessible — et pratiquement : laisser des zones libres pour le
raisonnement intermédiaire récupère et dépasse (**jusqu'à +10 points**).

Une ablation à **structure retardée** (raisonner librement, reformater ensuite) **récupère 80–87 %
de la perte**. Et le coût dépend de la marge de capacité :

| Modèle, MATH-Hard | CoT | JSON | Écart |
|---|---|---|---|
| Sonnet 4.6 | 89,3 % | 88,7 % | neutre |
| **Haiku 4.5** | 88,7 % | 52,5 % | **−36,2 pts** |
| Opus 4.7 (AIME) | 96,2 % | 91,0 % | −5,2 pts |

**📄 Confirmation officielle du mécanisme** : avec `tool_choice: any` ou `tool`, *« l'API
préremplit le message assistant pour forcer l'usage d'un outil. Les modèles **n'émettront aucune
réponse ou explication en langage naturel avant les blocs `tool_use`, même si on le leur demande
explicitement**. »* Contournement documenté : garder `tool_choice: auto` et demander l'outil dans
le message utilisateur.

**✅ Effet secondaire à connaître** : la sortie structurée **réduit la diversité des réponses**
quand plusieurs sont valides — fréquence de la réponse modale 41 % → 64 %, réponses distinctes
52 → 36 (arXiv:2607.18476, préprint). Mauvais pour `discovery` qui doit ouvrir des pistes, bon
pour `revue-plan` qui doit rendre un verdict reproductible. **La même discipline ne convient pas
aux deux familles de skills.**

**✅ Et la forme ne garantit jamais le fond** : le *Structured Output Benchmark* mesure une
conformité de schéma quasi parfaite pour une **exactitude de valeur plafonnant à 83 %**. Un
artefact bien formé n'est pas un artefact juste — c'est le faux succès du §1.3 vu sous un autre
angle.

---

## 6. Ce que voit un agent non-interactif

**📄** Faits documentés qui touchent directement la conception d'un `AgentStep`.

**Un sous-agent démarre sur un contexte vierge.** *« Il ne voit ni votre historique de
conversation, ni les skills déjà invoqués, ni les fichiers que Claude a déjà lus. »* Son contexte
initial : son prompt système, la description de tâche composée par le parent, `CLAUDE.md` (sauf
pour les agents intégrés `Explore` et `Plan`), et un **instantané du `git status` pris au début de
la session parente** — donc potentiellement périmé.

D'où l'avertissement officiel, qui est exactement le piège d'un pipeline « skill + carte » :
*« **Si une règle doit atteindre le sous-agent, par exemple "ignore le répertoire `vendor/`",
restituez-la dans le prompt que vous donnez à Claude au moment de déléguer.** »*

**Le champ `skills:` injecte le contenu complet au démarrage**, pas seulement la description —
donc **la divulgation progressive ne s'applique pas** dans un sous-agent préchargé. Un `SKILL.md`
de 490 lignes coûte 490 lignes dès la première seconde. Un skill destiné au préchargement se
découpe autrement qu'un skill destiné à la session principale.

**La fenêtre de contexte d'un sous-agent est celle de son propre modèle**, pas celle du parent.

**Un sous-agent en arrière-plan perd la plupart des outils intégrés**, et *« le retrait ne remonte
aucune erreur »* sauf s'il ne reste rien. La même définition peut résoudre vers des outils
différents au premier plan et en arrière-plan.

**`isolation: worktree`** — le sous-agent travaille dans un git worktree temporaire, sur une copie
isolée. **C'est le seul mécanisme qui rende le hors-périmètre structurellement impossible plutôt
que détectable après coup.** Les autres (`allowedTools`, modes de permission) réduisent la
surface ; celui-ci change la nature du problème.

**Le SDK ne fait pas de décodage contraint.** Il *« valide la sortie contre le schéma, en
re-promptant en cas de non-correspondance »* — une boucle valider-puis-réessayer, pas la garantie
de `strict: true`. Deux modes d'échec, dont le second est vicieux :
`error_max_structured_output_retries` (visible), et **`subtype: "success"` sans
`structured_output`** — que la doc dit explicitement de traiter comme un échec. *Un pipeline qui
teste `subtype == "success"` avale silencieusement un artefact absent.*

**Le contenu d'une carte de ticket est du texte non fiable.** Claude Code scanne le rapport final
de chaque sous-agent et **préfixe un marqueur** quand il imite une balise de contrôle ou mentionne
des réglages de permission — il ne retire ni ne reformule rien. La doc borne elle-même la portée :
*« ce n'est pas un substitut à restreindre ce qu'un sous-agent peut atteindre. »* Dans un flux où
chaque étape lit un ticket et passe un artefact à la suivante, cette frontière est franchie dix
fois.

**Bornes mécaniques disponibles** : `--max-turns` (défaut **10** dans la GitHub Action), `maxTurns`
en frontmatter de sous-agent, et surtout **`task_budget`** (bêta) — dont l'intérêt est que **le
modèle voit le compte à rebours** et se ménage, là où `max_tokens` lui coupe la parole et tronque
la sortie. Pour une étape dont l'artefact doit être exploitable, `task_budget` est la bonne borne
et `max_tokens` est le filet.

**Observabilité** : `--output-format stream-json` ; les messages de sous-agents portent
`parent_tool_use_id` ; l'événement `system/init` rapporte modèle, outils, MCP et plugins chargés —
**ses champs `plugins` / `plugin_errors` permettent de faire échouer la CI quand un plugin n'a pas
chargé**, mode de défaillance silencieux typique du headless.

---

## 7. Valider les skills, et les faire vivre

### 7.1 Ce que font réellement les équipes en production

**✅** *Measuring Agents in Production* (arXiv:2512.04123, **accepté ICML 2026 en oral**), 20
études de cas + 86 praticiens sur 26 domaines :

- **74 % dépendent principalement de l'évaluation humaine** ;
- **68 % des agents exécutent au plus 10 étapes** avant qu'une intervention humaine soit nécessaire ;
- la fiabilité reste le défi numéro un, traité par des approches **simples et contrôlables**,
  en concevant au niveau système plutôt qu'en empilant des techniques sophistiquées.

**Un projet solo qui décide de regarder plutôt que de construire un harnais n'est pas en retard
sur l'état de l'art — il est dessus.**

### 7.2 L'outil officiel existe depuis mars 2026

**📄** Le plugin `skill-creator` a gagné des modes **Eval / Improve / Benchmark** le 3 mars 2026.
Format **versionné dans le dépôt, à côté du `SKILL.md`** :

```
<skill>/evals/evals.json
{ "skill_name": "...", "evals": [{ "id": 1, "prompt": "...",
    "files": ["evals/files/…"], "expectations": ["…", "…"] }] }
```

Mécanique : pour chaque cas, **deux sous-agents en parallèle**, `with_skill` et `without_skill`,
en contextes isolés ; un *grader* note contre les `expectations` ; un ***comparator* fait de l'A/B
en aveugle entre deux versions**. Cela répond à deux questions distinctes qu'on confond toujours :

| Dispositif | Question |
|---|---|
| `with_skill` contre `without_skill` | **Ce skill sert-il à quelque chose ?** |
| *comparator* v1 contre v2, en aveugle | **Ai-je dégradé mon skill ?** |

Sorties : `grading.json` (expectations passées + evidence, pass_rate, tool_calls), `timing.json`,
`benchmark.json` — taux de réussite **± écart-type** sur N exécutions, l'écart-type étant
précisément la mesure de flakiness.

**Trois limites, sans complaisance :**

- **pas de CLI documentée** — l'invocation est conversationnelle, donc **pas d'équivalent
  `promptfoo eval` en CI** sans bricolage ;
- **le piège 100 %/100 %** : sur des cas trop faciles, avec et sans skill marquent pareil et le
  signal est nul. C'est le mode d'échec numéro un, et le remède est de **partir d'échecs réels**,
  où le cas discrimine par construction ;
- **le coût** : chaque cas est exécuté deux fois par un agent complet. 5 runs × 8 cas = **80
  exécutions d'agent**. Ce n'est pas un test unitaire, c'est un **rituel de jalon**.

### 7.3 Le golden set — taille et provenance

**📄** Anthropic, *Demystifying evals for AI agents* (9 janvier 2026) : *« **20 à 50 tâches
simples tirées d'échecs réels** est un excellent début. »* Deux mots comptent : **`échecs réels`**
(pas de génération synthétique de cas plausibles) et **20-50** (l'ordre de grandeur d'une test
list, pas d'un corpus).

À l'échelle d'un skill unique, la doc descend à **au moins trois évaluations**, testées sur Haiku,
Sonnet et Opus. Et pour un skill, **la moitié du risque n'est pas « il fait mal la chose » mais
« il se déclenche quand il ne devrait pas »** — le golden set doit contenir des prompts qui ne
doivent **pas** l'activer.

**📄 La séquence prescrite renverse l'ordre naturel** : identifier les trous en faisant tourner
Claude **sans** le skill → construire les scénarios qui les testent → mesurer la ligne de base →
écrire **juste assez** pour les passer → itérer. *« Créez les évaluations AVANT d'écrire une
documentation extensive. »*

**Le corollaire n'est écrit nulle part mais se déduit mécaniquement** : si l'évaluation qui a
justifié une règle **passe désormais sans elle**, la règle sort. Dans le vocabulaire de ce dépôt :
*une règle sans test rouge qui la réclame n'a pas de raison d'entrer, et une règle dont le test
passe au vert sans elle n'a plus de raison de rester.*

### 7.4 Le juge LLM est un détecteur de divergence, pas un instrument de mesure

**✅** Trois biais établis : **position** (le plus facile à corriger), **verbosité**, et
**auto-préférence** — ce dernier étant pile la configuration « Claude jugeant du Claude »
(arXiv:2410.21819, NeurIPS 2024 ; le papier reconnaît qu'il **manque des métriques fiables pour
le quantifier**).

Et le chiffre partout cité — « GPT-4 juge atteint ~80 % d'accord avec l'humain » — vient de
MT-Bench, date de **2023**, et porte sur des **réponses conversationnelles courtes**, pas sur des
trajectoires d'agent. Le transposer à « un juge peut noter si mon skill de TDD a été correctement
appliqué » est une extrapolation non fondée.

Le protocole de calibration canonique (100-300 traces, 2-3 annotateurs humains, kappa de Cohen)
est **hors de portée d'un projet solo**. Donc, franchement : **ne demandez jamais « ce skill
est-il bon ? »** (score non calibré, sans signification) mais **« la sortie B s'écarte-t-elle de
A, et en quoi ? »**, en aveugle et dans les deux ordres. La **permutation de position** (coût ×2)
est la seule atténuation au ratio franchement favorable.

### 7.5 Les signaux du flux — le meilleur ratio disponible

**✅** *Signals: Trajectory Sampling and Triage* (arXiv:2604.00356, avril 2026). Sept signaux
calculés **sans aucun appel de modèle**, en trois familles : **interaction** (désalignement,
stagnation, désengagement), **exécution** (défaillance, **boucles**), **environnement**
(épuisement). Ils s'attachent aux trajectoires comme attributs structurés **pour trier lesquelles
méritent un examen**.

Résultat sur τ-bench : **82 % d'informativité contre 74 % pour du filtrage heuristique et 54 %
pour de l'échantillonnage aléatoire** — gain de 1,52× par trajectoire informative.

Le dispositif n'est **pas** « remplacer le banc de test par des métriques d'usage ». C'est
**utiliser des signaux gratuits et déterministes pour décider quelles trajectoires méritent une
attention humaine**. On reste le juge ; les signaux évitent de lire au hasard.

**✅** *Agent Arena* (juin 2026) nomme cinq signaux opérationnels : *confirmed success*, *praise vs
complaint*, ***steerability*** (capacité à exécuter les corrections demandées), ***bash
recovery*** (nombre de tentatives pour se remettre d'une erreur), *tool hallucination*.

| | Banc de test synthétique | Signaux d'usage |
|---|---|---|
| Coût de construction | élevé | **quasi nul** |
| Coût marginal par vérification | tokens × cas × répétitions | **zéro** |
| Flakiness | structurelle | **aucune** (ce sont des compteurs) |
| Représentativité | cas imaginés | **usage réel** |
| Se périme | oui (cas trop faciles) | non |
| Bloque une régression | oui, si le cas la couvre | **non — rétrospectif** |

Le seul désavantage réel est la latence : le signal n'arrête pas une PR, il apprend que la semaine
a été mauvaise. Pour un projet solo où l'auteur du skill est aussi son unique utilisateur, le
ratio reste écrasant.

**✅ Et pour la question « l'agent a-t-il vraiment fait ce qu'il dit »**, le §1.3 tranche : un
détecteur **TF-IDF** (0,83–0,95 AUROC, 3 300× plus rapide) bat un juge LLM (0,54–0,65).

### 7.6 Le linter structurel

**⚠️→📄** `pulser eval` (mars 2026) : CLI npm sans dépendance qui **analyse statiquement** les
fichiers de skill — parsing du frontmatter, champs requis, qualité de la description, structure,
références croisées. **N'exécute jamais le skill** : zéro token, zéro non-déterminisme, < 200 ms
pour 40+ skills.

C'est un **linter, pas un eval**. Mais il attrape la classe de panne la plus fréquente et la plus
sournoise : **un `name` ou une `description` défaillante rend le skill invisible — sans erreur,
sans warning, sans trace.** Dans un pipeline, un skill qui ne se déclenche pas est indiscernable
d'un skill absent.

Coût : ~1 h (l'outil, ou quarante lignes maison). C'est le seul étage qui satisfasse le trilemme
de la CI — bon marché, rapide, et jamais flaky, parce qu'il ne mesure rien de statistique.

### 7.7 Le trilemme de la CI, et le régime qui en découle

**⚠️** Formulation praticien, non mesurée mais structurellement juste : *« une eval en CI sur
chaque PR doit être bon marché, rapide et statistiquement significative. Choisissez-en deux et le
gate est du théâtre. »*

Le régime qui en découle, et qui est le consensus :

| Cadence | Contenu |
|---|---|
| **Chaque commit** | Vérifications déterministes seulement — lint, structure, assertions de code |
| **La nuit** | Passe complète au juge, contre le jeu versionné |
| **Au jalon** | Benchmark comparatif, gating statistique |

**Sur les seuils** : ne pas exiger « score > 0,85 » mais **« pas de régression au-delà d'une
tolérance par rapport à la baseline »**. C'est la seule forme de seuil qui résiste au bruit.

### 7.8 La dérive dans le temps

**✅** Une étude longitudinale (PLOS One, 2 février 2026) sur **dix semaines** confirme une
**dérive comportementale significative** sur des services transformeur déployés. Deux nuances des
auteurs eux-mêmes : portée restreinte (trois familles de modèles), et **attribution impossible** —
les fournisseurs ne publient ni journaux de mise à jour ni détails d'entraînement, donc *« toute
attribution d'une dégradation observée serait purement spéculative »*. On constate la dérive, on
ne l'explique pas.

**Décision à prendre avant d'en avoir besoin** : loguer systématiquement **version du modèle,
horodatage, paramètres de décodage** avec chaque résultat. Sans ces métadonnées, aucune
comparaison n'est reproductible, et on ne saura pas distinguer « mon skill a régressé » de « le
modèle a changé sous lui ». C'est du logging, ça ne coûte rien.

**C'est le seul moment où un banc de test paie franchement sa construction** : un changement de
modèle est rare, brutal, et affecte les huit skills à la fois. **📄** Anthropic formule l'argument
en coût d'opportunité : les équipes sans evals affrontent *« des semaines de tests »* à chaque
nouveau modèle, celles qui en ont *« basculent en quelques jours »*.

### 7.9 Entretenir le corpus

**✅** *Evaluating AGENTS.md* (arXiv:2602.11988, ETH Zürich, février 2026) : *« fournir des
fichiers de contexte n'améliore pas généralement les taux de réussite, tout en augmentant le coût
d'inférence de plus de 20 % »*. Fichiers générés par IA : **−3 %** de réussite. Écrits par un
développeur : **+4 %**. Surconsommation de tokens de raisonnement : +14 à +22 %.

Une seconde étude (arXiv:2601.20404, 124 PR, 10 dépôts) mesure **−28,6 % de temps** et **−16,6 %
de tokens de sortie** à taux de complétion comparable.

**Les deux se réconcilient : un corpus d'instructions achète de la prévisibilité, pas de la
compétence.** Et le détail qui décide de ce qu'on écrit : **les instructions sont suivies ; les
présentations générales du dépôt, pourtant universellement recommandées, ne servent à rien.**

Recommandation des auteurs : **partir d'un fichier vide et ajouter les règles une par une, sur
erreurs répétées observées.**

**Les critères de sortie d'une règle**, du plus solide au plus faible :

| Critère | Étiquette |
|---|---|
| L'éval qui a justifié la règle passe désormais sans elle | déduit de §7.3 |
| Ce que la CI fait déjà respecter (lint, format, types) n'a rien à faire dans le corpus | **📄** |
| **Le fichier annexe que l'agent ne lit jamais** — mesurable par instrumentation | **📄** |
| Les règles se contredisent, ou le fichier dépasse son budget | ⚠️ |

Le troisième mérite d'être souligné : *« si Claude n'accède jamais à un fichier fourni, il est
peut-être inutile ou mal signalé »*. **C'est le seul critère d'élagage mesurable plutôt que
jugé** — et un moteur qui voit ce que ses étapes lisent le produit gratuitement.

**📄 Le coût marginal d'un skill de plus** : au démarrage, Claude Code construit un listing de
**tous** les skills disponibles. *« Chaque skill enregistré ajoute un peu au contexte du
modèle »* — donc chaque skill ajouté taxe **toutes** les invocations, y compris celles qui ne
l'utiliseront jamais.

**📄 Détection de règle périmée, industrialisée** : Claude Code Review lit le `CLAUDE.md` du dépôt
et fonctionne **dans les deux sens** — *« si votre PR change le code d'une manière qui rend une
affirmation du `CLAUDE.md` périmée, Claude signale que la doc doit être mise à jour aussi. »*
C'est la seule détection automatique de règle morte trouvée par cette recherche.

Et l'aveu de dilution, de l'éditeur lui-même : *« Parce que `REVIEW.md` est injecté en priorité
la plus haute, ces règles atterrissent **plus fiablement que les mêmes règles dans un long
`CLAUDE.md`**. […] **La longueur a un coût : un long `REVIEW.md` dilue les règles qui comptent le
plus.** »*

### 7.10 Le contexte se dégrade par falaises, pas linéairement

**✅** *Context Rot* (Chroma Research, juillet 2025), 18 modèles frontier : la fiabilité décroît
avec la longueur d'entrée **même sur des tâches triviales**, et la dégradation est **non
uniforme** — les modèles **rencontrent des falaises**. Certains tiennent à 32K et s'effondrent à
64K. Facteurs modulants : **similarité sémantique** entre l'information cherchée et la question
(moins elle est élevée, plus la dégradation accélère), présence de distracteurs.

**📄** Anthropic attribue la limite à l'architecture — la complexité en n² des relations
token-à-token. **Ce n'est donc pas un défaut qu'un modèle plus gros corrigera.**

C'est l'argument mesuré contre l'intuition « un ticket très détaillé vaut mieux qu'un ticket
court ». Et la non-linéarité a une conséquence désagréable : **un pipeline peut fonctionner
pendant des mois puis s'effondrer d'un coup** quand les cartes franchissent un seuil qu'on n'avait
pas mesuré.

---

## 8. Ce que la recherche ne dit pas

Ces trous sont des résultats. Ils dispensent de chercher, et ils désignent ce qu'il faudra
trancher seul.

### 8.1 Trous documentaires (produit)

1. **La langue.** *Aucune* source ne dit un mot sur l'écriture d'un skill dans une autre langue
   que l'anglais ; tous les exemples, sans exception, sont anglophones. Pour un dépôt dont la
   convention est le français, c'est une question directe.
2. **`when_to_use` hors Claude Code** — ignoré silencieusement, ou erreur de validation ? Non dit.
3. **`name` divergent du répertoire** — avertissement ou silence ? Non dit.
4. **Combien de skills, c'est trop ?** Le budget (1 %) et le comportement de débordement sont
   documentés ; **aucun ordre de grandeur** n'est donné.
5. **Arbitrage entre deux descriptions qui se recouvrent.** Le problème est reconnu, aucune règle
   de départage n'est décrite.
6. **`model`, `effort`, `context` via le SDK.** La doc dit que `allowed-tools` est ignoré ; elle
   est **muette** sur les autres.
7. **`@path` dans un corps de `SKILL.md`.** Une page officielle l'affirme, deux autres ne le
   mentionnent jamais et ne décrivent que des liens markdown lus à la demande. **Sémantiques
   opposées, divergence entre pages officielles** — à vérifier empiriquement.
8. **Aucun outil de validation officiel.** Il n'existe pas de `claude skills validate`.

### 8.2 Trous de recherche

1. **Aucune évaluation contrôlée de la qualité d'une discovery ou d'une spec produite par agent.**
   Le domaine n'a pas d'équivalent à SWE-bench. Il n'y a pas de science, il n'y a que des
   praticiens.
2. **Aucune mesure sur le cas « l'artefact est un plan, pas du code ».** Tous les benchmarks cités
   ont un oracle — état de base, tests, compilateur, étiquette. Aucun ne mesure la fiabilité d'une
   boucle dont la sortie est un document de conception. **C'est exactement notre cas, et c'est le
   moins couvert.**
3. **Aucune mesure d'oscillation** — l'agent qui défait au tour N+1 ce qu'il a fait au tour N.
4. **Aucune comparaison directe** entre « relecteurs à lentilles distinctes » et « un relecteur
   avec une grille multi-dimensionnelle ». La seconde a une mesure (−31,5 %), la première non.
5. **Aucune mesure de l'efficacité des gates anti-convergence** en discovery.
6. **Aucun budget de questionnement justifié** — les chiffres publiés (un tour de contestation,
   4 à 7 questions guidées) sont des choix d'auteur.
7. **Aucun récit longitudinal** d'un corpus d'instructions qui a gonflé puis été élagué. Beaucoup
   de prescriptions, aucun journal de bord.
8. **Aucun traitement du cas « règle écrite pour un défaut d'un modèle plus ancien »** — le plus
   insidieux.
9. **Aucune métrique standardisée** pour « allers-retours avant acceptation » ou « taux de reprise
   humaine ». Les concepts existent, la nomenclature n'est pas figée : **il faudra nommer les
   siennes.**

### 8.3 Ce qui circule et qui est faux

- **« Les instructions négatives sont moins efficaces que les positives »** — le chiffre « −50 % de
  violations » n'a aucune méthode publiée, et la seule mesure directe ne conclut à aucune
  différence significative (§4.6).
- **« 0,0 % à 17,1 % de hors-périmètre selon la formulation du prompt »** — ce chiffre est
  attribué à arXiv:2606.26924 ; **il n'y figure pas**, et sa source primaire est introuvable.
- **« GPT-4 juge à ~80 % d'accord humain »** — vrai en 2023, sur des réponses conversationnelles
  courtes ; **non transposable** aux trajectoires d'agent (§7.4).
- **`obra/superpowers-skills`** — encore cité partout comme « le dépôt communautaire », **mort
  depuis octobre 2025**. Vérifier la date du dernier push et l'existence de fichiers `SKILL.md`
  avant de citer un corpus coûte deux appels d'API.

---

## 9. Corpus à lire

**Les trois fichiers qui valent le détour**, dans l'ordre :

| Fichier | Pourquoi |
|---|---|
| [`writing-great-skills`](https://github.com/mattpocock/skills/blob/main/skills/productivity/writing-great-skills/SKILL.md) | Le vocabulaire : *context load*, *leading word*, *completion criterion*, *no-op*, *sediment*, *sprawl* |
| [`wayfinder`](https://github.com/mattpocock/skills/blob/main/skills/engineering/wayfinder/SKILL.md) | Planifier plus gros qu'une session d'agent ; tickets = décisions ; brouillard de guerre ; HITL/AFK |
| [`brainstorming`](https://github.com/obra/superpowers/blob/main/skills/brainstorming/SKILL.md) | La discovery gatée : 9 étapes, porte dure, 2-3 approches obligatoires, auto-revue en 4 passes |

**Les corpus**, avec leur caractère :

| Corpus | Volume | Caractère |
|---|---|---|
| [`mattpocock/skills`](https://github.com/mattpocock/skills) | 40 skills, médiane 4,6 ko | Petits et composables. Porte `deprecated/`, `in-progress/`, `.out-of-scope/` et **des ADR numérotés** |
| [`obra/superpowers`](https://github.com/obra/superpowers) | 14 skills, médiane 6,9 ko | Méthodologie complète. **Le seul corpus qui teste ses skills** (`run-haiku-test.sh`, `analyze-token-usage.py`) |
| [`anthropics/skills`](https://github.com/anthropics/skills) | 17 skills | La vitrine, plus le `skill-creator` et le gabarit canonique (140 octets) |
| [`anthropics/knowledge-work-plugins`](https://github.com/anthropics/knowledge-work-plugins) | **212 skills** | Le plus gros corpus non-codage. Meilleure ingénierie (dégradation gracieuse, `CONNECTORS.md`), diluée par beaucoup de référence générique |
| [`deanpeters/Product-Manager-Skills`](https://github.com/deanpeters/Product-Manager-Skills) | 70 skills, médiane 15,4 ko | Le canon PM. Fond solide, exécution la plus lourde. Licence NOASSERTION |

**Trois skills de méthode directement voisins de notre flux** :
[`to-spec`](https://github.com/mattpocock/skills/blob/main/skills/engineering/to-spec/SKILL.md)
(PRD **sans interview**, par synthèse de la conversation),
[`to-tickets`](https://github.com/mattpocock/skills/blob/main/skills/engineering/to-tickets/SKILL.md)
(découpage en *tracer bullets*, quiz sur la granularité, **le refacto large traité comme
l'exception nommée**), et
[`write-spec`](https://github.com/anthropics/knowledge-work-plugins/blob/main/product-management/skills/write-spec/SKILL.md)
(dégradation gracieuse sur outils absents).

**Ce qui n'existe nulle part** : aucun corpus ne traite le **ticket comme un artefact destiné à
être consommé par un agent** — écrit pour être le brief unique de quelqu'un qui n'a pas eu la
conversation. `to-tickets` s'en approche (`ready-for-agent`, « agent-grabbable par construction »)
et `wayfinder` aussi (« dimensionné à une session d'agent de 100K tokens », réclamation par
assignation). Mais **personne ne théorise la distinction de registre entre niveaux d'artefact, ni
ne fait dépendre le découpage d'un juge extérieur au code.** La thèse de `D-036` n'a pas
d'équivalent public.

---

## 10. Ce qui reste à trancher pour Cursus

Aucune de ces questions n'a de réponse dans la littérature. Elles vont dans `decisions.md`, pas
ici.

1. **Le régime d'exécution.** Passer à `--bare` maintenant et rendre les dépendances explicites,
   ou subir la bascule ? (§1.2) Et si `--bare`, par quel canal `CLAUDE.md` atteint-il l'agent —
   `--append-system-prompt-file`, ou recopié dans le skill ?
2. **Le grain.** Huit skills, ou moins ? `revue-plan` et `revue-code` partagent un squelette ; le
   patron **poignée/noyau** (§4.3) permet un noyau `revue` et deux poignées de dix lignes.
3. **Qui parle à Linear.** Un skill qui déplace la carte, ou un skill qui produit un artefact que
   l'étape suivante range ? Le patron **adaptateur** (§4.3) isole la variabilité de tracker.
4. **La langue.** Trou documentaire total (§8.1). La coupe naturelle serait `description` en
   anglais — seul texte en concurrence sémantique avec les descriptions anglophones du listing —
   et corps en français. **Hypothèse, à éprouver.**
5. **Le nom `tdd`.** Collision avec le skill personnel existant (§1.4).
6. **La forme du verdict de revue.** Structure retardée (raisonner puis formater, §5.7), sortie
   typée avec confiance et sévérité par constat (§5.5), filtrage en aval.
7. **Les compteurs à nommer.** Tours avant convergence, taux d'escalade, tentatives avant vert.
   Personne ne les a normalisés (§8.2.9) ; le moteur les produit déjà.

---

## Sources

**Documentation officielle** — [Agent Skills Specification](https://agentskills.io/specification) ·
[Skill authoring best practices](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices) ·
[Agent Skills overview](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/overview) ·
[Extend Claude with skills](https://code.claude.com/docs/en/skills) ·
[Run Claude Code programmatically](https://code.claude.com/docs/en/headless) ·
[Create custom subagents](https://code.claude.com/docs/en/sub-agents) ·
[Claude Code GitHub Actions](https://code.claude.com/docs/en/github-actions) ·
[Structured output (Agent SDK)](https://code.claude.com/docs/en/agent-sdk/structured-outputs) ·
[Extend Claude Code — feature comparison](https://code.claude.com/docs/en/features-overview) ·
[Optimizing skill descriptions](https://agentskills.io/skill-creation/optimizing-descriptions) ·
[Best practices for skill creators](https://agentskills.io/skill-creation/best-practices)

**Écrits Anthropic datés** — [Effective context engineering](https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents) (29 sept. 2025) ·
[How we built our multi-agent research system](https://www.anthropic.com/engineering/multi-agent-research-system) (13 juin 2025) ·
[Demystifying evals for AI agents](https://www.anthropic.com/engineering/demystifying-evals-for-ai-agents) (9 janv. 2026) ·
[Improving skill-creator](https://claude.com/blog/improving-skill-creator-test-measure-and-refine-agent-skills) (3 mars 2026) ·
[Steering Claude Code](https://claude.com/blog/steering-claude-code-skills-hooks-rules-subagents-and-more) (18 juin 2026) ·
[Lessons from building Claude Code: how we use skills](https://claude.com/blog/lessons-from-building-claude-code-how-we-use-skills) (3 juin 2026)

**Fiabilité et faux succès** — [From Confident Closing to Silent Failure](https://arxiv.org/abs/2606.09863) (juin 2026, préprint) ·
[Check Yourself Before You Wreck Yourself](https://arxiv.org/abs/2510.16492) (oct. 2025, rév. juin 2026) ·
[Why Do Multi-Agent LLM Systems Fail? (MAST)](https://arxiv.org/abs/2503.13657) (UC Berkeley, mars 2025) ·
[A Deterministic Control Plane for LLM Coding Agents](https://arxiv.org/abs/2606.26924) (juin 2026, préprint)

**Boucles de revue** — [LLMs Cannot Self-Correct Reasoning Yet](https://arxiv.org/abs/2310.01798) (**ICLR 2024**, DeepMind) ·
[Cross-Context Review](https://arxiv.org/abs/2603.12123) (mars 2026, préprint, N=30) ·
[Multi-Agent Debate with Adaptive Stability Detection](https://arxiv.org/abs/2510.12697) (préprint) ·
[Nine Judges, Two Effective Votes](https://arxiv.org/abs/2605.29800) (mai 2026, préprint) ·
[LLM Evaluators Recognize and Favor Their Own Generations](https://arxiv.org/abs/2404.13076) (**NeurIPS 2024**) ·
[Quantifying and Mitigating Self-Preference Bias](https://arxiv.org/abs/2604.22891) (avril 2026, préprint) ·
[Rethinking the Bounds of LLM Reasoning](https://arxiv.org/abs/2402.18272) (fév. 2024) ·
[Who Flips?](https://arxiv.org/abs/2606.16011) (préprint)

**Sortie structurée** — [CRANE](https://arxiv.org/abs/2502.09061) (**ICML 2025**) ·
[The Format Tax](https://arxiv.org/abs/2604.03616) (avril 2026, préprint) ·
[Capacity, Not Format](https://arxiv.org/abs/2606.09410) (juin 2026, préprint) ·
[Structured Output Benchmark](https://arxiv.org/abs/2604.25359) (avril 2026, préprint) ·
[Structured Output Collapses Answer Diversity](https://arxiv.org/abs/2607.18476) (juillet 2026, préprint)

**Évaluation et signaux** — [Measuring Agents in Production](https://arxiv.org/abs/2512.04123) (**ICML 2026 oral**) ·
[Signals: Trajectory Sampling and Triage](https://arxiv.org/abs/2604.00356) (avril 2026) ·
[Agent Arena](https://arena.ai/blog/agent-arena-methodology/) (4 juin 2026) ·
[Self-Preference Bias in LLM-as-a-Judge](https://arxiv.org/abs/2410.21819) ·
[SkillResolve-Bench](https://arxiv.org/abs/2606.10388) ·
[pulser eval](https://dev.to/thestack_ai/testing-claude-code-skills-in-ci-pulser-eval-github-action-3na9) (30 mars 2026) ·
[promptfoo](https://www.promptfoo.dev/) (racheté par OpenAI, 9 mars 2026)

**Corpus d'instructions et contexte** — [Evaluating AGENTS.md](https://arxiv.org/abs/2602.11988) (ETH Zürich, fév. 2026) ·
[arXiv:2601.20404](https://arxiv.org/abs/2601.20404) (124 PR) ·
[How Many Instructions Can LLMs Follow at Once? (IFScale)](https://arxiv.org/abs/2507.11538) (juillet 2025) ·
[A Regression Framework for Prompt Component Impact](https://arxiv.org/abs/2603.26830) (mars 2026, préprint) ·
[Context Rot](https://www.trychroma.com/research/context-rot) (Chroma, juillet 2025) ·
[Dérive comportementale longitudinale](https://journals.plos.org/plosone/article?id=10.1371/journal.pone.0339920) (PLOS One, 2 fév. 2026)

**Travail produit** — [From Customer Interviews to an OST with AI](https://www.producttalk.org/ai-opportunity-solution-trees/) (Teresa Torres, 18 fév. 2026 — **ne publie aucun prompt**) ·
[Ask don't tell: Reducing sycophancy](https://arxiv.org/abs/2602.23971) (fév. 2026) ·
[Four agents Atlassian's PMs use](https://sherifmansour.medium.com/four-agents-atlassians-product-managers-use-to-improve-their-product-discovery-workflows-2201b99b68af) (3 juillet 2025 — chiffres d'éditeur)

**Dates de fin de service à noter** — Workbench Anthropic, onglet *Evaluate* : **17 août 2026**.
OpenAI Evals : lecture seule au 31 octobre, **arrêt le 30 novembre 2026**.
