# OPS-002 — Identity-preserving management

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Repairing one failed fan-out target must not replay a successful sibling or disguise the repair as a new logical event. Operators need progress isolation as well as stable identity across management retries.

## Guarantees

- Requeue of a failed target MUST preserve its original message, event and target identities.
- Repair of one target MUST NOT reopen an independently completed sibling.

## Contract boundaries

Management addresses retained per-target state. It is distinct from creating new replay responsibilities or restaging a cleaned event. Successful external effects cannot be undone by management, and requeued delivery can repeat according to its delivery boundary.

## Acceptance criteria

### OPS-002-AC-01 — Repair one target

- **Given** target A is completed and target B is dead-lettered for the same event.
- **When** B is requeued.
- **Then** A remains completed and B retains its original MessageId, EventId and TargetId.

## Open decisions

None.
