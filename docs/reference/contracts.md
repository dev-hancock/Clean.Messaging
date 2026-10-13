# Contracts, identities, and serialization

## Scope and baseline

For engineers looking up the provider-neutral message contracts and durable identities. Source reviewed on 2026-10-13; this is not an integration-test report. Capability-specific admission, staging, and scheduling rules are in [Inbox](inbox.md), [Outbox](outbox.md), and [Scheduling](scheduling.md).

## Consumer contract

Namespace: `Clean.Messaging.Abstractions`. [Source](../../src/Clean.Messaging.Abstractions/IMessageConsumer.cs).

```csharp
public interface IMessageConsumer<in TMessage> where TMessage : notnull
{
    ValueTask Handle(TMessage message, CancellationToken cancellationToken);
}
```

Consumers are resolved in a scope by their registered concrete consumer type. The cancellation token belongs to the processing attempt, not to the lifetime of the message. Cancellation, retries, and local completion are described in [Processing](processing.md) and [Persistence](persistence.md).

Generic contravariance does not imply polymorphic registry fan-out: registry lookup groups registrations by exact message type. The serializer uses the concrete runtime type of its input.

## Registration and registries

[Registration source](../../src/Clean.Messaging/DependencyInjection.cs).

| Registration | Constraint | Effect |
| --- | --- | --- |
| `AddContract<TMessage>(string contract)` | `TMessage : notnull`; nonblank name, maximum 500 characters | Registers the stable serialized name for the CLR type and ensures core messaging services. |
| `AddConsumer<TMessage, TConsumer>(string consumerId)` | `TConsumer : class, IMessageConsumer<TMessage>`; nonblank ID, maximum 200 ASCII characters | Adds an invoker and `TryAddScoped<TConsumer>`, preserving a prior consumer service registration. |

Contract names and consumer IDs use ordinal, case-sensitive lookup. Duplicate contract types, duplicate contract names, and duplicate consumer IDs are rejected when registries are constructed, including repeated identical registrations. Consumer IDs are unique across all registered message types.

[`IMessageContractRegistry`](../../src/Clean.Messaging/Contracts/MessageContractRegistry.cs) exposes `GetContract(Type)` and `GetType(string)`. [`IConsumerRegistry`](../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs) exposes `All`, `Get(Type)` and `Get(string consumerId)`. A missing consumer list faults; it is not an empty successful lookup.

Hosted [`ConsumerValidation`](../../src/Clean.Messaging/Consumers/ConsumerValidation.cs) verifies that every consumer message type has a contract and its concrete consumer service is registered. It checks registration availability, not successful consumer execution or database connectivity.

## Durable serialization

Namespace: `Clean.Messaging.Serialization`.

```csharp
public readonly record struct MessageData(string Type, string Value);

public interface IMessageSerializer
{
    MessageData Serialize(object message);
    object Deserialize(MessageData data);
}
```

`Type` stores the registered contract identifier, not an assembly-qualified CLR name. `Value` stores JSON. The default [`MessageSerializer`](../../src/Clean.Messaging/Serialization/MessageSerializer.cs) uses System.Text.Json web defaults, unless a `JsonSerializerOptions` instance is supplied through DI. Serialization resolves `message.GetType()`; deserialization resolves the persisted contract to its registered CLR type. A JSON `null` result is a permanent deserialization failure.

Transport payload serialization is a separate contract: see [Transport](transport.md). Changing durable serializer options does not automatically change MQTT payload mapping or producer compatibility.

## Identity catalogue

| Identity | Role | Scope |
| --- | --- | --- |
| `MessageId` | Identifies a delivery batch; shared by its target entries | Durable entry lookup combines it with `TargetId`. |
| `EventId` | Logical event/operation identity | Duplicate protection uses it with target identity. |
| `TargetId` | Registered consumer or external target | A successful target is independent of another target's failure. |
| `CorrelationId` | Optional grouping of related operations | Metadata, not a uniqueness constraint or workflow state. |
| `CausationId` | Optional identity of the causing delivery | Metadata; outbox defaults can inherit the current delivery ID. |
| `ClaimId` | Token for a processing attempt's ownership | Database claim/settlement, not logical event identity. |
| `ScheduleId` | Stable caller-supplied scheduled operation identity | Used as logical event identity when a schedule delivers into outbox. |
| `SagaId` / `SagaKey` | Stored instance identity / definition-specific correlation key | Saga storage and handler correlation. |

