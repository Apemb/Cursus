> Annexe du skill `spec`, étape 3 — à ouvrir seulement si le premier jet de la recette reste flou.
>
> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

# Patrons de recette

Une recette n'est pas « ça marche ». Elle est faite de cas **observables**, vérifiables par
quelqu'un qui ne lit pas le code — le même juge que celui qui départage un incrément d'un pas
(`tickets.md` §1).

- **Un cas par ligne**, pas une prose continue. Forme à préférer : « Étant donné `<état>`, le
  parcours `<action>` produit `<résultat observable>` ».
- **Inclure la preuve négative** quand elle existe : ce qui doit **rester** vrai après le
  changement, pas seulement ce qui doit devenir vrai. C'est la partie qu'on oublie le plus souvent.
- **Plusieurs cas courts** plutôt qu'un critère fourre-tout. Un cas qui ne se vérifie pas seul se
  vérifie mal.
- La recette se répartit ensuite entre les incréments, au découpage. Si un cas n'atterrit dans
  aucun incrément, le découpage aura un trou — un signal à surveiller plus tard, pas à résoudre
  ici.
