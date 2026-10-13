# Inbox behavioural specifications

**Status:** Draft. **Source reviewed:** 2026-10-13. **Verification:** Source-reviewed; not tested.

## Interpretation and evidence rules

Required behaviour is grounded only in normative repository instructions or explicit API XML requirements. Observed behaviour is derived from inspected implementation. Source-derived Reference and Explanation pages are descriptive corroboration, not independent normative authority. Proposed behaviour is explicitly **unapproved**. Unknown behaviour uses **numbered undecided questions**, without acceptance-criterion IDs or verification rows. Every acceptance criterion inherits its specification classification unless explicitly marked otherwise; none below overrides it. Required criteria can identify suspected source mismatches and are not claims of conformance.

## Capability purpose, contracts and common boundaries

Admission -> retained original envelope and per-consumer entries -> claim/renew -> transactional local handler and fenced completion -> retry/dead-letter -> management/replay -> entry-only cleanup.

- **Preconditions:** consumer host owns registration, valid durable contracts and identities, configured relational schema/transactions where applicable, stable payload/source state and suitable cancellation/lifetime composition. Each focused specification narrows these preconditions.
- **Postconditions:** only observable admitted, staged, committed, acknowledged or per-target lifecycle outcomes explicitly stated below; success at one boundary does not imply success at another.
- **Invariants:** preserve logical event identity versus delivery identity, concrete durable contract names, original envelope/context metadata, target-specific completion, ownership fences, retry classifications and owning-context transaction boundaries. Repeated external effects remain possible.
- **Failures:** validation/serialization/resolution/database/transport failures and cancellation remain distinct; ordinary logging is not durable success. Source exceptions/cancellation racing commit require evidence, not an assumption of rollback.
- **Concurrency:** contexts and source collections are not automatically thread-safe. Lease ownership is fenced at settlement, not permission to suppress repeated external calls. Recovery requires a running healthy host/store; no strict latency, fairness or global completion order is specified.
- **Limitations:** Admission completion is not local-handler completion or transport acknowledgement. Independent admission does not commit arbitrary caller context changes. Exactly-once external effects, permanent target dedup after cleanup, no-consumer retention, replay cursor pagination and admission payload expiry are not supplied.

### Common public model and lifecycle contracts

Record positional fields, exposed init/set properties, constants, generic constraints, explicit interface implementations and protected extension operations are included in the source/contract table. Public primary constructors and inherited hosted StartAsync/StopAsync/Dispose behaviour are composition surfaces, not additional delivery guarantees. For entries, all identity/payload/context fields, Attempts, ClaimId, ClaimedUntil, NextAttemptAt, ProcessedAt, DeadLetteredAt, DiscardedAt, FailureCategory, FailureCode, ExceptionType, FailureMessage, LastFailedAt, LastFailedAttempt and derived Key participate in the lifecycle specifications. Port implementations (including internal EF adapters) are separately traced by responsibility.

### Supporting evidence reviewed

[Repository instructions](../../.github/copilot-instructions.md), [coding standards](../CODING_STANDARDS.md), [documentation index](../README.md); references: [architecture](../reference/architecture.md), [configuration](../reference/configuration.md), [contracts](../reference/contracts.md), [inbox](../reference/inbox.md), [outbox](../reference/outbox.md), [domain events](../reference/domain-events.md), [persistence](../reference/persistence.md), [processing](../reference/processing.md), [transport](../reference/transport.md). Explanations: [capability boundaries](../explanation/capability-boundaries.md), [message identity](../explanation/message-identity.md), [transaction boundaries](../explanation/transaction-boundaries.md), [delivery semantics](../explanation/delivery-semantics.md), [durable workflows](../explanation/durable-workflows.md). Adjacent ingress/broker/mapper APIs are outside these owned files; their source-derived descriptions are not independently tested guarantees.

### INBOX-001 — Typed admission and envelope identity

**Classification:** Observed behaviour

**Evidence:** Source: `Inbox.Accept`, `InboxMessage<T>`, `InboxBatch`, `InboxAdmission`; shared serializer and exact-type consumer registry. Reference: [inbox](../reference/inbox.md) and [contracts](../reference/contracts.md) are descriptive, not normative.

Purpose: admit a typed delivery for later local consumption. Contracts: `IInbox.Accept<T>` / `Inbox.Accept<TMessage>`; both inbox message records; admission/batch envelope members. Preconditions: nonnull envelope and payload, nonempty Id and supplied EventId, runtime payload contract and consumers registered. Postcondition: successful new admission retains one original envelope and one unresolved entry per current consumer. Invariants: `MessageId = Id`; `EventId = supplied EventId ?? Id`; payload, CreatedAt and context metadata are common to targets. The nongeneric record is representation only: there is no nongeneric Accept operation. Constructor/required members do not independently validate runtime values.

#### INBOX-001-AC-01 — Admit without claiming consumer completion

- **Given** a valid typed envelope with two registered exact-type consumers.
- **When** Accept finishes successfully.
- **Then** the original envelope and two independent pending targets are retained; neither consumer completion nor broker acknowledgement is implied.

#### INBOX-001-AC-02 — Default and distinct logical identity

- **Given** a nonempty delivery Id, first with absent EventId and then a distinct nonempty EventId.
- **When** each envelope is admitted in a clean store.
- **Then** the first logical identity equals delivery Id; the second preserves distinct delivery and event identities.

#### INBOX-001-AC-03 — Reject invalid input before durable admission

- **Given** a null envelope, null payload, empty Id or explicit empty EventId.
- **When** Accept is requested.
- **Then** the request fails and creates no admission or target entries.

#### INBOX-001-AC-04 — Use concrete contract and explicit metadata

- **Given** a payload held through a base/object generic type, registered concrete contract/consumers, and supplied nullable metadata.
- **When** Accept is requested while unrelated ambient delivery metadata exists.
- **Then** the concrete contract and payload are stored; supplied correlation/causation, including null, are preserved without ambient fallback.

#### INBOX-001-AC-05 — Reject missing contracts or consumers

- **Given** a valid envelope whose runtime contract is missing or whose exact runtime type has no consumers.
- **When** Accept is requested.
- **Then** contract failure or permanent consumer.message_unhandled failure propagates; no envelope is admitted for future replay.

### INBOX-002 — Independent atomic admission and duplicate retention

**Classification:** Observed behaviour

**Evidence:** Source: `InboxStore.Accept`, identity query and EF mappings; `IInboxStore.Accept` extends the durable port. [Persistence](../reference/persistence.md) is descriptive. No API XML grants payload-equivalent idempotency.

Precondition: correctly mapped relational store and no ambient Transaction.Current. Postcondition: admission and initial targets save together in an independent context. Duplicate acceptance is identity-only, not a content comparison or registration expansion. Failures: ordinary database errors propagate unless an admission with either identity is confirmed after DbUpdateException. Concurrency: unique constraints fence concurrent insertions; cancellation may race a durable save, so return failure alone does not prove absence.

