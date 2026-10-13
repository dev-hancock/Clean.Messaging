# Behavioural gaps and design decisions

**Authority:** Unresolved questions and unapproved recommendations. Source observations do not approve a policy or establish implementation conformance.

## Interpreting the register
Questions distinguish absent intent from suspected implementation defects. Recommendations are proposals, not requirements. Resolve identity/transaction/provider policies explicitly before freezing source observations in acceptance tests. A contradiction may be between normative coding guidance and source, or between descriptive documentation and source; those are different evidence levels.

All entries remain open unless a capability explicitly records a narrower decision.

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

**Question/decision:** Must direct MessageContract/custom invoker registration satisfy and independently validate the same identity constraints as fluent registration?

**Recommendation (not approved):** Decide the supported public registration boundary before specifying failure timing/type for bypassed validation.

**Affected specifications:** CORE-005

**Evidence:**
- [src/Clean.Messaging/Contracts/MessageContractRegistry.cs](../../src/Clean.Messaging/Contracts/MessageContractRegistry.cs)
- [src/Clean.Messaging/Consumers/ConsumerRegistry.cs](../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs)
- [src/Clean.Messaging/DependencyInjection.cs](../../src/Clean.Messaging/DependencyInjection.cs)

### Additional decision considerations

**Disposition category / risk:** C / Medium

**Evidence and risk:** `Clean.Messaging\Contracts\MessageContractRegistry.cs:69–81`; `Consumers\ConsumerRegistry.cs:66–84` omit fluent checks in `Clean.Messaging\DependencyInjection.cs:64–109`.

**Unapproved disposition and acceptance impact:** C6: reuse identity constraints at registry construction; preserve `CONFIG-003-AC-01`, `CONFIG-003-AC-02`, `CONFIG-003-AC-03` and uniqueness outcomes. New direct-registration cases in CORE-005; V1/V2.

## GAP-CORE-002

**Issue:** Ambient metadata scope misuse and child-flow lifetime
**Classification:** Unknown behaviour

**Question/decision:** Are LIFO disposal in the same flow and awaited handler work explicit preconditions? What is promised for out-of-order or child-flow disposal?

**Recommendation (not approved):** Document supported scope use; consider validation only if unsupported misuse must fail predictably.

**Affected specifications:** IDENTITY-005

**Evidence:**
- [src/Clean.Messaging/MessageContext.cs](../../src/Clean.Messaging/MessageContext.cs)

### Additional decision considerations

**Disposition category / risk:** L / Medium

**Evidence and risk:** `Clean.Messaging\MessageContext.cs:18–55`: AsyncLocal capture/restore, one-time Dispose, no flow/LIFO guard.

**Unapproved disposition and acceptance impact:** L1: document existing normal-flow preconditions; no misuse-isolation feature. `IDENTITY-003-AC-02`, `IDENTITY-003-AC-03`, `IDENTITY-003-AC-04` unchanged; new precondition cases if needed; V1.

## GAP-CORE-003

**Issue:** Null nested options and undefined retry backoff
**Classification:** Proposed behaviour

**Question/decision:** Should all null groups and undefined Backoff values be rejected as startup options validation failures?

**Recommendation (not approved):** Approve CONFIG-006 if desired. Currently null groups throw during validation; invalid modes pass retry validation then throw during delay evaluation.

**Affected specifications:** CONFIG-006, RETRY-006

**Evidence:**
- [src/Clean.Messaging/Processing/MessageOptionsValidator.cs](../../src/Clean.Messaging/Processing/MessageOptionsValidator.cs)
- [src/Clean.Messaging/Retry/RetryOptionsValidator.cs](../../src/Clean.Messaging/Retry/RetryOptionsValidator.cs)
- [src/Clean.Messaging/Retry/RetryPolicy.cs](../../src/Clean.Messaging/Retry/RetryPolicy.cs)

### Additional decision considerations

**Disposition category / risk:** C; S / Medium

**Evidence and risk:** `Clean.Messaging\Processing\MessageOptionsValidator.cs:8–17`; `Retry\RetryOptionsValidator.cs:8–36`; `Retry\RetryPolicy.cs:54–66`: null dereferences/undefined mode reaches delay calculation.

**Unapproved disposition and acceptance impact:** C6: startup validation; clarify authority of `CONFIG-006-AC-01`, `CONFIG-006-AC-02`, preserve valid delay rules. No new design approval; V1/V2.

## GAP-CORE-004

**Issue:** Public ownership time/input domain
**Classification:** Unknown behaviour

**Question/decision:** Must Renew reject shortening/past expiry, non-UTC DateTime, empty claim strings or overflowing instants?

**Recommendation (not approved):** Specify normal port preconditions and desired invalid-input outcomes without freezing incidental time-arithmetic exceptions.

**Affected specifications:** PERSIST-003, CLAIM-005

**Evidence:**
- [src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)
- [src/Clean.Messaging/Persistence/IDurableStore.cs](../../src/Clean.Messaging/Persistence/IDurableStore.cs)

### Additional decision considerations

**Disposition category / risk:** C; L / Medium

**Evidence and risk:** `Clean.Messaging.EntityFrameworkCore\DurableStore.cs:26–69,354–368,398–411`: positive claim guards, caller arithmetic/expiry, arbitrary renewal destination.

**Unapproved disposition and acceptance impact:** C6: document UTC/representable instants/nonblank claim and reject unusable expiry; do not infer extension-only semantics or cryptographic token format. Shortening a still-future lease need not become a new feature decision. `PERSIST-003-AC-01`, `PERSIST-003-AC-02`, `CLAIM-003-AC-03` unchanged; add CLAIM-005 edge cases; V1/V3.

## GAP-CORE-005

**Issue:** Provider concurrency and uncertain-commit attribution
**Classification:** Unknown behaviour

**Question/decision:** Which real relational providers and commit-loss/race scenarios must be supported? Are state-only verification predicates sufficient to attribute success to the intended operation?

**Recommendation (not approved):** Select providers and exercise real locking, conditional update, transient strategy and racing verification paths before claiming certification.

**Affected specifications:** PERSIST-004

**Evidence:**
- [src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs)

### Additional decision considerations

**Disposition category / risk:** A; C / High

**Evidence and risk:** `Clean.Messaging.EntityFrameworkCore\DurableStore.cs:371–395`; `DeadLetterStore.cs:120–149`: claim-specific versus state-only verification.

**Unapproved disposition and acceptance impact:** D7 provider scope; C1 attribution correction where evidence is insufficient. PERSIST-004 needs new race/commit-loss cases; existing ownership/atomicity Required criteria unchanged; V3.

## GAP-CORE-006

**Issue:** RetryPolicy direct invalid input domain
**Classification:** Unknown behaviour

**Question/decision:** What should Evaluate do for null exception, nonpositive attempts or undefined action/mode outside processor calls?

**Recommendation (not approved):** Define public preconditions and argument failures; do not convert observed negative/zero delay behavior into a supported policy.

**Affected specifications:** RETRY-006

**Evidence:**
- [src/Clean.Messaging/Retry/RetryPolicy.cs](../../src/Clean.Messaging/Retry/RetryPolicy.cs)

### Additional decision considerations

**Disposition category / risk:** C / Medium

**Evidence and risk:** `Clean.Messaging\Retry\RetryPolicy.cs:18–72`: no null-exception/positive-attempt guards, captures mutable options.

**Unapproved disposition and acceptance impact:** C6: reject direct invalid use and undefined configured modes using existing conventions; no negative-attempt retry policy. RETRY-006 needs new input cases; `RETRY-004-AC-01`, `RETRY-004-AC-02`, `RETRY-004-AC-03` unchanged; V1.

## GAP-CORE-007

**Issue:** Failure logs precede confirmed settlement
**Classification:** Proposed behaviour

**Question/decision:** Should retry/dead-letter logs distinguish intended settlement from confirmed durable success?

**Recommendation (not approved):** Consider wording or post-success outcome logs; no telemetry change is approved. Metrics already count true settlement only.

**Affected specifications:** OPS-005

**Evidence:**
- [src/Clean.Messaging.Inbox/Dispatching/InboxProcessor.cs](../../src/Clean.Messaging.Inbox/Dispatching/InboxProcessor.cs)
- [src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs](../../src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs)

### Additional decision considerations

**Disposition category / risk:** C; S / Medium

**Evidence and risk:** `Clean.Messaging.Inbox\Dispatching\InboxProcessor.cs:201–257`; `Clean.Messaging.Outbox\Runtime\OutboxProcessor.cs:205–271`: logs before boolean settlement, metrics after true.

**Unapproved disposition and acceptance impact:** C7: phase-accurate logs, preserve metrics and `OPS-004-AC-02`; new OPS-005 cases. No telemetry architecture decision; V1.

## GAP-CORE-008

**Issue:** Null invocation and malformed serialization records
**Classification:** Unknown behaviour

**Question/decision:** What failure type/code must a public invoker or deserializer return for null runtime input or default/null MessageData fields?

**Recommendation (not approved):** Direct null invocation currently dereferences message.GetType in its mismatch branch; agree argument versus permanent-message failure and malformed record handling before tests freeze accidental exceptions.

**Affected specifications:** CORE-003, CORE-004, CORE-005

**Evidence:**
- [src/Clean.Messaging/Consumers/ConsumerRegistry.cs](../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs)
- [src/Clean.Messaging/Serialization/MessageSerializer.cs](../../src/Clean.Messaging/Serialization/MessageSerializer.cs)

### Additional decision considerations

**Disposition category / risk:** C / Medium

**Evidence and risk:** `Clean.Messaging\Consumers\ConsumerRegistry.cs:33–48` dereferences null; `Serialization\MessageSerializer.cs:48–76` resolves record type before JSON catch.

**Unapproved disposition and acceptance impact:** C6: deliberate invalid-use guards; retain permanent malformed JSON and missing-contract classifications. `CORE-003-AC-03` explicitly covers nonnull incompatibility; `CORE-004-AC-04` unchanged. New null/default-record cases in CORE-005; V1.

## GAP-CORE-009

**Issue:** Runtime options and serializer reconfiguration
**Classification:** Unknown behaviour

**Question/decision:** Is mutating captured options or serializer options after startup supported, and when must changes become effective?

**Recommendation (not approved):** Specify immutable-after-start preconditions or an explicit reconfiguration protocol; do not infer live reload from mutable properties.

**Affected specifications:** CORE-004, CONFIG-002, RETRY-004

