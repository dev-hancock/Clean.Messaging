# MQTT ingress and dispatch — Behavioural specification

**Status:** Draft — 2026-10-13. **Validation:** Not tested.
**Scope:** `Clean.Messaging.Inbox.Ingress.Mqtt` and `Clean.Messaging.Outbox.Dispatch.Mqtt`, with their provider-neutral mapping and durable admission/completion dependencies. Shared mapping contracts are specified in [Transport](transport.md).

## Purpose and evidence policy

Hosts need MQTT subscription routing into durable inbox admission and per-target outgoing MQTT publication driven by durable outbox work. The adapter owns protocol routing, envelope metadata, connection and acknowledgement/publication, not consumer business policy, schema migrations, broker provisioning or external-effect idempotency.

Required blocks cite normative [repository instructions](../../.github/copilot-instructions.md), [coding standards](../CODING_STANDARDS.md), or explicit public XML contract text. Observed blocks describe source inspection, not approved intent. Unknown blocks contain **numbered questions explicitly pending decision**, not executable acceptance criteria or hidden requirements. Criteria inherit their parent block's classification unless explicitly overridden. Neither Required nor Observed is verified/tested. All matrix rows remain **Not tested**; no builds, broker runs or tests were performed.

The audit read every authored `.cs`, `.csproj` and `.md` in both adapters, including options, validators, builders, routes, internal handlers, workers, connection/factory/registry, logging, assembly attributes and the stale outbox README. Additional evidence: [transport reference](../reference/transport.md), [architecture](../reference/architecture.md), [configuration](../reference/configuration.md), [contracts](../reference/contracts.md), [delivery semantics](../explanation/delivery-semantics.md), [identity](../explanation/message-identity.md). These source-derived documents alone cannot approve behaviour. The companion audit manifest records exact paths and discrepancies.

## Contract and boundaries

- **Incoming preconditions:** registered payload/message contracts, a completed non-overlapping subscription route, valid options, composed durable inbox/store and consumers, available broker/database. MQTT 5 user properties provide application identity; MQTT packet identity does not.
- **Outgoing preconditions:** staged target with transport `mqtt`, a stored destination, resolvable current route, source/payload contracts, composed durable outbox/store/workers and broker access.
- **Incoming postcondition:** successful admission precedes acknowledgement; consumers complete later. Poison follows the explicit acknowledge-and-log policy without a durable poison record. Ordinary admission failure does not become successful acknowledgement.
- **Outgoing postcondition:** successful `PublishAsync` result returns to the generic external wrapper for conditional database completion; it does not assert subscriber consumption.
- **State/atomicity:** destination is evaluated against the original message when staging; payload mapping and current retain flag are evaluated later. Broker publication and database completion are not atomic. Retained inbox admissions enable duplicate recognition after uncertain acknowledgement.
- **Concurrency/lifecycle:** inbox receives in fresh async scopes and coalesces reconnect requests; outgoing singleton client serializes connection establishment, not all publication. No global ordering or arbitrary provider thread-safety promise is established. Shutdown cancellation differs from permanent poison and transient failure.
- **Compatibility:** topic/filter syntax, reserved property names/case, contract names, GUID values, target IDs, serializer shape and route changes affect peers/retained work. Content Type is descriptive; it does not select serializers. No version negotiation is implemented.
- **Explicit limits:** adapters select MQTT 5 and reference MQTTnet 5.2.0.1603; .NET 10 and Microsoft.Extensions 10.0.11 are inspected project dependencies, not a tested support matrix. No TLS security certification, broker retention/redelivery guarantee, exactly-once application effect, global ordering, payload-size cap, durable poison archive, or automatic broker/database provisioning follows.

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

## Specifications and acceptance criteria

### MQTT-001 — Registration and public fluent routes

**Classification:** Observed behaviour
**Evidence:** inbox [DependencyInjection](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/DependencyInjection.cs), [builder](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxBuilder.cs), [Route](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Route.cs), [MappedRoute](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MappedRoute.cs); outbox [DependencyInjection](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/DependencyInjection.cs), [builder](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/MqttOutboxBuilder.cs), [Route](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Route.cs), [MappedRoute](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/MappedRoute.cs).

`AddMqttInbox(configure,routes)` installs generic ingress, startup option validation, singleton router, scoped ingress handler and hosted inbox worker. Routes are completed by `On<TPayload>(filter).Subscribe()` or `.Map<TMessage,TMapper>().Subscribe()`. `On`/`Map` alone do not register a completed route.

`AddMqttOutbox(configure,routes)` installs startup option validation, singleton connection/route registry/target provider and scoped MQTT transport delivery; route completion installs generic dispatch mapping. Routes use `On<TMessage>(id).Publish(destination,retain=false)` or `.Map<TPayload,TMapper>().Publish(...)`. There is no separate outgoing MQTT hosted publishing worker; durable outbox workers drive it. Both APIs return the service collection; route completion returns its builder. Generic payload/message types are `notnull`; mapper must implement the appropriate `ITransportMapper` direction. Constructors of fluent route objects/builders are internal.

#### MQTT-001-AC-01 — Identity and mapped routes compose the correct boundary
**Given** a completed identity route and a completed typed mapped route, **When** inbox receives/outbox dispatches their respective configured payload/message types, **Then** the identity route preserves payload value while the mapped route uses its registered typed mapping; durable capabilities/contracts are still supplied separately by the host.

