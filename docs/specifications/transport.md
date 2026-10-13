# Transport mapping — Behavioural specification

**Status:** Draft — 2026-10-13. **Validation:** Not tested.
**Scope:** `Clean.Messaging.Transport`, `Clean.Messaging.Inbox.Ingress`, and `Clean.Messaging.Outbox.Dispatch`; durable inbox/outbox dependencies only at their admission and external-completion boundaries. MQTT-specific contracts are in [MQTT](mqtt.md).

## Intent and evidence policy

Provide a provider-neutral envelope and typed mapping boundary so hosts can admit incoming representations into durable consumer work and turn durable outgoing work into publishable representations without merging logical identity, delivery identity, or persistence with transport effects.

Required behaviour below has an explicit normative basis in [repository instructions](../../.github/copilot-instructions.md) or [coding standards](../CODING_STANDARDS.md); a source-derived [transport reference](../reference/transport.md) does not approve implementation as a requirement. Observed behaviour records inspected code, not agreed intent. Unknown behaviour identifies a pending decision, not an executable requirement. Acceptance criteria inherit their block's classification unless explicitly overridden. **Neither Required nor Observed means tested.** No production code or tests were changed or run for this draft.

Evidence includes the full authored file set in the three scoped projects, [architecture](../reference/architecture.md), [contracts](../reference/contracts.md), [configuration](../reference/configuration.md), [delivery semantics](../explanation/delivery-semantics.md), and [identity](../explanation/message-identity.md). The companion audit manifest enumerates each authored source/project file; source links below trace behaviour rather than asserting validation.

## Contract and boundaries

- **Inputs/preconditions:** registered exact ingress or dispatch type pair; a serializer and mapper; durable inbox services for admission; runtime contract registry and durable serializer for dispatch. A transport envelope contains payload bytes and a payload contract name, not persisted `MessageData`.
- **Postconditions:** ingress success means its awaited inbox admission succeeded, not consumer completion. Dispatch creation returns an envelope, not a network publication or durable settlement.
- **Identity:** nullable transport IDs are representational, not permission for anonymous durable admission. Ingress requires nonempty delivery identity; optional logical identity defaults in inbox. Dispatch copies stored delivery/event/context IDs.
- **Compatibility:** CLR map keys, serialized shapes, contract names, IDs and metadata are distinct boundaries. Generic ingress selects by supplied `IngressMap`, not envelope contract text. Dispatch resolves the outgoing payload type's registered name, not the original message's name.
- **State/transactions:** transport mapping owns no database transaction or broker acknowledgement. Built-in EF inbox admission uses an independent scope and rejects ambient transactions. External outbox publication precedes fenced database completion in separate operations.
- **Failure:** generic ingress uses ordinary `InboxIngressException` for mapping faults; durable admission faults propagate separately. Dispatch mapping faults are permanent `MessageException` failures with `outbox.dispatch.failed`. There is no mapping retry loop.
- **Concurrency/lifecycle:** ingress and dispatch registries/services and mappers are scoped; serializer is shared. Mapper thread safety and arbitrary consumer/provider implementations are not inferred from lifetimes. Each mapping call executes anew; no mapping result cache is specified.
- **Limits/non-guarantees:** no provider dependencies in transport models, global ordering, exactly-once effects, schema negotiation, content-type negotiation, envelope serializer, or automatic durable storage registration. No built-in payload-size/header-count limit is imposed by these projects. Broker and database support remain host/provider concerns.

## Specifications and acceptance criteria

### TRANSPORT-001 — Preserve representation and identity boundaries

**Classification:** Required behaviour
**Normative basis:** coding standards §§3, 9 (contracts and identity), 14; repository instructions require provider separation and preservation of message/event identity, contract names and transaction boundaries.
**Source evidence:** [TransportMessage](../../src/Clean.Messaging.Transport/TransportMessage.cs), [TransportHeader](../../src/Clean.Messaging.Transport/TransportHeader.cs), [TransportMapper](../../src/Clean.Messaging.Transport/TransportMapper.cs), [InboxMessageFactory](../../src/Clean.Messaging.Inbox.Ingress/InboxMessageFactory.cs), [TransportMessageFactory](../../src/Clean.Messaging.Outbox.Dispatch/TransportMessageFactory.cs).

#### TRANSPORT-001-AC-01 — Distinct delivery and logical identities survive mapping
**Given** valid incoming or stored outgoing work with distinct nonempty message/event IDs and explicit correlation/causation IDs, **When** it crosses the typed mapping boundary, **Then** the resulting inbox admission or transport envelope retains each ID in its original role, rather than substituting the payload's ID or a fresh ID.

