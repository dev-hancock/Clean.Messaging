# CONFIG-006 — Complete boundary validation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Proposed behaviour
**Evidence:** Coding standards §6/§17 require clear boundary validation; null nested groups and invalid enum values currently bypass the intended checks.

Unapproved proposal: null common option groups and undefined RetryBackoff values produce configuration-validation failures at startup, not later NullReferenceException/processing failure.

**Open decisions:** [GAP-CORE-003](../GAPS.md#gap-core-003).

## CONFIG-006-AC-01 — Missing nested options
**Given** a common options object has null Lease, Processing, Recovery or Cleanup.
**When** startup validation runs after this proposal is approved.
**Then** startup returns a clear options validation failure identifying the missing group.

## CONFIG-006-AC-02 — Undefined retry mode
**Given** RetryBackoff contains an undefined enum value.
**When** startup validation runs after this proposal is approved.
**Then** startup rejects the mode before any message attempt.
