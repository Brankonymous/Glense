using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Glense.VideoCatalogue.Services
{
    /// <summary>
    /// Abstraction for video file storage implementations.
    /// Implementations should return a stored identifier (filename or key)
    /// which is persisted in the database (e.g. Videos.VideoUrl).
    /// </summary>
    public interface IVideoStorage
    {
        Task<string> SaveAsync(Stream data, string originalFileName, CancellationToken cancellationToken = default);
        Task<Stream> OpenReadAsync(string storedName, CancellationToken cancellationToken = default);
        /// <summary>
        /// Open a readable stream for a byte range of the file. If start/end are null the whole file is returned.
        /// Returns the opened stream and the total length of the file.
        /// </summary>
        Task<(Stream Stream, long TotalLength)> OpenReadRangeAsync(string storedName, long? start = null, long? end = null, CancellationToken cancellationToken = default);
        Task DeleteAsync(string storedName, CancellationToken cancellationToken = default);
    }
}
