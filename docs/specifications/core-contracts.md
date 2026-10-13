# Core contracts and durable serialization — Behavioural specification

**Status:** Draft; source reviewed 2026-10-13; no acceptance criterion has been executed.
**Scope:** `IMessageConsumer<T>.Handle`, `IConsumerInvoker` (identity, message/consumer types and `Invoke`), `IConsumerRegistry.All/Get`, `MessageContract`, `IMessageContractRegistry.GetContract/GetType`, `IMessageSerializer.Serialize/Deserialize`, `MessageData`, `ConsumerDispatcher.Dispatch`.

## Purpose
Allow a host to associate stable durable contract names with message types, select consumers, and translate retained messages into typed invocations.

## Contracts, preconditions and postconditions
Preconditions: register message types and consumer services before processing; use valid registration identities. Postconditions: successful lookup returns the configured association; successful dispatch returns only after the selected consumer completes. Serialization here is durable JSON, not transport payload serialization. Cancellation of a consumer belongs to that invocation, not deletion of durable work.

## Invariants
- Registered names and target identities are compatibility data while retained work needs them.
- Registry selection and invocation must not invent a new logical operation or promise a global ordering.

## Failure, cancellation and consistency
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are counted separately as gaps, not executable requirements.

## Limitations
- No automatic upcasting, version negotiation, polymorphic fan-out, historical type resolution, or consumer business policy.
- Direct construction of public registrations is not equivalent to validated extension registration; see GAP-CORE-001.

## Specifications and acceptance criteria

### CORE-001 — Stable explicit contracts
**Classification:** Required behaviour
**Evidence:** Coding standards §9 and §14 require persisted names and shapes to remain compatibility contracts; public registry and serializer signatures establish lookup/translation operations.

Pre: a registered type/name association and a compatible payload. Post: serializing and reading retained data preserves the agreed message meaning; replacing internals must not replace names with assembly-qualified type identity.

#### CORE-001-AC-01 — Registered association
**Given** a message type registered as `orders.created.v1`.
**When** its registered contract is requested.
**Then** the configured name is returned and resolving that name identifies the registered type.

#### CORE-001-AC-02 — Retained compatible message
**Given** a retained message using an unchanged agreed contract and compatible shape.
**When** the serializer reads it after runtime internals are refactored.
**Then** the caller receives the agreed typed message with its represented values.

### CORE-002 — Registry uniqueness and selection
**Classification:** Observed behaviour
**Evidence:** Registry construction and lookups; reference/contracts.md is descriptive source evidence, not independent approval.

Pre: registrations supplied before registry resolution. Post: one name per type, one type per name, and globally unique consumer IDs in the supplied registries; lookup uses exact type and ordinal identity. Failures occur at registry resolution rather than durable admission.

#### CORE-002-AC-01 — Duplicate contract type or name
**Given** two entries share a CLR type or contract name, including an identical repeated entry.
**When** the contract registry is resolved.
**Then** construction fails rather than silently replacing either association.

#### CORE-002-AC-02 — Duplicate consumer identity
**Given** two consumers share an identity even across different message types.
**When** the consumer registry is resolved.
**Then** construction fails rather than choosing one consumer.

#### CORE-002-AC-03 — Exact message type
**Given** a consumer registered only for a base message type.
**When** consumers for a derived concrete type are requested.
**Then** lookup faults with `consumer.message_unhandled` rather than inferring base-type fan-out.

#### CORE-002-AC-04 — Ordinal names
**Given** a registered contract or consumer identity differs from a requested identity only in case.
**When** that requested identity is resolved.
**Then** it is not treated as the registered identity.

#### CORE-002-AC-05 — Complete consumer enumeration
**Given** several distinct consumer registrations exist.
**When** All and exact-type Get are examined.
**Then** All exposes each registration and Get exposes the registrations of that exact type without a promised enumeration order.

### CORE-003 — Lookup and dispatch failures
**Classification:** Observed behaviour
**Evidence:** Registry lookups and typed invoker branches; public MessageException code/action fields.

Pre: a consumer ID and retained MessageData. Post: lookup/deserialization succeeds before invoking the selected service. Failure codes classify known contract mismatches as permanent; service/handler errors are not uniformly wrapped.

#### CORE-003-AC-01 — Missing contract
**Given** an unregistered CLR type or nonblank persisted contract name.
**When** its mapping is requested.
**Then** a Fault MessageException reports `contract.type.not_registered` or `contract.identifier.not_registered`, respectively.

#### CORE-003-AC-02 — Missing consumer
**Given** a nonregistered consumer ID.
**When** dispatch is requested.
**Then** a Fault MessageException reports `consumer.not_registered` and no consumer handles the payload.

#### CORE-003-AC-03 — Incompatible consumer message
**Given** a selected consumer expects one type but receives an incompatible nonnull object.
**When** the invoker invokes it.
**Then** a Fault MessageException reports `consumer.message_type_mismatch` and the consumer is not called.

#### CORE-003-AC-04 — Handler completion and failure
**Given** a registered typed consumer that completes asynchronously or throws a known exception.
**When** its message is dispatched.
**Then** dispatch awaits completion or exposes the handler exception instead of returning a successful default.

#### CORE-003-AC-05 — Invocation token
**Given** a consumer observing its supplied CancellationToken.
**When** the dispatcher invokes it with a caller token.
**Then** the consumer observes that token; dispatch adds no independent timeout.

### CORE-004 — Durable JSON representation
**Classification:** Observed behaviour
**Evidence:** MessageSerializer resolves concrete runtime type and uses Web JSON defaults or DI JsonSerializerOptions.

