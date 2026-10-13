# SAGA-003 — Composition, persistence mapping and saga options

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** Runtime AddSagas adds runtime scheduling but not outbox/EF stores. EF AddSagas on MessagingBuilder, ModelBuilder and typed/untyped DbContextOptionsBuilder includes scheduling integration. SagaOptions exposes Section, SerializerOptions, Retention, CleanupInterval and CleanupBatchSize.

**Evidence:** Both DependencyInjection.cs; SagaValidation.cs; SagaRuntimeValidation.cs; SagaConfiguration.cs; SagaOptions.cs; SagaOptionsValidator.cs.

**Intent gaps:** [GAP-WF-013](../GAPS.md#gap-wf-013).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SAGA-003-AC-01 — Runtime composition is incomplete without stores/effects

**Given** runtime saga registration exists without saga persistence, writer or outgoing services.
**When** startup resolves saga integration.
**Then** required dependencies prevent valid durable startup; registration is not automatic outbox installation.

## SAGA-003-AC-02 — EF model and interceptors include timers

**Given** a valid relational context uses saga EF builder/model/options composition.
**When** state and timers are saved via typed or untyped overload.
**Then** saga and scheduling mappings/interception participate together; same installed hook is not duplicated; missing model/interceptors/nonrelational provider fail validation.

## SAGA-003-AC-03 — Defaults and explicit binding

**Given** fresh saga options or IConfiguration with Messaging:Sagas but no binding callback.
**When** saga composition reads options.
**Then** JSON web defaults, retention 30 days, cleanup interval 1 hour and batch 500 apply unless explicitly configured; Section alone does not bind values.

## SAGA-003-AC-04 — Cleanup numeric boundaries

**Given** Retention/CleanupInterval/BatchSize are individually zero or negative then positive.
**When** startup validation runs.
**Then** zero/negative values reject and positive values pass; no saga-specific retry/lease/concurrency options are inferred.
