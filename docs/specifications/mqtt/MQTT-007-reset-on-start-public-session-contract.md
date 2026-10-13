# MQTT-007 — Reset-on-start public session contract

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Normative basis:** public XML documentation on [MqttInboxOptions.ResetSessionOnStart](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Configuration/MqttInboxOptions.cs): discards broker-held session on the first successful connection after process starts; subsequent reconnects remain persistent.
**Source evidence:** [worker Connect/CreateOptions](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs). Broker actual session storage/expiry are outside the adapter's guarantee.

## MQTT-007-AC-01 — Reset happens on first successful connection only
**Given** ResetSessionOnStart=true and a first refused connection followed by a successful connection, **When** later reconnects occur, **Then** failed initial attempts keep requesting clean start, the first successful connection requests clean start, and subsequent reconnects request persistent session even if subscriptions failed after that first success.

## MQTT-007-AC-02 — Default startup does not request a session reset
**Given** ResetSessionOnStart=false, **When** startup and later reconnect options are sent, **Then** clean start remains false and configured expiry/client identity are sent; whether prior broker state exists is not inferred from these options.
