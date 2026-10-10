# Clean.Messaging.Outbox.Mqtt

MQTT dispatch provider for `Clean.Messaging.Outbox`.

The provider deliberately does **not** contain outbox persistence or EF Core. The durable outbox stores a resolved destination when the message is enqueued; this provider publishes that destination as the MQTT topic when the durable worker dispatches it.

## Expected Outbox provider SPI

This project targets the provider-facing API discussed for `Clean.Messaging.Outbox`:

```csharp
public sealed record OutboxDispatch(
    Guid MessageId,
    Guid EventId,
    string Contract,
    ReadOnlyMemory<byte> Payload,
    string Destination,
    Guid? CorrelationId = null,
    Guid? CausationId = null);

public interface IOutboxDispatcher
{
    string Transport { get; }

    ValueTask Dispatch(
        OutboxDispatch message,
        CancellationToken cancellationToken);
}
```

`OutboxDispatch.Destination` must already contain the resolved MQTT topic. Topic routing therefore happens before persistence/at enqueue time, not when a retry eventually executes.

## Registration

```csharp
using Clean.Messaging.Outbox.Mqtt;
using MQTTnet;
using MQTTnet.Protocol;

var mqtt = new MqttClientOptionsBuilder()
    .WithClientId("irrigation-api")
    .WithTcpServer("192.168.1.10", 1883)
    .Build();

services.AddMqttOutbox(
    mqtt,
    options =>
    {
        options.QualityOfService =
            MqttQualityOfServiceLevel.AtLeastOnce;

        options.Retain = false;
    });
```

For command topics, QoS 1 / `Retain = false` is the intended default. The durable outbox owns retry. This package performs no second application-level retry loop: connection/publish failures are allowed to escape so the outbox processor can retry or dead-letter the entry.

## Dispatch semantics

```text
OutboxEntry
   ↓
OutboxDispatch
   Destination = persisted MQTT topic
   ↓
MqttOutboxDispatcher
   ↓
connect if required
   ↓
PublishAsync
   ↓
MQTT acknowledgement
   ↓
return success to OutboxProcessor
```

The MQTT client is shared and safe for concurrent publishes. Connection establishment is serialized because MQTTnet requires connect/disconnect lifecycle changes not to run concurrently with publishing.
