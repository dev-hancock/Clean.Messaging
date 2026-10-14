# SCHED-009 — Due eligibility, claims and bounded execution

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Workers need exclusive recoverable ownership and bounded local execution, not a promise of real-time firing or global order.

The standards on durable temporary ownership, stale-owner fencing and bounded resources justify these guarantees; exact equality, selection order and budget accounting are separate policies. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Future-due, retry-deferred, terminal or currently owned work MUST not be admitted as new eligible work.
- Claimed batches and local execution MUST remain within configured bounds; replacing ownership MUST prevent the old token from settling the replacement owner’s work.

## Contract boundaries

No fairness, global semaphore, hard handler-call budget or exact firing time is promised. Deadline equality, ordering, abandoned-claim accounting and queued lease duration remain open.

## Acceptance criteria

### SCHED-009-AC-05 — Competing claims do not share ownership

**Given** competing workers recover one eligible schedule.
**When** a replacement claim becomes durable.
**Then** ownership tokens are not interchangeable; the displaced token cannot settle the replacement owner’s work.

### SCHED-009-AC-07 — Execution parallelism has local scope

**Given** one worker has more work than its configured execution concurrency.
**When** the worker processes its batch.
**Then** simultaneous local deliveries do not exceed that concurrency; completion need not follow due order and this is not a semaphore shared across all hosts.

## Open decisions

### SCHED-009-AC-01 — All eligibility predicates hold

**Reserved — no normative acceptance scenario.** At equality, are due/retry deadlines eligible and is a lease expired? What expiry rule governs admission, successful completion and retry/failure settlement?

Decision: [GAP-CLAIM-002](../GAPS.md#gap-claim-002), [GAP-WF-004](../GAPS.md#gap-wf-004) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SCHED-009-AC-02 — Boundary instant is claimable

**Reserved — no normative acceptance scenario.** At equality, are due/retry deadlines eligible and is a lease expired?

Decision: [GAP-CLAIM-002](../GAPS.md#gap-claim-002) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SCHED-009-AC-03 — Batch selection is bounded and ordered

**Reserved — no normative acceptance scenario.** Are claim order, group/timer order and manager list order contractual, and what tie-breaks or fairness are promised?

Decision: [GAP-INTENT-WF-001](../GAPS.md#gap-intent-wf-001) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SCHED-009-AC-04 — Claim increases attempts before delivery admission

**Reserved — no normative acceptance scenario.** Which unit consumes scheduling retry budget, especially lease-loss, cancelled and never-admitted claims? How does ownership remain usable while claimed work waits for capacity or execution outlasts a lease?

Decision: [GAP-CLAIM-001](../GAPS.md#gap-claim-001), [GAP-WF-003](../GAPS.md#gap-wf-003) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SCHED-009-AC-06 — Recovery is not a hard maximum invocation budget

**Reserved — no normative acceptance scenario.** Which unit consumes scheduling retry budget, especially lease-loss, cancelled and never-admitted claims?

Decision: [GAP-CLAIM-001](../GAPS.md#gap-claim-001) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
