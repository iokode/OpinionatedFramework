# Overview

OpinionatedFramework helps build application layers around explicit use cases instead of incidental framework plumbing.

It provides a set of conventions, contracts, default implementations, source generators, and integration points that reduce repetitive code while keeping application behavior visible in commands, queries, and declarations.

## What It Is

OpinionatedFramework is an application-layer framework for .NET. It focuses on the code that expresses what an application can do: execute use cases, read data, validate assumptions, dispatch events, send emails, enqueue jobs, access files, and expose behavior through generated adapters.

The framework is intentionally opinionated. It does not try to be a neutral bag of utilities. It encourages a specific structure so that applications are easier to understand, test, and extend.

## What It Optimizes For

- Clear separation between reads and operations with effects.
- Explicit declarations of intent through attributes, directives, and generated metadata.
- Less boilerplate around controllers, facades, query objects, and default service wiring.
- Replaceable defaults instead of infrastructure lock-in.
- A consistent application-layer vocabulary across projects.

## What It Does Not Try To Be

OpinionatedFramework is not a general-purpose web framework. ASP.NET Core integrations exist, but HTTP controllers are adapters over commands and queries, not the center of the architecture.

It is also not intended to hide the application model behind magic. Source generators remove repetitive code, but the generated code should reflect explicit declarations in the source application.

## Current Documentation Scope

This documentation focuses on consumers of the framework. Internal implementation details, repository structure, source generator internals, and release workflows are documented separately under [`contributing/`](contributing/README.md).
