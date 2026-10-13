# Core contracts and durable serialization — Behavioural specification

**Scope:** `IMessageConsumer<T>.Handle`, `IConsumerInvoker` (identity, message/consumer types and `Invoke`), `IConsumerRegistry.All/Get`, `MessageContract`, `IMessageContractRegistry.GetContract/GetType`, `IMessageSerializer.Serialize/Deserialize`, `MessageData`, `ConsumerDispatcher.Dispatch`.

## Purpose
Allow a host to associate stable durable contract names with message types, select consumers, and translate retained messages into typed invocations.

## Contracts, preconditions and postconditions
Preconditions: register message types and consumer services before processing; use valid registration identities. Postconditions: successful lookup returns the configured association; successful dispatch returns only after the selected consumer completes. Serialization here is durable JSON, not transport payload serialization. Cancellation of a consumer belongs to that invocation, not deletion of durable work.

## Invariants
- Registered names and target identities are compatibility data while retained work needs them.
- Registry selection and invocation must not invent a new logical operation or promise a global ordering.

## Failure, cancellation and consistency
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are unresolved decisions, not executable requirements.

## Limitations
- No automatic upcasting, version negotiation, polymorphic fan-out, historical type resolution, or consumer business policy.
- Direct construction of public registrations is not equivalent to validated extension registration; see GAP-CORE-001.

## Implementation trace
These locations explain contract ownership; they do not mandate internal implementation structure.

- [src/Clean.Messaging/Contracts/MessageContractRegistry.cs](../../../src/Clean.Messaging/Contracts/MessageContractRegistry.cs)
- [src/Clean.Messaging/Consumers/ConsumerRegistry.cs](../../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs)
- [src/Clean.Messaging/Serialization/MessageSerializer.cs](../../../src/Clean.Messaging/Serialization/MessageSerializer.cs)
- [src/Clean.Messaging/Processing/ConsumerDispatcher.cs](../../../src/Clean.Messaging/Processing/ConsumerDispatcher.cs)
- [src/Clean.Messaging.Abstractions/IMessageConsumer.cs](../../../src/Clean.Messaging.Abstractions/IMessageConsumer.cs)

## Open decisions
The [gap register](../GAPS.md) records unresolved input domains, provider evidence and proposed changes. [INDEX.md](../INDEX.md) explains classification authority and cross-capability ownership.

## Specifications

- [CORE-001 — Stable explicit contracts](CORE-001-stable-explicit-contracts.md)
- [CORE-002 — Registry uniqueness and selection](CORE-002-registry-uniqueness-and-selection.md)
- [CORE-003 — Lookup and dispatch failures](CORE-003-lookup-and-dispatch-failures.md)
- [CORE-004 — Durable JSON representation](CORE-004-durable-json-representation.md)
- [CORE-005 — Direct public contract input validation](CORE-005-direct-public-contract-input-validation.md)
