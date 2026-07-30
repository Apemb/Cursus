# Le PRD — un artefact abandonné par son promoteur, puis ressuscité par les agents

> **Pourquoi ce fichier.** Le *product requirements document* est la lingua franca de la
> spécification produit, et c'est le format que consomment les cadres agentiques existants —
> `docs/reference/task-master.md` est entièrement construit dessus. Nous avons, sans le nommer ainsi,
> **un PRD coupé en deux** : la Discovery et la Spec. Ce fichier établit ce que le concept recouvre
> vraiment, ce que sa trajectoire dit du risque qu'on prend, et où notre découpage tombe par rapport
> à lui. Sondé le **30 juillet 2026**.
>
> **Fiabilité des sources — et elle est plus faible que celle des six autres corpus.** Ce sondage est
> une passe **web**, pas une lecture de source primaire : aucun dépôt cloné, aucun fichier lu sur
> disque. Chaque affirmation porte donc son registre :
> **(lu)** = page effectivement récupérée et lue · **(rapporté)** = connu par une source secondaire
> qui le résume, l'original n'a pas été ouvert · **(non audité)** = chiffre qui circule, sans méthode
> vérifiable · **(mesuré ici)** = établi par notre propre sonde en source primaire.
>
> ⚠️ **Les deux textes fondateurs de Cagan n'ont pas été lus** — ni *How To Write a Good PRD* (2005),
> ni *Revisiting the Product Spec* (2006). Tout ce qui les concerne est **(rapporté)**. C'est la
> principale faiblesse de ce document, et elle est réparable : les deux vivent sur `svpg.com`.

---

## 1. Ce que le PRD est censé être

Un document unique qui rassemble, pour une fonctionnalité ou un produit : le problème, les
utilisateurs visés, les exigences fonctionnelles, les critères d'acceptation, et souvent la portée
technique. **Son trait définitoire est le regroupement** — besoin et solution dans le même artefact —
et c'est de là que viennent à la fois son utilité et sa critique.

## 2. Son promoteur l'a abandonné en un an

**(rapporté)** Marty Cagan écrit *How To Write a Good PRD* en **2005**. **Un an plus tard**, en 2006,
il publie *Revisiting the Product Spec* et cesse de le recommander à ses clients.

