using System;

namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Exceptions;

public class EntitySetException : Exception
{
    public EntitySetException(string message) : base(message)
    {
    }

    public EntitySetException(string message, Exception inner) : base(message, inner)
    {
    }
}