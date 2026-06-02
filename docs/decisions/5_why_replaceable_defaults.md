# Why Defaults Are Replaceable

## Decision

OpinionatedFramework provides default implementations for common capabilities, but consumers can replace them.

Defaults are conveniences, not infrastructure commitments.

## Why

Application projects need common infrastructure-facing services, but the correct provider can vary by project, environment, or maturity level.

An application may start with a default email sender, file system, command executor, logger, job enqueuer, query executor, password hasher, or encrypter. Later, it may need a provider-specific implementation, an implementation adapted to an existing platform, or a stricter operational model.

Defaults make new projects faster. Replaceability prevents those defaults from becoming infrastructure lock-in.

This is especially important because OpinionatedFramework is intentionally used from the application layer. If framework defaults were not replaceable, depending on the framework would risk coupling application code to infrastructure choices.

The framework should therefore provide capabilities that application code can depend on and defaults that infrastructure or bootstrapping code can replace.

Consumer documentation should focus on what capability is available, how to use the default, and how to replace it when needed.

## Trade-offs

Replaceable defaults require clear contracts and consistent registration patterns. This adds framework maintenance cost.

They can also create uncertainty if multiple defaults or extension points exist for the same capability. The default path should be obvious, and replacement should be explicit.
