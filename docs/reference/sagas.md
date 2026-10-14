# Sagas reference

This page describes saga definitions, handlers, state transitions, effects, timers, EF Core persistence, and management in this checkout. It is a lookup reference for integrators and operators, reviewed against source on **2026-10-13**. This was source inspection, not execution or database-provider validation.

For concepts and alternatives, see [durable workflows](../explanation/durable-workflows.md). Exact scheduling behavior lives in [scheduling](scheduling.md); outgoing delivery semantics live in [outbox](outbox.md).

## Packages and capability boundary

`Clean.Messaging.Sagas` contains provider-neutral contracts and orchestration, with dependencies on core messaging, abstractions, outbox, and scheduling. `Clean.Messaging.Sagas.EntityFrameworkCore` adds the relational store, mapping, conflict translation, and cleanup registration; it also references scheduling EF persistence. Both target `net10.0`; the EF integration references EF Core Relational `10.0.11`.

Sagas persist local workflow state. They do not supply an application domain model, automatic compensation, external distributed transactions, a workflow graph DSL, or automatic execution independent of incoming messages/timers. Consumer applications own state classes, correlation, permitted business transitions, timer meaning, message payloads, and effect identities.

## Registration API

### Runtime registration

In `Clean.Messaging.Sagas`, [registration](../../src/Clean.Messaging.Sagas/DependencyInjection.cs) exposes:

```csharp
IServiceCollection AddSagas(
    this IServiceCollection services,
    Action<SagaBuilder> configure,
    Action<SagaOptions>? configureOptions = null);
```

It adds core messaging and scheduling, saga options/startup validation, singleton state serializer/definition registry, scoped processing/effect/management services, and an `IScheduleDelivery` for target `saga`. At least one saga must be configured. It does **not** add `IOutbox`, a saga store, or scheduling persistence by itself. Runtime startup resolution requires `IOutbox`, `IScheduleWriter`, `ISagaStore`, the saga registry, and exactly one saga delivery.

### Definition builder

[`SagaBuilder`](../../src/Clean.Messaging.Sagas/SagaBuilder.cs) has no public constructor; registration provides it:

```csharp
SagaBuilder Add<TState>(
    string type,
    Action<SagaBuilder<TState>> configure)
    where TState : class;
```

`type` is a stable persisted identifier: nonblank, at most 200 ASCII characters. Each state CLR type and each ordinal type string may be registered once. A definition must handle at least one message. All configured message types, including timer-only types, must have registered messaging contracts; registry construction checks them. See [contracts](contracts.md).

`SagaBuilder<TState>` supports three message mappings, each returning the same builder:

```csharp
SagaBuilder<TState> On<TMessage, THandler>(
    string consumerId,
    Func<TMessage, SagaKey> correlate,
    Func<TMessage, TState> create);

SagaBuilder<TState> On<TMessage, THandler>(
    string consumerId,
    Func<TMessage, SagaKey> correlate);

SagaBuilder<TState> On<TMessage, THandler>();
```

For all overloads, `TMessage : notnull` and `THandler : class, ISagaHandler<TState, TMessage>`.

| Overload | Meaning |
| --- | --- |
| With correlation and creation | Start message: correlates by saga key, creates initial state if absent, then invokes the handler. Registers a durable consumer and scoped handler. |
| With correlation only | Continuation message: correlates to existing state. Registers a durable consumer and scoped handler. |
| Without arguments | Instance-directed message, normally a timer payload. Registers its handler but no ordinary consumer or correlation function. |

A definition cannot map the same message CLR type twice. Consumer IDs use core consumer rules: nonblank, at most 200 ASCII characters; global duplicate consumer registration rules also apply. Start and continuation mappings both become ordinary messaging consumers (`SagaConsumer<TState,TMessage>`), not new ingress endpoints. They need an actual durable delivery path such as inbox dispatch or local outbox delivery. The no-argument mapping cannot locate an instance for ordinary correlated consumer processing; timer dispatch supplies its instance directly.

### EF registration, model, and interceptors

In `Clean.Messaging.Sagas.EntityFrameworkCore`, [extensions](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/DependencyInjection.cs) expose:

