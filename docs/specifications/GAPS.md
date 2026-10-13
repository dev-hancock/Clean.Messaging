# Behavioural gaps and design decisions

**Status:** Open, source-reviewed 2026-10-13. No production fix, approval or test result is implied by this register.

## Interpreting the register
Questions distinguish absent intent from suspected implementation defects. Recommendations are proposals, not requirements. Resolve identity/transaction/provider policies explicitly before freezing source observations in acceptance tests. A contradiction may be between normative coding guidance and source, or between descriptive documentation and source; those are different evidence levels.

**45 recorded gaps**; all remain open unless a capability explicitly records a narrower decision.

| Gap | Classification | Decision | Affected specifications |
| --- | --- | --- | --- |
| [GAP-CORE-001](#gap-core-001) | Unknown behaviour | Validation bypass through public registration records | CORE-005 |
| [GAP-CORE-002](#gap-core-002) | Unknown behaviour | Ambient metadata scope misuse and child-flow lifetime | IDENTITY-005 |
| [GAP-CORE-003](#gap-core-003) | Proposed behaviour | Null nested options and undefined retry backoff | CONFIG-006, RETRY-006 |
| [GAP-CORE-004](#gap-core-004) | Unknown behaviour | Public ownership time/input domain | PERSIST-003, CLAIM-005 |
| [GAP-CORE-005](#gap-core-005) | Unknown behaviour | Provider concurrency and uncertain-commit attribution | PERSIST-004 |
| [GAP-CORE-006](#gap-core-006) | Unknown behaviour | RetryPolicy direct invalid input domain | RETRY-006 |
| [GAP-CORE-007](#gap-core-007) | Proposed behaviour | Failure logs precede confirmed settlement | OPS-005 |
| [GAP-CORE-008](#gap-core-008) | Unknown behaviour | Null invocation and malformed serialization records | CORE-003, CORE-004, CORE-005 |
| [GAP-CORE-009](#gap-core-009) | Unknown behaviour | Runtime options and serializer reconfiguration | CORE-004, CONFIG-002, RETRY-004 |
| [GAP-CORE-010](#gap-core-010) | Unknown behaviour | Resource arithmetic and direct management limit validation | WORK-004, OPS-001 |
| [GAP-DUR-001](#gap-dur-001) | Unknown behaviour | New empty inbox admission and no-consumer policy | INBOX-001, INBOX-003, INBOX-014 |
| [GAP-DUR-002](#gap-dur-002) | Unknown behaviour | Replay target syntax and type compatibility | INBOX-009, INBOX-014 |
| [GAP-DUR-003](#gap-dur-003) | Unknown behaviour | Replay update errors, count attribution and page progress | INBOX-009, INBOX-013, INBOX-014, INBOX-015 |
| [GAP-DUR-004](#gap-dur-004) | Unknown behaviour | Inbox admission payload retention and replay/dedup horizon | INBOX-002, INBOX-010, INBOX-014 |
| [GAP-DUR-005](#gap-dur-005) | Required behaviour | Hosted cleanup captures scoped store | CONFIG-001, INBOX-012, OUTBOX-017 |
| [GAP-DUR-006](#gap-dur-006) | Unknown behaviour | Default local targets prevent external-only events | OUTBOX-003, OUTBOX-018, DOMAIN-007 |
| [GAP-DUR-007](#gap-dur-007) | Unknown behaviour | Tracked versus persisted outgoing duplicate/content conflict | OUTBOX-004, OUTBOX-018 |
| [GAP-DUR-008](#gap-dur-008) | Unknown behaviour | Uncertain commit reconciliation and evidence expiry | OUTBOX-008, OUTBOX-018, DOMAIN-007, DOMAIN-009 |
| [GAP-DUR-009](#gap-dur-009) | Unknown behaviour | Postcommit callback failure and callback-only participation | OUTBOX-010, OUTBOX-018, DOMAIN-008, DOMAIN-010 |
| [GAP-DUR-010](#gap-dur-010) | Unknown behaviour | Custom event source identity, enumeration and acknowledgement assumptions | DOMAIN-004, DOMAIN-008, DOMAIN-010 |
| [GAP-DUR-011](#gap-dur-011) | Unknown behaviour | Targetless captured fact acknowledgement meaning | DOMAIN-007, DOMAIN-010, OUTBOX-018 |
| [GAP-WF-001](#gap-wf-001) | Unknown behaviour | Default value objects bypass admission constraints | SCHED-003, SCHED-015, SAGA-004, SAGA-015 |
| [GAP-WF-002](#gap-wf-002) | Unknown behaviour | Stable schedule identity does not imply duplicate insert success | SCHED-001, SCHED-006 |
| [GAP-WF-003](#gap-wf-003) | Unknown behaviour | Preleased backlog and absent schedule renewal | SCHED-009, SCHED-010 |
| [GAP-WF-004](#gap-wf-004) | Unknown behaviour | Schedule expiry differs between dispatch and settlement | SCHED-010, SCHED-012 |
| [GAP-WF-005](#gap-wf-005) | Unknown behaviour | Quartz fallback waits for scheduler I/O | SCHED-014, SCHED-015 |
| [GAP-WF-006](#gap-wf-006) | Unknown behaviour | Caller savepoints and uncertain commit ownership | SCHED-006, SCHED-013, SCHED-015, SAGA-011 |
| [GAP-WF-007](#gap-wf-007) | Unknown behaviour | Persisted saga identity equality is collation-dependent | SAGA-004, SAGA-015 |
| [GAP-WF-008](#gap-wf-008) | Unknown behaviour | SagaResult.Fail accepts malformed diagnostics | SAGA-006, SAGA-014, SAGA-015 |
| [GAP-WF-009](#gap-wf-009) | Unknown behaviour | Start collision translation is not unique-error-specific | SAGA-013, SAGA-015 |
| [GAP-WF-010](#gap-wf-010) | Unknown behaviour | Cancelled collision lookup hides cancellation behind original save error | SAGA-013, SAGA-014 |
| [GAP-WF-011](#gap-wf-011) | Unknown behaviour | Saga terminal identities retained indefinitely | SAGA-004, SAGA-017 |
| [GAP-WF-012](#gap-wf-012) | Unknown behaviour | Repeated unsaved starts lack tracked-instance merge | SAGA-005, SAGA-015 |
| [GAP-WF-013](#gap-wf-013) | Unknown behaviour | Null option objects and numeric upper limits are unspecified | SCHED-005, SCHED-015, SAGA-003 |
| [GAP-WF-014](#gap-wf-014) | Unknown behaviour | Empty custom outgoing targets can complete schedules without outgoing work | SCHED-011, SCHED-015 |
| [GAP-WF-015](#gap-wf-015) | Unknown behaviour | Retained workflow schema evolution has no migration contract | SAGA-007, SAGA-015 |
| [GAP-TX-001](#gap-tx-001) | Observed behaviour | Stale MQTT outbox package API documentation | MQTT-001, MQTT-015 |
| [GAP-TX-002](#gap-tx-002) | Observed behaviour | Synchronous mapping cancellation can become permanent or poison failure | TRANSPORT-005, TRANSPORT-006, TRANSPORT-008, MQTT-006 |
| [GAP-TX-003](#gap-tx-003) | Unknown behaviour | Retained schema and route compatibility policy is unapproved | TRANSPORT-008, MQTT-009, MQTT-015 |
| [GAP-TX-004](#gap-tx-004) | Unknown behaviour | Transport metadata stops at durable mapping boundary | TRANSPORT-004, TRANSPORT-006, MQTT-004, MQTT-010 |
| [GAP-TX-005](#gap-tx-005) | Observed behaviour | Optional empty GUID metadata is emitted but rejected by peer ingress | MQTT-010, MQTT-015 |
| [GAP-TX-006](#gap-tx-006) | Unknown behaviour | Shared-client concurrency and disposal safety not established | MQTT-011, MQTT-015 |
| [GAP-TX-007](#gap-tx-007) | Observed behaviour | Inbox poison acknowledgement and disconnect log wording overstate timing | MQTT-014 |
| [GAP-TX-008](#gap-tx-008) | Unknown behaviour | Route/map cross-registration validation happens at resolution | TRANSPORT-003, MQTT-001, MQTT-015 |
| [GAP-TX-009](#gap-tx-009) | Unknown behaviour | Inbox shutdown duration and synchronization resource policy not established | MQTT-008, MQTT-015 |

## GAP-CORE-001

**Issue:** Validation bypass through public registration records
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Must direct MessageContract/custom invoker registration satisfy and independently validate the same identity constraints as fluent registration?

**Recommendation (not approved):** Decide the supported public registration boundary before specifying failure timing/type for bypassed validation.

**Affected specifications:** CORE-005

**Evidence:**
- [src/Clean.Messaging/Contracts/MessageContractRegistry.cs](../../src/Clean.Messaging/Contracts/MessageContractRegistry.cs)
- [src/Clean.Messaging/Consumers/ConsumerRegistry.cs](../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs)
- [src/Clean.Messaging/DependencyInjection.cs](../../src/Clean.Messaging/DependencyInjection.cs)

## GAP-CORE-002

**Issue:** Ambient metadata scope misuse and child-flow lifetime
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Are LIFO disposal in the same flow and awaited handler work explicit preconditions? What is promised for out-of-order or child-flow disposal?

**Recommendation (not approved):** Document supported scope use; consider validation only if unsupported misuse must fail predictably.

**Affected specifications:** IDENTITY-005

**Evidence:**
- [src/Clean.Messaging/MessageContext.cs](../../src/Clean.Messaging/MessageContext.cs)

## GAP-CORE-003

**Issue:** Null nested options and undefined retry backoff
**Classification:** Proposed behaviour
**Status:** Open.

**Question/decision:** Should all null groups and undefined Backoff values be rejected as startup options validation failures?

**Recommendation (not approved):** Approve CONFIG-006 if desired. Currently null groups throw during validation; invalid modes pass retry validation then throw during delay evaluation.

**Affected specifications:** CONFIG-006, RETRY-006

**Evidence:**
- [src/Clean.Messaging/Processing/MessageOptionsValidator.cs](../../src/Clean.Messaging/Processing/MessageOptionsValidator.cs)
- [src/Clean.Messaging/Retry/RetryOptionsValidator.cs](../../src/Clean.Messaging/Retry/RetryOptionsValidator.cs)
- [src/Clean.Messaging/Retry/RetryPolicy.cs](../../src/Clean.Messaging/Retry/RetryPolicy.cs)

## GAP-CORE-004

**Issue:** Public ownership time/input domain
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Must Renew reject shortening/past expiry, non-UTC DateTime, empty claim strings or overflowing instants?

**Recommendation (not approved):** Specify normal port preconditions and desired invalid-input outcomes without freezing incidental time-arithmetic exceptions.

**Affected specifications:** PERSIST-003, CLAIM-005

**Evidence:**
- [src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)
- [src/Clean.Messaging/Persistence/IDurableStore.cs](../../src/Clean.Messaging/Persistence/IDurableStore.cs)

## GAP-CORE-005

**Issue:** Provider concurrency and uncertain-commit attribution
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Which real relational providers and commit-loss/race scenarios must be supported? Are state-only verification predicates sufficient to attribute success to the intended operation?

**Recommendation (not approved):** Select providers and exercise real locking, conditional update, transient strategy and racing verification paths before claiming certification.

**Affected specifications:** PERSIST-004

**Evidence:**
- [src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs)

## GAP-CORE-006

**Issue:** RetryPolicy direct invalid input domain
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** What should Evaluate do for null exception, nonpositive attempts or undefined action/mode outside processor calls?

**Recommendation (not approved):** Define public preconditions and argument failures; do not convert observed negative/zero delay behavior into a supported policy.

**Affected specifications:** RETRY-006

**Evidence:**
- [src/Clean.Messaging/Retry/RetryPolicy.cs](../../src/Clean.Messaging/Retry/RetryPolicy.cs)

## GAP-CORE-007

**Issue:** Failure logs precede confirmed settlement
**Classification:** Proposed behaviour
**Status:** Open.

**Question/decision:** Should retry/dead-letter logs distinguish intended settlement from confirmed durable success?

**Recommendation (not approved):** Consider wording or post-success outcome logs; no telemetry change is approved. Metrics already count true settlement only.

**Affected specifications:** OPS-005

**Evidence:**
- [src/Clean.Messaging.Inbox/Dispatching/InboxProcessor.cs](../../src/Clean.Messaging.Inbox/Dispatching/InboxProcessor.cs)
- [src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs](../../src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs)

## GAP-CORE-008

**Issue:** Null invocation and malformed serialization records
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** What failure type/code must a public invoker or deserializer return for null runtime input or default/null MessageData fields?

**Recommendation (not approved):** Direct null invocation currently dereferences message.GetType in its mismatch branch; agree argument versus permanent-message failure and malformed record handling before tests freeze accidental exceptions.

**Affected specifications:** CORE-003, CORE-004, CORE-005

**Evidence:**
- [src/Clean.Messaging/Consumers/ConsumerRegistry.cs](../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs)
- [src/Clean.Messaging/Serialization/MessageSerializer.cs](../../src/Clean.Messaging/Serialization/MessageSerializer.cs)

## GAP-CORE-009

**Issue:** Runtime options and serializer reconfiguration
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Is mutating captured options or serializer options after startup supported, and when must changes become effective?

**Recommendation (not approved):** Specify immutable-after-start preconditions or an explicit reconfiguration protocol; do not infer live reload from mutable properties.

**Affected specifications:** CORE-004, CONFIG-002, RETRY-004

**Evidence:**
- [src/Clean.Messaging/Retry/RetryPolicy.cs](../../src/Clean.Messaging/Retry/RetryPolicy.cs)
- [src/Clean.Messaging/Serialization/MessageSerializer.cs](../../src/Clean.Messaging/Serialization/MessageSerializer.cs)
- [src/Clean.Messaging/MessageOptions.cs](../../src/Clean.Messaging/MessageOptions.cs)

## GAP-CORE-010

**Issue:** Resource arithmetic and direct management limit validation
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Must extreme Capacity plus Concurrency and nonpositive direct DeadLetterStore.Get limits fail predictably at the public boundary?

**Recommendation (not approved):** Consider explicit resource-budget validation; managers guard positive limits but the direct provider port does not uniformly enforce the same boundary.

**Affected specifications:** WORK-004, OPS-001

**Evidence:**
- [src/Clean.Messaging/Processing/WorkLoop.cs](../../src/Clean.Messaging/Processing/WorkLoop.cs)
- [src/Clean.Messaging/Processing/WorkLoopOptions.cs](../../src/Clean.Messaging/Processing/WorkLoopOptions.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs)

## GAP-DUR-001

**Issue:** New empty inbox admission and no-consumer policy
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Should a valid envelope with no consumer entries be retained for future replay, rejected, or governed by an explicit contract?

**Recommendation (not approved):** Decide separately for default typed admission and direct empty batches; do not silently remove the current rejection.

**Affected specifications:** INBOX-001, INBOX-003, INBOX-014

**Evidence:**
- [src/Clean.Messaging.Inbox/Persistence/IInboxStore.cs](../../src/Clean.Messaging.Inbox/Persistence/IInboxStore.cs)
- [src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs)
- [src/Clean.Messaging/Consumers/ConsumerRegistry.cs](../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs)

## GAP-DUR-002

**Issue:** Replay target syntax and type compatibility
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Should replay require registered compatible <=200 ASCII target identity, or deliberately permit unresolved targets?

**Recommendation (not approved):** Approve manager validation policy and document deferred dispatch/storage errors; current manager checks blank only.

**Affected specifications:** INBOX-009, INBOX-014

**Evidence:**
- [src/Clean.Messaging.Inbox/Runtime/InboxManager.cs](../../src/Clean.Messaging.Inbox/Runtime/InboxManager.cs)
- [src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs)
- [docs/reference/inbox.md](../../docs/reference/inbox.md)

## GAP-DUR-003

**Issue:** Replay update errors, count attribution and page progress
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** How should failed nonduplicate saves remain visible, how are concurrent additions attributed, and how does replay progress beyond already-covered oldest pages?

**Recommendation (not approved):** Investigate required failure-transparency mismatch; proposed reconcile-only-confirmed-duplicate rule remains unapproved. Decide counts and continuation separately.

**Affected specifications:** INBOX-009, INBOX-013, INBOX-014, INBOX-015

**Evidence:**
- [src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs)
- [src/Clean.Messaging.Inbox/Runtime/ReplayResult.cs](../../src/Clean.Messaging.Inbox/Runtime/ReplayResult.cs)
- [docs/CODING_STANDARDS.md](../../docs/CODING_STANDARDS.md)

## GAP-DUR-004

**Issue:** Inbox admission payload retention and replay/dedup horizon
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Are indefinite admission payload retention and replay after entry cleanup intended, and what horizon governs deletion/duplicate guarantees?

**Recommendation (not approved):** Define admission retention separately from terminal-entry retention and make repeated-effect consequences explicit before adding cleanup.

**Affected specifications:** INBOX-002, INBOX-010, INBOX-014

**Evidence:**
- [src/Clean.Messaging.Inbox/Admission/InboxAdmission.cs](../../src/Clean.Messaging.Inbox/Admission/InboxAdmission.cs)
- [src/Clean.Messaging.Inbox/Runtime/InboxCleanupWorker.cs](../../src/Clean.Messaging.Inbox/Runtime/InboxCleanupWorker.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)

## GAP-DUR-005

**Issue:** Hosted cleanup captures scoped store
**Classification:** Required behaviour
**Status:** Open.

**Question/decision:** How should default hosted cleanup honor required scoped-service lifetime composition?

**Recommendation (not approved):** Record suspected conformance defect; approve scoped-per-cycle composition only through a separate code change and verify with scope validation.

**Affected specifications:** CONFIG-001, INBOX-012, OUTBOX-017

**Evidence:**
- [src/Clean.Messaging.Inbox/Runtime/InboxCleanupWorker.cs](../../src/Clean.Messaging.Inbox/Runtime/InboxCleanupWorker.cs)
- [src/Clean.Messaging.Inbox.EntityFrameworkCore/DependencyInjection.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/DependencyInjection.cs)
- [src/Clean.Messaging.Outbox/Runtime/OutboxCleanupWorker.cs](../../src/Clean.Messaging.Outbox/Runtime/OutboxCleanupWorker.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/DependencyInjection.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/DependencyInjection.cs)
- [docs/CODING_STANDARDS.md](../../docs/CODING_STANDARDS.md)

## GAP-DUR-006

**Issue:** Default local targets prevent external-only events
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Should no local consumers be an error when external providers supply work, and should zero-target events be valid?

**Recommendation (not approved):** Decide target provider composition contract without assuming external route registration overrides default Get failure.

**Affected specifications:** OUTBOX-003, OUTBOX-018, DOMAIN-007

**Evidence:**
- [src/Clean.Messaging.Outbox/Transport/OutboxTargetProvider.cs](../../src/Clean.Messaging.Outbox/Transport/OutboxTargetProvider.cs)
- [src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs](../../src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/DependencyInjection.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/DependencyInjection.cs)

## GAP-DUR-007

**Issue:** Tracked versus persisted outgoing duplicate/content conflict
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Should persisted event/target duplicates suppress, conflict or fail save, and is payload equality required?

**Recommendation (not approved):** Define idempotency scope and retention horizon explicitly; do not claim current tracked-only suppression is database-wide.

**Affected specifications:** OUTBOX-004, OUTBOX-018

**Evidence:**
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxConfiguration.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxConfiguration.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DurableEntryConfiguration.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableEntryConfiguration.cs)

## GAP-DUR-008

**Issue:** Uncertain commit reconciliation and evidence expiry
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** What recovery contract applies when expected keys are missing/cleaned, verification fails or zero-key captures cannot prove commit?

**Recommendation (not approved):** Define caller/context reconciliation and evidence lifetime, distinguishing unknown from rollback/success; verify database/provider behaviour before strengthening guarantees.

**Affected specifications:** OUTBOX-008, OUTBOX-018, DOMAIN-007, DOMAIN-009

**Evidence:**
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxCommitVerifier.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxCommitVerifier.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs)

## GAP-DUR-009

**Issue:** Postcommit callback failure and callback-only participation
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Should callbacks be nonthrowing/isolated/retried, how are postcommit errors reported, and should callbacks alone make pending state nonempty?

**Recommendation (not approved):** Approve callback lifecycle semantics separately; current exceptions can interrupt subsequent callbacks/wake/state cleanup after durable commit.

**Affected specifications:** OUTBOX-010, OUTBOX-018, DOMAIN-008, DOMAIN-010

**Evidence:**
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs)
- [src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs](../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs)

## GAP-DUR-010

**Issue:** Custom event source identity, enumeration and acknowledgement assumptions
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** What stable uniqueness/payload/collection/nonthrowing acknowledgement rules bind custom sources, and how are same-ID different-source collisions reported?

**Recommendation (not approved):** Specify source extension assumptions and observable conflicts before adding validation/locking or treating source order as policy.

**Affected specifications:** DOMAIN-004, DOMAIN-008, DOMAIN-010

**Evidence:**
- [src/Clean.Messaging.DomainEvents/RaisedEvent.cs](../../src/Clean.Messaging.DomainEvents/RaisedEvent.cs)
- [src/Clean.Messaging.DomainEvents/AggregateRoot.cs](../../src/Clean.Messaging.DomainEvents/AggregateRoot.cs)
- [src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs](../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs)

## GAP-DUR-011

**Issue:** Targetless captured fact acknowledgement meaning
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Should a successful observed capture commit with zero outgoing rows acknowledge the source event?

**Recommendation (not approved):** Choose reject/retain/acknowledge explicitly; never equate observed targetless acknowledgement with downstream delivery.

**Affected specifications:** DOMAIN-007, DOMAIN-010, OUTBOX-018

**Evidence:**
- [src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs](../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs)

## GAP-WF-001

**Issue:** Default value objects bypass admission constraints
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Must default target and default correlation key be rejected at the writer/runtime boundary?

**Recommendation (not approved):** Choose explicit boundary validation and compatibility treatment; constructors alone do not establish this guarantee.

**Affected specifications:** SCHED-003, SCHED-015, SAGA-004, SAGA-015

**Evidence:**
- [src/Clean.Messaging.Scheduling/ScheduleTarget.cs](../../src/Clean.Messaging.Scheduling/ScheduleTarget.cs)
- [src/Clean.Messaging.Scheduling/Runtime/ScheduleWriter.cs](../../src/Clean.Messaging.Scheduling/Runtime/ScheduleWriter.cs)
- [src/Clean.Messaging.Sagas/SagaKey.cs](../../src/Clean.Messaging.Sagas/SagaKey.cs)
- [src/Clean.Messaging.Sagas/Runtime/SagaProcessor.cs](../../src/Clean.Messaging.Sagas/Runtime/SagaProcessor.cs)

## GAP-WF-002

**Issue:** Stable schedule identity does not imply duplicate insert success
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Is an equal retry supposed to be idempotent admission, or must the owner retry/recognize a failed unit of work?

**Recommendation (not approved):** Document the approved duplicate-insert result without inventing upsert or allowing changed payload to replace retained work.

**Affected specifications:** SCHED-001, SCHED-006

**Evidence:**
- [src/Clean.Messaging.Scheduling/IMessageScheduler.cs](../../src/Clean.Messaging.Scheduling/IMessageScheduler.cs)
- [src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleStore.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleStore.cs)

## GAP-WF-003

**Issue:** Preleased backlog and absent schedule renewal
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Should queued/long-running schedules renew or lease only when admitted, and what bounds are operationally required?

**Recommendation (not approved):** Decide queue/renewal policy before approving compatibility tests; do not borrow shared inbox/outbox renewal guarantees.

**Affected specifications:** SCHED-009, SCHED-010

**Evidence:**
- [src/Clean.Messaging.Scheduling/Runtime/SchedulingWorker.cs](../../src/Clean.Messaging.Scheduling/Runtime/SchedulingWorker.cs)
- [src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleStore.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleStore.cs)

## GAP-WF-004

**Issue:** Schedule expiry differs between dispatch and settlement
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Is token-only expired failure settlement and no final lease check on successful execution deliberate?

**Recommendation (not approved):** Choose admission versus completion fencing; test expiry/reclaim races with real provider transactions.

**Affected specifications:** SCHED-010, SCHED-012

**Evidence:**
- [src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleExecutor.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleExecutor.cs)
- [src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleStore.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleStore.cs)

## GAP-WF-005

**Issue:** Quartz fallback waits for scheduler I/O
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Must local wake/deadline remain responsive when ScheduleJob hangs, rather than only throws?

**Recommendation (not approved):** Define a bounded scheduler-I/O/fallback policy and host shutdown assumptions; current adapter has no timeout or reconnect loop.

**Affected specifications:** SCHED-014, SCHED-015

**Evidence:**
- [src/Clean.Messaging.Scheduling.Quartz/SchedulingEngine.cs](../../src/Clean.Messaging.Scheduling.Quartz/SchedulingEngine.cs)

## GAP-WF-006

**Issue:** Caller savepoints and uncertain commit ownership
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Which caller savepoint and uncertain-commit patterns are supported across composed schedules/sagas/outbox?

**Recommendation (not approved):** Make composed restrictions explicit; preserve rejection of unsupported savepoints rather than promise tracked rollback semantics.

**Affected specifications:** SCHED-006, SCHED-013, SCHED-015, SAGA-011

**Evidence:**
- [src/Clean.Messaging.Scheduling.EntityFrameworkCore/SchedulingTracker.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/SchedulingTracker.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs)
- [src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaSaveInterceptor.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaSaveInterceptor.cs)

## GAP-WF-007

**Issue:** Persisted saga identity equality is collation-dependent
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Must type/key equality be ordinal across supported providers, or are provider collations part of the contract?

**Recommendation (not approved):** Select equality/provider support before unconditional case/Unicode concurrency expectations.

**Affected specifications:** SAGA-004, SAGA-015

**Evidence:**
- [src/Clean.Messaging.Sagas/SagaKey.cs](../../src/Clean.Messaging.Sagas/SagaKey.cs)
- [src/Clean.Messaging.Sagas/Definition/SagaRegistry.cs](../../src/Clean.Messaging.Sagas/Definition/SagaRegistry.cs)
- [src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConfiguration.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConfiguration.cs)

## GAP-WF-008

**Issue:** SagaResult.Fail accepts malformed diagnostics
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Should Fail validate like SagaFailure, and what should null/blank/overlong values do?

**Recommendation (not approved):** Decide validation parity and failure category; do not infer rejection solely from database length annotations.

**Affected specifications:** SAGA-006, SAGA-014, SAGA-015

**Evidence:**
- [src/Clean.Messaging.Sagas/SagaResult.cs](../../src/Clean.Messaging.Sagas/SagaResult.cs)
- [src/Clean.Messaging.Sagas/Persistence/SagaFailure.cs](../../src/Clean.Messaging.Sagas/Persistence/SagaFailure.cs)
- [src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConfiguration.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConfiguration.cs)

## GAP-WF-009

**Issue:** Start collision translation is not unique-error-specific
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** May an unrelated DbUpdateException translate when one added start and a matching winner exist?

**Recommendation (not approved):** Constrain translation to confirmed identity collision or explicitly accept broad heuristic.

**Affected specifications:** SAGA-013, SAGA-015

**Evidence:**
- [src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConflictResolver.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConflictResolver.cs)

## GAP-WF-010

**Issue:** Cancelled collision lookup hides cancellation behind original save error
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Should caller cancellation during conflict lookup propagate, or preserve original DbUpdateException?

**Recommendation (not approved):** Reconcile fallback with normative cancellation distinction and document the selected exception precedence.

**Affected specifications:** SAGA-013, SAGA-014

**Evidence:**
- [src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConflictResolver.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConflictResolver.cs)
- [docs/CODING_STANDARDS.md](../../docs/CODING_STANDARDS.md)

## GAP-WF-011

**Issue:** Saga terminal identities retained indefinitely
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Is indefinite type/key/start tombstone retention and no restart/delete API approved lifecycle intent?

**Recommendation (not approved):** Approve retention/storage policy or specify a separate safe identity-deletion/restart contract; do not silently delete in tests.

**Affected specifications:** SAGA-004, SAGA-017

**Evidence:**
- [src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaStore.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaStore.cs)
- [src/Clean.Messaging.Sagas/Persistence/SagaCleanupWorker.cs](../../src/Clean.Messaging.Sagas/Persistence/SagaCleanupWorker.cs)

## GAP-WF-012

**Issue:** Repeated unsaved starts lack tracked-instance merge
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** What happens contractually when two start messages for the same key run in one scope before SaveChanges?

**Recommendation (not approved):** Define unit-of-work replay/conflict semantics; current query consults persisted state and can propose duplicate additions.

**Affected specifications:** SAGA-005, SAGA-015

**Evidence:**
- [src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaStore.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaStore.cs)
- [src/Clean.Messaging.Sagas/Definition/SagaMessageDefinition.cs](../../src/Clean.Messaging.Sagas/Definition/SagaMessageDefinition.cs)

## GAP-WF-013

**Issue:** Null option objects and numeric upper limits are unspecified
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** What explicit failures apply to null nested scheduling options or null saga serializer options, and extreme otherwise-positive values?

**Recommendation (not approved):** Set validation ownership/diagnostics and operational bounds; no validated null/upper-bound guarantee exists today.

**Affected specifications:** SCHED-005, SCHED-015, SAGA-003

**Evidence:**
- [src/Clean.Messaging.Scheduling/Configuration/SchedulingOptionsValidator.cs](../../src/Clean.Messaging.Scheduling/Configuration/SchedulingOptionsValidator.cs)
- [src/Clean.Messaging.Sagas/Configuration/SagaOptionsValidator.cs](../../src/Clean.Messaging.Sagas/Configuration/SagaOptionsValidator.cs)

## GAP-WF-014

**Issue:** Empty custom outgoing targets can complete schedules without outgoing work
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Is zero-target custom delivery success intended, unlike the default unhandled-message fault?

**Recommendation (not approved):** Choose empty-target semantics before treating successful schedule completion as proof of any downstream obligation.

**Affected specifications:** SCHED-011, SCHED-015

**Evidence:**
- [src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs](../../src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs)
- [src/Clean.Messaging.Scheduling.Outbox/OutboxScheduleDelivery.cs](../../src/Clean.Messaging.Scheduling.Outbox/OutboxScheduleDelivery.cs)

## GAP-WF-015

**Issue:** Retained workflow schema evolution has no migration contract
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Which retained state/message schema changes are supported and who migrates incompatible work?

**Recommendation (not approved):** Keep version concurrency distinct from schema version; require consumer deployment policy or separately specified migration.

**Affected specifications:** SAGA-007, SAGA-015

**Evidence:**
- [src/Clean.Messaging.Sagas/Runtime/SagaStateSerializer.cs](../../src/Clean.Messaging.Sagas/Runtime/SagaStateSerializer.cs)
- [src/Clean.Messaging.Sagas/Runtime/SagaProcessor.cs](../../src/Clean.Messaging.Sagas/Runtime/SagaProcessor.cs)
- [src/Clean.Messaging/Serialization/MessageSerializer.cs](../../src/Clean.Messaging/Serialization/MessageSerializer.cs)

## GAP-TX-001

**Issue:** Stale MQTT outbox package API documentation
**Classification:** Observed behaviour
**Status:** Open.

**Question/decision:** Which current public SPI and migration guidance should replace the historical expected SPI?

**Recommendation (not approved):** Pending API-owner decision: retire/replace historical namespace, SPI and registration example; do not adopt README as normative API.

**Affected specifications:** MQTT-001, MQTT-015

**Evidence:**
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/README.md](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/README.md)
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/DependencyInjection.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/DependencyInjection.cs)
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/MqttOutboxOptions.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/MqttOutboxOptions.cs)

## GAP-TX-002

**Issue:** Synchronous mapping cancellation can become permanent or poison failure
**Classification:** Observed behaviour
**Status:** Open.

**Question/decision:** When a synchronous mapper throws cancellation, should an already-cancelled attempt retain cancellation rather than permanent mapping/poison classification?

**Recommendation (not approved):** Pending cancellation decision distinguishing user-thrown cancellation from attempt shutdown/lease loss; add targeted criteria before changing wrapping.

**Affected specifications:** TRANSPORT-005, TRANSPORT-006, TRANSPORT-008, MQTT-006

**Evidence:**
- [src/Clean.Messaging.Inbox.Ingress/TransportIngress.cs](../../src/Clean.Messaging.Inbox.Ingress/TransportIngress.cs)
- [src/Clean.Messaging.Outbox.Dispatch/TransportDispatch.cs](../../src/Clean.Messaging.Outbox.Dispatch/TransportDispatch.cs)
- [src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs)
- [docs/CODING_STANDARDS.md](../../docs/CODING_STANDARDS.md)

## GAP-TX-003

**Issue:** Retained schema and route compatibility policy is unapproved
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Which contract/schema/map/target changes are supported while old work or independently deployed peers exist?

**Recommendation (not approved):** Record owner-approved compatibility and migration/rejection policy; do not claim negotiation/upcasting.

**Affected specifications:** TRANSPORT-008, MQTT-009, MQTT-015

**Evidence:**
- [src/Clean.Messaging.Outbox.Dispatch/TransportMessageFactory.cs](../../src/Clean.Messaging.Outbox.Dispatch/TransportMessageFactory.cs)
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/Routing/MqttRouteRegistry.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Routing/MqttRouteRegistry.cs)
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttOutboxTransport.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttOutboxTransport.cs)
- [docs/reference/contracts.md](../../docs/reference/contracts.md)

## GAP-TX-004

**Issue:** Transport metadata stops at durable mapping boundary
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Is deliberate loss of arbitrary headers and ContentType intended, or is some propagation required?

**Recommendation (not approved):** Keep loss observed and any round-trip extension pending a concrete ownership/schema decision.

**Affected specifications:** TRANSPORT-004, TRANSPORT-006, MQTT-004, MQTT-010

**Evidence:**
- [src/Clean.Messaging.Inbox.Ingress/InboxMessageFactory.cs](../../src/Clean.Messaging.Inbox.Ingress/InboxMessageFactory.cs)
- [src/Clean.Messaging.Outbox.Dispatch/TransportMessageFactory.cs](../../src/Clean.Messaging.Outbox.Dispatch/TransportMessageFactory.cs)
- [src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs)

## GAP-TX-005

**Issue:** Optional empty GUID metadata is emitted but rejected by peer ingress
**Classification:** Observed behaviour
**Status:** Open.

**Question/decision:** Should empty optional correlation/causation IDs be rejected, omitted, or interpreted as absent consistently?

**Recommendation (not approved):** Pending interoperability decision; add agreed peer scenarios before modifying either side.

**Affected specifications:** MQTT-010, MQTT-015

**Evidence:**
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttMessageFactory.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttMessageFactory.cs)
- [src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs)

## GAP-TX-006

**Issue:** Shared-client concurrency and disposal safety not established
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** What overlapping publish/reconnect/disposal lifecycle is supported for MQTTnet 5.2.0.1603?

**Recommendation (not approved):** Validate with protocol/concurrency integration and narrow documented guarantees; connection lock alone is not publish fencing.

**Affected specifications:** MQTT-011, MQTT-015

**Evidence:**
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/README.md](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/README.md)
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/Connection/MqttConnection.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Connection/MqttConnection.cs)
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttOutboxTransport.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttOutboxTransport.cs)

## GAP-TX-007

**Issue:** Inbox poison acknowledgement and disconnect log wording overstate timing
**Classification:** Observed behaviour
**Status:** Open.

**Question/decision:** Should poison logging distinguish acknowledgement attempted versus confirmed and reconnect versus shutdown cleanup?

**Recommendation (not approved):** Recommend owner-reviewed wording correction with observable failed-ack scenarios; logging alone cannot prove ack success.

**Affected specifications:** MQTT-014

**Evidence:**
- [src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs)

## GAP-TX-008

**Issue:** Route/map cross-registration validation happens at resolution
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Which route/map/contract conflicts must fail at registration, explicit startup, or first scoped registry resolution?

**Recommendation (not approved):** Record validation-boundary decision; preserve current observed exceptions and do not infer startup route-contract readiness.

**Affected specifications:** TRANSPORT-003, MQTT-001, MQTT-015

**Evidence:**
- [src/Clean.Messaging.Inbox.Ingress/TransportIngressRegistry.cs](../../src/Clean.Messaging.Inbox.Ingress/TransportIngressRegistry.cs)
- [src/Clean.Messaging.Outbox.Dispatch/TransportDispatchRegistry.cs](../../src/Clean.Messaging.Outbox.Dispatch/TransportDispatchRegistry.cs)
- [src/Clean.Messaging.Inbox.Ingress.Mqtt/DependencyInjection.cs](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/DependencyInjection.cs)
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/Routing/MqttRouteRegistry.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Routing/MqttRouteRegistry.cs)
- [docs/CODING_STANDARDS.md](../../docs/CODING_STANDARDS.md)

## GAP-TX-009

**Issue:** Inbox shutdown duration and synchronization resource policy not established
**Classification:** Unknown behaviour
**Status:** Open.

**Question/decision:** Should final uncancelled disconnect be bounded and owned worker semaphores explicitly disposed, and how are in-flight callbacks drained?

**Recommendation (not approved):** Pending lifecycle decision and supported shutdown scenarios; no production repair within this audit.

**Affected specifications:** MQTT-008, MQTT-015

**Evidence:**
- [src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs)

## Existing documentation / implementation contradictions

### Discrepancy 1
**Documentation:** docs\reference\inbox.md
**Implementation:** src\Clean.Messaging.Inbox\Runtime\InboxManager.cs; src\Clean.Messaging.Inbox.EntityFrameworkCore\InboxStore.cs
Reference says replay consumer identity syntax is validated. Manager only checks nonblank consumerId, limit/range/contract; replay assigns TargetId without ASCII/length or registration/type compatibility checks. Storage mapping is not equivalent input syntax validation.

### Discrepancy 2
**Documentation:** docs\CODING_STANDARDS.md
**Implementation:** src\Clean.Messaging.Inbox.EntityFrameworkCore\InboxStore.cs
Normative failure transparency conflicts with Replay catching any DbUpdateException and returning reconciled counts even when no new pair exists; unrelated update errors can be hidden as zero additions.

### Discrepancy 3
**Documentation:** docs\CODING_STANDARDS.md
**Implementation:** src\Clean.Messaging.Inbox.EntityFrameworkCore\DependencyInjection.cs; src\Clean.Messaging.Inbox\Runtime\InboxCleanupWorker.cs; src\Clean.Messaging.Outbox.EntityFrameworkCore\DependencyInjection.cs; src\Clean.Messaging.Outbox\Runtime\OutboxCleanupWorker.cs
Normative singleton/scoped lifetime rule conflicts with singleton hosted cleanup constructor dependencies on scoped stores. Scope-validation failure/captive lifetime is suspected from source, not exercised.

### Discrepancy 4
**Documentation:** docs\explanation\delivery-semantics.md#leases-protect-ownership-not-all-execution
**Implementation:** src\Clean.Messaging.Scheduling.EntityFrameworkCore\ScheduleStore.cs; src\Clean.Messaging.Scheduling.EntityFrameworkCore\ScheduleExecutor.cs
Generic shared lease-renewal/settlement discussion is not a scheduling guarantee: scheduling does not renew, dispatch checks expiry at admission, and retry/fail settlement omits expiry. Capability-specific scheduling reference accurately records the narrower behaviour.

### Discrepancy 5
**Documentation:** docs\CODING_STANDARDS.md#7-error-handling-and-validation
**Implementation:** src\Clean.Messaging.Sagas.EntityFrameworkCore\SagaConflictResolver.cs
Normative instruction distinguishes and propagates cancellation; collision winner lookup catches cancellation and retains the preceding DbUpdateException. Exception precedence requires GAP-WF-010 decision; no code changed.

### Discrepancy 6
**Documentation:** src\Clean.Messaging.Outbox.Dispatch.Mqtt\README.md
**Implementation:** src\Clean.Messaging.Outbox.Dispatch.Mqtt\DependencyInjection.cs; src\Clean.Messaging.Outbox.Dispatch.Mqtt\Route.cs; src\Clean.Messaging.Outbox.Dispatch.Mqtt\MqttOutboxOptions.cs
Historical namespace/SPI/registration accepting MQTT client options and options.Retain do not match current callback-based builders, OutboxEnvelope/IOutboxTransport and per-route retain.

### Discrepancy 7
**Documentation:** src\Clean.Messaging.Outbox.Dispatch.Mqtt\README.md
**Implementation:** src\Clean.Messaging.Outbox.Dispatch.Mqtt\Connection\MqttConnection.cs; src\Clean.Messaging.Outbox.Dispatch.Mqtt\Publishing\MqttOutboxTransport.cs
README blanket safety and connect/disconnect versus publishing serialization claim is unsupported: connection semaphore does not gate PublishAsync and outgoing adapter supplies no explicit disconnect.

### Discrepancy 8
**Documentation:** docs\CODING_STANDARDS.md sections 7-9
**Implementation:** src\Clean.Messaging.Inbox.Ingress\TransportIngress.cs; src\Clean.Messaging.Outbox.Dispatch\TransportDispatch.cs; src\Clean.Messaging.Inbox.Ingress.Mqtt\MqttInboxIngressHandler.cs
Normative cancellation preservation conflicts with catch-all synchronous mapping wrapping; ingress wrapped cancellation can become poison and dispatch cancellation permanent mapping failure. Decision remains open rather than changing classification.

## Unexecuted evidence and additional investigation
- Relational locking/isolation, competing claims/starts/cancellations and uncertain commit verification need actual selected providers.
- Cancellation before/after a commit has occurred is not equivalent to rollback; tests must inspect retained truth after uncertainty.
- MQTT acknowledgement/session/reconnect/TLS and broker behavior require real protocol verification; source review is not broker certification.
- Quartz scheduler behavior/fallback and deployment/cluster behavior were not executed.
- Whole-host startup validation, schema/interceptor installation and singleton/scoped composition need integration fixtures.
- Retained contract compatibility, custom serializers/mappers and rolling-deployment payloads require explicitly agreed sample data.
- Null/undefined enum/out-of-range public inputs and malformed mutable model objects need deliberate supported-domain decisions, not fabricated universal validation.
