# Core contracts and durable serialization

[Specification index](../INDEX.md) · [Capability reference](../../reference/contracts.md)

## Purpose

Hosts associate stable durable contract names with message types, select unambiguous consumers and restore retained messages for typed asynchronous dispatch. Producers and consumers own message meaning and schema compatibility.

## Capability boundaries

IMessageConsumer, registries, MessageData, IMessageSerializer and ConsumerDispatcher define typed invocation and durable representation. Durable JSON is separate from [transport payloads](../transport/README.md). Dispatch completion is not durable target settlement.

## Decisions and limitations

No automatic version negotiation, historical upcasting, inheritance fan-out or processing order is promised. Direct record/custom invoker validation, malformed null records and live serializer reconfiguration remain [open decisions](../GAPS.md).

## Specifications

- [CORE-001 — Stable explicit contracts](CORE-001-stable-explicit-contracts.md)
- [CORE-002 — Registry uniqueness and selection](CORE-002-registry-uniqueness-and-selection.md)
- [CORE-003 — Lookup and dispatch failures](CORE-003-lookup-and-dispatch-failures.md)
- [CORE-004 — Durable JSON representation](CORE-004-durable-json-representation.md)
- [CORE-005 — Direct public contract input validation](CORE-005-direct-public-contract-input-validation.md)
