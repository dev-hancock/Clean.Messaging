# SCHED-014 — Quartz wake adapter, fallback, reconnect and shutdown

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Hosts optionally using Quartz need wake assistance without transferring durable schedule ownership to Quartz jobs.

UseQuartzScheduling is an explicit public host-composition adapter; provider separation, durable discovery and cooperative cancellation standards establish the bounded role of the adapter. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Quartz wake integration MUST retain durable schedule storage/execution as the owner of payload delivery.
- The adapter MUST use the host’s existing default scheduler when supplied, preserve cooperative cancellation and retain local recovery when Quartz scheduling reports an ordinary failure.

## Contract boundaries

Quartz storage, clustering, misfires and deployment lifecycle belong to the host/dependency. Local fallback after throwing I/O is not a guarantee for hung I/O. Exact per-wait reconnect cadence remains undecided.

## Acceptance criteria

### SCHED-014-AC-01 — Existing default scheduler is reused

**Given** the host already supplies an unkeyed default IScheduler.
**When** Quartz scheduling integration is composed.
**Then** that scheduler is reused and configuration applies to the default composition, not a new per-message scheduling service.

### SCHED-014-AC-02 — Wake hint does not own scheduled payload

**Given** multiple durable schedules are due.
**When** a Quartz wake executes.
**Then** the wake carries no individual payload ownership or completion; due processing still obtains work from durable scheduling storage.

### SCHED-014-AC-03 — Normal Quartz failure uses local fallback

**Given** Quartz scheduling for a future wait reports an ordinary non-cancellation failure.
**When** the local wake or due instant arrives.
**Then** availability failure is reported diagnostically and local waiting can release discovery; Quartz availability alone does not erase durable work.

### SCHED-014-AC-05 — Caller cancellation propagates through adapter

**Given** the caller token is cancelled during scheduler I/O or a future wait.
**When** the operation observes that cancellation.
**Then** cancellation propagates instead of being hidden as ordinary Quartz unavailability.

### SCHED-014-AC-06 — Shutdown waits for hosted jobs cooperatively

**Given** Quartz wake jobs are executing in the adapter’s hosted composition.
**When** the host initiates shutdown.
**Then** hosting is configured to wait for jobs cooperatively; persisted recovery remains independent of forced termination or guaranteed completion at shutdown.

## Open decisions

### SCHED-014-AC-04 — Later waits retry restored service

**Reserved — no normative acceptance scenario.** What happens to pre-cancelled already-due waits, and must every later future wait reattempt Quartz scheduling? Does a hung Quartz operation need a bounded or concurrent local fallback?

Decision: [GAP-INTENT-WF-005](../GAPS.md#gap-intent-wf-005), [GAP-WF-005](../GAPS.md#gap-wf-005) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