#### MQTT-001-AC-02 — Incomplete or null registration fails clearly
**Given** null configure/routes callbacks, or a route callback that only calls `On`/`Map` without Subscribe/Publish, **When** adapter registration runs, **Then** null callbacks fail argument validation and zero completed routes cause `InvalidOperationException`. A null inbox service collection is explicitly rejected; null outbox services are not made a valid registration by the adapter.

#### MQTT-001-AC-03 — Provider registration alone does not process durable work
**Given** MQTT services/routes/options but no composed durable inbox/outbox integration, **When** the host resolves/executes admission or durable outgoing processing, **Then** MQTT registration does not provision tables, supply missing stores/consumers/contracts or independently complete durable work.

### MQTT-002 — Startup options and MQTT connection configuration

**Classification:** Observed behaviour
**Evidence:** inbox [options](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Configuration/MqttInboxOptions.cs), [validator](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Configuration/MqttInboxOptionsValidator.cs), [worker](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs); outbox [options](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/MqttOutboxOptions.cs), [validator](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/MqttOutboxOptionsValidator.cs), [connection](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Connection/MqttConnection.cs).

#### MQTT-002-AC-01 — Invalid operational options prevent valid startup configuration
**Given** blank Host/ClientId, nonpositive Port, QoS 0/unknown enum, or inbox zero expiry/nonpositive or excessive reconnect delay, **When** registered startup validation evaluates options, **Then** it reports the relevant failures instead of treating those values as a valid operational configuration; multiple invalid values can yield multiple messages.

#### MQTT-002-AC-02 — Validator boundary is distinct from broker validity
**Given** positive Port 65536 and, for inbox, reconnect delay exactly TimeSpan.FromMilliseconds(uint.MaxValue - 1) and positive expiry, **When** adapter validators run with all other values valid, **Then** they accept those values; one millisecond beyond the delay bound is rejected. Validator acceptance does not claim that the networking stack/broker accepts that port or delay operationally.

#### MQTT-002-AC-03 — Defaults remain adapter-specific
**Given** options with Host/ClientId supplied and other properties untouched, **When** option values and resulting connection/publication configuration are inspected, **Then** the defaults match the inventory above, both connections explicitly use MQTT 5, and outgoing configuration does not acquire inbox session-expiry/reset/delay settings.

#### MQTT-002-AC-04 — Credentials and TLS are configured without implicit security claims
**Given** a password with blank Username, **When** connection options are built, **Then** credentials are not configured. **Given** nonblank Username and UseTls=true with Port=1883, **When** connection options are built, **Then** username/password and MQTTnet TLS are enabled without silently changing port or supplying custom certificate rules; successful secure deployment still needs host/broker validation.

### MQTT-003 — Incoming filters choose exactly one route

**Classification:** Observed behaviour
**Evidence:** [RouteFilter](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/RouteFilter.cs), [router](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Routing/MqttInboxRouter.cs), [route contract](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Routing/IMqttInboxRoute.cs), [poison exception](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Routing/MqttPoisonMessageException.cs).

#### MQTT-003-AC-01 — Wildcards and topic levels select one payload map
**Given** non-overlapping filters `devices/+/state` and `events/#`, **When** `devices/a/state` or `events/a/b` arrives, **Then** exactly its matching route supplies the payload/message map, not broadcast fan-out across routes. Durable consumer fan-out occurs after admission, not MQTT route selection.

#### MQTT-003-AC-02 — Invalid wildcard placement and overlap reject configuration
**Given** `a/#/b`, `a+b`, `a#`, duplicate filters, or overlapping `a/+` and `a/b`, **When** the router is constructed, **Then** invalid syntax or overlap fails configuration instead of selecting an arbitrary winner. A blank filter is already rejected by `On`/Subscribe.

#### MQTT-003-AC-03 — System-topic distinction and level boundaries remain explicit
**Given** `#` and `$SYS/#`, or distinct filters containing empty levels such as `a//b` and `a/b`, **When** overlap validation runs, **Then** the system/non-system filters are not deemed overlapping solely through `#`, and distinct empty/nonempty level paths remain distinct. Runtime route matching follows MQTTnet's topic-filter comparer, not a custom global ordering rule.

#### MQTT-003-AC-04 — Unroutable incoming work is poison
**Given** a received topic with no matching configured route, **When** routing resolves it, **Then** it raises MQTT poison classification and no durable admission occurs. If multiple runtime matches occur despite configuration validation, the same poison path rejects ambiguity rather than admitting twice.

### MQTT-004 — Incoming metadata, serialization and contract compatibility

**Classification:** Observed behaviour
**Evidence:** [ingress handler](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs), [reserved names](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Routing/MqttHeaders.cs), [generic ingress](../../src/Clean.Messaging.Inbox.Ingress/TransportIngress.cs), [inbox factory](../../src/Clean.Messaging.Inbox.Ingress/InboxMessageFactory.cs).

Reserved exact-case names are `message-id`, `event-id`, `contract`, `correlation-id`, `causation-id`. Lookup reads the last property with that exact name. Message ID is required indirectly by generic ingress; all present IDs must parse as nonempty GUIDs. Event/correlation/causation are optional. Declared contract is optional but, when present, must ordinally equal the selected payload type's registered contract.

#### MQTT-004-AC-01 — Valid peer metadata admits stable application identities
**Given** a compatible payload on one route with valid `message-id`, distinct `event-id` and valid optional context IDs, **When** MQTT ingress succeeds, **Then** durable admission contains the mapped consumer message with those identities; broker packet ID, QoS, duplicate and retain flags do not replace them.

