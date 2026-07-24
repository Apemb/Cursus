using Cursus.Core.Workflows;
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
