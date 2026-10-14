# SCHED-006 — Staging, duplicate insertion and owner visibility

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Producers need the caller transaction to own schedule visibility; stable identity must not be mistaken for a promised duplicate-insert result.

The public staging surface and the standards on transaction ownership and uncertain outcomes establish persistence visibility independently of insert implementation. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Staging MUST not independently save, publish or commit a schedule.
- Schedule visibility MUST follow the owning successful save/commit; a known rollback MUST leave its schedule uncommitted.

## Contract boundaries

Duplicate retry admission is unresolved under GAP-WF-002. A failed save/commit notification or a wake hint cannot prove persisted absence or success; the owner reconciles uncertainty.

## Acceptance criteria

### SCHED-006-AC-01 — Staged schedule requires save

**Given** a valid schedule is staged but its owner has not saved.
**When** persisted due-work discovery runs.
**Then** that unsaved addition is not durable executable work and staging has not published it.

### SCHED-006-AC-02 — Explicit transaction delays durable visibility

**Given** a schedule is saved inside an uncommitted explicit owner transaction.
**When** the transaction is known to roll back.
**Then** other committed readers do not observe a successfully committed new schedule, and known rollback leaves no durable new operation; saving alone did not establish its final visibility.

### SCHED-006-AC-03 — Standalone save and explicit commit publish durable work

**Given** a valid already-due schedule is staged.
**When** a standalone save or explicit owner commit succeeds.
**Then** the schedule becomes durable work available to eligibility checks, without a promise of immediate execution.

### SCHED-006-AC-06 — Save failure cannot establish successful staging

**Given** schedule persistence reports failure or uncertainty.
**When** the caller receives the result or discovery receives a wake.
**Then** neither report is proof of successful staging or rollback; persisted truth is reconciled before claiming a definitive outcome.

## Open decisions

Separate boundary questions remain unapproved in the [decision register](../GAPS.md): `GAP-WF-006`. These do not suspend the independently justified guarantees above.

### SCHED-006-AC-04 — Repeated insert is not an upsert

**Reserved — no normative acceptance scenario.** What result is promised for equal retries, differing payloads and concurrent insertion of one ScheduleId?

Decision: [GAP-WF-002](../GAPS.md#gap-wf-002) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SCHED-006-AC-05 — Duplicate insert race preserves one identity

**Reserved — no normative acceptance scenario.** What result is promised for equal retries, differing payloads and concurrent insertion of one ScheduleId?

Decision: [GAP-WF-002](../GAPS.md#gap-wf-002) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
