## Overview
OpinionatedFramework Ensure API is a comprehensive library written in C# for the validation of [preconditions, postconditions, and invariants](https://en.wikipedia.org/wiki/Design_by_contract).

One of the main features of this library is the ability to customize the exception thrown during each validation. It also excels in its extensibility, making it easy to add new validations.

While the Ensure API is part of the OpinionatedFramework, it is distributed in a separately NuGet package and remains decoupled from it.

## Installing
Install the NuGet package [`IOKode.OpinionatedFramework.Ensuring`](https://www.nuget.org/packages/IOKode.OpinionatedFramework.Ensuring).

## Getting started
The static `Ensure` class serves as the entry point to the library's functionality. The API is designed to offer natural usage to the developers.

The basic usage of the API follows this pattern:

```csharp
Ensure.{Validation Category}.{Validation}.ElseThrows(exception);
```

Let's take a look at some examples:

```csharp
Ensure.Stream.CanRead(dataStream).ElseThrows(new InvalidOperationException("Cannot read the stream."));
Ensure.String.IsNotNullOrEmpty(firstName).ElseThrows(new ArgumentNullException(nameof(firstName)));
```

## Built-in validations category (ensurers)
Each "validation category" is called "ensurer".

There are seven built-in ensurers available. Take a look in the [source code](https://github.com/iokode/OpinionatedFramework/tree/main/src/IOKode.OpinionatedFramework.Ensuring/Ensurers):
- `Boolean`
- `Enumerable`
- `Number`
- `Object`
- `Stream`
- `String`
- `Type`

## Adding custom ensurers
Extending the Ensure API with custom validations is straightforward. First, you need to reference the [`IOKode.OpinionatedFramework.Generators.Ensuring`](https://www.nuget.org/packages/IOKode.OpinionatedFramework.Generators.Ensuring) package:

```xml
<ItemGroup>
    <PackageReference Include="IOKode.OpinionatedFramework.Ensuring" Version="1.0.1" />
    <PackageReference Include="IOKode.OpinionatedFramework.Generators.Ensuring" Version="1.0.1" />
</ItemGroup>
```

Next, you should create a static class with public static methods that return a boolean value and decorate the class with the `[Ensurer]` attribute. The class name should end with `Ensurer`. A set of source-generators take care of generating the `Ensure` class and other needed stuff.

Here's an example of a custom ensurer for the `Person` class.

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

That's all. Now you can use it:

```csharp
Ensure.Person.IsInLegalAge(john).ElseThrows(new PersonIsUnderLegalAgeException(john));
```

## Custom exception throwing
`ElseThrows` method will throw the exception if the validation not passes.

To make exception selection more convenient, the library includes a set of extension methods:

- `ElseThrowsNullArgument(string paramName)` // throws ArgumentNullException
- `ElseThrowsIllegalArgument(string paramName, string message)` // throws ArgumentException
- `ElseThrowsInvalidOperation(string message)` // throws InvalidOperationException
- `ElseThrowsBase(string message)` // throws Exception

Using these extension methods, the previous examples can be rewritten as:

```csharp
Ensure.Stream.CanRead(dataStream).ElseThrowsInvalidOperation("Cannot read the stream.");
Ensure.String.IsNotNullOrEmpty(firstName).ElseThrowsNullArgument(nameof(firstName));
```

These methods provide a convenient way to throw custom exceptions that best fit the context of your application.

### Adding your own extension methods for throwing
You're free to add your own extension methods for throwing exceptions as needed.

The responsible of throwing the exception is the [`Thrower`](https://github.com/iokode/OpinionatedFramework/blob/main/src/IOKode.OpinionatedFramework.Ensuring/Thrower.cs) class, so you can add an extension method like this:

```csharp
public static class PersonThrowerExtensions
{
    public static void ElseThrowsPersonUnderLegalAge(this Thrower thrower, Person person)
    {
        var exception = new PersonIsUnderLegalAgeException(person);
        thrower.ElseThrows(exception);
    }
}
```

To use this extension method, simply call it like you would call the pre-existing exception extension methods:

```csharp
Ensure.Person.IsInLegalAge(john).ElseThrowsPersonUnderLegalAge(john);
```

In the above line, we used a custom ensurer and an a custom thrower's extension method.

## Behind the scenes
When utilizing the OpinionatedFramework Ensure API, you are harnessing a chain of calls that seamlessly connect your validation logic with exception handling. Let's unpack this chain to understand how it functions.

1. `Ensurer`: The static `Ensurer` class is the entry point of the validation API. It's is a generated class that contains properties that correspond to each `EnsurerThrowers` – bridges between Ensurers and the Thrower.

```csharp
// Generated code
public static class Ensurer
{
    // Other Ensurer-Thrower bridges
    public static PersonEnsurerThrower Person => new PersonEnsurerThrower();
    // Other Ensurer-Thrower bridges
}
```

2. `{EnsurerName}EnsurerThrower`: For each ensurer, a corresponding `{EnsurerName}EnsurerThrower` class is generated. This class comprises all methods from the original ensurer, but instead of returning a boolean, it yields an instance of the `Thrower` class.

```csharp
// Generated code
public class PersonEnsurerThrower
{
    public Thrower IsInLegalAge(Person person)
    {
        bool isValid = PersonEnsurer.IsInLegalAge(person); // Call to the Ensurer's static method.
        return new Thrower(isValid); // Returns a Thrower instance with the validation result.
    }
}

```

3. `Thrower`: The `Thrower` instance returned by the `{EnsurerName}EnsurerThrower` carries the result of the validation check. By calling the `ElseThrows(Exception)` method on the `Thrower` instance, an exception is thrown if the validation has not passed.

```csharp
Ensurer // Entry point
    .Person // Returns the Ensurer-Thrower bridge
        .IsInLegalAge(john) // Returns a Thrower instance
            .ElseThrows(new PersonIsUnderLegalAgeException(john)) // Throws the exception if the validation has not passed
```
