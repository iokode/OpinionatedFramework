# Writing A Query

Use a query for a read use case.

This guide focuses on SQL query objects generated from SQL files.

## 1. Create A SQL File

Add a `.sql` file in the configured query location.

```sql
-- @generate
-- @parameter string id
-- @result string name
-- @single

select name from users where id = :id;
```

## 2. Declare Parameters

Use `-- @parameter` directives to declare input parameters.

```sql
-- @parameter int skip
-- @parameter int take
```

## 3. Declare Result Fields

Use `-- @result` directives to declare the result shape.

```sql
-- @result string name
```

## 4. Declare Cardinality

Use cardinality directives when the query should return a single result or zero-or-one result.

```sql
-- @single
```

or:

```sql
-- @single_or_default
```

## 5. Invoke The Query

Generated query objects can be invoked through the query execution pipeline.

```csharp
var result = await new GetUserNameQuery { Id = "1" }.InvokeAsync(cancellationToken);
```

See [`../reference/2_query_directives.md`](../reference/2_query_directives.md) for directive details.
