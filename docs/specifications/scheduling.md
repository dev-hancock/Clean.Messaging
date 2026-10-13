# Scheduling behavioural specifications

**Status: Draft — 2026-10-13. Source-reviewed; not tested.**

## Purpose and classification

Specifies provider-neutral scheduling, EF durability, message-to-outbox delivery and optional Quartz wake integration. It covers one-shot deferred work, not application recurrence policy or a promise to execute at an exact instant.

This is a behavioural catalogue for a future TUnit suite, not an executable suite or approval of every implementation choice. **Required** is backed by explicit normative repository instructions or documented API contract; **Observed** records source-derived compatibility candidates requiring maintainer approval; **Proposed** is a conditional future policy; **Unknown** identifies undecided intent. ACs inherit their enclosing classification unless an explicit override is stated. Unknown behaviour has decision questions only, not acceptance criteria or verification-matrix rows. Observed reference pages are evidence of current description, not independent normative authority. A source-reviewed candidate is not a verified guarantee.

## Common contracts and preconditions

Unless a criterion deliberately violates one, use registered durable message contracts, a running composed host, configured relational context/schema/interceptors, valid options, UTC TimeProvider observations, cooperative cancellation, and the same supported scoped owner transaction for local participating state/effects. Public runtime registration alone is insufficient. Persistence isolation, collation, execution strategy, clocks and dependency behaviour require provider-specific validation; no provider compatibility matrix was tested. Consumer business policy and migrations remain outside these specifications.

## Common postconditions and invariants

Read results describe committed persisted state, not unsaved local mutation. Success is scoped to its named boundary: staging is not commit, schedule consumption is not downstream completion, and local commit is not exactly-once external effects. Retained logical IDs are not interchangeable with delivery or ownership IDs. Failure, cancellation, concurrency loss, replay and terminal outcomes remain distinct. No criterion requires internal class identity, private method shape, or handler call-count guarantees.

## Failure, concurrency and limitations

Exercise database/serialization/registration failures, duplicate identity, exact time/length/budget boundaries, cancelled I/O, competing claims/versions, owner rollback, uncertain commit, process loss and later recovery. Unknown and unresolved Proposed blocks use numbered decision questions without acceptance criteria or matrix rows. No unconditional assertion is approved before the linked gap decision. Scheduling has no renewal/upsert guarantee; saga state retention is not identity deletion. External custom delivery/handler work may repeat. These are specifications for observed outcomes, not tests executed during this audit.

### SCHED-001 — Stable logical schedule identity

**Classification:** Required behaviour

**Contracts and applicability:** IMessageScheduler.Schedule<T>; ScheduleId; IScheduleDelivery.Dispatch. A retry describes the same logical operation; it is not a replacement operation. This requirement concerns identity, not duplicate-insert success.

**Evidence:** IMessageScheduler XML contract explicitly says retries must reuse ScheduleId; docs/CODING_STANDARDS.md §9 requires stable logical identity across retries.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SCHED-001-AC-01 — Retry preserves operation identity

**Given** a caller retrying one logical scheduling operation.
**When** the retry is staged or redelivered.
**Then** the supplied schedule identity remains the original identity rather than a replacement identity.

#### SCHED-001-AC-02 — Distinct operations remain distinct

**Given** two distinct scheduled operations.
**When** the caller supplies their identities.
**Then** the operations use distinct schedule identities; shared correlation does not make them one operation.

#### SCHED-001-AC-03 — Message delivery preserves logical event identity

**Given** a valid message-target schedule with identity X.
**When** delivery stages outgoing work.
**Then** every staged target has EventId X; the generated delivery MessageId has a different role, and outgoing metadata does not replace EventId.

### SCHED-002 — Repeated delivery and external effects

**Classification:** Required behaviour

**Contracts and applicability:** IScheduleDelivery.Target and Dispatch(ScheduledDispatch, CancellationToken). Delivery is at least once, not exactly once. A delivery adapter must tolerate repeats with the same schedule identity.

**Evidence:** IScheduleDelivery XML contract explicitly requires duplicate tolerance; docs/CODING_STANDARDS.md §§8–9 excludes distributed atomicity and requires assuming repeated external delivery.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SCHED-002-AC-01 — Same scheduled operation can be delivered again

**Given** an effect succeeded but its local completion did not commit.
**When** recovery dispatches the schedule again.
**Then** the repeated dispatch carries the same schedule identity and does not claim that the external effect was applied exactly once.

#### SCHED-002-AC-02 — Participating outbox state commits together

**Given** a schedule and outgoing rows use the configured scoped context with supported integration.
**When** successful delivery commits its transaction.
**Then** schedule completion and outgoing staging are both durable; neither is committed alone.

#### SCHED-002-AC-03 — Owner rollback leaves no local successful effect

**Given** delivery stages outgoing work in the same configured transaction.
**When** that owner rolls back before successful commit.
**Then** neither outgoing staging nor schedule completion is durable; any external effect is outside that rollback guarantee.

### SCHED-003 — Input descriptions, serialization and UTC boundaries

**Classification:** Observed behaviour

**Contracts and applicability:** IMessageScheduler.Schedule returns the caller ScheduleId. IScheduleWriter.Schedule adds target, nullable group and metadata; cancellation ports take explicit target/group/except identity. ScheduleId and ScheduleGroupId expose Value, New and N-format text; ScheduleTarget exposes Value, Message and preserved text.

**Evidence:** ScheduleId.cs; ScheduleGroupId.cs; ScheduleTarget.cs; Runtime/ScheduleWriter.cs; OutboxMessageScheduler.cs; shared IMessageSerializer.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SCHED-003-AC-01 — Valid descriptions preserve values

**Given** nonempty IDs, an ASCII target, a registered concrete payload and an offset due time.
**When** the writer stages the operation.
**Then** the returned identity matches the supplied identity, stored due time is the equivalent UTC instant, and group, target and supplied metadata are preserved.

#### SCHED-003-AC-02 — Due time is eligibility not rejection

**Given** valid payloads with past, current and future due times.
**When** each is staged.
**Then** none is rejected merely for being past/current; future work remains ineligible before its instant.

#### SCHED-003-AC-03 — Identity guards cover all writer commands

**Given** an empty schedule ID, empty supplied group ID, or empty supplied except ID.
**When** the corresponding Schedule, Cancel or CancelGroup command is used.
**Then** invalid identity is rejected before a durable schedule mutation.

#### SCHED-003-AC-04 — Target constructor character and length boundary

**Given** blank/null strings, 100 ASCII characters, 101 ASCII characters and a non-ASCII character.
**When** ScheduleTarget is constructed.
**Then** 100 nonblank ASCII characters are accepted unchanged; blank/null, overlong and non-ASCII inputs are rejected; lookup remains case-sensitive.

#### SCHED-003-AC-05 — Null and unregistered payloads are not durable schedules

**Given** a null payload or a payload with no durable contract.
**When** Schedule is used.
**Then** null is rejected or contract.type.not_registered faults; no outgoing row or durable schedule is created.

#### SCHED-003-AC-06 — Serializer rejects unsupported durable representation

**Given** a registered payload whose JSON serializer throws JsonException or NotSupportedException.
**When** Schedule serializes it.
**Then** serialization.serialize.failed is a permanent message failure and no schedule is staged.

#### SCHED-003-AC-07 — Identity value objects are not admission validation

**Given** Guid.Empty and newly generated GUID values.
**When** identity objects are constructed and formatted.
**Then** constructors retain even empty values, New produces a nonempty identity, and schedule/group text is the GUID N representation; writer guards remain a separate boundary.

#### SCHED-003-AC-08 — Writer does not inherit execution metadata

**Given** a message context with metadata and a Schedule call omitting metadata.
**When** the schedule is saved.
**Then** stored schedule correlation and causation remain null; eventual outgoing fallback belongs to the delivery/outbox boundary.

### SCHED-004 — Runtime, EF, outbox and model composition

**Classification:** Observed behaviour

**Contracts and applicability:** AddScheduling callback; EF MessagingBuilder.AddScheduling, ModelBuilder.AddScheduling, typed/untyped DbContextOptionsBuilder.AddScheduling; AddOutboxScheduling. Runtime ports are provider-facing, not a standalone alternative-provider guarantee.

