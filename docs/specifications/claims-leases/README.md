# Claims, leases and settlement ownership — Behavioural specification

**Scope:** `IDurableStore<T>.Claim/Renew/Complete/Retry/DeadLetter`, `OwnedEntry<T>`, `ClaimRenewal.Run`, `MessageLeaseOptions`, `ClaimId`; [scheduling ownership](../scheduling/README.md) is distinct.

## Purpose
Allow recoverable temporary ownership of unresolved durable targets and reject stale settlement while explicitly limiting protection to the persistence boundary.

## Contracts, preconditions and postconditions
Pre: unresolved stored target, consistent supplied UTC basis and an appropriate lease. Post: owned updates either change the intended current target or report rejection. Rejected ownership is not ordinary consumer failure and does not identify a unique cause.

## Invariants
- A stale owner must not overwrite a successor's settlement.
- Renewal does not create a logical operation or increment attempt identity.
- Expiry permits recovery but cannot undo previous external effects.

## Failure, cancellation and consistency
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are unresolved decisions, not executable requirements.

## Limitations
- No forced handler termination, automatic clock synchronization, universal lease sizing or serialized mailbox.
- Supplied future expiry is not validated as an extension; batch claims share an opaque token in the current provider.

## Implementation trace
These locations explain contract ownership; they do not mandate internal implementation structure.

- [src/Clean.Messaging/Persistence/IDurableStore.cs](../../../src/Clean.Messaging/Persistence/IDurableStore.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs](../../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)
- [src/Clean.Messaging/Processing/ClaimRenewal.cs](../../../src/Clean.Messaging/Processing/ClaimRenewal.cs)
- [src/Clean.Messaging/ClaimId.cs](../../../src/Clean.Messaging/ClaimId.cs)

## Open decisions
The [gap register](../GAPS.md) records unresolved input domains, provider evidence and proposed changes. [INDEX.md](../INDEX.md) explains classification authority and cross-capability ownership.

## Specifications

- [CLAIM-001 — Claim eligibility and attempt accounting](CLAIM-001-claim-eligibility-and-attempt-accounting.md)
- [CLAIM-002 — Stale settlement rejection](CLAIM-002-stale-settlement-rejection.md)
- [CLAIM-003 — Owned-state and expiry predicates](CLAIM-003-owned-state-and-expiry-predicates.md)
- [CLAIM-004 — Renewal loss and cooperative cancellation](CLAIM-004-renewal-loss-and-cooperative-cancellation.md)
- [CLAIM-005 — Lease arithmetic and caller invariants](CLAIM-005-lease-arithmetic-and-caller-invariants.md)
