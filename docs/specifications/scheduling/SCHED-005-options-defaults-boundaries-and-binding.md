# SCHED-005 — Options defaults, boundaries and binding

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** SchedulingOptions.Section and Lease.Duration; Processing.Concurrency; Recovery.Interval/BatchSize; Retry.MaxAttempts/Delay/MaxDelay; Cleanup.Retention/Interval/BatchSize/MaxBatchesPerRun. Options callbacks configure these; the section constant is not automatic binding.

**Evidence:** Configuration/SchedulingOptions.cs; SchedulingOptionsValidator.cs; DependencyInjection.cs; SchedulingWorker.cs; ScheduleCleanupWorker.cs.

**Intent gaps:** [GAP-WF-013](../GAPS.md#gap-wf-013).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SCHED-005-AC-01 — Default policy is observable without binding

**Given** fresh options and no configuration callback.
**When** a composed host reads configured scheduling options.
**Then** lease is 1 minute, concurrency 4, recovery 30 seconds/batch 32, retry 5 attempts/2 seconds/cap 1 minute, cleanup 30 days/1 hour/batch 500/cap 20 batches.

## SCHED-005-AC-02 — Positive-only bounds reject zero and negatives

**Given** each of lease duration, concurrency, recovery interval/batch, retry max attempts and all cleanup settings is separately zero or negative.
**When** startup option validation runs.
**Then** each invalid setting fails validation while a positive boundary setting is accepted.

## SCHED-005-AC-03 — Retry delay zero and equal cap are valid

**Given** delay is zero with zero cap, or a nonnegative delay equals its cap.
**When** options are validated.
**Then** both combinations are accepted; negative delay or a cap below delay is rejected.

## SCHED-005-AC-04 — Configuration section name does not bind itself

**Given** configuration contains Messaging:Scheduling values but registration receives no binding callback.
**When** the host composes scheduling.
**Then** defaults remain unless the host explicitly configures/binds them; arbitrary supplied IConfiguration is not consumed by these overloads.
