#!/usr/bin/env node

// Le point d'entrée exécutable. Node dépouille les types à la volée, donc ce fichier est
// **la** commande : rien à construire avant de l'appeler, aucun build à relancer après
// avoir modifié la source.

import {run} from "../src/index.ts";

// Le filet de dernier recours. Les échecs des commandes sont déjà traduits en code de
// sortie par `execute` ; ne remonte jusqu'ici que ce qui échappe au câblage — une erreur
// d'analyse de Commander, par exemple, qui porte son propre `exitCode`.
run(process.argv).catch((error: unknown) => {
    const failure = error as { message?: string; exitCode?: number };
    process.stderr.write(`Erreur : ${failure.message ?? String(error)}\n`);
    process.exit(failure.exitCode ?? 1);
});
