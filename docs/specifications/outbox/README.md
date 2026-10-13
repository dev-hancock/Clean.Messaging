# Outbox behavioural specifications

## Purpose and authority

Required behaviour is grounded only in normative repository instructions or explicit API XML requirements. Observed behaviour is derived from inspected implementation. Source-derived Reference and Explanation pages are descriptive corroboration, not independent normative authority. Proposed behaviour is explicitly **unapproved**. Unknown behaviour uses **numbered undecided questions**, without acceptance-criterion IDs. Every acceptance criterion inherits its specification classification unless explicitly marked otherwise; none below overrides it. Required criteria can identify suspected source mismatches and are not claims of conformance.

## Capability purpose, contracts and common boundaries

Target selection -> scoped staging -> intercepted save/capture tracking -> observed/verified commit -> durable claim -> local transaction or external publication then fenced settlement -> retries/management -> terminal cleanup.

- **Preconditions:** consumer host owns registration, valid durable contracts and identities, configured relational schema/transactions where applicable, stable payload/source state and suitable cancellation/lifetime composition. Each focused specification narrows these preconditions.
- **Postconditions:** only observable admitted, staged, committed, acknowledged or per-target lifecycle outcomes explicitly stated below; success at one boundary does not imply success at another.
- **Invariants:** preserve logical event identity versus delivery identity, concrete durable contract names, original envelope/context metadata, target-specific completion, ownership fences, retry classifications and owning-context transaction boundaries. Repeated external effects remain possible.
- **Failures:** validation/serialization/resolution/database/transport failures and cancellation remain distinct; ordinary logging is not durable success. Source exceptions/cancellation racing commit require evidence, not an assumption of rollback.
- **Concurrency:** contexts and source collections are not automatically thread-safe. Lease ownership is fenced at settlement, not permission to suppress repeated external calls. Recovery requires a running healthy host/store; no strict latency, fairness or global completion order is specified.
- **Limitations:** Enqueue is staging, not save/commit/publication. Captured callback completion is not durable delivery. There is no atomic external publication+database settlement, unconditional persisted duplicate suppression, public outgoing replay history, universal user-savepoint support or certain outcome from unverifiable commits.

### Common public model and lifecycle contracts

Record positional fields, exposed init/set properties, constants, generic constraints, explicit interface implementations and protected extension operations are included in the source/contract table. Public primary constructors and inherited hosted StartAsync/StopAsync/Dispose behaviour are composition surfaces, not additional delivery guarantees. For entries, all identity/payload/context fields, Attempts, ClaimId, ClaimedUntil, NextAttemptAt, ProcessedAt, DeadLetteredAt, DiscardedAt, FailureCategory, FailureCode, ExceptionType, FailureMessage, LastFailedAt, LastFailedAttempt and derived Key participate in the lifecycle specifications. Port implementations (including internal EF adapters) are separately traced by responsibility.

### Related references

[Repository instructions](../../../.github/copilot-instructions.md), [coding standards](../../CODING_STANDARDS.md), [documentation index](../../README.md); references: [architecture](../../reference/architecture.md), [configuration](../../reference/configuration.md), [contracts](../../reference/contracts.md), [inbox](../../reference/inbox.md), [outbox](../../reference/outbox.md), [domain events](../../reference/domain-events.md), [persistence](../../reference/persistence.md), [processing](../../reference/processing.md), [transport](../../reference/transport.md). Explanations: [capability boundaries](../../explanation/capability-boundaries.md), [message identity](../../explanation/message-identity.md), [transaction boundaries](../../explanation/transaction-boundaries.md), [delivery semantics](../../explanation/delivery-semantics.md), [durable workflows](../../explanation/durable-workflows.md). Adjacent ingress/broker/mapper APIs are outside these owned files; their source-derived descriptions do not independently establish required behaviour.

## Public contract references

See the [package contract and source map](../../reference/contract-map.md) for public declarations, supporting sources and their specification IDs. The contracts and observable boundaries are defined here and in the numbered specifications.

### Shared dependency contracts

