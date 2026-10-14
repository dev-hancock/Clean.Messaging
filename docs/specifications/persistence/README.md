# Persistence capability catalogue

[Specification index](../INDEX.md) · [Documentation index](../../README.md)

## Purpose

Provider implementers and host authors need retained target identity, local transaction consistency and honest failure/cancellation outcomes. This catalogue defines durable outcomes rather than a particular query, index algorithm or universal provider-support claim.

## Capability lifecycle

Each retained delivery key identifies one target work record, and retained logical event/target uniqueness protects against duplicate representations while preserving sibling targets. Local inbox/outbox delivery commits configured-context effects with owned completion. Another context, database or external operation is outside that result.

## Shared contract boundaries

The host supplies a relational provider, mapped schema, scoped context and required capability integration. Standard stores guard positive claim/cleanup budgets and propagate I/O cancellation. Ownership is temporary authority for conditional settlement, not a guarantee that the supplied snapshot is current or that an external effect can be revoked. Ambiguous commit results require evidence, not an assumption of rollback.

The [coding standards](../../CODING_STANDARDS.md) establish ownership, identity, transaction, failure and resource obligations. Intentional public operation and persisted-contract compatibility further constrain these contracts. Reference pages describe implementation; they do not independently approve unresolved policy. Each numbered page separates required outcomes from reserved original obligations.

## Open decisions

Additional time/input-domain requirements remain in [GAP-CORE-004](../GAPS.md#gap-core-004). Provider scope and uncertain-commit attribution remain in [GAP-CORE-005](../GAPS.md#gap-core-005); none of its recommendations certifies a provider or weakens established local atomicity.

## Specifications

- [PERSIST-001 — Retained logical target uniqueness](PERSIST-001-retained-logical-target-uniqueness.md)
- [PERSIST-002 — Local completion atomicity](PERSIST-002-local-completion-atomicity.md)
- [PERSIST-003 — Store cancellation and invalid claim boundaries](PERSIST-003-store-cancellation-and-invalid-claim-boundaries.md)
- [PERSIST-004 — Commit ambiguity and provider boundaries](PERSIST-004-commit-ambiguity-and-provider-verification.md)

## Implementation trace

These links locate public/provider contracts for lookup; they do not prescribe internal structure.

- [src/Clean.Messaging/Persistence/IDurableEntry.cs](../../../src/Clean.Messaging/Persistence/IDurableEntry.cs)
- [src/Clean.Messaging/Persistence/IDurableStore.cs](../../../src/Clean.Messaging/Persistence/IDurableStore.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DurableEntryConfiguration.cs](../../../src/Clean.Messaging.EntityFrameworkCore/DurableEntryConfiguration.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs](../../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)
