# Writing A Command

Use a command for a use case that may produce effects.

## 1. Define The Input And Output

Use regular C# types for input and output.

```csharp
public class CreateUserInput
{
    public required string Name { get; set; }
}

public record CreateUserOutput
{
    public required string Id { get; init; }
}
```

## 2. Create The Command

Inherit from `Command<TResult>` if the command returns a value.

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

Use `Command` when there is no result.

## 3. Use The Execution Context

Use `ICommandExecutionContext` for cancellation, pipeline data, shared data, trace information, and execution metadata.

```csharp
var cancellationToken = executionContext.CancellationToken;
```

## 4. Execute The Command

Execute the command through the command executor or command extension methods configured in your application.

```csharp
var result = await command.InvokeAsync(cancellationToken);
```

## 5. Add Declarations When Needed

If the command should be exposed or adapted by the framework, add declarative metadata such as a resource attribute.

```csharp
[CreateResource("user")]
public class CreateUserCommand(CreateUserInput input) : Command<CreateUserOutput>
{
    // ...
}
```