**Evidence:**
- [src/Clean.Messaging/Retry/RetryPolicy.cs](../../src/Clean.Messaging/Retry/RetryPolicy.cs)
- [src/Clean.Messaging/Serialization/MessageSerializer.cs](../../src/Clean.Messaging/Serialization/MessageSerializer.cs)
- [src/Clean.Messaging/MessageOptions.cs](../../src/Clean.Messaging/MessageOptions.cs)

### Additional decision considerations

**Disposition category / risk:** L; S / Medium

**Evidence and risk:** `Clean.Messaging\Retry\RetryPolicy.cs:18–21`; `Serialization\MessageSerializer.cs:14–19`: captured options, no reload protocol.

**Unapproved disposition and acceptance impact:** L1: configure before startup; do not promise supported concurrent mutation. `CONFIG-002-AC-01`, `CORE-004-AC-03`, retry normal outcomes unchanged; preconditions/new snapshot tests if hardening; V1.

## GAP-CORE-010

**Issue:** Resource arithmetic and direct management limit validation
**Classification:** Unknown behaviour

**Question/decision:** Must extreme Capacity plus Concurrency and nonpositive direct DeadLetterStore.Get limits fail predictably at the public boundary?

**Recommendation (not approved):** Consider explicit resource-budget validation; managers guard positive limits but the direct provider port does not uniformly enforce the same boundary.

**Affected specifications:** WORK-004, OPS-001

**Evidence:**
- [src/Clean.Messaging/Processing/WorkLoop.cs](../../src/Clean.Messaging/Processing/WorkLoop.cs)
- [src/Clean.Messaging/Processing/WorkLoopOptions.cs](../../src/Clean.Messaging/Processing/WorkLoopOptions.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs)

### Additional decision considerations

**Disposition category / risk:** C / Medium

**Evidence and risk:** `Clean.Messaging\Processing\WorkLoop.cs:19–35`; `WorkLoopOptions.cs:11–38`; `Clean.Messaging.EntityFrameworkCore\DeadLetterStore.cs:19–27`: unchecked sums/direct Take.

**Unapproved disposition and acceptance impact:** C6: checked representability and positive direct listing limit, no arbitrary budget ceilings. `WORK-004-AC-03`, positive `OPS-001-AC-01` unchanged; new overflow/nonpositive cases; V1.

## GAP-DUR-001

**Issue:** New empty inbox admission and no-consumer policy
**Classification:** Unknown behaviour

**Question/decision:** Should a valid envelope with no consumer entries be retained for future replay, rejected, or governed by an explicit contract?

**Recommendation (not approved):** Decide separately for default typed admission and direct empty batches; do not silently remove the current rejection.

**Affected specifications:** INBOX-001, INBOX-003, INBOX-014

**Evidence:**
- [src/Clean.Messaging.Inbox/Persistence/IInboxStore.cs](../../src/Clean.Messaging.Inbox/Persistence/IInboxStore.cs)
- [src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs)
- [src/Clean.Messaging/Consumers/ConsumerRegistry.cs](../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs)

### Additional decision considerations

**Disposition category / risk:** L / Medium

**Evidence and risk:** `Clean.Messaging.Inbox\Inbox.cs:42–76` → exact registry rejection; `Clean.Messaging.Inbox.EntityFrameworkCore\InboxStore.cs:32–43,239–259` separately rejects new empty direct batch after duplicate check.

**Unapproved disposition and acceptance impact:** L2: retain both rejection policies; `INBOX-001-AC-05`, `INBOX-003-AC-01`, `INBOX-003-AC-02` unchanged. Buffering unhandled messages would be a future requirement, not a guard removal; V1/V3.

## GAP-DUR-002

**Issue:** Replay target syntax and type compatibility
**Classification:** Unknown behaviour

**Question/decision:** Should replay require registered compatible <=200 ASCII target identity, or deliberately permit unresolved targets?

**Recommendation (not approved):** Approve manager validation policy and document deferred dispatch/storage errors; current manager checks blank only.

**Affected specifications:** INBOX-009, INBOX-014

**Evidence:**
- [src/Clean.Messaging.Inbox/Runtime/InboxManager.cs](../../src/Clean.Messaging.Inbox/Runtime/InboxManager.cs)
- [src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs)
- [docs/reference/inbox.md](../reference/inbox.md)

### Additional decision considerations

**Disposition category / risk:** A; C / Medium–High

**Evidence and risk:** `Clean.Messaging.Inbox\Runtime\InboxManager.cs:86–109` lacks syntax/consumer compatibility; target fails later at dispatch/storage.

**Unapproved disposition and acceptance impact:** D5 suitability; C6 syntax consistency. `INBOX-009-AC-05` changes, existing invalid cases in `INBOX-009-AC-03` retained/extended; V1/V3.

## GAP-DUR-003

**Issue:** Replay update errors, count attribution and page progress
**Classification:** Unknown behaviour

**Question/decision:** How should failed nonduplicate saves remain visible, how are concurrent additions attributed, and how does replay progress beyond already-covered oldest pages?

**Recommendation (not approved):** Investigate required failure-transparency mismatch; proposed reconcile-only-confirmed-duplicate rule remains unapproved. Decide counts and continuation separately.

**Affected specifications:** INBOX-009, INBOX-013, INBOX-014, INBOX-015

**Evidence:**
- [src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs)
- [src/Clean.Messaging.Inbox/Runtime/ReplayResult.cs](../../src/Clean.Messaging.Inbox/Runtime/ReplayResult.cs)
- [docs/CODING_STANDARDS.md](../CODING_STANDARDS.md)

### Additional decision considerations

**Disposition category / risk:** C; A; S / High

**Evidence and risk:** `Clean.Messaging.Inbox.EntityFrameworkCore\InboxStore.cs:75–120,178–211`: limit before exclusion, broad catch, reread increase.

**Unapproved disposition and acceptance impact:** C1 transparent failures; D6 bounded progress/results. `INBOX-009-AC-04`, `INBOX-009-AC-06`, `INBOX-009-AC-07` as specified above; `INBOX-013-AC-01` unchanged, INBOX-015 authority clarified; V3.

## GAP-DUR-004

**Issue:** Inbox admission payload retention and replay/dedup horizon
**Classification:** Unknown behaviour

**Question/decision:** Are indefinite admission payload retention and replay after entry cleanup intended, and what horizon governs deletion/duplicate guarantees?

**Recommendation (not approved):** Define admission retention separately from terminal-entry retention and make repeated-effect consequences explicit before adding cleanup.

**Affected specifications:** INBOX-002, INBOX-010, INBOX-014

**Evidence:**
- [src/Clean.Messaging.Inbox/Admission/InboxAdmission.cs](../../src/Clean.Messaging.Inbox/Admission/InboxAdmission.cs)
- [src/Clean.Messaging.Inbox/Runtime/InboxCleanupWorker.cs](../../src/Clean.Messaging.Inbox/Runtime/InboxCleanupWorker.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)

### Additional decision considerations

**Disposition category / risk:** A / High

**Evidence and risk:** `Clean.Messaging.Inbox.EntityFrameworkCore\InboxStore.cs:168–175,225–236`; `Clean.Messaging.EntityFrameworkCore\DurableStore.cs:143–151`: admissions survive target cleanup; replay can recreate.

**Unapproved disposition and acceptance impact:** D3 retention/dedup/replay horizons; `INBOX-010-AC-02` changes only with admission expiry. Existing retained-identity/cleanup criteria preserved; V3.

## GAP-DUR-005

**Issue:** Hosted cleanup captures scoped store
**Classification:** Required behaviour

**Question/decision:** How should default hosted cleanup honor required scoped-service lifetime composition?

**Recommendation (not approved):** Record suspected conformance defect; approve scoped-per-cycle composition only through a separate code change and verify with scope validation.

**Affected specifications:** CONFIG-001, INBOX-012, OUTBOX-017

**Evidence:**
- [src/Clean.Messaging.Inbox/Runtime/InboxCleanupWorker.cs](../../src/Clean.Messaging.Inbox/Runtime/InboxCleanupWorker.cs)
- [src/Clean.Messaging.Inbox.EntityFrameworkCore/DependencyInjection.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/DependencyInjection.cs)
- [src/Clean.Messaging.Outbox/Runtime/OutboxCleanupWorker.cs](../../src/Clean.Messaging.Outbox/Runtime/OutboxCleanupWorker.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/DependencyInjection.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/DependencyInjection.cs)
- [docs/CODING_STANDARDS.md](../CODING_STANDARDS.md)

### Additional decision considerations

**Disposition category / risk:** C / High

**Evidence and risk:** `Clean.Messaging.Inbox.EntityFrameworkCore\DependencyInjection.cs:32–35,55–56`; `Clean.Messaging.Outbox.EntityFrameworkCore\DependencyInjection.cs:58–64,113–116`; cleanup worker constructors retain stores.

**Unapproved disposition and acceptance impact:** C2: safe lifetimes; Required `CONFIG-001-AC-02`, `CONFIG-001-AC-03`, `INBOX-012-AC-01`, `OUTBOX-017-AC-01` unchanged; V2.

## GAP-DUR-006

**Issue:** Default local targets prevent external-only events
**Classification:** Unknown behaviour

**Question/decision:** Should no local consumers be an error when external providers supply work, and should zero-target events be valid?

**Recommendation (not approved):** Decide target provider composition contract without assuming external route registration overrides default Get failure.

**Affected specifications:** OUTBOX-003, OUTBOX-018, DOMAIN-007

**Evidence:**
- [src/Clean.Messaging.Outbox/Transport/OutboxTargetProvider.cs](../../src/Clean.Messaging.Outbox/Transport/OutboxTargetProvider.cs)
- [src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs](../../src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/DependencyInjection.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/DependencyInjection.cs)

### Additional decision considerations

**Disposition category / risk:** A / High

**Evidence and risk:** `Clean.Messaging.Outbox\Transport\OutboxTargetProvider.cs:13–19`; `Persistence\OutboxEntryFactory.cs:32–39`: local throw prevents external-only materialization.

**Unapproved disposition and acceptance impact:** D1: local empty contribution versus aggregate zero policy. `OUTBOX-003-AC-03` and relevant `DOMAIN-007-AC-01` clause change; V3.

## GAP-DUR-007

**Issue:** Tracked versus persisted outgoing duplicate/content conflict
**Classification:** Unknown behaviour

**Question/decision:** Should persisted event/target duplicates suppress, conflict or fail save, and is payload equality required?