| Signature | Effect |
| --- | --- |
| `MessagingBuilder<TDbContext> AddSagas<TDbContext>(Action<SagaBuilder> configure, Action<SagaOptions>? configureOptions = null) where TDbContext : DbContext` | Adds scheduling EF runtime, saga runtime, scoped saga store/conflict resolver/save interceptor, saga cleanup worker and EF validation. Does not add outbox persistence. |
| `ModelBuilder AddSagas()` | Calls scheduling model registration, then maps `Sagas`. |
| `DbContextOptionsBuilder AddSagas(IServiceProvider services)` | Attaches scheduling save/transaction interceptors and saga save interceptor. |
| `DbContextOptionsBuilder<TDbContext> AddSagas<TDbContext>(IServiceProvider services) where TDbContext : DbContext` | Typed options equivalent. |

Startup requires a relational EF provider, saga model, and saga save interceptor; scheduling performs its own model/interceptor checks. Outbox persistence must be composed separately. The atomicity described below requires saga, outbox, scheduling, and business writes to use the **same scoped context and owning transaction**. Registering services that happen to resolve does not establish cross-context atomicity. See [persistence](persistence.md), [inbox](inbox.md), and [outbox](outbox.md).

## Identities and correlation

| Identity | Current meaning |
| --- | --- |
| [`SagaId(Guid Value)`](../../src/Clean.Messaging.Sagas/SagaId.cs) | Generated internal instance ID. `New()` creates a GUID; `ToString()` uses GUID format `D`. Also used as the timer group GUID. |
| [`SagaKey(string value)`](../../src/Clean.Messaging.Sagas/SagaKey.cs) | Caller-defined correlation key: nonblank, maximum 500 characters; `From(string)` constructs it. `Value`/`ToString()` return the exact value. |
| Saga type + key | Database-unique logical workflow identity, including terminal rows/tombstones. Database collation/provider determines persisted equality; no provider-independent case-normalization rule is imposed. |
| `StartedByMessageId` | Durable incoming delivery identity recorded at creation; detects redelivery of that same start message. It is not the saga ID or outbox EventId. |
| Consumer ID | Durable consumer identity registered by correlated mappings, independent of saga type/key. Changing it changes target identity. |
| Outgoing effect GUID | Caller-owned logical outbox event identity; the factory creates a separate outgoing delivery `MessageId`. |
| Timer GUID | Caller-owned `ScheduleId`, globally primary-keyed in `ScheduledMessages`, not merely unique within one saga. |
| Timer group | `ScheduleGroupId(saga.Id.Value)` with target `saga`; ensures timer cancellation stays within its owning saga and capability. |

Key/type changes and contract-name changes require consideration of existing persisted workflows. New timers/effects need distinct nonempty identities; a retried logical effect needs a stable identity. `SagaId` itself is generated during creation, so an unsuccessful creation attempt can have a different proposed instance ID on retry; the unique type/key and start-message identity determine which persisted start wins.

[`SagaProcessor`](../../src/Clean.Messaging.Sagas/Runtime/SagaProcessor.cs) requires `IMessageContext.MessageId` for ordinary consumer processing. Correlation/causation metadata are carried into effects; the key is selected exclusively by the mapping's `correlate` function, not by `CorrelationId`.

## Handler and result contracts

In `Clean.Messaging.Sagas`, [`ISagaHandler<in TState, in TMessage>`](../../src/Clean.Messaging.Sagas/ISagaHandler.cs), with `TState : class`, `TMessage : notnull`, exposes:

```csharp
ValueTask<SagaResult> Handle(
    TState state,
    TMessage message,
    CancellationToken cancellationToken);
```

The handler receives a freshly deserialized state object, may mutate it, and must return a non-null result. The runtime serializes that object after a successful return. Returning a result does not save or commit independently; thrown exceptions skip result/state application and belong to the owning durable delivery failure policy. Handler dependencies are scoped. No public saga context argument, typed state getter, or direct instance mutation API is provided.

[`SagaResult`](../../src/Clean.Messaging.Sagas/SagaResult.cs) is an abstract record with a restricted constructor: the supplied Continue/Complete/Fail factories form the current result hierarchy. It supports implicit conversion to `ValueTask<SagaResult>`:

