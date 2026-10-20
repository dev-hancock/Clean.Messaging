# EF Core persistence reference

## Scope

This page covers shared relational durable-entry storage and EF installation/capture contracts. Detailed capability behaviour is in [Inbox](inbox.md), [Outbox](outbox.md), [Domain events](domain-events.md), [Scheduling](scheduling.md) and [Sagas](sagas.md).

Relevant projects target .NET 10 and reference EF Core Relational 10.0.11. A relational-provider requirement is not a provider-support matrix. The [testing reference](testing.md#postgresql-persistence-integration) describes the PostgreSQL integration boundary; it does not certify all providers or failure scenarios. The application owns its context, provider, connection settings, migrations and database deployment.

## Composition and installation contracts

### Shared registration

Namespace: `Clean.Messaging.EntityFrameworkCore`. Sources: [DependencyInjection.cs](../../src/Clean.Messaging.EntityFrameworkCore/DependencyInjection.cs), [MessagingBuilder.cs](../../src/Clean.Messaging.EntityFrameworkCore/MessagingBuilder.cs).

```csharp
IServiceCollection AddMessaging<TDbContext>(
    this IServiceCollection services,
    Action<MessagingBuilder<TDbContext>> configure)
    where TDbContext : DbContext;
```

The extension registers core messaging services, records the context type and invokes the builder callback. It does not register the application's context/provider, add models, attach interceptors or create the schema. A null callback is rejected.

`MessagingBuilder<TDbContext>` exposes `IServiceCollection Services { get; }` and these fluent methods:

```csharp
MessagingBuilder<TDbContext> AddContract<TMessage>(string contract)
    where TMessage : notnull;

MessagingBuilder<TDbContext> AddConsumer<TMessage, TConsumer>(string consumerId)
    where TMessage : notnull
    where TConsumer : class, IMessageConsumer<TMessage>;

MessagingBuilder<TDbContext> ConfigureRetry(Action<RetryOptions> configure);
```

Capability packages add builder extensions; [Contracts](contracts.md) and [Configuration](configuration.md) describe registration constraints and options.

Public utility [`PersistenceServices`](../../src/Clean.Messaging.EntityFrameworkCore/PersistenceServices.cs):

```csharp
static void EnsurePersistence<TDbContext>(IServiceCollection services)
    where TDbContext : DbContext;

static void AddOptions<TOptions>(
    IServiceCollection services,
    Action<TOptions>? configure)
    where TOptions : MessageOptions, new();
```

`EnsurePersistence` records one context **type** per service collection and rejects a subsequent different type. Inbox/outbox invoke it, so their standard registrations must use the same scoped context type. This type check is not proof that manually constructed contexts share an instance, connection or transaction. `AddOptions` configures optional callbacks, registers the common validator and enables startup validation.

### Installation matrix

These are separate service, model and options-builder contracts; enabling one column does not install the others.

| Capability | Service/builder registration | Model installation | Context options installation |
| --- | --- | --- | --- |
| Inbox | `services.AddInbox<TDbContext>(...)` or `builder.AddInbox(...)` | `modelBuilder.AddInbox()` | No inbox-specific save interceptor is installed or required. |
| Outbox | `services.AddOutbox<TDbContext>(...)` or `builder.AddOutbox(...)` | `modelBuilder.AddOutbox()` | `options.AddMessaging(services)` attaches outbox save **and** transaction interceptors. |
| Domain events | `services.AddDomainEvents()` or `builder.AddDomainEvents()` | No separate event table/mapping. Application maps its entities. | Uses outbox capture and its installed interceptors; registration does not automatically add outbox. |
| Scheduling | EF `builder.AddScheduling(...)` | `modelBuilder.AddScheduling()` | `options.AddScheduling(services)` attaches scheduling save and transaction interceptors. |
| Sagas | EF `builder.AddSagas(configure, configureOptions)` also registers scheduling | `modelBuilder.AddSagas()` also maps scheduling | `options.AddSagas(services)` also attaches scheduling interceptors, then saga save interceptor. It does not attach outbox interceptors. |

Sources: [Inbox registration](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/DependencyInjection.cs), [Outbox registration](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/DependencyInjection.cs), [Domain-event registration](../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DependencyInjection.cs), [Scheduling registration](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/DependencyInjection.cs), [Saga registration](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/DependencyInjection.cs).

The options extensions return `DbContextOptionsBuilder`, with corresponding `DbContextOptionsBuilder<TDbContext>` overloads; their inputs are the builder and `IServiceProvider services`. Outbox's options `AddMessaging` is declared in `Clean.Messaging.Outbox.EntityFrameworkCore`, not the shared EF package. It is a no-op if no `OutboxInterceptor` is registered. Scheduling/saga options extensions resolve required interceptors and avoid adding the same interceptor instance twice; outbox's extension does not make that same duplicate-instance check.

Inbox/outbox service extensions have callback (`Action<InboxOptions>?` / `Action<OutboxOptions>?`) and `IConfiguration` overloads; their builder extensions accept the callback. Startup validators check relational provider/model presence. Outbox additionally checks that both outbox interceptors are attached; these checks do not verify deployed schema, every mapping constraint, migration compatibility or provider SQL translation.

**DI limitation visible in source:** [InboxCleanupWorker](../../src/Clean.Messaging.Inbox/Runtime/InboxCleanupWorker.cs) and [OutboxCleanupWorker](../../src/Clean.Messaging.Outbox/Runtime/OutboxCleanupWorker.cs) are singleton hosted services that directly request scoped stores. A scope-validating service provider can reject them; a non-validating provider can retain a store/context dependency for the host lifetime. The documentation does not imply this wiring was successfully executed.

## Inbox and outbox mappings

Namespace: `Clean.Messaging.EntityFrameworkCore`. Source: [DurableEntryConfiguration.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableEntryConfiguration.cs).

```csharp
public static void Configure<TEntry>(
    EntityTypeBuilder<TEntry> builder,
    string table)
    where TEntry : class, IDurableEntry;
```

This public model helper is used for `InboxEntry` → `Inbox` and `OutboxEntry` → `Outbox`.

| Mapping | Contract |
| --- | --- |
| Primary key | `(MessageId, TargetId)`. |
| Logical uniqueness | Unique index `(EventId, TargetId)`, scoped to each entry table. |
| `TargetId` | Max length 200, non-Unicode. Application/factory validation additionally enforces ASCII for standard target registrations. |
| `ClaimId` | Max length 32, non-Unicode. |
| `FailureCode` | Max length 200. |
| `ExceptionType`, `FailureMessage` | Max length 500 each. |
| `Message` | EF complex property, with required `Type` (max length 500) and required `Value`. No explicit SQL column names/types are assigned here. |
| `Key` | Computed convenience property ignored by EF. |
| Work index | `(ProcessedAt, DeadLetteredAt, DiscardedAt, NextAttemptAt, ClaimedUntil, CreatedAt)`. |
| Dead-letter index | `(DiscardedAt, DeadLetteredAt)`. |

Other envelope, lifecycle and failure properties use EF conventions unless the consuming model customises them. The shared mapping declares no row-version property or concurrency token; durable ownership is enforced by conditional update predicates instead. Outbox additionally stores required `Transport` and nullable `Destination`; this mapping sets no explicit max lengths for them.

[`InboxConfiguration`](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxConfiguration.cs) also maps `InboxAdmission` to `InboxAdmission` with primary key `MessageId`, unique index `EventId`, index `(CreatedAt, MessageId)` and the same required complex `Message` contract/payload mapping. Its init-only metadata and creation timestamp are stored independently of consumer entries. There is no configured foreign key/cascade relationship tying admission retention to inbox entry cleanup.

For adjacent models, [`SchedulingConfiguration`](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/SchedulingConfiguration.cs) maps `ScheduledMessages` and uses `ScheduleEntry.Version` as a concurrency token. [`SagaConfiguration`](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConfiguration.cs) maps `Sagas`, uses `SagaEntry.Version` as a concurrency token, and names the unique `(Type, Key)` index `UX_Sagas_Type_Key`. They have distinct store/lifecycle contracts; do not apply the inbox/outbox generic store model to them. See [Scheduling](scheduling.md) and [Sagas](sagas.md) for complete field, concurrency and retention details.

## Durable entry representation

Namespace: `Clean.Messaging.Persistence`. Source: [IDurableEntry.cs](../../src/Clean.Messaging/Persistence/IDurableEntry.cs).

| Properties | Types and ownership |
| --- | --- |
| `MessageId`, `EventId` | Read-only `Guid` identities. Concrete inbox/outbox entries use init-only setters. |
| `TargetId` | Read-only `string` target identity. |
| `Message` | Read-only `MessageData`; `readonly record struct MessageData(string Type, string Value)`. |
| `CorrelationId`, `CausationId` | Read-only nullable `Guid`. |
| `CreatedAt` | Read-only `DateTime`; standard creation uses UTC. |
| `Attempts` | Mutable `int`, incremented during claim. |
| `ClaimId` | Mutable nullable `string` ownership token. |
| `ClaimedUntil`, `NextAttemptAt` | Mutable nullable `DateTime`, lease expiry and retry due time. |
| `ProcessedAt`, `DeadLetteredAt`, `DiscardedAt` | Mutable nullable terminal timestamps. |
| `FailureCategory` | Mutable nullable `FailureCategory` (`Transient`, `Permanent`, `Unknown`). |
| `FailureCode`, `ExceptionType`, `FailureMessage` | Mutable nullable strings for bounded diagnostics. |
| `LastFailedAt`, `LastFailedAttempt` | Mutable nullable `DateTime` / `int` for latest recorded failure. |
| `Key` | Read-only `EntryKey`; `readonly record struct EntryKey(Guid MessageId, string TargetId)`. |

Direct mutation of these public model properties is not an alternate lifecycle API with claim fencing. The supplied store uses database predicates and bulk operations; application-owned arbitrary updates can bypass those protections.

## Durable-store port

Sources: [IDurableStore.cs](../../src/Clean.Messaging/Persistence/IDurableStore.cs), [DurableStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs).

```csharp
public sealed record OwnedEntry<TEntry>(TEntry Entry, string ClaimId);

public interface IDurableStore<TEntry>
{
    ValueTask<IReadOnlyList<OwnedEntry<TEntry>>> Claim(
        DateTime now, int limit, TimeSpan lease,
        CancellationToken cancellationToken);

    ValueTask<bool> Renew(
        EntryKey key, string claimId, DateTime now, DateTime claimedUntil,
        CancellationToken cancellationToken);

    ValueTask<bool> Complete(
        EntryKey key, string claimId, DateTime now,
        CancellationToken cancellationToken);

    ValueTask<bool> Retry(
        EntryKey key, string claimId, MessageFailure failure,
        DateTime now, DateTime nextAttemptAt,
        CancellationToken cancellationToken);

    ValueTask<bool> DeadLetter(
        EntryKey key, string claimId, MessageFailure failure, DateTime now,
        CancellationToken cancellationToken);

    ValueTask<int> Cleanup(
        DateTime before, int limit,
        CancellationToken cancellationToken);
}
```

These port methods require an explicit token; no default token is declared. `OwnedEntry` pairs the retrieved entry snapshot with its ownership token; possession of that object is not proof that its lease is still valid.

EF provider base declaration:

```csharp
public abstract class DurableStore<TDbContext, TEntry>(
    TDbContext db,
    IServiceScopeFactory scopes) : IDurableStore<TEntry>
    where TDbContext : DbContext
    where TEntry : class, IDurableEntry;
```

Its protected extension members are `TDbContext Db { get; }` and `AsyncServiceScope CreateScope()`. `InboxStore` and `OutboxStore` derive from it; they are internal EF implementations of public capability-specific store ports.

## Durable lifecycle and concurrency

### Eligibility and claiming

A row is claimable if all three terminal timestamps are null, `NextAttemptAt` is null or `<= now`, and its claim is absent, its expiry is absent, or its expiry is `<= now`.

`Claim` rejects a non-positive limit or lease duration. It creates one claim token for the batch, calculates `now + lease`, and uses an EF execution strategy with an explicit transaction and verification callback. It bulk-updates eligible rows selected by `CreatedAt`, `MessageId`, `TargetId` with a bounded limit:

- set `ClaimId` and `ClaimedUntil`;
- clear `NextAttemptAt`;
- increment `Attempts` by one.

It rereads rows for that token without tracking and returns them ordered by the same keys. If recovered-row count differs from the affected-row count, it throws `InvalidOperationException`. Uncertain-commit verification checks whether any row with the batch claim ID exists.

This is a conditional-update implementation, **not** a provider-independent `SKIP LOCKED` contract. SQL translation, locking, isolation and concurrent contender behaviour depend on the relational provider. No tested multi-instance/provider guarantee is asserted by this review. Ordered selection is not ordered processing/completion: there can be concurrent consumers, queued entries and delayed retries.

### Owned updates

`Renew`, `Complete`, `Retry` and `DeadLetter` update only a row satisfying:

- exact `(MessageId, TargetId)`;
- no processed/dead-lettered/discarded timestamp;
- `NextAttemptAt == null`;
- matching `ClaimId`;
- non-null `ClaimedUntil > now`.

At expiry equality, the old owner cannot settle. Each operation returns whether one row was affected. `false` is ownership/state rejection, not proof of a particular cause. Claim predicates protect persistence settlement but cannot prevent or undo an external side effect by a stale consumer.

| Operation | State update | Context/transaction placement |
| --- | --- | --- |
| `Renew` | Replace lease expiry with supplied `claimedUntil`. | Fresh DI scope/context. |
| `Complete` | Set `ProcessedAt = now`; clear claim, expiry and next attempt. | Uses the store's existing `Db`, allowing participation in the local delivery transaction. |
| `Retry` | Record supplied latest failure; set next due time; clear claim/expiry. | Fresh scope/context; conditional bulk update. |
| `DeadLetter` | Record latest failure; set dead-letter timestamp; clear claim/expiry/next attempt. | Fresh scope/context; execution-strategy transaction with dead-letter-state verification. |
| `Cleanup` | Delete eligible terminal rows; return count. | Fresh scope/context; bulk delete. |

All use `ExecuteUpdateAsync`/`ExecuteDeleteAsync`, which operate directly against the database rather than waiting for `SaveChanges` or synchronising already tracked entities. Consumer code should not rely on a tracked entry object being refreshed after settlement.

`Cleanup` rejects non-positive limits. It selects rows with **any** processed/dead-lettered/discarded timestamp strictly earlier than `before`, orders by creation time, limits the batch and deletes it. It does not delete unresolved rows or `InboxAdmission`. Terminal cleanup removes the retained unique logical target key as well as payload and diagnostics.

### Failure data

[`MessageFailure`](../../src/Clean.Messaging/Failures/MessageFailure.cs) is:

```csharp
public sealed record MessageFailure(
    FailureCategory Category,
    string Code,
    string ExceptionType,
    string Message,
    DateTime FailedAt,
    int Attempt);
```

The factory maps messaging exceptions' retry/fault action to transient/permanent category; unrecognised exceptions become `Unknown` with a generic safe failure message. It truncates code to 200 and type/message to 500 characters. Retry/dead-letter updates persist these values and last-failure time/attempt. Successful completion and requeue do not erase all historic diagnostic fields. See [Processing](processing.md) for exception codes, retry evaluation, cancellation and settlement timeout.

## Dead-letter store and projections

Namespace: `Clean.Messaging.DeadLetters`. Source: [IDeadLetterStore.cs](../../src/Clean.Messaging/DeadLetters/IDeadLetterStore.cs).

```csharp
public interface IDeadLetterStore<TEntry>
    where TEntry : class, IDurableEntry
{
    ValueTask<IReadOnlyList<DeadLetter>> Get(
        int limit, CancellationToken cancellationToken);

    ValueTask<bool> Requeue(
        EntryKey key, CancellationToken cancellationToken);

    ValueTask<bool> Discard(
        EntryKey key, CancellationToken cancellationToken);
}
```

Namespace: `Clean.Messaging.Abstractions`. Sources: [DeadLetter.cs](../../src/Clean.Messaging.Abstractions/DeadLetter.cs), [RequeueResult.cs](../../src/Clean.Messaging.Abstractions/RequeueResult.cs).

```csharp
public sealed record DeadLetter(
    EntryKey Key,
    Guid EventId,
    DateTime DeadLetteredAt,
    int Attempts,
    string? Code,
    string? Error);

public readonly record struct RequeueResult(int Requested, int Requeued);
```

Public EF implementation [`DeadLetterStore<TDbContext, TEntry>`](../../src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs) has constructor inputs `TDbContext db, TimeProvider time`, constraints `TDbContext : DbContext` and `TEntry : class, IDurableEntry`, and implements the three operations above.

- `Get` queries rows with non-null `DeadLetteredAt` and null `DiscardedAt`, without tracking, ordered by `DeadLetteredAt`, limited and projected. No tie-breaker or pagination cursor is supplied. The store itself does not enforce a positive limit; inbox/outbox managers do.
- `Requeue` only updates such non-discarded dead letters. It clears dead-letter/discard/next-attempt/claim/expiry fields and resets `Attempts` to zero. It retains delivery/event/target identity, payload, creation timestamp and latest failure diagnostics. It does not reset successful rows.
- `Discard` marks such rows with current UTC `DiscardedAt` and clears claim/expiry/next-attempt. It retains dead-letter timestamp, attempts and failure diagnostics. It is logical discard, not immediate physical deletion.
- Each mutation rejects a non-null ambient `Transaction.Current` and uses an EF execution-strategy transaction with a state-verification query. The code does not silently suppress an ambient transaction or combine a manager's whole collection into one transaction.
- Requeue/discard return whether a row was changed. An absent, already discarded or otherwise non-matching row normally returns `false`. Under uncertain-commit verification the required target state is used to determine success.

There is no general list of pending/processed work, payload inspection API, batch rollback guarantee or successful-entry replay API on this store. Capability management adds validation, counts and signals, not an additional audit/query system.

## Outbox capture extension contracts

Namespace: `Clean.Messaging.Outbox.EntityFrameworkCore`. Sources: [ISaveCapture.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/ISaveCapture.cs), [OutboxWriter.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs), [OutboxTracker.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs).

```csharp
public interface ISaveCapture
{
    void Capture(DbContext db);
}

public interface IOutboxWriter
{
    void Stage(DbContext db, OutboxBatch batch);
}
```

`ISaveCapture` is a scoped multi-registration hook run by the installed outbox save interceptor. It is synchronous even for asynchronous saving. It stages local work within the supplied context; it is not a notification that the transaction has committed. `IOutboxWriter` is the singleton EF writer; its default implementation adds only targets missing from tracked entries with the same event ID and does not query persisted duplicates.

`OutboxTracker` is a public scoped type constructed with `OutboxSignal`. Its public members are integration/lifecycle primitives, not an application unit-of-work replacement:

```csharp
bool IsSaving(DbContext db);
bool IsCommitting(DbContext db);
void BeginSave(DbContext db);
bool Capture<TCapture>(DbContext db, Guid eventId);
void AcknowledgeAfterCommit(DbContext db, Guid eventId, Action acknowledge);
void TrackEntries(DbContext db);
void SaveSucceeded(DbContext db);
void SaveFailed(DbContext db);
void TransactionCommitting(DbContext db);
void TransactionCommitted(DbContext db);
EntryKey[] GetPendingKeys(DbContext db);
void TransactionCommitUnknown(DbContext db);
void TransactionRolledBack(DbContext db);
```

`Capture<TCapture>` requires an active save and suppresses repeated capture by `(typeof(TCapture), eventId)` within current/pending work. `AcknowledgeAfterCommit` requires an active save, rejects a null callback and keeps at most one callback per event ID. Capture-key namespaces include capture type, but acknowledgement keys do not: custom integrations must not infer that two sources with the same event ID each get an independent callback. `GetPendingKeys` returns keys only while a commit is in progress.

The tracker keeps state in a `ConditionalWeakTable<DbContext, ...>`. During `BeginSave` it rejects ambient transactions, nested saving, and a transaction-ID change with pending work lacking an observed commit/rollback. Capture hooks run between `BeginSave` and `TrackEntries`.

### Save and transaction handling

- The save interceptor tracks entries and invokes save success/failure/cancellation methods for both sync and async EF APIs.
- Success without a current explicit EF transaction acknowledges and signals immediately after the save completes; success with a transaction defers these until observed commit.
- Save failure detaches outbox entries newly introduced by capture in that save, not all pre-staged or application changes. Events remain unacknowledged for recapture.
- Observed explicit rollback detaches recorded outbox entries and clears tracker state; restoring an aggregate's application state remains the application's responsibility.
- Commit failure verification uses a fresh context to check all pending entry keys. Verification uses `CancellationToken.None` in the asynchronous transaction-failure path. Verification errors are swallowed in favour of retaining an unknown outcome; no acknowledgement or success claim follows from uncertainty.
- If no pending entry keys exist, a failed commit cannot be verified from outbox rows, even if capture callbacks exist.
- Caller-managed savepoint creation/rollback outside active saving is rejected. EF savepoint operations while saving are permitted.
- Changing/discarding a transaction without observed end callbacks can leave pending state; the tracker rejects an incompatible later save rather than assuming a commit.

Detailed staging/delivery paths and uncertainty limits are in [Outbox](outbox.md#save-capture-and-commit-lifecycle). Domain-event use of these hooks is in [Domain events](domain-events.md).

## Transaction boundary summary

| Operation | What is locally joined | What is not joined |
| --- | --- | --- |
| Inbox admission | Admission and initial consumer entries in one fresh-context save. | Caller context changes, ambient transactions, broker acknowledgement and eventual handling. |
| Owning outbox save | Owning tracked changes and staged target entries in the same context/save or EF transaction. | Later consumer delivery or broker publication. |
| Inbox / local outbox delivery | Consumer's same-context changes, owned completion and locally staged follow-on work in one EF transaction. | Other contexts/databases and external side effects. |
| External outbox delivery | Transport call completes before database completion is attempted. | No distributed atomic commit spans publication and completion. |
| Retry/dead-letter settlement | Conditional owner update in a separate scope after the failed local attempt. | Rolled-back consumer changes and previously performed external effects. |

EF execution strategies can rerun local dispatch code. The initial completion query recognises some already-committed outcomes but does not make arbitrary consumer side effects exactly once. Database constraints and lease predicates are conditional safeguards, not blanket guarantees of duplicate-free external execution.

## Operational limits

- Ensure migrations preserve logical uniqueness and claim-related indexes; the library supplies model configuration, not migration deployment.
- Contract names and target IDs remain compatibility data while rows exist. Changing registrations can make retained work fail or change which new work is created.
- Entry cleanup reduces storage and removes retained target identity protection. Inbox admissions are not automatically cleaned up.
- Claim/renewal use UTC values supplied through runtime clocks. The store signatures themselves accept `DateTime` without enforcing `Kind`; custom providers/callers must preserve a consistent time basis.
- Bulk operations and execution-strategy transaction helpers require suitable relational translation. Actual locking, isolation and operational performance were not tested here.
- Public model/helper types allow custom integrations; custom writes are responsible for equivalent constraints, transaction placement, fencing and failure handling.

## Related references

[Inbox](inbox.md) · [Outbox](outbox.md) · [Domain events](domain-events.md) · [Contracts](contracts.md) · [Processing](processing.md) · [Configuration](configuration.md) · [Transport](transport.md) · [Scheduling](scheduling.md) · [Sagas](sagas.md) · [Transaction boundaries](../explanation/transaction-boundaries.md)
