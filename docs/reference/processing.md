# Shared durable processing reference

## Scope and verification

This page is the authoritative lookup for shared `MessageOptions` and `RetryOptions` defaults/validation, inbox/outbox processing, durable-entry ownership, retry settlement, recovery, cleanup, management, metrics, and logs. It describes the provider-neutral ports and the current EF Core implementation separately.

**Source reviewed: 2026-10-13; not integration-tested.** Scheduling and sagas have their own lifecycles/options; they do not automatically inherit the inbox/outbox state machine below. See [Scheduling](scheduling.md) and [Sagas](sagas.md). MQTT configuration/acknowledgement belongs to [Transport](transport.md); registration/binding/lifetime composition belongs to [Configuration](configuration.md).

## Contracts and component map

| Contract / component | Responsibility |
| --- | --- |
| [`IDurableEntry`](../../src/Clean.Messaging/Persistence/IDurableEntry.cs) | Identity, stored message/context, timestamps, attempts, lease fields, failure fields, and `EntryKey`. Implemented by inbox and outbox entries. |
| [`EntryKey`](../../src/Clean.Messaging.Abstractions/EntryKey.cs) | Value key `(Guid MessageId, string TargetId)`. |
| [`OwnedEntry<TEntry>`](../../src/Clean.Messaging/Persistence/IDurableStore.cs) | Record `(TEntry Entry, string ClaimId)` passed from claiming to processing. |
| [`IDurableStore<TEntry>`](../../src/Clean.Messaging/Persistence/IDurableStore.cs) | Claim, renew, complete, retry, dead-letter, and cleanup port. |
| [`IWorkProcessor<TEntry>`](../../src/Clean.Messaging/Processing/IWorkProcessor.cs) | `ValueTask Process(OwnedEntry<TEntry> owned, CancellationToken cancellationToken)`. |
| [`IWorkSignal`](../../src/Clean.Messaging/Processing/IWorkSignal.cs) | `ValueTask WaitAsync(CancellationToken cancellationToken)` and `void Wake()`. Wake-up hint, not durable work storage. |
| [`WorkLoop<TEntry>`](../../src/Clean.Messaging/Processing/WorkLoop.cs) | Scoped claiming and processing with bounded queued work/concurrent consumers. |
| [`ClaimRenewal`](../../src/Clean.Messaging/Processing/ClaimRenewal.cs) | Periodic renewal; cancels a claim-loss token on rejected renewal or renewal exception. |
| [`InboxProcessor`](../../src/Clean.Messaging.Inbox/Dispatching/InboxProcessor.cs) / [`OutboxProcessor`](../../src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs) | Attempt context, lease renewal, dispatch, failure classification and fenced settlement. |
| [`RetryPolicy`](../../src/Clean.Messaging/Retry/RetryPolicy.cs) / [`MessageFailureFactory`](../../src/Clean.Messaging/Failures/MessageFailure.cs) | Retry decision/delay and stored failure snapshot. |
| [`IDeadLetterStore<TEntry>`](../../src/Clean.Messaging/DeadLetters/IDeadLetterStore.cs) | Listing, requeue, and discard operations; implemented by EF `DeadLetterStore<TDbContext, TEntry>`. |

`IDurableEntry` has required semantic identity values `MessageId`, `EventId`, `TargetId`, stored `MessageData`, nullable correlation/causation IDs, and `CreatedAt`. Mutable lifecycle fields are `Attempts`, `ClaimId`, `ClaimedUntil`, `NextAttemptAt`, `ProcessedAt`, `DeadLetteredAt`, and `DiscardedAt`. Failure fields are nullable category/code/type/message/last-failed timestamp/last-failed attempt. There is no single inbox/outbox status enum: state is expressed by these fields.

