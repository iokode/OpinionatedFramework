# Repository Structure

This page documents the repository structure for contributors.

## Main Areas

- `src/Foundation/`: public framework capabilities and application-layer abstractions.
- `src/Bootstrapping/`: framework container and service registration support.
- `src/ContractImplementations.*`: default or provider-specific implementations.
- `src/SourceGenerators.*`: compile-time generators.
- `src/Tests.*`: test projects for framework areas.
- `docs/`: consumer and contributor documentation.

## Consumer Perspective

Do not expose internal repository organization as a primary concept in consumer documentation unless it helps users install, configure, or replace behavior.
