# Transport, ingress, dispatch, and MQTT reference

## Scope and verification

This page is the lookup reference for `Clean.Messaging.Transport`, `Clean.Messaging.Inbox.Ingress`, `Clean.Messaging.Outbox.Dispatch`, and their MQTT adapters. It owns the MQTT option defaults and validation tables. Shared durable-processing options and retries are in [Processing](processing.md); host composition and option binding are in [Configuration](configuration.md).

**Source reviewed: 2026-10-13.** Behaviour below was traced in this checkout, not integration-tested against a broker or database. MQTT adapters use MQTTnet 5.2.0.1603 and explicitly select MQTT 5. Defaults supplied by MQTTnet but not set by these adapters are not library guarantees.

## Provider-neutral representations

| Contract | Members / signature | Semantics |
| --- | --- | --- |
| [`TransportMessage`](../../src/Clean.Messaging.Transport/TransportMessage.cs) | Required `string Contract`, `ReadOnlyMemory<byte> Payload`; optional `Guid? MessageId`, `EventId`, `CorrelationId`, `CausationId`; optional `string? ContentType`; `IReadOnlyList<TransportHeader> Headers = []` | Transport payload and envelope metadata, not a persisted `MessageData` or a consumer message. Required members do not perform runtime validity checks themselves. |
| [`TransportHeader`](../../src/Clean.Messaging.Transport/TransportHeader.cs) | Record `(string Name, string Value)` | List representation permits repeated names; the transport determines interpretation. |
| [`ITransportMapper<TSource, TDestination>`](../../src/Clean.Messaging.Transport/TransportMapper.cs) | `TDestination Map(TSource source)`; source contravariant, destination covariant | Synchronous, payload-only mapping. Receives neither headers nor a cancellation token. |
| [`ITransportSerializer`](../../src/Clean.Messaging.Transport/TransportSerializer.cs) | `ReadOnlyMemory<byte> Serialize<TMessage>(TMessage message)`; `TMessage Deserialize<TMessage>(ReadOnlyMemory<byte> payload)`; `TMessage : notnull` | Serializes the payload, not the full transport envelope. |
| `TransportSerializers.Json` | Shared `ITransportSerializer` instance | System.Text.Json UTF-8 serialization with `JsonSerializerDefaults.Web`. Deserialization returning null throws `InvalidOperationException`. |

Ingress and dispatch registration use `TryAddSingleton(TransportSerializers.Json)` for the serializer service. A caller-supplied `ITransportSerializer` registered first is retained. Transport JSON options are separate from the runtime's durable-message JSON options; the built-in transport serializer does not expose an options-registration overload. For durable contracts and `MessageData`, see [Contracts](contracts.md).

### Metadata propagation

Ingress maps the payload object and carries message/event/context IDs into `InboxMessage<TMessage>`. It does **not** persist `ContentType` or arbitrary transport headers, nor expose them to `ITransportMapper.Map`. Dispatch builds a new transport message from stored IDs and a mapped payload. Its built-in factory leaves `ContentType` null and `Headers` empty; it does not infer `application/json` merely because JSON is used.

## Inbox ingress API

Sources: [registration](../../src/Clean.Messaging.Inbox.Ingress/DependencyInjection.cs), [ingress dispatch](../../src/Clean.Messaging.Inbox.Ingress/InboxIngress.cs), [mapping execution](../../src/Clean.Messaging.Inbox.Ingress/TransportIngress.cs), and [inbox factory](../../src/Clean.Messaging.Inbox.Ingress/InboxMessageFactory.cs).

| API | Contract |
| --- | --- |
| `services.AddIngress()` | Registers the serializer, singleton inbox-message factory, scoped mapping registry, and scoped `IInboxIngress`; does not install durable inbox persistence. |
| `services.AddIngress<TMessage>()` | Registers identity payload-to-message mapping. |
| `services.AddIngress<TPayload, TMessage, TMapper>()` | Registers a scoped mapper implementing `ITransportMapper<TPayload, TMessage>` and a scoped mapping; both payload/message types are `notnull`. |
| `IngressMap(Type PayloadType, Type MessageType)` | Value key for the mapping registry; selection is by the exact type pair. |
| `IInboxIngress.Accept(IngressMap map, TransportMessage message, CancellationToken cancellationToken = default)` | Returns `ValueTask`; deserializes `TPayload`, invokes the mapper, creates `InboxMessage<TMessage>`, then awaits `IInbox.Accept`. Rejects a null transport message. |

