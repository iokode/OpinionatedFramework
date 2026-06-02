# Why A Framework Over The Application Layer

## Decision

OpinionatedFramework intentionally provides a framework for the application layer instead of expecting every project to define its own application ports (interfaces to interact with the infrastructure layer), contracts, execution model, and default infrastructure integrations from scratch.

It is a deliberate application-layer dependency. It provides shared application-layer capabilities and replaceable defaults without coupling the application to concrete infrastructure.

## Why

Many projects repeat the same foundational decisions: how to represent use cases, how to execute them, how to expose them, how to validate assumptions, how to send emails, how to log, how to enqueue work, how to access files, how to dispatch events, and how to wire infrastructure.

The usual answer is to define application-layer ports for each capability and then implement those ports in the infrastructure layer. This keeps the application layer independent from concrete infrastructure, but it also means every project rewrites similar interfaces again and again.

Because each project owns its own ports, there cannot be a shared default implementation. A project-specific `IEmailSender`, `IUnitOfWork` or `IFileSystem` requires a project-specific implementation or adapter, even when the underlying need is common.

This creates two kinds of repetition:

- Architectural repetition: each project decides how to model the same application-layer capabilities.
- Implementation repetition: each project writes infrastructure implementations for ports that are semantically similar to ports written in many other projects.

This repetition often comes from a bad interpretation of the rule that **the application or domain layer must have no dependencies**. The application layer already has dependencies. It depends on the programming language, the type system, the runtime, and the standard library. The useful question is not whether dependencies exist, but what kind of dependencies are acceptable.

OpinionatedFramework treats itself as an *standard library extension*. It provides stable vocabulary and reusable ports for common application concerns without coupling the application to concrete infrastructure.

Depending on `IEmailSender`, `IFileSystem`, `IUnitOfWork`, or `IEventDispatcher` from the framework is different from depending on SMTP, Google Cloud Storage, NHibernate, Hangfire, ASP.NET Core, or any other infrastructure provider.

The framework exists so applications can share these common abstractions and, because the abstractions are shared, also share default implementations. Projects can start from working defaults and replace them only when they have a concrete reason.

This keeps the application layer focused on application-specific behavior instead of repeatedly rebuilding the same ports and adapters.