**Evidence:** All scheduling DependencyInjection.cs files; SchedulingRuntimeValidation.cs; SchedulingValidation.cs; OutboxSchedulingValidation.cs; SchedulingConfiguration.cs.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SCHED-004-AC-01 — Runtime registration is not durable installation

**Given** only runtime AddScheduling is called.
**When** a host attempts application-facing scheduling or durable processing.
**Then** registration alone has not supplied IMessageScheduler, a persistence executor/store or durable workers; the host must compose those dependencies.

#### SCHED-004-AC-02 — Outbox adapter does not install outbox persistence

**Given** AddOutboxScheduling without required outgoing/persistence services.
**When** startup validation runs.
**Then** missing outgoing writer/scheduler/delivery dependencies prevent valid startup instead of silently supplying a database or publishing.

#### SCHED-004-AC-03 — EF composition requires relational configured context

**Given** nonrelational provider, missing schedule model or missing required interceptors.
**When** EF scheduling startup validation runs.
**Then** startup rejects the deficient integration; registration does not create schema or migrations.

#### SCHED-004-AC-04 — Typed and untyped options install equivalent integration

**Given** valid model and services with either options overload.
**When** a schedule is saved and its explicit transaction committed.
**Then** both overloads support the same save/commit visibility and processing boundary; reapplying the same installed interceptor instance does not duplicate integration.

#### SCHED-004-AC-05 — Target registration is exact and unique

**Given** two delivery registrations with the same ordinal target or only differently cased targets.
**When** runtime delivery registry validation runs.
**Then** the identical target is rejected; differently cased targets remain distinct routing identities.

#### SCHED-004-AC-06 — Unknown retained target faults when encountered

**Given** a persisted eligible schedule whose exact target is no longer registered.
**When** processing attempts delivery.
**Then** the schedule faults with scheduling.delivery.target_not_registered; startup does not promise a scan of every retained row.

### SCHED-005 — Options defaults, boundaries and binding

**Classification:** Observed behaviour

**Contracts and applicability:** SchedulingOptions.Section and Lease.Duration; Processing.Concurrency; Recovery.Interval/BatchSize; Retry.MaxAttempts/Delay/MaxDelay; Cleanup.Retention/Interval/BatchSize/MaxBatchesPerRun. Options callbacks configure these; the section constant is not automatic binding.

**Evidence:** Configuration/SchedulingOptions.cs; SchedulingOptionsValidator.cs; DependencyInjection.cs; SchedulingWorker.cs; ScheduleCleanupWorker.cs.