The current EF [durable-entry mapping](../../src/Clean.Messaging.EntityFrameworkCore/DurableEntryConfiguration.cs) uses primary key `(MessageId, TargetId)` and unique `(EventId, TargetId)`. Target IDs are limited to 200 non-Unicode characters, claim IDs to 32 non-Unicode characters, contract names to 500 characters, failure codes to 200, and exception types/failure messages to 500. See [Persistence](persistence.md) for the wider model and transactions.

### Durable store operations

All operations return `ValueTask` and take an explicit `CancellationToken`:

| Operation | Parameters besides token | Result |
| --- | --- | --- |
| `Claim` | `DateTime now, int limit, TimeSpan lease` | `IReadOnlyList<OwnedEntry<TEntry>>`. EF validates positive limit and lease. |
| `Renew` | `EntryKey key, string claimId, DateTime now, DateTime claimedUntil` | `bool`: exactly one currently owned entry renewed. |
| `Complete` | `EntryKey key, string claimId, DateTime now` | `bool`: exactly one owned entry completed. |
| `Retry` | `EntryKey key, string claimId, MessageFailure failure, DateTime now, DateTime nextAttemptAt` | `bool`: exactly one owned entry scheduled for retry. |
| `DeadLetter` | `EntryKey key, string claimId, MessageFailure failure, DateTime now` | `bool`: owned terminal settlement, with execution-strategy verification in EF. |
| `Cleanup` | `DateTime before, int limit` | Number deleted; EF validates positive limit. |

The interface alone does not enforce a storage provider's atomicity or fencing. The following predicates/transitions are from [`DurableStore<TDbContext, TEntry>`](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs).

## Eligibility, state transitions, and fencing

A claim candidate must satisfy **all** of:

- Unresolved: `ProcessedAt`, `DeadLetteredAt`, and `DiscardedAt` are null.
- Due: `NextAttemptAt` is null or `<= now`.
- Available ownership: `ClaimId` is null, `ClaimedUntil` is null, or `ClaimedUntil <= now`.

Claim updates are transactional under the EF execution strategy. The candidate query orders by `CreatedAt`, then `MessageId`, then `TargetId`, and takes the limit. A new [`ClaimId.Create()`](../../src/Clean.Messaging/ClaimId.cs) value is a GUID in `N` format; the same token is used for all entries in that claim batch. Claiming sets `ClaimedUntil = now + lease`, clears `NextAttemptAt`, and increments each entry's `Attempts`. Entries are read back without tracking by claim ID and checked against the affected-row count. Execution-strategy verification checks for the claim ID in storage.

**An attempt is counted when claimed, not when the consumer or transport begins.** A crash, queue wait, or shutdown after claiming can consume an attempt without a handler invocation. There is no maximum-attempt filter in claim eligibility; `MaxAttempts` is applied when evaluating a processing failure, not as an absolute cap on claims after repeated lease loss.

Renewal and settlement require key match, all terminal timestamps null, `NextAttemptAt == null`, matching claim ID, and `ClaimedUntil > now`. At exact expiry, an old owner cannot renew/settle and the entry is available for reclaim. Fencing is a conditional database update; rejected updates normally return false rather than overwriting another attempt's state.

| Transition | Persisted effect |
| --- | --- |
| Available/due → claimed | New token and lease, next-attempt cleared, attempts incremented. |
| Claimed → renewed | Extend `ClaimedUntil`; no attempt increment. |
| Claimed → completed | Set `ProcessedAt = now`; clear claim/lease and next-attempt time. Previous failure details and attempt count remain. |
| Claimed → retry waiting | Store failure; set `NextAttemptAt`; clear claim/lease; remain unresolved. |
| Claimed → dead-lettered | Store failure; set `DeadLetteredAt = now`; clear claim/lease and next-attempt time. |
| Claimed → expired | No immediate field mutation required; eligibility permits another claim when the clock reaches expiry. |
| Dead-lettered → requeued | Clear dead-letter/discard/next-attempt/claim/lease; reset attempts to zero. Previous failure snapshot remains. |
| Dead-lettered → discarded | Set `DiscardedAt`; clear next-attempt/claim/lease. Dead-letter timestamp and failure snapshot remain. |
| Retained terminal entry → deleted | Cleanup removes the entry once a terminal timestamp is strictly older than the cutoff. |

