# Getting Started

This guide introduces the usual path for building with OpinionatedFramework.

The exact package set depends on the capabilities your application needs. Use this page as a high-level path, then follow the specific guides for commands, queries, declarations, HTTP endpoints, and validation.

## 1. Install The Needed Packages

Start with the packages required by the features you use. For example, Ensure is distributed as `IOKode.OpinionatedFramework.Ensuring`.

See [`../reference/3_packages.md`](../reference/3_packages.md).

## 2. Register Framework Capabilities

Register the framework services your application needs, then initialize the container when using the framework container.

```csharp
Container.Services.AddDefaultCommandExecutor(options => { });
Container.Initialize();
```

Some applications can use default bootstrapping to register a common set of capabilities.

## 3. Write A Command

Create a command for an operation with effects.

```csharp
public class CreateUserCommand(CreateUserInput input) : Command<CreateUserOutput>
{
    protected override Task<CreateUserOutput> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(new CreateUserOutput { Id = input.Name });
    }
}
```

See [`2_writing_a_command.md`](2_writing_a_command.md).

## 4. Write A Query

Use a query for reads. SQL query objects can be generated from SQL files with directives.

See [`3_writing_a_query.md`](3_writing_a_query.md).

## 5. Declare Intent

Add declarations when a use case should be adapted or described by the framework.

```csharp
[CreateResource("user")]
public class CreateUserCommand(CreateUserInput input) : Command<CreateUserOutput>
{
    // ...
}
```

See [`4_declaring_intent.md`](4_declaring_intent.md).

## 6. Expose The Application

Use generated adapters, such as resource controllers, when you want framework declarations to become HTTP endpoints.

See [`5_exposing_http_endpoints.md`](5_exposing_http_endpoints.md).
