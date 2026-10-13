# Outbox behavioural specifications

**Status:** Draft. **Source reviewed:** 2026-10-13. **Verification:** Source-reviewed; not tested.

## Interpretation and evidence rules

Required behaviour is grounded only in normative repository instructions or explicit API XML requirements. Observed behaviour is derived from inspected implementation. Source-derived Reference and Explanation pages are descriptive corroboration, not independent normative authority. Proposed behaviour is explicitly **unapproved**. Unknown behaviour uses **numbered undecided questions**, without acceptance-criterion IDs or verification rows. Every acceptance criterion inherits its specification classification unless explicitly marked otherwise; none below overrides it. Required criteria can identify suspected source mismatches and are not claims of conformance.

## Capability purpose, contracts and common boundaries

Target selection -> scoped staging -> intercepted save/capture tracking -> observed/verified commit -> durable claim -> local transaction or external publication then fenced settlement -> retries/management -> terminal cleanup.

- **Preconditions:** consumer host owns registration, valid durable contracts and identities, configured relational schema/transactions where applicable, stable payload/source state and suitable cancellation/lifetime composition. Each focused specification narrows these preconditions.
- **Postconditions:** only observable admitted, staged, committed, acknowledged or per-target lifecycle outcomes explicitly stated below; success at one boundary does not imply success at another.
- **Invariants:** preserve logical event identity versus delivery identity, concrete durable contract names, original envelope/context metadata, target-specific completion, ownership fences, retry classifications and owning-context transaction boundaries. Repeated external effects remain possible.
- **Failures:** validation/serialization/resolution/database/transport failures and cancellation remain distinct; ordinary logging is not durable success. Source exceptions/cancellation racing commit require evidence, not an assumption of rollback.
- **Concurrency:** contexts and source collections are not automatically thread-safe. Lease ownership is fenced at settlement, not permission to suppress repeated external calls. Recovery requires a running healthy host/store; no strict latency, fairness or global completion order is specified.
- **Limitations:** Enqueue is staging, not save/commit/publication. Captured callback completion is not durable delivery. There is no atomic external publication+database settlement, unconditional persisted duplicate suppression, public outgoing replay history, universal user-savepoint support or certain outcome from unverifiable commits.

### Common public model and lifecycle contracts

Record positional fields, exposed init/set properties, constants, generic constraints, explicit interface implementations and protected extension operations are included in the source/contract table. Public primary constructors and inherited hosted StartAsync/StopAsync/Dispose behaviour are composition surfaces, not additional delivery guarantees. For entries, all identity/payload/context fields, Attempts, ClaimId, ClaimedUntil, NextAttemptAt, ProcessedAt, DeadLetteredAt, DiscardedAt, FailureCategory, FailureCode, ExceptionType, FailureMessage, LastFailedAt, LastFailedAttempt and derived Key participate in the lifecycle specifications. Port implementations (including internal EF adapters) are separately traced by responsibility.

### Supporting evidence reviewed

[Repository instructions](../../.github/copilot-instructions.md), [coding standards](../CODING_STANDARDS.md), [documentation index](../README.md); references: [architecture](../reference/architecture.md), [configuration](../reference/configuration.md), [contracts](../reference/contracts.md), [inbox](../reference/inbox.md), [outbox](../reference/outbox.md), [domain events](../reference/domain-events.md), [persistence](../reference/persistence.md), [processing](../reference/processing.md), [transport](../reference/transport.md). Explanations: [capability boundaries](../explanation/capability-boundaries.md), [message identity](../explanation/message-identity.md), [transaction boundaries](../explanation/transaction-boundaries.md), [delivery semantics](../explanation/delivery-semantics.md), [durable workflows](../explanation/durable-workflows.md). Adjacent ingress/broker/mapper APIs are outside these owned files; their source-derived descriptions are not independently tested guarantees.

### OUTBOX-001 — Enqueue stages a concrete envelope and target snapshot

**Classification:** Observed behaviour

**Evidence:** Source: IOutbox/Outbox.Enqueue, OutboxMessage<T>, OutboxEntryFactory.Create, OutboxBatch and OutboxEntry. [Outbox](../reference/outbox.md), [contracts](../reference/contracts.md) and [transport](../reference/transport.md) are descriptive, not normative.

Purpose: stage deferred local/external work in the owning persistence context. Contracts: synchronous Enqueue<TMessage> has no cancellation/save/commit promise; factory Create<T> returns batch; OutboxMessage Id/Message/CorrelationId/CausationId. Preconditions: nonnull envelope/payload, nonempty Id, registered concrete contract and usable providers. Postcondition: targets have a fresh delivery MessageId shared across the batch; caller Id becomes logical EventId. Factory serializes concrete runtime type and evaluates providers against the original object before staging. Destination/transport/target snapshots and UTC CreatedAt are persisted per target.

#### OUTBOX-001-AC-01 — Do not confuse staging with persistence or delivery

- **Given** a valid event and an owning unsaved context.
- **When** Enqueue returns and the context is later abandoned.
- **Then** no successful save, durable rows, consumer completion or external publication is implied.

#### OUTBOX-001-AC-02 — Preserve logical identity with new delivery identity

- **Given** a valid caller event Id and several targets.
- **When** Create/Enqueue creates a batch and the owning save commits.
- **Then** all targets share fresh MessageId, caller Id as EventId, serialized concrete contract/payload and one creation time; another factory invocation can generate a different MessageId.

#### OUTBOX-001-AC-03 — Reject invalid envelope and serialization

- **Given** null envelope/payload, empty Id, missing concrete contract or serializer failure.
- **When** Create/Enqueue runs.
- **Then** it fails without a successfully staged batch; record construction alone supplies no runtime validation.

#### OUTBOX-001-AC-04 — Snapshot targets before storage

- **Given** target providers calculate transport/destination from the original concrete payload.
- **When** factory creation succeeds, routing logic later changes and the batch is saved.
- **Then** stored target/transport/destination remain the staged values; providers see the concrete runtime type and original payload.

### OUTBOX-002 — Ambient outgoing metadata and representation members

**Classification:** Observed behaviour

**Evidence:** Source: OutboxEntryFactory.Create, OutboxEnvelope, MessageContext. [Message identity](../explanation/message-identity.md) explains, not mandates, context inheritance.

Contracts: envelope MessageId/EventId/TargetId/Destination/Message/CorrelationId/CausationId and entry lifecycle members; envelope has no Transport member (delivery selects it before dispatch). Explicit correlation/causation wins; absent correlation inherits context.CorrelationId, absent causation inherits context.MessageId, not context.CausationId. Null cannot explicitly clear an inherited value. These records do not validate their construction independently.

#### OUTBOX-002-AC-01 — Inherit or override outgoing context

- **Given** an active delivered MessageId/correlation/causation and an outgoing envelope with supplied or null optional IDs.
- **When** factory creates targets.
- **Then** supplied values win; null correlation inherits current correlation and null causation becomes current delivery MessageId, not its parent causation.

#### OUTBOX-002-AC-02 — Carry persisted metadata to external port

- **Given** a stored external target with payload, destination and nullable context IDs.
- **When** the transport wrapper dispatches.
- **Then** IOutboxTransport receives an envelope preserving delivery/event/target/destination/payload/context IDs; arbitrary headers or content type are not added by this core envelope.

### OUTBOX-003 — Target validation, local selection and empty batches

**Classification:** Observed behaviour

