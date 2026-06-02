# Query Directives

SQL query directives are comments that declare metadata for generated query objects.

Directives are written as SQL comments starting with `-- @`.

## Common Directives

| Directive | Purpose |
| --- | --- |
| `-- @generate` | Marks the SQL file for query object generation. |
| `-- @parameter <type> <name>` | Declares an input parameter. |
| `-- @result <type> <name>` | Declares a result field. |
| `-- @single` | Declares that the query expects exactly one result. |
| `-- @single_or_default` | Declares that the query expects zero or one result. |
| `-- @count <name>` | Declares a count column for paginated results. |
| `-- @query_result_name <name>` | Overrides the generated result type name. |
| `-- @namespace <namespace>` | Overrides or declares the generated namespace. |
| `-- @using <namespace>` | Adds a using directive to generated code. |
| `-- @attribute <attribute>` | Adds an attribute to the generated query class. |
| `-- @internal` | Generates the query as internal. |
| `-- @map` | Enables or declares custom result mapping. |

## Example

```sql
-- @generate
-- @using IOKode.OpinionatedFramework.Resources.Attributes
-- @attribute [RetrieveResource("active user", key: "name")]
-- @parameter string id
-- @result string name
-- @single

select name from users where id = :id;
```

## Cardinality

If no single-result directive is provided, the query is treated as a collection query.

Use `-- @single` when exactly one row is required. Use `-- @single_or_default` when no row is acceptable but multiple rows are not.
