# Scheduling reference

This page describes the deferred-delivery contracts, EF Core persistence, outbox bridge, and optional Quartz wake engine in this checkout. It is a lookup reference for integrators and operators, reviewed against source on **2026-10-13**. The review was not an execution, database-provider, or deployment validation.

For workflow concepts, see [durable workflows](../explanation/durable-workflows.md). For the surrounding package graph, see [architecture](architecture.md).

## Packages and namespaces

| Package | Responsibility |
| --- | --- |
| `Clean.Messaging.Scheduling` | Identities, scheduling/delivery ports, options, scheduling writer, due-work and cleanup workers, default wake engine. |
| `Clean.Messaging.Scheduling.EntityFrameworkCore` | Relational schedule store, transactional executor, model mapping, save/transaction tracking, worker registration. |
| `Clean.Messaging.Scheduling.Outbox` | `IMessageScheduler` implementation and `message` target delivery into `IOutbox`. |
| `Clean.Messaging.Scheduling.Quartz` | Replaces the wake engine with a Quartz-assisted engine; does not replace schedule persistence or execution. |

All four projects target `net10.0`. The EF integration references EF Core Relational `10.0.11`; the Quartz integration references Quartz `4.0.1`. Dependencies and runtime/provider distinctions are also recorded in [architecture](architecture.md).

## Application API

[`IMessageScheduler`](../../src/Clean.Messaging.Scheduling/IMessageScheduler.cs) is in `Clean.Messaging.Scheduling`:

```csharp
ScheduleId Schedule<TMessage>(
    ScheduleId id,
    TMessage message,
    DateTimeOffset dueAt,
    ScheduleGroupId? groupId = null,
    Guid? correlationId = null,
    Guid? causationId = null)
    where TMessage : notnull;

ValueTask Cancel(
    ScheduleId id,
    CancellationToken cancellationToken = default);

ValueTask CancelGroup(
    ScheduleGroupId groupId,
    CancellationToken cancellationToken = default);
```

The supplied implementation comes from `AddOutboxScheduling()`, not `AddScheduling()` alone. It always uses `ScheduleTarget.Message`. Its single cancellation is target-filtered but not group-filtered; group cancellation cancels all nonterminal `message` schedules in that group.

`Schedule` serializes and stages an EF-tracked schedule; it does **not** call `SaveChanges`, commit, wait until due, or publish. Cancellation changes tracked/persisted entities in the current scope but also needs the owning `SaveChanges`/transaction to persist. Missing schedules and already-terminal schedules are no-ops for cancellation. There is no reschedule or recurring-schedule operation.

### Identities and inputs

| Type/input | Contract |
| --- | --- |
| [`ScheduleId(Guid Value)`](../../src/Clean.Messaging.Scheduling/ScheduleId.cs) | Caller-owned logical schedule identity and `ScheduledMessages` primary key. `New()` generates a GUID; `ToString()` uses GUID format `N`. The writer rejects `Guid.Empty`. |
| [`ScheduleGroupId(Guid Value)`](../../src/Clean.Messaging.Scheduling/ScheduleGroupId.cs) | Optional grouping identity, not a message ID or routing target. `New()` and `ToString()` behave like `ScheduleId`; writer operations reject an empty group GUID when supplied. |
| [`ScheduleTarget(string value)`](../../src/Clean.Messaging.Scheduling/ScheduleTarget.cs) | Nonblank, at most 100 ASCII characters. `Value` and `ToString()` expose the exact string; delivery lookup uses ordinal case-sensitive comparison. `ScheduleTarget.Message` is `message`. |
| `message` | Non-null and serializable through the registered messaging contract. See [contracts](contracts.md). |
| `dueAt` | Stored as `dueAt.UtcDateTime`. The writer does not require a future instant; an already-due schedule becomes eligible after persistence. |
| `correlationId`, `causationId` | Stored and forwarded as provided. The schedule writer does not fill them from `IMessageContext`. The later outbox factory has its own context fallback semantics. |

