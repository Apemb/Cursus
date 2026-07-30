> **Écrit avant d'avoir été exécuté.** Ce fichier et les trois documents de cycle qu'il sert
> décrivent un dispositif dont **aucun tour n'a encore tourné**. Ils existent pour être le
> référentiel contre lequel une exécution réelle sera jugée — sans quoi un échec ne se range ni
> dans « le cycle est mauvais », ni dans « le skill est mauvais », ni dans « la méthode est
> mauvaise ». C'est leur seule justification, et c'est aussi ce qui les rend révisables : au
> premier désaccord avec le terrain, `docs/methode/journal-frictions.md` prime.

# Le cycle de revue — vocabulaire commun aux trois niveaux

> **À quoi sert ce fichier.** Il donne le **vocabulaire d'états** que les trois documents de
> cycle emploient, et la mécanique de la boucle. Il ne dit pas ce qui se passe à chaque niveau —
> ça, c'est [`cycle-feature.md`](cycle-feature.md), [`cycle-increment.md`](cycle-increment.md) et
> [`cycle-pas.md`](cycle-pas.md) ; ni les **gestes** — ça, c'est les skills ; ni les **critères
> opposables** — ça, c'est [`dod/`](dod/) ; ni ce que **contient** un ticket — ça, c'est
> [`tickets.md`](tickets.md).
>
> **Pourquoi il existe.** Le vocabulaire est identique aux trois niveaux. Le recopier trois fois
> serait garantir qu'il diverge — et un document de méthode faux est pire qu'aucun document.

---

## 1. Deux axes, et ils ne disent pas la même chose

| Axe | Ce qu'il porte | Ce qu'il permet |
|---|---|---|
| **La colonne** | *quel travail se fait* | Linear calcule ses temps de cycle sur les **transitions de statut** — donc seul cet axe produit une métrique comparable avec celle d'une autre équipe |
| **L'étiquette** | *où en est le cycle de ce travail* | Se déplace sans migrer le tableau — donc c'est l'axe qui reste remodelable tant qu'un processus n'est pas stabilisé |

**Le choix de l'axe se fait par niveau, pas par temps** — et le critère est : *ce processus est-il
stabilisé au point de mériter d'être mesuré, ou encore assez diffus pour qu'on veuille le
déplacer ?* Conséquence concrète, développée dans chaque document : à l'**incrément**, une
frontière de colonne sépare l'écriture de la revue (`Planning` › `Plan Review`, `In Progress` ›
`Code Review`) ; à la **feature**, non — la revue se joue **dans** `Discovery` et dans `Spec`.

**Ce que ni l'un ni l'autre axe ne porte : le nombre de tours.** Il se **compte** sur les fils de
remarques — chaque réponse dans un fil *est* un tour (`D-045` §7). Il ne sortira donc d'aucun
rapport Linear, et c'est assumé : un compteur qui se déclare se fausse.

---

## 2. Les six états

Cinq étiquettes **mutuellement exclusives** dans le groupe `Advancement Labels` — Linear impose
l'exclusivité au sein d'un groupe, et c'est ce qui fait de la colonne « étiquette » un état et non
un sac. L'**absence** d'étiquette est le sixième état, et c'est l'état initial.

| Temps | Étiquette | Ce que ça veut dire | Qui est appelé |
|---|---|---|---|
| ① | *aucune* | L'artefact s'écrit | l'auteur |
| ② | `Review Requested` | Écrit, à relire contre sa DoD | un relecteur tiers |
| ③ | `Rework Needed` | Des remarques sont ouvertes, à reprendre | **variable selon le niveau — voir §5** |
| ④ | `Rework Done` | Les reprises sont faites, à vérifier une par une | un vérificateur |
| ⑤ | `Human Review` | La boucle agent est sèche, l'humain relit | l'humain |
| ⑥ | `Done` | Zéro remarque ouverte, la carte est **tirable** | l'aval, qui tire |

Et une étiquette **hors groupe**, donc cumulable avec l'état :

