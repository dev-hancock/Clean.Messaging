# SCHED-015 — Undecided integration hardening

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Proposed behaviour

**Contracts and applicability:** Potential improvements are not current guarantees: validate default targets and null nested options; bound Quartz scheduling I/O; decide empty outgoing targets; define savepoint ownership. Each candidate is conditional on a gap decision.

**Evidence:** GAP-WF-001, GAP-WF-005, GAP-WF-006, GAP-WF-013, GAP-WF-014; current paths are ScheduleWriter, Quartz SchedulingEngine, SchedulingTracker and OutboxEntryFactory.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## Question SCHED-015-Q-01 ? — Default target admission policy

**Scenario:** default(ScheduleTarget) bypassed its constructor. a policy for writer admission is approved.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-001 chooses validation, the invalid target is rejected before durable mutation; current writer does not guarantee that outcome.

## Question SCHED-015-Q-02 ? — Hung Quartz operation policy

**Scenario:** ScheduleJob neither returns nor throws. a bounded-wait policy is approved.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-005 approves a timeout/concurrent fallback, local wake/deadline remains responsive within that bound; current fallback awaits scheduler I/O first.

## Question SCHED-015-Q-03 ? — User savepoint policy

**Scenario:** a caller creates or rolls back savepoints while schedules/saga/outbox effects share a context. the ownership policy is selected.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-006 approves support, rolled-back effects and wake observation must match persisted truth; current outbox integration rejects caller savepoints outside active saves and scheduling alone does not track caller rollback-to-savepoint.

## Question SCHED-015-Q-04 ? — Malformed options policy

**Scenario:** a nested scheduling options object is null. configuration hardening is approved.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-013 chooses explicit validation, startup reports that invalid value as an option error; current dereference failures are not an approved diagnostic contract.

## Question SCHED-015-Q-05 ? — Empty custom outgoing targets policy

**Scenario:** custom providers return no outgoing targets for a valid schedule. empty-target policy is selected.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-014 accepts current empty success, the schedule may complete with zero outgoing rows; alternatively rejection must be specified before tests assume it.
