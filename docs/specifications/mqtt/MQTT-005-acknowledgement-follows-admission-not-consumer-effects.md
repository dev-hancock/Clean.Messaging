# MQTT-005 — Acknowledgement follows admission, not consumer effects

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Normative basis:** coding standards §9 requires separating durable admission from dispatch, not acknowledging broker admission failures as successful processing, and preserving explicit poison policy; repository instructions preserve identities and transaction boundaries.
**Source evidence:** [worker Receive](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs), [ingress handler](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs), dependency [InboxStore](../../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs).

## MQTT-005-AC-01 — Successful admission transfers processing responsibility
**Given** valid input and available durable inbox integration with consumers paused, **When** the message is received and durable admission commits, **Then** successful MQTT acknowledgement can occur before consumer completion, but not before admission success; consumers remain independently outstanding local work.

## MQTT-005-AC-02 — Admission failure is not acknowledged as success
**Given** valid route/payload/metadata but unavailable database or failing durable admission, **When** receipt fails admission, **Then** it remains unacknowledged as a successful admission and reconnect is requested; no consumer success or poison discard is invented to hide the persistence fault.

## MQTT-005-AC-03 — Duplicate receipt after lost acknowledgement does not reset work
**Given** admission committed, acknowledgement was lost, and the broker retained session/delivery state and redelivers the same IDs, **When** the duplicate reaches the relational inbox and is acknowledged, **Then** one retained admission/obligation set remains and completed retained consumers are not deliberately reset. This is conditional redelivery, not a promise that the broker retains any particular session.

## MQTT-005-AC-04 — Explicit poison policy stays distinct from admission failure
**Given** unroutable, malformed-metadata or ingress-mapping poison input, **When** receipt classifies it, **Then** the worker logs rejection and attempts acknowledgement without storing an inbox poison/dead-letter record; ordinary durable admission errors do not silently use this discard policy.