**Intent gaps:** [GAP-WF-013](GAPS.md#gap-wf-013).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SCHED-005-AC-01 — Default policy is observable without binding

**Given** fresh options and no configuration callback.
**When** a composed host reads configured scheduling options.
**Then** lease is 1 minute, concurrency 4, recovery 30 seconds/batch 32, retry 5 attempts/2 seconds/cap 1 minute, cleanup 30 days/1 hour/batch 500/cap 20 batches.

#### SCHED-005-AC-02 — Positive-only bounds reject zero and negatives

**Given** each of lease duration, concurrency, recovery interval/batch, retry max attempts and all cleanup settings is separately zero or negative.
**When** startup option validation runs.
**Then** each invalid setting fails validation while a positive boundary setting is accepted.

#### SCHED-005-AC-03 — Retry delay zero and equal cap are valid

**Given** delay is zero with zero cap, or a nonnegative delay equals its cap.
**When** options are validated.
**Then** both combinations are accepted; negative delay or a cap below delay is rejected.

#### SCHED-005-AC-04 — Configuration section name does not bind itself

**Given** configuration contains Messaging:Scheduling values but registration receives no binding callback.
**When** the host composes scheduling.
**Then** defaults remain unless the host explicitly configures/binds them; arbitrary supplied IConfiguration is not consumed by these overloads.

### SCHED-006 — Staging, duplicate insertion and owner visibility

**Classification:** Observed behaviour

**Contracts and applicability:** Schedule stages work, not an immediate durable insert/publication. The EF mapping keys schedules by ScheduleId and marks Version for concurrency. Owner SaveChanges and transaction commit define durability.

**Evidence:** Runtime/ScheduleWriter.cs; ScheduleStore.Add; SchedulingConfiguration.cs; SchedulingTracker.cs; save/transaction interceptors; shared owner dispatchers.

**Intent gaps:** [GAP-WF-002](GAPS.md#gap-wf-002), [GAP-WF-006](GAPS.md#gap-wf-006).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SCHED-006-AC-01 — Staged schedule requires save

**Given** a valid unsaved schedule.
**When** a separate persisted read or due worker queries.
**Then** the staged schedule is not visible as durable work until the owner saves and commits.

#### SCHED-006-AC-02 — Explicit transaction delays durable visibility

**Given** the owner saved a new schedule inside a user transaction.
**When** the transaction is still open and later rolls back.
**Then** other committed readers do not observe a successful new schedule and rollback leaves no durable new operation.

#### SCHED-006-AC-03 — Standalone save and explicit commit publish durable work

**Given** a valid schedule in an implicit save or explicit transaction.
**When** the save succeeds or the explicit transaction commits.
**Then** the operation becomes available for due processing after its committed due instant.

#### SCHED-006-AC-04 — Repeated insert is not an upsert

**Given** a retained schedule with ID X and a retry using ID X with equal or changed payload/due/group.
**When** the retry is staged and saved.
**Then** the existing row is not replaced or rescheduled; duplicate tracked/database identity can reject the attempt rather than return idempotent success.

#### SCHED-006-AC-05 — Duplicate insert race preserves one identity

**Given** two contexts stage the same ScheduleId.
**When** they compete to commit.
**Then** at most one row with that identity is committed; the losing insertion is not guaranteed a domain-specific duplicate result.

#### SCHED-006-AC-06 — Save failure cannot establish successful staging

**Given** serialization succeeded but database save/commit fails or is cancelled.
**When** the owner handles the failure.
**Then** a wake hint is not evidence of a durable schedule; only independently established committed state is processed.

### SCHED-007 — Cancellation filters, terminal states and races

**Classification:** Observed behaviour

**Contracts and applicability:** IMessageScheduler.Cancel/CancelGroup use message target; IScheduleWriter.Cancel has optional group, CancelGroup has exceptId. Cancellation stages changes and does not revoke external effects.

**Evidence:** OutboxMessageScheduler.cs; Runtime/ScheduleWriter.cs; ScheduleStore.Cancel/CancelGroup; ScheduleEntry.Cancel; SchedulingConfiguration.cs.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SCHED-007-AC-01 — Single cancellation respects target and optional group

**Given** active schedule X with a particular target/group.
**When** Cancel uses a different target or a nonmatching supplied group.
**Then** X remains active; a matching command followed by save/commit records cancellation.

#### SCHED-007-AC-02 — Public group cancellation is message-only

**Given** one group contains message and saga target schedules.
**When** IMessageScheduler.CancelGroup commits.
**Then** only matching active message-target schedules cancel; saga/other groups remain unchanged.

#### SCHED-007-AC-03 — Tracked and persisted group work are included

**Given** a group has unsaved new/modified and persisted active schedules plus terminal rows.
**When** a matching group cancel excluding X commits.
**Then** all matching nonterminal schedules except X cancel, including newly staged ones; terminal rows do not transition.

#### SCHED-007-AC-04 — Missing and terminal cancellation are no-ops

**Given** an absent schedule or an already dispatched/cancelled/failed row.
**When** matching single or group cancellation completes.
**Then** no replacement row, reopened work or altered terminal outcome is created.

#### SCHED-007-AC-05 — Cancellation removes retry ownership without erasing diagnostics

**Given** an active claimed/retrying schedule has diagnostics.
**When** cancellation commits.
**Then** CancelledAt is present, claim/lease/retry eligibility is cleared, and existing diagnostics remain visible.

#### SCHED-007-AC-06 — Concurrent cancellation does not reverse delivery

**Given** processing overlaps cancellation of the same schedule.
**When** their transactions compete.
**Then** a stale write may fail optimistic concurrency; the winning terminal result is not undone, and already performed external effects cannot be recalled.

#### SCHED-007-AC-07 — Cancel obeys owner rollback and cancellation token

**Given** a cancellation is staged or its database query receives a cancelled token.
**When** the owner rolls back or I/O is cancelled.
**Then** rolled-back cancellation is not durable; cancellation is propagated rather than reported as successful terminal mutation.

### SCHED-008 — Persisted management snapshots

**Classification:** Observed behaviour

**Contracts and applicability:** IScheduleReader.GetGroup(groupId, target?, token) returns ScheduleStatus snapshots: ScheduleId/ScheduleGroupId/Target/MessageData/due/attempts/retry/three terminal timestamps/code/message. Claims, lease, version and exception type are not management fields.

**Evidence:** IScheduleReader.cs; ScheduleStatus.cs; ScheduleStore.GetGroup.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SCHED-008-AC-01 — Group snapshots include terminal history

**Given** mixed active and terminal rows in two groups and targets.
**When** GetGroup reads with a target or no target.
**Then** only the requested group is returned, target filter is exact when supplied, all targets when omitted, and terminal rows remain included.

#### SCHED-008-AC-02 — Ordering is stable for equal due instants

**Given** group rows with differing due times and IDs including equal due times.
**When** GetGroup completes.
**Then** snapshots are ordered by due instant then schedule identity rather than completion or insertion order.

#### SCHED-008-AC-03 — Read does not expose unsaved mutations

**Given** a context has an unsaved addition/cancellation and persisted rows.
**When** GetGroup is used.
**Then** the result describes persisted state, not an in-memory merge, and changing snapshot data does not mutate stored lifecycle state.

#### SCHED-008-AC-04 — Empty group and read I/O failures

**Given** a group is absent or storage is unavailable/cancelled.
**When** GetGroup is used.
**Then** absent group returns an empty list; storage failure/cancellation propagates rather than being an empty successful read.

### SCHED-009 — Due eligibility, claims and bounded execution

**Classification:** Observed behaviour

**Contracts and applicability:** Committed active schedules become claimable at due/retry and lease boundaries. Attempts measures claims, not invocations. Processing.Concurrency bounds admitted delivery work within one worker; Recovery.BatchSize bounds selection, not global work.

**Evidence:** ScheduleStore.NextDue/Claim; SchedulingWorker.cs; OwnedSchedule.cs; SchedulingOptions.cs; ScheduleExecutor.cs.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SCHED-009-AC-01 — All eligibility predicates hold

**Given** rows differ by terminal status, original due, retry due and lease expiry.
**When** claim runs at time T.
**Then** only nonterminal rows with DueAt <= T, retry absent or <= T, and lease absent or <= T become owned.

#### SCHED-009-AC-02 — Boundary instant is claimable

**Given** eligible rows have due/retry/old lease exactly T.
**When** claim runs at T.
**Then** equality permits claiming; a timestamp just after T blocks the corresponding predicate.

#### SCHED-009-AC-03 — Batch selection is bounded and ordered

**Given** more eligible schedules than batch size including equal due instants.
**When** claim completes.
**Then** no more than the configured batch is owned, ordered by original due then identity; nonselected rows remain available.

#### SCHED-009-AC-04 — Claim increases attempts before delivery admission

**Given** a selected schedule waits behind busy execution slots.
**When** the claim is committed but delivery has not started.
**Then** Attempts has already increased; the counter does not purport to count handler calls or EF strategy repeats.

#### SCHED-009-AC-05 — Competing claims do not share ownership

**Given** two workers claim overlapping eligible rows.
**When** claim transactions complete.
**Then** committed ownership fencing distinguishes current attempts; an old token cannot settle a replacement token.

#### SCHED-009-AC-06 — Recovery is not a hard maximum invocation budget

**Given** a row repeatedly expires or processing is cancelled without failure settlement and its Attempts already reaches MaxAttempts.
**When** it becomes eligible again.
**Then** it can be claimed again; MaxAttempts gates failure disposition, not universal claim eligibility.

#### SCHED-009-AC-07 — Execution parallelism has local scope

**Given** a worker has more claimed rows than configured concurrency.
**When** deliveries are pending.
**Then** at most that worker concurrency are admitted together; completion need not follow due order, and multiple hosts are not a global semaphore.

### SCHED-010 — Expiry before admission and asymmetric settlement

**Classification:** Unknown behaviour

**Contracts and applicability:** Candidate compatibility facts: schedules have no renewal; batch ownership starts before slot waiting; dispatch requires current token and lease > now, while failure/retry settlement checks token and active state without lease expiry. Successful admitted dispatch has no final lease-expiry check. These facts are not approved ownership policy.

**Evidence:** SchedulingWorker.cs; ScheduleStore.Retry/Fail; ScheduleExecutor.Dispatch; ScheduleEntry.Complete. Decision required: GAP-WF-003 and GAP-WF-004.

**Intent gaps:** [GAP-WF-003](GAPS.md#gap-wf-003), [GAP-WF-004](GAPS.md#gap-wf-004).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### Question SCHED-010-Q-01 ? Queued claim outlives lease

**Scenario:** a batch exceeds concurrency and a waiting row lease expires before admission; the dispatch gate is reached after expiry.
**Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-003, if current no-renewal behaviour is retained, that row performs no delivery and later recovery may reclaim it?
**Verification:** no acceptance criterion or test outcome is approved until the linked gap is resolved.

#### Question SCHED-010-Q-02 ? Old unreclaimed token can record failure after expiry

**Scenario:** a dispatched attempt fails after its lease expired but before another claim replaces its token; failure/retry settlement runs.
**Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-004, if current token-only failure fencing is retained, the expired unreclaimed attempt can record retry/failure; a replaced token cannot?
**Verification:** no acceptance criterion or test outcome is approved until the linked gap is resolved.

#### Question SCHED-010-Q-03 ? Accepted dispatch may finish past its lease

**Scenario:** the start gate succeeded before expiry and no competing successful mutation intervenes; same-context delivery and commit finish after expiry.
**Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-004, if current admission fencing is retained, completion can commit without a final unexpired-lease predicate; no exactly-once external claim follows?
**Verification:** no acceptance criterion or test outcome is approved until the linked gap is resolved.

#### Question SCHED-010-Q-04 ? Decide renewal and queue exposure

**Scenario:** slow delivery and a preleased backlog threaten expiry; an ownership policy is selected.
**Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-003, a chosen renewal or lease-at-admission policy must define whether queued work remains owned and how ownership loss cancels execution; no unconditional outcome is approved?
**Verification:** no acceptance criterion or test outcome is approved until the linked gap is resolved.

### SCHED-011 — Delivery transaction, routing and retained contract failures

**Classification:** Observed behaviour

**Contracts and applicability:** ScheduledDispatch carries ScheduleId/ScheduleGroupId/MessageData/correlation/causation, not due/attempt/claim information. Delivery resolves target at processing; message delivery deserializes retained contract and selects current outgoing targets.

**Evidence:** ScheduleExecutor.cs; OutboxScheduleDelivery.cs; ScheduleDeliveryRegistry.cs; OutboxEntryFactory.cs; IMessageSerializer; owner transaction dispatchers.

**Intent gaps:** [GAP-WF-014](GAPS.md#gap-wf-014).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SCHED-011-AC-01 — Owned start gate rejects missing or stale work

**Given** a row is absent, terminal, wrong-token or expired.
**When** dispatch attempts to enter.
**Then** no outgoing effect is staged by that attempt; absent/terminal is settled, an active row owned elsewhere is not represented as that attempt success.

#### SCHED-011-AC-02 — Successful message schedule stages current targets

**Given** a retained message-target schedule and target providers changed since staging.
**When** due delivery succeeds.
**Then** outgoing obligations use the delivery-time target set, shared EventId=schedule ID and one delivery batch identity; schedule completion does not await each downstream target.

#### SCHED-011-AC-03 — Corrupt retained contract is a failure not completion

**Given** a schedule retains missing contract name, malformed JSON or JSON null.
**When** delivery deserializes it.
**Then** contract.identifier.not_registered or serialization.deserialize.failed is classified permanent and no successful outgoing dispatch is committed.

#### SCHED-011-AC-04 — Unhandled message default route fails

**Given** the default local target provider sees a message with no consumers.
**When** due delivery attempts outgoing staging.
**Then** consumer.message_unhandled faults the schedule rather than silently creating an empty local delivery.

#### SCHED-011-AC-05 — Rollback and execution-strategy repetition preserve durable truth

**Given** a delivery ran but save/commit rolls back or an EF strategy repeats.
**When** processing retries against persisted state.
**Then** uncommitted completion/outgoing rows are absent; retained completed state is recognized and local state is not duplicated, while external custom effects may already have repeated.

#### SCHED-011-AC-06 — Commit uncertainty is not external success proof

**Given** commit result is uncertain and a rollback attempt itself fails.
**When** processing recovers.
**Then** the original failure/uncertainty is not replaced by a rollback error; persisted completion/ownership determines later processing, not an assertion that an external effect was reversed.

### SCHED-012 — Failure budgets, diagnostics and cancellation recovery

**Classification:** Observed behaviour

**Contracts and applicability:** Transient/ordinary failure retries until claimed Attempts reaches MaxAttempts; permanent MessageException faults immediately. Scheduling retry is deterministic capped exponential, distinct from shared jittered entry retry.

**Evidence:** SchedulingWorker.cs; ScheduleFailure.cs; ScheduleStore.Retry/Fail; ScheduleEntry.Complete; MessageException.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SCHED-012-AC-01 — Permanent failure settles immediately

**Given** a current claimed active schedule fails with FailureAction.Fault MessageException.
**When** failure settlement succeeds.
**Then** FailedAt is recorded immediately with that code/message and claim/lease/retry cleared, even below MaxAttempts.

#### SCHED-012-AC-02 — Ordinary and transient failure retry within budget

**Given** a current attempt below MaxAttempts fails ordinarily or with retry-classified MessageException.
**When** settlement succeeds.
**Then** the row remains nonterminal with NextAttemptAt, no claim/lease, and failure diagnostics; it cannot retry before that instant.

#### SCHED-012-AC-03 — Exhaustion occurs on failed attempt at boundary

**Given** Attempts equals MaxAttempts or is greater and delivery fails transiently.
**When** settlement succeeds.
**Then** the schedule is failed rather than rescheduled; prior lease-loss claims count toward this decision.

#### SCHED-012-AC-04 — Capped exponential delays have no jitter

**Given** fixed clock and attempts 1, 2 and a sufficiently high attempt with valid delays.
**When** retry time is calculated.
**Then** delays equal min(MaxDelay, Delay * 2^max(0, attempt-1)); zero delay is immediate and no random variation is introduced.

#### SCHED-012-AC-05 — Diagnostic bounds and fallback code

**Given** MessageException or ordinary exception with long code/message/type.
**When** retry/failure diagnostics persist.
**Then** MessageException.Code or scheduling.unhandled is stored, code/message/exception type are limited to 200/500/500 characters, and ordinary exception text is not a guaranteed sanitized public message.

#### SCHED-012-AC-06 — Success retains earlier retry history

**Given** a schedule with failed-attempt diagnostics later delivers successfully.
**When** completion commits.
**Then** DispatchedAt is present and ownership/retry cleared, but Attempts and previous diagnostic values are retained.

#### SCHED-012-AC-07 — Shutdown cancellation is not normal message failure

**Given** processing token is cancelled during dispatch.
**When** cancellation propagates.
**Then** the cancellation is not converted into a normal scheduling retry/failure; unfinished persisted work relies on lease/recovery unless already committed.

#### SCHED-012-AC-08 — Failed settlement is recoverable unfinished work

**Given** dispatch failed but database retry/failure settlement fails or is cancelled.
**When** the worker later recovers with storage restored.
**Then** no terminal/retry state is inferred from a log; still-active work becomes claimable through expiry/recovery.

### SCHED-013 — Local wake engine, commit hints and polling recovery

**Classification:** Observed behaviour

**Contracts and applicability:** ISchedulingEngine.Wake/Wait(DateTime,token) supply local hints, not durable delivery. Save interception covers sync/async observed saves and explicit transaction outcomes. Recovery also polls.

**Evidence:** Runtime/SchedulingEngine.cs; SchedulingWorker.cs; SchedulingTracker.cs; SchedulingSaveInterceptor.cs; SchedulingTransactionInterceptor.cs.

**Intent gaps:** [GAP-WF-006](GAPS.md#gap-wf-006).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SCHED-013-AC-01 — Time and local hint both release wait

**Given** a future wake instant and no cancellation.
**When** time reaches the instant or a local Wake arrives first.
**Then** Wait returns to permit re-evaluation, without itself completing or claiming any durable schedule.

#### SCHED-013-AC-02 — Past wait and cancelled future wait

**Given** a past instant or a cancelled token for a future wait.
**When** Wait is called.
**Then** past wait returns without delay even if the token is already cancelled; future waiting observes cancellation rather than a false delivery success.

#### SCHED-013-AC-03 — Committed saves cause reevaluation

**Given** changed schedules are saved synchronously or asynchronously with implicit or explicit ownership.
**When** standalone save succeeds or explicit commit finishes.
**Then** due processing can re-evaluate durable work; an explicit save alone does not establish commit.

#### SCHED-013-AC-04 — Missed hints are eventually rechecked

**Given** a schedule commits without a usable local wake or belongs to another host.
**When** polling and recovery continue with storage available.
**Then** eligible work is revisited using due/retry eligibility and recovery interval, not an indefinite wait for one lost notification.

#### SCHED-013-AC-05 — Wake after failure is only a hint

**Given** save cancellation/failure or transaction failure produced a wake.
**When** worker re-evaluates storage.
**Then** it processes only actually committed eligible rows; waking is not proof of rollback or successful uncertain commit.

#### SCHED-013-AC-06 — Nested saves and ambient changed-work transactions are rejected

**Given** changed schedules are saved inside an ambient transaction or a nested observed save.
**When** save observation begins.
**Then** unsupported ownership is rejected instead of promising atomic processing; unrelated saves without changed schedules are not rejected by this scheduling check.

#### SCHED-013-AC-07 — Infrastructure fault does not abandon worker forever

**Given** claim/query/dispatch infrastructure raises an error.
**When** host remains running and storage recovers.
**Then** the worker can reevaluate after its recovery delay; fault delay does not terminally mark every schedule.

### SCHED-014 — Quartz wake adapter, fallback, reconnect and shutdown

**Classification:** Observed behaviour

**Contracts and applicability:** UseQuartzScheduling(Action<IQuartzBuilder>?) replaces the scheduling engine, uses the default unkeyed scheduler, and integrates Quartz hosting. Quartz contains a replacement wake hint, not one durable job per message.

**Evidence:** Scheduling.Quartz/DependencyInjection.cs; SchedulingEngine.cs; SchedulingWakeJob.cs. Quartz 4.0.1 host lifecycle is a dependency boundary, not a tested broker/cluster guarantee.

**Intent gaps:** [GAP-WF-005](GAPS.md#gap-wf-005).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SCHED-014-AC-01 — Existing default scheduler is reused

**Given** an unkeyed default IScheduler already exists.
**When** Quartz scheduling is composed.
**Then** scheduling uses that default scheduler; configuration is applied to unnamed/default composition rather than replacing it with message-specific jobs.

#### SCHED-014-AC-02 — Wake hint does not own scheduled payload

**Given** multiple persisted schedules become due.
**When** Quartz wake executes.
**Then** durable processing still selects from schedule storage; the hint does not contain or complete each scheduled payload.

#### SCHED-014-AC-03 — Normal Quartz failure uses local fallback

**Given** a future Wait whose Quartz scheduling operation throws without caller cancellation.
**When** time or a local wake arrives.
**Then** the failure is logged and local timing/wake can release Wait so durable polling remains possible.

#### SCHED-014-AC-04 — Later waits retry restored service

**Given** a prior Quartz scheduling failure and a subsequently restored service.
**When** another future Wait occurs.
**Then** scheduling is attempted again; no adapter reconnect guarantee or permanently disabled state is implied.

#### SCHED-014-AC-05 — Caller cancellation propagates through adapter

**Given** caller cancels during scheduler I/O or waiting.
**When** Wait observes cancellation.
**Then** cancellation propagates rather than being hidden as an ordinary Quartz availability warning.

#### SCHED-014-AC-06 — Shutdown waits for hosted jobs cooperatively

**Given** Quartz hosted integration has an executing wake job.
**When** the host shuts down.
**Then** hosting is configured to wait for jobs to complete; durable work recovery still depends on persisted state and cooperative cancellation, not forced job termination.

### SCHED-015 — Undecided integration hardening

**Classification:** Proposed behaviour

**Contracts and applicability:** Potential improvements are not current guarantees: validate default targets and null nested options; bound Quartz scheduling I/O; decide empty outgoing targets; define savepoint ownership. Each candidate is conditional on a gap decision.

**Evidence:** GAP-WF-001, GAP-WF-005, GAP-WF-006, GAP-WF-013, GAP-WF-014; current paths are ScheduleWriter, Quartz SchedulingEngine, SchedulingTracker and OutboxEntryFactory.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### Question SCHED-015-Q-01 ? — Default target admission policy

**Scenario:** default(ScheduleTarget) bypassed its constructor. a policy for writer admission is approved.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-001 chooses validation, the invalid target is rejected before durable mutation; current writer does not guarantee that outcome.
**Verification:** no acceptance criterion or test outcome is approved before the linked gap decision.

#### Question SCHED-015-Q-02 ? — Hung Quartz operation policy

**Scenario:** ScheduleJob neither returns nor throws. a bounded-wait policy is approved.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-005 approves a timeout/concurrent fallback, local wake/deadline remains responsive within that bound; current fallback awaits scheduler I/O first.
**Verification:** no acceptance criterion or test outcome is approved before the linked gap decision.

#### Question SCHED-015-Q-03 ? — User savepoint policy

**Scenario:** a caller creates or rolls back savepoints while schedules/saga/outbox effects share a context. the ownership policy is selected.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-006 approves support, rolled-back effects and wake observation must match persisted truth; current outbox integration rejects caller savepoints outside active saves and scheduling alone does not track caller rollback-to-savepoint.
**Verification:** no acceptance criterion or test outcome is approved before the linked gap decision.

#### Question SCHED-015-Q-04 ? — Malformed options policy

**Scenario:** a nested scheduling options object is null. configuration hardening is approved.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-013 chooses explicit validation, startup reports that invalid value as an option error; current dereference failures are not an approved diagnostic contract.
**Verification:** no acceptance criterion or test outcome is approved before the linked gap decision.

#### Question SCHED-015-Q-05 ? — Empty custom outgoing targets policy

**Scenario:** custom providers return no outgoing targets for a valid schedule. empty-target policy is selected.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-014 accepts current empty success, the schedule may complete with zero outgoing rows; alternatively rejection must be specified before tests assume it.
**Verification:** no acceptance criterion or test outcome is approved before the linked gap decision.

### SCHED-016 — Terminal retention, recovery boundaries and management limits

**Classification:** Observed behaviour

**Contracts and applicability:** Cleanup removes old terminal schedules, not active overdue work. There is no public schedule upsert, recurring schedule, requeue/dead-letter repair, failed-work restart or lease-renewal API. Read/cancel are not repair commands.

**Evidence:** ScheduleStore.Cleanup; ScheduleCleanupWorker.cs; SchedulingOptions.Cleanup; all owned public scheduling surfaces.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SCHED-016-AC-01 — Strict terminal retention cutoff

**Given** dispatched/cancelled/failed rows before, equal to and after cutoff plus very old active work.
**When** cleanup completes.
**Then** only terminal rows with terminal timestamp strictly before cutoff are deleted; equal/after and active rows remain.

#### SCHED-016-AC-02 — Cleanup is periodic and bounded

**Given** more eligible rows than batch times cap.
**When** a cleanup tick runs.
**Then** deletion uses batches ordered by CreatedAt, stops at configured per-run cap with warning when capped, and leaves remaining work for later ticks.

#### SCHED-016-AC-03 — Cleanup recovery and initial timing

**Given** a host starts or a cleanup query fails.
**When** periodic cleanup lifecycle continues.
**Then** no immediate startup purge is assumed; failed cleanup can retry on a later tick without changing active schedules into terminal ones.

#### SCHED-016-AC-04 — Deletion removes identity protection and history

**Given** an old terminal schedule is physically removed.
**When** the same identity is inserted later.
**Then** the deleted schedule row no longer prevents reuse or supplies history; retained outgoing/timer evidence has its own independent lifetime.

#### SCHED-016-AC-05 — Cancellation is not failure requeue

**Given** a retained failed schedule is read or cancelled.
**When** management APIs complete.
**Then** it remains failed and is not automatically reopened, rescheduled or compensated.

## Public contract and code-path traceability

Each authored owned file was reviewed in full, including internal implementation, project references and friend-assembly declarations. The grouped public contracts appear in the audit manifest; this table ties every owned path to outcome specifications. Shared message contracts/options/outbox/owner ports retain their owning capability specifications rather than becoming new workflow APIs.

| Reviewed source/project | Behaviour specifications |
| --- | --- |
| [src\Clean.Messaging.Scheduling\Clean.Messaging.Scheduling.csproj](../../src/Clean.Messaging.Scheduling/Clean.Messaging.Scheduling.csproj) | SCHED-004 |
| [src\Clean.Messaging.Scheduling\Configuration\SchedulingOptions.cs](../../src/Clean.Messaging.Scheduling/Configuration/SchedulingOptions.cs) | SCHED-005 |
| [src\Clean.Messaging.Scheduling\Configuration\SchedulingOptionsValidator.cs](../../src/Clean.Messaging.Scheduling/Configuration/SchedulingOptionsValidator.cs) | SCHED-005, SCHED-015 |
| [src\Clean.Messaging.Scheduling\Configuration\SchedulingRuntimeValidation.cs](../../src/Clean.Messaging.Scheduling/Configuration/SchedulingRuntimeValidation.cs) | SCHED-004 |
| [src\Clean.Messaging.Scheduling\Delivery\IScheduleDelivery.cs](../../src/Clean.Messaging.Scheduling/Delivery/IScheduleDelivery.cs) | SCHED-002, SCHED-004, SCHED-011 |
| [src\Clean.Messaging.Scheduling\Delivery\ScheduledDispatch.cs](../../src/Clean.Messaging.Scheduling/Delivery/ScheduledDispatch.cs) | SCHED-011 |
| [src\Clean.Messaging.Scheduling\Delivery\ScheduleDeliveryRegistry.cs](../../src/Clean.Messaging.Scheduling/Delivery/ScheduleDeliveryRegistry.cs) | SCHED-004, SCHED-011 |
| [src\Clean.Messaging.Scheduling\DependencyInjection.cs](../../src/Clean.Messaging.Scheduling/DependencyInjection.cs) | SCHED-004, SCHED-005 |
| [src\Clean.Messaging.Scheduling\IMessageScheduler.cs](../../src/Clean.Messaging.Scheduling/IMessageScheduler.cs) | SCHED-001, SCHED-003, SCHED-006, SCHED-007 |
| [src\Clean.Messaging.Scheduling\Persistence\IScheduleReader.cs](../../src/Clean.Messaging.Scheduling/Persistence/IScheduleReader.cs) | SCHED-008 |
| [src\Clean.Messaging.Scheduling\Persistence\IScheduleStore.cs](../../src/Clean.Messaging.Scheduling/Persistence/IScheduleStore.cs) | SCHED-006, SCHED-007, SCHED-008, SCHED-009, SCHED-012, SCHED-016 |
| [src\Clean.Messaging.Scheduling\Persistence\IScheduleWriter.cs](../../src/Clean.Messaging.Scheduling/Persistence/IScheduleWriter.cs) | SCHED-003, SCHED-006, SCHED-007 |
| [src\Clean.Messaging.Scheduling\Persistence\OwnedSchedule.cs](../../src/Clean.Messaging.Scheduling/Persistence/OwnedSchedule.cs) | SCHED-009, SCHED-010 |
| [src\Clean.Messaging.Scheduling\Persistence\ScheduleEntry.cs](../../src/Clean.Messaging.Scheduling/Persistence/ScheduleEntry.cs) | SCHED-006, SCHED-007, SCHED-009, SCHED-010, SCHED-012 |
| [src\Clean.Messaging.Scheduling\Persistence\ScheduleFailure.cs](../../src/Clean.Messaging.Scheduling/Persistence/ScheduleFailure.cs) | SCHED-012 |
| [src\Clean.Messaging.Scheduling\Persistence\ScheduleStatus.cs](../../src/Clean.Messaging.Scheduling/Persistence/ScheduleStatus.cs) | SCHED-008 |
| [src\Clean.Messaging.Scheduling\Properties\AssemblyInfo.cs](../../src/Clean.Messaging.Scheduling/Properties/AssemblyInfo.cs) | SCHED-004 |
| [src\Clean.Messaging.Scheduling\Runtime\IScheduleExecutor.cs](../../src/Clean.Messaging.Scheduling/Runtime/IScheduleExecutor.cs) | SCHED-010, SCHED-011 |
| [src\Clean.Messaging.Scheduling\Runtime\ISchedulingEngine.cs](../../src/Clean.Messaging.Scheduling/Runtime/ISchedulingEngine.cs) | SCHED-013, SCHED-014 |
| [src\Clean.Messaging.Scheduling\Runtime\ScheduleCleanupWorker.cs](../../src/Clean.Messaging.Scheduling/Runtime/ScheduleCleanupWorker.cs) | SCHED-016 |
| [src\Clean.Messaging.Scheduling\Runtime\ScheduleWriter.cs](../../src/Clean.Messaging.Scheduling/Runtime/ScheduleWriter.cs) | SCHED-003, SCHED-006, SCHED-007, SCHED-015 |
| [src\Clean.Messaging.Scheduling\Runtime\SchedulingEngine.cs](../../src/Clean.Messaging.Scheduling/Runtime/SchedulingEngine.cs) | SCHED-013 |
| [src\Clean.Messaging.Scheduling\Runtime\SchedulingWorker.cs](../../src/Clean.Messaging.Scheduling/Runtime/SchedulingWorker.cs) | SCHED-009, SCHED-010, SCHED-012, SCHED-013 |
| [src\Clean.Messaging.Scheduling\ScheduleGroupId.cs](../../src/Clean.Messaging.Scheduling/ScheduleGroupId.cs) | SCHED-003, SCHED-007, SCHED-008 |
| [src\Clean.Messaging.Scheduling\ScheduleId.cs](../../src/Clean.Messaging.Scheduling/ScheduleId.cs) | SCHED-001, SCHED-003 |
| [src\Clean.Messaging.Scheduling\ScheduleTarget.cs](../../src/Clean.Messaging.Scheduling/ScheduleTarget.cs) | SCHED-003, SCHED-015 |
| [src\Clean.Messaging.Scheduling.EntityFrameworkCore\Clean.Messaging.Scheduling.EntityFrameworkCore.csproj](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/Clean.Messaging.Scheduling.EntityFrameworkCore.csproj) | SCHED-004 |
| [src\Clean.Messaging.Scheduling.EntityFrameworkCore\DependencyInjection.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/DependencyInjection.cs) | SCHED-004, SCHED-005, SCHED-013 |
| [src\Clean.Messaging.Scheduling.EntityFrameworkCore\ScheduleExecutor.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleExecutor.cs) | SCHED-002, SCHED-010, SCHED-011 |
| [src\Clean.Messaging.Scheduling.EntityFrameworkCore\ScheduleStore.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleStore.cs) | SCHED-006, SCHED-007, SCHED-008, SCHED-009, SCHED-010, SCHED-012, SCHED-016 |
| [src\Clean.Messaging.Scheduling.EntityFrameworkCore\SchedulingConfiguration.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/SchedulingConfiguration.cs) | SCHED-004, SCHED-006, SCHED-007 |
| [src\Clean.Messaging.Scheduling.EntityFrameworkCore\SchedulingSaveInterceptor.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/SchedulingSaveInterceptor.cs) | SCHED-013 |
| [src\Clean.Messaging.Scheduling.EntityFrameworkCore\SchedulingTracker.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/SchedulingTracker.cs) | SCHED-006, SCHED-013, SCHED-015 |
| [src\Clean.Messaging.Scheduling.EntityFrameworkCore\SchedulingTransactionInterceptor.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/SchedulingTransactionInterceptor.cs) | SCHED-013 |
| [src\Clean.Messaging.Scheduling.EntityFrameworkCore\SchedulingValidation.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/SchedulingValidation.cs) | SCHED-004 |
| [src\Clean.Messaging.Scheduling.Outbox\Clean.Messaging.Scheduling.Outbox.csproj](../../src/Clean.Messaging.Scheduling.Outbox/Clean.Messaging.Scheduling.Outbox.csproj) | SCHED-004 |
| [src\Clean.Messaging.Scheduling.Outbox\DependencyInjection.cs](../../src/Clean.Messaging.Scheduling.Outbox/DependencyInjection.cs) | SCHED-004 |
| [src\Clean.Messaging.Scheduling.Outbox\OutboxMessageScheduler.cs](../../src/Clean.Messaging.Scheduling.Outbox/OutboxMessageScheduler.cs) | SCHED-001, SCHED-003, SCHED-007 |
| [src\Clean.Messaging.Scheduling.Outbox\OutboxScheduleDelivery.cs](../../src/Clean.Messaging.Scheduling.Outbox/OutboxScheduleDelivery.cs) | SCHED-001, SCHED-002, SCHED-011, SCHED-015 |
| [src\Clean.Messaging.Scheduling.Outbox\OutboxSchedulingValidation.cs](../../src/Clean.Messaging.Scheduling.Outbox/OutboxSchedulingValidation.cs) | SCHED-004 |
| [src\Clean.Messaging.Scheduling.Quartz\Clean.Messaging.Scheduling.Quartz.csproj](../../src/Clean.Messaging.Scheduling.Quartz/Clean.Messaging.Scheduling.Quartz.csproj) | SCHED-014 |
| [src\Clean.Messaging.Scheduling.Quartz\DependencyInjection.cs](../../src/Clean.Messaging.Scheduling.Quartz/DependencyInjection.cs) | SCHED-014 |
| [src\Clean.Messaging.Scheduling.Quartz\SchedulingEngine.cs](../../src/Clean.Messaging.Scheduling.Quartz/SchedulingEngine.cs) | SCHED-014, SCHED-015 |
| [src\Clean.Messaging.Scheduling.Quartz\SchedulingWakeJob.cs](../../src/Clean.Messaging.Scheduling.Quartz/SchedulingWakeJob.cs) | SCHED-014 |

### End-to-end ownership trace

`IMessageScheduler` / `IScheduleWriter` → serialization/UTC/identity validation → EF staging → owner save/commit observation → local/Quartz wake plus polling → due/retry/lease claim → capacity wait → owned transactional dispatch → current delivery target → outbox EventId capture or saga timer → save/commit → later outgoing target delivery. Cancellation and cleanup operate on persisted/staged lifecycle; neither revokes external effects. See SCHED-001 through SCHED-016.

### Shared evidence and boundary ownership

The following dependencies/references were read for workflow composition (not part of the 89-file owned-project inventory):

- [docs/CODING_STANDARDS.md](../../docs/CODING_STANDARDS.md)
- [docs/reference/architecture.md](../../docs/reference/architecture.md)
- [docs/reference/configuration.md](../../docs/reference/configuration.md)
- [docs/reference/contracts.md](../../docs/reference/contracts.md)
- [docs/reference/persistence.md](../../docs/reference/persistence.md)
- [docs/reference/outbox.md](../../docs/reference/outbox.md)
- [docs/reference/processing.md](../../docs/reference/processing.md)
- [docs/reference/scheduling.md](../../docs/reference/scheduling.md)
- [docs/reference/sagas.md](../../docs/reference/sagas.md)
- [docs/explanation/durable-workflows.md](../../docs/explanation/durable-workflows.md)
- [docs/explanation/transaction-boundaries.md](../../docs/explanation/transaction-boundaries.md)
- [docs/explanation/delivery-semantics.md](../../docs/explanation/delivery-semantics.md)
- [docs/explanation/message-identity.md](../../docs/explanation/message-identity.md)
- [docs/explanation/capability-boundaries.md](../../docs/explanation/capability-boundaries.md)
- [src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs](../../src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs)
- [src/Clean.Messaging.Outbox/IOutbox.cs](../../src/Clean.Messaging.Outbox/IOutbox.cs)
- [src/Clean.Messaging.Outbox/Outbox.cs](../../src/Clean.Messaging.Outbox/Outbox.cs)
- [src/Clean.Messaging.Outbox/Transport/OutboxTargetProvider.cs](../../src/Clean.Messaging.Outbox/Transport/OutboxTargetProvider.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs)
- [src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxDispatcher.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxDispatcher.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/LocalOutboxDelivery.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/LocalOutboxDelivery.cs)
- [src/Clean.Messaging/Serialization/MessageSerializer.cs](../../src/Clean.Messaging/Serialization/MessageSerializer.cs)
- [src/Clean.Messaging/Processing/ConsumerDispatcher.cs](../../src/Clean.Messaging/Processing/ConsumerDispatcher.cs)
- [src/Clean.Messaging/Consumers/ConsumerRegistry.cs](../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs)
- [src/Clean.Messaging/DependencyInjection.cs](../../src/Clean.Messaging/DependencyInjection.cs)
- [src/Clean.Messaging.EntityFrameworkCore/PersistenceServices.cs](../../src/Clean.Messaging.EntityFrameworkCore/PersistenceServices.cs)

## Intent gaps

| Gap | Unresolved decision | Affected specifications |
| --- | --- | --- |
| [GAP-WF-001](GAPS.md#gap-wf-001) | Must default target and default correlation key be rejected at the writer/runtime boundary? | SCHED-003, SCHED-015 |
| [GAP-WF-002](GAPS.md#gap-wf-002) | Is an equal retry supposed to be idempotent admission, or must the owner retry/recognize a failed unit of work? | SCHED-001, SCHED-006 |
| [GAP-WF-003](GAPS.md#gap-wf-003) | Should queued/long-running schedules renew or lease only when admitted, and what bounds are operationally required? | SCHED-009, SCHED-010 |
| [GAP-WF-004](GAPS.md#gap-wf-004) | Is token-only expired failure settlement and no final lease check on successful execution deliberate? | SCHED-010, SCHED-012 |
| [GAP-WF-005](GAPS.md#gap-wf-005) | Must local wake/deadline remain responsive when ScheduleJob hangs, rather than only throws? | SCHED-014, SCHED-015 |
| [GAP-WF-006](GAPS.md#gap-wf-006) | Which caller savepoint and uncertain-commit patterns are supported across composed schedules/sagas/outbox? | SCHED-006, SCHED-013, SCHED-015 |
| [GAP-WF-013](GAPS.md#gap-wf-013) | What explicit failures apply to null nested scheduling options or null saga serializer options, and extreme otherwise-positive values? | SCHED-005, SCHED-015 |
| [GAP-WF-014](GAPS.md#gap-wf-014) | Is zero-target custom delivery success intended, unlike the default unhandled-message fault? | SCHED-011, SCHED-015 |

## Verification matrix

Every criterion appears once. Levels describe future verification intent: unit for pure contract/description outcomes; persistence for database durability; concurrency for controlled overlapping ownership; transport for external protocol boundaries; workflow for composed host/processing lifecycle. Workflow-level Quartz criteria are not claims of broker transport validation. All results remain **Not tested**. Conditional rows are falsifiable only after the stated policy decision.

| Acceptance criterion | Level | Result | Falsifiable observable outcome |
| --- | --- | --- | --- |
| SCHED-001-AC-01 | workflow | Not tested | the supplied schedule identity remains the original identity rather than a replacement identity. |
| SCHED-001-AC-02 | unit | Not tested | the operations use distinct schedule identities; shared correlation does not make them one operation. |
| SCHED-001-AC-03 | persistence | Not tested | every staged target has EventId X; the generated delivery MessageId has a different role, and outgoing metadata does not replace EventId. |
| SCHED-002-AC-01 | workflow | Not tested | the repeated dispatch carries the same schedule identity and does not claim that the external effect was applied exactly once. |
| SCHED-002-AC-02 | persistence | Not tested | schedule completion and outgoing staging are both durable; neither is committed alone. |
| SCHED-002-AC-03 | persistence | Not tested | neither outgoing staging nor schedule completion is durable; any external effect is outside that rollback guarantee. |
| SCHED-003-AC-01 | persistence | Not tested | the returned identity matches the supplied identity, stored due time is the equivalent UTC instant, and group, target and supplied metadata are preserved. |
| SCHED-003-AC-02 | persistence | Not tested | none is rejected merely for being past/current; future work remains ineligible before its instant. |
| SCHED-003-AC-03 | unit | Not tested | invalid identity is rejected before a durable schedule mutation. |
| SCHED-003-AC-04 | unit | Not tested | 100 nonblank ASCII characters are accepted unchanged; blank/null, overlong and non-ASCII inputs are rejected; lookup remains case-sensitive. |
| SCHED-003-AC-05 | persistence | Not tested | null is rejected or contract.type.not_registered faults; no outgoing row or durable schedule is created. |
| SCHED-003-AC-06 | unit | Not tested | serialization.serialize.failed is a permanent message failure and no schedule is staged. |
| SCHED-003-AC-07 | unit | Not tested | constructors retain even empty values, New produces a nonempty identity, and schedule/group text is the GUID N representation; writer guards remain a separate boundary. |
| SCHED-003-AC-08 | persistence | Not tested | stored schedule correlation and causation remain null; eventual outgoing fallback belongs to the delivery/outbox boundary. |
| SCHED-004-AC-01 | unit | Not tested | registration alone has not supplied IMessageScheduler, a persistence executor/store or durable workers; the host must compose those dependencies. |
| SCHED-004-AC-02 | workflow | Not tested | missing outgoing writer/scheduler/delivery dependencies prevent valid startup instead of silently supplying a database or publishing. |
| SCHED-004-AC-03 | persistence | Not tested | startup rejects the deficient integration; registration does not create schema or migrations. |
| SCHED-004-AC-04 | persistence | Not tested | both overloads support the same save/commit visibility and processing boundary; reapplying the same installed interceptor instance does not duplicate integration. |
| SCHED-004-AC-05 | unit | Not tested | the identical target is rejected; differently cased targets remain distinct routing identities. |
| SCHED-004-AC-06 | persistence | Not tested | the schedule faults with scheduling.delivery.target_not_registered; startup does not promise a scan of every retained row. |
| SCHED-005-AC-01 | unit | Not tested | lease is 1 minute, concurrency 4, recovery 30 seconds/batch 32, retry 5 attempts/2 seconds/cap 1 minute, cleanup 30 days/1 hour/batch 500/cap 20 batches. |
| SCHED-005-AC-02 | unit | Not tested | each invalid setting fails validation while a positive boundary setting is accepted. |
| SCHED-005-AC-03 | unit | Not tested | both combinations are accepted; negative delay or a cap below delay is rejected. |
| SCHED-005-AC-04 | unit | Not tested | defaults remain unless the host explicitly configures/binds them; arbitrary supplied IConfiguration is not consumed by these overloads. |
| SCHED-006-AC-01 | persistence | Not tested | the staged schedule is not visible as durable work until the owner saves and commits. |
| SCHED-006-AC-02 | persistence | Not tested | other committed readers do not observe a successful new schedule and rollback leaves no durable new operation. |
| SCHED-006-AC-03 | persistence | Not tested | the operation becomes available for due processing after its committed due instant. |
| SCHED-006-AC-04 | persistence | Not tested | the existing row is not replaced or rescheduled; duplicate tracked/database identity can reject the attempt rather than return idempotent success. |
| SCHED-006-AC-05 | concurrency | Not tested | at most one row with that identity is committed; the losing insertion is not guaranteed a domain-specific duplicate result. |
| SCHED-006-AC-06 | persistence | Not tested | a wake hint is not evidence of a durable schedule; only independently established committed state is processed. |
| SCHED-007-AC-01 | persistence | Not tested | X remains active; a matching command followed by save/commit records cancellation. |
| SCHED-007-AC-02 | persistence | Not tested | only matching active message-target schedules cancel; saga/other groups remain unchanged. |
| SCHED-007-AC-03 | persistence | Not tested | all matching nonterminal schedules except X cancel, including newly staged ones; terminal rows do not transition. |
| SCHED-007-AC-04 | persistence | Not tested | no replacement row, reopened work or altered terminal outcome is created. |
| SCHED-007-AC-05 | persistence | Not tested | CancelledAt is present, claim/lease/retry eligibility is cleared, and existing diagnostics remain visible. |
| SCHED-007-AC-06 | concurrency | Not tested | a stale write may fail optimistic concurrency; the winning terminal result is not undone, and already performed external effects cannot be recalled. |
| SCHED-007-AC-07 | persistence | Not tested | rolled-back cancellation is not durable; cancellation is propagated rather than reported as successful terminal mutation. |
| SCHED-008-AC-01 | persistence | Not tested | only the requested group is returned, target filter is exact when supplied, all targets when omitted, and terminal rows remain included. |
| SCHED-008-AC-02 | persistence | Not tested | snapshots are ordered by due instant then schedule identity rather than completion or insertion order. |
| SCHED-008-AC-03 | persistence | Not tested | the result describes persisted state, not an in-memory merge, and changing snapshot data does not mutate stored lifecycle state. |
| SCHED-008-AC-04 | persistence | Not tested | absent group returns an empty list; storage failure/cancellation propagates rather than being an empty successful read. |
| SCHED-009-AC-01 | persistence | Not tested | only nonterminal rows with DueAt <= T, retry absent or <= T, and lease absent or <= T become owned. |
| SCHED-009-AC-02 | persistence | Not tested | equality permits claiming; a timestamp just after T blocks the corresponding predicate. |
| SCHED-009-AC-03 | persistence | Not tested | no more than the configured batch is owned, ordered by original due then identity; nonselected rows remain available. |
| SCHED-009-AC-04 | concurrency | Not tested | Attempts has already increased; the counter does not purport to count handler calls or EF strategy repeats. |
| SCHED-009-AC-05 | concurrency | Not tested | committed ownership fencing distinguishes current attempts; an old token cannot settle a replacement token. |
| SCHED-009-AC-06 | persistence | Not tested | it can be claimed again; MaxAttempts gates failure disposition, not universal claim eligibility. |
| SCHED-009-AC-07 | concurrency | Not tested | at most that worker concurrency are admitted together; completion need not follow due order, and multiple hosts are not a global semaphore. |
| SCHED-011-AC-01 | persistence | Not tested | no outgoing effect is staged by that attempt; absent/terminal is settled, an active row owned elsewhere is not represented as that attempt success. |
| SCHED-011-AC-02 | workflow | Not tested | outgoing obligations use the delivery-time target set, shared EventId=schedule ID and one delivery batch identity; schedule completion does not await each downstream target. |
| SCHED-011-AC-03 | persistence | Not tested | contract.identifier.not_registered or serialization.deserialize.failed is classified permanent and no successful outgoing dispatch is committed. |
| SCHED-011-AC-04 | workflow | Not tested | consumer.message_unhandled faults the schedule rather than silently creating an empty local delivery. |
| SCHED-011-AC-05 | persistence | Not tested | uncommitted completion/outgoing rows are absent; retained completed state is recognized and local state is not duplicated, while external custom effects may already have repeated. |
| SCHED-011-AC-06 | persistence | Not tested | the original failure/uncertainty is not replaced by a rollback error; persisted completion/ownership determines later processing, not an assertion that an external effect was reversed. |
| SCHED-012-AC-01 | persistence | Not tested | FailedAt is recorded immediately with that code/message and claim/lease/retry cleared, even below MaxAttempts. |
| SCHED-012-AC-02 | persistence | Not tested | the row remains nonterminal with NextAttemptAt, no claim/lease, and failure diagnostics; it cannot retry before that instant. |
| SCHED-012-AC-03 | persistence | Not tested | the schedule is failed rather than rescheduled; prior lease-loss claims count toward this decision. |
| SCHED-012-AC-04 | unit | Not tested | delays equal min(MaxDelay, Delay * 2^max(0, attempt-1)); zero delay is immediate and no random variation is introduced. |
| SCHED-012-AC-05 | persistence | Not tested | MessageException.Code or scheduling.unhandled is stored, code/message/exception type are limited to 200/500/500 characters, and ordinary exception text is not a guaranteed sanitized public message. |
| SCHED-012-AC-06 | persistence | Not tested | DispatchedAt is present and ownership/retry cleared, but Attempts and previous diagnostic values are retained. |
| SCHED-012-AC-07 | workflow | Not tested | the cancellation is not converted into a normal scheduling retry/failure; unfinished persisted work relies on lease/recovery unless already committed. |
| SCHED-012-AC-08 | persistence | Not tested | no terminal/retry state is inferred from a log; still-active work becomes claimable through expiry/recovery. |
| SCHED-013-AC-01 | unit | Not tested | Wait returns to permit re-evaluation, without itself completing or claiming any durable schedule. |
| SCHED-013-AC-02 | unit | Not tested | past wait returns without delay even if the token is already cancelled; future waiting observes cancellation rather than a false delivery success. |
| SCHED-013-AC-03 | workflow | Not tested | due processing can re-evaluate durable work; an explicit save alone does not establish commit. |
| SCHED-013-AC-04 | workflow | Not tested | eligible work is revisited using due/retry eligibility and recovery interval, not an indefinite wait for one lost notification. |
| SCHED-013-AC-05 | persistence | Not tested | it processes only actually committed eligible rows; waking is not proof of rollback or successful uncertain commit. |
| SCHED-013-AC-06 | persistence | Not tested | unsupported ownership is rejected instead of promising atomic processing; unrelated saves without changed schedules are not rejected by this scheduling check. |
| SCHED-013-AC-07 | workflow | Not tested | the worker can reevaluate after its recovery delay; fault delay does not terminally mark every schedule. |
| SCHED-014-AC-01 | unit | Not tested | scheduling uses that default scheduler; configuration is applied to unnamed/default composition rather than replacing it with message-specific jobs. |
| SCHED-014-AC-02 | workflow | Not tested | durable processing still selects from schedule storage; the hint does not contain or complete each scheduled payload. |
| SCHED-014-AC-03 | workflow | Not tested | the failure is logged and local timing/wake can release Wait so durable polling remains possible. |
| SCHED-014-AC-04 | workflow | Not tested | scheduling is attempted again; no adapter reconnect guarantee or permanently disabled state is implied. |
| SCHED-014-AC-05 | workflow | Not tested | cancellation propagates rather than being hidden as an ordinary Quartz availability warning. |
| SCHED-014-AC-06 | workflow | Not tested | hosting is configured to wait for jobs to complete; durable work recovery still depends on persisted state and cooperative cancellation, not forced job termination. |
| SCHED-016-AC-01 | persistence | Not tested | only terminal rows with terminal timestamp strictly before cutoff are deleted; equal/after and active rows remain. |
| SCHED-016-AC-02 | persistence | Not tested | deletion uses batches ordered by CreatedAt, stops at configured per-run cap with warning when capped, and leaves remaining work for later ticks. |
| SCHED-016-AC-03 | workflow | Not tested | no immediate startup purge is assumed; failed cleanup can retry on a later tick without changing active schedules into terminal ones. |
| SCHED-016-AC-04 | persistence | Not tested | the deleted schedule row no longer prevents reuse or supplies history; retained outgoing/timer evidence has its own independent lifetime. |
| SCHED-016-AC-05 | persistence | Not tested | it remains failed and is not automatically reopened, rescheduled or compensated. |

## Coverage and audit limitations

16 numbered specifications; 80 acceptance criteria, each mapped above. All 44 scheduling authored owned files (source and project; no owned Markdown files existed) were reviewed, excluding generated bin/obj. The audit manifest has 89 files in all six projects. No code, tests, builds, providers, broker or Quartz instance were executed. Static review cannot settle maintainer intent, prove SQL isolation/clock behaviour, or approve operational limits. Gap links target the shared GAPS page assembled separately; these files do not modify that page.
