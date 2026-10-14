# SCHED-016 — Terminal retention, recovery boundaries and management limits

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Operators need retention that bounds terminal schedule evidence without expiring active work or confusing read/cancel with repair.

The standards on bounded retention, durable unfinished work and explicit management limitations establish this lifecycle boundary; exact cleanup cutoffs and cadence require policy selection. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Retention cleanup MUST exclude active work and remain bounded by configured cleanup resources.
- Deleting a terminal schedule MUST not be represented as preserving its identity/history or as repairing another capability’s delivery outcome.

## Contract boundaries

No failure requeue, recurrence, timer upsert or automatic compensation is promised. Exact cutoff equality, batch ordering and initial cleanup timing are open. Other durable evidence has independent retention.

## Acceptance criteria

### SCHED-016-AC-04 — Deletion removes identity protection and history

**Given** terminal schedule evidence has been physically deleted.
**When** the same identity is submitted later or history is requested.
**Then** that deleted row supplies neither identity exclusion nor history; other outgoing/saga evidence has its own lifetime.

### SCHED-016-AC-05 — Cancellation is not failure requeue

**Given** a retained failed schedule.
**When** supported read or cancellation completes.
**Then** the schedule remains failed; those operations do not reopen, reschedule or compensate it.

## Open decisions

### SCHED-016-AC-01 — Strict terminal retention cutoff

**Reserved — no normative acceptance scenario.** What exact cutoff equality, batch ordering, startup cadence and purge version/timestamp semantics are promised?

Decision: [GAP-INTENT-WF-004](../GAPS.md#gap-intent-wf-004) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SCHED-016-AC-02 — Cleanup is periodic and bounded

**Reserved — no normative acceptance scenario.** What exact cutoff equality, batch ordering, startup cadence and purge version/timestamp semantics are promised? Are claim order, group/timer order and manager list order contractual, and what tie-breaks or fairness are promised?

Decision: [GAP-INTENT-WF-004](../GAPS.md#gap-intent-wf-004), [GAP-INTENT-WF-001](../GAPS.md#gap-intent-wf-001) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SCHED-016-AC-03 — Cleanup recovery and initial timing

**Reserved — no normative acceptance scenario.** What exact cutoff equality, batch ordering, startup cadence and purge version/timestamp semantics are promised?

Decision: [GAP-INTENT-WF-004](../GAPS.md#gap-intent-wf-004) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