The supplied `IngressMap`, not the text in `TransportMessage.Contract`, chooses the deserialization type. Generic ingress does not compare that text with the registered payload contract. MQTT supplies that additional boundary check.

The factory requires a present, nonempty `MessageId`; it never assigns an anonymous incoming delivery a replacement ID. Missing/empty message identity throws `InboxIngressException`. The optional event ID is passed through; `Inbox.Accept` defaults an absent event ID to the message ID and rejects an empty event ID. Correlation and causation are passed through unchanged.

Missing mappings throw `InboxIngressException`. Deserialization, mapping, and factory failures are wrapped in that exception (an existing `InboxIngressException` is preserved). `IInbox.Accept` is outside that wrapping block: persistence, durable serialization, and other admission failures propagate separately. The ingress registry uses `ToFrozenDictionary` over map keys; conflicting registrations are not resolved by precedence. There is no generic ingress retry or acknowledgement loop.

## Outbox dispatch API and transport port

Sources: [registration](../../src/Clean.Messaging.Outbox.Dispatch/DependencyInjection.cs), [dispatch API](../../src/Clean.Messaging.Outbox.Dispatch/IOutboxDispatch.cs), [mapping execution](../../src/Clean.Messaging.Outbox.Dispatch/TransportDispatch.cs), [message factory](../../src/Clean.Messaging.Outbox.Dispatch/TransportMessageFactory.cs), and [registry](../../src/Clean.Messaging.Outbox.Dispatch/TransportDispatchRegistry.cs).

| API | Contract |
| --- | --- |
| `services.AddDispatch()` | Registers the transport serializer and scoped factory, mapping registry, and `IOutboxDispatch`; does not install outbox persistence. |
| `services.AddDispatch<TMessage>()` | Identity message-to-payload mapping. |
| `services.AddDispatch<TMessage, TPayload, TMapper>()` | Scoped mapper implementing `ITransportMapper<TMessage, TPayload>`; message/payload types are `notnull`. |
| `DispatchMap(Type MessageType, Type PayloadType)` | Exact message/payload type pair used as registry key. |
| `IOutboxDispatch.Create(DispatchMap map, OutboxEnvelope envelope)` | Synchronous `TransportMessage` creation; rejects a null envelope. |
| [`OutboxEnvelope`](../../src/Clean.Messaging.Outbox/Transport/OutboxEnvelope.cs) | Record `(Guid MessageId, Guid EventId, string TargetId, string? Destination, MessageData Message, Guid? CorrelationId, Guid? CausationId)` passed to a transport. |
| [`IOutboxTransport`](../../src/Clean.Messaging.Outbox/Transport/IOutboxTransport.cs) | `string Name`; `ValueTask Dispatch(OutboxEnvelope envelope, CancellationToken cancellationToken)` | External transport port; returning does not itself record durable completion. |
| `services.AddOutboxTransport<TTransport>()` | Registers the concrete transport scoped and an `IOutboxDelivery` wrapper. |

Dispatch deserializes the stored `MessageData` using `IMessageSerializer`, checks that it is a `TMessage`, maps to `TPayload`, resolves the **payload** type's registered contract, and serializes using `ITransportSerializer`. Message/event/context IDs are copied from the envelope. The source message contract and outgoing payload contract can therefore differ and must both be resolvable in their respective paths.

Duplicate map pairs, missing mappings, incompatible deserialized source types, mapper failures, and outgoing contract/serialization failures become [`OutboxDispatchException`](../../src/Clean.Messaging.Outbox.Dispatch/OutboxDispatchException.cs), a permanent `MessageException` with code `outbox.dispatch.failed`. These faults are not retried by the shared message retry policy.

[`OutboxTransport<TTransport>`](../../src/Clean.Messaging.Outbox/Transport/OutboxTransport.cs) calls the transport and only then calls `store.Complete(key, claimId, now, token)`. Publish and completion are separate operations. [`OutboxDispatcher`](../../src/Clean.Messaging.Outbox/Delivery/OutboxDispatcher.cs) selects a delivery by the stored transport name with ordinal comparison; duplicate transport registrations and missing deliveries throw `InvalidOperationException`.

## MQTT route contracts

### Incoming subscriptions

[`AddMqttInbox`](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/DependencyInjection.cs) takes `Action<MqttInboxOptions> configure` and `Action<MqttInboxBuilder> routes`. Both callbacks and the service collection are required. It installs ingress, validates options on startup, and registers a singleton router, scoped ingress handler, and hosted MQTT inbox worker. At least one completed subscription registration is required. Durable inbox storage and payload/message contracts still require separate registration.

The [builder](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxBuilder.cs) offers:

