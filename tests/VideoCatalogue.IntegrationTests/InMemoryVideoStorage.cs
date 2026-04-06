using System.Collections.Concurrent;
using Glense.VideoCatalogue.Services;

namespace VideoCatalogue.IntegrationTests;

/// <summary>
/// In-memory implementation of IVideoStorage for integration tests.
/// Uses a ConcurrentDictionary to store files instead of disk I/O.
/// </summary>
public class InMemoryVideoStorage : IVideoStorage
{
    private readonly ConcurrentDictionary<string, byte[]> _store = new();

    public async Task<string> SaveAsync(Stream data, string originalFileName, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var ext = Path.GetExtension(originalFileName) ?? string.Empty;
        var storedName = id + ext;

        using var ms = new MemoryStream();
        await data.CopyToAsync(ms, cancellationToken);
        _store[storedName] = ms.ToArray();

        return storedName;
    }

    public Task<Stream> OpenReadAsync(string storedName, CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetValue(storedName, out var bytes))
            throw new FileNotFoundException($"File not found: {storedName}");

        return Task.FromResult<Stream>(new MemoryStream(bytes));
    }

    public Task<(Stream Stream, long TotalLength)> OpenReadRangeAsync(string storedName, long? start = null, long? end = null, CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetValue(storedName, out var bytes))
            throw new FileNotFoundException($"File not found: {storedName}");

        var ms = new MemoryStream(bytes);
        if (start.HasValue)
            ms.Seek(start.Value, SeekOrigin.Begin);

        return Task.FromResult<(Stream, long)>((ms, bytes.Length));
    }

    public Task DeleteAsync(string storedName, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(storedName, out _);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns null since there's no physical file on disk.
    /// Thumbnail/stream endpoints will return 404 — acceptable for tests.
    /// </summary>
    public string? GetPhysicalPath(string storedName) => null;
}
