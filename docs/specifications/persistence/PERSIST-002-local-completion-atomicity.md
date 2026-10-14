# PERSIST-002 — Local completion atomicity

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Local delivery must keep participating effects consistent with the completion evidence used by recovery. A target must neither complete without its local changes nor commit those changes while leaving the same target unresolved.

## Guarantees

- Successful local delivery MUST commit participating configured-context changes and owned target completion together.
- A failed consumer or rejected stale completion MUST NOT authorize committing that attempt’s local changes as a successful result.
- Rollback of the local attempt MUST NOT be represented as reversal of an independently committed external effect.

## Contract boundaries

This contract applies to correctly composed local inbox/outbox dispatch in the same scoped relational context and transaction. Another context, database or service is not a participant. Handler invocation can repeat. A failed or cancelled commit notification does not prove rollback: an ambiguous result may still have committed both participating effects and completion, never a permitted partial result.

## Acceptance criteria

### PERSIST-002-AC-01 — Commit effect and completion

- **Given** a locally dispatched consumer changes configured-context state while owning its target.
- **When** delivery succeeds and commits.
- **Then** an independent reader observes both the participating business change and completed target.

### PERSIST-002-AC-02 — Handler fails before commit

- **Given** a local consumer changes configured-context state and then fails before commit.
- **When** that attempt rolls back.
- **Then** neither its business change nor successful target completion is persisted.

### PERSIST-002-AC-03 — Stale completion

- **Given** a local consumer changed participating state but no longer has valid ownership.
- **When** its completion is rejected.
- **Then** the attempted local changes do not commit as a successful delivery.

### PERSIST-002-AC-04 — External effect excluded

- **Given** a consumer committed an external operation independently and its later local save fails.
- **When** the local transaction rolls back.
- **Then** local state/completion remain unsuccessful, without any claim that the external operation was undone.

## Open decisions

- [GAP-CORE-005](../GAPS.md#gap-core-005): the related policy remains undecided; its recommendation is not approved.
