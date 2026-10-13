# MQTT-008 — Inbox reconnect, subscription results and cleanup

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** entire [MqttInboxWorker](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs), including connect/subscription checks, one-slot reconnect semaphore, connection lock, disconnected callback and final cleanup.

## MQTT-008-AC-01 — Each successful connection subscribes all routes again
**Given** configured routes and a successful MQTT connect response, **When** startup or reconnect completes its subscriptions, **Then** all filters are subscribed again regardless of session-present status. Each result must contain exactly one granted QoS 1 or 2 result; granted QoS need not equal requested QoS.

## MQTT-008-AC-02 — Refused connection or rejected subscription uses fixed reconnect loop
**Given** non-success connection result, subscription rejection/QoS 0, or an unexpected subscription result count, **When** connection setup runs, **Then** setup fails, logs a warning and enters disconnect/fixed configured delay/reconnect recovery instead of a successful ready state or durable entry retry loop. A zero/multiple result count is not accepted as valid subscription success.

## MQTT-008-AC-03 — Concurrent failure signals do not queue unbounded reconnects
**Given** a connected inbox client and several concurrent admission failures or unexpected disconnect signals, **When** reconnect is signalled, **Then** requests are coalesced rather than creating unbounded independent pending reconnects; connect/disconnect transitions are serialized. Worker-initiated disconnect does not recursively resignal through the disconnected callback.

## MQTT-008-AC-04 — Shutdown removes callbacks and attempts final disconnect
**Given** the worker owns a connected client, **When** host shutdown ends its loop, **Then** receive/disconnected callbacks are removed, final disconnect is attempted with CancellationToken.None and disconnect exceptions are logged, and the client is disposed. This does not assert a bounded network disconnect duration or durable replay after broker session expiry/reset.
