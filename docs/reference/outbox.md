# Outbox reference

## Scope and evidence

This page describes outgoing staging, target selection, delivery and management in `Clean.Messaging.Outbox` and `Clean.Messaging.Outbox.EntityFrameworkCore`. Baseline: source review on **13 October 2026**; no build, tests, database or broker execution was performed. The consuming application supplies the context, migrations, consumers and transport integrations.

Exact shared store and interceptor contracts are in [Persistence](persistence.md). See [Transaction boundaries](../explanation/transaction-boundaries.md) for conceptual rationale, [Domain events](domain-events.md) for automatic capture and [Transport](transport.md) for dispatch mapping and MQTT publication.

## Application-facing staging API

Namespace: `Clean.Messaging.Outbox`. Sources: [IOutbox.cs](../../src/Clean.Messaging.Outbox/IOutbox.cs), [Outbox.cs](../../src/Clean.Messaging.Outbox/Outbox.cs).

```csharp
public sealed record OutboxMessage<T>(
    Guid Id,
    T Message,
    Guid? CorrelationId = null,
    Guid? CausationId = null);

public interface IOutbox
{
    void Enqueue<TMessage>(OutboxMessage<TMessage> message)
        where TMessage : notnull;
}
```

`Enqueue` synchronously creates a batch and calls `IOutboxStore.Stage`. With the EF provider it adds missing entries to the configured scoped context's change tracker. **It does not save, commit, publish or await a consumer.** Durability starts with the owning save/commit, not with `Enqueue` returning. There is no cancellation token because this API stages local in-memory tracked work rather than doing database I/O.

### Identity and metadata

[`OutboxEntryFactory`](../../src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs) exposes `OutboxBatch Create<T>(OutboxMessage<T> message) where T : notnull`.

| Field | Creation behaviour |
| --- | --- |
| `OutboxMessage.Id` | Required non-empty **logical `EventId`**, not the delivery ID. |
| `OutboxEntry.MessageId` | A new `Guid.NewGuid()` for each factory call, shared by all entries in its batch. |
| `OutboxEntry.EventId` | The caller's `OutboxMessage.Id`, shared by all entries. |
| `TargetId` | Stable ID returned by a target provider; one entry per selected target. |
| `Message` | `MessageData` serialised once from the payload's concrete runtime type. |
| `CorrelationId` | Explicit value, otherwise current `IMessageContext.CorrelationId`. |
| `CausationId` | Explicit value, otherwise current `IMessageContext.MessageId` (not its `CausationId`). |
| `CreatedAt` | UTC `DateTime` from `TimeProvider`, shared by the batch. |
| `Transport`, `Destination` | Target provider's persisted transport name and optional destination. |

Null envelope/payload and empty caller ID are rejected. A registered durable contract must exist. Metadata uses null-coalescing: there is no separate way to explicitly clear inherited metadata through this record. No general validation rejects empty correlation/causation GUIDs.

For retries of the **same logical outgoing operation**, retain the event ID. The factory may produce a new delivery ID, while the database unique `(EventId, TargetId)` protects retained logical target identities. This is not an application-facing idempotent database insert API: duplicates already present in the database can fail the owning save.

## Target selection and provider ports

Namespace: `Clean.Messaging.Outbox.Transport`. Sources: [IOutboxTargetProvider.cs](../../src/Clean.Messaging.Outbox/Transport/IOutboxTargetProvider.cs), [OutboxTargetProvider.cs](../../src/Clean.Messaging.Outbox/Transport/OutboxTargetProvider.cs).

```csharp
public sealed record OutboxTarget(
    string TargetId,
    string Transport,
    string? Destination);

public interface IOutboxTargetProvider
{
    IEnumerable<OutboxTarget> GetTargets(Type messageType, object message);
}
```

The factory enumerates **all** registered target providers before staging. Providers receive the concrete runtime message type and the original object, so target selection can depend on payload values. The resulting transport/destination are stored with the entry; processing does not rerun target discovery.

Validation across the combined targets requires:

- nonblank `TargetId`, at most 200 ASCII characters;
- nonblank `Transport`;
- no duplicate `TargetId`, using ordinal comparison, even if the transports/destinations differ.

Duplicate target identities cause `InvalidOperationException`. No destination format or transport-name length validation is performed by the factory. Destination requirements belong to the provider.