**Recommendation (not approved):** Define idempotency scope and retention horizon explicitly; do not claim current tracked-only suppression is database-wide.

**Affected specifications:** OUTBOX-004, OUTBOX-018

**Evidence:**
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxConfiguration.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxConfiguration.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DurableEntryConfiguration.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableEntryConfiguration.cs)

### Additional decision considerations

**Disposition category / risk:** A; L / High

**Evidence and risk:** `Clean.Messaging.Outbox.EntityFrameworkCore\OutboxWriter.cs:25–44`; `Clean.Messaging.EntityFrameworkCore\DurableEntryConfiguration.cs:16–20,51–55`: tracked suppression, durable uniqueness.

**Unapproved disposition and acceptance impact:** D4 conflict semantics; retain persisted duplicate conflict, no prequery-as-idempotency. `OUTBOX-004-AC-01` conflicting subset changes; `OUTBOX-004-AC-02` unchanged; V3.

## GAP-DUR-008

**Issue:** Uncertain commit reconciliation and evidence expiry
**Classification:** Unknown behaviour

**Question/decision:** What recovery contract applies when expected keys are missing/cleaned, verification fails or zero-key captures cannot prove commit?

**Recommendation (not approved):** Define caller/context reconciliation and evidence lifetime, distinguishing unknown from rollback/success; verify database/provider behaviour before strengthening guarantees.

**Affected specifications:** OUTBOX-008, OUTBOX-018, DOMAIN-007, DOMAIN-009

**Evidence:**
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxCommitVerifier.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxCommitVerifier.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs)

### Additional decision considerations

**Disposition category / risk:** C; A / High

**Evidence and risk:** `Clean.Messaging.Outbox.EntityFrameworkCore\OutboxCommitVerifier.cs:69–130`; `OutboxTransactionInterceptor.cs:152–224`; cleanup can remove keys.

**Unapproved disposition and acceptance impact:** C1 unknown rather than unsupported negative inference; D7 verification envelope. Missing-key subset of `OUTBOX-008-AC-02` changes; zero-key `OUTBOX-008-AC-03` unchanged. Actual loss/race unexecuted; V3.

## GAP-DUR-009

**Issue:** Postcommit callback failure and callback-only participation
**Classification:** Unknown behaviour

**Question/decision:** Should callbacks be nonthrowing/isolated/retried, how are postcommit errors reported, and should callbacks alone make pending state nonempty?

**Recommendation (not approved):** Approve callback lifecycle semantics separately; current exceptions can interrupt subsequent callbacks/wake/state cleanup after durable commit.

**Affected specifications:** OUTBOX-010, OUTBOX-018, DOMAIN-008, DOMAIN-010

**Evidence:**
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs)
- [src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs](../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs)

### Additional decision considerations

**Disposition category / risk:** C; L / High

**Evidence and risk:** `Clean.Messaging.Outbox.EntityFrameworkCore\OutboxTracker.cs:79–91,249–260,315–317`; failure catch encompasses verified callbacks.

**Unapproved disposition and acceptance impact:** C4 internal cleanup/error isolation; L4 no callback ledger. `OUTBOX-010-AC-02`, `OUTBOX-010-AC-04`, `DOMAIN-008-AC-02`, `DOMAIN-008-AC-04` revise; callback-only `OUTBOX-010-AC-03` clarify, not universally dropped; V3.

## GAP-DUR-010

**Issue:** Custom event source identity, enumeration and acknowledgement assumptions
**Classification:** Unknown behaviour

**Question/decision:** What stable uniqueness/payload/collection/nonthrowing acknowledgement rules bind custom sources, and how are same-ID different-source collisions reported?

**Recommendation (not approved):** Specify source extension assumptions and observable conflicts before adding validation/locking or treating source order as policy.

**Affected specifications:** DOMAIN-004, DOMAIN-008, DOMAIN-010

**Evidence:**
- [src/Clean.Messaging.DomainEvents/RaisedEvent.cs](../../src/Clean.Messaging.DomainEvents/RaisedEvent.cs)
- [src/Clean.Messaging.DomainEvents/AggregateRoot.cs](../../src/Clean.Messaging.DomainEvents/AggregateRoot.cs)
- [src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs](../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs)

### Additional decision considerations

**Disposition category / risk:** A; C; L / High

**Evidence and risk:** `Clean.Messaging.DomainEvents.EntityFrameworkCore\DomainEventCapture.cs:17–21,35–55`; `Clean.Messaging.DomainEvents\RaisedEvent.cs:3–12`: custom enumeration/IDs and first-source capture.

**Unapproved disposition and acceptance impact:** D4 collision meaning; C6 reject empty/null invalid facts; L4 single-owner enumeration and source-owned ack. `DOMAIN-008-AC-01` changes if collision rejection approved; `DOMAIN-004-AC-04`, `DOMAIN-008-AC-03` otherwise unchanged; V1/V3.

## GAP-DUR-011

**Issue:** Targetless captured fact acknowledgement meaning
**Classification:** Unknown behaviour

**Question/decision:** Should a successful observed capture commit with zero outgoing rows acknowledge the source event?

**Recommendation (not approved):** Choose reject/retain/acknowledge explicitly; never equate observed targetless acknowledgement with downstream delivery.

**Affected specifications:** DOMAIN-007, DOMAIN-010, OUTBOX-018

**Evidence:**
- [src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs](../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs)

### Additional decision considerations

**Disposition category / risk:** A / High

**Evidence and risk:** `Clean.Messaging.DomainEvents.EntityFrameworkCore\DomainEventCapture.cs:37–55`; `Clean.Messaging.Outbox.EntityFrameworkCore\OutboxWriter.cs:20–23`; tracker captures can participate with zero keys.

**Unapproved disposition and acceptance impact:** D1: choose targetless capture meaning, never equate ack with delivery. `DOMAIN-007-AC-02` changes under rejection; new DOMAIN-010/OUTBOX-018 cases; V3.

## GAP-WF-001

**Issue:** Default value objects bypass admission constraints
**Classification:** Unknown behaviour

**Question/decision:** Must default target and default correlation key be rejected at the writer/runtime boundary?

**Recommendation (not approved):** Choose explicit boundary validation and compatibility treatment; constructors alone do not establish this guarantee.

**Affected specifications:** SCHED-003, SCHED-015, SAGA-004, SAGA-015

**Evidence:**
- [src/Clean.Messaging.Scheduling/ScheduleTarget.cs](../../src/Clean.Messaging.Scheduling/ScheduleTarget.cs)
- [src/Clean.Messaging.Scheduling/Runtime/ScheduleWriter.cs](../../src/Clean.Messaging.Scheduling/Runtime/ScheduleWriter.cs)
- [src/Clean.Messaging.Sagas/SagaKey.cs](../../src/Clean.Messaging.Sagas/SagaKey.cs)
- [src/Clean.Messaging.Sagas/Runtime/SagaProcessor.cs](../../src/Clean.Messaging.Sagas/Runtime/SagaProcessor.cs)

### Additional decision considerations

**Disposition category / risk:** C / Medium

**Evidence and risk:** `Clean.Messaging.Scheduling\ScheduleTarget.cs:5–18`; `Runtime\ScheduleWriter.cs:12–44`; `Clean.Messaging.Sagas\SagaKey.cs:7–19`; `Definition\SagaMessageDefinition.cs:58–78`: default structs bypass constructor rules.

**Unapproved disposition and acceptance impact:** C6: reject default target/correlation before staging/lookup/handler. Constructor `SCHED-003-AC-03`, `SAGA-004-AC-01` unchanged; new SCHED-015/SAGA-015 boundary cases; V1.

## GAP-WF-002

**Issue:** Stable schedule identity does not imply duplicate insert success
**Classification:** Unknown behaviour

**Question/decision:** Is an equal retry supposed to be idempotent admission, or must the owner retry/recognize a failed unit of work?

**Recommendation (not approved):** Document the approved duplicate-insert result without inventing upsert or allowing changed payload to replace retained work.

**Affected specifications:** SCHED-001, SCHED-006

**Evidence:**
- [src/Clean.Messaging.Scheduling/IMessageScheduler.cs](../../src/Clean.Messaging.Scheduling/IMessageScheduler.cs)
- [src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleStore.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleStore.cs)

### Additional decision considerations

**Disposition category / risk:** L; S / Medium

**Evidence and risk:** `Clean.Messaging.Scheduling.EntityFrameworkCore\ScheduleStore.cs:12–17`; `SchedulingConfiguration.cs:19–28`: Add plus primary key, no upsert.

**Unapproved disposition and acceptance impact:** L2: retained duplicate conflict; stable identity not successful repeated insertion. `SCHED-006-AC-04`, `SCHED-006-AC-05` unchanged; V1/V3.

## GAP-WF-003

**Issue:** Preleased backlog and absent schedule renewal
**Classification:** Unknown behaviour

**Question/decision:** Should queued/long-running schedules renew or lease only when admitted, and what bounds are operationally required?

**Recommendation (not approved):** Decide queue/renewal policy before approving compatibility tests; do not borrow shared inbox/outbox renewal guarantees.

**Affected specifications:** SCHED-009, SCHED-010

**Evidence:**
- [src/Clean.Messaging.Scheduling/Runtime/SchedulingWorker.cs](../../src/Clean.Messaging.Scheduling/Runtime/SchedulingWorker.cs)
- [src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleStore.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleStore.cs)

### Additional decision considerations

**Disposition category / risk:** C / High

**Evidence and risk:** `Clean.Messaging.Scheduling\Runtime\SchedulingWorker.cs:53–82`; `Clean.Messaging.Scheduling.EntityFrameworkCore\ScheduleStore.cs:303–343`: prelease before local slots.

**Unapproved disposition and acceptance impact:** C5: immediately usable capacity (current batch loop waits for the batch), no renewal initially. Rescope setups in `SCHED-009-AC-04`, `SCHED-009-AC-07`; claim-based attempt/local concurrency outcomes unchanged; new SCHED-010 cases; V1/V3.

## GAP-WF-004

**Issue:** Schedule expiry differs between dispatch and settlement
**Classification:** Unknown behaviour

**Question/decision:** Is token-only expired failure settlement and no final lease check on successful execution deliberate?

**Recommendation (not approved):** Choose admission versus completion fencing; test expiry/reclaim races with real provider transactions.

**Affected specifications:** SCHED-010, SCHED-012

**Evidence:**
- [src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleExecutor.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleExecutor.cs)
- [src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleStore.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/ScheduleStore.cs)