Renew, retry, and dead-letter settlement use a fresh async scope/context in EF. `Complete` uses the store's configured context, permitting participation in the local dispatch transaction. Dead-letter settlement uses an execution strategy with transactional operation and verification by retained dead-letter timestamp. These mechanisms are not a tested provider-support matrix or proof of cross-system atomicity.

## Work loop, recovery, and scopes

`WorkLoop.Run(IWorkSignal signal, WorkLoopOptions options, CancellationToken token)` validates options and creates:

- One producer and `Concurrency` consumers.
- A bounded channel of `OwnedEntry<TEntry>` with `Capacity`, wait-on-full behaviour, one writer, and asynchronous continuations.
- A semaphore with `Capacity + Concurrency` slots to limit claimed/queued/in-flight work.

The loop sends an initial wake. On each wake, the producer reserves at least one available slot, then up to `BatchSize`, creates a fresh async scope, resolves `IDurableStore<TEntry>`, and claims using `TimeProvider.GetUtcNow().UtcDateTime`. Unused reservations are released. A full `BatchSize` result schedules another wake. Each consumer creates a fresh async scope, resolves the processor, and releases its slot and wakes the producer after processing, including failures.

Claims begin their lease **before** channel queueing. Renewal starts only when the processor starts, not while an entry waits in the channel. The channel therefore bounds memory/work but does not guarantee every queued claim is still valid at dispatch time. There is no per-target FIFO guarantee: claim ordering is deterministic, but concurrent handlers, retries, and reclaimed work can finish in another order.

[`AsyncAutoResetEvent`](../../src/Clean.Messaging/Processing/AsyncAutoResetEvent.cs), used by inbox/outbox signals, is a one-slot channel with `DropWrite`; multiple wake calls coalesce. Persistence remains the source of work. Admission/commit/requeue wake signals reduce latency; [inbox](../../src/Clean.Messaging.Inbox/Runtime/InboxRecoveryWorker.cs) and [outbox](../../src/Clean.Messaging.Outbox/Runtime/OutboxRecoveryWorker.cs) recovery workers periodically wake at `Recovery.Interval`, allowing recovery when a process-local signal is missed.

Claim errors are logged and delayed by `min(FailureMaxDelay, RecoveryInterval × 2^min(failures - 1, 30))`; this delay is not jittered. Success resets the producer failure counter. Processing exceptions escaping a processor are logged by the work loop and leave durable recovery to retry/reclaim. This infrastructure recovery is distinct from per-message `RetryPolicy` settlement.

`WorkLoopOptions` is a public record with positional parameters `Capacity`, `Concurrency`, `BatchSize`, `Lease`, `RecoveryInterval`, `FailureMaxDelay`. `Validate()` requires every number/time to be positive; unlike `MessageOptionsValidator`, it does not compare failure maximum with recovery interval or renewal interval with duration.

Inbox/outbox workers map their capability options to this record. EF capability registration installs the work loop, scoped processor/store, and hosted processing/recovery/cleanup workers. Core `AddMessaging` alone installs no durable inbox/outbox worker/store. See [Configuration](configuration.md) for complete registration and lifetime requirements.

## Attempt lifecycle and cancellation

Both processors push message/context IDs and target ID into [`MessageContext`](../../src/Clean.Messaging/MessageContext.cs). Its `AsyncLocal` scope restores the previous state on disposal. For outbox external targets, the ambient `ConsumerId` slot contains the target ID even though no local consumer is invoked. This is ambient metadata, not automatic distributed tracing.

Per attempt, the processor creates:

1. A claim-loss cancellation source.
2. A handling token linked to host cancellation and claim loss.
3. A renewal token linked to host cancellation, cancelled when the attempt ends.

