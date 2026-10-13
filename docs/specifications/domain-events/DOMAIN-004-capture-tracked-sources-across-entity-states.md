# DOMAIN-004 — Capture tracked sources across entity states

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: DomainEventCapture.Capture(DbContext) snapshots tracked source entities and directly enumerates each Events collection; tracker.Capture<DomainEventCapture>. [GAP-DUR-010](../GAPS.md#gap-dur-010) covers source assumptions.

Contracts: any tracked IDomainEventSource, not only AggregateRoot. Preconditions: stable valid source collection during save. Added/Modified/Unchanged/Deleted tracked sources are included; detached/untracked sources excluded. Each source event is transformed through OutboxMessage<object> but serialized by concrete runtime payload type. Captures deduplicate (DomainEventCapture type,eventId) across all sources in that context/pending transaction. Source enumeration has no event-list snapshot/lock, payload immutability check or unique-ID validation.

## DOMAIN-004-AC-01 — Capture unchanged and custom tracked sources

- **Given** tracked custom/aggregate sources in Added, Modified, Unchanged or Deleted state contain valid events and untracked sources also contain events.
- **When** an intercepted owning save runs.
- **Then** tracked sources contribute captures independent of entity state; untracked sources do not.

## DOMAIN-004-AC-02 — Serialize concrete event payload

- **Given** a custom RaisedEvent holds a registered concrete payload through object.
- **When** capture stages and owning save commits.
- **Then** outgoing contract/payload is concrete, event Id becomes EventId and target batch has a new delivery identity.

## DOMAIN-004-AC-03 — Keep empty collections inert

- **Given** tracked sources have empty Events collections.
- **When** an owning save commits.
- **Then** no domain outgoing entries or domain acknowledgements are created solely because a source is tracked.

## DOMAIN-004-AC-04 — Do not authorize concurrent source mutation

- **Given** a live Events collection or payload mutates concurrently with save capture.
- **When** current capture enumerates it.
- **Then** there is no supplied synchronization/snapshot guarantee; enumeration/serialization can fail or capture a source-derived view, not a guaranteed atomic domain snapshot.
