---
name: spec
description: Arbitrer ce qu'une feature construit, une fois son besoin diagnostiqué. Utiliser à la prise d'une carte de feature en colonne `Spec`, quand la Discovery de cette feature est close, ou quand on demande d'écrire la spec d'une feature.
---

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

Un **arbitrage**, pas un second diagnostic. Par construction, l'alignement a déjà eu lieu : la
Discovery a établi le besoin, nommé pour qui il compte et pourquoi il compte maintenant.
**Ne pas ré-interroger l'humain sur le besoin, le public ou l'urgence** — les redemander est le
signal qu'il faut reposer la carte en `Discovery`, pas continuer ici.

⚠️ **C'est le binôme qui arbitre ; le document en porte la trace** (`D-053`). L'acte appartient à
l'humain et à l'agent qui rédigent ensemble — la spec ne tranche rien, elle **enregistre**. Deux
conséquences pratiques : ne jamais écrire un arbitrage que l'humain n'a pas prononcé, fût-il
évident ; et si une option reste non tranchée, ce n'est pas la rédaction qu'il faut reprendre mais
l'interrogatoire qu'il faut rouvrir.

## 1. Charger le socle

Lire le document `Discovery` lié à la feature. En retenir le besoin, le pour-qui, le
pourquoi-maintenant comme des **faits acquis** — ne pas les recopier dans la spec, y **renvoyer**
par un lien.

Complet quand : le lien vers le document Discovery est identifié, et aucune de ses trois réponses
n'a été réécrite ici.

## 2. Arbitrer les options

Pour chaque piste ouverte en Discovery — et toute piste apparue depuis — évaluer faisabilité et
coût, légèrement : ça sert à choisir, pas à s'engager. Invoquer le skill `interrogatoire` pour
trancher : les faits de faisabilité, l'agent les établit seul en explorant ; les
choix — quelle option retenir, à quel prix on l'accepte, quelle capacité et quelle recette en
découlent — reviennent à l'humain.

**Écrire les écarts** : chaque piste non retenue garde sa raison, dans la spec, à côté du choix
fait. Une piste qui disparaît sans laisser de trace se reproposera dans six mois en croyant
l'inventer.

Complet quand : chaque piste porte soit le choix qui la retient, soit la raison écrite qui
l'écarte — aucune ne reste muette.

## 3. Énoncer la capacité et la recette

Écrire la **capacité** gagnée en une phrase à l'indicatif — « le jeton vit dans le trousseau »,
pas « gérer les secrets » ni une liste de tâches. Elle ouvre le §1.1.

Définir la **recette** : comment on recettera la feature entière, à l'étape `Validation`. C'est la
clause dont dépend toute l'acceptation finale — si elle reste vague, `Validation` improvisera son
propre jugement, et le découpage n'aura rien à répartir entre les incréments.

**Elle s'écrit en Gherkin, en annexe B** (`D-054`) — *Étant donné / Quand / Alors*, la même
convention que les titres de test. Voir [`recette.md`](recette.md) pour la forme et des patrons.

⚠️ **Deux choses ne partent pas en annexe, et les confondre coûte cher :**

- **les règles d'atterrissage** — qu'une clause soit exemptée de tomber dans un incrément, ou
  qu'elle se réparte en charge sans se répartir en référentiel. Ce sont des **instructions au
  découpage**, pas des scénarios : elles restent dans le corps, là où le découpeur les lit ;
- **un inventaire**, si la feature en produit un — une liste close de ce que le produit doit
  permettre. Ce n'est pas de la recette mais une **spécification fonctionnelle détaillée** : sa
  place est le §1.2, et le convertir en scénarios détruirait l'exhaustivité qui en fait la valeur.

Complet quand : la capacité est une phrase, pas une liste, et chaque clause de recette est un
scénario Gherkin dont le *Alors* est observable par qui ne lit pas le code.

## 4. Compléter les champs structurels

Pour chacun, une réponse ou un **« sans objet » explicite** — jamais un silence :
- le **socle**, ce qui est déjà construit, par renvoi (§2.2) ;
- le **pré-requis**, nommé ou déclaré inexistant (§3.2) ;
- les **trois registres** — construit / tranché non construit / question ouverte (§3.1) ;
- les **invariants à ne pas casser** (§2.3).

⚠️ **Les invariants n'accueillent que le non-dérivable.** Une vertu déjà écrite dans `CLAUDE.md`
(zéro warning, suite verte, TDD) ou dans `architecture.md` (le noyau sans dépendance sortante) s'y
**renvoie**, elle ne s'y recopie pas — la recopier la fera diverger de sa source. N'y restent que
les invariants **de cette feature-ci**, que rien d'autre n'écrit : ce que trahir viderait la feature
de son motif. En général deux ou trois, pas dix.

