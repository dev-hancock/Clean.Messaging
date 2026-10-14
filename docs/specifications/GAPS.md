# Behavioural gaps and design decisions

**Authority:** Unresolved questions and unapproved recommendations. Source observations do not approve a policy or establish implementation conformance.

## Interpreting the register
Questions distinguish absent intent from suspected implementation defects. Recommendations are proposals, not requirements. Resolve identity/transaction/provider policies explicitly before freezing source observations in acceptance tests. A contradiction may be between normative coding guidance and source, or between descriptive documentation and source; those are different evidence levels.

All entries remain open unless a capability explicitly records a narrower decision.

| Gap | Classification | Decision | Affected specifications |
| --- | --- | --- | --- |
| [GAP-CORE-001](#gap-core-001) | Unknown behaviour | Validation bypass through public registration records | CONFIG-003, CORE-002, CORE-005 |
| [GAP-CORE-002](#gap-core-002) | Unknown behaviour | Ambient metadata scope misuse and child-flow lifetime | IDENTITY-003, IDENTITY-005 |
| [GAP-CORE-003](#gap-core-003) | Proposed behaviour | Null nested options and undefined retry backoff | CONFIG-004, CONFIG-006, RETRY-004, RETRY-006 |
| [GAP-CORE-004](#gap-core-004) | Unknown behaviour | Public ownership time/input domain | CLAIM-003, CLAIM-005, PERSIST-003 |
| [GAP-CORE-005](#gap-core-005) | Unknown behaviour | Provider concurrency and uncertain-commit attribution | CLAIM-001, CLAIM-002, OPS-001, PERSIST-001, PERSIST-002, PERSIST-004, RETRY-005, WORK-001 |
| [GAP-CORE-006](#gap-core-006) | Unknown behaviour | RetryPolicy direct invalid input domain | RETRY-003, RETRY-006 |
| [GAP-CORE-007](#gap-core-007) | Proposed behaviour | Failure logs precede confirmed settlement | OPS-004, OPS-005 |
| [GAP-CORE-008](#gap-core-008) | Unknown behaviour | Null invocation and malformed serialization records | CORE-003, CORE-004, CORE-005 |
| [GAP-CORE-009](#gap-core-009) | Unknown behaviour | Runtime options and serializer reconfiguration | CONFIG-002, CORE-004, RETRY-004 |
| [GAP-CORE-010](#gap-core-010) | Unknown behaviour | Resource arithmetic and direct management limit validation | WORK-004, OPS-001 |
| [GAP-CLAIM-001](#gap-claim-001) | Unknown behaviour | Attempt accounting, abandoned work and budget exhaustion | CLAIM-001, IDENTITY-001, INBOX-004, INBOX-005, INBOX-006, INBOX-007, MQTT-012, OUTBOX-011, OUTBOX-013, OUTBOX-014, RETRY-002, RETRY-003, RETRY-005, SCHED-009, SCHED-012, WORK-001 |
| [GAP-CLAIM-002](#gap-claim-002) | Unknown behaviour | Due-time eligibility and lease deadline semantics | CLAIM-001, CLAIM-002, CLAIM-003, INBOX-004, OUTBOX-013, SCHED-009, SCHED-011 |
| [GAP-DUR-001](#gap-dur-001) | Unknown behaviour | New empty inbox admission and no-consumer policy | INBOX-001, INBOX-003, INBOX-014, MQTT-006 |
| [GAP-DUR-002](#gap-dur-002) | Unknown behaviour | Replay target syntax and type compatibility | INBOX-009, INBOX-014 |
| [GAP-DUR-003](#gap-dur-003) | Unknown behaviour | Replay update errors, count attribution and page progress | INBOX-009, INBOX-013, INBOX-014, INBOX-015 |
| [GAP-DUR-004](#gap-dur-004) | Unknown behaviour | Inbox admission payload retention and replay/dedup horizon | INBOX-002, INBOX-005, INBOX-010, INBOX-014, OPS-003 |
| [GAP-DUR-005](#gap-dur-005) | Required behaviour | Hosted cleanup captures scoped store | CONFIG-001, INBOX-012, OUTBOX-017 |
| [GAP-DUR-006](#gap-dur-006) | Unknown behaviour | Default local targets prevent external-only events | DOMAIN-007, OUTBOX-003, OUTBOX-018 |
| [GAP-DUR-007](#gap-dur-007) | Unknown behaviour | Tracked versus persisted outgoing duplicate/content conflict | INBOX-002, INBOX-014, OUTBOX-004, OUTBOX-018, TRANSPORT-005 |
| [GAP-DUR-008](#gap-dur-008) | Unknown behaviour | Uncertain commit reconciliation and evidence expiry | DOMAIN-005, DOMAIN-007, DOMAIN-009, INBOX-005, OUTBOX-008, OUTBOX-018 |
| [GAP-DUR-009](#gap-dur-009) | Unknown behaviour | Postcommit callback failure and callback-only participation | DOMAIN-005, DOMAIN-008, DOMAIN-010, OUTBOX-006, OUTBOX-010, OUTBOX-018 |
| [GAP-DUR-010](#gap-dur-010) | Unknown behaviour | Custom event source identity, enumeration and acknowledgement assumptions | DOMAIN-001, DOMAIN-004, DOMAIN-007, DOMAIN-008, DOMAIN-010 |
| [GAP-DUR-011](#gap-dur-011) | Unknown behaviour | Targetless captured fact acknowledgement meaning | DOMAIN-007, DOMAIN-010, OUTBOX-018 |
| [GAP-WF-001](#gap-wf-001) | Unknown behaviour | Default value objects bypass admission constraints | SAGA-004, SAGA-015, SCHED-003, SCHED-015 |
| [GAP-WF-002](#gap-wf-002) | Unknown behaviour | Stable schedule identity does not imply duplicate insert success | SAGA-009, SCHED-001, SCHED-006 |
| [GAP-WF-003](#gap-wf-003) | Unknown behaviour | Preleased backlog and absent schedule renewal | SCHED-009, SCHED-010, WORK-004 |
| [GAP-WF-004](#gap-wf-004) | Unknown behaviour | Schedule expiry differs between dispatch and settlement | SCHED-009, SCHED-010, SCHED-011, SCHED-012 |
| [GAP-WF-005](#gap-wf-005) | Unknown behaviour | Quartz fallback waits for scheduler I/O | SCHED-014, SCHED-015 |
| [GAP-WF-006](#gap-wf-006) | Unknown behaviour | Caller savepoints and uncertain commit ownership | SAGA-011, SCHED-006, SCHED-013, SCHED-015 |
| [GAP-WF-007](#gap-wf-007) | Unknown behaviour | Persisted saga identity equality is collation-dependent | SAGA-004, SAGA-015 |
| [GAP-WF-008](#gap-wf-008) | Unknown behaviour | SagaResult.Fail accepts malformed diagnostics | SAGA-006, SAGA-014, SAGA-015 |
| [GAP-WF-009](#gap-wf-009) | Unknown behaviour | Start collision translation is not unique-error-specific | SAGA-013, SAGA-015 |
| [GAP-WF-010](#gap-wf-010) | Unknown behaviour | Cancelled collision lookup hides cancellation behind original save error | SAGA-013, SAGA-014 |
| [GAP-WF-011](#gap-wf-011) | Unknown behaviour | Saga terminal identities retained indefinitely | SAGA-004, SAGA-017 |
| [GAP-WF-012](#gap-wf-012) | Unknown behaviour | Repeated unsaved starts lack tracked-instance merge | SAGA-005, SAGA-015 |
| [GAP-WF-013](#gap-wf-013) | Unknown behaviour | Null option objects and numeric upper limits are unspecified | SAGA-003, SCHED-005, SCHED-015 |
| [GAP-WF-014](#gap-wf-014) | Unknown behaviour | Empty custom outgoing targets can complete schedules without outgoing work | SCHED-011, SCHED-015 |
| [GAP-WF-015](#gap-wf-015) | Unknown behaviour | Retained workflow schema evolution has no migration contract | SAGA-007, SAGA-015 |
| [GAP-TX-001](#gap-tx-001) | Observed behaviour | Stale MQTT outbox package API documentation | MQTT-001, MQTT-015 |
| [GAP-TX-002](#gap-tx-002) | Observed behaviour | Synchronous mapping cancellation can become permanent or poison failure | MQTT-006, TRANSPORT-005, TRANSPORT-006, TRANSPORT-008 |
| [GAP-TX-003](#gap-tx-003) | Unknown behaviour | Retained schema and route compatibility policy is unapproved | MQTT-009, MQTT-015, TRANSPORT-008 |
| [GAP-TX-004](#gap-tx-004) | Unknown behaviour | Transport metadata stops at durable mapping boundary | MQTT-004, MQTT-010, TRANSPORT-004, TRANSPORT-006 |
| [GAP-TX-005](#gap-tx-005) | Observed behaviour | Optional empty GUID metadata is emitted but rejected by peer ingress | MQTT-010, MQTT-015 |
| [GAP-TX-006](#gap-tx-006) | Unknown behaviour | Shared-client concurrency and disposal safety not established | MQTT-011, MQTT-015 |
| [GAP-TX-007](#gap-tx-007) | Observed behaviour | Inbox poison acknowledgement and disconnect log wording overstate timing | MQTT-014 |
| [GAP-TX-008](#gap-tx-008) | Unknown behaviour | Route/map cross-registration validation happens at resolution | MQTT-001, MQTT-015, TRANSPORT-003 |
| [GAP-TX-009](#gap-tx-009) | Unknown behaviour | Inbox shutdown duration and synchronization resource policy not established | MQTT-008, MQTT-015 |
| [GAP-INTENT-CORE-001](#gap-intent-core-001) | Unknown behaviour | Opaque token compatibility versus exact GUID encoding | IDENTITY-004 |
| [GAP-INTENT-DUR-001](#gap-intent-dur-001) | Unknown behaviour | Outgoing route validation responsibility and timing | OUTBOX-003 |
| [GAP-INTENT-WF-001](#gap-intent-wf-001) | Unknown behaviour | Ordering and fairness of workflow selection and reads | SAGA-016, SAGA-017, SCHED-008, SCHED-009, SCHED-016 |
| [GAP-INTENT-WF-002](#gap-intent-wf-002) | Unknown behaviour | Workflow retry delay policy | SCHED-012 |
| [GAP-INTENT-WF-003](#gap-intent-wf-003) | Unknown behaviour | Workflow diagnostics and retained attempt/outcome history | SAGA-006, SCHED-007, SCHED-012 |
| [GAP-INTENT-WF-004](#gap-intent-wf-004) | Unknown behaviour | Workflow retention cutoff and cleanup cadence | SAGA-017, SCHED-016 |
| [GAP-INTENT-WF-005](#gap-intent-wf-005) | Unknown behaviour | Wake cancellation precedence and Quartz recovery frequency | SCHED-013, SCHED-014 |
| [GAP-INTENT-WF-006](#gap-intent-wf-006) | Unknown behaviour | Value construction and restricted cancellation descriptions | SAGA-004, SAGA-006, SCHED-003 |
| [GAP-INTENT-WF-007](#gap-intent-wf-007) | Unknown behaviour | Unknown scheduling target failure surface | SCHED-004 |
| [GAP-INTENT-TX-001](#gap-intent-tx-001) | Unknown behaviour | Declared contract versus supplied ingress mapping authority | TRANSPORT-004 |
| [GAP-INTENT-TX-002](#gap-intent-tx-002) | Unknown behaviour | Supported connection and destination validation contract | MQTT-002, MQTT-009 |
| [GAP-INTENT-TX-003](#gap-intent-tx-003) | Unknown behaviour | Repeated reserved metadata ambiguity | MQTT-004 |
| [GAP-INTENT-TX-004](#gap-intent-tx-004) | Unknown behaviour | Subscription readiness and connection recovery contract | MQTT-008 |
| [GAP-INTENT-LIFE-001](#gap-intent-life-001) | Unknown behaviour | Retention clock after discard | OPS-003 |
| [GAP-INTENT-LIFE-003](#gap-intent-life-003) | Unknown behaviour | Infrastructure recovery backoff compatibility | WORK-004 |

## GAP-INTENT-CORE-001

**Issue:** Opaque token compatibility versus exact GUID encoding


**Affected contracts:** [IDENTITY-004](identity-context/IDENTITY-004-opaque-claim-token-representation.md).

**Reserved identifiers (not approved scenarios):** `IDENTITY-004-AC-01`.

**Question/decision:** Is 32-character GUID N-format an intentional public compatibility promise, or merely the current encoding of an opaque usable claim token?

**Recommendation (not approved):** Unapproved recommendation: require usable opaque tokens within the supported persisted capacity, not a GUID algorithm; explicitly review known token-parsing consumers before any implementation change.

**Consequences and alternatives:** Removing exact output format would narrow the original composite obligation and could affect callers that parse it. Persisted claim-field capacity and separation from logical identity remain relevant; no such callers were established by this workstream.

**Proposed identifier disposition:** Reserve IDENTITY-004-AC-01 in full; approve it unchanged if exact GUID format is required, otherwise retire/split the full criterion under an explicit migration. Do not reuse it for usability alone; assign no new ID before approval.

**Implementation comparison:** Inspection only: ClaimId.Create currently returns Guid.NewGuid().ToString("N"), fitting the 32-character non-Unicode field.

**Original assertions retained for decision, not approved requirements:**

- `IDENTITY-004-AC-01`: Given the public claim-token factory. When a token is created. Then it is a 32-character N-format GUID string accepted by the standard claim field representation.


## GAP-INTENT-DUR-001

**Issue:** Outgoing route validation responsibility and timing


**Affected contracts:** [OUTBOX-003](outbox/OUTBOX-003-target-validation-local-selection-and-empty-batches.md).

**Reserved identifiers (not approved scenarios):** `OUTBOX-003-AC-02`.

**Question/decision:** Which transport-name/destination constraints must be validated before staging, and which remain exclusively owned by the selected transport/provider?

**Recommendation (not approved):** Unapproved recommendation: Keep protocol-specific destination semantics provider-owned, but decide the portable persisted-field constraints and error boundary explicitly; do not require permissiveness merely because the factory currently omits checks.

**Consequences and alternatives:** Early portable validation avoids deferred save failures; universal destination validation would couple provider-neutral routing to protocols. Existing permissive inputs are a compatibility consideration, not independent authority for every omitted check.

**Proposed identifier disposition:** Reserve OUTBOX-003-AC-02 in its original form. Retain target identity validation and provider-neutral ownership; create separate approved validation/non-guarantee IDs if its composite assertion changes.

**Implementation comparison:** Inspection only: Factory validates target ID ASCII/length and nonblank transport, but not comprehensive transport length or destination semantics.

**Original assertions retained for decision, not approved requirements:**

- `OUTBOX-003-AC-02`: - **Given** a nonblank unusually long transport or null/blank/custom destination and otherwise valid target. - **When** factory creates a batch. - **Then** factory does not itself reject by destination semantics or transport maximum length; provider mapping/persistence/delivery may reject later.


## GAP-INTENT-WF-001

**Issue:** Ordering and fairness of workflow selection and reads


**Affected contracts:** [SAGA-016](sagas/SAGA-016-persisted-manager-and-timer-snapshots.md), [SAGA-017](sagas/SAGA-017-retained-tombstones-purge-boundaries-and-lifecycle-limits.md), [SCHED-008](scheduling/SCHED-008-persisted-management-snapshots.md), [SCHED-009](scheduling/SCHED-009-due-eligibility-claims-and-bounded-execution.md), [SCHED-016](scheduling/SCHED-016-terminal-retention-recovery-boundaries-and-management-limits.md).

**Reserved identifiers (not approved scenarios):** `SAGA-016-AC-02`, `SAGA-016-AC-04`, `SAGA-017-AC-03`, `SCHED-008-AC-02`, `SCHED-009-AC-03`, `SCHED-016-AC-02`.

**Question/decision:** Are claim order, group/timer order and manager list order contractual, and what tie-breaks or fairness are promised?

**Recommendation (not approved):** Unapproved: Retain best-effort earliest-due scheduling selection and deterministic DueAt/Id order for schedule group and timer snapshots. Retain CreatedAt ascending for active-saga lists and FailedAt descending for failed-saga lists, leaving equal-timestamp saga ties unspecified. Do not promise execution/completion order, global FIFO or starvation freedom. This is the simplest compatibility-oriented option; strict fairness would be a separate capability.

**Consequences and alternatives:** Operators keep useful due-time and incident ordering without a new fairness mechanism. Local parallelism, lock contention and multiple hosts can still deliver or complete later work first; the first bounded list page is not a stable pagination cursor, and equal-time saga rows can move between reads. Existing supporting indexes can remain useful. An unordered-read alternative reduces sorting/index expectations but makes management displays and limited-result subsets less predictable. A global FIFO/fairness alternative needs durable coordination and can reduce throughput or block ready work behind a slow/locked item. Selecting any changed composite requires the existing explicit ID disposition, not silently weakening the reserved ordered-selection criterion.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: Claims/group timers sort DueAt then Id; active/failed saga lists use CreatedAt ascending/FailedAt descending.

**Original assertions retained for decision, not approved requirements:**

- `SAGA-016-AC-02`: **Given** mixed active/complete/failed rows and positive requested limit. **When** GetActive or GetFailed reads. **Then** active list excludes all terminal rows and orders CreatedAt ascending; failed list excludes active/complete and orders FailedAt descending; no more than limit are returned, ties unspecified.
- `SAGA-016-AC-04`: **Given** a saga has active and terminal timers plus other-target schedules. **When** GetTimers reads. **Then** only saga group/target timers appear ordered due then ID, ConsumedAt reflects schedule dispatch, and IsTerminal is true for consumed/cancelled/failed timestamps.
- `SAGA-017-AC-03`: **Given** host starts or purge fails with more eligible rows than batch. **When** cleanup lifecycle continues. **Then** no immediate startup purge is assumed; one configured batch ordered by UpdatedAt is attempted per tick; failure retries on later tick.
- `SCHED-008-AC-02`: **Given** group rows with differing due times and IDs including equal due times. **When** GetGroup completes. **Then** snapshots are ordered by due instant then schedule identity rather than completion or insertion order.
- `SCHED-009-AC-03`: **Given** more eligible schedules than batch size including equal due instants. **When** claim completes. **Then** no more than the configured batch is owned, ordered by original due then identity; nonselected rows remain available.
- `SCHED-016-AC-02`: **Given** more eligible rows than batch times cap. **When** a cleanup tick runs. **Then** deletion uses batches ordered by CreatedAt, stops at configured per-run cap with warning when capped, and leaves remaining work for later ticks.


## GAP-INTENT-WF-002

**Issue:** Workflow retry delay policy


**Affected contracts:** [SCHED-012](scheduling/SCHED-012-failure-budgets-diagnostics-and-cancellation-recovery.md).

**Reserved identifiers (not approved scenarios):** `SCHED-012-AC-04`.

**Question/decision:** Is exact deterministic capped exponential retry, including no jitter, the intended scheduling policy?

**Recommendation (not approved):** Unapproved: Keep capped exponential delay without jitter as the simplest initial policy: min(MaxDelay, Delay * 2^(attemptOrdinal-1)), with cap-saturating arithmetic and zero base delay remaining zero. Define attemptOrdinal through the separately selected retry-accounting policy before approving this formula; do not silently equate it with whichever counter currently increments on claim.

**Consequences and alternatives:** Deterministic delays are easy for operators to predict and for acceptance scenarios to reproduce, and do not require random-number state. Simultaneous failures can synchronize many schedules and overload a recovering dependency; zero delay intentionally permits immediate retries. Claim-based accounting can increase backoff even for never-admitted work, so GAP-CLAIM-001 and GAP-WF-003 must settle ordinal/budget meaning together. Bounded jitter would spread recovery load but require an explicit delay range, randomness policy and corresponding new/split criterion identity if the existing no-jitter assertion changes. Fixed delay is simpler still but loses exponential pressure reduction; extreme-value admission remains GAP-WF-013.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: Scheduling computes min(MaxDelay, Delay * 2^max(0, attempt-1)) without jitter.

**Original assertions retained for decision, not approved requirements:**

- `SCHED-012-AC-04`: **Given** fixed clock and attempts 1, 2 and a sufficiently high attempt with valid delays. **When** retry time is calculated. **Then** delays equal min(MaxDelay, Delay * 2^max(0, attempt-1)); zero delay is immediate and no random variation is introduced.


## GAP-INTENT-WF-003

**Issue:** Workflow diagnostics and retained attempt/outcome history


**Affected contracts:** [SAGA-006](sagas/SAGA-006-handler-results-and-immutable-effect-descriptions.md), [SCHED-007](scheduling/SCHED-007-cancellation-filters-terminal-states-and-races.md), [SCHED-012](scheduling/SCHED-012-failure-budgets-diagnostics-and-cancellation-recovery.md).

**Reserved identifiers (not approved scenarios):** `SAGA-006-AC-01`, `SCHED-007-AC-05`, `SCHED-012-AC-05`, `SCHED-012-AC-06`.

**Question/decision:** Which diagnostic fallback codes, truncation/sanitization rules and post-success/cancellation/completion histories are guaranteed for schedules and saga outcomes?

**Recommendation (not approved):** Unapproved: Retain bounded last-failure diagnostics rather than add an audit ledger: scheduling code/message/type limits of 200/500/500, last diagnostics and Attempts retained after schedule success/cancellation, and saga failure code/message cleared by business Complete. Preserve deliberate MessageException codes, but give unexpected exceptions scheduling.unhandled with a fixed generic consumer-visible message; keep detailed exception text only in access-controlled diagnostics. This selects a small safe presentation change without changing outcome classification. Raw SagaResult.Fail admission remains the separate GAP-WF-008 decision.

**Consequences and alternatives:** Existing column sizes and schedule history remain usable without a new history store; managers must use terminal timestamps/outcome, not the mere presence of an old error, to identify failure. Clearing saga diagnostics on completion sacrifices the preceding failure detail unless operators have separate retained logs. A fixed unexpected-error message avoids copying arbitrary exception text, which can contain secrets or personal data, into broadly readable status snapshots, but operators need restricted diagnostics to investigate the actual cause. Keeping raw text instead is more immediately descriptive but unsafe without an explicit access/redaction policy; clearing all diagnostics on success is simpler for displays but loses troubleshooting history; a separate immutable audit stream preserves more evidence at additional storage/retention cost. Diagnostic safety cannot be claimed by truncation alone, and Fail validation/retention must not be accidentally approved together.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: ScheduleFailure truncates 200/500/500 with scheduling.unhandled fallback; completion/cancellation retain old diagnostics and Attempts. SagaEntry Complete clears failure code/message; the public result outcome alone does not independently establish the exact retained diagnostic history.

**Original assertions retained for decision, not approved requirements:**

- `SAGA-006-AC-01`: **Given** active state and a handler result Continue, Complete or valid Fail. **When** the owner commits the transition. **Then** Continue remains active, Complete sets completion and clears diagnostics, Fail sets failure/code/message; terminal outcome does not wait for external delivery.
- `SCHED-007-AC-05`: **Given** an active claimed/retrying schedule has diagnostics. **When** cancellation commits. **Then** CancelledAt is present, claim/lease/retry eligibility is cleared, and existing diagnostics remain visible.
- `SCHED-012-AC-05`: **Given** MessageException or ordinary exception with long code/message/type. **When** retry/failure diagnostics persist. **Then** MessageException.Code or scheduling.unhandled is stored, code/message/exception type are limited to 200/500/500 characters, and ordinary exception text is not a guaranteed sanitized public message.
- `SCHED-012-AC-06`: **Given** a schedule with failed-attempt diagnostics later delivers successfully. **When** completion commits. **Then** DispatchedAt is present and ownership/retry cleared, but Attempts and previous diagnostic values are retained.


## GAP-INTENT-WF-004

**Issue:** Workflow retention cutoff and cleanup cadence


**Affected contracts:** [SAGA-017](sagas/SAGA-017-retained-tombstones-purge-boundaries-and-lifecycle-limits.md), [SCHED-016](scheduling/SCHED-016-terminal-retention-recovery-boundaries-and-management-limits.md).

**Reserved identifiers (not approved scenarios):** `SAGA-017-AC-01`, `SAGA-017-AC-02`, `SAGA-017-AC-03`, `SCHED-016-AC-01`, `SCHED-016-AC-02`, `SCHED-016-AC-03`.

**Question/decision:** What exact cutoff equality, batch ordering, startup cadence and purge version/timestamp semantics are promised?

**Recommendation (not approved):** Unapproved: Use terminal timestamps strictly older than the retention cutoff, leaving equality retained until a later pass. Run bounded periodic cleanup without a promised immediate startup pass or contractual deletion order. Physically delete eligible schedule rows; purge eligible saga payload to JSON null with a distinct StatePurgedAt marker, retaining its identity/outcome and business Version/UpdatedAt. Treat payload purge as maintenance rather than a business transition. Tombstone lifetime/restart remains GAP-WF-011, not an implicit indefinite-retention approval.

**Consequences and alternatives:** Retention becomes a minimum evidence horizon rather than a precise deletion deadline: polling intervals, batch caps, backlog and failures can retain data longer, so operators must provision storage for overshoot. Physical schedule deletion ends that row's identity exclusion/history, while saga payload purge does not release type/key identity; downstream evidence has its own horizon. Keeping business Version/UpdatedAt unchanged avoids presenting cleanup as a workflow transition, but readers need StatePurgedAt to detect lost reconstructible state. Immediate startup cleanup can reduce backlog but create restart-time I/O spikes; inclusive cutoff changes equality behavior; ordered deletion can help predictable reclamation but may require indexes; advancing business version on purge adds concurrency/audit churn. Retained tombstones can grow indefinitely unless GAP-WF-011 selects a separate lifetime, and active-work exclusion must survive races under the chosen provider.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: Schedule cleanup uses strict terminal cutoff, CreatedAt batches and periodic capped runs; saga purge uses strict cutoff, JSON null, UpdatedAt order and no Version/UpdatedAt increment.

**Original assertions retained for decision, not approved requirements:**

- `SAGA-017-AC-01`: **Given** completed/failed rows with terminal timestamp before/equal/after cutoff and old active rows. **When** a purge batch runs. **Then** only previously unpurged terminal state strictly before cutoff is replaced with JSON null; equality/after/active rows retain their state.
- `SAGA-017-AC-02`: **Given** an eligible terminal saga is purged. **When** persisted snapshot and uniqueness are read. **Then** StatePurgedAt records purge time; type/key/start identity/outcome/diagnostics remain, and Version and UpdatedAt are not incremented by purge.
- `SAGA-017-AC-03`: **Given** host starts or purge fails with more eligible rows than batch. **When** cleanup lifecycle continues. **Then** no immediate startup purge is assumed; one configured batch ordered by UpdatedAt is attempted per tick; failure retries on later tick.
- `SCHED-016-AC-01`: **Given** dispatched/cancelled/failed rows before, equal to and after cutoff plus very old active work. **When** cleanup completes. **Then** only terminal rows with terminal timestamp strictly before cutoff are deleted; equal/after and active rows remain.
- `SCHED-016-AC-02`: **Given** more eligible rows than batch times cap. **When** a cleanup tick runs. **Then** deletion uses batches ordered by CreatedAt, stops at configured per-run cap with warning when capped, and leaves remaining work for later ticks.
- `SCHED-016-AC-03`: **Given** a host starts or a cleanup query fails. **When** periodic cleanup lifecycle continues. **Then** no immediate startup purge is assumed; failed cleanup can retry on a later tick without changing active schedules into terminal ones.


## GAP-INTENT-WF-005

**Issue:** Wake cancellation precedence and Quartz recovery frequency


**Affected contracts:** [SCHED-013](scheduling/SCHED-013-local-wake-engine-commit-hints-and-polling-recovery.md), [SCHED-014](scheduling/SCHED-014-quartz-wake-adapter-fallback-reconnect-and-shutdown.md).

**Reserved identifiers (not approved scenarios):** `SCHED-013-AC-02`, `SCHED-014-AC-04`.

**Question/decision:** What happens to pre-cancelled already-due waits, and must every later future wait reattempt Quartz scheduling?

**Recommendation (not approved):** Unapproved: Observe caller cancellation before the already-due fast path; otherwise return immediately for already-due work. After an ordinary Quartz scheduling failure, try Quartz again on the next future wait, with no background reconnect loop or additional retry timer. This is the simplest recovery mechanism, while bounded/hung scheduler I/O remains governed by GAP-WF-005 rather than assumed solved by per-wait retry.

**Consequences and alternatives:** Pre-cancelled already-due waits would now surface cancellation instead of apparent success, changing the reserved compatibility assertion without changing durable schedule state. One attempt per future wait avoids a new lifecycle/background service, but rapid waits can repeatedly log failures or pressure Quartz, and reconnection is not attempted until another future wait occurs. Existing or uncertain Quartz wake jobs may still repeat harmless hints, not payload completion. A cooldown/circuit-breaker alternative reduces outage noise but delays renewed shared hints; a background reconnect service improves independence from workload at extra lifecycle/shutdown cost. Neither alternative nor the recommended cadence protects local due/cancellation responsiveness while ScheduleJob hangs: GAP-WF-005 must select that bound separately.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: Both engines return immediately for already-due waits without checking cancellation; Quartz attempts scheduling on each later future wait.

**Original assertions retained for decision, not approved requirements:**

- `SCHED-013-AC-02`: **Given** a past instant or a cancelled token for a future wait. **When** Wait is called. **Then** past wait returns without delay even if the token is already cancelled; future waiting observes cancellation rather than a false delivery success.
- `SCHED-014-AC-04`: **Given** a prior Quartz scheduling failure and a subsequently restored service. **When** another future Wait occurs. **Then** scheduling is attempted again; no adapter reconnect guarantee or permanently disabled state is implied.


## GAP-INTENT-WF-006

**Issue:** Value construction and restricted cancellation descriptions


**Affected contracts:** [SAGA-004](sagas/SAGA-004-identity-values-and-durable-correlation.md), [SAGA-006](sagas/SAGA-006-handler-results-and-immutable-effect-descriptions.md), [SCHED-003](scheduling/SCHED-003-input-descriptions-serialization-and-utc-boundaries.md).

**Reserved identifiers (not approved scenarios):** `SAGA-004-AC-02`, `SAGA-006-AC-07`, `SCHED-003-AC-07`.

**Question/decision:** Is acceptance/formatting of directly constructed empty identity values and empty SagaCancel an intentional compatibility contract?

**Recommendation (not approved):** Unapproved: Preserve permissive direct Guid-wrapper and SagaCancel construction for compatibility, including existing ScheduleId/ScheduleGroup N formatting and SagaId D formatting; continue rejecting empty IDs when supported commands/results admit durable work. A SagaCancel remains a description, not an arbitrary external effect execution port. Default target/key runtime admission remains GAP-WF-001 and is not approved by keeping constructor behavior.

**Consequences and alternatives:** Callers can still hold empty sentinel values in memory, and existing string representations remain stable for consumers that display or serialize them; those values do not become valid durable operations. Failures remain at the supported command/result boundary, so direct persistence bypasses are not covered by this compatibility choice. Rejecting empty values in constructors gives earlier feedback but breaks placeholder construction and possibly serializer paths, and still cannot prevent default structs, so boundary validation remains necessary. Normalizing formatting would break string comparisons or external persisted identifiers without improving operation identity. Supporting arbitrary externally implemented effects would add failure/transaction ownership obligations and is a different API decision, not a consequence of the public cancellation descriptor.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: GUID wrapper constructors preserve empty values; ScheduleId/Group format N, SagaId D; SagaCancel constructor accepts empty but Result.Cancel rejects it.

**Original assertions retained for decision, not approved requirements:**

- `SAGA-004-AC-02`: **Given** new and directly supplied SagaId values including Guid.Empty. **When** Value and ToString are read. **Then** Value is preserved and text uses GUID D format; New supplies a nonempty GUID, not a correlation key.
- `SAGA-006-AC-07`: **Given** new SagaCancel(Guid.Empty) or a custom effect intent is constructed externally. **When** the supported result APIs are considered. **Then** constructor alone does not reject empty SagaCancel, but supported Result.Cancel rejects it; no public append/apply port makes arbitrary SagaEffect subclass execution a supported plugin contract.
- `SCHED-003-AC-07`: **Given** Guid.Empty and newly generated GUID values. **When** identity objects are constructed and formatted. **Then** constructors retain even empty values, New produces a nonempty identity, and schedule/group text is the GUID N representation; writer guards remain a separate boundary.


## GAP-INTENT-WF-007

**Issue:** Unknown scheduling target failure surface


**Affected contracts:** [SCHED-004](scheduling/SCHED-004-runtime-ef-outbox-and-model-composition.md).

**Reserved identifiers (not approved scenarios):** `SCHED-004-AC-06`.

**Question/decision:** Is scheduling.delivery.target_not_registered/Fault the deliberate public recovery outcome for a retained unknown target?

**Recommendation (not approved):** Unapproved: Make scheduling.delivery.target_not_registered/Fault the deliberate permanent recovery outcome for a retained unknown scheduling target, with no successful dispatch or fallback to a different target. This is the smallest compatibility-oriented option and distinguishes an invalid routing composition from temporary transport failure.

**Consequences and alternatives:** An unknown target stops consuming retries and is visible as terminal failure, but a later deployment that registers the missing target does not automatically revive the retained failed schedule. Operators must correct composition and use an explicitly supported recovery approach; blindly resubmitting the same retained ID can collide with GAP-WF-002 admission rules, and a new ID must not be mistaken for a harmless replay of the same logical operation. Retry with a bounded registration-grace window is useful during rolling deployments but consumes retry budget/storage and requires duration/exhaustion policy alongside GAP-CLAIM-001 and GAP-WF-004. Silent success would hide lost work; target fallback could misroute payloads. Exposing the code publicly creates a compatibility obligation for monitoring and error handling.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: Internal delivery registry throws that string-coded Fault when retained target lookup fails.

**Original assertions retained for decision, not approved requirements:**

- `SCHED-004-AC-06`: **Given** a persisted eligible schedule whose exact target is no longer registered. **When** processing attempts delivery. **Then** the schedule faults with scheduling.delivery.target_not_registered; startup does not promise a scan of every retained row.


## GAP-INTENT-TX-001

**Issue:** Declared contract versus supplied ingress mapping authority


**Affected contracts:** [TRANSPORT-004](transport/TRANSPORT-004-incoming-typed-admission-and-metadata-loss-boundary.md).

**Reserved identifiers (not approved scenarios):** `TRANSPORT-004-AC-02`.

**Question/decision:** Does generic ingress authorize decoding/admission solely from its supplied typed pair, or must declared envelope Contract agree at that boundary?

**Recommendation (not approved):** unapproved: Make caller/provider trust and contract-validation ownership explicit. Prefer validated declarations at untrusted ingress, while permitting deliberate trusted internal typed paths only with clear documented authority.

**Consequences and alternatives:** A wrong declaration can currently be ignored even though typed bytes are compatible. Enforcing agreement can break callers that deliberately choose maps; choose compatibility/migration before new acceptance IDs.

**Proposed identifier disposition:** Reserve every original ID and exact heading anchor. Restore unchanged only if its entire original meaning is approved; otherwise retire/reserve it and assign fresh IDs to approved replacements/splits. Update test mappings only after approval.

**Implementation comparison:** Inspection-only: TransportIngress uses supplied PayloadType/MessageType independent of envelope Contract. MQTT handler independently validates optional declaration before generic ingress. The two trust boundaries are not a universal contract agreement guarantee.

**Original assertions retained for decision, not approved requirements:**

- `TRANSPORT-004-AC-02`: **Given** valid A bytes and an A-to-B map but unrelated envelope `Contract` text, **When** generic ingress admission runs without a provider-specific contract check, **Then** it deserializes A and admits B according to the map rather than rejecting solely for the text mismatch.


## GAP-INTENT-TX-002

**Issue:** Supported connection and destination validation contract


**Affected contracts:** [MQTT-002](mqtt/MQTT-002-startup-options-and-mqtt-connection-configuration.md), [MQTT-009](mqtt/MQTT-009-outgoing-target-and-destination-staging.md).

**Reserved identifiers (not approved scenarios):** `MQTT-002-AC-01`, `MQTT-002-AC-02`, `MQTT-002-AC-04`, `MQTT-009-AC-03`.

**Question/decision:** Which operational numeric limits, credential combinations, failure diagnostics and topic-validation boundary constitute supported adapter configuration rather than incidental validator behavior?

**Recommendation (not approved):** unapproved: Recommend the smallest explicit operational matrix, with no implicit correction. Host and ClientId: nonblank. Port: integers 1..65535 inclusive; TLS uses exactly the selected port, including 1883, with no automatic switch to 8883 or certificate-policy override. Configured QoS: 1 or 2 only; reject 0 and unknown enum values. Inbox session expiry: positive. Inbox reconnect delay: TimeSpan.FromMilliseconds(1)..TimeSpan.FromMilliseconds(uint.MaxValue - 1), inclusive; accept fractional milliseconds within that interval, reject shorter/longer delays, and keep the existing default. Recovery waits remain cancellable. Credentials: blank/null Username plus null/empty Password means anonymous; nonblank Username plus null, empty or nonempty Password means configure those supplied credentials without trimming/changing them; blank/null Username plus any nonempty Password, including whitespace, is a startup error rather than silent anonymous fallback. Reject invalid static options before connection with attributable aggregated validation errors. Topic ownership: the MQTT target provider evaluates the original-message destination callback and validates its complete result before durable staging. Require a nonblank Topic Name satisfying mandatory MQTT 5 UTF-8/topic rules (1..65535 UTF-8 bytes, no U+0000, unpaired surrogate, + or #); permit otherwise valid empty slash levels and $-prefixed names, with no trimming, prefixing or case normalization. Invalid callback output is a permanent outbox.dispatch.failed construction fault before staging; callback exceptions retain the existing propagation boundary. Dispatch applies the same validation to the stored destination before publication, covering old retained rows; it never reevaluates the callback to fix a bad stored topic. The adapter owns syntax, while the broker owns ACL/authorization and operational acceptance. Alternatives: (A) freeze current permissive boundaries: positive ports including 65536, positive sub-millisecond delay up to the existing maximum, silent omission of password-only credentials, and only nonblank topic checks before staging, leaving further rejection to MQTTnet/broker. This minimizes immediate compatibility changes but preserves delayed unusable configuration and possible unintended anonymous connections. (B) use the recommended static matrix but leave complete topic validation solely to dispatch: staging remains compatible for nonblank topics, while invalid durable entries permanently fail later and require operator repair. This avoids a staging behavior change but commits unusable work and delays producer feedback. (C) support password-only credentials explicitly by sending an empty username rather than ignoring the password; choose this only for a demonstrated broker requirement and a verified MQTTnet representation, because it changes the on-wire authentication behavior.

**Consequences and alternatives:** Recommended changes reject Port=65536 and other out-of-range positive ports, sub-millisecond reconnect settings, password-only anonymous fallbacks, and nonblank invalid topics that current validators/staging accept. Operators should preflight configuration before rollout, provide an explicit username or remove the unused password, and retain their chosen TLS port/certificate deployment policy. Earlier destination validation moves failure from durable dispatch to the producer staging call; consumers of that call must handle the permanent construction fault. Revalidation of old retained invalid destinations stops futile publication retries but can newly dead-letter them under owned failure handling; operators must inventory and explicitly migrate/recreate affected work while preserving identity and target-state contracts rather than silently recalculate destinations. No new defaults, broker availability/authorization guarantees, certificate guarantees or application delivery guarantees follow from validation. All four original composite IDs/anchors and their assertions stay reserved. Port-bound acceptance, credential omission and late-only topic validation are materially different under this proposal: use fresh replacement/split IDs if approved, not silently reuse those originals. Restore any original only after separate approval of its entire original assertion. Alternative A retains surprising behavior; B delays feedback and commits invalid work; C risks changing previously anonymous broker authentication.

**Proposed identifier disposition:** Reserve original IDs and exact anchors. Restore unchanged only with approval of entire original meaning; otherwise keep retired/reserved and assign fresh IDs to approved replacements/splits. Update test mappings only after approval.

**Implementation comparison:** Inspection-only: validators accept any positive port including 65536, bound inbox delay to uint.MaxValue-1 milliseconds, omit credentials for blank Username and destination staging only rejects whitespace. Nonblank invalid topics may reach MQTTnet/broker validation; TLS does not change the configured port. Inspection-only delta for this unapproved option: add the port upper bound, 1 ms delay floor and password-without-username rejection; current delay ceiling and nonblank credentials/TLS port behavior otherwise align. Complete MQTT Topic Name validation would move into the target provider and be reused for stored-destination dispatch, replacing reliance on downstream MQTTnet/broker rejection.

**Original assertions retained for decision, not approved requirements:**

- `MQTT-002-AC-01`: **Given** blank Host/ClientId, nonpositive Port, QoS 0/unknown enum, or inbox zero expiry/nonpositive or excessive reconnect delay, **When** registered startup validation evaluates options, **Then** it reports the relevant failures instead of treating those values as a valid operational configuration; multiple invalid values can yield multiple messages.
- `MQTT-002-AC-02`: **Given** positive Port 65536 and, for inbox, reconnect delay exactly TimeSpan.FromMilliseconds(uint.MaxValue - 1) and positive expiry, **When** adapter validators run with all other values valid, **Then** they accept those values; one millisecond beyond the delay bound is rejected. Validator acceptance does not claim that the networking stack/broker accepts that port or delay operationally.
- `MQTT-002-AC-04`: **Given** a password with blank Username, **When** connection options are built, **Then** credentials are not configured. **Given** nonblank Username and UseTls=true with Port=1883, **When** connection options are built, **Then** username/password and MQTTnet TLS are enabled without silently changing port or supplying custom certificate rules; successful secure deployment still needs host/broker validation.
- `MQTT-009-AC-03`: **Given** a destination callback returning whitespace or throwing, **When** its exact-type message is staged, **Then** whitespace yields permanent `OutboxDispatchException` and a callback exception propagates before successful staging. **Given** a nonblank syntactically invalid MQTT publication topic, **When** the target provider evaluates it, **Then** it is not rejected solely by this nonblank check; MQTTnet/broker validation may reject it later.


## GAP-INTENT-TX-003

**Issue:** Repeated reserved metadata ambiguity


**Affected contracts:** [MQTT-004](mqtt/MQTT-004-incoming-metadata-serialization-and-contract-compatibility.md).

**Reserved identifiers (not approved scenarios):** `MQTT-004-AC-04`.

**Question/decision:** How should repeated exact-name reserved MQTT properties be resolved: agreement, rejection or an explicit precedence contract?

**Recommendation (not approved):** unapproved: Reject ambiguous identity/contract declarations or require agreement unless a deliberate wire-compatibility precedence is approved; retain the exact-case reserved-name boundary.

**Consequences and alternatives:** Last-value-wins can change logical identity/contract with header order and can poison otherwise valid earlier metadata. Any change needs peer compatibility consideration and fresh IDs if the original assertion is not approved intact.

**Proposed identifier disposition:** Reserve original IDs and exact anchors. Restore unchanged only with approval of entire original meaning; otherwise keep retired/reserved and assign fresh IDs to approved replacements/splits. Update test mappings only after approval.

**Implementation comparison:** Inspection-only: handler reads the last exact lowercase reserved property; case-distinct names are not aliases. A malformed last ID poisons the receipt even if an earlier value was valid.

**Original assertions retained for decision, not approved requirements:**

- `MQTT-004-AC-04`: **Given** repeated `message-id`/`contract` properties and case-distinct `Message-Id`, **When** metadata is read, **Then** the last exact lowercase reserved property determines validation/admission and the case-distinct property does not override it. A malformed last reserved ID is poison even when an earlier one was valid.


## GAP-INTENT-TX-004

**Issue:** Subscription readiness and connection recovery contract


**Affected contracts:** [MQTT-008](mqtt/MQTT-008-inbox-reconnect-subscription-results-and-cleanup.md).

**Reserved identifiers (not approved scenarios):** `MQTT-008-AC-01`, `MQTT-008-AC-02`.

**Question/decision:** What accepted subscription result/granted-QoS and session-present behavior establishes readiness, and which failed setup requires fixed-delay disconnect/reconnect rather than ready state?

**Recommendation (not approved):** unapproved: Recommend unconditional subscription re-establishment with a minimum nonzero grant, rather than strict requested-QoS equality or trusting session restoration. After every successful MQTT connection, including SessionPresent=true, subscribe every configured filter again. Declare setup ready only after every filter receives exactly its one successful SUBACK result with granted QoS 1 or 2 and granted QoS no greater than requested QoS. Accept a requested-2/granted-1 downgrade; reject QoS 0, refusal, missing/extra results and above-request grants. A successful connect alone, or a partial set of subscriptions, is not readiness. Existing admission-before-ACK handling of receipts is unchanged; readiness does not invent an atomic subscription batch or forbid valid receipts arriving during setup. For a failed connection or subscription attempt, log an attributable setup warning, attempt disconnect/cleanup, wait once for the configured cancellable fixed reconnect delay, reconnect, then reinstall all subscriptions. Do not mark partial setup ready or spend a durable message retry budget for this connection recovery. Host stopping cancels setup/wait and exits recovery without an ordinary setup-fault loop; final-disconnect/drain bounds remain the separate GAP-TX-009 question. Alternatives: (A) require granted QoS exactly equal requested QoS, retaining full resubscription and the same recovery sequence. Choose this if loss of the requested protocol level must block readiness rather than degrade to QoS 1. (B) reuse broker-restored subscriptions only with SessionPresent=true and independently established unchanged route configuration/client-session identity; otherwise reinstall all filters. Without that route-generation assurance, do not take the skip-resubscribe fast path. (C) retain full resubscription and the recommended grant checks but replace fixed delay with capped exponential reconnect backoff plus jitter, resetting after completed setup. This needs a separately selected initial delay/cap/reset contract rather than silently changing the meaning of the existing delay setting.

**Consequences and alternatives:** The recommended option preserves current unconditional resubscription, requested-2/granted-1 tolerance, warning/disconnect/fixed-delay recovery and refusal/QoS-0/count-mismatch rejection. It adds rejection of above-request grants that current code accepts; these indicate a protocol-invalid response, not stronger supported delivery. Unconditional resubscription costs broker round trips on restored sessions and can replay retained publications under the configured MQTT subscription behavior; admission duplicate protection applies only to legitimate stable message/event identities while local evidence remains, so subscription recovery can neither promise no repeats nor infer exactly-once effects. Alternative A makes brokers that cap grants at QoS 1 never ready for requested QoS 2, potentially creating an indefinite recovery loop until an operator changes the broker or route request; it does not establish exactly-once application effects. Alternative B saves round trips and may avoid resubscription-triggered retained replay, but stale restored filters can omit new routes or retain removed routes unless configuration generations and session identity are owned explicitly. Alternative C reduces synchronized retries during broker outages but delays readiness and adds configuration/state beyond the simplest fixed-delay option. Even a nonzero subscription grant does not prove that every publisher used nonzero QoS, that broker/session retention will preserve every receipt, or that consumers completed. All original composite IDs/anchors remain reserved: approve the complete original assertion separately before restoring it; use fresh replacement/split IDs for changed grant validation or recovery choices. No shutdown/draining policy, warning-event inventory or durable retry-accounting policy is approved here.

**Proposed identifier disposition:** Reserve original IDs/exact anchors. Restore unchanged only on approval of entire original meaning; otherwise keep retired/reserved and assign fresh IDs to approved replacements/splits. Update test mappings only after approval.

**Implementation comparison:** Inspection-only: every successful connect resubscribes all filters regardless of session present; one QoS1/2 grant is required but need not match request. Rejected/count-mismatched results trigger warning/disconnect/fixed configured delay/reconnect. Inspection-only delta for this unapproved option: existing setup does not reject a granted QoS above the requested level; add that compatibility check if approved. Full resubscription and fixed recovery otherwise align. No route-generation restoration check, capped backoff or broker behavior certification is established.

**Original assertions retained for decision, not approved requirements:**

- `MQTT-008-AC-01`: **Given** configured routes and a successful MQTT connect response, **When** startup or reconnect completes its subscriptions, **Then** all filters are subscribed again regardless of session-present status. Each result must contain exactly one granted QoS 1 or 2 result; granted QoS need not equal requested QoS.
- `MQTT-008-AC-02`: **Given** non-success connection result, subscription rejection/QoS 0, or an unexpected subscription result count, **When** connection setup runs, **Then** setup fails, logs a warning and enters disconnect/fixed configured delay/reconnect recovery instead of a successful ready state or durable entry retry loop. A zero/multiple result count is not accepted as valid subscription success.


## GAP-INTENT-LIFE-001

**Issue:** Retention clock after discard


**Affected contracts:** [OPS-003](observability/OPS-003-terminal-cleanup-boundaries.md).

**Reserved identifiers (not approved scenarios):** `OPS-003-AC-04`.

**Question/decision:** Does discard restart retention, or can any earlier terminal timestamp authorize deletion?

**Recommendation (not approved):** unapproved: select and document a retention clock; consider the latest terminal action when operators require a fresh discard inspection window, with explicit compatibility review.

**Consequences and alternatives:** Using the earlier dead-letter time can delete a just-discarded row; choosing a later time retains diagnostic/dedup evidence longer and changes storage timing.

**Proposed identifier disposition:** Reserve OPS-003-AC-04 in full. If a later-clock policy is approved, assign a replacement criterion with migration from the reserved assertion; strict cutoff and pending protection stay unchanged.

**Implementation comparison:** Inspection-only: DurableStore currently qualifies rows when any terminal timestamp is strictly before cutoff, so an older dead-letter timestamp can override recent discard.

**Original assertions retained for decision, not approved requirements:**

- `OPS-003-AC-04`: OPS-003-AC-04 — Discard age  **Given** a recently discarded row has an older dead-letter timestamp before cutoff. **When** Cleanup runs. **Then** it may be removed based on that older terminal timestamp rather than restarting retention at discard.


## GAP-INTENT-LIFE-003

**Issue:** Infrastructure recovery backoff compatibility


**Affected contracts:** [WORK-004](recovery-lifecycle/WORK-004-queue-leases-and-producer-delay.md).

**Reserved identifiers (not approved scenarios):** `WORK-004-AC-02`.

**Question/decision:** Is exact doubling/reset of claim-failure delay a contractual algorithm, or are recoverability and configured timing budgets the intended contract?

**Recommendation (not approved):** unapproved: prefer recoverability and explicit configured timing bounds over a private exponential algorithm; confirm whether consumers require exact delay compatibility before removing that assertion.

**Consequences and alternatives:** Freezing doubling prevents alternate recovery strategies; dropping it without review may break timing expectations. No fairness or strict recovery deadline follows from either choice.

**Proposed identifier disposition:** Reserve WORK-004-AC-02 for the entire original doubling/cap/reset obligation; assign replacement criteria only after approval, never silently narrow to bounded recovery.

**Implementation comparison:** Inspection-only: WorkLoop.RecoveryDelay doubles from RecoveryInterval with FailureMaxDelay cap, and successful claim resets the local failure sequence.

**Original assertions retained for decision, not approved requirements:**

- `WORK-004-AC-02`: WORK-004-AC-02 — Producer delay budget  **Given** successive claim calls fail and a later call succeeds. **When** recovery time is controlled. **Then** delay begins at RecoveryInterval, doubles with failures up to FailureMaxDelay, and successful claiming resets that failure sequence.

## GAP-CLAIM-001

**Question/decision:** What does a durable attempt count mean, does abandonment spend the budget, and what happens when that budget is exhausted? These are one accounting and recovery policy, not independent choices about individual storage fields.

**Affected contracts:** [CLAIM-001](claims-leases/CLAIM-001-claim-eligibility-and-attempt-accounting.md), and the recovery boundaries of [INBOX-005](inbox/INBOX-005-atomic-local-delivery-and-repeatable-execution.md) and [OUTBOX-011](outbox/OUTBOX-011-transport-dispatch-and-external-repeat-window.md). Existing retry classification must remain distinct from acquisition and local transaction re-execution.

**Original assertions requiring authority:**

- `CLAIM-001-AC-03`: a claim returns no more than the positive limit, persists ownership and increments Attempts once for each returned entry.
- `CLAIM-001-AC-04`: an entry acquired twice accounts for both claims even when the first expired without a handler invocation.
- `CLAIM-001-AC-05`: repeated expiry without failure settlement permits another acquisition even after MaxAttempts; the failure budget is evaluated only on settlement.

The bounded-ownership part of AC-03 is supported by resource and ownership standards. Neither the counter semantics nor the absence of an acquisition cap has independent approval. These three composite/policy identifiers remain reserved in CLAIM-001, not reassigned to narrower or opposite requirements.

**Recommendation (not approved):** Define an attempt as a durable acquisition, including acquisitions abandoned before invocation, and use a finite acquisition budget. When exhausted, unfinished work must reach an explicit non-success terminal disposition with an exhaustion reason, rather than disappearing from acquisition while remaining indefinitely unresolved. Acquisition is a durable fact; whether a handler actually started or a remote effect occurred may be unknowable after a crash. A bounded budget makes repeated crashes operationally finite and follows the coding standard's bounded-retry requirement.

**Consequences and alternatives:** Counting acquisitions can exhaust work that never reached its handler, so operators need an explicit recovery/replay path. Counting invocations instead would spare pre-handler abandonment but requires a separately defined durable invocation boundary and still cannot establish exactly-once execution. Keeping a failure-settlement-only budget preserves indefinite recovery opportunities, but repeated abandoned claims are then not bounded by MaxAttempts; that exception needs explicit approval rather than an assertion that retry is universally bounded.

A finite acquisition budget would change the meaning of existing MaxAttempts configuration and persisted counters. Decide how existing rows, already-exhausted work and management replay are treated before implementing it. This recommendation is not permission to change production code.

**Proposed acceptance-identity migration:** Retire composite AC-03 in favour of separately identified bounded-ownership and accounting criteria, retain AC-04 only if acquisition accounting is approved, and replace AC-05 under a new ID if finite acquisition exhaustion is approved. Alternatively, explicitly approve the original composite and policies and restore those exact obligations. Assign no replacement IDs until the policy and migration are approved. Do not silently use AC-03 for only the batch bound or AC-05 for its opposite.

**Implementation comparison:** The [shared store](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs) increments Attempts on acquisition and does not gate acquisition on MaxAttempts; the [Inbox](../../src/Clean.Messaging.Inbox/Dispatching/InboxProcessor.cs) and [Outbox](../../src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs) processors evaluate the budget during failure settlement. The current implementation therefore differs from the finite-acquisition recommendation if that option is approved. This is a policy-dependent discrepancy, not an already-established defect. It does not weaken existing atomicity, identity or stale-owner protections.

### Contract and identifier dispositions

**Affected contracts:** [CLAIM-001](claims-leases/CLAIM-001-claim-eligibility-and-attempt-accounting.md), [IDENTITY-001](identity-context/IDENTITY-001-stable-identity-across-durable-retries.md), [INBOX-004](inbox/INBOX-004-durable-entry-state-and-fenced-claims.md), [INBOX-006](inbox/INBOX-006-processor-metadata-failure-settlement-and-cancellation.md), [INBOX-007](inbox/INBOX-007-signals-workers-and-restart-recovery.md), [MQTT-012](mqtt/MQTT-012-publish-result-durable-completion-and-failure-classification.md), [OUTBOX-011](outbox/OUTBOX-011-transport-dispatch-and-external-repeat-window.md), [OUTBOX-013](outbox/OUTBOX-013-owned-durable-state-processor-policy-and-cancellation.md), [OUTBOX-014](outbox/OUTBOX-014-workers-signals-bounded-queue-and-recovery.md), [RETRY-002](retry-failures/RETRY-002-failure-snapshot-values-and-bounds.md), [RETRY-003](retry-failures/RETRY-003-failure-retry-budget.md), [RETRY-005](retry-failures/RETRY-005-processing-settlement-boundary.md), [SCHED-009](scheduling/SCHED-009-due-eligibility-claims-and-bounded-execution.md), [SCHED-012](scheduling/SCHED-012-failure-budgets-diagnostics-and-cancellation-recovery.md), [WORK-001](recovery-lifecycle/WORK-001-durability-independent-of-wake-hints.md).

**Reserved identifiers (not approved scenarios):** `CLAIM-001-AC-03`, `CLAIM-001-AC-04`, `CLAIM-001-AC-05`, `INBOX-004-AC-01`, `INBOX-007-AC-02`, `MQTT-012-AC-03`, `OUTBOX-013-AC-01`, `OUTBOX-014-AC-01`, `SCHED-009-AC-04`, `SCHED-009-AC-06`, `SCHED-012-AC-03`.

**Related established criteria remain normative:** `WORK-001-AC-01`, `WORK-001-AC-02`. This decision does not suspend those guarantees.

**Cross-capability consequences:** The accounting decision also covers scheduled work, cancelled or never-admitted claims, and MQTT durable recovery. It does not redefine handler invocation counts, local transaction re-execution or maximum remote-effect counts. The absence of a second MQTT adapter retry loop remains deliberate; it does not approve unlimited claim acquisition.

**Identifier disposition:** All listed composite claim/recovery identifiers remain reserved in their owning specifications. The proposal above applies across these contracts: restore an original obligation only if approved in full; materially changed or split requirements need explicitly approved new identities and test-mapping migration. No IDs are retired, replaced or reassigned here. Bounded ownership, stable identity, failure classification and durability of eligible stored work remain binding independently of the accounting decision.

**Original assertions retained for decision, not approved requirements:**

- `CLAIM-001-AC-03`: Given more eligible entries than a positive limit exist. When Claim runs. Then no more than limit entries are returned, with persisted ownership and one increased Attempts count per returned entry.
- `CLAIM-001-AC-04`: Given an entry is claimed but no handler starts before expiry. When a later claim succeeds. Then Attempts accounts for both claims even though the abandoned attempt had no invocation.
- `CLAIM-001-AC-05`: Given an unresolved entry repeatedly expires without failure settlement even after MaxAttempts. When Claim runs when due/available. Then MaxAttempts alone does not prevent another claim; the failure budget is evaluated on settlement.
- `INBOX-004-AC-01`: - **Given** pending entries include future-due, due-at-now, actively claimed, expired-at-now and terminal rows. - **When** Claim(now, positive limit, positive lease) completes. - **Then** only unresolved due work with absent/expired leases is returned, at most the limit, with a claim token, incremented Attempts and a new expiry.
- `INBOX-007-AC-02`: - **Given** durable unresolved work remains after restart or queue loss and any old lease expires. - **When** startup or periodic recovery wakes a running healthy worker. - **Then** eligible work is eventually attempted subject to host/database availability; crash claims may increase Attempts without a completed handler.
- `MQTT-012-AC-03`: **Given** another unsuccessful publish reason, or an unclassified network/connect/publish exception, **When** a durable attempt fails below its configured retry limit, **Then** shared retry handling can schedule an owned retry; exhaustion can dead-letter. There is no second adapter retry around the publish call and no universal maximum external-effect count is asserted.
- `OUTBOX-013-AC-01`: - **Given** due/future/terminal/actively leased/expired entries and nonpositive claim inputs. - **When** Claim or stale-owner settlement runs. - **Then** only eligible bounded work is owned with increased Attempts; invalid limits/lease fail and stale/expired Renew/Complete/Retry/DeadLetter return false.
- `OUTBOX-014-AC-01`: - **Given** commit hint was lost or host crashed after claiming queued entries. - **When** restart/recovery runs and stale leases expire. - **Then** eligible committed work becomes attemptable again, potentially increasing Attempts without prior handler completion.
- `SCHED-009-AC-04`: **Given** a selected schedule waits behind busy execution slots. **When** the claim is committed but delivery has not started. **Then** Attempts has already increased; the counter does not purport to count handler calls or EF strategy repeats.
- `SCHED-009-AC-06`: **Given** a row repeatedly expires or processing is cancelled without failure settlement and its Attempts already reaches MaxAttempts. **When** it becomes eligible again. **Then** it can be claimed again; MaxAttempts gates failure disposition, not universal claim eligibility.
- `SCHED-012-AC-03`: **Given** Attempts equals MaxAttempts or is greater and delivery fails transiently. **When** settlement succeeds. **Then** the schedule is failed rather than rescheduled; prior lease-loss claims count toward this decision.

## GAP-CLAIM-002

**Question/decision:** When is unfinished work eligible, and precisely when does an owner cease to have authority? Decide due-time and lease-expiry equality together so acquisition and settlement do not disagree at the same instant.

**Affected contracts:** [CLAIM-001](claims-leases/CLAIM-001-claim-eligibility-and-attempt-accounting.md) and [CLAIM-003](claims-leases/CLAIM-003-owned-state-and-expiry-predicates.md). Coordinate input/time validity with [GAP-CORE-004](#gap-core-004), provider concurrency with [GAP-CORE-005](#gap-core-005), and schedule-specific expiry with [GAP-WF-004](#gap-wf-004); those decisions are not closed here.

**Original assertion requiring authority:** `CLAIM-001-AC-01` makes next-attempt equality and lease-expiry equality eligible while excluding future-due and unexpired-owned entries. Its exact temporal policy is reserved, not replaced with an unrelated exclusive-acquisition scenario. The related CLAIM-003 expiry obligation is subject to the same temporal decision; reauthoring it does not approve this proposal.

**Recommendation (not approved):** Respect a target's scheduled retry time, exclude targets still owned by another worker, make work due at its deadline, and treat ownership as ending at its expiry instant. With a consistent UTC time basis, equality then permits recovery and rejects settlement by the expired owner. This avoids a gap or overlap between expiry and eligibility without inventing a grace period. Acquisition must also respect the attempt-budget policy selected in GAP-CLAIM-001.

**Consequences and alternatives:** Strictly-after acquisition would introduce a boundary at which no worker can acquire work even though the former owner has expired. Keeping ownership valid at equality instead requires consistent postponement of reacquisition. A grace interval or tolerance would change recovery latency and authority and must be separately justified. The recommendation does not synchronize clocks or solve provider concurrency by itself.

**Proposed acceptance-identity migration:** Restore AC-01 unchanged only if its original boundary policy is approved. Otherwise reserve/retire it and give any materially different temporal requirement a new ID. Do not redefine AC-01 as general concurrency coverage.

**Implementation comparison:** The [shared store](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs) uses inclusive due/expiry eligibility and rejects owned settlement at expiry equality, aligning with the recommendation for valid inputs. Matching predicates alone do not establish a concurrency guarantee or resolve invalid clock/expiry inputs.

### Contract and identifier dispositions

**Affected contracts:** [CLAIM-001](claims-leases/CLAIM-001-claim-eligibility-and-attempt-accounting.md), [CLAIM-002](claims-leases/CLAIM-002-stale-settlement-rejection.md), [CLAIM-003](claims-leases/CLAIM-003-owned-state-and-expiry-predicates.md), [INBOX-004](inbox/INBOX-004-durable-entry-state-and-fenced-claims.md), [OUTBOX-013](outbox/OUTBOX-013-owned-durable-state-processor-policy-and-cancellation.md), [SCHED-009](scheduling/SCHED-009-due-eligibility-claims-and-bounded-execution.md), [SCHED-011](scheduling/SCHED-011-delivery-transaction-routing-and-retained-contract-failures.md).

**Reserved identifiers (not approved scenarios):** `CLAIM-001-AC-01`, `CLAIM-003-AC-01`, `INBOX-004-AC-01`, `INBOX-004-AC-02`, `OUTBOX-013-AC-01`, `SCHED-009-AC-01`, `SCHED-009-AC-02`, `SCHED-011-AC-01`.

**Cross-capability consequences:** Shared claiming, schedule eligibility and dispatch admission must use a coordinated temporal policy; matching current predicates alone do not approve equality semantics or prove provider-level exclusivity.

**Identifier disposition:** Every listed equality/composite identifier remains reserved with its original heading and assertion. Restore the original temporal obligation only if approved in full; changed or split policies require an explicit identity migration. No replacement IDs are assigned. General due-time respect and stale-owner fencing remain binding.

**Original assertions retained for decision, not approved requirements:**

- `CLAIM-001-AC-01`: Given unresolved entries include next-attempt equal to now and lease-expiry equal to now. When Claim runs at now. Then boundary-equal due/expired entries are eligible while future-due or unexpired-owned entries are excluded.
- `CLAIM-003-AC-01`: Given a current matching claim expires at T. When Renew/Complete/Retry/DeadLetter is attempted at T. Then the update returns false rather than extending or settling the expired claim.
- `INBOX-004-AC-01`: - **Given** pending entries include future-due, due-at-now, actively claimed, expired-at-now and terminal rows. - **When** Claim(now, positive limit, positive lease) completes. - **Then** only unresolved due work with absent/expired leases is returned, at most the limit, with a claim token, incremented Attempts and a new expiry.
- `INBOX-004-AC-02`: - **Given** an entry has been reclaimed with a new token or its lease equals/exceeds expiry at settlement time. - **When** the stale owner attempts Renew, Complete, Retry or DeadLetter. - **Then** false is returned and no state is overwritten; only a matching unexpired owner can settle.
- `OUTBOX-013-AC-01`: - **Given** due/future/terminal/actively leased/expired entries and nonpositive claim inputs. - **When** Claim or stale-owner settlement runs. - **Then** only eligible bounded work is owned with increased Attempts; invalid limits/lease fail and stale/expired Renew/Complete/Retry/DeadLetter return false.
- `SCHED-009-AC-01`: **Given** rows differ by terminal status, original due, retry due and lease expiry. **When** claim runs at time T. **Then** only nonterminal rows with DueAt <= T, retry absent or <= T, and lease absent or <= T become owned.
- `SCHED-009-AC-02`: **Given** eligible rows have due/retry/old lease exactly T. **When** claim runs at T. **Then** equality permits claiming; a timestamp just after T blocks the corresponding predicate.
- `SCHED-011-AC-01`: **Given** a row is absent, terminal, wrong-token or expired. **When** dispatch attempts to enter. **Then** no outgoing effect is staged by that attempt; absent/terminal is settled, an active row owned elsewhere is not represented as that attempt success.

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

### Contract and identifier dispositions

**Affected contracts:** [CONFIG-004](configuration/CONFIG-004-startup-validation-and-configuration-binding.md), [CONFIG-006](configuration/CONFIG-006-complete-boundary-validation.md), [RETRY-004](retry-failures/RETRY-004-backoff-and-jitter-boundaries.md), [RETRY-006](retry-failures/RETRY-006-out-of-domain-public-policy-inputs.md).

**Reserved identifiers (not approved scenarios):** `CONFIG-006-AC-01`, `CONFIG-006-AC-02`.

**Question/decision:** Should null common option groups and undefined backoff become clear startup options-validation failures?

**Recommendation (not approved):** Unapproved existing recommendation: named validation failures for null groups and enum rejection before attempts.

**Consequences and alternatives:** This changes failure classification/timing while preserving valid default and numeric-option semantics.

**Proposed identifier disposition:** Keep CONFIG-006-AC-01/02 as reserved proposal IDs/anchors, with no executable GWT assertion; restore unchanged only after approval of their outcomes. No new IDs assigned.

**Implementation comparison:** Inspection only: null groups currently dereference; undefined enum passes validator and can throw during delay calculation.

**Original assertions retained for decision, not approved requirements:**

- `CONFIG-006-AC-01`: Given a common options object has null Lease, Processing, Recovery or Cleanup. When startup validation runs after this proposal is approved. Then startup returns a clear options validation failure identifying the missing group.
- `CONFIG-006-AC-02`: Given RetryBackoff contains an undefined enum value. When startup validation runs after this proposal is approved. Then startup rejects the mode before any message attempt.

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

**Recommendation (not approved):** Define a supported provider/concurrency envelope and operation-specific commit evidence, then exercise real locking, conditional updates, transient execution and commit-loss/race scenarios before claiming certification. State-only evidence must be assessed for whether it distinguishes the intended attempt from a competing operation.

**Consequences and existing obligations:** Provider selection and attribution rules remain unresolved. Existing ownership, fencing and local atomicity requirements remain binding within their documented boundaries; this decision does not suspend them. An attribution correction would require a deliberate contract and corresponding race/commit-loss criteria, not reinterpretation of existing IDs.

The standard one-context-type rule in [CONFIG-005](configuration/CONFIG-005-one-messaging-persistence-context-type.md) is a composition constraint, not evidence of provider support, instance sharing or transaction success.

**Affected specifications:** PERSIST-004

**Evidence:**
- [src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs)

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

### Contract and identifier dispositions

**Affected contracts:** [INBOX-001](inbox/INBOX-001-typed-admission-and-envelope-identity.md), [INBOX-003](inbox/INBOX-003-empty-admission-boundary.md), [INBOX-014](inbox/INBOX-014-undecided-admission-and-replay-policies.md), [MQTT-006](mqtt/MQTT-006-receive-failures-poison-acknowledgement-and-shutdown.md).

**Reserved identifiers (not approved scenarios):** `INBOX-001-AC-05`, `INBOX-003-AC-01`, `INBOX-003-AC-02`, `MQTT-006-AC-03`.

**Question/decision:** May a valid incoming envelope be durably admitted with no initial targets, and how should an empty repeated batch be validated?

Should a valid mapped message with no registered consumers be rejected, retained for later replay or handled by an explicit alternate admission policy?

**Recommendation (not approved):** Unapproved recommendation: Decide admission-only support together with replay and retention; do not adopt rejection merely because the default registry and store currently reject new empty work.

unapproved: Choose admission ownership first; never manufacture successful admission or poison discard merely to hide unresolved consumer composition.

**Consequences and alternatives:** Supporting admission-only capture changes accepted inputs and requires a durable envelope even without an entry; rejecting it prevents later replay of otherwise valid consumerless messages. Duplicate validation order is separately visible to callers.

Changing no-consumer disposition changes broker ACK/reconnect and retained evidence. The full original classification criterion stays reserved, not narrowed to generic failure-before-ACK.

**Proposed identifier disposition:** Keep all original empty/no-consumer composite IDs reserved. If admission-only is approved, give changed outcomes new IDs; split missing-contract validation from no-consumer policy with new IDs rather than narrowing INBOX-001-AC-05.

Reserve original IDs and exact anchors. Restore unchanged only with approval of entire original meaning; otherwise keep retired/reserved and assign fresh IDs to approved replacements/splits. Update test mappings only after approval.

**Implementation comparison:** Inspection only: Default typed consumer selection throws for no consumers. New empty EF batches fail; retained identity returns before the first-entry requirement.

Inspection-only: current consumer lookup fails and MQTT worker follows ordinary admission-failure/reconnect without success ACK. Empty admission batches do not establish retained admission.

**Original assertions retained for decision, not approved requirements:**

- `INBOX-001-AC-05`: - **Given** a valid envelope whose runtime contract is missing or whose exact runtime type has no consumers. - **When** Accept is requested. - **Then** contract failure or permanent consumer.message_unhandled failure propagates; no envelope is admitted for future replay.
- `INBOX-003-AC-01`: - **Given** an unretained InboxBatch with complete original envelope and Entries empty. - **When** the EF store accepts it directly. - **Then** InvalidOperationException occurs and no admission is retained.
- `INBOX-003-AC-02`: - **Given** an already retained MessageId or EventId and an otherwise empty batch. - **When** the EF store accepts the duplicate directly. - **Then** it returns without adding targets; empty-batch validation is not applied to retained identities.
- `MQTT-006-AC-03`: **Given** valid MQTT payload/identity and mapping but no registered consumer for the mapped runtime message, **When** admission executes, **Then** the runtime consumer failure propagates through the ordinary admission-failure path and no successful acknowledgement is made; reconnect is requested rather than a durable poison record being created.

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

### Contract and identifier dispositions

**Affected contracts:** [INBOX-009](inbox/INBOX-009-replay-retained-admissions-into-missing-targets.md), [INBOX-014](inbox/INBOX-014-undecided-admission-and-replay-policies.md).

**Reserved identifiers (not approved scenarios):** `INBOX-009-AC-05`.

**Question/decision:** Which target identities are valid for replay, and must registration/type compatibility be established before historical work is created?

**Recommendation (not approved):** Unapproved recommendation: Validate a stable resolvable target compatible with the historical contract, including identity syntax consistent with persistence, unless delayed registration is explicitly supported.

**Consequences and alternatives:** Early validation avoids unusable durable work but could rule out deliberately delayed deployment. Syntax and compatibility need an explicit contract rather than reliance on provider errors.

**Proposed identifier disposition:** Reserve the original permissive target criterion. Restore it only if permissiveness is approved; otherwise allocate new IDs for approved validation outcomes.

**Implementation comparison:** Inspection only: Replay checks nonblank target and the message contract, not consumer existence/type compatibility or ASCII/length. Persistence and dispatch can fail later.

**Original assertions retained for decision, not approved requirements:**

- `INBOX-009-AC-05`: - **Given** a nonblank but unregistered, incompatible, non-ASCII or overlong target ID. - **When** Replay is requested. - **Then** manager-level compatibility/syntax rejection is not supplied; provider storage or later consumer resolution can fail, rather than proving a viable replay target.

## GAP-DUR-003

**Issue:** Replay update errors, count attribution and page progress
**Classification:** Unknown behaviour

**Question/decision:** How should failed nonduplicate saves remain visible, how are concurrent additions attributed, and how does replay progress beyond already-covered oldest pages?

**Established requirement and suspected defect:** [INBOX-013](inbox/INBOX-013-replay-failure-transparency-requirement.md) already requires unreconciled persistence failures to remain visible. Concealing an unrelated save failure is a suspected implementation discrepancy, not an alternative policy awaiting approval.

**Recommendation (not approved):** Investigate that discrepancy independently of the recovery design. For recovery, provide explicit resumable progress and truthful result semantics, reconciling only evidence-backed successful/duplicate outcomes. The reconciliation rule, concurrent count attribution and continuation policy remain unapproved.

**Affected specifications:** INBOX-009, INBOX-013, INBOX-014, INBOX-015

**Evidence:**
- [src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs)
- [src/Clean.Messaging.Inbox/Runtime/ReplayResult.cs](../../src/Clean.Messaging.Inbox/Runtime/ReplayResult.cs)
- [docs/CODING_STANDARDS.md](../CODING_STANDARDS.md)

### Contract and identifier dispositions

**Affected contracts:** [INBOX-009](inbox/INBOX-009-replay-retained-admissions-into-missing-targets.md), [INBOX-013](inbox/INBOX-013-replay-failure-transparency-requirement.md), [INBOX-014](inbox/INBOX-014-undecided-admission-and-replay-policies.md), [INBOX-015](inbox/INBOX-015-unapproved-replay-failure-improvement.md).

**Reserved identifiers (not approved scenarios):** `INBOX-009-AC-02`, `INBOX-009-AC-04`, `INBOX-009-AC-06`, `INBOX-009-AC-07`, `INBOX-015-AC-01`.

**Consequences and alternatives:** A cursor/missing-target page changes selected-window semantics. Count attribution cannot be inferred from a count increase when other callers inserted rows. Failure transparency is already required, independently of the chosen reconciliation algorithm.

**Proposed identifier disposition:** Reserve original page-order, concurrent-count and swallowed-error composites. Preserve envelope/state/uniqueness and required failure transparency in Guarantees. Decide whether to restore originals or replace/split them under new IDs; the proposed reconcile-only-confirmed criterion remains unapproved.

**Implementation comparison:** Inspection only: Limit precedes target exclusion and there is no cursor. Any DbUpdateException is swallowed; the reread count difference can include other callers and zero can conceal failure.

**Original assertions retained for decision, not approved requirements:**

- `INBOX-009-AC-02`: - **Given** the selected history has existing pending, processed, dead-lettered or discarded event/target pairs. - **When** Replay is requested for the same target. - **Then** those entries retain state and are excluded from additions; matched entries still count toward the selection limit.
- `INBOX-009-AC-04`: - **Given** the oldest limited history window already has every requested target, and later history lacks it. - **When** the same limited Replay is repeated. - **Then** Matched remains the selected window size, Replayed stays zero, and later admissions are not automatically visited.
- `INBOX-009-AC-06`: - **Given** independent Replay requests select the same missing logical event/target pairs. - **When** their insertions race. - **Then** unique keys prevent duplicate rows; a DbUpdateException path reports the increase in persisted pairs, which can include another caller's additions.
- `INBOX-009-AC-07`: - **Given** a replay SaveChanges throws DbUpdateException unrelated to a duplicate and reconciliation finds no new pairs. - **When** Replay returns from its current EF catch path. - **Then** it reports selected Matched and Replayed=0 instead of rethrowing that update error; this is a suspected mismatch, not approved behaviour to require.
- `INBOX-015-AC-01`: - **Given** a replay save fails with either a confirmed concurrent uniqueness conflict or an unrelated update failure. - **When** an approved implementation of the proposal is evaluated. - **Then** unapproved proposal: return reconciled results only for confirmed successful/duplicate outcomes and preserve the other error; this criterion is not yet binding.

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

### Contract and identifier dispositions

**Affected contracts:** [INBOX-005](inbox/INBOX-005-atomic-local-delivery-and-repeatable-execution.md), [INBOX-010](inbox/INBOX-010-entry-cleanup-and-indefinite-admission-retention.md), [INBOX-014](inbox/INBOX-014-undecided-admission-and-replay-policies.md), [OPS-003](observability/OPS-003-terminal-cleanup-boundaries.md).

**Reserved identifiers (not approved scenarios):** `INBOX-010-AC-02`.

**Related established criteria remain normative:** `OPS-003-AC-01`, `OPS-003-AC-02`, `OPS-003-AC-03`, `OPS-003-AC-05`. This decision does not suspend those guarantees.

**Question/decision:** How long should incoming envelopes and target completion evidence remain, and what repeat/replay contract follows their independent deletion?

What retention and replay/dedup horizons govern admissions independently of terminal target entries?

**Recommendation (not approved):** Unapproved recommendation: Choose an explicit admission-retention and payload-deletion policy together with the target deduplication/replay horizon; do not infer indefinite retention approval.

unapproved: define admission retention separately without automatically approving indefinite storage.

**Consequences and alternatives:** Keeping admissions indefinitely enables history/replay but grows payload storage. Deleting target evidence permits repeat effects on replay; deleting admissions also ends admission-level duplicate recognition.

Deleting target evidence can permit replay while retained admission continues to suppress ordinary duplicates; admission expiry changes that horizon.

**Proposed identifier disposition:** Reserve INBOX-010-AC-02 as its original indefinite-retention/replay composite. Preserve bounded terminal cleanup and the limitation that deleted evidence cannot protect against repeats. Restore the original only on explicit approval, otherwise assign new retention/replay criteria.

Keep OPS-003-AC-05 normative only within its retained-admission premise; do not endorse indefinite admission retention.

**Implementation comparison:** Inspection only: Shared cleanup removes terminal entries only; admissions remain without supplied cleanup, and replay can recreate cleaned target entries.

Inspection-only: Generic Cleanup removes entry rows, not admissions; InboxStore checks retained admissions and creates missing consumer/event replay work.

**Original assertions retained for decision, not approved requirements:**

- `INBOX-010-AC-02`: - **Given** an old completed target has been cleaned but its original admission remains. - **When** duplicate Accept and explicit Replay for that target run. - **Then** Accept stays duplicate; Replay can create that target again, so cleaned completion no longer prevents repeated local effects.

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

### Contract and identifier dispositions

**Affected contracts:** [OUTBOX-003](outbox/OUTBOX-003-target-validation-local-selection-and-empty-batches.md), [OUTBOX-018](outbox/OUTBOX-018-undecided-duplicate-commit-and-callback-policies.md).

**Reserved identifiers (not approved scenarios):** `OUTBOX-003-AC-03`, `OUTBOX-003-AC-04`.

**Question/decision:** Must external-only outgoing work bypass absent local consumers, and is a targetless batch a valid captured operation?

**Recommendation (not approved):** Unapproved recommendation: Permit external-only routing without requiring a local consumer; decide explicitly whether zero targets is a valid no-op or an error and how capture acknowledgement follows it.

**Consequences and alternatives:** Changing default local lookup affects externally routed messages that currently fail; accepting zero targets can silently discard intended delivery unless composition and acknowledgement are explicit.

**Proposed identifier disposition:** Reserve the original default-local failure and empty-custom-batch criteria. If policy changes, use new IDs for external-only/zero-target outcomes; do not redefine the originals as generic routing criteria.

**Implementation comparison:** Inspection only: Default local provider calls exact consumer lookup and can throw before external work is staged. A custom composition can yield an empty batch and writer stages no rows.

**Original assertions retained for decision, not approved requirements:**

- `OUTBOX-003-AC-03`: - **Given** a payload has no exact-type local consumers but an external provider could supply targets and the default local provider remains installed. - **When** Create/Enqueue enumerates providers. - **Then** consumer.message_unhandled fails staging; external provider availability alone does not make default composition external-only.
- `OUTBOX-003-AC-04`: - **Given** a custom provider composition returns no targets and serialization succeeds. - **When** factory and writer Stage run. - **Then** IsEmpty is true and no entries are added; empty batch staging is not external delivery.

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

### Contract and identifier dispositions

**Affected contracts:** [INBOX-002](inbox/INBOX-002-independent-atomic-admission-and-duplicate-retention.md), [INBOX-014](inbox/INBOX-014-undecided-admission-and-replay-policies.md), [OUTBOX-004](outbox/OUTBOX-004-tracked-duplicate-suppression-versus-persisted-uniqueness.md), [OUTBOX-018](outbox/OUTBOX-018-undecided-duplicate-commit-and-callback-policies.md), [TRANSPORT-005](transport/TRANSPORT-005-admission-failures-duplicates-and-cancellation.md).

**Reserved identifiers (not approved scenarios):** `INBOX-002-AC-03`, `OUTBOX-004-AC-01`, `TRANSPORT-005-AC-03`.

**Question/decision:** When an incoming admission or outgoing event/target identity is reused with different payload, metadata or routing, should the API reject, compare, or retain the first content?

Which identity/payload equivalence contract distinguishes legitimate duplicates from conflicting operations? Extend the existing retained outgoing content-conflict question to the newly reserved inbound admission case without assuming identical mechanisms.

**Recommendation (not approved):** Unapproved recommendation: Define one immutable identity/content conflict policy across admission and outgoing target deduplication; equivalent retries and conflicting reuse must be distinguished explicitly.

unapproved: Specify identity uniqueness and explicit conflicting-content disposition; retain original committed data and decide whether conflicting admission is rejected or reported distinctly rather than silently deemed an equivalent replay.

**Consequences and alternatives:** Comparison needs a defined equivalence rule and may add persistence reads. First-write retention avoids mutation but can conceal caller errors; unconditional idempotent enqueue is not currently supplied. Newly registered incoming targets must not be silently introduced by a transport retry.

The existing gap concerns outgoing tracked/persisted conflicts; inbound either-ID suppression exposes the same underlying content-equivalence choice. Resolving outgoing behavior alone does not automatically approve inbound conflicting acceptance.

**Proposed identifier disposition:** Reserve INBOX-002-AC-03 and OUTBOX-004-AC-01 whole, retaining their exact original obligations. Established duplicate recognition and per-target uniqueness remain guarantees; split conflict handling from equivalent retry in new IDs only after approval.

Reserve every original ID and exact heading anchor. Restore unchanged only if its entire original meaning is approved; otherwise retire/reserve it and assign fresh IDs to approved replacements/splits. Update test mappings only after approval.

**Implementation comparison:** Inspection only: Inbox suppresses either retained admission identity without content comparison or expanding targets. Outbox suppresses only nondeleted tracked event/target pairs without content comparison; persisted untracked duplicates can fail save.

Inspection-only: InboxStore checks either MessageId or EventId and returns duplicate acceptance without comparing serialized payload/context or replacing stored data. This is not approval to reuse identity for distinct operations.

**Original assertions retained for decision, not approved requirements:**

- `INBOX-002-AC-03`: - **Given** a retained admission, including terminal targets, and a new envelope reusing MessageId or EventId but different payload/metadata or new registrations. - **When** Accept finishes. - **Then** the existing original envelope and target set remain unchanged; no new-consumer expansion or conflict report is supplied.
- `OUTBOX-004-AC-01`: - **Given** same logical event/target is already tracked nondeleted with possibly different payload; another target is absent. - **When** Stage runs in that context. - **Then** existing tracked pair is left unchanged and only the missing target is staged; no payload-equivalence guarantee is given.
- `TRANSPORT-005-AC-03`: **Given** a retained admission and a newly mapped envelope sharing either message ID or event ID but carrying different payload/context, **When** admission reaches the built-in EF store, **Then** it returns as already accepted without replacing the stored admission or comparing payload equivalence. This is an observed conflict policy, not approval for identity reuse for distinct operations.

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

### Contract and identifier dispositions

**Affected contracts:** [DOMAIN-005](domain-events/DOMAIN-005-atomic-captured-facts-and-application-commit.md), [DOMAIN-007](domain-events/DOMAIN-007-invalid-custom-facts-and-targetless-acknowledgement.md), [DOMAIN-009](domain-events/DOMAIN-009-crash-recovery-and-retention-boundaries.md), [INBOX-005](inbox/INBOX-005-atomic-local-delivery-and-repeatable-execution.md), [OUTBOX-008](outbox/OUTBOX-008-commit-verification-and-uncertain-outcome.md), [OUTBOX-018](outbox/OUTBOX-018-undecided-duplicate-commit-and-callback-policies.md).

**Reserved identifiers (not approved scenarios):** `OUTBOX-008-AC-01`, `OUTBOX-008-AC-02`, `OUTBOX-008-AC-04`.

**Related established criteria remain normative:** `DOMAIN-005-AC-01`, `DOMAIN-005-AC-02`, `DOMAIN-005-AC-03`, `DOMAIN-007-AC-03`, `DOMAIN-009-AC-01`, `DOMAIN-009-AC-02`, `DOMAIN-009-AC-03`. This decision does not suspend those guarantees.

**Question/decision:** What evidence establishes commit or rollback after a failed response, how does retention affect it, and what lifetime/cancellation bound applies to independent verification?

What evidence establishes capture commit when keys are missing, cleaned or absent and how should callers reconcile unknown outcomes?

**Recommendation (not approved):** Unapproved recommendation: Do not infer rollback solely from absent keys; keep unavailable, expired or empty evidence unknown. Define bounded verification independent of the failed request token, while preserving distinct cancellation and database outcome.

unapproved: preserve unknown separately from rollback/success and establish evidence scope/lifetime before strengthening reconciliation.

**Consequences and alternatives:** All-key queries can become misleading after cleanup; cancellation-independent verification can run beyond request shutdown unless its lifetime is bounded. Changes affect callback release and recovery, not local atomicity.

Absent evidence is not proof of rollback; cleanup and zero-key participation constrain verification.

**Proposed identifier disposition:** Reserve key-presence success, missing-key rollback/unknown and CancellationToken.None composites. Restore those originals only if approved; otherwise create new evidence and verification-lifetime criteria. Keep zero-evidence uncertainty normative.

Keep DOMAIN-007-AC-03 and DOMAIN-009 boundaries normative without selecting a reconciliation protocol; pending evidence policy remains open.

**Implementation comparison:** Inspection only: Fresh-context verifier checks all pending keys. Missing keys clear tracking as rollback; verifier failure or zero keys leaves unknown. Async verification uses CancellationToken.None without a dedicated verification timeout.

Inspection-only: Zero keys lead to unknown. Verification checks retained outbox keys; missing keys can be treated as rollback while verification/callback exceptions produce unknown. Real races are unexecuted.

**Original assertions retained for decision, not approved requirements:**

- `OUTBOX-008-AC-01`: - **Given** transaction commit notification fails after database commit and every pending outgoing key remains queryable. - **When** fresh-context verification succeeds. - **Then** tracker acknowledges commit and can wake outgoing processing while original commit-failure handling remains subject to EF behaviour.
- `OUTBOX-008-AC-02`: - **Given** commit fails during marked commit with some expected keys absent, or verifier throws. - **When** transaction failure interception runs. - **Then** absent-key evidence selects rollback tracking; verification error selects unknown state and no definite acknowledgement.
- `OUTBOX-008-AC-04`: - **Given** the commit token is cancelled and a marked commit requires async verification. - **When** the failure hook verifies. - **Then** verification uses an independent None token; its success/error, not the cancelled commit token alone, controls tracker reconciliation.

## GAP-DUR-009

**Issue:** Postcommit callback failure and callback-only participation
**Classification:** Unknown behaviour

**Question/decision:** What participates in postcommit acknowledgement, how are callback identities separated, and how should callback failures affect later acknowledgements, recovery, wake hints and error reporting? This includes nonthrowing, isolation or retry obligations, callback-only participation and verified-commit reconciliation.

**Recommendation (not approved):** Separate durable commit truth from callback failure, define recoverable acknowledgement and error visibility, and decide callback identity scope and callback-only participation explicitly. Do not infer a durable callback ledger or approve swallowed failures.

**Affected specifications:** OUTBOX-010, OUTBOX-018, DOMAIN-008, DOMAIN-010

**Evidence:**
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs)
- [src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs](../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs)

**Policy impact:** Error isolation and internal cleanup must be decided without confusing callback failure with commit truth. No durable callback ledger is implied. Callback-only participation is an explicit part of the decision, not a scenario to drop by default. The reserved identifiers below remain unchanged pending approval.

### Contract and identifier dispositions

**Affected contracts:** [DOMAIN-005](domain-events/DOMAIN-005-atomic-captured-facts-and-application-commit.md), [DOMAIN-008](domain-events/DOMAIN-008-custom-source-collisions-and-acknowledgement-failures.md), [DOMAIN-010](domain-events/DOMAIN-010-undecided-source-and-acknowledgement-contract.md), [OUTBOX-006](outbox/OUTBOX-006-public-save-capture-protocol-and-per-context-deduplication.md), [OUTBOX-010](outbox/OUTBOX-010-postcommit-callbacks-and-wake-hints.md), [OUTBOX-018](outbox/OUTBOX-018-undecided-duplicate-commit-and-callback-policies.md).

**Reserved identifiers (not approved scenarios):** `DOMAIN-008-AC-02`, `DOMAIN-008-AC-04`, `OUTBOX-006-AC-02`, `OUTBOX-010-AC-02`, `OUTBOX-010-AC-03`, `OUTBOX-010-AC-04`.

**Consequences and alternatives:** Callbacks run after durable commit and cannot roll it back. Retry/recovery semantics are needed to avoid lost later callbacks. Changing event-only callback keys or callback-only participation affects public capture integrations.

**Proposed identifier disposition:** Reserve callback-key composite OUTBOX-006-AC-02 and OUTBOX-010-AC-02/03/04 without narrowing. Keep per-context capture isolation, commit-before-acknowledgement and durable recoverability guarantees. Restore originals only on explicit approval, otherwise split under new IDs.

Reserve original composite/error-handling IDs in full. Keep irreversible committed-state boundaries as guarantees; any isolated-callback outcome needs an explicit identity migration.

**Implementation comparison:** Callbacks deduplicate only by event ID, and `PendingCommit.IsEmpty` ignores callback-only work. `OutboxTracker.Commit` invokes synchronous callbacks before wake/removal without isolating their errors. A throw can interrupt later callbacks, wake and cleanup; transaction-failure reconciliation can swallow the callback error and mark tracker state unknown. These observations do not resolve the policy.

**Original assertions retained for decision, not approved requirements:**

- `DOMAIN-008-AC-02`: DOMAIN-008-AC-02 — Expose throwing acknowledgement after commit  - **Given** custom source Acknowledge throws and other sources also have callbacks. - **When** owning transaction has committed and callbacks run. - **Then** durable state can coexist with pending source events and interrupted later callbacks/wake; error does not imply transaction rollback.
- `DOMAIN-008-AC-04`: DOMAIN-008-AC-04 — Preserve observed ambiguity of callback error handling  - **Given** custom acknowledgement throws during commit-failure reconciliation after outgoing keys were verified. - **When** the enclosing transaction-failure catch handles it. - **Then** the callback error can be swallowed into unknown tracker state rather than universally surfaced; committed rows and unacknowledged source facts may coexist.
- `OUTBOX-006-AC-02`: - **Given** two contexts or two different capture types encounter the same event ID. - **When** Capture operates inside each active save. - **Then** capture state is independent per context/type; acknowledgement callbacks keyed only by eventId do not imply one callback per capture type.
- `OUTBOX-010-AC-02`: - **Given** a custom acknowledgement callback throws before later callbacks/wake/state removal. - **When** commit bookkeeping runs. - **Then** the database can already be committed while acknowledgement sequence/wake/cleanup is interrupted; recovery can still discover persisted rows.
- `OUTBOX-010-AC-03`: - **Given** a custom integration registers acknowledgement only with no captures or keys. - **When** save/transaction bookkeeping examines pending state. - **Then** callbacks alone do not make PendingCommit nonempty; registration is not a proven durable postcommit scheduling facility.
- `OUTBOX-010-AC-04`: - **Given** a callback throws while verified committed handling runs inside the transaction-failure reconciliation catch. - **When** the interceptor handles that exception. - **Then** the reconciliation catch can classify tracker state as unknown without rethrowing that callback exception; durable commit remains possible and later callbacks/wake/cleanup can be incomplete.

## GAP-DUR-010

**Issue:** Custom event source identity, enumeration and acknowledgement assumptions
**Classification:** Unknown behaviour

**Question/decision:** Which source-ID conflict, collection-view lifetime/stability, payload ownership, validation timing and identity-specific acknowledgement rules bind custom sources? How are same-ID different-source conflicts and callback errors exposed?

**Recommendation (not approved):** Retain single-owner valid-source operation and explicitly decide stable identity, conflict visibility and acknowledgement obligations. Assess live-view and constructor compatibility before adding validation, snapshots or locking; traversal order is not sufficient authority for a collision policy.

**Affected specifications:** DOMAIN-004, DOMAIN-008, DOMAIN-010

**Evidence:**
- [src/Clean.Messaging.DomainEvents/RaisedEvent.cs](../../src/Clean.Messaging.DomainEvents/RaisedEvent.cs)
- [src/Clean.Messaging.DomainEvents/AggregateRoot.cs](../../src/Clean.Messaging.DomainEvents/AggregateRoot.cs)
- [src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs](../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs)

**Policy impact:** Collision rejection would change the first-source outcome retained under `DOMAIN-008-AC-01`. Invalid-fact capture requirements do not independently establish constructor validation, snapshots or concurrent source safety. Keep single-owner enumeration and source-owned acknowledgement assumptions distinct from those possible extensions; reserved criteria remain intact until the policy is approved.

### Contract and identifier dispositions

**Affected contracts:** [DOMAIN-001](domain-events/DOMAIN-001-raise-pending-events-without-owning-business-policy.md), [DOMAIN-004](domain-events/DOMAIN-004-capture-tracked-sources-across-entity-states.md), [DOMAIN-007](domain-events/DOMAIN-007-invalid-custom-facts-and-targetless-acknowledgement.md), [DOMAIN-008](domain-events/DOMAIN-008-custom-source-collisions-and-acknowledgement-failures.md), [DOMAIN-010](domain-events/DOMAIN-010-undecided-source-and-acknowledgement-contract.md).

**Reserved identifiers (not approved scenarios):** `DOMAIN-001-AC-03`, `DOMAIN-007-AC-01`, `DOMAIN-008-AC-01`, `DOMAIN-008-AC-03`.

**Consequences and alternatives:** Stronger validation, snapshots or collision rejection affect custom sources and public compatibility; approving source order would risk silent fact loss.

**Proposed identifier disposition:** Reserve all affected original IDs intact, including composites; retain independently established invalid capture and ownership boundaries in Guarantees. After approval assign explicitly migrated/split criteria rather than reusing IDs silently.

**Implementation comparison:** Inspection-only: DomainEventCapture deduplicates same IDs across sources and directly enumerates source collections; AggregateRoot exposes a live view; RaisedEvent has no constructor guards; custom acknowledgement removal is not verified.

**Original assertions retained for decision, not approved requirements:**

- `DOMAIN-001-AC-03`: DOMAIN-001-AC-03 — Expose live pending collection  - **Given** a caller obtained the Events read-only view before another Raise or acknowledgement. - **When** it observes the view afterward. - **Then** the view reflects current pending events rather than an immutable snapshot; no concurrent mutation safety is promised.
- `DOMAIN-007-AC-01`: DOMAIN-007-AC-01 — Fail invalid custom fact capture  - **Given** custom source returns empty Id, null/unserializable payload, missing contract or default no-consumer payload. - **When** owning save capture runs. - **Then** factory failure aborts save capture and source facts stay pending; invalid RaisedEvent construction alone had not rejected input.
- `DOMAIN-008-AC-01`: DOMAIN-008-AC-01 — Observe duplicate IDs across tracked sources  - **Given** two sources supply the same event ID, possibly different payloads. - **When** capture scans both inside one transaction. - **Then** only the first authorized capture contributes work/ack; the second can remain pending, without a payload-conflict error or global durable dedup guarantee.
- `DOMAIN-008-AC-03`: DOMAIN-008-AC-03 — Do not infer retry-safe custom acknowledgement  - **Given** a custom source removes more facts, does nothing or mutates collection during acknowledgement. - **When** normal callback invokes its Acknowledge. - **Then** integration does not independently verify event removal or enforce AggregateRoot semantics; observable results depend on the custom source.

## GAP-DUR-011

**Issue:** Targetless captured fact acknowledgement meaning
**Classification:** Unknown behaviour

**Question/decision:** Should a valid zero-target domain capture reject, remain pending, or acknowledge after an observed commit?

**Recommendation (not approved):** Choose the zero-target contract explicitly; never equate acknowledgement with delivered work. A rejection policy would change the original targetless acknowledgement assertion under `DOMAIN-007-AC-02` and require an approved identity migration, not silent replacement.

**Affected specifications:** DOMAIN-007, DOMAIN-010, OUTBOX-018

**Evidence:**
- [src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs](../../src/Clean.Messaging.DomainEvents.EntityFrameworkCore/DomainEventCapture.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs](../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTracker.cs)

### Contract and identifier dispositions

**Affected contracts:** [DOMAIN-007](domain-events/DOMAIN-007-invalid-custom-facts-and-targetless-acknowledgement.md), [DOMAIN-010](domain-events/DOMAIN-010-undecided-source-and-acknowledgement-contract.md).

**Reserved identifiers (not approved scenarios):** `DOMAIN-007-AC-02`.

**Consequences and alternatives:** Rejecting or retaining currently acknowledged targetless facts changes save/ack behavior; acknowledging loses pending facts without delivery evidence.

**Proposed identifier disposition:** Retain DOMAIN-007-AC-02 as reserved for its original targetless acknowledgement assertion; both original DOMAIN-010 questions remain open.

**Implementation comparison:** Inspection-only: DomainEventCapture queues acknowledgement before factory/writer; an empty batch stages no rows but observed commit can acknowledge.

**Original assertions retained for decision, not approved requirements:**

- `DOMAIN-007-AC-02`: DOMAIN-007-AC-02 — Observe targetless successful acknowledgement  - **Given** a valid event uses custom providers returning no targets. - **When** save and observed commit succeed. - **Then** no outgoing target rows are committed but the captured source event can still be acknowledged; this is observed, not an approved delivery guarantee.

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

### Contract and identifier dispositions

**Affected contracts:** [SAGA-004](sagas/SAGA-004-identity-values-and-durable-correlation.md), [SAGA-015](sagas/SAGA-015-undecided-identity-schema-and-diagnostic-hardening.md), [SCHED-015](scheduling/SCHED-015-undecided-integration-hardening.md).

**Reserved identifiers (not approved scenarios):** `SAGA-015-Q-01`, `SCHED-015-Q-01`.

**Question/decision:** Where are default ScheduleTarget and SagaKey values rejected, and what failure is exposed?

**Recommendation (not approved):** Unapproved: Define validation at the owning admission boundary without assuming constructors cover default structs.

**Consequences and alternatives:** Approval establishes policy and evidence obligations; rejection requires explicit criterion retirement or new identities rather than silent semantic reuse.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: ScheduleWriter and ordinary saga routing do not revalidate default targets/keys.

**Original assertions retained for decision, not approved requirements:**

- `SAGA-015-Q-01`: **Scenario:** correlation returns default(SagaKey) bypassing constructor validation. admission hardening is approved. **Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-001 approves revalidation, handling rejects the invalid key before durable creation; current code does not promise this rejection.
- `SCHED-015-Q-01`: **Scenario:** default(ScheduleTarget) bypassed its constructor. a policy for writer admission is approved. **Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-001 chooses validation, the invalid target is rejected before durable mutation; current writer does not guarantee that outcome.

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

### Contract and identifier dispositions

**Affected contracts:** [SAGA-009](sagas/SAGA-009-outgoing-metadata-timers-and-terminal-effect-ordering.md), [SCHED-006](scheduling/SCHED-006-staging-duplicate-insertion-and-owner-visibility.md).

**Reserved identifiers (not approved scenarios):** `SAGA-009-AC-06`, `SCHED-006-AC-04`, `SCHED-006-AC-05`.

**Question/decision:** What result is promised for equal retries, differing payloads and concurrent insertion of one ScheduleId?

**Recommendation (not approved):** Unapproved: Define owner retry and duplicate-result semantics separately from the already-required stable identity obligation.

**Consequences and alternatives:** Approval establishes policy and evidence obligations; rejection requires explicit criterion retirement or new identities rather than silent semantic reuse.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: ScheduleStore.Add stages inserts; primary-key uniqueness is not an idempotent result or an upsert API.

**Original assertions retained for decision, not approved requirements:**

- `SAGA-009-AC-06`: **Given** two results/sagas use the same global timer ID or a retained timer already has it. **When** effects are saved. **Then** there is no timer upsert; identity conflict can fail save and participating state/effects require owner rollback.
- `SCHED-006-AC-04`: **Given** a retained schedule with ID X and a retry using ID X with equal or changed payload/due/group. **When** the retry is staged and saved. **Then** the existing row is not replaced or rescheduled; duplicate tracked/database identity can reject the attempt rather than return idempotent success.
- `SCHED-006-AC-05`: **Given** two contexts stage the same ScheduleId. **When** they compete to commit. **Then** at most one row with that identity is committed; the losing insertion is not guaranteed a domain-specific duplicate result.

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

### Contract and identifier dispositions

**Affected contracts:** [SCHED-009](scheduling/SCHED-009-due-eligibility-claims-and-bounded-execution.md), [SCHED-010](scheduling/SCHED-010-expiry-before-admission-and-asymmetric-settlement.md), [WORK-004](recovery-lifecycle/WORK-004-queue-leases-and-producer-delay.md).

**Reserved identifiers (not approved scenarios):** `SCHED-009-AC-04`, `SCHED-010-Q-01`, `SCHED-010-Q-04`, `WORK-004-AC-01`.

**Question/decision:** How does ownership remain usable while claimed work waits for capacity or execution outlasts a lease?

Should queued claimed work be renewed, claimed just in time, revalidated before invocation, or allowed to expire while settlement remains fenced?

**Recommendation (not approved):** Unapproved: Follow the existing GAPS C5 proposal: claim only work that fits immediately usable execution capacity, with no scheduling renewal initially. Integrate WORK-004 queue protection under this same decision so a protected local queue does not merely retain a preleased backlog that cannot start. Long-running admitted work must use the separately selected GAP-WF-004 completion/failure fence; do not import Inbox/Outbox renewal promises.

unapproved: keep current-owner settlement mandatory; evaluate just-in-time acquisition or pre-invocation validation before promising queued lease freshness. Do not silently prohibit queue renewal.

**Consequences and alternatives:** Capacity-aligned claiming reduces lease expiry before admission, duplicated recovery and budget consumption by work waiting for a slot. With the current batch-waits-for-all loop, smaller batches can leave capacity idle behind one slow item; continuously replenishing available slots is a throughput alternative but requires coordination with worker queue protection. No renewal means a legitimately running handler can still outlast its lease: GAP-WF-004 decides whether its success is protected by a continuously held transaction fence and whether its later Retry/Fail is rejected. GAP-CLAIM-001 must separately settle whether acquired-but-never-invoked claims spend budget; queue protection alone does not establish an invocation cap. Renewal supports long execution but adds heartbeat failure and ownership-loss behavior; preleasing larger batches keeps throughput simpler at the cost of more expired queued work and lease sizing tied to worst-case backlog.

Long queue waits can expire claims before processing and allow repeated invocation. New protection changes resource use, recovery latency and ownership behavior.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

Reserve composite WORK-004-AC-01 intact; keep valid-ownership settlement in Guarantees. An approved queue strategy requires a migrated/split acceptance identity. Consolidated under existing GAP-WF-003 for parent merging with scheduling; shared-worker processing renewal does not imply queued ownership protection before invocation.

**Implementation comparison:** Inspection only: Whole batches are leased before parallel slots; scheduling has no renewal loop.

Inspection-only: WorkLoop claims before enqueueing; renewal starts in the processors and does not independently protect channel residence.

**Original assertions retained for decision, not approved requirements:**

- `SCHED-009-AC-04`: **Given** a selected schedule waits behind busy execution slots. **When** the claim is committed but delivery has not started. **Then** Attempts has already increased; the counter does not purport to count handler calls or EF strategy repeats.
- `SCHED-010-Q-01`: **Scenario:** a batch exceeds concurrency and a waiting row lease expires before admission; the dispatch gate is reached after expiry. **Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-003, if current no-renewal behaviour is retained, that row performs no delivery and later recovery may reclaim it?
- `SCHED-010-Q-04`: **Scenario:** slow delivery and a preleased backlog threaten expiry; an ownership policy is selected. **Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-003, a chosen renewal or lease-at-admission policy must define whether queued work remains owned and how ownership loss cancels execution; no unconditional outcome is approved?
- `WORK-004-AC-01`: WORK-004-AC-01 — Queued lease expiry  **Given** one claimed target waits behind a held handler longer than its lease. **When** its processing slot becomes available. **Then** the queue did not independently renew that target; a completion still requires valid current ownership.

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

### Contract and identifier dispositions

**Affected contracts:** [SCHED-009](scheduling/SCHED-009-due-eligibility-claims-and-bounded-execution.md), [SCHED-010](scheduling/SCHED-010-expiry-before-admission-and-asymmetric-settlement.md), [SCHED-011](scheduling/SCHED-011-delivery-transaction-routing-and-retained-contract-failures.md).

**Reserved identifiers (not approved scenarios):** `SCHED-009-AC-01`, `SCHED-010-Q-02`, `SCHED-010-Q-03`, `SCHED-011-AC-01`.

**Question/decision:** What expiry rule governs admission, successful completion and retry/failure settlement?

**Recommendation (not approved):** Unapproved: Follow the existing GAPS C5/D2 split: Retry/Fail settlement requires current unexpired ownership; successful completion after admission may outlive the nominal lease only when a continuously held owner transaction fence is proven to prevent successor acquisition through completion. Without that verified fence, reject the expired candidate completion rather than authorize it by token equality alone. Keep exact deadline equality under GAP-CLAIM-002.

**Consequences and alternatives:** After expiry, an ordinary delivery failure cannot postpone or terminally fail a schedule merely because its token has not yet been replaced; recovery owns the next attempt and detailed failed-attempt evidence may remain only in diagnostics. Under GAP-WF-003 no-renewal capacity policy, long successful work either relies on proven lock-held completion or must roll back and repeat, so lease duration must cover intended work or renewal must be separately chosen. Provider transactions must establish that a successor cannot claim through the completion fence; current code structure alone is not proof. External effects may already have happened before a rejected completion, so this does not give exactly-once delivery. A strict final-expiry check everywhere is simpler to state but can repeatedly discard valid long work; token-only settlement avoids that waste but permits an expired owner to alter recovery when no successor yet replaced it. Abandoned-claim accounting and bounded queue admission must be reviewed together to avoid exhaustion driven by lease loss rather than actual failed delivery.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: Admission requires an unexpired lease; retry/fail are token-only; admitted completion has no final expiry predicate.

**Original assertions retained for decision, not approved requirements:**

- `SCHED-009-AC-01`: **Given** rows differ by terminal status, original due, retry due and lease expiry. **When** claim runs at time T. **Then** only nonterminal rows with DueAt <= T, retry absent or <= T, and lease absent or <= T become owned.
- `SCHED-010-Q-02`: **Scenario:** a dispatched attempt fails after its lease expired but before another claim replaces its token; failure/retry settlement runs. **Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-004, if current token-only failure fencing is retained, the expired unreclaimed attempt can record retry/failure; a replaced token cannot?
- `SCHED-010-Q-03`: **Scenario:** the start gate succeeded before expiry and no competing successful mutation intervenes; same-context delivery and commit finish after expiry. **Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-004, if current admission fencing is retained, completion can commit without a final unexpired-lease predicate; no exactly-once external claim follows?
- `SCHED-011-AC-01`: **Given** a row is absent, terminal, wrong-token or expired. **When** dispatch attempts to enter. **Then** no outgoing effect is staged by that attempt; absent/terminal is settled, an active row owned elsewhere is not represented as that attempt success.

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

### Contract and identifier dispositions

**Affected contracts:** [SCHED-014](scheduling/SCHED-014-quartz-wake-adapter-fallback-reconnect-and-shutdown.md), [SCHED-015](scheduling/SCHED-015-undecided-integration-hardening.md).

**Reserved identifiers (not approved scenarios):** `SCHED-014-AC-04`, `SCHED-015-Q-02`.

**Question/decision:** Does a hung Quartz operation need a bounded or concurrent local fallback?

**Recommendation (not approved):** Unapproved: Define responsiveness and recovery bounds before asserting a timeout or reconnect contract.

**Consequences and alternatives:** Approval establishes policy and evidence obligations; rejection requires explicit criterion retirement or new identities rather than silent semantic reuse.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: The adapter awaits ScheduleJob before starting its local wait; throwing failures fall back, hung calls can block.

**Original assertions retained for decision, not approved requirements:**

- `SCHED-014-AC-04`: **Given** a prior Quartz scheduling failure and a subsequently restored service. **When** another future Wait occurs. **Then** scheduling is attempted again; no adapter reconnect guarantee or permanently disabled state is implied.
- `SCHED-015-Q-02`: **Scenario:** ScheduleJob neither returns nor throws. a bounded-wait policy is approved. **Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-005 approves a timeout/concurrent fallback, local wake/deadline remains responsive within that bound; current fallback awaits scheduler I/O first.

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

### Contract and identifier dispositions

**Affected contracts:** [SAGA-011](sagas/SAGA-011-owner-transactions-rollback-savepoints-and-replay.md), [SCHED-006](scheduling/SCHED-006-staging-duplicate-insertion-and-owner-visibility.md), [SCHED-013](scheduling/SCHED-013-local-wake-engine-commit-hints-and-polling-recovery.md), [SCHED-015](scheduling/SCHED-015-undecided-integration-hardening.md).

**Reserved identifiers (not approved scenarios):** `SCHED-015-Q-03`.

**Question/decision:** Which caller-managed savepoints and uncertain commit recovery patterns are supported across saga, schedule and outbox effects?

**Recommendation (not approved):** Unapproved: Define a shared owner boundary before promising savepoint restoration or commit verification.

**Consequences and alternatives:** Approval establishes policy and evidence obligations; rejection requires explicit criterion retirement or new identities rather than silent semantic reuse.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: Outbox capture rejects caller savepoints outside saves; scheduling tracking observes commit/failure, not arbitrary rollback-to-savepoint.

**Original assertions retained for decision, not approved requirements:**

- `SCHED-015-Q-03`: **Scenario:** a caller creates or rolls back savepoints while schedules/saga/outbox effects share a context. the ownership policy is selected. **Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-006 approves support, rolled-back effects and wake observation must match persisted truth; current outbox integration rejects caller savepoints outside active saves and scheduling alone does not track caller rollback-to-savepoint.

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

### Contract and identifier dispositions

**Affected contracts:** [SAGA-004](sagas/SAGA-004-identity-values-and-durable-correlation.md), [SAGA-015](sagas/SAGA-015-undecided-identity-schema-and-diagnostic-hardening.md).

**Reserved identifiers (not approved scenarios):** `SAGA-015-Q-02`.

**Question/decision:** Is equality of persisted type/key provider-independent or part of supported database collation?

**Recommendation (not approved):** Unapproved: Make equality a deployment contract or explicitly constrain providers; do not infer persisted ordinal comparison from registry lookup.

**Consequences and alternatives:** Approval establishes policy and evidence obligations; rejection requires explicit criterion retirement or new identities rather than silent semantic reuse.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: Registry type lookup is ordinal; unique type/key equality follows provider collation.

**Original assertions retained for decision, not approved requirements:**

- `SAGA-015-Q-02`: **Scenario:** keys/types differ only by case or provider collation treatment. a supported equality policy is chosen. **Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-007 approves ordinal persisted semantics, those identities remain distinct consistently across providers; current collation is provider-owned.

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

### Contract and identifier dispositions

**Affected contracts:** [SAGA-006](sagas/SAGA-006-handler-results-and-immutable-effect-descriptions.md), [SAGA-014](sagas/SAGA-014-failures-cancellation-and-malformed-result-intent.md), [SAGA-015](sagas/SAGA-015-undecided-identity-schema-and-diagnostic-hardening.md).

**Reserved identifiers (not approved scenarios):** `SAGA-014-Q-01`, `SAGA-015-Q-03`.

**Question/decision:** What admission behavior applies to null, blank and overlong SagaResult.Fail diagnostics?

**Recommendation (not approved):** Unapproved: Select parity with SagaFailure or an explicitly different result contract.

**Consequences and alternatives:** Approval establishes policy and evidence obligations; rejection requires explicit criterion retirement or new identities rather than silent semantic reuse.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: SagaFailure validates nonblank 200/500 lengths; SagaResult.Fail passes raw values into state persistence.

**Original assertions retained for decision, not approved requirements:**

- `SAGA-014-Q-01`: **Scenario:** null/blank code/message, code lengths 200/201 and message lengths 500/501; SagaFailure is constructed versus SagaResult.Fail used. **Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-008, if current disparity is retained, SagaFailure rejects invalid/overlong values while Fail accepts them until provider/state persistence; tests must not impose universal provider rejection?
- `SAGA-015-Q-03`: **Scenario:** Fail receives invalid diagnostic values. GAP-WF-008 chooses validation parity. **Policy question:** Should the following conditional proposal be adopted: only under that decision, malformed Fail rejects before applying state/effects using agreed 200/500 bounds; this is not current validated behaviour.

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

### Contract and identifier dispositions

**Affected contracts:** [SAGA-013](sagas/SAGA-013-start-collision-translation-and-recovery-outcomes.md), [SAGA-015](sagas/SAGA-015-undecided-identity-schema-and-diagnostic-hardening.md).

**Reserved identifiers (not approved scenarios):** `SAGA-013-AC-01`, `SAGA-013-AC-04`, `SAGA-013-AC-05`, `SAGA-015-Q-04`.

**Question/decision:** What evidence permits translating a save failure into same-start race, conflict or closed outcomes?

**Recommendation (not approved):** Unapproved: Follow the existing GAPS C1 proposal: require positive evidence of the relevant saga type/key uniqueness collision before translating a save failure; use a persisted winner only to select the retained-start outcome after that proof. If the provider cannot establish the relevant identity collision, preserve the original save error even when a matching winner exists. Keep public different-start Conflict/Closed outcomes and the existing same-start race/parity questions separate.

**Consequences and alternatives:** An incidental winner can no longer turn a constraint, connection or other unrelated save failure into a permanent conflict/closed result, avoiding false terminal classification and hidden infrastructure failures. Conservative fallback means some real collisions remain ordinary provider errors until an adapter recognizes the relevant violation; owners then apply their established failure policy rather than an invented saga classification. Provider-specific positive evidence must stay at the persistence boundary and cannot rely only on DbUpdateException, entry count or a matching row. GAP-WF-010 decides what happens when proof/winner lookup is cancelled; a cancelled or failed lookup must not fabricate a proven collision. The broad-heuristic alternative is provider-simple but risks permanently abandoning valid starts and masking the original fault. Same-start retry identity and owner atomicity remain necessary because both competing handlers may have executed before one loses.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: One Added identity plus a fresh persisted winner qualifies any DbUpdateException, including potentially unrelated failures.

**Original assertions retained for decision, not approved requirements:**

- `SAGA-013-AC-01`: **Given** two contexts insert the same type/key/StartedByMessageId and one commits. **When** loser save fails with one qualifying added identity and finds winner. **Then** failure is translated to retry-classified saga.start.race; a later replay sees the retained start and is a no-op.
- `SAGA-013-AC-04`: **Given** error is not DbUpdateException, has zero/multiple distinct Added starts, has no persisted winner or lookup itself fails. **When** translation is considered. **Then** the original failure is preserved rather than fabricating conflict/closed/race without qualifying evidence.
- `SAGA-013-AC-05`: **Given** equivalent qualifying insertion races via SaveChanges and SaveChangesAsync. **When** winner lookup succeeds. **Then** both expose the same race/conflict/closed classification for the same retained start relationship.
- `SAGA-015-Q-04`: **Scenario:** unrelated DbUpdateException has one added start and a coincidental persisted winner. translation hardening is selected. **Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-009 restricts translation to proven identity collisions, the unrelated original error remains; current matching is not unique-error-code-specific.

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

### Contract and identifier dispositions

**Affected contracts:** [SAGA-013](sagas/SAGA-013-start-collision-translation-and-recovery-outcomes.md), [SAGA-014](sagas/SAGA-014-failures-cancellation-and-malformed-result-intent.md).

**Reserved identifiers (not approved scenarios):** `SAGA-013-AC-04`, `SAGA-014-Q-02`.

**Question/decision:** Should cancellation during winner lookup replace or preserve the original database save failure?

**Recommendation (not approved):** Unapproved: Follow the existing GAPS C3 proposal: propagate cancellation attributable to the supplied caller token during collision lookup; preserve the original save failure for other lookup failures. Retain both events in appropriate restricted diagnostics when needed, rather than replacing caller cancellation with a new business Fail or fabricated race/conflict/closed outcome.

**Consequences and alternatives:** Host shutdown/caller abort remains distinguishable from retryable or permanent workflow failure, and an incomplete GAP-WF-009 proof lookup cannot manufacture a collision classification. The original save error may no longer be the surfaced exception when caller cancellation wins, so diagnostics must preserve enough context for investigation without changing public precedence. Cancellation does not prove rollback: owners still reconcile uncertain committed state, and replay uses retained start identity/fencing instead of creating a replacement operation. Preserving the save error for every lookup exception keeps one stable causal error but hides cancellation and may drive unintended retries or faults during shutdown; always surfacing lookup errors loses the original failure even for incidental lookup faults. Cancellation from an unrelated source must not be mislabeled as the caller's abort.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: Winner lookup catches cancellation and preserves the original DbUpdateException.

**Original assertions retained for decision, not approved requirements:**

- `SAGA-013-AC-04`: **Given** error is not DbUpdateException, has zero/multiple distinct Added starts, has no persisted winner or lookup itself fails. **When** translation is considered. **Then** the original failure is preserved rather than fabricating conflict/closed/race without qualifying evidence.
- `SAGA-014-Q-02`: **Scenario:** qualifying DbUpdateException and caller cancellation during fresh winner lookup; async collision resolution runs. **Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-010, if current fallback is retained, the original DbUpdateException remains instead of cancellation propagation; normative cancellation policy needs a deliberate decision?

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

### Contract and identifier dispositions

**Affected contracts:** [SAGA-004](sagas/SAGA-004-identity-values-and-durable-correlation.md), [SAGA-017](sagas/SAGA-017-retained-tombstones-purge-boundaries-and-lifecycle-limits.md).

**Reserved identifiers (not approved scenarios):** `SAGA-017-AC-02`, `SAGA-017-AC-04`.

**Question/decision:** How long are saga type/key/start tombstones retained, and when can identity be deleted or restarted?

**Recommendation (not approved):** Unapproved: Separate payload retention from identity lifetime and define any administrative deletion/restart boundary.

**Consequences and alternatives:** Approval establishes policy and evidence obligations; rejection requires explicit criterion retirement or new identities rather than silent semantic reuse.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: Purge preserves tombstones indefinitely; no public restart or identity-delete API exists.

**Original assertions retained for decision, not approved requirements:**

- `SAGA-017-AC-02`: **Given** an eligible terminal saga is purged. **When** persisted snapshot and uniqueness are read. **Then** StatePurgedAt records purge time; type/key/start identity/outcome/diagnostics remain, and Version and UpdatedAt are not incremented by purge.
- `SAGA-017-AC-04`: **Given** a terminal tombstone survives far beyond retention. **When** same start, different start or continuation arrives. **Then** same start remains no-op, different start remains closed and continuation remains ignored; retention does not free that type/key.

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

### Contract and identifier dispositions

**Affected contracts:** [SAGA-005](sagas/SAGA-005-start-continuation-and-duplicate-routing-matrix.md), [SAGA-015](sagas/SAGA-015-undecided-identity-schema-and-diagnostic-hardening.md).

**Reserved identifiers (not approved scenarios):** `SAGA-015-Q-05`.

**Question/decision:** What result applies to repeated starts for one type/key before the owning save?

**Recommendation (not approved):** Unapproved: Define tracked-new-instance replay/conflict semantics or explicitly reject that usage.

**Consequences and alternatives:** Approval establishes policy and evidence obligations; rejection requires explicit criterion retirement or new identities rather than silent semantic reuse.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: SagaStore.Find queries persistence without merging newly Added instances.

**Original assertions retained for decision, not approved requirements:**

- `SAGA-015-Q-05`: **Scenario:** two starts for one type/key are handled in one scope before save. GAP-WF-012 defines tracked-new-instance semantics. **Policy question:** Should the following conditional proposal be adopted: only under the selected policy, the second operation either observes tracked start replay/conflict or deliberately fails the unit of work; current persisted query does not explicitly merge added rows.

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

### Contract and identifier dispositions

**Affected contracts:** [SAGA-003](sagas/SAGA-003-composition-persistence-mapping-and-saga-options.md), [SCHED-005](scheduling/SCHED-005-options-defaults-boundaries-and-binding.md), [SCHED-015](scheduling/SCHED-015-undecided-integration-hardening.md).

**Reserved identifiers (not approved scenarios):** `SCHED-015-Q-04`.

**Question/decision:** What failures cover null option subobjects, null serializer options and extreme otherwise-positive settings?

**Recommendation (not approved):** Unapproved: Add a deliberate supported-shape and bound policy; do not treat dereference failures as option contracts.

**Consequences and alternatives:** Approval establishes policy and evidence obligations; rejection requires explicit criterion retirement or new identities rather than silent semantic reuse.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: Positive numeric validators dereference nested scheduling options; saga serializer options have no explicit null guard.

**Original assertions retained for decision, not approved requirements:**

- `SCHED-015-Q-04`: **Scenario:** a nested scheduling options object is null. configuration hardening is approved. **Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-013 chooses explicit validation, startup reports that invalid value as an option error; current dereference failures are not an approved diagnostic contract.

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

### Contract and identifier dispositions

**Affected contracts:** [SCHED-011](scheduling/SCHED-011-delivery-transaction-routing-and-retained-contract-failures.md), [SCHED-015](scheduling/SCHED-015-undecided-integration-hardening.md).

**Reserved identifiers (not approved scenarios):** `SCHED-015-Q-05`.

**Question/decision:** Does successful scheduling delivery require at least one outgoing target when custom providers select none?

**Recommendation (not approved):** Unapproved: Choose empty success or explicit rejection, distinct from the deliberate default unhandled-consumer failure.

**Consequences and alternatives:** Approval establishes policy and evidence obligations; rejection requires explicit criterion retirement or new identities rather than silent semantic reuse.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: Custom providers can return zero rows; the default consumer registry throws consumer.message_unhandled.

**Original assertions retained for decision, not approved requirements:**

- `SCHED-015-Q-05`: **Scenario:** custom providers return no outgoing targets for a valid schedule. empty-target policy is selected. **Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-014 accepts current empty success, the schedule may complete with zero outgoing rows; alternatively rejection must be specified before tests assume it.

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

### Contract and identifier dispositions

**Affected contracts:** [SAGA-007](sagas/SAGA-007-state-serialization-and-retained-schema-compatibility.md), [SAGA-015](sagas/SAGA-015-undecided-identity-schema-and-diagnostic-hardening.md).

**Reserved identifiers (not approved scenarios):** `SAGA-015-Q-06`.

**Question/decision:** Which retained state/message evolution scenarios are supported, and who migrates or rejects incompatible data?

**Recommendation (not approved):** Unapproved: Define host compatibility/migration responsibilities without interpreting concurrency Version as schema version.

**Consequences and alternatives:** Approval establishes policy and evidence obligations; rejection requires explicit criterion retirement or new identities rather than silent semantic reuse.

**Proposed identifier disposition:** Keep each original ID and heading reserved. Restore the original assertion only if approved unchanged; otherwise explicitly retire it and assign new IDs to materially different or split criteria after approval.

**Implementation comparison:** Inspection only: State/message serializers resolve current schemas; no automatic state migration mechanism exists.

**Original assertions retained for decision, not approved requirements:**

- `SAGA-015-Q-06`: **Scenario:** retained state/message schemas differ from a new deployment. GAP-WF-015 selects compatibility or migration approach. **Policy question:** Should the following conditional proposal be adopted: only under the selected policy, compatible work remains interpretable or an explicit migration/rejection occurs; numeric Version is not an automatic schema migration mechanism.

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

### Contract and identifier dispositions

**Affected contracts:** [MQTT-006](mqtt/MQTT-006-receive-failures-poison-acknowledgement-and-shutdown.md), [TRANSPORT-005](transport/TRANSPORT-005-admission-failures-duplicates-and-cancellation.md), [TRANSPORT-006](transport/TRANSPORT-006-outgoing-payload-construction-and-permanent-mapping-faults.md), [TRANSPORT-008](transport/TRANSPORT-008-unapproved-compatibility-and-synchronous-cancellation-decisions.md).

**Reserved identifiers (not approved scenarios):** `MQTT-006-AC-04`, `TRANSPORT-005-AC-01`, `TRANSPORT-005-AC-05`, `TRANSPORT-006-AC-02`.

**Question/decision:** How should synchronous mapper/serializer cancellation be attributed to a cancelled attempt versus user-thrown cancellation with an active token, without breaking public API or bounded failure-settlement semantics?

**Recommendation (not approved):** unapproved: Propagate genuine attempt cancellation without poison/permanent mapping classification at a token-aware compatible boundary; preserve cause/classification for unrelated user exceptions.

**Consequences and alternatives:** Ingress retry can currently acknowledge/discard a cancelled receipt as poison; outgoing mapping can permanently fault cancelled work. Whole composite IDs cannot be restored with a silently narrowed noncancellation-only scenario.

**Proposed identifier disposition:** Reserve every original ID and exact heading anchor. Restore unchanged only if its entire original meaning is approved; otherwise retire/reserve it and assign fresh IDs to approved replacements/splits. Update test mappings only after approval.

**Implementation comparison:** Inspection-only: TransportIngress and TransportDispatch catch synchronous exceptions including OperationCanceledException. Ingress wrapping reaches MqttInboxIngressHandler poison handling. Dispatch Create has no token attribution contract. Direct async I/O cancellation remains distinct; bounded postfailure settlement is independently protected by standards section 7.

**Original assertions retained for decision, not approved requirements:**

- `MQTT-006-AC-04`: **Given** a synchronous mapper throws cancellation which generic ingress wraps as `InboxIngressException`, **When** MQTT handler observes that wrapped exception, **Then** it becomes poison and invokes the poison acknowledgement path; this differs from direct admission/shutdown cancellation. See [GAP-TX-002](#gap-tx-002); this observation is not approval of the policy.
- `TRANSPORT-005-AC-01`: **Given** a serializer/mapper that throws before inbox invocation, **When** ingress accepts the envelope, **Then** it exposes `InboxIngressException` preserving the original exception as its inner cause (or preserves an existing ingress exception). **Given** successful mapping followed by an inbox admission failure, **When** admission fails, **Then** that failure escapes without ingress wrapping.
- `TRANSPORT-005-AC-05`: **Given** a mapper or payload serializer throwing `OperationCanceledException` before inbox invocation, **When** ingress runs even with its token cancelled, **Then** generic ingress wraps that exception as `InboxIngressException`, since the synchronous mapper has no token contract. This discrepancy with normative cancellation guidance is pending [GAP-TX-002](#gap-tx-002), not an approved cancellation policy.
- `TRANSPORT-006-AC-02`: **Given** unknown stored contract, invalid stored JSON, an incompatible deserialized source, a throwing mapper, missing payload contract or failing transport serialization, **When** dispatch creates a transport message, **Then** it throws `OutboxDispatchException` with code `outbox.dispatch.failed`, action `Fault` and preserved diagnostic cause where wrapping occurred; no publishable successful result is returned.

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

### Contract and identifier dispositions

**Affected contracts:** [MQTT-009](mqtt/MQTT-009-outgoing-target-and-destination-staging.md), [TRANSPORT-008](transport/TRANSPORT-008-unapproved-compatibility-and-synchronous-cancellation-decisions.md).

**Reserved identifiers (not approved scenarios):** `MQTT-009-AC-04`.

**Question/decision:** Which stored schemas, contract names, map/target changes and independently deployed peer versions must remain compatible, and who owns explicit migration or classified rejection?

**Recommendation (not approved):** unapproved: Document a bounded producer/consumer support matrix and explicit migration/rejection strategy for retained data and route IDs before changing compatible wire or stored representations.

**Consequences and alternatives:** Retry uses retained destination but changed current map/retain, which can alter publication semantics. Stable IDs alone do not authorize arbitrary schema or routing changes.

**Proposed identifier disposition:** Reserve every original ID and exact heading anchor. Restore unchanged only if its entire original meaning is approved; otherwise retire/reserve it and assign fresh IDs to approved replacements/splits. Update test mappings only after approval.

**Implementation comparison:** Inspection-only: dispatch resolves current payload/source contracts; MQTT retry reads current route/map/retain but stored destination. Removed route fails permanent outbox.dispatch.failed; no upcasting/negotiated migration was found.

**Original assertions retained for decision, not approved requirements:**

- `MQTT-009-AC-04`: **Given** staged work with destination X and route ID R, **When** a later deployment changes R's destination callback, map or retain and retries the entry, **Then** the stored destination remains X while the current map/retain are used. Removing R causes permanent route lookup failure; changing target ID does not automatically migrate old retained work.

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

### Contract and identifier dispositions

**Affected contracts:** [MQTT-004](mqtt/MQTT-004-incoming-metadata-serialization-and-contract-compatibility.md), [TRANSPORT-004](transport/TRANSPORT-004-incoming-typed-admission-and-metadata-loss-boundary.md), [TRANSPORT-006](transport/TRANSPORT-006-outgoing-payload-construction-and-permanent-mapping-faults.md).

**Reserved identifiers (not approved scenarios):** `MQTT-004-AC-05`, `TRANSPORT-004-AC-04`, `TRANSPORT-006-AC-01`.

**Question/decision:** Which arbitrary headers/content type are durable context, which remain transport-only, and which boundary may select a serializer from them?

**Recommendation (not approved):** unapproved: Choose an explicit allowed metadata/serializer authority contract rather than promise either unconditional round-trip propagation or mandatory metadata loss.

**Consequences and alternatives:** A composite ID including identity/segmentation plus metadata loss cannot be reused as a narrowed identity/segmentation scenario. Propagation can change serialized contracts and consumer context.

**Proposed identifier disposition:** Reserve every original ID and exact heading anchor. Restore unchanged only if its entire original meaning is approved; otherwise retire/reserve it and assign fresh IDs to approved replacements/splits. Update test mappings only after approval.

**Implementation comparison:** Inspection-only: incoming handler reconstructs segments and intermediate metadata; generic payload-only mapping drops headers/ContentType. Outgoing TransportMessageFactory copies IDs but creates empty metadata. Separate payload/durable serializers do not negotiate via ContentType.

**Original assertions retained for decision, not approved requirements:**

- `MQTT-004-AC-05`: **Given** identical payload bytes delivered as one segment and multiple segments with a nondefault Content Type and nonreserved UTF-8 properties, **When** ingress processes them, **Then** both yield the same mapped message bytes/value; Content Type and nonreserved headers enter the intermediate envelope only, do not select another serializer and are not persisted by built-in generic ingress.
- `TRANSPORT-004-AC-04`: **Given** a transport envelope with Content Type and arbitrary headers, **When** the built-in ingress mapping admits its payload, **Then** the mapper receives the payload only, and durable inbox data/context do not persist those headers or Content Type as additional envelope fields.
- `TRANSPORT-006-AC-01`: **Given** persisted A data, an A-to-B dispatch map and both source and B contracts registered, **When** `Create` succeeds, **Then** its payload is serialized mapped B data, `Contract` is B's registered name, stored IDs/context are copied, and Content Type is null with empty headers in the built-in factory.

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

### Contract and identifier dispositions

**Affected contracts:** [MQTT-010](mqtt/MQTT-010-outgoing-packet-metadata-and-retain-semantics.md), [MQTT-015](mqtt/MQTT-015-pending-interoperability-and-lifecycle-policies.md).

**Reserved identifiers (not approved scenarios):** `MQTT-010-AC-04`.

**Question/decision:** Should empty optional correlation/causation be omitted, rejected before publication or treated as absent by both MQTT peers?

**Recommendation (not approved):** unapproved: Treat empty optional output identities as absent, subject to explicit wire-compatibility approval; preserve strict required-ID validation and meaningful context IDs.

**Consequences and alternatives:** Current producer can emit packets its own consumer classifies poison; changing optional representation requires compatibility assessment, not silent mutation of a reserved ID.

**Proposed identifier disposition:** Reserve original IDs/exact anchors. Restore unchanged only on approval of entire original meaning; otherwise keep retired/reserved and assign fresh IDs to approved replacements/splits. Update test mappings only after approval.

**Implementation comparison:** Inspection-only: MqttMessageFactory emits present optional GUIDs including empty values; MqttInboxIngressHandler rejects any present empty reserved GUID. Absent optional IDs are omitted/accepted.

**Original assertions retained for decision, not approved requirements:**

- `MQTT-010-AC-04`: **Given** transport correlation/causation set to Guid.Empty, **When** the outgoing factory constructs metadata, **Then** it emits their D-format empty values without separate validation; an incoming adapter receiving that present value classifies it as poison. Absent optional IDs are omitted. See [GAP-TX-005](#gap-tx-005); this is not a requirement for interoperable empty IDs.

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

### Contract and identifier dispositions

**Affected contracts:** [MQTT-011](mqtt/MQTT-011-outgoing-connection-ownership-concurrency-and-disposal.md), [MQTT-015](mqtt/MQTT-015-pending-interoperability-and-lifecycle-policies.md).

**Reserved identifiers (not approved scenarios):** `MQTT-011-AC-04`.

**Question/decision:** Which publication, establishment and disposal overlaps are supported, and what draining/graceful-disconnect obligations apply to the client owner?

**Recommendation (not approved):** unapproved: Bound supported overlap explicitly and validate MQTTnet behavior; retain connection-only locking rather than assert or introduce global publication serialization without need.

**Consequences and alternatives:** Singleton lifetime does not establish safe concurrent publish/dispose. The full lifecycle criterion includes observed lack of graceful draining and cannot be narrowed to simple disposal under its old ID.

**Proposed identifier disposition:** Reserve original IDs/exact anchors. Restore unchanged only on approval of entire original meaning; otherwise keep retired/reserved and assign fresh IDs to approved replacements/splits. Update test mappings only after approval.

**Implementation comparison:** Inspection-only: MqttConnection serializes connect and disposes semaphore/client; PublishAsync calls do not share the establishment lock and there is no explicit outgoing graceful disconnect/drain.

**Original assertions retained for decision, not approved requirements:**

- `MQTT-011-AC-04`: **Given** the outgoing connection owns its semaphore/client, **When** DI disposes it after host work stops, **Then** both are disposed; there is no explicit outgoing graceful-disconnect operation or promise that racing active publications are drained. Concurrent-use/disposal and full client thread safety remain [GAP-TX-006](#gap-tx-006).

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

### Contract and identifier dispositions

**Affected contracts:** [MQTT-014](mqtt/MQTT-014-observable-logs-do-not-overstate-settlement.md).

**Reserved identifiers (not approved scenarios):** `MQTT-014-AC-01`, `MQTT-014-AC-02`.

**Question/decision:** How should attempt/result wording, event ordering and deliberate operator fields/levels distinguish confirmed ACK from attempted ACK and cleanup from shutdown?

**Recommendation (not approved):** unapproved: Use truthful attempt/confirmed-result language, preserving independently supported causes/boundaries; approve any exact event-ID/level compatibility commitments separately.

**Consequences and alternatives:** Current poison text can claim success before failure, and reconnect cleanup can be labelled shutdown. Existing exact event-inventory IDs stay reserved until their full meanings are approved or retired.

**Proposed identifier disposition:** Reserve original IDs/exact anchors. Restore unchanged only on approval of entire original meaning; otherwise keep retired/reserved and assign fresh IDs to approved replacements/splits. Update test mappings only after approval.

**Implementation comparison:** Inspection-only: worker logs poison event 3 before ACK; event 4 can then report ACK failure. Disconnect event 6 has shutdown wording even during reconnect. Truthfulness follows standards independently; exact inventory is not asserted as approved.

**Original assertions retained for decision, not approved requirements:**

- `MQTT-014-AC-01`: **Given** poison input and failing acknowledgement, **When** receive handles it, **Then** event 3 is observable before event 4 and no successful acknowledgement is implied by database/broker evidence; event 3's current wording is misleading rather than proof of success. See [GAP-TX-007](#gap-tx-007).
- `MQTT-014-AC-02`: **Given** setup failure, ordinary admission/ack failure, or disconnect cleanup failure, **When** the worker logs it, **Then** the corresponding levels/fields above identify setup, receipt or cleanup rather than claiming consumer completion; outgoing durable completion metrics cannot prove subscriber application effects.

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

### Contract and identifier dispositions

**Affected contracts:** [TRANSPORT-003](transport/TRANSPORT-003-registration-and-exact-mapping-resolution.md).

**Reserved identifiers (not approved scenarios):** `TRANSPORT-003-AC-03`.

**Question/decision:** At which registration/startup/resolution phase must distinct duplicate maps/routes fail, and which diagnostics and identical-registration behavior are deliberate contracts?

**Recommendation (not approved):** unapproved: Detect actionable ambiguous configuration before processing when feasible, with an explicitly approved compatibility/timing contract rather than invent a startup validator guarantee.

**Consequences and alternatives:** Ambiguity prevention is independently supported, but early validation, exception type and repeated-identical registration comprise an unapproved original criterion.

**Proposed identifier disposition:** Reserve every original ID and exact heading anchor. Restore unchanged only if its entire original meaning is approved; otherwise retire/reserve it and assign fresh IDs to approved replacements/splits. Update test mappings only after approval.

**Implementation comparison:** Inspection-only: exact registries fail distinct duplicates during scoped resolution; ingress dictionary and dispatch permanent-exception attribution differ. TryAddEnumerable suppresses identical registrations. Provider registration does not provision durable readiness.

**Original assertions retained for decision, not approved requirements:**

- `TRANSPORT-003-AC-03`: **Given** two distinct mapper implementations registered for one ingress pair or one dispatch pair, **When** the corresponding registry is resolved, **Then** resolution fails rather than choosing first/last; dispatch reports `OutboxDispatchException` and ingress dictionary construction reports a configuration exception. Repeating the same typed registration alone does not create this conflict.

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

### Contract and identifier dispositions

**Affected contracts:** [MQTT-008](mqtt/MQTT-008-inbox-reconnect-subscription-results-and-cleanup.md), [MQTT-015](mqtt/MQTT-015-pending-interoperability-and-lifecycle-policies.md).

**Reserved identifiers (not approved scenarios):** `MQTT-008-AC-04`.

**Question/decision:** What shutdown bound, callback-draining order and resource disposal contract applies to final inbox disconnect and synchronization ownership?

**Recommendation (not approved):** unapproved: Select finite owned shutdown/drain work with safe resource release and truthful uncertain-delivery outcomes; approve implementation-specific ordering separately.

**Consequences and alternatives:** An uncancelled network disconnect can delay shutdown; disposal cannot safely race active callbacks without an owned lifetime contract. Broker expiry still bounds replay independently.

**Proposed identifier disposition:** Reserve original IDs/exact anchors. Restore unchanged only on approval of entire original meaning; otherwise keep retired/reserved and assign fresh IDs to approved replacements/splits. Update test mappings only after approval.

**Implementation comparison:** Inspection-only: worker removes callbacks, attempts CancellationToken.None disconnect, logs failure and disposes client, but has no explicit disconnect timeout or semaphore disposal. In-flight draining was not certified.

**Original assertions retained for decision, not approved requirements:**

- `MQTT-008-AC-04`: **Given** the worker owns a connected client, **When** host shutdown ends its loop, **Then** receive/disconnected callbacks are removed, final disconnect is attempted with CancellationToken.None and disconnect exceptions are logged, and the client is disposed. This does not assert a bounded network disconnect duration or durable replay after broker session expiry/reset.
