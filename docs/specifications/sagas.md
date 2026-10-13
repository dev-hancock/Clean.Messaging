# Saga behavioural specifications

**Status: Draft — 2026-10-13. Source-reviewed; not tested.**

## Purpose and classification

Specifies durable saga definition/routing, consumer-owned state transitions, outgoing/timer intentions, owner transactions, concurrency, observation and terminal-state retention. It does not prescribe business deadlines, compensation or a domain-specific process.

This is a behavioural catalogue for a future TUnit suite, not an executable suite or approval of every implementation choice. **Required** is backed by explicit normative repository instructions or documented API contract; **Observed** records source-derived compatibility candidates requiring maintainer approval; **Proposed** is a conditional future policy; **Unknown** identifies undecided intent. ACs inherit their enclosing classification unless an explicit override is stated. Unknown behaviour has decision questions only, not acceptance criteria or verification-matrix rows. Observed reference pages are evidence of current description, not independent normative authority. A source-reviewed candidate is not a verified guarantee.

## Common contracts and preconditions

Unless a criterion deliberately violates one, use registered durable message contracts, a running composed host, configured relational context/schema/interceptors, valid options, UTC TimeProvider observations, cooperative cancellation, and the same supported scoped owner transaction for local participating state/effects. Public runtime registration alone is insufficient. Persistence isolation, collation, execution strategy, clocks and dependency behaviour require provider-specific validation; no provider compatibility matrix was tested. Consumer business policy and migrations remain outside these specifications.

## Common postconditions and invariants

Read results describe committed persisted state, not unsaved local mutation. Success is scoped to its named boundary: staging is not commit, schedule consumption is not downstream completion, and local commit is not exactly-once external effects. Retained logical IDs are not interchangeable with delivery or ownership IDs. Failure, cancellation, concurrency loss, replay and terminal outcomes remain distinct. No criterion requires internal class identity, private method shape, or handler call-count guarantees.

## Failure, concurrency and limitations

Exercise database/serialization/registration failures, duplicate identity, exact time/length/budget boundaries, cancelled I/O, competing claims/versions, owner rollback, uncertain commit, process loss and later recovery. Unknown and unresolved Proposed blocks use numbered decision questions without acceptance criteria or matrix rows. No unconditional assertion is approved before the linked gap decision. Scheduling has no renewal/upsert guarantee; saga state retention is not identity deletion. External custom delivery/handler work may repeat. These are specifications for observed outcomes, not tests executed during this audit.

### SAGA-001 — Workflow identity and local outgoing consistency

**Classification:** Required behaviour

**Contracts and applicability:** Consumer supplies business state/policy; stable effect identity is distinct from delivery identity. Transactional outbox guarantees apply only to the configured participating local transaction, never arbitrary external effects.

**Evidence:** docs/CODING_STANDARDS.md §§7–9: explicit transaction boundaries, stable logical identity, atomic outgoing work where outbox guarantees apply, cancellation classification.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SAGA-001-AC-01 — Effect identity survives retry

**Given** one logical saga Send/ timer operation is retried.
**When** its durable effect is captured again.
**Then** the logical effect/timer identity remains stable; a new delivery identity does not redefine it as a new logical operation.

#### SAGA-001-AC-02 — Local state and durable intentions are atomic

**Given** saga state, owning entry completion, outbox and timers use one supported configured scoped transaction.
**When** owner commits successfully.
**Then** participating state and durable effects commit together, not a state-only/outgoing-only successful transition.

#### SAGA-001-AC-03 — External effects remain outside rollback guarantee

**Given** a handler invokes an external service before local owner commit.
**When** owner rolls back or repeats invocation.
**Then** external application effects are not claimed to have rolled back or happened exactly once; their policy remains the consumer responsibility.

#### SAGA-001-AC-04 — Handler cancellation has distinct owner meaning

**Given** handler receives a cancelled processing token and propagates OperationCanceledException.
**When** owner rolls back.
**Then** the candidate consistent-with-standards outcome is no normal business SagaResult.Fail and no committed participating effects; actual owner cancellation must be verified separately from the conflict-lookup exception window.

### SAGA-002 — Definitions, handler overloads and registration validation

**Classification:** Observed behaviour

**Contracts and applicability:** AddSagas(callback, optionsCallback?); SagaBuilder.Add<TState>(type,callback); On<TMessage,THandler>(consumerId,correlate,create), On(consumerId,correlate), On(). ISagaHandler.Handle receives state/message/token and returns ValueTask<SagaResult>.

**Evidence:** DependencyInjection.cs; SagaBuilder.cs; SagaRegistry.cs; SagaDefinition.cs; SagaMessageDefinition.cs; SagaRuntimeValidation.cs; shared core registries.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SAGA-002-AC-01 — Definitions require valid type and callbacks

**Given** null callback, no definitions, no messages, blank/overlong/non-ASCII type or duplicate state/type.
**When** saga definitions are registered.
**Then** invalid configuration rejects; nonblank ASCII type length 200 is accepted, 201 rejects, and duplicate identities/types do not silently override.

#### SAGA-002-AC-02 — Message mapping is unique per definition

**Given** the same CLR message is registered twice by any combination of On overloads.
**When** the definition is built.
**Then** duplicate mapping rejects; another distinct message type can be registered.

#### SAGA-002-AC-03 — Start and continuation overloads install consumers

**Given** valid consumerId and correlation with optional create callback.
**When** ordinary message handling is composed.
**Then** with create it can start; without create it can only continue; required delegates reject null and consumer IDs obey global nonblank ASCII <=200 uniqueness.

#### SAGA-002-AC-04 — Timer-only mapping is instance-directed

**Given** On<TMessage,THandler>() without consumer identity/correlation.
**When** an ordinary message versus instance-directed timer is dispatched.
**Then** the mapping is available to that saga timer but supplies no ordinary durable consumer by itself.

#### SAGA-002-AC-05 — All mappings require retained contracts

**Given** a definition includes a timer-only message without registered durable contract.
**When** saga startup validation runs.
**Then** the missing contract fails configuration validation rather than being silently exempt because no ordinary consumer exists.

#### SAGA-002-AC-06 — Registered handler is scope-resolved

**Given** a host supplied valid handlers and a prior scoped handler registration.
**When** a message is handled.
**Then** the configured handler service is used in the owner scope; registering a saga does not make handler/context shared state thread-safe.

### SAGA-003 — Composition, persistence mapping and saga options

**Classification:** Observed behaviour

**Contracts and applicability:** Runtime AddSagas adds runtime scheduling but not outbox/EF stores. EF AddSagas on MessagingBuilder, ModelBuilder and typed/untyped DbContextOptionsBuilder includes scheduling integration. SagaOptions exposes Section, SerializerOptions, Retention, CleanupInterval and CleanupBatchSize.

**Evidence:** Both DependencyInjection.cs; SagaValidation.cs; SagaRuntimeValidation.cs; SagaConfiguration.cs; SagaOptions.cs; SagaOptionsValidator.cs.

