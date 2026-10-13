# TRANSPORT-004 — Incoming typed admission and metadata loss boundary

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** [IInboxIngress / InboxIngress](../../../src/Clean.Messaging.Inbox.Ingress/InboxIngress.cs), [IngressMap](../../../src/Clean.Messaging.Inbox.Ingress/IngressMap.cs), [TransportIngress](../../../src/Clean.Messaging.Inbox.Ingress/TransportIngress.cs), [InboxMessageFactory](../../../src/Clean.Messaging.Inbox.Ingress/InboxMessageFactory.cs), dependency [Inbox](../../../src/Clean.Messaging.Inbox/Inbox.cs).

## TRANSPORT-004-AC-01 — Typed mapping admits the mapped consumer message
**Given** an A-to-B ingress map, valid A payload bytes, valid delivery identity and configured durable B consumers, **When** `Accept(map, envelope, token)` succeeds, **Then** durable admission contains mapped B message data and the supplied IDs/context; A's transport contract and B's durable contract can differ.

## TRANSPORT-004-AC-02 — Generic ingress trusts the selected pair rather than contract text
**Given** valid A bytes and an A-to-B map but unrelated envelope `Contract` text, **When** generic ingress admission runs without a provider-specific contract check, **Then** it deserializes A and admits B according to the map rather than rejecting solely for the text mismatch.

## TRANSPORT-004-AC-03 — Anonymous or empty identity is rejected
**Given** valid mapped payload but absent or empty `MessageId`, **When** ingress accepts it, **Then** it throws `InboxIngressException` and performs no inbox admission or replacement-ID generation. **Given** a valid message ID and absent event ID, **When** admission succeeds, **Then** durable event identity equals message identity; an explicitly empty event ID instead fails in inbox validation.

## TRANSPORT-004-AC-04 — Transport-only metadata does not become consumer data
**Given** a transport envelope with Content Type and arbitrary headers, **When** the built-in ingress mapping admits its payload, **Then** the mapper receives the payload only, and durable inbox data/context do not persist those headers or Content Type as additional envelope fields.

## TRANSPORT-004-AC-05 — Null envelope or missing map is not accepted
**Given** a null envelope or an unregistered exact ingress map, **When** `IInboxIngress.Accept` is called, **Then** the former fails with argument-null validation and the latter with `InboxIngressException`; no durable admission occurs.
