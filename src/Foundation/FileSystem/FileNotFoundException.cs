using System;

namespace IOKode.OpinionatedFramework.FileSystem;

public class FileNotFoundException : Exception
{
    public string AttemptedFileName { get; }

    public FileNotFoundException(string attemptedFileName) : base($"The file {attemptedFileName} does not exists in the disk.")
    {
        AttemptedFileName = attemptedFileName;
    }
}