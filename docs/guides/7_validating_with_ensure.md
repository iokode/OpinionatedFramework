# Validating With Ensure

The Ensure API provides fluent validation for preconditions, postconditions, and invariants.

## 1. Install Ensure

Install the `IOKode.OpinionatedFramework.Ensuring` package.

## 2. Validate A Condition

Use the static `Ensure` entry point.

```csharp
Ensure.String.IsNotNullOrEmpty(firstName)
    .ElseThrowsNullArgument(nameof(firstName));
```

## 3. Choose The Exception

Use an existing throwing helper or pass your own exception.

```csharp
Ensure.Stream.CanRead(dataStream)
    .ElseThrowsInvalidOperation("Cannot read the stream.");
```

## 4. Add Custom Ensurers When Needed

Create a custom ensurer when a validation belongs to your domain or application vocabulary.

```csharp
[Ensurer]
public static class PersonEnsurer
{
    public static bool IsInLegalAge(Person person)
    {
        return person.Age >= 18;
    }
}
```

See [`../ensure/README.md`](../ensure/README.md) for the detailed Ensure documentation.
