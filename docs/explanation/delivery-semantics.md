# Understanding delivery, processing, and effects

## What does “delivered” mean?

A broker can successfully deliver a packet while the corresponding consumer has not run. A consumer can run while its database transaction eventually rolls back. An outgoing publication can succeed while the publisher's completion record remains unresolved. These are different observations, not contradictory reports about one indivisible operation.

Clean.Messaging connects these boundaries with durable records, identity, retries, and conditional ownership. It does not turn a broker, a local database, and arbitrary external systems into one transaction. This explanation helps readers reason about the resulting failure windows and trade-offs. Exact contracts and settings belong to the [Transport](../reference/transport.md), [Processing](../reference/processing.md), [Inbox](../reference/inbox.md), [Outbox](../reference/outbox.md), and [Persistence](../reference/persistence.md) references.

**Source reviewed: 2026-10-13; not integration-tested.** The mechanisms described are derived from this checkout. The causal explanations below are consequences of those mechanisms, not claims about undocumented historical design intent, broker deployments, or tested provider support.

## Three boundaries, three meanings of success

| Boundary | What success establishes | What it does not establish |
| --- | --- | --- |
| Protocol delivery | The relevant MQTT operation reports success/acknowledgement at its protocol boundary. | Consumer completion, a business outcome, or an atomic cross-system commit. |
| Durable local processing | An entry has been admitted, or a particular target has completed under the local persistence rules. Admission and completion are themselves different stages. | Completion of every target, downstream consumption, or rollback of arbitrary external effects. |
| External effect | A recipient performs an operation such as sending an email, charging a payment, updating another service, or reacting to an MQTT payload. | That the originating local completion was recorded, or that a repeat will be harmless. |

For incoming MQTT, the worker acknowledges after durable inbox admission. This transfers responsibility for later attempts from broker redelivery to local durable processing; it does **not** announce that all consumers have succeeded. A message can be acknowledged and subsequently retry or dead-letter locally.

For outgoing MQTT, outbox dispatch awaits publication, then records completion in the database. That completion means the transport attempt succeeded and its target entry was settled. It is not a receipt from every subscriber, and certainly not evidence that a subscriber's real-world effect happened once.

MQTT QoS 1/2 regulates protocol delivery exchanges. The name “ExactlyOnce” in a MQTT enum cannot be expanded into exactly-once consumer invocation or external effects. A database completion retry, replay operation, application-generated second publication, session reset, or failed downstream transaction sits outside that protocol guarantee. The library does not supply an end-to-end exactly-once guarantee.

## Repeated delivery is not the same as repeated work

Repeats arise at different layers:

- The broker may redeliver an unacknowledged incoming message.
- An inbox/outbox entry may be claimed again after a failure or expired lease.
- An EF execution strategy may retry local consumer dispatch before durable processing schedules its next attempt.
- A publisher may repeat after publication succeeded but local completion was not recorded.
- An operator may requeue a dead letter or replay retained admissions.

Duplicate protection needs an identity meaningful at the layer where it is applied. Clean.Messaging distinguishes the delivery `MessageId`, logical `EventId`, and per-consumer/per-route `TargetId`. Outbox staging generates a delivery ID for a batch and uses the caller's outbox ID as logical event identity. A retry of an already-stored entry preserves both IDs. Inbox uses the provided event identity, defaulting it to the delivery identity when omitted.

A packet identifier is not a substitute for these logical identities. Incoming MQTT admission requires valid `message-id` metadata; it does not generate a new identity every time an anonymous packet arrives. Generating a new event ID on each repeat would prevent the local store from recognizing the same logical operation.

### Admission and per-target protection answer different questions

The current EF inbox considers an admission already accepted when either its message ID or event ID exists. It does not compare the repeated payload for equivalence. Reusing an identity for a genuinely different operation can therefore suppress that operation rather than update the earlier admission. Identity stability requires identity uniqueness for distinct operations as well as reuse for legitimate repeats.

Consumer entries and outbox target entries have unique `(EventId, TargetId)` keys alongside their delivery/target primary key. This separates each target's progress. If one consumer completes and another fails, local recovery works on the unresolved entry; it does not deliberately reset the successful sibling. This is useful fan-out independence, not atomic “all recipients succeed together” processing.

It is also important not to infer a universal idempotent enqueue API from a unique index. The current outbox writer suppresses duplicate event/target entries already tracked in its context; the database also enforces uniqueness. A separate attempt to stage a duplicate against an existing stored row is not necessarily silently accepted. The [Outbox](../reference/outbox.md) and [Persistence](../reference/persistence.md) references define the actual staging/save behaviour.

Deduplicating a stored delivery protects that record's lifecycle. It does not automatically deduplicate an external effect performed before local completion, nor a second logical event describing the same business action. Applications may need an effect-specific idempotency identity meaningful to the external recipient, rather than relying solely on transport delivery identity.

## Local atomicity has a deliberate scope