#### MQTT-004-AC-02 — Optional declaration and event identity have defined absence semantics
**Given** valid message identity and payload but no `contract` or `event-id` property, **When** ingress admits it, **Then** the route's registered payload contract selects deserialization and durable event identity defaults to message identity. A missing route payload contract is a runtime contract/admission failure, not an automatically inferred CLR name.

#### MQTT-004-AC-03 — Malformed identity and mismatched contract are poison
**Given** absent message identity, a present malformed/empty GUID in any reserved ID, or a declared contract unequal by case/value to the selected registered contract, **When** MQTT ingress evaluates it, **Then** poison is raised before successful durable admission; invalid present metadata is not treated as absent or replaced with a fresh identity.

#### MQTT-004-AC-04 — Last exact-name property wins
**Given** repeated `message-id`/`contract` properties and case-distinct `Message-Id`, **When** metadata is read, **Then** the last exact lowercase reserved property determines validation/admission and the case-distinct property does not override it. A malformed last reserved ID is poison even when an earlier one was valid.

#### MQTT-004-AC-05 — Payload segmentation and Content Type do not alter decoding policy
**Given** identical payload bytes delivered as one segment and multiple segments with a nondefault Content Type and nonreserved UTF-8 properties, **When** ingress processes them, **Then** both yield the same mapped message bytes/value; Content Type and nonreserved headers enter the intermediate envelope only, do not select another serializer and are not persisted by built-in generic ingress.

### MQTT-005 — Acknowledgement follows admission, not consumer effects

**Classification:** Required behaviour
**Normative basis:** coding standards §9 requires separating durable admission from dispatch, not acknowledging broker admission failures as successful processing, and preserving explicit poison policy; repository instructions preserve identities and transaction boundaries.
**Source evidence:** [worker Receive](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs), [ingress handler](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs), dependency [InboxStore](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs).

#### MQTT-005-AC-01 — Successful admission transfers processing responsibility
**Given** valid input and available durable inbox integration with consumers paused, **When** the message is received and durable admission commits, **Then** successful MQTT acknowledgement can occur before consumer completion, but not before admission success; consumers remain independently outstanding local work.

#### MQTT-005-AC-02 — Admission failure is not acknowledged as success
**Given** valid route/payload/metadata but unavailable database or failing durable admission, **When** receipt fails admission, **Then** it remains unacknowledged as a successful admission and reconnect is requested; no consumer success or poison discard is invented to hide the persistence fault.

#### MQTT-005-AC-03 — Duplicate receipt after lost acknowledgement does not reset work
**Given** admission committed, acknowledgement was lost, and the broker retained session/delivery state and redelivers the same IDs, **When** the duplicate reaches the relational inbox and is acknowledged, **Then** one retained admission/obligation set remains and completed retained consumers are not deliberately reset. This is conditional redelivery, not a promise that the broker retains any particular session.

#### MQTT-005-AC-04 — Explicit poison policy stays distinct from admission failure
**Given** unroutable, malformed-metadata or ingress-mapping poison input, **When** receipt classifies it, **Then** the worker logs rejection and attempts acknowledgement without storing an inbox poison/dead-letter record; ordinary durable admission errors do not silently use this discard policy.

### MQTT-006 — Receive failures, poison acknowledgement and shutdown

**Classification:** Observed behaviour
**Evidence:** [worker](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs), [handler](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxIngressHandler.cs), dependency [Inbox](../../src/Clean.Messaging.Inbox/Inbox.cs) and [InboxStore](../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs).

AutoAcknowledge is disabled. Each receipt resolves one route and a fresh async scope, awaits ingress then explicit acknowledgement with host stopping token. Handler preserves direct token cancellation and translates `InboxIngressException` to poison; it does not translate ordinary durable admission exceptions. No matching consumer contract fails before admission through the runtime consumer registry; an empty batch supplied to the EF store also cannot create a new admission because it lacks a first entry. These are not silently acknowledged poison.

#### MQTT-006-AC-01 — Acknowledgement failure requests recovery without erasing admission
**Given** committed admission or already-classified poison, **When** explicit acknowledgement throws a non-shutdown exception, **Then** the worker logs the appropriate normal/poison acknowledgement failure and signals reconnect; previously committed admission stays available, and acknowledgement is not reported as successful by the exception path.

#### MQTT-006-AC-02 — Shutdown cancellation leaves unsettled delivery unacknowledged
**Given** admission/acknowledgement is awaiting cancellable work and the host stopping token is cancelled, **When** direct `OperationCanceledException` reaches receive handling, **Then** ordinary admission-failure logging/reconnect classification is suppressed and the delivery is not manufactured as acknowledged. Already committed database work is not automatically rolled back.

#### MQTT-006-AC-03 — No consumer is an admission failure rather than automatic poison discard
**Given** valid MQTT payload/identity and mapping but no registered consumer for the mapped runtime message, **When** admission executes, **Then** the runtime consumer failure propagates through the ordinary admission-failure path and no successful acknowledgement is made; reconnect is requested rather than a durable poison record being created.

