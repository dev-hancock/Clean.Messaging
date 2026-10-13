# Domain events behavioural specifications

**Status:** Draft. **Source reviewed:** 2026-10-13. **Verification:** Source-reviewed; not tested.

## Interpretation and evidence rules

Required behaviour is grounded only in normative repository instructions or explicit API XML requirements. Observed behaviour is derived from inspected implementation. Source-derived Reference and Explanation pages are descriptive corroboration, not independent normative authority. Proposed behaviour is explicitly **unapproved**. Unknown behaviour uses **numbered undecided questions**, without acceptance-criterion IDs or verification rows. Every acceptance criterion inherits its specification classification unless explicitly marked otherwise; none below overrides it. Required criteria can identify suspected source mismatches and are not claims of conformance.

## Capability purpose, contracts and common boundaries

Consumer-owned raising/custom source -> tracked-source capture in owning save -> concrete outbox target staging -> commit observation/verification -> source acknowledgement -> independent durable outbox delivery.

- **Preconditions:** consumer host owns registration, valid durable contracts and identities, configured relational schema/transactions where applicable, stable payload/source state and suitable cancellation/lifetime composition. Each focused specification narrows these preconditions.
- **Postconditions:** only observable admitted, staged, committed, acknowledged or per-target lifecycle outcomes explicitly stated below; success at one boundary does not imply success at another.
- **Invariants:** preserve logical event identity versus delivery identity, concrete durable contract names, original envelope/context metadata, target-specific completion, ownership fences, retry classifications and owning-context transaction boundaries. Repeated external effects remain possible.
- **Failures:** validation/serialization/resolution/database/transport failures and cancellation remain distinct; ordinary logging is not durable success. Source exceptions/cancellation racing commit require evidence, not an assumption of rollback.
- **Concurrency:** contexts and source collections are not automatically thread-safe. Lease ownership is fenced at settlement, not permission to suppress repeated external calls. Recovery requires a running healthy host/store; no strict latency, fairness or global completion order is specified.
- **Limitations:** Raising is in memory, not event sourcing. The base enforces no application-specific invariant. Ack means capture committed, not consumers finished. No automatic uncaptured-event crash recovery, historic domain replay, custom-source synchronization or global source-ID validation exists.

### Common public model and lifecycle contracts

Record positional fields, exposed init/set properties, constants, generic constraints, explicit interface implementations and protected extension operations are included in the source/contract table. Public primary constructors and inherited hosted StartAsync/StopAsync/Dispose behaviour are composition surfaces, not additional delivery guarantees. For entries, all identity/payload/context fields, Attempts, ClaimId, ClaimedUntil, NextAttemptAt, ProcessedAt, DeadLetteredAt, DiscardedAt, FailureCategory, FailureCode, ExceptionType, FailureMessage, LastFailedAt, LastFailedAttempt and derived Key participate in the lifecycle specifications. Port implementations (including internal EF adapters) are separately traced by responsibility.

### Supporting evidence reviewed

[Repository instructions](../../.github/copilot-instructions.md), [coding standards](../CODING_STANDARDS.md), [documentation index](../README.md); references: [architecture](../reference/architecture.md), [configuration](../reference/configuration.md), [contracts](../reference/contracts.md), [inbox](../reference/inbox.md), [outbox](../reference/outbox.md), [domain events](../reference/domain-events.md), [persistence](../reference/persistence.md), [processing](../reference/processing.md), [transport](../reference/transport.md). Explanations: [capability boundaries](../explanation/capability-boundaries.md), [message identity](../explanation/message-identity.md), [transaction boundaries](../explanation/transaction-boundaries.md), [delivery semantics](../explanation/delivery-semantics.md), [durable workflows](../explanation/durable-workflows.md). Adjacent ingress/broker/mapper APIs are outside these owned files; their source-derived descriptions are not independently tested guarantees.

### DOMAIN-001 — Raise pending events without owning business policy

**Classification:** Observed behaviour

