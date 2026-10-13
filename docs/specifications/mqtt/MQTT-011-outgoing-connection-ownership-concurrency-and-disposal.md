# MQTT-011 — Outgoing connection ownership, concurrency and disposal

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** [MqttConnection](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Connection/MqttConnection.cs), [transport](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttOutboxTransport.cs), registration singleton lifetime.

## MQTT-011-AC-01 — Concurrent disconnected dispatches share connection establishment
**Given** the singleton client is disconnected and multiple outgoing attempts call for it concurrently, **When** the first connection succeeds and others recheck connectivity, **Then** they use the shared connected client without simultaneously establishing separate connections. Once connected, publication is not globally serialized by this connection lock.

## MQTT-011-AC-02 — Connection rejection and later disconnect require another durable attempt
**Given** connect returns while the client remains disconnected, **When** dispatch requests the client, **Then** the attempt fails rather than publishing on assumed connectivity. **Given** a later disconnected client, **When** a later durable attempt requests it, **Then** it attempts reconnection; no independent outgoing background reconnect/retry loop is supplied.

## MQTT-011-AC-03 — Connection wait cancellation does not imply publication
**Given** connection establishment is serialized and another caller waits for the lock or cancellable connect I/O, **When** its attempt token is cancelled, **Then** waiting/I/O can cancel without manufactured publication or durable completion. An already-connected fast-path result still relies on subsequent PublishAsync's cancellation handling.

## MQTT-011-AC-04 — Disposal releases owned client without promised graceful disconnect
**Given** the outgoing connection owns its semaphore/client, **When** DI disposes it after host work stops, **Then** both are disposed; there is no explicit outgoing graceful-disconnect operation or promise that racing active publications are drained. Concurrent-use/disposal and full client thread safety remain [GAP-TX-006](../GAPS.md#gap-tx-006).
