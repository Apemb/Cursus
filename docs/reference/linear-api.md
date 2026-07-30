# API Linear — référence sondée

> **Pourquoi ce document existe.** Le client Linear (jambe 2·2b·2) doit se poser sur la forme
> **réelle** de l'API, pas sur ce qu'on en suppose. Tout ce qui suit a été obtenu **par sonde**
> (le 2026-07-25) contre l'espace `cursus-app`, avec une *Personal API key*. Sans ce fichier, la
> connaissance vivrait hors du dépôt et se reperdrait au premier doute.
>
> ⚠️ **Aucun secret ici.** Le jeton vit dans le trousseau (`ISecretStore`, `D-033`), jamais dans le
> dépôt. Les exemples ci-dessous supposent qu'on l'a sous la main.

---

## 1. Comment re-sonder

Un seul endpoint, GraphQL, en POST :

```bash
curl -s -X POST https://api.linear.app/graphql \
  -H "Content-Type: application/json" \
  -H "Authorization: $LINEAR_TOKEN" \
  -d '{"query":"{ viewer { name } organization { name urlKey } }"}'
```

⚠️ **Le jeton se passe brut**, sans préfixe `Bearer` — c'est une *Personal API key*. (Un jeton OAuth,
lui, prendrait `Bearer`. On ne vise pas OAuth : Cursus est un outil de dev mono-utilisateur.)

⚠️ **L'introspection, elle, ne demande aucun jeton** (mesuré le 2026-07-27). Le même endpoint rend le
schéma sans en-tête `Authorization` — c'est de loin la façon la moins chère de vérifier qu'un champ
existe **avant** d'écrire du code contre lui, et elle ne consomme aucun budget de complexité :

```bash
curl -s -X POST https://api.linear.app/graphql \
  -H "Content-Type: application/json" \
  -d '{"query":"{ __type(name: \"CommentCreateInput\") { inputFields { name } } }"}'
```

Le jeton n'est requis que pour lire ou écrire des **données**. Corollaire pratique : on peut sonder la
forme d'une mutation depuis n'importe où, sans clé et sans rien risquer sur le vrai tableau.

## 2. L'espace sondé

| | |
|---|---|
| Organisation | `Cursus`, `urlKey` = **`cursus-app`** |
| Équipe | une seule — clé **`CUR`** |
| Projets | 6, un par *feature* |

Convention de l'utilisateur, à respecter par le client : **projet = feature · issue = US ·
sous-tâche = commit** (voir `docs/methode/tickets.md`).

## 2bis. ⚠️ Une clé = un espace, et il n'y a rien à choisir

`organization` n'existe qu'au **singulier** dans le schéma — `organizations` est refusé par la
validation GraphQL. Une *Personal API key* est donc attachée à **exactement un workspace**, déterminé
à sa création : le périmètre d'un jeton ne se déclare pas, il se **constate**.

```graphql
{ organization { id name urlKey } }
```

C'est la requête d'épreuve d'un jeton : elle valide la clé *et* identifie l'espace, pour une fraction
du budget de complexité qu'aurait coûté la liste des projets. Sur cet espace : `Cursus` / `cursus-app`,
une seule équipe (`teams` → `CUR`).

Le nom « Personal API key » induit en erreur : la clé est personnelle **et** attachée à un espace, pas
à un utilisateur qui en survolerait plusieurs.

## 3. La maille — et pourquoi elle tombe juste

