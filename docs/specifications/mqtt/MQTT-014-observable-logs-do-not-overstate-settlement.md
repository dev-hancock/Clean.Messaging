# MQTT-014 — Observable logs do not overstate settlement

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** source-generated events in [MqttInboxWorker](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs), outgoing source inventory and shared [OutboxProcessor](../../../src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs).

Inbox events: 1 Information connected Host/Port/RouteCount; 2 Warning setup failure Host/Port/exception; 3 Error poison Topic/exception before acknowledgement (wording says already acknowledged); 4 Warning poison ack failure Topic/exception; 5 Error admission/normal ack failure Topic/exception; 6 Debug disconnect failure (wording mentions shutdown also in reconnect cleanup). Outgoing has no dedicated MQTT metrics/logger in the audited adapter; shared durable metrics/logs describe local outcomes, not subscriber effects.

## MQTT-014-AC-01 — Poison logging can precede a failed acknowledgement
**Given** poison input and failing acknowledgement, **When** receive handles it, **Then** event 3 is observable before event 4 and no successful acknowledgement is implied by database/broker evidence; event 3's current wording is misleading rather than proof of success. See [GAP-TX-007](../GAPS.md#gap-tx-007).

## MQTT-014-AC-02 — Connection and admission diagnostics retain their operational boundary
**Given** setup failure, ordinary admission/ack failure, or disconnect cleanup failure, **When** the worker logs it, **Then** the corresponding levels/fields above identify setup, receipt or cleanup rather than claiming consumer completion; outgoing durable completion metrics cannot prove subscriber application effects.