```csharp
mqtt.On<TPayload>(topicFilter).Subscribe();
mqtt.On<TPayload>(topicFilter).Map<TMessage, TMapper>().Subscribe();
```

`On` requires a nonblank filter; `Subscribe` creates the route. Filters and incoming topics use MQTT syntax, not durable consumer IDs. [Filter validation](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/RouteFilter.cs) requires `+` and `#` to occupy whole topic levels and `#` to be last. The [router](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Routing/MqttInboxRouter.cs) rejects overlapping filters at construction, including duplicate filters; it distinguishes filters beginning with `$` when detecting overlap. Runtime matching uses `MqttTopicFilterComparer` and requires exactly one match. No match or multiple matches is poison, not broadcast fan-out. Durable inbox fan-out to consumers occurs after this single route selection.

### Outgoing targets

[`AddMqttOutbox`](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/DependencyInjection.cs) takes `Action<MqttOutboxOptions> configure` and `Action<MqttOutboxBuilder> routes`. It requires both callbacks and at least one completed publish route, validates options on startup, registers a singleton connection/route registry/target provider, and installs the scoped MQTT outbox transport. It does not create a MQTT publishing hosted worker: durable outbox workers drive publication.

The [builder](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/MqttOutboxBuilder.cs) offers:

```csharp
mqtt.On<TMessage>(targetId).Publish(message => destination, retain: false);
mqtt.On<TMessage>(targetId).Map<TPayload, TMapper>()
    .Publish(message => destination, retain: false);
```

These are API-shape examples, not executable setup. `targetId` must be nonblank, at most 200 ASCII characters, and unique within that builder. The target provider matches the concrete runtime message type exactly. Route target IDs must also be unique across all targets in an outbox batch, including local consumers. The stored transport name is [`MqttTransport.Name = "mqtt"`](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttTransport.cs).

[`MqttOutboxTargets`](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Routing/MqttOutboxTargets.cs) evaluates the destination callback against the **original message when staging** and rejects a blank destination with `OutboxDispatchException`. Destination is stored per target; retries do not recompute it from current routing logic. Payload mapping happens later during dispatch. The route's current map and `Retain` flag are looked up by stored target ID on each dispatch, so changing/removing a route can affect already-stored work. `retain` defaults to false. It is an MQTT retain flag, not a durable entry retention setting.

The library checks destination nonblankness, not a comprehensive outbound MQTT topic grammar at registration. MQTTnet/broker validation can reject a generated topic later. Incoming filters and outgoing destination callbacks are not cross-validated.

## MQTT metadata and serialization

Sources: [incoming metadata handler](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs), [incoming reserved names](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Routing/MqttHeaders.cs), [outgoing message factory](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttMessageFactory.cs), and [outgoing reserved names](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttMessageMetadata.cs).

Reserved user-property names are ordinal and case-sensitive:

| Property | Ingress | Outgoing publication |
| --- | --- | --- |
| `message-id` | Required for ingress admission; missing becomes a mapping/factory poison failure. If present, must parse as a nonempty GUID. | Required nonempty GUID; UTF-8 GUID `D` format. |
| `event-id` | Optional; if present, nonempty GUID. If absent, inbox uses message identity. | Required nonempty GUID; UTF-8 GUID `D` format. |
| `contract` | Optional declaration; if present, must exactly equal the route payload type's registered contract. Route contract determines deserialization. | Payload type's registered contract encoded as UTF-8. |
| `correlation-id` | Optional; if present, nonempty GUID. | Included if present, GUID `D` format. Factory does not separately reject an empty optional GUID. |
| `causation-id` | Optional; if present, nonempty GUID. | Included if present, GUID `D` format. Factory does not separately reject an empty optional GUID. |

For repeated reserved names, ingress reads the **last** exact-name property. A malformed present ID is poison rather than treated as absent. An absent contract declaration is accepted, but the configured payload type must still have a registered contract.

Incoming payload bytes are taken directly from a single segment or copied from multiple segments. MQTT Content Type and nonreserved user properties are copied into the intermediate `TransportMessage`; they do not change the selected serializer. Reserved properties are excluded from its `Headers` list. Incoming QoS, duplicate/retain flags, topic, and packet identity are not persisted as the durable message identity.

Outgoing MQTT construction sets the stored destination topic, serialized payload, configured QoS, and route retain flag. It includes Content Type only when nonblank and appends nonreserved headers as UTF-8 properties; caller headers cannot override exact reserved names. The built-in dispatch factory supplies no arbitrary headers or Content Type. No content-type negotiation or payload schema version negotiation is implemented.

