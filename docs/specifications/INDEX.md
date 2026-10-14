# Behavioural specification index

A capability-based catalogue of behavioural contracts and unresolved decisions. Each numbered specification owns its acceptance criteria; capability README pages hold shared contracts, invariants and limitations.

## Reading the contracts

Every numbered specification states its intent, guarantees, contract boundaries, acceptance criteria and open decisions. Guarantees describe intended outcomes grounded in repository requirements or deliberate public contracts, not an inventory of current implementation branches.

Acceptance criteria under **Acceptance criteria** express those obligations. An existing identifier retained under **Open decisions** is reserved for its original meaning: it is not an approved scenario and must not be reused for a different requirement. Existing question identifiers remain questions, not additional acceptance criteria. [GAPS.md](GAPS.md) preserves the unresolved question, original assertion and proposed disposition.

Source observations, recommendations and test mappings do not approve contractual choices. A linked test supplies traceability, not authority or proof of conformance. Implementation descriptions belong in [reference documentation](../README.md); pending decisions do not silently become guarantees.

## Capabilities

### [Claims, leases and settlement ownership](claims-leases/README.md)

- [CLAIM-001 — Recoverable ownership of durable work](claims-leases/CLAIM-001-claim-eligibility-and-attempt-accounting.md)
- [CLAIM-002 — Stale settlement rejection](claims-leases/CLAIM-002-stale-settlement-rejection.md)
- [CLAIM-003 — Renewal and settlement preserve ownership lifecycle](claims-leases/CLAIM-003-owned-state-and-expiry-predicates.md)
- [CLAIM-004 — Renewal loss and cooperative cancellation](claims-leases/CLAIM-004-renewal-loss-and-cooperative-cancellation.md)
- [CLAIM-005 — Lease arithmetic and caller invariants](claims-leases/CLAIM-005-lease-arithmetic-and-caller-invariants.md)

### [Configuration, validation and host composition](configuration/README.md)

- [CONFIG-001 — Host ownership and resource lifetimes](configuration/CONFIG-001-host-ownership-and-resource-lifetimes.md)
- [CONFIG-002 — Public registration defaults and validation](configuration/CONFIG-002-core-defaults-and-override-preservation.md)
- [CONFIG-003 — Registration identifier boundaries](configuration/CONFIG-003-registration-identifier-boundaries.md)
- [CONFIG-004 — Startup validation and configuration binding](configuration/CONFIG-004-startup-validation-and-configuration-binding.md)
- [CONFIG-005 — Persistence-context composition constraints](configuration/CONFIG-005-one-messaging-persistence-context-type.md)
- [CONFIG-006 — Complete boundary validation](configuration/CONFIG-006-complete-boundary-validation.md)

### [Core contracts and durable serialization](core-contracts/README.md)

- [CORE-001 — Stable explicit contracts](core-contracts/CORE-001-stable-explicit-contracts.md)
- [CORE-002 — Registry uniqueness and selection](core-contracts/CORE-002-registry-uniqueness-and-selection.md)
- [CORE-003 — Lookup and dispatch failures](core-contracts/CORE-003-lookup-and-dispatch-failures.md)
- [CORE-004 — Durable JSON representation](core-contracts/CORE-004-durable-json-representation.md)
- [CORE-005 — Direct public contract input validation](core-contracts/CORE-005-direct-public-contract-input-validation.md)

### [Domain events behavioural specifications](domain-events/README.md)

- [DOMAIN-001 — Raise pending events without owning business policy](domain-events/DOMAIN-001-raise-pending-events-without-owning-business-policy.md)
- [DOMAIN-002 — Acknowledgement removes pending facts by identity](domain-events/DOMAIN-002-acknowledgement-removes-pending-facts-by-identity.md)
- [DOMAIN-003 — Opt-in EF capture composition](domain-events/DOMAIN-003-opt-in-ef-capture-composition.md)
- [DOMAIN-004 — Capture tracked sources across entity states](domain-events/DOMAIN-004-capture-tracked-sources-across-entity-states.md)
- [DOMAIN-005 — Atomic captured facts and application commit](domain-events/DOMAIN-005-atomic-captured-facts-and-application-commit.md)
- [DOMAIN-006 — Defer acknowledgement and deduplicate multi-save capture](domain-events/DOMAIN-006-defer-acknowledgement-and-deduplicate-multi-save-capture.md)
- [DOMAIN-007 — Invalid custom facts and targetless acknowledgement](domain-events/DOMAIN-007-invalid-custom-facts-and-targetless-acknowledgement.md)
- [DOMAIN-008 — Custom source collisions and acknowledgement failures](domain-events/DOMAIN-008-custom-source-collisions-and-acknowledgement-failures.md)
- [DOMAIN-009 — Crash, recovery and retention boundaries](domain-events/DOMAIN-009-crash-recovery-and-retention-boundaries.md)
- [DOMAIN-010 — Event-source and acknowledgement contract](domain-events/DOMAIN-010-undecided-source-and-acknowledgement-contract.md)

