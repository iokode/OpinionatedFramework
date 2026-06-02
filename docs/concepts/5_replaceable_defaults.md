# Replaceable Defaults

OpinionatedFramework provides default behavior for common application capabilities, but those defaults should be replaceable.

## Purpose

Most applications need similar infrastructure-facing capabilities: command execution, logging, emailing, configuration, encryption, password hashing, jobs, persistence, file systems, and events.

Creating these abstractions and default implementations repeatedly is boilerplate. OpinionatedFramework provides them so projects can focus on application-specific behavior.

## Defaults Are Starting Points

A default implementation is a working starting point, not a permanent decision.

For example, an application can start with a MailKit-based email sender and later replace it with a provider-specific implementation without changing the application use cases that depend on email sending.

## Consumer View

As a consumer, the important point is not where the framework stores contracts and implementations internally. The important point is that framework capabilities can be used immediately and replaced when needed.

## When To Replace A Default

Replace a default when your project needs provider-specific behavior, integration with an existing system, different performance characteristics, different observability, or stricter operational guarantees.