`ClaimRenewal.Run<TEntry>` uses a `PeriodicTimer(RenewalInterval, TimeProvider)`. Each tick calls `store.Renew(key, token, now, now + Duration, cancellationToken)`. False renewal cancels claim loss; an unexpected renewal exception logs Warning and cancels claim loss. Intentional renewal cancellation is suppressed. A rejected renewal itself has no dedicated log; the processor subsequently records claim loss if the attempt does not complete successfully.

The processor awaits the dispatcher and interprets:

| Outcome | Action |
| --- | --- |
| Dispatcher returns true | Completion metric/log; dispatcher has already performed completion. It can also return true for an already-completed local entry. |
| Dispatcher returns false | Claim-loss metric/log; no failure retry is scheduled. |
| `OperationCanceledException` due to claim loss, but not host cancellation | No ordinary failure settlement; record claim loss. |
| `OperationCanceledException` with host token cancelled | Propagate cancellation; leave claimed work for expiry/recovery. |
| Other exception, claim still held | Create failure, evaluate retry, and conditionally settle retry/dead-letter. |
| Other exception after claim-loss signal | Record claim loss instead of retry/dead-letter settlement. |

Cancellation is cooperative. No thread is forcibly stopped, and a handler/transport ignoring cancellation can continue effects after another attempt gains ownership. The processor cancels and awaits renewal in `finally`.

### Settlement token and transaction boundary

Failure settlement uses its own `SettlementTimeout` source linked to **claim loss**, not the host stopping token. This permits bounded recording of a processing failure even if shutdown begins after the failure. Timeout/settlement exceptions can escape to the work loop; they are not converted to successful settlement. Retry/dead-letter metrics increment only when settlement returns true. False settlement records claim loss.

[`InboxDispatcher<TDbContext>`](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxDispatcher.cs) and [`LocalOutboxDelivery<TDbContext>`](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/LocalOutboxDelivery.cs) use an EF execution strategy and fresh scope on each strategy invocation. They first check whether the entry is already completed. Otherwise they begin a transaction, invoke the consumer, perform fenced completion, save the configured context, and commit. False completion rolls back; exceptions trigger rollback with `CancellationToken.None` while preserving the original failure if rollback also fails.

Only changes participating in that configured scoped context/transaction commit with completion. External calls and writes through another context are not rolled back by it. Consumer code can be invoked before discovering stale ownership at completion, and execution-strategy retries can invoke it again. External outbox delivery instead performs publication and then completion separately using the handling token; `SettlementTimeout` is a failure-settlement timeout, not a publish/completion or total-handler time limit. See [Delivery semantics](../explanation/delivery-semantics.md).

## `MessageOptions` defaults and validation

Sources: [`MessageOptions`](../../src/Clean.Messaging/MessageOptions.cs), [`MessageOptionsValidator<TOptions>`](../../src/Clean.Messaging/Processing/MessageOptionsValidator.cs), and [persistence options registration](../../src/Clean.Messaging.EntityFrameworkCore/PersistenceServices.cs).

`InboxOptions : MessageOptions` uses section `Inbox`; `OutboxOptions : MessageOptions` uses section `Outbox`. Both inherit these defaults without overrides. Each nested options object is initialized with `new()`. The validator expects nonnull nested objects; it does not convert null groups into defaults or produce a separate null-group validation result.

