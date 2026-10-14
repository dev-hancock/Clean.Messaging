# SAGA-005 — Start, continuation and duplicate routing matrix

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Workflow authors need replay-safe creation and terminal stability without treating a new start as an update or missing continuation as business completion.

Stable type/key/start identity and terminal-invariant standards, deliberate start/continuation builder semantics and the public NotFound/Conflict/Closed recovery surface establish these outcomes. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- A retained same-start delivery MUST be a replay no-op; a different start MUST not overwrite the active or terminal workflow.
- An active continuation MUST target existing state; a terminal continuation MUST not reopen it, and missing continuation MUST remain distinguishable from completion.

## Contract boundaries

These guarantees use committed retained identity, not two unsaved starts in one scope. General continuation deduplication is the durable owner’s responsibility; there is no exactly-once handler-call guarantee.

## Acceptance criteria

### SAGA-005-AC-01 — Absent start creates initial state

**Given** a valid start mapping targets an absent type/key.
**When** the start is handled successfully.
**Then** initial state comes from its configured factory and a new SagaId records the durable start MessageId and UTC creation/update timestamps; the committed state/outcome reflect the configured handler’s transition.

### SAGA-005-AC-02 — Same start identity is replay no-op

**Given** an active or terminal workflow retains the same StartedByMessageId as an incoming start.
**When** that start is replayed.
**Then** it is a no-op without reinvoking the start handler or changing the retained workflow.

### SAGA-005-AC-03 — Different active start conflicts

**Given** an active workflow retains another start identity for the same type/key.
**When** a different start arrives.
**Then** SagaConflictException reports saga.start.conflict/Fault with the SagaType and Key, and existing state is not replaced.

### SAGA-005-AC-04 — Different terminal start is closed

**Given** a terminal workflow retains another start identity for the same type/key.
**When** a different start arrives.
**Then** SagaClosedException reports saga.start.closed/Fault rather than reopening the workflow or acknowledging an equal replay.

### SAGA-005-AC-05 — Absent continuation is transient missing

**Given** a continuation maps to absent retained state.
**When** it is handled.
**Then** SagaNotFoundException reports saga.not_found/Retry without creating state or effects; a later committed start can make a retry meaningful, but the missing continuation is not business completion.

### SAGA-005-AC-06 — Active continuation can transition

**Given** a continuation maps to an active workflow.
**When** its handler succeeds and its owner commits.
**Then** that existing state/outcome and participating effects reflect the handler result, with the persisted version advancing under the owner boundary.

### SAGA-005-AC-07 — Terminal continuation is ignored

**Given** a continuation maps to a terminal workflow.
**When** it arrives.
**Then** the continuation is ignored without handler execution or reopening terminal state.

### SAGA-005-AC-08 — General duplicate continuation belongs to owner deduplication

**Given** a continuation is redelivered to an active workflow without retained owner deduplication.
**When** the durable owner routes it again.
**Then** general duplicate suppression is not supplied by a per-saga continuation ledger; repeated execution remains possible.

## Open decisions

Separate boundary questions remain unapproved in the [decision register](../GAPS.md): `GAP-WF-012`. These do not suspend the independently justified guarantees above.