| Étiquette | Ce que ça veut dire |
|---|---|
| `Escalated` | La boucle agentique n'est pas arrivée au bout toute seule. Ne remplace pas l'état, elle le qualifie |

**`Done` n'avance pas la carte, elle autorise qu'on l'avance** (`D-041`). Le flux est *tiré* : la
colonne dit *« ça se fait ici »*, jamais *« c'est fini »*, et c'est l'aval qui tire après avoir
vérifié la DoD. `Done` est le verdict de **conformité**, prononcé par le relecteur contre un
référentiel écrit ; la **justesse** — *est-ce ça qu'on veut ?* — reste à l'humain, et il la
prononce en tirant.

---

## 3. La boucle, et comment elle s'arrête

```
       ┌──────────────────────────────────────┐
       ▼                                      │
② Review Requested ──► ③ Rework Needed ──► ④ Rework Done
       │                                      │
       │  aucune remarque                     │  toutes soldées
       ▼                                      ▼
   ⑥ Done  ◄─────────────────────────────────┘
       ▲
       │  l'humain a tranché
   ⑤ Human Review  ◄── la boucle ne peut plus avancer
```

La porte de sortie est **mécanique et vérifiable** : zéro remarque ouverte. Elle se lit par

```bash
cursus linear comment list <référence> --unresolved
```

dont le champ `open` ne compte que les remarques **racines** — une réponse qui solde reste
`resolvedAt: null`, et la compter ferait que la porte ne se refermerait jamais (`D-046`).

**La terminaison est par remarque, l'étiquette est par carte.** Une remarque a droit à **deux**
passes correction/vérification ; au troisième désaccord, le vérificateur écrit dans son fil la
réponse qui nomme le litige — elle est un tour de plus, donc elle se compte — et cette remarque
cesse de circuler. La boucle continue sur les autres. Quand plus aucune remarque ne peut avancer,
la carte passe en `Human Review`, et porte en plus `Escalated` si au moins une remarque a atteint
son troisième désaccord.

**Ce que `Escalated` est vraiment.** Pas une alarme : un **fait mesurable**. Compter ses occurrences
par colonne dit où la boucle agentique ne tient pas — et la conclusion peut être que le skill de
revue est à refaire, pas que l'artefact était mauvais.

---

## 4. Qui pose l'étiquette

**L'agent qui finit son temps pose l'étiquette du temps suivant.** Il n'y a pas d'autorité
centrale, et c'est délibéré : c'est le seul régime qui fonctionne sans moteur, donc le seul qui
marche aujourd'hui.

Un skill **ne déplace jamais la carte** — il pose l'étiquette et s'arrête là
([`revue`](../../.claude/skills/revue/SKILL.md) §8). Déplacer est l'acte de celui qui **tire**, et
c'est ce qui rend le flux tiré observable plutôt que déclaratif.

---

## 5. Le piège : la même étiquette n'appelle pas le même acteur

`Rework Needed` convoque **celui qui a écrit**. Or l'auteur n'est pas le même à tous les niveaux :

| Niveau | L'auteur du temps ① | Donc `Rework Needed` convoque |
|---|---|---|
| **Feature — `Discovery`** | binôme humain ⇄ agent | **l'humain**, avec son agent |
| **Feature — `Spec`** | binôme humain ⇄ agent, puis un agent correcteur | un agent |
| **Incrément** | un agent | un agent |

⚠️ **C'est l'endroit où on se trompe en lisant vite.** En `Discovery`, une carte qui porte
`Rework Needed` n'attend pas une machine : elle attend une personne. La §6 dit pourquoi.

---

## 6. Quand les temps ③ et ④ existent — et quand ils n'existent pas

**Un agent correcteur ne se justifie que là où la correction est textuelle.**

En `Spec`, en `Plan Review`, en `Code Review`, une remarque désigne un manque **dans l'artefact** :
un écart non écrit, une capacité formulée en liste de tâches, un test dont le titre ne dit pas ce
qu'il vérifie. Tout cela se reprend en relisant l'artefact et son référentiel — donc un agent le
fait, et un autre agent vérifie qu'il l'a fait.