**(rapporté)** Sa raison n'est **pas** que le PRD serait inefficace. C'est qu'il est trop facile pour
un PM d'y passer trop de temps, et pas assez sur le produit
([UserVoice](https://uservoice.com/blog/is-the-product-requirements-document-dead)).

> ⚠️ **Cette objection est de coût, pas de validité.** La distinction compte pour nous : elle ne dit
> pas qu'écrire une spécification est faux, elle dit qu'on peut s'y noyer. Une méthode qui produit
> beaucoup de méthode tombe sous cette critique, quelle que soit la qualité de ce qu'elle produit.

## 3. Ce qu'il met à la place — la *Product Opportunity Assessment*

**(lu)** [SVPG, *Assessing Product Opportunities*](https://www.svpg.com/assessing-product-opportunities/).
Dix questions, à mener *avant* le travail de spécification, et présentées comme *« quick, lightweight,
yet effective »* — aucune durée ni longueur n'est prescrite.

| # | La question | Ce qu'elle vise |
|---|---|---|
| 1 | Quel problème exactement cela résout-il ? | proposition de valeur |
| 2 | Pour qui résout-on ce problème ? | marché cible |
| 3 | Quelle est la taille de l'opportunité ? | taille de marché |
| 4 | Quelles alternatives existent ? | paysage concurrentiel |
| 5 | Pourquoi sommes-nous les mieux placés ? | différenciateur |
| 6 | Pourquoi maintenant ? | fenêtre de marché |
| 7 | Comment met-on ce produit sur le marché ? | go-to-market |
| 8 | Comment mesure-t-on le succès / gagne-t-on de l'argent ? | métriques |
| 9 | Quels facteurs sont critiques au succès ? | exigences de la solution |
| 10 | **Au vu de ce qui précède, quelle est la recommandation ?** | **go / no-go** |

**(lu)** Le texte ne présente **pas** la POA comme remplaçant le PRD : elle le **précède**. Cagan a
donc lui aussi *deux moments*, et non un document unique — fait qui pèse sur la §5.

## 4. Puis la réduction agile, puis le retournement de 2026

**(rapporté)** En contexte agile, le PRD se réduit à un **one-pager**, adossé à un backlog de user
stories portant leurs critères d'acceptation
([Planio](https://plan.io/blog/one-pager-prd-product-requirements-document/)).

**(rapporté)** Puis le mouvement s'inverse. Le *spec-driven development* refait de la spécification
l'**artefact souverain** — celui dont l'implémentation est dérivée, vérifiée et gouvernée — et le
motif est neuf : le problème n'est plus la vitesse de production mais la **dérive**, du code plausible
et confiant qui résout le mauvais problème
([zeroshot](https://zeroshot.ghost.io/spec-driven-development-with-ai-coding-agents/)).

**(non audité)** Deux chiffres circulent pour étayer ce diagnostic, attribués à une étude GitClear
2025 : **41 %** du code généré par IA réécrit dans les six mois, et des prompts identiques ne
produisant une sortie cohérente que **62 %** du temps. Aucune méthode n'a été vérifiée. **Ne pas les
citer comme acquis** — même précaution que pour le « +500 % de PR » de `symphony.md`.

**(rapporté)** La formulation qui porte réellement le retournement, et qui vaut d'être retenue telle
quelle ([ChatPRD](https://www.chatprd.ai/learn/prd-for-ai-codegen)) :

> Un PRD est écrit pour des lecteurs **humains**, qui savent interpréter une ambiguïté et combler un
> trou avec le contexte de l'organisation. Les agents comblent les trous aussi — **mais pas comme on
> voudrait**.

**(rapporté)** Et une distinction que la littérature pose et que nous n'avions pas posée
([SSOJet](https://ssojet.com/blog/prd-spec-templates-ai-agents)) : les outils **copilotes** prospèrent
sur l'itération *dans* le document ; les agents **autopilotes** exigent l'exhaustivité *en amont*. Ce
ne sont pas deux qualités de spécification, ce sont deux régimes.

---

## 5. La cartographie avec notre dispositif

**Nous avons un PRD, coupé en deux à l'endroit précis où le PRD classique brouille.** La
correspondance est plus serrée que ce que l'emprunt aurait produit — elle n'est pas un emprunt, les
deux découpages ont été faits indépendamment.

| POA de Cagan | Chez nous | Où |
|---|---|---|
| problème résolu · pour qui · pourquoi maintenant | presque mot pour mot | `dod/feature/discovery.md` §1 |
| alternatives existantes | les **pistes ouvertes**, et l'interdiction d'en écarter une | `dod/feature/discovery.md` §2 |
| recommandation go / no-go | la sortie légitime vers `Canceled` | `cycle-feature.md` §3 |
| facteurs critiques de succès | les **vertus qui doivent survivre** | `dod/feature/spec.md` §1 |
| taille de marché · pourquoi nous · go-to-market · monétisation | **absents** — projet solo, pas de marché | — |

L'autre moitié du PRD classique — exigences et acceptation — est notre **Spec** (capacité énoncée à
l'indicatif, recette définie), qui redescend en **acceptation d'incrément** au découpage.

**Le point qui rassure, et il n'était pas acquis** : Cagan place sa POA *avant* la spécification
(§3). `D-041` a coupé au même endroit, pour un motif écrit sans connaître le sien — *réunies,
Discovery et Spec invitent à arbitrer en rédigeant le besoin, ce que la Discovery s'interdit*.
Convergence indépendante, pas imitation.

## 6. Ce que la littérature n'a pas, et que nous avons

Toutes les sources ci-dessus produisent des **templates** : ils disent quelles sections doivent
exister. Aucune ne produit de **critère de suffisance** : ce qui fait qu'une section est assez
remplie pour qu'on avance.

C'est exactement ce que sont nos **treize DoD** — un référentiel opposable par niveau et par statut,
avec pour chacun une section « ce qui n'est *pas* un critère ». C'est une catégorie d'artefact que ce
sondage n'a rencontrée nulle part. À confronter à `bmad.md`, dont le constat était déjà qu'**aucun
cadre existant n'a de tiers bloquant**.

## 7. Le contre-exemple, et il est mesuré chez nous

**(mesuré ici)** `docs/reference/task-master.md` §1 démonte le seul outil qui fasse vraiment
PRD → tâches, sur source primaire. Trois faits, qui composent l'avertissement le plus utile de ce
document :

1. **Aucune capacité de rédaction n'existe.** Ni commande, ni outil MCP, ni skill ne *produit* un
   PRD ; la rédaction est renvoyée au chat libre. Le seul outillage agit **après** que le fichier
   existe.
2. **Un template de 511 lignes, richement instrumenté, qu'aucun chemin d'exécution ne charge** — et
   que ses propres auteurs n'ont jamais employé sur leurs cinq PRD réels.
3. **Le prompt de `parse-prd` porte l'instruction inverse d'un gate de clarification** :
   *« Focus on filling in any gaps left by the PRD »*, avec *« infer … based only on the PRD
   content »*.

Le troisième est la phrase de ChatPRD (§4) réalisée en prompt système : **l'ambiguïté est absorbée,
jamais levée.** C'est le mode d'échec que tout le mouvement SDD prétend traiter, présent dans
l'implémentation de référence de ce mouvement.

## 8. Ce qu'on en retient, et ce qu'on refuse

- **Le mot « PRD » n'est pas adopté.** Il nomme un *document* là où nos trois niveaux nomment des
  *décisions*, et il charrie le regroupement besoin+solution que `D-041` a défait exprès.
- **Mais une table de correspondance sera nécessaire** le jour où Cursus consommera des tickets venus
  d'ailleurs : personne n'écrira « Discovery au sens de `D-041` ». La §5 en est le brouillon.
- **L'objection de coût de Cagan (§2) nous vise directement** et ne se réfute pas par l'argument
  qu'on s'est donné. Le référentiel écrit d'avance se défend **une fois** — parce qu'il rend un échec
  attribuable. Si un second cycle produit encore de la méthode plutôt que du produit, c'est
  l'objection qui a raison, et il faudra l'écrire.
- **La question copilote / autopilote (§4) est ouverte chez nous.** Nos documents de cycle sont
  écrits pour l'autopilote — l'agent ne demande rien — alors qu'on opère en copilote, l'humain dans
  le binôme. ⚠️ Et le partage n'est pas libre : `interrogatoire` est bâti sur *« pose une question,
  attends la réponse de l'humain, elle seule »*, et `discovery` §2 l'invoque. **Ces deux-là ne
  peuvent pas tourner en headless** — un agent privé d'interlocuteur inventera les réponses, ce qui
  est le faux succès qu'on cherche à détecter. Lesquels des treize skills sont autopilotables est
  donc une question à trancher **par la mesure**, pas par la doctrine.

## 9. Ce qui n'a pas été lu, et vaudrait de l'être

- **Cagan, *How To Write a Good PRD* (2005)** et ***Revisiting the Product Spec* (2006)**, sur
  `svpg.com` — les deux textes qui fondent tout le §2, connus uniquement par un résumé tiers.
- **L'étude GitClear 2025** dont sortent les deux chiffres du §4, jamais auditée.
- **Le PR-FAQ / narrative d'Amazon**, cité nulle part dans cette passe alors qu'il est l'autre
  réponse historique au PRD, et qu'il partage avec notre Discovery l'interdiction d'arbitrer trop tôt.
