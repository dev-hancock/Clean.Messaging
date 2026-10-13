# MQTT-001 — Registration and public fluent routes

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** inbox [DependencyInjection](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/DependencyInjection.cs), [builder](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MqttInboxBuilder.cs), [Route](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Route.cs), [MappedRoute](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/MappedRoute.cs); outbox [DependencyInjection](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/DependencyInjection.cs), [builder](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/MqttOutboxBuilder.cs), [Route](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Route.cs), [MappedRoute](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/MappedRoute.cs).

`AddMqttInbox(configure,routes)` installs generic ingress, startup option validation, singleton router, scoped ingress handler and hosted inbox worker. Routes are completed by `On<TPayload>(filter).Subscribe()` or `.Map<TMessage,TMapper>().Subscribe()`. `On`/`Map` alone do not register a completed route.

`AddMqttOutbox(configure,routes)` installs startup option validation, singleton connection/route registry/target provider and scoped MQTT transport delivery; route completion installs generic dispatch mapping. Routes use `On<TMessage>(id).Publish(destination,retain=false)` or `.Map<TPayload,TMapper>().Publish(...)`. There is no separate outgoing MQTT hosted publishing worker; durable outbox workers drive it. Both APIs return the service collection; route completion returns its builder. Generic payload/message types are `notnull`; mapper must implement the appropriate `ITransportMapper` direction. Constructors of fluent route objects/builders are internal.

## MQTT-001-AC-01 — Identity and mapped routes compose the correct boundary
**Given** a completed identity route and a completed typed mapped route, **When** inbox receives/outbox dispatches their respective configured payload/message types, **Then** the identity route preserves payload value while the mapped route uses its registered typed mapping; durable capabilities/contracts are still supplied separately by the host.

## MQTT-001-AC-02 — Incomplete or null registration fails clearly
**Given** null configure/routes callbacks, or a route callback that only calls `On`/`Map` without Subscribe/Publish, **When** adapter registration runs, **Then** null callbacks fail argument validation and zero completed routes cause `InvalidOperationException`. A null inbox service collection is explicitly rejected; null outbox services are not made a valid registration by the adapter.

## MQTT-001-AC-03 — Provider registration alone does not process durable work
**Given** MQTT services/routes/options but no composed durable inbox/outbox integration, **When** the host resolves/executes admission or durable outgoing processing, **Then** MQTT registration does not provision tables, supply missing stores/consumers/contracts or independently complete durable work.
