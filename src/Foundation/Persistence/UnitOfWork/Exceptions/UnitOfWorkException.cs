using System;

namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork.Exceptions;

public class UnitOfWorkException : Exception
{
    public UnitOfWorkException(string message) : base(message)
    {
    }

    public UnitOfWorkException(string message, Exception inner) : base(message, inner)
    {
    }
}