## MQTT options: defaults and validation

These tables are authoritative for library-supplied MQTT defaults. The two adapters have distinct option types and QoS property names. Both configure credentials only when `Username` is nonblank. `UseTls = true` enables MQTTnet TLS with an empty TLS customization callback; the adapter offers no custom certificate/validation options here. Port does not change automatically when TLS is enabled. No username/password/TLS combination validation is supplied.

### `MqttInboxOptions`

Namespace: `Clean.Messaging.Inbox.Ingress.Mqtt.Configuration`. Sources: [options](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Configuration/MqttInboxOptions.cs), [validator](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Configuration/MqttInboxOptionsValidator.cs).

| Property | Default | Library validation / behaviour |
| --- | --- | --- |
| `Host` | Required member; no usable default | Nonblank. |
| `Port` | `1883` | Greater than zero; validator does not impose a 65535 upper bound. |
| `ClientId` | Required member; no usable default | Nonblank; identifies the broker session. |
| `SessionExpiryInterval` | `uint.MaxValue` | Greater than zero; passed as MQTT session-expiry seconds. |
| `Username` | `null` | Optional; blank means credentials are not configured. |
| `Password` | `null` | Optional; used only with a nonblank username. |
| `UseTls` | `false` | Boolean; no extra adapter validation. |
| `ReconnectDelay` | 5 seconds | Greater than zero and no greater than `TimeSpan.FromMilliseconds(uint.MaxValue - 1)`. |
| `QualityOfServiceLevel` | `AtLeastOnce` (QoS 1) | Only QoS 1 or `ExactlyOnce` (QoS 2). QoS 0 and other enum values are rejected. |
| `ResetSessionOnStart` | `false` | If true, first successful connection uses clean start; subsequent reconnects remain persistent. |

### `MqttOutboxOptions`

Namespace: `Clean.Messaging.Outbox.Dispatch.Mqtt`. Sources: [options](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/MqttOutboxOptions.cs), [validator](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/MqttOutboxOptionsValidator.cs).

| Property | Default | Library validation / behaviour |
| --- | --- | --- |
| `Host` | Required member; no usable default | Nonblank. |
| `Port` | `1883` | Greater than zero; no validator upper bound. |
| `ClientId` | Required member; no usable default | Nonblank. |
| `Username` | `null` | Optional; blank means credentials are not configured. |
| `Password` | `null` | Optional; used only with a nonblank username. |
| `UseTls` | `false` | Boolean; no extra adapter validation. |
| `QualityOfService` | `AtLeastOnce` (QoS 1) | Only QoS 1 or QoS 2. |

Outgoing options have no `ReconnectDelay`, `ResetSessionOnStart`, session-expiry, or separate publish-retry settings. Outgoing connect options do not explicitly set clean start/session expiry; do not substitute the inbox values for MQTTnet defaults. These default values are not a recommendation for production security or broker session provisioning.

## Incoming acknowledgement, sessions, and failures

Source: [`MqttInboxWorker`](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs).

1. The worker connects with MQTT 5, configured client ID, `WithCleanStart(_resetSession)`, and session expiry. It checks the connection result for `Success`.
2. After that successful connection, `_resetSession` becomes false **before subscriptions are attempted**. Each configured filter is subscribed again on every connection; the worker does not branch on session-present status.
3. Each subscription must return exactly one result granted at QoS 1 or 2. It need not equal the requested QoS; granted QoS 0/rejection causes connection-loop failure.
4. Received deliveries disable `AutoAcknowledge`. Routing and admission run in a fresh async scope using the host stopping token.
5. The worker calls `AcknowledgeAsync` only after `IInboxIngress.Accept` returns successfully, except for the explicit poison policy below. This means acknowledgement follows durable **admission**, not consumer completion.
6. Unexpected disconnection or a signalled admission/acknowledgement failure wakes reconnect. The loop attempts disconnect, waits the configured fixed reconnect delay, and reconnects. Connect/subscription failures use this same loop rather than durable message retries.

| Receive outcome | Worker action |
| --- | --- |
| Valid payload and successful admission, including detected duplicate admission | Acknowledge using the stopping token. |
| Shutdown cancellation with stopping token cancelled | Leave delivery unacknowledged; no ordinary admission-failure classification. |
| `MqttPoisonMessageException` (routing ambiguity/miss, conflicting contract, malformed identity, wrapped ingress mapping failure) | Error log, then explicit acknowledge attempt. No durable poison record is written. |
| Poison acknowledgement failure | Warning log and reconnect signal; shutdown cancellation is suppressed. |
| Other admission or normal acknowledgement exception | Error log and reconnect signal; no successful acknowledgement is manufactured. |

