# Saga behavioural specifications

## Purpose and authority

Specifies durable saga definition/routing, consumer-owned state transitions, outgoing/timer intentions, owner transactions, concurrency, observation and terminal-state retention. It does not prescribe business deadlines, compensation or a domain-specific process.

**Required** is backed by explicit normative repository instructions or documented API contract; **Observed** records source-derived compatibility candidates requiring maintainer approval; **Proposed** is a conditional future policy; **Unknown** identifies undecided intent. ACs inherit their enclosing classification unless an explicit override is stated. Unknown behaviour has decision questions only, not acceptance criteria. Observed reference pages are evidence of current description, not independent normative authority.

## Common contracts and preconditions

Unless a criterion deliberately violates one, use registered durable message contracts, a running composed host, configured relational context/schema/interceptors, valid options, UTC TimeProvider observations, cooperative cancellation, and the same supported scoped owner transaction for local participating state/effects. Public runtime registration alone is insufficient. Persistence isolation, collation, execution strategy, clocks and dependency behaviour require provider-specific validation; no universal provider compatibility is guaranteed. Consumer business policy and migrations remain outside these specifications.

## Common postconditions and invariants

Read results describe committed persisted state, not unsaved local mutation. Success is scoped to its named boundary: staging is not commit, schedule consumption is not downstream completion, and local commit is not exactly-once external effects. Retained logical IDs are not interchangeable with delivery or ownership IDs. Failure, cancellation, concurrency loss, replay and terminal outcomes remain distinct. No criterion requires internal class identity, private method shape, or handler call-count guarantees.

## Failure, concurrency and limitations

Unknown and unresolved Proposed blocks retain decision questions; no unconditional outcome is approved before the linked gap decision. Scheduling has no renewal/upsert guarantee; saga state retention is not identity deletion. External custom delivery/handler work may repeat.

## Public contract references

See the [package contract and source map](../../reference/contract-map.md) for public declarations, supporting sources and their specification IDs. The contracts and observable boundaries are defined here and in the numbered specifications.

### End-to-end ownership trace

Ordinary consumer start/continuation context or schedule timer instance → retained definition/key/start identity → state deserialization/handler/result → expected version/state/outcome → append-ordered outgoing/timer/cancel intentions → terminal remaining-timer cancellation → inbox/local-outbox/schedule owner save/commit or rollback → later effects. Save failure resolves qualified start collisions; management and state purge retain terminal identities. See SAGA-001 through SAGA-017.

### Related contracts and boundary ownership

