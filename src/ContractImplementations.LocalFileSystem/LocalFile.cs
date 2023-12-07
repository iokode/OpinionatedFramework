using System.IO;
using System.Threading.Tasks;
using File = IOKode.OpinionatedFramework.FileSystem.File;

namespace IOKode.OpinionatedFramework.ContractImplementations.LocalFileSystem;

public class LocalFile : File
{
    public override Task<Stream> OpenReadStreamAsync()
    {
        var stream = (Stream)System.IO.File.OpenRead(FullName);
        return Task.FromResult(stream);
    }
}