### Additional decision considerations

**Disposition category / risk:** C; A / High

**Evidence and risk:** `Clean.Messaging.Scheduling.EntityFrameworkCore\ScheduleExecutor.cs:30–83,87–111`; `ScheduleStore.cs:380–391`: expiry admission/update transaction, no final gate; token-only failure settle.

**Unapproved disposition and acceptance impact:** C5 expired Retry/Fail correction; D2 successful lock-held completion. New SCHED-010 expiry/gate cases; valid `SCHED-012-AC-01`, `SCHED-012-AC-02`, `SCHED-012-AC-03` retained. No proven successor acquisition while lock held; V3.

## GAP-WF-005

**Issue:** Quartz fallback waits for scheduler I/O
**Classification:** Unknown behaviour

**Question/decision:** Must local wake/deadline remain responsive when ScheduleJob hangs, rather than only throws?

**Recommendation (not approved):** Define a bounded scheduler-I/O/fallback policy and host shutdown assumptions; current adapter has no timeout or reconnect loop.

**Affected specifications:** SCHED-014, SCHED-015

**Evidence:**
- [src/Clean.Messaging.Scheduling.Quartz/SchedulingEngine.cs](../../src/Clean.Messaging.Scheduling.Quartz/SchedulingEngine.cs)

### Additional decision considerations

**Disposition category / risk:** C / High

**Evidence and risk:** `Clean.Messaging.Scheduling.Quartz\SchedulingEngine.cs:39–108`: scheduler I/O precedes local wait.

**Unapproved disposition and acceptance impact:** C5: responsive local wake/deadline with bounded outstanding scheduler work; normal `SCHED-014-AC-03` unchanged; new SCHED-015 hung-I/O case. No forced termination of noncooperative dependency; V5.

## GAP-WF-006

**Issue:** Caller savepoints and uncertain commit ownership
**Classification:** Unknown behaviour

**Question/decision:** Which caller savepoint and uncertain-commit patterns are supported across composed schedules/sagas/outbox?

**Recommendation (not approved):** Make composed restrictions explicit; preserve rejection of unsupported savepoints rather than promise tracked rollback semantics.

**Affected specifications:** SCHED-006, SCHED-013, SCHED-015, SAGA-011

**Evidence:**
- [src/Clean.Messaging.Scheduling.EntityFrameworkCore/SchedulingTracker.cs](../../src/Clean.Messaging.Scheduling.EntityFrameworkCore/SchedulingTracker.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs)
- [src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaSaveInterceptor.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaSaveInterceptor.cs)

### Additional decision considerations

**Disposition category / risk:** L; S / High conditional

**Evidence and risk:** `Clean.Messaging.Scheduling.EntityFrameworkCore\SchedulingTracker.cs:14–147`; `Clean.Messaging.Outbox.EntityFrameworkCore\OutboxTransactionInterceptor.cs:104–150,227–235`; saga hooks can throw.

**Unapproved disposition and acceptance impact:** L3 composed savepoint restrictions/fresh context after uncertainty; C1 uncertainty handling. `SAGA-011-AC-06`, `SCHED-006-AC-06`, `SCHED-013-AC-06` unchanged; scheduling-only guard would need new SCHED-015 case; V3.

## GAP-WF-007

**Issue:** Persisted saga identity equality is collation-dependent
**Classification:** Unknown behaviour

**Question/decision:** Must type/key equality be ordinal across supported providers, or are provider collations part of the contract?

**Recommendation (not approved):** Select equality/provider support before unconditional case/Unicode concurrency expectations.

**Affected specifications:** SAGA-004, SAGA-015

**Evidence:**
- [src/Clean.Messaging.Sagas/SagaKey.cs](../../src/Clean.Messaging.Sagas/SagaKey.cs)
- [src/Clean.Messaging.Sagas/Definition/SagaRegistry.cs](../../src/Clean.Messaging.Sagas/Definition/SagaRegistry.cs)
- [src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConfiguration.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConfiguration.cs)

### Additional decision considerations

**Disposition category / risk:** A; S / High

**Evidence and risk:** `Clean.Messaging.Sagas\Definition\SagaRegistry.cs:16–43`; `Clean.Messaging.Sagas.EntityFrameworkCore\SagaStore.cs:11–22`; `SagaConfiguration.cs:31–41,60–66`: ordinal registry, SQL equality, no collation configured.

**Unapproved disposition and acceptance impact:** D8 persisted equality; no silent normalization. `SAGA-004-AC-01`, `SAGA-004-AC-03` unchanged; new SAGA-015 provider/schema criteria; V3.

## GAP-WF-008

**Issue:** SagaResult.Fail accepts malformed diagnostics
**Classification:** Unknown behaviour

**Question/decision:** Should Fail validate like SagaFailure, and what should null/blank/overlong values do?

**Recommendation (not approved):** Decide validation parity and failure category; do not infer rejection solely from database length annotations.

**Affected specifications:** SAGA-006, SAGA-014, SAGA-015

**Evidence:**
- [src/Clean.Messaging.Sagas/SagaResult.cs](../../src/Clean.Messaging.Sagas/SagaResult.cs)
- [src/Clean.Messaging.Sagas/Persistence/SagaFailure.cs](../../src/Clean.Messaging.Sagas/Persistence/SagaFailure.cs)
- [src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConfiguration.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConfiguration.cs)

### Additional decision considerations

**Disposition category / risk:** C / Medium

**Evidence and risk:** `Clean.Messaging.Sagas\SagaResult.cs:42–65,93–101` versus `Persistence\SagaFailure.cs:5–28`: raw Fail bypasses established diagnostics validation.

**Unapproved disposition and acceptance impact:** C6 reuse validation; valid `SAGA-006-AC-01` unchanged; new SAGA-014 malformed Fail cases, reference rather than duplicate SAGA-015 question; V1.

## GAP-WF-009

**Issue:** Start collision translation is not unique-error-specific
**Classification:** Unknown behaviour

**Question/decision:** May an unrelated DbUpdateException translate when one added start and a matching winner exist?

**Recommendation (not approved):** Constrain translation to confirmed identity collision or explicitly accept broad heuristic.

**Affected specifications:** SAGA-013, SAGA-015

**Evidence:**
- [src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConflictResolver.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConflictResolver.cs)

### Additional decision considerations

**Disposition category / risk:** C / High

**Evidence and risk:** `Clean.Messaging.Sagas.EntityFrameworkCore\SagaConflictResolver.cs:13–46,95–144`: any qualifying DbUpdateException plus winner can translate.

**Unapproved disposition and acceptance impact:** C1: require positive identity-collision evidence, preserve unrelated original failure even with incidental winner. True `SAGA-013-AC-01`, `SAGA-013-AC-02`, `SAGA-013-AC-03`, `SAGA-013-AC-05` unchanged with strengthened collision preconditions; new unrelated-error case; V1/V3.

## GAP-WF-010

**Issue:** Cancelled collision lookup hides cancellation behind original save error
**Classification:** Unknown behaviour

**Question/decision:** Should caller cancellation during conflict lookup propagate, or preserve original DbUpdateException?

**Recommendation (not approved):** Reconcile fallback with normative cancellation distinction and document the selected exception precedence.

**Affected specifications:** SAGA-013, SAGA-014

**Evidence:**
- [src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConflictResolver.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaConflictResolver.cs)
- [docs/CODING_STANDARDS.md](../CODING_STANDARDS.md)

### Additional decision considerations

**Disposition category / risk:** C / Medium–High

**Evidence and risk:** `Clean.Messaging.Sagas.EntityFrameworkCore\SagaConflictResolver.cs:70–92`: token passed, cancellation caught.

**Unapproved disposition and acceptance impact:** C3: cancelled-lookup subset of `SAGA-013-AC-04` changes to caller cancellation; unrelated lookup errors retain original failure. New SAGA-014 case; V1/V3.

## GAP-WF-011

**Issue:** Saga terminal identities retained indefinitely
**Classification:** Unknown behaviour

**Question/decision:** Is indefinite type/key/start tombstone retention and no restart/delete API approved lifecycle intent?

**Recommendation (not approved):** Approve retention/storage policy or specify a separate safe identity-deletion/restart contract; do not silently delete in tests.

**Affected specifications:** SAGA-004, SAGA-017

**Evidence:**
- [src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaStore.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaStore.cs)
- [src/Clean.Messaging.Sagas/Persistence/SagaCleanupWorker.cs](../../src/Clean.Messaging.Sagas/Persistence/SagaCleanupWorker.cs)

### Additional decision considerations

**Disposition category / risk:** L / Medium

**Evidence and risk:** `Clean.Messaging.Sagas.EntityFrameworkCore\SagaStore.cs:81–104`; `Clean.Messaging.Sagas\Persistence\SagaCleanupWorker.cs:46–61`: purge payload, retain identity/diagnostics.

**Unapproved disposition and acceptance impact:** L3 retain tombstones; no automatic restart/delete contract. `SAGA-004-AC-05`, `SAGA-017-AC-02` unchanged; V3.

## GAP-WF-012

**Issue:** Repeated unsaved starts lack tracked-instance merge
**Classification:** Unknown behaviour

**Question/decision:** What happens contractually when two start messages for the same key run in one scope before SaveChanges?

**Recommendation (not approved):** Define unit-of-work replay/conflict semantics; current query consults persisted state and can propose duplicate additions.

**Affected specifications:** SAGA-005, SAGA-015

**Evidence:**
- [src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaStore.cs](../../src/Clean.Messaging.Sagas.EntityFrameworkCore/SagaStore.cs)
- [src/Clean.Messaging.Sagas/Definition/SagaMessageDefinition.cs](../../src/Clean.Messaging.Sagas/Definition/SagaMessageDefinition.cs)

### Additional decision considerations

**Disposition category / risk:** C; L / High

**Evidence and risk:** `Clean.Messaging.Sagas.EntityFrameworkCore\SagaStore.cs:11–22`; `Clean.Messaging.Sagas\Runtime\SagaRuntime.cs:38–63`; `Definition\SagaMessageDefinition.cs:65–110`: persisted lookup can stage second unsaved start and invoke handler.

**Unapproved disposition and acceptance impact:** C6 reject a second pending start before handler/effects; no tracked merge or invented idempotent ack. Add no-pending-start precondition to `SAGA-005-AC-01`; persisted replay/conflict `SAGA-005-AC-02`, `SAGA-005-AC-03`, `SAGA-005-AC-04` unchanged. New SAGA-015 rejection case; V3.

