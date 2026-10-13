# SCHED-004 — Runtime, EF, outbox and model composition

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** AddScheduling callback; EF MessagingBuilder.AddScheduling, ModelBuilder.AddScheduling, typed/untyped DbContextOptionsBuilder.AddScheduling; AddOutboxScheduling. Runtime ports are provider-facing, not a standalone alternative-provider guarantee.

**Evidence:** All scheduling DependencyInjection.cs files; SchedulingRuntimeValidation.cs; SchedulingValidation.cs; OutboxSchedulingValidation.cs; SchedulingConfiguration.cs.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SCHED-004-AC-01 — Runtime registration is not durable installation

**Given** only runtime AddScheduling is called.
**When** a host attempts application-facing scheduling or durable processing.
**Then** registration alone has not supplied IMessageScheduler, a persistence executor/store or durable workers; the host must compose those dependencies.

## SCHED-004-AC-02 — Outbox adapter does not install outbox persistence

**Given** AddOutboxScheduling without required outgoing/persistence services.
**When** startup validation runs.
**Then** missing outgoing writer/scheduler/delivery dependencies prevent valid startup instead of silently supplying a database or publishing.

## SCHED-004-AC-03 — EF composition requires relational configured context

**Given** nonrelational provider, missing schedule model or missing required interceptors.
**When** EF scheduling startup validation runs.
**Then** startup rejects the deficient integration; registration does not create schema or migrations.

## SCHED-004-AC-04 — Typed and untyped options install equivalent integration

**Given** valid model and services with either options overload.
**When** a schedule is saved and its explicit transaction committed.
**Then** both overloads support the same save/commit visibility and processing boundary; reapplying the same installed interceptor instance does not duplicate integration.

## SCHED-004-AC-05 — Target registration is exact and unique

**Given** two delivery registrations with the same ordinal target or only differently cased targets.
**When** runtime delivery registry validation runs.
**Then** the identical target is rejected; differently cased targets remain distinct routing identities.

## SCHED-004-AC-06 — Unknown retained target faults when encountered

**Given** a persisted eligible schedule whose exact target is no longer registered.
**When** processing attempts delivery.
**Then** the schedule faults with scheduling.delivery.target_not_registered; startup does not promise a scan of every retained row.
