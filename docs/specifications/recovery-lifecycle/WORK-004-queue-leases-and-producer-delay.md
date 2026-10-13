# WORK-004 — Queue, leases and producer delay

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** WorkLoop claims before enqueueing and renews only through processors; producer uses capped exponential infrastructure delay.

Pre: standard shared work loop, capacity/concurrency/batch valid. Post: deterministic claim selection does not imply ordered finish or fresh queued leases.

## WORK-004-AC-01 — Queued lease expiry
**Given** one claimed target waits behind a held handler longer than its lease.
**When** its processing slot becomes available.
**Then** the queue did not independently renew that target; a completion still requires valid current ownership.

## WORK-004-AC-02 — Producer delay budget
**Given** successive claim calls fail and a later call succeeds.
**When** recovery time is controlled.
**Then** delay begins at RecoveryInterval, doubles with failures up to FailureMaxDelay, and successful claiming resets that failure sequence.

## WORK-004-AC-03 — Invalid loop options
**Given** Capacity/Concurrency/BatchSize <=0 or a loop time value <=0.
**When** Run/Validate is requested.
**Then** validation fails before normal processing begins.

## WORK-004-AC-04 — No FIFO completion guarantee
**Given** several targets are claimed in selection order but handlers finish at different speeds.
**When** concurrent processing completes.
**Then** no invariant requires completion to follow that selection order.
