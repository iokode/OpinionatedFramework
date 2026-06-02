# Commands

Commands represent application use cases that may have effects.

Examples include creating a user, updating an order, deleting a file, sending an email, or enqueuing a job.

## Purpose

A command gives an operation a name, an input shape, and an execution boundary. Instead of placing behavior directly in controllers, services, or UI handlers, the operation is modeled as an explicit application use case.

## Shape

A command inherits from `Command` when it does not return a value, or from `Command<TResult>` when it returns a result.

```csharp
public class CreateUserCommand(CreateUserInput input) : Command<CreateUserOutput>
{
    protected override Task<CreateUserOutput> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(new CreateUserOutput
        {
            Id = input.Name
        });
    }
}
```

## Execution

Commands are executed through an `ICommandExecutor` or through command extension methods when available.

The execution context provides cancellation, pipeline data, shared data, trace information, and execution metadata useful for middleware.

## Middleware

Command middleware can run before and after command execution. This allows cross-cutting behavior such as logging, exception handling, unit-of-work boundaries, metrics, authorization, or contextual setup.

## When To Use Commands

Use a command when the use case may change application state, interact with an external system, dispatch an event, enqueue work, or otherwise produce an effect.

Do not use a command for a pure read just because it is convenient. Reads should normally be modeled as queries.
