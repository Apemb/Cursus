# `cursus-cli`

La ligne de commande de Cursus. Elle donne aux revues le geste qui leur manquait : poser une
divergence qui **désigne** un passage précis d'un document Linear, et la **solder** en nommant ce qui
la solde.

C'est de l'**outillage de méthode**, pas une part du produit — le noyau déterministe est en C# et
ignore tout de ce dossier. Le pourquoi de cette séparation, et le prix payé pour une seconde stack,
sont en `D-044`.

## Pourquoi elle existe

Le MCP Linear ne sait pas **résoudre** un commentaire : son input n'a pas de champ de résolution.
GraphQL le sait. Sans cet outil, une revue empile des `## Tour N` dans un document au lieu de solder
remarque par remarque.

⚠️ **Une seconde raison a été retirée par la mesure, le 2026-07-30.** On croyait aussi que GraphQL
savait *ancrer* un commentaire sur un passage, là où le MCP ne savait pas. C'est faux : **personne ne
sait ancrer**. L'ancre de Linear est une marque posée dans l'état de l'éditeur, et aucune API ne
l'écrit — un commentaire de document créé par programme est **invisible**, rangé hors du texte avec
les résolus. Le détail mesuré est en `docs/reference/linear-api.md` §10d, la décision qui en découle
en `D-045`.

Ce que la CLI vise donc désormais : les remarques se posent sur le **projet** (pour une Discovery ou
une Spec) ou sur l'**issue** (pour un plan de design), où elles sont visibles sans marque, et le
passage visé est **désigné** par la citation plus un repère calculé.

## Installer

```bash
cd cursus-cli
npm install
```

C'est tout : **il n'y a pas de build**. Node ≥ 24 exécute la source TypeScript en dépouillant les
types à la volée, donc `bin/cursus.ts` — le `bin` déclaré par le `package.json` — *est* la commande.

Volontairement **pas de lien global** : la commande n'existe que dans le dépôt. Deux façons de
l'appeler, selon que [`mise`](https://mise.jdx.dev) est installé ou non :

```bash
cursus linear whoami     # avec mise : le PATH porte cursus-cli/bin dans ce dépôt seulement
./cursus linear whoami   # sans mise : le lien de la racine, invoqué par son chemin
```

Le `mise.toml` de la racine ajoute `cursus-cli/bin` au `PATH` à l'entrée dans le dépôt, et l'en
retire à la sortie — c'est ce qu'aucun lien global ne sait faire. Le dossier contient `cursus`, un
lien **sans extension** vers `cursus.ts` : l'extension compte, puisque Node ne dépouille les types
que sur un `.ts`, mais il résout le lien avant d'en décider.

Sans `mise`, le `./` n'est pas esquivable — un shell n'exécute pas un fichier du répertoire courant
sans lui.

Dans les deux cas, la commande veut être lancée **depuis la racine du dépôt** : elle lit
`.cursus/project.json` dans le répertoire courant, sans remonter l'arborescence.

Ce que le dépouillement des types coûte, et qu'il faut savoir avant d'écrire :

- les imports relatifs portent l'extension **`.ts`**, pas `.js` — c'est le fichier que Node ouvre ;
- la syntaxe TypeScript qui *produit du code* est interdite : `enum`, `namespace`, propriétés de
  paramètre de constructeur. `erasableSyntaxOnly` la refuse au typecheck, pour que la faute se
  découvre là plutôt qu'à l'exécution, sur le seul chemin qui l'emprunte.

## Les verbes

| Commande | Ce qu'elle fait |
|---|---|
| `linear login [-t <jeton>]` | Éprouve un jeton, puis le dépose au trousseau. Sans `-t`, il est demandé sans écho — ou lu sur l'entrée standard hors terminal |
| `linear logout` | Retire la connexion de cet espace, et le jeton qu'elle désignait |
| `linear whoami` | À qui l'on parle et de quel espace — éprouve toute la chaîne de résolution d'un coup |
| `linear doc list` | Les documents de l'espace, avec ce qui situe chacun |
| `linear doc show <réf>` | Le contenu d'un document. **Le préalable de `comment add`** : on ne cite pas un passage sans l'avoir sous les yeux |
| `linear comment add <réf> -q <passage> -b <markdown>` | Pose une remarque sur la **carte** qui porte le document, en situant le passage par un repère calculé |
| `linear comment list <réf> [-u]` | Les remarques posées sur cette carte, et lesquelles restent ouvertes (`-u` : les non soldées seulement) |
| `linear comment resolve <id> -w <raison>` | Solde une divergence en écrivant ce qui la solde |

`<réf>` désigne un document par identifiant d'issue (`CUR-45`), nom de projet, titre ou fragment de
titre. Une référence ambiguë est **refusée** en énumérant les candidats : une issue porte volontiers
sa Discovery, sa Spec et son plan, et en choisir un au hasard revient à commenter le mauvais une fois
sur deux.

`-q`, `-b` et `-w` acceptent `-` pour lire l'entrée standard — un corps Markdown multi-ligne n'a pas
à être échappé pour le shell.

## Où une remarque se pose, et ce qui la situe

⚠️ **Une remarque ne se pose pas sur le document.** Mesuré le 2026-07-30 (`D-045`) : un commentaire de
document est créé, `success: true`, il porte la bonne citation — et l'interface le range hors du texte
avec les résolus, où personne ne le lit. L'ancre est une marque `inlineComment` dans l'état Yjs du
document, que **seul le client écrit** : aucun chemin programmatique n'en produit.

