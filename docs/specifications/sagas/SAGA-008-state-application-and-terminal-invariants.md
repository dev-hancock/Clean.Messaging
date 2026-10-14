# SAGA-008 — State application and terminal invariants

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Workflow authors need one candidate transition from active state with explicit terminal and concurrency invariants, not state mutation merely from constructing a result.

The saga state/version/terminal standards and public declarative result outcomes establish the applied-state boundary independently of internal invocation choreography. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- An accepted active transition MUST preserve its candidate state/outcome, advance version once and record its update instant.
- A stale expected version or terminal workflow MUST not accept replacement active state.

## Contract boundaries

Candidate application is still staged until the owner commits. Handler mutation outside this context or external side effects are excluded; persistence concurrency remains an additional fence.

## Acceptance criteria

### SAGA-008-AC-01 — Active result applies one candidate state transition

**Given** an active instance at expected version has a valid candidate state/result.
**When** that transition is accepted and committed.
**Then** serialized state/outcome are durable, its version advances once and its update timestamp represents the applied transition in UTC; an effect failure still requires owner rollback rather than a partial successful transition.

### SAGA-008-AC-02 — Rejected expected version cannot overwrite state

**Given** a candidate was based on a version no longer current.
**When** state replacement is attempted.
**Then** stale candidate state/outcome/effects are rejected and cannot overwrite current durable state; database optimistic concurrency remains the separate commit guard.

### SAGA-008-AC-03 — Terminal state cannot accept an active transition

**Given** the workflow is already terminal.
**When** an active candidate transition is attempted.
**Then** terminal state is not reopened or replaced by that candidate.

### SAGA-008-AC-04 — Result construction is not state mutation

**Given** a result/effect description is constructed but no owner transition commits.
**When** persisted workflow state is inspected.
**Then** description construction alone does not mutate durable state, save it or publish effects.

## Open decisions

None.