### [Message identity and execution context](identity-context/README.md)

- [IDENTITY-001 — Stable identity across durable retries](identity-context/IDENTITY-001-stable-identity-across-durable-retries.md)
- [IDENTITY-002 — Relationship metadata is not deduplication](identity-context/IDENTITY-002-relationship-metadata-is-not-deduplication.md)
- [IDENTITY-003 — Execution-local context scope](identity-context/IDENTITY-003-execution-local-context-scope.md)
- [IDENTITY-004 — Opaque claim token representation](identity-context/IDENTITY-004-opaque-claim-token-representation.md)
- [IDENTITY-005 — Scope misuse and child-flow lifetime](identity-context/IDENTITY-005-scope-misuse-and-child-flow-lifetime.md)

### [Inbox behavioural specifications](inbox/README.md)

- [INBOX-001 — Typed admission and envelope identity](inbox/INBOX-001-typed-admission-and-envelope-identity.md)
- [INBOX-002 — Independent atomic admission and duplicate retention](inbox/INBOX-002-independent-atomic-admission-and-duplicate-retention.md)
- [INBOX-003 — Empty admission boundary](inbox/INBOX-003-empty-admission-boundary.md)
- [INBOX-004 — Durable entry state and fenced claims](inbox/INBOX-004-durable-entry-state-and-fenced-claims.md)
- [INBOX-005 — Atomic local delivery and repeatable execution](inbox/INBOX-005-atomic-local-delivery-and-repeatable-execution.md)
- [INBOX-006 — Processor metadata, failure settlement and cancellation](inbox/INBOX-006-processor-metadata-failure-settlement-and-cancellation.md)
- [INBOX-007 — Signals, workers and restart recovery](inbox/INBOX-007-signals-workers-and-restart-recovery.md)
- [INBOX-008 — Dead-letter inspection and independent management](inbox/INBOX-008-dead-letter-inspection-and-independent-management.md)
- [INBOX-009 — Replay retained admissions into missing targets](inbox/INBOX-009-replay-retained-admissions-into-missing-targets.md)
- [INBOX-010 — Terminal cleanup and admission-retention boundary](inbox/INBOX-010-entry-cleanup-and-indefinite-admission-retention.md)
- [INBOX-011 — Composition, mappings and public options](inbox/INBOX-011-composition-mappings-and-public-options.md)
- [INBOX-012 — Scoped cleanup composition requirement](inbox/INBOX-012-scoped-cleanup-composition-requirement-and-current-mismatch.md)
- [INBOX-013 — Replay failure transparency requirement](inbox/INBOX-013-replay-failure-transparency-requirement.md)
- [INBOX-014 — Admission and replay policy boundaries](inbox/INBOX-014-undecided-admission-and-replay-policies.md)
- [INBOX-015 — Replay failure reconciliation policy](inbox/INBOX-015-unapproved-replay-failure-improvement.md)

### [Dead letters, retention and observability](observability/README.md)

- [OPS-001 — Dead-letter listing and transitions](observability/OPS-001-dead-letter-listing-and-transitions.md)
- [OPS-002 — Identity-preserving management](observability/OPS-002-identity-preserving-management.md)
- [OPS-003 — Terminal cleanup boundaries](observability/OPS-003-terminal-cleanup-boundaries.md)
- [OPS-004 — Observable metric boundary](observability/OPS-004-observable-metric-boundary.md)
- [OPS-005 — Settlement log attribution](observability/OPS-005-settlement-log-attribution.md)

### [MQTT ingress and dispatch](mqtt/README.md)

