# Exposing HTTP Endpoints

OpinionatedFramework can generate ASP.NET Core resource controllers from declared resource intent.

## 1. Write A Command Or Query

Start with the application use case. The use case remains the source of behavior.

```csharp
public class CreateUserCommand(CreateUserInput input) : Command<CreateUserOutput>
{
    protected override Task<CreateUserOutput> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(new CreateUserOutput { Id = input.Name });
    }
}
```

## 2. Add A Resource Attribute

Declare how the use case maps to a resource.

```csharp
[CreateResource("user")]
public class CreateUserCommand(CreateUserInput input) : Command<CreateUserOutput>
{
    // ...
}
```

## 3. Configure Resource Controller Generation

Configure the application to include generated resource controllers and the assemblies that contain declared resources.

The exact setup depends on the ASP.NET Core integration and source generator configuration used by the project.

## 4. Use The Generated Endpoint

A create resource declaration for `user` can be adapted into an HTTP endpoint such as:

```text
POST /users
```

The generated controller calls the command or query. It should not contain application behavior itself.

See [`../reference/1_resource_attributes.md`](../reference/1_resource_attributes.md).
