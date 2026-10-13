# DOMAIN-002 — Acknowledgement removes pending facts by identity

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: AggregateRoot explicit IDomainEventSource.Acknowledge and RaisedEvent. Interface supplies an operation, not XML requirements for custom implementations.

Contracts: Acknowledge(Guid) and Events; postcondition for AggregateRoot: all matching IDs removed, other facts remain, absent/previously acknowledged IDs no-op. Acknowledgement timing supplied by EF integration means capture committed, not consumers/transport complete. Custom sources can implement different mechanics and must be source-reviewed separately.

## DOMAIN-002-AC-01 — Remove matching identity without changing others

- **Given** several pending aggregate events exist.
- **When** one existing ID is acknowledged.
- **Then** that ID is absent and other payload/IDs remain pending.

## DOMAIN-002-AC-02 — Make absent acknowledgement a no-op

- **Given** an unknown, empty or already acknowledged Guid.
- **When** AggregateRoot is acknowledged.
- **Then** remaining pending events are unchanged and no new fact is created.
