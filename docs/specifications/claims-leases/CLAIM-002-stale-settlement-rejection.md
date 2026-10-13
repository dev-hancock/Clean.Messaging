# CLAIM-002 — Stale settlement rejection

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Evidence:** Coding standards §8/§9 explicitly requires stale-owner fencing; standard store predicates support exact-key ownership.

Pre: owned-update operation with an exact key and token. Post: current valid owner may settle; old/wrong owner cannot change durable evidence.

## CLAIM-002-AC-01 — Wrong token
**Given** one target is currently owned by B.
**When** A attempts Complete, Retry, DeadLetter or Renew using its obsolete token.
**Then** each update is rejected and B's stored ownership/state is unchanged.

## CLAIM-002-AC-02 — Wrong target
**Given** a valid token exists for one entry.
**When** an update addresses a different unmatched key.
**Then** it does not settle a different target.

## CLAIM-002-AC-03 — Reclaimed target
**Given** an expired target is reclaimed and settled by a successor.
**When** the earlier handler later attempts settlement.
**Then** the earlier attempt cannot overwrite successor completion or diagnostics.
