# SAGA-017 — Retained tombstones, purge boundaries and lifecycle limits

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Operators need separate state-payload and timer-history retention without assuming cleanup repairs business failure or erases retained workflow identity.

The standards on terminal retention, identity evidence and application-owned saga recovery/compensation establish these lifecycle limits; indefinite tombstone duration and exact purge mechanics remain policy decisions. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Payload cleanup MUST exclude active workflows and MUST not by itself free retained type/key identity.
- Timer-history lifetime MUST remain independent of saga payload retention; timer failure MUST not automatically fail, purge or compensate an active workflow.

## Contract boundaries

Payload purge removes state needed for forensic reconstruction. Identity lifetime/restart is GAP-WF-011; exact cutoff, cadence and purge concurrency/timestamps are open. Management snapshots are not repair commands.

## Acceptance criteria

### SAGA-017-AC-05 — Timer history has independent retention

**Given** a retained saga identity or purged state outlives its timer rows.
**When** timer retention deletes eligible scheduled history.
**Then** the saga record does not preserve deleted timer history; scheduling and saga retention have separate evidence horizons.

### SAGA-017-AC-06 — Active failed-timer workflow needs application policy

**Given** an active saga has a failed/exhausted timer.
**When** state cleanup or manager observation runs.
**Then** the timer outcome alone does not purge active state, business-fail or compensate it; business recovery remains application policy.

## Open decisions

### SAGA-017-AC-01 — Strict terminal purge cutoff

**Reserved — no normative acceptance scenario.** What exact cutoff equality, batch ordering, startup cadence and purge version/timestamp semantics are promised?

Decision: [GAP-INTENT-WF-004](../GAPS.md#gap-intent-wf-004) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SAGA-017-AC-02 — Purge retains identity and lifecycle diagnostics

**Reserved — no normative acceptance scenario.** What exact cutoff equality, batch ordering, startup cadence and purge version/timestamp semantics are promised? How long are saga type/key/start tombstones retained, and when can identity be deleted or restarted?

Decision: [GAP-INTENT-WF-004](../GAPS.md#gap-intent-wf-004), [GAP-WF-011](../GAPS.md#gap-wf-011) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SAGA-017-AC-03 — Purge is periodic bounded and recoverable

**Reserved — no normative acceptance scenario.** What exact cutoff equality, batch ordering, startup cadence and purge version/timestamp semantics are promised? Are claim order, group/timer order and manager list order contractual, and what tie-breaks or fairness are promised?

Decision: [GAP-INTENT-WF-004](../GAPS.md#gap-intent-wf-004), [GAP-INTENT-WF-001](../GAPS.md#gap-intent-wf-001) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SAGA-017-AC-04 — Tombstone blocks key restart indefinitely today

**Reserved — no normative acceptance scenario.** How long are saga type/key/start tombstones retained, and when can identity be deleted or restarted?

Decision: [GAP-WF-011](../GAPS.md#gap-wf-011) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