**Intent gaps:** [GAP-WF-013](GAPS.md#gap-wf-013).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SAGA-003-AC-01 — Runtime composition is incomplete without stores/effects

**Given** runtime saga registration exists without saga persistence, writer or outgoing services.
**When** startup resolves saga integration.
**Then** required dependencies prevent valid durable startup; registration is not automatic outbox installation.

#### SAGA-003-AC-02 — EF model and interceptors include timers

**Given** a valid relational context uses saga EF builder/model/options composition.
**When** state and timers are saved via typed or untyped overload.
**Then** saga and scheduling mappings/interception participate together; same installed hook is not duplicated; missing model/interceptors/nonrelational provider fail validation.

#### SAGA-003-AC-03 — Defaults and explicit binding

**Given** fresh saga options or IConfiguration with Messaging:Sagas but no binding callback.
**When** saga composition reads options.
**Then** JSON web defaults, retention 30 days, cleanup interval 1 hour and batch 500 apply unless explicitly configured; Section alone does not bind values.

#### SAGA-003-AC-04 — Cleanup numeric boundaries

**Given** Retention/CleanupInterval/BatchSize are individually zero or negative then positive.
**When** startup validation runs.
**Then** zero/negative values reject and positive values pass; no saga-specific retry/lease/concurrency options are inferred.

### SAGA-004 — Identity values and durable correlation

**Classification:** Observed behaviour

**Contracts and applicability:** SagaId.Value/New/D-format text; SagaKey.Value/From/preserved text. Ordinary correlation uses the configured function, not message CorrelationId. Uniqueness is persisted (Type,Key), including terminal records.

**Evidence:** SagaId.cs; SagaKey.cs; SagaProcessor.cs; SagaRuntime.cs; SagaConfiguration.cs.

**Intent gaps:** [GAP-WF-001](GAPS.md#gap-wf-001), [GAP-WF-007](GAPS.md#gap-wf-007), [GAP-WF-011](GAPS.md#gap-wf-011).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SAGA-004-AC-01 — Key boundaries preserve Unicode and exact text

**Given** nonblank Unicode/text keys at 500 characters, blank/null and 501 characters.
**When** SagaKey constructor or From is used.
**Then** 500 characters are accepted unchanged including Unicode; blank/null and 501 reject; no trim/case-normalisation is implied.

#### SAGA-004-AC-02 — Instance identity formatting

**Given** new and directly supplied SagaId values including Guid.Empty.
**When** Value and ToString are read.
**Then** Value is preserved and text uses GUID D format; New supplies a nonempty GUID, not a correlation key.

#### SAGA-004-AC-03 — Correlation function chooses workflow

**Given** two messages share metadata CorrelationId but correlate to different keys or conversely.
**When** ordinary durable saga handling occurs.
**Then** the definition-specific key controls instance lookup; metadata correlation does not merge or split instances by itself.

#### SAGA-004-AC-04 — Ordinary message needs durable trigger identity

**Given** no message MessageId is in the execution context.
**When** ordinary saga consumer handling is attempted.
**Then** handling rejects the absent durable trigger identity instead of inventing a start identity.

#### SAGA-004-AC-05 — Terminal key still occupies durable identity

**Given** an active or terminal row has a type/key.
**When** another start tries the same type/key.
**Then** the retained unique identity prevents a second committed row, even if payload state was purged.

### SAGA-005 — Start, continuation and duplicate routing matrix

**Classification:** Observed behaviour

**Contracts and applicability:** Start is defined by correlate+create. StartedByMessageId identifies the original incoming delivery, not EventId. Continuations have no per-saga consumed-message ledger.

**Evidence:** SagaMessageDefinition.cs; SagaRuntime.Create/Find; SagaProcessor.cs; SagaEntry.cs; SagaConsumer.cs.

**Intent gaps:** [GAP-WF-012](GAPS.md#gap-wf-012).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SAGA-005-AC-01 — Absent start creates initial state

**Given** no persisted type/key and a nonnull state factory result.
**When** a start is handled and owner commits.
**Then** a new SagaId, the incoming MessageId as start identity, initial timestamps and handler-produced state/outcome are retained.

#### SAGA-005-AC-02 — Same start identity is replay no-op

**Given** active or terminal saga has StartedByMessageId X.
**When** start message X is handled again.
**Then** no new saga/state transition/outgoing effects are committed, including when terminal state was purged.

#### SAGA-005-AC-03 — Different active start conflicts

**Given** active saga at type/key was started by X.
**When** start Y != X arrives.
**Then** saga.start.conflict is permanent and exposes type/key; the existing workflow is not overwritten.

#### SAGA-005-AC-04 — Different terminal start is closed

**Given** completed/failed saga at type/key was started by X.
**When** start Y != X arrives.
**Then** saga.start.closed is permanent; completion/failure is not reopened.

#### SAGA-005-AC-05 — Absent continuation is transient missing

**Given** no persisted saga at the correlated type/key.
**When** continuation arrives.
**Then** saga.not_found is retry-classified with no new saga or effects, allowing a later committed start to make a retry meaningful.

#### SAGA-005-AC-06 — Active continuation can transition

**Given** a persisted active saga exists.
**When** a continuation handles state and owner commits.
**Then** state/outcome/effects reflect that handler result and the persisted version advances.

#### SAGA-005-AC-07 — Terminal continuation is ignored

**Given** a terminal saga exists, possibly with purged JSON state.
**When** continuation arrives.
**Then** no handler state transition/effects or reopening is committed.

#### SAGA-005-AC-08 — General duplicate continuation belongs to owner deduplication

**Given** an active saga receives the same continuation in separate eligible owner executions.
**When** both executions are admitted.
**Then** no per-saga message ledger guarantees suppression; inbox/outbox owner identity and fencing, not the saga key alone, govern duplicate processing.

### SAGA-006 — Handler results and immutable effect descriptions

**Classification:** Observed behaviour

**Contracts and applicability:** SagaResult.Continue/Complete/Fail, implicit ValueTask conversion; Send(Guid,payload)/Send(SagaMessage), Schedule(Guid,due,payload)/Schedule(SagaSchedule), Cancel(Guid). SagaMessage exposes ID/payload/optional correlation/causation; SagaSchedule exposes ID/due/payload; SagaCancel exposes TimerId. The public effect hierarchy is not an external apply-plugin port.

**Evidence:** ISagaHandler.cs; SagaResult.cs; SagaEffect.cs; SagaAttempt.cs; SagaRuntime.cs.

**Intent gaps:** [GAP-WF-008](GAPS.md#gap-wf-008).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SAGA-006-AC-01 — Continue complete and fail transition outcomes

**Given** active state and a handler result Continue, Complete or valid Fail.
**When** the owner commits the transition.
**Then** Continue remains active, Complete sets completion and clears diagnostics, Fail sets failure/code/message; terminal outcome does not wait for external delivery.

#### SAGA-006-AC-02 — Effect append preserves original result

**Given** a reusable result and added send/schedule/cancel descriptions.
**When** the original and augmented results are used separately.
**Then** the original produces only its original intentions; the augmented one adds effects in append order without mutating the original description.

#### SAGA-006-AC-03 — Effect overloads describe equivalent operations

**Given** equivalent values supplied through primitive and object Send/Schedule overloads.
**When** the returned result is processed.
**Then** both forms capture the same logical ID/payload/due/explicit metadata rather than differing durable obligations.

#### SAGA-006-AC-04 — Empty IDs and null effects reject

**Given** Guid.Empty Send/Schedule/Cancel, null payload or null message/schedule object.
**When** the corresponding public result API is used.
**Then** invalid argument is rejected before a valid effect description is appended; past due alone is permitted.

#### SAGA-006-AC-05 — Null state factory and null handler result fail

**Given** a start factory returns null or a handler returns null.
**When** handling is attempted.
**Then** the invalid result/state is rejected and no successful owner transition/effects commit.

#### SAGA-006-AC-06 — Throwing handler does not apply a returned result

**Given** handler mutates its deserialized state and throws or observes cancellation.
**When** the enclosing owner rolls back.
**Then** no mutated saga state or result effects become durable; ordinary exception and cancellation follow their distinct owner classifications.

#### SAGA-006-AC-07 — Public cancellation descriptor is not direct effect execution

**Given** new SagaCancel(Guid.Empty) or a custom effect intent is constructed externally.
**When** the supported result APIs are considered.
**Then** constructor alone does not reject empty SagaCancel, but supported Result.Cancel rejects it; no public append/apply port makes arbitrary SagaEffect subclass execution a supported plugin contract.

### SAGA-007 — State serialization and retained schema compatibility

**Classification:** Observed behaviour

**Contracts and applicability:** SagaOptions.SerializerOptions independently configure state JSON. Durable message payload serialization uses registered message contracts; numeric Version is optimistic concurrency, not JSON schema version.

**Evidence:** SagaStateSerializer.cs; SagaAttempt.cs; SagaRuntime.cs; SagaEntry.cs; shared IMessageSerializer.

**Intent gaps:** [GAP-WF-015](GAPS.md#gap-wf-015).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SAGA-007-AC-01 — State mutations round-trip using state options

**Given** active valid state and configured JSON options.
**When** handler mutates state and owner commits then later continues.
**Then** the persisted JSON can reconstruct that configured state and its committed mutations; outgoing serializer options are not implicitly the state options.

#### SAGA-007-AC-02 — Malformed null or unsupported state is permanent

**Given** retained active state JSON is malformed/null or state serialization is unsupported.
**When** state deserialize/serialize is attempted.
**Then** saga.serialization.failed is permanent, with no successful owner state/effects transition.

#### SAGA-007-AC-03 — Retained type and message compatibility are required

**Given** retained active saga type/JSON or timer contract no longer resolves compatibly.
**When** execution is attempted.
**Then** the corresponding definition/state/message error fails rather than automatically migrating, upcasting or inventing a replacement state.

#### SAGA-007-AC-04 — Terminal replay does not need purged state

**Given** terminal saga state is the tombstone JSON null.
**When** same-start replay, terminal continuation or terminal timer is handled.
**Then** the terminal no-op path does not reopen or require reconstructing purged state.

### SAGA-008 — State application and terminal invariants

**Classification:** Observed behaviour

**Contracts and applicability:** A handler result describes one attempted transition. State is applied only to an active saga at its expected version; state application precedes outcome/effect capture, without independently committing the unit of work.

**Evidence:** SagaRuntime.cs; SagaAttempt.cs; SagaCommit.cs; SagaEntry.ApplyState/Complete/Fail.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SAGA-008-AC-01 — Active result applies one candidate state transition

**Given** an active saga and a result computed from its expected version.
**When** the candidate transition is applied and owner commits.
**Then** serialized state, advanced version and UpdatedAt describe the same committed transition; an effect failure still requires owner rollback.

#### SAGA-008-AC-02 — Rejected expected version cannot overwrite state

**Given** a candidate was computed for an earlier version than the saga now carries.
**When** that candidate is applied.
**Then** state/outcome/effects from the stale candidate are rejected rather than silently replacing the newer candidate; database concurrency remains a separate commit guard.

#### SAGA-008-AC-03 — Terminal state cannot accept an active transition

**Given** a saga is already complete or failed.
**When** a candidate result attempts state application.
**Then** the terminal instance cannot be reopened by that candidate; terminal replay routing remains a no-op before normal state application.

#### SAGA-008-AC-04 — Result construction is not state mutation

**Given** Continue/Complete/Fail and appended intentions have been constructed without processing.
**When** stored saga snapshot is read.
**Then** the result description alone has not changed persisted state/outcome/version or staged an external publication.

### SAGA-009 — Outgoing metadata, timers and terminal effect ordering

**Classification:** Observed behaviour

**Contracts and applicability:** Send logical ID becomes EventId; explicit metadata wins, otherwise trigger correlation and trigger MessageId supply defaults. Timer ID is a global ScheduleId; group is SagaId, target saga. Effect order is append order followed by terminal timer-group cancellation.

**Evidence:** SagaEffectWriter.cs; SagaResult.cs; SagaRuntime.cs; SagaTrigger.cs; ScheduleWriter.cs; OutboxEntryFactory.cs.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SAGA-009-AC-01 — Send identity and explicit metadata

**Given** handler sends ID X with explicit correlation C and causation D.
**When** owner commits outgoing staging.
**Then** outgoing EventId is X, generated MessageId is delivery identity, and C/D win over trigger metadata.

#### SAGA-009-AC-02 — Default send uses triggering delivery causation

**Given** trigger has MessageId X, correlation C and a different causation D; Send omits metadata.
**When** outgoing work is captured.
**Then** correlation inherits C and causation inherits X, not D; when values are null the outgoing context fallback still belongs to outbox.

#### SAGA-009-AC-03 — Timer description routes by instance

**Given** handler schedules timer X due at offset instant with trigger metadata.
**When** owner commits.
**Then** schedule ID is X, UTC due is equivalent instant, target is saga, group identifies the SagaId, correlation follows trigger and causation is trigger MessageId.

#### SAGA-009-AC-04 — Cancel respects timer scope and current trigger

**Given** two saga instances or targets share cancellation-related IDs and one is current triggering timer.
**When** a handler cancels timer IDs.
**Then** only matching saga group/target active timers cancel; cancelling the current triggering timer is a no-op, and unrelated timers remain.

#### SAGA-009-AC-05 — Terminal sends are not discarded

**Given** a Complete/Fail result contains sends and schedules.
**When** owner commits effects and terminal outcome.
**Then** sends remain staged, newly scheduled and other nonterminal timers in that saga group are cancelled after appended effects, excluding the current triggering timer.

#### SAGA-009-AC-06 — Timer identity collision is not update

**Given** two results/sagas use the same global timer ID or a retained timer already has it.
**When** effects are saved.
**Then** there is no timer upsert; identity conflict can fail save and participating state/effects require owner rollback.

#### SAGA-009-AC-07 — Order has observable cancellation consequences

**Given** Continue result schedules then cancels a timer versus cancels then schedules a previously absent timer.
**When** each isolated owner commits.
**Then** schedule-then-cancel leaves it cancelled; cancel-then-schedule can leave the newly staged timer active, subject to identity uniqueness.

### SAGA-010 — Timer delivery, business failure and exhaustion

**Classification:** Observed behaviour

**Contracts and applicability:** Timers use the scheduling delivery contract. Schedule GroupId routes to SagaId and schedule Id is both trigger MessageId and TimerId. Any mapped On overload can be instance-directed. Delivery failure is not SagaResult.Fail.

**Evidence:** SagaScheduleDelivery.cs; SagaProcessor.cs; SagaDefinition.cs; SagaTrigger.cs; SchedulingWorker.cs; ScheduleExecutor.cs.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SAGA-010-AC-01 — Missing timer group is permanent

**Given** a saga-target schedule lacks GroupId.
**When** timer delivery runs.
**Then** saga.serialization.failed faults timer delivery without a saga state transition.

#### SAGA-010-AC-02 — Missing timer instance can retry

**Given** a valid group identifies no saga.
**When** timer delivery occurs.
**Then** saga.not_found is transient; the timer may retry under scheduling policy, without constructing a saga from timer payload.

#### SAGA-010-AC-03 — Active timer invokes mapped handler by instance

**Given** active saga and valid retained mapped timer payload.
**When** timer is delivered.
**Then** that instance receives the mapped message without correlation/create routing; timer identity is the triggering delivery identity for resulting effects.

#### SAGA-010-AC-04 — Terminal timer completes without reopening

**Given** terminal saga and even obsolete message mapping/contract.
**When** timer arrives.
**Then** the terminal instance is ignored before active payload/state handling and the timer can complete without new workflow effects.

#### SAGA-010-AC-05 — Unknown active mapping is a delivery fault

**Given** active saga type/mapped handler or retained payload is invalid.
**When** timer delivery resolves it.
**Then** definition/handler/type mismatch faults as saga.serialization.failed or shared serializer contract error; it is not successful timer consumption.

#### SAGA-010-AC-06 — Exhausted timer leaves active business state

**Given** timer fails transiently until scheduling retry exhaustion or permanently before a committed handler outcome.
**When** failure settles.
**Then** timer is failed but saga remains active; no automatic SagaResult.Fail, compensation or timeout escalation is inferred.

#### SAGA-010-AC-07 — Timer handler and consumption join owner commit

**Given** timer handler changes state and captures outgoing work.
**When** schedule delivery transaction succeeds or rolls back.
**Then** state/effects and current timer consumption commit together on success, or remain uncommitted on rollback.

### SAGA-011 — Owner transactions, rollback, savepoints and replay

**Classification:** Observed behaviour

**Contracts and applicability:** Saga runtime stages state/effects without independently calling save/starting a business transaction. Inbox/local-outbox dispatchers and schedule executor own supported local transactions. EF saga save hook rejects changed sagas in ambient transactions.

**Evidence:** SagaRuntime.cs; SagaAttempt.cs; SagaSaveInterceptor.cs; InboxDispatcher.cs; LocalOutboxDelivery.cs; ScheduleExecutor.cs; OutboxTransactionInterceptor.cs.

**Intent gaps:** [GAP-WF-006](GAPS.md#gap-wf-006).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SAGA-011-AC-01 — No implicit commit from result application

**Given** valid handling stages saga/outgoing/timer state.
**When** the caller has not saved or committed.
**Then** separate committed readers do not see a completed workflow transition; result construction/handling is not transaction commit.

#### SAGA-011-AC-02 — Inbox and local-outbox owners capture all local state

**Given** handler changes saga, business entity, outgoing work and timers in configured owner scope.
**When** entry dispatch commits successfully.
**Then** participating business/saga/effects and owned entry completion are durable together; another context/network effect has no such guarantee.

#### SAGA-011-AC-03 — Failed effect rolls back earlier applied local changes

**Given** result state and earlier send effect were staged but a later timer/effect/save fails.
**When** owner rolls back.
**Then** none of the attempt local state or effects is durably committed merely because an earlier effect was staged.

#### SAGA-011-AC-04 — Execution strategy may repeat handler

**Given** a local transactional invocation ran but its strategy repeats before a known committed completion.
**When** recovery executes the owner.
**Then** handler invocation can repeat; only committed owner fencing/state/effects determine successful local completion.

#### SAGA-011-AC-05 — Ambient changed-saga save is unsupported

**Given** a changed saga participates in ambient TransactionScope.
**When** sync or async SaveChanges begins.
**Then** the saga integration rejects unsupported ambient ownership; unrelated no-saga saves are not rejected by this hook.

#### SAGA-011-AC-06 — Outbox composition rejects caller savepoint operations

**Given** saga/schedule/outbox share configured transaction integration.
**When** caller creates or rolls back to a savepoint outside active save.
**Then** outbox integration rejects that unsupported operation; EF internal savepoints during saves are distinct and permitted.

### SAGA-012 — State version races and concurrent continuation

**Classification:** Observed behaviour

**Contracts and applicability:** Version increments on applied state; an attempt requires active state and its expected version. EF optimistic concurrency fences commit, not all handler invocation. No saga-specific reload/retry loop or serialized external-effect guarantee is supplied.

**Evidence:** SagaAttempt.cs; SagaCommit.cs; SagaRuntime.cs; SagaEntry.ApplyState; SagaConfiguration.cs; SagaSaveInterceptor.cs.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SAGA-012-AC-01 — One applied transition advances version

**Given** active version V and a valid handler result.
**When** owner commits.
**Then** version becomes V+1 with UpdatedAt and result state/outcome; newly started version zero is advanced by its first applied handler transition.

#### SAGA-012-AC-02 — Concurrent continuations fence committed state

**Given** two contexts load active version V and produce different outcomes/effects.
**When** they compete to save/commit.
**Then** one conflicting commit is rejected by optimistic concurrency instead of overwriting the winning state/effects; handlers may both have run.

#### SAGA-012-AC-03 — Terminal race cannot reopen state

**Given** one attempt completes/fails while a stale continuation still carries expected active version.
**When** stale attempt applies or saves.
**Then** it cannot commit a reopened active workflow over the winning terminal transition; conflict/owner policy determines recovery.

#### SAGA-012-AC-04 — Retries belong to processing owner

**Given** a saga version conflict is surfaced during owner dispatch.
**When** owner applies its retry policy later.
**Then** no saga-only immediate reload/reinvoke is assumed; a later eligible attempt observes then-current durable state.

#### SAGA-012-AC-05 — Rejected ownership undoes local workflow commit

**Given** handler ran but owning inbox/outbox/schedule completion is rejected.
**When** the owner rolls back.
**Then** participating saga/effect changes are not committed, although a nontransactional handler effect may already exist.

### SAGA-013 — Start collision translation and recovery outcomes

**Classification:** Observed behaviour

**Contracts and applicability:** Save-failure translation consults a fresh persisted winner for exactly one distinct Added saga identity in DbUpdateException. Same original start becomes retry-classified saga.start.race; different active/terminal winner becomes Conflict/Closed. It is not a generic upsert or blanket database error conversion.

**Evidence:** SagaConflictResolver.cs; ISagaConflictResolver.cs (internal); SagaRaceException.cs; SagaSaveInterceptor.cs; SagaConfiguration.cs.

**Intent gaps:** [GAP-WF-009](GAPS.md#gap-wf-009), [GAP-WF-010](GAPS.md#gap-wf-010).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SAGA-013-AC-01 — Same-start insertion race is transient

**Given** two contexts insert the same type/key/StartedByMessageId and one commits.
**When** loser save fails with one qualifying added identity and finds winner.
**Then** failure is translated to retry-classified saga.start.race; a later replay sees the retained start and is a no-op.

#### SAGA-013-AC-02 — Different-start active race is permanent conflict

**Given** two different start identities compete for the same type/key.
**When** loser resolves an active persisted winner.
**Then** saga.start.conflict is permanent and exposes the winning type/key boundary without replacing its state.

#### SAGA-013-AC-03 — Winner terminal race is permanent closed

**Given** different start lost to a now-terminal persisted workflow.
**When** fresh conflict lookup succeeds.
**Then** saga.start.closed is permanent, not reopening or an idempotent acknowledgement of the different start.

#### SAGA-013-AC-04 — Nonqualifying save failures retain original error

**Given** error is not DbUpdateException, has zero/multiple distinct Added starts, has no persisted winner or lookup itself fails.
**When** translation is considered.
**Then** the original failure is preserved rather than fabricating conflict/closed/race without qualifying evidence.

#### SAGA-013-AC-05 — Sync and async collision paths agree

**Given** equivalent qualifying insertion races via SaveChanges and SaveChangesAsync.
**When** winner lookup succeeds.
**Then** both expose the same race/conflict/closed classification for the same retained start relationship.

#### SAGA-013-AC-06 — Public exception classification is compatible

**Given** missing saga by ID or type/key, conflict, closed and serialization exception are constructed.
**When** their public Code/Action/IsTransient and identity fields are read.
**Then** as a candidate compatibility baseline, missing is saga.not_found/Retry, conflict saga.start.conflict/Fault, closed saga.start.closed/Fault and serialization saga.serialization.failed/Fault; this fact does not decide malformed-result policy.

### SAGA-014 — Failures, cancellation and malformed result intent

**Classification:** Unknown behaviour

**Contracts and applicability:** Public SagaNotFoundException is Retry; SagaConflictException and SagaClosedException are Fault and expose SagaType/Key; SagaSerializationException is Fault with optional inner exception. Observed malformed-result discrepancy: SagaFailure validates diagnostics but SagaResult.Fail does not use it. Conflict lookup catches cancellation and preserves original save error.

**Evidence:** Exceptions/*.cs; SagaFailure.cs; SagaResult.Fail; SagaEntry.Fail; SagaConflictResolver.Find; gaps GAP-WF-008 and GAP-WF-010.

**Intent gaps:** [GAP-WF-008](GAPS.md#gap-wf-008), [GAP-WF-010](GAPS.md#gap-wf-010).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### Question SAGA-014-Q-01 ? Validated diagnostic value versus raw Fail result

**Scenario:** null/blank code/message, code lengths 200/201 and message lengths 500/501; SagaFailure is constructed versus SagaResult.Fail used.
**Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-008, if current disparity is retained, SagaFailure rejects invalid/overlong values while Fail accepts them until provider/state persistence; tests must not impose universal provider rejection?
**Verification:** no acceptance criterion or test outcome is approved until the linked gap is resolved.

#### Question SAGA-014-Q-02 ? Cancelled conflict lookup preserves save error today

**Scenario:** qualifying DbUpdateException and caller cancellation during fresh winner lookup; async collision resolution runs.
**Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-010, if current fallback is retained, the original DbUpdateException remains instead of cancellation propagation; normative cancellation policy needs a deliberate decision?
**Verification:** no acceptance criterion or test outcome is approved until the linked gap is resolved.

### SAGA-015 — Undecided identity, schema and diagnostic hardening

**Classification:** Proposed behaviour

**Contracts and applicability:** Proposals require approval, not compatibility obligations: revalidate default SagaKey; define persisted equality; validate Fail consistently; constrain collision translation; define unsaved repeated-start handling and retained schema migration policy.

**Evidence:** GAP-WF-001, GAP-WF-007, GAP-WF-008, GAP-WF-009, GAP-WF-012, GAP-WF-015; SagaKey, SagaResult, SagaConflictResolver, SagaStore and state serializer.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### Question SAGA-015-Q-01 ? — Default key admission policy

**Scenario:** correlation returns default(SagaKey) bypassing constructor validation. admission hardening is approved.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-001 approves revalidation, handling rejects the invalid key before durable creation; current code does not promise this rejection.
**Verification:** no acceptance criterion or test outcome is approved before the linked gap decision.

#### Question SAGA-015-Q-02 ? — Persisted equality policy

**Scenario:** keys/types differ only by case or provider collation treatment. a supported equality policy is chosen.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-007 approves ordinal persisted semantics, those identities remain distinct consistently across providers; current collation is provider-owned.
**Verification:** no acceptance criterion or test outcome is approved before the linked gap decision.

#### Question SAGA-015-Q-03 ? — Fail diagnostic consistency policy

**Scenario:** Fail receives invalid diagnostic values. GAP-WF-008 chooses validation parity.
**Policy question:** Should the following conditional proposal be adopted: only under that decision, malformed Fail rejects before applying state/effects using agreed 200/500 bounds; this is not current validated behaviour.
**Verification:** no acceptance criterion or test outcome is approved before the linked gap decision.

#### Question SAGA-015-Q-04 ? — Narrow collision translation policy

**Scenario:** unrelated DbUpdateException has one added start and a coincidental persisted winner. translation hardening is selected.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-009 restricts translation to proven identity collisions, the unrelated original error remains; current matching is not unique-error-code-specific.
**Verification:** no acceptance criterion or test outcome is approved before the linked gap decision.

#### Question SAGA-015-Q-05 ? — Unsaved repeated-start policy

**Scenario:** two starts for one type/key are handled in one scope before save. GAP-WF-012 defines tracked-new-instance semantics.
**Policy question:** Should the following conditional proposal be adopted: only under the selected policy, the second operation either observes tracked start replay/conflict or deliberately fails the unit of work; current persisted query does not explicitly merge added rows.
**Verification:** no acceptance criterion or test outcome is approved before the linked gap decision.

#### Question SAGA-015-Q-06 ? — State evolution policy

**Scenario:** retained state/message schemas differ from a new deployment. GAP-WF-015 selects compatibility or migration approach.
**Policy question:** Should the following conditional proposal be adopted: only under the selected policy, compatible work remains interpretable or an explicit migration/rejection occurs; numeric Version is not an automatic schema migration mechanism.
**Verification:** no acceptance criterion or test outcome is approved before the linked gap decision.

### SAGA-016 — Persisted manager and timer snapshots

**Classification:** Observed behaviour

**Contracts and applicability:** ISagaManager.Get(id,token), GetActive(limit=100,token), GetFailed(limit=100,token), GetTimers(id,token). Saga snapshot exposes Id/Type/Key/Version/timestamps/code/message and IsActive/IsTerminal, not state/start identity. SagaTimer exposes ID/owner/type/contract/due/attempt/retry/consumed/cancelled/failed/code/message, not payload/ownership metadata.

**Evidence:** ISagaManager.cs; Saga.cs; SagaTimer.cs; SagaManager.cs; SagaStore.Get/GetActive/GetFailed; ScheduleStore.GetGroup.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SAGA-016-AC-01 — Get and absent timer listing are nonmutating

**Given** persisted active/terminal/absent SagaId.
**When** Get or GetTimers completes.
**Then** Get returns the matching snapshot or null; absent saga yields no timers; reads do not reopen/create/purge work.

#### SAGA-016-AC-02 — Lists filter and bound persisted state

**Given** mixed active/complete/failed rows and positive requested limit.
**When** GetActive or GetFailed reads.
**Then** active list excludes all terminal rows and orders CreatedAt ascending; failed list excludes active/complete and orders FailedAt descending; no more than limit are returned, ties unspecified.

#### SAGA-016-AC-03 — Limits reject nonpositive values

**Given** limit zero or negative versus omitted or positive.
**When** GetActive/GetFailed is called.
**Then** nonpositive limit rejects; omitted limit is 100; positive limit bounds persisted results.

#### SAGA-016-AC-04 — Timer projection uses saga target and history

**Given** a saga has active and terminal timers plus other-target schedules.
**When** GetTimers reads.
**Then** only saga group/target timers appear ordered due then ID, ConsumedAt reflects schedule dispatch, and IsTerminal is true for consumed/cancelled/failed timestamps.

#### SAGA-016-AC-05 — Snapshot flags and unsaved changes

**Given** persisted rows have completion/failure/null terminal timestamps while local changes remain unsaved.
**When** manager reads snapshots.
**Then** IsActive iff both saga terminal timestamps are absent, IsTerminal is inverse; persisted values are returned without unsaved-state merge or mutable tracked state exposure.

#### SAGA-016-AC-06 — Read failure and cancellation are not empty success

**Given** manager database I/O fails or is cancelled.
**When** Get/list/timer read runs.
**Then** failure or cancellation propagates; only genuinely absent persisted data becomes null/empty.

### SAGA-017 — Retained tombstones, purge boundaries and lifecycle limits

**Classification:** Observed behaviour

**Contracts and applicability:** Saga cleanup purges JSON state of old terminal records, not identity/tombstones. Scheduling retention independently deletes timer rows. There is no public reopen/reset/force-result/replay/identity-delete/state-migration or stuck-workflow repair command.

**Evidence:** SagaStore.PurgeState; SagaCleanupWorker.cs; SagaEntry.cs; SagaConfiguration.cs; ISagaManager.cs; Scheduling cleanup.

**Intent gaps:** [GAP-WF-011](GAPS.md#gap-wf-011).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

#### SAGA-017-AC-01 — Strict terminal purge cutoff

**Given** completed/failed rows with terminal timestamp before/equal/after cutoff and old active rows.
**When** a purge batch runs.
**Then** only previously unpurged terminal state strictly before cutoff is replaced with JSON null; equality/after/active rows retain their state.

#### SAGA-017-AC-02 — Purge retains identity and lifecycle diagnostics

**Given** an eligible terminal saga is purged.
**When** persisted snapshot and uniqueness are read.
**Then** StatePurgedAt records purge time; type/key/start identity/outcome/diagnostics remain, and Version and UpdatedAt are not incremented by purge.

#### SAGA-017-AC-03 — Purge is periodic bounded and recoverable

**Given** host starts or purge fails with more eligible rows than batch.
**When** cleanup lifecycle continues.
**Then** no immediate startup purge is assumed; one configured batch ordered by UpdatedAt is attempted per tick; failure retries on later tick.

#### SAGA-017-AC-04 — Tombstone blocks key restart indefinitely today

**Given** a terminal tombstone survives far beyond retention.
**When** same start, different start or continuation arrives.
**Then** same start remains no-op, different start remains closed and continuation remains ignored; retention does not free that type/key.

#### SAGA-017-AC-05 — Timer history has independent retention

**Given** saga tombstone survives while old terminal timer rows are deleted.
**When** GetTimers and Get are read.
**Then** saga identity/outcome can remain while timer history is absent; this is not a complete indefinite workflow audit trail.

#### SAGA-017-AC-06 — Active failed-timer workflow needs application policy

**Given** saga remains active with failed/exhausted timers and no later business message.
**When** cleanup and manager reads occur.
**Then** active saga is not automatically purged, failed, compensated or repaired; manager APIs provide observation, not a business policy engine.

## Public contract and code-path traceability

Each authored owned file was reviewed in full, including internal implementation, project references and friend-assembly declarations. The grouped public contracts appear in the audit manifest; this table ties every owned path to outcome specifications. Shared message contracts/options/outbox/owner ports retain their owning capability specifications rather than becoming new workflow APIs.

| Reviewed source/project | Behaviour specifications |
| --- | --- |
| [src\Clean.Messaging.Sagas\Clean.Messaging.Sagas.csproj](../../src/Clean.Messaging.Sagas/Clean.Messaging.Sagas.csproj) | SAGA-002, SAGA-003 |
| [src\Clean.Messaging.Sagas\Configuration\SagaOptions.cs](../../src/Clean.Messaging.Sagas/Configuration/SagaOptions.cs) | SAGA-003, SAGA-007 |
| [src\Clean.Messaging.Sagas\Configuration\SagaOptionsValidator.cs](../../src/Clean.Messaging.Sagas/Configuration/SagaOptionsValidator.cs) | SAGA-003 |
| [src\Clean.Messaging.Sagas\Configuration\SagaRuntimeValidation.cs](../../src/Clean.Messaging.Sagas/Configuration/SagaRuntimeValidation.cs) | SAGA-002, SAGA-003 |
| [src\Clean.Messaging.Sagas\Definition\SagaDefinition.cs](../../src/Clean.Messaging.Sagas/Definition/SagaDefinition.cs) | SAGA-002, SAGA-008, SAGA-010 |
| [src\Clean.Messaging.Sagas\Definition\SagaMessageDefinition.cs](../../src/Clean.Messaging.Sagas/Definition/SagaMessageDefinition.cs) | SAGA-002, SAGA-005, SAGA-015 |
| [src\Clean.Messaging.Sagas\Definition\SagaRegistry.cs](../../src/Clean.Messaging.Sagas/Definition/SagaRegistry.cs) | SAGA-002, SAGA-004, SAGA-008 |
| [src\Clean.Messaging.Sagas\Delivery\SagaScheduleDelivery.cs](../../src/Clean.Messaging.Sagas/Delivery/SagaScheduleDelivery.cs) | SAGA-010 |
| [src\Clean.Messaging.Sagas\DependencyInjection.cs](../../src/Clean.Messaging.Sagas/DependencyInjection.cs) | SAGA-002, SAGA-003 |
| [src\Clean.Messaging.Sagas\Effects\SagaEffect.cs](../../src/Clean.Messaging.Sagas/Effects/SagaEffect.cs) | SAGA-006, SAGA-009 |
| [src\Clean.Messaging.Sagas\Effects\SagaEffectWriter.cs](../../src/Clean.Messaging.Sagas/Effects/SagaEffectWriter.cs) | SAGA-001, SAGA-009 |
| [src\Clean.Messaging.Sagas\Effects\SagaTimer.cs](../../src/Clean.Messaging.Sagas/Effects/SagaTimer.cs) | SAGA-016 |
| [src\Clean.Messaging.Sagas\Exceptions\SagaClosedException.cs](../../src/Clean.Messaging.Sagas/Exceptions/SagaClosedException.cs) | SAGA-005, SAGA-013, SAGA-014 |
| [src\Clean.Messaging.Sagas\Exceptions\SagaConflictException.cs](../../src/Clean.Messaging.Sagas/Exceptions/SagaConflictException.cs) | SAGA-005, SAGA-013, SAGA-014 |
| [src\Clean.Messaging.Sagas\Exceptions\SagaNotFoundException.cs](../../src/Clean.Messaging.Sagas/Exceptions/SagaNotFoundException.cs) | SAGA-005, SAGA-010, SAGA-014 |
| [src\Clean.Messaging.Sagas\Exceptions\SagaSerializationException.cs](../../src/Clean.Messaging.Sagas/Exceptions/SagaSerializationException.cs) | SAGA-007, SAGA-010, SAGA-014 |
| [src\Clean.Messaging.Sagas\ISagaHandler.cs](../../src/Clean.Messaging.Sagas/ISagaHandler.cs) | SAGA-006, SAGA-007 |
| [src\Clean.Messaging.Sagas\ISagaManager.cs](../../src/Clean.Messaging.Sagas/ISagaManager.cs) | SAGA-016, SAGA-017 |
| [src\Clean.Messaging.Sagas\Persistence\ISagaStore.cs](../../src/Clean.Messaging.Sagas/Persistence/ISagaStore.cs) | SAGA-005, SAGA-016, SAGA-017 |
| [src\Clean.Messaging.Sagas\Persistence\SagaCleanupWorker.cs](../../src/Clean.Messaging.Sagas/Persistence/SagaCleanupWorker.cs) | SAGA-017 |
| [src\Clean.Messaging.Sagas\Persistence\SagaEntry.cs](../../src/Clean.Messaging.Sagas/Persistence/SagaEntry.cs) | SAGA-005, SAGA-006, SAGA-012, SAGA-017 |
| [src\Clean.Messaging.Sagas\Persistence\SagaFailure.cs](../../src/Clean.Messaging.Sagas/Persistence/SagaFailure.cs) | SAGA-014, SAGA-015 |
| [src\Clean.Messaging.Sagas\Properties\AssemblyInfo.cs](../../src/Clean.Messaging.Sagas/Properties/AssemblyInfo.cs) | SAGA-003 |
| [src\Clean.Messaging.Sagas\Runtime\SagaAttempt.cs](../../src/Clean.Messaging.Sagas/Runtime/SagaAttempt.cs) | SAGA-006, SAGA-007, SAGA-012 |
| [src\Clean.Messaging.Sagas\Runtime\SagaCommit.cs](../../src/Clean.Messaging.Sagas/Runtime/SagaCommit.cs) | SAGA-006, SAGA-012 |
| [src\Clean.Messaging.Sagas\Runtime\SagaConsumer.cs](../../src/Clean.Messaging.Sagas/Runtime/SagaConsumer.cs) | SAGA-002, SAGA-005, SAGA-011 |
| [src\Clean.Messaging.Sagas\Runtime\SagaManager.cs](../../src/Clean.Messaging.Sagas/Runtime/SagaManager.cs) | SAGA-016 |
| [src\Clean.Messaging.Sagas\Runtime\SagaProcessor.cs](../../src/Clean.Messaging.Sagas/Runtime/SagaProcessor.cs) | SAGA-004, SAGA-005, SAGA-008, SAGA-010 |
| [src\Clean.Messaging.Sagas\Runtime\SagaRuntime.cs](../../src/Clean.Messaging.Sagas/Runtime/SagaRuntime.cs) | SAGA-005, SAGA-006, SAGA-009, SAGA-011, SAGA-012 |
| [src\Clean.Messaging.Sagas\Runtime\SagaStateSerializer.cs](../../src/Clean.Messaging.Sagas/Runtime/SagaStateSerializer.cs) | SAGA-007, SAGA-015 |
| [src\Clean.Messaging.Sagas\Runtime\SagaTrigger.cs](../../src/Clean.Messaging.Sagas/Runtime/SagaTrigger.cs) | SAGA-004, SAGA-009, SAGA-010 |
| [src\Clean.Messaging.Sagas\Saga.cs](../../src/Clean.Messaging.Sagas/Saga.cs) | SAGA-016 |
| [src\Clean.Messaging.Sagas\SagaBuilder.cs](../../src/Clean.Messaging.Sagas/SagaBuilder.cs) | SAGA-002 |
| [src\Clean.Messaging.Sagas\SagaId.cs](../../src/Clean.Messaging.Sagas/SagaId.cs) | SAGA-004 |
| [src\Clean.Messaging.Sagas\SagaKey.cs](../../src/Clean.Messaging.Sagas/SagaKey.cs) | SAGA-004, SAGA-015 |
| [src\Clean.Messaging.Sagas\SagaResult.cs](../../src/Clean.Messaging.Sagas/SagaResult.cs) | SAGA-006, SAGA-009, SAGA-014, SAGA-015 |
| [src\Clean.Messaging.Sagas.EntityFrameworkCore\Clean.Messaging.Sagas.EntityFrameworkCore.csproj](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/Clean.Messaging.Sagas.EntityFrameworkCore.csproj) | SAGA-003 |
| [src\Clean.Messaging.Sagas.EntityFrameworkCore\DependencyInjection.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/DependencyInjection.cs) | SAGA-002, SAGA-003, SAGA-011 |
| [src\Clean.Messaging.Sagas.EntityFrameworkCore\ISagaConflictResolver.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/ISagaConflictResolver.cs) | SAGA-013, SAGA-014 |
| [src\Clean.Messaging.Sagas.EntityFrameworkCore\SagaConfiguration.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConfiguration.cs) | SAGA-003, SAGA-004, SAGA-012, SAGA-014, SAGA-017 |
| [src\Clean.Messaging.Sagas.EntityFrameworkCore\SagaConflictResolver.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConflictResolver.cs) | SAGA-013, SAGA-014, SAGA-015 |
| [src\Clean.Messaging.Sagas.EntityFrameworkCore\SagaRaceException.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaRaceException.cs) | SAGA-013 |
| [src\Clean.Messaging.Sagas.EntityFrameworkCore\SagaSaveInterceptor.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaSaveInterceptor.cs) | SAGA-011, SAGA-013, SAGA-014 |
| [src\Clean.Messaging.Sagas.EntityFrameworkCore\SagaStore.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaStore.cs) | SAGA-005, SAGA-015, SAGA-016, SAGA-017 |
| [src\Clean.Messaging.Sagas.EntityFrameworkCore\SagaValidation.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaValidation.cs) | SAGA-003 |

### End-to-end ownership trace

Ordinary consumer start/continuation context or schedule timer instance → retained definition/key/start identity → state deserialization/handler/result → expected version/state/outcome → append-ordered outgoing/timer/cancel intentions → terminal remaining-timer cancellation → inbox/local-outbox/schedule owner save/commit or rollback → later effects. Save failure resolves qualified start collisions; management and state purge retain terminal identities. See SAGA-001 through SAGA-017.

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
| [GAP-WF-001](GAPS.md#gap-wf-001) | Must default target and default correlation key be rejected at the writer/runtime boundary? | SAGA-004, SAGA-015 |
| [GAP-WF-006](GAPS.md#gap-wf-006) | Which caller savepoint and uncertain-commit patterns are supported across composed schedules/sagas/outbox? | SAGA-011 |
| [GAP-WF-007](GAPS.md#gap-wf-007) | Must type/key equality be ordinal across supported providers, or are provider collations part of the contract? | SAGA-004, SAGA-015 |
| [GAP-WF-008](GAPS.md#gap-wf-008) | Should Fail validate like SagaFailure, and what should null/blank/overlong values do? | SAGA-006, SAGA-014, SAGA-015 |
| [GAP-WF-009](GAPS.md#gap-wf-009) | May an unrelated DbUpdateException translate when one added start and a matching winner exist? | SAGA-013, SAGA-015 |
| [GAP-WF-010](GAPS.md#gap-wf-010) | Should caller cancellation during conflict lookup propagate, or preserve original DbUpdateException? | SAGA-013, SAGA-014 |
| [GAP-WF-011](GAPS.md#gap-wf-011) | Is indefinite type/key/start tombstone retention and no restart/delete API approved lifecycle intent? | SAGA-004, SAGA-017 |
| [GAP-WF-012](GAPS.md#gap-wf-012) | What happens contractually when two start messages for the same key run in one scope before SaveChanges? | SAGA-005, SAGA-015 |
| [GAP-WF-013](GAPS.md#gap-wf-013) | What explicit failures apply to null nested scheduling options or null saga serializer options, and extreme otherwise-positive values? | SAGA-003 |
| [GAP-WF-015](GAPS.md#gap-wf-015) | Which retained state/message schema changes are supported and who migrates incompatible work? | SAGA-007, SAGA-015 |

## Verification matrix

Every criterion appears once. Levels describe future verification intent: unit for pure contract/description outcomes; persistence for database durability; concurrency for controlled overlapping ownership; transport for external protocol boundaries; workflow for composed host/processing lifecycle. Workflow-level Quartz criteria are not claims of broker transport validation. All results remain **Not tested**. Conditional rows are falsifiable only after the stated policy decision.

| Acceptance criterion | Level | Result | Falsifiable observable outcome |
| --- | --- | --- | --- |
| SAGA-001-AC-01 | workflow | Not tested | the logical effect/timer identity remains stable; a new delivery identity does not redefine it as a new logical operation. |
| SAGA-001-AC-02 | persistence | Not tested | participating state and durable effects commit together, not a state-only/outgoing-only successful transition. |
| SAGA-001-AC-03 | workflow | Not tested | external application effects are not claimed to have rolled back or happened exactly once; their policy remains the consumer responsibility. |
| SAGA-001-AC-04 | workflow | Not tested | the candidate consistent-with-standards outcome is no normal business SagaResult.Fail and no committed participating effects; actual owner cancellation must be verified separately from the conflict-lookup exception window. |
| SAGA-002-AC-01 | unit | Not tested | invalid configuration rejects; nonblank ASCII type length 200 is accepted, 201 rejects, and duplicate identities/types do not silently override. |
| SAGA-002-AC-02 | unit | Not tested | duplicate mapping rejects; another distinct message type can be registered. |
| SAGA-002-AC-03 | unit | Not tested | with create it can start; without create it can only continue; required delegates reject null and consumer IDs obey global nonblank ASCII <=200 uniqueness. |
| SAGA-002-AC-04 | workflow | Not tested | the mapping is available to that saga timer but supplies no ordinary durable consumer by itself. |
| SAGA-002-AC-05 | unit | Not tested | the missing contract fails configuration validation rather than being silently exempt because no ordinary consumer exists. |
| SAGA-002-AC-06 | workflow | Not tested | the configured handler service is used in the owner scope; registering a saga does not make handler/context shared state thread-safe. |
| SAGA-003-AC-01 | workflow | Not tested | required dependencies prevent valid durable startup; registration is not automatic outbox installation. |
| SAGA-003-AC-02 | persistence | Not tested | saga and scheduling mappings/interception participate together; same installed hook is not duplicated; missing model/interceptors/nonrelational provider fail validation. |
| SAGA-003-AC-03 | unit | Not tested | JSON web defaults, retention 30 days, cleanup interval 1 hour and batch 500 apply unless explicitly configured; Section alone does not bind values. |
| SAGA-003-AC-04 | unit | Not tested | zero/negative values reject and positive values pass; no saga-specific retry/lease/concurrency options are inferred. |
| SAGA-004-AC-01 | unit | Not tested | 500 characters are accepted unchanged including Unicode; blank/null and 501 reject; no trim/case-normalisation is implied. |
| SAGA-004-AC-02 | unit | Not tested | Value is preserved and text uses GUID D format; New supplies a nonempty GUID, not a correlation key. |
| SAGA-004-AC-03 | persistence | Not tested | the definition-specific key controls instance lookup; metadata correlation does not merge or split instances by itself. |
| SAGA-004-AC-04 | unit | Not tested | handling rejects the absent durable trigger identity instead of inventing a start identity. |
| SAGA-004-AC-05 | persistence | Not tested | the retained unique identity prevents a second committed row, even if payload state was purged. |
| SAGA-005-AC-01 | persistence | Not tested | a new SagaId, the incoming MessageId as start identity, initial timestamps and handler-produced state/outcome are retained. |
| SAGA-005-AC-02 | persistence | Not tested | no new saga/state transition/outgoing effects are committed, including when terminal state was purged. |
| SAGA-005-AC-03 | persistence | Not tested | saga.start.conflict is permanent and exposes type/key; the existing workflow is not overwritten. |
| SAGA-005-AC-04 | persistence | Not tested | saga.start.closed is permanent; completion/failure is not reopened. |
| SAGA-005-AC-05 | persistence | Not tested | saga.not_found is retry-classified with no new saga or effects, allowing a later committed start to make a retry meaningful. |
| SAGA-005-AC-06 | persistence | Not tested | state/outcome/effects reflect that handler result and the persisted version advances. |
| SAGA-005-AC-07 | persistence | Not tested | no handler state transition/effects or reopening is committed. |
| SAGA-005-AC-08 | workflow | Not tested | no per-saga message ledger guarantees suppression; inbox/outbox owner identity and fencing, not the saga key alone, govern duplicate processing. |
| SAGA-006-AC-01 | persistence | Not tested | Continue remains active, Complete sets completion and clears diagnostics, Fail sets failure/code/message; terminal outcome does not wait for external delivery. |
| SAGA-006-AC-02 | unit | Not tested | the original produces only its original intentions; the augmented one adds effects in append order without mutating the original description. |
| SAGA-006-AC-03 | unit | Not tested | both forms capture the same logical ID/payload/due/explicit metadata rather than differing durable obligations. |
| SAGA-006-AC-04 | unit | Not tested | invalid argument is rejected before a valid effect description is appended; past due alone is permitted. |
| SAGA-006-AC-05 | workflow | Not tested | the invalid result/state is rejected and no successful owner transition/effects commit. |
| SAGA-006-AC-06 | persistence | Not tested | no mutated saga state or result effects become durable; ordinary exception and cancellation follow their distinct owner classifications. |
| SAGA-006-AC-07 | unit | Not tested | constructor alone does not reject empty SagaCancel, but supported Result.Cancel rejects it; no public append/apply port makes arbitrary SagaEffect subclass execution a supported plugin contract. |
| SAGA-007-AC-01 | persistence | Not tested | the persisted JSON can reconstruct that configured state and its committed mutations; outgoing serializer options are not implicitly the state options. |
| SAGA-007-AC-02 | persistence | Not tested | saga.serialization.failed is permanent, with no successful owner state/effects transition. |
| SAGA-007-AC-03 | workflow | Not tested | the corresponding definition/state/message error fails rather than automatically migrating, upcasting or inventing a replacement state. |
| SAGA-007-AC-04 | persistence | Not tested | the terminal no-op path does not reopen or require reconstructing purged state. |
| SAGA-008-AC-01 | persistence | Not tested | serialized state, advanced version and UpdatedAt describe the same committed transition; an effect failure still requires owner rollback. |
| SAGA-008-AC-02 | concurrency | Not tested | state/outcome/effects from the stale candidate are rejected rather than silently replacing the newer candidate; database concurrency remains a separate commit guard. |
| SAGA-008-AC-03 | unit | Not tested | the terminal instance cannot be reopened by that candidate; terminal replay routing remains a no-op before normal state application. |
| SAGA-008-AC-04 | unit | Not tested | the result description alone has not changed persisted state/outcome/version or staged an external publication. |
| SAGA-009-AC-01 | persistence | Not tested | outgoing EventId is X, generated MessageId is delivery identity, and C/D win over trigger metadata. |
| SAGA-009-AC-02 | persistence | Not tested | correlation inherits C and causation inherits X, not D; when values are null the outgoing context fallback still belongs to outbox. |
| SAGA-009-AC-03 | persistence | Not tested | schedule ID is X, UTC due is equivalent instant, target is saga, group identifies the SagaId, correlation follows trigger and causation is trigger MessageId. |
| SAGA-009-AC-04 | persistence | Not tested | only matching saga group/target active timers cancel; cancelling the current triggering timer is a no-op, and unrelated timers remain. |
| SAGA-009-AC-05 | persistence | Not tested | sends remain staged, newly scheduled and other nonterminal timers in that saga group are cancelled after appended effects, excluding the current triggering timer. |
| SAGA-009-AC-06 | persistence | Not tested | there is no timer upsert; identity conflict can fail save and participating state/effects require owner rollback. |
| SAGA-009-AC-07 | persistence | Not tested | schedule-then-cancel leaves it cancelled; cancel-then-schedule can leave the newly staged timer active, subject to identity uniqueness. |
| SAGA-010-AC-01 | workflow | Not tested | saga.serialization.failed faults timer delivery without a saga state transition. |
| SAGA-010-AC-02 | workflow | Not tested | saga.not_found is transient; the timer may retry under scheduling policy, without constructing a saga from timer payload. |
| SAGA-010-AC-03 | workflow | Not tested | that instance receives the mapped message without correlation/create routing; timer identity is the triggering delivery identity for resulting effects. |
| SAGA-010-AC-04 | workflow | Not tested | the terminal instance is ignored before active payload/state handling and the timer can complete without new workflow effects. |
| SAGA-010-AC-05 | workflow | Not tested | definition/handler/type mismatch faults as saga.serialization.failed or shared serializer contract error; it is not successful timer consumption. |
| SAGA-010-AC-06 | workflow | Not tested | timer is failed but saga remains active; no automatic SagaResult.Fail, compensation or timeout escalation is inferred. |
| SAGA-010-AC-07 | persistence | Not tested | state/effects and current timer consumption commit together on success, or remain uncommitted on rollback. |
| SAGA-011-AC-01 | persistence | Not tested | separate committed readers do not see a completed workflow transition; result construction/handling is not transaction commit. |
| SAGA-011-AC-02 | persistence | Not tested | participating business/saga/effects and owned entry completion are durable together; another context/network effect has no such guarantee. |
| SAGA-011-AC-03 | persistence | Not tested | none of the attempt local state or effects is durably committed merely because an earlier effect was staged. |
| SAGA-011-AC-04 | workflow | Not tested | handler invocation can repeat; only committed owner fencing/state/effects determine successful local completion. |
| SAGA-011-AC-05 | persistence | Not tested | the saga integration rejects unsupported ambient ownership; unrelated no-saga saves are not rejected by this hook. |
| SAGA-011-AC-06 | persistence | Not tested | outbox integration rejects that unsupported operation; EF internal savepoints during saves are distinct and permitted. |
| SAGA-012-AC-01 | persistence | Not tested | version becomes V+1 with UpdatedAt and result state/outcome; newly started version zero is advanced by its first applied handler transition. |
| SAGA-012-AC-02 | concurrency | Not tested | one conflicting commit is rejected by optimistic concurrency instead of overwriting the winning state/effects; handlers may both have run. |
| SAGA-012-AC-03 | concurrency | Not tested | it cannot commit a reopened active workflow over the winning terminal transition; conflict/owner policy determines recovery. |
| SAGA-012-AC-04 | workflow | Not tested | no saga-only immediate reload/reinvoke is assumed; a later eligible attempt observes then-current durable state. |
| SAGA-012-AC-05 | concurrency | Not tested | participating saga/effect changes are not committed, although a nontransactional handler effect may already exist. |
| SAGA-013-AC-01 | concurrency | Not tested | failure is translated to retry-classified saga.start.race; a later replay sees the retained start and is a no-op. |
| SAGA-013-AC-02 | concurrency | Not tested | saga.start.conflict is permanent and exposes the winning type/key boundary without replacing its state. |
| SAGA-013-AC-03 | concurrency | Not tested | saga.start.closed is permanent, not reopening or an idempotent acknowledgement of the different start. |
| SAGA-013-AC-04 | persistence | Not tested | the original failure is preserved rather than fabricating conflict/closed/race without qualifying evidence. |
| SAGA-013-AC-05 | concurrency | Not tested | both expose the same race/conflict/closed classification for the same retained start relationship. |
| SAGA-013-AC-06 | unit | Not tested | as a candidate compatibility baseline, missing is saga.not_found/Retry, conflict saga.start.conflict/Fault, closed saga.start.closed/Fault and serialization saga.serialization.failed/Fault; this fact does not decide malformed-result policy. |
| SAGA-016-AC-01 | persistence | Not tested | Get returns the matching snapshot or null; absent saga yields no timers; reads do not reopen/create/purge work. |
| SAGA-016-AC-02 | persistence | Not tested | active list excludes all terminal rows and orders CreatedAt ascending; failed list excludes active/complete and orders FailedAt descending; no more than limit are returned, ties unspecified. |
| SAGA-016-AC-03 | unit | Not tested | nonpositive limit rejects; omitted limit is 100; positive limit bounds persisted results. |
| SAGA-016-AC-04 | persistence | Not tested | only saga group/target timers appear ordered due then ID, ConsumedAt reflects schedule dispatch, and IsTerminal is true for consumed/cancelled/failed timestamps. |
| SAGA-016-AC-05 | persistence | Not tested | IsActive iff both saga terminal timestamps are absent, IsTerminal is inverse; persisted values are returned without unsaved-state merge or mutable tracked state exposure. |
| SAGA-016-AC-06 | persistence | Not tested | failure or cancellation propagates; only genuinely absent persisted data becomes null/empty. |
| SAGA-017-AC-01 | persistence | Not tested | only previously unpurged terminal state strictly before cutoff is replaced with JSON null; equality/after/active rows retain their state. |
| SAGA-017-AC-02 | persistence | Not tested | StatePurgedAt records purge time; type/key/start identity/outcome/diagnostics remain, and Version and UpdatedAt are not incremented by purge. |
| SAGA-017-AC-03 | workflow | Not tested | no immediate startup purge is assumed; one configured batch ordered by UpdatedAt is attempted per tick; failure retries on later tick. |
| SAGA-017-AC-04 | persistence | Not tested | same start remains no-op, different start remains closed and continuation remains ignored; retention does not free that type/key. |
| SAGA-017-AC-05 | persistence | Not tested | saga identity/outcome can remain while timer history is absent; this is not a complete indefinite workflow audit trail. |
| SAGA-017-AC-06 | workflow | Not tested | active saga is not automatically purged, failed, compensated or repaired; manager APIs provide observation, not a business policy engine. |

## Coverage and audit limitations

17 numbered specifications; 85 acceptance criteria, each mapped above. All 45 saga authored owned files (source and project; no owned Markdown files existed) were reviewed, excluding generated bin/obj. The audit manifest has 89 files in all six projects. No code, tests, builds, providers, broker or Quartz instance were executed. Static review cannot settle maintainer intent, prove SQL isolation/clock behaviour, or approve operational limits. Gap links target the shared GAPS page assembled separately; these files do not modify that page.
