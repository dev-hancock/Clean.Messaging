# CLAIM-002 — Stale settlement rejection

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Successor workers and operators rely on retained target state remaining authoritative after work changes hands. Settlement must affect only the target for which the caller still holds ownership.

## Guarantees

- An owned update MUST require the addressed target and its current ownership token; a token for another target or an obsolete token MUST NOT authorize a change.
- A rejected Complete, Retry, DeadLetter or Renew MUST preserve the current owner's target state, completion evidence and diagnostics.

## Contract boundaries

This is a durable-state fence, not revocation of handler execution or an external effect. Callers use a valid key/token and the agreed time domain. Rejection does not identify a unique cause. Exact expiry equality is unresolved in [GAP-CLAIM-002](../GAPS.md#gap-claim-002); provider certification and uncertain-commit attribution remain in [GAP-CORE-005](../GAPS.md#gap-core-005).

## Acceptance criteria

### CLAIM-002-AC-01 — Wrong token

- **Given** a target currently belongs to worker B.
- **When** worker A attempts Complete, Retry, DeadLetter or Renew with its obsolete token.
- **Then** each operation rejects A's update and preserves B's ownership and durable target state.

### CLAIM-002-AC-02 — Wrong target

- **Given** a worker holds a valid token for one entry.
- **When** it addresses a different unmatched EntryKey with that token.
- **Then** the operation does not settle or modify that different target.

### CLAIM-002-AC-03 — Reclaimed target

- **Given** a successor reclaimed an expired target and settled it.
- **When** the earlier handler attempts settlement using its former ownership.
- **Then** successor completion and diagnostics remain unchanged.

## Open decisions

[GAP-CLAIM-002](../GAPS.md#gap-claim-002) covers expiry equality; [GAP-CORE-005](../GAPS.md#gap-core-005) covers supported provider evidence and uncertain settlement attribution.
