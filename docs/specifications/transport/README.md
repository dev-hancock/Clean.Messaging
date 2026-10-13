# Transport mapping — Behavioural specification

**Scope:** `Clean.Messaging.Transport`, `Clean.Messaging.Inbox.Ingress`, and `Clean.Messaging.Outbox.Dispatch`; durable inbox/outbox dependencies only at their admission and external-completion boundaries. MQTT-specific contracts are in [MQTT](../mqtt/README.md).

## Purpose and authority

Provide a provider-neutral envelope and typed mapping boundary so hosts can admit incoming representations into durable consumer work and turn durable outgoing work into publishable representations without merging logical identity, delivery identity, or persistence with transport effects.

Required behaviour below has an explicit normative basis in [repository instructions](../../../.github/copilot-instructions.md) or [coding standards](../../CODING_STANDARDS.md); a source-derived [transport reference](../../reference/transport.md) does not approve implementation as a requirement. Observed behaviour records inspected code, not agreed intent. Unknown behaviour identifies a pending decision, not an executable requirement. Acceptance criteria inherit their block's classification unless explicitly overridden.

Evidence includes the full authored file set in the three scoped projects, [architecture](../../reference/architecture.md), [contracts](../../reference/contracts.md), [configuration](../../reference/configuration.md), [delivery semantics](../../explanation/delivery-semantics.md), and [identity](../../explanation/message-identity.md).

## Contract and boundaries

- **Inputs/preconditions:** registered exact ingress or dispatch type pair; a serializer and mapper; durable inbox services for admission; runtime contract registry and durable serializer for dispatch. A transport envelope contains payload bytes and a payload contract name, not persisted `MessageData`.
- **Postconditions:** ingress success means its awaited inbox admission succeeded, not consumer completion. Dispatch creation returns an envelope, not a network publication or durable settlement.
- **Identity:** nullable transport IDs are representational, not permission for anonymous durable admission. Ingress requires nonempty delivery identity; optional logical identity defaults in inbox. Dispatch copies stored delivery/event/context IDs.
- **Compatibility:** CLR map keys, serialized shapes, contract names, IDs and metadata are distinct boundaries. Generic ingress selects by supplied `IngressMap`, not envelope contract text. Dispatch resolves the outgoing payload type's registered name, not the original message's name.
- **State/transactions:** transport mapping owns no database transaction or broker acknowledgement. Built-in EF inbox admission uses an independent scope and rejects ambient transactions. External outbox publication precedes fenced database completion in separate operations.
- **Failure:** generic ingress uses ordinary `InboxIngressException` for mapping faults; durable admission faults propagate separately. Dispatch mapping faults are permanent `MessageException` failures with `outbox.dispatch.failed`. There is no mapping retry loop.
- **Concurrency/lifecycle:** ingress and dispatch registries/services and mappers are scoped; serializer is shared. Mapper thread safety and arbitrary consumer/provider implementations are not inferred from lifetimes. Each mapping call executes anew; no mapping result cache is specified.
- **Limits/non-guarantees:** no provider dependencies in transport models, global ordering, exactly-once effects, schema negotiation, content-type negotiation, envelope serializer, or automatic durable storage registration. No built-in payload-size/header-count limit is imposed by these projects. Broker and database support remain host/provider concerns.

## Invariants and limits

