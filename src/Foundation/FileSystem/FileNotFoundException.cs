using System;

namespace IOKode.OpinionatedFramework.FileSystem;

public class FileNotFoundException : Exception
{
    public string AttemptedName { get; }

    public FileNotFoundException(string attemptedName) : base($"The file {attemptedName} does not exists in the disk.")
    {
        AttemptedName = attemptedName;
    }
}