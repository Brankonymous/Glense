using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Glense.VideoCatalogue.Services
{
    /// <summary>
    /// Simple local-disk implementation of <see cref="IVideoStorage"/>.
    /// Stores files under a configured base path (defaults to `Videos/`).
    /// </summary>
    public class LocalFileVideoStorage : IVideoStorage
    {
        private readonly string _basePath;
        private readonly ILogger<LocalFileVideoStorage> _logger;

        public LocalFileVideoStorage(IConfiguration config, ILogger<LocalFileVideoStorage> logger)
        {
            _logger = logger;
            _basePath = config.GetValue<string>("VideoStorage:BasePath") ?? "Videos";
            if (!Path.IsPathRooted(_basePath))
            {
                _basePath = Path.Combine(AppContext.BaseDirectory, _basePath);
            }

            Directory.CreateDirectory(_basePath);
        }

        public async Task<string> SaveAsync(Stream data, string originalFileName, CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid();
            var ext = Path.GetExtension(originalFileName) ?? string.Empty;
            var storedName = id + ext;
            var path = Path.Combine(_basePath, storedName);

            _logger.LogInformation("Saving video to {Path}", path);

            // Stream to disk
            using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await data.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
            }

            return storedName;
        }

        public Task DeleteAsync(string storedName, CancellationToken cancellationToken = default)
        {
            var path = Path.Combine(_basePath, storedName);
            if (File.Exists(path)) File.Delete(path);
            return Task.CompletedTask;
        }

        public Task<Stream> OpenReadAsync(string storedName, CancellationToken cancellationToken = default)
        {
            var path = Path.Combine(_basePath, storedName);
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            return Task.FromResult<Stream>(fs);
        }

        public Task<(Stream Stream, long TotalLength)> OpenReadRangeAsync(string storedName, long? start = null, long? end = null, CancellationToken cancellationToken = default)
        {
            var path = Path.Combine(_basePath, storedName);
            var fi = new FileInfo(path);
            var total = fi.Length;

            var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);

            if (start.HasValue)
            {
                fs.Seek(start.Value, SeekOrigin.Begin);
            }

            // Note: callers are responsible for disposing the stream
            return Task.FromResult<(Stream, long)>((fs, total));
        }
    }
}
