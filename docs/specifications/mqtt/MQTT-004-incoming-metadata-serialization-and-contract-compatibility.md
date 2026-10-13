# MQTT-004 — Incoming metadata, serialization and contract compatibility

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** [ingress handler](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs), [reserved names](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Routing/MqttHeaders.cs), [generic ingress](../../../src/Clean.Messaging.Inbox.Ingress/TransportIngress.cs), [inbox factory](../../../src/Clean.Messaging.Inbox.Ingress/InboxMessageFactory.cs).

Reserved exact-case names are `message-id`, `event-id`, `contract`, `correlation-id`, `causation-id`. Lookup reads the last property with that exact name. Message ID is required indirectly by generic ingress; all present IDs must parse as nonempty GUIDs. Event/correlation/causation are optional. Declared contract is optional but, when present, must ordinally equal the selected payload type's registered contract.

## MQTT-004-AC-01 — Valid peer metadata admits stable application identities
**Given** a compatible payload on one route with valid `message-id`, distinct `event-id` and valid optional context IDs, **When** MQTT ingress succeeds, **Then** durable admission contains the mapped consumer message with those identities; broker packet ID, QoS, duplicate and retain flags do not replace them.

## MQTT-004-AC-02 — Optional declaration and event identity have defined absence semantics
**Given** valid message identity and payload but no `contract` or `event-id` property, **When** ingress admits it, **Then** the route's registered payload contract selects deserialization and durable event identity defaults to message identity. A missing route payload contract is a runtime contract/admission failure, not an automatically inferred CLR name.

## MQTT-004-AC-03 — Malformed identity and mismatched contract are poison
**Given** absent message identity, a present malformed/empty GUID in any reserved ID, or a declared contract unequal by case/value to the selected registered contract, **When** MQTT ingress evaluates it, **Then** poison is raised before successful durable admission; invalid present metadata is not treated as absent or replaced with a fresh identity.

## MQTT-004-AC-04 — Last exact-name property wins
**Given** repeated `message-id`/`contract` properties and case-distinct `Message-Id`, **When** metadata is read, **Then** the last exact lowercase reserved property determines validation/admission and the case-distinct property does not override it. A malformed last reserved ID is poison even when an earlier one was valid.

## MQTT-004-AC-05 — Payload segmentation and Content Type do not alter decoding policy
**Given** identical payload bytes delivered as one segment and multiple segments with a nondefault Content Type and nonreserved UTF-8 properties, **When** ingress processes them, **Then** both yield the same mapped message bytes/value; Content Type and nonreserved headers enter the intermediate envelope only, do not select another serializer and are not persisted by built-in generic ingress.
