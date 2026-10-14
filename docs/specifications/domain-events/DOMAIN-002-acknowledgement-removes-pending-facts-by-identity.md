# DOMAIN-002 — Acknowledgement removes pending facts by identity

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Applications need to remove only the facts whose capture has been acknowledged, without losing unrelated pending facts. Identity-based acknowledgement keeps capture bookkeeping separate from business state and downstream delivery.

## Guarantees

- AggregateRoot acknowledgement MUST remove pending facts matching the acknowledged event ID and preserve all other pending IDs and payloads.
- Acknowledging an absent, empty or already acknowledged ID MUST leave the pending collection unchanged. Acknowledgement MUST NOT create a new fact.

## Contract boundaries

The guarantees apply to AggregateRoot, not arbitrary implementations of IDomainEventSource. An EF capture acknowledgement means local capture commit, not consumer completion or transport receipt. The timing of that acknowledgement is specified in DOMAIN-005 and DOMAIN-006; custom-source semantics remain a separate decision.

## Acceptance criteria

### DOMAIN-002-AC-01 — Remove matching identity without changing others

- **Given** several aggregate events are pending.
- **When** one existing event ID is acknowledged.
- **Then** matching facts are absent and every other pending ID and payload remains unchanged.

### DOMAIN-002-AC-02 — Make absent acknowledgement a no-op

- **Given** the acknowledged ID is unknown, empty or already removed.
- **When** AggregateRoot processes the acknowledgement.
- **Then** its remaining pending facts are unchanged and no new fact is created.

## Open decisions

None.