| Signature | Behavior |
| --- | --- |
| `static SagaResult Continue()` | Persist changed state and leave the saga active. |
| `static SagaResult Complete()` | Persist changed state and mark completed. |
| `static SagaResult Fail(string code, string message)` | Persist changed state and mark failed with the supplied diagnostics. This is a business workflow result, not an exception/dead-letter command. |
| `SagaResult Send<TMessage>(Guid id, TMessage message)` | Append an outgoing message effect. |
| `SagaResult Send<TMessage>(SagaMessage<TMessage> message)` | Append an explicit message effect including optional metadata. |
| `SagaResult Schedule<TMessage>(Guid timerId, DateTimeOffset dueAt, TMessage message)` | Append an instance-directed timer effect. |
| `SagaResult Schedule<TMessage>(SagaSchedule<TMessage> message)` | Append an explicit timer effect. |
| `SagaResult Cancel(Guid timerId)` | Append timer cancellation; rejects an empty GUID. |

Generic effect methods require `TMessage : notnull`. Results are immutable: each appended effect returns a new result preserving the same terminal/continue outcome. Retain the returned value. Effects preserve append order and run after the state/outcome has been applied. Completed and failed results can also contain send effects; they are not silently discarded.

Minimal result expression, assuming `state.TimeoutId` is a previously allocated stable nonempty GUID and `DeadlineReached` is a registered contract/mapping:

```csharp
return SagaResult.Continue()
    .Schedule(state.TimeoutId, deadline, new DeadlineReached());
```

This illustrates result composition only; it is not an executed registration or end-to-end example.

### Public effect types

In `Clean.Messaging.Sagas.Effects`, [`SagaEffect` and concrete records](../../src/Clean.Messaging.Sagas/Effects/SagaEffect.cs) are:

```csharp
SagaMessage<TMessage>(
    Guid id, TMessage message,
    Guid? correlationId = null,
    Guid? causationId = null);

SagaSchedule<TMessage>(
    Guid id, DateTimeOffset dueAt, TMessage message);

SagaCancel(Guid TimerId);
```

`SagaMessage<TMessage>` exposes read-only `Id`, `Message`, `CorrelationId`, `CausationId`. `SagaSchedule<TMessage>` exposes read-only `Id`, `DueAt`, `Message`. Their constructors reject empty IDs and null payloads. `SagaCancel` is public, but there is no general public `Add(SagaEffect)` result API; normal result cancellation is through `Cancel(Guid)`. The abstract effect's apply contract is internal, not a general external effect plug-in port.

[`SagaEffectWriter`](../../src/Clean.Messaging.Sagas/Effects/SagaEffectWriter.cs) implements these semantics:

- **Send:** stage `OutboxMessage<TMessage>` with effect ID as EventId; explicit correlation overrides trigger correlation, explicit causation overrides trigger **MessageId**. Trigger `CausationId` is not used as the outgoing default cause. The outbox factory retains its own fallback rules.
- **Schedule:** stage the timer ID, payload, due time, target `saga`, owning saga group, trigger correlation, and trigger MessageId as causation. There is no correlation override in `SagaSchedule`.
- **Cancel:** filter by timer ID, saga group, and target `saga`. Cancelling the currently executing timer is a no-op; its executor will complete it.
- **Terminal result:** after all explicit effects, cancel remaining nonterminal timers in the saga group, excluding the current timer. A newly scheduled timer on a terminal result is therefore also cancelled unless it is the excluded identity; terminal completion is not a request to keep timers alive.

The schedule writer serializes payloads through the message contract registry. It stages additions, not timer replacement/upsert; repeated inserts still face the schedule primary key. Effect IDs alone do not make arbitrary handler side effects idempotent.

## Runtime lifecycle

### Start and continuation routing

The [message definition](../../src/Clean.Messaging.Sagas/Definition/SagaMessageDefinition.cs) first correlates and looks up the type/key:

| Existing instance | Start mapping (`create` supplied) | Continuation mapping |
| --- | --- | --- |
| Absent | Create state/instance, record incoming MessageId, invoke handler. | Throw transient `SagaNotFoundException`. |
| Active, same start MessageId | Return without reinvoking the start handler. | Invoke handler; general continuation deduplication is the durable consumer's responsibility. |
| Terminal, same start MessageId | Return without reinvoking the start handler. | Return without invoking handler. |
| Active, different start MessageId | Throw permanent `SagaConflictException`. | Invoke handler. |
| Terminal, different start MessageId | Throw permanent `SagaClosedException`. | Return without invoking handler. |

