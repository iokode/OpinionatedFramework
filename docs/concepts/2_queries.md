# Queries

Queries represent application use cases that read data.

They are separated from commands so reads can have their own model, execution pipeline, result cardinality, and generated support.

## Purpose

A query makes a read operation explicit. It describes the input parameters, the expected result, and the cardinality of the result.

This separation is inspired by Command Query Separation. OpinionatedFramework does not require a strict CQRS architecture, but it does encourage separating read use cases from use cases with effects.

## SQL Query Objects

The framework can generate query objects from SQL files. SQL directives declare the generated class, parameters, result fields, cardinality, and optional metadata.

```sql
-- @generate
-- @parameter string id
-- @result string name
-- @single

select name from users where id = :id;
```

This can produce a query object that implements `IQuery<TResult>` and can be invoked through the query execution pipeline.

## Cardinality

Queries can declare whether they expect many results, exactly one result, or zero-or-one result.

Cardinality matters because it lets the executor enforce expectations instead of leaving every caller to repeat the same checks.

## Middleware

Query middleware can inspect or modify execution context, including raw SQL, parameters, directives, transaction, cancellation token, and results.

## When To Use Queries

Use a query when the use case is a read. If the operation changes state, calls an external effect, or triggers follow-up behavior, use a command instead.
