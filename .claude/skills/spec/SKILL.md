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
pas « gérer les secrets » ni une liste de tâches.

Définir la **recette** : comment on recettera la feature entière, à l'étape `Validation`. C'est la
clause dont dépend toute l'acceptation finale — si elle reste vague, `Validation` improvisera son
propre jugement, et le découpage n'aura rien à répartir entre les incréments. Voir
[`recette.md`](recette.md) pour des patrons de recette si le premier jet reste flou.

Complet quand : la capacité est une phrase, pas une liste, et la recette énonce des cas
observables plutôt qu'un critère du type « ça marche ».

## 4. Compléter les champs structurels

Pour chacun, une réponse ou un **« sans objet » explicite** — jamais un silence :
- le **socle**, ce qui est déjà construit, par renvoi ;
- le **pré-requis**, nommé ou déclaré inexistant ;
- les **trois registres** — construit / tranché non construit / question ouverte ;
- les **vertus qui doivent survivre**, les invariants que l'implémentation ne doit pas casser.

Complet quand : les quatre champs portent chacun une réponse écrite, aucun n'est simplement
absent.

## 5. Publier le document

Un document Linear **distinct** de la Discovery, qui lui succède sans la fondre — renvoyer au
besoin, ne pas le rédiger une seconde fois. Ne pas y nommer les incréments : le découpage a lieu
au passage en `In Progress`, pas ici. Ne pas y écrire le plan d'archi : il appartient à
l'incrément, à sa prise.

Complet quand : le document est publié, lié à la Discovery, et ne contient ni incréments ordonnés
ni plan d'archi.
