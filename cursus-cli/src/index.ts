import {Command} from "commander";

import {commentAdd, commentList, commentResolve} from "./commands/comment.ts";
import {docList, docShow} from "./commands/doc.ts";
import {login, logout} from "./commands/login.ts";
import {whoami} from "./commands/whoami.ts";
import {execute} from "./output.ts";

export async function run(argv: string[]): Promise<Command> {
    const program = new Command();

    program
        .name("cursus")
        .description("La ligne de commande de Cursus.")
        .version("0.1.0");

    const linear = program
        .command("linear")
        .description("Ce qui se passe sur le tableau Linear du projet courant.");

    linear
        .command("login")
        .description("Dépose un jeton Linear au trousseau, après l'avoir éprouvé.")
        .option("-t, --token <jeton>", "le jeton ; à défaut il est demandé sans écho, ou lu sur l'entrée standard")
        .action((options: { token?: string }) => execute(() => login(options)));

    linear
        .command("logout")
        .description("Retire la connexion de cet espace, et le jeton qu'elle désignait.")
        .action(() => execute(logout));

    linear
        .command("whoami")
        .description("À qui parle-t-on, et de quel espace — éprouve toute la chaîne de résolution.")
        .action(() => execute(whoami));

    const doc = linear.command("doc").description("Les documents attachés aux projets et aux issues.");

    doc
        .command("list")
        .description("Les documents de l'espace, avec ce qui situe chacun.")
        .action(() => execute(docList));

    doc
        .command("show <référence>")
        .description("Le contenu d'un document — par identifiant d'issue (CUR-45), nom de projet ou titre.")
        .action((reference: string) => execute(() => docShow(reference)));

    const comment = linear
        .command("comment")
        .description("Les commentaires ancrés sur les documents — poser, lister, solder.");

    comment
        .command("add <référence>")
        .description("Pose un commentaire ancré sur un passage exact du document.")
        .requiredOption("-q, --quote <passage>", "le passage à citer, tel qu'il figure dans le document ; « - » pour l'entrée standard")
        .requiredOption("-b, --body <markdown>", "le corps du commentaire ; « - » pour l'entrée standard")
        .action((reference: string, options: { quote: string; body: string }) =>
            execute(() => commentAdd(reference, options)),
        );

    comment
        .command("list <référence>")
        .description("Les commentaires d'un document, et lesquels restent ouverts.")
        .option("-u, --unresolved", "ne rendre que les commentaires non soldés")
        .action((reference: string, options: { unresolved?: boolean }) =>
            execute(() => commentList(reference, options)),
        );

    comment
        .command("resolve <commentId>")
        .description("Solde une divergence en écrivant ce qui la solde.")
        .requiredOption("-w, --with <raison>", "la reprise faite, ou le refus et sa raison ; « - » pour l'entrée standard")
        .action((commentId: string, options: { with: string }) =>
            execute(() => commentResolve(commentId, options)),
        );

    return await program.parseAsync(argv);
}
