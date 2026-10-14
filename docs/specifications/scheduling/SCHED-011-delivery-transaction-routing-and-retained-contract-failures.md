# SCHED-011 — Delivery transaction, routing and retained contract failures

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Delivery integrators need valid target/payload routing and atomic local consumption, with uncertainty distinguished from external success.

The public at-least-once delivery port, shared public contract/serialization error surface and the atomicity/recovery standards establish the supported outcomes. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Successful message-target delivery MUST preserve logical identity and durably stage the selected outgoing intentions with schedule completion.
- A retained payload that cannot resolve or deserialize MUST not become a successful dispatch; rollback and replay MUST preserve local durable truth.

## Contract boundaries

Targets are selected at due delivery, not frozen at scheduling. Empty custom target policy and exact expiry/settled-reporting gate policy are undecided. Dispatch completion is not recipient receipt.

## Acceptance criteria

### SCHED-011-AC-02 — Successful message schedule stages current targets

**Given** a message schedule whose current outgoing target set differs from scheduling time.
**When** successful due delivery commits.
**Then** outgoing rows reflect the current selected targets, share one outgoing delivery batch MessageId and retain ScheduleId as their logical EventId; schedule completion does not await individual downstream targets or assert their receipt.

### SCHED-011-AC-03 — Corrupt retained contract is a failure not completion

**Given** retained message data has an unknown contract identifier, malformed JSON or JSON null.
**When** delivery interprets that data.
**Then** contract.identifier.not_registered or serialization.deserialize.failed is exposed as a permanent failure, with no successful outgoing dispatch committed.

### SCHED-011-AC-04 — Unhandled message default route fails

**Given** the default local target provider has no registered consumers for the scheduled message.
**When** due delivery selects outgoing targets.
**Then** consumer.message_unhandled faults that delivery rather than silently creating an empty local delivery; custom providers are outside this criterion.

### SCHED-011-AC-05 — Rollback and execution-strategy repetition preserve durable truth

**Given** an attempt rolls back or is replayed by persistence recovery.
**When** its final local outcome is reconciled.
**Then** rolled-back outgoing effects/completion are not durable successes, while a confirmed retained completion is not repeated as a new committed operation; custom external effects may already have repeated.

### SCHED-011-AC-06 — Commit uncertainty is not external success proof

**Given** commit fails with an uncertain outcome and rollback may also fail.
**When** the failure is surfaced.
**Then** the original failure is preserved rather than replaced by rollback error; persisted completion/ownership determines recovery, and neither local uncertainty nor rollback failure proves that an external effect succeeded once or was reversed.

## Open decisions

Separate boundary questions remain unapproved in the [decision register](../GAPS.md): `GAP-WF-014`. These do not suspend the independently justified guarantees above.

### SCHED-011-AC-01 — Owned start gate rejects missing or stale work

**Reserved — no normative acceptance scenario.** What expiry rule governs admission, successful completion and retry/failure settlement? At equality, are due/retry deadlines eligible and is a lease expired?

Decision: [GAP-WF-004](../GAPS.md#gap-wf-004), [GAP-CLAIM-002](../GAPS.md#gap-claim-002) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
