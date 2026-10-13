# Persistence and transaction consistency — Behavioural specification

**Scope:** `IDurableEntry`, `IDurableStore<T>`, `OwnedEntry<T>`, public `DurableStore<TContext,TEntry>` operations/protected provider extension boundary, `DurableEntryConfiguration.Configure`; model/transaction helpers and store variants in capability pages.

## Purpose
Preserve durable local effects, retained identities and conditional ownership within explicit database boundaries without asserting distributed atomicity.

## Contracts, preconditions and postconditions
Pre: suitable relational provider, configured models/schema and standard store lifecycle; host-owned arbitrary model writes may bypass safeguards. Post: successful supported operations are observable through independent database reads. External transport effects and another context are outside local transaction participation.

## Invariants
- Persisted MessageId/TargetId identifies delivery work and EventId/TargetId protects retained logical target uniqueness.
- Configured-context effects and owned local completion must commit together where local dispatcher integration applies.
- A database claim cannot retract an issued remote effect.

## Failure, cancellation and consistency
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are unresolved decisions, not executable requirements.

## Limitations
- No universal provider/locking/isolation guarantee, SKIP LOCKED promise, universal DateTime.Kind enforcement or cross-database transaction.
- Bulk store mutations do not guarantee refresh of caller-tracked entities.
- Schema/index details are compatibility lookup in reference/persistence.md, not a mandate for every future internal storage design.

## Implementation trace
These locations explain contract ownership; they do not mandate internal implementation structure.

- [src/Clean.Messaging/Persistence/IDurableEntry.cs](../../../src/Clean.Messaging/Persistence/IDurableEntry.cs)
- [src/Clean.Messaging/Persistence/IDurableStore.cs](../../../src/Clean.Messaging/Persistence/IDurableStore.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DurableEntryConfiguration.cs](../../../src/Clean.Messaging.EntityFrameworkCore/DurableEntryConfiguration.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs](../../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)

## Open decisions
The [gap register](../GAPS.md) records unresolved input domains, provider evidence and proposed changes. [INDEX.md](../INDEX.md) explains classification authority and cross-capability ownership.

## Specifications

- [PERSIST-001 — Retained logical target uniqueness](PERSIST-001-retained-logical-target-uniqueness.md)
- [PERSIST-002 — Local completion atomicity](PERSIST-002-local-completion-atomicity.md)
- [PERSIST-003 — Store cancellation and invalid claim boundaries](PERSIST-003-store-cancellation-and-invalid-claim-boundaries.md)
- [PERSIST-004 — Commit ambiguity and provider verification](PERSIST-004-commit-ambiguity-and-provider-verification.md)
