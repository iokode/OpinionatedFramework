# Replacing Default Behavior

OpinionatedFramework provides default implementations for common capabilities. Replace them when your application needs different behavior.

## 1. Identify The Capability

Decide which capability should change: command execution, logging, emailing, configuration, encryption, password hashing, persistence, jobs, file systems, or another framework service.

## 2. Implement The Contract

Create an implementation that satisfies the framework capability your application uses.

```csharp
public class CustomEmailSender : IEmailSender
{
    // Implement the required members.
}
```

## 3. Register Your Implementation

Register your implementation during bootstrapping.

```csharp
Container.Services.AddTransient<IEmailSender, CustomEmailSender>();
```

## 4. Keep Use Cases Stable

Commands and queries should depend on the capability, not on a provider-specific implementation. Replacing the registered implementation should not require changing application use cases.
