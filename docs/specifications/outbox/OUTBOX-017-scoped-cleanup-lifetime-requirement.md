# OUTBOX-017 — Scoped cleanup lifetime requirement

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour

**Evidence:** Normative: coding standards forbid singleton capture of scoped services. Descriptive counter-evidence: singleton hosted OutboxCleanupWorker directly requires scoped IOutboxStore; [GAP-DUR-005](../GAPS.md#gap-dur-005).

Purpose: safe lifetime composition of public workers/custom providers. Required invariant: singleton hosted service cannot consume root scoped persistence. Source appears to violate this; no successful-host validation was executed.

## OUTBOX-017-AC-01 — Compose without captive cleanup store

- **Given** default outbox registration and provider scope validation enabled.
- **When** hosted cleanup services resolve.
- **Then** required outcome: no scoped store is consumed from singleton/root lifetime; present constructor/registration appears nonconforming.
