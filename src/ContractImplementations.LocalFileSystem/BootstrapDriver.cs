using System.Collections.Generic;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.FileSystem;
using Microsoft.Extensions.DependencyInjection;

[assembly: BootstrapDriver<IFileDisk,
    IOKode.OpinionatedFramework.ContractImplementations.LocalFileSystem.LocalFileDiskBootstrapDriver>(
    "FileSystem:Disks", "local", supportsNamedInstances: true)]

namespace IOKode.OpinionatedFramework.ContractImplementations.LocalFileSystem;

public sealed class LocalFileDiskBootstrapDriver : IBootstrapDriverRegistrar
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

        if (string.IsNullOrWhiteSpace(context.DriverConfiguration["Path"]))
        {
            errors.Add(new BootstrapValidationError(
                $"{context.DriverConfiguration.Path}:Path",
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
        fileSystem.AddDisk(context.InstanceName!, new LocalDisk(context.DriverConfiguration["Path"]!));
    }
}