- Required: identities retain their roles across retries; per-target settlement preserves ownership; external publication is not atomically rolled back by database failure.
- Observed: incoming Content Type/headers stop at the intermediate representation; outgoing built-in factory does not recreate them. See [GAP-TX-004](../GAPS.md#gap-tx-004) for any future propagation intent.
- Observed: ingress uses the supplied map independently of textual contract; dispatch uses durable contract lookup then outgoing payload type lookup. Type compatibility is not schema compatibility.
- No required universal thread-safety, payload limit, ordering, independent-provider support matrix or exactly-once guarantee follows from the project files or DI lifetimes.

## Public contract references

See the [package contract and source map](../../reference/contract-map.md) for public declarations, supporting sources and their specification IDs. The contracts and observable boundaries are defined here and in the numbered specifications.

The grouped adapter and dependency surfaces below complement the per-source map.

| Public surface | Owned source | Specifications |
| --- | --- | --- |
| `TransportMessage` required Contract/Payload; optional MessageId/EventId/CorrelationId/CausationId/ContentType; Headers init/default | Transport/TransportMessage.cs | TRANSPORT-001, 002, 004, 006 |
| `TransportHeader(Name, Value)` record constructor/properties | Transport/TransportHeader.cs | TRANSPORT-002 |
| `ITransportMapper<in TSource, out TDestination>.Map` | Transport/TransportMapper.cs | TRANSPORT-001, 002, 003, 008 |
| `ITransportSerializer.Serialize<TMessage>`, `Deserialize<TMessage>`; `TransportSerializers.Json` | Transport/TransportSerializer.cs | TRANSPORT-002, 003, 006, 008 |
| `AddIngress()`, `AddIngress<TMessage>()`, `AddIngress<TPayload,TMessage,TMapper>()` | Inbox.Ingress/DependencyInjection.cs | TRANSPORT-003 |
| `IInboxIngress.Accept(map,message,token=default)` | Inbox.Ingress/InboxIngress.cs | TRANSPORT-004, 005 |
| `IngressMap(PayloadType, MessageType)` record constructor/properties | Inbox.Ingress/IngressMap.cs | TRANSPORT-003, 004 |
| `InboxIngressException(message)` and `(message,innerException)` | Inbox.Ingress/InboxIngressException.cs | TRANSPORT-005 |
| `AddDispatch()`, `AddDispatch<TMessage>()`, `AddDispatch<TMessage,TPayload,TMapper>()` | Outbox.Dispatch/DependencyInjection.cs | TRANSPORT-003 |
| `IOutboxDispatch.Create(map,envelope)` | Outbox.Dispatch/IOutboxDispatch.cs | TRANSPORT-006 |
| `DispatchMap(MessageType, PayloadType)` record constructor/properties | Outbox.Dispatch/DispatchMap.cs | TRANSPORT-003, 006 |
| `OutboxDispatchException(message,innerException=null)` and inherited Code/Action/IsTransient | Outbox.Dispatch/OutboxDispatchException.cs | TRANSPORT-006 |
| Dependency `IInbox.Accept`, InboxMessage IDs/context; relational admission/store/schema | Inbox and Inbox.EntityFrameworkCore (outside owned projects) | TRANSPORT-004, 005 |
| Dependency `OutboxEnvelope`, `IOutboxTransport.Name/Dispatch`, `AddOutboxTransport<TTransport>`, external wrapper/store/processor | Outbox and EntityFrameworkCore (outside owned projects) | TRANSPORT-007 |

Source paths in this table omit `src/Clean.Messaging.` for compactness. Transport has no provider/project references; ingress references Inbox and Transport; dispatch references Outbox, Transport and runtime and DI abstractions 10.0.11. All target net10.0 with implicit usings/nullability; these describe dependencies, not guaranteed framework/provider support. Dispatch's friend assembly is `Clean.Messaging.Outbox.Dispatch.Mqtt.Tests`, not an execution contract.

## Open decisions and discrepancies

- [GAP-TX-002](../GAPS.md#gap-tx-002): synchronous mapping cancellation is wrapped and can become MQTT poison, unlike cancellation after inbox invocation. Candidate policy remains pending.
- [GAP-TX-003](../GAPS.md#gap-tx-003): no negotiated schema/version/migration policy for retained work or independent peers was found; host compatibility ownership does not specify which changes are supported.
- [GAP-TX-004](../GAPS.md#gap-tx-004): metadata loss at generic ingress and empty built-in outgoing headers/content type are observed, not evidence that full round-trip propagation is intended.
- [GAP-TX-008](../GAPS.md#gap-tx-008): mapping conflicts resolve at registry construction, not an explicit mapping startup validator; cross-registration validation intent needs decision under coding standards §6.
- Unknown blocks contain decision questions only; they are not unconditional executable acceptance criteria. Source inspection alone cannot resolve these gaps or change their classification.

## Specifications

- [TRANSPORT-001 — Preserve representation and identity boundaries](TRANSPORT-001-preserve-representation-and-identity-boundaries.md)
- [TRANSPORT-002 — Envelope and payload serialization surface](TRANSPORT-002-envelope-and-payload-serialization-surface.md)
- [TRANSPORT-003 — Registration and exact mapping resolution](TRANSPORT-003-registration-and-exact-mapping-resolution.md)
- [TRANSPORT-004 — Incoming typed admission and metadata loss boundary](TRANSPORT-004-incoming-typed-admission-and-metadata-loss-boundary.md)
- [TRANSPORT-005 — Admission failures, duplicates and cancellation](TRANSPORT-005-admission-failures-duplicates-and-cancellation.md)
- [TRANSPORT-006 — Outgoing payload construction and permanent mapping faults](TRANSPORT-006-outgoing-payload-construction-and-permanent-mapping-faults.md)
- [TRANSPORT-007 — External publication and per-target completion are separate](TRANSPORT-007-external-publication-and-per-target-completion-are-separate.md)
- [TRANSPORT-008 — Unapproved compatibility and synchronous cancellation decisions](TRANSPORT-008-unapproved-compatibility-and-synchronous-cancellation-decisions.md)
