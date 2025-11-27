using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using NodaTime;
using File = IOKode.OpinionatedFramework.FileSystem.File;

namespace IOKode.OpinionatedFramework.ContractImplementations.GoogleCloudStorage;

public sealed class GoogleCloudStorageFile : File
{
    private readonly StorageClient _client;
    private readonly string _bucket;
    public Object Obj { get; }

    public override string Name => Obj.Name.Split("/").Last();
    public GoogleCloudStorageClass StorageClass { get; private init; }

    public GoogleCloudStorageFile(StorageClient client, Object obj, string bucket)
    {
        _client = client;
        _bucket = bucket;
        Obj = obj;

        Name = obj.Name.Split("/").Last();
        FullName = obj.Name;
        Size = obj.Size!.Value;
        CreationTime = Instant.FromDateTimeUtc(obj.TimeCreated!.Value.ToUniversalTime());
        UpdateTime = Instant.FromDateTimeUtc(obj.Updated!.Value.ToUniversalTime());
        StorageClass = GoogleCloudStorageHelpers.GetStorageClassFromString(obj.StorageClass);
    }
    
    public override async Task<Stream> OpenReadStreamAsync()
    {
        var memoryStream = new MemoryStream();
        await _client.DownloadObjectAsync(_bucket, FullName, memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }
}