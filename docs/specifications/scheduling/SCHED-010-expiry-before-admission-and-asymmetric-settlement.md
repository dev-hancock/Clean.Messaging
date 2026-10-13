# SCHED-010 — Expiry before admission and asymmetric settlement

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Unknown behaviour

**Contracts and applicability:** Candidate compatibility facts: schedules have no renewal; batch ownership starts before slot waiting; dispatch requires current token and lease > now, while failure/retry settlement checks token and active state without lease expiry. Successful admitted dispatch has no final lease-expiry check. These facts are not approved ownership policy.

**Evidence:** SchedulingWorker.cs; ScheduleStore.Retry/Fail; ScheduleExecutor.Dispatch; ScheduleEntry.Complete. Decision required: GAP-WF-003 and GAP-WF-004.

**Intent gaps:** [GAP-WF-003](../GAPS.md#gap-wf-003), [GAP-WF-004](../GAPS.md#gap-wf-004).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## Question SCHED-010-Q-01 ? Queued claim outlives lease

**Scenario:** a batch exceeds concurrency and a waiting row lease expires before admission; the dispatch gate is reached after expiry.
**Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-003, if current no-renewal behaviour is retained, that row performs no delivery and later recovery may reclaim it?

## Question SCHED-010-Q-02 ? Old unreclaimed token can record failure after expiry

**Scenario:** a dispatched attempt fails after its lease expired but before another claim replaces its token; failure/retry settlement runs.
**Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-004, if current token-only failure fencing is retained, the expired unreclaimed attempt can record retry/failure; a replaced token cannot?

## Question SCHED-010-Q-03 ? Accepted dispatch may finish past its lease

**Scenario:** the start gate succeeded before expiry and no competing successful mutation intervenes; same-context delivery and commit finish after expiry.
**Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-004, if current admission fencing is retained, completion can commit without a final unexpired-lease predicate; no exactly-once external claim follows?

## Question SCHED-010-Q-04 ? Decide renewal and queue exposure

**Scenario:** slow delivery and a preleased backlog threaten expiry; an ownership policy is selected.
**Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-003, a chosen renewal or lease-at-admission policy must define whether queued work remains owned and how ownership loss cancels execution; no unconditional outcome is approved?
