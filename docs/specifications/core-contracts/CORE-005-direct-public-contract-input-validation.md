# CORE-005 — Direct public contract input validation

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Extension authors need to know which registration and invocation boundaries validate identities and malformed data when they construct public records or custom invokers directly.

## Guarantees

- Supported extension registration and known contract/JSON mismatch handling MUST retain the guarantees of [CONFIG-003](../configuration/CONFIG-003-registration-identifier-boundaries.md), [CORE-003](CORE-003-lookup-and-dispatch-failures.md) and [CORE-004](CORE-004-durable-json-representation.md).

## Contract boundaries

This page concerns direct public records/custom invokers and malformed/null inputs outside those established domains. Public construction does not by itself approve bypassing registration constraints. No new direct-boundary failure timing/type is selected here.

## Acceptance criteria

No additional acceptance outcomes are assigned while the policy below remains unresolved.

## Open decisions

[GAP-CORE-001](../GAPS.md#gap-core-001): decide whether direct records/invokers enforce the extension identity domain. [GAP-CORE-008](../GAPS.md#gap-core-008): decide null invocation/default-record outcomes. Recommendations remain unapproved: define the supported direct boundary, then add clear guards without changing known permanent JSON/lookup failures.
