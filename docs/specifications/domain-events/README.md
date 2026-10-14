# Domain events capability catalogue

[Specification index](../INDEX.md) · [Documentation index](../../README.md)

## Purpose

Application authors use this capability to expose pending facts selected by their domain and capture them as outgoing work in the owning local transaction. The library supplies source mechanics and capture integration, not business invariants, synchronous domain dispatch or event sourcing.

## Capability lifecycle

Raising creates in-memory pending facts with logical event identities. A correctly composed tracked-source save stages concrete-contract, per-target outbox work. Successful local commit permits acknowledgement of captured identities; later outbox delivery has its own target completion, retry and external repeat boundaries. An acknowledgement is never a receipt from consumers or a broker.

## Shared contract boundaries

AggregateRoot supplies identity-based pending-event mechanics; IDomainEventSource permits application-defined sources. The host maps its entities and separately installs outbox services, models and both interceptors. Context/source state remains application-owned and single-owner during capture. Retained outbox targets survive process loss; uncaptured pending lists do not. Retention is not a domain history or permanent duplicate ledger.

The [coding standards](../../CODING_STANDARDS.md) establish ownership, identity, transaction, failure and resource obligations. Intentional public operation and persisted-contract compatibility further constrain these contracts. Reference pages describe implementation; they do not independently approve unresolved policy. Each numbered page separates required outcomes from reserved original obligations.

## Open decisions

Custom source collisions, collection-view/stability and acknowledgement failures remain in [GAP-DUR-010](../GAPS.md#gap-dur-010) and [GAP-DUR-009](../GAPS.md#gap-dur-009); targetless capture remains in [GAP-DUR-011](../GAPS.md#gap-dur-011). Their recommendations are not approved.

## Specifications

- [DOMAIN-001 — Raise pending events without owning business policy](DOMAIN-001-raise-pending-events-without-owning-business-policy.md)
- [DOMAIN-002 — Acknowledgement removes pending facts by identity](DOMAIN-002-acknowledgement-removes-pending-facts-by-identity.md)
- [DOMAIN-003 — Opt-in EF capture composition](DOMAIN-003-opt-in-ef-capture-composition.md)
- [DOMAIN-004 — Capture tracked sources across entity states](DOMAIN-004-capture-tracked-sources-across-entity-states.md)
- [DOMAIN-005 — Atomic captured facts and application commit](DOMAIN-005-atomic-captured-facts-and-application-commit.md)
- [DOMAIN-006 — Defer acknowledgement and deduplicate multi-save capture](DOMAIN-006-defer-acknowledgement-and-deduplicate-multi-save-capture.md)
- [DOMAIN-007 — Invalid custom facts and targetless acknowledgement](DOMAIN-007-invalid-custom-facts-and-targetless-acknowledgement.md)
- [DOMAIN-008 — Custom source collisions and acknowledgement failures](DOMAIN-008-custom-source-collisions-and-acknowledgement-failures.md)
- [DOMAIN-009 — Crash, recovery and retention boundaries](DOMAIN-009-crash-recovery-and-retention-boundaries.md)
- [DOMAIN-010 — Event-source and acknowledgement contract](DOMAIN-010-undecided-source-and-acknowledgement-contract.md)

## Public contract references

[AggregateRoot](../../../src/Clean.Messaging.DomainEvents/AggregateRoot.cs) and [RaisedEvent/IDomainEventSource](../../../src/Clean.Messaging.DomainEvents/RaisedEvent.cs) define source mechanics. [DomainEventCapture](../../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs) connects those sources to the owning outbox’s save/capture lifecycle. The [package contract map](../../reference/contract-map.md) supplies declaration lookup, not extra behavioral guarantees.

### Shared dependency contracts

Contract serialization, consumer/target resolution and durable store ownership retain their adjacent capability contracts; see the [outbox catalogue](../outbox/README.md).

## Contract limits

Local capture, downstream delivery and retention are distinct boundaries. Open source/callback policies do not authorize stronger delivery claims.

### Related references

- [Repository instructions](../../../.github/copilot-instructions.md)
- [coding standards](../../CODING_STANDARDS.md)
- [documentation index](../../README.md)
- [architecture](../../reference/architecture.md)
- [configuration](../../reference/configuration.md)
- [contracts](../../reference/contracts.md)
- [inbox](../../reference/inbox.md)
- [outbox](../../reference/outbox.md)
- [domain events](../../reference/domain-events.md)
- [persistence](../../reference/persistence.md)
- [processing](../../reference/processing.md)
- [transport](../../reference/transport.md)
- [capability boundaries](../../explanation/capability-boundaries.md)
- [message identity](../../explanation/message-identity.md)
- [transaction boundaries](../../explanation/transaction-boundaries.md)
- [delivery semantics](../../explanation/delivery-semantics.md)
- [durable workflows](../../explanation/durable-workflows.md)
- [package contract and source map](../../reference/contract-map.md)
- [src\Clean.Messaging\Consumers\ConsumerRegistry.cs](../../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs)
- [src\Clean.Messaging\Serialization\MessageSerializer.cs](../../../src/Clean.Messaging/Serialization/MessageSerializer.cs)
- [src\Clean.Messaging\Persistence\IDurableStore.cs](../../../src/Clean.Messaging/Persistence/IDurableStore.cs)
- [src\Clean.Messaging\Persistence\IDurableEntry.cs](../../../src/Clean.Messaging/Persistence/IDurableEntry.cs)
- [src\Clean.Messaging.EntityFrameworkCore\DurableStore.cs](../../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)
- [src\Clean.Messaging.EntityFrameworkCore\PersistenceServices.cs](../../../src/Clean.Messaging.EntityFrameworkCore/PersistenceServices.cs)