#### INBOX-002-AC-01 — Do not join caller changes

- **Given** unsaved application changes or an explicit transaction in the caller context, without an ambient transaction.
- **When** Accept succeeds and caller changes are later abandoned.
- **Then** admission remains independently durable and caller changes are not saved by admission.

#### INBOX-002-AC-02 — Reject ambient admission

- **Given** Transaction.Current is present.
- **When** Accept or store Replay is requested.
- **Then** the independent-admission restriction throws without joining the ambient transaction.

#### INBOX-002-AC-03 — Suppress duplicates by either retained identity

- **Given** a retained admission, including terminal targets, and a new envelope reusing MessageId or EventId but different payload/metadata or new registrations.
- **When** Accept finishes.
- **Then** the existing original envelope and target set remain unchanged; no new-consumer expansion or conflict report is supplied.

#### INBOX-002-AC-04 — Resolve concurrent duplicate admission

- **Given** two independent callers submit the same new delivery/event identity.
- **When** their saves race.
- **Then** at most one admission and one row per original target persist; the losing duplicate can succeed when retained identity is confirmed; unrelated save errors remain failures.

#### INBOX-002-AC-05 — Recover after admission and lost acknowledgement

- **Given** admission saved but the process stopped or caller was cancelled before upstream acknowledgement.
- **When** the same identity is submitted again after restart.
- **Then** retained admission blocks additional initial entries; pending targets remain recoverable regardless of a lost wake hint.

### INBOX-003 — Empty admission boundary

**Classification:** Observed behaviour

