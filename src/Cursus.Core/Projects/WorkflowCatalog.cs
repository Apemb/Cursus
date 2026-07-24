using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Editing;
using Cursus.Core.Workflows.Serialization;

namespace Cursus.Core.Projects;

/// <summary>
/// Les workflows que porte un projet. Ne traduit rien lui-même : il apporte le
/// disque et l'identité, et délègue la lecture du document au sérialiseur.
/// </summary>
public sealed class WorkflowCatalog(Project project)
{
    /// <summary>
    /// Énumère sans rien ouvrir : un document cassé se découvre au chargement,
    /// il ne doit pas rendre le projet entier inutilisable.
    /// </summary>
    public IReadOnlyList<WorkflowEntry> List() =>
        Directory.EnumerateFiles(project.WorkflowsDirectory, "*.json")
                 .Select(path => new WorkflowEntry(Path.GetFileNameWithoutExtension(path), path))
                 // Tri explicite : l'ordre d'énumération du système de fichiers
                 // n'est garanti nulle part, et une liste qui se réordonne toute
                 // seule d'un affichage à l'autre est incompréhensible.
                 .OrderBy(entry => entry.Id, StringComparer.Ordinal)
                 .ToList();

    /// <summary>
    /// Lit le document et le confie au sérialiseur. Un identifiant qu'aucun
    /// fichier ne porte lève le <see cref="FileNotFoundException"/> du
    /// framework : l'invariant violé est celui du système de fichiers, pas celui
    /// du catalogue.
    /// </summary>
    public LoadResult Load(string id) =>
        WorkflowSerializer.Read(File.ReadAllText(PathOf(id)));

    /// <summary>
    /// La porte sœur de <see cref="Load"/>, <b>pour éditer</b> : rend la définition
    /// parsée même invalide, la validité se lisant dans le rapport. C'est ainsi
    /// qu'un brouillon cassé — que <see cref="Save"/> a laissé écrire — se rouvre
    /// pour être corrigé, là où <see cref="Load"/> l'aurait annulé. Même convention
    /// qu'elle pour l'absence : un identifiant qu'aucun fichier ne porte lève le
    /// <see cref="FileNotFoundException"/> du framework.
    /// </summary>
    public ParsedWorkflow Open(string id) =>
        WorkflowSerializer.ReadEditable(File.ReadAllText(PathOf(id)));

    /// <summary>
    /// Fait naître un workflow vide — sans point d'entrée ni étape. Il est donc
    /// <b>invalide mais éditable</b> : « brouillons permis » commence dès la
    /// naissance, l'éditeur remplit ensuite.
    /// </summary>
    public void Create(string id)
    {
        RefuseToOverwrite(id);
        File.WriteAllText(PathOf(id), WorkflowSerializer.Write(new WorkflowDefinition("", [])));
    }

    /// <summary>
    /// Fait naître un workflow à partir de son <b>titre humain</b> : l'id du fichier
    /// en est le slug, et il est <b>retourné</b> pour que l'appelant ouvre aussitôt
    /// l'éditeur dessus. Jumelle symétrique de <see cref="WorkflowDraft.AddStep"/>,
    /// qui slugifie de même le titre d'une <em>étape</em> — sauf qu'ici on
    /// <b>refuse</b> une collision (via <see cref="Create"/>) au lieu de
    /// désambiguïser : le nom d'un fichier de workflow est un choix délibéré, pas un
    /// id dérivé en masse. Un titre qui slugifie en chaîne vide lève
    /// l'<see cref="InvalidWorkflowIdException"/> du choke <see cref="PathOf"/>.
    /// </summary>
    public string CreateFromTitle(string title)
    {
        var id = Slug.From(title);
        Create(id);
        return id;
    }

    /// <summary>
    /// Persiste une définition telle quelle — <b>sans la valider</b>. Un graphe
    /// cassé se sauvegarde comme un graphe sain : c'est le chargement qui
    /// signalera ses problèmes. C'est là que vit « brouillons permis ».
    /// </summary>
    public void Save(string id, WorkflowDefinition definition) =>
        File.WriteAllText(PathOf(id), WorkflowSerializer.Write(definition));

    /// <summary>
    /// Retire le fichier du workflow. Un identifiant qu'aucun fichier ne porte
    /// lève le <see cref="FileNotFoundException"/> du framework — même convention
    /// que <see cref="Load"/> : <see cref="File.Delete"/> serait silencieux, mais
    /// supprimer un absent est une méprise qui mérite d'être signalée.
    /// </summary>
    public void Delete(string id)
    {
        var path = PathOf(id);
        if (!File.Exists(path))
            throw new FileNotFoundException("Aucun workflow à supprimer sous cet identifiant.", path);

        File.Delete(path);
    }

    /// <summary>
    /// Déplace le workflow sous un nouvel identifiant. Refuse d'écraser une cible
    /// déjà prise (<see cref="WorkflowAlreadyExistsException"/>) plutôt que de
    /// laisser <see cref="File.Move"/> lever une <see cref="IOException"/>
    /// anonyme ; un <paramref name="oldId"/> absent lève, lui, le
    /// <see cref="FileNotFoundException"/> du framework, comme <see cref="Load"/>.
    /// </summary>
    public void Rename(string oldId, string newId)
    {
        RefuseToOverwrite(newId);
        File.Move(PathOf(oldId), PathOf(newId));
    }

    /// <summary>
    /// Le garde-fou commun de <see cref="Create"/> et <see cref="Rename"/> :
    /// écraser une identité déjà prise changerait silencieusement le contenu d'un
    /// autre workflow. Même refus que <c>ProjectStore.Create</c>.
    /// </summary>
    private void RefuseToOverwrite(string id)
    {
        if (File.Exists(PathOf(id)))
            throw new WorkflowAlreadyExistsException(id);
    }

    /// <summary>
    /// Le seul endroit qui compose un chemin de fichier à partir d'un identifiant
    /// — donc le seul point de choke où en vérifier la légalité. Un séparateur de
    /// chemin ferait échapper le fichier du dossier des workflows ; un identifiant
    /// vide ne désigne rien.
    /// </summary>
    private string PathOf(string id)
    {
        if (string.IsNullOrWhiteSpace(id)
            || id.Contains(Path.DirectorySeparatorChar)
            || id.Contains(Path.AltDirectorySeparatorChar))
            throw new InvalidWorkflowIdException(id);

        return Path.Combine(project.WorkflowsDirectory, $"{id}.json");
    }
}

/// <summary>Un workflow présent dans le projet, désigné par son fichier.</summary>
public sealed record WorkflowEntry(string Id, string Path);
