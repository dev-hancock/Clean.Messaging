# CLAIM-003 — Owned-state and expiry predicates

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** Shared owned updates require unresolved state, null next attempt, matching token and ClaimedUntil > supplied now.

Pre: standard store. Post: expiry equality is rejected; terminal/retry-waiting work cannot be renewed/settled as if still owned.

## CLAIM-003-AC-01 — Exact expiry
**Given** a current matching claim expires at T.
**When** Renew/Complete/Retry/DeadLetter is attempted at T.
**Then** the update returns false rather than extending or settling the expired claim.

## CLAIM-003-AC-02 — Waiting or terminal target
**Given** matching key/token is supplied but target is retry-waiting, completed, dead-lettered or discarded.
**When** an owned update is attempted.
**Then** it does not change that target as an unresolved in-flight claim.

## CLAIM-003-AC-03 — Successful renewal
**Given** a current unexpired target is renewed to a valid later expiry.
**When** renewal succeeds.
**Then** expiry changes while Attempts, identities and logical completion remain unchanged.

## CLAIM-003-AC-04 — Settlement clears ownership
**Given** a valid owner completes, retries or dead-letters.
**When** the operation succeeds.
**Then** claim/expiry are cleared; completion/dead-letter is terminal, while retry is unresolved at its supplied next due time.