The same-start no-op is conditional on the persisted instance; it does not acknowledge an uncommitted start. Terminal continuation messages become successfully ignored by saga routing; they do not reopen the workflow. Start mappings are not alternative ways to update an existing active saga.

### State and effects application

[`SagaAttempt`](../../src/Clean.Messaging.Sagas/Runtime/SagaAttempt.cs) deserializes the saved state, invokes the scoped handler, verifies its non-null result, serializes the modified state, and captures the observed version. [`SagaRuntime`](../../src/Clean.Messaging.Sagas/Runtime/SagaRuntime.cs) then:

1. Ensures the instance remains active and its in-memory version equals the expected version.
2. Applies serialized state, increments version, and updates `UpdatedAt`.
3. Applies Continue/Complete/Fail outcome.
4. Applies effects in their declared order.
5. Cancels the remaining timer group for a terminal outcome.

Database optimistic concurrency is checked when the owning context saves, not while a handler is running. If effect application or persistence fails, the surrounding transaction must roll back the staged state/effects. The runtime does not open its own transaction or call `SaveChanges`; durable EF inbox/local-outbox dispatch and scheduling execution provide the transaction boundaries. Handler code using another context, direct external I/O, or an independent transaction is outside those guarantees.

### Timers

The internal [`SagaScheduleDelivery`](../../src/Clean.Messaging.Sagas/Delivery/SagaScheduleDelivery.cs) requires a schedule group, derives the saga ID from it, and directly invokes saga processing. Both trigger MessageId and TimerId are the schedule GUID. No separate saga-timer storage or timer worker exists: timers are `ScheduledMessages` rows, claimed, retried, completed, and cleaned by the scheduling capability.

A missing saga throws a transient not-found exception. A terminal saga returns without deserializing or running a handler, and the schedule can complete. Otherwise the current saga type resolves its definition and the timer payload resolves its message contract/handler. The timer message does not need a correlate function because its owning group selects the instance. The same instance-directed path can invoke a type registered by any `On` overload.

A successful timer execution means its local saga processing and schedule completion committed; it does not mean outgoing messages were delivered externally. A permanently failed timer leaves the saga active unless application policy has already changed it. Deadline semantics are handler policy: a timer being due is not proof it fired at that instant, nor proof other events have not won a concurrent transition.

## Options and state serialization

[`SagaOptions`](../../src/Clean.Messaging.Sagas/Configuration/SagaOptions.cs), in `Clean.Messaging.Sagas.Configuration`, exposes:

| Property | Default | Constraint / meaning |
| --- | --- | --- |
| `Section` | `Messaging:Sagas` | Constant section name; registration takes delegates, not automatic binding. |
| `SerializerOptions` | `new JsonSerializerOptions(JsonSerializerDefaults.Web)` | Saga-state JSON serialization, separate from message payload serializer configuration. |
| `Retention` | 30 days | Positive; terminal state age before purging. |
| `CleanupInterval` | 1 hour | Positive; periodic cleanup tick, no immediate startup purge. |
| `CleanupBatchSize` | 500 | Positive; one batch per tick. |

State serialization uses [`SagaStateSerializer`](../../src/Clean.Messaging.Sagas/Runtime/SagaStateSerializer.cs), wrapping JSON/unsupported-type failures in `SagaSerializationException`; a deserialized null state is also a permanent serialization failure. There is no explicit persisted state-schema version or built-in migration hook. The numeric saga Version is a concurrency counter, **not** a serializer schema version. Rolling deployments must keep state, saga type, message contracts, and handler interpretations compatible or provide application migration tooling.

Saga options contain no retry count, handler lease, processing concurrency, or timer interval. Incoming dispatch uses inbox/outbox retry/lease settings; timers use scheduling settings. Serializer and cleanup services use `IOptions<SagaOptions>` rather than a reload-aware monitor. See [configuration](configuration.md) and [processing](processing.md).

## EF schema, concurrency, and transaction boundaries

