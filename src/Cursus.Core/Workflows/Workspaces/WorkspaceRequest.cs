namespace Cursus.Core.Workflows.Workspaces;

/// <summary>
/// Ce qu'un run veut comme point de départ pour son répertoire de travail isolé,
/// sans dire <b>comment</b> l'isoler. Les variantes sont imbriquées, comme celles
/// de <see cref="WorkflowEvent"/>. Le provisionneur ne forge jamais l'identité
/// d'une branche : elle est portée ici, ou créée plus tard par une étape.
/// </summary>
public abstract record WorkspaceRequest
{
    private WorkspaceRequest() { }

    /// <summary>
    /// Un travail neuf, à partir d'une base. Le worktree est monté en <b>HEAD
    /// détaché</b> : la branche nommée sera créée par une étape une fois son nom
    /// connu (souvent calculé en cours de workflow). Le détachement évite aussi
    /// le refus git « branch already checked out » quand deux runs partent de la
    /// même base.
    /// </summary>
    public sealed record NewWork(string BaseRef) : WorkspaceRequest;

    /// <summary>
    /// La relecture d'une ref existante : le worktree la checkout telle quelle.
    /// </summary>
    public sealed record Review(string Reference) : WorkspaceRequest;
}
