# MQTT-010 — Outgoing packet metadata and retain semantics

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** [MqttMessageFactory](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttMessageFactory.cs), [reserved names](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttMessageMetadata.cs), [transport name](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttTransport.cs), [dispatch factory](../../../src/Clean.Messaging.Outbox.Dispatch/TransportMessageFactory.cs).

## MQTT-010-AC-01 — Packet includes stored route destination, payload and application identities
**Given** valid mapped transport data, stored topic, route retain and validated outgoing QoS, **When** the packet is constructed/published, **Then** topic/payload/QoS/retain match those inputs and UTF-8 reserved user properties contain required nonempty message/event GUIDs in D format and the mapped payload contract. Retain defaults false and is unrelated to durable retention.

## MQTT-010-AC-02 — Reserved metadata cannot be overridden by caller headers
**Given** a transport message with exact reserved-name header attempts plus duplicate nonreserved headers and nonblank Content Type, **When** the packet is constructed, **Then** factory-owned reserved properties prevail, nonreserved properties are appended in UTF-8, duplicate nonreserved values remain representable and nonblank Content Type is included. Case-distinct names are not reserved aliases.

## MQTT-010-AC-03 — Missing required IDs or destination reject outgoing construction
**Given** absent/empty required message/event ID or blank destination, **When** packet construction runs, **Then** permanent `OutboxDispatchException` is raised and no successful packet publication is reported. Packet construction follows connected-client acquisition in the current transport path, so connection attempts are not excluded by this failure.

## MQTT-010-AC-04 — Optional empty IDs expose current peer incompatibility
**Given** transport correlation/causation set to Guid.Empty, **When** the outgoing factory constructs metadata, **Then** it emits their D-format empty values without separate validation; an incoming adapter receiving that present value classifies it as poison. Absent optional IDs are omitted. See [GAP-TX-005](../GAPS.md#gap-tx-005); this is not a requirement for interoperable empty IDs.
