# Configuration and host integration

## Scope and baseline

For host developers consulting registration boundaries, service lifetimes, validation, and configuration ownership. This page describes composition contracts, not an executable setup walkthrough or proof of provider compatibility.

## Core registration

Namespace: `Clean.Messaging`. [Source](../../src/Clean.Messaging/DependencyInjection.cs).

| Entry point | Behaviour |
| --- | --- |
| `IServiceCollection.AddMessaging(Action<RetryOptions>? configure = null)` | Registers core services, applies optional retry configuration, enables retry startup validation, and registers consumer startup validation. |
| `IServiceCollection.AddMessaging(IConfiguration configuration)` | Binds retry options from `Messaging:Retry` through the callback overload. It does not bind every capability's options. |
| `AddContract<TMessage>(string contract)` | Registers a persisted contract name and ensures core registration. |
| `AddConsumer<TMessage, TConsumer>(string consumerId)` | Registers an invoker and the consumer service, and ensures core registration. |

Repeated service registration uses `TryAdd` for core service implementations, but contract and consumer registrations remain explicit entries: duplicate identities/types still fail registry construction. Core registration alone creates no inbox/outbox tables or durable worker integration.

## Persistence builder

Namespace: `Clean.Messaging.EntityFrameworkCore`. [Registration](../../src/Clean.Messaging.EntityFrameworkCore/DependencyInjection.cs), [builder](../../src/Clean.Messaging.EntityFrameworkCore/MessagingBuilder.cs).

`AddMessaging<TDbContext>(Action<MessagingBuilder<TDbContext>> configure)` requires `TDbContext : DbContext` and a nonnull callback. It selects the messaging persistence context, registers the core, and invokes the callback with a builder. It does not itself register or configure the application `DbContext`.

| Builder member | Contract |
| --- | --- |
| `Services` | Underlying `IServiceCollection` for composition. |
| `AddContract<TMessage>(string contract)` | Delegates contract registration and returns the builder. |
| `AddConsumer<TMessage, TConsumer>(string consumerId)` | Delegates consumer registration and returns the builder. |
| `ConfigureRetry(Action<RetryOptions> configure)` | Adds retry option configuration; callback must be nonnull. |

One configured messaging context type is supported per service collection. See [Persistence](persistence.md) for transaction constraints, model configuration and interceptor contracts.

## Capability integration map

These names identify registration surfaces, not complete signatures or a required setup order. Exact overloads, validation, provider requirements and associated model/interceptor APIs are documented in the linked capability pages.

| Surface | Purpose | Authoritative reference |
| --- | --- | --- |
| `AddInbox` | Durable inbox services, options and EF processing integration | [Inbox](inbox.md) |
| `AddOutbox` | Outgoing staging, target delivery and EF save/transaction integration | [Outbox](outbox.md) |
| `AddDomainEvents` | Pending event capture into outbox saves | [Domain events](domain-events.md) |
| `AddIngress`, `AddDispatch` | Typed incoming/outgoing transport mappings | [Transport](transport.md) |
| `AddMqttInbox`, `AddMqttOutbox` | MQTT adapters, route/subscription options and validation | [Transport](transport.md) |
| `AddScheduling`, `AddOutboxScheduling` | Durable schedule integration and application-facing outbox scheduling | [Scheduling](scheduling.md) |
| `UseQuartzScheduling` | Optional scheduling wake-engine replacement | [Scheduling](scheduling.md) |
| `AddSagas` | Saga definitions and durable state integration | [Sagas](sagas.md) |

## Default core lifetimes

Registered by `AddMessaging` unless a prior `TryAdd`-compatible registration exists:

| Lifetime | Services |
| --- | --- |
| Singleton | `TimeProvider.System`, `MessageContext` / `IMessageContext`, contract and consumer registries, `IMessageSerializer`, `MessageFailureFactory`, `RetryPolicy`, `ClaimRenewal` |
| Scoped | `ConsumerDispatcher`; concrete consumers added through `TryAddScoped<TConsumer>` |
| Hosted validation | `ConsumerValidation`; retry options validation on startup |

`IMessageContext` resolves the same `MessageContext` instance; its metadata is execution-local, not one shared mutable message for all workers. Feature integrations supply their own store, writer, worker, cleanup and validation registrations. Singleton runtime services must not be taken as evidence that scoped consumers or contexts are thread-safe.

## Options and validation ownership

| Configuration family | Details |
| --- | --- |
| Shared `MessageOptions` (lease, processing, recovery, cleanup) | Defaults and validity constraints: [Processing](processing.md). |
| `RetryOptions` | Defaults, backoff and validation: [Processing](processing.md). |
| Inbox/outbox options and binding overloads | [Inbox](inbox.md), [Outbox](outbox.md). |
| MQTT connection, session, routing and QoS options | [Transport](transport.md). |
| Scheduling and saga options | [Scheduling](scheduling.md), [Sagas](sagas.md). |

The callback and `IConfiguration` overloads are different mechanisms; a section constant does not mean all registrations automatically bind that section. Startup validation does not establish universal hot-reload support. Consult each consumer of options and feature reference before relying on runtime configuration changes.

Consumer startup validation checks registered contracts and service availability, not database provisioning, broker reachability, or consumer correctness. Exact retry validity constraints are maintained in [Processing](processing.md), alongside their defaults.

## Host responsibilities and conditional guarantees

| Concern | Library supplies | Host/application supplies |
| --- | --- | --- |
| Dispatch | Contract lookup, serialization and registered invokers | Stable message schemas, consumers and compatible registrations |
| Persistence | Mapping/store/transaction integration APIs | Scoped context, relational provider, required mappings/interceptors and schema/migrations |
| Processing | Workers, claims, retry/settlement and cleanup implementation | Running host lifetime and suitable operational settings |
| MQTT | Adapter mapping, connection handling and protocol interaction | Broker, credentials, TLS/session configuration and compatible peers |
| Business effects | Invocation and local transaction participation where supported | Business policy and idempotency for effects outside that transaction |

Successful DI registration is not successful durable integration. Atomic local guarantees require the configured scope/context and supported transaction integration. An available broker or QoS setting does not add distributed atomicity. See [Architecture](architecture.md), [Transaction boundaries](../explanation/transaction-boundaries.md), and [Delivery semantics](../explanation/delivery-semantics.md).

## Extension boundaries

Application-facing consumer contracts do not expose EF or MQTT types. Provider registration APIs may expose their own framework-specific configuration. Public runtime ports are not automatically independent alternative-provider guarantees: some integration assemblies use `InternalsVisibleTo`. The [package dependency map](architecture.md#compile-time-dependencies) records actual coupling; no alternate database/provider support matrix is implied.
