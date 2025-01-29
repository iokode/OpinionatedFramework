using System;

namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Exceptions;

public class NonUniqueResultException : EntitySetException
{
    private const string ExceptionMessage = "The query returned more than one result when exactly one was expected.";

    public NonUniqueResultException() : base(ExceptionMessage)
    {
    }

    public NonUniqueResultException(Exception inner) : base(ExceptionMessage, inner)
    {
    }
}