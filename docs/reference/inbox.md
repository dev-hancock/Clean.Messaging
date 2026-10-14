# Inbox reference

## Scope and evidence

This page describes durable admission, per-consumer processing and management in `Clean.Messaging.Inbox` and `Clean.Messaging.Inbox.EntityFrameworkCore`. It is a source-based reference reviewed on **13 October 2026**. No build, tests, database or broker execution was performed for this review; behaviour below describes the inspected implementation, not a provider-support certification.

Transport decoding and broker acknowledgement are separate contracts: see [Transport](transport.md). Shared ownership and failure mechanics are described in [Processing](processing.md) and [Persistence](persistence.md); rationale is in [Transaction boundaries](../explanation/transaction-boundaries.md).

## Application-facing admission API

Namespace: `Clean.Messaging.Inbox`. Source: [IInbox.cs](../../src/Clean.Messaging.Inbox/IInbox.cs).

```csharp
public sealed record InboxMessage<T>(
    Guid Id,
    T Message,
    Guid? CorrelationId = null,
    Guid? CausationId = null,
    Guid? EventId = null);

public interface IInbox
{
    ValueTask Accept<T>(
        InboxMessage<T> message,
        CancellationToken cancellationToken = default)
        where T : notnull;
}
```

The same file also declares `InboxMessage(Guid Id, string Type, object Payload, Guid? CorrelationId = null, Guid? CausationId = null, Guid? EventId = null)`. There is **no non-generic `IInbox.Accept` overload** taking that record. The typed API serialises the supplied message through the registered durable contract; it does not accept arbitrary pre-serialised payloads.

| Input | Current meaning and validation |
| --- | --- |
| `Id` | Delivery `MessageId`; must not be `Guid.Empty`. Reuse the identity for the same incoming delivery. |
| `Message` | Non-null payload. Serialization and consumer selection use its concrete runtime type, not merely `T`. |
| `EventId` | Logical event identity. Defaults to `Id`; an explicitly empty value is rejected. |
| `CorrelationId`, `CausationId` | Optional metadata copied verbatim; admission does not inherit these from `IMessageContext`. |
| `cancellationToken` | Passed to durable admission I/O. A completed admission does not mean a consumer has completed. |

[`Inbox.Accept`](../../src/Clean.Messaging.Inbox/Inbox.cs) rejects a null envelope or payload, validates identities, serialises the payload, and selects registered consumers. It takes a UTC creation timestamp from `TimeProvider` and creates one `InboxEntry` per consumer, using that consumer's durable ID as `TargetId`. After the store returns, it wakes the inbox signal for a non-empty batch. The signal is an in-process wake-up, not a durability record.

Serialization requires a registered contract. Consumer lookup is exact-type lookup. [`ConsumerRegistry.Get(Type)`](../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs) throws a permanent `MessageException` with code `consumer.message_unhandled` if there are no consumers. Thus the standard API does **not** presently admit messages solely for later replay when no consumer exists. See [Contracts](contracts.md) for registration and serialization failures.

## Durable admission contract

Namespace: `Clean.Messaging.Inbox.Persistence`. Source: [IInboxStore.cs](../../src/Clean.Messaging.Inbox/Persistence/IInboxStore.cs).

```csharp
public sealed record InboxBatch(
    Guid MessageId,
    Guid EventId,
    MessageData Message,
    Guid? CorrelationId,
    Guid? CausationId,
    DateTime CreatedAt,
    IReadOnlyList<InboxEntry> Entries)
{
    public bool IsEmpty => Entries.Count == 0;
}

public interface IInboxStore : IDurableStore<InboxEntry>
{
    ValueTask Accept(InboxBatch batch, CancellationToken cancellationToken);

    ValueTask<ReplayResult> Replay(
        string consumerId,
        string contract,
        DateTime? from,
        DateTime? to,
        int limit,
        CancellationToken cancellationToken);
}
```

