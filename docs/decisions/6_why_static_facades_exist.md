# Why Static Facades Exist

## Decision

OpinionatedFramework supports generated static facades for selected capabilities.

Facades are convenience APIs over registered framework capabilities. They should not make the underlying implementation less replaceable.

## Why

Some framework capabilities are called frequently. Repeated service resolution can add noise, especially in application-layer code that should focus on the use case.

Static facades provide a concise entry point while still routing through registered framework capabilities.

They also support one of the framework's goals: reducing boilerplate around common application-layer capabilities. If a capability is already part of the framework vocabulary, a facade can make common usage more direct.

Facades should be treated as convenience APIs. They should not prevent explicit dependencies where explicit dependencies communicate the design better.

## Trade-offs

Static access can hide dependencies and make testing harder if used carelessly.

For that reason, facades should be used selectively. They are useful for common framework capabilities, but they should not become a replacement for modeling important application dependencies explicitly.
