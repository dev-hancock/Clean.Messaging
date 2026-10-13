# TRANSPORT-007 — External publication and per-target completion are separate

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Normative basis:** coding standards §§8, 9 (inbox/outbox and claims); repository instructions preserve per-target completion, leases and transaction boundaries and prohibit exactly-once claims.
**Source evidence:** dependency [OutboxEnvelope](../../../src/Clean.Messaging.Outbox/Transport/OutboxEnvelope.cs), [transport wrapper](../../../src/Clean.Messaging.Outbox/Transport/OutboxTransport.cs), [dispatcher](../../../src/Clean.Messaging.Outbox/Delivery/OutboxDispatcher.cs), [durable store](../../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs), [processor](../../../src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs).

## TRANSPORT-007-AC-01 — Failed publication does not record successful completion
**Given** a claimed outgoing target, **When** its external transport fails before returning success, **Then** successful completion is not recorded for that target; durable failure handling owns retry/fault classification separately from mapping and publication.

## TRANSPORT-007-AC-02 — Publication success followed by lost settlement can repeat
**Given** a publication that succeeds but whose database completion does not persist, **When** recovery claims the unresolved retained target, **Then** another publication can occur with the same stored identities; the library does not report the first external effect as rolled back or exactly-once.

## TRANSPORT-007-AC-03 — Stale ownership cannot complete another attempt
**Given** an old claim expired and a newer owner acquired the target, **When** the old external attempt returns and tries to settle, **Then** its completion is rejected by claim/lease checks and the new owner's durable state is not completed by that old claim, even though the old publication may already have occurred.

## TRANSPORT-007-AC-04 — Sibling completion is independent
**Given** one completed target and one unresolved target in a retained outgoing batch, **When** the unresolved target retries or faults, **Then** that operation does not reset/reinvoke the completed sibling merely because the batch is not wholly successful; no atomic all-target external effect is asserted.
