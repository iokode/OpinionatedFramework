using System;

namespace IOKode.OpinionatedFramework.Persistence.QueryBuilder.Exceptions;

public class EntityNotFoundException : EntitySetException
{
    public object AttemptedId { get; }
    private const string ExceptionMessage = "No entity was found with the specified ID.";

    public EntityNotFoundException(object attemptedId) : base(ExceptionMessage)
    {
        AttemptedId = attemptedId;
    }

    public EntityNotFoundException(object attemptedId, Exception inner) : base(ExceptionMessage, inner)
    {
        AttemptedId = attemptedId;
    }
}