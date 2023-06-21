using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.FileSystem;

/// <summary>
/// Defines the contract for a system that manages multiple disk systems.
/// </summary>
/// <remarks>
/// This interface abstracts the operations for managing multiple disk systems,
/// such as adding, retrieving, and removing disks. It allows for handling different
/// storage options within the same application.
/// </remarks>
[AddToFacade("FS")]
public interface IFileSystem
{
    public IFileDisk GetDisk(string diskName);
}