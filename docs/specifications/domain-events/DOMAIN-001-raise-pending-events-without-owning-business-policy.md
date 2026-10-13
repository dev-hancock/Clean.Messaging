# DOMAIN-001 — Raise pending events without owning business policy

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: AggregateRoot.Raise<TEvent>, explicit IDomainEventSource.Events/Acknowledge and RaisedEvent record; domain core project is dependency-free. [Domain events](../../reference/domain-events.md) and architecture are descriptive.

Purpose: record in-memory facts selected by a consuming domain. Contracts: public abstract AggregateRoot, protected Raise<TEvent>(notnull), IDomainEventSource.Events and Acknowledge(Guid), RaisedEvent.Id/Message. Preconditions: nonnull raised payload. Postcondition: new nonempty Guid and payload are appended to a live read-only-view collection in raising order. No persistence key, timestamp, event store, business invariant, direct handler execution or event-sourcing protocol is owned here. Custom record/source construction is not validated by the record.

## DOMAIN-001-AC-01 — Record separate raised facts

- **Given** an AggregateRoot subclass raises a valid payload twice.
- **When** pending Events are read through IDomainEventSource.
- **Then** two entries carry separate generated nonempty IDs and payload references in order; no consumer is invoked by Raise.

## DOMAIN-001-AC-02 — Reject null raised payload

- **Given** an AggregateRoot subclass attempts Raise with null.
- **When** Raise runs.
- **Then** it throws and adds no pending event.

## DOMAIN-001-AC-03 — Expose live pending collection

- **Given** a caller obtained the Events read-only view before another Raise or acknowledgement.
- **When** it observes the view afterward.
- **Then** the view reflects current pending events rather than an immutable snapshot; no concurrent mutation safety is promised.
