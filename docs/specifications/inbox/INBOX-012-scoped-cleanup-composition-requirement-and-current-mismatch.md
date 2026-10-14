# INBOX-012 — Scoped cleanup composition requirement

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

An application must be able to start a scope-validating host and run cleanup without retaining a scoped persistence context for the singleton worker's lifetime.

## Guarantees

- Hosted cleanup MUST compose successfully with scope validation enabled and MUST NOT directly retain a scoped store/context for the host lifetime.
- Each cleanup operation MUST own and dispose an appropriate persistence scope without leaking its context across cleanup operations.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards section 6 requires singleton-to-scoped dependencies to be resolved through bounded operation scopes; this lifetime safety requirement is independent of current worker wiring.

## Contract boundaries

This requirement applies to standard Inbox hosted cleanup and its configured scoped persistence services. It does not certify arbitrary application replacements or make a DbContext thread-safe. A host configured without scope validation does not waive the requirement.

## Acceptance criteria

### INBOX-012-AC-01 — Resolve cleanup without captive scoped persistence

- **Given** standard Inbox cleanup is registered with scoped persistence and scope validation enabled.
- **When** the host resolves its hosted worker and runs successive cleanup operations.
- **Then** composition does not fail for a singleton-to-scoped capture, and each cleanup operation uses and disposes its own valid scope rather than retaining a root-scoped context.

## Open decisions

[GAP-DUR-005](../GAPS.md#gap-dur-005) records the required composition correction. The implementation choice does not reopen or weaken the safe-lifetime guarantee.