## GAP-WF-013

**Issue:** Null option objects and numeric upper limits are unspecified
**Classification:** Unknown behaviour

**Question/decision:** What explicit failures apply to null nested scheduling options or null saga serializer options, and extreme otherwise-positive values?

**Recommendation (not approved):** Set validation ownership/diagnostics and operational bounds; no validated null/upper-bound guarantee exists today.

**Affected specifications:** SCHED-005, SCHED-015, SAGA-003

**Evidence:**
- [src/Clean.Messaging.Scheduling/Configuration/SchedulingOptionsValidator.cs](../../src/Clean.Messaging.Scheduling/Configuration/SchedulingOptionsValidator.cs)
- [src/Clean.Messaging.Sagas/Configuration/SagaOptionsValidator.cs](../../src/Clean.Messaging.Sagas/Configuration/SagaOptionsValidator.cs)

### Additional decision considerations

**Disposition category / risk:** C; S / Medium

**Evidence and risk:** `Clean.Messaging.Scheduling\Configuration\SchedulingOptionsValidator.cs:8–72`; `Clean.Messaging.Sagas\Configuration\SagaOptionsValidator.cs:8–32`; `Runtime\SagaStateSerializer.cs:12–23,34–44`: null groups unsafe; null JSON options use dependency defaults, not necessarily NRE.

**Unapproved disposition and acceptance impact:** C6 explicit nonnullable-options validation/actual timer/date bounds, no arbitrary ceilings. Existing `SCHED-005-AC-01`, `SAGA-003-AC-03` positive validation unchanged; new null/extreme cases; V1/V2.

## GAP-WF-014

**Issue:** Empty custom outgoing targets can complete schedules without outgoing work
**Classification:** Unknown behaviour

**Question/decision:** Is zero-target custom delivery success intended, unlike the default unhandled-message fault?

**Recommendation (not approved):** Choose empty-target semantics before treating successful schedule completion as proof of any downstream obligation.

**Affected specifications:** SCHED-011, SCHED-015

**Evidence:**
- [src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs](../../src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs)
- [src/Clean.Messaging.Scheduling.Outbox/OutboxScheduleDelivery.cs](../../src/Clean.Messaging.Scheduling.Outbox/OutboxScheduleDelivery.cs)

### Additional decision considerations

**Disposition category / risk:** A / High conditional

**Evidence and risk:** `Clean.Messaging.Scheduling.Outbox\OutboxScheduleDelivery.cs:24–40,60–71`; outbox factory/writer zero-target no-op can let schedule complete.

**Unapproved disposition and acceptance impact:** D1 aggregate target policy. Normal `SCHED-011-AC-02`, `SCHED-011-AC-04` unchanged with nonempty-target setup; new SCHED-015 empty-target case and `OUTBOX-003-AC-04` factory revision; V3.

## GAP-WF-015

**Issue:** Retained workflow schema evolution has no migration contract
**Classification:** Unknown behaviour

**Question/decision:** Which retained state/message schema changes are supported and who migrates incompatible work?

**Recommendation (not approved):** Keep version concurrency distinct from schema version; require consumer deployment policy or separately specified migration.

**Affected specifications:** SAGA-007, SAGA-015

**Evidence:**
- [src/Clean.Messaging.Sagas/Runtime/SagaStateSerializer.cs](../../src/Clean.Messaging.Sagas/Runtime/SagaStateSerializer.cs)
- [src/Clean.Messaging.Sagas/Runtime/SagaProcessor.cs](../../src/Clean.Messaging.Sagas/Runtime/SagaProcessor.cs)
- [src/Clean.Messaging/Serialization/MessageSerializer.cs](../../src/Clean.Messaging/Serialization/MessageSerializer.cs)

### Additional decision considerations

**Disposition category / risk:** L; S / High deployment

**Evidence and risk:** `Clean.Messaging.Sagas\Runtime\SagaStateSerializer.cs:12–56`; `Persistence\SagaEntry.cs:39–54`; `Clean.Messaging\Serialization\MessageSerializer.cs:48–76`: no schema migration; Version is concurrency.

**Unapproved disposition and acceptance impact:** L5 host/application compatibility; no migration framework requirement. `SAGA-007-AC-03` unchanged; new ownership/old-fixture cases, not automatic upgrade guarantees; V1/V3.

## GAP-TX-001

**Issue:** Stale MQTT outbox package API documentation
**Classification:** Observed behaviour

**Question/decision:** Which current public SPI and migration guidance should replace the historical expected SPI?

**Recommendation (not approved):** Pending API-owner decision: retire/replace historical namespace, SPI and registration example; do not adopt README as normative API.

**Affected specifications:** MQTT-001, MQTT-015

**Evidence:**
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/README.md](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/README.md)
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/DependencyInjection.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/DependencyInjection.cs)
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/MqttOutboxOptions.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/MqttOutboxOptions.cs)

### Additional decision considerations

**Disposition category / risk:** S / Medium

**Evidence and risk:** `Clean.Messaging.Outbox.Dispatch.Mqtt\README.md:1–53,77` versus `DependencyInjection.cs:13–54`, `MqttOutboxBuilder.cs:35–74`: stale SPI/options/sample.

**Unapproved disposition and acceptance impact:** Mechanical current-API correction; no speculative historical migration. `MQTT-001-AC-01`, `MQTT-001-AC-02`, `MQTT-001-AC-03` unchanged; documentation symbol/signature checks.

## GAP-TX-002

**Issue:** Synchronous mapping cancellation can become permanent or poison failure
**Classification:** Observed behaviour

**Question/decision:** When a synchronous mapper throws cancellation, should an already-cancelled attempt retain cancellation rather than permanent mapping/poison classification?

**Recommendation (not approved):** Pending cancellation decision distinguishing user-thrown cancellation from attempt shutdown/lease loss; add targeted criteria before changing wrapping.

**Affected specifications:** TRANSPORT-005, TRANSPORT-006, TRANSPORT-008, MQTT-006

**Evidence:**
- [src/Clean.Messaging.Inbox.Ingress/TransportIngress.cs](../../src/Clean.Messaging.Inbox.Ingress/TransportIngress.cs)
- [src/Clean.Messaging.Outbox.Dispatch/TransportDispatch.cs](../../src/Clean.Messaging.Outbox.Dispatch/TransportDispatch.cs)
- [src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs)
- [docs/CODING_STANDARDS.md](../CODING_STANDARDS.md)

### Additional decision considerations

**Disposition category / risk:** C / High

**Evidence and risk:** `Clean.Messaging.Inbox.Ingress\TransportIngress.cs:40–65`; `Clean.Messaging.Outbox.Dispatch\TransportDispatch.cs:37–61`; MQTT handler `:30–40`: wrapping crosses cancellation/poison boundary.

**Unapproved disposition and acceptance impact:** C3 token-attributed cancellation; exact changed/retained ACs listed in C3. No arbitrary-mapper OCE equals shutdown rule; V1/V4.

## GAP-TX-003

**Issue:** Retained schema and route compatibility policy is unapproved
**Classification:** Unknown behaviour

**Question/decision:** Which contract/schema/map/target changes are supported while old work or independently deployed peers exist?

**Recommendation (not approved):** Record owner-approved compatibility and migration/rejection policy; do not claim negotiation/upcasting.

**Affected specifications:** TRANSPORT-008, MQTT-009, MQTT-015

**Evidence:**
- [src/Clean.Messaging.Outbox.Dispatch/TransportMessageFactory.cs](../../src/Clean.Messaging.Outbox.Dispatch/TransportMessageFactory.cs)
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/Routing/MqttRouteRegistry.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Routing/MqttRouteRegistry.cs)
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttOutboxTransport.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttOutboxTransport.cs)
- [docs/reference/contracts.md](../reference/contracts.md)

### Additional decision considerations

**Disposition category / risk:** L; S / High deployment

**Evidence and risk:** `Clean.Messaging.Outbox.Dispatch.Mqtt\Routing\MqttOutboxTargets.cs:20–37`; `Publishing\MqttOutboxTransport.cs:22–36`; `Clean.Messaging.Outbox.Dispatch\TransportDispatch.cs:39–49,64–78`: stored destination/current mapper.

**Unapproved disposition and acceptance impact:** L5 host-owned retained compatibility/current route policy. `MQTT-009-AC-04`, `MQTT-012-AC-04` unchanged with explicit compatibility preconditions; V1/V3/V4.

## GAP-TX-004

**Issue:** Transport metadata stops at durable mapping boundary
**Classification:** Unknown behaviour

**Question/decision:** Is deliberate loss of arbitrary headers and ContentType intended, or is some propagation required?

**Recommendation (not approved):** Keep loss observed and any round-trip extension pending a concrete ownership/schema decision.

**Affected specifications:** TRANSPORT-004, TRANSPORT-006, MQTT-004, MQTT-010

**Evidence:**
- [src/Clean.Messaging.Inbox.Ingress/InboxMessageFactory.cs](../../src/Clean.Messaging.Inbox.Ingress/InboxMessageFactory.cs)
- [src/Clean.Messaging.Outbox.Dispatch/TransportMessageFactory.cs](../../src/Clean.Messaging.Outbox.Dispatch/TransportMessageFactory.cs)
- [src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs)

### Additional decision considerations

**Disposition category / risk:** L / Medium

**Evidence and risk:** `Clean.Messaging.Inbox.Ingress.Mqtt\MqttInboxIngressHandler.cs:77–87`; generic inbox/outgoing factories carry IDs, not arbitrary metadata.

**Unapproved disposition and acceptance impact:** L5 accept documented metadata boundary; no new headers schema absent requirement. `TRANSPORT-004-AC-04`, `TRANSPORT-006-AC-01`, `MQTT-004-AC-05` unchanged; V1/V4.

## GAP-TX-005

**Issue:** Optional empty GUID metadata is emitted but rejected by peer ingress
**Classification:** Observed behaviour

**Question/decision:** Should empty optional correlation/causation IDs be rejected, omitted, or interpreted as absent consistently?

**Recommendation (not approved):** Pending interoperability decision; add agreed peer scenarios before modifying either side.

**Affected specifications:** MQTT-010, MQTT-015

**Evidence:**
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttMessageFactory.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttMessageFactory.cs)
- [src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs)

### Additional decision considerations

**Disposition category / risk:** C / High conditional