Retries of the same logical scheduling operation must reuse the schedule ID; distinct operations require distinct IDs. **This is an identity obligation, not a duplicate-insert API.** The EF store uses `Add` and a primary key: it does not load an existing schedule, compare its payload, or convert a repeated insertion into success. Callers must coordinate retried writes with their transaction/idempotency strategy. A new ID on every retry permits multiple timers; an existing ID is not an instruction to update the due time. Schedule cleanup eventually removes the primary-key record, so identity protection is not retained indefinitely.

## Registration surface and dependencies

Registration extension methods are in each package's root namespace:

| Signature | Effect |
| --- | --- |
| `IServiceCollection AddScheduling(Action<SchedulingOptions>? configure = null)` | Adds core messaging services/options, default singleton `ISchedulingEngine`, scoped writer and delivery registry, and runtime delivery validation. Does not register a persistence provider, executor, scheduling worker, or `IMessageScheduler`. |
| `MessagingBuilder<TDbContext> AddScheduling<TDbContext>(Action<SchedulingOptions>? configure = null) where TDbContext : DbContext` | Adds the runtime, scoped EF schedule store/reader/executor and tracking interceptors, scheduling worker, cleanup worker, and EF startup validation. |
| `ModelBuilder AddScheduling()` | Maps `ScheduledMessages`; returns the builder. |
| `DbContextOptionsBuilder AddScheduling(IServiceProvider services)` | Attaches save and transaction interceptors, avoiding reattachment of the same interceptor instance. |
| `DbContextOptionsBuilder<TDbContext> AddScheduling<TDbContext>(IServiceProvider services) where TDbContext : DbContext` | Typed options-builder equivalent. |
| `IServiceCollection AddOutboxScheduling()` | Adds the runtime, scoped `IMessageScheduler`, `message` delivery, and validation that `IOutbox`, scheduler, and a single matching delivery resolve. Does not itself add EF scheduling or outbox persistence. |
| `IServiceCollection UseQuartzScheduling(Action<IQuartzBuilder>? configure = null)` | Adds runtime scheduling, configures/adds the default Quartz scheduler, adds its hosted service with `WaitForJobsToComplete = true`, and replaces all `ISchedulingEngine` registrations. |

Sources: [runtime DI](../../src/Clean.Messaging.Scheduling/DependencyInjection.cs), [EF DI](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/DependencyInjection.cs), [outbox bridge DI](../../src/Clean.Messaging.Scheduling.Outbox/DependencyInjection.cs), [Quartz DI](../../src/Clean.Messaging.Scheduling.Quartz/DependencyInjection.cs).

A functioning EF/outbox composition needs the registered scoped relational `TDbContext`, its scheduling model and interceptors, the outbox model/interceptors and `IOutbox`, and all message contracts/targets. Staging outbox rows and marking schedule completion are atomic **only when those services participate in the executor's same scoped EF context and transaction**. Resolving a delivery service is not proof of cross-context atomicity. See [persistence](persistence.md) and [outbox](outbox.md).

Startup validates relational EF use, schedule model presence, save/transaction interceptors, and store/executor resolution. Delivery registration rejects duplicate target strings. A delivery missing for a persisted target is detected when dispatched, not by scanning all database rows at startup. The configuration section constant is not automatic binding; the scheduling extensions take configuration delegates. See [configuration](configuration.md).

## Provider-facing ports and snapshots

These are public extension/read contracts; internal stores and executors are not supported public provider ports.

### Writer and reader

In `Clean.Messaging.Scheduling.Persistence`, [`IScheduleWriter`](../../src/Clean.Messaging.Scheduling/Persistence/IScheduleWriter.cs) exposes:

```csharp
ScheduleId Schedule<TMessage>(
    ScheduleId id, TMessage message, DateTimeOffset dueAt,
    ScheduleTarget target, ScheduleGroupId? groupId,
    Guid? correlationId, Guid? causationId)
    where TMessage : notnull;

ValueTask Cancel(
    ScheduleId id, ScheduleTarget target, ScheduleGroupId? groupId,
    CancellationToken cancellationToken);

ValueTask CancelGroup(
    ScheduleGroupId groupId, ScheduleTarget target, ScheduleId? exceptId,
    CancellationToken cancellationToken);
```

