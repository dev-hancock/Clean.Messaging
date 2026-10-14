# Domain-event capture reference

## Scope and evidence

`Clean.Messaging.DomainEvents` supplies an in-memory event-source abstraction. `Clean.Messaging.DomainEvents.EntityFrameworkCore` captures events from tracked sources into the transactional outbox. Neither package supplies business rules, aggregate persistence mappings, an event store, or synchronous domain-event handler dispatch.

Baseline: source review on **13 October 2026**. No build, tests or persistence execution was performed. Current implementation facts are distinguished from conceptual discussion in [Transaction boundaries](../explanation/transaction-boundaries.md).

## Event-source APIs

Namespace: `Clean.Messaging.DomainEvents`. Sources: [RaisedEvent.cs](../../src/Clean.Messaging.DomainEvents/RaisedEvent.cs), [AggregateRoot.cs](../../src/Clean.Messaging.DomainEvents/AggregateRoot.cs).

```csharp
public sealed record RaisedEvent(Guid Id, object Message);

public interface IDomainEventSource
{
    IReadOnlyCollection<RaisedEvent> Events { get; }
    void Acknowledge(Guid eventId);
}

public abstract class AggregateRoot : IDomainEventSource
{
    protected void Raise<TEvent>(TEvent message)
        where TEvent : notnull;
}
```

| Contract | Current behaviour |
| --- | --- |
| `RaisedEvent.Id` | Logical event identity used as outbox `EventId`. The record constructor itself does not validate empty IDs. |
| `RaisedEvent.Message` | Event payload object. The record constructor itself has no runtime null guard. Capture later uses outbox payload validation/serialization. |
| `AggregateRoot.Raise` | Rejects null payload, generates a new GUID and appends a raised event to its private list. It does not serialise, save or publish. |
| `IDomainEventSource.Events` | Explicit implementation on `AggregateRoot`, returning its pending list through `IReadOnlyCollection<RaisedEvent>`. Access requires the interface; no snapshot copy or concurrency guarantee is supplied. |
| `IDomainEventSource.Acknowledge` | Explicit implementation removes every pending list element whose ID matches. Unknown IDs do nothing. It does not mutate the aggregate's business state. |

`AggregateRoot` has no entity key, state-transition rules, repository API or timestamps. Applications may derive from it or implement `IDomainEventSource` on their own mapped entities. The source interface is independent of EF Core and runtime dependencies.

`Raise` always assigns a new ID; applications implementing the interface themselves own their ID stability and acknowledgement semantics. Repeated attempts to capture an existing pending event must preserve its identity. Raising a new event again after a failure is a different logical event, not an idempotent recapture of the original.

## EF registration and installation

Namespace: `Clean.Messaging.DomainEvents.EntityFrameworkCore`. Source: [DependencyInjection.cs](../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DependencyInjection.cs).

```csharp
IServiceCollection AddDomainEvents(this IServiceCollection services);

MessagingBuilder<TDbContext> AddDomainEvents<TDbContext>(
    this MessagingBuilder<TDbContext> builder)
    where TDbContext : DbContext;
```

The extension adds a scoped `ISaveCapture` registration for internal `DomainEventCapture`, using `TryAddEnumerable`. The builder overload delegates to the service extension and returns the builder.

**This registration does not register outbox services, map outbox entries, attach interceptors or map application entities.** Capture requires:

- the consuming application's context and tracked event-source entities;
- standard outbox services, including `OutboxEntryFactory`, `IOutboxWriter` and `OutboxTracker`;
- `ModelBuilder.AddOutbox()` for durable rows;
- options `AddMessaging(services)` from the outbox EF namespace to attach both save and transaction interceptors;
- durable contracts and suitable targets for event payload types.

