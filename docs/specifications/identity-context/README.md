# Message identity and execution context — Behavioural specification

**Scope:** `EntryKey`, `IMessageContext`, `MessageContext.Push` and disposable scope; `ClaimId.Create`; envelope identities and metadata are specified in inbox/outbox/domain/scheduling/saga/transport pages.

## Purpose
Separate logical operations, deliveries, targets, related operations and temporary ownership, while exposing execution-local metadata to handlers.

## Contracts, preconditions and postconditions
Pre: use identities appropriate to the initiating API and dispose metadata scopes in nesting order. Post: handlers see the selected delivery metadata and follow-on operations use the capability-specific propagation rules. Correlation and causation are optional relationships, not uniqueness constraints.

## Invariants
- Retries of stored work preserve logical, delivery and target identity.
- A target's success is independent of sibling failure.
- Ownership tokens describe attempts, not business identity.

## Failure, cancellation and consistency
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are unresolved decisions, not executable requirements.

## Limitations
- No exactly-once external effect, universal identifier meaning for every property called Id, automatic distributed tracing, or global ordering.
- Cross-context disposal and out-of-order scopes are unresolved; execution-local state can flow into child async work and is not durable.

## Implementation trace
These locations explain contract ownership; they do not mandate internal implementation structure.

- [src/Clean.Messaging.Abstractions/EntryKey.cs](../../../src/Clean.Messaging.Abstractions/EntryKey.cs)
- [src/Clean.Messaging.Abstractions/IMessageContext.cs](../../../src/Clean.Messaging.Abstractions/IMessageContext.cs)
- [src/Clean.Messaging/MessageContext.cs](../../../src/Clean.Messaging/MessageContext.cs)
- [src/Clean.Messaging/ClaimId.cs](../../../src/Clean.Messaging/ClaimId.cs)

## Open decisions
The [gap register](../GAPS.md) records unresolved input domains, provider evidence and proposed changes. [INDEX.md](../INDEX.md) explains classification authority and cross-capability ownership.

## Specifications

- [IDENTITY-001 — Stable identity across durable retries](IDENTITY-001-stable-identity-across-durable-retries.md)
- [IDENTITY-002 — Relationship metadata is not deduplication](IDENTITY-002-relationship-metadata-is-not-deduplication.md)
- [IDENTITY-003 — Execution-local context scope](IDENTITY-003-execution-local-context-scope.md)
- [IDENTITY-004 — Opaque claim token representation](IDENTITY-004-opaque-claim-token-representation.md)
- [IDENTITY-005 — Scope misuse and child-flow lifetime](IDENTITY-005-scope-misuse-and-child-flow-lifetime.md)
