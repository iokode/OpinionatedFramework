using System;

namespace IOKode.OpinionatedFramework.FileSystem;

public class FileAlreadyExistsException : Exception
{
    public string AttemptedFileName { get; }

    public FileAlreadyExistsException(string attemptedFileName):base($"The file {attemptedFileName} already exists in the disk.")
    {
        AttemptedFileName = attemptedFileName;
    }
}