| Group/property | Type | Default | Validation |
| --- | --- | --- | --- |
| `Lease.Duration` | `TimeSpan` | 30 seconds | Greater than zero. |
| `Lease.RenewalInterval` | `TimeSpan` | 10 seconds | Greater than zero and strictly less than duration. |
| `Processing.Concurrency` | `int` | 4 | Greater than zero. |
| `Processing.Capacity` | `int` | 1024 | Greater than zero. |
| `Processing.BatchSize` | `int` | 100 | Greater than zero. |
| `Processing.SettlementTimeout` | `TimeSpan` | 5 seconds | Greater than zero. |
| `Recovery.Interval` | `TimeSpan` | 10 seconds | Greater than zero. |
| `Recovery.FailureMaxDelay` | `TimeSpan` | 1 minute | At least `Recovery.Interval`. |
| `Cleanup.Interval` | `TimeSpan` | 1 hour | Greater than zero. |
| `Cleanup.Retention` | `TimeSpan` | 7 days | Greater than zero. |
| `Cleanup.BatchSize` | `int` | 500 | Greater than zero. |

The validator aggregates validation messages; EF capability registration enables `ValidateOnStart`. There is no shared upper-bound validation, batch-size-versus-capacity relationship, automatic lease sizing against queue/handler time, maximum processing duration, or disable-cleanup flag. A positive retention does not bound pending work age.

## `RetryOptions` defaults and validation

Sources: [`RetryOptions`](../../src/Clean.Messaging/Retry/RetryOptions.cs), [`RetryOptionsValidator`](../../src/Clean.Messaging/Retry/RetryOptionsValidator.cs), and [`RetryPolicy`](../../src/Clean.Messaging/Retry/RetryPolicy.cs). These options are shared by inbox and outbox; section constant is `Messaging:Retry`.

| Property | Default | Validation |
| --- | --- | --- |
| `MaxAttempts` | `5` | Greater than zero. |
| `InitialDelay` | 2 seconds | Nonnegative; zero is permitted. |
| `MaxDelay` | 1 minute | At least `InitialDelay`. |
| `Backoff` | `RetryBackoff.Exponential` | Validator does not check enum membership; invalid values throw during retry delay evaluation. Defined values are `Fixed`, `Linear`, `Exponential`. |
| `UseJitter` | `true` | Boolean; no extra validation. |

`AddMessaging` enables startup validation. `RetryPolicy.Evaluate(Exception exception, int attempt)` returns `RetryResult(bool ShouldRetry, TimeSpan Delay)`, using `RetryResult.Fault` or `RetryResult.After(delay)`:

- A `MessageException` with `IsTransient == false` faults immediately, independent of remaining attempts.
- Otherwise retry is allowed only when `attempt < MaxAttempts`, including unknown exceptions and transient message exceptions.
- Unjittered delay is `min(MaxDelay, InitialDelay × multiplier)`, where multiplier is 1 for fixed, `attempt` for linear, and `2^max(0, attempt - 1)` for exponential.
- Jitter replaces the delay with `Random.Shared.NextDouble() × delay`, ranging from zero inclusive to the computed delay exclusive (subject to time-value rounding). It is full jitter, not an added positive margin.

With defaults and jitter disabled, failures on attempts 1–4 schedule delays of 2, 4, 8, and 16 seconds; failure on attempt 5 dead-letters. With jitter enabled these are upper bounds, not exact waits. A due timestamp does not promise immediate execution: polling, slots, database availability, and competing workers affect when work is claimed.

This policy does not control MQTT ingress reconnects, EF execution-strategy attempts, work-claim infrastructure recovery, scheduling retries, or saga concurrency policy. `RetryPolicy` does not implement an outer catch-and-repeat execution loop; processors persist its decision for a later claim.

### Stored failure classification

[`MessageException`](../../src/Clean.Messaging/Exceptions/MessageException.cs) carries `Code`, `FailureAction` (`Retry` or `Fault`), and `IsTransient`, which is true only for `Retry`.

`MessageFailureFactory.Create(exception, attempt, DateTimeOffset now)` creates `MessageFailure(Category, Code, ExceptionType, Message, FailedAt, Attempt)`:

| Exception | Stored category/code/message |
| --- | --- |
| `MessageException` with `Retry` | `Transient`; exception code/message. |
| `MessageException` with `Fault` | `Permanent`; exception code/message. |
| Other exception | `Unknown`; code `processing.message.unhandled`; fixed message `An unexpected message processing failure occurred.` |

