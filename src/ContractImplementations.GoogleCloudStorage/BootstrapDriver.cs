using System.Collections.Generic;
using Google.Cloud.Storage.V1;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.FileSystem;
using Microsoft.Extensions.DependencyInjection;

[assembly: BootstrapDriver<IFileDisk,
    IOKode.OpinionatedFramework.ContractImplementations.GoogleCloudStorage.GoogleCloudStorageDiskBootstrapDriver>(
    "FileSystem:Disks", "google-cloud-storage", supportsNamedInstances: true)]

namespace IOKode.OpinionatedFramework.ContractImplementations.GoogleCloudStorage;

public sealed class GoogleCloudStorageDiskBootstrapDriver : IBootstrapDriverRegistrar
{
    private const string FileSystemStateKey = "OpinionatedFramework.FileSystem";

    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        var errors = new List<BootstrapValidationError>();
        if (string.IsNullOrWhiteSpace(context.InstanceName))
        {
            errors.Add(new BootstrapValidationError(
                context.DriverConfiguration.Path,
                "A filesystem disk name is required."));
        }

        if (string.IsNullOrWhiteSpace(context.DriverConfiguration["Bucket"]))
        {
            errors.Add(new BootstrapValidationError(
                $"{context.DriverConfiguration.Path}:Bucket",
                "A value is required."));
        }

        return new BootstrapValidationResult(errors);
    }

    public static void Register(BootstrapDriverContext context)
    {
        var fileSystem = context.GetOrAddSharedState(FileSystemStateKey, () =>
        {
            var createdFileSystem = new IOKode.OpinionatedFramework.ContractImplementations.FileSystem.FileSystem();
            context.Services.AddSingleton<IFileSystem>(createdFileSystem);
            return createdFileSystem;
        });
        fileSystem.AddDisk(context.InstanceName!,
            new GoogleCloudStorageDisk(StorageClient.Create(), context.DriverConfiguration["Bucket"]!));
    }
}
