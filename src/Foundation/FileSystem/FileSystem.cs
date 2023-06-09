using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Contracts;

namespace IOKode.OpinionatedFramework.FileSystem;

public class FileSystem : IFileSystem
{
    private readonly Dictionary<string, IFileDisk> _disks = new();
    
    public void AddDisk(string diskName, IFileDisk disk)
    {
        _disks[diskName] = disk;
    }
    
    public IFileDisk GetDisk(string diskName)
    {
        if (!_disks.TryGetValue(diskName, out var disk))
        {
            throw new ArgumentException($"No disk named {diskName} is registered.", nameof(diskName));
        }

        return disk;
    }
}