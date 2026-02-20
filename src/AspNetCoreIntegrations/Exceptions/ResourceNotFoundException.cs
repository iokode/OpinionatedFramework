using System;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.Exceptions;

/// <summary>
/// The exception that is thrown when a resource is not found.
/// </summary>
public class ResourceNotFoundException : Exception
{
    private const string ExceptionMessage = "The requested resource was not found.";
    
    public ResourceNotFoundException() : base(ExceptionMessage)
    {
    }

    public ResourceNotFoundException(Exception inner) : base(ExceptionMessage, inner)
    {
    }
}