The internal [`SagaEntry`](../../src/Clean.Messaging.Sagas/Persistence/SagaEntry.cs) maps to [`Sagas`](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConfiguration.cs):

| Stored member | Mapping / purpose |
| --- | --- |
| `Id` | GUID primary key. |
| `Type` | Required, maximum 200 ASCII characters. |
| `Key` | Required converted string, maximum 500 characters. |
| `State` | Required serialized JSON string; purged tombstone value is the literal string `null`. |
| `Version` | Numeric EF concurrency token; advances for applied handler state. |
| `StartedByMessageId` | Start delivery identity retained for duplicate-start detection. |
| `CreatedAt`, `UpdatedAt` | UTC clock values from `TimeProvider`. |
| `StatePurgedAt`, `CompletedAt`, `FailedAt` | Nullable cleanup/outcome timestamps. |
| `FailureCode`, `FailureMessage` | Nullable diagnostics with mapped maximum lengths 200 and 500. |

`UX_Sagas_Type_Key` is a unique index on type and key across active and terminal instances. Another index covers state-purge and terminal/update timestamps. `IsActive`/`IsTerminal` are computed and not mapped. There is no persisted per-saga lease, consumed-message history, compensation journal, or separate timer entity in the saga table.

`SagaEntry.ApplyState` guards the in-memory expected version; EF's Version token guards saved updates against concurrent writers. A competing committed update normally surfaces as a `DbUpdateConcurrencyException`; there is no saga-specific concurrency retry loop that reloads and reruns handlers. The owning durable delivery's retry/execution strategy is responsible for a later attempt. Handler external effects may already have occurred before that failure, so they require independent protection.

The [save interceptor](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaSaveInterceptor.cs) rejects ambient transactions when saga entities change. It translates failed starts through the [conflict resolver](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConflictResolver.cs): when a `DbUpdateException` identifies exactly one added saga identity, the resolver queries persisted type/key in a fresh scope. Same start MessageId becomes transient `saga.start.race`; an existing terminal instance becomes `SagaClosedException`; an active instance becomes `SagaConflictException`. No persisted instance, multiple starts, or a failed lookup leaves the original exception unchanged. Translation is based on the observable added entity and persisted winner, not a database-specific unique-constraint error-code parser.

EF inbox dispatch and local outbox delivery open a transaction, invoke the consumer, complete its owned durable row, save, and commit. Schedule execution similarly gates its owned schedule, invokes the saga timer, saves state/effects and schedule completion, and commits. Under same-context composition this makes a successful local transition and its staged effects one local commit. It is not a transaction spanning broker delivery or another service. See [persistence](persistence.md) for host composition and [durable workflows](../explanation/durable-workflows.md) for consequences.

## Exceptions and failure results

Public exceptions are in `Clean.Messaging.Sagas.Exceptions` and derive from `MessageException`:

| Type / constructor | Code | Classification |
| --- | --- | --- |
| `SagaNotFoundException(string sagaType, SagaKey key)` or `SagaNotFoundException(SagaId sagaId)` | `saga.not_found` | Retry: a needed start/state can arrive later. |
| `SagaConflictException(string sagaType, SagaKey key)` | `saga.start.conflict` | Fault: another active start owns the key. Exposes `SagaType`, `Key`. |
| `SagaClosedException(string sagaType, SagaKey key)` | `saga.start.closed` | Fault: another terminal start owns the key. Exposes `SagaType`, `Key`. |
| `SagaSerializationException(string message, Exception? innerException = null)` | `saga.serialization.failed` | Fault: unsupported/missing saga definition, invalid timer group, unexpected payload, or state serialization failure. |

The EF same-start race exception is internal and retry-classified. Unexpected handler exceptions and database conflicts propagate to the owning processing policy; shutdown cancellation is not a workflow `Fail` result. Exhausting inbox/outbox or timer attempts faults that delivery, **not automatically the saga**.

The public [`SagaFailure(string code, string message)`](../../src/Clean.Messaging.Sagas/Persistence/SagaFailure.cs) record in `Clean.Messaging.Sagas.Persistence` validates nonblank code/message and maximum lengths 200/500 and exposes read-only properties. However, `SagaResult.Fail(string,string)` does **not** construct/use that record or perform those validations; it stores its arguments through the result. The EF model's lengths remain relevant, with enforcement dependent on the database. Do not assume validation by `SagaFailure` is automatically applied to Fail results.

