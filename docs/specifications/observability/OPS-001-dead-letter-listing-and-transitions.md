# OPS-001 — Dead-letter listing and transitions

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** DeadLetterStore lists non-discarded dead letters and mutates only that state; public snapshot fields.

Pre: retained non-discarded dead letter. Post: requeue resets attempt budget/eligibility while retaining identity/failure; discard hides listing but does not immediately delete.

## OPS-001-AC-01 — List projection
**Given** dead letters, discarded and successful entries exist.
**When** positive limited listing runs.
**Then** only non-discarded dead letters appear with Key/EventId/time/Attempts/Code/Error, ordered by dead-letter time without a tie-order promise.

## OPS-001-AC-02 — Requeue reset
**Given** a retained non-discarded dead letter has previous failure diagnostics.
**When** Requeue succeeds.
**Then** it becomes unresolved/immediately eligible with Attempts zero, cleared ownership/dead-letter status and unchanged identity/payload/failure history.

## OPS-001-AC-03 — Discard visibility
**Given** a non-discarded dead letter.
**When** Discard succeeds.
**Then** it is absent from dead-letter listing and retains a stored discarded outcome until cleanup.

## OPS-001-AC-04 — Ineligible mutation
**Given** a key is absent, already discarded or successful.
**When** Requeue or Discard runs.
**Then** the normal operation returns false and does not reset successful work.

## OPS-001-AC-05 — Independent mutation restriction
**Given** an ambient TransactionScope is active.
**When** standard Requeue or Discard is requested.
**Then** InvalidOperationException rejects the unsupported ambient boundary rather than silently suppressing it.