- [MQTT-001 — Registration and public fluent routes](mqtt/MQTT-001-registration-and-public-fluent-routes.md)
- [MQTT-002 — Startup options and MQTT connection configuration](mqtt/MQTT-002-startup-options-and-mqtt-connection-configuration.md)
- [MQTT-003 — Incoming filters choose exactly one route](mqtt/MQTT-003-incoming-filters-choose-exactly-one-route.md)
- [MQTT-004 — Incoming metadata, serialization and contract compatibility](mqtt/MQTT-004-incoming-metadata-serialization-and-contract-compatibility.md)
- [MQTT-005 — Acknowledgement follows admission, not consumer effects](mqtt/MQTT-005-acknowledgement-follows-admission-not-consumer-effects.md)
- [MQTT-006 — Receive failures, poison acknowledgement and shutdown](mqtt/MQTT-006-receive-failures-poison-acknowledgement-and-shutdown.md)
- [MQTT-007 — Reset-on-start public session contract](mqtt/MQTT-007-reset-on-start-public-session-contract.md)
- [MQTT-008 — Inbox reconnect, subscription results and cleanup](mqtt/MQTT-008-inbox-reconnect-subscription-results-and-cleanup.md)
- [MQTT-009 — Outgoing target and destination staging](mqtt/MQTT-009-outgoing-target-and-destination-staging.md)
- [MQTT-010 — Outgoing packet metadata and retain semantics](mqtt/MQTT-010-outgoing-packet-metadata-and-retain-semantics.md)
- [MQTT-011 — Outgoing connection ownership, concurrency and disposal](mqtt/MQTT-011-outgoing-connection-ownership-concurrency-and-disposal.md)
- [MQTT-012 — Publish result, durable completion and failure classification](mqtt/MQTT-012-publish-result-durable-completion-and-failure-classification.md)
- [MQTT-013 — External repeat, cancellation and independently successful targets](mqtt/MQTT-013-external-repeat-cancellation-and-independently-successful-targets.md)
- [MQTT-014 — Observable logs do not overstate settlement](mqtt/MQTT-014-observable-logs-do-not-overstate-settlement.md)
- [MQTT-015 — Pending interoperability and lifecycle policies](mqtt/MQTT-015-pending-interoperability-and-lifecycle-policies.md)

### [Outbox behavioural specifications](outbox/README.md)

- [OUTBOX-001 — Enqueue stages a concrete envelope and target snapshot](outbox/OUTBOX-001-enqueue-stages-a-concrete-envelope-and-target-snapshot.md)
- [OUTBOX-002 — Ambient outgoing metadata and representation members](outbox/OUTBOX-002-ambient-outgoing-metadata-and-representation-members.md)
- [OUTBOX-003 — Target validation, local selection and empty batches](outbox/OUTBOX-003-target-validation-local-selection-and-empty-batches.md)
- [OUTBOX-004 — Tracked duplicate suppression versus persisted uniqueness](outbox/OUTBOX-004-tracked-duplicate-suppression-versus-persisted-uniqueness.md)
- [OUTBOX-005 — Atomic owning save and commit boundary](outbox/OUTBOX-005-atomic-owning-save-and-commit-boundary.md)
- [OUTBOX-006 — Public save capture protocol and per-context deduplication](outbox/OUTBOX-006-public-save-capture-protocol-and-per-context-deduplication.md)
- [OUTBOX-007 — Save failure, cancellation, rollback and retry tracking](outbox/OUTBOX-007-save-failure-cancellation-rollback-and-retry-tracking.md)
- [OUTBOX-008 — Commit verification and uncertain outcome](outbox/OUTBOX-008-commit-verification-and-uncertain-outcome.md)
- [OUTBOX-009 — Savepoints, ambient transactions and observer restrictions](outbox/OUTBOX-009-savepoints-ambient-transactions-and-observer-restrictions.md)
- [OUTBOX-010 — Postcommit callbacks and wake hints](outbox/OUTBOX-010-postcommit-callbacks-and-wake-hints.md)
- [OUTBOX-011 — Transport dispatch and external repeat window](outbox/OUTBOX-011-transport-dispatch-and-external-repeat-window.md)
- [OUTBOX-012 — Delivery registration and local atomic dispatch](outbox/OUTBOX-012-delivery-registration-and-local-atomic-dispatch.md)
- [OUTBOX-013 — Owned durable state, processor policy and cancellation](outbox/OUTBOX-013-owned-durable-state-processor-policy-and-cancellation.md)
- [OUTBOX-014 — Workers, signals, bounded queue and recovery](outbox/OUTBOX-014-workers-signals-bounded-queue-and-recovery.md)
- [OUTBOX-015 — Dead-letter management and cleanup horizon](outbox/OUTBOX-015-dead-letter-management-and-cleanup-horizon.md)
- [OUTBOX-016 — EF composition, mappings, options and validation](outbox/OUTBOX-016-ef-composition-mappings-options-and-validation.md)
- [OUTBOX-017 — Scoped cleanup lifetime requirement](outbox/OUTBOX-017-scoped-cleanup-lifetime-requirement.md)
- [OUTBOX-018 — Undecided duplicate, commit and callback policies](outbox/OUTBOX-018-undecided-duplicate-commit-and-callback-policies.md)

