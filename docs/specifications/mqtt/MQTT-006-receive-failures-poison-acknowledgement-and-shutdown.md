# MQTT-006 — Receive failures, poison acknowledgement and shutdown

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** [worker](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs), [handler](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs), dependency [Inbox](../../../src/Clean.Messaging.Inbox/Inbox.cs) and [InboxStore](../../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs).

AutoAcknowledge is disabled. Each receipt resolves one route and a fresh async scope, awaits ingress then explicit acknowledgement with host stopping token. Handler preserves direct token cancellation and translates `InboxIngressException` to poison; it does not translate ordinary durable admission exceptions. No matching consumer contract fails before admission through the runtime consumer registry; an empty batch supplied to the EF store also cannot create a new admission because it lacks a first entry. These are not silently acknowledged poison.

## MQTT-006-AC-01 — Acknowledgement failure requests recovery without erasing admission
**Given** committed admission or already-classified poison, **When** explicit acknowledgement throws a non-shutdown exception, **Then** the worker logs the appropriate normal/poison acknowledgement failure and signals reconnect; previously committed admission stays available, and acknowledgement is not reported as successful by the exception path.

## MQTT-006-AC-02 — Shutdown cancellation leaves unsettled delivery unacknowledged
**Given** admission/acknowledgement is awaiting cancellable work and the host stopping token is cancelled, **When** direct `OperationCanceledException` reaches receive handling, **Then** ordinary admission-failure logging/reconnect classification is suppressed and the delivery is not manufactured as acknowledged. Already committed database work is not automatically rolled back.

## MQTT-006-AC-03 — No consumer is an admission failure rather than automatic poison discard
**Given** valid MQTT payload/identity and mapping but no registered consumer for the mapped runtime message, **When** admission executes, **Then** the runtime consumer failure propagates through the ordinary admission-failure path and no successful acknowledgement is made; reconnect is requested rather than a durable poison record being created.

## MQTT-006-AC-04 — Wrapped mapper cancellation currently reaches poison policy
**Given** a synchronous mapper throws cancellation which generic ingress wraps as `InboxIngressException`, **When** MQTT handler observes that wrapped exception, **Then** it becomes poison and invokes the poison acknowledgement path; this differs from direct admission/shutdown cancellation. See [GAP-TX-002](../GAPS.md#gap-tx-002); this observation is not approval of the policy.
