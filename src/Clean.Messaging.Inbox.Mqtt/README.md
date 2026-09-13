# Clean.Messaging.Inbox.Mqtt

MQTT ingress adapter for `Clean.Messaging.Inbox`.

It does **not** process application consumers itself. Its only job is:

```text
MQTT PUBLISH
    ↓
topic route
    ↓
deserialize payload
    ↓
IInbox.Accept(...)
    ↓
durable inbox admission succeeds
    ↓
MQTT acknowledgement
```

Once `IInbox.Accept` returns, the MQTT delivery has crossed the durable boundary. Normal inbox fan-out, claiming, retries, dead-lettering and consumer execution continue through `Clean.Messaging.Inbox` exactly as they do for any other inbox admission source.

## Why acknowledgement happens after inbox admission

For QoS 1/2 messages the adapter disables MQTTnet auto-acknowledgement and only acknowledges after `IInbox.Accept` succeeds. That prevents the broker delivery being acknowledged before it has been durably persisted.

A downstream consumer failure does **not** hold the MQTT acknowledgement open. At that point the durable inbox owns the work and its normal retry/dead-letter policy applies.

## Registration

```csharp
using Clean.Messaging.Inbox.Mqtt;

services.AddMqttInbox(
    options =>
    {
        options.Host = "localhost";
        options.Port = 1883;
        options.ClientId = "irrigation-api";
        options.QualityOfServiceLevel =
            MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce;
    },
    mqtt =>
    {
        mqtt.SubscribeJson<ValveStatusEvent>(
                "irrigation/+/state/valve/+",
                static (_, message) => message.MessageId)
            .EventId(
                static (_, message) => message.EventId)
            .CorrelationId(
                static (_, message) => message.CorrelationId);
    });
```

`Clean.Messaging.Inbox` and its persistence provider must already be registered normally.

## Stable message IDs are intentional

Every route requires a message-ID selector:

```csharp
Func<MqttApplicationMessage, TMessage, Guid> messageId
```

Do not use `Guid.NewGuid()` there when MQTT duplicate suppression matters. QoS 1 is at-least-once, so the same MQTT PUBLISH can be delivered again. A stable ID lets `IInbox` recognise repeated admission of the same message.

The ID can come from the payload, an MQTT 5 property, or another stable protocol field owned by your application. The transport adapter deliberately does not guess one.

`EventId` is optional. If it is omitted, the existing `InboxMessage<T>` behaviour can use the message ID as the logical event identity.

## Custom payloads

JSON is only a convenience. For non-JSON device payloads:

```csharp
mqtt.Subscribe<ValveStatusEvent>(
    "irrigation/+/state/valve/+",
    message => DecodeValveStatus(message.Payload),
    static (_, status) => status.MessageId);
```

The route can also derive correlation/causation metadata from the decoded message or raw `MqttApplicationMessage`.

## Topic routing

The adapter subscribes to every registered MQTT topic filter at QoS 1 by default and supports `+` and `#` matching locally.

One incoming topic must resolve to exactly one registered route. Overlapping route filters that both match the same incoming topic are treated as a configuration error. Fan-out belongs to the inbox consumer registry after the message has been decoded and admitted, rather than by decoding one MQTT PUBLISH through several competing route definitions.

## Reconnect behaviour

The hosted worker uses an explicit MQTT 5 persistent session: configure a stable, deployment-specific `ClientId`; it connects with `CleanStart = false`; and its session expiry defaults to `uint.MaxValue`. Do not use an ephemeral or generated client ID, because it would discard the broker session and undermine redelivery after a reconnect. `SessionExpiryInterval` must be nonzero, but can be reduced when the broker's retained session duration is intentionally bounded.

The worker serializes connection operations, reconnects and re-subscribes after connection loss, and backs off using `ReconnectDelay`. Every subscription must be granted at QoS 1 or QoS 2; QoS 0 or a rejected subscription faults the connection cycle.

Malformed payloads, invalid route resolution, and route decoding failures are poison messages. They are logged with their topic and acknowledged so a permanent transport fault cannot block redelivery forever. Failures admitting an otherwise valid message to `IInbox`, or other infrastructure failures, are transient: the message remains unacknowledged, the worker reconnects, and the broker redelivers it through the persistent session.

## Dependencies

- `Clean.Messaging.Inbox`
- `MQTTnet` `5.2.0.1603`
- Microsoft hosting/DI/logging/options abstractions

There is deliberately no EF Core dependency. MQTT is an ingress transport; `Clean.Messaging.Inbox.EntityFrameworkCore` remains an independent persistence provider.
