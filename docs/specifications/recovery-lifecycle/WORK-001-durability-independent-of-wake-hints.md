# WORK-001 — Durability independent of wake hints

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Accepted durable work must not disappear because a process-local notification was missed or a worker restarted. Recovery finds existing responsibilities instead of manufacturing replacement admission or outgoing staging.

## Guarantees

- Retained eligible durable work MUST remain discoverable without its original wake hint.
- After restart, unresolved stored work and expired abandoned ownership MUST remain available for recovery under their durable eligibility policy.

## Contract boundaries

Recovery requires an operational host/store, valid contracts and retained work. It does not promise a deadline through outages, permanent-failure success, fairness or global ordering. Wake reduces discovery latency but is not acceptance, publication or completion evidence. Retry-budget choices remain separate from restart durability.

## Acceptance criteria

### WORK-001-AC-01 — Missed notification

- **Given** eligible durable work exists but its process-local wake was lost.
- **When** recovery runs with the host and store operational.
- **Then** the work can be discovered and processed without recreating admission or staging.

### WORK-001-AC-02 — Restart recovery

- **Given** a process stopped with unresolved work or abandoned claims.
- **When** a later host starts and the stored work is eligible after expiry.
- **Then** the retained work remains available for another attempt.

## Open decisions

- [GAP-CLAIM-001](../GAPS.md#gap-claim-001): the related policy remains undecided; its recommendation is not approved.

- [GAP-CORE-005](../GAPS.md#gap-core-005): the related policy remains undecided; its recommendation is not approved.