**Evidence:** Source: `InboxBatch.IsEmpty`, `InboxStore.CreateAdmission` requires a first entry despite a complete batch envelope; the default consumer registry rejects no-consumer types earlier. See [GAP-DUR-001](GAPS.md#gap-dur-001).

Contracts: public IInboxStore.Accept permits representing an empty InboxBatch; EF implementation does not accept a new empty batch. A retained duplicate is checked before creation. This is a source-derived boundary, not an approved desired contract.

#### INBOX-003-AC-01 — Reject a new empty batch

- **Given** an unretained InboxBatch with complete original envelope and Entries empty.
- **When** the EF store accepts it directly.
- **Then** InvalidOperationException occurs and no admission is retained.

#### INBOX-003-AC-02 — Recognize retained empty duplicate

- **Given** an already retained MessageId or EventId and an otherwise empty batch.
- **When** the EF store accepts the duplicate directly.
- **Then** it returns without adding targets; empty-batch validation is not applied to retained identities.

### INBOX-004 — Durable entry state and fenced claims

**Classification:** Observed behaviour

**Evidence:** Source: `InboxEntry` and inherited `DurableStore.Claim/Renew/Complete/Retry/DeadLetter`, `DurableEntryConfiguration`. Shared paths are traced below. [Processing](../reference/processing.md) is descriptive.

Contracts: IInboxStore inherited durable operations and InboxEntry identity, payload, metadata, lifecycle and diagnostic properties; Key is (MessageId, TargetId). Preconditions: Claim limit and lease positive; mappings installed. Postconditions: claims increment Attempts and clear NextAttemptAt; settlement returns a bool for ownership. Invariants: terminal rows are not eligible; target rows complete independently. ClaimedUntil equals now is expired, never valid ownership. Store ordering is creation/message/target order, not a guarantee of handler completion order.

#### INBOX-004-AC-01 — Claim only available due work

- **Given** pending entries include future-due, due-at-now, actively claimed, expired-at-now and terminal rows.
- **When** Claim(now, positive limit, positive lease) completes.
- **Then** only unresolved due work with absent/expired leases is returned, at most the limit, with a claim token, incremented Attempts and a new expiry.

#### INBOX-004-AC-02 — Fence stale and concurrent owners

- **Given** an entry has been reclaimed with a new token or its lease equals/exceeds expiry at settlement time.
- **When** the stale owner attempts Renew, Complete, Retry or DeadLetter.
- **Then** false is returned and no state is overwritten; only a matching unexpired owner can settle.

#### INBOX-004-AC-03 — Keep independent target progress

- **Given** one logical event has several target entries.
- **When** one owned target completes and another schedules retry.
- **Then** only the first has ProcessedAt; the retry target retains identities/payload and diagnostics with its next due time.

#### INBOX-004-AC-04 — Validate claim limits and renew successful ownership

- **Given** invalid nonpositive limit/lease, or a matching unresolved unexpired owned row.
- **When** Claim is requested for invalid inputs or Renew extends the owned lease.
- **Then** invalid Claim fails; valid Renew updates the expiry and returns true without changing identities.

### INBOX-005 — Atomic local delivery and repeatable execution

**Classification:** Required behaviour

**Evidence:** Normative: [coding standards](../CODING_STANDARDS.md), durable-message consistency and scoped transaction rules, and repository instructions require identity/per-target completion and transaction preservation. Implementation evidence: `InboxDispatcher.Dispatch`, `ConsumerDispatcher.Dispatch`, fenced `DurableStore.Complete`. Reference/explanation pages describe that implementation and add no independent requirement.

Purpose: couple configured-context consumer effects, target completion and newly staged outbox rows. Preconditions: owned entry, resolvable stored consumer/contract, relational scoped configured context. Postcondition: committed success or no committed changes from this local attempt. Invariant: failure/ownership loss cannot authorize a successful local transaction. Boundaries: other contexts and external effects are excluded; execution-strategy retries can reinvoke handlers. This requirement does not promise exactly-once external effects.

#### INBOX-005-AC-01 — Commit scoped effects with completion

- **Given** an owned local target whose handler changes the configured scoped context and stages an outbox message.
- **When** local dispatch commits successfully.
- **Then** handler changes, this target completion and staged outbox rows are durable together.

#### INBOX-005-AC-02 — Roll back failure and ownership loss

- **Given** a handler has made configured-context changes, but then throws or fenced completion returns false.
- **When** local dispatch exits.
- **Then** the attempt does not commit those changes or outbox rows; an original failure remains a failure and false denotes lost ownership.

#### INBOX-005-AC-03 — Recover a committed target without repeating local effects

- **Given** the local transaction committed but the process stopped before the worker observed success.
- **When** a later dispatch checks the retained completed target.
- **Then** it reports completed without repeating its local application effects.

#### INBOX-005-AC-04 — Preserve uncertainty without claiming exactly once

- **Given** a commit fails ambiguously or an execution strategy reinvokes a local attempt.
- **When** recovery or strategy retry runs.
- **Then** identity and per-target completion remain the boundary; rollback failure must not replace the original failure, and external side effects are not claimed atomic.

### INBOX-006 — Processor metadata, failure settlement and cancellation

**Classification:** Observed behaviour

**Evidence:** Source: `InboxProcessor.Process`, shared `ClaimRenewal`, `RetryPolicy`, `MessageFailureFactory`, `MessageContext`; [processing](../reference/processing.md) describes outcomes.

Contracts: InboxProcessor.Process and IInboxDispatcher.Dispatch bool; handling context contains delivery ID, correlation, causation and target consumer ID and is restored afterward. Failures: permanent MessageException faults immediately; transient/unknown exceptions retry within policy; max attempts is evaluated after a failed claim and is not a hard ceiling on reclaimed crash attempts. Claim-loss and shutdown cancellation are distinct from ordinary failure. Settlement has a timeout linked to claim loss, not the host token.

#### INBOX-006-AC-01 — Expose and restore delivery metadata

- **Given** an owned entry and preexisting ambient context.
- **When** the processor dispatches and finishes or throws.
- **Then** the handler sees persisted delivery metadata and target ID; the previous context is restored.

#### INBOX-006-AC-02 — Classify retry and permanent failure

- **Given** a target throws transient/unknown failure below the retry threshold or a permanent failure / failure at the threshold.
- **When** the processor settles while it still owns the claim.
- **Then** the former is due later with failure diagnostics; the latter is dead-lettered; successful settlement clears claim ownership.

#### INBOX-006-AC-03 — Keep shutdown separate from message failure

- **Given** the host token is cancelled during handling.
- **When** the handler observes cancellation and the processor exits.
- **Then** shutdown cancellation propagates rather than generating an ordinary retry/dead-letter; unsettled work can recover after expiry.

#### INBOX-006-AC-04 — Stop on claim loss or renewal error

- **Given** renewal returns false or fails while handling.
- **When** claim-loss cancellation reaches the handler or dispatch returns false.
- **Then** no ordinary failure settlement is authorized by the lost owner; recovery can reclaim unresolved work.

#### INBOX-006-AC-05 — Bound failed settlement independently

- **Given** ordinary handling failed, the host later stops, and the owner still has a lease.
- **When** retry/dead-letter settlement runs.
- **Then** settlement may finish using its separate timeout; timeout/ownership loss can leave unresolved leased work, not manufactured completion.

### INBOX-007 — Signals, workers and restart recovery

**Classification:** Observed behaviour

**Evidence:** Source: `InboxSignal.WaitAsync/Wake`, `InboxWorker`, `InboxRecoveryWorker`, shared WorkLoop and AsyncAutoResetEvent. The signal is a coalesced in-memory hint, not persisted work.

Contracts: public signal and BackgroundService lifecycle inherited by workers. Options bound concurrency, capacity, batch, lease, settlement, recovery and cleanup. Preconditions: host running with resolvable services; no exact latency/order/fairness guarantee. Postconditions: startup/recovery wakes enable available work to be claimed, including work whose admission hint was lost. Claim leases begin before queueing; renewal begins during processing.

#### INBOX-007-AC-01 — Coalesce wake hints and cancel a wait

- **Given** no signal is pending, then several Wake calls occur.
- **When** a WaitAsync consumes a hint and a later waiting token is cancelled.
- **Then** one wake can satisfy the wait without persisting a count of calls; cancelled waiting propagates cancellation.

#### INBOX-007-AC-02 — Recover after crash without a hint

- **Given** durable unresolved work remains after restart or queue loss and any old lease expires.
- **When** startup or periodic recovery wakes a running healthy worker.
- **Then** eligible work is eventually attempted subject to host/database availability; crash claims may increase Attempts without a completed handler.

#### INBOX-007-AC-03 — Recover after claim or processing infrastructure failures

- **Given** a claim fails or processing cannot settle and a database/service later becomes available.
- **When** the worker logs failure and continues with configured recovery.
- **Then** durable work remains recoverable, with bounded in-process execution rather than silently deleted or marked completed.

### INBOX-008 — Dead-letter inspection and independent management

**Classification:** Observed behaviour

**Evidence:** Source: `IInboxManager`, `InboxManager`, shared `DeadLetterStore.Get/Requeue/Discard`. [Inbox](../reference/inbox.md) is descriptive.

Contracts: GetDeadLetters(default limit 100), Requeue, Discard and RequeueResult (shared). Positive listing limit, nonnull key collections; empty collections valid. List includes dead-lettered but not discarded entries ordered by dead-letter timestamp; it is not snapshot pagination. Operations process keys sequentially in independent transactions and preserve original event/message identity, payload and failure diagnostics. Requeue resets Attempts and terminal/claim/due state; Discard retains dead-letter timestamp and diagnostics. No batch atomicity or inverse discard operation.

#### INBOX-008-AC-01 — Inspect active dead letters with limit validation

- **Given** dead-lettered, discarded and nonterminal entries exist.
- **When** GetDeadLetters uses positive limit or a nonpositive limit.
- **Then** positive listing exposes at most that many nondiscarded dead letters including key/event/attempt/failure summary; nonpositive limit fails.

#### INBOX-008-AC-02 — Requeue only eligible keys and wake

- **Given** eligible, absent, completed and repeated keys are supplied.
- **When** Requeue finishes.
- **Then** only eligible dead letters become unresolved, Attempts zero and claim/due/terminal state cleared, diagnostics retained; Requested counts all supplied keys and Requeued counts transitions.

#### INBOX-008-AC-03 — Discard and empty collections

- **Given** an eligible dead letter, already discarded/absent keys, or an empty collection.
- **When** Discard or Requeue runs.
- **Then** eligible discard records DiscardedAt without losing diagnosis; ineligible keys do not transition and empty collections return zero work.

#### INBOX-008-AC-04 — Expose partial success and reject ambient management

- **Given** several keys, with cancellation/failure after an earlier transition, or Transaction.Current present.
- **When** a manager mutation runs.
- **Then** earlier independently committed transitions remain durable on later failure; ambient mutations are rejected rather than joining a batch transaction.

#### INBOX-008-AC-05 — Reject null management collections

- **Given** Requeue or Discard receives a null key collection.
- **When** the manager operation is requested.
- **Then** ArgumentNullException is observable and no key transition occurs.

### INBOX-009 — Replay retained admissions into missing targets

**Classification:** Observed behaviour

**Evidence:** Source: `IInboxManager.Replay<TMessage>`, `InboxManager.Replay`, `InboxStore.Replay`; `ReplayResult.Matched/Replayed`. [GAP-DUR-002](GAPS.md#gap-dur-002) covers target validation/compatibility, [GAP-DUR-003](GAPS.md#gap-dur-003) covers count/error ambiguity.

Purpose: explicitly introduce a target for retained history, not resubmit an envelope or reset an existing target. Preconditions at manager: consumerId nonblank, limit positive (default 1000), from <= to when both supplied, registered TMessage contract. No consumer registration/type compatibility check or ASCII/length validation occurs here. Inclusive CreatedAt filtering; sort CreatedAt then MessageId; limit applied before checking existing event/target pairs of every state. Result Matched means selected admissions, not total history; Replayed means inserted/reconciled pair count. Empty selection returns zero/zero; wake occurs when Replayed > 0.

#### INBOX-009-AC-01 — Preserve original envelope in replay

- **Given** retained admissions of the registered contract lie inside inclusive from/to boundaries and the target is absent.
- **When** Replay selects them.
- **Then** new unresolved targets preserve original MessageId, EventId, contract, payload, metadata and CreatedAt, and counts reflect selected/added entries.

#### INBOX-009-AC-02 — Do not reset existing targets of any state

- **Given** the selected history has existing pending, processed, dead-lettered or discarded event/target pairs.
- **When** Replay is requested for the same target.
- **Then** those entries retain state and are excluded from additions; matched entries still count toward the selection limit.

#### INBOX-009-AC-03 — Handle invalid and empty replay selection

- **Given** blank target, nonpositive limit, inverted dates or missing contract, versus a valid query with no matching admissions.
- **When** Replay is requested.
- **Then** invalid manager arguments/contracts fail; an empty valid selection reports Matched=0 and Replayed=0.

#### INBOX-009-AC-04 — Bound before exclusion without cursor advancement

- **Given** the oldest limited history window already has every requested target, and later history lacks it.
- **When** the same limited Replay is repeated.
- **Then** Matched remains the selected window size, Replayed stays zero, and later admissions are not automatically visited.

#### INBOX-009-AC-05 — Allow incompatible target names at management boundary

- **Given** a nonblank but unregistered, incompatible, non-ASCII or overlong target ID.
- **When** Replay is requested.
- **Then** manager-level compatibility/syntax rejection is not supplied; provider storage or later consumer resolution can fail, rather than proving a viable replay target.

#### INBOX-009-AC-06 — Reconcile concurrent replay inserts

- **Given** independent Replay requests select the same missing logical event/target pairs.
- **When** their insertions race.
- **Then** unique keys prevent duplicate rows; a DbUpdateException path reports the increase in persisted pairs, which can include another caller's additions.

#### INBOX-009-AC-07 — Report current swallowed update failure

- **Given** a replay SaveChanges throws DbUpdateException unrelated to a duplicate and reconciliation finds no new pairs.
- **When** Replay returns from its current EF catch path.
- **Then** it reports selected Matched and Replayed=0 instead of rethrowing that update error; this is a suspected mismatch, not approved behaviour to require.

### INBOX-010 — Entry cleanup and indefinite admission retention

**Classification:** Observed behaviour

**Evidence:** Source: `InboxCleanupWorker`, inherited `DurableStore.Cleanup`; no owned admission cleanup operation exists. [GAP-DUR-004](GAPS.md#gap-dur-004) records the retention decision.

Contracts: IInboxStore.Cleanup inherited; cleanup worker starts immediately then periodically with UTC cutoff now minus retention and one batch per tick. Positive batch limit required. Only entry terminal timestamps strictly older than cutoff qualify; pending rows survive. Ordering is CreatedAt. Admissions and original payloads are retained indefinitely by this capability. Cleanup removes per-target completion/dedup evidence, enabling later replay of the deleted pair.

#### INBOX-010-AC-01 — Respect terminal and strict cutoff retention

- **Given** pending and terminal rows exist with terminal timestamps before, equal to and after cutoff.
- **When** Cleanup(before, positive limit) runs.
- **Then** only terminal rows strictly older are deleted, up to the limit; pending and equality-boundary rows remain.

#### INBOX-010-AC-02 — Retain admission while replay evidence expires

- **Given** an old completed target has been cleaned but its original admission remains.
- **When** duplicate Accept and explicit Replay for that target run.
- **Then** Accept stays duplicate; Replay can create that target again, so cleaned completion no longer prevents repeated local effects.

#### INBOX-010-AC-03 — Retry periodic cleanup without successful deletion claim

- **Given** cleanup throws an ordinary exception, or host cancellation occurs.
- **When** a running correctly resolved cleanup worker handles it.
- **Then** ordinary failure is warning-logged and a later tick can retry; host cancellation stops without implying deleted rows.

### INBOX-011 — Composition, mappings and public options

**Classification:** Observed behaviour

**Evidence:** Source: Inbox EF `DependencyInjection`, `InboxConfiguration`, `InboxValidation`, project references; `InboxOptions.Section`. [Configuration](../reference/configuration.md) and [processing defaults](../reference/processing.md) are descriptive.

Contracts: services AddInbox<TDbContext>(Action<InboxOptions>?), IConfiguration overload, MessagingBuilder overload, ModelBuilder.AddInbox; InboxOptions inherits Lease/Processing/Recovery/Cleanup with Section="Inbox". Preconditions: relational mapped schema, same scoped context type across inbox/outbox, contracts/consumers installed by host. Registrations preserve prior TryAdd services. Model keys: admission MessageId and unique EventId; entry composite MessageId/TargetId and unique EventId/TargetId, target max 200 ASCII mapping. Validation checks relational provider, both model entities and processor resolvability, not database schema/migration/provider correctness. Hosted cleanup lifetime caveat is separately specified.

#### INBOX-011-AC-01 — Bind configure and configuration overloads

- **Given** a host calls the service or builder overload and uses callback settings or the Inbox configuration section.
- **When** options resolve and startup validation executes.
- **Then** requested shared durable settings are available; missing/nonrelational mappings or mismatched configured persistence context types are rejected.

#### INBOX-011-AC-02 — Require positive valid shared settings

- **Given** configured nonpositive durations/concurrency/capacity/batch/settlement/retention, renewal >= lease duration or failure max delay < recovery interval.
- **When** options startup validation runs.
- **Then** invalid settings fail rather than authorize an unsafe processing schedule.

#### INBOX-011-AC-03 — Do not infer installed schema from model validation

- **Given** a relational context maps admissions and entries but database tables are absent or incompatible.
- **When** startup model validation succeeds and an admission/store operation follows.
- **Then** startup success alone does not certify schema; actual persistence can fail.

### INBOX-012 — Scoped cleanup composition requirement and current mismatch

**Classification:** Required behaviour

**Evidence:** Normative: [coding standards](../CODING_STANDARDS.md) require scoped dependencies not be captured by singleton services. Descriptive counter-evidence: AddInbox registers singleton IHostedService InboxCleanupWorker whose constructor takes scoped IInboxStore. [GAP-DUR-005](GAPS.md#gap-dur-005); this specification is not a statement that current source conforms.

Purpose/invariant: safe lifetime composition under scope validation. Applies to cleanup and any singleton extension dependencies. Current source can fail host scope validation or retain a root-resolved scoped store when validation is disabled; source review is not a successful-host claim.

#### INBOX-012-AC-01 — Resolve cleanup without captive scoped persistence

- **Given** default inbox registrations with service-provider scope validation enabled.
- **When** the host resolves hosted cleanup services.
- **Then** required outcome: cleanup composition resolves without consuming a scoped store from the root; current constructor/registration appears to violate this outcome.

### INBOX-013 — Replay failure transparency requirement

**Classification:** Required behaviour

**Evidence:** Normative: coding standards prohibit hiding failure behind successful/default outcomes. Descriptive counter-evidence: InboxStore.Replay catches any DbUpdateException and returns a count after reconciliation. See [GAP-DUR-003](GAPS.md#gap-dur-003).

Precondition: an actual persistence error not positively reconciled as successful replay. Required postcondition: observable failure remains distinguishable from successful zero additions. This is a suspected implementation mismatch, not authorization to fix code.

#### INBOX-013-AC-01 — Do not turn unreconciled replay failure into success

- **Given** a replay insert fails and no evidence confirms a successful operation.
- **When** the operation completes its failure path.
- **Then** required outcome: the caller can distinguish storage failure from a successful no-op; the observed zero-count path does not currently demonstrate this.

### INBOX-014 — Undecided admission and replay policies

**Classification:** Unknown behaviour

**Evidence:** No normative policy located for conflicting identity payloads, empty admission, replay target suitability, pagination, count attribution or admission expiry. Evidence: InboxStore/InboxManager and [GAP-DUR-001](GAPS.md#gap-dur-001), [GAP-DUR-002](GAPS.md#gap-dur-002), [GAP-DUR-003](GAPS.md#gap-dur-003), [GAP-DUR-004](GAPS.md#gap-dur-004).

These are numbered undecided questions, not acceptance requirements. Purpose: make consumer-visible decision gaps explicit. Proposed resolution must define invariants and failure/recovery boundaries before implementation or tests.

**Undecided questions (not acceptance criteria):**

1. **empty or conflicting admission policy:** What approved policy should apply when a new no-target delivery or reused identity with changed payload? Decision to resolve: choose rejection, retain-without-targets, or explicit conflict handling and define durable/cancellation outcomes; current observations alone do not choose.

2. **replay suitability and progress contract:** What approved policy should apply when history exceeds a page or a requested target is incompatible? Decision to resolve: choose target validation and continuation/count semantics, distinguishing this request's additions from concurrent additions.

3. **admission retention and replay horizon:** What approved policy should apply when entry completion has expired but admissions retain payload indefinitely? Decision to resolve: define payload deletion and dedup/replay horizons, including any permitted repeated effects; no existing admission cleanup guarantee is inferred.

### INBOX-015 — Unapproved replay failure improvement

**Classification:** Proposed behaviour

**Evidence:** Recommendation only, not approved: [GAP-DUR-003](GAPS.md#gap-dur-003). Motivated by normative failure transparency and descriptive replay reconciliation evidence; no implementation change was made.

Purpose: candidate reconciliation semantics, subject to approval. Preconditions: failed replay save; postcondition proposal: duplicate conflicts may reconcile but unrelated unreconciled storage errors stay visible. Does not decide count attribution or pagination.

#### INBOX-015-AC-01 — Proposal reconcile only confirmed duplicate outcomes

- **Given** a replay save fails with either a confirmed concurrent uniqueness conflict or an unrelated update failure.
- **When** an approved implementation of the proposal is evaluated.
- **Then** unapproved proposal: return reconciled results only for confirmed successful/duplicate outcomes and preserve the other error; this criterion is not yet binding.

## Verification matrix

Every row is a recommended future verification, not evidence that a test exists or passed. Contract-decision rows require a decision before a binding assertion can be implemented. Required mismatch rows must not be marked conforming from this audit.

| Acceptance criterion | Recommended test level | Observable assertion / decision | Status |
| --- | --- | --- | --- |
| INBOX-001-AC-01 | Relational integration | the original envelope and two independent pending targets are retained; neither consumer completion nor broker acknowledgement is implied | Not tested |
| INBOX-001-AC-02 | Relational integration | the first logical identity equals delivery Id; the second preserves distinct delivery and event identities | Not tested |
| INBOX-001-AC-03 | Unit + relational integration | the request fails and creates no admission or target entries | Not tested |
| INBOX-001-AC-04 | Unit + relational integration | the concrete contract and payload are stored; supplied correlation/causation, including null, are preserved without ambient fallback | Not tested |
| INBOX-001-AC-05 | Unit + relational integration | contract failure or permanent consumer.message_unhandled failure propagates; no envelope is admitted for future replay | Not tested |
| INBOX-002-AC-01 | Relational integration | admission remains independently durable and caller changes are not saved by admission | Not tested |
| INBOX-002-AC-02 | Relational integration | the independent-admission restriction throws without joining the ambient transaction | Not tested |
| INBOX-002-AC-03 | Relational integration | the existing original envelope and target set remain unchanged; no new-consumer expansion or conflict report is supplied | Not tested |
| INBOX-002-AC-04 | Relational integration | at most one admission and one row per original target persist; the losing duplicate can succeed when retained identity is confirmed; unrelated save errors remain failures | Not tested |
| INBOX-002-AC-05 | Relational integration | retained admission blocks additional initial entries; pending targets remain recoverable regardless of a lost wake hint | Not tested |
| INBOX-003-AC-01 | Relational integration | InvalidOperationException occurs and no admission is retained | Not tested |
| INBOX-003-AC-02 | Relational integration | it returns without adding targets; empty-batch validation is not applied to retained identities | Not tested |
| INBOX-004-AC-01 | Relational integration | only unresolved due work with absent/expired leases is returned, at most the limit, with a claim token, incremented Attempts and a new expiry | Not tested |
| INBOX-004-AC-02 | Relational integration | false is returned and no state is overwritten; only a matching unexpired owner can settle | Not tested |
| INBOX-004-AC-03 | Relational integration | only the first has ProcessedAt; the retry target retains identities/payload and diagnostics with its next due time | Not tested |
| INBOX-004-AC-04 | Unit + relational integration | invalid Claim fails; valid Renew updates the expiry and returns true without changing identities | Not tested |
| INBOX-005-AC-01 | Relational integration | handler changes, this target completion and staged outbox rows are durable together | Not tested |
| INBOX-005-AC-02 | Relational integration | the attempt does not commit those changes or outbox rows; an original failure remains a failure and false denotes lost ownership | Not tested |
| INBOX-005-AC-03 | Relational integration | it reports completed without repeating its local application effects | Not tested |
| INBOX-005-AC-04 | Fault-injection integration | identity and per-target completion remain the boundary; rollback failure must not replace the original failure, and external side effects are not claimed atomic | Not tested |
| INBOX-006-AC-01 | Unit | the handler sees persisted delivery metadata and target ID; the previous context is restored | Not tested |
| INBOX-006-AC-02 | Relational integration | the former is due later with failure diagnostics; the latter is dead-lettered; successful settlement clears claim ownership | Not tested |
| INBOX-006-AC-03 | Unit + integration | shutdown cancellation propagates rather than generating an ordinary retry/dead-letter; unsettled work can recover after expiry | Not tested |
| INBOX-006-AC-04 | Unit + integration | no ordinary failure settlement is authorized by the lost owner; recovery can reclaim unresolved work | Not tested |
| INBOX-006-AC-05 | Fault-injection integration | settlement may finish using its separate timeout; timeout/ownership loss can leave unresolved leased work, not manufactured completion | Not tested |
| INBOX-007-AC-01 | Unit | one wake can satisfy the wait without persisting a count of calls; cancelled waiting propagates cancellation | Not tested |
| INBOX-007-AC-02 | Relational integration | eligible work is eventually attempted subject to host/database availability; crash claims may increase Attempts without a completed handler | Not tested |
| INBOX-007-AC-03 | Fault-injection integration | durable work remains recoverable, with bounded in-process execution rather than silently deleted or marked completed | Not tested |
| INBOX-008-AC-01 | Relational integration | positive listing exposes at most that many nondiscarded dead letters including key/event/attempt/failure summary; nonpositive limit fails | Not tested |
| INBOX-008-AC-02 | Relational integration | only eligible dead letters become unresolved, Attempts zero and claim/due/terminal state cleared, diagnostics retained; Requested counts all supplied keys and Requeued counts transitions | Not tested |
| INBOX-008-AC-03 | Relational integration | eligible discard records DiscardedAt without losing diagnosis; ineligible keys do not transition and empty collections return zero work | Not tested |
| INBOX-008-AC-04 | Relational integration | earlier independently committed transitions remain durable on later failure; ambient mutations are rejected rather than joining a batch transaction | Not tested |
| INBOX-008-AC-05 | Unit | ArgumentNullException is observable and no key transition occurs | Not tested |
| INBOX-009-AC-01 | Relational integration | new unresolved targets preserve original MessageId, EventId, contract, payload, metadata and CreatedAt, and counts reflect selected/added entries | Not tested |
| INBOX-009-AC-02 | Relational integration | those entries retain state and are excluded from additions; matched entries still count toward the selection limit | Not tested |
| INBOX-009-AC-03 | Unit + integration | invalid manager arguments/contracts fail; an empty valid selection reports Matched=0 and Replayed=0 | Not tested |
| INBOX-009-AC-04 | Relational integration | Matched remains the selected window size, Replayed stays zero, and later admissions are not automatically visited | Not tested |
| INBOX-009-AC-05 | Relational integration | manager-level compatibility/syntax rejection is not supplied; provider storage or later consumer resolution can fail, rather than proving a viable replay target | Not tested |
| INBOX-009-AC-06 | Relational integration | unique keys prevent duplicate rows; a DbUpdateException path reports the increase in persisted pairs, which can include another caller's additions | Not tested |
| INBOX-009-AC-07 | Fault-injection integration | it reports selected Matched and Replayed=0 instead of rethrowing that update error; this is a suspected mismatch, not approved behaviour to require | Not tested |
| INBOX-010-AC-01 | Relational integration | only terminal rows strictly older are deleted, up to the limit; pending and equality-boundary rows remain | Not tested |
| INBOX-010-AC-02 | Relational integration | Accept stays duplicate; Replay can create that target again, so cleaned completion no longer prevents repeated local effects | Not tested |
| INBOX-010-AC-03 | Fault-injection integration | ordinary failure is warning-logged and a later tick can retry; host cancellation stops without implying deleted rows | Not tested |
| INBOX-011-AC-01 | Composition integration | requested shared durable settings are available; missing/nonrelational mappings or mismatched configured persistence context types are rejected | Not tested |
| INBOX-011-AC-02 | Unit | invalid settings fail rather than authorize an unsafe processing schedule | Not tested |
| INBOX-011-AC-03 | Relational integration | startup success alone does not certify schema; actual persistence can fail | Not tested |
| INBOX-012-AC-01 | Composition integration | required outcome: cleanup composition resolves without consuming a scoped store from the root; current constructor/registration appears to violate this outcome | Not tested |
| INBOX-013-AC-01 | Fault-injection integration | required outcome: the caller can distinguish storage failure from a successful no-op; the observed zero-count path does not currently demonstrate this | Not tested |
| INBOX-015-AC-01 | Fault-injection integration | unapproved proposal: return reconciled results only for confirmed successful/duplicate outcomes and preserve the other error; this criterion is not yet binding | Not tested |

## Owned source and public contract traceability

Every authored .cs/.csproj/.md file in the two owned projects is included, excluding bin/obj. A dash means no publicly accessible type/member in that file; implementation operations remain recorded in responsibility. Exact signatures/positional members are retained in the session audit JSON. Constructor/service composition is covered with its owning type.

| Source path | Public contracts / operations | Responsibility | Specifications |
| --- | --- | --- | --- |
| [src\Clean.Messaging.Inbox\Admission\InboxAdmission.cs](../../src/Clean.Messaging.Inbox/Admission/InboxAdmission.cs) | `Clean.Messaging.Inbox.Admission.public sealed class InboxAdmission`<br>`Clean.Messaging.Inbox.Admission.InboxAdmission: public Guid MessageId`<br>`Clean.Messaging.Inbox.Admission.InboxAdmission: public Guid EventId`<br>`Clean.Messaging.Inbox.Admission.InboxAdmission: public required MessageData Message`<br>`Clean.Messaging.Inbox.Admission.InboxAdmission: public Guid? CorrelationId`<br>`Clean.Messaging.Inbox.Admission.InboxAdmission: public Guid? CausationId`<br>`Clean.Messaging.Inbox.Admission.InboxAdmission: public DateTime CreatedAt` | Retained original delivery/event identity, serialized envelope, nullable context metadata and creation timestamp. | INBOX-001, INBOX-002, INBOX-009, INBOX-010 |
| [src\Clean.Messaging.Inbox\Clean.Messaging.Inbox.csproj](../../src/Clean.Messaging.Inbox/Clean.Messaging.Inbox.csproj) | — (internal implementation or project metadata) | Targets .NET 10 and references core provider-neutral messaging; no public operations. | INBOX-011 |
| [src\Clean.Messaging.Inbox\Configuration\InboxOptions.cs](../../src/Clean.Messaging.Inbox/Configuration/InboxOptions.cs) | `Clean.Messaging.Inbox.Configuration.public sealed class InboxOptions : MessageOptions`<br>`Clean.Messaging.Inbox.Configuration.InboxOptions: public const string Section = "Inbox";` | Inbox configuration section and inherited durable processing, lease, recovery and cleanup settings. | INBOX-007, INBOX-010, INBOX-011 |
| [src\Clean.Messaging.Inbox\Dispatching\IInboxDispatcher.cs](../../src/Clean.Messaging.Inbox/Dispatching/IInboxDispatcher.cs) | `Clean.Messaging.Inbox.Dispatching.public interface IInboxDispatcher`<br>`Clean.Messaging.Inbox.Dispatching.IInboxDispatcher: ValueTask<bool> Dispatch( OwnedEntry<InboxEntry> owned, CancellationToken cancellationToken)` | Owned local dispatch port returning completion versus ownership loss. | INBOX-005, INBOX-006 |
| [src\Clean.Messaging.Inbox\Dispatching\InboxProcessor.cs](../../src/Clean.Messaging.Inbox/Dispatching/InboxProcessor.cs) | `Clean.Messaging.Inbox.Dispatching.public sealed partial class InboxProcessor( IInboxStore store, IInboxDispatcher dispatcher, ClaimRenewal renewal, MessageContext context, RetryPolicy retry, MessageFailureFactory failures, TimeProvider time, IOptions<InboxOptions> options, ILogger<InboxProcessor> logger) : IWorkProcessor<InboxEntry>`<br>`Clean.Messaging.Inbox.Dispatching.InboxProcessor: public async ValueTask Process( OwnedEntry<InboxEntry> owned, CancellationToken cancellationToken)` | Owned processing, ambient context, lease renewal, exception/retry/dead-letter settlement and outcome telemetry. | INBOX-004, INBOX-006, INBOX-007 |
| [src\Clean.Messaging.Inbox\IInbox.cs](../../src/Clean.Messaging.Inbox/IInbox.cs) | `Clean.Messaging.Inbox.public sealed record InboxMessage<T>( Guid Id, T Message, Guid? CorrelationId = null, Guid? CausationId = null, Guid? EventId = null)`<br>`Clean.Messaging.Inbox.public sealed record InboxMessage( Guid Id, string Type, object Payload, Guid? CorrelationId = null, Guid? CausationId = null, Guid? EventId = null)`<br>`Clean.Messaging.Inbox.public interface IInbox`<br>`Clean.Messaging.Inbox.IInbox: ValueTask Accept<T>( InboxMessage<T> message, CancellationToken cancellationToken = default)` | Typed admission port and generic/nongeneric envelope representations; constructor fields and default metadata. | INBOX-001, INBOX-002 |
| [src\Clean.Messaging.Inbox\Inbox.cs](../../src/Clean.Messaging.Inbox/Inbox.cs) | `Clean.Messaging.Inbox.public sealed class Inbox( IInboxStore store, IConsumerRegistry consumers, IMessageSerializer serializer, InboxSignal signal, TimeProvider time) : IInbox`<br>`Clean.Messaging.Inbox.Inbox: public async ValueTask Accept<TMessage>( InboxMessage<TMessage> message, CancellationToken cancellationToken = default)` | Validates envelopes, serializes concrete messages, creates consumer target batches and wakes after admission. | INBOX-001, INBOX-002, INBOX-007 |
| [src\Clean.Messaging.Inbox\Persistence\IInboxStore.cs](../../src/Clean.Messaging.Inbox/Persistence/IInboxStore.cs) | `Clean.Messaging.Inbox.Persistence.public sealed record InboxBatch( Guid MessageId, Guid EventId, MessageData Message, Guid? CorrelationId, Guid? CausationId, DateTime CreatedAt, IReadOnlyList<InboxEntry> Entries)`<br>`Clean.Messaging.Inbox.Persistence.InboxBatch: public bool IsEmpty =>`<br>`Clean.Messaging.Inbox.Persistence.public interface IInboxStore : IDurableStore<InboxEntry>`<br>`Clean.Messaging.Inbox.Persistence.IInboxStore: ValueTask Accept( InboxBatch batch, CancellationToken cancellationToken)`<br>`Clean.Messaging.Inbox.Persistence.IInboxStore: ValueTask<ReplayResult> Replay( string consumerId, string contract, DateTime? from, DateTime? to, int limit, CancellationToken cancellationToken)` | Admission/replay port, batch envelope/entries/IsEmpty and inherited claim/renew/complete/retry/dead-letter/cleanup operations. | INBOX-001, INBOX-002, INBOX-003, INBOX-004, INBOX-009, INBOX-010 |
| [src\Clean.Messaging.Inbox\Persistence\InboxEntry.cs](../../src/Clean.Messaging.Inbox/Persistence/InboxEntry.cs) | `Clean.Messaging.Inbox.Persistence.public sealed class InboxEntry : IDurableEntry`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public Guid MessageId`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public Guid EventId`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public required string TargetId`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public required MessageData Message`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public Guid? CorrelationId`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public Guid? CausationId`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public DateTime CreatedAt`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public int Attempts`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public string? ClaimId`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public DateTime? ClaimedUntil`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public DateTime? NextAttemptAt`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public DateTime? ProcessedAt`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public DateTime? DeadLetteredAt`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public DateTime? DiscardedAt`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public FailureCategory? FailureCategory`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public string? FailureCode`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public string? ExceptionType`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public string? FailureMessage`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public DateTime? LastFailedAt`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public int? LastFailedAttempt`<br>`Clean.Messaging.Inbox.Persistence.InboxEntry: public EntryKey Key => new(MessageId, TargetId);` | Per-target durable identity, payload/context, key, attempts/lease/due/terminal state and diagnostics. | INBOX-001, INBOX-004, INBOX-006, INBOX-008, INBOX-010 |
| [src\Clean.Messaging.Inbox\Runtime\IInboxManager.cs](../../src/Clean.Messaging.Inbox/Runtime/IInboxManager.cs) | `Clean.Messaging.Inbox.Runtime.public interface IInboxManager`<br>`Clean.Messaging.Inbox.Runtime.IInboxManager: ValueTask<IReadOnlyList<DeadLetter>> GetDeadLetters( int limit = 100, CancellationToken cancellationToken = default)`<br>`Clean.Messaging.Inbox.Runtime.IInboxManager: ValueTask<RequeueResult> Requeue( IReadOnlyCollection<EntryKey> entries, CancellationToken cancellationToken = default)`<br>`Clean.Messaging.Inbox.Runtime.IInboxManager: ValueTask<int> Discard( IReadOnlyCollection<EntryKey> entries, CancellationToken cancellationToken = default)`<br>`Clean.Messaging.Inbox.Runtime.IInboxManager: ValueTask<ReplayResult> Replay<TMessage>( string consumerId, DateTime? from = null, DateTime? to = null, int limit = 1000, CancellationToken cancellationToken = default)` | Public dead-letter listing/requeue/discard and typed retained-admission replay APIs including defaults/cancellation. | INBOX-008, INBOX-009 |
| [src\Clean.Messaging.Inbox\Runtime\InboxCleanupWorker.cs](../../src/Clean.Messaging.Inbox/Runtime/InboxCleanupWorker.cs) | `Clean.Messaging.Inbox.Runtime.public sealed partial class InboxCleanupWorker( IInboxStore store, TimeProvider time, IOptions<InboxOptions> options, ILogger<InboxCleanupWorker> logger) : BackgroundService`<br>`Clean.Messaging.Inbox.Runtime.InboxCleanupWorker: protected override async Task ExecuteAsync(CancellationToken stoppingToken)` | Immediate/periodic bounded terminal cleanup; warning/error and shutdown handling; captive scoped-store constructor. | INBOX-010, INBOX-012 |
| [src\Clean.Messaging.Inbox\Runtime\InboxManager.cs](../../src/Clean.Messaging.Inbox/Runtime/InboxManager.cs) | `Clean.Messaging.Inbox.Runtime.public sealed class InboxManager( IInboxStore inbox, IDeadLetterStore<InboxEntry> deadLetters, IMessageContractRegistry contracts, InboxSignal signal) : IInboxManager`<br>`Clean.Messaging.Inbox.Runtime.InboxManager: public ValueTask<IReadOnlyList<DeadLetter>> GetDeadLetters( int limit = 100, CancellationToken cancellationToken = default)`<br>`Clean.Messaging.Inbox.Runtime.InboxManager: public async ValueTask<RequeueResult> Requeue( IReadOnlyCollection<EntryKey> entries, CancellationToken cancellationToken = default)`<br>`Clean.Messaging.Inbox.Runtime.InboxManager: public async ValueTask<int> Discard( IReadOnlyCollection<EntryKey> entries, CancellationToken cancellationToken = default)`<br>`Clean.Messaging.Inbox.Runtime.InboxManager: public async ValueTask<ReplayResult> Replay<TMessage>( string consumerId, DateTime? from = null, DateTime? to = null, int limit = 1000, CancellationToken cancellationToken = default)` | Argument validation, sequential key operations/results, replay contract selection, counts and wake hints. | INBOX-008, INBOX-009, INBOX-013 |
| [src\Clean.Messaging.Inbox\Runtime\InboxRecoveryWorker.cs](../../src/Clean.Messaging.Inbox/Runtime/InboxRecoveryWorker.cs) | `Clean.Messaging.Inbox.Runtime.public sealed class InboxRecoveryWorker( InboxSignal signal, TimeProvider time, IOptions<InboxOptions> options) : BackgroundService`<br>`Clean.Messaging.Inbox.Runtime.InboxRecoveryWorker: protected override async Task ExecuteAsync(CancellationToken stoppingToken)` | Periodic recovery hint and inherited hosted lifecycle/cancellation. | INBOX-007, INBOX-011 |
| [src\Clean.Messaging.Inbox\Runtime\InboxSignal.cs](../../src/Clean.Messaging.Inbox/Runtime/InboxSignal.cs) | `Clean.Messaging.Inbox.Runtime.public sealed class InboxSignal : IWorkSignal`<br>`Clean.Messaging.Inbox.Runtime.InboxSignal: public ValueTask WaitAsync( CancellationToken cancellationToken)`<br>`Clean.Messaging.Inbox.Runtime.InboxSignal: public void Wake()` | Coalesced in-memory wake/wait signal with cancellation. | INBOX-007 |
| [src\Clean.Messaging.Inbox\Runtime\InboxWorker.cs](../../src/Clean.Messaging.Inbox/Runtime/InboxWorker.cs) | `Clean.Messaging.Inbox.Runtime.public sealed class InboxWorker( WorkLoop<InboxEntry> loop, InboxSignal signal, IOptions<InboxOptions> options) : BackgroundService`<br>`Clean.Messaging.Inbox.Runtime.InboxWorker: protected override Task ExecuteAsync( CancellationToken stoppingToken)` | Hosted WorkLoop adaptation using inbox lease/batch/concurrency/recovery options. | INBOX-007, INBOX-011 |
| [src\Clean.Messaging.Inbox\Runtime\ReplayResult.cs](../../src/Clean.Messaging.Inbox/Runtime/ReplayResult.cs) | `Clean.Messaging.Inbox.Runtime.public readonly record struct ReplayResult( int Matched, int Replayed)` | Matched selected admissions versus Replayed persisted additions result fields. | INBOX-009, INBOX-013, INBOX-014 |
| [src\Clean.Messaging.Inbox.EntityFrameworkCore\Clean.Messaging.Inbox.EntityFrameworkCore.csproj](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/Clean.Messaging.Inbox.EntityFrameworkCore.csproj) | — (internal implementation or project metadata) | Relational EF adapter dependencies and neutral inbox references for .NET 10. | INBOX-011 |
| [src\Clean.Messaging.Inbox.EntityFrameworkCore\DependencyInjection.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/DependencyInjection.cs) | `Clean.Messaging.Inbox.EntityFrameworkCore.public static class DependencyInjection`<br>`Clean.Messaging.Inbox.EntityFrameworkCore.DependencyInjection: public static IServiceCollection AddInbox<TDbContext>( this IServiceCollection services, Action<InboxOptions>? configure = null)`<br>`Clean.Messaging.Inbox.EntityFrameworkCore.DependencyInjection: public static IServiceCollection AddInbox<TDbContext>( this IServiceCollection services, IConfiguration configuration)`<br>`Clean.Messaging.Inbox.EntityFrameworkCore.DependencyInjection: public static MessagingBuilder<TDbContext> AddInbox<TDbContext>( this MessagingBuilder<TDbContext> builder, Action<InboxOptions>? configure = null)`<br>`Clean.Messaging.Inbox.EntityFrameworkCore.DependencyInjection: public static ModelBuilder AddInbox( this ModelBuilder modelBuilder)` | Public AddInbox callback/configuration/builder and model extensions; scoped stores, hosted workers and shared context registration. | INBOX-011, INBOX-012 |
| [src\Clean.Messaging.Inbox.EntityFrameworkCore\InboxConfiguration.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxConfiguration.cs) | — (internal implementation or project metadata) | Internal Configure maps admission identity/envelope and common target-entry model/constraints. | INBOX-002, INBOX-004, INBOX-011 |
| [src\Clean.Messaging.Inbox.EntityFrameworkCore\InboxDispatcher.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxDispatcher.cs) | — (internal implementation or project metadata) | Internal IInboxDispatcher.Dispatch implements fresh-scope strategy, completion precheck and atomic local consumer transaction. | INBOX-005, INBOX-006 |
| [src\Clean.Messaging.Inbox.EntityFrameworkCore\InboxStore.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs) | — (internal implementation or project metadata) | Internal port implementation Accept/Replay with independent contexts, duplicate identity query, retained history/count reconciliation; inherited durable operations. | INBOX-002, INBOX-003, INBOX-004, INBOX-009, INBOX-010, INBOX-013, INBOX-014, INBOX-015 |
| [src\Clean.Messaging.Inbox.EntityFrameworkCore\InboxValidation.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxValidation.cs) | — (internal implementation or project metadata) | Internal StartAsync/StopAsync startup checks relational model/entities and processor resolvability; does not validate physical schema. | INBOX-011, INBOX-012 |

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