The default target provider selects exact-type registered consumers and creates `OutboxTarget(consumer.ConsumerId, "local", null)`. `LocalTransport.Name` is the constant `"local"`. **The default consumer lookup throws if no local consumer exists**; its presence can prevent staging an externally routed message with no local consumers even when another target provider could supply targets. Do not infer external-only support from the target-provider abstraction without considering that registration behaviour. A custom composition can replace providers, but must preserve their lifetime and identity contracts.

## Staging provider contracts and model

Namespace: `Clean.Messaging.Outbox.Persistence`. Sources: [IOutboxStore.cs](../../src/Clean.Messaging.Outbox/Persistence/IOutboxStore.cs), [OutboxEntry.cs](../../src/Clean.Messaging.Outbox/Persistence/OutboxEntry.cs).

```csharp
public sealed record OutboxBatch(
    Guid MessageId,
    Guid EventId,
    IReadOnlyList<OutboxEntry> Entries)
{
    public bool IsEmpty => Entries.Count == 0;
}

public interface IOutboxStore : IDurableStore<OutboxEntry>
{
    void Stage(OutboxBatch batch);
}
```

`OutboxEntry` implements `IDurableEntry`: init-only delivery/event/target identity, required transport and payload, nullable destination and metadata, creation time, plus mutable claim, attempts, next-attempt, terminal and failure fields. `Key` is `(MessageId, TargetId)`.

EF provider extension, namespace `Clean.Messaging.Outbox.EntityFrameworkCore`:

```csharp
public interface IOutboxWriter
{
    void Stage(DbContext db, OutboxBatch batch);
}
```

[`OutboxWriter`](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs) does nothing for an empty batch. For a non-empty batch it gathers target IDs of already tracked non-deleted `OutboxEntry` entities with the same event ID, then adds only targets absent from that tracked set. It does **not** query existing database rows, compare payload equality, or merge/rewrite a previously tracked entry. A later save can therefore encounter a persisted unique-key conflict.

