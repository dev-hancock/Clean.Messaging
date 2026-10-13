# Understanding durable workflows

How do sagas, outbox messages, and durable timers cooperate without pretending that a database transaction can include another service?

Clean.Messaging implements three different kinds of durable record: workflow state, outgoing delivery work, and future local delivery work. Understanding their relationship helps distinguish a successful state transition from a successfully delivered message, and both from a completed business process.

This explanation is grounded in the current source, reviewed on **2026-10-13**. The trade-offs below are consequences of the implementation and comparisons with alternatives, **not claims about the maintainers' original motivations**. No examples, database behavior, Quartz cluster, or deployment were executed for this review. Exact APIs and limits are in [sagas](../reference/sagas.md), [scheduling](../reference/scheduling.md), [outbox](../reference/outbox.md), and [persistence](../reference/persistence.md).

## Three records answer three different questions

A workflow spanning messages and time cannot be represented adequately by one in-memory call stack. A process can stop after deciding what to do but before notifying another participant; a response can arrive much later; a deadline can pass while the host is offline.

The mechanisms address distinct questions:

| Record | Question it answers | What it does not prove |
| --- | --- | --- |
| Saga state | What does this local workflow currently know, and which transition is allowed next? | That another system has performed an action. |
| Outbox entry | Which target still needs this outgoing message delivered? | That delivery can never repeat, or that the recipient's business effect happened. |
| Scheduled message | Which local message should become eligible at or after a due instant? | That it ran exactly on time, or that its business outcome is already true. |

These are connected, not interchangeable. A saga's `Send` result becomes staged outbox work. Its timer becomes a scheduled message addressed to that saga instance. When the timer is processed, it invokes a saga handler directly; that handler may update state and stage further outbox work. Ordinary scheduled messages instead enter the outbox when due.

The [saga runtime](../../src/Clean.Messaging.Sagas/Runtime/SagaRuntime.cs), [effect writer](../../src/Clean.Messaging.Sagas/Effects/SagaEffectWriter.cs), [saga timer delivery](../../src/Clean.Messaging.Sagas/Delivery/SagaScheduleDelivery.cs), and [scheduled outbox delivery](../../src/Clean.Messaging.Scheduling.Outbox/OutboxScheduleDelivery.cs) show these distinct paths. There is no independent saga timer table or saga scheduler competing with scheduling.

## Durable intent closes the local dual-write gap

Suppose an application needs to record that a reservation is pending and request payment. If it commits reservation state and then directly publishes the payment request, a crash between those steps loses the request. Publishing first reverses the risk: payment can be requested even though the reservation transaction later rolls back.

The outbox makes the request a local persisted intention. Under same-context EF composition, saga state, business changes, and outgoing outbox rows can be saved in the same transaction as completion of the incoming durable delivery. The outgoing message is delivered later.

A timer fits that same local commitment: the state can say “await payment” while the transaction also records a future payment-deadline message. The local commit is then a bundle of facts and intentions:

```text
incoming durable message
        |
        v
saga handler interprets message + saved state
        |
        v
one local EF transaction
  - business changes, if using this same context
  - updated saga state and version
  - outgoing outbox entries
  - new or cancelled schedules
  - incoming delivery completion
        |
        +---- later outbox processing ----> local consumer or external transport
        |
        +---- later schedule processing -> saga timer handler or outbox staging
```

For timer-driven transitions, the scheduling executor owns that local transaction and completes the timer rather than an inbox/outbox consumer row. For ordinary saga messages, the EF inbox or local outbox dispatcher owns it. The saga runtime itself neither starts a transaction nor saves independently.

That qualification matters. A handler writing through another `DbContext`, committing separately, or calling a payment API directly leaves the shared transaction boundary. Dependency-injection registration cannot turn those independent effects into an atomic unit. The [EF schedule executor](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleExecutor.cs), [inbox dispatcher](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxDispatcher.cs), and [local outbox delivery](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/LocalOutboxDelivery.cs) are the concrete transaction owners.

## Publication remains a separate uncertainty

The outbox solves local state-plus-intent capture, not a distributed commit with a broker. An external publish can succeed and then the local completion write can fail. A subsequent attempt can publish again. A claim prevents a stale worker from settling someone else's database work; it cannot recall a packet already sent.

A saga handler can also execute more than once following a persistence conflict, failed transaction, or a later durable retry. Staging local effects in the transaction gives those effects the same rollback boundary as state. Performing irreversible work in the handler does not.

This makes message and effect identity part of business integration, rather than a cosmetic tracing detail:

