# Claims, leases and settlement ownership — Behavioural specification

**Status:** Draft; source reviewed 2026-10-13; no acceptance criterion has been executed.
**Scope:** `IDurableStore<T>.Claim/Renew/Complete/Retry/DeadLetter`, `OwnedEntry<T>`, `ClaimRenewal.Run`, `MessageLeaseOptions`, `ClaimId`; scheduling ownership is distinct in scheduling.md.

## Purpose
Allow recoverable temporary ownership of unresolved durable targets and reject stale settlement while explicitly limiting protection to the persistence boundary.

## Contracts, preconditions and postconditions
Pre: unresolved stored target, consistent supplied UTC basis and an appropriate lease. Post: owned updates either change the intended current target or report rejection. Rejected ownership is not ordinary consumer failure and does not identify a unique cause.

## Invariants
- A stale owner must not overwrite a successor's settlement.
- Renewal does not create a logical operation or increment attempt identity.
- Expiry permits recovery but cannot undo previous external effects.

## Failure, cancellation and consistency
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are counted separately as gaps, not executable requirements.

## Limitations
- No forced handler termination, automatic clock synchronization, universal lease sizing or serialized mailbox.
- Supplied future expiry is not validated as an extension; batch claims share an opaque token in the current provider.

## Specifications and acceptance criteria

### CLAIM-001 — Claim eligibility and attempt accounting
**Classification:** Observed behaviour
**Evidence:** DurableStore eligibility and claim mutation predicates; descriptive processing/persistence reference.

Pre: requested limit/lease are valid. Post: only unresolved due and unowned/expired work is claimed; claim count is bounded and Attempts increases on claim, not invocation.

#### CLAIM-001-AC-01 — Eligible boundaries
**Given** unresolved entries include next-attempt equal to now and lease-expiry equal to now.
**When** Claim runs at now.
**Then** boundary-equal due/expired entries are eligible while future-due or unexpired-owned entries are excluded.

#### CLAIM-001-AC-02 — Terminal exclusion
**Given** completed, dead-lettered and discarded entries coexist with unresolved work.
**When** Claim runs.
**Then** terminal entries are excluded even if old lease/due fields would otherwise qualify.

#### CLAIM-001-AC-03 — Claim count
**Given** more eligible entries than a positive limit exist.
**When** Claim runs.
**Then** no more than limit entries are returned, with persisted ownership and one increased Attempts count per returned entry.

#### CLAIM-001-AC-04 — Abandoned claim
**Given** an entry is claimed but no handler starts before expiry.
**When** a later claim succeeds.
**Then** Attempts accounts for both claims even though the abandoned attempt had no invocation.

#### CLAIM-001-AC-05 — No absolute claim cap
**Given** an unresolved entry repeatedly expires without failure settlement even after MaxAttempts.
**When** Claim runs when due/available.
**Then** MaxAttempts alone does not prevent another claim; the failure budget is evaluated on settlement.

### CLAIM-002 — Stale settlement rejection
**Classification:** Required behaviour
**Evidence:** Coding standards §8/§9 explicitly requires stale-owner fencing; standard store predicates support exact-key ownership.

Pre: owned-update operation with an exact key and token. Post: current valid owner may settle; old/wrong owner cannot change durable evidence.

#### CLAIM-002-AC-01 — Wrong token
**Given** one target is currently owned by B.
**When** A attempts Complete, Retry, DeadLetter or Renew using its obsolete token.
**Then** each update is rejected and B's stored ownership/state is unchanged.

#### CLAIM-002-AC-02 — Wrong target
**Given** a valid token exists for one entry.
**When** an update addresses a different unmatched key.
**Then** it does not settle a different target.

#### CLAIM-002-AC-03 — Reclaimed target
**Given** an expired target is reclaimed and settled by a successor.
**When** the earlier handler later attempts settlement.
**Then** the earlier attempt cannot overwrite successor completion or diagnostics.

### CLAIM-003 — Owned-state and expiry predicates
**Classification:** Observed behaviour
**Evidence:** Shared owned updates require unresolved state, null next attempt, matching token and ClaimedUntil > supplied now.

Pre: standard store. Post: expiry equality is rejected; terminal/retry-waiting work cannot be renewed/settled as if still owned.

#### CLAIM-003-AC-01 — Exact expiry
**Given** a current matching claim expires at T.
**When** Renew/Complete/Retry/DeadLetter is attempted at T.
**Then** the update returns false rather than extending or settling the expired claim.

#### CLAIM-003-AC-02 — Waiting or terminal target
**Given** matching key/token is supplied but target is retry-waiting, completed, dead-lettered or discarded.
**When** an owned update is attempted.
**Then** it does not change that target as an unresolved in-flight claim.

#### CLAIM-003-AC-03 — Successful renewal
**Given** a current unexpired target is renewed to a valid later expiry.
**When** renewal succeeds.
**Then** expiry changes while Attempts, identities and logical completion remain unchanged.

#### CLAIM-003-AC-04 — Settlement clears ownership
**Given** a valid owner completes, retries or dead-letters.
**When** the operation succeeds.
**Then** claim/expiry are cleared; completion/dead-letter is terminal, while retry is unresolved at its supplied next due time.