There is no separate `AddDomainEvents` model/options extension or domain-event table. There is no dedicated domain-event hosted startup validator in this package. The absence of a startup error is not evidence that event capture has been installed: without the outbox save interceptor, registered capture hooks are not invoked. See [Persistence](persistence.md#composition-and-installation-contracts) for complete installation contracts and [Configuration](configuration.md) for options.

## Capture ordering

Source: [DomainEventCapture.cs](../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs).

The installed outbox save interceptor calls `Capture(DbContext db)` during both synchronous and asynchronous saving. The capture implementation:

1. Enumerates the supplied context's `ChangeTracker.Entries()`, selects entities implementing `IDomainEventSource`, and snapshots that source list.
2. Visits each source's pending `Events` collection.
3. Calls `tracker.Capture<DomainEventCapture>(db, event.Id)`. If the event was already captured in the current save or pending transaction, skips it.
4. Registers `() => source.Acknowledge(event.Id)` for execution after commit.
5. Creates `OutboxMessage<object>(event.Id, event.Message)` and stages the resulting target batch through `IOutboxWriter` using the **same context**.

No filter limits sources to `Added` or `Modified` entity states. Any source returned by the change tracker can be inspected, including an unchanged tracked source with pending events. Untracked sources and detached entities are not discovered. No reflection scans assemblies or aggregate graphs outside the tracker.

The generic `object` envelope does not discard the payload's type: `OutboxEntryFactory` serialises and selects targets using `event.Message.GetType()`. Registered durable contract names, payload compatibility and exact-type consumer lookup therefore remain applicable; see [Contracts](contracts.md).

### Event and delivery identity

| Identity | Source |
| --- | --- |
| Logical `EventId` | `RaisedEvent.Id`. |
| Delivery `MessageId` | A fresh GUID per factory call, shared across that event's selected targets. |
| `TargetId` | Selected consumer/transport target ID. |
| Correlation | Current `IMessageContext.CorrelationId`, because capture supplies no explicit override. |
| Causation | Current `IMessageContext.MessageId`, because capture supplies no explicit override. |

Repeated capture uses logical event identity, not a permanent delivery GUID. The tracker deduplicates by capture type plus event ID within a save/pending transaction. The writer skips event/target pairs already tracked; the database unique index protects retained persisted event/target pairs. These are distinct mechanisms, not an unconditional idempotent save guarantee.

The default target provider requires registered local consumers; it throws if the exact event type has no local consumers, even if another provider could supply external targets. No-target capture is not automatically a durable event archive. If custom target-provider composition produces an empty batch, the writer stages no rows, but the already queued acknowledgement can still occur on a successfully observed commit.

## Acknowledgement lifecycle

Sources: [OutboxInterceptor.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxInterceptor.cs), [OutboxTracker.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs), [OutboxTransactionInterceptor.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs).

| Outcome | Pending event behaviour |
| --- | --- |
| Save succeeds without a current explicit EF transaction | Queued acknowledgement runs after successful save; pending event removed by `AggregateRoot`. |
| Save succeeds inside an explicit EF transaction | Event remains pending until the transaction commit is observed. Later saves in the same pending transaction do not recapture that capture-type/event ID. |
| Save/capture fails or save is cancelled | Current-save capture entries are detached and current-save state reset; event is not acknowledged, permitting recapture. Pre-staged outbox/application changes have different tracking ownership. |
| Explicit rollback is observed | Recorded outbox rows are detached and pending tracker state cleared; event remains unacknowledged. This is not automatic restoration of application entity state. |
| Commit fails but all pending outbox keys are verified present | Tracker treats capture as committed and acknowledges. The original commit exception can still reach the caller. |
| Commit cannot be verified | No acknowledgement; outcome remains unknown. Fresh-context/database reconciliation may be required. |

Acknowledgement means **the local capture commit was observed or verified**, not that local consumers ran, a broker acknowledged, or external receivers handled the event. Delivery happens later through the outbox; independent target failures do not restore an event to the aggregate's pending list.

The tracker rejects ambient transactions and nested `SaveChanges`; caller-managed savepoints outside active saving are unsupported. Unobserved transaction changes with pending work are rejected. See [Outbox](outbox.md#save-capture-and-commit-lifecycle) and [Persistence](persistence.md#outbox-capture-extension-contracts) for the exact failure/detachment rules.

### Custom-source identity limitations

Custom `IDomainEventSource` implementations should expose stable pending IDs and synchronous acknowledgement behaviour appropriate to their own state:

- capture suppression is keyed by event ID across sources for this capture type; two tracked sources exposing the same ID are not independently captured;
- acknowledgement callbacks are keyed only by event ID, so duplicate source identities cannot be assumed to receive two callbacks;
- callback invocation is synchronous and exceptions are not caught by `OutboxTracker.Commit`; a throwing acknowledgement can surface after the database commit and interrupt remaining callbacks/signal cleanup;
- capture iterates `Events` directly; concurrent mutation or acknowledgement during enumeration is not protected by a lock or snapshot of the event collection;
- the package does not enforce immutable event payloads. Serialization occurs during save, so mutations between raising and capture can change the stored representation.

The provided `AggregateRoot` acknowledgement only removes matching list entries and does not intentionally throw. The additional behaviours of custom implementations are application responsibilities.

## Persistence, failures and limits

- Capture stages rows in the owning context, allowing owning state and outgoing work to share a local relational commit when the application's persistence path actually uses that context/transaction.
- Serialization, target discovery and staging failures occur during saving and can fail the owning save. Invalid event identity/payload and missing contracts/consumers are not silently ignored.
- Duplicate persisted `(EventId, TargetId)` values may fail `SaveChanges`; the writer only deduplicates tracked entries, not all existing rows.
- Failed saves leave events pending; they do not guarantee that application business state has been restored to its pre-attempt state. Reusing entities after rollback requires application-level reasoning.
- Pending events exist in memory until capture; they are not durable independently of saving the application state and outbox rows. Losing the process before capture loses that unsaved event list.
- Captured events become ordinary target-specific outbox work. Cleanup and contract evolution therefore follow the outbox's retention/compatibility rules.
- This is not event sourcing: no complete aggregate history, stream version, append-only event-store API, aggregate rehydration or replay of aggregate invariants is supplied.
- It is not a direct domain-event mediator: no handler pipeline runs synchronously from `Raise` or `Capture`. Consumers execute through the outbox later.
- No exactly-once remote effect or automatic compensation is provided.

## Related references

[Outbox](outbox.md) · [Persistence](persistence.md) · [Contracts](contracts.md) · [Processing](processing.md) · [Configuration](configuration.md) · [Transport](transport.md) · [Scheduling](scheduling.md) · [Sagas](sagas.md) · [Transaction boundaries](../explanation/transaction-boundaries.md)
