using System;

namespace IOKode.OpinionatedFramework.FileSystem.Exceptions;

public class FileAlreadyExistsException : Exception
{
    public string AttemptedName { get; }

    public FileAlreadyExistsException(string attemptedName):base($"The file {attemptedName} already exists in the disk.")
    {
        AttemptedName = attemptedName;
    }
}