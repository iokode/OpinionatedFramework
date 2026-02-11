using System.IO;
using System.Threading.Tasks;
using File = IOKode.OpinionatedFramework.FileSystem.File;

namespace IOKode.OpinionatedFramework.ContractImplementations.LocalFileSystem;

/// <summary>
/// Represents a file stored in a <see cref="LocalDisk"/>.
/// </summary>
/// <remarks>
/// This type extends the framework file abstraction and keeps the physical path
/// required to open read streams from the local filesystem.
/// </remarks>
public class LocalFile : File
{
    /// <summary>
    /// Absolute path used to access the file content on disk.
    /// </summary>
    public required string PathOnDisk { get; init; }

    /// <inheritdoc />
    public override Task<Stream> OpenReadStreamAsync()
    {
        var stream = (Stream)System.IO.File.OpenRead(this.PathOnDisk);
        return Task.FromResult(stream);
    }
}