These parameters have no optional defaults. Target/group filters prevent cancellation from crossing capabilities; `exceptId` leaves one schedule untouched. The writer validates IDs, but does not revalidate a default-constructed `ScheduleTarget`.

[`IScheduleReader`](../../src/Clean.Messaging.Scheduling/Persistence/IScheduleReader.cs) has:

```csharp
ValueTask<IReadOnlyList<ScheduleStatus>> GetGroup(
    ScheduleGroupId groupId,
    ScheduleTarget? target,
    CancellationToken cancellationToken);
```

The EF reader returns persisted, no-tracking rows ordered by `DueAt`, then ID. A null target selects all targets. It includes terminal rows and does not merge unsaved local changes.

[`ScheduleStatus`](../../src/Clean.Messaging.Scheduling/Persistence/ScheduleStatus.cs) is a sealed positional record with fields `ScheduleId`, nullable `ScheduleGroupId`, `Target`, `MessageData`, `DueAt`, `Attempts`, and nullable `NextAttemptAt`, `DispatchedAt`, `CancelledAt`, `FailedAt`, `FailureCode`, `FailureMessage`. Timestamp fields use `DateTime`; payload is `MessageData(string Type, string Value)`. The snapshot excludes lease, claim, version, and exception type. There is no public scheduling manager, lookup-by-ID API, failure requeue API, or dead-letter API in these packages.

### Delivery

In `Clean.Messaging.Scheduling.Delivery`, [`IScheduleDelivery`](../../src/Clean.Messaging.Scheduling/Delivery/IScheduleDelivery.cs) has `ScheduleTarget Target { get; }` and:

```csharp
ValueTask Dispatch(
    ScheduledDispatch message,
    CancellationToken cancellationToken);
```

[`ScheduledDispatch`](../../src/Clean.Messaging.Scheduling/Delivery/ScheduledDispatch.cs) is a readonly positional record struct containing `ScheduleId`, nullable `ScheduleGroupId`, `MessageData`, nullable `CorrelationId`, nullable `CausationId`. It deliberately omits due time, attempt count, and claim ownership. Implementations must tolerate repeated dispatch; the port explicitly specifies at-least-once delivery semantics. Register one delivery per target.

The outbox delivery deserializes the payload and calls `IOutbox.Enqueue(new OutboxMessage<TMessage>(scheduleId.Value, payload, correlationId, causationId))`. The GUID becomes the outbox **EventId**, not its generated delivery `MessageId`. The outbox factory selects current targets at this later enqueue time; target fan-out is not captured in the schedule. An empty target collection can produce an empty batch with custom target providers; the default local provider instead throws `consumer.message_unhandled` when the message has no registered consumers. `DispatchedAt` means the scheduling delivery completed, not that any external recipient received the message. See [outbox](outbox.md) for per-target rows and publication semantics.

### Wake engine

In `Clean.Messaging.Scheduling.Runtime`, [`ISchedulingEngine`](../../src/Clean.Messaging.Scheduling/Runtime/ISchedulingEngine.cs) exposes `void Wake()` and `ValueTask Wait(DateTime wakeAt, CancellationToken cancellationToken)`. The default engine coalesces wakes in a bounded one-item channel and races that signal against a `TimeProvider` delay. Wake signals are local hints, not durable records.

## Options and defaults

[`SchedulingOptions`](../../src/Clean.Messaging.Scheduling/Configuration/SchedulingOptions.cs), in `Clean.Messaging.Scheduling.Configuration`, has `Section = "Messaging:Scheduling"` and mutable `Lease`, `Processing`, `Recovery`, `Retry`, `Cleanup` objects:

