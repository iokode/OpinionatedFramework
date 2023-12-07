using System;

namespace IOKode.OpinionatedFramework.FileSystem.Exceptions;

public class FileNotFoundException : Exception
{
    public string AttemptedName { get; }

    public FileNotFoundException(string attemptedName) : base($"The file {attemptedName} does not exists in the disk.")
    {
        AttemptedName = attemptedName;
    }
}