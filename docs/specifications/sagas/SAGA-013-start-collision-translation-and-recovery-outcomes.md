# SAGA-013 — Start collision translation and recovery outcomes

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Hosts need stable public start conflict/closed outcomes while avoiding fabricated collision classifications from an unrelated database failure.

The deliberate public SagaNotFound/Conflict/Closed/Serialization exceptions and error-code compatibility standards justify their public outcomes; an internal winner-lookup heuristic is not independent authority. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- A proven different-start collision MUST not replace an active winner or reopen a terminal winner.
- Public saga exception codes, recovery actions and exposed identity fields MUST remain compatible.

## Contract boundaries

Proof of collision, same-start race code, lookup-error precedence and translation parity are undecided. These guarantees do not authorize translating every DbUpdateException merely because a matching row exists.

## Acceptance criteria

### SAGA-013-AC-02 — Different-start active race is permanent conflict

**Given** a proven competing different-start insertion lost to an active retained type/key winner.
**When** the conflicting start outcome is exposed.
**Then** saga.start.conflict/Fault identifies the type/key and does not replace the winner’s state.

### SAGA-013-AC-03 — Winner terminal race is permanent closed

**Given** a proven different-start insertion lost to a retained winner now terminal.
**When** the conflicting start outcome is exposed.
**Then** saga.start.closed/Fault is not a replay acknowledgement or an instruction to reopen that workflow.

### SAGA-013-AC-06 — Public exception classification is compatible

**Given** public missing-by-ID/type-key, conflict, closed or serialization exceptions.
**When** Code, Action, IsTransient and documented identity fields are read.
**Then** missing is saga.not_found/Retry; conflict is saga.start.conflict/Fault; closed is saga.start.closed/Fault; serialization is saga.serialization.failed/Fault; conflict/closed retain SagaType and Key.

## Open decisions

### SAGA-013-AC-01 — Same-start insertion race is transient

**Reserved — no normative acceptance scenario.** What evidence permits translating a save failure into same-start race, conflict or closed outcomes?

Decision: [GAP-WF-009](../GAPS.md#gap-wf-009) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SAGA-013-AC-04 — Nonqualifying save failures retain original error

**Reserved — no normative acceptance scenario.** What evidence permits translating a save failure into same-start race, conflict or closed outcomes? Should cancellation during winner lookup replace or preserve the original database save failure?

Decision: [GAP-WF-009](../GAPS.md#gap-wf-009), [GAP-WF-010](../GAPS.md#gap-wf-010) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SAGA-013-AC-05 — Sync and async collision paths agree

**Reserved — no normative acceptance scenario.** What evidence permits translating a save failure into same-start race, conflict or closed outcomes?

Decision: [GAP-WF-009](../GAPS.md#gap-wf-009) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
