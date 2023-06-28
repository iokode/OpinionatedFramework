using System;

namespace IOKode.OpinionatedFramework.FileSystem.Exceptions;

public class DirectoryAlreadyExistsException : Exception
{
    public string AttemptedName { get; }

    public DirectoryAlreadyExistsException(string attemptedName):base($"The directory {attemptedName} already exists in the disk.")
    {
        AttemptedName = attemptedName;
    }
}