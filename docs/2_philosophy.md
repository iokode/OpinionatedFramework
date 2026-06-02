# Philosophy

OpinionatedFramework is based on a simple idea: application code should clearly say what the application can do, while repetitive adaptation and infrastructure work should be automated or provided by replaceable defaults.

## Opinionated By Design

The framework intentionally nudges applications toward a consistent structure. This is not accidental. The goal is to reduce repeated architectural decisions and make use cases easier to find, name, compose, expose, and test.

The opinions are not meant to remove flexibility. They define a default path. When a project needs a different implementation, the framework should allow that implementation to be replaced.

## Use Cases First

Application behavior should be represented by use cases. In OpinionatedFramework, the main use-case shapes are commands and queries.

Commands represent operations that may produce effects. Queries represent reads. Keeping these separate makes application behavior easier to reason about and allows each side to have its own execution pipeline, middleware, generated adapters, and infrastructure concerns.

## Declarative Intent

A use case should not only contain code. It should also be able to declare what it represents.

Resource attributes are one form of declarative intent: they describe how a command or query relates to a resource-oriented interface. SQL directives are another form: they describe parameters, result shape, cardinality, generated types, and optional attributes.

Declarative intent lets the framework generate adapters and metadata from explicit source declarations instead of requiring developers to repeat the same meaning in several places.

## Generated Adapters, Explicit Behavior

OpinionatedFramework uses source generators to remove boilerplate, not to hide behavior.

Generated controllers, query objects, and facades should be understood as adapters around explicit application code. The command, query, or declaration remains the source of truth.

## Replaceable Defaults

The framework provides ready-to-use defaults for common capabilities such as command execution, emailing, logging, password hashing, encryption, jobs, persistence, and configuration.

These defaults are conveniences, not permanent constraints. A consumer should be able to replace default behavior when the application requires a different provider or implementation.

## Pragmatic Application-Layer Coupling

Many architectures try to keep the application layer independent from any framework. OpinionatedFramework deliberately takes a different position.

Depending on a small application-layer framework can be worthwhile when it removes repeated boilerplate, establishes a shared vocabulary, and avoids coupling to concrete infrastructure. The important boundary is not whether the application references the framework, but whether it is forced to depend on a specific infrastructure implementation.
