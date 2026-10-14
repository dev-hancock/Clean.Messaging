# DOMAIN-001 — Raise pending events without owning business policy

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Application aggregates need to record the facts their own business rules select, so persistence can capture those facts later. Raising records pending work; it does not execute consumers or substitute library policy for the application’s domain.

## Guarantees

- Each valid Raise MUST record a separate pending fact with a generated nonempty event identity and the supplied payload, preserving raising order.
- Raise MUST reject a null payload without adding a fact. Raising MUST NOT invoke consumers, persist state or publish externally.

## Contract boundaries

This contract belongs to the supplied AggregateRoot. Its protected Raise operation and IDomainEventSource view expose pending facts, not an event store, entity key or business-state model. Payload references remain application-owned; immutable domain snapshots are not created automatically. Custom source validation and the lifetime of a previously obtained collection view are separate extension-contract questions.

## Acceptance criteria

### DOMAIN-001-AC-01 — Record separate raised facts

- **Given** an aggregate raises the same valid payload twice.
- **When** the application reads its pending events.
- **Then** two pending facts have separate generated nonempty IDs, the supplied payload references and raising order; raising has invoked no consumer.

### DOMAIN-001-AC-02 — Reject null raised payload

- **Given** an aggregate attempts to raise a null payload.
- **When** Raise is called.
- **Then** the call throws and its pending events remain unchanged.

## Open decisions

These original identifiers and heading anchors remain reserved, not normative scenarios. No replacement IDs are assigned before the underlying policy and identity migration are decided.

### DOMAIN-001-AC-03 — Expose live pending collection

Reserved — [GAP-DUR-010](../GAPS.md#gap-dur-010). Whether a previously obtained Events view must stay live rather than become a snapshot remains reserved. Neither interface collection shape nor single-owner state establishes that lifetime promise.
