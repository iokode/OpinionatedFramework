# Declaring Intent

Use declarations when a command, query, or SQL file needs to describe its meaning to the framework.

## 1. Choose The Intent

Start by identifying what the use case represents.

- Does it create, retrieve, list, update, replace, delete, or act on a resource?
- Does it expose a read with known parameters and result shape?
- Does it need generated adapters?

## 2. Add A Declaration

For resource-oriented intent, use a resource attribute.

```csharp
[RetrieveResource("user", key: "id")]
public class RetrieveUserCommand(int id) : Command<string>
{
    // ...
}
```

For SQL query intent, use directives.

```sql
-- @generate
-- @parameter string name
-- @result string id
-- @single_or_default
```

## 3. Keep Behavior Explicit

The declaration should not contain application behavior. Keep behavior in the command, query, SQL, or domain model.

## 4. Let Adapters Be Generated

Once the intent is declared, the relevant source generator can produce integration code such as controllers or query objects.
