using System;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.Exceptions;

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