The meanings of caller-facing `Id` properties differ: inbox ID is its admission/delivery `MessageId`, while outbox ID becomes logical `EventId`. See the exact capability references before constructing or retrying work. [Message identity](../explanation/message-identity.md) explains the relationship.

[`EntryKey`](../../src/Clean.Messaging.Abstractions/EntryKey.cs) is `readonly record struct EntryKey(Guid MessageId, string TargetId)`. Durable EF mappings use primary key `(MessageId, TargetId)` and unique `(EventId, TargetId)`; inbox admission also has a unique `EventId`. Retention bounds these protections; correlation IDs do not substitute for deduplication keys.

## Runtime context

[`IMessageContext`](../../src/Clean.Messaging.Abstractions/IMessageContext.cs) exposes nullable `MessageId`, `CorrelationId`, `CausationId`, and `ConsumerId`. It does not expose `EventId`, `ClaimId`, or the current database transaction.

The singleton [`MessageContext`](../../src/Clean.Messaging/MessageContext.cs) stores execution-local state in `AsyncLocal`. Its `Push(Guid messageId, Guid? correlationId, Guid? causationId = null, string? consumerId = null)` returns an `IDisposable` scope that restores the previous state. Outside a pushed scope its properties are null. This metadata scope is not durable storage and does not extend a transaction into background work.

[`ConsumerDispatcher.Dispatch`](../../src/Clean.Messaging/Processing/ConsumerDispatcher.cs) resolves the consumer ID, deserializes `MessageData`, and invokes the typed consumer in its supplied scope. The invoker faults if the deserialized object is not compatible with its expected message type.

## Shared management result types

All are in `Clean.Messaging.Abstractions`:

| Type | Fields |
| --- | --- |
| [`DeadLetter`](../../src/Clean.Messaging.Abstractions/DeadLetter.cs) | `EntryKey Key`, `Guid EventId`, `DateTime DeadLetteredAt`, `int Attempts`, nullable `Code` and `Error` |
| [`RequeueResult`](../../src/Clean.Messaging.Abstractions/RequeueResult.cs) | `int Requested`, `int Requeued` |

Requeue results distinguish requested entries from those actually eligible and changed. Consult capability management APIs for filtering and transition rules.

## Contract failure codes

[`MessageException`](../../src/Clean.Messaging/Exceptions/MessageException.cs) carries `Code`, `FailureAction Action`, and `IsTransient`; the latter is true only for `FailureAction.Retry`. These default contract/serialization failures use `Fault`:

| Code | Condition |
| --- | --- |
| `contract.type.not_registered` | CLR type has no registered serialized contract. |
| `contract.identifier.not_registered` | Persisted contract name is not registered. |
| `serialization.serialize.failed` | JSON serialization fails with `JsonException` or `NotSupportedException`. |
| `serialization.deserialize.failed` | JSON deserialization fails or returns null. |
| `consumer.message_unhandled` | Exact message type has no registered consumers. |
| `consumer.not_registered` | Consumer ID cannot be resolved. |
| `consumer.message_type_mismatch` | Invoker receives an incompatible object. |

Blank/overlong registration arguments and duplicate registrations are configuration errors, not successful durable processing results. Other errors are not universally wrapped; consult [Processing](processing.md) for retry classification.

## Compatibility boundaries

Persisted names, JSON shapes, consumer IDs, target IDs, and transport metadata must remain interpretable while relevant retained work exists. The runtime has no automatic contract version negotiation, upcasting, or producer/consumer deployment coordination. [Configuration](configuration.md) describes composition; [Message identity](../explanation/message-identity.md) explains identity reuse and retention consequences.