Pre: nonnull registered concrete message. Post: MessageData.Type is its contract and Value is JSON. A configured durable serializer does not configure transport serialization.

#### CORE-004-AC-01 — Concrete runtime serialization
**Given** an object held in a base-typed variable whose concrete type is registered.
**When** it is serialized.
**Then** the retained contract and JSON correspond to the concrete registered type.

#### CORE-004-AC-02 — Default web JSON
**Given** a registered message with a PascalCase property.
**When** it is serialized using default options.
**Then** its ordinary property name uses web camel-case and compatible deserialization restores its value.

#### CORE-004-AC-03 — Configured JSON options
**Given** DI supplies custom JsonSerializerOptions, including a converter or naming policy.
**When** a supported message is serialized and deserialized.
**Then** that durable serializer applies the supplied options rather than independently replacing them.

#### CORE-004-AC-04 — Invalid JSON and null result
**Given** a known contract paired with malformed JSON, unsupported JSON conversion, or JSON `null`.
**When** deserialization is requested.
**Then** a Fault MessageException reports `serialization.deserialize.failed` and no message is returned.

#### CORE-004-AC-05 — Serialization failure
**Given** a registered object whose JSON serialization throws JsonException or NotSupportedException.
**When** serialization is requested.
**Then** a Fault MessageException reports `serialization.serialize.failed` and preserves the original exception as diagnostic cause.

#### CORE-004-AC-06 — Null input
**Given** a null object.
**When** Serialize is called.
**Then** ArgumentNullException is raised before a successful MessageData is produced.

### CORE-005 — Direct public contract input validation
**Classification:** Unknown behaviour
**Evidence:** Public MessageContract and custom IConsumerInvoker values can bypass AddContract/AddConsumer checks; malformed-data behavior is not uniformly guarded.

Unknown: whether public registration records and provider-created invokers must independently enforce the extension APIs' constraints. Current validation bypass must not become an approved alternate contract.

**Open decisions:** [GAP-CORE-001](GAPS.md#gap-core-001).

## Traceability and verification matrix
All rows are **Not tested**. Unit means isolated observable contract verification, not internal-call assertions. Persistence/concurrency verification requires a suitable real relational provider, not EF InMemory. The eventual TUnit suite can combine criteria while retaining their IDs.

| Specification | Criterion | Classification | Verification | Observable assertion | Status |
| --- | --- | --- | --- | --- | --- |
| CORE-001 | CORE-001-AC-01 | Required behaviour | Unit | Registered association | Not tested |
| CORE-001 | CORE-001-AC-02 | Required behaviour | Unit | Retained compatible message | Not tested |
| CORE-002 | CORE-002-AC-01 | Observed behaviour | Unit | Duplicate contract type or name | Not tested |
| CORE-002 | CORE-002-AC-02 | Observed behaviour | Unit | Duplicate consumer identity | Not tested |
| CORE-002 | CORE-002-AC-03 | Observed behaviour | Unit | Exact message type | Not tested |
| CORE-002 | CORE-002-AC-04 | Observed behaviour | Unit | Ordinal names | Not tested |
| CORE-002 | CORE-002-AC-05 | Observed behaviour | Unit | Complete consumer enumeration | Not tested |
| CORE-003 | CORE-003-AC-01 | Observed behaviour | Unit | Missing contract | Not tested |
| CORE-003 | CORE-003-AC-02 | Observed behaviour | Unit | Missing consumer | Not tested |
| CORE-003 | CORE-003-AC-03 | Observed behaviour | Unit | Incompatible consumer message | Not tested |
| CORE-003 | CORE-003-AC-04 | Observed behaviour | Unit | Handler completion and failure | Not tested |
| CORE-003 | CORE-003-AC-05 | Observed behaviour | Unit | Invocation token | Not tested |
| CORE-004 | CORE-004-AC-01 | Observed behaviour | Unit | Concrete runtime serialization | Not tested |
| CORE-004 | CORE-004-AC-02 | Observed behaviour | Unit | Default web JSON | Not tested |
| CORE-004 | CORE-004-AC-03 | Observed behaviour | Unit | Configured JSON options | Not tested |
| CORE-004 | CORE-004-AC-04 | Observed behaviour | Unit | Invalid JSON and null result | Not tested |
| CORE-004 | CORE-004-AC-05 | Observed behaviour | Unit | Serialization failure | Not tested |
| CORE-004 | CORE-004-AC-06 | Observed behaviour | Unit | Null input | Not tested |

## Implementation trace
These locations are evidence, not mandated internal architecture. The full inspected-file/public-surface matrix is in [INSPECTION.md](INSPECTION.md).

- [src/Clean.Messaging/Contracts/MessageContractRegistry.cs](../../src/Clean.Messaging/Contracts/MessageContractRegistry.cs)
- [src/Clean.Messaging/Consumers/ConsumerRegistry.cs](../../src/Clean.Messaging/Consumers/ConsumerRegistry.cs)
- [src/Clean.Messaging/Serialization/MessageSerializer.cs](../../src/Clean.Messaging/Serialization/MessageSerializer.cs)
- [src/Clean.Messaging/Processing/ConsumerDispatcher.cs](../../src/Clean.Messaging/Processing/ConsumerDispatcher.cs)
- [src/Clean.Messaging.Abstractions/IMessageConsumer.cs](../../src/Clean.Messaging.Abstractions/IMessageConsumer.cs)

## Open decisions and evidence limits
The [gap register](GAPS.md) records unresolved input domains, provider evidence and proposed changes. Source inspection is not execution proof. [INDEX.md](INDEX.md) explains classification authority and cross-capability ownership.