### [Persistence and transaction consistency](persistence/README.md)

- [PERSIST-001 — Retained logical target uniqueness](persistence/PERSIST-001-retained-logical-target-uniqueness.md)
- [PERSIST-002 — Local completion atomicity](persistence/PERSIST-002-local-completion-atomicity.md)
- [PERSIST-003 — Store cancellation and invalid claim boundaries](persistence/PERSIST-003-store-cancellation-and-invalid-claim-boundaries.md)
- [PERSIST-004 — Commit ambiguity and provider boundaries](persistence/PERSIST-004-commit-ambiguity-and-provider-verification.md)

### [Worker lifecycle, signaling and recovery](recovery-lifecycle/README.md)

- [WORK-001 — Durability independent of wake hints](recovery-lifecycle/WORK-001-durability-independent-of-wake-hints.md)
- [WORK-002 — Signal observation and cancellation](recovery-lifecycle/WORK-002-signal-observation-and-cancellation.md)
- [WORK-003 — Bounded processing and failure recovery](recovery-lifecycle/WORK-003-bounded-processing-and-failure-recovery.md)
- [WORK-004 — Queue, leases and producer delay](recovery-lifecycle/WORK-004-queue-leases-and-producer-delay.md)

### [Retry policy and failure classification](retry-failures/README.md)

- [RETRY-001 — Failure action and safe diagnostics](retry-failures/RETRY-001-failure-action-and-safe-diagnostics.md)
- [RETRY-002 — Failure snapshot values and bounds](retry-failures/RETRY-002-failure-snapshot-values-and-bounds.md)
- [RETRY-003 — Failure-retry budget](retry-failures/RETRY-003-failure-retry-budget.md)
- [RETRY-004 — Backoff and jitter boundaries](retry-failures/RETRY-004-backoff-and-jitter-boundaries.md)
- [RETRY-005 — Processing settlement boundary](retry-failures/RETRY-005-processing-settlement-boundary.md)
- [RETRY-006 — Out-of-domain public policy inputs](retry-failures/RETRY-006-out-of-domain-public-policy-inputs.md)

### [Saga behavioural specifications](sagas/README.md)

- [SAGA-001 — Workflow identity and local outgoing consistency](sagas/SAGA-001-workflow-identity-and-local-outgoing-consistency.md)
- [SAGA-002 — Definitions, handler overloads and registration validation](sagas/SAGA-002-definitions-handler-overloads-and-registration-validation.md)
- [SAGA-003 — Composition, persistence mapping and saga options](sagas/SAGA-003-composition-persistence-mapping-and-saga-options.md)
- [SAGA-004 — Identity values and durable correlation](sagas/SAGA-004-identity-values-and-durable-correlation.md)
- [SAGA-005 — Start, continuation and duplicate routing matrix](sagas/SAGA-005-start-continuation-and-duplicate-routing-matrix.md)
- [SAGA-006 — Handler results and immutable effect descriptions](sagas/SAGA-006-handler-results-and-immutable-effect-descriptions.md)
- [SAGA-007 — State serialization and retained schema compatibility](sagas/SAGA-007-state-serialization-and-retained-schema-compatibility.md)
- [SAGA-008 — State application and terminal invariants](sagas/SAGA-008-state-application-and-terminal-invariants.md)
- [SAGA-009 — Outgoing metadata, timers and terminal effect ordering](sagas/SAGA-009-outgoing-metadata-timers-and-terminal-effect-ordering.md)
- [SAGA-010 — Timer delivery, business failure and exhaustion](sagas/SAGA-010-timer-delivery-business-failure-and-exhaustion.md)
- [SAGA-011 — Owner transactions, rollback, savepoints and replay](sagas/SAGA-011-owner-transactions-rollback-savepoints-and-replay.md)
- [SAGA-012 — State version races and concurrent continuation](sagas/SAGA-012-state-version-races-and-concurrent-continuation.md)
- [SAGA-013 — Start collision translation and recovery outcomes](sagas/SAGA-013-start-collision-translation-and-recovery-outcomes.md)
- [SAGA-014 — Failures, cancellation and malformed result intent](sagas/SAGA-014-failures-cancellation-and-malformed-result-intent.md)
- [SAGA-015 — Undecided identity, schema and diagnostic hardening](sagas/SAGA-015-undecided-identity-schema-and-diagnostic-hardening.md)
- [SAGA-016 — Persisted manager and timer snapshots](sagas/SAGA-016-persisted-manager-and-timer-snapshots.md)
- [SAGA-017 — Retained tombstones, purge boundaries and lifecycle limits](sagas/SAGA-017-retained-tombstones-purge-boundaries-and-lifecycle-limits.md)

