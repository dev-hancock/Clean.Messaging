# RETRY-005 — Processing settlement boundary

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Failed processing must remain recoverable without committing rolled-back consumer changes or overwriting a successor. Hosts need shutdown and settlement uncertainty to remain distinct from message failure.

## Guarantees

- Failure settlement by current ownership MUST be able to record a later retry without committing the failed attempt's rolled-back participating changes.
- Loss of ownership MUST NOT authorize retry/dead-letter metadata overwriting successor state.
- Host-stop cancellation of handling MUST leave unfinished work recoverable by expiry rather than dead-letter solely for shutdown.
- Failure settlement MUST have its separate configured timeout; timeout or failure MUST NOT be represented as confirmed successful settlement.

## Contract boundaries

This concerns failed local attempts and durable failure settlement, not a transaction covering external effects. Settlement is conditional on current ownership and bounded separately from handler execution; it can continue after shutdown begins for an already observed ordinary failure. A timeout/ambiguous I/O result does not prove no durable update committed; later inspection/recovery may be needed.

## Acceptance criteria

### RETRY-005-AC-01 — Failure rollback and retry

- **Given** consumer state rolls back while claim remains valid and policy permits retry.
- **When** failure settlement succeeds.
- **Then** a later due attempt is recorded without committing the failed consumer changes.

### RETRY-005-AC-02 — Lost ownership

- **Given** the attempt fails after loss is signalled.
- **When** processor handles that failure.
- **Then** it does not overwrite successor state with retry/dead-letter metadata.

### RETRY-005-AC-03 — Host cancellation

- **Given** handling throws cancellation due to the host stopping token.
- **When** processor exits.
- **Then** the target is left recoverable by expiry rather than dead-lettered solely because of shutdown.

### RETRY-005-AC-04 — Settlement timeout

- **Given** failure-settlement I/O cannot establish a confirmed result within its separate configured timeout.
- **When** that timeout interrupts settlement.
- **Then** processing does not claim a confirmed retry/dead-letter update; durable state remains subject to later inspection/recovery rather than an assertion that nothing committed.

## Open decisions

[GAP-CLAIM-001](../GAPS.md#gap-claim-001) leaves budget policy open. [GAP-CORE-005](../GAPS.md#gap-core-005) covers uncertain-commit attribution; [GAP-CORE-007](../GAPS.md#gap-core-007) covers pre-confirmation log wording.