- Saga type and key identify a logical workflow; the generated saga ID identifies its retained instance.
- A start records the incoming delivery MessageId. Repeating that same persisted start is a no-op; a different start on the same key conflicts or is closed.
- A saga outgoing effect ID becomes outbox EventId. Outbox processing uses a separate delivery MessageId and target identity.
- A timer ID is a global schedule identity. Its group associates it with the saga; it is also the timer-trigger MessageId.
- Correlation groups related work for context; it is not automatically the saga key or the deduplication key.

Changing IDs on every retry creates new logical work. Conversely, repeating a schedule ID is not a built-in upsert: the current EF schedule writer adds a row and relies on its primary key. Correct retry handling must coordinate stable identity with the actual persistence behavior. See [contracts](../reference/contracts.md) and the two capability references for precise propagation.

Recipients still need a duplicate policy appropriate to their effects. A local inbox can durably separate admission and per-consumer processing, but its protection is bounded by retained records. A remote payment endpoint may need its own idempotency key or status reconciliation. No local “exactly once” label bridges those boundaries.

## Time is an input to a transition, not a perfect alarm

A durable timer survives loss of an in-memory wait because its due instant and payload are database state. When the host returns, the worker can find eligible rows. A local wake signal reduces latency, while recovery polling provides another discovery path. The optional Quartz integration adds a wake job; it does not store one Quartz job per message or replace the database's schedule lifecycle. Its local timed fallback remains active even if Quartz scheduling fails.

The consequence is deliberately weaker than a real-time alarm: a schedule becomes **eligible** when due, then competes for a claim and execution slot. Host downtime, backlog, contention, provider faults, or retries can delay it. A business handler therefore must interpret the timer against current state.

For an illustrative reservation workflow:

1. A start sets “awaiting payment,” stages the payment request, and stores the deadline timer identity in state.
2. A payment response can transition to “confirmed” and cancel the deadline timer.
3. A deadline message can transition to “expired” and stage a cancellation/refund intention if policy calls for it.
4. If response and deadline race, version checking prevents both conflicting local state updates from simply overwriting one another. A failed attempt must be reconsidered through the owning delivery's retry path.

This is a conceptual example, not a supplied application or prescription for payment policy. The handler must decide whether expiry is based on response receipt time, a provider's payment timestamp, a deadline stored in state, or another rule. The library does not make that decision.

Cancellation likewise means “prevent this still-active local schedule from being dispatched if cancellation wins the database coordination.” It does not undo a transition already committed, stop a remote action already requested, or roll back an external effect. Completing or failing a saga cancels its remaining timers; it does not erase outgoing outbox messages already staged. [Scheduling](../reference/scheduling.md) details dispatch gating, lease checks, and the absence of claim renewal.

## Concurrency protects state, not the handler's past

Different durable inputs can legitimately address the same saga. Each may deserialize the same old state and make a different decision. The saga's numeric version is an optimistic concurrency token: only an update consistent with the saved version can commit normally. A unique type/key index also prevents competing start instances from both owning the same logical key.

Those checks happen at local persistence boundaries. They do not ensure that only one handler ever ran, and they do not make a handler's earlier external calls disappear. This is why an effect description returned by the handler has a different consistency profile from an immediate HTTP call made inside it.

There is also no built-in serialized mailbox or long-lived per-saga lease. An ordinary saga continuation relies on the incoming durable consumer's processing controls and optimistic state concurrency. A timer uses scheduling's separate claim mechanism. Scheduling batches acquire their leases before parallel dispatch slots are available, and there is no scheduling renewal loop; batch size, execution latency, and lease duration therefore interact.

The implementation's competing-start translation distinguishes a same-message race from a different logical start, but it is not a general conflict-resolution algorithm. Business operations that cannot tolerate a later re-evaluation may need explicit serialization, stronger domain constraints, or a different workflow architecture. Version protects against lost updates; it is not a replay log or state-schema version.

## Business failure differs from delivery failure

A saga result `Fail` says the workflow itself has reached a failed terminal outcome. It persists state/diagnostics, can stage outgoing effects, and cancels outstanding timers. A thrown handler exception says this attempt did not successfully apply its result; the owning durable processor may retry or fault that delivery.

The difference has operational consequences:

- A payment refusal may be a valid business transition producing `Fail` or another state, not a transport exception.
- A database outage can fail a timer attempt without proving that the reservation is business-failed.
- A permanently failed/exhausted timer can leave its saga active and waiting indefinitely. The runtime does not infer the business outcome from delivery exhaustion.
- A late continuation for a terminal saga is ignored; it does not reopen it.
- Completing a saga does not wait for every outgoing target to finish, nor certify that the whole external business process is finished.

A host that needs escalation for stuck workflows must define what “stuck” means using saga state, timer status, and delivery status together. The management API is read-only; it is not a repair console or automatic recovery engine. [Sagas](../reference/sagas.md) and [processing](../reference/processing.md) separate these behaviors precisely.