#### MQTT-006-AC-04 — Wrapped mapper cancellation currently reaches poison policy
**Given** a synchronous mapper throws cancellation which generic ingress wraps as `InboxIngressException`, **When** MQTT handler observes that wrapped exception, **Then** it becomes poison and invokes the poison acknowledgement path; this differs from direct admission/shutdown cancellation. See [GAP-TX-002](GAPS.md#gap-tx-002); this observation is not approval of the policy.

### MQTT-007 — Reset-on-start public session contract

**Classification:** Required behaviour
**Normative basis:** public XML documentation on [MqttInboxOptions.ResetSessionOnStart](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Configuration/MqttInboxOptions.cs): discards broker-held session on the first successful connection after process starts; subsequent reconnects remain persistent.
**Source evidence:** [worker Connect/CreateOptions](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs). Broker actual session storage/expiry are outside the adapter's guarantee.

#### MQTT-007-AC-01 — Reset happens on first successful connection only
**Given** ResetSessionOnStart=true and a first refused connection followed by a successful connection, **When** later reconnects occur, **Then** failed initial attempts keep requesting clean start, the first successful connection requests clean start, and subsequent reconnects request persistent session even if subscriptions failed after that first success.

#### MQTT-007-AC-02 — Default startup does not request a session reset
**Given** ResetSessionOnStart=false, **When** startup and later reconnect options are sent, **Then** clean start remains false and configured expiry/client identity are sent; whether prior broker state exists is not inferred from these options.

### MQTT-008 — Inbox reconnect, subscription results and cleanup

**Classification:** Observed behaviour
**Evidence:** entire [MqttInboxWorker](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs), including connect/subscription checks, one-slot reconnect semaphore, connection lock, disconnected callback and final cleanup.

#### MQTT-008-AC-01 — Each successful connection subscribes all routes again
**Given** configured routes and a successful MQTT connect response, **When** startup or reconnect completes its subscriptions, **Then** all filters are subscribed again regardless of session-present status. Each result must contain exactly one granted QoS 1 or 2 result; granted QoS need not equal requested QoS.

#### MQTT-008-AC-02 — Refused connection or rejected subscription uses fixed reconnect loop
**Given** non-success connection result, subscription rejection/QoS 0, or an unexpected subscription result count, **When** connection setup runs, **Then** setup fails, logs a warning and enters disconnect/fixed configured delay/reconnect recovery instead of a successful ready state or durable entry retry loop. A zero/multiple result count is not accepted as valid subscription success.

#### MQTT-008-AC-03 — Concurrent failure signals do not queue unbounded reconnects
**Given** a connected inbox client and several concurrent admission failures or unexpected disconnect signals, **When** reconnect is signalled, **Then** requests are coalesced rather than creating unbounded independent pending reconnects; connect/disconnect transitions are serialized. Worker-initiated disconnect does not recursively resignal through the disconnected callback.

#### MQTT-008-AC-04 — Shutdown removes callbacks and attempts final disconnect
**Given** the worker owns a connected client, **When** host shutdown ends its loop, **Then** receive/disconnected callbacks are removed, final disconnect is attempted with CancellationToken.None and disconnect exceptions are logged, and the client is disposed. This does not assert a bounded network disconnect duration or durable replay after broker session expiry/reset.

### MQTT-009 — Outgoing target and destination staging

**Classification:** Observed behaviour
**Evidence:** [MqttOutboxBuilder](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/MqttOutboxBuilder.cs), [target provider](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Routing/MqttOutboxTargets.cs), [route](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Routing/MqttOutboxRoute.cs), [registry](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Routing/MqttRouteRegistry.cs), dependency [OutboxEntryFactory](../../src/Clean.Messaging.Outbox/Persistence/OutboxEntryFactory.cs).

Target IDs are nonblank, <=200 ASCII characters and ordinally unique within the builder. Destination callbacks are nonnull; runtime return must be nonblank. `MqttOutboxTargets.GetTargets` matches exact concrete runtime message type and evaluates against the original message, not mapped payload. All batch targets, including local consumers, must have unique target IDs. Destination is stored per target; mapping/retain use the route found at dispatch by stored target ID. The default local target provider's missing-consumer lookup can fail even with MQTT routes; registering a MQTT route alone does not prove external-only staging composition.

#### MQTT-009-AC-01 — Destination is selected before persistence, mapping later
**Given** an A-to-B outgoing map and a destination callback based on A, **When** A is staged then dispatched after commit, **Then** its stored target contains the destination evaluated from original A, transport `mqtt` and route ID, while published bytes are later mapped B data.

#### MQTT-009-AC-02 — Target identity validation and duplicates prevent ambiguous work
**Given** blank/non-ASCII/201-character target ID, repeated completed route ID, null destination callback, or collision with a local consumer ID in one batch, **When** route registration or staging reaches that boundary, **Then** the relevant argument/configuration failure occurs without accepting ambiguous duplicate target work. A valid 200-character ASCII ID is within the adapter's identity boundary.

#### MQTT-009-AC-03 — Blank destination fails staging; topic grammar has another boundary
**Given** a destination callback returning whitespace or throwing, **When** its exact-type message is staged, **Then** whitespace yields permanent `OutboxDispatchException` and a callback exception propagates before successful staging. **Given** a nonblank syntactically invalid MQTT publication topic, **When** the target provider evaluates it, **Then** it is not rejected solely by this nonblank check; MQTTnet/broker validation may reject it later.

#### MQTT-009-AC-04 — Retries preserve destination but current route can change payload or retain
**Given** staged work with destination X and route ID R, **When** a later deployment changes R's destination callback, map or retain and retries the entry, **Then** the stored destination remains X while the current map/retain are used. Removing R causes permanent route lookup failure; changing target ID does not automatically migrate old retained work.

#### MQTT-009-AC-05 — Exact-type target matching does not imply polymorphic publication
**Given** a route registered for base type A but a staged concrete subclass B without its own MQTT route, **When** the MQTT target provider enumerates targets for B, **Then** that A route contributes no MQTT target. Other target providers/runtime consumers determine whether staging otherwise succeeds; absence is not proof of global dispatch success.

### MQTT-010 — Outgoing packet metadata and retain semantics

**Classification:** Observed behaviour
**Evidence:** [MqttMessageFactory](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttMessageFactory.cs), [reserved names](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttMessageMetadata.cs), [transport name](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttTransport.cs), [dispatch factory](../../src/Clean.Messaging.Outbox.Dispatch/TransportMessageFactory.cs).

#### MQTT-010-AC-01 — Packet includes stored route destination, payload and application identities
**Given** valid mapped transport data, stored topic, route retain and validated outgoing QoS, **When** the packet is constructed/published, **Then** topic/payload/QoS/retain match those inputs and UTF-8 reserved user properties contain required nonempty message/event GUIDs in D format and the mapped payload contract. Retain defaults false and is unrelated to durable retention.

#### MQTT-010-AC-02 — Reserved metadata cannot be overridden by caller headers
**Given** a transport message with exact reserved-name header attempts plus duplicate nonreserved headers and nonblank Content Type, **When** the packet is constructed, **Then** factory-owned reserved properties prevail, nonreserved properties are appended in UTF-8, duplicate nonreserved values remain representable and nonblank Content Type is included. Case-distinct names are not reserved aliases.

#### MQTT-010-AC-03 — Missing required IDs or destination reject outgoing construction
**Given** absent/empty required message/event ID or blank destination, **When** packet construction runs, **Then** permanent `OutboxDispatchException` is raised and no successful packet publication is reported. Packet construction follows connected-client acquisition in the current transport path, so connection attempts are not excluded by this failure.

#### MQTT-010-AC-04 — Optional empty IDs expose current peer incompatibility
**Given** transport correlation/causation set to Guid.Empty, **When** the outgoing factory constructs metadata, **Then** it emits their D-format empty values without separate validation; an incoming adapter receiving that present value classifies it as poison. Absent optional IDs are omitted. See [GAP-TX-005](GAPS.md#gap-tx-005); this is not a requirement for interoperable empty IDs.

### MQTT-011 — Outgoing connection ownership, concurrency and disposal

**Classification:** Observed behaviour
**Evidence:** [MqttConnection](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Connection/MqttConnection.cs), [transport](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttOutboxTransport.cs), registration singleton lifetime.

#### MQTT-011-AC-01 — Concurrent disconnected dispatches share connection establishment
**Given** the singleton client is disconnected and multiple outgoing attempts call for it concurrently, **When** the first connection succeeds and others recheck connectivity, **Then** they use the shared connected client without simultaneously establishing separate connections. Once connected, publication is not globally serialized by this connection lock.

#### MQTT-011-AC-02 — Connection rejection and later disconnect require another durable attempt
**Given** connect returns while the client remains disconnected, **When** dispatch requests the client, **Then** the attempt fails rather than publishing on assumed connectivity. **Given** a later disconnected client, **When** a later durable attempt requests it, **Then** it attempts reconnection; no independent outgoing background reconnect/retry loop is supplied.

#### MQTT-011-AC-03 — Connection wait cancellation does not imply publication
**Given** connection establishment is serialized and another caller waits for the lock or cancellable connect I/O, **When** its attempt token is cancelled, **Then** waiting/I/O can cancel without manufactured publication or durable completion. An already-connected fast-path result still relies on subsequent PublishAsync's cancellation handling.

#### MQTT-011-AC-04 — Disposal releases owned client without promised graceful disconnect
**Given** the outgoing connection owns its semaphore/client, **When** DI disposes it after host work stops, **Then** both are disposed; there is no explicit outgoing graceful-disconnect operation or promise that racing active publications are drained. Concurrent-use/disposal and full client thread safety remain [GAP-TX-006](GAPS.md#gap-tx-006).

### MQTT-012 — Publish result, durable completion and failure classification

**Classification:** Observed behaviour
**Evidence:** [MqttOutboxTransport](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttOutboxTransport.cs), [MqttPublishException](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttPublishException.cs), dependencies [external wrapper](../../src/Clean.Messaging.Outbox/Transport/OutboxTransport.cs), [processor](../../src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs), [retry policy](../../src/Clean.Messaging/Retry/RetryPolicy.cs).

Current route lookup → durable-source deserialize/map/outgoing serialize → connected client → packet factory → awaited PublishAsync(token) → result.IsSuccess → generic wrapper's fenced completion. No adapter-level publish retry occurs. Published bytes are not a receipt from subscribers.

| Unsuccessful broker result | Code | Action |
| --- | --- | --- |
| NotAuthorized | mqtt.publish.not_authorized | Fault/permanent |
| TopicNameInvalid | mqtt.publish.topic_name_invalid | Fault/permanent |
| PayloadFormatInvalid | mqtt.publish.payload_format_invalid | Fault/permanent |
| Other unsuccessful result | mqtt.publish.rejected | Retry/transient |

#### MQTT-012-AC-01 — Successful publication returns for owned settlement
**Given** a valid outgoing target and a successful broker publish result, **When** dispatch returns, **Then** the external wrapper attempts completion for that target/key/claim; the adapter did not wait for downstream application consumption, and false/rejected settlement is not changed into a successful database completion.

#### MQTT-012-AC-02 — Permanent publish rejections do not receive transient retries
**Given** an owned entry and broker NotAuthorized, TopicNameInvalid or PayloadFormatInvalid result, **When** durable failure handling classifies the thrown publish failure, **Then** its code/action match the table and owned dead-letter settlement is attempted instead of scheduling a transient retry. Broker reason code/string remain in diagnostics.

#### MQTT-012-AC-03 — Other rejection or network failure uses durable retry budget
**Given** another unsuccessful publish reason, or an unclassified network/connect/publish exception, **When** a durable attempt fails below its configured retry limit, **Then** shared retry handling can schedule an owned retry; exhaustion can dead-letter. There is no second adapter retry around the publish call and no universal maximum external-effect count is asserted.

#### MQTT-012-AC-04 — Missing route or mapping prevents successful publication
**Given** a retained target whose current route is absent, or whose stored source/map/contract is incompatible, **When** MQTT dispatch resolves/maps it, **Then** it fails as permanent `outbox.dispatch.failed` before successful publication/completion; it does not select a different route by topic or target-ID prefix.

### MQTT-013 — External repeat, cancellation and independently successful targets

**Classification:** Required behaviour
**Normative basis:** repository instructions preserve per-target completion, leases, cancellation/failure classifications and reject exactly-once external effects; coding standards §§7–9 require separating publication from local settlement and rejecting stale ownership.
**Source evidence:** dependencies [transport wrapper](../../src/Clean.Messaging.Outbox/Transport/OutboxTransport.cs), [processor](../../src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs), [durable store](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs), MQTT publishing path.

#### MQTT-013-AC-01 — Post-publish crash can produce the same logical publication again
**Given** the broker accepted publication but the process failed before persistent target completion, **When** durable recovery reclaims the unresolved entry, **Then** publication may repeat with the same retained message/event IDs and stored destination; neither QoS 2 nor local uniqueness proves a single subscriber effect.

#### MQTT-013-AC-02 — Lease fencing limits settlement, not an issued publish
**Given** a publish in flight when its claim expires and another owner starts, **When** the stale publish finishes and tries database completion, **Then** its old claim cannot complete the new owner's work, although remote effects may overlap or have already succeeded.

#### MQTT-013-AC-03 — Shutdown and claim-loss cancellation are not ordinary poison
**Given** a cancellable outgoing attempt, **When** shutdown or lease-loss cancels handling, **Then** cancellation flows to connection/publication/settlement boundaries and is not treated as a broker poison discard or ordinary permanent publish rejection. A publication accepted before cancellation is not claimed to be reversed.

#### MQTT-013-AC-04 — Successful sibling target remains settled during another failure
**Given** one retained MQTT/local target is completed and another MQTT target fails, **When** durable retries/recovery process the failed target, **Then** the completed sibling is not deliberately re-executed or reset solely for that failure; no global topic/event completion ordering is promised.

### MQTT-014 — Observable logs do not overstate settlement

**Classification:** Observed behaviour
**Evidence:** source-generated events in [MqttInboxWorker](../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Runtime/MqttInboxWorker.cs), outgoing source inventory and shared [OutboxProcessor](../../src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs).

Inbox events: 1 Information connected Host/Port/RouteCount; 2 Warning setup failure Host/Port/exception; 3 Error poison Topic/exception before acknowledgement (wording says already acknowledged); 4 Warning poison ack failure Topic/exception; 5 Error admission/normal ack failure Topic/exception; 6 Debug disconnect failure (wording mentions shutdown also in reconnect cleanup). Outgoing has no dedicated MQTT metrics/logger in the audited adapter; shared durable metrics/logs describe local outcomes, not subscriber effects.

#### MQTT-014-AC-01 — Poison logging can precede a failed acknowledgement
**Given** poison input and failing acknowledgement, **When** receive handles it, **Then** event 3 is observable before event 4 and no successful acknowledgement is implied by database/broker evidence; event 3's current wording is misleading rather than proof of success. See [GAP-TX-007](GAPS.md#gap-tx-007).

#### MQTT-014-AC-02 — Connection and admission diagnostics retain their operational boundary
**Given** setup failure, ordinary admission/ack failure, or disconnect cleanup failure, **When** the worker logs it, **Then** the corresponding levels/fields above identify setup, receipt or cleanup rather than claiming consumer completion; outgoing durable completion metrics cannot prove subscriber application effects.

### MQTT-015 — Pending interoperability and lifecycle policies

**Classification:** Unknown behaviour
**Evidence:** [stale outbox README](../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/README.md), current registration/builders/connection/factories and inbox lifecycle. No decision approving the stale SPI, full shared-client concurrency claim, optional empty-ID interoperability or bounded shutdown/resource policy was found.
**Pending decisions:** [GAP-TX-001](GAPS.md#gap-tx-001), [GAP-TX-005](GAPS.md#gap-tx-005), [GAP-TX-006](GAPS.md#gap-tx-006), [GAP-TX-009](GAPS.md#gap-tx-009). These numbered questions are not executable acceptance criteria. An owner decision must select definite observable outcomes before criteria can be authored.

1. **Empty optional IDs between peers:** Must empty correlation/causation identities be omitted, rejected before publication or interpreted as absent by ingress? Which representation must both sides support? No alternative is selected here; current asymmetric behaviour remains MQTT-010.
2. **Supported concurrency and shutdown:** Which overlaps among publication, reconnect and resource disposal must be supported? Must shutdown drain in-flight callbacks/publications, within what bound, and with which allowed outcomes? Which component owns synchronization disposal? No MQTTnet safety or bounded termination guarantee is selected here.
3. **Authoritative registration documentation:** Should the historical README SPI be retired or replaced with the current public API, and what migration guidance is required? Who approves that API/documentation decision? No production or reference-documentation repair is performed in this scoped specification task.

## Invariants, limitations and failure windows

- Required: acknowledgement after successful durable admission except explicit poison policy; admission success is not consumer completion. Publication success is not local settlement or subscriber effect.
- Required: retry/claim ownership retains logical/delivery identities and independently settled targets. Broker session reset/expiry can prevent redelivery even when input stayed unacknowledged.
- Observed: incoming reserved IDs are nonempty when present; outgoing optional empty IDs currently violate that peer expectation. Exact-name reserved header overrides are excluded; casing is a compatibility boundary.
- Observed: destination is stored at staging, but map/retain are current-route decisions at dispatch. No automatic retained-target migration is supplied.
- Source checks do not certify broker session persistence, TLS/certificates, connection/publish concurrency, provider SQL semantics, MQTTnet defaults not explicitly set, or full syntax/size constraints. No exact end-to-end latency, global ordering, unlimited duplicate protection or exactly-once application effects are asserted.
- Poison has no local durable archive/requeue contract. Retry/recovery belongs to durable processing after work exists; connection recovery is a separate inbox loop.

## Verification matrix

Levels below are prospective and must actually exercise the claimed boundary. Relational identity/fencing needs a real configured provider. MQTT acknowledgement, session/subscription/grant, reconnect and repeat scenarios need controllable protocol/broker observation; local fakes alone cannot certify broker redelivery. Unknown-block decision questions are excluded from this matrix and executable acceptance-criterion totals.

| Criterion | Recommended level | Observable assertion | Status |
| --- | --- | --- | --- |
| MQTT-001-AC-01 | DI + transport integration | Identity/mapped routes use the registered mapping direction. | Not tested |
| MQTT-001-AC-02 | Unit / DI integration | Null callbacks and incomplete routes fail at registration. | Not tested |
| MQTT-001-AC-03 | DI integration | Missing durable integration is not provisioned by MQTT setup. | Not tested |
| MQTT-002-AC-01 | Unit / options integration | Each invalid option produces startup validation failure. | Not tested |
| MQTT-002-AC-02 | Unit | Delay boundary/positive overrange port produce stated validator results. | Not tested |
| MQTT-002-AC-03 | Unit + transport integration | Inspect defaults and sent MQTT 5 connection fields. | Not tested |
| MQTT-002-AC-04 | Unit + transport integration | Inspect credentials/TLS/unchanged port, not a security certification. | Not tested |
| MQTT-003-AC-01 | Transport integration | Topic packets select one expected route and mapped admission. | Not tested |
| MQTT-003-AC-02 | Unit / DI integration | Wildcard placement and overlapping filters reject construction. | Not tested |
| MQTT-003-AC-03 | Unit + transport integration | System/empty-level filters validate and match at their topic boundaries. | Not tested |
| MQTT-003-AC-04 | Transport integration | Unmatched/ambiguous topic takes poison path without admission. | Not tested |
| MQTT-004-AC-01 | Transport + persistence integration | Admission IDs equal MQTT metadata rather than packet identity. | Not tested |
| MQTT-004-AC-02 | Transport + persistence integration | Absent declaration/event yields route contract and event=message ID. | Not tested |
| MQTT-004-AC-03 | Unit + transport integration | Invalid IDs/contracts cause poison and no successful admission. | Not tested |
| MQTT-004-AC-04 | Unit | Last exact-name properties control result; case aliases do not. | Not tested |
| MQTT-004-AC-05 | Unit + persistence integration | Segmented bytes decode equally; headers/type do not persist/select codec. | Not tested |
| MQTT-005-AC-01 | Transport + persistence integration | Capture admission commit before ack while consumers remain pending. | Not tested |
| MQTT-005-AC-02 | Transport + persistence integration | Inject storage fault; observe no ack and reconnect request. | Not tested |
| MQTT-005-AC-03 | Transport + concurrency integration | Lose ack/redeliver retained packet; one admission and obligation set. | Not tested |
| MQTT-005-AC-04 | Transport + persistence integration | Poison ack attempted; no poison admission/dead-letter row. | Not tested |
| MQTT-006-AC-01 | Transport + persistence integration | Ack fault requests reconnect and leaves committed admission unchanged. | Not tested |
| MQTT-006-AC-02 | Transport integration | Cancel blocked admission/ack; no synthetic ack or ordinary error classification. | Not tested |
| MQTT-006-AC-03 | Transport + DI integration | Missing consumer triggers ordinary admission-failure path, not poison ack. | Not tested |
| MQTT-006-AC-04 | Unit + transport integration | Wrapped mapper cancellation produces current poison path. | Not tested |
| MQTT-007-AC-01 | Transport integration | Inspect clean-start flags over refused, first successful and reconnect attempts. | Not tested |
| MQTT-007-AC-02 | Transport integration | Default connects send clean-start=false plus configured expiry/client ID. | Not tested |
| MQTT-008-AC-01 | Transport integration | All filters resubscribe; grant 1/2 accepted even differing from request. | Not tested |
| MQTT-008-AC-02 | Transport integration | Refusal/invalid grants/counts recover only via fixed connection loop. | Not tested |
| MQTT-008-AC-03 | Concurrency + transport integration | Racing signals yield bounded pending reconnect and serialized lifecycle. | Not tested |
| MQTT-008-AC-04 | Transport integration | Shutdown removes callbacks, attempts final disconnect and disposes client. | Not tested |
| MQTT-009-AC-01 | Persistence + transport integration | Staged topic from original A; later bytes encode mapped B. | Not tested |
| MQTT-009-AC-02 | Unit / persistence integration | Invalid IDs/callback and cross-provider target collision reject. | Not tested |
| MQTT-009-AC-03 | Unit + transport integration | Blank/throwing callback fails staging; nonblank invalid topic reaches later boundary. | Not tested |
| MQTT-009-AC-04 | Persistence + transport integration | Old destination unchanged; current map/retain used; removed route faults. | Not tested |
| MQTT-009-AC-05 | Unit | Base route contributes no subclass MQTT target. | Not tested |
| MQTT-010-AC-01 | Transport integration | Capture topic/bytes/QoS/retain and UTF-8 required D-format properties. | Not tested |
| MQTT-010-AC-02 | Unit + transport integration | Reserved overrides absent; arbitrary headers/type included as specified. | Not tested |
| MQTT-010-AC-03 | Unit + transport integration | Invalid required packet inputs fail with no successful publication. | Not tested |
| MQTT-010-AC-04 | Unit + transport integration | Outgoing empty optional GUID emitted, then rejected by ingress peer. | Not tested |
| MQTT-011-AC-01 | Concurrency + transport integration | Concurrent disconnected attempts establish one shared client connection. | Not tested |
| MQTT-011-AC-02 | Transport integration | Rejected connect fails; disconnected later attempt reconnects with no background loop. | Not tested |
| MQTT-011-AC-03 | Concurrency + transport integration | Cancel waiting/connecting attempt; no manufactured publish/complete. | Not tested |
| MQTT-011-AC-04 | Lifecycle integration | Disposal after processing releases client/lock without explicit graceful-disconnect contract. | Not tested |
| MQTT-012-AC-01 | Transport + persistence integration | Publish success precedes conditional settlement, not subscriber receipt. | Not tested |
| MQTT-012-AC-02 | Transport + persistence integration | Three permanent broker reasons yield expected code and owned dead-letter. | Not tested |
| MQTT-012-AC-03 | Transport + persistence integration | Other/network failures schedule owned retry below budget and can fault on exhaustion. | Not tested |
| MQTT-012-AC-04 | Unit + transport integration | Missing/incompatible current route fails permanently before successful publish. | Not tested |
| MQTT-013-AC-01 | Transport + persistence integration | Inject post-publish crash; reclaim repeats same IDs/destination. | Not tested |
| MQTT-013-AC-02 | Concurrency + transport integration | Expired claim cannot settle after new claim despite old publish result. | Not tested |
| MQTT-013-AC-03 | Transport + persistence integration | Cancel shutdown/lease loss; preserve cancellation, no claim that accepted effect rolled back. | Not tested |
| MQTT-013-AC-04 | Persistence + transport integration | Retry/fault affects unresolved target only; completed sibling remains complete. | Not tested |
| MQTT-014-AC-01 | Transport integration | Poison event 3 precedes failed-ack event 4; broker truth does not imply ack success. | Not tested |
| MQTT-014-AC-02 | Transport / logging integration | Levels/fields reflect setup/admission/cleanup, not downstream effects. | Not tested |

## Public-contract and authored-source traceability

All public source operations/properties are grouped below. Exact source paths, internal responsibility and project/README/assembly coverage are in the companion audit manifest; internal visibility does not remove an implementation entry point from the audit.

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

- [GAP-TX-001](GAPS.md#gap-tx-001): outbox README names old `Clean.Messaging.Outbox.Mqtt`, unimplemented expected OutboxDispatch/IOutboxDispatcher SPI and a registration/options shape absent from current code. Its historical command-topic default and blanket shared-client safety statement are not normative approval of current APIs.
- [GAP-TX-002](GAPS.md#gap-tx-002): wrapped synchronous cancellation becomes poison whereas direct shutdown/admission cancellation is preserved; the conflict requires a decision rather than source-derived approval.
- [GAP-TX-003](GAPS.md#gap-tx-003): no approved retained-route/schema compatibility policy or supported rolling-deployment change matrix.
- [GAP-TX-004](GAPS.md#gap-tx-004): Content Type/arbitrary headers are not end-to-end durable round-trip metadata; desired propagation is not established.
- [GAP-TX-005](GAPS.md#gap-tx-005): outgoing optional empty GUIDs can be poison for this repository's own ingress adapter.
- [GAP-TX-006](GAPS.md#gap-tx-006): README overstates connection/publish serialization and concurrency safety; current lock gates connection only. MQTTnet/broker concurrency/disposal outcomes need validation and explicit scope.
- [GAP-TX-007](GAPS.md#gap-tx-007): poison log says acknowledged before acknowledgement succeeds; disconnect wording says shutdown even in reconnect cleanup.
- [GAP-TX-008](GAPS.md#gap-tx-008): route/map cross-registration constraints surface on registry resolution; no explicit route-contract startup validator was found.
- [GAP-TX-009](GAPS.md#gap-tx-009): final disconnect uses an uncancelled token with no adapter timeout; inbox-owned synchronization primitives have no explicit disposal in the authored worker. Bounded shutdown/resource policy is unapproved, not silently repaired.

The consolidated [gap register](GAPS.md) records affected specifications, evidence and approval-needed decisions. The [inspection register](INSPECTION.md) preserves the full source and public-contract inventory.