### [Scheduling behavioural specifications](scheduling/README.md)

- [SCHED-001 — Stable logical schedule identity](scheduling/SCHED-001-stable-logical-schedule-identity.md)
- [SCHED-002 — Repeated delivery and external effects](scheduling/SCHED-002-repeated-delivery-and-external-effects.md)
- [SCHED-003 — Input descriptions, serialization and UTC boundaries](scheduling/SCHED-003-input-descriptions-serialization-and-utc-boundaries.md)
- [SCHED-004 — Runtime, EF, outbox and model composition](scheduling/SCHED-004-runtime-ef-outbox-and-model-composition.md)
- [SCHED-005 — Options defaults, boundaries and binding](scheduling/SCHED-005-options-defaults-boundaries-and-binding.md)
- [SCHED-006 — Staging, duplicate insertion and owner visibility](scheduling/SCHED-006-staging-duplicate-insertion-and-owner-visibility.md)
- [SCHED-007 — Cancellation filters, terminal states and races](scheduling/SCHED-007-cancellation-filters-terminal-states-and-races.md)
- [SCHED-008 — Persisted management snapshots](scheduling/SCHED-008-persisted-management-snapshots.md)
- [SCHED-009 — Due eligibility, claims and bounded execution](scheduling/SCHED-009-due-eligibility-claims-and-bounded-execution.md)
- [SCHED-010 — Expiry before admission and asymmetric settlement](scheduling/SCHED-010-expiry-before-admission-and-asymmetric-settlement.md)
- [SCHED-011 — Delivery transaction, routing and retained contract failures](scheduling/SCHED-011-delivery-transaction-routing-and-retained-contract-failures.md)
- [SCHED-012 — Failure budgets, diagnostics and cancellation recovery](scheduling/SCHED-012-failure-budgets-diagnostics-and-cancellation-recovery.md)
- [SCHED-013 — Local wake engine, commit hints and polling recovery](scheduling/SCHED-013-local-wake-engine-commit-hints-and-polling-recovery.md)
- [SCHED-014 — Quartz wake adapter, fallback, reconnect and shutdown](scheduling/SCHED-014-quartz-wake-adapter-fallback-reconnect-and-shutdown.md)
- [SCHED-015 — Undecided integration hardening](scheduling/SCHED-015-undecided-integration-hardening.md)
- [SCHED-016 — Terminal retention, recovery boundaries and management limits](scheduling/SCHED-016-terminal-retention-recovery-boundaries-and-management-limits.md)

### [Transport mapping](transport/README.md)

- [TRANSPORT-001 — Preserve representation and identity boundaries](transport/TRANSPORT-001-preserve-representation-and-identity-boundaries.md)
- [TRANSPORT-002 — Envelope and payload serialization surface](transport/TRANSPORT-002-envelope-and-payload-serialization-surface.md)
- [TRANSPORT-003 — Registration and exact mapping resolution](transport/TRANSPORT-003-registration-and-exact-mapping-resolution.md)
- [TRANSPORT-004 — Incoming typed admission and metadata loss boundary](transport/TRANSPORT-004-incoming-typed-admission-and-metadata-loss-boundary.md)
- [TRANSPORT-005 — Admission failures, duplicates and cancellation](transport/TRANSPORT-005-admission-failures-duplicates-and-cancellation.md)
- [TRANSPORT-006 — Outgoing payload construction and permanent mapping faults](transport/TRANSPORT-006-outgoing-payload-construction-and-permanent-mapping-faults.md)
- [TRANSPORT-007 — External publication and per-target completion are separate](transport/TRANSPORT-007-external-publication-and-per-target-completion-are-separate.md)
- [TRANSPORT-008 — Unapproved compatibility and synchronous cancellation decisions](transport/TRANSPORT-008-unapproved-compatibility-and-synchronous-cancellation-decisions.md)

## Related references

- [Unresolved decisions](GAPS.md)
- [Verification boundaries](../reference/testing.md#verification-boundaries)
- [Architecture](../reference/architecture.md)