The inbox admission path uses a fresh scope and rejects an ambient transaction. Admission is independent of a caller's state changes. Consequently, once admission succeeds, a later caller failure does not imply that admission disappeared. This supports the worker's acknowledgement boundary, but callers cannot treat admission as part of an arbitrary surrounding transaction.

Local inbox dispatch and local outbox delivery begin a transaction in the configured scoped context. Consumer changes, owned completion, save, and commit happen together. If owned completion is rejected, that transaction rolls back. If the process fails before commit, its uncommitted local changes are not a successful completed attempt. On an execution-strategy repeat, the dispatcher checks an existing completion before invoking the consumer again.

The qualifier **configured scoped context** matters. A consumer's HTTP request, file write, MQTT publication, second database context, or device command is not part of this transaction merely because it occurs inside `Handle`. Such effects may survive rollback. Invoking a consumer exactly once is therefore a stronger claim than committing one successful local transition, and this implementation does not make the former claim.

Outbox staging can capture outgoing work alongside its owning database state when the required save/transaction integration is installed. This addresses the local dual-write window between changing state and recording an intention to publish. It moves network publication out of that transaction; it does not make publication atomic with it. [Domain-event capture](../reference/domain-events.md), [Scheduling](../reference/scheduling.md), and [Sagas](../reference/sagas.md) can stage further durable work, but that additional local coordination does not remove the later external-effect boundary.

## Failure windows and their consequences

| Window | Possible result | Recovery implication |
| --- | --- | --- |
| Before inbox admission commits | No accepted local work yet. | Ordinary admission failure leaves MQTT unacknowledged and requests reconnect; eventual broker redelivery depends on its session/storage policy. |
| Admission committed, acknowledgement missing or lost | Local admission exists while the broker may still repeat delivery. | Stable IDs let retained admission recognize the repeat; consumer work is already local responsibility. |
| Incoming poison detected | The worker logs and attempts acknowledgement without storing a durable poison record. | The message may be discarded from broker delivery without an inbox dead letter. A failed poison acknowledgement can still repeat. |
| Consumer invoked, local transaction not committed | Consumer invocation occurred, but configured-context changes/completion may roll back. | A later attempt can invoke the consumer again; any nontransactional effect may already have happened. |
| Local commit succeeded, caller observed an exception/uncertainty | Database truth and the caller's observation can disagree. | Execution-strategy/completion checks help recognize persisted completion; they cannot undo an external effect. |
| Outgoing publish succeeded, completion missing | Subscriber may have received the message while the outbox entry remains unresolved. | Reclaim/retry can publish the same event again. Recipient duplicate protection is needed for repeat-sensitive effects. |
| Claim expired while old handler continues | New owner can start while old execution still runs. | Conditional settlement protects database state, but cancellation must be honored and external effects may overlap. |
| Retry/dead-letter settlement fails | No recorded retry or terminal transition is certain. | The lease and periodic recovery permit another claim; a pre-settlement log alone is not proof of state. |
| Terminal records removed by cleanup | Earlier target identity/completion evidence no longer exists in that table. | New staging or replay can behave differently from when the records were retained. |

The same interval can have a different consequence depending on whether a recipient cooperates with identity and idempotency. For example, repeating a “set state to X” operation may be naturally harmless; repeating “increment balance” usually is not. The library cannot infer business semantics from a message's serialized shape.

A transactional recipient can atomically record its own consumed identity with its own local change. An external API supporting a stable idempotency key can deduplicate the effect at its ownership boundary. Without those facilities, reconciliation or tolerating repeats may be necessary. These are possible application strategies, not mechanisms supplied automatically for every external transport by Clean.Messaging.

## Leases protect ownership, not all execution

A lease allows unfinished work to become available after a worker disappears. Claim tokens distinguish attempts, and settlement checks the key, token, unresolved state, and unexpired lease. A stale owner cannot simply mark a new owner's entry complete. This is **fencing at the database update boundary**.

Renewal extends that ownership while a handler runs. Rejected renewal or a renewal exception cancels the attempt's handling token. Cancellation is a request, not forced termination. A remote service can accept a request even as its caller is cancelled; a consumer can ignore its token; a network result can arrive after lease loss. Fencing does not revoke an already-issued external effect.

The local transactional path offers a stronger protection for its participating state: the stale attempt's failure to complete rolls back that transaction. But the consumer runs before the completion check, so nontransactional effects can still occur during a stale attempt. The external outbox path similarly publishes before checking completion. Neither path uses a database ownership token as a remote-system fencing token.

### Lease duration, queue capacity, and recovery are coupled

Short leases improve recovery latency after an abandoned attempt, but make slow work and pauses more likely to lose ownership. Longer leases reduce those interruptions but delay reclamation of a genuinely dead worker. Renewal reduces dependence on a fixed total handler duration, while adding dependence on timely renewal I/O and the clocks used to compute expiry.

