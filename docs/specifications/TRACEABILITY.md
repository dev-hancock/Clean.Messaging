# Expected verification boundaries

This map identifies the expected verification boundary for specification groups, not test execution or approval of Observed/Proposed behaviour. Each default applies to all criteria of the listed specifications except the explicitly listed exceptions. The linked specification owns each outcome and its authority; passing a narrower test does not replace the expected boundary.

## Specification-group defaults

| Capability | Specifications | Expected verification boundary |
| --- | --- | --- |
| [claims-leases](claims-leases/README.md) | CLAIM-001, CLAIM-003 | Persistence integration |
| [claims-leases](claims-leases/README.md) | CLAIM-002 | Concurrency integration |
| [claims-leases](claims-leases/README.md) | CLAIM-004 | Integration |
| [configuration](configuration/README.md) | CONFIG-001 | Integration |
| [configuration](configuration/README.md) | CONFIG-002, CONFIG-003, CONFIG-004, CONFIG-005, CONFIG-006 | Unit |
| [core-contracts](core-contracts/README.md) | CORE-001, CORE-002, CORE-003, CORE-004 | Unit |
| [domain-events](domain-events/README.md) | DOMAIN-001, DOMAIN-002 | Unit |
| [domain-events](domain-events/README.md) | DOMAIN-003 | Composition integration |
| [domain-events](domain-events/README.md) | DOMAIN-004, DOMAIN-007 | Unit + integration |
| [domain-events](domain-events/README.md) | DOMAIN-005, DOMAIN-006 | Relational integration |
| [domain-events](domain-events/README.md) | DOMAIN-008, DOMAIN-009 | Fault-injection integration |
| [identity-context](identity-context/README.md) | IDENTITY-001, IDENTITY-002 | Persistence integration |
| [identity-context](identity-context/README.md) | IDENTITY-003, IDENTITY-004 | Unit |
| [inbox](inbox/README.md) | INBOX-001 | Unit + relational integration |
| [inbox](inbox/README.md) | INBOX-002, INBOX-003, INBOX-004, INBOX-005, INBOX-008, INBOX-009, INBOX-010 | Relational integration |
| [inbox](inbox/README.md) | INBOX-006 | Unit + integration |
| [inbox](inbox/README.md) | INBOX-007 | Unit |
| [inbox](inbox/README.md) | INBOX-011, INBOX-012 | Composition integration |
| [inbox](inbox/README.md) | INBOX-013, INBOX-015 | Fault-injection integration |
| [management-observability](observability/README.md) | OPS-001, OPS-003 | Persistence integration |
| [management-observability](observability/README.md) | OPS-002 | Workflow integration |
| [management-observability](observability/README.md) | OPS-004 | Unit |
| [mqtt](mqtt/README.md) | MQTT-001 | DI + transport integration |
| [mqtt](mqtt/README.md) | MQTT-002, MQTT-010 | Unit + transport integration |
| [mqtt](mqtt/README.md) | MQTT-003, MQTT-007, MQTT-008, MQTT-014 | Transport integration |
| [mqtt](mqtt/README.md) | MQTT-004, MQTT-005, MQTT-006, MQTT-012, MQTT-013 | Transport + persistence integration |
| [mqtt](mqtt/README.md) | MQTT-009 | Persistence + transport integration |
| [mqtt](mqtt/README.md) | MQTT-011 | Concurrency + transport integration |
| [outbox](outbox/README.md) | OUTBOX-001, OUTBOX-004, OUTBOX-005, OUTBOX-009, OUTBOX-012, OUTBOX-013, OUTBOX-015 | Relational integration |
| [outbox](outbox/README.md) | OUTBOX-002, OUTBOX-003, OUTBOX-006 | Unit |
| [outbox](outbox/README.md) | OUTBOX-007, OUTBOX-008, OUTBOX-010, OUTBOX-011, OUTBOX-014 | Fault-injection integration |
| [outbox](outbox/README.md) | OUTBOX-016, OUTBOX-017 | Composition integration |
| [persistence](persistence/README.md) | PERSIST-001, PERSIST-002 | Persistence integration |
| [persistence](persistence/README.md) | PERSIST-003 | Unit |
| [recovery-lifecycle](recovery-lifecycle/README.md) | WORK-001 | Workflow integration |
| [recovery-lifecycle](recovery-lifecycle/README.md) | WORK-002 | Unit |
| [recovery-lifecycle](recovery-lifecycle/README.md) | WORK-003 | Integration |
| [recovery-lifecycle](recovery-lifecycle/README.md) | WORK-004 | Concurrency integration |
| [retry-failures](retry-failures/README.md) | RETRY-001, RETRY-002, RETRY-003, RETRY-004 | Unit |
| [retry-failures](retry-failures/README.md) | RETRY-005 | Integration |
| [sagas](sagas/README.md) | SAGA-001, SAGA-010 | workflow |
| [sagas](sagas/README.md) | SAGA-002, SAGA-003, SAGA-004, SAGA-006, SAGA-008 | unit |
| [sagas](sagas/README.md) | SAGA-005, SAGA-007, SAGA-009, SAGA-011, SAGA-016, SAGA-017 | persistence |
| [sagas](sagas/README.md) | SAGA-012, SAGA-013 | concurrency |
| [scheduling](scheduling/README.md) | SCHED-001, SCHED-013, SCHED-014 | workflow |
| [scheduling](scheduling/README.md) | SCHED-002, SCHED-003, SCHED-004, SCHED-006, SCHED-007, SCHED-008, SCHED-009, SCHED-011, SCHED-012, SCHED-016 | persistence |
| [scheduling](scheduling/README.md) | SCHED-005 | unit |
| [transport](transport/README.md) | TRANSPORT-001, TRANSPORT-004, TRANSPORT-005 | Persistence integration |
| [transport](transport/README.md) | TRANSPORT-002, TRANSPORT-006 | Unit |
| [transport](transport/README.md) | TRANSPORT-003 | Unit / DI integration |
| [transport](transport/README.md) | TRANSPORT-007 | Transport + persistence integration |