`MqttInboxIngressHandler` wraps `InboxIngressException` as poison and preserves shutdown cancellation. It does not wrap ordinary durable admission exceptions as poison. For example, the current EF inbox rejects a newly admitted message with no consumer entries; that failure remains an admission failure, not an automatically discarded poison message. See [Inbox](inbox.md).

Reconnect requests are coalesced by a one-slot semaphore. Connection/disconnect operations share a lock; worker-initiated disconnects do not re-signal through the disconnected callback. Shutdown removes event handlers and attempts final disconnect with `CancellationToken.None`, logging failure. Broker storage, session expiry/reset, granted QoS, and broker configuration determine whether an unacknowledged delivery can actually be redelivered; the worker cannot guarantee redelivery after session data is discarded.

## Outgoing connection, publication, and failures

Sources: [`MqttConnection`](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Connection/MqttConnection.cs), [`MqttOutboxTransport`](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttOutboxTransport.cs), and [`MqttPublishException`](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttPublishException.cs).

The connection is a singleton owning one MQTT client. `GetClient(token)` returns an already-connected client, otherwise serializes connection establishment using a semaphore and checks connectivity again inside the lock. It connects with MQTT 5 and rejects a result leaving the client disconnected. It has no separate background reconnect loop; a later durable dispatch attempt reconnects if necessary. The lock gates connection, not every publication. Disposal disposes the semaphore and client; no explicit outgoing graceful disconnect is supplied.

Dispatch resolves the current route by target ID, maps/serializes the payload, obtains a connected client, constructs the MQTT message, and awaits `PublishAsync(token)`. It tests `result.IsSuccess`; success returns to the generic outbox wrapper for fenced database completion. It does not await downstream application consumption.

| Failed publish reason | Stored code | Shared retry classification |
| --- | --- | --- |
| `NotAuthorized` | `mqtt.publish.not_authorized` | Permanent (`FailureAction.Fault`). |
| `TopicNameInvalid` | `mqtt.publish.topic_name_invalid` | Permanent. |
| `PayloadFormatInvalid` | `mqtt.publish.payload_format_invalid` | Permanent. |
| Any other unsuccessful result | `mqtt.publish.rejected` | Transient (`FailureAction.Retry`). |

`MqttPublishException` includes the broker reason code/string in its exception message. Network/connect/publish exceptions not translated into this exception propagate to the durable processor. Unknown exceptions are retryable within [shared retry limits](processing.md#retryoptions-defaults-and-validation). Cancellation caused by shutdown or claim loss is handled separately by that processor. There is no independent adapter retry around `PublishAsync`.

## MQTT logs and telemetry

The inbox worker uses source-generated `ILogger<MqttInboxWorker>` events:

| Event ID | Level | Signal / structured fields |
| --- | --- | --- |
| 1 | Information | Connected; `Host`, `Port`, `RouteCount`. |
| 2 | Warning | Connection/subscription failure; `Host`, `Port`, exception. |
| 3 | Error | Poison rejected and acknowledgement about to be attempted; `Topic`, exception. The wording says "acknowledged" before acknowledgement succeeds. |
| 4 | Warning | Poison acknowledgement failed; reconnect requested; `Topic`, exception. |
| 5 | Error | Admission or normal acknowledgement failed; reconnect requested; `Topic`, exception. |
| 6 | Debug | Disconnect failure; exception. Template mentions shutdown even when used in reconnect cleanup. |

The outgoing adapter has no dedicated logger events or MQTT metrics in these sources. Its failures/completions are observed through [outbox processing logs and the `Clean.Messaging` meter](processing.md#metrics-and-logs). Durable processing counters are not broker acknowledgement or downstream-effect counters.

## Related references and explanation

- [Architecture](architecture.md): package dependencies and host boundary.
- [Contracts](contracts.md): durable contract names, consumer identity, serialization, and context.
- [Inbox](inbox.md) / [Outbox](outbox.md): admission, staging, targets, and management.
- [Persistence](persistence.md): EF transaction integration and schema requirements.
- [Processing](processing.md): ownership, retry, recovery, cleanup, and operational signals.
- [Configuration](configuration.md): registrations, bindings, and lifetimes.
- [Delivery semantics](../explanation/delivery-semantics.md): protocol delivery versus local processing and external effects.

The [documentation index](../README.md) lists the complete Reference and Explanation set. Cross-references provide related contracts and reasoning, not additional implementation guarantees.
