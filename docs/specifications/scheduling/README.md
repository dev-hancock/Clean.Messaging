# Scheduling behavioural specifications

## Purpose and authority

Specifies provider-neutral scheduling, EF durability, message-to-outbox delivery and optional Quartz wake integration. It covers one-shot deferred work, not application recurrence policy or a promise to execute at an exact instant.

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

`IMessageScheduler` / `IScheduleWriter` → serialization/UTC/identity validation → EF staging → owner save/commit observation → local/Quartz wake plus polling → due/retry/lease claim → capacity wait → owned transactional dispatch → current delivery target → outbox EventId capture or saga timer → save/commit → later outgoing target delivery. Cancellation and cleanup operate on persisted/staged lifecycle; neither revokes external effects. See SCHED-001 through SCHED-016.

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
| [GAP-WF-001](../GAPS.md#gap-wf-001) | Must default target and default correlation key be rejected at the writer/runtime boundary? | SCHED-003, SCHED-015 |
| [GAP-WF-002](../GAPS.md#gap-wf-002) | Is an equal retry supposed to be idempotent admission, or must the owner retry/recognize a failed unit of work? | SCHED-001, SCHED-006 |
| [GAP-WF-003](../GAPS.md#gap-wf-003) | Should queued/long-running schedules renew or lease only when admitted, and what bounds are operationally required? | SCHED-009, SCHED-010 |
| [GAP-WF-004](../GAPS.md#gap-wf-004) | Is token-only expired failure settlement and no final lease check on successful execution deliberate? | SCHED-010, SCHED-012 |
| [GAP-WF-005](../GAPS.md#gap-wf-005) | Must local wake/deadline remain responsive when ScheduleJob hangs, rather than only throws? | SCHED-014, SCHED-015 |
| [GAP-WF-006](../GAPS.md#gap-wf-006) | Which caller savepoint and uncertain-commit patterns are supported across composed schedules/sagas/outbox? | SCHED-006, SCHED-013, SCHED-015 |
| [GAP-WF-013](../GAPS.md#gap-wf-013) | What explicit failures apply to null nested scheduling options or null saga serializer options, and extreme otherwise-positive values? | SCHED-005, SCHED-015 |
| [GAP-WF-014](../GAPS.md#gap-wf-014) | Is zero-target custom delivery success intended, unlike the default unhandled-message fault? | SCHED-011, SCHED-015 |

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
