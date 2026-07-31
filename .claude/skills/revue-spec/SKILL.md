---
name: revue-spec
description: Valider la conformité d'une spec de feature contre `docs/methode/dod/feature/spec.md`, avant que le découpage ne la tire (`flux.md` #3). Utiliser à la prise d'une carte de feature en colonne `Spec` dont l'artefact est publié, ou quand on demande de relire, valider ou faire la revue d'une spec.
---

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

Une **instance** du skill `revue` — l'invoquer (« Suis le protocole du skill `revue` ») avec le
mandat ci-dessous. Ce skill ne prononce que sur la **conformité** ; il ne juge jamais si c'est la
bonne chose à construire — ça n'a pas de référentiel, et ça revient à l'humain en tirant la carte
(`tickets.md` §6.3).

## 1. Exiger un contexte neuf

Refuser de continuer si cette session a participé à l'écriture de la spec, ou a vu le prompt ou la
conversation qui l'a produite. Un agent qui a co-écrit ne valide pas — sollicité quand même, il
rendra un verdict, et un faux accord est pire qu'aucune relecture (`tickets.md` §6.3, *le piège du
binôme*). Ce n'est pas une précaution cosmétique : une session neuve qui ne reçoit que l'artefact
détecte davantage d'erreurs, et nettement plus sur les erreurs critiques, qu'une relecture qui
reçoit en plus le prompt d'origine — recevoir l'intention de l'auteur **ancre** le relecteur
(`docs/reference/skills.md` §5.2, *contexte séparé contre répétition* ; `flux.md` §5, *Refermé*).

Complet quand : soit la session ne porte que le document `Spec`, sans le fil qui l'a produit, soit
elle s'arrête ici et le dit.

## 2. Passer le mandat à `revue`

Fournir l'artefact — le document `Spec` attaché à la feature — et exactement deux axes.

**Axe Conformité** (référentiel : `docs/methode/dod/feature/spec.md` §1 et §2, clause par clause —
les **douze** cases de §1, les trois de §2 ; les trois dernières de §1 portent le **plan
d'implémentation**, `D-049`). Une case sans réponse **et** sans « sans objet » explicite
est une **violation dure** ; l'omission silencieuse est le seul cas que la DoD interdit
(`tickets.md` §5). Une case répondue par un « sans objet » n'est pas une divergence.

⚠️ **Une case y exige un schéma, donc cet axe porte une contradiction de plus à chercher :** celle
entre le schéma et la prose qui l'annonce (`revue` §3, dernier paragraphe). C'est le défaut que deux
tours réels ont laissé passer, et il ne se voit pas en cochant des cases — la case « au moins un
schéma » était tenue **pendant que le schéma contredisait le paragraphe d'au-dessus**.

**Axe Découpabilité** (référentiel : `docs/methode/dod/feature/spec.md` §3 — *« une spec est finie
quand le découpage peut avoir lieu sans revenir poser de question »*). Ce n'est pas cochable :
tenter mentalement le découpage de la spec en incréments, et signaler chaque endroit où il
faudrait revenir demander quelque chose. Ce sont par nature des **jugements** — la DoD elle-même
le dit, *« si le découpage bute, le manque est dans la spec »*.

Complet quand : les deux rapports sont reçus séparément — l'un en violations dures citées contre
§1/§2, l'autre en points d'achoppement contre §3 — sans mélange entre eux.

## 3. Écarter ce qui n'est ni l'un ni l'autre axe

Si un constat porte sur si c'est **la bonne chose à construire** — une option d'architecture qui
semble mauvaise, un besoin qui semble mal choisi — ce n'est ni une violation dure ni un jugement de
découpabilité. C'est une question de **justesse**, sans référentiel dans la DoD ; elle revient à
l'humain, qui la prononce en tirant la carte (`tickets.md` §6.3). La noter à part, sous une ligne
**« hors mandat — justesse »**, jamais glissée dans l'un des deux axes.

Complet quand : aucune ligne des deux axes ne discute si la spec est un bon choix — seulement si
elle est conforme et découpable.

## 4. Poser le verdict, sans escalade

Poser `Done` si aucune violation dure ne reste sans réponse et si l'axe Découpabilité ne relève
aucun achoppement ; sinon `Rework Needed`, avec le point en litige de chaque axe qui motive le
refus. Ne jamais déplacer la carte.

Le régime `Spec` est *Trio*, pas *Boucle* (`tickets.md` §6.3, dernière ligne) : pas de compteur de
tours, pas d'assignation à relire ici — l'humain est déjà à la table ; c'est lui qui prononce
l'accord sur les divergences soldées, et qui tire la carte vers le découpage
(`docs/methode/dod/feature/spec.md` §2).

Complet quand : une étiquette est posée, jamais les deux, et aucune tentative de convergence
automatique au-delà de ce seul passage.