The factory truncates code to 200 characters, exception CLR full name to 500, and stored message to 500. `FailedAt` is UTC. Logs retain the exception separately. Exhausting attempts does not change a transient/unknown failure into the `Permanent` category; terminal dead-letter state and failure category are separate concepts. Transport-specific mapping/publish codes are in [Transport](transport.md).

## Cleanup and retained identities

[Inbox cleanup](../../src/Clean.Messaging.Inbox/Runtime/InboxCleanupWorker.cs) and [outbox cleanup](../../src/Clean.Messaging.Outbox/Runtime/OutboxCleanupWorker.cs) run once immediately, then once each interval via `PeriodicTimer` and `TimeProvider`. Each pass computes `before = UTC now - Retention` and invokes one store cleanup batch; it does not drain all eligible rows in a loop. Failure logs Warning and waits for a later pass; shutdown cancellation propagates to the worker cancellation catch.

The EF shared store deletes at most `BatchSize` rows ordered by `CreatedAt`, where **any** of `ProcessedAt`, `DeadLetteredAt`, or `DiscardedAt` is nonnull and strictly `< before`. Retention is tested against terminal timestamps, not creation time. Pending/retrying/claimed unresolved rows are not age-deleted. Discard does not restart retention from scratch: the older dead-letter timestamp can already satisfy cleanup eligibility.

**Current inbox limitation:** `InboxStore<TDbContext>` inherits this entry cleanup and does not override it to delete `InboxAdmission` records. Thus the shared retention setting removes terminal inbox **entries**, not retained admissions. Admissions remain available to duplicate-admission checks and replay in this implementation; no admission cleanup/retention enforcement was found. Deleting entries removes their per-consumer `(EventId, TargetId)` protection, so replay can recreate work for that consumer from an admission even though ordinary redelivery remains deduplicated by the admission. Do not assume admission retention is seven days. See [Inbox](inbox.md) and [Persistence](persistence.md).

Outbox duplicate protection provided by retained unique event/target rows is lost when those rows are deleted. Cleanup frees storage but is also a change to the available duplicate/replay evidence. This page does not prescribe deployment-specific retention values.

## Shared dead-letter management

Application-facing interfaces: [`IInboxManager`](../../src/Clean.Messaging.Inbox/Runtime/IInboxManager.cs) and [`IOutboxManager`](../../src/Clean.Messaging.Outbox/Runtime/IOutboxManager.cs).

| Operation on both managers | Return / behaviour |
| --- | --- |
| `GetDeadLetters(int limit = 100, CancellationToken cancellationToken = default)` | `ValueTask<IReadOnlyList<DeadLetter>>`; requires positive limit. |
| `Requeue(IReadOnlyCollection<EntryKey> entries, CancellationToken cancellationToken = default)` | `ValueTask<RequeueResult>`; collection required, empty allowed. Returns `(Requested, Requeued)` and wakes processing if anything requeued. |
| `Discard(IReadOnlyCollection<EntryKey> entries, CancellationToken cancellationToken = default)` | `ValueTask<int>` number successfully discarded; collection required, empty allowed. |

[`DeadLetter`](../../src/Clean.Messaging.Abstractions/DeadLetter.cs) is a snapshot `(EntryKey Key, Guid EventId, DateTime DeadLetteredAt, int Attempts, string? Code, string? Error)`. It does not contain payload, correlation, or the entire failure snapshot.

The EF [`DeadLetterStore`](../../src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs) lists dead-lettered, nondiscarded entries ordered by dead-letter timestamp. Requeue/discard match only that state and key. They require no ambient `Transaction.Current`; otherwise `InvalidOperationException` is thrown. Each is an execution-strategy transaction with success verification. Managers iterate keys sequentially; the requested collection is not one atomic batch, and earlier changes can persist if a later key fails. Duplicate keys count toward `Requested` but only the first still-eligible transition normally changes state.

