using System.IO;
using System.Threading.Tasks;
using File = IOKode.OpinionatedFramework.FileSystem.File;

namespace IOKode.OpinionatedFramework.ContractImplementations.FileSystem;

public class LocalFile : File
{
    internal Stream _fileContent;
    
    public override Task<Stream> OpenReadStreamAsync()
    {
        return Task.FromResult(_fileContent);
    }
}