Une remarque se pose donc sur la **carte** qui porte le document, déduite de `document.project` /
`document.issue` et jamais choisie par l'appelant : le **projet** pour une Discovery ou une Spec,
l'**issue** pour un plan de design. Le mapping a été vérifié sur les quatre documents de l'espace, sans
cas particulier. Un document attaché à rien fait échouer la commande franchement — il n'y a nulle part
où poser la remarque.

Ce qui remplace l'ancrage perdu est un repère **calculé**, en tête du corps :

```
*Ref : Discovery — Un agent pilote Cursus › 3. Face à ce besoin, comment pourrait-on y répondre ?*

<la remarque>
```

Il se déduit du passage — le titre le plus proche au-dessus de lui — donc un agent ne peut ni l'oublier
ni le fausser. Le **titre complet** du document y figure parce que c'est lui qui départage la Discovery
de la Spec, qui vivent sur la même carte. Le repère va dans le corps et non dans la citation parce que
l'interface **aplatit** `quotedText` sur une seule ligne : aucune mise en page n'y survit, alors que le
corps est du Markdown rendu.

⚠️ **Les dièses d'un bloc de code ne sont pas des titres**, et l'oublier coûte un repère faux : un
`# dotnet build …` dans un bloc `bash` serait retenu comme section. `headingAt` suit les clôtures. Le
cas a été rencontré pour de vrai à la première épreuve — un passage cité *à l'intérieur* d'un bloc
mermaid, dont le repère est resté le titre qui surplombe le bloc.

### Ce que `list` liste, et pourquoi ce n'est pas le document

`<réf>` désigne un document, mais ce qui est listé est sa **carte** — partagée : une Discovery et une
Spec vivent sur le même projet, donc les remarques des deux apparaissent, chacune portant son repère.
C'est cohérent avec la porte du cycle de revue, qui se ferme par carte et non par document : c'est le
projet qu'on juge dégrossi.

⚠️ **Le décompte ne compte que les racines.** Mesuré : la réponse qui solde un fil a son propre
`resolvedAt` **nul**. La compter ferait que *zéro remarque ouverte* ne serait jamais atteint, chaque
solde en ajoutant une.

## Ce qu'elle protège, et que Linear ne protège pas

⚠️ **Linear ne vérifie jamais une citation.** Mesuré : une citation *absente du document* est
acceptée, `success: true`. La remarque cite alors un passage que personne ne peut retrouver dans le
document — et rien à l'écran ne le signale.

`anchor.ts` est la seule garde qui existe. Elle refuse une citation introuvable, refuse une citation
**ambiguë** en disant combien de fois elle apparaît, tolère des blancs recopiés autrement — et envoie
le passage **du document**, jamais la frappe de l'appelant.

⚠️ **Son métier a changé le 2026-07-30, son code non.** Elle ne prépare plus une ancre pour Linear,
qui ne fait rien de la citation. Elle garantit qu'une citation **désigne un seul passage**, pour
l'humain et pour l'agent qui viendront corriger — ce qui rend le refus de l'ambiguïté *plus*
important qu'avant, pas moins : privée de surlignage, une remarque mal située ne se remarque plus à
l'œil, alors qu'un surlignage au mauvais endroit sautait aux yeux. C'est aussi de cet offset que le
repère de section est calculé, donc une citation ambiguë produirait un repère faux en silence.

Corollaire à connaître : une citation est une **empreinte**, pas une référence. Le document édité,
elle se périme en silence. Le signalement des citations périmées n'est pas construit.

## Solder, et pourquoi `--with` prend un texte

`commentResolve` accepte un `resolvingCommentId`, mais il doit désigner une **réponse du fil** : un
commentaire frère fait rendre un `INTERNAL_SERVER_ERROR` — un 500 nu, qui ressemble à une panne alors
que c'est une faute d'usage. Le solde s'écrit donc en deux temps, et `--with` prend **la raison**, pas
un identifiant : la réponse est créée, puis la résolution la nomme.

Ce n'est pas un contournement. La clause de `docs/methode/dod/feature/spec.md` §2 — *« reprise, ou
refusée avec sa raison écrite ; une divergence sans suite écrite n'est pas soldée »* — cesse d'être
une règle qu'on rappelle pour devenir une contrainte qu'on ne peut pas contourner.

## Le trousseau est partagé avec l'app

| | |
|---|---|
| Service | `cursus` |
| Compte | `tracker:{id}` |
| Valeur | **base64 d'UTF-8** |
| Registre | `$XDG_CONFIG_HOME/cursus/trackers.json`, sinon `~/.config/cursus/trackers.json` |

⚠️ **L'encodage n'est pas un détail.** `security -w` rend la valeur en hexadécimal dès qu'un octet
sort de l'ASCII imprimable, sans le signaler. Ranger en clair casserait l'app en silence, et
seulement sur les jetons contenant un accent. Même exigence pour le registre : `login` le réécrit en
**préservant les connexions d'un genre que cette CLI ignore**.

## Développer

```bash
npm test          # vitest
npm run typecheck # tsc strict, sources, tests et point d'entrée
```

Le standard du dépôt s'applique : suite verte, zéro erreur de typage. Les tests du trousseau
travaillent sur un trousseau **jetable**, jamais celui de l'utilisateur.
