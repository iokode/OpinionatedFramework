# Core Concepts

This page summarizes the main concepts used throughout OpinionatedFramework.

## Commands

Commands are use cases that may have effects. They can return a result or complete without one. They run through a command executor and can participate in middleware, scoped services, shared data, cancellation, and tracing.

See [`concepts/1_commands.md`](concepts/1_commands.md).

## Queries

Queries are use cases for reading data. They are separated from commands to keep reads distinct from operations with effects. SQL query objects can be generated from SQL files and directives.

See [`concepts/2_queries.md`](concepts/2_queries.md).

## Declarative Intent

Declarative intent is the practice of describing what a use case means through metadata. Resource attributes and SQL directives are examples.

See [`concepts/3_declarative_intent.md`](concepts/3_declarative_intent.md).

## Generated Adapters

Generated adapters turn declared intent into integration code, such as ASP.NET Core controllers, facades, or query objects.

See [`concepts/4_generated_adapters.md`](concepts/4_generated_adapters.md).

## Replaceable Defaults

The framework provides defaults for common application capabilities while allowing projects to replace them.

See [`concepts/5_replaceable_defaults.md`](concepts/5_replaceable_defaults.md).

## Facades

Facades provide convenient static access to framework capabilities. They are generated from public contracts marked for facade generation.

See [`concepts/6_facades.md`](concepts/6_facades.md).

## Source Generators

Source generators are used to remove repetitive code while keeping source declarations explicit.

See [`concepts/7_source_generators.md`](concepts/7_source_generators.md).

## Validation

Validation is handled through the Ensure API, a separately distributed package focused on preconditions, postconditions, and invariants.

See [`concepts/8_validation.md`](concepts/8_validation.md) and [`ensure/README.md`](ensure/README.md).
