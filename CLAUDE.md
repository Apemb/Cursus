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

Le plan **indique son propre chemin de fichier en tout premier** (avant le titre) : le schéma-delta est
un bloc `mermaid` qui ne se rend en graphique que lorsqu'on **ouvre le fichier** dans un aperçu — jamais
inline dans le terminal. Sans le chemin sous la main, le lecteur ne peut pas atteindre le seul endroit
où le schéma existe vraiment. Une ligne `> Fichier : <chemin absolu>` en tête suffit.

## Écrire un ticket

Le backlog vit dans Linear (espace `cursus-app`, équipe `CUR`) ; ce que doit **contenir** un ticket vit
dans **`docs/methode/tickets.md`** — trois niveaux (feature / incrément / pas) et les questions
auxquelles chacun répond.

La frontière avec le plan gaté ci-dessus mérite d'être tenue : **le ticket dit *quoi* et *pourquoi*, le
plan dit *comment*.** Un ticket qui prescrit l'implémentation a mangé le plan, et il sera périmé avant
d'être pris. L'enjeu n'est pas cosmétique : la trajectoire mène à ce que Cursus **consomme ces tickets**,
et un ticket devient alors l'unique brief d'un agent qui n'a pas eu la conversation.

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

## Commits

- Un commit par comportement terminé (suite verte, refactor fait).
- Message argumenté : le **pourquoi**, et les alternatives écartées quand il y a eu arbitrage. Les
  messages de ce dépôt sont longs à dessein — ils portent le raisonnement que le code ne peut pas dire.
- Travail sur `main`. **Ne jamais pousser sans demande explicite.**
