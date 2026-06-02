# Validation

Validation in OpinionatedFramework is centered on preconditions, postconditions, and invariants.

The Ensure API provides a fluent way to assert those conditions and throw appropriate exceptions when they fail.

## Purpose

Validation should make assumptions explicit. A method, command, or domain operation can state what must be true before continuing.

```csharp
Ensure.String.IsNotNullOrEmpty(firstName)
    .ElseThrowsNullArgument(nameof(firstName));
```

## Ensure API

The Ensure API is part of OpinionatedFramework but distributed as a separate package. It can be used independently from the rest of the framework.

See [`../ensure/README.md`](../ensure/README.md) for detailed package documentation.

## Custom Validation

Projects can add custom ensurers and custom exception helpers. This keeps validation expressive without forcing all validation logic into generic utility methods.

See [`../guides/7_validating_with_ensure.md`](../guides/7_validating_with_ensure.md).