Requeue preserves identities/payload and failure history, resets attempts, and makes work immediately eligible. It does not force successful entries to run again. Discard makes dead letters absent from listing but keeps their rows until cleanup. The management methods themselves do not increment processor completion/retry/dead-letter counters.

Inbox additionally exposes `Replay<TMessage>(string consumerId, DateTime? from = null, DateTime? to = null, int limit = 1000, CancellationToken cancellationToken = default)` returning `ReplayResult(Matched, Replayed)`. It requires a nonblank consumer, positive limit, and `from <= to` if both supplied. Replay filters retained admissions by the registered message contract, uses inclusive timestamp bounds, and creates missing consumer/event entries; it is not a reset of existing successful/dead-letter entries. Details and registration checks belong to [Inbox](inbox.md).

## Metrics and logs

Source: [`MessageMetrics`](../../src/Clean.Messaging/Processing/MessageMetrics.cs). Meter name is **`Clean.Messaging`**, with no explicit meter version or units/descriptions. Each instrument is `Counter<long>` and every increment includes the single tag `source`:

| Instrument | Inbox/outbox call condition |
| --- | --- |
| `messaging.completed` | Dispatcher reports completion; `source = inbox` or `outbox`. Can include the local already-completed short-circuit, so not necessarily one new database transition per increment. |
| `messaging.retries` | Retry settlement returns true. |
| `messaging.dead_letters` | Dead-letter settlement returns true. |
| `messaging.lease_losses` | Processor observes claim loss, false dispatch, or false settlement. Not an independent database expiry count. |

The public metric methods accept any source string; the inspected inbox/outbox processors use exactly `inbox` and `outbox`. Scheduling/saga runtime sources do not call these metrics. No shared queue-depth, processing-duration, admission, MQTT-acknowledgement, cleanup-count, or management-requeue instruments are implemented here. Exporters/listeners are application-owned.

Operational logs use source-generated `ILogger<T>` methods:

| Category | Level | Signal and fields |
| --- | --- | --- |
| `InboxProcessor` / `OutboxProcessor` | Information | `{MessageId}/{TargetId}` completed on `{Attempt}`. |
| Same | Warning | Failure on `{Attempt}`; retry in `{Delay}`; original exception included. |
| Same | Error | Entry dead-lettered on `{Attempt}`; original exception included. |
| Same | Warning | Entry lost its claim; message/target IDs. |
| `ClaimRenewal` | Warning | Renewal exception; message/target IDs and exception. |
| `WorkLoop<TEntry>` | Error | Claim failure, or escaped processing failure with message/target IDs; durable recovery will retry. |
| `InboxCleanupWorker` / `OutboxCleanupWorker` | Warning | Cleanup failure; exception; explicit event ID 1 in each category. |

Processor retry/dead-letter logs are emitted **before** the conditional settlement call; a log saying "dead-lettered" does not prove the update succeeded. Metrics for those operations follow a true result. Completion logging follows the dispatcher result. Shared logs do not automatically include correlation/causation or payload; context propagation is not the same as logger scope enrichment. MQTT log IDs/templates are documented in [Transport](transport.md#mqtt-logs-and-telemetry).

## Related references and explanation

- [Architecture](architecture.md), [Contracts](contracts.md), [Configuration](configuration.md).
- [Inbox](inbox.md), [Outbox](outbox.md), [Persistence](persistence.md).
- [Transport](transport.md), [Scheduling](scheduling.md), [Sagas](sagas.md), [Domain events](domain-events.md).
- [Delivery semantics](../explanation/delivery-semantics.md): repeats, ownership, transaction/effect boundaries, and retention trade-offs.

The exact defaults and validation above are owned here rather than duplicated in configuration overview pages. Behavioural requirements, source observations and open decisions are separated in the [specification index](../specifications/INDEX.md).
