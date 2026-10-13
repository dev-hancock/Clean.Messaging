# CLAIM-004 — Renewal loss and cooperative cancellation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Evidence:** Coding standards §7/§9 says propagate cancellation and distinguish ownership loss from ordinary faults; ClaimRenewal provides cooperative loss signaling.

Pre: processor has an active ownership renewal loop and observes its tokens. Post: renewal rejection/failure requests handling cancellation; host shutdown is not classified as a permanent message fault.

## CLAIM-004-AC-01 — Renewal rejected
**Given** an active attempt's next renewal is rejected.
**When** the renewal loop observes rejection.
**Then** the handling attempt receives claim-loss cancellation instead of continued asserted ownership.

## CLAIM-004-AC-02 — Renewal I/O failure
**Given** renewal throws an unexpected store exception.
**When** the loop handles it.
**Then** claim-loss cancellation is requested and the failure is observable diagnostically.

## CLAIM-004-AC-03 — Shutdown renewal
**Given** host cancellation stops the renewal loop.
**When** its wait or I/O is cancelled.
**Then** no ordinary renewal fault or new successful settlement is fabricated from intentional shutdown.

## CLAIM-004-AC-04 — Uncooperative handler
**Given** an old handler ignores cancellation and issues an external request after lease loss.
**When** a successor acquires the target.
**Then** the persistence fence does not promise to undo or prevent the old external effect.