En `Discovery`, ce qui manque à une remarque n'est presque jamais de la prose : c'est de la
**matière** — un entretien qui n'a pas eu lieu, une piste qu'on n'a pas explorée, une hypothèse
non testée. C'est ce que dit l'état de l'art de la discovery continue, où l'alignement se fait par
la preuve et non par le document, et où la production est portée par un binôme ou un trio plutôt
que par une chaîne producteur → correcteur. Un agent correcteur y produirait de la prose plus
lisse sur un besoin toujours aussi mal établi — un faux succès, qui est le mode de défaillance
dominant (`docs/reference/skills.md`).

D'où deux formes de cycle, et le critère qui les départage :

| Forme | Où | Temps |
|---|---|---|
| **Cycle court** | `Discovery` | ① binôme → ② revue → ① binôme → … → ⑥ |
| **Cycle complet** | `Spec`, `Plan Review`, `Code Review` | ① → ② → ③ → ④ → (② …) → ⑤ → ⑥ |

---

## 7. Comment se lit une table de transition

Chaque document de cycle porte, par colonne, une table à quatre colonnes :

| État observé | Skill invoqué | Livrable | État posé |
|---|---|---|---|

- **État observé** — le couple `(colonne, étiquette)`, augmenté du compte de remarques ouvertes
  quand il départage deux lignes. C'est tout ce qu'il faut lire pour savoir quoi faire.
- **Skill invoqué** — le fichier qui porte le geste. La table ne recopie **jamais** le geste ; un
  skill qui vivrait en double serait faux dans l'une de ses deux copies avant un mois.
- **Livrable** — ce qui doit exister quand le temps est fini. C'est ce qu'un vérificateur cherche.
- **État posé** — l'étiquette laissée en sortie. Quand deux sorties sont possibles, la table les
  sépare par `|` et dit ce qui les départage.

**La table ne nomme pas son exécutant, et c'est le point.** Aujourd'hui c'est un humain qui la
parcourt et lance chaque temps à la main. Demain c'est Cursus, qui route sur des codes de sortie.
Le texte est le même : la table **est** le workflow, écrit d'avance dans une forme qui n'a pas
besoin d'être traduite. C'est le seul choix de forme de ces documents qui soit irréversible, et
c'est pour ça qu'il est explicite.

---

## 8. Registre

**Construit** : le vocabulaire n'existe qu'à moitié dans Linear — `Rework Needed` et `Done` sont
en place depuis `D-041` ; `Review Requested`, `Rework Done`, `Human Review` et `Escalated` sont à
créer. Côté outillage, les gestes de la boucle sont construits et éprouvés contre le vrai Linear :
poser une remarque située, lister les ouvertes, en solder une en écrivant ce qui la solde
(`D-046`).

**Tranché mais pas construit** : tout le reste de ce fichier. Aucun tour n'a tourné. Les deux
primitifs que le cycle réclame — `correction` et `verification` — **n'existent pas**, et
`revue-discovery` non plus.

**Questions ouvertes** :

- **Deux passes est un chiffre repris de `D-045`, pas une mesure.** Il n'a jamais été confronté à
  une boucle réelle. Trop bas, il remonte du bruit ; trop haut, il laisse un correcteur tourner
  sur une exigence qu'il ne sait pas satisfaire.
- **Distinguer trois tours sur le même litige de trois tours qui dérivent de sujet.** Ils se
  comptent pareil, ne valent pas pareil, et le second ressemble à du progrès.
- **Le vérificateur complaisant** — celui qui solde tout et referme la boucle sans rien avoir
  obtenu — n'a pas de garde-fou propre. Il est couvert par le régime *relecteur chicanier*
  (`docs/reference/skills.md` §5.5) et par la double issue qu'une DoD laisse à une divergence :
  reprise, **ou** refus motivé. Aucun des deux n'a été éprouvé.