## Management, retention, and recovery

[`ISagaManager`](../../src/Clean.Messaging.Sagas/ISagaManager.cs) is a read-only public API:

```csharp
ValueTask<Saga?> Get(
    SagaId id, CancellationToken cancellationToken = default);

ValueTask<IReadOnlyList<Saga>> GetActive(
    int limit = 100, CancellationToken cancellationToken = default);

ValueTask<IReadOnlyList<Saga>> GetFailed(
    int limit = 100, CancellationToken cancellationToken = default);

ValueTask<IReadOnlyList<SagaTimer>> GetTimers(
    SagaId sagaId, CancellationToken cancellationToken = default);
```

Active/failed limits must be positive. EF queries are no-tracking: active instances are ordered by `CreatedAt` ascending, failed by `FailedAt` descending. `Get` returns null if absent. `GetTimers` first finds the saga, returns an empty list if absent, and otherwise queries its persisted `saga` schedule group ordered by due time and schedule ID. Unsaved local changes are not included. No API is supplied to list completed instances separately, retrieve typed state, reopen/reset a saga, force failure/completion, purge its identity, or replay a timer.

### Snapshot shapes

[`Saga`](../../src/Clean.Messaging.Sagas/Saga.cs) is a sealed positional record containing `Id`, `Type`, `Key`, `Version`, `CreatedAt`, `UpdatedAt`, nullable `StatePurgedAt`, `CompletedAt`, `FailedAt`, `FailureCode`, `FailureMessage`. `IsActive` means neither terminal timestamp is set; `IsTerminal` is its inverse. Serialized state and StartedByMessageId are intentionally absent from this management snapshot.

[`SagaTimer`](../../src/Clean.Messaging.Sagas/Effects/SagaTimer.cs), in `Clean.Messaging.Sagas.Effects`, is a sealed positional record containing `Guid Id`, `SagaId`, `SagaType`, `MessageContract`, `DateTime DueAt`, `Attempts`, nullable `NextAttemptAt`, `ConsumedAt`, `CancelledAt`, `FailedAt`, `FailureCode`, `FailureMessage`. `ConsumedAt` maps schedule `DispatchedAt`, not broker acknowledgement. `IsTerminal` means consumed, cancelled, or failed. It does not expose payload, lease/claim, correlation, or version.

### State purge and identity retention

The [cleanup worker](../../src/Clean.Messaging.Sagas/Persistence/SagaCleanupWorker.cs) processes one configured batch on each tick. The [store](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaStore.cs) selects unpurged terminal states with completion/failure strictly older than the cutoff, ordered by `UpdatedAt`; it replaces State with `null` and sets `StatePurgedAt` using a bulk update. It does not delete rows, increment Version, or change UpdatedAt. Active instances are not expired merely because they are old.

Type/key, start identity, outcome, diagnostics, and uniqueness survive as tombstones. This prevents restarting the same logical key after payload retention expires and allows terminal message no-ops without deserializing purged state. It also means row/key storage grows independently of payload cleanup and state is unavailable for later forensic recovery through that record.

Timer rows have **scheduling's** separate retention/deletion policy; a retained saga tombstone does not preserve timer history indefinitely. Outbox/inbox records have their own retention boundaries as well. There is no saga recovery poller that retries a failed business result or compensates it: later ordinary messages, scheduled timer recovery, and durable delivery management are separate mechanisms. Business-level recovery, compensation, restart under a new key, and archival tooling must be designed by the application.

## Related references

- [Scheduling](scheduling.md): timer claims, due-time handling, retries, transaction gating, and cleanup.
- [Outbox](outbox.md): staging outgoing effects, identity fan-out, local and external delivery.
- [Inbox](inbox.md): durable admission, per-consumer completion, retry and replay.
- [Contracts](contracts.md): message registration and serialized compatibility.
- [Processing](processing.md): retry policies, ownership, and cancellation outside saga state.
- [Configuration](configuration.md): option binding and startup validation.
- [Durable workflows](../explanation/durable-workflows.md): consistency, trade-offs, and alternate architectures.
