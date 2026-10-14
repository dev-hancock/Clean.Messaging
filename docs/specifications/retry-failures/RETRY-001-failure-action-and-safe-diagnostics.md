# RETRY-001 — Failure action and safe diagnostics

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Operators and processing policy need actionable failure classification without exposing arbitrary business payload text in durable diagnostics.

## Guarantees

- Failure snapshots MUST preserve MessageException code within the diagnostic field bound and distinguish Retry as Transient from Fault as Permanent.
- An arbitrary nonmessaging exception's Message MUST NOT be copied into the standard durable snapshot; the snapshot MUST use a generic unexpected-processing description instead.

## Contract boundaries

This specifies stored failure data, not blanket suppression of exception logging. Applications remain responsible for safe text in explicit messaging exceptions and logging. Failure classification does not itself authorize settlement or prove terminal state.

## Acceptance criteria

### RETRY-001-AC-01 — Messaging classification

- **Given** MessageException conveys Retry or Fault with a code.
- **When** its failure snapshot is created.
- **Then** Retry maps to Transient and Fault to Permanent while the code is preserved within the bounded diagnostic field.

### RETRY-001-AC-02 — Unknown exception message

- **Given** an arbitrary exception includes sensitive business payload text in its Message.
- **When** the standard failure snapshot is created.
- **Then** stored Message is the generic unexpected-processing description rather than the arbitrary exception Message.

## Open decisions

None.
