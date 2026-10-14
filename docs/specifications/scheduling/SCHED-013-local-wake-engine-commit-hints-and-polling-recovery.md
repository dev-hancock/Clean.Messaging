# SCHED-013 — Local wake engine, commit hints and polling recovery

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Workers need prompt local reevaluation and durable fallback discovery so a lost signal cannot permanently strand committed due work.

The public wake port and standards on durable recovery, non-authoritative notifications, cancellation and transaction ownership justify reevaluation rather than a specific channel algorithm. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- A future wait MUST be releasable by its due instant or a local wake; committed work MUST remain discoverable when wake hints are missed.
- Wake signals MUST not be treated as proof of commit/rollback; unsupported ambient or nested observed saves MUST not silently corrupt transaction observation.

## Contract boundaries

Hints may coalesce and are not durable work. Polling assumes a running healthy worker/store; it does not guarantee real-time firing. Already-due cancellation precedence and caller savepoint support remain undecided.

## Acceptance criteria

### SCHED-013-AC-01 — Time and local hint both release wait

**Given** a future wait under a healthy local engine.
**When** its due instant arrives or a local wake is delivered.
**Then** the wait releases for reevaluation without itself claiming or completing durable work; multiple hints need not produce multiple deliveries.

### SCHED-013-AC-03 — Committed saves cause reevaluation

**Given** changed schedule work commits through the supported save/transaction integration.
**When** discovery is waiting.
**Then** the local wake prompts reevaluation after a standalone save or explicit commit; an explicit transaction’s save alone is not commit and the hint does not execute or publish the schedule.

### SCHED-013-AC-04 — Missed hints are eventually rechecked

**Given** committed due work exists but its hint is lost or another host wrote it.
**When** the healthy running worker continues its recovery polling.
**Then** persisted eligibility is rechecked rather than leaving that work dependent on the missing hint forever.

### SCHED-013-AC-05 — Wake after failure is only a hint

**Given** a save/transaction failure emits a wake hint.
**When** the worker reevaluates.
**Then** it inspects durable state; the hint does not prove success, rollback or schedule absence.

### SCHED-013-AC-06 — Nested saves and ambient changed-work transactions are rejected

**Given** changed schedule work is saved through an ambient transaction or nested observed SaveChanges.
**When** that unsupported save is attempted.
**Then** the ownership pattern is rejected rather than accepted as correctly observed scheduling persistence; this scheduling check does not reject an unrelated save with no changed schedules.

### SCHED-013-AC-07 — Infrastructure fault does not abandon worker forever

**Given** a worker-level infrastructure operation fails and infrastructure later becomes usable.
**When** the worker remains running.
**Then** recovery reevaluates persisted work after the configured recovery interval rather than abandoning discovery permanently or marking every schedule terminal because discovery failed.

## Open decisions

Separate boundary questions remain unapproved in the [decision register](../GAPS.md): `GAP-WF-006`. These do not suspend the independently justified guarantees above.

### SCHED-013-AC-02 — Past wait and cancelled future wait

**Reserved — no normative acceptance scenario.** What happens to pre-cancelled already-due waits, and must every later future wait reattempt Quartz scheduling?

Decision: [GAP-INTENT-WF-005](../GAPS.md#gap-intent-wf-005) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
