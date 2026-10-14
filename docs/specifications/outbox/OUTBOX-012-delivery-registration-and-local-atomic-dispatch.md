# OUTBOX-012 — Delivery registration and local atomic dispatch

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Hosts need unambiguous selection of a target's registered delivery integration. Local delivery must join its participating effects with target completion, while custom external delivery must retain the persisted envelope boundary.

## Guarantees

- Duplicate transport names MUST fail as ambiguous configuration. A missing or case-different name MUST fail target dispatch rather than choose another integration.
- Local delivery MUST commit participating consumer state, newly staged outgoing work and owned target completion together, or commit none of them on a known failed/stale attempt.
- Retained completed local targets MUST report completion without repeating their committed local effects after recovery.
- Registered custom external transports MUST receive their persisted target envelope through the publication boundary; their registration MUST not imply automatic broker-host composition.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Public delivery registration/name compatibility and coding standards sections 6, 7, 8, 9 and 14 establish ordinal persisted identities, visible configuration failures, atomic local effects and completion-evidence recovery.

## Contract boundaries

Transport names use ordinal comparison; persisted names must stay resolvable. Local atomicity requires the configured scoped context and transaction, not another context or external effects. Completion deletion ends this repeat-protection horizon. External publication can repeat and is separately governed by [OUTBOX-011](OUTBOX-011-transport-dispatch-and-external-repeat-window.md).

## Acceptance criteria

### OUTBOX-012-AC-01 — Reject ambiguous or missing transport selection

- **Given** delivery registrations duplicate a transport name, or a stored name is missing/case-different from registrations.
- **When** configuration builds or that target is dispatched.
- **Then** ambiguous names fail configuration and absent/case-different lookup fails visibly without selecting a different transport.

### OUTBOX-012-AC-02 — Commit or roll back local target effects

- **Given** a valid local target changes participating state and stages further outgoing work, then either commits or fails/loses settlement authority with a known outcome.
- **When** local delivery finishes.
- **Then** successful participating state, newly staged outgoing work and target completion commit together; a failed/stale attempt commits none of them, without undoing arbitrary external effects.

### OUTBOX-012-AC-03 — Avoid repeat of retained completed local target

- **Given** a local target's complete result committed and completion evidence remains after a lost response.
- **When** the same target is recovered.
- **Then** it reports completed without repeating its committed local effects.

### OUTBOX-012-AC-04 — Use registered external wrapper

- **Given** a host registers a custom external transport and a durable target names it.
- **When** that target is delivered.
- **Then** the selected registered wrapper passes its persisted envelope to publication, and successful publication requires separate fenced local completion; publication/completion are not one transaction and registration alone does not compose a broker host.

## Open decisions

None.
