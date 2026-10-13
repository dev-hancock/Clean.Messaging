# MQTT-009 — Outgoing target and destination staging

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** [MqttOutboxBuilder](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/MqttOutboxBuilder.cs), [target provider](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Routing/MqttOutboxTargets.cs), [route](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Routing/MqttOutboxRoute.cs), [registry](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Routing/MqttRouteRegistry.cs), dependency [OutboxEntryFactory](../../../src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs).

Target IDs are nonblank, <=200 ASCII characters and ordinally unique within the builder. Destination callbacks are nonnull; runtime return must be nonblank. `MqttOutboxTargets.GetTargets` matches exact concrete runtime message type and evaluates against the original message, not mapped payload. All batch targets, including local consumers, must have unique target IDs. Destination is stored per target; mapping/retain use the route found at dispatch by stored target ID. The default local target provider's missing-consumer lookup can fail even with MQTT routes; registering a MQTT route alone does not prove external-only staging composition.

## MQTT-009-AC-01 — Destination is selected before persistence, mapping later
**Given** an A-to-B outgoing map and a destination callback based on A, **When** A is staged then dispatched after commit, **Then** its stored target contains the destination evaluated from original A, transport `mqtt` and route ID, while published bytes are later mapped B data.

## MQTT-009-AC-02 — Target identity validation and duplicates prevent ambiguous work
**Given** blank/non-ASCII/201-character target ID, repeated completed route ID, null destination callback, or collision with a local consumer ID in one batch, **When** route registration or staging reaches that boundary, **Then** the relevant argument/configuration failure occurs without accepting ambiguous duplicate target work. A valid 200-character ASCII ID is within the adapter's identity boundary.

## MQTT-009-AC-03 — Blank destination fails staging; topic grammar has another boundary
**Given** a destination callback returning whitespace or throwing, **When** its exact-type message is staged, **Then** whitespace yields permanent `OutboxDispatchException` and a callback exception propagates before successful staging. **Given** a nonblank syntactically invalid MQTT publication topic, **When** the target provider evaluates it, **Then** it is not rejected solely by this nonblank check; MQTTnet/broker validation may reject it later.

## MQTT-009-AC-04 — Retries preserve destination but current route can change payload or retain
**Given** staged work with destination X and route ID R, **When** a later deployment changes R's destination callback, map or retain and retries the entry, **Then** the stored destination remains X while the current map/retain are used. Removing R causes permanent route lookup failure; changing target ID does not automatically migrate old retained work.

## MQTT-009-AC-05 — Exact-type target matching does not imply polymorphic publication
**Given** a route registered for base type A but a staged concrete subclass B without its own MQTT route, **When** the MQTT target provider enumerates targets for B, **Then** that A route contributes no MQTT target. Other target providers/runtime consumers determine whether staging otherwise succeeds; absence is not proof of global dispatch success.
