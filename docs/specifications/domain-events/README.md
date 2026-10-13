# Domain events behavioural specifications

## Purpose and authority

Required behaviour is grounded only in normative repository instructions or explicit API XML requirements. Observed behaviour is derived from inspected implementation. Source-derived Reference and Explanation pages are descriptive corroboration, not independent normative authority. Proposed behaviour is explicitly **unapproved**. Unknown behaviour uses **numbered undecided questions**, without acceptance-criterion IDs. Every acceptance criterion inherits its specification classification unless explicitly marked otherwise; none below overrides it. Required criteria can identify suspected source mismatches and are not claims of conformance.

## Capability purpose, contracts and common boundaries

Consumer-owned raising/custom source -> tracked-source capture in owning save -> concrete outbox target staging -> commit observation/verification -> source acknowledgement -> independent durable outbox delivery.

- **Preconditions:** consumer host owns registration, valid durable contracts and identities, configured relational schema/transactions where applicable, stable payload/source state and suitable cancellation/lifetime composition. Each focused specification narrows these preconditions.
- **Postconditions:** only observable admitted, staged, committed, acknowledged or per-target lifecycle outcomes explicitly stated below; success at one boundary does not imply success at another.
- **Invariants:** preserve logical event identity versus delivery identity, concrete durable contract names, original envelope/context metadata, target-specific completion, ownership fences, retry classifications and owning-context transaction boundaries. Repeated external effects remain possible.
- **Failures:** validation/serialization/resolution/database/transport failures and cancellation remain distinct; ordinary logging is not durable success. Source exceptions/cancellation racing commit require evidence, not an assumption of rollback.
- **Concurrency:** contexts and source collections are not automatically thread-safe. Lease ownership is fenced at settlement, not permission to suppress repeated external calls. Recovery requires a running healthy host/store; no strict latency, fairness or global completion order is specified.
- **Limitations:** Raising is in memory, not event sourcing. The base enforces no application-specific invariant. Ack means capture committed, not consumers finished. No automatic uncaptured-event crash recovery, historic domain replay, custom-source synchronization or global source-ID validation exists.

### Common public model and lifecycle contracts

Record positional fields, exposed init/set properties, constants, generic constraints, explicit interface implementations and protected extension operations are included in the source/contract table. Public primary constructors and inherited hosted StartAsync/StopAsync/Dispose behaviour are composition surfaces, not additional delivery guarantees. For entries, all identity/payload/context fields, Attempts, ClaimId, ClaimedUntil, NextAttemptAt, ProcessedAt, DeadLetteredAt, DiscardedAt, FailureCategory, FailureCode, ExceptionType, FailureMessage, LastFailedAt, LastFailedAttempt and derived Key participate in the lifecycle specifications. Port implementations (including internal EF adapters) are separately traced by responsibility.

### Related references

[Repository instructions](../../../.github/copilot-instructions.md), [coding standards](../../CODING_STANDARDS.md), [documentation index](../../README.md); references: [architecture](../../reference/architecture.md), [configuration](../../reference/configuration.md), [contracts](../../reference/contracts.md), [inbox](../../reference/inbox.md), [outbox](../../reference/outbox.md), [domain events](../../reference/domain-events.md), [persistence](../../reference/persistence.md), [processing](../../reference/processing.md), [transport](../../reference/transport.md). Explanations: [capability boundaries](../../explanation/capability-boundaries.md), [message identity](../../explanation/message-identity.md), [transaction boundaries](../../explanation/transaction-boundaries.md), [delivery semantics](../../explanation/delivery-semantics.md), [durable workflows](../../explanation/durable-workflows.md). Adjacent ingress/broker/mapper APIs are outside these owned files; their source-derived descriptions do not independently establish required behaviour.

## Public contract references

See the [package contract and source map](../../reference/contract-map.md) for public declarations, supporting sources and their specification IDs. The contracts and observable boundaries are defined here and in the numbered specifications.

