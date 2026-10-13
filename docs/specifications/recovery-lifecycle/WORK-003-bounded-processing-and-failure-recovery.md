# WORK-003 — Bounded processing and failure recovery

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Evidence:** Coding standards §16 requires bounded resources; §7 permits deliberate recovery with original failure semantics.

Pre: configured capacity/concurrency and failing/recovering dependencies. Post: failed infrastructure work does not become falsely completed; processing respects the configured local concurrency budget.

## WORK-003-AC-01 — Concurrency bound
**Given** more queued targets than configured Concurrency and handlers are deliberately held.
**When** work runs.
**Then** no more than the configured number of local processing calls overlap.

## WORK-003-AC-02 — Claim outage recovery
**Given** the store first fails claiming then becomes available.
**When** the live loop continues and its recovery delay passes.
**Then** claiming is retried without claiming that unavailable work already completed.

## WORK-003-AC-03 — Escaped processor failure
**Given** one processing call throws unexpectedly and other stored work remains.
**When** the loop handles the exception.
**Then** the failure is diagnostic and subsequent work can continue; the failed target is not automatically marked completed.

## WORK-003-AC-04 — Cooperative shutdown
**Given** waiting/processing dependencies honor cancellation.
**When** the host stops.
**Then** the loop terminates and disposes owned operation scopes without claiming abandoned targets succeeded.