| Source path | Responsibility | Related specifications |
| --- | --- | --- |
| [src\Clean.Messaging\Consumers\ConsumerRegistry.cs](../../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs) | Exact-type/name consumer selection, permanent faults | INBOX-001, INBOX-009; OUTBOX-003; DOMAIN-007 |
| [src\Clean.Messaging\Processing\ConsumerDispatcher.cs](../../../src/Clean.Messaging/Processing/ConsumerDispatcher.cs) | Persisted contract deserialization and scoped invocation | INBOX-005; OUTBOX-012 |
| [src\Clean.Messaging\Serialization\MessageSerializer.cs](../../../src/Clean.Messaging/Serialization/MessageSerializer.cs) | Concrete runtime contract serialization and fault classification | INBOX-001; OUTBOX-001; DOMAIN-004, DOMAIN-007 |
| [src\Clean.Messaging\MessageContext.cs](../../../src/Clean.Messaging/MessageContext.cs) | Push/restoration and ambient IDs | INBOX-006; OUTBOX-002, OUTBOX-013 |
| [src\Clean.Messaging\Processing\WorkLoop.cs](../../../src/Clean.Messaging/Processing/WorkLoop.cs) | Bounded queue, scoped claim/processing and recovery | INBOX-007; OUTBOX-014 |
| [src\Clean.Messaging\Processing\ClaimRenewal.cs](../../../src/Clean.Messaging/Processing/ClaimRenewal.cs) | Renewal error/lost-claim cancellation | INBOX-006; OUTBOX-013 |
| [src\Clean.Messaging\Processing\AsyncAutoResetEvent.cs](../../../src/Clean.Messaging/Processing/AsyncAutoResetEvent.cs) | Coalesced cancelable in-memory wake | INBOX-007; OUTBOX-014 |
| [src\Clean.Messaging\Retry\RetryPolicy.cs](../../../src/Clean.Messaging/Retry/RetryPolicy.cs) | Retry classification and delay threshold | INBOX-006; OUTBOX-013 |
| [src\Clean.Messaging\Failures\MessageFailure.cs](../../../src/Clean.Messaging/Failures/MessageFailure.cs) | Retained bounded diagnostics/classification | INBOX-004, INBOX-006; OUTBOX-013 |
| [src\Clean.Messaging\MessageOptions.cs](../../../src/Clean.Messaging/MessageOptions.cs) | Inherited durable option members and defaults | INBOX-011; OUTBOX-016 |
| [src\Clean.Messaging\Processing\MessageOptionsValidator.cs](../../../src/Clean.Messaging/Processing/MessageOptionsValidator.cs) | Positive and cross-option validation | INBOX-011; OUTBOX-016 |
| [src\Clean.Messaging\Persistence\IDurableStore.cs](../../../src/Clean.Messaging/Persistence/IDurableStore.cs) | Inherited Claim/Renew/Complete/Retry/DeadLetter/Cleanup and OwnedEntry | INBOX-004; OUTBOX-013 |
| [src\Clean.Messaging\Persistence\IDurableEntry.cs](../../../src/Clean.Messaging/Persistence/IDurableEntry.cs) | Common entry identity/state contract | INBOX-004; OUTBOX-013 |
| [src\Clean.Messaging.EntityFrameworkCore\DurableStore.cs](../../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs) | Claim ordering/eligibility, ownership fence, lifecycle updates/cleanup | INBOX-004, INBOX-010; OUTBOX-013, OUTBOX-015 |
| [src\Clean.Messaging.EntityFrameworkCore\DeadLetterStore.cs](../../../src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs) | Independent management and diagnostic retention | INBOX-008; OUTBOX-015 |
| [src\Clean.Messaging.EntityFrameworkCore\DurableEntryConfiguration.cs](../../../src/Clean.Messaging.EntityFrameworkCore/DurableEntryConfiguration.cs) | Composite/logical unique keys and state mapping | INBOX-011; OUTBOX-004, OUTBOX-016 |
| [src\Clean.Messaging.EntityFrameworkCore\PersistenceServices.cs](../../../src/Clean.Messaging.EntityFrameworkCore/PersistenceServices.cs) | Same scoped persistence context and options composition | INBOX-011; OUTBOX-016 |

Management shared result contracts `DeadLetter(Key, EventId, DeadLetteredAt, Attempts, Code, Error)` and `RequeueResult(Requested, Requeued)` are referenced through the manager ports. No public API XML in these owned files supplies a stronger duplicate, completion or retention promise.

## Contract limits

Source-derived ordering, transaction interception and uniqueness outcomes do not establish universal provider guarantees. Observed suspected discrepancies remain unresolved in [GAPS.md](../GAPS.md), not approved fixes.

## Specifications

- [OUTBOX-001 — Enqueue stages a concrete envelope and target snapshot](OUTBOX-001-enqueue-stages-a-concrete-envelope-and-target-snapshot.md)
- [OUTBOX-002 — Ambient outgoing metadata and representation members](OUTBOX-002-ambient-outgoing-metadata-and-representation-members.md)
- [OUTBOX-003 — Target validation, local selection and empty batches](OUTBOX-003-target-validation-local-selection-and-empty-batches.md)
- [OUTBOX-004 — Tracked duplicate suppression versus persisted uniqueness](OUTBOX-004-tracked-duplicate-suppression-versus-persisted-uniqueness.md)
- [OUTBOX-005 — Atomic owning save and commit boundary](OUTBOX-005-atomic-owning-save-and-commit-boundary.md)
- [OUTBOX-006 — Public save capture protocol and per-context deduplication](OUTBOX-006-public-save-capture-protocol-and-per-context-deduplication.md)
- [OUTBOX-007 — Save failure, cancellation, rollback and retry tracking](OUTBOX-007-save-failure-cancellation-rollback-and-retry-tracking.md)
- [OUTBOX-008 — Commit verification and uncertain outcome](OUTBOX-008-commit-verification-and-uncertain-outcome.md)
- [OUTBOX-009 — Savepoints, ambient transactions and observer restrictions](OUTBOX-009-savepoints-ambient-transactions-and-observer-restrictions.md)
- [OUTBOX-010 — Postcommit callbacks and wake hints](OUTBOX-010-postcommit-callbacks-and-wake-hints.md)
- [OUTBOX-011 — Transport dispatch and external repeat window](OUTBOX-011-transport-dispatch-and-external-repeat-window.md)
- [OUTBOX-012 — Delivery registration and local atomic dispatch](OUTBOX-012-delivery-registration-and-local-atomic-dispatch.md)
- [OUTBOX-013 — Owned durable state, processor policy and cancellation](OUTBOX-013-owned-durable-state-processor-policy-and-cancellation.md)
- [OUTBOX-014 — Workers, signals, bounded queue and recovery](OUTBOX-014-workers-signals-bounded-queue-and-recovery.md)
- [OUTBOX-015 — Dead-letter management and cleanup horizon](OUTBOX-015-dead-letter-management-and-cleanup-horizon.md)
- [OUTBOX-016 — EF composition, mappings, options and validation](OUTBOX-016-ef-composition-mappings-options-and-validation.md)
- [OUTBOX-017 — Scoped cleanup lifetime requirement](OUTBOX-017-scoped-cleanup-lifetime-requirement.md)
- [OUTBOX-018 — Undecided duplicate, commit and callback policies](OUTBOX-018-undecided-duplicate-commit-and-callback-policies.md)
