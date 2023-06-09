using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.FileSystem;
using FileNotFoundException = IOKode.OpinionatedFramework.FileSystem.FileNotFoundException;

namespace IOKode.OpinionatedFramework.Contracts;

[AddToFacade("FS")]
public interface IFileSystem
{
    public IFileDisk GetDisk(string diskName);
}

public interface IFileDisk
{
    public Task<bool> ExistsFileAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <exception cref="FileAlreadyExistsException"></exception>
    public Task PutFileAsync(string filePath, Stream fileContent, CancellationToken cancellationToken = default);
    
    public Task ReplaceFileAsync(string filePath, Stream fileContent, CancellationToken cancellationToken = default);
    
    /// <exception cref="FileNotFoundException"></exception>
    public Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    
    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}