### CLAIM-004 — Renewal loss and cooperative cancellation
**Classification:** Required behaviour
**Evidence:** Coding standards §7/§9 says propagate cancellation and distinguish ownership loss from ordinary faults; ClaimRenewal provides cooperative loss signaling.

Pre: processor has an active ownership renewal loop and observes its tokens. Post: renewal rejection/failure requests handling cancellation; host shutdown is not classified as a permanent message fault.

#### CLAIM-004-AC-01 — Renewal rejected
**Given** an active attempt's next renewal is rejected.
**When** the renewal loop observes rejection.
**Then** the handling attempt receives claim-loss cancellation instead of continued asserted ownership.

#### CLAIM-004-AC-02 — Renewal I/O failure
**Given** renewal throws an unexpected store exception.
**When** the loop handles it.
**Then** claim-loss cancellation is requested and the failure is observable diagnostically.

#### CLAIM-004-AC-03 — Shutdown renewal
**Given** host cancellation stops the renewal loop.
**When** its wait or I/O is cancelled.
**Then** no ordinary renewal fault or new successful settlement is fabricated from intentional shutdown.

#### CLAIM-004-AC-04 — Uncooperative handler
**Given** an old handler ignores cancellation and issues an external request after lease loss.
**When** a successor acquires the target.
**Then** the persistence fence does not promise to undo or prevent the old external effect.

### CLAIM-005 — Lease arithmetic and caller invariants
**Classification:** Unknown behaviour
**Evidence:** Renew accepts an arbitrary claimedUntil; store DateTime.Kind and overflow are not explicitly validated.

Unknown: whether shrinking leases, past expiry, non-UTC instants, overflowing times or empty claim strings are valid provider-level inputs. Normal runtime supplies UTC/later expiry, but a public port contract needs a deliberate decision.

**Open decisions:** [GAP-CORE-004](GAPS.md#gap-core-004).

## Traceability and verification matrix
All rows are **Not tested**. Unit means isolated observable contract verification, not internal-call assertions. Persistence/concurrency verification requires a suitable real relational provider, not EF InMemory. The eventual TUnit suite can combine criteria while retaining their IDs.

| Specification | Criterion | Classification | Verification | Observable assertion | Status |
| --- | --- | --- | --- | --- | --- |
| CLAIM-001 | CLAIM-001-AC-01 | Observed behaviour | Persistence integration | Eligible boundaries | Not tested |
| CLAIM-001 | CLAIM-001-AC-02 | Observed behaviour | Persistence integration | Terminal exclusion | Not tested |
| CLAIM-001 | CLAIM-001-AC-03 | Observed behaviour | Persistence integration | Claim count | Not tested |
| CLAIM-001 | CLAIM-001-AC-04 | Observed behaviour | Persistence integration | Abandoned claim | Not tested |
| CLAIM-001 | CLAIM-001-AC-05 | Observed behaviour | Persistence integration | No absolute claim cap | Not tested |
| CLAIM-002 | CLAIM-002-AC-01 | Required behaviour | Concurrency integration | Wrong token | Not tested |
| CLAIM-002 | CLAIM-002-AC-02 | Required behaviour | Persistence integration | Wrong target | Not tested |
| CLAIM-002 | CLAIM-002-AC-03 | Required behaviour | Concurrency integration | Reclaimed target | Not tested |
| CLAIM-003 | CLAIM-003-AC-01 | Observed behaviour | Persistence integration | Exact expiry | Not tested |
| CLAIM-003 | CLAIM-003-AC-02 | Observed behaviour | Persistence integration | Waiting or terminal target | Not tested |
| CLAIM-003 | CLAIM-003-AC-03 | Observed behaviour | Persistence integration | Successful renewal | Not tested |
| CLAIM-003 | CLAIM-003-AC-04 | Observed behaviour | Persistence integration | Settlement clears ownership | Not tested |
| CLAIM-004 | CLAIM-004-AC-01 | Required behaviour | Integration | Renewal rejected | Not tested |
| CLAIM-004 | CLAIM-004-AC-02 | Required behaviour | Integration | Renewal I/O failure | Not tested |
| CLAIM-004 | CLAIM-004-AC-03 | Required behaviour | Integration | Shutdown renewal | Not tested |
| CLAIM-004 | CLAIM-004-AC-04 | Required behaviour | Concurrency integration | Uncooperative handler | Not tested |

## Implementation trace
These locations are evidence, not mandated internal architecture. The full inspected-file/public-surface matrix is in [INSPECTION.md](INSPECTION.md).

- [src/Clean.Messaging/Persistence/IDurableStore.cs](../../src/Clean.Messaging/Persistence/IDurableStore.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)
- [src/Clean.Messaging/Processing/ClaimRenewal.cs](../../src/Clean.Messaging/Processing/ClaimRenewal.cs)
- [src/Clean.Messaging/ClaimId.cs](../../src/Clean.Messaging/ClaimId.cs)

## Open decisions and evidence limits
The [gap register](GAPS.md) records unresolved input domains, provider evidence and proposed changes. Source inspection is not execution proof. [INDEX.md](INDEX.md) explains classification authority and cross-capability ownership.
