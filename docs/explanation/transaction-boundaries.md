# Understanding transaction boundaries

## The question

Why does Clean.Messaging join some operations in a local database transaction, but deliberately keep admission, delivery and failure settlement separate?

A transaction is useful only when its boundary matches the state that must remain consistent. In messaging, several different facts are easily confused:

- a message was accepted for durable processing;
- a particular consumer completed its local state change;
- an outgoing operation was captured durably;
- a remote system accepted or acted on a publication.

These facts happen at different times and are owned by different systems. Treating them as one universal “message processed” event conceals failure windows rather than removing them.

This explanation interprets the implementation reviewed on **13 October 2026**. The causal arguments and alternatives below are **conceptual interpretations, not historical decision records or evidence of the original authors' intentions**. Exact source-based contracts and known limitations are in [Inbox](../reference/inbox.md), [Outbox](../reference/outbox.md), [Persistence](../reference/persistence.md) and [Domain events](../reference/domain-events.md). No execution, build or tests were performed for this documentation review.

## Local atomic capture answers the dual-write problem

Suppose an application changes an order and then publishes an outgoing notification. Two separate writes create two asymmetric failure windows:

1. Save the order, then fail before publishing: the order exists without its notification.
2. Publish first, then fail before saving: recipients see a notification for a state change that did not commit.

A local outbox changes what is written together. Instead of trying to atomically save a database row and contact a broker, the application saves its owning state **and a durable description of outgoing work** in the same database boundary. A later worker handles delivery.

The important claim is not “publication became atomic”. It is “the decision to publish was captured with the state that justified it”. The distinction explains both the value and the limit of the pattern:

```text
Owning local transaction                 Later delivery
-----------------------------------     --------------------------
application state + outbox targets  -->  local consumer or transport
              commit                    record target completion
```

If the owning transaction rolls back, its newly captured work should not become independent committed work. If it commits but the process stops before a wake-up is observed, the rows still exist for polling/recovery. An in-memory wake signal is therefore an optimisation for latency, not the evidence of durability.

This comes with costs: extra rows, delayed delivery, workers, retention policies, serialization compatibility and operational management. A notification does not necessarily arrive before the originating request returns. A recipient can temporarily observe an older state even though the originating transaction has committed.

The [outbox reference](../reference/outbox.md) also documents a practical constraint: `Enqueue` only stages tracked rows. Atomic capture depends on the application's same-context save or explicit EF transaction and on the relevant interceptor installation. Merely using a type called “outbox” does not establish that boundary across another database or arbitrary SQL.

## Domain-event acknowledgement follows capture, not delivery

An in-memory domain event says that an aggregate has produced a fact that still needs capture. Removing it before the owning save succeeds risks losing the intent when the save fails. Never removing it risks recapturing it on each later save.

The event-source/outbox integration associates acknowledgement with the observed or verified local commit. During an explicit transaction, a successful `SaveChanges` is not enough: the transaction can still roll back. This is why the tracker separates current-save state from pending-commit state.

After that commit, the event's remaining delivery responsibility lives in the outbox rows rather than in the aggregate's list. A dead-lettered outgoing target does not put the event back into the aggregate. Retrying delivery and deciding what the aggregate means are different responsibilities.

This is an interpretation of the [domain-event capture mechanism](../reference/domain-events.md), not a claim that the library implements event sourcing. Pending events are in memory before capture; the library provides no complete aggregate event history or event-stream rehydration.

## Independent admission separates acceptance from success

An incoming message has a different starting point. The transport needs an answer about durable acceptance, not a promise that all future business processing has already succeeded.

Clean.Messaging's EF inbox creates a fresh scope/context for admission and rejects ambient transactions. It saves the original admission and initial per-consumer entries independently of the caller's scoped context. Subsequent processing owns its own transactions.

Conceptually, this separation prevents an unrelated caller's later rollback from silently undoing the evidence on which acceptance was based. It also permits slow or temporarily failing consumers to run after transport receipt rather than holding a broker callback open for an entire business workflow.

```text
Receive envelope
      |
      v
Fresh local admission save ----> transport-specific acknowledgement decision
      |
      v
Claim consumer A --> A's local state + A's completion commit
Claim consumer B --> B's local state + B's completion commit
```

The arrow to acknowledgement is a relationship, **not** a distributed atomic transaction. A process can commit admission and fail before acknowledging the broker; a repeated delivery must be recognised by identity. Likewise an admission exception must not be interpreted as successful acceptance simply because the broker can retry later. Exact broker and poison-message policies are in [Transport](../reference/transport.md).

