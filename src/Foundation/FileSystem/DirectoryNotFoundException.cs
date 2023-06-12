using System;

namespace IOKode.OpinionatedFramework.FileSystem;

public class DirectoryNotFoundException : Exception
{
    public string AttemptedName { get; }

    public DirectoryNotFoundException(string attemptedName) : base($"The directory {attemptedName} does not exists in the disk.")
    {
        AttemptedName = attemptedName;
    }
}