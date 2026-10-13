# INBOX-008 — Dead-letter inspection and independent management

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: `IInboxManager`, `InboxManager`, shared `DeadLetterStore.Get/Requeue/Discard`. [Inbox](../../reference/inbox.md) is descriptive.

Contracts: GetDeadLetters(default limit 100), Requeue, Discard and RequeueResult (shared). Positive listing limit, nonnull key collections; empty collections valid. List includes dead-lettered but not discarded entries ordered by dead-letter timestamp; it is not snapshot pagination. Operations process keys sequentially in independent transactions and preserve original event/message identity, payload and failure diagnostics. Requeue resets Attempts and terminal/claim/due state; Discard retains dead-letter timestamp and diagnostics. No batch atomicity or inverse discard operation.

## INBOX-008-AC-01 — Inspect active dead letters with limit validation

- **Given** dead-lettered, discarded and nonterminal entries exist.
- **When** GetDeadLetters uses positive limit or a nonpositive limit.
- **Then** positive listing exposes at most that many nondiscarded dead letters including key/event/attempt/failure summary; nonpositive limit fails.

## INBOX-008-AC-02 — Requeue only eligible keys and wake

- **Given** eligible, absent, completed and repeated keys are supplied.
- **When** Requeue finishes.
- **Then** only eligible dead letters become unresolved, Attempts zero and claim/due/terminal state cleared, diagnostics retained; Requested counts all supplied keys and Requeued counts transitions.

## INBOX-008-AC-03 — Discard and empty collections

- **Given** an eligible dead letter, already discarded/absent keys, or an empty collection.
- **When** Discard or Requeue runs.
- **Then** eligible discard records DiscardedAt without losing diagnosis; ineligible keys do not transition and empty collections return zero work.

## INBOX-008-AC-04 — Expose partial success and reject ambient management

- **Given** several keys, with cancellation/failure after an earlier transition, or Transaction.Current present.
- **When** a manager mutation runs.
- **Then** earlier independently committed transitions remain durable on later failure; ambient mutations are rejected rather than joining a batch transaction.

## INBOX-008-AC-05 — Reject null management collections

- **Given** Requeue or Discard receives a null key collection.
- **When** the manager operation is requested.
- **Then** ArgumentNullException is observable and no key transition occurs.
