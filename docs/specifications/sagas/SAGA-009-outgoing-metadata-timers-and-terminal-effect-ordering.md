# SAGA-009 — Outgoing metadata, timers and terminal effect ordering

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Workflow authors need stable outgoing causality and instance-owned timers, with ordered cancellation and terminal cleanup that do not suppress valid sends.

Public SagaMessage/SagaSchedule/ordered SagaResult descriptions and the standards on logical effect identity, trigger causation and saga-owned timer lifecycle establish these effects. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Sends MUST preserve effect EventId and explicit metadata, using trigger correlation/MessageId when metadata is omitted.
- Timers MUST retain their logical identity, UTC instant and owning saga group/target; terminal outcomes MUST retain sends while stopping remaining timers.

## Contract boundaries

Timer IDs are globally scheduling identities, not per-saga upserts. Supported cancellation is scoped and excludes the executing timer. Effects remain local intentions until the owner commits and downstream delivery may repeat.

## Acceptance criteria

### SAGA-009-AC-01 — Send identity and explicit metadata

**Given** a send descriptor has a stable effect ID and explicit correlation/causation.
**When** the owner durably stages its outgoing message.
**Then** EventId and supplied metadata are preserved, independently of a generated outgoing delivery MessageId.

### SAGA-009-AC-02 — Default send uses triggering delivery causation

**Given** a send omits correlation/causation while the trigger has correlation, MessageId and prior causation.
**When** the outgoing intention is staged.
**Then** default correlation follows the trigger and default causation is the trigger MessageId, not its earlier CausationId; remaining null metadata is subject to the separate Outbox context fallback boundary.

### SAGA-009-AC-03 — Timer description routes by instance

**Given** a timer effect specifies a stable ID, payload and offset due instant for one instance.
**When** the owner stages that timer.
**Then** the ScheduleId is the effect ID, group identifies that saga, target is saga, due represents the same UTC instant, and trigger correlation/MessageId causation are retained.

### SAGA-009-AC-04 — Cancel respects timer scope and current trigger

**Given** timer cancellation identifies matching nonterminal work, unrelated group/target work or the executing timer.
**When** the effect commits with its owner.
**Then** only matching saga-group/saga-target active timers cancel; unrelated timers are unchanged and the current trigger is not cancelled by its own effect, because its successful consumption belongs to the scheduling owner.

### SAGA-009-AC-05 — Terminal sends are not discarded

**Given** a terminal result includes sends and new or existing nonterminal timers.
**When** the owner commits the result.
**Then** sends are retained while remaining saga timers are cancelled, including newly described timers and excluding the current trigger.

### SAGA-009-AC-07 — Order has observable cancellation consequences

**Given** an active Continue result describes a previously absent timer followed by cancellation, or cancellation followed by that timer.
**When** each ordered description commits in isolation with valid unique identity.
**Then** schedule-then-cancel leaves the timer cancelled; cancel-then-schedule leaves the new timer active, preserving declared effect order.

## Open decisions

### SAGA-009-AC-06 — Timer identity collision is not update

**Reserved — no normative acceptance scenario.** What result is promised for equal retries, differing payloads and concurrent insertion of one ScheduleId?

Decision: [GAP-WF-002](../GAPS.md#gap-wf-002) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