#### TRANSPORT-001-AC-02 — Retry does not create a new logical operation
**Given** a stored outgoing target whose previous publication outcome is uncertain, **When** it is mapped for another attempt with unchanged retained data and configuration, **Then** the published envelope retains the stored message/event IDs; an attempt identity does not replace either ID.

#### TRANSPORT-001-AC-03 — Consumer contracts remain provider-neutral
**Given** a consumer implementing the library consumer contract and a mapper implementing `ITransportMapper<TSource, TDestination>`, **When** the consumer and mapping contracts are inspected/consumed without MQTTnet or EF APIs, **Then** their signatures require neither broker packet nor ORM transaction types; provider-specific mechanics remain at integration boundaries.

### TRANSPORT-002 — Envelope and payload serialization surface

**Classification:** Observed behaviour
**Evidence:** all four `.cs` files and the [project](../../src/Clean.Messaging.Transport/Clean.Messaging.Transport.csproj) in Transport; especially [serializer](../../src/Clean.Messaging.Transport/TransportSerializer.cs). Source-only observation; no compatibility certification.

`TransportMessage` is a sealed record with required `Contract` and `Payload`, optional `MessageId`, `EventId`, `CorrelationId`, `CausationId`, `ContentType`, and default empty `Headers`. `TransportHeader(Name, Value)` permits repeated names. Required members impose compile-time initialization, not runtime nonblank/content validation. `Map(source)` is synchronous and receives neither envelope nor cancellation. Payload serializer methods constrain message types to `notnull`.

#### TRANSPORT-002-AC-01 — Web JSON payload round trip
**Given** a supported payload type with ordinary public properties and the built-in `TransportSerializers.Json`, **When** the payload is serialized and deserialized as that type, **Then** UTF-8 web-default JSON preserves its values and the serialized bytes contain payload data rather than an added transport envelope.

#### TRANSPORT-002-AC-02 — Invalid or null JSON is not a successful payload
**Given** malformed JSON, an unsupported JSON shape/type, or JSON `null` for a nullable reference deserialization result, **When** built-in payload deserialization runs, **Then** it throws rather than returning an accepted default message; a null result produces `InvalidOperationException` before any durable admission.

#### TRANSPORT-002-AC-03 — Header multiplicity and absent metadata remain representable
**Given** an envelope with two headers of the same name, omitted optional IDs/content type, and explicitly initialized payload/contract, **When** its members are read, **Then** both headers remain present and optional values remain absent; the record performs no deduplication, contract registration check, or identity generation.

### TRANSPORT-003 — Registration and exact mapping resolution

**Classification:** Observed behaviour
**Evidence:** ingress [registration](../../src/Clean.Messaging.Inbox.Ingress/DependencyInjection.cs), [registry](../../src/Clean.Messaging.Inbox.Ingress/TransportIngressRegistry.cs), dispatch [registration](../../src/Clean.Messaging.Outbox.Dispatch/DependencyInjection.cs), [registry](../../src/Clean.Messaging.Outbox.Dispatch/TransportDispatchRegistry.cs), identity mappers in both projects.

`AddIngress()` installs shared default transport serializer, singleton inbox factory, scoped registry and `IInboxIngress`. `AddDispatch()` installs shared default serializer, scoped factory/registry and `IOutboxDispatch`. Generic single-type overloads register identity mapping; three-type overloads use scoped concrete mappers and typed map registrations. `TryAdd` preserves earlier compatible registrations. Identical typed implementation registrations are suppressed by `TryAddEnumerable`; distinct mapper implementations for the same pair conflict when the registry resolves. Neither API installs persistence, consumer contracts, hosted processing or broker connections.

#### TRANSPORT-003-AC-01 — Custom serializer and mapper registration are retained
**Given** an earlier custom `ITransportSerializer` and concrete mapper registration, **When** ingress/dispatch registration is added and its mapping service is resolved, **Then** mapping uses the earlier services rather than replacing them with built-in JSON or a new mapper registration.

#### TRANSPORT-003-AC-02 — Exact type pair chooses mapping
**Given** mappings A-to-B and A-to-C, but none for a subclass of A to B, **When** each registered exact pair is requested and then the unregistered pair is requested, **Then** registered requests use their respective mapper and the unregistered pair fails; variance in the mapper interface does not create registry fallback.

