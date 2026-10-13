# Configuration, validation and host composition — Behavioural specification

**Scope:** Core AddMessaging overloads, AddContract/AddConsumer; MessagingBuilder<T>.Services/AddContract/AddConsumer/ConfigureRetry; PersistenceServices.EnsurePersistence/AddOptions; MessageOptions and nested options, MessageOptionsValidator; feature registrars and validators are traced in their capability specifications.

## Purpose
Compose library capabilities with explicit host-owned contracts, scoped persistence and resources, rejecting specified invalid configurations without implying operational readiness.

## Contracts, preconditions and postconditions
Pre: host supplies provider, DbContext, mappings, applicable interceptors, deployed schema, consumers/routes and running service lifetime. Post: successful registration makes configured library services available, not an operational broker or database. Registration and option/model/interceptor validation occur at distinct boundaries.

## Invariants
- A singleton must not retain a scoped dependency (coding standards §6).
- Invalid critical configuration must fail clearly at an appropriate boundary.
- Provider-neutral consumer contracts do not require EF/MQTT/Quartz representations.

## Failure, cancellation and consistency
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are unresolved decisions, not executable requirements.

## Limitations
- No provisioning/migration execution, universal hot reload, provider certification or host framework is supplied.
- Public options remain mutable; modifying already-consumed objects is not a documented reconfiguration protocol.

## Implementation trace
These locations explain contract ownership; they do not mandate internal implementation structure.

- [src/Clean.Messaging/DependencyInjection.cs](../../../src/Clean.Messaging/DependencyInjection.cs)
- [src/Clean.Messaging/Consumers/ConsumerValidation.cs](../../../src/Clean.Messaging/Consumers/ConsumerValidation.cs)
- [src/Clean.Messaging/MessageOptions.cs](../../../src/Clean.Messaging/MessageOptions.cs)
- [src/Clean.Messaging/Processing/MessageOptionsValidator.cs](../../../src/Clean.Messaging/Processing/MessageOptionsValidator.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DependencyInjection.cs](../../../src/Clean.Messaging.EntityFrameworkCore/DependencyInjection.cs)
- [src/Clean.Messaging.EntityFrameworkCore/MessagingBuilder.cs](../../../src/Clean.Messaging.EntityFrameworkCore/MessagingBuilder.cs)
- [src/Clean.Messaging.EntityFrameworkCore/PersistenceServices.cs](../../../src/Clean.Messaging.EntityFrameworkCore/PersistenceServices.cs)

## Open decisions
The [gap register](../GAPS.md) records unresolved input domains, provider evidence and proposed changes. [INDEX.md](../INDEX.md) explains classification authority and cross-capability ownership.

## Specifications

- [CONFIG-001 — Host ownership and resource lifetimes](CONFIG-001-host-ownership-and-resource-lifetimes.md)
- [CONFIG-002 — Core defaults and override preservation](CONFIG-002-core-defaults-and-override-preservation.md)
- [CONFIG-003 — Registration identifier boundaries](CONFIG-003-registration-identifier-boundaries.md)
- [CONFIG-004 — Startup validation and configuration binding](CONFIG-004-startup-validation-and-configuration-binding.md)
- [CONFIG-005 — One messaging persistence context type](CONFIG-005-one-messaging-persistence-context-type.md)
- [CONFIG-006 — Complete boundary validation](CONFIG-006-complete-boundary-validation.md)
