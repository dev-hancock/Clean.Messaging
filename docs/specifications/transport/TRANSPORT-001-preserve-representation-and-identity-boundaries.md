# TRANSPORT-001 — Preserve representation and identity boundaries

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Normative basis:** coding standards §§3, 9 (contracts and identity), 14; repository instructions require provider separation and preservation of message/event identity, contract names and transaction boundaries.
**Source evidence:** [TransportMessage](../../../src/Clean.Messaging.Transport/TransportMessage.cs), [TransportHeader](../../../src/Clean.Messaging.Transport/TransportHeader.cs), [TransportMapper](../../../src/Clean.Messaging.Transport/TransportMapper.cs), [InboxMessageFactory](../../../src/Clean.Messaging.Inbox.Ingress/InboxMessageFactory.cs), [TransportMessageFactory](../../../src/Clean.Messaging.Outbox.Dispatch/TransportMessageFactory.cs).

## TRANSPORT-001-AC-01 — Distinct delivery and logical identities survive mapping
**Given** valid incoming or stored outgoing work with distinct nonempty message/event IDs and explicit correlation/causation IDs, **When** it crosses the typed mapping boundary, **Then** the resulting inbox admission or transport envelope retains each ID in its original role, rather than substituting the payload's ID or a fresh ID.

## TRANSPORT-001-AC-02 — Retry does not create a new logical operation
**Given** a stored outgoing target whose previous publication outcome is uncertain, **When** it is mapped for another attempt with unchanged retained data and configuration, **Then** the published envelope retains the stored message/event IDs; an attempt identity does not replace either ID.

## TRANSPORT-001-AC-03 — Consumer contracts remain provider-neutral
**Given** a consumer implementing the library consumer contract and a mapper implementing `ITransportMapper<TSource, TDestination>`, **When** the consumer and mapping contracts are inspected/consumed without MQTTnet or EF APIs, **Then** their signatures require neither broker packet nor ORM transaction types; provider-specific mechanics remain at integration boundaries.
