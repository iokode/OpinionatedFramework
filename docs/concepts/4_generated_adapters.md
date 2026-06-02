# Generated Adapters

Generated adapters convert explicit application declarations into integration code.

They are one of the main ways OpinionatedFramework reduces boilerplate without moving behavior out of commands and queries.

## Purpose

Adapters connect application use cases to the outside world. A controller adapts HTTP to commands and queries. A facade adapts a contract to convenient static access. A generated query object adapts a SQL file to a typed query API.

## Source Of Truth

Generated adapters should not become the source of truth. The source of truth remains the command, query, SQL file, attribute, or directive that triggered generation.

## Examples

- Resource controller generation from resource attributes.
- Query object generation from SQL directives.
- Facade generation from contracts marked with facade metadata.
- Ensure API generation for custom ensurers.

## Benefits

- Less repetitive controller and adapter code.
- More consistent generated routes and method shapes.
- Reduced risk of mismatches between declared use cases and exposed endpoints.
- Faster application development without hiding the actual use-case code.
