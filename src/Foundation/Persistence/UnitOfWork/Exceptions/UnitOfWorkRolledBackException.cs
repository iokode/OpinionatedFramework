using System;

namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork.Exceptions;

public class UnitOfWorkRolledBackException : UnitOfWorkException
{
    private const string ExceptionMessage = "The unit of work has been rolled back and can no longer be used.";

    public UnitOfWorkRolledBackException() : base(ExceptionMessage)
    {
    }

    public UnitOfWorkRolledBackException(Exception inner) : base(ExceptionMessage, inner)
    {
    }
}