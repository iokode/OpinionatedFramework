# Core Capabilities

OpinionatedFramework provides application-layer capabilities through contracts and replaceable implementations.

This page is a consumer-oriented reference. It describes what capabilities the framework exposes, not how the repository is internally organized.

## Capabilities

| Capability | Contract | Purpose |
| --- | --- | --- |
| Command execution | `ICommandExecutor` | Executes commands through the command pipeline. |
| Query execution | `IQueryExecutor` | Executes read queries. |
| File system | `IFileSystem` | Provides infrastructure-agnostic file access. |
| Emailing | `IEmailSender` | Sends emails through a replaceable provider. |
| Encryption | `IEncrypter` | Encrypts and decrypts sensitive data. |
| Events | `IEventDispatcher` | Dispatches application or domain events. |
| Job queue | `IJobEnqueuer` | Enqueues background work. |
| Job scheduling | `IJobScheduler` | Schedules background work. |
| Logging | `ILogging` | Writes logs through a replaceable logging capability. |
| Notifications | `INotificationDispatcher` | Dispatches notifications through one or more channels. |
| Password hashing | `IPasswordHasher` | Hashes and verifies passwords. |
| Persistence | `IUnitOfWorkFactory` | Creates unit-of-work boundaries for persistence operations. |
| Text translation | `ITextTranslator` | Translates text for localization scenarios. |

## Replaceable Defaults

Framework capabilities may have default implementations, but application code should depend on the capability rather than on a concrete provider.

For example, an application can use the emailing capability without coupling use cases to SMTP, MailKit, or any other provider-specific implementation.

Use the default implementation when it fits the project. Replace it when the application needs a provider-specific integration, a custom operational model, or different behavior.

## Related Pages

- [`../concepts/5_replaceable_defaults.md`](../concepts/5_replaceable_defaults.md)
- [`../guides/6_replacing_default_behavior.md`](../guides/6_replacing_default_behavior.md)
