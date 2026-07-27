# DoD — pas, sortie de `Todo`

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

> **La question** : ce pas peut-il être **tiré** en `In Progress` ?
>
> **Le flux est tiré.** Une DoD n'est pas une condition de sortie que l'amont s'applique à
> lui-même : c'est **ce que l'aval vérifie avant de tirer**. Celui qui prend le pas et bute sur un
> manque le repose plutôt que de deviner.
>
> Le *contenu* attendu est en `tickets.md` §4. Ici : uniquement de quoi il faut s'être acquitté.

## 1. Le contexte tient dans la carte, sans la conversation

- [ ] Le titre tient en une action
- [ ] **Pourquoi celui-là, à cette place, et où il s'arrête** est écrit — le frère voisin nommé
      s'il éclaire la frontière. C'est la question la plus importante des trois, et la seule qui
      ne se rattrape pas (`tickets.md` §4, q.2)
- [ ] Le piège local est noté s'il y en a un ; son absence est un état légitime, pas un manque
- [ ] **Comportemental, jamais procédural.** Un chemin de fichier ou un numéro de ligne dans la
      carte est un signal d'alerte, pas un détail : ils périment avant la prise
      (`mattpocock-skills.md` §2.3, *durability over precision*)

## 2. Éligibilité mécanique

- [ ] L'incrément parent est `In Progress`
- [ ] Plus aucun `blockedBy` ouvert sur ce pas

## 3. Le critère opposable

> Un pas est prêt quand un agent qui n'a pas eu la conversation peut se mettre au travail sans
> revenir demander où ça s'arrête.

Il se **teste** : le premier « jusqu'où ça va » qui se pose une fois le travail commencé signale
un manque en amont, pas une question légitime du pas — reposer la carte, `Rework Needed`.

## 4. Ce qui n'est *pas* un critère ici

- **La test list.** Elle s'écrit à la prise du pas, jamais avant (`tickets.md` §4) — une carte qui
  la porte déjà a mangé l'étape suivante.
- **Une acceptation formelle.** Le pas n'en a pas ; la suite verte et le zéro warning valent pour
  tous les pas, ça n'a pas à être répété carte par carte.
- **Le plan d'archi.** Il appartient à l'incrément et s'écrit à sa prise, en `Planning`.
