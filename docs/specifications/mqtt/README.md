# MQTT ingress and dispatch — Behavioural specification

**Scope:** `Clean.Messaging.Inbox.Ingress.Mqtt` and `Clean.Messaging.Outbox.Dispatch.Mqtt`, with their provider-neutral mapping and durable admission/completion dependencies. Shared mapping contracts are specified in [Transport](../transport/README.md).

## Purpose and authority

Hosts need MQTT subscription routing into durable inbox admission and per-target outgoing MQTT publication driven by durable outbox work. The adapter owns protocol routing, envelope metadata, connection and acknowledgement/publication, not consumer business policy, schema migrations, broker provisioning or external-effect idempotency.

Required blocks cite normative [repository instructions](../../../.github/copilot-instructions.md), [coding standards](../../CODING_STANDARDS.md), or explicit public XML contract text. Observed blocks describe source inspection, not approved intent. Unknown blocks contain **numbered questions explicitly pending decision**, not executable acceptance criteria or hidden requirements. Criteria inherit their parent block's classification unless explicitly overridden.

## Contract and boundaries

- **Incoming preconditions:** registered payload/message contracts, a completed non-overlapping subscription route, valid options, composed durable inbox/store and consumers, available broker/database. MQTT 5 user properties provide application identity; MQTT packet identity does not.
- **Outgoing preconditions:** staged target with transport `mqtt`, a stored destination, resolvable current route, source/payload contracts, composed durable outbox/store/workers and broker access.
- **Incoming postcondition:** successful admission precedes acknowledgement; consumers complete later. Poison follows the explicit acknowledge-and-log policy without a durable poison record. Ordinary admission failure does not become successful acknowledgement.
- **Outgoing postcondition:** successful `PublishAsync` result returns to the generic external wrapper for conditional database completion; it does not assert subscriber consumption.
- **State/atomicity:** destination is evaluated against the original message when staging; payload mapping and current retain flag are evaluated later. Broker publication and database completion are not atomic. Retained inbox admissions enable duplicate recognition after uncertain acknowledgement.
- **Concurrency/lifecycle:** inbox receives in fresh async scopes and coalesces reconnect requests; outgoing singleton client serializes connection establishment, not all publication. No global ordering or arbitrary provider thread-safety promise is established. Shutdown cancellation differs from permanent poison and transient failure.
- **Compatibility:** topic/filter syntax, reserved property names/case, contract names, GUID values, target IDs, serializer shape and route changes affect peers/retained work. Content Type is descriptive; it does not select serializers. No version negotiation is implemented.
- **Explicit limits:** adapters select MQTT 5 and reference MQTTnet 5.2.0.1603; .NET 10 and Microsoft.Extensions 10.0.11 are project dependencies, not a support guarantee. No TLS security certification, broker retention/redelivery guarantee, exactly-once application effect, global ordering, payload-size cap, durable poison archive, or automatic broker/database provisioning follows.

## Options contract inventory

All values below are **Observed behaviour**, with source evidence in MQTT-002. `required` is a C# initialization declaration, not a usable default string. The adapters use `IOptions`, not a documented hot-reload mechanism.

| Property | Inbox default and validation | Outbox default and validation |
| --- | --- | --- |
| Host | Required; nonblank | Required; nonblank |
| ClientId | Required; nonblank | Required; nonblank |
| Port | 1883; > 0; no validator upper bound | 1883; > 0; no validator upper bound |
| Username | null; credentials only if nonblank | Same |
| Password | null; used only with nonblank Username | Same |
| UseTls | false; enables MQTTnet TLS callback without custom settings | Same |
| QualityOfServiceLevel / QualityOfService | Inbox `QualityOfServiceLevel`: AtLeastOnce; only QoS 1 or 2 | Outbox `QualityOfService`: AtLeastOnce; only QoS 1 or 2 |
| SessionExpiryInterval | uint.MaxValue seconds; > 0 | Not exposed/set by adapter |
| ReconnectDelay | 5 seconds; > 0 and <= TimeSpan.FromMilliseconds(uint.MaxValue - 1) | Not exposed; later durable attempts reconnect |
| ResetSessionOnStart | false; first successful connection may reset session | Not exposed/set by adapter |

TLS does not change port. No username/password/TLS combination validation or custom certificate policy is exposed. No outgoing retain option exists in `MqttOutboxOptions`; retain is per route and defaults false. Broker/MQTTnet constraints beyond the adapter validator remain separate.

## Invariants, limitations and failure windows

