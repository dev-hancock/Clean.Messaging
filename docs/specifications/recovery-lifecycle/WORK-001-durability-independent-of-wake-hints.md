# WORK-001 — Durability independent of wake hints

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Evidence:** Coding standards §9 and architecture distinguish durable rows from operational signals; admission/outbox commit and periodic recovery paths.

Pre: committed unresolved work and an operational later host. Post: a lost in-memory signal does not erase accepted work.

## WORK-001-AC-01 — Missed notification
**Given** durable eligible work exists but its process-local wake was lost.
**When** recovery runs with the host/store available.
**Then** work can be discovered and processed without recreating admission or staging.

## WORK-001-AC-02 — Restart recovery
**Given** a process stops with unresolved work or abandoned claims.
**When** a later host starts and expired work is eligible.
**Then** the stored work remains available for another attempt.