[`OutboxStore<TDbContext>`](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxStore.cs) simply delegates staging to this writer using its scoped `Db` context. The table is `Outbox`; primary key is `(MessageId, TargetId)`, unique index is `(EventId, TargetId)`. See [Persistence](persistence.md#inbox-and-outbox-mappings) for all mapped fields/indexes and the implications of bulk updates bypassing tracking.

## Save, capture and commit lifecycle

Sources: [OutboxInterceptor.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxInterceptor.cs), [OutboxTracker.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs), [OutboxTransactionInterceptor.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs).

The installed save interceptor runs all registered `ISaveCapture.Capture(DbContext)` hooks during synchronous and asynchronous saving, then records Added outbox entry keys. Its tracker associates state with the context and transaction, and maintains per-save versus pending-commit work.

| Boundary | Current behaviour |
| --- | --- |
| Saving starts | Reject ambient transactions, nested saves and an unobserved transaction change with pending captured work. Capture hooks run before the save completes. |
| Save succeeds without a current explicit EF transaction | Treat as committed: invoke queued acknowledgement callbacks and wake the outbox signal if entry keys exist. |
| Save succeeds inside an explicit EF transaction | Merge keys/capture identities/callbacks into pending commit; defer acknowledgement and wake-up. |
| Save fails or is cancelled | Detach entries newly created by capture during that save, clear current-save state; retain raised-event acknowledgement callbacks only if already pending from earlier successful saves. Pre-staged entries are not treated as newly captured entries. |
| Observed explicit commit | Acknowledge pending captures and wake processing after commit, then remove tracked context state. |
| Observed explicit rollback | Detach recorded outbox entries and remove tracked transaction state. It does not restore arbitrary application entities or their original property values. |
| Commit exception | If committing with pending keys, check in a fresh context whether **all** pending `(MessageId, TargetId)` keys exist. Presence is treated as committed; absence as rollback. Verification failure, or no keys to verify, leaves commit outcome unknown and does not acknowledge. The original EF failure is not thereby turned into a successful caller result. |

The tracker rejects caller-managed savepoint creation/rollback outside an active save (`NotSupportedException`); EF's savepoint callbacks while saving are permitted. Transaction changes not observed through the installed interceptors are not supported as a substitute for an observed commit/rollback. After unresolved or unobserved transaction termination, a fresh context is safer than reusing pending tracked state; the tracker explicitly instructs callers to discard a context after an uncommitted transaction.

These contracts apply to saves using the installed interceptors, not arbitrary SQL or an externally managed transaction the tracker never observes. Atomicity with owning data depends on using the same context/save or explicit EF transaction. Domain-event acknowledgement means capture committed, **not** that any target was delivered.

## Claimed delivery and completion

Sources: [OutboxProcessor.cs](../../src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs), [OutboxDispatcher.cs](../../src/Clean.Messaging.Outbox/Delivery/OutboxDispatcher.cs).

`OutboxWorker` uses the shared bounded work loop. The processor establishes metadata and renews the claim while dispatch runs. Delivery is per target; a failure in one row does not reset another target's processed row. All claim/settlement predicates and attempt transitions are described in [Persistence](persistence.md#durable-lifecycle-and-concurrency).

Namespace: `Clean.Messaging.Outbox.Delivery`:

```csharp
public interface IOutboxDispatcher
{
    ValueTask<bool> Dispatch(
        OwnedEntry<OutboxEntry> owned,
        CancellationToken cancellationToken);
}

public interface IOutboxDelivery
{
    string Transport { get; }

    ValueTask<bool> Dispatch(
        OwnedEntry<OutboxEntry> owned,
        CancellationToken cancellationToken);
}
```

`OutboxDispatcher` builds an ordinal transport-name map. Duplicate delivery transport names cause `InvalidOperationException` during construction; a stored transport with no delivery registered throws during dispatch. A delivery must both perform its work and settle completion; `true` means completed according to that delivery. Returning `false` is interpreted as ownership loss, not a retry-classified exception.

### Local delivery

[`LocalOutboxDelivery<TDbContext>`](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/LocalOutboxDelivery.cs) handles transport `local`. Within each EF execution-strategy invocation, it creates a fresh scope, checks the row for previous completion, opens an EF transaction, invokes the target consumer, conditionally completes using the claim, saves tracked changes and commits.

If the completion update affects no row, it rolls back and returns `false`; exceptions cause rollback attempts and are propagated. Local consumer changes are atomic with completion **only if they use the same configured scoped context/transaction**. Outbox entries generated by that consumer can be captured in that save. External calls and other contexts are not included; EF execution-strategy retries can rerun consumer code.

### External transport adapter

Namespace: `Clean.Messaging.Outbox.Transport`. Sources: [IOutboxTransport.cs](../../src/Clean.Messaging.Outbox/Transport/IOutboxTransport.cs), [OutboxEnvelope.cs](../../src/Clean.Messaging.Outbox/Transport/OutboxEnvelope.cs), [OutboxTransport.cs](../../src/Clean.Messaging.Outbox/Transport/OutboxTransport.cs).

```csharp
public interface IOutboxTransport
{
    string Name { get; }
    ValueTask Dispatch(OutboxEnvelope envelope, CancellationToken cancellationToken);
}

public sealed record OutboxEnvelope(
    Guid MessageId,
    Guid EventId,
    string TargetId,
    string? Destination,
    MessageData Message,
    Guid? CorrelationId,
    Guid? CausationId);
```

`AddOutboxTransport<TTransport>(this IServiceCollection services) where TTransport : class, IOutboxTransport` returns the service collection, registers `TTransport` scoped and adapts it to a scoped `IOutboxDelivery` ([registration source](../../src/Clean.Messaging.Outbox/DependencyInjection.cs)).

The adapter copies the persisted envelope, awaits `transport.Dispatch`, and **then** calls `store.Complete` using the original claim. No transaction joins remote publication with that completion. A transport can successfully publish and then encounter cancellation, a database failure or lost ownership before completion; the row may be delivered again. This adapter does not pre-check ownership immediately before external dispatch. Downstream duplicate protection must use stable identities appropriate to the receiver. MQTT delivery/acknowledgement details remain provider-specific: see [Transport](transport.md).

### Failure and recovery

- Host cancellation propagates; it is not recorded as a normal message failure.
- Claim loss cancels handling and suppresses normal retry/dead-letter settlement unless dispatch already returned completed.
- Other exceptions are classified, retried or dead-lettered under the shared retry policy. Non-transient `MessageException` faults immediately; other failures retry only while attempts remain.
- Settlement uses a separate bounded timeout linked to claim loss. Conditional updates prevent an expired/stale owner from completing, retrying or dead-lettering a row.
- Renewal or settlement failure can leave an entry for lease-expiry recovery. Database fencing cannot retract a remote side effect already made.
- Chronological claim selection is not a FIFO completion guarantee: workers run concurrently, targets are independent, and retries can run later.

## Management and retained data

Namespace: `Clean.Messaging.Outbox.Runtime`. Sources: [IOutboxManager.cs](../../src/Clean.Messaging.Outbox/Runtime/IOutboxManager.cs), [OutboxManager.cs](../../src/Clean.Messaging.Outbox/Runtime/OutboxManager.cs).

```csharp
public interface IOutboxManager
{
    ValueTask<IReadOnlyList<DeadLetter>> GetDeadLetters(
        int limit = 100,
        CancellationToken cancellationToken = default);

    ValueTask<RequeueResult> Requeue(
        IReadOnlyCollection<EntryKey> entries,
        CancellationToken cancellationToken = default);

    ValueTask<int> Discard(
        IReadOnlyCollection<EntryKey> entries,
        CancellationToken cancellationToken = default);
}
```

`GetDeadLetters` requires a positive limit. Requeue/discard reject null collections, then operate on keys **sequentially with independent per-entry store calls**, not as an all-or-nothing batch. Cancellation or failure after an earlier operation can leave earlier keys changed. Requeue returns requested/successful counts and wakes processing if any operation succeeded. Discard returns the number marked discarded and does not delete rows.

See [Persistence](persistence.md#dead-letter-store-and-projections) for the `DeadLetter`/`RequeueResult` shapes, sort order, ambient-transaction restrictions, reset fields and retained diagnostics. There is **no `IOutboxManager.Replay`**, general outbox query API or API to reset successful rows. Requeue targets retained, non-discarded dead letters only.

Cleanup initially and periodically deletes a bounded batch of processed, dead-lettered or discarded rows whose applicable terminal timestamp is older than the retention cutoff. It does not remove unresolved rows. Deleting rows also deletes their unique `(EventId, TargetId)` duplicate-protection record: a later staging of the same logical event/target may become insertable. Retention is therefore not merely an audit-storage setting.

## Registration and installation

Namespace: `Clean.Messaging.Outbox.EntityFrameworkCore`. Source: [DependencyInjection.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/DependencyInjection.cs).

```csharp
IServiceCollection AddOutbox<TDbContext>(
    this IServiceCollection services,
    Action<OutboxOptions>? configure = null)
    where TDbContext : DbContext;

IServiceCollection AddOutbox<TDbContext>(
    this IServiceCollection services,
    IConfiguration configuration)
    where TDbContext : DbContext;

MessagingBuilder<TDbContext> AddOutbox<TDbContext>(
    this MessagingBuilder<TDbContext> builder,
    Action<OutboxOptions>? configure = null)
    where TDbContext : DbContext;

DbContextOptionsBuilder AddMessaging(
    this DbContextOptionsBuilder options,
    IServiceProvider services);

DbContextOptionsBuilder<TDbContext> AddMessaging<TDbContext>(
    this DbContextOptionsBuilder<TDbContext> options,
    IServiceProvider services)
    where TDbContext : DbContext;

ModelBuilder AddOutbox(this ModelBuilder modelBuilder);
```

The configuration overload binds section `Outbox`. Service registration, options/interceptor installation and model mapping are distinct requirements:

- `AddOutbox<TDbContext>` registers runtime/store/delivery/capture infrastructure and hosted validation/processing/recovery/cleanup services.
- `ModelBuilder.AddOutbox()` installs the `OutboxEntry` mapping.
- Options `AddMessaging(services)` attaches the save and transaction interceptors when the outbox save interceptor is registered; otherwise it returns options unchanged.
- Startup validation requires a relational context, an outbox model, both interceptors, and a resolvable processor. It does not apply migrations or certify all relational providers.

Scoped services include outbox/store/manager/processor/delivery/tracker/interceptors. Signal, work loop, entry factory, default target provider and writer are registered singleton. Custom target providers/serializers used by the singleton entry factory must be lifetime-compatible.

**Source-visible hosting limitation:** `OutboxCleanupWorker` is a singleton hosted service whose constructor takes scoped `IOutboxStore` directly; inbox cleanup has the equivalent pattern. Scope validation can reject that resolution, and without validation it can capture a scoped store for the host lifetime. Registration alone is not evidence that the host starts successfully with scope validation enabled. This review documents the wiring rather than changing it.

Full configuration defaults and validation are in [Configuration](configuration.md); common EF installation and capture restrictions are in [Persistence](persistence.md).

## Related references

[Inbox](inbox.md) · [Domain events](domain-events.md) · [Contracts](contracts.md) · [Processing](processing.md) · [Persistence](persistence.md) · [Configuration](configuration.md) · [Transport](transport.md) · [Scheduling](scheduling.md) · [Sagas](sagas.md)
