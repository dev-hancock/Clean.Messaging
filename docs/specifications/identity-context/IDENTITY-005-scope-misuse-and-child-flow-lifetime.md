# IDENTITY-005 — Scope misuse and child-flow lifetime

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Applications spawning child work or disposing metadata scopes unusually need a defined lifetime contract so that current-delivery context is not accidentally treated as durable authority.

## Guarantees

- Normal delivery-scope use MUST retain the restoration and isolation guarantees of [IDENTITY-003](IDENTITY-003-execution-local-context-scope.md); no broader misuse outcome is assigned here.

## Contract boundaries

Out-of-order disposal, disposal from a different flow and fire-and-forget child handlers are not covered by the normal awaited/nested scope scenarios. Inherited execution context is not a transaction or durable scope extension.

## Acceptance criteria

No additional acceptance outcomes are assigned while the policy below remains unresolved.

## Open decisions

[GAP-CORE-002](../GAPS.md#gap-core-002): decide supported disposal/child-flow use. Recommendation remains unapproved: document normal awaited, nested use; only add misuse validation if predictable rejection is required.
