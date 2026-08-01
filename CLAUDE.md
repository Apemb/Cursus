# Cursus — instructions de travail

## Le projet en deux phrases

Cursus vise à devenir un manageur de workflow agentique (C#/.NET 10, Avalonia, RoyalTerminal).
Trajectoire actée : on construit **d'abord un noyau déterministe** — un moteur qui parcourt un graphe
d'étapes-scripts et route chaque étape sur son code de sortie, **sans jamais savoir ce qu'est un
agent**. L'`AgentStep` se greffera plus tard comme un `StepKind` de plus.

Avant toute intervention non triviale, lire **`docs/design/architecture.md`** : c'est la référence sur
le découpage, les décisions prises et celles qui ont été écartées.

## Entretenir le document d'architecture

`docs/design/architecture.md` **doit évoluer avec le code**. Il n'est pas un artefact figé : un
document d'architecture faux est pire que pas de document.

Le mettre à jour **dans le même commit ou immédiatement après** dès qu'un développement :

- ajoute, supprime ou renomme un type structurant du domaine ;
- déplace une responsabilité d'un objet à un autre, ou change une frontière entre couches ;
- tranche une **question ouverte** que le document liste — la déplacer alors vers les décisions ;
- écarte une alternative après discussion — **l'écart mérite d'être écrit autant que le choix**, c'est
  ce qui évite de refaire le même débat dans six mois ;
- referme un trou connu (ex. la jonction entre le noyau déterministe et la partie sessions/PTY) ;
- introduit une dépendance externe, ou un invariant que le code seul ne rend pas évident.

Ne pas y consigner ce qui est déjà lisible ailleurs : l'historique git raconte les hashes et la
chronologie, le code raconte le comment. Le document raconte le **pourquoi** et le **découpage**.

Le **pourquoi *dans le temps*** — un pivot, un arbitrage tranché, une alternative écartée après débat, une
décision qu'une décision plus tardive vient superséder — va, lui, dans **`docs/design/decisions.md`** (journal
ADR, **append-only** : on n'y réécrit jamais). C'est le complément de `architecture.md`, qui décrit l'état
*présent* et ne garde pas les décisions périmées. Ajouter une entrée `D-NNN` dès qu'une décision structurante
est prise ou renversée ; y renvoyer depuis le message de commit quand c'est utile.

Y maintenir la distinction en trois registres : **construit** / **tranché mais pas encore construit** /
**question ouverte**. Un « prévu » présenté comme un « fait » désoriente le lecteur suivant.

### Avec quel dispositif

Trois régimes, selon l'ampleur. Le critère qui les départage : **une relecture adversariale ne paie que
s'il n'existe pas de source de vérité à laquelle se comparer.** Pour une mise à jour incrémentale, le
diff du code *est* cette source ; pour une rédaction depuis zéro, il n'y a rien, et la critique croisée
change tout.

| Régime | Quand | Dispositif |
|---|---|---|
| **Inline** | Cas normal, la grande majorité | Mise à jour directe, dans le commit qui la rend nécessaire. Aucun agent. C'est le seul régime qui empêche le document de prendre du retard, puisqu'il ne peut pas en prendre. |
| **Vérification** | Fin de jalon | **Un seul** agent en arrière-plan. Il ne réécrit pas : il relit le document contre le dépôt et **liste les divergences** (chiffres périmés, types renommés, trous refermés, registres qui ont glissé). Filet contre la myopie de celui qui vient d'écrire. |
| **Refonte** | Une section entière devient fausse d'un coup | Orchestration multi-agents : lecteurs en parallèle → rédaction → critiques sur lentilles distinctes (exactitude vérifiée contre le code, complétude, utilité réelle) → révision. Réservé aux moments de re-création — jonction noyau ↔ sessions, arrivée d'un `AgentStep`, nouveau pivot. |

Ne pas déclencher une refonte pour un changement incrémental : le coût est sans commune mesure avec le
gain, et un document réécrit en entier perd les formulations que les relectures précédentes avaient
affinées.

## Méthode de développement

**TDD discipliné**, sans exception sur la logique métier :

- jamais de code de production sans un test rouge qui le réclame, et le rouge doit être **observé**
  (et pour la bonne raison) ;
- un test à la fois ;
- au vert, l'implémentation la plus simple, quitte à tricher — le test suivant force la généralisation ;
- refactor une fois vert, sur le code de test comme sur le code testé.

Dès qu'un changement crée ou supprime une classe, traverse plusieurs modules, ou implique un choix de
découpe non évident : **plan validé avant d'écrire le moindre test**.

Ce plan porte **en tête un schéma-delta** : la table « Objets impactés » rendue visuellement, les blocs
*ajoutés* en vert, *modifiés* en ambre, *supprimés* en rouge, chaque bloc portant sa responsabilité et,
pour un modifié, la ligne `+ <incrément>` du comportement neuf. La convention (couleurs, anatomie d'un
nœud, où le schéma périme) et les cartes d'état permanentes vivent dans **`docs/design/schemas.md`** —
compagnon visuel de l'architecture, sans autorité sur elle. Le schéma se lit sur le vocabulaire du
modèle §3 de ce fichier (définition vs exécution) : il double la prose du plan, il ne la remplace pas.

**Où vit le plan.** Le schéma-delta est un bloc `mermaid` : il ne se rend jamais inline dans le
terminal, seulement là où quelqu'un peut l'ouvrir. Deux cas, et le premier est le nominal dès qu'un
backlog porte le travail :

- **Le travail est porté par une carte** — le plan vit dans le **document attaché** à l'incrément,
  écrit en `Planning`. Linear **rend le mermaid nativement** (`/diagram`, ou un bloc ` ```mermaid `
  collé) : le schéma se lit sur la carte, sans fichier intermédiaire à créer puis à nettoyer, et la
  revue a lieu au même endroit que le reste.
- **Le travail n'est porté par aucune carte** — le plan est un fichier, et il **indique son propre
  chemin en tout premier** (avant le titre), parce que le schéma n'existe vraiment que dans l'aperçu
  du fichier. Une ligne `> Fichier : <chemin absolu>` en tête suffit.

Le plan de design est l'artefact de l'**incrément** — un pas n'en a pas, il porte une **test list** ;
une feature n'en a pas, elle porte une **spec**. Voir `docs/methode/tickets.md` §1 et `D-036`.

### Les trois échelles de conception

La conception se fait à **trois échelles**, de plus en plus fine, et chacune a son artefact
(`D-053`). Les noms disent l'échelle, dans l'ordre : système → objets → code.

| Échelle | Où | Artefact | Décide |
|---|---|---|---|
| **Architecture** — système / module | Feature, en `Spec` | la **spec**, moitié technique | Composants, frontières, dépendances externes |
| **Design** — objets / classes | Incrément, en `Planning` | le **plan de design** | Objets qui naissent, changent, meurent ; responsabilités ; ordre des pas |
| **Implémentation** — code | Pas, à sa prise | la **test list** | Les cas à prouver, fichier par fichier |

⚠️ **Le découpage d'une feature en incréments n'est pas une de ces échelles** : c'est un
ordonnancement. Il livre à chaque incrément sa **direction** et son **acceptation** — vers où il va,
ce qu'on vérifiera à la fin — jamais sa structure, et encore moins ses pas.

⚠️ **L'autorité ne suit pas l'échelle** : le plan **le plus haut est le moins engageant**. Le plan
d'architecture est d'ensemble et **indicatif** — il montre que ça **peut** fonctionner et comment
c'est **censé** fonctionner. Le plan de design est **local et engageant**. Un plan de design a donc
le droit de s'écarter du plan d'architecture au contact du réel, à condition de le dire (`D-049`).

## Écrire un ticket

Le backlog vit dans Linear (espace `cursus-app`, équipe `CUR`) ; ce que doit **contenir** un ticket vit
dans **`docs/methode/tickets.md`** — trois niveaux (feature / incrément / pas) et les questions
auxquelles chacun répond.

La frontière avec le plan gaté ci-dessus mérite d'être tenue : **le ticket dit *quoi* et *pourquoi*, le
plan dit *comment*.** Un ticket qui prescrit l'implémentation a mangé le plan, et il sera périmé avant
d'être pris. L'enjeu n'est pas cosmétique : la trajectoire mène à ce que Cursus **consomme ces tickets**,
et un ticket devient alors l'unique brief d'un agent qui n'a pas eu la conversation.

Trois registres, un par niveau, qu'il ne faut pas confondre (`D-036`, amendé par `D-049` et
`D-053`) : en **feature**, le **binôme** arbitre *quelle solution et si elle vaut le coup* et montre
que ça peut marcher — la **spec** n'arbitre pas, elle **enregistre** cet arbitrage ; l'**incrément**
conçoit *comment c'est structuré* dans son **plan de design** ; le **pas** prouve, et sa **test
list** s'écrit à sa prise, jamais d'avance. Le juge de ce qui mérite d'être un incrément plutôt qu'un pas est le **rôle produit** :
est-ce recettable par quelqu'un qui ne lit pas le code ?

## Entretenir la carte visuelle

`docs/design/schemas.md` suit la même règle de fraîcheur que l'architecture, mais avec une nuance : ses
**cartes d'état** (§1–§5 : couches, ports, modèle, coutures) se mettent à jour quand la structure bouge ;
ses **schémas-delta** sont par nature éphémères et **ne doivent jamais se figer en carte d'état** — un
« ajouté » de la marche d'avant devient un mensonge deux marches plus tard.

## Conventions

| Domaine | Règle |
|---|---|
| Langue du code | Anglais — classes, méthodes, propriétés, exceptions |
| Langue du reste | Français — commentaires, documentation XML, messages de test, commits, docs |
| Diacritiques | Toujours corrects et complets. Jamais d'ASCII dégradé |
| Titres de test | `étant donné <état>, quand <action>, alors <conséquence observable>` |
| Corps de test | Sections commentées `// arrange`, `// act`, `// assert` |
| Tests d'I/O | Adossés aux binaires POSIX du système. Non portable Windows, **assumé** (cible macOS/Linux) |
| Modélisation | Pas de nullable pour distinguer des **types d'objets** différents. Héritage / interface, discriminant dans le JSON seulement |

Les commentaires expliquent **pourquoi**, jamais **quoi**. Un commentaire qui paraphrase la ligne
suivante est du bruit ; un commentaire qui explique un piège évité vaut de l'or.

**Modéliser les variantes par le type, jamais par des nullables.** Un objet qui peut être « un A
**ou** un B » se modélise par un **héritage** (`abstract record` + sous-types) ou une **interface** —
chaque sous-type ne portant que ses propres propriétés, toutes non-nulles. Encoder la variante par
deux champs nullables mutuellement exclusifs (`A? xor B?`) plus un discriminant est proscrit : cela
laisse représentables des états illégaux (les deux nuls, ou les deux pleins) que le type devrait
rendre impossibles. Le discriminant (`kind`) vit **dans le document JSON seulement** ; un adaptateur
(le sérialiseur) le lit pour construire le bon sous-type, et il ne remonte jamais en propriété du
modèle de domaine. **Nuance** : la règle vise les *variantes de type*, pas les *valeurs optionnelles* —
un `Description?` absent (une donnée qui peut manquer, façon `Option`) reste parfaitement légitime.

## Standard de qualité — non négociable

À **chaque** commit :

```bash
dotnet build   # 0 warning
dotnet test    # suite entièrement verte
```

Un warning n'est pas toléré, même dans les tests. Ce standard est tenu depuis le premier jalon.

## Branches

**Aucun code n'arrive sur `main` autrement que par une PR.** Une branche et une PR par niveau de
ticket, fusionnées en cascade — `pas/` en **squash** dans `story/`, `story/` en **rebase puis
fast-forward** dans `feature/`, `feature/` en **rebase puis `--no-ff`** dans `main`. La branche de
feature n'est pas systématique : elle se décide en Spec, et seulement si la feature expose une
surface qui doit apparaître d'un bloc. Le détail, les pièges de chaque mode de fusion et le motif
vivent dans `docs/methode/flux.md` §6 et `D-042`.

Sur une branche de pas, **commiter librement** — WIP, correction de revue, refactor. Le squash de
fusion produit le commit propre ; ce n'est plus une discipline à tenir, c'est une mécanique.

**L'exception** : le travail sur *la façon de travailler* — méthode, documentation, outillage — va
directement sur `main`. Il n'est porté par aucune carte, donc aucune branche ne lui correspond. Si
ce cas devenait fréquent au point de mériter sa propre règle, ce sera le moment de légiférer.

**Sur un squash, réécrire le corps à la main** : GitHub y colle par défaut la concaténation des
messages de WIP, et c'est ce commit-là qui reste dans l'histoire.

**Ne jamais citer un hash de commit dans la documentation** — écrire l'identifiant Linear (`CUR-45`).
Le rebase des branches périme tout hash écrit au fil du développement, et dans `decisions.md`, qui
est append-only, un hash périmé est incorrigible. Pour désigner un état précis du code, un **tag**.
