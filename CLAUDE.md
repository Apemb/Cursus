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

Y maintenir la distinction en trois registres : **construit** / **tranché mais pas encore construit** /
**question ouverte**. Un « prévu » présenté comme un « fait » désoriente le lecteur suivant.

## Méthode de développement

**TDD discipliné**, sans exception sur la logique métier :

- jamais de code de production sans un test rouge qui le réclame, et le rouge doit être **observé**
  (et pour la bonne raison) ;
- un test à la fois ;
- au vert, l'implémentation la plus simple, quitte à tricher — le test suivant force la généralisation ;
- refactor une fois vert, sur le code de test comme sur le code testé.

Dès qu'un changement crée ou supprime une classe, traverse plusieurs modules, ou implique un choix de
découpe non évident : **plan validé avant d'écrire le moindre test**.

## Conventions

| Domaine | Règle |
|---|---|
| Langue du code | Anglais — classes, méthodes, propriétés, exceptions |
| Langue du reste | Français — commentaires, documentation XML, messages de test, commits, docs |
| Diacritiques | Toujours corrects et complets. Jamais d'ASCII dégradé |
| Titres de test | `étant donné <état>, quand <action>, alors <conséquence observable>` |
| Corps de test | Sections commentées `// arrange`, `// act`, `// assert` |
| Tests d'I/O | Adossés aux binaires POSIX du système. Non portable Windows, **assumé** (cible macOS/Linux) |

Les commentaires expliquent **pourquoi**, jamais **quoi**. Un commentaire qui paraphrase la ligne
suivante est du bruit ; un commentaire qui explique un piège évité vaut de l'or.

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
