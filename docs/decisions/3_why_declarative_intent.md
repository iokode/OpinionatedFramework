# Why Declarative Intent Matters

## Decision

OpinionatedFramework encourages commands, queries, and SQL files to declare intent explicitly.

Application code should not only contain behavior. When useful, it should also expose metadata about what the behavior represents, what it affects, or how it can be adapted to the outside world.

## Why

Application meaning is often duplicated across use cases, controllers, generated clients, OpenAPI descriptions, and documentation. This duplication creates drift.

For example, a command can create a user, a controller can expose `POST /users`, an OpenAPI document can describe the operation, and a client can generate a method for it. If each layer repeats the same intent manually, they can become inconsistent.

Explicit declarations let the framework generate adapters and metadata from a single source of truth.

Resource attributes and SQL directives are examples. They let source code say what a use case represents without repeating that meaning in every adapter.

Declarations should remain close to the behavior they describe. A resource attribute on a command or query is useful because it says something about that use case. A SQL directive is useful because it says something about that SQL file.

The goal is not to move behavior into metadata. The goal is to declare enough intent for the framework to generate repetitive integration code safely.

## Trade-offs

Declarations add metadata to source code. If declarations become too broad or vague, they can feel like magic.

To avoid that, declarations should stay small, explicit, and limited to intent. Application behavior should remain in commands, queries, SQL, and domain code.