**Evidence:** Source: AggregateRoot.Raise<TEvent>, explicit IDomainEventSource.Events/Acknowledge and RaisedEvent record; domain core project is dependency-free. [Domain events](../reference/domain-events.md) and architecture are descriptive.

Purpose: record in-memory facts selected by a consuming domain. Contracts: public abstract AggregateRoot, protected Raise<TEvent>(notnull), IDomainEventSource.Events and Acknowledge(Guid), RaisedEvent.Id/Message. Preconditions: nonnull raised payload. Postcondition: new nonempty Guid and payload are appended to a live read-only-view collection in raising order. No persistence key, timestamp, event store, business invariant, direct handler execution or event-sourcing protocol is owned here. Custom record/source construction is not validated by the record.

#### DOMAIN-001-AC-01 — Record separate raised facts

- **Given** an AggregateRoot subclass raises a valid payload twice.
- **When** pending Events are read through IDomainEventSource.
- **Then** two entries carry separate generated nonempty IDs and payload references in order; no consumer is invoked by Raise.

#### DOMAIN-001-AC-02 — Reject null raised payload

- **Given** an AggregateRoot subclass attempts Raise with null.
- **When** Raise runs.
- **Then** it throws and adds no pending event.

#### DOMAIN-001-AC-03 — Expose live pending collection

- **Given** a caller obtained the Events read-only view before another Raise or acknowledgement.
- **When** it observes the view afterward.
- **Then** the view reflects current pending events rather than an immutable snapshot; no concurrent mutation safety is promised.

### DOMAIN-002 — Acknowledgement removes pending facts by identity

**Classification:** Observed behaviour

**Evidence:** Source: AggregateRoot explicit IDomainEventSource.Acknowledge and RaisedEvent. Interface supplies an operation, not XML requirements for custom implementations.

Contracts: Acknowledge(Guid) and Events; postcondition for AggregateRoot: all matching IDs removed, other facts remain, absent/previously acknowledged IDs no-op. Acknowledgement timing supplied by EF integration means capture committed, not consumers/transport complete. Custom sources can implement different mechanics and must be source-reviewed separately.

#### DOMAIN-002-AC-01 — Remove matching identity without changing others

- **Given** several pending aggregate events exist.
- **When** one existing ID is acknowledged.
- **Then** that ID is absent and other payload/IDs remain pending.

#### DOMAIN-002-AC-02 — Make absent acknowledgement a no-op

- **Given** an unknown, empty or already acknowledged Guid.
- **When** AggregateRoot is acknowledged.
- **Then** remaining pending events are unchanged and no new fact is created.

### DOMAIN-003 — Opt-in EF capture composition

**Classification:** Observed behaviour

**Evidence:** Source: DomainEvents EF DependencyInjection, internal DomainEventCapture implements ISaveCapture; project references outbox EF/core. [Persistence](../reference/persistence.md) and [configuration](../reference/configuration.md) describe host composition.

Contracts: services.AddDomainEvents() and MessagingBuilder<TDbContext>.AddDomainEvents; TryAddEnumerable scoped ISaveCapture registration. Preconditions for durable operation: outbox services/model, relational configured context, both interceptors, concrete contracts and targets installed separately. Domain registration alone installs no outbox mappings/interceptors/store/dedicated validator. Host owns transaction and domain policy. Internal port implementation Capture(DbContext) is traced despite not being a public type.

#### DOMAIN-003-AC-01 — Register capture without manufacturing persistence

- **Given** host calls services or builder AddDomainEvents alone.
- **When** services are inspected or saving is attempted without outbox composition.
- **Then** domain capture is registered, but durable outbox composition is not implicitly installed and no successful persistence guarantee follows.

#### DOMAIN-003-AC-02 — Capture through correctly composed owning context

- **Given** outbox and domain capture are installed with mappings/interceptors/contracts/targets.
- **When** a tracked source owning save commits.
- **Then** pending domain facts are captured as outgoing work on that same owning context rather than a separate domain dispatcher.