Complet quand : les quatre champs portent chacun une réponse écrite, aucun n'est simplement
absent, et aucun invariant listé n'est déjà écrit ailleurs dans le dépôt.

## 5. Écrire le plan d'architecture

La spec est **fonctionnelle et technique** : la moitié fonctionnelle ne s'engage sur rien sans
l'autre (`D-049`). Écrire, en dernier — il se nourrit de tout ce qui précède :

- les **solutions techniques envisageables** ;
- **laquelle on priorise**, et pourquoi ;
- **comment on compte la concevoir** — assez pour qu'on voie que ça tient debout ;
- les **grandes dépendances** à ajouter ou modifier, nommées (un paquet, un framework, un service) ;
- **au moins un schéma**, en bloc `mermaid` : Linear le rend nativement. La convention visuelle
  vit dans `docs/design/schemas.md`.

**Établir les faits, ne pas les supposer.** Ce plan affirme que ça peut fonctionner — donc ce
qui est mesurable se mesure : une cohabitation de frameworks, une contrainte de packaging, un
comportement de bibliothèque sous concurrence. Une faisabilité citée de mémoire est ce que la
revue suivante démontera.

**L'échelle est celle du système et du module** (`D-053`) : quels composants, quelles frontières
entre eux, quelles dépendances externes. **Pas la forme des objets** — descendre aux classes ici est
le débordement le plus courant, et il est doublement coûteux : il fait le travail de `Planning`, qui
n'en sait pas encore assez pour le faire bien, et il périme avant d'être lu.

⚠️ **La profondeur est celle dont le découpage a besoin.** Le consommateur désigné de la spec est le
découpage : le plan doit porter assez de vue d'ensemble pour qu'on puisse tracer les **frontières**
entre incréments et donner à chacun son **orientation technique** — pas une ligne de plus. Le test
est concret : tenter le découpage. S'il bute, le plan est trop court ; s'il n'a rien à faire de ce
qu'on a écrit, le plan est trop long. Ce plan n'a **aucune autorité littérale** : un plan de design
d'incrément qui s'en écarte au contact du réel est dans son droit, il le dit et c'est tout.

⚠️ **N'écrire que ce qui est décidé. Ce dont le plan ne parle pas est ouvert par défaut.** Ne pas
lui ajouter une section « ce que je laisse au plan de design » : la liste de ce qu'on n'a pas tranché
est infinie, donc toute tentative de l'écrire est arbitraire — et elle **gonfle à chaque reprise**,
puisqu'une remarque soldée y dépose sa réserve. Elle finit par contenir plus de décisions que la
prose, et par contredire son propre titre. Les questions ouvertes qu'on sait déjà nommer ont leur
place, et elle est unique : le **registre du §4** — construit / tranché non construit / question
ouverte. Deux endroits pour la même chose divergent ; ici l'un des deux disait « aucune » pendant
que l'autre en listait six.

Complet quand : les quatre points portent chacun une réponse, au moins un schéma existe, et
chaque affirmation de faisabilité est soit mesurée, soit annoncée comme non vérifiée.

## 6. Publier le document

Un document Linear **distinct** de la Discovery, qui lui succède sans la fondre — renvoyer au
besoin, ne pas le rédiger une seconde fois. Ne pas y nommer les incréments : le découpage a lieu
au passage en `In Progress`, pas ici. Ne pas y écrire le plan de design : il appartient à
l'incrément, à sa prise.

**Le document suit le plan de `tickets.md` §2.2** (`D-054`) :

```
1. Spécification fonctionnelle      1.1 La solution retenue
                                    1.2 Spécifications fonctionnelles détaillées
                                    1.3 Hors périmètre fonctionnel
2. Spécification technique          2.1 Les choix, en bref
                                    2.2 Le plan d'architecture   (sous-parties libres)
                                    2.3 Les invariants à ne pas casser
3. État des décisions               3.1 Les trois registres
                                    3.2 Le pré-requis
Annexes                             A. L'arbitrage technico-fonctionnel
                                    B. Les scénarios de recette, en Gherkin
                                    C. Les mesures de faisabilité
```

⚠️ **Écrire le contenu, pas la méthode.** Un document qui explique ce qu'est une spec, comment il
se lit ou pourquoi il a deux registres est un document qui se commente au lieu de dire. La
définition vit dans `tickets.md` ; ici, on énonce. En particulier, **aucune table « où j'ai répondu
à quoi »** : cette vérification appartient au relecteur, et une table logée dans l'artefact
qu'elle décrit finit par diverger de lui.

⚠️ **Pas de titre maison.** Ceux du gabarit sont explicites pour qui arrive sans contexte — ce qui
est exactement la situation de l'agent qui consommera le document.

Complet quand : le document est publié, lié à la Discovery, suit le plan, et ne contient ni
incréments ordonnés, ni plan de design, ni commentaire sur sa propre nature.