**Evidence:** Source: IOutboxTargetProvider.GetTargets, OutboxTarget, OutboxTargetProvider, LocalTransport.Name, factory validation and OutboxWriter.Stage. [GAP-DUR-006](GAPS.md#gap-dur-006) covers external-only default selection.

Contracts: GetTargets(Type messageType, object message); target ID/transport/destination; local transport name "local". Target IDs: nonblank <=200 ASCII, unique ordinally across the whole batch even across transports; transport nonblank. Destination and transport lengths/grammar are not comprehensively checked in the factory. Default local provider uses exact-type consumer Get and throws if none even when external providers could supply work. A custom composition may return no targets, producing IsEmpty true and no-op Stage.

#### OUTBOX-003-AC-01 — Validate target boundary and duplicates

- **Given** targets contain blank, 201-character or non-ASCII IDs, blank transport, or duplicate ordinal TargetIds across routes.
- **When** Create evaluates the complete target set.
- **Then** invalid IDs/transports fail; duplicate target identity fails independent of destination or transport; distinct case-sensitive IDs remain distinct.

#### OUTBOX-003-AC-02 — Do not infer transport or destination schema validation

- **Given** a nonblank unusually long transport or null/blank/custom destination and otherwise valid target.
- **When** factory creates a batch.
- **Then** factory does not itself reject by destination semantics or transport maximum length; provider mapping/persistence/delivery may reject later.

#### OUTBOX-003-AC-03 — Observe external-only default local failure

- **Given** a payload has no exact-type local consumers but an external provider could supply targets and the default local provider remains installed.
- **When** Create/Enqueue enumerates providers.
- **Then** consumer.message_unhandled fails staging; external provider availability alone does not make default composition external-only.

#### OUTBOX-003-AC-04 — Stage an empty custom batch without work

- **Given** a custom provider composition returns no targets and serialization succeeds.
- **When** factory and writer Stage run.
- **Then** IsEmpty is true and no entries are added; empty batch staging is not external delivery.

### OUTBOX-004 — Tracked duplicate suppression versus persisted uniqueness

**Classification:** Observed behaviour

**Evidence:** Source: IOutboxStore.Stage, IOutboxWriter.Stage, OutboxWriter, OutboxStore and mappings. [GAP-DUR-007](GAPS.md#gap-dur-007) covers the scope of idempotency.

Precondition: valid batch and owning context. Postcondition: missing tracked logical event/target pairs become Added. Invariant: same EventId/TargetId tracked nondeleted rows suppress new rows without comparing payload or envelope. No database lookup occurs; fresh-context persisted duplicates can fail the owning save. Entry primary key (MessageId,TargetId), unique key (EventId,TargetId); retention deletes that evidence. Deleted tracked rows do not suppress replacement.

#### OUTBOX-004-AC-01 — Suppress only nondeleted tracked target pairs

- **Given** same logical event/target is already tracked nondeleted with possibly different payload; another target is absent.
- **When** Stage runs in that context.
- **Then** existing tracked pair is left unchanged and only the missing target is staged; no payload-equivalence guarantee is given.

#### OUTBOX-004-AC-02 — Expose duplicate persisted save failure

- **Given** the pair is stored but not tracked in a new context.
- **When** Enqueue/Stage adds the duplicate and owning SaveChanges runs.
- **Then** staging can return normally but unique event/target persistence can fail; successful enqueue is not unconditional durable idempotency.

#### OUTBOX-004-AC-03 — Allow deleted tracked replacements and cleaned identities

- **Given** an existing pair is tracked Deleted or its old terminal row was physically cleaned.
- **When** a replacement is staged and saved with valid transaction semantics.
- **Then** deleted tracked state does not suppress staging; absent retained logical evidence no longer prevents a new delivery.

### OUTBOX-005 — Atomic owning save and commit boundary

**Classification:** Required behaviour

**Evidence:** Normative: coding standards and repository instructions require durable consistency and scoped transaction preservation. Source evidence: save/transaction interceptors, tracker, configured-context staging. Descriptive persistence/transaction-boundary docs do not make every interceptor detail normative.

Purpose: commit application changes and outgoing entries together. Preconditions: configured relational owning context with model and save/transaction interception; correctly staged rows. Postconditions: rows do not authorize delivery before owning commit, and failed transaction does not commit intended application/outbox state. Other contexts and external publication are outside this boundary. Caller must not interpret staging or an uncommitted save as completed delivery.

#### OUTBOX-005-AC-01 — Commit application state with outgoing work

- **Given** application changes and staged targets share the configured scoped context.
- **When** the owning transaction commits successfully.
- **Then** both application state and outgoing rows are durable together and available for postcommit processing.

#### OUTBOX-005-AC-02 — Do not publish uncommitted work

- **Given** an explicit transaction has saved application/outbox rows but has not committed.
- **When** a worker using an independent store tries to discover work.
- **Then** uncommitted rows do not constitute committed outgoing work; acknowledgement/wake based on commit is deferred.

#### OUTBOX-005-AC-03 — Roll back the owning transaction

- **Given** saved outgoing rows and application changes are in an explicit transaction.
- **When** the transaction is rolled back.
- **Then** those database changes are not durable outgoing work; in-memory rollback does not reset arbitrary application state.

### OUTBOX-006 — Public save capture protocol and per-context deduplication

**Classification:** Observed behaviour

**Evidence:** Source: ISaveCapture.Capture, OutboxInterceptor save success/failure/cancellation callbacks and public OutboxTracker operations. [Persistence](../reference/persistence.md) is descriptive.

Contracts: OutboxTracker IsSaving/IsCommitting, BeginSave, Capture<TCapture>, AcknowledgeAfterCommit, TrackEntries, SaveSucceeded/SaveFailed, TransactionCommitting/Committed/CommitUnknown/RolledBack, GetPendingKeys. State belongs to each DbContext; capture uniqueness is (capture type,eventId), acknowledgement uniqueness is eventId only. BeginSave validates ambient/nesting/pending transaction identity. Capture/ack/track require active save; acknowledgement callback nonnull. Successful save merges capture/key/ack state; explicit transaction defers callbacks. Newly captured current-save rows are distinct from pre-staged and previous pending-save rows.

#### OUTBOX-006-AC-01 — Deduplicate captures across saves in one transaction

- **Given** the same capture type/event ID is encountered repeatedly in one context/explicit transaction.
- **When** successful saves repeat before commit.
- **Then** Capture authorizes the first only; pending acknowledgements stay deferred until observed commit, with no duplicate capture rows.

#### OUTBOX-006-AC-02 — Distinguish contexts and capture types

- **Given** two contexts or two different capture types encounter the same event ID.
- **When** Capture operates inside each active save.
- **Then** capture state is independent per context/type; acknowledgement callbacks keyed only by eventId do not imply one callback per capture type.

#### OUTBOX-006-AC-03 — Reject invalid capture protocol

- **Given** Capture/ack/TrackEntries occurs without BeginSave, acknowledgement is null, save is nested, or ambient transaction is present.
- **When** the public tracker operation is requested.
- **Then** invalid lifecycle/input is rejected rather than silently treated as a successful capture.

#### OUTBOX-006-AC-04 — Expose saving/committing and pending-key boundaries

- **Given** active saving, ordinary idle state and a marked commit phase.
- **When** IsSaving, IsCommitting and GetPendingKeys are inspected.
- **Then** saving/committing state reflects the phase; pending keys are exposed during commit rather than proving transaction success.

### OUTBOX-007 — Save failure, cancellation, rollback and retry tracking

**Classification:** Observed behaviour

**Evidence:** Source: OutboxTracker.BeginSave/TrackEntries/SaveFailed/TransactionRolledBack and interceptor failure/cancellation hooks. No claim that all business tracking is restored.

Preconditions: tracked active save or observed explicit transaction. Postconditions: a failed/cancelled save removes newly captured rows for that save, not pre-staged rows or all prior successful pending captures; no acknowledgement occurs. Observed rollback removes recorded transaction work and tracker state; subsequent retry needs application state/captures reconciled by caller. TransactionRolledBack without recorded transaction ID returns without inventing rollback evidence.

#### OUTBOX-007-AC-01 — Detach current captured rows on failed save

- **Given** pre-staged rows, earlier successful transaction captures and newly captured rows coexist.
- **When** the current save fails or is cancelled.
- **Then** newly captured current-save entries detach; pre-staged/earlier pending entries are not all discarded and acknowledgement does not run.

#### OUTBOX-007-AC-02 — Retain events for retry and clear observed rollback keys

- **Given** captured events remain in memory while a save fails, or an explicit transaction rollback is observed.
- **When** a later valid save/capture is attempted with reconciled application state.
- **Then** failed events can be captured again; observed rollback removes recorded outgoing tracked keys without pretending to revert arbitrary domain state.

#### OUTBOX-007-AC-03 — Reject changed transaction identity with pending work

- **Given** pending captured/staged keys belong to an earlier transaction whose end was not observed.
- **When** BeginSave occurs after that transaction identity changes.
- **Then** it fails rather than silently acknowledging or transferring pending work into another transaction.

### OUTBOX-008 — Commit verification and uncertain outcome

**Classification:** Observed behaviour

**Evidence:** Source: OutboxTransactionInterceptor.TransactionFailed/Async, OutboxCommitVerifier, OutboxTracker.GetPendingKeys/TransactionCommitUnknown. [GAP-DUR-008](GAPS.md#gap-dur-008) covers recovery/verification horizon.

Purpose: reconcile failed commit notification using retained outgoing entry keys in a fresh context. Preconditions: marked commit with pending keys. All keys present implies observed committed handling; not all present selects rollback handling; query error or zero keys leaves unknown. Verification is source-derived inference, not certification of other writes or external effects; async verification ignores the caller cancellation token (None). Unknown state does not acknowledge or claim successful rollback. Cleanup/removal and callback-only captures complicate inference.

#### OUTBOX-008-AC-01 — Verify a lost successful commit response

- **Given** transaction commit notification fails after database commit and every pending outgoing key remains queryable.
- **When** fresh-context verification succeeds.
- **Then** tracker acknowledges commit and can wake outgoing processing while original commit-failure handling remains subject to EF behaviour.

#### OUTBOX-008-AC-02 — Handle missing keys versus unavailable verification

- **Given** commit fails during marked commit with some expected keys absent, or verifier throws.
- **When** transaction failure interception runs.
- **Then** absent-key evidence selects rollback tracking; verification error selects unknown state and no definite acknowledgement.

#### OUTBOX-008-AC-03 — Keep zero-key outcome unknown

- **Given** a failed commit has no outgoing keys suitable for verification.
- **When** failure interception attempts reconciliation.
- **Then** no committed outcome is inferred from empty keys; acknowledgement and application state certainty are not proved.

#### OUTBOX-008-AC-04 — Do not cancel verification with failed commit token

- **Given** the commit token is cancelled and a marked commit requires async verification.
- **When** the failure hook verifies.
- **Then** verification uses an independent None token; its success/error, not the cancelled commit token alone, controls tracker reconciliation.

### OUTBOX-009 — Savepoints, ambient transactions and observer restrictions

**Classification:** Observed behaviour

**Evidence:** Source: tracker BeginSave and transaction interceptor CreatingSavepoint/RollingBackToSavepoint synchronous/asynchronous paths. [Persistence](../reference/persistence.md) describes restrictions.

Contracts: sync/async interceptors share lifecycle semantics. Ambient transactions are unsupported for capture; explicit transaction end must be observed. User savepoint creation/rollback outside active save throws NotSupportedException; EF save-time savepoints are allowed. Raw/unobserved commit or disposing without interception is not a supported acknowledgement protocol. No snapshot of callback/key state per user savepoint.

#### OUTBOX-009-AC-01 — Reject user savepoints outside active save

- **Given** outbox tracking is attached and no save is active.
- **When** user savepoint creation or rollback-to-savepoint is intercepted.
- **Then** NotSupportedException reports unsupported tracking rather than a silently partial state rewind.

#### OUTBOX-009-AC-02 — Allow EF save-time savepoints

- **Given** an intercepted SaveChanges is active in an explicit relational transaction.
- **When** EF creates or rolls back its save-time savepoint.
- **Then** the active-save path is permitted and save success/failure still controls capture bookkeeping.

#### OUTBOX-009-AC-03 — Expose unobserved transaction end restriction

- **Given** an integration bypasses observed transaction end with pending work.
- **When** it tries a subsequent save on the context.
- **Then** unsupported lifecycle can be rejected; caller must reconcile/discard rather than infer event acknowledgement from raw commit.

### OUTBOX-010 — Postcommit callbacks and wake hints

**Classification:** Observed behaviour

**Evidence:** Source: OutboxTracker.Commit and PendingCommit.IsEmpty. [GAP-DUR-009](GAPS.md#gap-dur-009) records callback failure/zero-key ambiguities.

Postcondition on normal observed commit: synchronous acknowledgements execute before wake, then state removal. Callback exceptions are not isolated and cannot roll back a durable commit. Pending IsEmpty checks keys/captures, not callbacks alone; callback-only custom integration can fail to participate in commit tracking. No durable callback ledger or retry policy exists. Wake is a coalesced hint; persisted rows remain recoverable if it is lost.

#### OUTBOX-010-AC-01 — Acknowledge capture rather than delivery

- **Given** a successful observed commit includes captured events and outgoing keys.
- **When** commit callbacks finish normally.
- **Then** events are acknowledged and outgoing work is signalled without proving any target consumer or external transport has completed.

#### OUTBOX-010-AC-02 — Surface callback failure after durable commit

- **Given** a custom acknowledgement callback throws before later callbacks/wake/state removal.
- **When** commit bookkeeping runs.
- **Then** the database can already be committed while acknowledgement sequence/wake/cleanup is interrupted; recovery can still discover persisted rows.

#### OUTBOX-010-AC-03 — Observe callback-only participation gap

- **Given** a custom integration registers acknowledgement only with no captures or keys.
- **When** save/transaction bookkeeping examines pending state.
- **Then** callbacks alone do not make PendingCommit nonempty; registration is not a proven durable postcommit scheduling facility.

#### OUTBOX-010-AC-04 — Distinguish ordinary callback failure from commit-failure reconciliation

- **Given** a callback throws while verified committed handling runs inside the transaction-failure reconciliation catch.
- **When** the interceptor handles that exception.
- **Then** the reconciliation catch can classify tracker state as unknown without rethrowing that callback exception; durable commit remains possible and later callbacks/wake/cleanup can be incomplete.

### OUTBOX-011 — Transport dispatch and external repeat window

**Classification:** Required behaviour

**Evidence:** Normative: repository instructions/coding standards preserve identity, claims and failure classification and explicitly forbid exactly-once external-effect claims. Source: OutboxDispatcher.Dispatch, IOutboxDelivery, IOutboxTransport, OutboxTransport wrapper.

Purpose: route persisted work while retaining repeat-safe message identity. Contracts: IOutboxDelivery.Transport/Dispatch returns true for completion, false for ownership loss; dispatcher selects stored transport ordinally; IOutboxTransport.Name/Dispatch performs publication, not durable completion. Preconditions: resolvable delivery and valid ownership for settlement. Invariant: a successful transport return precedes separate fenced completion, never an atomic database+broker transaction. Duplicate delivery transport names fail construction; unknown stored name fails dispatch (observed details).

#### OUTBOX-011-AC-01 — Preserve envelope across retries

- **Given** a stored external target is published and completion then fails or the process crashes.
- **When** the unresolved row is recovered and dispatched again.
- **Then** external publication may repeat with the same delivery/event/target/context identity; no exactly-once downstream effects are claimed.

#### OUTBOX-011-AC-02 — Do not complete on transport failure

- **Given** external transport throws or observes cancellation before returning success.
- **When** the wrapper exits to the processor.
- **Then** durable completion is not manufactured; failure/cancellation remains distinguishable for processor policy.

#### OUTBOX-011-AC-03 — Fence settlement after successful publication

- **Given** external publication succeeds but the owner loses its claim before Complete.
- **When** the wrapper attempts settlement.
- **Then** false completion cannot overwrite the new owner; publication already happened and repeat remains possible.

### OUTBOX-012 — Delivery registration and local atomic dispatch

**Classification:** Observed behaviour

**Evidence:** Source: Outbox core AddOutboxTransport<TTransport>, OutboxDispatcher and EF LocalOutboxDelivery; local transaction outcome is constrained by OUTBOX-005 and repository atomicity requirements.

Contracts: AddOutboxTransport<TTransport> registers scoped concrete transport and wrapper delivery; no broker host is automatically installed by that port. Dispatcher transport lookup ordinal; local name "local". Local dispatcher fresh scope per EF strategy attempt, retained completion precheck, configured-context handler, fenced completion, SaveChanges and commit. False completion rolls back. Catch attempts rollback with None and preserves original failure including uncertain commit; handlers/external side effects can repeat during strategy retry.

#### OUTBOX-012-AC-01 — Reject ambiguous or missing transport selection

- **Given** deliveries have duplicate Transport names or a row stores an unregistered/case-different name.
- **When** dispatcher construction or Dispatch runs.
- **Then** duplicate construction or missing exact ordinal lookup throws rather than selecting an arbitrary route.

#### OUTBOX-012-AC-02 — Commit or roll back local target effects

- **Given** a local target changes configured-context state and stages further work.
- **When** dispatch commits, handler fails or fenced completion returns false.
- **Then** success commits state/completion/new outbox together; failure or false rolls back that local transaction.

#### OUTBOX-012-AC-03 — Avoid repeat of retained completed local target

- **Given** local transaction committed but success observation was lost.
- **When** dispatch runs again with retained ProcessedAt.
- **Then** it reports completed without reinvoking local effects; cleaned records or external effects are outside this retained-completion protection.

#### OUTBOX-012-AC-04 — Use registered external wrapper

- **Given** AddOutboxTransport<TTransport> is composed with a custom scoped transport and a stored matching target.
- **When** dispatch resolves the delivery.
- **Then** the registered transport receives its persisted envelope and successful publication is followed by separate completion.

### OUTBOX-013 — Owned durable state, processor policy and cancellation

**Classification:** Observed behaviour

**Evidence:** Source: OutboxEntry, OutboxProcessor.Process, inherited DurableStore, ClaimRenewal, retry/failure factories. [Processing](../reference/processing.md) provides descriptive defaults/metrics.

Contracts: IOutboxStore inherited Claim/Renew/Complete/Retry/DeadLetter/Cleanup; entries carry common identity/payload/context/lifecycle/diagnostics plus Transport/Destination; Key is MessageId/TargetId. Claim positive limit/lease, unresolved due <=now and lease absent/expired <=now; Attempts increments on claim. Settlement requires exact key/token, unresolved state, no NextAttemptAt and ClaimedUntil > now. Processor pushes/restores metadata, renews during processing, links handling to host and claim loss; failed settlement timeout linked to claim loss only. Retry max attempts is failure-policy threshold, not hard maximum crash claims.

#### OUTBOX-013-AC-01 — Claim and fence target state

- **Given** due/future/terminal/actively leased/expired entries and nonpositive claim inputs.
- **When** Claim or stale-owner settlement runs.
- **Then** only eligible bounded work is owned with increased Attempts; invalid limits/lease fail and stale/expired Renew/Complete/Retry/DeadLetter return false.

#### OUTBOX-013-AC-02 — Keep target completion independent

- **Given** several targets share an event and one succeeds while another fails.
- **When** processors settle valid owned rows.
- **Then** successful target alone completes; failed target retries or dead-letters according to transient/permanent/unknown classification with preserved identity/payload/diagnostics.

#### OUTBOX-013-AC-03 — Expose and restore ambient metadata

- **Given** owned stored delivery and prior ambient context.
- **When** Process completes or fails.
- **Then** delivery MessageId/correlation/causation/target are exposed during dispatch and previous context restored afterward.

#### OUTBOX-013-AC-04 — Separate shutdown and ownership cancellation

- **Given** handling is interrupted by host cancellation or claim renewal false/error.
- **When** Process handles the interruption.
- **Then** host cancellation propagates without ordinary failure settlement; claim-loss stops authorizing settlement and leaves unresolved work recoverable.

#### OUTBOX-013-AC-05 — Do not invent completion on settlement infrastructure failure

- **Given** handling failed but retry/dead-letter persistence times out or fails.
- **When** the processor/work loop handles the failure.
- **Then** the error is observable/logged and unresolved work recovers after expiry; separate settlement timeout may run after host stop.

### OUTBOX-014 — Workers, signals, bounded queue and recovery

**Classification:** Observed behaviour

**Evidence:** Source: OutboxSignal.WaitAsync/Wake, OutboxWorker, OutboxRecoveryWorker and shared WorkLoop. No persisted notification, global FIFO or fixed-latency guarantee.

Purpose: execute committed due work eventually under healthy host/store conditions. Preconditions: valid options and resolvable scoped services. Worker startup wakes, periodic recovery wakes, hints coalesce; bounded queued/active work with configured concurrency. Leases start before queue wait; renewal only starts when processing, so stalled queued ownership can expire. Claim errors back off; processing errors log and later work continues.

#### OUTBOX-014-AC-01 — Recover lost signal and crashed queued claims

- **Given** commit hint was lost or host crashed after claiming queued entries.
- **When** restart/recovery runs and stale leases expire.
- **Then** eligible committed work becomes attemptable again, potentially increasing Attempts without prior handler completion.

#### OUTBOX-014-AC-02 — Coalesce signals and cancel waits

- **Given** multiple Wake calls occur and a later wait has a cancelled token.
- **When** WaitAsync consumes or waits for an in-memory hint.
- **Then** wake counts are not persisted; available work, not signal count, drives claims and cancellation can stop a wait.

#### OUTBOX-014-AC-03 — Continue after infrastructure recovery

- **Given** claim/processing infrastructure is temporarily unavailable.
- **When** the running worker logs failures and configured recovery wakes after availability returns.
- **Then** durable work is retried without being marked delivered by infrastructure-error logging.

### OUTBOX-015 — Dead-letter management and cleanup horizon

**Classification:** Observed behaviour

**Evidence:** Source: IOutboxManager/OutboxManager, OutboxCleanupWorker, shared DeadLetterStore and DurableStore.Cleanup. Outbox has no replay-from-admissions API.

Contracts: GetDeadLetters(default 100), Requeue, Discard; inherited Cleanup. Positive listing/cleanup limit and nonnull key collections; empty allowed. Per-key independent transactions, no batch atomicity; ambient mutations rejected. Requeue resets Attempts and claim/due/terminal state but retains diagnostics; discard preserves dead-letter diagnosis. Cleanup immediate/periodic one batch, only terminal timestamps strictly older than UTC retention cutoff. Deletes identity/completion history; pending work survives, no admission archive or outgoing replay is provided.

#### OUTBOX-015-AC-01 — Inspect and manage eligible dead letters

- **Given** eligible, discarded, absent/completed and duplicate keys, or invalid listing/empty mutation inputs.
- **When** manager operations run.
- **Then** positive listing returns nondiscarded dead letters; nonpositive fails; only eligible transitions count, empty collections do nothing, duplicates count Requested but not extra transitions.

#### OUTBOX-015-AC-02 — Preserve diagnosis and partial transitions

- **Given** several eligible keys and later failure/cancellation or ambient Transaction.Current.
- **When** Requeue/Discard executes.
- **Then** successful earlier independent transitions persist; requeue resets Attempts/terminal/claim/due while preserving diagnosis; discard retains dead-letter state; ambient mutation is rejected.

#### OUTBOX-015-AC-03 — Delete only old terminal evidence

- **Given** pending and terminal rows lie before/equal/after cutoff.
- **When** Cleanup runs with positive bounded limit.
- **Then** only strictly older terminal rows are deleted up to limit; pending/equality rows survive and deleted logical pairs can later be staged again.

#### OUTBOX-015-AC-04 — Do not hide cleanup failure

- **Given** a correctly resolved cleanup worker encounters ordinary storage failure or host cancellation.
- **When** its cleanup cycle runs.
- **Then** ordinary failure is warning-logged for later retry; host cancellation stops and neither path claims successful deletion.

#### OUTBOX-015-AC-05 — Reject null key collections and invalid cleanup limit

- **Given** Requeue/Discard receives null or Cleanup receives a nonpositive limit.
- **When** the operation is requested.
- **Then** invalid input fails rather than performing an empty successful operation.

### OUTBOX-016 — EF composition, mappings, options and validation

**Classification:** Observed behaviour

**Evidence:** Source: EF DependencyInjection/OutboxConfiguration/OutboxValidation, project references, OutboxOptions.Section; core AddOutboxTransport. [Configuration](../reference/configuration.md) and [architecture](../reference/architecture.md) are descriptive.

Contracts: services AddOutbox<TDbContext>(callback? / IConfiguration), MessagingBuilder overload, generic/nongeneric DbContextOptionsBuilder.AddMessaging(options,services), ModelBuilder.AddOutbox; Section="Outbox" and inherited durable settings. Outbox factory/default local target/writer singleton; store/processor/dispatcher/tracker/interceptors scoped. Custom singleton dependencies must be lifetime-compatible. AddMessaging options returns unchanged if save interceptor absent; otherwise attaches save plus transaction interceptors and has no duplicate-instance guard. Validator checks relational provider, entity mapping, both interceptor kinds, processor resolution, not migrations/schema/provider guarantees. Inbox/outbox share configured context type.

#### OUTBOX-016-AC-01 — Compose overloads and inherited options

- **Given** host uses service/builder registration callback or Outbox section configuration.
- **When** options resolve and mapped/intercepted context starts.
- **Then** requested durable settings bind; positive validation applies and missing/nonrelational model/interceptors or mismatched persistence context type fail.

#### OUTBOX-016-AC-02 — Attach interceptors only when registered

- **Given** generic/nongeneric options extension is called with or without registered OutboxInterceptor.
- **When** AddMessaging(options,services) runs.
- **Then** absent interceptor leaves options unchanged; present registration attaches both kinds; repeated calls are not guaranteed duplicate-free.

#### OUTBOX-016-AC-03 — Do not equate model validation with schema certification

- **Given** model includes OutboxEntry and both interceptors on relational context, but store schema is missing/incompatible.
- **When** startup validation and later SaveChanges execute.
- **Then** model validation can succeed while real persistence fails; target/transport mapping lengths and unique keys remain provider/schema obligations.

### OUTBOX-017 — Scoped cleanup lifetime requirement

**Classification:** Required behaviour

**Evidence:** Normative: coding standards forbid singleton capture of scoped services. Descriptive counter-evidence: singleton hosted OutboxCleanupWorker directly requires scoped IOutboxStore; [GAP-DUR-005](GAPS.md#gap-dur-005).

Purpose: safe lifetime composition of public workers/custom providers. Required invariant: singleton hosted service cannot consume root scoped persistence. Source appears to violate this; no successful-host validation was executed.

#### OUTBOX-017-AC-01 — Compose without captive cleanup store

- **Given** default outbox registration and provider scope validation enabled.
- **When** hosted cleanup services resolve.
- **Then** required outcome: no scoped store is consumed from singleton/root lifetime; present constructor/registration appears nonconforming.

### OUTBOX-018 — Undecided duplicate, commit and callback policies

**Classification:** Unknown behaviour

**Evidence:** Evidence: tracked writer, target provider, tracker/commit verifier and [GAP-DUR-006](GAPS.md#gap-dur-006), [GAP-DUR-007](GAPS.md#gap-dur-007), [GAP-DUR-008](GAPS.md#gap-dur-008), [GAP-DUR-009](GAPS.md#gap-dur-009). No normative resolution found.

The numbered questions below remain undecided. Preconditions: explicit product policy selection; no implicit repair, guarantee of durable callbacks or idempotent save is approved. Limitations include commit evidence deletion and callback-only/custom capture integrations.

**Undecided questions (not acceptance criteria):**

1. **external-only and empty targeting contract:** What approved policy should apply when a valid event has only external targets or no targets? Decision to resolve: specify empty/local-miss handling without relying on current Get exception or silently acknowledging zero outgoing work.

2. **persisted duplicate and content conflict contract:** What approved policy should apply when the logical pair already exists with equal or conflicting payload? Decision to resolve: choose duplicate suppression, conflict failure or documented uniqueness failure and define tracked versus persisted equivalence/retention scope.

3. **uncertain commit recovery and callback safety:** What approved policy should apply when keys are absent/cleaned, verifier unavailable, or postcommit callback throws? Decision to resolve: define context disposal/recovery, evidence horizon, callback isolation/error reporting and zero-key outcome without declaring commit/rollback from uncertainty.

## Verification matrix

Every row is a recommended future verification, not evidence that a test exists or passed. Contract-decision rows require a decision before a binding assertion can be implemented. Required mismatch rows must not be marked conforming from this audit.

| Acceptance criterion | Recommended test level | Observable assertion / decision | Status |
| --- | --- | --- | --- |
| OUTBOX-001-AC-01 | Relational integration | no successful save, durable rows, consumer completion or external publication is implied | Not tested |
| OUTBOX-001-AC-02 | Unit + relational integration | all targets share fresh MessageId, caller Id as EventId, serialized concrete contract/payload and one creation time; another factory invocation can generate a different MessageId | Not tested |
| OUTBOX-001-AC-03 | Unit | it fails without a successfully staged batch; record construction alone supplies no runtime validation | Not tested |
| OUTBOX-001-AC-04 | Unit + integration | stored target/transport/destination remain the staged values; providers see the concrete runtime type and original payload | Not tested |
| OUTBOX-002-AC-01 | Unit | supplied values win; null correlation inherits current correlation and null causation becomes current delivery MessageId, not its parent causation | Not tested |
| OUTBOX-002-AC-02 | Unit | IOutboxTransport receives an envelope preserving delivery/event/target/destination/payload/context IDs; arbitrary headers or content type are not added by this core envelope | Not tested |
| OUTBOX-003-AC-01 | Unit | invalid IDs/transports fail; duplicate target identity fails independent of destination or transport; distinct case-sensitive IDs remain distinct | Not tested |
| OUTBOX-003-AC-02 | Unit + integration | factory does not itself reject by destination semantics or transport maximum length; provider mapping/persistence/delivery may reject later | Not tested |
| OUTBOX-003-AC-03 | Unit | consumer.message_unhandled fails staging; external provider availability alone does not make default composition external-only | Not tested |
| OUTBOX-003-AC-04 | Unit + integration | IsEmpty is true and no entries are added; empty batch staging is not external delivery | Not tested |
| OUTBOX-004-AC-01 | Relational integration | existing tracked pair is left unchanged and only the missing target is staged; no payload-equivalence guarantee is given | Not tested |
| OUTBOX-004-AC-02 | Relational integration | staging can return normally but unique event/target persistence can fail; successful enqueue is not unconditional durable idempotency | Not tested |
| OUTBOX-004-AC-03 | Relational integration | deleted tracked state does not suppress staging; absent retained logical evidence no longer prevents a new delivery | Not tested |
| OUTBOX-005-AC-01 | Relational integration | both application state and outgoing rows are durable together and available for postcommit processing | Not tested |
| OUTBOX-005-AC-02 | Relational integration | uncommitted rows do not constitute committed outgoing work; acknowledgement/wake based on commit is deferred | Not tested |
| OUTBOX-005-AC-03 | Relational integration | those database changes are not durable outgoing work; in-memory rollback does not reset arbitrary application state | Not tested |
| OUTBOX-006-AC-01 | Unit + integration | Capture authorizes the first only; pending acknowledgements stay deferred until observed commit, with no duplicate capture rows | Not tested |
| OUTBOX-006-AC-02 | Unit | capture state is independent per context/type; acknowledgement callbacks keyed only by eventId do not imply one callback per capture type | Not tested |
| OUTBOX-006-AC-03 | Unit | invalid lifecycle/input is rejected rather than silently treated as a successful capture | Not tested |
| OUTBOX-006-AC-04 | Unit | saving/committing state reflects the phase; pending keys are exposed during commit rather than proving transaction success | Not tested |
| OUTBOX-007-AC-01 | Fault-injection integration | newly captured current-save entries detach; pre-staged/earlier pending entries are not all discarded and acknowledgement does not run | Not tested |
| OUTBOX-007-AC-02 | Relational integration | failed events can be captured again; observed rollback removes recorded outgoing tracked keys without pretending to revert arbitrary domain state | Not tested |
| OUTBOX-007-AC-03 | Unit + integration | it fails rather than silently acknowledging or transferring pending work into another transaction | Not tested |
| OUTBOX-008-AC-01 | Fault-injection integration | tracker acknowledges commit and can wake outgoing processing while original commit-failure handling remains subject to EF behaviour | Not tested |
| OUTBOX-008-AC-02 | Fault-injection integration | absent-key evidence selects rollback tracking; verification error selects unknown state and no definite acknowledgement | Not tested |
| OUTBOX-008-AC-03 | Fault-injection integration | no committed outcome is inferred from empty keys; acknowledgement and application state certainty are not proved | Not tested |
| OUTBOX-008-AC-04 | Fault-injection integration | verification uses an independent None token; its success/error, not the cancelled commit token alone, controls tracker reconciliation | Not tested |
| OUTBOX-009-AC-01 | Relational integration | NotSupportedException reports unsupported tracking rather than a silently partial state rewind | Not tested |
| OUTBOX-009-AC-02 | Relational integration | the active-save path is permitted and save success/failure still controls capture bookkeeping | Not tested |
| OUTBOX-009-AC-03 | Fault-injection integration | unsupported lifecycle can be rejected; caller must reconcile/discard rather than infer event acknowledgement from raw commit | Not tested |
| OUTBOX-010-AC-01 | Unit + integration | events are acknowledged and outgoing work is signalled without proving any target consumer or external transport has completed | Not tested |
| OUTBOX-010-AC-02 | Fault-injection integration | the database can already be committed while acknowledgement sequence/wake/cleanup is interrupted; recovery can still discover persisted rows | Not tested |
| OUTBOX-010-AC-03 | Unit | callbacks alone do not make PendingCommit nonempty; registration is not a proven durable postcommit scheduling facility | Not tested |
| OUTBOX-010-AC-04 | Fault-injection integration | the reconciliation catch can classify tracker state as unknown without rethrowing that callback exception; durable commit remains possible and later callbacks/wake/cleanup can be incomplete | Not tested |
| OUTBOX-011-AC-01 | Fault-injection integration | external publication may repeat with the same delivery/event/target/context identity; no exactly-once downstream effects are claimed | Not tested |
| OUTBOX-011-AC-02 | Unit + integration | durable completion is not manufactured; failure/cancellation remains distinguishable for processor policy | Not tested |
| OUTBOX-011-AC-03 | Fault-injection integration | false completion cannot overwrite the new owner; publication already happened and repeat remains possible | Not tested |
| OUTBOX-012-AC-01 | Unit | duplicate construction or missing exact ordinal lookup throws rather than selecting an arbitrary route | Not tested |
| OUTBOX-012-AC-02 | Relational integration | success commits state/completion/new outbox together; failure or false rolls back that local transaction | Not tested |
| OUTBOX-012-AC-03 | Relational integration | it reports completed without reinvoking local effects; cleaned records or external effects are outside this retained-completion protection | Not tested |
| OUTBOX-012-AC-04 | Composition integration | the registered transport receives its persisted envelope and successful publication is followed by separate completion | Not tested |
| OUTBOX-013-AC-01 | Relational integration | only eligible bounded work is owned with increased Attempts; invalid limits/lease fail and stale/expired Renew/Complete/Retry/DeadLetter return false | Not tested |
| OUTBOX-013-AC-02 | Relational integration | successful target alone completes; failed target retries or dead-letters according to transient/permanent/unknown classification with preserved identity/payload/diagnostics | Not tested |
| OUTBOX-013-AC-03 | Unit | delivery MessageId/correlation/causation/target are exposed during dispatch and previous context restored afterward | Not tested |
| OUTBOX-013-AC-04 | Unit + integration | host cancellation propagates without ordinary failure settlement; claim-loss stops authorizing settlement and leaves unresolved work recoverable | Not tested |
| OUTBOX-013-AC-05 | Fault-injection integration | the error is observable/logged and unresolved work recovers after expiry; separate settlement timeout may run after host stop | Not tested |
| OUTBOX-014-AC-01 | Fault-injection integration | eligible committed work becomes attemptable again, potentially increasing Attempts without prior handler completion | Not tested |
| OUTBOX-014-AC-02 | Unit | wake counts are not persisted; available work, not signal count, drives claims and cancellation can stop a wait | Not tested |
| OUTBOX-014-AC-03 | Fault-injection integration | durable work is retried without being marked delivered by infrastructure-error logging | Not tested |
| OUTBOX-015-AC-01 | Relational integration | positive listing returns nondiscarded dead letters; nonpositive fails; only eligible transitions count, empty collections do nothing, duplicates count Requested but not extra transitions | Not tested |
| OUTBOX-015-AC-02 | Relational integration | successful earlier independent transitions persist; requeue resets Attempts/terminal/claim/due while preserving diagnosis; discard retains dead-letter state; ambient mutation is rejected | Not tested |
| OUTBOX-015-AC-03 | Relational integration | only strictly older terminal rows are deleted up to limit; pending/equality rows survive and deleted logical pairs can later be staged again | Not tested |
| OUTBOX-015-AC-04 | Fault-injection integration | ordinary failure is warning-logged for later retry; host cancellation stops and neither path claims successful deletion | Not tested |
| OUTBOX-015-AC-05 | Unit + integration | invalid input fails rather than performing an empty successful operation | Not tested |
| OUTBOX-016-AC-01 | Composition integration | requested durable settings bind; positive validation applies and missing/nonrelational model/interceptors or mismatched persistence context type fail | Not tested |
| OUTBOX-016-AC-02 | Composition integration | absent interceptor leaves options unchanged; present registration attaches both kinds; repeated calls are not guaranteed duplicate-free | Not tested |
| OUTBOX-016-AC-03 | Relational integration | model validation can succeed while real persistence fails; target/transport mapping lengths and unique keys remain provider/schema obligations | Not tested |
| OUTBOX-017-AC-01 | Composition integration | required outcome: no scoped store is consumed from singleton/root lifetime; present constructor/registration appears nonconforming | Not tested |

## Owned source and public contract traceability

Every authored .cs/.csproj/.md file in the two owned projects is included, excluding bin/obj. A dash means no publicly accessible type/member in that file; implementation operations remain recorded in responsibility. Exact signatures/positional members are retained in the session audit JSON. Constructor/service composition is covered with its owning type.

| Source path | Public contracts / operations | Responsibility | Specifications |
| --- | --- | --- | --- |
| [src\Clean.Messaging.Outbox\Clean.Messaging.Outbox.csproj](../../src/Clean.Messaging.Outbox/Clean.Messaging.Outbox.csproj) | — (internal implementation or project metadata) | Neutral outbox project references core messaging; no EF or external transport implementation dependency. | OUTBOX-016 |
| [src\Clean.Messaging.Outbox\Configuration\OutboxOptions.cs](../../src/Clean.Messaging.Outbox/Configuration/OutboxOptions.cs) | `Clean.Messaging.Outbox.Configuration.public sealed class OutboxOptions : MessageOptions`<br>`Clean.Messaging.Outbox.Configuration.OutboxOptions: public const string Section = "Outbox";` | Outbox configuration section and inherited durable settings. | OUTBOX-013, OUTBOX-014, OUTBOX-015, OUTBOX-016 |
| [src\Clean.Messaging.Outbox\Delivery\IOutboxDelivery.cs](../../src/Clean.Messaging.Outbox/Delivery/IOutboxDelivery.cs) | `Clean.Messaging.Outbox.Delivery.public interface IOutboxDelivery`<br>`Clean.Messaging.Outbox.Delivery.IOutboxDelivery: ValueTask<bool> Dispatch( OwnedEntry<OutboxEntry> owned, CancellationToken cancellationToken)`<br>`Clean.Messaging.Outbox.Delivery.IOutboxDelivery: string Transport { get; }` | Delivery extension port exposes ordinal transport name and owned completion/ownership-loss outcome. | OUTBOX-011, OUTBOX-012 |
| [src\Clean.Messaging.Outbox\Delivery\IOutboxDispatcher.cs](../../src/Clean.Messaging.Outbox/Delivery/IOutboxDispatcher.cs) | `Clean.Messaging.Outbox.Delivery.public interface IOutboxDispatcher`<br>`Clean.Messaging.Outbox.Delivery.IOutboxDispatcher: ValueTask<bool> Dispatch( OwnedEntry<OutboxEntry> owned, CancellationToken cancellationToken)` | Owned persisted-target dispatch port. | OUTBOX-011, OUTBOX-012, OUTBOX-013 |
| [src\Clean.Messaging.Outbox\Delivery\OutboxDispatcher.cs](../../src/Clean.Messaging.Outbox/Delivery/OutboxDispatcher.cs) | `Clean.Messaging.Outbox.Delivery.public sealed class OutboxDispatcher( IEnumerable<IOutboxDelivery> deliveries) : IOutboxDispatcher`<br>`Clean.Messaging.Outbox.Delivery.OutboxDispatcher: public ValueTask<bool> Dispatch( OwnedEntry<OutboxEntry> owned, CancellationToken cancellationToken)` | Builds ordinal delivery lookup, rejects duplicates and dispatches stored transport names. | OUTBOX-011, OUTBOX-012 |
| [src\Clean.Messaging.Outbox\DependencyInjection.cs](../../src/Clean.Messaging.Outbox/DependencyInjection.cs) | `Clean.Messaging.Outbox.public static class DependencyInjection`<br>`Clean.Messaging.Outbox.DependencyInjection: public static IServiceCollection AddOutboxTransport<TTransport>( this IServiceCollection services)` | Public AddOutboxTransport<TTransport> scoped concrete transport/delivery registration extension. | OUTBOX-011, OUTBOX-012, OUTBOX-016 |
| [src\Clean.Messaging.Outbox\IOutbox.cs](../../src/Clean.Messaging.Outbox/IOutbox.cs) | `Clean.Messaging.Outbox.public sealed record OutboxMessage<T>( Guid Id, T Message, Guid? CorrelationId = null, Guid? CausationId = null)`<br>`Clean.Messaging.Outbox.public interface IOutbox`<br>`Clean.Messaging.Outbox.IOutbox: void Enqueue<TMessage>( OutboxMessage<TMessage> message)` | Synchronous enqueue port and typed outgoing envelope with logical event Id and optional metadata. | OUTBOX-001, OUTBOX-002 |
| [src\Clean.Messaging.Outbox\Outbox.cs](../../src/Clean.Messaging.Outbox/Outbox.cs) | `Clean.Messaging.Outbox.public sealed class Outbox( OutboxEntryFactory entries, IOutboxStore store) : IOutbox`<br>`Clean.Messaging.Outbox.Outbox: public void Enqueue<T>( OutboxMessage<T> message)` | Creates a target batch then stages it in the scoped store without saving or dispatching. | OUTBOX-001, OUTBOX-004 |
| [src\Clean.Messaging.Outbox\Persistence\IOutboxStore.cs](../../src/Clean.Messaging.Outbox/Persistence/IOutboxStore.cs) | `Clean.Messaging.Outbox.Persistence.public sealed record OutboxBatch( Guid MessageId, Guid EventId, IReadOnlyList<OutboxEntry> Entries)`<br>`Clean.Messaging.Outbox.Persistence.OutboxBatch: public bool IsEmpty => Entries.Count == 0;`<br>`Clean.Messaging.Outbox.Persistence.public interface IOutboxStore : IDurableStore<OutboxEntry>`<br>`Clean.Messaging.Outbox.Persistence.IOutboxStore: void Stage(OutboxBatch batch);` | Stage port, OutboxBatch envelope identity/entries/IsEmpty and inherited durable lifecycle operations. | OUTBOX-001, OUTBOX-004, OUTBOX-013, OUTBOX-015 |
| [src\Clean.Messaging.Outbox\Persistence\OutboxEntry.cs](../../src/Clean.Messaging.Outbox/Persistence/OutboxEntry.cs) | `Clean.Messaging.Outbox.Persistence.public sealed class OutboxEntry : IDurableEntry`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public Guid MessageId`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public Guid EventId`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public required string TargetId`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public required string Transport`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public string? Destination`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public required MessageData Message`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public Guid? CorrelationId`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public Guid? CausationId`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public DateTime CreatedAt`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public int Attempts`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public string? ClaimId`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public DateTime? ClaimedUntil`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public DateTime? NextAttemptAt`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public DateTime? ProcessedAt`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public DateTime? DeadLetteredAt`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public DateTime? DiscardedAt`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public FailureCategory? FailureCategory`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public string? FailureCode`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public string? ExceptionType`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public string? FailureMessage`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public DateTime? LastFailedAt`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public int? LastFailedAttempt`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntry: public EntryKey Key => new( MessageId, TargetId)` | Per-target transport/destination plus common envelope, key, mutable attempts/claim/retry/terminal/diagnostic properties. | OUTBOX-001, OUTBOX-002, OUTBOX-003, OUTBOX-004, OUTBOX-013, OUTBOX-015 |
| [src\Clean.Messaging.Outbox\Persistence\OutboxEntryFactory.cs](../../src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs) | `Clean.Messaging.Outbox.Persistence.public sealed class OutboxEntryFactory( IEnumerable<IOutboxTargetProvider> targets, IMessageSerializer serializer, IMessageContext context, TimeProvider time)`<br>`Clean.Messaging.Outbox.Persistence.OutboxEntryFactory: public OutboxBatch Create<T>( OutboxMessage<T> message)` | Creates concrete serialized batches, new delivery identity, metadata inheritance, target snapshot and validation. | OUTBOX-001, OUTBOX-002, OUTBOX-003 |
| [src\Clean.Messaging.Outbox\Runtime\IOutboxManager.cs](../../src/Clean.Messaging.Outbox/Runtime/IOutboxManager.cs) | `Clean.Messaging.Outbox.Runtime.public interface IOutboxManager`<br>`Clean.Messaging.Outbox.Runtime.IOutboxManager: ValueTask<IReadOnlyList<DeadLetter>> GetDeadLetters( int limit = 100, CancellationToken cancellationToken = default)`<br>`Clean.Messaging.Outbox.Runtime.IOutboxManager: ValueTask<RequeueResult> Requeue( IReadOnlyCollection<EntryKey> entries, CancellationToken cancellationToken = default)`<br>`Clean.Messaging.Outbox.Runtime.IOutboxManager: ValueTask<int> Discard( IReadOnlyCollection<EntryKey> entries, CancellationToken cancellationToken = default)` | Dead-letter list/requeue/discard port and cancellation/defaults; no replay API. | OUTBOX-015 |
| [src\Clean.Messaging.Outbox\Runtime\OutboxCleanupWorker.cs](../../src/Clean.Messaging.Outbox/Runtime/OutboxCleanupWorker.cs) | `Clean.Messaging.Outbox.public sealed partial class OutboxCleanupWorker( IOutboxStore store, TimeProvider time, IOptions<OutboxOptions> options, ILogger<OutboxCleanupWorker> logger) : BackgroundService`<br>`Clean.Messaging.Outbox.OutboxCleanupWorker: protected override async Task ExecuteAsync(CancellationToken stoppingToken)` | Periodic bounded cleanup and logs/cancellation; singleton worker captures scoped store. | OUTBOX-015, OUTBOX-017 |
| [src\Clean.Messaging.Outbox\Runtime\OutboxManager.cs](../../src/Clean.Messaging.Outbox/Runtime/OutboxManager.cs) | `Clean.Messaging.Outbox.Runtime.public sealed class OutboxManager( IDeadLetterStore<OutboxEntry> store, OutboxSignal signal) : IOutboxManager`<br>`Clean.Messaging.Outbox.Runtime.OutboxManager: public ValueTask<IReadOnlyList<DeadLetter>> GetDeadLetters( int limit = 100, CancellationToken cancellationToken = default)`<br>`Clean.Messaging.Outbox.Runtime.OutboxManager: public async ValueTask<RequeueResult> Requeue( IReadOnlyCollection<EntryKey> entries, CancellationToken cancellationToken = default)`<br>`Clean.Messaging.Outbox.Runtime.OutboxManager: public async ValueTask<int> Discard( IReadOnlyCollection<EntryKey> entries, CancellationToken cancellationToken = default)` | Validates inputs and manages sequential independent dead-letter transitions/counts and requeue wake. | OUTBOX-015 |
| [src\Clean.Messaging.Outbox\Runtime\OutboxProcessor.cs](../../src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs) | `Clean.Messaging.Outbox.public sealed partial class OutboxProcessor( IOutboxStore store, IOutboxDispatcher dispatcher, ClaimRenewal renewal, MessageContext context, RetryPolicy retry, MessageFailureFactory failures, TimeProvider time, IOptions<OutboxOptions> options, ILogger<OutboxProcessor> logger) : IWorkProcessor<OutboxEntry>`<br>`Clean.Messaging.Outbox.OutboxProcessor: public async ValueTask Process( OwnedEntry<OutboxEntry> owned, CancellationToken cancellationToken)` | Ambient delivery metadata, renewal, retry/failure settlement and logs/metrics with cancellation distinction. | OUTBOX-013, OUTBOX-014 |
| [src\Clean.Messaging.Outbox\Runtime\OutboxRecoveryWorker.cs](../../src/Clean.Messaging.Outbox/Runtime/OutboxRecoveryWorker.cs) | `Clean.Messaging.Outbox.Runtime.public sealed class OutboxRecoveryWorker( OutboxSignal signal, TimeProvider time, IOptions<OutboxOptions> options) : BackgroundService`<br>`Clean.Messaging.Outbox.Runtime.OutboxRecoveryWorker: protected override async Task ExecuteAsync(CancellationToken stoppingToken)` | Periodic coalesced wake for due/expired durable work and hosted shutdown. | OUTBOX-014 |
| [src\Clean.Messaging.Outbox\Runtime\OutboxSignal.cs](../../src/Clean.Messaging.Outbox/Runtime/OutboxSignal.cs) | `Clean.Messaging.Outbox.Runtime.public sealed class OutboxSignal : IWorkSignal`<br>`Clean.Messaging.Outbox.Runtime.OutboxSignal: public ValueTask WaitAsync( CancellationToken cancellationToken)`<br>`Clean.Messaging.Outbox.Runtime.OutboxSignal: public void Wake()` | Outbox in-memory Wake/WaitAsync hint; not a durable completion ledger. | OUTBOX-014 |
| [src\Clean.Messaging.Outbox\Runtime\OutboxWorker.cs](../../src/Clean.Messaging.Outbox/Runtime/OutboxWorker.cs) | `Clean.Messaging.Outbox.Runtime.public sealed class OutboxWorker( WorkLoop<OutboxEntry> loop, OutboxSignal signal, IOptions<OutboxOptions> options) : BackgroundService`<br>`Clean.Messaging.Outbox.Runtime.OutboxWorker: protected override Task ExecuteAsync( CancellationToken stoppingToken)` | Hosted bounded WorkLoop adaptation and processing/lease/recovery option projection. | OUTBOX-014, OUTBOX-016 |
| [src\Clean.Messaging.Outbox\Transport\IOutboxTargetProvider.cs](../../src/Clean.Messaging.Outbox/Transport/IOutboxTargetProvider.cs) | `Clean.Messaging.Outbox.Transport.public sealed record OutboxTarget( string TargetId, string Transport, string? Destination)`<br>`Clean.Messaging.Outbox.Transport.public interface IOutboxTargetProvider`<br>`Clean.Messaging.Outbox.Transport.IOutboxTargetProvider: IEnumerable<OutboxTarget> GetTargets( Type messageType, object message)` | Target provider port and target identity/transport/destination record. | OUTBOX-001, OUTBOX-003 |
| [src\Clean.Messaging.Outbox\Transport\IOutboxTransport.cs](../../src/Clean.Messaging.Outbox/Transport/IOutboxTransport.cs) | `Clean.Messaging.Outbox.Transport.public interface IOutboxTransport`<br>`Clean.Messaging.Outbox.Transport.IOutboxTransport: ValueTask Dispatch( OutboxEnvelope envelope, CancellationToken cancellationToken)`<br>`Clean.Messaging.Outbox.Transport.IOutboxTransport: string Name { get; }` | External transport name/publication port receiving persisted OutboxEnvelope and cancellation. | OUTBOX-011, OUTBOX-012 |
| [src\Clean.Messaging.Outbox\Transport\LocalTransport.cs](../../src/Clean.Messaging.Outbox/Transport/LocalTransport.cs) | `Clean.Messaging.Outbox.Transport.public static class LocalTransport`<br>`Clean.Messaging.Outbox.Transport.LocalTransport: public const string Name = "local";` | Stable built-in local transport name constant. | OUTBOX-003, OUTBOX-012 |
| [src\Clean.Messaging.Outbox\Transport\OutboxEnvelope.cs](../../src/Clean.Messaging.Outbox/Transport/OutboxEnvelope.cs) | `Clean.Messaging.Outbox.Transport.public sealed record OutboxEnvelope( Guid MessageId, Guid EventId, string TargetId, string? Destination, MessageData Message, Guid? CorrelationId, Guid? CausationId)` | Persisted external envelope delivery/event/target/destination/payload/context representation. | OUTBOX-002, OUTBOX-011 |
| [src\Clean.Messaging.Outbox\Transport\OutboxTargetProvider.cs](../../src/Clean.Messaging.Outbox/Transport/OutboxTargetProvider.cs) | `Clean.Messaging.Outbox.Transport.public sealed class OutboxTargetProvider( IConsumerRegistry consumers) : IOutboxTargetProvider`<br>`Clean.Messaging.Outbox.Transport.OutboxTargetProvider: public IEnumerable<OutboxTarget> GetTargets( Type messageType, object message)` | Default exact-type registered consumer fan-out into local targets; no-consumer Get throws. | OUTBOX-003 |
| [src\Clean.Messaging.Outbox\Transport\OutboxTransport.cs](../../src/Clean.Messaging.Outbox/Transport/OutboxTransport.cs) | — (internal implementation or project metadata) | Internal IOutboxDelivery.Transport/Dispatch adapter publishes then separately fences completion. | OUTBOX-002, OUTBOX-011, OUTBOX-012 |
| [src\Clean.Messaging.Outbox.EntityFrameworkCore\Clean.Messaging.Outbox.EntityFrameworkCore.csproj](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/Clean.Messaging.Outbox.EntityFrameworkCore.csproj) | — (internal implementation or project metadata) | EF relational outbox adapter dependencies and neutral project references. | OUTBOX-016 |
| [src\Clean.Messaging.Outbox.EntityFrameworkCore\DependencyInjection.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/DependencyInjection.cs) | `Clean.Messaging.Outbox.EntityFrameworkCore.public static class DependencyInjection`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.DependencyInjection: public static IServiceCollection AddOutbox<TDbContext>( this IServiceCollection services, Action<OutboxOptions>? configure = null)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.DependencyInjection: public static IServiceCollection AddOutbox<TDbContext>( this IServiceCollection services, IConfiguration configuration)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.DependencyInjection: public static MessagingBuilder<TDbContext> AddOutbox<TDbContext>( this MessagingBuilder<TDbContext> builder, Action<OutboxOptions>? configure = null)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.DependencyInjection: public static DbContextOptionsBuilder AddMessaging( this DbContextOptionsBuilder options, IServiceProvider services)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.DependencyInjection: public static DbContextOptionsBuilder<TDbContext> AddMessaging<TDbContext>( this DbContextOptionsBuilder<TDbContext> options, IServiceProvider services)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.DependencyInjection: public static ModelBuilder AddOutbox( this ModelBuilder modelBuilder)` | Public AddOutbox callback/configuration/builder, generic/nongeneric options AddMessaging and model extensions; tracker/interceptors/lifetimes. | OUTBOX-006, OUTBOX-009, OUTBOX-016, OUTBOX-017 |
| [src\Clean.Messaging.Outbox.EntityFrameworkCore\ISaveCapture.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/ISaveCapture.cs) | `Clean.Messaging.Outbox.EntityFrameworkCore.public interface ISaveCapture`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.ISaveCapture: void Capture(DbContext db);` | Public Capture(DbContext) extension participating in intercepted owning saves. | OUTBOX-006, OUTBOX-007, OUTBOX-010 |
| [src\Clean.Messaging.Outbox.EntityFrameworkCore\LocalOutboxDelivery.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/LocalOutboxDelivery.cs) | — (internal implementation or project metadata) | Internal IOutboxDelivery.Transport/Dispatch with scoped transactional consumer effects and completion. | OUTBOX-005, OUTBOX-012 |
| [src\Clean.Messaging.Outbox.EntityFrameworkCore\OutboxCommitVerifier.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxCommitVerifier.cs) | — (internal implementation or project metadata) | Internal IsCommitted/IsCommittedAsync fresh-context all-key evidence check; empty keys not success. | OUTBOX-008 |
| [src\Clean.Messaging.Outbox.EntityFrameworkCore\OutboxConfiguration.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxConfiguration.cs) | — (internal implementation or project metadata) | Internal Configure maps common durable entry constraints plus transport/destination. | OUTBOX-004, OUTBOX-013, OUTBOX-016 |
| [src\Clean.Messaging.Outbox.EntityFrameworkCore\OutboxInterceptor.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxInterceptor.cs) | — (internal implementation or project metadata) | Internal sync/async Saving/Saved/Failed/Canceled callbacks and ISaveCapture execution preserving original failure. | OUTBOX-005, OUTBOX-006, OUTBOX-007, OUTBOX-010 |
| [src\Clean.Messaging.Outbox.EntityFrameworkCore\OutboxStore.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxStore.cs) | — (internal implementation or project metadata) | Internal Stage adapter delegates owning-context writer; inherited fenced durable store operations. | OUTBOX-004, OUTBOX-013, OUTBOX-015 |
| [src\Clean.Messaging.Outbox.EntityFrameworkCore\OutboxTracker.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs) | `Clean.Messaging.Outbox.EntityFrameworkCore.public sealed class OutboxTracker( OutboxSignal signal)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.OutboxTracker: public bool IsSaving( DbContext db)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.OutboxTracker: public bool IsCommitting( DbContext db)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.OutboxTracker: public void BeginSave( DbContext db)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.OutboxTracker: public bool Capture<TCapture>( DbContext db, Guid eventId)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.OutboxTracker: public void AcknowledgeAfterCommit( DbContext db, Guid eventId, Action acknowledge)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.OutboxTracker: public void TrackEntries( DbContext db)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.OutboxTracker: public void SaveSucceeded( DbContext db)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.OutboxTracker: public void SaveFailed( DbContext db)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.OutboxTracker: public void TransactionCommitting( DbContext db)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.OutboxTracker: public void TransactionCommitted( DbContext db)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.OutboxTracker: public EntryKey[] GetPendingKeys( DbContext db)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.OutboxTracker: public void TransactionCommitUnknown( DbContext db)`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.OutboxTracker: public void TransactionRolledBack( DbContext db)` | Public per-context save/commit capture protocol, key/callback bookkeeping, rollback/unknown outcomes and lifecycle validation. | OUTBOX-006, OUTBOX-007, OUTBOX-008, OUTBOX-009, OUTBOX-010, OUTBOX-018 |
| [src\Clean.Messaging.Outbox.EntityFrameworkCore\OutboxTransactionInterceptor.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs) | — (internal implementation or project metadata) | Internal sync/async committing/committed/rolled-back/failed and savepoint callbacks; key verification and uncertainty handling. | OUTBOX-005, OUTBOX-007, OUTBOX-008, OUTBOX-009, OUTBOX-010 |
| [src\Clean.Messaging.Outbox.EntityFrameworkCore\OutboxValidation.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxValidation.cs) | — (internal implementation or project metadata) | Internal StartAsync/StopAsync model/relational/interceptor/processor validation with physical schema limitations. | OUTBOX-016, OUTBOX-017 |
| [src\Clean.Messaging.Outbox.EntityFrameworkCore\OutboxWriter.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs) | `Clean.Messaging.Outbox.EntityFrameworkCore.public interface IOutboxWriter`<br>`Clean.Messaging.Outbox.EntityFrameworkCore.IOutboxWriter: void Stage( DbContext db, OutboxBatch batch)` | Public writer Stage(DbContext,batch) port; internal tracked logical-pair suppression and missing-row AddRange. | OUTBOX-003, OUTBOX-004 |

### Shared dependency trace (excluded from owned manifest count)

| Source path | Reviewed responsibility | Related specifications |
| --- | --- | --- |
| [src\Clean.Messaging\Consumers\ConsumerRegistry.cs](../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs) | Exact-type/name consumer selection, permanent faults | INBOX-001, INBOX-009; OUTBOX-003; DOMAIN-007 |
| [src\Clean.Messaging\Processing\ConsumerDispatcher.cs](../../src/Clean.Messaging/Processing/ConsumerDispatcher.cs) | Persisted contract deserialization and scoped invocation | INBOX-005; OUTBOX-012 |
| [src\Clean.Messaging\Serialization\MessageSerializer.cs](../../src/Clean.Messaging/Serialization/MessageSerializer.cs) | Concrete runtime contract serialization and fault classification | INBOX-001; OUTBOX-001; DOMAIN-004, DOMAIN-007 |
| [src\Clean.Messaging\MessageContext.cs](../../src/Clean.Messaging/MessageContext.cs) | Push/restoration and ambient IDs | INBOX-006; OUTBOX-002, OUTBOX-013 |
| [src\Clean.Messaging\Processing\WorkLoop.cs](../../src/Clean.Messaging/Processing/WorkLoop.cs) | Bounded queue, scoped claim/processing and recovery | INBOX-007; OUTBOX-014 |
| [src\Clean.Messaging\Processing\ClaimRenewal.cs](../../src/Clean.Messaging/Processing/ClaimRenewal.cs) | Renewal error/lost-claim cancellation | INBOX-006; OUTBOX-013 |
| [src\Clean.Messaging\Processing\AsyncAutoResetEvent.cs](../../src/Clean.Messaging/Processing/AsyncAutoResetEvent.cs) | Coalesced cancelable in-memory wake | INBOX-007; OUTBOX-014 |
| [src\Clean.Messaging\Retry\RetryPolicy.cs](../../src/Clean.Messaging/Retry/RetryPolicy.cs) | Retry classification and delay threshold | INBOX-006; OUTBOX-013 |
| [src\Clean.Messaging\Failures\MessageFailure.cs](../../src/Clean.Messaging/Failures/MessageFailure.cs) | Retained bounded diagnostics/classification | INBOX-004, INBOX-006; OUTBOX-013 |
| [src\Clean.Messaging\MessageOptions.cs](../../src/Clean.Messaging/MessageOptions.cs) | Inherited durable option members and defaults | INBOX-011; OUTBOX-016 |
| [src\Clean.Messaging\Processing\MessageOptionsValidator.cs](../../src/Clean.Messaging/Processing/MessageOptionsValidator.cs) | Positive and cross-option validation | INBOX-011; OUTBOX-016 |
| [src\Clean.Messaging\Persistence\IDurableStore.cs](../../src/Clean.Messaging/Persistence/IDurableStore.cs) | Inherited Claim/Renew/Complete/Retry/DeadLetter/Cleanup and OwnedEntry | INBOX-004; OUTBOX-013 |
| [src\Clean.Messaging\Persistence\IDurableEntry.cs](../../src/Clean.Messaging/Persistence/IDurableEntry.cs) | Common entry identity/state contract | INBOX-004; OUTBOX-013 |
| [src\Clean.Messaging.EntityFrameworkCore\DurableStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs) | Claim ordering/eligibility, ownership fence, lifecycle updates/cleanup | INBOX-004, INBOX-010; OUTBOX-013, OUTBOX-015 |
| [src\Clean.Messaging.EntityFrameworkCore\DeadLetterStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs) | Independent management and diagnostic retention | INBOX-008; OUTBOX-015 |
| [src\Clean.Messaging.EntityFrameworkCore\DurableEntryConfiguration.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableEntryConfiguration.cs) | Composite/logical unique keys and state mapping | INBOX-011; OUTBOX-004, OUTBOX-016 |
| [src\Clean.Messaging.EntityFrameworkCore\PersistenceServices.cs](../../src/Clean.Messaging.EntityFrameworkCore/PersistenceServices.cs) | Same scoped persistence context and options composition | INBOX-011; OUTBOX-016 |

Management shared result contracts `DeadLetter(Key, EventId, DeadLetteredAt, Attempts, Code, Error)` and `RequeueResult(Requested, Requeued)` are referenced through the manager ports. No public API XML in these owned files supplies a stronger duplicate, completion or retention promise.

## Review limitations

- No production/test changes or test/build execution were performed. Verification matrices are entirely **Not tested**.
- Source-derived ordering, transaction interception and uniqueness outcomes are not provider certification. Fault-injection and real relational concurrency verification remain necessary.
- The [gap register](GAPS.md) records unresolved decisions and their evidence. Observed suspected mismatches are not approved fixes.