This is a provider extension contract. Its inherited claim, renewal, completion, retry, dead-letter and cleanup signatures are listed in [Persistence](persistence.md#durable-store-port).

### EF admission ordering and duplicate handling

[`InboxStore<TDbContext>`](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs) implements admission as follows:

1. Reject any non-null `System.Transactions.Transaction.Current` with `InvalidOperationException` (`Inbox admission cannot run inside an ambient transaction.`).
2. Create a fresh DI scope and resolve the configured context type. The caller's scoped context and its tracked changes are not used for this save.
3. Query `InboxAdmission` without tracking. An admission is considered accepted if **either** its `MessageId` matches the batch or its `EventId` matches the batch.
4. If already accepted, return without adding target entries or comparing payloads/metadata.
5. Otherwise add one admission and all batch entries, then call `SaveChangesAsync` once.
6. On `DbUpdateException`, clear the fresh context's tracker and check the same identities again. Suppress the exception only if an admission now exists; otherwise rethrow.

With the supplied relational mappings and normal EF save transaction behaviour, the admission and initial consumer entries are captured by the same save. No distributed transaction with a broker is provided. Admission is independent of an explicit transaction on the caller's other context; ambient transactions are rejected rather than silently suppressed.

The admission table has primary key `MessageId` and a unique `EventId`. Consumer rows have primary key `(MessageId, TargetId)` and unique `(EventId, TargetId)`. Concurrent admissions therefore rely on database constraints as well as the pre-check. Identity reuse with a different payload is not diagnosed: the existing admission wins. Repeating `Accept` does not add newly registered consumers to an existing admission; replay is the separate operation for creating missing per-consumer work.

**Empty-batch limitation:** `InboxBatch` can represent an empty batch, but the EF store's `CreateAdmission` explicitly throws `InvalidOperationException` when `Entries.FirstOrDefault()` is null, even though the envelope is present on the batch. Provider callers must not infer empty-batch admission support from `IsEmpty`.

## Persisted models

Sources: [InboxEntry.cs](../../src/Clean.Messaging.Inbox/Persistence/InboxEntry.cs), [InboxAdmission.cs](../../src/Clean.Messaging.Inbox/Admission/InboxAdmission.cs) and [InboxConfiguration.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxConfiguration.cs).

| Model | Fields and role |
| --- | --- |
| `InboxAdmission` (`Clean.Messaging.Inbox.Admission`) | `MessageId`, `EventId`, required `MessageData Message`, nullable correlation/causation IDs, `CreatedAt`; original retained admission envelope. All properties are init-only. |
| `InboxEntry` (`Clean.Messaging.Inbox.Persistence`) | Init-only envelope fields, required `TargetId`, and mutable attempts, claim, scheduling, terminal timestamps and failure fields from `IDurableEntry`. `Key` computes `new EntryKey(MessageId, TargetId)`. |

The tables are `InboxAdmission` and `Inbox`. `MessageData.Type` is a registered durable contract name; `Value` is the serialised payload. `InboxAdmission` additionally has an index `(CreatedAt, MessageId)` for chronological access. Exact mapping lengths and shared indexes are in [Persistence](persistence.md#inbox-and-outbox-mappings).

There is no general application-facing inbox query/list/get API. Dead-letter projections and replay selection are the supported management queries. EF models are public and may be queried through the application's context, but such queries are application-owned rather than a separate library query service.

## Consumer processing and completion

Sources: [InboxProcessor.cs](../../src/Clean.Messaging.Inbox/Dispatching/InboxProcessor.cs), [InboxDispatcher.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxDispatcher.cs).

Provider port, namespace `Clean.Messaging.Inbox.Dispatching`:

```csharp
public interface IInboxDispatcher
{
    ValueTask<bool> Dispatch(
        OwnedEntry<InboxEntry> owned,
        CancellationToken cancellationToken);
}
```

The worker claims eligible entries through `IDurableStore<InboxEntry>`. Claiming increments `Attempts`; it is not incremented on consumer return. The processor pushes message, correlation, causation and target/consumer metadata into `MessageContext`, renews the claim and links claim-loss cancellation with host cancellation.

Each EF dispatch uses the configured context's execution strategy. **Each strategy invocation creates a fresh scope** and:

1. Checks whether that `(MessageId, TargetId)` already has `ProcessedAt != null`; if so, returns `true` without invoking the consumer.
2. Opens an EF transaction.
3. Deserialises the payload and invokes only the entry's registered consumer through `ConsumerDispatcher`.
4. Conditionally completes the entry using its claim ID and an unexpired lease.
5. If completion affects no row, rolls back and returns `false`.
6. Otherwise saves tracked changes and commits; returns `true`.

Exceptions trigger a rollback attempt with `CancellationToken.None`. Rollback exceptions are suppressed to preserve the original failure, including an uncertain commit. An EF execution strategy may invoke the consumer again; its initial completion check can recognise an already persisted completion on a later invocation.

**Conditional local atomicity:** consumer changes made through the **same configured scoped context**, owned completion and any correctly staged outbox work in that transaction commit together. Another context/database, an HTTP request, file write or broker publication is outside this boundary. Consumers should not treat their invocation as an exactly-once opportunity to perform an external effect.

### Failure and recovery

- Successful `Dispatch` is recorded by processor metrics/logging; the dispatcher itself performs completion.
- `false`, or renewal loss without a completed dispatch, is treated as lost ownership, not a message fault.
- Host-requested cancellation propagates without normal retry/dead-letter settlement.
- Other exceptions are classified by `MessageFailureFactory` and `RetryPolicy`. A non-transient `MessageException` faults immediately; otherwise retries are bounded by configured maximum attempts.
- Retry and dead-letter updates require the original claim to remain valid. Settlement has its own configured timeout and is linked to claim loss, not to the host cancellation token.
- Settlement failure may leave the row claimed until expiry; durable recovery can claim it again. Claim fencing protects local settlement, not an external effect that has already occurred.

Shared transition predicates, failure fields, retry policy and ordering limitations are covered by [Persistence](persistence.md) and [Processing](processing.md).

## Management APIs

Namespace: `Clean.Messaging.Inbox.Runtime`. Sources: [IInboxManager.cs](../../src/Clean.Messaging.Inbox/Runtime/IInboxManager.cs), [InboxManager.cs](../../src/Clean.Messaging.Inbox/Runtime/InboxManager.cs).

```csharp
ValueTask<IReadOnlyList<DeadLetter>> GetDeadLetters(
    int limit = 100,
    CancellationToken cancellationToken = default);

ValueTask<RequeueResult> Requeue(
    IReadOnlyCollection<EntryKey> entries,
    CancellationToken cancellationToken = default);

ValueTask<int> Discard(
    IReadOnlyCollection<EntryKey> entries,
    CancellationToken cancellationToken = default);

ValueTask<ReplayResult> Replay<TMessage>(
    string consumerId,
    DateTime? from = null,
    DateTime? to = null,
    int limit = 1000,
    CancellationToken cancellationToken = default)
    where TMessage : notnull;
```

| Operation | Behaviour |
| --- | --- |
| `GetDeadLetters` | Requires positive limit. Lists non-discarded dead letters, oldest dead-letter timestamp first, with no payload. |
| `Requeue` | Requires a non-null collection. Performs one store operation per supplied key; returns `RequeueResult(Requested, Requeued)` and wakes processing if any were requeued. Not an atomic batch. |
| `Discard` | Requires a non-null collection. Marks individual non-discarded dead letters discarded; returns successful count. Not a deletion or an atomic batch. |
| `Replay<TMessage>` | Requires nonblank consumer ID, positive limit and `from <= to` when both bounds exist. Resolves `TMessage`'s registered contract, then creates missing entries from retained admissions. Wakes processing when `Replayed > 0`. |

The shared [dead-letter reference](persistence.md#dead-letter-store-and-projections) gives projection signatures, transaction restrictions and precisely which state is reset. Requeue retains failure diagnostics and identity, resets attempts to zero, and makes work immediately eligible. Discard retains the row and its diagnostics until cleanup.

### Replay selection and limits

[`ReplayResult`](../../src/Clean.Messaging.Inbox/Runtime/ReplayResult.cs) is `readonly record struct ReplayResult(int Matched, int Replayed)`.

- Admissions are filtered by stored contract, with inclusive `CreatedAt >= from` and `CreatedAt <= to` bounds.
- Selection is ordered by `CreatedAt`, then `MessageId`, and limited **before** existing consumer entries are removed. `Matched` is this limited selection count, not the total database match count.
- Any existing `(EventId, consumerId)` entry blocks creation, whether pending, processed, dead-lettered or discarded. Replay is **not** a reset/re-execution operation for existing rows.
- New rows preserve admission delivery/event identity, payload, metadata and original creation timestamp. They start with default unprocessed state.
- The implementation validates consumer-ID syntax but does not check that the consumer exists or consumes `TMessage`. An invalid association can create work that later fails in dispatch.
- Replay rejects ambient transactions and saves in a fresh scope.
- On **any** `DbUpdateException`, the store clears tracking, rereads matching entries and returns the difference in existing-event counts. It does not verify that every intended row was inserted or rethrow solely because the original exception was not a duplicate race. A small or zero replay count must not be interpreted as proof that persistence succeeded for all candidates.
- There is no cursor or offset. Repeated calls with the same bounds and limit revisit the earliest admissions, including those already represented by entries; they do not automatically advance through later history.

## Registration, workers and retention

`Clean.Messaging.Inbox.EntityFrameworkCore.DependencyInjection` provides:

```csharp
IServiceCollection AddInbox<TDbContext>(
    this IServiceCollection services,
    Action<InboxOptions>? configure = null)
    where TDbContext : DbContext;

IServiceCollection AddInbox<TDbContext>(
    this IServiceCollection services,
    IConfiguration configuration)
    where TDbContext : DbContext;

MessagingBuilder<TDbContext> AddInbox<TDbContext>(
    this MessagingBuilder<TDbContext> builder,
    Action<InboxOptions>? configure = null)
    where TDbContext : DbContext;

ModelBuilder AddInbox(this ModelBuilder modelBuilder);
```

The configuration overload binds section `Inbox`. Registration adds scoped inbox/store/manager/processor/dispatcher services, singleton signal/work loop, and hosted processing, recovery, cleanup and validation services. Startup validation requires a relational context, both inbox models, and a resolvable processor. It does not create/migrate the database. See [Persistence](persistence.md#composition-and-installation-contracts) and [Configuration](configuration.md).

The work loop wakes at startup; admission, requeue and replay can signal it. Periodic recovery also wakes it so missed signals, due retries and expired claims can be rediscovered. Cleanup runs initially and periodically, deleting a bounded batch of terminal **InboxEntry** rows older than the configured retention cutoff.

**Admission retention differs from entry retention:** the inspected `InboxStore` does not override shared cleanup, and the cleanup worker only calls `IDurableStore<InboxEntry>.Cleanup`. There is no supplied cleanup operation for `InboxAdmission`. Therefore terminal entry cleanup removes per-consumer duplicate/replay-blocking records but does not remove original admissions:

- ordinary duplicate `Accept` remains blocked while an admission exists;
- replay can recreate a consumer entry after its former terminal row has been deleted;
- admission payload storage can continue growing unless the application adds its own retention policy;
- deleting admissions externally changes admission duplicate protection and historical replay availability.

No FIFO execution, permanent duplicate protection, automatic payload migration or exactly-once external effects are promised.

## Related references

- [Contracts](contracts.md): durable contracts, consumers and context.
- [Processing](processing.md): claim renewal, work loops, retries, failures and metrics.
- [Persistence](persistence.md): schema, store extension ports and transaction integration.
- [Configuration](configuration.md): inbox and shared processing settings.
- [Transport](transport.md): incoming representation and broker acknowledgement policy.
- [Outbox](outbox.md): durable outgoing work from a consumer transaction.
- [Scheduling](scheduling.md) and [Sagas](sagas.md): deferred delivery and durable workflows.
