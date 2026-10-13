# PERSIST-001 — Retained logical target uniqueness

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Evidence:** Coding standards §9 requires duplicate protection and per-target progress; standard model implements these boundaries.

Pre: standard mapped durable stores retain the relevant identity. Post: no two persisted outstanding/completed representations of one logical event/target are introduced by competing normal writes.

## PERSIST-001-AC-01 — Same target logical duplicate
**Given** two candidate entries share EventId/TargetId but have different MessageIds.
**When** both are saved against the standard retained model.
**Then** they cannot both persist as distinct representations for the same logical target.

## PERSIST-001-AC-02 — Distinct targets
**Given** one event has different target IDs.
**When** valid entries are saved.
**Then** each target can persist independently without the event ID globally suppressing siblings.

## PERSIST-001-AC-03 — Delivery key duplicate
**Given** two entries share MessageId/TargetId.
**When** both are saved.
**Then** the retained key does not identify two independent work records.