**Evidence and risk:** `Clean.Messaging.Outbox.Dispatch.Mqtt\Publishing\MqttMessageFactory.cs:56–70` versus ingress handler `:129–152`: empty optional GUID emitted then rejected.

**Unapproved disposition and acceptance impact:** C7 recommend omit empty optional IDs; do not weaken ingress. `MQTT-010-AC-04` changes, `MQTT-004-AC-03` unchanged; V4 peer roundtrip.

## GAP-TX-006

**Issue:** Shared-client concurrency and disposal safety not established
**Classification:** Unknown behaviour

**Question/decision:** What overlapping publish/reconnect/disposal lifecycle is supported for MQTTnet 5.2.0.1603?

**Recommendation (not approved):** Validate with protocol/concurrency integration and narrow documented guarantees; connection lock alone is not publish fencing.

**Affected specifications:** MQTT-011, MQTT-015

**Evidence:**
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/README.md](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/README.md)
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/Connection/MqttConnection.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Connection/MqttConnection.cs)
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttOutboxTransport.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttOutboxTransport.cs)

### Additional decision considerations

**Disposition category / risk:** L; C; S / Medium

**Evidence and risk:** `Clean.Messaging.Outbox.Dispatch.Mqtt\Connection\MqttConnection.cs:17–49,79–83`; transport `:29–40`: connection gate, ungated publication, immediate disposal.

**Unapproved disposition and acceptance impact:** L6 qualify concurrency; C5 safe lifetime/drain. No missing-lock inference or global publication gate. Normal `MQTT-011-AC-01`, `MQTT-011-AC-02`, `MQTT-011-AC-03` unchanged; drain-expanded `MQTT-011-AC-04` only if needed; V4.

## GAP-TX-007

**Issue:** Inbox poison acknowledgement and disconnect log wording overstate timing
**Classification:** Observed behaviour

**Question/decision:** Should poison logging distinguish acknowledgement attempted versus confirmed and reconnect versus shutdown cleanup?

**Recommendation (not approved):** Recommend owner-reviewed wording correction with observable failed-ack scenarios; logging alone cannot prove ack success.

**Affected specifications:** MQTT-014

**Evidence:**
- [src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs)

### Additional decision considerations

**Disposition category / risk:** C; S / Medium

**Evidence and risk:** `Clean.Messaging.Inbox.Ingress.Mqtt\Runtime\MqttInboxWorker.cs:323–378,112–125,425–458`: ack/admission/disconnect logs overstate/mislabel phases.

**Unapproved disposition and acceptance impact:** C7 revise `MQTT-014-AC-01`, `MQTT-014-AC-02`; committed admission/recovery `MQTT-006-AC-01` unchanged. V4 failed/cancelled ack after admission.

## GAP-TX-008

**Issue:** Route/map cross-registration validation happens at resolution
**Classification:** Unknown behaviour

**Question/decision:** Which route/map/contract conflicts must fail at registration, explicit startup, or first scoped registry resolution?

**Recommendation (not approved):** Record validation-boundary decision; preserve current observed exceptions and do not infer startup route-contract readiness.

**Affected specifications:** TRANSPORT-003, MQTT-001, MQTT-015

**Evidence:**
- [src/Clean.Messaging.Inbox.Ingress/TransportIngressRegistry.cs](../../src/Clean.Messaging.Inbox.Ingress/TransportIngressRegistry.cs)
- [src/Clean.Messaging.Outbox.Dispatch/TransportDispatchRegistry.cs](../../src/Clean.Messaging.Outbox.Dispatch/TransportDispatchRegistry.cs)
- [src/Clean.Messaging.Inbox.Ingress.Mqtt/DependencyInjection.cs](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/DependencyInjection.cs)
- [src/Clean.Messaging.Outbox.Dispatch.Mqtt/Routing/MqttRouteRegistry.cs](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Routing/MqttRouteRegistry.cs)
- [docs/CODING_STANDARDS.md](../CODING_STANDARDS.md)

### Additional decision considerations

**Disposition category / risk:** C / High

**Evidence and risk:** Ingress/dispatch registries resolve conflicts at construction; MQTT handler `:47–48` resolves payload contract on first receive; repeated adapter registrations can conflict.

**Unapproved disposition and acceptance impact:** C6 assembled startup checks; change timing in `TRANSPORT-003-AC-03`, missing-config clause in `MQTT-004-AC-02`. `TRANSPORT-003-AC-04` still does not promise durable readiness from mapping-only registration; retained-work runtime failure `MQTT-012-AC-04` unchanged; V2.

## GAP-TX-009

**Issue:** Inbox shutdown duration and synchronization resource policy not established
**Classification:** Unknown behaviour

**Question/decision:** Should final uncancelled disconnect be bounded and owned worker semaphores explicitly disposed, and how are in-flight callbacks drained?

**Recommendation (not approved):** Pending lifecycle decision and supported shutdown scenarios; no production repair within this audit.

**Affected specifications:** MQTT-008, MQTT-015

**Evidence:**
- [src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs)

### Additional decision considerations

**Disposition category / risk:** C / High

**Evidence and risk:** `Clean.Messaging.Inbox.Ingress.Mqtt\Runtime\MqttInboxWorker.cs:24–28,43–77,267–301`: callback unsubscribe then final disconnect, no whole-drain budget/owned semaphore disposal.

**Unapproved disposition and acceptance impact:** C5 bounded shutdown and safe callback/resource ownership; dependency network timeouts do not bound every internal task/callback. Revise `MQTT-008-AC-04`; normal shutdown cancellation `MQTT-006-AC-02` unchanged. New MQTT-015 lifecycle cases; V4.

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
Normative instruction distinguishes and propagates cancellation; collision winner lookup catches cancellation and retains the preceding DbUpdateException. Exception precedence requires the GAP-WF-010 decision.

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

## Decision evidence and verification boundaries
- Relational locking/isolation, competing claims/starts/cancellations and uncertain commit verification need actual selected providers.
- Cancellation before/after a commit has occurred is not equivalent to rollback; tests must inspect retained truth after uncertainty.
- MQTT acknowledgement/session/reconnect/TLS and broker behavior require real protocol verification; source review is not broker certification.
- Quartz scheduler behavior/fallback and deployment/cluster behavior require integration verification.
- Whole-host startup validation, schema/interceptor installation and singleton/scoped composition need integration fixtures.
- Retained contract compatibility, custom serializers/mappers and rolling-deployment payloads require explicitly agreed sample data.
- Null/undefined enum/out-of-range public inputs and malformed mutable model objects need deliberate supported-domain decisions, not fabricated universal validation.

## Interpreting the additional proposals