- [docs/CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- [docs/reference/architecture.md](../../reference/architecture.md)
- [docs/reference/configuration.md](../../reference/configuration.md)
- [docs/reference/contracts.md](../../reference/contracts.md)
- [docs/reference/persistence.md](../../reference/persistence.md)
- [docs/reference/outbox.md](../../reference/outbox.md)
- [docs/reference/processing.md](../../reference/processing.md)
- [docs/reference/scheduling.md](../../reference/scheduling.md)
- [docs/reference/sagas.md](../../reference/sagas.md)
- [docs/explanation/durable-workflows.md](../../explanation/durable-workflows.md)
- [docs/explanation/transaction-boundaries.md](../../explanation/transaction-boundaries.md)
- [docs/explanation/delivery-semantics.md](../../explanation/delivery-semantics.md)
- [docs/explanation/message-identity.md](../../explanation/message-identity.md)
- [docs/explanation/capability-boundaries.md](../../explanation/capability-boundaries.md)
- [src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs](../../../src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs)
- [src/Clean.Messaging.Outbox/IOutbox.cs](../../../src/Clean.Messaging.Outbox/IOutbox.cs)
- [src/Clean.Messaging.Outbox/Outbox.cs](../../../src/Clean.Messaging.Outbox/Outbox.cs)
- [src/Clean.Messaging.Outbox/Transport/OutboxTargetProvider.cs](../../../src/Clean.Messaging.Outbox/Transport/OutboxTargetProvider.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs](../../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxWriter.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs](../../../src/Clean.Messaging.Outbox.EntityFrameworkCore/OutboxTransactionInterceptor.cs)
- [src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxDispatcher.cs](../../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxDispatcher.cs)
- [src/Clean.Messaging.Outbox.EntityFrameworkCore/LocalOutboxDelivery.cs](../../../src/Clean.Messaging.Outbox.EntityFrameworkCore/LocalOutboxDelivery.cs)
- [src/Clean.Messaging/Serialization/MessageSerializer.cs](../../../src/Clean.Messaging/Serialization/MessageSerializer.cs)
- [src/Clean.Messaging/Processing/ConsumerDispatcher.cs](../../../src/Clean.Messaging/Processing/ConsumerDispatcher.cs)
- [src/Clean.Messaging/Consumers/ConsumerRegistry.cs](../../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs)
- [src/Clean.Messaging/DependencyInjection.cs](../../../src/Clean.Messaging/DependencyInjection.cs)
- [src/Clean.Messaging.EntityFrameworkCore/PersistenceServices.cs](../../../src/Clean.Messaging.EntityFrameworkCore/PersistenceServices.cs)

## Intent gaps

| Gap | Unresolved decision | Affected specifications |
| --- | --- | --- |
| [GAP-WF-001](../GAPS.md#gap-wf-001) | Must default target and default correlation key be rejected at the writer/runtime boundary? | SAGA-004, SAGA-015 |
| [GAP-WF-006](../GAPS.md#gap-wf-006) | Which caller savepoint and uncertain-commit patterns are supported across composed schedules/sagas/outbox? | SAGA-011 |
| [GAP-WF-007](../GAPS.md#gap-wf-007) | Must type/key equality be ordinal across supported providers, or are provider collations part of the contract? | SAGA-004, SAGA-015 |
| [GAP-WF-008](../GAPS.md#gap-wf-008) | Should Fail validate like SagaFailure, and what should null/blank/overlong values do? | SAGA-006, SAGA-014, SAGA-015 |
| [GAP-WF-009](../GAPS.md#gap-wf-009) | May an unrelated DbUpdateException translate when one added start and a matching winner exist? | SAGA-013, SAGA-015 |
| [GAP-WF-010](../GAPS.md#gap-wf-010) | Should caller cancellation during conflict lookup propagate, or preserve original DbUpdateException? | SAGA-013, SAGA-014 |
| [GAP-WF-011](../GAPS.md#gap-wf-011) | Is indefinite type/key/start tombstone retention and no restart/delete API approved lifecycle intent? | SAGA-004, SAGA-017 |
| [GAP-WF-012](../GAPS.md#gap-wf-012) | What happens contractually when two start messages for the same key run in one scope before SaveChanges? | SAGA-005, SAGA-015 |
| [GAP-WF-013](../GAPS.md#gap-wf-013) | What explicit failures apply to null nested scheduling options or null saga serializer options, and extreme otherwise-positive values? | SAGA-003 |
| [GAP-WF-015](../GAPS.md#gap-wf-015) | Which retained state/message schema changes are supported and who migrates incompatible work? | SAGA-007, SAGA-015 |

## Specifications

- [SAGA-001 — Workflow identity and local outgoing consistency](SAGA-001-workflow-identity-and-local-outgoing-consistency.md)
- [SAGA-002 — Definitions, handler overloads and registration validation](SAGA-002-definitions-handler-overloads-and-registration-validation.md)
- [SAGA-003 — Composition, persistence mapping and saga options](SAGA-003-composition-persistence-mapping-and-saga-options.md)
- [SAGA-004 — Identity values and durable correlation](SAGA-004-identity-values-and-durable-correlation.md)
- [SAGA-005 — Start, continuation and duplicate routing matrix](SAGA-005-start-continuation-and-duplicate-routing-matrix.md)
- [SAGA-006 — Handler results and immutable effect descriptions](SAGA-006-handler-results-and-immutable-effect-descriptions.md)
- [SAGA-007 — State serialization and retained schema compatibility](SAGA-007-state-serialization-and-retained-schema-compatibility.md)
- [SAGA-008 — State application and terminal invariants](SAGA-008-state-application-and-terminal-invariants.md)
- [SAGA-009 — Outgoing metadata, timers and terminal effect ordering](SAGA-009-outgoing-metadata-timers-and-terminal-effect-ordering.md)
- [SAGA-010 — Timer delivery, business failure and exhaustion](SAGA-010-timer-delivery-business-failure-and-exhaustion.md)
- [SAGA-011 — Owner transactions, rollback, savepoints and replay](SAGA-011-owner-transactions-rollback-savepoints-and-replay.md)
- [SAGA-012 — State version races and concurrent continuation](SAGA-012-state-version-races-and-concurrent-continuation.md)
- [SAGA-013 — Start collision translation and recovery outcomes](SAGA-013-start-collision-translation-and-recovery-outcomes.md)
- [SAGA-014 — Failures, cancellation and malformed result intent](SAGA-014-failures-cancellation-and-malformed-result-intent.md)
- [SAGA-015 — Undecided identity, schema and diagnostic hardening](SAGA-015-undecided-identity-schema-and-diagnostic-hardening.md)
- [SAGA-016 — Persisted manager and timer snapshots](SAGA-016-persisted-manager-and-timer-snapshots.md)
- [SAGA-017 — Retained tombstones, purge boundaries and lifecycle limits](SAGA-017-retained-tombstones-purge-boundaries-and-lifecycle-limits.md)
