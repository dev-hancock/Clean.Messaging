# CLAIM-001 — Claim eligibility and attempt accounting

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** DurableStore eligibility and claim mutation predicates; descriptive processing/persistence reference.

Pre: requested limit/lease are valid. Post: only unresolved due and unowned/expired work is claimed; claim count is bounded and Attempts increases on claim, not invocation.

## CLAIM-001-AC-01 — Eligible boundaries
**Given** unresolved entries include next-attempt equal to now and lease-expiry equal to now.
**When** Claim runs at now.
**Then** boundary-equal due/expired entries are eligible while future-due or unexpired-owned entries are excluded.

## CLAIM-001-AC-02 — Terminal exclusion
**Given** completed, dead-lettered and discarded entries coexist with unresolved work.
**When** Claim runs.
**Then** terminal entries are excluded even if old lease/due fields would otherwise qualify.

## CLAIM-001-AC-03 — Claim count
**Given** more eligible entries than a positive limit exist.
**When** Claim runs.
**Then** no more than limit entries are returned, with persisted ownership and one increased Attempts count per returned entry.

## CLAIM-001-AC-04 — Abandoned claim
**Given** an entry is claimed but no handler starts before expiry.
**When** a later claim succeeds.
**Then** Attempts accounts for both claims even though the abandoned attempt had no invocation.

## CLAIM-001-AC-05 — No absolute claim cap
**Given** an unresolved entry repeatedly expires without failure settlement even after MaxAttempts.
**When** Claim runs when due/available.
**Then** MaxAttempts alone does not prevent another claim; the failure budget is evaluated on settlement.
