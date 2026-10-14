# Message identity and execution context

[Specification index](../INDEX.md) · [Capability reference](../../reference/contracts.md)

## Purpose

Retry and recovery preserve logical operations and individual target completion. Execution-local delivery metadata lets handlers relate follow-on work without treating correlation as duplicate identity.

## Capability boundaries

Message/event/target IDs, optional correlation/causation and temporary claim tokens have distinct roles. IMessageContext/MessageContext expose scoped metadata, not EventId, ownership authority, a database transaction or distributed tracing. Initiating capabilities own identity generation and propagation.

## Decisions and limitations

Normal awaited, nested scope use is covered; unusual disposal/child lifetimes remain [GAP-CORE-002](../GAPS.md#gap-core-002). Opaque token usability does not automatically approve its exact GUID encoding. Retention limits recovery evidence; external effects can repeat.

## Specifications

- [IDENTITY-001 — Stable identity across durable retries](IDENTITY-001-stable-identity-across-durable-retries.md)
- [IDENTITY-002 — Relationship metadata is not deduplication](IDENTITY-002-relationship-metadata-is-not-deduplication.md)
- [IDENTITY-003 — Execution-local context scope](IDENTITY-003-execution-local-context-scope.md)
- [IDENTITY-004 — Opaque claim token representation](IDENTITY-004-opaque-claim-token-representation.md)
- [IDENTITY-005 — Scope misuse and child-flow lifetime](IDENTITY-005-scope-misuse-and-child-flow-lifetime.md)
