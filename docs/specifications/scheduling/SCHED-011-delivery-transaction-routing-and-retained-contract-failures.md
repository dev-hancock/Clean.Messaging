# SCHED-011 — Delivery transaction, routing and retained contract failures

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** ScheduledDispatch carries ScheduleId/ScheduleGroupId/MessageData/correlation/causation, not due/attempt/claim information. Delivery resolves target at processing; message delivery deserializes retained contract and selects current outgoing targets.

**Evidence:** ScheduleExecutor.cs; OutboxScheduleDelivery.cs; ScheduleDeliveryRegistry.cs; OutboxEntryFactory.cs; IMessageSerializer; owner transaction dispatchers.

**Intent gaps:** [GAP-WF-014](../GAPS.md#gap-wf-014).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SCHED-011-AC-01 — Owned start gate rejects missing or stale work

**Given** a row is absent, terminal, wrong-token or expired.
**When** dispatch attempts to enter.
**Then** no outgoing effect is staged by that attempt; absent/terminal is settled, an active row owned elsewhere is not represented as that attempt success.

## SCHED-011-AC-02 — Successful message schedule stages current targets

**Given** a retained message-target schedule and target providers changed since staging.
**When** due delivery succeeds.
**Then** outgoing obligations use the delivery-time target set, shared EventId=schedule ID and one delivery batch identity; schedule completion does not await each downstream target.

## SCHED-011-AC-03 — Corrupt retained contract is a failure not completion

**Given** a schedule retains missing contract name, malformed JSON or JSON null.
**When** delivery deserializes it.
**Then** contract.identifier.not_registered or serialization.deserialize.failed is classified permanent and no successful outgoing dispatch is committed.

## SCHED-011-AC-04 — Unhandled message default route fails

**Given** the default local target provider sees a message with no consumers.
**When** due delivery attempts outgoing staging.
**Then** consumer.message_unhandled faults the schedule rather than silently creating an empty local delivery.

## SCHED-011-AC-05 — Rollback and execution-strategy repetition preserve durable truth

**Given** a delivery ran but save/commit rolls back or an EF strategy repeats.
**When** processing retries against persisted state.
**Then** uncommitted completion/outgoing rows are absent; retained completed state is recognized and local state is not duplicated, while external custom effects may already have repeated.

## SCHED-011-AC-06 — Commit uncertainty is not external success proof

**Given** commit result is uncertain and a rollback attempt itself fails.
**When** processing recovers.
**Then** the original failure/uncertainty is not replaced by a rollback error; persisted completion/ownership determines later processing, not an assertion that an external effect was reversed.
