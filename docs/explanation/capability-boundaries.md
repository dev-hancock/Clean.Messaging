# Capability boundaries and host ownership

## The question

How can a reusable messaging library serve a Clean Architecture application without becoming that application's architecture?

Clean.Messaging is organised around messaging capabilities and technical integrations. A consuming application is organised around its business meaning and use cases. Those are different axes: the library's `Sagas` project is not the consuming application's Application layer, and its domain-event support does not define that application's domain model.

This explanation draws consequences from the current [package map](../reference/architecture.md#package-map) and public contracts. It does not claim that every dependency resulted from a recorded architectural decision.

## Reusable mechanisms versus business decisions

The library can preserve identity, stage durable work, invoke a registered consumer, and manage processing ownership. It cannot decide whether a business operation is valid, which compensation is appropriate, or what a producer's event means to another application.

Keeping those meanings outside the library allows the same mechanisms to support unrelated applications. A consumer can coordinate a use case or translate an external event without requiring the library to understand the domain. Likewise, `AggregateRoot` and `IDomainEventSource` support collecting raised objects; they do not replace domain invariants or an application's aggregate design.

The trade-off is explicit host work: contracts, consumers, mappings and saga definitions must be supplied. A more opinionated application framework could prescribe these conventions, but that would expand the library's coupling to business/application choices. The current [host integration boundary](../reference/configuration.md#host-responsibilities-and-conditional-guarantees) records what the library actually supplies.

## Provider-neutral does not mean framework-free

The runtime uses Microsoft.Extensions for DI, hosting, logging, options and configuration. Provider-specific EF, MQTTnet and Quartz types remain in integration projects. These are distinct coupling decisions: reusable processing policy can use .NET hosting abstractions while remaining independent of a particular broker implementation.

Moving every interface into one dependency-free assembly would not automatically improve ownership. A port belongs to the capability that understands its semantics. Conversely, exposing broker-specific representations through application-facing consumer contracts would force users of those contracts to depend on transport details.

The current dependency graph includes tightly coupled integration assemblies and friend access. That means project boundaries should not be mistaken for a promise that every implementation can be replaced by an arbitrary third-party provider. A genuinely independent provider requires complete contracts for lifetimes, transactions, ownership and failure translation, not merely a matching interface name.

## Transport models protect different contracts

A producer's transport payload and a consumer's durable message can evolve for different reasons. MQTT topics and user properties describe protocol interaction; stored contract names and JSON describe retained work; the typed message expresses what a local consumer accepts.

Ingress and Dispatch mappings make those boundaries explicit. They permit translation instead of requiring every peer to share a CLR type. They also add configuration and compatibility obligations: a mapper cannot infer business intent, and serializer compatibility is still a shared producer/consumer responsibility.

Directly sharing payload classes would reduce translation code where all peers truly share a contract. It would also couple local processing to external representation. The library supports explicit mappings; whether two particular representations should be identical is an application decision. See [Transport](../reference/transport.md) and [Contracts](../reference/contracts.md).

## Composition connects the pieces

The host owns the database provider and schema, broker connection settings, message contracts and running services. DI registration connects library implementations to those resources, but does not provision them or prove their availability.

There are therefore several distinct notions of validity: a registration may be structurally valid, its EF model may still be incomplete, and its database or broker may be unavailable. Startup checks can catch specified configuration errors without establishing complete operational readiness. Treating registration success as delivery success would obscure that distinction.

## Capability coupling can be meaningful

The current saga implementation writes outgoing effects through outbox and durable timers through scheduling. Those dependencies express real runtime interactions, not simply organisational resemblance. Their consequences are examined in [Durable workflows](durable-workflows.md).

An alternative saga engine could define narrower external-effect and timer ports. That might allow independent implementations, but would need to preserve atomic effect capture, stable identity, timer cancellation and concurrency semantics. Replacing dependencies without preserving those relationships would change the workflow contract, not merely clean up a graph.

## Related reading

- [Architecture](../reference/architecture.md): actual projects and references.
- [Configuration](../reference/configuration.md): service composition and ownership.
- [Domain events](../reference/domain-events.md): capture and acknowledgement contracts.
- [Transaction boundaries](transaction-boundaries.md): local consistency versus external effects.
