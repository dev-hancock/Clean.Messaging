# SAGA-016 — Persisted manager and timer snapshots

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Operators need read-only persisted workflow and timer snapshots without implying repair, handler state access or delivery confirmation.

ISagaManager, immutable Saga/SagaTimer records and snapshot/resource-bound standards establish nonmutating reads, filters and explicit failure/absence. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Manager reads MUST not mutate workflow/timer state and MUST distinguish absence from I/O failure or cancellation.
- Positive list limits MUST be respected; snapshot flags MUST reflect persisted lifecycle rather than unsaved local changes.

## Contract boundaries

No typed state getter, force-complete/reopen/replay operation or global list ordering is promised. Timer snapshots use the owning saga target/group and scheduling retention; exact list/projection ordering remains reserved.

## Acceptance criteria

### SAGA-016-AC-01 — Get and absent timer listing are nonmutating

**Given** an existing or absent SagaId.
**When** Get or timer listing is read.
**Then** Get returns a retained snapshot or null, absent-instance timers are empty, and neither operation mutates state.

### SAGA-016-AC-03 — Limits reject nonpositive values

**Given** default, positive, zero or negative manager list limits.
**When** GetActive or GetFailed is requested.
**Then** the default limit is 100, a supplied positive bound is honored, and nonpositive limits are rejected.

### SAGA-016-AC-05 — Snapshot flags and unsaved changes

**Given** persisted lifecycle timestamps differ from unsaved local mutations.
**When** manager snapshots are read.
**Then** Saga IsActive is true exactly when both CompletedAt and FailedAt are absent, and IsTerminal is its inverse; a timer is terminal when consumed, cancelled or failed. These flags follow persisted timestamps without unsaved-state merge or mutable tracked-state exposure.

### SAGA-016-AC-06 — Read failure and cancellation are not empty success

**Given** a manager persistence read fails or observes cancellation.
**When** its outcome is returned.
**Then** failure/cancellation is exposed, not replaced by successful null or empty results.

## Open decisions

### SAGA-016-AC-02 — Lists filter and bound persisted state

**Reserved — no normative acceptance scenario.** Are claim order, group/timer order and manager list order contractual, and what tie-breaks or fairness are promised?

Decision: [GAP-INTENT-WF-001](../GAPS.md#gap-intent-wf-001) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SAGA-016-AC-04 — Timer projection uses saga target and history

**Reserved — no normative acceptance scenario.** Are claim order, group/timer order and manager list order contractual, and what tie-breaks or fairness are promised?

Decision: [GAP-INTENT-WF-001](../GAPS.md#gap-intent-wf-001) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