| Property | Default | Validation / use |
| --- | --- | --- |
| `Lease.Duration` | 1 minute | Must be positive; each claimed batch receives this lease. No scheduling claim-renewal loop is implemented. |
| `Processing.Concurrency` | 4 | Positive; parallel dispatch limit within each batch. |
| `Recovery.Interval` | 30 seconds | Positive; maximum normal wait before polling again, and delay after a worker-level failure. |
| `Recovery.BatchSize` | 32 | Positive; maximum rows claimed per iteration. |
| `Retry.MaxAttempts` | 5 | Positive; attempt count includes every claim, including recovered claims. |
| `Retry.Delay` | 2 seconds | Nonnegative initial delay. |
| `Retry.MaxDelay` | 1 minute | Must be at least `Retry.Delay`; caps exponential delay. |
| `Cleanup.Retention` | 30 days | Positive; age of terminal timestamp before deletion. |
| `Cleanup.Interval` | 1 hour | Positive; cleanup starts on periodic ticks, not immediately at startup. |
| `Cleanup.BatchSize` | 500 | Positive; maximum deletions per batch. |
| `Cleanup.MaxBatchesPerRun` | 20 | Positive; cap per cleanup tick, with a warning when reached. |

Nested public classes are `SchedulingLeaseOptions`, `SchedulingProcessingOptions`, `SchedulingRecoveryOptions`, `SchedulingRetryOptions`, and `SchedulingCleanupOptions`. Validation occurs through `ValidateOnStart`; workers retain the `IOptions<SchedulingOptions>.Value` object rather than using a reload-aware monitor. Null nested objects are not a supported configuration shape. Scheduling has its own retry options; shared inbox/outbox `RetryOptions` do not control this worker.

## Persistence and lifecycle

The internal [`ScheduleEntry`](../../src/Clean.Messaging.Scheduling/Persistence/ScheduleEntry.cs) maps to [`ScheduledMessages`](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/SchedulingConfiguration.cs):

- Primary key: schedule GUID. Optional group GUID and required target string (100 ASCII characters).
- Payload: required message-contract type (maximum 500 characters) and serialized value; correlation/causation GUIDs and `DueAt`/`CreatedAt`.
- Ownership: claim ID (maximum 32 ASCII characters), nullable `ClaimedUntil`, integer `Attempts`.
- Lifecycle: `NextAttemptAt`, `DispatchedAt`, `CancelledAt`, `FailedAt` and numeric `Version` concurrency token.
- Diagnostics: failure code (200 characters), failure message (500), exception type (500).
- Indexes: terminal/retry/lease/due columns for work discovery, and group/target/due columns for group queries. No separate schedule dead-letter table is written.

A schedule is terminal when dispatched, cancelled, or failed. Cancellation increments version and clears claim, lease, and retry time; it leaves prior diagnostics intact. Completion clears ownership/retry time and sets `DispatchedAt`; it also leaves prior failure diagnostics intact.

### Due discovery and claims

The [EF store](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleStore.cs) claims only nonterminal rows with `DueAt <= now`, no future retry time, and no unexpired lease. Selection is ordered by original `DueAt`, then ID. It updates a batch with a generated claim ID, sets `ClaimedUntil`, clears `NextAttemptAt`, and increments attempts. Claim acquisition uses an EF execution-strategy transaction with claim-existence verification for uncertain commits.

The [worker](../../src/Clean.Messaging.Scheduling/Runtime/SchedulingWorker.cs) creates separate scopes for claiming and each execution, processes up to the configured concurrency, then waits until the earlier of next available due/retry time and the recovery interval. Backlogs can cause an immediate next iteration. Multiple rows in one batch receive their lease before they reach a parallel execution slot; concurrency and lease duration therefore affect long batches. There is no renewal, guaranteed exact firing time, or global execution ordering.

### Transactional dispatch

The [executor](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleExecutor.cs) runs an EF execution strategy, with a fresh scope/transaction for each attempt:

1. Conditionally increment version for the same schedule/claim, an unexpired lease, and a nonterminal row. This database write gates entry into dispatch.
2. Load the tracked row and invoke its target delivery inside that transaction.
3. Mark complete, `SaveChangesAsync`, and commit.
4. If the gate fails, roll back and check whether the schedule is already terminal or absent. Such a row counts as settled; an active row owned elsewhere does not.

The gate and concurrency token participate in local database consistency; they cannot undo an external effect. The delivery can run again after a failed/uncertain transaction or provider execution-strategy retry. A custom delivery doing network I/O has a duplicate-publication window and holds the EF transaction open during that I/O. The implemented outbox delivery instead stages local outbox rows. A saga delivery updates saga state/timers in the same scheduling transaction when correctly composed; see [sagas](sagas.md).

