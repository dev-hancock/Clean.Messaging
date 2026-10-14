# Sagas specifications

[Specification index](../INDEX.md) · [Open decision register](../GAPS.md)

Local workflow state with explicit correlation, replay-safe retained starts, version-fenced transitions and declarative outgoing/timer effects.

## Capability boundary

Applications own business transitions, correlation, timer meaning, effect identities and compensation. The host supplies one scoped persistence/transaction composition. Saga state and effects join the Inbox, local-Outbox or Scheduling owner commit; they do not create a distributed transaction. Terminal payload retention is distinct from retained identity and timer-history lifetime.

Each numbered page separates independently justified guarantees from reserved policy questions. Stable criterion IDs do not turn implementation observations or unapproved proposals into contracts. Snapshot reads report persisted retained evidence, not unsaved changes or operational repair.

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

## Related contracts and boundaries

The [package contract and source map](../../reference/contract-map.md) locates public declarations and supporting implementation without making observations normative.

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

## Open policy navigation

All proposals in the [decision register](../GAPS.md) remain unapproved. The numbered pages also preserve exact ordering, diagnostic-history and purge-policy questions.

- Identity and retained state: [default keys](../GAPS.md#gap-wf-001), [persisted equality](../GAPS.md#gap-wf-007), [tombstone/restart lifetime](../GAPS.md#gap-wf-011), [unsaved starts](../GAPS.md#gap-wf-012), [schema evolution](../GAPS.md#gap-wf-015).
- Failures and composition: [Fail diagnostics](../GAPS.md#gap-wf-008), [collision proof](../GAPS.md#gap-wf-009), [lookup cancellation](../GAPS.md#gap-wf-010), [savepoints/uncertain commit](../GAPS.md#gap-wf-006), [malformed/extreme options](../GAPS.md#gap-wf-013).