### Shared dependency contracts

| Source path | Responsibility | Related specifications |
| --- | --- | --- |
| [src\Clean.Messaging\Consumers\ConsumerRegistry.cs](../../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs) | Exact-type/name consumer selection, permanent faults | INBOX-001, INBOX-009; OUTBOX-003; DOMAIN-007; DOMAIN-003–DOMAIN-009 via composed outbox |
| [src\Clean.Messaging\Serialization\MessageSerializer.cs](../../../src/Clean.Messaging/Serialization/MessageSerializer.cs) | Concrete runtime contract serialization and fault classification | INBOX-001; OUTBOX-001; DOMAIN-004, DOMAIN-007; DOMAIN-003–DOMAIN-009 via composed outbox |
| [src\Clean.Messaging\Persistence\IDurableStore.cs](../../../src/Clean.Messaging/Persistence/IDurableStore.cs) | Inherited Claim/Renew/Complete/Retry/DeadLetter/Cleanup and OwnedEntry | INBOX-004; OUTBOX-013; DOMAIN-003–DOMAIN-009 via composed outbox |
| [src\Clean.Messaging\Persistence\IDurableEntry.cs](../../../src/Clean.Messaging/Persistence/IDurableEntry.cs) | Common entry identity/state contract | INBOX-004; OUTBOX-013; DOMAIN-003–DOMAIN-009 via composed outbox |
| [src\Clean.Messaging.EntityFrameworkCore\DurableStore.cs](../../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs) | Claim ordering/eligibility, ownership fence, lifecycle updates/cleanup | INBOX-004, INBOX-010; OUTBOX-013, OUTBOX-015; DOMAIN-003–DOMAIN-009 via composed outbox |
| [src\Clean.Messaging.EntityFrameworkCore\PersistenceServices.cs](../../../src/Clean.Messaging.EntityFrameworkCore/PersistenceServices.cs) | Same scoped persistence context and options composition | INBOX-011; OUTBOX-016; DOMAIN-003–DOMAIN-009 via composed outbox |

Domain capture also depends directly on the outbox EF tracker/writer/interceptors and provider-neutral OutboxEntryFactory; their full source/contract trace is in [outbox specifications](../outbox/README.md#public-contract-references).

## Contract limits

Source-derived ordering, transaction interception and uniqueness outcomes do not establish universal provider guarantees. Observed suspected discrepancies remain unresolved in [GAPS.md](../GAPS.md), not approved fixes.

## Specifications

- [DOMAIN-001 — Raise pending events without owning business policy](DOMAIN-001-raise-pending-events-without-owning-business-policy.md)
- [DOMAIN-002 — Acknowledgement removes pending facts by identity](DOMAIN-002-acknowledgement-removes-pending-facts-by-identity.md)
- [DOMAIN-003 — Opt-in EF capture composition](DOMAIN-003-opt-in-ef-capture-composition.md)
- [DOMAIN-004 — Capture tracked sources across entity states](DOMAIN-004-capture-tracked-sources-across-entity-states.md)
- [DOMAIN-005 — Atomic captured facts and application commit](DOMAIN-005-atomic-captured-facts-and-application-commit.md)
- [DOMAIN-006 — Defer acknowledgement and deduplicate multi-save capture](DOMAIN-006-defer-acknowledgement-and-deduplicate-multi-save-capture.md)
- [DOMAIN-007 — Invalid custom facts and targetless acknowledgement](DOMAIN-007-invalid-custom-facts-and-targetless-acknowledgement.md)
- [DOMAIN-008 — Custom source collisions and acknowledgement failures](DOMAIN-008-custom-source-collisions-and-acknowledgement-failures.md)
- [DOMAIN-009 — Crash, recovery and retention boundaries](DOMAIN-009-crash-recovery-and-retention-boundaries.md)
- [DOMAIN-010 — Undecided source and acknowledgement contract](DOMAIN-010-undecided-source-and-acknowledgement-contract.md)
