# Declarative Intent

Declarative intent is the practice of making application meaning explicit through metadata.

In OpinionatedFramework, code should not only execute behavior. It should also be able to declare what that behavior represents, what it affects, and how it can be adapted to external interfaces.

## Why It Matters

Without declarations, the same intent is often repeated in several places: a use case class, a controller route, OpenAPI metadata, a client contract, and documentation.

Declarative intent makes the use case the source of truth. Generated adapters can then derive repetitive integration code from that source.

## Resource Declarations

Resource attributes are one example of declarative intent.

```csharp
[CreateResource("user")]
public class CreateUserCommand(CreateUserInput input) : Command<CreateUserOutput>
{
    protected override Task<CreateUserOutput> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        // Application behavior lives here.
    }
}
```

The attribute declares that the command creates a `user` resource. A resource controller generator can use that declaration to create an HTTP adapter.

## Query Directives

SQL directives are another example.

```sql
-- @generate
-- @parameter int skip
-- @parameter int take
-- @result string name
-- @count total
```

These directives declare query parameters, result shape, and count behavior. The SQL remains the source of the read, while directives provide metadata for generation and execution.

## Declarations Are Not Behavior

Declarations describe intent. They should not replace the command or query body.

The framework should use declarations to generate adapters, validate assumptions, and create metadata, while the actual application behavior remains explicit in commands, queries, and domain code.