- Required: acknowledgement after successful durable admission except explicit poison policy; admission success is not consumer completion. Publication success is not local settlement or subscriber effect.
- Required: retry/claim ownership retains logical/delivery identities and independently settled targets. Broker session reset/expiry can prevent redelivery even when input stayed unacknowledged.
- Observed: incoming reserved IDs are nonempty when present; outgoing optional empty IDs currently violate that peer expectation. Exact-name reserved header overrides are excluded; casing is a compatibility boundary.
- Observed: destination is stored at staging, but map/retain are current-route decisions at dispatch. No automatic retained-target migration is supplied.
- Source checks do not certify broker session persistence, TLS/certificates, connection/publish concurrency, provider SQL semantics, MQTTnet defaults not explicitly set, or full syntax/size constraints. No exact end-to-end latency, global ordering, unlimited duplicate protection or exactly-once application effects are asserted.
- Poison has no local durable archive/requeue contract. Retry/recovery belongs to durable processing after work exists; connection recovery is a separate inbox loop.

## Public contract references

See the [package contract and source map](../../reference/contract-map.md) for public declarations, supporting sources and their specification IDs. The contracts and observable boundaries are defined here and in the numbered specifications.

The grouped adapter and dependency surfaces below complement the per-source map.

| Public surface | Source (under src/Clean.Messaging.) | Specifications |
| --- | --- | --- |
| Inbox `AddMqttInbox(services,configure,routes)` | Inbox.Ingress.Mqtt/DependencyInjection.cs | MQTT-001, 002, 008 |
| `MqttInboxBuilder.On<TPayload>(topicFilter)` | Inbox.Ingress.Mqtt/MqttInboxBuilder.cs | MQTT-001, 003 |
| Inbox `Route<TPayload>.Map<TMessage,TMapper>()`, `.Subscribe()` | Inbox.Ingress.Mqtt/Route.cs | MQTT-001, 003 |
| Inbox `MappedRoute<TPayload,TMessage,TMapper>.Subscribe()` | Inbox.Ingress.Mqtt/MappedRoute.cs | MQTT-001, 003 |
| `MqttInboxOptions` Host/Port/ClientId/SessionExpiryInterval/Username/Password/UseTls/ReconnectDelay/QualityOfServiceLevel/ResetSessionOnStart get/set | Inbox.Ingress.Mqtt/Configuration/MqttInboxOptions.cs | MQTT-002, 007, 008 |
| Outbox `AddMqttOutbox(services,configure,routes)` | Outbox.Dispatch.Mqtt/DependencyInjection.cs | MQTT-001, 002, 011 |
| `MqttOutboxBuilder.On<TMessage>(id)` | Outbox.Dispatch.Mqtt/MqttOutboxBuilder.cs | MQTT-001, 009 |
| Outbox `Route<TMessage>.Map<TPayload,TMapper>()`, `.Publish(destination,retain=false)` | Outbox.Dispatch.Mqtt/Route.cs | MQTT-001, 009, 010 |
| Outbox `MappedRoute<TMessage,TPayload,TMapper>.Publish(destination,retain=false)` | Outbox.Dispatch.Mqtt/MappedRoute.cs | MQTT-001, 009, 010 |
| `MqttOutboxOptions` Host/Port/ClientId/Username/Password/UseTls/QualityOfService get/set | Outbox.Dispatch.Mqtt/MqttOutboxOptions.cs | MQTT-002, 010, 011 |
| `Publishing.MqttTransport.Name = "mqtt"` | Outbox.Dispatch.Mqtt/Publishing/MqttTransport.cs | MQTT-009, 012, 013 |
| Internal incoming route/filter/registry/handler/header/poison machinery | Inbox.Ingress.Mqtt root + Routing | MQTT-003, 004, 005, 006 |
| Internal incoming worker lifecycle/ack/reconnect/logs | Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs | MQTT-005, 006, 007, 008, 014 |
| Internal outgoing route registry/target provider/destination/retain | Outbox.Dispatch.Mqtt/Routing | MQTT-009, 010, 012 |
| Internal outgoing connection, packet factory/metadata, transport, publish exception | Outbox.Dispatch.Mqtt/Connection + Publishing | MQTT-010, 011, 012, 013 |
| Both csproj files: net10.0, nullable/implicit usings, packable, direct provider-neutral project references, MQTTnet 5.2.0.1603/Microsoft.Extensions 10.0.11 | Both adapter project files | MQTT-001, 002, 015 |
| AssemblyInfo friend test assemblies (not test-execution evidence) | Both adapters/Properties/AssemblyInfo.cs | MQTT-015 |
| Historical SPI/registration/concurrency claims | Outbox.Dispatch.Mqtt/README.md | MQTT-009, 011, 015 |

