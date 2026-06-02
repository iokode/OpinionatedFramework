# Why Generated Adapters Are Used

## Decision

OpinionatedFramework uses source generators to create repetitive adapter code.

Generated code should adapt explicit application declarations to integration surfaces. It should not become the source of application behavior.

## Why

Controllers, query objects, facades, and validation helpers often repeat information already present in application declarations.

For example, a resource controller can be derived from a command or query annotated with resource intent. A query object can be derived from a SQL file and its directives. A facade can be derived from a contract marked for facade generation.

Generated adapters reduce boilerplate while keeping behavior in commands, queries, SQL, and domain code.

Compile-time generation also makes generated code available to tooling and avoids runtime discovery where possible.

This preserves a useful boundary: developers write the application use case and its declarations; the framework writes the repetitive adapter.

## Trade-offs

Generated code can be harder to debug if the source declaration is unclear. It can also surprise users if generation rules are not documented.

To reduce that risk, generated behavior must be documented through concepts, guides, and reference pages. The declarations that trigger generation should be easy to find and understand.
