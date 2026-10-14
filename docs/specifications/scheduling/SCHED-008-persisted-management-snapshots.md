# SCHED-008 — Persisted management snapshots

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Operators need nonmutating persisted group snapshots to inspect retained work without confusing a read with cancellation or repair.

IScheduleReader and immutable ScheduleStatus descriptions plus the standards on snapshot reads and retained evidence establish this read boundary. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Group reads MUST report matching persisted rows, including retained terminal history, without mutating work.
- Unsaved local changes and read failures MUST not be presented as committed snapshots or successful empty results.

## Contract boundaries

Snapshots are bounded by retention and their observation time; they do not establish external receipt, current lease ownership or future ordering. Exact list ordering is undecided.

## Acceptance criteria

### SCHED-008-AC-01 — Group snapshots include terminal history

**Given** persisted active and terminal schedules in a group and other groups/targets.
**When** the group is read with an optional target filter.
**Then** the result contains the matching retained history and excludes nonmatching group/target work.

### SCHED-008-AC-03 — Read does not expose unsaved mutations

**Given** unsaved additions or mutations exist in the reader scope.
**When** a persisted group snapshot is read.
**Then** those changes are not exposed as committed state; neither the read nor changes to returned snapshot data mutate durable lifecycle state.

### SCHED-008-AC-04 — Empty group and read I/O failures

**Given** an absent group or a failing/cancelled persistence read.
**When** the read result is observed.
**Then** absence yields an empty snapshot; read failure or cancellation is not converted into empty success.

## Open decisions

### SCHED-008-AC-02 — Ordering is stable for equal due instants

**Reserved — no normative acceptance scenario.** Are claim order, group/timer order and manager list order contractual, and what tie-breaks or fairness are promised?

Decision: [GAP-INTENT-WF-001](../GAPS.md#gap-intent-wf-001) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
