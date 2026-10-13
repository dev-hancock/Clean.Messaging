# DOMAIN-003 — Opt-in EF capture composition

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: DomainEvents EF DependencyInjection, internal DomainEventCapture implements ISaveCapture; project references outbox EF/core. [Persistence](../../reference/persistence.md) and [configuration](../../reference/configuration.md) describe host composition.

Contracts: services.AddDomainEvents() and MessagingBuilder<TDbContext>.AddDomainEvents; TryAddEnumerable scoped ISaveCapture registration. Preconditions for durable operation: outbox services/model, relational configured context, both interceptors, concrete contracts and targets installed separately. Domain registration alone installs no outbox mappings/interceptors/store/dedicated validator. Host owns transaction and domain policy. Internal port implementation Capture(DbContext) is traced despite not being a public type.

## DOMAIN-003-AC-01 — Register capture without manufacturing persistence

- **Given** host calls services or builder AddDomainEvents alone.
- **When** services are inspected or saving is attempted without outbox composition.
- **Then** domain capture is registered, but durable outbox composition is not implicitly installed and no successful persistence guarantee follows.

## DOMAIN-003-AC-02 — Capture through correctly composed owning context

- **Given** outbox and domain capture are installed with mappings/interceptors/contracts/targets.
- **When** a tracked source owning save commits.
- **Then** pending domain facts are captured as outgoing work on that same owning context rather than a separate domain dispatcher.