C denotes a suspected correctness defect, A an architectural/product decision, L an unapproved limitation candidate, and S a documentation/specification issue. High risk concerns durability, transactions, ownership or availability; medium risk concerns boundary, API or operations. These labels do not replace each gap?s original behavioural classification. Source ranges are lookup evidence, not runtime results; paths in additional considerations are relative to `src\`. C1?C7, D1?D8, L1?L6 and V1?V5 below explain the references in those considerations. All recommendations remain unapproved; standards-derived requirements retain their existing authority.

## Shared correction proposals (C1-C7)

| Group | Confirmed behaviour / evidence limit | Recommended technical resolution and contract basis | Acceptance impact | Verification |
| --- | --- | --- | --- | --- |
| C1 — Failure and commit truth | Replay catches any `DbUpdateException` and can return zero additions. Missing outbox keys select rollback despite possible evidence deletion. State-only dead-letter/requeue/discard verification cannot uniquely attribute a result. Commit-loss attribution requires provider evidence. | Preserve unreconciled errors; do not infer rollback from inconclusive absence or claim request-owned success from another operation's state. Use fresh-context recovery when the unit of work is uncertain. Standards §7–8 and INBOX-013 already determine failure transparency; stronger provider certification is D7. | Revise `INBOX-009-AC-07` and missing-evidence branch of `OUTBOX-008-AC-02`; retain `INBOX-013-AC-01`, `OUTBOX-008-AC-03`, `PERSIST-002-AC-01`, `PERSIST-002-AC-02`, `PERSIST-002-AC-03`. `INBOX-015-AC-01` duplicates the required transparency outcome: recommend authority clarification, not silent approval. Add uncertain-attribution cases to PERSIST-004. | V3: nonduplicate replay failure, partial concurrent coverage, lost commit response, cleaned/partial keys and competing management operations; independently inspect durable truth. |
| C2 — Cleanup lifetimes | Default singleton hosted cleanup workers retain scoped stores. The store's own nested scope does not fix constructor capture. | Resolve operation-scoped dependencies without singleton capture. Standards §6 already prohibit this; scope-per-cycle is an implementation recommendation, not the requirement. | Required `CONFIG-001-AC-02`, `CONFIG-001-AC-03`, `INBOX-012-AC-01`, `OUTBOX-017-AC-01` unchanged. | V2: validated host startup and two cleanup cycles with independent dependency disposal. |
| C3 — Cancellation classification | Synchronous mapping wrapping can turn a cancelled attempt into ingress poison/permanent dispatch failure. Saga collision lookup catches cancellation and preserves the preceding database error. | Preserve cooperative cancellation when the relevant attempt/caller token is cancelled; retain diagnostics and ordinary mapping/error classification for an arbitrary mapper OCE with an active token. Keep bounded postfailure settlement/commit verification distinct. Outgoing synchronous dispatch has no token today: solve attribution at a token-aware boundary with API compatibility assessed, not by unconditional OCE rethrow. Standards §7 already establish the outcome. | Revise `TRANSPORT-005-AC-05`, `MQTT-006-AC-04`, cancelled-lookup subset of `SAGA-013-AC-04`; qualify throwing-mapper wording in `TRANSPORT-005-AC-01`, `TRANSPORT-006-AC-02`. Preserve `RETRY-005-AC-03`, `MQTT-006-AC-02`, `MQTT-013-AC-03`. New cancellation cases, not invented Unknown-block IDs. | V1/V3/V4: cancelled versus active token, no poison ack/ordinary settlement for cancellation, preserved original cause for unrelated failure. |
| C4 — Postcommit bookkeeping | Throwing acknowledgement stops later callbacks/wake/state removal; a broad reconciliation catch can label callback failure after positively verified commit as unknown. | Separate commit verification from callback failure, report callback errors, continue independent callbacks where possible, and finalize owned tracker state despite callback failure. This is safe bookkeeping/failure transparency, not a durable callback-delivery guarantee. No callback retry ledger or transaction rollback is promised. | Revise interruption/ambiguity in `OUTBOX-010-AC-02`, `OUTBOX-010-AC-04`, `DOMAIN-008-AC-02`, `DOMAIN-008-AC-04`; normal `OUTBOX-010-AC-01` unchanged. Callback-only participation remains L4. | V3: throwing first/middle callback on normal and verified-commit paths; inspect rows, pending source IDs, later callbacks, recovery and subsequent context use. |
| C5 — Ownership and bounded work | Scheduling preclaims a batch before execution slots; Retry/Fail accepts an expired unreplaced token after dispatch rollback. Quartz local wait starts only after scheduler I/O. Whole MQTT shutdown/drain is not bounded by this library. | Claim immediately usable capacity, fence expired failure settlement, keep local wake/deadline responsive independently of optional scheduler I/O, and bound owned shutdown/drain work. No new scheduling renewal API or global publication lock is justified. Successful lock-held schedule completion has a separate policy question, D2. Standards §8–9/§16 establish ownership/resource obligations. | Rescope queued setups in `SCHED-009-AC-04`, `SCHED-009-AC-07` without changing claim-based attempts/local concurrency; add expiry/hung-dependency cases. Revise lifecycle wording in `MQTT-008-AC-04`; valid `SCHED-012-AC-01`, `SCHED-012-AC-02`, `SCHED-012-AC-03` remain, with unexpired ownership explicit. | V1/V3/V4/V5: blocked delivery and dependencies, controlled clock, expired unreplaced/replaced tokens, finite shutdown budget, no disposal while callbacks can still use owned synchronization. |
| C6 — Boundary and configuration correctness | Direct registrations bypass fluent identity checks; null groups/default structs/invalid modes escape intended validation; route/map conflicts emerge at resolution/first work; unchecked resource/date arithmetic can fail incidentally. | Reuse established identity/diagnostic constraints at supported public boundaries, reject invalid use before side effects, validate assembled startup configuration, and check actual representability. Do not invent arbitrary upper limits or a new validation framework. Freeze configured state after startup rather than infer live reload. | `CONFIG-003-AC-01`, `CONFIG-003-AC-02`, `CONFIG-003-AC-03`, `WORK-004-AC-03` unchanged; recommend CONFIG-006 authority clarification. Revise startup timing in `TRANSPORT-003-AC-03` and missing-config clause of `MQTT-004-AC-02`. New malformed/default/extreme-input criteria required. | V1/V2: exact boundaries, default/null/undefined values, failed startup before message processing, no persistence/handler invocation after rejection. |
| C7 — Honest protocol and telemetry outcomes | Retry/dead-letter logs precede successful boolean settlement; poison log precedes ack; disconnect wording also applies to reconnect cleanup. Optional empty outgoing GUIDs are rejected by peer ingress. | Use intent/attempt versus confirmed-result wording; preserve true-settlement-only metrics. Keep strict inbound IDs; recommend treating empty *optional* output GUIDs as absent, preserving meaningful IDs and preventing unusable peer packets. Assess this representation change explicitly in the patch; no need to ask for an arbitrary new metadata architecture. | New OPS-005 log cases; preserve `OPS-004-AC-02`. Revise `MQTT-014-AC-01`, `MQTT-014-AC-02`, `MQTT-010-AC-04`; preserve `MQTT-004-AC-03`. | V1/V4: settlement false/throw/timeout; admission committed but ack failed; emitted empty/absent/valid optional IDs accepted by peer. |

## Architectural/product decisions requiring approval

These are proposals, not selected policies. Risk includes compatibility and operations, not just proven data loss. Each question can be answered independently. Unknown blocks named below have **no existing ACs**; new criteria must be assigned only when drafting the agreed contract.

### D1 — May a message-producing operation succeed with no delivery targets? — High

**Question:** Should custom target selection be allowed to consume an event or schedule without creating any outgoing obligation?

**Consequences:** Today the default local provider can block external-only delivery, while custom zero-target selection can acknowledge a domain fact or complete a schedule with no outgoing rows. These are distinct paths, not proof of publication.

**Recommendation:** Allow the local provider to contribute no targets when external providers supply work, but reject an aggregate zero-target result for message-producing capture/enqueue/scheduling. Retain the source fact or leave the schedule unfinished through the normal failed-unit/retry boundary. A direct low-level empty `Stage` may remain an inert no-op. This prevents accidental disappearance without introducing a pending-event subsystem.

**Alternatives:** Preserve explicit custom no-op success and document it; or add a separately specified deferred-routing facility. Neither is downstream delivery.

**AC impact:** Change `OUTBOX-003-AC-03`, factory-zero-target portion of `OUTBOX-003-AC-04`, default-local-miss portion of `DOMAIN-007-AC-01`, and targetless acknowledgement in `DOMAIN-007-AC-02`. Normal schedule effects in `SCHED-011-AC-02`, `SCHED-011-AC-04` remain with nonempty-target preconditions. `OUTBOX-003-AC-01` does **not** already require nonempty targets. Add aggregate/provider-composition cases in OUTBOX-018/DOMAIN-010/SCHED-015. V3 verifies save/rollback/source acknowledgement and zero/one external targets together.

### D2 — When does a running schedule lose authority to complete? — High

**Question:** If its lease expires during a transaction that already admitted execution and holds the row's write lock, may the schedule still commit?

**Consequences:** Admission checks token and expiry and writes Version inside the execution transaction; completion has no final expiry check. A successor is not proven able to reclaim while that lock is held. Expired failure settlement after rollback is a separate C5 correction, not this decision.

**Recommendation:** Require a token-plus-unexpired-lease completion gate after delivery and before committing participating local effects. Rejection rolls them back. Define authority at the **gate instant**, not an impossible guarantee that expiry cannot occur between gate and physical commit. Use capacity-aware claiming; do not add renewal initially.

**Alternatives:** Treat the admitted write transaction as authority until completion/rollback, explicitly accepting completion after wall-clock lease expiry; or specify renewal for long-running work as a separate extension. External effects can repeat under every option.

**AC impact:** Add completion/expired-token criteria to SCHED-010; normal `SCHED-011-AC-01`, `SCHED-011-AC-05`, `SAGA-012-AC-05` remain for valid authority. Do not claim existing success criteria prove final expiry fencing. V3 must observe actual provider locking, then separately test rollback, expiry and late settlement.

### D3 — How long must inbox admissions retain replayable payload and identity? — High

**Question:** Is indefinite retained inbox history acceptable, or must it expire under a separately defined lifecycle policy?

**Consequences:** Terminal target cleanup does not remove admission payloads. Retained admissions still suppress duplicate Accept, but explicit replay can recreate a cleaned target and repeat application effects. Deleting admissions would also change the deduplication horizon.

**Recommendation:** Preserve existing retention until a concrete data-retention requirement supplies separate payload, identity and replay horizons. Document the storage/data-governance cost now; do not silently couple admission deletion to target cleanup.

**Alternatives:** Finite admissions with explicitly expiring dedup/replay; or payload expiry with retained identity tombstones and a restricted replay contract. The latter needs additional durable state and should not be introduced speculatively.

**AC impact:** Finite admission deletion changes `INBOX-010-AC-02`; preserve retained-identity preconditions in `INBOX-002-AC-03`, `INBOX-002-AC-05`, and target cleanup in `INBOX-010-AC-01`, `OPS-003-AC-05`. New expiry/race criteria in INBOX-014 only if policy changes. V3 uses controlled cutoff and Accept/replay/cleanup races.

### D4 — What does reusing an outgoing/custom-source event identity mean? — High

**Question:** Must reused identity denote exactly the same immutable fact, and should conflicting reuse fail rather than silently select the first source/entry?

**Consequences:** Tracked outgoing pairs suppress later content without comparison; fresh-context duplicates can fail the owning save. Custom sources sharing an event ID can suppress later capture/acknowledgement. Globally successful duplicate admission would require a stronger reconciliation contract.

**Recommendation:** Require stable unique custom-source event IDs within the capture boundary; reject conflicting reuse before owning commit, keep persisted duplicate conflicts rather than add a general upsert/idempotent-insert facility. Preserve legitimate repeated capture of the same fact in one unit of work. Specify which contract/payload/target/route fields constitute that fact before adding equality checks.

**Alternatives:** Preserve first-wins tracked semantics as an explicit limitation; or support equal durable duplicates as success with race-safe conflict detection and owning-state atomicity. Do not overwrite retained work.

**AC impact:** Conflicting tracked reuse changes `OUTBOX-004-AC-01`; cross-source rejection changes `DOMAIN-008-AC-01`. Retaining persisted conflict leaves `OUTBOX-004-AC-02` unchanged; `OUTBOX-004-AC-03` changes only if deletion/replacement horizons change. Existing normal repeated-capture `OUTBOX-006-AC-01` remains. New equality/conflict cases in OUTBOX-018/DOMAIN-010. V3 covers equal/unequal, tracked/persisted, sibling targets, cleaned evidence and owner-state rollback.

### D5 — Must replay targets be usable in the current host? — Medium–High

**Question:** Should replay reject unregistered or type-incompatible consumers, or intentionally stage obligations for a future deployment?

**Consequences:** Current replay validates only nonblank identity/date/limit/contract, so incompatible work can fail later. Syntax consistency is C6, not an optional product choice. Deferred registration is a separate capability choice.

**Recommendation:** Require a currently registered compatible consumer at the application-facing manager boundary. Keep direct provider ports governed by explicit trusted-input preconditions. This fails early without inventing deferred-consumer lifecycle policy.

**Alternative:** Allow deferred registrations with a documented recovery/deployment responsibility; still reject malformed identity syntax.

**AC impact:** Change compatibility/syntax outcome of `INBOX-009-AC-05`; extend `INBOX-009-AC-03` invalid cases without changing its existing outcomes. Preserve envelope/nonreset guarantees in `INBOX-009-AC-01`, `INBOX-009-AC-02`. New compatibility scenarios in INBOX-014. V1/V3 cover invalid, absent, incompatible and compatible consumers.

### D6 — What progress and result does bounded replay promise? — Medium–High

**Question:** Is replay a bounded window scan, or must repeated use reliably reach later uncovered history, and does its count mean request-owned inserts or observed coverage?

**Consequences:** The oldest limited window is selected before target exclusion, with no continuation; repeated calls can stay on already-covered history. Reconciled counts can include another caller's inserts. Neither progress nor request attribution is established by failure transparency alone.

**Recommendation:** Keep a bounded scanned-history limit and add stable continuation (timestamp plus identity, including ties). State separate selected-history and confirmed coverage/addition semantics; do not promise request-owned insertion counts without operation-specific evidence. This avoids an unbounded search for missing targets and makes no-op pages navigable.

**Alternatives:** Select uncovered targets first and define the limit as additions, with a bounded scan strategy; or preserve the current window semantics and require callers to advance explicit ranges. Request-owned counts need stronger attribution than current rereads.

**AC impact:** Continuation changes `INBOX-009-AC-04`; clarify `INBOX-009-AC-06` count semantics while preserving uniqueness. `INBOX-009-AC-02` remains if the limit still counts scanned admissions; change it only for uncovered-first limits. New tie/progress/result criteria in INBOX-014; `INBOX-009-AC-07` failure correction is independent. V3 covers more than one page, equal timestamps, existing terminal targets and concurrent callers.

### D7 — Which relational providers are supported and verified? — High certification risk

**Question:** Which concrete provider/version/configuration combinations must pass the durable concurrency and commit-loss contract before support is claimed?

**Consequences:** EF relational APIs are not certification of locking, execution strategies, unique errors, uncertain commits or callback ordering. State-only success predicates and generic heuristics need tests against the selected provider; actual incorrect attribution is still a hypothesis.

**Recommendation:** Select the deployment-required provider matrix first; certify the existing ownership/atomicity/failure contracts on it. Keep provider-specific mechanics inside EF integration. No provider can be chosen from repository evidence alone, and no generic provider framework is required.

**Alternatives:** Explicitly offer an unverified relational integration with host qualification; or commit to multiple provider families with the corresponding test/support cost.

**AC impact:** Add provider/configuration preconditions and race/fault criteria in PERSIST-004. Do not weaken `CLAIM-002-AC-01`, `CLAIM-002-AC-02`, `CLAIM-002-AC-03`, or `PERSIST-002-AC-01`, `PERSIST-002-AC-02`, `PERSIST-002-AC-03`. V3 is mandatory; EF InMemory is insufficient.

### D8 — Who defines persisted saga identity equality? — High compatibility risk

**Question:** Must saga type/key equality be ordinal across providers, or is the host's explicitly chosen database collation part of its identity contract?

**Consequences:** Registry type comparison is ordinal; SQL lookup/unique keys use database equality without library collation configuration. Case/Unicode aliases depend on the actual schema. Changing equality can require migration and collide existing identities.

**Recommendation:** Keep exact public text and host-owned persisted equality; require coherent lookup and uniqueness under the selected schema and document its collation. Do not normalize identities silently or inject provider rules into neutral packages.

**Alternative:** Require cross-provider ordinal equality, implemented and migrated in each supported EF provider/schema configuration.

**AC impact:** Existing constructor/type-key outcomes in `SAGA-004-AC-01`, `SAGA-004-AC-03` remain; add explicit persisted-equality/race criteria in SAGA-015. V3 tests case/Unicode and concurrent uniqueness on the approved schema.

## Limitation candidates (L1-L6; not approved)

| ID | Recommended disposition | Contract and verification |
| --- | --- | --- |
| L1 — Supported scopes and fixed configuration | Preserve LIFO disposal in the originating logical flow, await owned work, and configure before startup. No fire-and-forget isolation, out-of-order restoration or live-reload protocol is promised. | `IDENTITY-003-AC-02`, `IDENTITY-003-AC-03`, `IDENTITY-003-AC-04`, `CORE-004-AC-03`, `CONFIG-002-AC-01` unchanged. Add usage preconditions, not guarantees for misuse. V1 checks normal nesting/parallel awaited flows. |
| L2 — Admission and schedule insertion | Retain rejection of new direct empty inbox batches and typed no-consumer admission; these are different paths. Retain schedule duplicate conflict, not upsert. A requested future buffering/idempotent-admission feature would need new policy. | `INBOX-003-AC-01`, `INBOX-003-AC-02`, no-consumer portion of `INBOX-001-AC-05`, `SCHED-006-AC-04`, `SCHED-006-AC-05` unchanged. V1/V3 distinguish retained empty duplicate, typed registry failure and database insertion race. |
| L3 — Transactions and terminal identity | Keep composed caller savepoint restrictions and discard failed/uncertain contexts rather than promise tracked-state rewind. Retain saga tombstones after state purge; no restart/delete API inferred. | `SAGA-011-AC-06`, `SCHED-006-AC-06`, `SAGA-004-AC-05`, `SAGA-017-AC-02` unchanged. Scheduling-only savepoint hardening would need a new explicit case, not a claimed existing guard. V3 checks composed hooks, retained identity and terminal replay. |
| L4 — Custom sources/callbacks | Stable enumeration and single-owner mutation are supported preconditions; custom acknowledgement must respect the source's fact ownership. No automatic removal verification, durable callback ledger or callback-only scheduling guarantee. C4 repairs internal bookkeeping independently. | `DOMAIN-004-AC-04`, `DOMAIN-008-AC-03`, `OUTBOX-010-AC-03` unchanged unless stronger support is requested. Callback-only callbacks can run on successful commit: their omission from pending participation does not mean they are universally dropped. V3 exercises successful/failed callback-only participation. |
| L5 — Deployment and metadata | Host/application owns retained schema/state compatibility and migrations. Saga Version is concurrency, not schema version. Retained MQTT destination is fixed, current route/map/retain settings apply later. Arbitrary ingress headers/content type are not durable metadata. | `SAGA-007-AC-03`, `MQTT-009-AC-04`, `MQTT-012-AC-04`, `TRANSPORT-004-AC-04`, `TRANSPORT-006-AC-01`, `MQTT-004-AC-05` unchanged; compatible-map preconditions must be explicit. V1/V3/V4 use actual previous-deployment fixtures. No migration framework or header store inferred. |
| L6 — Client concurrency | Do not add a global MQTT publish gate because the adapter lacks one. MQTTnet dependency packet synchronization is not universal publish/reconnect/disposal certification; qualify lifecycle with tests and gate only lifetime transitions as needed. | Normal `MQTT-011-AC-01`, `MQTT-011-AC-02`, `MQTT-011-AC-03` unchanged. `MQTT-011-AC-04` changes only if outgoing drain support is expanded. V4 overlaps publish/reconnect/disposal; neither blanket safety nor blanket unsafety is asserted. |

## Unresolved specification authority and wording questions

These editing proposals do not change specification authority or acceptance outcomes. Behavioural policy changes remain tied to D1-D8.

| Issue | Mechanical improvement or explicit limitation |
| --- | --- |
| Authority confused with observation | CONFIG-006 and INBOX-015 make standards-derived validation/transparency conditional on approval. Recommend recording their existing authority and retaining IDs, rather than asking for a new architecture. Do not silently promote Observed criteria. |
| Duplicate requirements/projections | Repeated identity, per-target completion, cancellation and external-repeat coverage occurs across shared/capability specifications and generated matrices. Keep shared authority, capability-specific scenarios and stable links; update projections from one source rather than delete traceability. INBOX-013/INBOX-015 overlap on failure transparency. |
| Contradictory defect criteria | `INBOX-009-AC-07` describes swallowed failure against Required `INBOX-013-AC-01`. `MQTT-014` title says logs do not overstate while its body records misleading logs. Label these as defect evidence, not desired regression outcomes. |
| Implementation details as requirements | Rewrite `MQTT-008-AC-04`'s mandated `CancellationToken.None` into bounded observable lifecycle behaviour; independent commit verification in `OUTBOX-008-AC-04` is a different consistency boundary. Rewrite `OUTBOX-010-AC-03`'s internal `PendingCommit` test into supported participation outcomes, and startup timing in `TRANSPORT-003-AC-03` without mandating dictionaries. Keep legitimate provider-extension boundaries/interceptor prerequisites. |
| Unverifiable or vague criteria | Replace “no claim is made” (`OPS-004-AC-04`) and evidence-only wording (`MQTT-014-AC-01`) with observable telemetry/ack outcomes. Specify fixtures for “unsupported shape/type” in `TRANSPORT-002-AC-02`; supply operational/fairness preconditions and controlled-clock bounds for eventual recovery in `INBOX-007-AC-02`. Do not introduce universal deadlines/exactly-once effects. |
| Gap-register discrepancies 1–8 | 1: correct inbox reference's claimed replay syntax validation while C6/D5 are pending. 2: C1 failure correction. 3: C2 lifetime correction. 4: qualify shared lease explanation: scheduling currently has no renewal. 5: C3 cancellation correction, not a new policy. 6–7: replace stale MQTT package API examples and unsupported blanket thread-safety/disconnect claims with current source facts. 8: C3 mapping correction. |
| Unknown blocks and missing cases | CORE-005, IDENTITY-005, CLAIM-005, RETRY-006, PERSIST-004, OPS-005, INBOX-014, OUTBOX-018, DOMAIN-010, SCHED-010, SCHED-015, SAGA-014, SAGA-015, TRANSPORT-008 and MQTT-015 names identify discussion locations, not invented ACs. Check each block's actual classification; some contain Proposed/Observed questions but no ACs. Assign new IDs only when the new criteria are drafted. |

## Evidence boundaries for the proposals (V1?V5)

The following boundaries identify evidence needed to resolve the questions; they are not test-execution status or implementation approval.

- **V1 — Boundary/unit:** recording collaborators, exact 200/201 and 500/501 limits, null/default/undefined inputs, controlled clocks/tasks and observable handler/source/log outcomes. Avoid asserting incidental private call sequences.
- **V2 — Host integration:** ValidateScopes/ValidateOnBuild, options resolution, assembled route/map/contract validation, startup and independent scope disposal. Provider/model startup does not certify installed schema.
- **V3 — Real relational/fault integration:** selected provider/version/schema/isolation; barriers rather than sleeps; competing owners/starts/duplicates; explicit/implicit transactions, rollback and lost commit responses. Read rows and participating business state through a fresh context. Never substitute EF InMemory for locking/uniqueness certification.
- **V4 — MQTT protocol/lifecycle:** controllable broker/client failure points plus real-peer interoperability; ack failures after durable admission, QoS/session redelivery, publish/reconnect/disposal and blocked callbacks. Dependency network timeouts are not proof of a bound for the entire drain. Never infer subscriber effects from publish success.
- **V5 — Quartz adapter:** controlled scheduler calls, wake signals and clock; throwing, cancelled and never-completing I/O. Assert responsive local fallback and bounded outstanding attempts, not forced cancellation of arbitrary code.
