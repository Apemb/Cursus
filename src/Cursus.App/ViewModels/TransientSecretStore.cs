using System.Threading;
using System.Threading.Tasks;

using Cursus.Core.Secrets;

namespace Cursus.App.ViewModels;

/// <summary>
/// Un trousseau de passage : il rend le jeton qu'on lui a confié, sans jamais rien
/// écrire nulle part. Il sert à <b>éprouver</b> un jeton avant de décider s'il mérite
/// d'être rangé — la seule façon d'interroger le tracker sans avoir d'abord déposé un
/// secret qu'il faudrait peut-être reprendre.
/// </summary>
internal sealed class TransientSecretStore(string token) : ISecretStore
{
    public Task<string?> ReadAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(token);

    public Task WriteAsync(string key, string value, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
