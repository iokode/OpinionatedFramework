# Why Commands And Queries Are Separated

## Decision

OpinionatedFramework separates read use cases from use cases that may produce effects.

Read use cases are modeled as queries. Use cases that may change state, interact with external systems, dispatch events, enqueue work, or otherwise produce effects are modeled as commands.

## Why

Reads and effectful operations need different execution rules.

Commands may need transactions, event dispatching, authorization, job enqueuing, unit-of-work boundaries, idempotency controls, or effect-oriented middleware. Queries may need SQL-specific directives, result cardinality, projections, read-only transactions, caching, or read-oriented middleware.

Using a single generic use-case abstraction for both reads and writes would force that abstraction to support concerns that do not apply equally to both sides. Over time, it would either become too broad or push important behavior into ad-hoc conventions.

Separating commands and queries makes the application model easier to reason about. A developer can see whether a use case is intended to read or to do something with effects before reading its implementation.

The separation also gives each side room to evolve independently. Query objects can be generated from SQL files and directives. Commands can focus on behavior, orchestration, and effect boundaries. Both can have their own execution pipeline and middleware.

This is inspired by Command Query Separation. It does not require every application to implement full CQRS with separate databases, models, or deployment units.

## Trade-offs

The separation adds ceremony in very small applications. A simple application may feel that having both commands and queries is more structure than it immediately needs.

OpinionatedFramework accepts this cost because the distinction becomes more valuable as the application grows. The framework documentation should describe commands and queries as separate concepts, not as variants of a generic handler pattern.
