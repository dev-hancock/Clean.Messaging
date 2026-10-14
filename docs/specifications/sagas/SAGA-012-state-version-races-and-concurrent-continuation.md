# SAGA-012 — State version races and concurrent continuation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Workflow authors need optimistic concurrency so competing continuations cannot both overwrite the same observed state or reopen a terminal winner.

The standards explicitly require version fencing, transaction consistency and owner-managed retries for saga state. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- A committed transition MUST advance its observed version once; stale competing transitions MUST not overwrite a committed winner.
- Rejected durable ownership MUST leave participating candidate state/effects uncommitted; retry MUST remain the delivery owner’s responsibility.

## Contract boundaries

Version is a state-concurrency counter, not a lease or schema version. Competing handlers may execute before save; external side effects are outside optimistic rollback.

## Acceptance criteria

### SAGA-012-AC-01 — One applied transition advances version

**Given** one active transition is based on version V.
**When** the owner commits it.
**Then** the durable version is V+1 with that transition’s state/outcome and UpdatedAt; a new instance starts at version zero and its first applied handler transition advances it.

### SAGA-012-AC-02 — Concurrent continuations fence committed state

**Given** two contexts prepare continuations from the same version.
**When** one transition wins the save race.
**Then** optimistic concurrency rejects the stale competing commit rather than overwriting the winner; handlers may both have run, and the losing participating writes require owner rollback/recovery.

### SAGA-012-AC-03 — Terminal race cannot reopen state

**Given** a competing winner makes the workflow terminal.
**When** a stale active candidate attempts to commit.
**Then** the candidate cannot reopen the terminal winner.

### SAGA-012-AC-04 — Retries belong to processing owner

**Given** a candidate fails concurrency persistence.
**When** recovery later retries the durable delivery.
**Then** the later eligible owner attempt observes then-current durable state; fresh processing belongs to the owning delivery policy, not an independent saga reload-and-repeat loop inside the failed transition.

### SAGA-012-AC-05 — Rejected ownership undoes local workflow commit

**Given** the durable owner cannot authorize completion of a candidate transition.
**When** the owner rolls back.
**Then** candidate workflow state, outgoing/timer effects and completion do not commit as a successful owned transition; nontransactional handler effects may already exist.

## Open decisions

None.
