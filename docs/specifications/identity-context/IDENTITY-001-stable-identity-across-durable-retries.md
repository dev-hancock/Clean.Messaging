# IDENTITY-001 — Stable identity across durable retries

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Evidence:** Coding standards §9 explicitly requires stable logical identity, distinct EventId/MessageId, and independent target completion.

Pre: retained admitted/staged work undergoing later attempts. Post: the same logical operation and target remain identifiable even if claim ownership changes.

## IDENTITY-001-AC-01 — Retry identity
**Given** an unresolved durable target with message, event and target IDs.
**When** it fails, becomes eligible and is reclaimed.
**Then** the same message/event/target IDs remain associated with it while a new ownership attempt is represented.

## IDENTITY-001-AC-02 — Independent target completion
**Given** one logical operation has two targets and the first has committed completion.
**When** the second fails and retries.
**Then** the first is not reset or deliberately rerun merely to retry its sibling.