#### TRANSPORT-003-AC-03 — Duplicate pair has no precedence winner
**Given** two distinct mapper implementations registered for one ingress pair or one dispatch pair, **When** the corresponding registry is resolved, **Then** resolution fails rather than choosing first/last; dispatch reports `OutboxDispatchException` and ingress dictionary construction reports a configuration exception. Repeating the same typed registration alone does not create this conflict.

#### TRANSPORT-003-AC-04 — Invalid composition is not durable readiness
**Given** a null service collection, **When** any `AddIngress` or `AddDispatch` overload is invoked, **Then** argument validation fails. **Given** only valid mapping registrations without durable capability/runtime dependencies, **When** admission or dispatch services requiring those dependencies are resolved/used, **Then** registration alone supplies no successful durable admission, consumer processing or publication.

### TRANSPORT-004 — Incoming typed admission and metadata loss boundary

**Classification:** Observed behaviour
**Evidence:** [IInboxIngress / InboxIngress](../../src/Clean.Messaging.Inbox.Ingress/InboxIngress.cs), [IngressMap](../../src/Clean.Messaging.Inbox.Ingress/IngressMap.cs), [TransportIngress](../../src/Clean.Messaging.Inbox.Ingress/TransportIngress.cs), [InboxMessageFactory](../../src/Clean.Messaging.Inbox.Ingress/InboxMessageFactory.cs), dependency [Inbox](../../src/Clean.Messaging.Inbox/Inbox.cs).

#### TRANSPORT-004-AC-01 — Typed mapping admits the mapped consumer message
**Given** an A-to-B ingress map, valid A payload bytes, valid delivery identity and configured durable B consumers, **When** `Accept(map, envelope, token)` succeeds, **Then** durable admission contains mapped B message data and the supplied IDs/context; A's transport contract and B's durable contract can differ.

#### TRANSPORT-004-AC-02 — Generic ingress trusts the selected pair rather than contract text
**Given** valid A bytes and an A-to-B map but unrelated envelope `Contract` text, **When** generic ingress admission runs without a provider-specific contract check, **Then** it deserializes A and admits B according to the map rather than rejecting solely for the text mismatch.

#### TRANSPORT-004-AC-03 — Anonymous or empty identity is rejected
**Given** valid mapped payload but absent or empty `MessageId`, **When** ingress accepts it, **Then** it throws `InboxIngressException` and performs no inbox admission or replacement-ID generation. **Given** a valid message ID and absent event ID, **When** admission succeeds, **Then** durable event identity equals message identity; an explicitly empty event ID instead fails in inbox validation.

#### TRANSPORT-004-AC-04 — Transport-only metadata does not become consumer data
**Given** a transport envelope with Content Type and arbitrary headers, **When** the built-in ingress mapping admits its payload, **Then** the mapper receives the payload only, and durable inbox data/context do not persist those headers or Content Type as additional envelope fields.

#### TRANSPORT-004-AC-05 — Null envelope or missing map is not accepted
**Given** a null envelope or an unregistered exact ingress map, **When** `IInboxIngress.Accept` is called, **Then** the former fails with argument-null validation and the latter with `InboxIngressException`; no durable admission occurs.

### TRANSPORT-005 — Admission failures, duplicates and cancellation

**Classification:** Observed behaviour
**Evidence:** [TransportIngress](../../src/Clean.Messaging.Inbox.Ingress/TransportIngress.cs), [InboxIngressException](../../src/Clean.Messaging.Inbox.Ingress/InboxIngressException.cs), dependency [InboxStore](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs), [inbox mappings](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxConfiguration.cs), [Inbox](../../src/Clean.Messaging.Inbox/Inbox.cs).

The EF observations here are conditional on correct relational configuration, retained admissions and registered consumer contracts. Generic ingress itself implements neither duplicate detection nor acknowledgement.

#### TRANSPORT-005-AC-01 — Mapping and persistence failures remain distinguishable
**Given** a serializer/mapper that throws before inbox invocation, **When** ingress accepts the envelope, **Then** it exposes `InboxIngressException` preserving the original exception as its inner cause (or preserves an existing ingress exception). **Given** successful mapping followed by an inbox admission failure, **When** admission fails, **Then** that failure escapes without ingress wrapping.

#### TRANSPORT-005-AC-02 — Concurrent retained duplicate admissions create one obligation set
**Given** valid identical envelopes, registered consumers and the relational inbox schema, **When** two concurrent admissions compete using the same message/event identity, **Then** successful admissions leave one admission and no duplicate event/consumer entries; completed retained consumer work is not reset by the duplicate.

