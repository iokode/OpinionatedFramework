# Facades

Facades provide convenient static access to framework capabilities.

They are intended to reduce repetitive service resolution in application code while keeping the underlying capability replaceable.

## Purpose

Some framework capabilities are used frequently enough that resolving them manually everywhere becomes noisy. Facades provide a compact entry point for those capabilities.

For example, a command facade can expose command execution methods generated from the command executor contract.

## Relationship With Defaults

A facade should not imply a hard dependency on a default implementation. It is a convenience over the registered capability. If the registered implementation changes, the facade should continue to route calls through the same contract.

## Trade-Off

Static access is convenient, but it can hide dependencies if overused. Prefer explicit constructor dependencies when they communicate important design information. Use facades when the convenience is worth the reduced ceremony.

For the rationale, see [`../decisions/6_why_static_facades_exist.md`](../decisions/6_why_static_facades_exist.md).
