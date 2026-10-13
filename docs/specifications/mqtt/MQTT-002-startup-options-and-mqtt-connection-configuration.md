# MQTT-002 — Startup options and MQTT connection configuration

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** inbox [options](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Configuration/MqttInboxOptions.cs), [validator](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Configuration/MqttInboxOptionsValidator.cs), [worker](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs); outbox [options](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/MqttOutboxOptions.cs), [validator](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/MqttOutboxOptionsValidator.cs), [connection](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Connection/MqttConnection.cs).

## MQTT-002-AC-01 — Invalid operational options prevent valid startup configuration
**Given** blank Host/ClientId, nonpositive Port, QoS 0/unknown enum, or inbox zero expiry/nonpositive or excessive reconnect delay, **When** registered startup validation evaluates options, **Then** it reports the relevant failures instead of treating those values as a valid operational configuration; multiple invalid values can yield multiple messages.

## MQTT-002-AC-02 — Validator boundary is distinct from broker validity
**Given** positive Port 65536 and, for inbox, reconnect delay exactly TimeSpan.FromMilliseconds(uint.MaxValue - 1) and positive expiry, **When** adapter validators run with all other values valid, **Then** they accept those values; one millisecond beyond the delay bound is rejected. Validator acceptance does not claim that the networking stack/broker accepts that port or delay operationally.

## MQTT-002-AC-03 — Defaults remain adapter-specific
**Given** options with Host/ClientId supplied and other properties untouched, **When** option values and resulting connection/publication configuration are inspected, **Then** the defaults match the inventory above, both connections explicitly use MQTT 5, and outgoing configuration does not acquire inbox session-expiry/reset/delay settings.

## MQTT-002-AC-04 — Credentials and TLS are configured without implicit security claims
**Given** a password with blank Username, **When** connection options are built, **Then** credentials are not configured. **Given** nonblank Username and UseTls=true with Port=1883, **When** connection options are built, **Then** username/password and MQTTnet TLS are enabled without silently changing port or supplying custom certificate rules; successful secure deployment still needs host/broker validation.