#### TRANSPORT-005-AC-03 — Identity conflict does not overwrite retained data
**Given** a retained admission and a newly mapped envelope sharing either message ID or event ID but carrying different payload/context, **When** admission reaches the built-in EF store, **Then** it returns as already accepted without replacing the stored admission or comparing payload equivalence. This is an observed conflict policy, not approval for identity reuse for distinct operations.

#### TRANSPORT-005-AC-04 — Admission I/O cancellation remains cancellation
**Given** successful mapping and an admission operation blocked on cancellable I/O, **When** its supplied cancellation token is cancelled, **Then** inbox I/O receives that token and the cancellation exception propagates without ordinary ingress-failure wrapping; no successful admission result is manufactured. A commit that already happened is not undone by later cancellation.

#### TRANSPORT-005-AC-05 — Synchronous mapper cancellation currently becomes a mapping fault
**Given** a mapper or payload serializer throwing `OperationCanceledException` before inbox invocation, **When** ingress runs even with its token cancelled, **Then** generic ingress wraps that exception as `InboxIngressException`, since the synchronous mapper has no token contract. This discrepancy with normative cancellation guidance is pending [GAP-TX-002](GAPS.md#gap-tx-002), not an approved cancellation policy.

#### TRANSPORT-005-AC-06 — Independent admission rejects ambient transactions
**Given** valid ingress input and an ambient transaction in the caller, **When** the EF inbox admission path is invoked, **Then** admission fails rather than joining that transaction. **Given** no ambient transaction and committed admission, **When** unrelated caller work later fails, **Then** that caller failure does not by itself erase admitted work.

### TRANSPORT-006 — Outgoing payload construction and permanent mapping faults

**Classification:** Observed behaviour
**Evidence:** [IOutboxDispatch](../../src/Clean.Messaging.Outbox.Dispatch/IOutboxDispatch.cs), [DispatchMap](../../src/Clean.Messaging.Outbox.Dispatch/DispatchMap.cs), [TransportDispatch](../../src/Clean.Messaging.Outbox.Dispatch/TransportDispatch.cs), [factory](../../src/Clean.Messaging.Outbox.Dispatch/TransportMessageFactory.cs), [exception](../../src/Clean.Messaging.Outbox.Dispatch/OutboxDispatchException.cs), runtime [serializer](../../src/Clean.Messaging/Serialization/MessageSerializer.cs) and [registry](../../src/Clean.Messaging/Contracts/MessageContractRegistry.cs).

#### TRANSPORT-006-AC-01 — Stored source maps to registered outgoing contract
**Given** persisted A data, an A-to-B dispatch map and both source and B contracts registered, **When** `Create` succeeds, **Then** its payload is serialized mapped B data, `Contract` is B's registered name, stored IDs/context are copied, and Content Type is null with empty headers in the built-in factory.

#### TRANSPORT-006-AC-02 — Invalid persisted or mapped data is permanent failure
**Given** unknown stored contract, invalid stored JSON, an incompatible deserialized source, a throwing mapper, missing payload contract or failing transport serialization, **When** dispatch creates a transport message, **Then** it throws `OutboxDispatchException` with code `outbox.dispatch.failed`, action `Fault` and preserved diagnostic cause where wrapping occurred; no publishable successful result is returned.

#### TRANSPORT-006-AC-03 — Null envelope and missing dispatch map are distinct errors
**Given** a null envelope or absent exact dispatch map, **When** `Create` is called, **Then** the null envelope fails argument validation while the missing map reports permanent `OutboxDispatchException`; neither publishes nor completes work.

#### TRANSPORT-006-AC-04 — Durable JSON configuration is independent of transport JSON
**Given** custom durable JSON options and the unchanged default transport serializer, **When** stored A is deserialized and mapped B is serialized, **Then** durable options govern reading A while web-default transport JSON governs B. No automatic shared-options negotiation is performed.

### TRANSPORT-007 — External publication and per-target completion are separate

**Classification:** Required behaviour
**Normative basis:** coding standards §§8, 9 (inbox/outbox and claims); repository instructions preserve per-target completion, leases and transaction boundaries and prohibit exactly-once claims.
**Source evidence:** dependency [OutboxEnvelope](../../src/Clean.Messaging.Outbox/Transport/OutboxEnvelope.cs), [transport wrapper](../../src/Clean.Messaging.Outbox/Transport/OutboxTransport.cs), [dispatcher](../../src/Clean.Messaging.Outbox/Delivery/OutboxDispatcher.cs), [durable store](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs), [processor](../../src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs).

#### TRANSPORT-007-AC-01 — Failed publication does not record successful completion
**Given** a claimed outgoing target, **When** its external transport fails before returning success, **Then** successful completion is not recorded for that target; durable failure handling owns retry/fault classification separately from mapping and publication.

#### TRANSPORT-007-AC-02 — Publication success followed by lost settlement can repeat
**Given** a publication that succeeds but whose database completion does not persist, **When** recovery claims the unresolved retained target, **Then** another publication can occur with the same stored identities; the library does not report the first external effect as rolled back or exactly-once.

#### TRANSPORT-007-AC-03 — Stale ownership cannot complete another attempt
**Given** an old claim expired and a newer owner acquired the target, **When** the old external attempt returns and tries to settle, **Then** its completion is rejected by claim/lease checks and the new owner's durable state is not completed by that old claim, even though the old publication may already have occurred.

#### TRANSPORT-007-AC-04 — Sibling completion is independent
**Given** one completed target and one unresolved target in a retained outgoing batch, **When** the unresolved target retries or faults, **Then** that operation does not reset/reinvoke the completed sibling merely because the batch is not wholly successful; no atomic all-target external effect is asserted.

### TRANSPORT-008 — Unapproved compatibility and synchronous cancellation decisions

**Classification:** Unknown behaviour
**Evidence:** [transport serializer](../../src/Clean.Messaging.Transport/TransportSerializer.cs), both mapping implementations, and coding standards §§7, 9, 14. No approved schema migration or mapper-cancellation contract was found in scoped authored files.
**Pending decisions:** [GAP-TX-002](GAPS.md#gap-tx-002) and [GAP-TX-003](GAPS.md#gap-tx-003). These numbered questions are not executable acceptance criteria, requirements or pass/fail promises. An owner decision must select definite observable outcomes before criteria can be authored.

1. **Retained payload compatibility:** Which earlier payload schema/contract configurations must a later deployment support? Which changes require migration or a classified rejection, and who owns the strategy? No upcasting or compatibility outcome is selected here.
2. **Synchronous mapping cancellation:** If a synchronous serializer/mapper throws cancellation while the attempt token is cancelled, must the boundary propagate cancellation or classify a mapping fault before admission/publication? How should user-thrown cancellation with an uncancelled attempt token differ? No classification change is prescribed here.

## Invariants and limits

- Required: identities retain their roles across retries; per-target settlement preserves ownership; external publication is not atomically rolled back by database failure.
- Observed: incoming Content Type/headers stop at the intermediate representation; outgoing built-in factory does not recreate them. See [GAP-TX-004](GAPS.md#gap-tx-004) for any future propagation intent.
- Observed: ingress uses the supplied map independently of textual contract; dispatch uses durable contract lookup then outgoing payload type lookup. Type compatibility is not schema compatibility.
- No required universal thread-safety, payload limit, ordering, independent-provider support matrix or exactly-once guarantee follows from the project files or DI lifetimes.

## Verification matrix

All assertions are recommendations for future validation, not executed evidence. Relational guarantees require a configured real relational provider, not EF InMemory; external-effect scenarios require instrumented transports and persistent stores.

| Criterion | Recommended level | Observable assertion | Status |
| --- | --- | --- | --- |
| TRANSPORT-001-AC-01 | Persistence integration | IDs/context in admitted/staged envelopes equal supplied distinct values. | Not tested |
| TRANSPORT-001-AC-02 | Transport integration | Both publication captures retain stored message/event IDs. | Not tested |
| TRANSPORT-001-AC-03 | API contract review | Consumer/mapper API signatures contain no broker/ORM types. | Not tested |
| TRANSPORT-002-AC-01 | Unit | Decode bytes and round-trip property values with web JSON. | Not tested |
| TRANSPORT-002-AC-02 | Unit | Invalid/null payload throws with no result. | Not tested |
| TRANSPORT-002-AC-03 | Unit | Duplicate header entries and optional nulls survive envelope construction. | Not tested |
| TRANSPORT-003-AC-01 | Unit / DI integration | Resolved mapping uses earlier custom registrations. | Not tested |
| TRANSPORT-003-AC-02 | Unit / DI integration | Exact pairs produce distinct results; unregistered derived pair fails. | Not tested |
| TRANSPORT-003-AC-03 | Unit / DI integration | Distinct duplicate pair fails resolution; identical registration alone does not. | Not tested |
| TRANSPORT-003-AC-04 | Unit / DI integration | Null arguments fail and mapping-only composition supplies no durable result. | Not tested |
| TRANSPORT-004-AC-01 | Persistence integration | Admission contains mapped B data and expected contract/IDs. | Not tested |
| TRANSPORT-004-AC-02 | Unit | Unrelated envelope contract text alone does not reject the selected map. | Not tested |
| TRANSPORT-004-AC-03 | Unit / persistence integration | Missing/empty delivery IDs reject; absent event defaults; empty event rejects. | Not tested |
| TRANSPORT-004-AC-04 | Persistence integration | Stored data/context have no copied arbitrary envelope metadata. | Not tested |
| TRANSPORT-004-AC-05 | Unit | Null/missing map failures occur without admission. | Not tested |
| TRANSPORT-005-AC-01 | Unit | Inner mapping cause preserved; admission exception not wrapped. | Not tested |
| TRANSPORT-005-AC-02 | Concurrency integration | Racing admissions leave one admission and unique event/consumer obligations. | Not tested |
| TRANSPORT-005-AC-03 | Persistence integration | Identity-conflicting repeat leaves original payload/context unchanged. | Not tested |
| TRANSPORT-005-AC-04 | Persistence integration | Cancel blocked admission; no manufactured success; inspect commit boundary. | Not tested |
| TRANSPORT-005-AC-05 | Unit | Mapper cancellation is currently an inner cause of ingress failure. | Not tested |
| TRANSPORT-005-AC-06 | Persistence integration | Ambient admission fails; independent committed admission survives caller failure. | Not tested |
| TRANSPORT-006-AC-01 | Unit | Outgoing bytes/contract/metadata describe B and stored IDs. | Not tested |
| TRANSPORT-006-AC-02 | Unit | Each invalid-source/map/serialization case yields permanent dispatch failure. | Not tested |
| TRANSPORT-006-AC-03 | Unit | Null argument and missing-map exceptions remain distinct. | Not tested |
| TRANSPORT-006-AC-04 | Unit | Durable option change does not change default transport byte shape. | Not tested |
| TRANSPORT-007-AC-01 | Transport + persistence integration | Failed transport attempt leaves successful completion absent. | Not tested |
| TRANSPORT-007-AC-02 | Transport + persistence integration | Inject post-publish settlement failure; reclaim produces repeat with same IDs. | Not tested |
| TRANSPORT-007-AC-03 | Concurrency integration | Old claim settlement returns false and new owner state remains unchanged. | Not tested |
| TRANSPORT-007-AC-04 | Persistence integration | Retry/fault changes unresolved target only. | Not tested |

## Public-contract and source traceability

Public members are grouped by consumer-facing type/operation, not one block per property. The audit manifest also maps every internal entry point, project file and friend-assembly declaration to these IDs.

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

Source paths in this table omit `src/Clean.Messaging.` for compactness; exact Windows repository-relative paths are in the audit manifest. Transport has no provider/project references; ingress references Inbox and Transport; dispatch references Outbox, Transport and runtime and DI abstractions 10.0.11. All target net10.0 with implicit usings/nullability; these are inspected compatibility surfaces, not tested framework/provider support. Dispatch's friend assembly is `Clean.Messaging.Outbox.Dispatch.Mqtt.Tests`, not evidence of executed tests.

## Open decisions and discrepancies

- [GAP-TX-002](GAPS.md#gap-tx-002): synchronous mapping cancellation is wrapped and can become MQTT poison, unlike cancellation after inbox invocation. Candidate policy remains pending.
- [GAP-TX-003](GAPS.md#gap-tx-003): no negotiated schema/version/migration policy for retained work or independent peers was found; host compatibility ownership does not specify which changes are supported.
- [GAP-TX-004](GAPS.md#gap-tx-004): metadata loss at generic ingress and empty built-in outgoing headers/content type are observed, not evidence that full round-trip propagation is intended.
- [GAP-TX-008](GAPS.md#gap-tx-008): mapping conflicts resolve at registry construction, not an explicit mapping startup validator; cross-registration validation intent needs decision under coding standards §6.
- Unknown blocks contain decision questions only; they are excluded from executable acceptance-criterion totals and the verification matrix. Source inspection alone cannot resolve these gaps or change their classification.
