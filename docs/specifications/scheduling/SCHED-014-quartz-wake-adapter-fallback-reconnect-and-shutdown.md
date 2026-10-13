# SCHED-014 — Quartz wake adapter, fallback, reconnect and shutdown

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** UseQuartzScheduling(Action<IQuartzBuilder>?) replaces the scheduling engine, uses the default unkeyed scheduler, and integrates Quartz hosting. Quartz contains a replacement wake hint, not one durable job per message.

**Evidence:** Scheduling.Quartz/DependencyInjection.cs; SchedulingEngine.cs; SchedulingWakeJob.cs. Quartz 4.0.1 host lifecycle is a dependency boundary, not a tested broker/cluster guarantee.

**Intent gaps:** [GAP-WF-005](../GAPS.md#gap-wf-005).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SCHED-014-AC-01 — Existing default scheduler is reused

**Given** an unkeyed default IScheduler already exists.
**When** Quartz scheduling is composed.
**Then** scheduling uses that default scheduler; configuration is applied to unnamed/default composition rather than replacing it with message-specific jobs.

## SCHED-014-AC-02 — Wake hint does not own scheduled payload

**Given** multiple persisted schedules become due.
**When** Quartz wake executes.
**Then** durable processing still selects from schedule storage; the hint does not contain or complete each scheduled payload.

## SCHED-014-AC-03 — Normal Quartz failure uses local fallback

**Given** a future Wait whose Quartz scheduling operation throws without caller cancellation.
**When** time or a local wake arrives.
**Then** the failure is logged and local timing/wake can release Wait so durable polling remains possible.

## SCHED-014-AC-04 — Later waits retry restored service

**Given** a prior Quartz scheduling failure and a subsequently restored service.
**When** another future Wait occurs.
**Then** scheduling is attempted again; no adapter reconnect guarantee or permanently disabled state is implied.

## SCHED-014-AC-05 — Caller cancellation propagates through adapter

**Given** caller cancels during scheduler I/O or waiting.
**When** Wait observes cancellation.
**Then** cancellation propagates rather than being hidden as an ordinary Quartz availability warning.

## SCHED-014-AC-06 — Shutdown waits for hosted jobs cooperatively

**Given** Quartz hosted integration has an executing wake job.
**When** the host shuts down.
**Then** hosting is configured to wait for jobs to complete; durable work recovery still depends on persisted state and cooperative cancellation, not forced job termination.