## Criterion-specific boundary exceptions

Only criteria whose expected boundary differs from their specification-group default appear here.

| Specification | Criterion | Expected verification boundary |
| --- | --- | --- |
| CLAIM-002 | [CLAIM-002-AC-02](claims-leases/CLAIM-002-stale-settlement-rejection.md#claim-002-ac-02--wrong-target) | Persistence integration |
| CLAIM-004 | [CLAIM-004-AC-04](claims-leases/CLAIM-004-renewal-loss-and-cooperative-cancellation.md#claim-004-ac-04--uncooperative-handler) | Concurrency integration |
| CONFIG-001 | [CONFIG-001-AC-01](configuration/CONFIG-001-host-ownership-and-resource-lifetimes.md#config-001-ac-01--host-owned-persistence-setup) | Unit |
| DOMAIN-003 | [DOMAIN-003-AC-02](domain-events/DOMAIN-003-opt-in-ef-capture-composition.md#domain-003-ac-02--capture-through-correctly-composed-owning-context) | Relational integration |
| DOMAIN-004 | [DOMAIN-004-AC-01](domain-events/DOMAIN-004-capture-tracked-sources-across-entity-states.md#domain-004-ac-01--capture-unchanged-and-custom-tracked-sources) | Relational integration |
| DOMAIN-004 | [DOMAIN-004-AC-04](domain-events/DOMAIN-004-capture-tracked-sources-across-entity-states.md#domain-004-ac-04--do-not-authorize-concurrent-source-mutation) | Fault-injection integration |
| DOMAIN-005 | [DOMAIN-005-AC-02](domain-events/DOMAIN-005-atomic-captured-facts-and-application-commit.md#domain-005-ac-02--do-not-acknowledge-failed-or-cancelled-save) | Fault-injection integration |
| DOMAIN-006 | [DOMAIN-006-AC-03](domain-events/DOMAIN-006-defer-acknowledgement-and-deduplicate-multi-save-capture.md#domain-006-ac-03--acknowledge-only-selected-committed-identities) | Unit + integration |
| DOMAIN-007 | [DOMAIN-007-AC-02](domain-events/DOMAIN-007-invalid-custom-facts-and-targetless-acknowledgement.md#domain-007-ac-02--observe-targetless-successful-acknowledgement) | Relational integration |
| DOMAIN-007 | [DOMAIN-007-AC-03](domain-events/DOMAIN-007-invalid-custom-facts-and-targetless-acknowledgement.md#domain-007-ac-03--keep-targetless-failed-commit-uncertain) | Fault-injection integration |
| DOMAIN-008 | [DOMAIN-008-AC-01](domain-events/DOMAIN-008-custom-source-collisions-and-acknowledgement-failures.md#domain-008-ac-01--observe-duplicate-ids-across-tracked-sources) | Unit + integration |
| DOMAIN-008 | [DOMAIN-008-AC-03](domain-events/DOMAIN-008-custom-source-collisions-and-acknowledgement-failures.md#domain-008-ac-03--do-not-infer-retry-safe-custom-acknowledgement) | Unit |
| DOMAIN-009 | [DOMAIN-009-AC-03](domain-events/DOMAIN-009-crash-recovery-and-retention-boundaries.md#domain-009-ac-03--bound-durable-evidence-by-outbox-retention) | Relational integration |
| IDENTITY-001 | [IDENTITY-001-AC-02](identity-context/IDENTITY-001-stable-identity-across-durable-retries.md#identity-001-ac-02--independent-target-completion) | Workflow integration |
| IDENTITY-002 | [IDENTITY-002-AC-02](identity-context/IDENTITY-002-relationship-metadata-is-not-deduplication.md#identity-002-ac-02--causation-distinction) | Workflow integration |
| IDENTITY-003 | [IDENTITY-003-AC-04](identity-context/IDENTITY-003-execution-local-context-scope.md#identity-003-ac-04--parallel-flow-isolation) | Concurrency integration |
| IDENTITY-003 | [IDENTITY-003-AC-06](identity-context/IDENTITY-003-execution-local-context-scope.md#identity-003-ac-06--exceptional-handler-exit) | Workflow integration |
| INBOX-001 | [INBOX-001-AC-01](inbox/INBOX-001-typed-admission-and-envelope-identity.md#inbox-001-ac-01--admit-without-claiming-consumer-completion) | Relational integration |
| INBOX-001 | [INBOX-001-AC-02](inbox/INBOX-001-typed-admission-and-envelope-identity.md#inbox-001-ac-02--default-and-distinct-logical-identity) | Relational integration |
| INBOX-004 | [INBOX-004-AC-04](inbox/INBOX-004-durable-entry-state-and-fenced-claims.md#inbox-004-ac-04--validate-claim-limits-and-renew-successful-ownership) | Unit + relational integration |
| INBOX-005 | [INBOX-005-AC-04](inbox/INBOX-005-atomic-local-delivery-and-repeatable-execution.md#inbox-005-ac-04--preserve-uncertainty-without-claiming-exactly-once) | Fault-injection integration |
| INBOX-006 | [INBOX-006-AC-01](inbox/INBOX-006-processor-metadata-failure-settlement-and-cancellation.md#inbox-006-ac-01--expose-and-restore-delivery-metadata) | Unit |
| INBOX-006 | [INBOX-006-AC-02](inbox/INBOX-006-processor-metadata-failure-settlement-and-cancellation.md#inbox-006-ac-02--classify-retry-and-permanent-failure) | Relational integration |
| INBOX-006 | [INBOX-006-AC-05](inbox/INBOX-006-processor-metadata-failure-settlement-and-cancellation.md#inbox-006-ac-05--bound-failed-settlement-independently) | Fault-injection integration |
| INBOX-007 | [INBOX-007-AC-02](inbox/INBOX-007-signals-workers-and-restart-recovery.md#inbox-007-ac-02--recover-after-crash-without-a-hint) | Relational integration |
| INBOX-007 | [INBOX-007-AC-03](inbox/INBOX-007-signals-workers-and-restart-recovery.md#inbox-007-ac-03--recover-after-claim-or-processing-infrastructure-failures) | Fault-injection integration |
| INBOX-008 | [INBOX-008-AC-05](inbox/INBOX-008-dead-letter-inspection-and-independent-management.md#inbox-008-ac-05--reject-null-management-collections) | Unit |
| INBOX-009 | [INBOX-009-AC-03](inbox/INBOX-009-replay-retained-admissions-into-missing-targets.md#inbox-009-ac-03--handle-invalid-and-empty-replay-selection) | Unit + integration |
| INBOX-009 | [INBOX-009-AC-07](inbox/INBOX-009-replay-retained-admissions-into-missing-targets.md#inbox-009-ac-07--report-current-swallowed-update-failure) | Fault-injection integration |
| INBOX-010 | [INBOX-010-AC-03](inbox/INBOX-010-entry-cleanup-and-indefinite-admission-retention.md#inbox-010-ac-03--retry-periodic-cleanup-without-successful-deletion-claim) | Fault-injection integration |
| INBOX-011 | [INBOX-011-AC-02](inbox/INBOX-011-composition-mappings-and-public-options.md#inbox-011-ac-02--require-positive-valid-shared-settings) | Unit |
| INBOX-011 | [INBOX-011-AC-03](inbox/INBOX-011-composition-mappings-and-public-options.md#inbox-011-ac-03--do-not-infer-installed-schema-from-model-validation) | Relational integration |
| OPS-003 | [OPS-003-AC-05](observability/OPS-003-terminal-cleanup-boundaries.md#ops-003-ac-05--evidence-removal) | Workflow integration |
| OPS-004 | [OPS-004-AC-02](observability/OPS-004-observable-metric-boundary.md#ops-004-ac-02--rejected-settlement-metrics) | Integration |
| OPS-004 | [OPS-004-AC-03](observability/OPS-004-observable-metric-boundary.md#ops-004-ac-03--completion-observation) | Integration |
| MQTT-001 | [MQTT-001-AC-02](mqtt/MQTT-001-registration-and-public-fluent-routes.md#mqtt-001-ac-02--incomplete-or-null-registration-fails-clearly) | Unit / DI integration |
| MQTT-001 | [MQTT-001-AC-03](mqtt/MQTT-001-registration-and-public-fluent-routes.md#mqtt-001-ac-03--provider-registration-alone-does-not-process-durable-work) | DI integration |
| MQTT-002 | [MQTT-002-AC-01](mqtt/MQTT-002-startup-options-and-mqtt-connection-configuration.md#mqtt-002-ac-01--invalid-operational-options-prevent-valid-startup-configuration) | Unit / options integration |
| MQTT-002 | [MQTT-002-AC-02](mqtt/MQTT-002-startup-options-and-mqtt-connection-configuration.md#mqtt-002-ac-02--validator-boundary-is-distinct-from-broker-validity) | Unit |
| MQTT-003 | [MQTT-003-AC-02](mqtt/MQTT-003-incoming-filters-choose-exactly-one-route.md#mqtt-003-ac-02--invalid-wildcard-placement-and-overlap-reject-configuration) | Unit / DI integration |
| MQTT-003 | [MQTT-003-AC-03](mqtt/MQTT-003-incoming-filters-choose-exactly-one-route.md#mqtt-003-ac-03--system-topic-distinction-and-level-boundaries-remain-explicit) | Unit + transport integration |
| MQTT-004 | [MQTT-004-AC-03](mqtt/MQTT-004-incoming-metadata-serialization-and-contract-compatibility.md#mqtt-004-ac-03--malformed-identity-and-mismatched-contract-are-poison) | Unit + transport integration |
| MQTT-004 | [MQTT-004-AC-04](mqtt/MQTT-004-incoming-metadata-serialization-and-contract-compatibility.md#mqtt-004-ac-04--last-exact-name-property-wins) | Unit |
| MQTT-004 | [MQTT-004-AC-05](mqtt/MQTT-004-incoming-metadata-serialization-and-contract-compatibility.md#mqtt-004-ac-05--payload-segmentation-and-content-type-do-not-alter-decoding-policy) | Unit + persistence integration |
| MQTT-005 | [MQTT-005-AC-03](mqtt/MQTT-005-acknowledgement-follows-admission-not-consumer-effects.md#mqtt-005-ac-03--duplicate-receipt-after-lost-acknowledgement-does-not-reset-work) | Transport + concurrency integration |
| MQTT-006 | [MQTT-006-AC-02](mqtt/MQTT-006-receive-failures-poison-acknowledgement-and-shutdown.md#mqtt-006-ac-02--shutdown-cancellation-leaves-unsettled-delivery-unacknowledged) | Transport integration |
| MQTT-006 | [MQTT-006-AC-03](mqtt/MQTT-006-receive-failures-poison-acknowledgement-and-shutdown.md#mqtt-006-ac-03--no-consumer-is-an-admission-failure-rather-than-automatic-poison-discard) | Transport + DI integration |
| MQTT-006 | [MQTT-006-AC-04](mqtt/MQTT-006-receive-failures-poison-acknowledgement-and-shutdown.md#mqtt-006-ac-04--wrapped-mapper-cancellation-currently-reaches-poison-policy) | Unit + transport integration |
| MQTT-008 | [MQTT-008-AC-03](mqtt/MQTT-008-inbox-reconnect-subscription-results-and-cleanup.md#mqtt-008-ac-03--concurrent-failure-signals-do-not-queue-unbounded-reconnects) | Concurrency + transport integration |
| MQTT-009 | [MQTT-009-AC-02](mqtt/MQTT-009-outgoing-target-and-destination-staging.md#mqtt-009-ac-02--target-identity-validation-and-duplicates-prevent-ambiguous-work) | Unit / persistence integration |
| MQTT-009 | [MQTT-009-AC-03](mqtt/MQTT-009-outgoing-target-and-destination-staging.md#mqtt-009-ac-03--blank-destination-fails-staging-topic-grammar-has-another-boundary) | Unit + transport integration |
| MQTT-009 | [MQTT-009-AC-05](mqtt/MQTT-009-outgoing-target-and-destination-staging.md#mqtt-009-ac-05--exact-type-target-matching-does-not-imply-polymorphic-publication) | Unit |
| MQTT-010 | [MQTT-010-AC-01](mqtt/MQTT-010-outgoing-packet-metadata-and-retain-semantics.md#mqtt-010-ac-01--packet-includes-stored-route-destination-payload-and-application-identities) | Transport integration |
| MQTT-011 | [MQTT-011-AC-02](mqtt/MQTT-011-outgoing-connection-ownership-concurrency-and-disposal.md#mqtt-011-ac-02--connection-rejection-and-later-disconnect-require-another-durable-attempt) | Transport integration |
| MQTT-011 | [MQTT-011-AC-04](mqtt/MQTT-011-outgoing-connection-ownership-concurrency-and-disposal.md#mqtt-011-ac-04--disposal-releases-owned-client-without-promised-graceful-disconnect) | Lifecycle integration |
| MQTT-012 | [MQTT-012-AC-04](mqtt/MQTT-012-publish-result-durable-completion-and-failure-classification.md#mqtt-012-ac-04--missing-route-or-mapping-prevents-successful-publication) | Unit + transport integration |
| MQTT-013 | [MQTT-013-AC-02](mqtt/MQTT-013-external-repeat-cancellation-and-independently-successful-targets.md#mqtt-013-ac-02--lease-fencing-limits-settlement-not-an-issued-publish) | Concurrency + transport integration |
| MQTT-013 | [MQTT-013-AC-04](mqtt/MQTT-013-external-repeat-cancellation-and-independently-successful-targets.md#mqtt-013-ac-04--successful-sibling-target-remains-settled-during-another-failure) | Persistence + transport integration |
| MQTT-014 | [MQTT-014-AC-02](mqtt/MQTT-014-observable-logs-do-not-overstate-settlement.md#mqtt-014-ac-02--connection-and-admission-diagnostics-retain-their-operational-boundary) | Transport / logging integration |
| OUTBOX-001 | [OUTBOX-001-AC-02](outbox/OUTBOX-001-enqueue-stages-a-concrete-envelope-and-target-snapshot.md#outbox-001-ac-02--preserve-logical-identity-with-new-delivery-identity) | Unit + relational integration |
| OUTBOX-001 | [OUTBOX-001-AC-03](outbox/OUTBOX-001-enqueue-stages-a-concrete-envelope-and-target-snapshot.md#outbox-001-ac-03--reject-invalid-envelope-and-serialization) | Unit |
| OUTBOX-001 | [OUTBOX-001-AC-04](outbox/OUTBOX-001-enqueue-stages-a-concrete-envelope-and-target-snapshot.md#outbox-001-ac-04--snapshot-targets-before-storage) | Unit + integration |
| OUTBOX-003 | [OUTBOX-003-AC-02](outbox/OUTBOX-003-target-validation-local-selection-and-empty-batches.md#outbox-003-ac-02--do-not-infer-transport-or-destination-schema-validation) | Unit + integration |
| OUTBOX-003 | [OUTBOX-003-AC-04](outbox/OUTBOX-003-target-validation-local-selection-and-empty-batches.md#outbox-003-ac-04--stage-an-empty-custom-batch-without-work) | Unit + integration |
| OUTBOX-006 | [OUTBOX-006-AC-01](outbox/OUTBOX-006-public-save-capture-protocol-and-per-context-deduplication.md#outbox-006-ac-01--deduplicate-captures-across-saves-in-one-transaction) | Unit + integration |
| OUTBOX-007 | [OUTBOX-007-AC-02](outbox/OUTBOX-007-save-failure-cancellation-rollback-and-retry-tracking.md#outbox-007-ac-02--retain-events-for-retry-and-clear-observed-rollback-keys) | Relational integration |
| OUTBOX-007 | [OUTBOX-007-AC-03](outbox/OUTBOX-007-save-failure-cancellation-rollback-and-retry-tracking.md#outbox-007-ac-03--reject-changed-transaction-identity-with-pending-work) | Unit + integration |
| OUTBOX-009 | [OUTBOX-009-AC-03](outbox/OUTBOX-009-savepoints-ambient-transactions-and-observer-restrictions.md#outbox-009-ac-03--expose-unobserved-transaction-end-restriction) | Fault-injection integration |
| OUTBOX-010 | [OUTBOX-010-AC-01](outbox/OUTBOX-010-postcommit-callbacks-and-wake-hints.md#outbox-010-ac-01--acknowledge-capture-rather-than-delivery) | Unit + integration |
| OUTBOX-010 | [OUTBOX-010-AC-03](outbox/OUTBOX-010-postcommit-callbacks-and-wake-hints.md#outbox-010-ac-03--observe-callback-only-participation-gap) | Unit |
| OUTBOX-011 | [OUTBOX-011-AC-02](outbox/OUTBOX-011-transport-dispatch-and-external-repeat-window.md#outbox-011-ac-02--do-not-complete-on-transport-failure) | Unit + integration |
| OUTBOX-012 | [OUTBOX-012-AC-01](outbox/OUTBOX-012-delivery-registration-and-local-atomic-dispatch.md#outbox-012-ac-01--reject-ambiguous-or-missing-transport-selection) | Unit |
| OUTBOX-012 | [OUTBOX-012-AC-04](outbox/OUTBOX-012-delivery-registration-and-local-atomic-dispatch.md#outbox-012-ac-04--use-registered-external-wrapper) | Composition integration |
| OUTBOX-013 | [OUTBOX-013-AC-03](outbox/OUTBOX-013-owned-durable-state-processor-policy-and-cancellation.md#outbox-013-ac-03--expose-and-restore-ambient-metadata) | Unit |
| OUTBOX-013 | [OUTBOX-013-AC-04](outbox/OUTBOX-013-owned-durable-state-processor-policy-and-cancellation.md#outbox-013-ac-04--separate-shutdown-and-ownership-cancellation) | Unit + integration |
| OUTBOX-013 | [OUTBOX-013-AC-05](outbox/OUTBOX-013-owned-durable-state-processor-policy-and-cancellation.md#outbox-013-ac-05--do-not-invent-completion-on-settlement-infrastructure-failure) | Fault-injection integration |
| OUTBOX-014 | [OUTBOX-014-AC-02](outbox/OUTBOX-014-workers-signals-bounded-queue-and-recovery.md#outbox-014-ac-02--coalesce-signals-and-cancel-waits) | Unit |
| OUTBOX-015 | [OUTBOX-015-AC-04](outbox/OUTBOX-015-dead-letter-management-and-cleanup-horizon.md#outbox-015-ac-04--do-not-hide-cleanup-failure) | Fault-injection integration |
| OUTBOX-015 | [OUTBOX-015-AC-05](outbox/OUTBOX-015-dead-letter-management-and-cleanup-horizon.md#outbox-015-ac-05--reject-null-key-collections-and-invalid-cleanup-limit) | Unit + integration |
| OUTBOX-016 | [OUTBOX-016-AC-03](outbox/OUTBOX-016-ef-composition-mappings-options-and-validation.md#outbox-016-ac-03--do-not-equate-model-validation-with-schema-certification) | Relational integration |
| PERSIST-002 | [PERSIST-002-AC-03](persistence/PERSIST-002-local-completion-atomicity.md#persist-002-ac-03--stale-completion) | Concurrency integration |
| PERSIST-002 | [PERSIST-002-AC-04](persistence/PERSIST-002-local-completion-atomicity.md#persist-002-ac-04--external-effect-excluded) | Workflow integration |
| PERSIST-003 | [PERSIST-003-AC-03](persistence/PERSIST-003-store-cancellation-and-invalid-claim-boundaries.md#persist-003-ac-03--pre-cancelled-database-operation) | Persistence integration |
| WORK-002 | [WORK-002-AC-04](recovery-lifecycle/WORK-002-signal-observation-and-cancellation.md#work-002-ac-04--initial-discovery) | Integration |
| WORK-003 | [WORK-003-AC-01](recovery-lifecycle/WORK-003-bounded-processing-and-failure-recovery.md#work-003-ac-01--concurrency-bound) | Concurrency integration |
| WORK-004 | [WORK-004-AC-02](recovery-lifecycle/WORK-004-queue-leases-and-producer-delay.md#work-004-ac-02--producer-delay-budget) | Integration |
| WORK-004 | [WORK-004-AC-03](recovery-lifecycle/WORK-004-queue-leases-and-producer-delay.md#work-004-ac-03--invalid-loop-options) | Unit |
| RETRY-005 | [RETRY-005-AC-01](retry-failures/RETRY-005-processing-settlement-boundary.md#retry-005-ac-01--failure-rollback-and-retry) | Persistence integration |
| RETRY-005 | [RETRY-005-AC-02](retry-failures/RETRY-005-processing-settlement-boundary.md#retry-005-ac-02--lost-ownership) | Concurrency integration |
| SAGA-001 | [SAGA-001-AC-02](sagas/SAGA-001-workflow-identity-and-local-outgoing-consistency.md#saga-001-ac-02--local-state-and-durable-intentions-are-atomic) | persistence |
| SAGA-002 | [SAGA-002-AC-04](sagas/SAGA-002-definitions-handler-overloads-and-registration-validation.md#saga-002-ac-04--timer-only-mapping-is-instance-directed) | workflow |
| SAGA-002 | [SAGA-002-AC-06](sagas/SAGA-002-definitions-handler-overloads-and-registration-validation.md#saga-002-ac-06--registered-handler-is-scope-resolved) | workflow |
| SAGA-003 | [SAGA-003-AC-01](sagas/SAGA-003-composition-persistence-mapping-and-saga-options.md#saga-003-ac-01--runtime-composition-is-incomplete-without-storeseffects) | workflow |
| SAGA-003 | [SAGA-003-AC-02](sagas/SAGA-003-composition-persistence-mapping-and-saga-options.md#saga-003-ac-02--ef-model-and-interceptors-include-timers) | persistence |
| SAGA-004 | [SAGA-004-AC-03](sagas/SAGA-004-identity-values-and-durable-correlation.md#saga-004-ac-03--correlation-function-chooses-workflow) | persistence |
| SAGA-004 | [SAGA-004-AC-05](sagas/SAGA-004-identity-values-and-durable-correlation.md#saga-004-ac-05--terminal-key-still-occupies-durable-identity) | persistence |
| SAGA-005 | [SAGA-005-AC-08](sagas/SAGA-005-start-continuation-and-duplicate-routing-matrix.md#saga-005-ac-08--general-duplicate-continuation-belongs-to-owner-deduplication) | workflow |
| SAGA-006 | [SAGA-006-AC-01](sagas/SAGA-006-handler-results-and-immutable-effect-descriptions.md#saga-006-ac-01--continue-complete-and-fail-transition-outcomes) | persistence |
| SAGA-006 | [SAGA-006-AC-05](sagas/SAGA-006-handler-results-and-immutable-effect-descriptions.md#saga-006-ac-05--null-state-factory-and-null-handler-result-fail) | workflow |
| SAGA-006 | [SAGA-006-AC-06](sagas/SAGA-006-handler-results-and-immutable-effect-descriptions.md#saga-006-ac-06--throwing-handler-does-not-apply-a-returned-result) | persistence |
| SAGA-007 | [SAGA-007-AC-03](sagas/SAGA-007-state-serialization-and-retained-schema-compatibility.md#saga-007-ac-03--retained-type-and-message-compatibility-are-required) | workflow |
| SAGA-008 | [SAGA-008-AC-01](sagas/SAGA-008-state-application-and-terminal-invariants.md#saga-008-ac-01--active-result-applies-one-candidate-state-transition) | persistence |
| SAGA-008 | [SAGA-008-AC-02](sagas/SAGA-008-state-application-and-terminal-invariants.md#saga-008-ac-02--rejected-expected-version-cannot-overwrite-state) | concurrency |
| SAGA-010 | [SAGA-010-AC-07](sagas/SAGA-010-timer-delivery-business-failure-and-exhaustion.md#saga-010-ac-07--timer-handler-and-consumption-join-owner-commit) | persistence |
| SAGA-011 | [SAGA-011-AC-04](sagas/SAGA-011-owner-transactions-rollback-savepoints-and-replay.md#saga-011-ac-04--execution-strategy-may-repeat-handler) | workflow |
| SAGA-012 | [SAGA-012-AC-01](sagas/SAGA-012-state-version-races-and-concurrent-continuation.md#saga-012-ac-01--one-applied-transition-advances-version) | persistence |
| SAGA-012 | [SAGA-012-AC-04](sagas/SAGA-012-state-version-races-and-concurrent-continuation.md#saga-012-ac-04--retries-belong-to-processing-owner) | workflow |
| SAGA-013 | [SAGA-013-AC-04](sagas/SAGA-013-start-collision-translation-and-recovery-outcomes.md#saga-013-ac-04--nonqualifying-save-failures-retain-original-error) | persistence |
| SAGA-013 | [SAGA-013-AC-06](sagas/SAGA-013-start-collision-translation-and-recovery-outcomes.md#saga-013-ac-06--public-exception-classification-is-compatible) | unit |
| SAGA-016 | [SAGA-016-AC-03](sagas/SAGA-016-persisted-manager-and-timer-snapshots.md#saga-016-ac-03--limits-reject-nonpositive-values) | unit |
| SAGA-017 | [SAGA-017-AC-03](sagas/SAGA-017-retained-tombstones-purge-boundaries-and-lifecycle-limits.md#saga-017-ac-03--purge-is-periodic-bounded-and-recoverable) | workflow |
| SAGA-017 | [SAGA-017-AC-06](sagas/SAGA-017-retained-tombstones-purge-boundaries-and-lifecycle-limits.md#saga-017-ac-06--active-failed-timer-workflow-needs-application-policy) | workflow |
| SCHED-001 | [SCHED-001-AC-02](scheduling/SCHED-001-stable-logical-schedule-identity.md#sched-001-ac-02--distinct-operations-remain-distinct) | unit |
| SCHED-001 | [SCHED-001-AC-03](scheduling/SCHED-001-stable-logical-schedule-identity.md#sched-001-ac-03--message-delivery-preserves-logical-event-identity) | persistence |
| SCHED-002 | [SCHED-002-AC-01](scheduling/SCHED-002-repeated-delivery-and-external-effects.md#sched-002-ac-01--same-scheduled-operation-can-be-delivered-again) | workflow |
| SCHED-003 | [SCHED-003-AC-03](scheduling/SCHED-003-input-descriptions-serialization-and-utc-boundaries.md#sched-003-ac-03--identity-guards-cover-all-writer-commands) | unit |
| SCHED-003 | [SCHED-003-AC-04](scheduling/SCHED-003-input-descriptions-serialization-and-utc-boundaries.md#sched-003-ac-04--target-constructor-character-and-length-boundary) | unit |
| SCHED-003 | [SCHED-003-AC-06](scheduling/SCHED-003-input-descriptions-serialization-and-utc-boundaries.md#sched-003-ac-06--serializer-rejects-unsupported-durable-representation) | unit |
| SCHED-003 | [SCHED-003-AC-07](scheduling/SCHED-003-input-descriptions-serialization-and-utc-boundaries.md#sched-003-ac-07--identity-value-objects-are-not-admission-validation) | unit |
| SCHED-004 | [SCHED-004-AC-01](scheduling/SCHED-004-runtime-ef-outbox-and-model-composition.md#sched-004-ac-01--runtime-registration-is-not-durable-installation) | unit |
| SCHED-004 | [SCHED-004-AC-02](scheduling/SCHED-004-runtime-ef-outbox-and-model-composition.md#sched-004-ac-02--outbox-adapter-does-not-install-outbox-persistence) | workflow |
| SCHED-004 | [SCHED-004-AC-05](scheduling/SCHED-004-runtime-ef-outbox-and-model-composition.md#sched-004-ac-05--target-registration-is-exact-and-unique) | unit |
| SCHED-006 | [SCHED-006-AC-05](scheduling/SCHED-006-staging-duplicate-insertion-and-owner-visibility.md#sched-006-ac-05--duplicate-insert-race-preserves-one-identity) | concurrency |
| SCHED-007 | [SCHED-007-AC-06](scheduling/SCHED-007-cancellation-filters-terminal-states-and-races.md#sched-007-ac-06--concurrent-cancellation-does-not-reverse-delivery) | concurrency |
| SCHED-009 | [SCHED-009-AC-04](scheduling/SCHED-009-due-eligibility-claims-and-bounded-execution.md#sched-009-ac-04--claim-increases-attempts-before-delivery-admission) | concurrency |
| SCHED-009 | [SCHED-009-AC-05](scheduling/SCHED-009-due-eligibility-claims-and-bounded-execution.md#sched-009-ac-05--competing-claims-do-not-share-ownership) | concurrency |
| SCHED-009 | [SCHED-009-AC-07](scheduling/SCHED-009-due-eligibility-claims-and-bounded-execution.md#sched-009-ac-07--execution-parallelism-has-local-scope) | concurrency |
| SCHED-011 | [SCHED-011-AC-02](scheduling/SCHED-011-delivery-transaction-routing-and-retained-contract-failures.md#sched-011-ac-02--successful-message-schedule-stages-current-targets) | workflow |
| SCHED-011 | [SCHED-011-AC-04](scheduling/SCHED-011-delivery-transaction-routing-and-retained-contract-failures.md#sched-011-ac-04--unhandled-message-default-route-fails) | workflow |
| SCHED-012 | [SCHED-012-AC-04](scheduling/SCHED-012-failure-budgets-diagnostics-and-cancellation-recovery.md#sched-012-ac-04--capped-exponential-delays-have-no-jitter) | unit |
| SCHED-012 | [SCHED-012-AC-07](scheduling/SCHED-012-failure-budgets-diagnostics-and-cancellation-recovery.md#sched-012-ac-07--shutdown-cancellation-is-not-normal-message-failure) | workflow |
| SCHED-013 | [SCHED-013-AC-01](scheduling/SCHED-013-local-wake-engine-commit-hints-and-polling-recovery.md#sched-013-ac-01--time-and-local-hint-both-release-wait) | unit |
| SCHED-013 | [SCHED-013-AC-02](scheduling/SCHED-013-local-wake-engine-commit-hints-and-polling-recovery.md#sched-013-ac-02--past-wait-and-cancelled-future-wait) | unit |
| SCHED-013 | [SCHED-013-AC-05](scheduling/SCHED-013-local-wake-engine-commit-hints-and-polling-recovery.md#sched-013-ac-05--wake-after-failure-is-only-a-hint) | persistence |
| SCHED-013 | [SCHED-013-AC-06](scheduling/SCHED-013-local-wake-engine-commit-hints-and-polling-recovery.md#sched-013-ac-06--nested-saves-and-ambient-changed-work-transactions-are-rejected) | persistence |
| SCHED-014 | [SCHED-014-AC-01](scheduling/SCHED-014-quartz-wake-adapter-fallback-reconnect-and-shutdown.md#sched-014-ac-01--existing-default-scheduler-is-reused) | unit |
| SCHED-016 | [SCHED-016-AC-03](scheduling/SCHED-016-terminal-retention-recovery-boundaries-and-management-limits.md#sched-016-ac-03--cleanup-recovery-and-initial-timing) | workflow |
| TRANSPORT-001 | [TRANSPORT-001-AC-02](transport/TRANSPORT-001-preserve-representation-and-identity-boundaries.md#transport-001-ac-02--retry-does-not-create-a-new-logical-operation) | Transport integration |
| TRANSPORT-001 | [TRANSPORT-001-AC-03](transport/TRANSPORT-001-preserve-representation-and-identity-boundaries.md#transport-001-ac-03--consumer-contracts-remain-provider-neutral) | API contract review |
| TRANSPORT-004 | [TRANSPORT-004-AC-02](transport/TRANSPORT-004-incoming-typed-admission-and-metadata-loss-boundary.md#transport-004-ac-02--generic-ingress-trusts-the-selected-pair-rather-than-contract-text) | Unit |
| TRANSPORT-004 | [TRANSPORT-004-AC-03](transport/TRANSPORT-004-incoming-typed-admission-and-metadata-loss-boundary.md#transport-004-ac-03--anonymous-or-empty-identity-is-rejected) | Unit / persistence integration |
| TRANSPORT-004 | [TRANSPORT-004-AC-05](transport/TRANSPORT-004-incoming-typed-admission-and-metadata-loss-boundary.md#transport-004-ac-05--null-envelope-or-missing-map-is-not-accepted) | Unit |
| TRANSPORT-005 | [TRANSPORT-005-AC-01](transport/TRANSPORT-005-admission-failures-duplicates-and-cancellation.md#transport-005-ac-01--mapping-and-persistence-failures-remain-distinguishable) | Unit |
| TRANSPORT-005 | [TRANSPORT-005-AC-02](transport/TRANSPORT-005-admission-failures-duplicates-and-cancellation.md#transport-005-ac-02--concurrent-retained-duplicate-admissions-create-one-obligation-set) | Concurrency integration |
| TRANSPORT-005 | [TRANSPORT-005-AC-05](transport/TRANSPORT-005-admission-failures-duplicates-and-cancellation.md#transport-005-ac-05--synchronous-mapper-cancellation-currently-becomes-a-mapping-fault) | Unit |
| TRANSPORT-007 | [TRANSPORT-007-AC-03](transport/TRANSPORT-007-external-publication-and-per-target-completion-are-separate.md#transport-007-ac-03--stale-ownership-cannot-complete-another-attempt) | Concurrency integration |
| TRANSPORT-007 | [TRANSPORT-007-AC-04](transport/TRANSPORT-007-external-publication-and-per-target-completion-are-separate.md#transport-007-ac-04--sibling-completion-is-independent) | Persistence integration |


## Verification selection
- Unit: pure registration/serialization/mapping/state/policy outcomes.
- Persistence integration: real relational constraints, commits/rollbacks, retained truth and tracked-versus-stored effects.
- Concurrency integration: explicitly coordinated competing owners/start/cancel/commit paths without timing-only assertions.
- Transport integration: real MQTT packets/acknowledgements/session behavior with controlled dependencies.
- Workflow integration: complete inbox/outbox/schedule/saga effects and restart recovery boundaries.
- Host integration: service scopes, startup failure and explicitly configured persistence/interceptors.
- Verification should reference criterion IDs while asserting observable outcomes rather than internal helper calls.
