using System;

namespace IOKode.OpinionatedFramework.Persistence.QueryBuilder.Exceptions;

public class EmptyResultException : EntitySetException
{
    private const string ExceptionMessage = "The query returned no results when at least one was expected.";

    public EmptyResultException() : base(ExceptionMessage)
    {
    }

    public EmptyResultException(Exception inner) : base(ExceptionMessage, inner)
    {
    }
}