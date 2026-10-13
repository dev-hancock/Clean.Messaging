# OPS-002 — Identity-preserving management

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Evidence:** Coding standards §9 requires preserving retry identity and independently successful targets.

Pre: multiple targets for one logical event, one successful and one dead letter. Post: repairing the failed target does not rewrite successful siblings or turn the same event into a new event.

## OPS-002-AC-01 — Repair one target
**Given** target A completed and B dead-lettered for the same event.
**When** B is requeued.
**Then** A remains completed and B retains its original message/event/target identity.