### Save/commit observation

[Scheduling tracking](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/SchedulingTracker.cs) observes added, modified, or deleted schedules. It wakes after a standalone successful save or explicit transaction commit. Rollback clears pending notifications. Save failure/cancellation and transaction failure can also wake discovery, allowing it to inspect persisted state rather than treating a failed callback as proof of rollback. Recovery polling remains necessary if a notification is lost or another host writes schedules.

Changed schedules reject ambient `System.Transactions.Transaction.Current`; use an EF transaction. Nested observed saves are rejected. The tracker is not an independent database commit verifier or a replacement for caller transaction recovery.

## Failures, recovery, and cleanup

| Condition | Current behavior |
| --- | --- |
| `MessageException` with `IsTransient == false` | Permanent failure on that attempt. Missing target uses `scheduling.delivery.target_not_registered` with `FailureAction.Fault`. |
| Other exception, including transient `MessageException` | Retry unless claimed attempts have reached `Retry.MaxAttempts`. |
| Retry | Clear claim/lease; persist next attempt and diagnostics; wake discovery. Delay is `min(MaxDelay, Delay * 2^(attempt - 1))` with the exponent bounded below at zero; no jitter. |
| Permanent/exhausted failure | Set `FailedAt`, clear ownership/retry time, store diagnostics, increment version. |
| Shutdown cancellation | Propagates; no ordinary failure settlement for cancellation of the processing token. The lease can expire and work can be reclaimed. |
| Ownership no longer matches | Retry/fail affects no row, or executor returns unsettled; worker logs claim loss. No stale-owner completion is assumed. |
| Worker or settlement infrastructure failure | Outer loop logs an error and waits the recovery interval; persisted eligibility and lease expiry govern later recovery. |

Failure codes use a `MessageException.Code`, otherwise `scheduling.unhandled`; code/message/exception-type are truncated to mapped lengths. Retry/fail updates require matching claim ID and nonterminal status, **not** a still-unexpired lease or expected version. Dispatch admission, in contrast, checks lease expiry. These predicates are distinct; neither gives exactly-once external effects.

Cleanup deletes rows whose dispatched/cancelled/failed timestamp is strictly before the retention cutoff, ordered by `CreatedAt`, in capped batches. Active schedules are not age-expired by cleanup. Failed rows have no built-in requeue operation; group snapshots and application-defined operational tooling are the available inspection/recovery boundary. Cleanup removes schedule history and allows that primary-key identity to be inserted again. Saga timer history shares this cleanup policy, independently of saga-state retention.

## Quartz-specific behavior and limits

The [Quartz engine](../../src/Clean.Messaging.Scheduling.Quartz/SchedulingEngine.cs) schedules a replacement job and trigger, both named `scheduler-wake` in group `Clean.Messaging`. The [job](../../src/Clean.Messaging.Scheduling.Quartz/SchedulingWakeJob.cs) calls `ISchedulingEngine.Wake()`; it does not deserialize or deliver individual scheduled messages.

`UseQuartzScheduling` reuses an existing unkeyed default `IScheduler`; its configuration delegate is applied to the unnamed/default scheduler. Otherwise it calls `AddQuartz(configure)`. Every future wait still races a local wake against a local timed delay. Quartz scheduling failure logs a warning and local waiting remains active. No Quartz-specific scheduling options class, per-message Quartz jobs, recurrence API, persistent Quartz job store, cluster configuration, or misfire policy is supplied by this adapter. Quartz provider/deployment choices belong to host configuration and were not validated in this documentation review.

## Related references

- [Sagas](sagas.md): saga timer grouping, state transitions, and terminal timer cancellation.
- [Outbox](outbox.md): target fan-out, event/delivery IDs, local commit and external publication.
- [Processing](processing.md): other durable workers and their distinct leases/retry semantics.
- [Persistence](persistence.md): EF composition and schema ownership.
- [Configuration](configuration.md): binding and host configuration.
