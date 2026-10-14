# SAGA-011 — Owner transactions, rollback, savepoints and replay

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Host composers need a single local owner transaction across workflow state and effects, including failure/replay behavior and unsupported transaction shapes.

The coding standards on atomic local workflow effects, repeatable handlers, uncertain commit and explicit supported transaction ownership establish this boundary. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Saga result application MUST not commit independently of the durable delivery owner.
- A known owner rollback MUST undo participating state/effects/completion; replay MUST not be represented as a once-only handler-call guarantee.

## Contract boundaries

Same-context EF Inbox, local Outbox and Scheduling owners supply the boundary. Ambient changed-saga saves and caller-managed Outbox savepoints are unsupported; cross-capability savepoint recovery remains GAP-WF-006.

## Acceptance criteria

### SAGA-011-AC-01 — No implicit commit from result application

**Given** a valid result is applied without an owning save/commit.
**When** persistence is inspected.
**Then** no implicit saga commit is performed; state/effects remain staged intentions.

### SAGA-011-AC-02 — Inbox and local-outbox owners capture all local state

**Given** Inbox or local-Outbox owner, saga state, outgoing/timer effects and any participating business changes share a scoped context/transaction.
**When** the owner commits a successful transition.
**Then** owner completion and all participating local changes become durable together; another context or network effect is outside this guarantee.

### SAGA-011-AC-03 — Failed effect rolls back earlier applied local changes

**Given** a later effect fails after earlier state/effects were staged in the owner transaction.
**When** the owner is known to roll back.
**Then** earlier participating changes and completion do not remain committed as a partial transition.

### SAGA-011-AC-04 — Execution strategy may repeat handler

**Given** persistence recovery replays a delivery after failed or uncertain execution.
**When** the handler runs again.
**Then** repeat execution is permitted; local rollback guarantees do not promise exactly-once handler calls or revoke external effects.

### SAGA-011-AC-05 — Ambient changed-saga save is unsupported

**Given** changed saga state is saved under an ambient transaction.
**When** the save is attempted.
**Then** that unsupported transaction shape is rejected instead of accepted as a supported owner commit; this saga hook does not reject an unrelated save with no changed sagas.

### SAGA-011-AC-06 — Outbox composition rejects caller savepoint operations

**Given** composed Outbox capture is outside its internal save operation.
**When** the caller attempts creation of or rollback to a savepoint.
**Then** caller-managed savepoint operation is rejected; EF internal savepoints during supported saves remain distinct and permitted. This does not imply support for restoring arbitrary partially captured saga/schedule effects.

## Open decisions

Separate boundary questions remain unapproved in the [decision register](../GAPS.md): `GAP-WF-006`. These do not suspend the independently justified guarantees above.