End-to-end trace: inbox builder Subscribe → scoped typed ingress mapping → router/handler reserved metadata validation → runtime mapped-message serialization/consumer entries → relational independent admission → worker acknowledgement → broker-dependent reconnect/redelivery. Outbox builder Publish → exact-type target provider/destination callback → batch target validation/staging → committed retained entry → target-ID current route/map → outgoing metadata packet → shared connected client/PublishAsync → generic external wrapper → fenced durable completion. The runtime default local target provider must also be considered when composing external-only staging; no new composition contract is invented here.

## Open decisions and contradictions

- [GAP-TX-001](../GAPS.md#gap-tx-001): outbox README names old `Clean.Messaging.Outbox.Mqtt`, unimplemented expected OutboxDispatch/IOutboxDispatcher SPI and a registration/options shape absent from current code. Its historical command-topic default and blanket shared-client safety statement are not normative approval of current APIs.
- [GAP-TX-002](../GAPS.md#gap-tx-002): wrapped synchronous cancellation becomes poison whereas direct shutdown/admission cancellation is preserved; the conflict requires a decision rather than source-derived approval.
- [GAP-TX-003](../GAPS.md#gap-tx-003): no approved retained-route/schema compatibility policy or supported rolling-deployment change matrix.
- [GAP-TX-004](../GAPS.md#gap-tx-004): Content Type/arbitrary headers are not end-to-end durable round-trip metadata; desired propagation is not established.
- [GAP-TX-005](../GAPS.md#gap-tx-005): outgoing optional empty GUIDs can be poison for this repository's own ingress adapter.
- [GAP-TX-006](../GAPS.md#gap-tx-006): README overstates connection/publish serialization and concurrency safety; current lock gates connection only. MQTTnet/broker concurrency/disposal outcomes need validation and explicit scope.
- [GAP-TX-007](../GAPS.md#gap-tx-007): poison log says acknowledged before acknowledgement succeeds; disconnect wording says shutdown even in reconnect cleanup.
- [GAP-TX-008](../GAPS.md#gap-tx-008): route/map cross-registration constraints surface on registry resolution; no explicit route-contract startup validator was found.
- [GAP-TX-009](../GAPS.md#gap-tx-009): final disconnect uses an uncancelled token with no adapter timeout; inbox-owned synchronization primitives have no explicit disposal in the authored worker. Bounded shutdown/resource policy is unapproved, not silently repaired.

The consolidated [gap register](../GAPS.md) records affected specifications, evidence and approval-needed decisions. The [inspection register](../../reference/contract-map.md) preserves the full source and public-contract inventory.

## Specifications

- [MQTT-001 — Registration and public fluent routes](MQTT-001-registration-and-public-fluent-routes.md)
- [MQTT-002 — Startup options and MQTT connection configuration](MQTT-002-startup-options-and-mqtt-connection-configuration.md)
- [MQTT-003 — Incoming filters choose exactly one route](MQTT-003-incoming-filters-choose-exactly-one-route.md)
- [MQTT-004 — Incoming metadata, serialization and contract compatibility](MQTT-004-incoming-metadata-serialization-and-contract-compatibility.md)
- [MQTT-005 — Acknowledgement follows admission, not consumer effects](MQTT-005-acknowledgement-follows-admission-not-consumer-effects.md)
- [MQTT-006 — Receive failures, poison acknowledgement and shutdown](MQTT-006-receive-failures-poison-acknowledgement-and-shutdown.md)
- [MQTT-007 — Reset-on-start public session contract](MQTT-007-reset-on-start-public-session-contract.md)
- [MQTT-008 — Inbox reconnect, subscription results and cleanup](MQTT-008-inbox-reconnect-subscription-results-and-cleanup.md)
- [MQTT-009 — Outgoing target and destination staging](MQTT-009-outgoing-target-and-destination-staging.md)
- [MQTT-010 — Outgoing packet metadata and retain semantics](MQTT-010-outgoing-packet-metadata-and-retain-semantics.md)
- [MQTT-011 — Outgoing connection ownership, concurrency and disposal](MQTT-011-outgoing-connection-ownership-concurrency-and-disposal.md)
- [MQTT-012 — Publish result, durable completion and failure classification](MQTT-012-publish-result-durable-completion-and-failure-classification.md)
- [MQTT-013 — External repeat, cancellation and independently successful targets](MQTT-013-external-repeat-cancellation-and-independently-successful-targets.md)
- [MQTT-014 — Observable logs do not overstate settlement](MQTT-014-observable-logs-do-not-overstate-settlement.md)
- [MQTT-015 — Pending interoperability and lifecycle policies](MQTT-015-pending-interoperability-and-lifecycle-policies.md)