There is also an explicit trade-off: admission is not rolled back with arbitrary work the caller was doing. Applications cannot use `Accept` as a substitute for capturing an outgoing operation inside their own transaction; that is the outbox's role. Rejecting `TransactionScope` makes the unsupported boundary visible instead of implicitly enlisting or suppressing it.

### Acceptance is not a promise about every future consumer

Initial consumers are selected when admission is created. A duplicate acceptance returns for the existing message/event identity without expanding its targets or comparing payloads. Later consumer participation is a replay operation, not an accidental side effect of transport redelivery.

This keeps “same input was received again” separate from “new processing responsibility has been assigned”. It also means identity stability matters: using a different ID on each transport retry defeats admission duplicate recognition, while reusing an ID for a different payload conceals that mismatch.

The current standard consumer registry rejects messages without local consumers; the EF empty-batch path also rejects empty admission batches. These are implementation limits, not an inferred architectural rule that every possible inbox must forbid admission for later replay. See [Inbox](../reference/inbox.md) for those details.

## Local completion joins the effect with its evidence

A consumer changes local state and then needs to mark its inbox/outbox target complete. If those actions commit independently, another dual-write problem appears:

- state commits but completion does not: the consumer can be invoked again and repeat the local change;
- completion commits but state does not: the message appears finished without its intended local effect.

The EF inbox dispatcher and local outbox delivery join consumer work and conditional completion in one local transaction, provided the consumer uses the configured scoped context. If the claim is no longer valid, completion affects no row and the transaction is rolled back.

This ties two statements together: “these local changes were made” and “this owned target completed”. It does not tie every side effect performed by the consumer together. An HTTP call, another context or a broker publish can still escape the transaction. The implementation can roll back database work; it cannot un-send a request.

EF execution strategies add another consideration. They may rerun the transaction delegate after a transient failure. A fresh invocation first checks for persisted completion, which can avoid repeating a known committed local result. Nevertheless, execution-strategy retries are not a proof that the handler ran only once. Handler designs must account for repeated execution and for any effects outside the joined local transaction.

## Claims fence settlement, not the world

A durable claim gives a worker temporary ownership. Its token and expiry are used in conditional completion, retry and dead-letter updates. Another worker can claim expired work, and the old worker must not then overwrite the new owner's settlement.

The conceptual benefit is narrow but valuable: the database refuses stale ownership at the point where local evidence changes. Lease renewal gives long-running work a chance to retain ownership; renewal failure causes cancellation.

Cancellation is cooperative, however. A stale worker can already have made a remote request, or its transport may not immediately stop when cancellation is requested. A lease predicate cannot reach into the remote system and retract that request. Increasing a lease can reduce some overlaps but delays recovery; decreasing it improves recovery speed but increases sensitivity to pauses, queued claims and renewal latency. There is no lease setting that transforms arbitrary remote effects into exactly-once execution.

The [processing reference](../reference/processing.md) explains renewal and work-loop settings; [persistence](../reference/persistence.md) gives exact ownership predicates and provider-sensitive claiming behaviour.

## Failure settlement needs its own boundary

If a local consumer transaction fails, its business changes are rolled back. The runtime still needs to record when the work can retry or why it should dead-letter. Recording that metadata in the transaction that just failed would also roll back the metadata.

Separate failure settlement therefore serves a different consistency requirement from successful completion. It records a failed attempt against a still-owned durable entry after the unsuccessful local attempt ends. The processor bounds settlement time and links it to claim loss. If settlement itself fails, the row can remain claimed until expiry and later recovery.

This explains why there can be more than one retry mechanism without them being interchangeable:

- EF execution strategies retry provider operations/transaction delegates;
- durable retry schedules a later message attempt;
- a broker can redeliver a publication when acknowledgement is uncertain.

These layers can multiply execution opportunities. Retry does not replace stable logical identities or downstream idempotency. See [Processing](../reference/processing.md) and [Transport](../reference/transport.md) for exact retry and acknowledgement contracts.

## External publication retains a completion gap

The external outbox adapter calls the transport and only then records completion in the database. If publication succeeds and database completion fails, the durable entry remains available for another delivery after retry/recovery. Reversing the order would replace duplicate risk with loss risk: marking complete first could abandon a publication that never occurred.

This is the classic trade-off of an outbox without a distributed commit protocol. Durable capture favours recoverable delivery attempts, while remote duplicate handling remains necessary. Stable event/delivery/target identities help receivers distinguish retries from new operations, but receivers must implement their own appropriately scoped duplicate protection.

