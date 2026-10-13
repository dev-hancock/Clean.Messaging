# OUTBOX-002 — Ambient outgoing metadata and representation members

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: OutboxEntryFactory.Create, OutboxEnvelope, MessageContext. [Message identity](../../explanation/message-identity.md) explains, not mandates, context inheritance.

Contracts: envelope MessageId/EventId/TargetId/Destination/Message/CorrelationId/CausationId and entry lifecycle members; envelope has no Transport member (delivery selects it before dispatch). Explicit correlation/causation wins; absent correlation inherits context.CorrelationId, absent causation inherits context.MessageId, not context.CausationId. Null cannot explicitly clear an inherited value. These records do not validate their construction independently.

## OUTBOX-002-AC-01 — Inherit or override outgoing context

- **Given** an active delivered MessageId/correlation/causation and an outgoing envelope with supplied or null optional IDs.
- **When** factory creates targets.
- **Then** supplied values win; null correlation inherits current correlation and null causation becomes current delivery MessageId, not its parent causation.

## OUTBOX-002-AC-02 — Carry persisted metadata to external port

- **Given** a stored external target with payload, destination and nullable context IDs.
- **When** the transport wrapper dispatches.
- **Then** IOutboxTransport receives an envelope preserving delivery/event/target/destination/payload/context IDs; arbitrary headers or content type are not added by this core envelope.
