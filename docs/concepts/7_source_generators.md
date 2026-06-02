# Source Generators

Source generators are used to generate repetitive code from explicit source declarations.

## Purpose

OpinionatedFramework uses source generators to keep application code concise while preserving compile-time visibility. Generated code should be predictable and traceable back to attributes, SQL directives, or marked contracts.

## Generated Areas

- Resource controllers.
- SQL query objects.
- Facades.
- Ensure API helpers for custom ensurers.
- Other framework integration code as the framework evolves.

## Design Rule

Generation should remove repetition, not hide decisions.

If a developer cannot understand why something was generated, the declaration that caused generation is probably not explicit enough.

## Consumer Expectations

As a consumer, you should expect generated code to be based on declarations in your source files. You should not normally need to edit generated code.
