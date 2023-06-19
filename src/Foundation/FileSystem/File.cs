using System;
using System.IO;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.FileSystem;

/// <summary>
/// A generic representation of a file, intended to be extended for specific filesystems.
/// <remarks>
/// This class provides basic details such as the file's name, its full path,
/// and its creation and update times.
/// </remarks>
/// </summary>
public abstract class File
{
    /// <summary>
    /// The name of the file, which includes only the name and excludes the path or directory.
    /// </summary>
    public virtual string Name { get; init; }

    /// <summary>
    /// The full name of the file, which includes its path within the filesystem.
    /// </summary>
    public virtual string FullName { get; init; }

    /// <summary>
    /// The point in time when the file was originally created.
    /// </summary>
    public virtual DateTimeOffset CreationTime { get; init; }

    /// <summary>
    /// The point in time when the file was last updated or modified.
    /// </summary>
    public virtual DateTimeOffset UpdateTime { get; init; }
    
    /// <summary>
    /// The size of the file, in bytes.
    /// </summary>
    public virtual ulong Size { get; init; }
    
    /// <summary>
    /// Opens a stream for reading the file.
    /// </summary>
    /// <returns>A stream to read the file.</returns>
    public abstract Task<Stream> OpenReadStreamAsync();
}