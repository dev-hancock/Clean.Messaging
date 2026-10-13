# OUTBOX-012 — Delivery registration and local atomic dispatch

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: Outbox core AddOutboxTransport<TTransport>, OutboxDispatcher and EF LocalOutboxDelivery; local transaction outcome is constrained by OUTBOX-005 and repository atomicity requirements.

Contracts: AddOutboxTransport<TTransport> registers scoped concrete transport and wrapper delivery; no broker host is automatically installed by that port. Dispatcher transport lookup ordinal; local name "local". Local dispatcher fresh scope per EF strategy attempt, retained completion precheck, configured-context handler, fenced completion, SaveChanges and commit. False completion rolls back. Catch attempts rollback with None and preserves original failure including uncertain commit; handlers/external side effects can repeat during strategy retry.

## OUTBOX-012-AC-01 — Reject ambiguous or missing transport selection

- **Given** deliveries have duplicate Transport names or a row stores an unregistered/case-different name.
- **When** dispatcher construction or Dispatch runs.
- **Then** duplicate construction or missing exact ordinal lookup throws rather than selecting an arbitrary route.

## OUTBOX-012-AC-02 — Commit or roll back local target effects

- **Given** a local target changes configured-context state and stages further work.
- **When** dispatch commits, handler fails or fenced completion returns false.
- **Then** success commits state/completion/new outbox together; failure or false rolls back that local transaction.

## OUTBOX-012-AC-03 — Avoid repeat of retained completed local target

- **Given** local transaction committed but success observation was lost.
- **When** dispatch runs again with retained ProcessedAt.
- **Then** it reports completed without reinvoking local effects; cleaned records or external effects are outside this retained-completion protection.

## OUTBOX-012-AC-04 — Use registered external wrapper

- **Given** AddOutboxTransport<TTransport> is composed with a custom scoped transport and a stored matching target.
- **When** dispatch resolves the delivery.
- **Then** the registered transport receives its persisted envelope and successful publication is followed by separate completion.