## Compensation is another forward operation

Database rollback can discard the current local transaction. It cannot refund a payment previously committed by another service. Compensation instead asks for a new action that semantically mitigates an earlier one: release a reservation, cancel a shipment if possible, or issue a refund.

Clean.Messaging can durably stage such a message as an ordinary saga effect, but it does not invent compensation steps or execute rollback across participants. The application must define correlation, compensation identities, conditions, retries, partial failure handling, and when compensation is no longer possible. A compensation can itself be delivered twice or fail independently.

This is one reason “saga completed” is only a local workflow outcome. Its business meaning must account for what has actually been observed from participants, rather than treating durable intent as confirmation of success.

## Retention changes what can be known and repeated

The mechanisms intentionally have different current cleanup effects:

- Saga cleanup purges terminal **state payloads**, replacing them with `null`, but retains type/key identity, start MessageId, outcome, and the unique-key record.
- Schedule cleanup deletes old terminal rows, including saga timer history.
- Inbox/outbox retention follows their own completion/dead-letter policies.

A saga key therefore remains closed after its state is purged. This supports duplicate-start/terminal-message behavior without needing the old state, but also grows identity rows and prevents an automatic “reuse this key after 30 days” interpretation. Repeating a business process generally needs an intentional new logical key or application-defined lifecycle tooling, not an assumption that payload cleanup reopened it.

At the same time, inspecting a retained saga later cannot necessarily reconstruct its timers or outgoing deliveries: those records may already be gone. This is not event sourcing. Operational audit requirements, privacy requirements, and deduplication horizons may need additional application-owned records and retention policies. [Sagas](../reference/sagas.md) describes tombstones; [scheduling](../reference/scheduling.md) describes timer deletion.

## When another architecture is a better fit

These comparisons are design alternatives, not additional implemented features:

| Alternative | Useful when | Main trade-off relative to this composition |
| --- | --- | --- |
| One synchronous local transaction without a saga | The operation finishes within one database boundary and needs no later event/time input. | Simpler state and fewer durable workers; not a solution to remote dual writes. |
| Outbox without saga state | A local change merely needs reliable later notification, with no multi-step coordination. | Avoids workflow machinery; cannot itself track a conversation or deadline-dependent business transition. |
| Scheduled outbox message without a saga | One deferred message is enough, independent of accumulated workflow state. | Simpler timer ownership; cancellation/business recovery still needs explicit policy. |
| Event choreography | Participants can react independently without one local coordinator tracking the whole process. | Less central orchestration, but end-to-end status and cycle/compensation reasoning become distributed. |
| Serialized per-workflow queue or partition | Strict handling order or reduced concurrent state conflicts is essential. | Adds routing/partition and head-of-line blocking concerns; still needs persistence and external-effect protection. |
| Dedicated durable workflow engine | Workflow graphs, human tasks, rich replay/history, built-in orchestration tooling, or complex timer semantics dominate. | More platform/runtime coupling and operational infrastructure; external effects still have their own idempotency boundaries. |
| Event-sourced process manager | Reconstructable transition history is a core requirement. | Adds event evolution, projections, and replay discipline; saga JSON snapshots/tombstones here are not that history. |
| Direct external call plus reconciliation | A synchronous user-visible response matters more than asynchronous handoff and uncertainty can be reconciled. | Requires explicit handling of ambiguous outcomes and duplicate requests; a local transaction cannot prove remote success. |
| Distributed transaction | All participants genuinely support and can operate under the same transaction protocol. | Strong participant/protocol coupling and availability constraints; not supplied for broker/API effects by this library. |

A saga is appropriate when the application genuinely needs durable, local state across separate inputs. It is not necessary merely because code calls more than one method. Durable timers and outbox effects can be useful on their own; adding saga state should correspond to a real coordination question.

## Reading the guarantees at the right boundary

The practical mental model is a sequence of local commitments, not one long transaction:

1. The incoming record durably represents accepted work.
2. A local transition commits state and outgoing/future intentions with its local completion marker when correctly composed.
3. Each later delivery has its own claim, attempt, and completion boundary.
4. Remote business confirmation arrives as another input, not as a property of local outbox completion.

This model exposes where duplicates, latency, conflicts, and uncertainty remain. It also makes the application responsibilities explicit: stable identity, compatible state/contracts, deadline interpretation, business failure policy, compensation, remote idempotency, and operational reconciliation.

For factual lookup, continue with [sagas](../reference/sagas.md), [scheduling](../reference/scheduling.md), [outbox](../reference/outbox.md), [inbox](../reference/inbox.md), and [persistence](../reference/persistence.md). The wider ownership/dependency map is in [architecture](../reference/architecture.md).
