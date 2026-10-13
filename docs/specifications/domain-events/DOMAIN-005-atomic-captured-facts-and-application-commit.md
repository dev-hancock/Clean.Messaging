# DOMAIN-005 — Atomic captured facts and application commit

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour

**Evidence:** Normative: repository/coding standards require transaction consistency and meaningful consumer-owned domain policy. Source: DomainEventCapture uses owning context IOutboxWriter and OutboxTracker; outbox interceptors. Reference/explanation are descriptive evidence only.

Purpose/invariant: successfully captured outgoing targets and domain/application changes share the owning commit; acknowledgment is not permission to claim completed downstream effects. Preconditions: valid nonempty-target capture and correctly composed context. Boundaries: untracked sources, external effects, other contexts and in-memory pre-save events are outside durable transaction.

## DOMAIN-005-AC-01 — Commit state and event targets atomically

- **Given** tracked source contains valid event with targets and application state changes.
- **When** the owning transaction commits successfully.
- **Then** application changes and captured outgoing rows become durable together; source acknowledgement means capture commit, not consumer/broker completion.

## DOMAIN-005-AC-02 — Do not acknowledge failed or cancelled save

- **Given** capture has staged new event rows but save fails or is cancelled before successful commit observation.
- **When** save failure/cancellation bookkeeping runs.
- **Then** event remains pending and no successful event acknowledgement is invented; newly captured rows are detached for retry.

## DOMAIN-005-AC-03 — Retain pending events on explicit rollback

- **Given** one or more captures saved inside explicit transaction.
- **When** the transaction is observed rolled back.
- **Then** captured outgoing database state is not committed and events are not acknowledged; arbitrary aggregate changes still require application reconciliation.