### DOMAIN-004 — Capture tracked sources across entity states

**Classification:** Observed behaviour

**Evidence:** Source: DomainEventCapture.Capture(DbContext) snapshots tracked source entities and directly enumerates each Events collection; tracker.Capture<DomainEventCapture>. [GAP-DUR-010](GAPS.md#gap-dur-010) covers source assumptions.

Contracts: any tracked IDomainEventSource, not only AggregateRoot. Preconditions: stable valid source collection during save. Added/Modified/Unchanged/Deleted tracked sources are included; detached/untracked sources excluded. Each source event is transformed through OutboxMessage<object> but serialized by concrete runtime payload type. Captures deduplicate (DomainEventCapture type,eventId) across all sources in that context/pending transaction. Source enumeration has no event-list snapshot/lock, payload immutability check or unique-ID validation.

#### DOMAIN-004-AC-01 — Capture unchanged and custom tracked sources

- **Given** tracked custom/aggregate sources in Added, Modified, Unchanged or Deleted state contain valid events and untracked sources also contain events.
- **When** an intercepted owning save runs.
- **Then** tracked sources contribute captures independent of entity state; untracked sources do not.

#### DOMAIN-004-AC-02 — Serialize concrete event payload

- **Given** a custom RaisedEvent holds a registered concrete payload through object.
- **When** capture stages and owning save commits.
- **Then** outgoing contract/payload is concrete, event Id becomes EventId and target batch has a new delivery identity.

#### DOMAIN-004-AC-03 — Keep empty collections inert

- **Given** tracked sources have empty Events collections.
- **When** an owning save commits.
- **Then** no domain outgoing entries or domain acknowledgements are created solely because a source is tracked.

#### DOMAIN-004-AC-04 — Do not authorize concurrent source mutation

- **Given** a live Events collection or payload mutates concurrently with save capture.
- **When** current capture enumerates it.
- **Then** there is no supplied synchronization/snapshot guarantee; enumeration/serialization can fail or capture a source-derived view, not a guaranteed atomic domain snapshot.

### DOMAIN-005 — Atomic captured facts and application commit

**Classification:** Required behaviour

**Evidence:** Normative: repository/coding standards require transaction consistency and meaningful consumer-owned domain policy. Source: DomainEventCapture uses owning context IOutboxWriter and OutboxTracker; outbox interceptors. Reference/explanation are descriptive evidence only.

Purpose/invariant: successfully captured outgoing targets and domain/application changes share the owning commit; acknowledgment is not permission to claim completed downstream effects. Preconditions: valid nonempty-target capture and correctly composed context. Boundaries: untracked sources, external effects, other contexts and in-memory pre-save events are outside durable transaction.

#### DOMAIN-005-AC-01 — Commit state and event targets atomically

- **Given** tracked source contains valid event with targets and application state changes.
- **When** the owning transaction commits successfully.
- **Then** application changes and captured outgoing rows become durable together; source acknowledgement means capture commit, not consumer/broker completion.

#### DOMAIN-005-AC-02 — Do not acknowledge failed or cancelled save

- **Given** capture has staged new event rows but save fails or is cancelled before successful commit observation.
- **When** save failure/cancellation bookkeeping runs.
- **Then** event remains pending and no successful event acknowledgement is invented; newly captured rows are detached for retry.

#### DOMAIN-005-AC-03 — Retain pending events on explicit rollback

- **Given** one or more captures saved inside explicit transaction.
- **When** the transaction is observed rolled back.
- **Then** captured outgoing database state is not committed and events are not acknowledged; arbitrary aggregate changes still require application reconciliation.

### DOMAIN-006 — Defer acknowledgement and deduplicate multi-save capture

**Classification:** Observed behaviour

**Evidence:** Source: DomainEventCapture registers callback before factory/writer; OutboxTracker Capture/AcknowledgeAfterCommit/SaveSucceeded/TransactionCommitted. [Outbox specification](outbox.md#outbox-006--public-save-capture-protocol-and-per-context-deduplication) covers the shared tracker protocol.

Preconditions: stable pending source and explicit owning transaction observed by interceptors. Postconditions: same event/type captures once across successful saves before commit; acknowledgements run only after successful observed/verified commit. Events raised between saves can join pending transaction work. Acknowledge callback carries source/event identity but callback dictionary keyed solely eventId. No permanent inbox-like event admission ledger exists for these in-memory facts.

#### DOMAIN-006-AC-01 — Keep facts pending across multiple saves

- **Given** a source event was captured in the first save of an explicit transaction.
- **When** further saves occur before commit then transaction commits.
- **Then** the event remains pending before commit, duplicate capture is suppressed, then normal acknowledgement removes it after commit.

#### DOMAIN-006-AC-02 — Capture newly raised facts in later save

- **Given** an initial event is pending in transaction state and a distinct event is raised before a second save.
- **When** second save and final commit succeed.
- **Then** both facts have their respective outgoing target sets committed and acknowledged without recapturing the earlier event.

#### DOMAIN-006-AC-03 — Acknowledge only selected committed identities

- **Given** an event is raised after the last capture but before acknowledgement processing without corrupting enumeration.
- **When** earlier captured work commits.
- **Then** acknowledgement removes the captured ID only; the newer uncaptured event remains pending.

### DOMAIN-007 — Invalid custom facts and targetless acknowledgement

**Classification:** Observed behaviour

**Evidence:** Source: RaisedEvent lacks guards; DomainEventCapture queues acknowledgement then calls factory/writer; factory validates envelope/payload/contracts/targets. [GAP-DUR-011](GAPS.md#gap-dur-011) covers zero-target acknowledgement.

Contracts: custom IDomainEventSource events and outgoing factories. Preconditions: nonempty custom event ID, nonnull serializable payload and applicable targets for ordinary durable fan-out. Capture failures occur inside save interception, preserving source pending facts. However an empty custom target batch successfully stages no rows while capture/ack state exists; successful observed commit can acknowledge without outgoing rows. Failed commit without keys cannot be verified as success. Default no-consumer provider throws earlier, not an empty target result.

#### DOMAIN-007-AC-01 — Fail invalid custom fact capture

- **Given** custom source returns empty Id, null/unserializable payload, missing contract or default no-consumer payload.
- **When** owning save capture runs.
- **Then** factory failure aborts save capture and source facts stay pending; invalid RaisedEvent construction alone had not rejected input.

#### DOMAIN-007-AC-02 — Observe targetless successful acknowledgement

- **Given** a valid event uses custom providers returning no targets.
- **When** save and observed commit succeed.
- **Then** no outgoing target rows are committed but the captured source event can still be acknowledged; this is observed, not an approved delivery guarantee.

#### DOMAIN-007-AC-03 — Keep targetless failed commit uncertain

- **Given** targetless captured event has no outgoing keys and commit notification fails.
- **When** reconciliation executes.
- **Then** key verification cannot prove committed capture; acknowledgement remains tied to observed/verified outcome rather than assuming success.

### DOMAIN-008 — Custom source collisions and acknowledgement failures

**Classification:** Observed behaviour

**Evidence:** Source: capture-type/event-ID dedup across tracked sources, event-ID-only callback dictionary and synchronous postcommit callbacks. [GAP-DUR-010](GAPS.md#gap-dur-010), [GAP-DUR-009](GAPS.md#gap-dur-009). Interface XML provides no uniqueness/nonthrowing/concurrency contract.

Purpose: expose risks at the source extension boundary. Preconditions for current normal behaviour: distinct stable IDs across sources and nonthrowing acknowledgement with stable enumeration. Current integration does not enforce these conditions. Postcommit callback failure cannot undo committed domain/outbox state and may interrupt later acknowledgements, wake and tracker cleanup. Duplicate IDs across sources suppress later source capture/ack; source order is not approved conflict resolution.

#### DOMAIN-008-AC-01 — Observe duplicate IDs across tracked sources

- **Given** two sources supply the same event ID, possibly different payloads.
- **When** capture scans both inside one transaction.
- **Then** only the first authorized capture contributes work/ack; the second can remain pending, without a payload-conflict error or global durable dedup guarantee.

#### DOMAIN-008-AC-02 — Expose throwing acknowledgement after commit

- **Given** custom source Acknowledge throws and other sources also have callbacks.
- **When** owning transaction has committed and callbacks run.
- **Then** durable state can coexist with pending source events and interrupted later callbacks/wake; error does not imply transaction rollback.

#### DOMAIN-008-AC-03 — Do not infer retry-safe custom acknowledgement

- **Given** a custom source removes more facts, does nothing or mutates collection during acknowledgement.
- **When** normal callback invokes its Acknowledge.
- **Then** integration does not independently verify event removal or enforce AggregateRoot semantics; observable results depend on the custom source.

#### DOMAIN-008-AC-04 — Preserve observed ambiguity of callback error handling

- **Given** custom acknowledgement throws during commit-failure reconciliation after outgoing keys were verified.
- **When** the enclosing transaction-failure catch handles it.
- **Then** the callback error can be swallowed into unknown tracker state rather than universally surfaced; committed rows and unacknowledged source facts may coexist.

### DOMAIN-009 — Crash, recovery and retention boundaries

**Classification:** Observed behaviour

**Evidence:** Source: AggregateRoot in-memory list, capture/ack bookkeeping, outbox persisted entries and shared recovery/cleanup. [Durable workflows](../explanation/durable-workflows.md) and [delivery semantics](../explanation/delivery-semantics.md) explain boundaries.

Invariants: in-memory raising is not durable recording; captured delivery is driven by outbox after commit, independent of source event removal. Crash before capture can lose pending events; crash after durable commit can lose in-memory acknowledgement but not retained outgoing rows. Reloading an entity does not automatically reconstruct its pending event list from outbox. Outbox cleanup removes target/dedup evidence after retention; no domain event history or domain replay API is supplied.

#### DOMAIN-009-AC-01 — Do not recover uncaptured in-memory facts automatically

- **Given** an event was raised but no owning capture/commit persisted it.
- **When** the process crashes and a fresh aggregate/context loads.
- **Then** this capability provides no durable pending-event recovery or event-store reconstruction for that fact.

#### DOMAIN-009-AC-02 — Recover committed delivery without source acknowledgement

- **Given** domain/outbox transaction committed and process crashed before in-memory acknowledgement or wake.
- **When** host restarts with retained outgoing rows.
- **Then** outbox recovery can deliver the persisted targets independently of the old source object; downstream effects can repeat according to delivery boundaries.

#### DOMAIN-009-AC-03 — Bound durable evidence by outbox retention

- **Given** captured event targets have completed and their rows are later cleaned.
- **When** a caller asks for historical domain replay or submits the same logical ID again.
- **Then** no domain history/replay is supplied; expired outgoing dedup evidence cannot promise permanent suppression.

### DOMAIN-010 — Undecided source and acknowledgement contract

**Classification:** Unknown behaviour

**Evidence:** No normative rule found for custom-source globally unique IDs, collection/payload stability, nonthrowing acknowledgement, duplicate-source conflict or empty-target handling. Evidence: IDomainEventSource/DomainEventCapture and [GAP-DUR-010](GAPS.md#gap-dur-010), [GAP-DUR-011](GAPS.md#gap-dur-011), [GAP-DUR-009](GAPS.md#gap-dur-009).

All numbered questions are undecided, not acceptance requirements. Consumer owns domain policy; library extension contracts need a deliberate decision before imposing new validations or acknowledging zero-target facts.

**Undecided questions (not acceptance criteria):**

1. **valid custom source contract:** What approved policy should apply when custom sources can collide, mutate or throw during acknowledgment? Decision to resolve: select uniqueness/stability/acknowledgement guarantees and conflict/error visibility without silently imposing AggregateRoot-only usage.

2. **targetless commit acknowledgement:** What approved policy should apply when captured domain fact has zero outgoing targets? Decision to resolve: choose reject, retain pending, or acknowledge committed targetless capture and explicitly distinguish it from delivered downstream work.

## Verification matrix

Every row is a recommended future verification, not evidence that a test exists or passed. Contract-decision rows require a decision before a binding assertion can be implemented. Required mismatch rows must not be marked conforming from this audit.

| Acceptance criterion | Recommended test level | Observable assertion / decision | Status |
| --- | --- | --- | --- |
| DOMAIN-001-AC-01 | Unit | two entries carry separate generated nonempty IDs and payload references in order; no consumer is invoked by Raise | Not tested |
| DOMAIN-001-AC-02 | Unit | it throws and adds no pending event | Not tested |
| DOMAIN-001-AC-03 | Unit | the view reflects current pending events rather than an immutable snapshot; no concurrent mutation safety is promised | Not tested |
| DOMAIN-002-AC-01 | Unit | that ID is absent and other payload/IDs remain pending | Not tested |
| DOMAIN-002-AC-02 | Unit | remaining pending events are unchanged and no new fact is created | Not tested |
| DOMAIN-003-AC-01 | Composition integration | domain capture is registered, but durable outbox composition is not implicitly installed and no successful persistence guarantee follows | Not tested |
| DOMAIN-003-AC-02 | Relational integration | pending domain facts are captured as outgoing work on that same owning context rather than a separate domain dispatcher | Not tested |
| DOMAIN-004-AC-01 | Relational integration | tracked sources contribute captures independent of entity state; untracked sources do not | Not tested |
| DOMAIN-004-AC-02 | Unit + integration | outgoing contract/payload is concrete, event Id becomes EventId and target batch has a new delivery identity | Not tested |
| DOMAIN-004-AC-03 | Unit + integration | no domain outgoing entries or domain acknowledgements are created solely because a source is tracked | Not tested |
| DOMAIN-004-AC-04 | Fault-injection integration | there is no supplied synchronization/snapshot guarantee; enumeration/serialization can fail or capture a source-derived view, not a guaranteed atomic domain snapshot | Not tested |
| DOMAIN-005-AC-01 | Relational integration | application changes and captured outgoing rows become durable together; source acknowledgement means capture commit, not consumer/broker completion | Not tested |
| DOMAIN-005-AC-02 | Fault-injection integration | event remains pending and no successful event acknowledgement is invented; newly captured rows are detached for retry | Not tested |
| DOMAIN-005-AC-03 | Relational integration | captured outgoing database state is not committed and events are not acknowledged; arbitrary aggregate changes still require application reconciliation | Not tested |
| DOMAIN-006-AC-01 | Relational integration | the event remains pending before commit, duplicate capture is suppressed, then normal acknowledgement removes it after commit | Not tested |
| DOMAIN-006-AC-02 | Relational integration | both facts have their respective outgoing target sets committed and acknowledged without recapturing the earlier event | Not tested |
| DOMAIN-006-AC-03 | Unit + integration | acknowledgement removes the captured ID only; the newer uncaptured event remains pending | Not tested |
| DOMAIN-007-AC-01 | Unit + integration | factory failure aborts save capture and source facts stay pending; invalid RaisedEvent construction alone had not rejected input | Not tested |
| DOMAIN-007-AC-02 | Relational integration | no outgoing target rows are committed but the captured source event can still be acknowledged; this is observed, not an approved delivery guarantee | Not tested |
| DOMAIN-007-AC-03 | Fault-injection integration | key verification cannot prove committed capture; acknowledgement remains tied to observed/verified outcome rather than assuming success | Not tested |
| DOMAIN-008-AC-01 | Unit + integration | only the first authorized capture contributes work/ack; the second can remain pending, without a payload-conflict error or global durable dedup guarantee | Not tested |
| DOMAIN-008-AC-02 | Fault-injection integration | durable state can coexist with pending source events and interrupted later callbacks/wake; error does not imply transaction rollback | Not tested |
| DOMAIN-008-AC-03 | Unit | integration does not independently verify event removal or enforce AggregateRoot semantics; observable results depend on the custom source | Not tested |
| DOMAIN-008-AC-04 | Fault-injection integration | the callback error can be swallowed into unknown tracker state rather than universally surfaced; committed rows and unacknowledged source facts may coexist | Not tested |
| DOMAIN-009-AC-01 | Fault-injection integration | this capability provides no durable pending-event recovery or event-store reconstruction for that fact | Not tested |
| DOMAIN-009-AC-02 | Fault-injection integration | outbox recovery can deliver the persisted targets independently of the old source object; downstream effects can repeat according to delivery boundaries | Not tested |
| DOMAIN-009-AC-03 | Relational integration | no domain history/replay is supplied; expired outgoing dedup evidence cannot promise permanent suppression | Not tested |

## Owned source and public contract traceability

Every authored .cs/.csproj/.md file in the two owned projects is included, excluding bin/obj. A dash means no publicly accessible type/member in that file; implementation operations remain recorded in responsibility. Exact signatures/positional members are retained in the session audit JSON. Constructor/service composition is covered with its owning type.

| Source path | Public contracts / operations | Responsibility | Specifications |
| --- | --- | --- | --- |
| [src\Clean.Messaging.DomainEvents\AggregateRoot.cs](../../src/Clean.Messaging.DomainEvents/AggregateRoot.cs) | `Clean.Messaging.DomainEvents.public abstract class AggregateRoot : IDomainEventSource`<br>`Clean.Messaging.DomainEvents.AggregateRoot: protected void Raise<TEvent>( TEvent message)`<br>`Clean.Messaging.DomainEvents.AggregateRoot: IReadOnlyCollection<RaisedEvent> IDomainEventSource.Events =>`<br>`Clean.Messaging.DomainEvents.AggregateRoot: void IDomainEventSource.Acknowledge( Guid eventId)` | Public abstract consumer-domain base, protected Raise, explicit live Events view and identity-based acknowledgement. | DOMAIN-001, DOMAIN-002, DOMAIN-005, DOMAIN-009 |
| [src\Clean.Messaging.DomainEvents\Clean.Messaging.DomainEvents.csproj](../../src/Clean.Messaging.DomainEvents/Clean.Messaging.DomainEvents.csproj) | — (internal implementation or project metadata) | Dependency-free .NET 10 domain event representation/base project. | DOMAIN-001, DOMAIN-003 |
| [src\Clean.Messaging.DomainEvents\RaisedEvent.cs](../../src/Clean.Messaging.DomainEvents/RaisedEvent.cs) | `Clean.Messaging.DomainEvents.public sealed record RaisedEvent( Guid Id, object Message)`<br>`Clean.Messaging.DomainEvents.public interface IDomainEventSource`<br>`Clean.Messaging.DomainEvents.IDomainEventSource: void Acknowledge( Guid eventId)`<br>`Clean.Messaging.DomainEvents.IDomainEventSource: IReadOnlyCollection<RaisedEvent> Events { get; }` | Event identity/payload record and custom event source pending collection/acknowledgement port. | DOMAIN-001, DOMAIN-002, DOMAIN-004, DOMAIN-007, DOMAIN-008, DOMAIN-010 |
| [src\Clean.Messaging.DomainEvents.EntityFrameworkCore\Clean.Messaging.DomainEvents.EntityFrameworkCore.csproj](../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/Clean.Messaging.DomainEvents.EntityFrameworkCore.csproj) | — (internal implementation or project metadata) | EF capture adapter references domain core and outbox EF; no business policy implementation. | DOMAIN-003, DOMAIN-005 |
| [src\Clean.Messaging.DomainEvents.EntityFrameworkCore\DependencyInjection.cs](../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DependencyInjection.cs) | `Clean.Messaging.DomainEvents.EntityFrameworkCore.public static class DependencyInjection`<br>`Clean.Messaging.DomainEvents.EntityFrameworkCore.DependencyInjection: public static IServiceCollection AddDomainEvents( this IServiceCollection services)`<br>`Clean.Messaging.DomainEvents.EntityFrameworkCore.DependencyInjection: public static MessagingBuilder<TDbContext> AddDomainEvents<TDbContext>( this MessagingBuilder<TDbContext> builder)` | Public service/builder AddDomainEvents scoped ISaveCapture registration only; outbox composition remains explicit. | DOMAIN-003, DOMAIN-005 |
| [src\Clean.Messaging.DomainEvents.EntityFrameworkCore\DomainEventCapture.cs](../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs) | — (internal implementation or project metadata) | Internal ISaveCapture.Capture scans tracked sources, deduplicates event capture, registers postcommit ack and stages concrete event targets. | DOMAIN-003, DOMAIN-004, DOMAIN-005, DOMAIN-006, DOMAIN-007, DOMAIN-008, DOMAIN-009, DOMAIN-010 |

### Shared dependency trace (excluded from owned manifest count)

| Source path | Reviewed responsibility | Related specifications |
| --- | --- | --- |
| [src\Clean.Messaging\Consumers\ConsumerRegistry.cs](../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs) | Exact-type/name consumer selection, permanent faults | INBOX-001, INBOX-009; OUTBOX-003; DOMAIN-007; DOMAIN-003–DOMAIN-009 via composed outbox |
| [src\Clean.Messaging\Serialization\MessageSerializer.cs](../../src/Clean.Messaging/Serialization/MessageSerializer.cs) | Concrete runtime contract serialization and fault classification | INBOX-001; OUTBOX-001; DOMAIN-004, DOMAIN-007; DOMAIN-003–DOMAIN-009 via composed outbox |
| [src\Clean.Messaging\Persistence\IDurableStore.cs](../../src/Clean.Messaging/Persistence/IDurableStore.cs) | Inherited Claim/Renew/Complete/Retry/DeadLetter/Cleanup and OwnedEntry | INBOX-004; OUTBOX-013; DOMAIN-003–DOMAIN-009 via composed outbox |
| [src\Clean.Messaging\Persistence\IDurableEntry.cs](../../src/Clean.Messaging/Persistence/IDurableEntry.cs) | Common entry identity/state contract | INBOX-004; OUTBOX-013; DOMAIN-003–DOMAIN-009 via composed outbox |
| [src\Clean.Messaging.EntityFrameworkCore\DurableStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs) | Claim ordering/eligibility, ownership fence, lifecycle updates/cleanup | INBOX-004, INBOX-010; OUTBOX-013, OUTBOX-015; DOMAIN-003–DOMAIN-009 via composed outbox |
| [src\Clean.Messaging.EntityFrameworkCore\PersistenceServices.cs](../../src/Clean.Messaging.EntityFrameworkCore/PersistenceServices.cs) | Same scoped persistence context and options composition | INBOX-011; OUTBOX-016; DOMAIN-003–DOMAIN-009 via composed outbox |

Domain capture also depends directly on the audited outbox EF tracker/writer/interceptors and provider-neutral OutboxEntryFactory; their full source/contract trace is in [outbox specifications](outbox.md#owned-source-and-public-contract-traceability).

## Review limitations

- No production/test changes or test/build execution were performed. Verification matrices are entirely **Not tested**.
- Source-derived ordering, transaction interception and uniqueness outcomes are not provider certification. Fault-injection and real relational concurrency verification remain necessary.
- The [gap register](GAPS.md) records unresolved decisions and their evidence. Observed suspected mismatches are not approved fixes.
