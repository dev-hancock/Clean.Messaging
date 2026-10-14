# IDENTITY-001 — Stable identity across durable retries

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Retry and recovery must remain recognisable as continued work on the same logical operation and target. A sibling target's failure must not undo successful local completion.

## Guarantees

- Durable retry/reacquisition MUST preserve message, event and target IDs even when ownership changes.
- Retrying one target MUST NOT reset or deliberately rerun a sibling whose completion is retained.

## Contract boundaries

This applies to retained admitted/staged work, not a new caller operation or newly authorized replay target. Retention limits completion evidence. Stable identity supports external deduplication but does not provide exactly-once external effects. The meaning/accounting of an attempt remains [GAP-CLAIM-001](../GAPS.md#gap-claim-001).

## Acceptance criteria

### IDENTITY-001-AC-01 — Retry identity

- **Given** an unfinished durable target has retained message, event and target IDs.
- **When** it fails, waits for eligibility and is reacquired with new ownership.
- **Then** it remains the same message/event/target operation rather than receiving replacement logical identities.

### IDENTITY-001-AC-02 — Independent target completion

- **Given** one logical operation has two targets and the first has committed completion.
- **When** the second fails and retries.
- **Then** the first is not reset or deliberately rerun merely to retry its sibling.

## Open decisions

[GAP-CLAIM-001](../GAPS.md#gap-claim-001) concerns retry accounting, not permission to replace logical identity.
