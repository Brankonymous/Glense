using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Glense.VideoCatalogue.Services;

/// <summary>
/// Thin helper wrapper around IVideoStorage to provide common helpers.
/// This is a convenience class and delegates to the registered IVideoStorage.
/// </summary>
public class VideoStorage
{
    private readonly IVideoStorage _inner;

    public VideoStorage(IVideoStorage inner)
    {
        _inner = inner;
    }

    public Task<string> SaveAsync(Stream data, string originalFileName, CancellationToken cancellationToken = default)
        => _inner.SaveAsync(data, originalFileName, cancellationToken);

    public async Task<string> SaveFromFormFileAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null) throw new System.ArgumentNullException(nameof(file));
        await using var s = file.OpenReadStream();
        return await _inner.SaveAsync(s, file.FileName, cancellationToken).ConfigureAwait(false);
    }

    public Task DeleteAsync(string storedName, CancellationToken cancellationToken = default)
        => _inner.DeleteAsync(storedName, cancellationToken);

    public Task<Stream> OpenReadAsync(string storedName, CancellationToken cancellationToken = default)
        => _inner.OpenReadAsync(storedName, cancellationToken);

    public Task<(Stream Stream, long TotalLength)> OpenReadRangeAsync(string storedName, long? start = null, long? end = null, CancellationToken cancellationToken = default)
        => _inner.OpenReadRangeAsync(storedName, start, end, cancellationToken);
}

