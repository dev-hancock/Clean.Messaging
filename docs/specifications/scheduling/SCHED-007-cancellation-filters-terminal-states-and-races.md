# SCHED-007 — Cancellation filters, terminal states and races

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Producers and saga adapters need scoped cancellation that stops remaining local work without recalling effects already delivered.

IScheduleWriter cancellation filters, IMessageScheduler message-only scope and the standards on terminal lifecycle and transaction/external boundaries establish the contract. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Cancellation MUST affect only matching identity, target and any supplied group/exclusion scope.
- Missing or terminal work MUST remain unchanged; participating cancellation MUST obey owner rollback and cancellation, not imply external recall.

## Contract boundaries

Cancellation requires the owning save/commit and cannot reopen failed work. A race is resolved by durable ownership/concurrency, not by a guarantee that external delivery has never happened. Exact retained diagnostic history is undecided.

## Acceptance criteria

### SCHED-007-AC-01 — Single cancellation respects target and optional group

**Given** schedules sharing an ID/group context but subject to target and optional group filters.
**When** single cancellation commits.
**Then** only the schedule matching the ID, exact target and any specified group is cancelled; unrelated work is unchanged.

### SCHED-007-AC-02 — Public group cancellation is message-only

**Given** a group contains message-target and saga-target work.
**When** IMessageScheduler group cancellation commits.
**Then** nonterminal message schedules in the group are cancelled; saga-target schedules are not cancelled by that public operation.

### SCHED-007-AC-03 — Tracked and persisted group work are included

**Given** matching group work includes saved rows and tracked unsaved additions, with an excluded schedule.
**When** writer group cancellation commits with the owner.
**Then** both kinds of matching nonterminal work are cancelled, the excluded identity remains untouched and terminal rows do not transition.

### SCHED-007-AC-04 — Missing and terminal cancellation are no-ops

**Given** a missing, dispatched, cancelled or failed schedule.
**When** cancellation is applied.
**Then** the operation does not create, reopen or rewrite a terminal schedule.

### SCHED-007-AC-06 — Concurrent cancellation does not reverse delivery

**Given** cancellation races with scheduled delivery.
**When** the competing outcomes are observed.
**Then** cancellation cannot reverse an already committed delivery or revoke an external effect; a stale local write may fail optimistic concurrency, and ownership/concurrency fencing protects the winning terminal result.

### SCHED-007-AC-07 — Cancel obeys owner rollback and cancellation token

**Given** cancellation changes participate in the owner transaction or the operation observes its cancellation token.
**When** the owner rolls back or cancellation aborts the operation.
**Then** no rolled-back cancellation is represented as committed and token cancellation is not ordinary successful settlement.

## Open decisions

### SCHED-007-AC-05 — Cancellation removes retry ownership without erasing diagnostics

**Reserved — no normative acceptance scenario.** Which diagnostic fallback codes, truncation/sanitization rules and post-success/cancellation history are guaranteed?

Decision: [GAP-INTENT-WF-003](../GAPS.md#gap-intent-wf-003) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
