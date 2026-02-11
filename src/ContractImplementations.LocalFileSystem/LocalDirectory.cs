using IOKode.OpinionatedFramework.FileSystem;

namespace IOKode.OpinionatedFramework.ContractImplementations.LocalFileSystem;

/// <summary>
/// Represents a directory stored in a <see cref="LocalDisk"/>.
/// </summary>
/// <remarks>
/// This type extends the framework directory abstraction and carries
/// the corresponding absolute path on disk.
/// </remarks>
public class LocalDirectory : Directory
{
    /// <summary>
    /// Absolute path used to access the directory on disk.
    /// </summary>
    public required string PathOnDisk { get; init; }
}