Linear n'a pas d'« epic » (c'est un mot Jira). La hiérarchie native se lit ainsi :

```
Project  ──►  Issue  ──►  Sub-issue
(feature)     (US)        (commit)
```

- `project.issues` rend **toutes** les issues du projet — **parents et enfants confondus, à plat**.
  La hiérarchie ne se lit donc pas de la liste : elle se **reconstruit** par `parent`/`children`.
- Une sous-issue porte `parent { identifier }` ; une issue-mère porte `children { nodes { … } }`.
- Une issue **sans** `parent` et **sans** `children` est une US isolée, pas une anomalie.

## 4. Le point dur, et sa bonne surprise

**`issue(id: "CUR-12")` accepte l'identifiant humain**, pas seulement l'UUID :

```graphql
{ issue(id:"CUR-12"){ id identifier title description url state { name } } }
```

C'est ce qui rend `ITaskTracker.ReadAsync(key)` direct : la clé que porte le `RunTrigger`
(« CUR-12 ») est utilisable telle quelle, **sans résolution préalable**. Le champ `id` (UUID) reste
rendu, et c'est lui que réclameront les **mutations**.

`description` est du **Markdown**, ce qui tombe bien : `ReadTask` l'écrit dans `TASK.md` (`D-032`).

## 5. Colonnes et étiquettes

Les colonnes sont des **`workflowStates`**, propres à une équipe. Celles de `CUR` :

| Nom | `type` |
|---|---|
| Backlog | `backlog` |
| Todo | `unstarted` |
| Planning · Plan Review · In Progress · Code Review · QA Review | `started` |
| Done | `completed` |
| Canceled | `canceled` · Duplicate `duplicate` |

⚠️ **`position` n'est PAS un ordre de flux.** C'est un flottant de tri d'affichage, et sur cet espace
il est incohérent avec le cycle réel : trié par `position`, `Done` (3) précède `In Progress` (905) et
`Planning` vaut −937. **Conséquence à retenir pour §7.10.6** : l'invariant de sûreté des déclencheurs
auto — *un workflow auto-déclenché doit déplacer vers une colonne strictement postérieure* — **ne peut
pas s'appuyer sur `position`**. Il faudra un ordre déclaré côté Cursus (dans `project.json`), ou se
rabattre sur `type`, qui est grossier mais monotone.

Étiquettes (`issueLabels`) sur cet espace : `Feature`, `Bug`. Rien qui ressemble encore au `Done` /
`Comments` évoqué au §7.10.3 — **le contrat colonnes/étiquettes reste à définir** (c'est `CUR-5`).

## 6. ⚠️ Deux limites de nature différente — on les confond à ses frais

Linear en applique **deux**. Les mélanger conduit à optimiser la mauvaise (mesuré le
2026-07-26, en-têtes de réponse à l'appui) :

| | Ce que c'est | Valeur | Comment on l'apprend |
|---|---|---|---|
| **Complexité par requête** | un plafond *a priori*, calculé sur les `first:` **avant** d'exécuter | **10 000** | un **400** `INPUT_ERROR` — la requête n'a pas tourné |
| **Budget par fenêtre** | ce que les requêtes acceptées ont réellement **consommé** | **3 000 000** | les en-têtes `x-ratelimit-complexity-*` |

Plus un plafond de **2 500 requêtes** par fenêtre (`x-ratelimit-requests-limit`), qui est
la seule limite qu'une pagination longue peut approcher.

⚠️ **Toute réponse porte son coût dans l'en-tête `x-complexity`.** C'est la façon la moins
chère de mesurer : inutile de chercher le mur par dichotomie de 400 — il suffit de lire
l'en-tête d'une requête qui passe.

⚠️ Le corps d'une réponse 400 porte un `userPresentableMessage` qui **donne le chiffre exact**. Tout
diagnostic sur cette API doit lire le corps : le code HTTP seul n'apprend rien.

### 6a. Ce que coûte chaque forme — et pourquoi la forme actuelle est au bord du mur

| Requête | `x-complexity` | Verdict |
|---|---|---|
| `projects(25) × issues(50) × labels(10)` — **celle du client aujourd'hui** | **8 280** | ✅ mais à **83 %** du plafond |
| `projects(50) × issues(100) × labels(10)` | **33 060** | ❌ 400, refusée |
| `issues(250) × labels(10)` — **à la racine** | **9** | ✅ |
| `projects(250)` nu | 300 | ✅ |
| `projects(6)` nu | 8 | ✅ |

Et les deux requêtes que le client compose désormais (mesurées le 2026-07-26, après bascule) :

| Requête du client | `x-complexity` |
|---|---|
| `projects(250) { pageInfo, id, name }` | **600** |
| `issues(250) { … labels(10), project { id } }` | **8** |
| **total pour le tableau entier** | **608** — contre 8 280 |

⚠️ Le poste dominant a changé de camp : c'est désormais la liste des **projets** qui coûte,
et son coût suit sa borne `first:` (le `pageInfo` en double le prix : 300 → 600). Le
descendre serait facile mais imposerait de paginer plus souvent ; à 6 % du plafond, il n'y
a rien à gagner.

⚠️ **Correction d'une affirmation de ce document.** Il disait que `25 × 50` tenait « avec de
la marge » : c'est faux, il n'en reste **1 720**. L'ajout de `labels(first: 10)` en a mangé
une large part — sur la forme haute, le refus mesuré est passé de 22 555 à **33 060**. Un
champ de plus sur cette requête pouvait la faire sauter, et l'aurait fait sans prévenir
autrement que par un 400.

⚠️ **Une anomalie non expliquée, et assumée comme telle.** Le coût de `projects` suit sa
borne `first:` (8 à 6, 300 à 250), tandis que celui d'`issues` **racine** n'en dépend pas du
tout (9 à 50, 100 et 250 indifféremment). Aucune formule connue ne rend compte des deux, et
on ne va pas en inventer une. Le client ne s'appuie donc pas sur la formule mais sur la
**borne du pire cas** : même en supposant le calcul multiplicatif, `issues(250) × labels(10)`
vaut 2 500 estimés, sous le plafond dans les *deux* hypothèses. C'est ce raisonnement — et
non le 9 mesuré, qui pourrait n'être vrai que sur un petit espace — qui justifie la borne
retenue.

## 6bis. Les formes d'échec — sondées

Trois façons distinctes d'échouer, qui appellent trois remèdes différents :

| Cause | HTTP | Ce que dit le corps |
|---|---|---|
| Jeton invalide ou révoqué | **401** | `code: AUTHENTICATION_ERROR` |
| Requête refusée (trop complexe) | **400** | `code: INPUT_ERROR` + le chiffre dans `userPresentableMessage` |
| Entité introuvable | **200** ⚠️ | `errors` peuplé, `data: null` |

⚠️ **La troisième ligne est le piège** : GraphQL rend **200** alors que rien n'a abouti. Conclure au
succès sur le seul code HTTP laisse passer l'échec silencieusement — c'est la présence d'`errors`
qui tranche, jamais le statut.

⚠️ **`message` n'est pas `userPresentableMessage`.** Le premier est laconique (« Query too
complex ») ; le second porte le diagnostic exploitable (« Complexity: 17055… Maximum allowed:
10000 »). Toujours préférer le second, se rabattre sur le premier.

**L'authentification passe avant l'analyse de la requête** : une requête trop complexe présentée
avec un mauvais jeton rend 401, pas 400. On ne peut donc pas calibrer la complexité sans une clé
valide.

Ces trois formes sont traduites par `LinearFailure` (testé sur ces corps réels) vers
`TrackerRejectedException` / `TrackerUnreachableException`.

## 7. Pagination — mesurée de bout en bout

Toutes les connexions sont en `first: n` / `after: cursor`, avec
`pageInfo { hasNextPage endCursor }`. **Vérifié non théorique** : un projet de 4 issues rend déjà
`hasNextPage: true` à `first: 2`.

| | |
|---|---|
| Page **maximale** | **250** — au-delà, un 400 `INVALID_INPUT` : « *first must not be greater than 250* » |
| Page par défaut | **50**, si l'on ne dit rien |
| Ordre | `createdAt` par défaut, `updatedAt` disponible par `orderBy` |

⚠️ Le refus au-delà de 250 est une **erreur de validation d'argument**, pas un refus de
complexité : les deux plafonds sont indépendants, et le premier atteint gagne.

**`after:` fonctionne — vérifié en chaînant deux appels**, pas en le supposant :
`issues(first: 2)` rend `CUR-45, CUR-44` avec un `endCursor` ; la même requête portant cet
`after:` rend `CUR-43, CUR-42`. Le test vaut la peine : un curseur silencieusement ignoré
rendrait la première page indéfiniment, et **la boucle ne s'arrêterait jamais**.

### 7bis. ⚠️ La pagination imbriquée est le vrai piège — `issues` racine le contourne

`projects { issues }` porte **un curseur par projet**. Paginer sous cette forme demande N
boucles imbriquées, une par projet, chacune avec son propre état — et le coût se multiplie.

`issues` **existe à la racine**, et rend `project { id name }` sur chaque issue. Un seul
curseur, donc **une seule boucle**. C'est la forme retenue par `CUR-45`, et la table du §6a
dit l'écart de coût : 9 contre 8 280.

⚠️ Contrepartie, qui impose **deux** requêtes plutôt qu'une : un projet **sans aucune issue**
n'apparaît dans aucune issue, et disparaîtrait donc du tableau. Les projets se demandent à
part (`projects` nu, bon marché), et les issues s'y raccrochent par `project.id`.

⚠️ Une issue peut n'appartenir à **aucun** projet (`project: null`). Invisible quand on
partait des projets, elle remonte dès qu'on part des issues — il faut donc en décider.

## 8. `filter:` — le filtre serveur

Sondé, et il marche **sur `issues` racine** :

| Forme | Verdict |
|---|---|
| `issues(filter: { labels: { name: { eq: "Feature" } } })` | ✅ accepté (0 résultat ici, voir l'avertissement ci-dessous) |
| `issues(filter: { project: { null: false } })` | ✅ accepté — **non documenté**, mesuré |

Comparateurs (documentés, non tous mesurés) : `eq` `neq` `in` `nin` partout ; `lt` `lte`
`gt` `gte` sur nombres et dates ; `contains` `startsWith` `endsWith` et leurs variantes
`IgnoreCase` / `not…` sur les chaînes ; `null` sur les champs optionnels.

⚠️ **Aucune carte de cet espace ne porte d'étiquette** au 2026-07-26 — toutes les sondes
rendent `labels: { nodes: [] }`. La lecture des étiquettes (`TaskSummary.Labels`) est donc
prouvée par ses tests unitaires sur fragments réels, **pas encore par une carte réelle
étiquetée**. À reprendre quand le prédicat de `CUR-5` en aura besoin.

## 9. ⚠️ L'API rend les noms HTML-échappés

Mesuré : le projet nommé `Finition de l'app — visuel & configuration` revient du GraphQL
comme `…visuel &amp; configuration`. Le tiret cadratin, lui, passe intact — seules les
**entités HTML** sont touchées. Le MCP Linear, interrogé sur le même espace, rend `&` : ce
n'est donc pas la donnée stockée qui porte l'entité, c'est **cette API** qui l'échappe.

Conséquence directe, et visible : **l'écran des tâches de Cursus affiche `&amp;`**. À
dé-échapper à la traduction, là où le reste de la lecture se fait — pas dans la vue, sinon
chaque affichage devra s'en souvenir.

## 10. Commentaires — les ancrer, et les solder

> Schéma introspecté le 2026-07-27 ; **mutations exécutées pour de vrai le 2026-07-28**, sur le
> document du plan d'archi de `CUR-45` (incrément clos), et les cinq commentaires de sonde
> supprimés derrière. Ce qui suit dit donc ce que l'API *fait*, non ce qu'elle promet — et
> l'écart entre les deux s'est révélé considérable (§10d).
>
> **Seconde campagne le 2026-07-30**, déclenchée par une observation de l'utilisateur : un
> commentaire posé par l'API apparaissait « resolved » dans l'application. Elle a **renversé la
> conclusion du §10d**, qui affirmait que l'ancrage était une recherche de texte à l'affichage.
> C'est faux. La leçon de dispositif vaut d'être notée : le §10d était une **inférence** tirée
> d'une mesure juste (`quotedText` n'est pas validé), et elle a tenu deux jours parce que rien ne
> l'avait confrontée à l'interface. Une mutation qui réussit ne dit pas ce que l'utilisateur voit.

**Le motif de la sonde.** Le MCP Linear (`save_comment`) ne sait pas **résoudre** un commentaire —
son input n'a pas de champ de résolution. GraphQL sait le faire. C'est ce qui départage les deux
voies dès qu'un agent doit solder une divergence sur la carte plutôt que dans un terminal.

⚠️ **Ce motif était plus large, et il a été réduit par la mesure.** On croyait aussi que GraphQL
savait *ancrer* là où le MCP ne savait pas. **Personne ne sait ancrer** : l'ancre est une marque
dans le document, et aucune API ne l'écrit (§10d). Un agent ne peut donc pas poser de remarque
visible sur un **document** — il pose sur le **projet** ou l'**issue** qui le porte (§10e).

### 10a. L'ancre

`CommentCreateInput` porte `quotedText` **et** `documentContentId`. Et `Document` expose
`documentContentId` en champ direct — inutile de traverser `DocumentContent` pour l'obtenir :

```graphql
{ document(id: "…") { documentContentId } }

mutation {
  commentCreate(input: {
    documentContentId: "…"
    quotedText: "le passage exact, tel qu'il figure dans le document"
    body: "la divergence, en Markdown"
  }) { comment { id } }
}
```

Les autres ancres du même input : `issueId`, `projectId`, `initiativeId`, `projectUpdateId`,
`initiativeUpdateId`, `postId`. **Exactement une** est exigée — mesuré, message de validation à
l'appui. `parentId`, qui répond dans un fil, **n'en dispense pas** : une réponse porte à la fois son
parent et l'ancre du fil, faute de quoi la mutation est refusée en `INVALID_INPUT`.

⚠️ **Deux identifiants à ne pas confondre.** On **écrit** contre le `documentContentId`, mais on
**lit** par l'`id` du document (`document(id:) { comments }`) — et `documentContent` n'existe pas à
la racine du schéma. `Document` expose les deux champs, donc l'aller-retour se fait en une requête.

### 10b. Le solde

```
commentResolve(id, resolvingCommentId)
commentUnresolve(id)
```

⚠️ `resolvingCommentId` nomme **quel commentaire solde la divergence**, pas seulement qu'elle l'est.
C'est très exactement la clause de `docs/methode/dod/feature/spec.md` §2 — *« reprise, ou refusée avec
sa raison écrite ; une divergence sans suite écrite n'est pas soldée »*. Le modèle de données porte
déjà la sémantique de la méthode : il n'y a rien à simuler à côté.

Trois mesures qui bornent l'usage :

| Ce qu'on tente | Ce que Linear fait |
|---|---|
| `commentResolve` **sans** `resolvingCommentId` | ✅ résout, pose `resolvedAt` et `resolvingUser` — le champ est **optionnel** |
| `resolvingCommentId` = une **réponse du fil** (enfant du commentaire résolu) | ✅ résout, `resolvingComment` relisible |
| `resolvingCommentId` = un commentaire **frère** | ❌ `INTERNAL_SERVER_ERROR` — un 500 nu, pas une erreur de validation |

⚠️ Le troisième cas est le piège : l'erreur ne se présente pas comme une faute d'usage, elle
ressemble à une panne de Linear. **Un solde s'écrit donc en deux temps** — créer la réponse (ancre +
`parentId`), puis résoudre en la nommant. C'est aussi la bonne ergonomie : la raison du solde
s'écrit, et c'est elle qui solde.

Résoudre un commentaire **déjà résolu** réussit et re-pose `resolvedAt` — l'idempotence exigée au
§7.10.3 est donc tenue ici. (Non mesuré : si un `resolvingCommentId` déjà posé survit à un second
`commentResolve` qui l'omet.)

Corollaire de répartition — **révisé le 2026-07-30**. On écrivait ici « l'agent pose, l'humain
résout », en s'appuyant sur la DoD (*« l'humain prononce l'accord »*). `D-045` a tranché autrement :
un agent-vérificateur **solde**, et l'humain garde le dernier mot en relisant ce que le cycle a
dégrossi. L'API ne contraint rien dans un sens ni dans l'autre — c'est bien une décision de méthode,
et elle est là-bas, pas ici.

### 10c. Deux champs à éprouver avant de bâtir dessus

`CommentCreateInput` porte aussi `createAsUser` et `displayIconUrl` : de quoi faire signer un
commentaire par un relecteur **nommé** plutôt que par le porteur de la clé — ce qui rendrait une revue
tierce lisible comme telle sur la carte. Ces champs sont, chez d'autres API, réservés aux jetons
d'application ; avec une *Personal API key*, c'est à vérifier **tôt**, avant d'avoir construit autour.

### 10d. ⚠️ L'ancre n'est pas une ancre — elle n'est jamais vérifiée

Le fait le plus important de cette sonde, et le plus contre-intuitif. **Quatre citations envoyées,
quatre acceptées**, `success: true` à chaque fois :

| Citation envoyée | Réponse de Linear |
|---|---|
| un passage **exact et unique** du document | acceptée |
| un passage **absent** du document, inventé pour la sonde | **acceptée** — rendue verbatim |
| un passage **présent deux fois** (ambigu) | **acceptée** — sans signalement |
| un passage **à cheval sur deux blocs** (fin de paragraphe + titre `##`) | **acceptée** — sauts de ligne conservés |

Et l'introspection de `Comment` le confirme : le type ne porte **aucun champ positionnel** — ni
offset, ni sélection, ni intervalle. Idem pour `CommentCreateInput`, dont les 18 champs ont été
listés : rien qui situe. `quotedText` est un `String`, rien de plus.

**Mais la conclusion qu'on en tirait était fausse.** L'ancrage n'est pas une recherche de texte à
l'affichage : c'est une **marque posée dans le document**, et `quotedText` n'est qu'un texte
d'affichage — ce qui explique enfin pourquoi Linear ne le valide jamais. Il ne s'en sert pas pour
ancrer.

**La marque, mesurée le 2026-07-30.** `DocumentContent` porte un champ `contentState` : l'état
[Yjs](https://github.com/yjs/yjs) de l'éditeur, en base64. Décodé, on y trouve une marque par
commentaire ancré :

```json
inlineComment {"commentId":"606b4fc5-…","createdBy":null,"resolved":false,"block":false}
```

Le protocole qui l'a établie, sur un document portant neuf commentaires :

| Ce qu'on observe | Ce qu'on en tire |
|---|---|
| `documentContent.updatedAt` bouge **175 ms avant** la création d'un commentaire depuis l'UI | c'est le **client** qui écrit la marque, pas le serveur |
| un commentaire créé par `commentCreate` n'a **aucune** marque | l'API ne l'écrit jamais |
| 2 commentaires sur 9 portent une marque, et ce sont **exactement** les 2 que l'interface affiche | la marque **est** l'ancre ; corrélation parfaite |
| les 7 autres sont rangés par l'UI avec les **résolus**, alors que `resolvedAt` est `null` | un commentaire sans marque n'a pas de position, donc l'UI le sort du texte |

Le cas qui isole la cause : un commentaire posé par l'API dont le passage cité était **toujours
présent, au caractère près** — vérifié jusque dans l'état Yjs décodé — et qui n'apparaissait pas
malgré tout. Ce n'est donc pas la disparition du texte qui décide, c'est la marque.

**Comment une marque meurt.** Six des sept commentaires sans marque en avaient une : leur passage a
été réécrit, et Yjs a supprimé la marque **avec** le texte qui la portait. C'est structurel, et
lourd de conséquence pour tout cycle de revue — l'étape qui *corrige* détruit par construction les
ancres de l'étape qui a *relu*. Deux marques orphelines subsistent par ailleurs, pointant vers un
commentaire supprimé : `commentDelete` ne nettoie pas le document.

**Ce que l'interface lit pour l'état résolu.** `Comment.resolvedAt`, et non le `resolved` de la
marque — mesuré : une résolution par l'API a replié le fil dans l'application **en temps réel**,
alors que la marque portait encore `resolved: false`. Le client la rattrape ensuite (18 s plus tard
dans la mesure), donc les deux porteurs divergent transitoirement. Sans importance à l'usage, mais
il ne faut pas lire la marque pour connaître l'état d'un commentaire.

**Trois conséquences, toutes à la charge du client :**

1. **Ne pas ancrer sur un document par l'API.** C'est impossible, et le résultat est pire qu'un
   échec : le commentaire existe, se lit comme une divergence située, et **personne ne le voit**.
   Poser sur le projet ou l'issue (§10e).
2. **Vérifier quand même que la citation existe, et qu'elle est unique.** Non plus pour Linear, qui
   n'en fait rien, mais pour l'**humain et l'agent qui liront** : une citation est le seul moyen de
   désigner un passage, et une citation ambiguë ne désigne rien. Le refus à l'écriture, avec le
   nombre d'occurrences, force à élargir jusqu'à ce qu'elle désigne.
3. **Ne pas présumer de la stabilité.** Le document édité après coup, la citation ne correspond plus
   à rien. Un passage cité n'est pas une référence, c'est une **empreinte**.

Corollaire pour tout client qui écrit ici : la validation de la citation reste du **vrai travail**,
mais son métier a changé — elle ne prépare plus une ancre, elle garantit qu'une désignation est
sans ambiguïté pour le lecteur suivant.

### 10e. Poser une remarque là où elle se voit — le projet ou l'issue

Puisqu'un document est hors de portée (§10d), la remarque se pose sur ce qui le **porte**. Et le
rattachement, mesuré sur les quatre documents de l'espace, épouse exactement les niveaux de
`docs/methode/tickets.md` :

| Document | `Document.…` | Ancre du commentaire |
|---|---|---|
| Discovery, Spec | `project` | `projectId` |
| Plan d'archi | `issue` | `issueId` |

`Document` expose aussi `initiative`, `release` et `cycle`, non utilisés ici. Le porteur se **déduit**
donc du document visé : c'est une lecture, pas une décision — ni l'appelant ni un agent n'a à le
choisir.

Ces deux ancres n'ont **rien à ancrer**, donc rien qui puisse échouer : un commentaire de projet ou
d'issue est visible sans marque. Mesuré le 2026-07-30 sur `CUR-20` (issue, état `Canceled`) et sur le
projet `Un agent pilote Cursus`, les commentaires de sonde supprimés derrière :

- `quotedText` est **accepté et affiché** sur les deux, alors qu'il n'y a aucun texte à citer ;
- le fil (`parentId`) et le solde (`commentResolve` + `resolvingCommentId`) fonctionnent, et
  l'interface montre l'en-tête « Resolution » sur la réponse qui solde ;
- un commentaire de projet atterrit dans l'onglet **Activity** du projet.

**Le solde sur un projet est mesuré depuis** (2026-07-30, seconde passe — il manquait, et c'était le
porteur nominal d'une Discovery). Il se comporte comme sur une issue : `commentResolve` nommant la
réponse du fil renseigne `resolvedAt`, `resolvingComment` et `resolvingUser`. Et un point qui n'allait
pas de soi : **`parentId` et `projectId` sont acceptés ensemble** — là où, sur un document, `parentId`
seul se fait refuser en « exactly one of … must be defined ». Une réponse doit donc porter la cible de
son fil, jamais seulement son parent.

⚠️ **La réponse qui solde a son propre `resolvedAt` nul.** Elle apparaît dans la liste des commentaires
de la carte comme n'importe quel autre, `parent` renseigné et non soldée. Un décompte des « remarques
ouvertes » qui ne filtre pas sur `parent === null` compte donc les soldes : la porte *zéro remarque
ouverte* ne se ferme jamais, chaque solde en ajoutant une. Le piège est indolore tant que les fils sont
rares, et il devient faux dès qu'il gouverne un gate.

⚠️ **`quotedText` est aplati à l'affichage.** Les sauts de ligne sont conservés par l'API — relus,
ils sont bien là — mais l'interface rend la citation **sur une seule ligne**. Aucune mise en page
n'est donc possible dedans. Le corollaire est pratique : tout ce qui doit être mis en forme (le
repère du passage, par exemple) va dans le **corps**, qui est du Markdown rendu ; la citation reste
le passage nu.

### 10f. ⚠️ Deux façons de lire les commentaires, dont une qui ment

| Porteur | Ce qui marche |
|---|---|
| document | `document(id:) { comments { nodes } }` — et **non** par `documentContentId`, cf. §10a |
| issue | `issue(id:) { comments { nodes } }` |
| **projet** | ❌ `project(id:) { comments }` renvoie **une liste vide** |

Pour un projet, il faut la racine filtrée :

```graphql
{ comments(filter: { project: { id: { eq: "…" } } }, first: 50) { nodes { id body quotedText resolvedAt } } }
```

**Le même filtre marche sur une issue** — mesuré en 2026-07-30 avec un commentaire vivant, pour ne pas
confondre « filtre inopérant » et « carte sans commentaire » : `filter: { issue: { id: { eq: … } } }`
rend la même forme. Un client n'a donc **qu'un seul chemin de lecture à écrire** pour les deux porteurs,
et il vaut mieux l'écrire ainsi que d'emprunter `issue.comments` : c'est le chemin dont on sait qu'il ne
mentira pas si le porteur change de genre. Dans les deux cas, les réponses arrivent **à plat**, au même
niveau que les remarques qu'elles soldent, leur `parent` renseigné.

Le champ `comments` **existe** sur `Project` et ne rend aucune erreur — il rend `[]`. Mesuré avec
quatre commentaires bel et bien présents sur le projet, visibles dans l'interface. C'est le mode
d'échec le plus coûteux qui soit : silencieux, et indiscernable d'un projet sans commentaire. Un
client qui construit une revue là-dessus conclurait « aucune remarque » et passerait le gate.

### 10g. ⚠️⚠️ Réécrire un document par l'API détruit l'ancrage de ses commentaires

Le piège le plus grave de cette référence, parce qu'il est invisible et qu'il frappe précisément au
moment où l'on travaille bien.

`DocumentUpdateInput` n'expose **pas** `contentState` — introspecté : ses 16 champs comportent
`content` (une `String` Markdown) et rien qui touche à l'état de l'éditeur. Écrire par ce chemin
**reconstruit** l'état Yjs depuis le Markdown, donc **efface toutes les marques** `inlineComment` du
document, donc désancre tous ses commentaires d'un coup. Ils survivent comme objets ; ils disparaissent
du texte.

C'est ce que fait `save_document` du MCP Linear — l'outil qu'un agent prend naturellement pour
appliquer les corrections d'une revue. **Un agent qui corrige d'après les remarques efface les
remarques qu'il corrige.** Non mesuré directement (on n'a pas voulu détruire un document réel pour
l'établir), mais déduit de deux faits mesurés : `contentState` est le seul porteur des marques, et
aucune mutation ne l'accepte en entrée.

La conséquence est déjà intégrée par `D-045`, qui sort les remarques du document pour cette raison
parmi d'autres. Mais la règle vaut au-delà du cycle de revue : **ne jamais réécrire par l'API un
document qui porte des commentaires qu'on veut garder ancrés.** Éditer dans l'interface est sans
danger — c'est le client qui tient l'état, et il déplace les marques avec le texte.

### 10h. Trois murs rencontrés en usage réel — 2026-07-30

Non issus d'une sonde : payés en travaillant, sur la Discovery d'*Un agent pilote Cursus*.

**La racine d'un fil ancré ne se supprime pas.** `delete_comment` la refuse, avec un message
explicite : *« Cannot delete the root comment of an inline description thread. Delete its replies
individually, or resolve the thread instead. »* La marque vit dans l'état de l'éditeur (§10d), et
la détruire depuis l'API laisserait le document pointer vers rien. Restent deux voies : **résoudre**
le fil, ou le supprimer **dans l'interface**. Conséquence pratique : un fil posé au mauvais endroit
ne se rattrape pas par outil — il faut une main humaine, ou vivre avec.

**Aucune création d'étiquette de projet — et `list_project_labels` masque le groupe.** L'outillage
MCP porte `create_issue_label` et `list_project_labels`, mais **rien** qui crée une étiquette de
projet. Or Linear sépare strictement les deux familles : une étiquette d'issue ne s'applique pas à
un projet, même à nom identique — il faut donc créer chaque étiquette **deux fois**, et la seconde
à la main. ⚠️ Mesuré sur le MCP seulement ; que la mutation GraphQL existe ou non n'a pas été
introspecté.

⚠️⚠️ **Piège dans le piège** : `list_issue_labels` rend `parent` (le groupe) sur chaque étiquette,
`list_project_labels` **ne le rend pas**. Conclure de son absence que les étiquettes de projet sont
hors groupe est faux — l'erreur a été commise, puis corrigée par la mesure. **L'appartenance au
groupe ne se lit pas, elle se teste** : poser deux étiquettes du même groupe sur un projet rend

```
400 invalid_request — The label 'Done' is in the same group as 'Review Requested'.
Only one label in a group can be applied to a project.
```

L'exclusivité de groupe tient donc côté projet comme côté issue. Règle générale : quand la sortie
d'un outil est muette sur une propriété, **provoquer l'erreur** plutôt que lire l'absence.

**Un `patch` ne désancre pas ce qu'il ne traverse pas** — et cela nuance §10g. Le contenu que rend
`get_document` **contient les balises** `<linear-comment id="…" resolved="…">…</linear-comment>` en
clair. Une écriture par `patch` dont aucune opération ne recouvre une balise l'a laissée intacte,
commentaire toujours ancré (observé une fois, sur un commentaire résolu, pendant qu'un autre
passage du même document était réécrit). L'inverse reste vrai et non mesuré : envoyer un `content`
entier, ou patcher **par-dessus** une balise, la fait disparaître. La règle utile n'est donc pas
« ne jamais écrire par l'API » mais **« patcher, jamais remplacer, et ne pas traverser une
marque »**.

## 11. Ce que la sonde n'a pas couvert

À sonder avant de s'y appuyer :

- **les mutations** (`issueUpdate` pour déplacer, `issueAddLabel` pour étiqueter) — elles écrivent
  sur le vrai tableau, donc réservées à `2·2b`+ et à faire sur une issue de test ;
- **`createAsUser` / `displayIconUrl`** (§10c) : le reste des mutations de commentaire est mesuré
  depuis le 2026-07-28, mais pas ceux-là — reste à savoir si une *Personal API key* y a droit ;
- **`documentUpdate` face aux marques** (§10g) : la destruction de l'ancrage est déduite, pas
  exécutée. La mesurer demande un document jetable portant un commentaire ancré — à faire, parce que
  c'est un piège qu'on préfère connaître par une sonde que par une revue perdue ;
- **l'écriture de la marque** : aucune voie trouvée pour la poser depuis l'API. L'absence d'un chemin
  se prouve mal ; ce qui est établi, c'est qu'aucun champ des inputs introspectés ne l'accepte ;
- **l'idempotence** exigée au §7.10.3 : mesurée sur `commentResolve` seulement (§10b). Pour le
  déplacement de colonne — déplacer vers la colonne où la carte est déjà — c'est toujours à
  vérifier, pas à supposer ;
- les **limites de débit** en pratique : les plafonds sont connus (§6), mais aucune fenêtre n'a
  été poussée jusqu'au 429 ;
- la **stabilité du curseur** sous écriture concurrente : si une carte est créée entre deux pages,
  rien ne dit si elle est vue deux fois, une fois, ou pas du tout ;
- le champ `triage` et les projets d'autres équipes (une seule équipe ici).
