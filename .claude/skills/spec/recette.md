> Annexe du skill `spec`, étape 3 — à ouvrir seulement si le premier jet de la recette reste flou.
>
> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

# Patrons de recette

Une recette n'est pas « ça marche ». Elle est faite de cas **observables**, vérifiables par
quelqu'un qui ne lit pas le code — le même juge que celui qui départage un incrément d'un pas
(`tickets.md` §1).

## La forme : Gherkin, en annexe B

Un **scénario par clause** (`D-054`). Le format est conventionnel : il se lit sans qu'on explique
comment le lire, y compris par qui ne connaît pas le dépôt.

```gherkin
Scénario : Deux clients à la fois
  Étant donné une instance de Cursus dont le serveur est activé
  Quand deux clients MCP distincts s'y connectent en HTTP
  Alors aucun ne fait naître une seconde instance de Cursus
```

La recette **est manuelle** — aucun Cucumber ne la joue, et rien n'oblige à s'interdire une
formulation qu'un moteur refuserait. Le format sert la lecture, pas l'outillage.

## Les règles

- **Un scénario par clause**, pas une prose continue. Le *Alors* porte le résultat **observable** ;
  s'il faut lire le code pour en juger, ce n'est pas un *Alors*.
- **Inclure la preuve négative** quand elle existe : ce qui doit **rester** vrai après le
  changement, pas seulement ce qui doit devenir vrai. C'est la partie qu'on oublie le plus souvent.
- **Plusieurs scénarios courts** plutôt qu'un critère fourre-tout. Un cas qui ne se vérifie pas seul
  se vérifie mal.
- **`Et` plutôt qu'un second `Alors`** quand une clause a plusieurs conséquences ; au-delà de trois,
  c'est le signe qu'il y avait deux scénarios.
- La recette se répartit ensuite entre les incréments, au découpage. Si un cas n'atterrit dans
  aucun incrément, le découpage aura un trou — un signal à surveiller plus tard, pas à résoudre
  ici.

## Ce qui n'est pas un scénario

⚠️ Deux choses se déguisent en recette et doivent rester ailleurs :

- **Une règle d'atterrissage** — « cette clause est exemptée de tomber dans un incrément », « celle-ci
  se répartit en charge mais pas en référentiel ». C'est une instruction au **découpage**. La mettre
  en Gherkin la rend illisible et la fait disparaître de là où le découpeur la lit : elle reste dans
  le corps.
- **Un inventaire** — une liste close de ce que le produit doit permettre. C'est une **spécification
  fonctionnelle détaillée** (§1.2), pas de la recette. Sa valeur tient à son exhaustivité et au fait
  qu'on la coche ligne à ligne ; en scénarios, elle devient trente répétitions de la même phrase, et
  ses lignes qui ne sont pas des comportements — une exception, un écart connu — n'y survivent pas.
