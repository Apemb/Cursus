using System;

using Cursus.Core.Projects;
using Cursus.Persistence;

namespace Cursus.App.ViewModels;

/// <summary>
/// Ce dont l'écran d'un projet ouvert a besoin, monté d'un bloc : son
/// <see cref="ProjectHost"/> (lancer, relire), son magasin d'artefacts (suivre
/// le log d'une visite) et son <see cref="WorkflowCatalog"/> (créer, renommer,
/// supprimer, ouvrir pour éditer). Regroupés ici pour que la coquille les tienne
/// comme un tout — un projet à la fois, disposé quand on en change — sans apprendre
/// que c'est du SQLite ni du disque : c'est la racine de composition (App) qui lie
/// ce bundle au préréglage concret.
///
/// <para>
/// Le catalogue vit ici, et non dans le host : « lister et charger restent
/// <see cref="WorkflowCatalog"/> » (doc du <see cref="ProjectHost"/>) — le router
/// par le host en ferait un Service Locator. C'est un wrapper sans état sur le
/// disque du projet, sans connexion à disposer, d'où l'absence de traitement dans
/// <see cref="Dispose"/>.
/// </para>
/// </summary>
public sealed class ProjectWorkspace : IDisposable
{
    public ProjectWorkspace(ProjectHost host, RunArtifactStore artifacts, WorkflowCatalog catalog)
    {
        Host = host;
        Artifacts = artifacts;
        Catalog = catalog;
    }

    public ProjectHost Host { get; }

    public RunArtifactStore Artifacts { get; }

    /// <summary>Le catalogue des workflows du projet : la gestion de leur cycle de vie (créer/renommer/supprimer/éditer).</summary>
    public WorkflowCatalog Catalog { get; }

    /// <summary>Disposer le workspace ferme le host — et donc l'unique connexion SQLite du projet.</summary>
    public void Dispose() => Host.Dispose();
}
