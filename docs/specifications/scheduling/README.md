# Scheduling specifications

[Specification index](../INDEX.md) · [Open decision register](../GAPS.md)

One-shot deferred delivery with caller-owned logical identities, persisted eligibility, scoped cancellation and recoverable delivery.

## Capability boundary

The host supplies persistence, schema and delivery composition. Due time is an eligibility instant, not a real-time promise. Wake engines, including Quartz, assist discovery; they do not own payload delivery. Same-context delivery effects and schedule completion share one local commit. External effects can repeat.

Each numbered page separates independently justified guarantees from reserved policy questions. Stable criterion IDs do not turn implementation observations or unapproved proposals into contracts. Snapshot reads report persisted retained evidence, not unsaved changes or operational repair.

## Specifications

- [SCHED-001 — Stable logical schedule identity](SCHED-001-stable-logical-schedule-identity.md)
- [SCHED-002 — Repeated delivery and external effects](SCHED-002-repeated-delivery-and-external-effects.md)
- [SCHED-003 — Input descriptions, serialization and UTC boundaries](SCHED-003-input-descriptions-serialization-and-utc-boundaries.md)
- [SCHED-004 — Runtime, EF, outbox and model composition](SCHED-004-runtime-ef-outbox-and-model-composition.md)
- [SCHED-005 — Options defaults, boundaries and binding](SCHED-005-options-defaults-boundaries-and-binding.md)
- [SCHED-006 — Staging, duplicate insertion and owner visibility](SCHED-006-staging-duplicate-insertion-and-owner-visibility.md)
- [SCHED-007 — Cancellation filters, terminal states and races](SCHED-007-cancellation-filters-terminal-states-and-races.md)
- [SCHED-008 — Persisted management snapshots](SCHED-008-persisted-management-snapshots.md)
- [SCHED-009 — Due eligibility, claims and bounded execution](SCHED-009-due-eligibility-claims-and-bounded-execution.md)
- [SCHED-010 — Expiry before admission and asymmetric settlement](SCHED-010-expiry-before-admission-and-asymmetric-settlement.md)
- [SCHED-011 — Delivery transaction, routing and retained contract failures](SCHED-011-delivery-transaction-routing-and-retained-contract-failures.md)
- [SCHED-012 — Failure budgets, diagnostics and cancellation recovery](SCHED-012-failure-budgets-diagnostics-and-cancellation-recovery.md)
- [SCHED-013 — Local wake engine, commit hints and polling recovery](SCHED-013-local-wake-engine-commit-hints-and-polling-recovery.md)
- [SCHED-014 — Quartz wake adapter, fallback, reconnect and shutdown](SCHED-014-quartz-wake-adapter-fallback-reconnect-and-shutdown.md)
- [SCHED-015 — Undecided integration hardening](SCHED-015-undecided-integration-hardening.md)
- [SCHED-016 — Terminal retention, recovery boundaries and management limits](SCHED-016-terminal-retention-recovery-boundaries-and-management-limits.md)

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

All proposals in the [decision register](../GAPS.md) remain unapproved. The numbered pages also preserve exact deadline, budget, ordering, diagnostic and retention questions.

- Admission: [default values](../GAPS.md#gap-wf-001), [repeated identity](../GAPS.md#gap-wf-002), [malformed/extreme options](../GAPS.md#gap-wf-013), [empty custom target sets](../GAPS.md#gap-wf-014).
- Ownership and discovery: [queued/long-running leases](../GAPS.md#gap-wf-003), [expired settlement](../GAPS.md#gap-wf-004), [unresponsive Quartz I/O](../GAPS.md#gap-wf-005), [savepoints and uncertain commit](../GAPS.md#gap-wf-006).
