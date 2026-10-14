# OUTBOX-017 — Scoped cleanup lifetime requirement

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

An application must be able to start scope-validating Outbox cleanup without a singleton worker retaining scoped persistence throughout the host lifetime.

## Guarantees

- Standard hosted cleanup MUST resolve with scope validation enabled and MUST NOT directly capture a scoped outgoing store/context.
- Successive cleanup operations MUST own and dispose valid persistence scopes rather than sharing a captive host-lifetime context.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards section 6 requires singleton-to-scoped access through bounded operation scopes; safe cleanup composition is an established requirement rather than an optional new policy.

## Contract boundaries

This applies to standard Outbox hosted cleanup and configured scoped services; arbitrary application replacements still own their lifetime contracts. Disabling scope validation does not make a captive context safe or authorize concurrent use.

## Acceptance criteria

### OUTBOX-017-AC-01 — Compose without captive cleanup store

- **Given** standard Outbox cleanup is composed with scoped persistence and scope validation enabled.
- **When** the host resolves its worker and executes successive cleanup operations.
- **Then** resolution does not fail for singleton-to-scoped capture, and each operation uses and disposes a valid bounded scope without retaining the store/context for host lifetime.

## Open decisions

[GAP-DUR-005](../GAPS.md#gap-dur-005) records the required correction. The mechanics of that correction do not reopen lifetime safety.
