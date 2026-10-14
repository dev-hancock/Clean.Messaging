# DOMAIN-003 — Opt-in EF capture composition

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Hosts need to opt into domain-event capture without handing the library ownership of application persistence. Correct composition connects tracked facts to the existing transactional outbox rather than introducing a separate domain dispatcher.

## Guarantees

- AddDomainEvents MUST enable the domain capture hook without implicitly installing outbox persistence, model mappings or context interceptors.
- With the required composition installed, captured facts MUST be staged through the owning persistence context’s outbox, not an independent domain-event dispatcher.

## Contract boundaries

The host supplies outbox services, application entity mappings, durable outbox mappings, both outbox interceptors, a relational context and registered payload contracts/targets. Service registration alone is not evidence of durable readiness. Business rules, transaction choice and host lifetime remain application responsibilities.

## Acceptance criteria

### DOMAIN-003-AC-01 — Register capture without manufacturing persistence

- **Given** a host installs AddDomainEvents without outbox composition.
- **When** the host examines the installed capability or attempts to save.
- **Then** the capture hook is registered, but outbox persistence and interceptor/model installation have not been supplied implicitly and durable capture is not established.

### DOMAIN-003-AC-02 — Capture through correctly composed owning context

- **Given** the host has installed outbox/domain capture, mappings, interceptors, contracts and targets.
- **When** a tracked source’s owning save commits.
- **Then** its pending facts become outgoing work in that same owning context, without a separate domain dispatcher.

## Open decisions

None.