The current work loop leases entries when claiming, before queueing. Renewal starts only when processing starts. A large queue behind slow consumers can consume most of an entry's lease before its handler begins. Increasing capacity/batch size does not necessarily increase safe throughput; it can increase this ownership exposure. Deterministic claim ordering also does not imply ordered completion under concurrency or retries.

Expiry and settlement compare UTC timestamps supplied through `TimeProvider`. Multiple hosts depend on reasonable clock alignment with the persistence operations. Provider SQL/transaction behaviour, deployment latency, and handler duration require real integration/operational validation; a source review does not establish universal multi-host performance or safety margins.

## Retry policy is a recovery budget, not a guarantee of success

Durable retries allow transient faults to be retried later without keeping the current handler call open indefinitely. Permanent `MessageException` failures dead-letter immediately; unknown failures are retryable within the policy. Exhaustion stops automatic failure retries by dead-lettering the entry, preserving diagnostic context for management.

Backoff spreads repeated failures over time; a capped delay bounds the waiting interval, and full jitter reduces synchronization between many failing entries. This trades latency for reduced retry pressure. A due time is eligibility, not a scheduling promise: worker capacity, polling, claims, and unavailable storage can delay execution further.

There are multiple retry layers. MQTT ingress reconnect, EF execution strategies, producer claim recovery, and durable per-entry retry have different ownership and counters. They are not interchangeable. In particular, durable `Attempts` increments on claim, so it does not count every EF invocation or network packet exchange. It can also increment when claimed work never reaches a handler. Maximum failure attempts should not be interpreted as a universal maximum number of effects: eligibility has no hard maximum-claim filter after repeated lease-loss/cancellation without settlement.

Dead-lettering separates work that no longer automatically retries from work that succeeded. A transient failure can be dead-lettered by exhaustion while its stored category remains transient. Requeue resets attempts but preserves identity and failure history. It starts another local processing budget; it cannot assert that previous external effects never occurred. Discard stops local retry without reversing any effect already performed.

## Retention changes the meaning of “already seen”

Duplicate detection depends on retained evidence. Removing a completed outbox row removes that target's stored event identity and completion protection. A later operation using that identity can no longer be judged from that removed record. Choosing retention therefore balances storage/maintenance cost against the expected repeat, reconciliation, and audit window. It is not merely cosmetic housekeeping.

The current inbox has an important asymmetry. Shared cleanup removes terminal consumer entries but does **not** remove `InboxAdmission` rows. Ordinary redelivery can remain deduplicated by its admission even after per-consumer records disappear. Replay, however, finds admissions and creates consumer entries missing for an event/consumer pair; after cleanup it can recreate previously completed consumer work. Entry retention is thus not admission retention, and “replay” is not equivalent to ordinary broker redelivery.

This follows the inspected store implementation, not an assertion that indefinite admission retention was intentionally selected or is suitable for every deployment. Storage growth and any application-driven admission deletion require separate consideration. Exact cleanup cutoffs, batch limits, and management contracts are in [Processing](../reference/processing.md#cleanup-and-retained-identities).

Retention also interacts with poison policy: acknowledged MQTT poison has no durable poison record to retain or requeue here. Inbox/outbox dead letters represent failures **after local durable work exists**, not a universal archive of everything rejected at the transport boundary.

## Observations must match their boundary

A “completed” counter is not a subscriber-effect counter. A retry/dead-letter log emitted before conditional settlement is not proof that the database update succeeded. A broker acknowledgement is not an all-consumers-completed signal. A lost-claim counter does not enumerate every lease expiry independently.

The [processing reference](../reference/processing.md#metrics-and-logs) distinguishes log timing from confirmed settlement and documents exact instruments. Reading these signals together with persisted state avoids treating normal boundary gaps as impossible states or assuming a single metric represents end-to-end success.

## A bounded reliability model

The source boundaries behind this model are the [MQTT inbox worker](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs), [inbox admission store](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs), [durable ownership store](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs), and [shared work loop](../../src/Clean.Messaging/Processing/WorkLoop.cs). The references linked above provide the remaining contract and implementation pointers.

The useful mental model is a chain of locally owned responsibilities:

```text
MQTT receipt → durable admission → per-consumer local transaction
owning state + outbox intention → durable target attempt → MQTT publication
                                                    → recorded completion
```

Identity limits some repeats; local transactions protect participating local state; leases allow abandoned work to be reclaimed; retries provide bounded recovery after classified failure; retention governs the evidence remaining for future duplicates and replay. None of those alone proves exactly-once external effects.

The architecture gives concrete local boundaries on which an application can build. The application still owns message meaning, recipient idempotency, repeat-sensitive effects, broker/session provisioning, storage lifecycle, and operational reconciliation. For the current component map and authoritative APIs, continue with [Architecture](../reference/architecture.md), [Contracts](../reference/contracts.md), [Configuration](../reference/configuration.md), [Transport](../reference/transport.md), and [Processing](../reference/processing.md). The [documentation index](../README.md) lists the complete Reference and Explanation set.
