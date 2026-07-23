using System;

using Cursus.Core.Projects;
using Cursus.Persistence;

namespace Cursus.App.ViewModels;

/// <summary>
/// Ce dont l'écran d'un projet ouvert a besoin, monté d'un bloc : son
/// <see cref="ProjectHost"/> (lancer, relire) et son magasin d'artefacts (suivre
/// le log d'une visite). Regroupés ici pour que la coquille les tienne comme un
/// tout — un projet à la fois, disposé quand on en change — sans apprendre que
/// c'est du SQLite ni du disque : c'est la racine de composition (App) qui lie ce
/// bundle au préréglage concret.
/// </summary>
public sealed class ProjectWorkspace : IDisposable
{
    public ProjectWorkspace(ProjectHost host, RunArtifactStore artifacts)
    {
        Host = host;
        Artifacts = artifacts;
    }

    public ProjectHost Host { get; }

    public RunArtifactStore Artifacts { get; }

    /// <summary>Disposer le workspace ferme le host — et donc l'unique connexion SQLite du projet.</summary>
    public void Dispose() => Host.Dispose();
}
