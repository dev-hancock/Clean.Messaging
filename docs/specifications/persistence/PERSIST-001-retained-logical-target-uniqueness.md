# PERSIST-001 — Retained logical target uniqueness

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Durable fan-out needs one retained representation for each logical event/target while allowing independent target responsibilities. Competing writes or replacement delivery IDs must not create duplicate retained work for the same logical target.

## Guarantees

- Retained durable entries MUST identify one work record per MessageId/TargetId delivery key.
- Retained EventId/TargetId identity MUST prevent separate delivery IDs from persisting duplicate logical target work.
- Distinct targets of the same event MUST remain independently persistable.

## Contract boundaries

The standard durable model/schema and stores retain the identities in question. These guarantees apply within each inbox/outbox entry store, not across different stores or after evidence has been cleaned. Application-owned arbitrary schema changes/writes are outside the standard integration. The contract specifies uniqueness outcomes, not a universal database locking algorithm.

## Acceptance criteria

### PERSIST-001-AC-01 — Same target logical duplicate

- **Given** two candidate entries share EventId/TargetId but have different MessageIds.
- **When** both are saved against the standard retained model.
- **Then** they cannot persist as two separate representations of the same logical target.

### PERSIST-001-AC-02 — Distinct targets

- **Given** valid entries for one event identify different targets.
- **When** the entries are saved.
- **Then** each target can persist independently rather than event-wide suppression losing sibling work.

### PERSIST-001-AC-03 — Delivery key duplicate

- **Given** two candidate entries have the same MessageId/TargetId.
- **When** both are saved.
- **Then** the retained delivery key does not identify two independent work records.

## Open decisions

- [GAP-CORE-005](../GAPS.md#gap-core-005): the related policy remains undecided; its recommendation is not approved.