Remote acknowledgement can mean different things at different layers: a broker accepted a publication, a subscription received it, or an application committed its effect. Those are not equivalent. MQTT QoS, for example, cannot by itself establish atomic application processing across this database boundary. The [transport reference](../reference/transport.md) owns the actual provider claims.

## Independent targets avoid coupling successful work to failed work

Fan-out is stored as separate target entries. This allows target A to complete while target B retries or dead-letters. A failure does not require replaying every already-successful target just to reach the failed one.

The benefit is isolation and focused recovery. The cost is that fan-out is not an all-or-nothing delivery transaction: observers may see a partially delivered event. If a business process needs a durable coordinated response to partial success, it needs an explicit workflow and state model rather than treating outbox fan-out as distributed rollback. [Sagas](../reference/sagas.md) describes the supplied workflow capability; compensation, where implemented by an application, is another business action rather than undoing a past database commit.

[Scheduling](../reference/scheduling.md) introduces another local boundary: due work can stage later effects while completing its own local dispatch. It does not remove the later remote publication gap.

## Retention changes the meaning of duplicate protection

A unique `(EventId, TargetId)` index protects a logical target only while the corresponding row exists. Deleting terminal outbox entries removes that protection. Reusing the logical identity later can therefore create new work after retention has removed old evidence.

Inbox admission and consumer-entry retention are separate in the current implementation. Shared cleanup deletes terminal inbox entries, not `InboxAdmission`. Consequently, ordinary duplicate `Accept` can remain blocked while replay recreates a deleted per-consumer entry from the retained admission. Replay is missing-target creation, not a universal reset of existing successful work.

The trade-off is not simply “more disk versus less disk”. It is storage, diagnostics, replay availability, consumer reassignment and the lifetime of duplicate evidence. Applications need a retention policy consistent with the longest plausible retry/redelivery horizon and their data-governance obligations. The library does not supply an unlimited exactly-once history or automatic admission cleanup.

## Alternatives and when the boundary differs

The following are conceptual alternatives, not features claimed to be implemented by Clean.Messaging.

| Alternative | What it can simplify or strengthen | What must still be addressed |
| --- | --- | --- |
| Direct publish after saving | Fewer persistence/work-loop components and lower immediate complexity. | Crash/failure gap between local save and publish; a recovery mechanism or accepted loss semantics is needed. |
| Publish before saving | Immediate transport action. | Recipients can observe an operation whose owning state rolls back. |
| Hold transport receipt through handler execution | One synchronous operational path and potentially simpler latency reasoning. | Long broker callbacks, redelivery after uncertain success, and local/remote transaction mismatch. |
| Distributed transaction across participants | A coordinated atomic commit where every participant genuinely supports the protocol. | Provider/broker support, operational coupling, availability and recovery complexity; the current outbox rejects ambient transactions rather than providing this. |
| Database change-data capture / transaction-log publication | Capture changes from committed database history instead of application outbox staging. | Translation from physical changes into semantic contracts, log offsets/retention, duplicate handling and external completion semantics. |
| Event sourcing | Durable domain facts can become the primary state history. | Stream concurrency, event evolution and rebuilding state; outbound delivery still needs reliable projection/publication. The current event-source helper is not such an event store. |
| Receiver-side idempotency | Can suppress repeat remote effects within the receiver's own commit boundary. | Correct operation/consumer key, concurrent duplicate handling and retained evidence; not provided automatically by a publisher's lease. |
| Explicit saga/process manager | Durable coordination of independent transactions and responses. | Partial progress and compensation are explicit workflow semantics, not global database rollback. |

The useful comparison is which **fact** each option commits atomically, not whether an option advertises “reliable messaging”. Local transactions are powerful because their participant set is explicit; claiming a wider boundary than the mechanism supports is where false guarantees begin.

## Reading the implementation without overclaiming

A precise way to summarise the current design is:

> Durable admission records accepted input independently. Local outbox capture joins outgoing intent to owning state. Local delivery joins same-context effects to owned completion. Remote delivery remains a publish-then-complete sequence, with repeat delivery possible.

That summary remains conditional on correct model/interceptor installation, a supported relational execution path, stable identities and the application's use of the configured context. Source-visible limitations—including no-local-consumer target lookup, replay exception/count behaviour and cleanup-service lifetime wiring—are recorded in the references rather than rationalised as deliberate permanent rules.

For exact signatures and integration restrictions, consult [Persistence](../reference/persistence.md), [Inbox](../reference/inbox.md), [Outbox](../reference/outbox.md), [Domain events](../reference/domain-events.md), [Contracts](../reference/contracts.md), [Configuration](../reference/configuration.md) and [Processing](../reference/processing.md).
